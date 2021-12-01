namespace TarbikMap.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using TarbikMap.AreaSources;
    using TarbikMap.Common;
    using TarbikMap.Common.Downloader;
    using TarbikMap.DTO;
    using TarbikMap.Hubs;
    using TarbikMap.Storage;
    using TarbikMap.TaskSources;
    using TarbikMap.Utils;
    using GameTask = TarbikMap.Storage.GameTask;

    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Controller")]
    internal class GameController : ControllerBase
    {
        private const int SessionKeyLength = 32;

        private const int DefaultTasksCount = 5;
        private const int DefaultAnsweringTimeLimitSeconds = 30;
        private const int DefaultCompletingTimeLimitSeconds = 20;

        private static readonly RandomGenerator RandomGenerator = new RandomGenerator();
        private static readonly TimeSpan GameValidity = TimeSpan.FromHours(2);

        private static readonly ReasonableStringChecker StringCheckerForPlayerName = new ReasonableStringChecker().AllowMaxLength(20).AllowAllReasonableCharacters();
        private static readonly ReasonableStringChecker StringCheckerForFileName = new ReasonableStringChecker().AllowMaxLength(20).AllowLowercaseLetters().AllowNumbers().AllowAdditionalCharacters(new[] { '_', '-', '.', '@' });

        private readonly ILogger<GameController> logger;
        private readonly IHubContext<GameHub>? hubContext;
        private readonly StorageMain storage;
        private readonly IAreaSource areaSource;
        private readonly ITaskSource taskSource;
        private readonly IDownloader downloader;
        private readonly GameStateGetter gameStateGetter;

        private readonly EnvironmentConfig environmentConfig;

        public GameController(ILogger<GameController> logger, IHubContext<GameHub>? hubContext, StorageMain storage, IAreaSource areaSource, ITaskSource taskSource, IDownloader downloader, EnvironmentConfig environmentConfig, GameStateGetter gameStateGetter)
        {
            this.logger = logger;
            this.hubContext = hubContext;
            this.storage = storage;
            this.areaSource = areaSource;
            this.taskSource = taskSource;
            this.downloader = downloader;
            this.environmentConfig = environmentConfig;
            this.gameStateGetter = gameStateGetter;
        }

        public static string? LastSetCookieForTests { get; private set; }

        [HttpPost]
        [Route("games/configurations")]
        public async Task<GameAvailableConfigurationsDTO> GetAvailableConfigurations([FromBody] string query)
        {
            List<GameAreaDTO> areas = (await this.areaSource.Search(query).ConfigureAwait(false)).Select(t => new GameAreaDTO(t.Key, t.Label)).ToList();
            List<GameTypeDTO> gameTypes = (await this.taskSource.Search(query).ConfigureAwait(false)).Select(t => new GameTypeDTO(t.Key, t.Label, t.CategoryKey)).ToList();

            return new GameAvailableConfigurationsDTO(areas, gameTypes);
        }

        [HttpPost]
        [Route("games/create")]
        public async Task<NewGameDTO> CreateGame()
        {
            var createdGameId = this.CreateGameInternal(new GameConfiguration(
                (await this.taskSource.Search(string.Empty).ConfigureAwait(false))[0].Key,
                (await this.areaSource.Search(string.Empty).ConfigureAwait(false))[0].Key,
                DefaultTasksCount,
                DefaultAnsweringTimeLimitSeconds,
                DefaultCompletingTimeLimitSeconds));

            return new NewGameDTO(createdGameId);
        }

        [HttpPost]
        [Route("games/{gameId}/join")]
        public void JoinGame(string gameId, [FromBody] string playerName)
        {
            if (!StringCheckerForPlayerName.Check(playerName))
            {
                throw new ArgumentException("Invalid player name");
            }

            string? sessionKey = this.GetSessionKeyCookie_orNull();
            if (sessionKey == null)
            {
                sessionKey = RandomGenerator.UppercaseString(SessionKeyLength);
                this.Response.Cookies.Append("session_key", sessionKey, new CookieOptions() { SameSite = SameSiteMode.Strict, Secure = true });
                LastSetCookieForTests = sessionKey;
            }

            Player player = new Player(
                playerName.Trim(),
                sessionKey,
                new List<PlayerAnswer>(),
                0);

            Game game = this.storage.FindGame(gameId);

            lock (game)
            {
                if (game.Players.Any(player => string.Equals(player.Name, playerName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("Name already exists");
                }

                if (game.Players.Any(player => player.SessionKey == sessionKey))
                {
                    throw new InvalidOperationException("Session key already joined");
                }

                game.Players.Add(player);
            }

            this.SendGameState(gameId);
        }

        [HttpPost]
        [Route("games/{gameId}/geometry")]
        public async Task<GeometryDTO> GetGameGeometry(string gameId)
        {
            Game game = this.storage.FindGame(gameId);

            string area;
            lock (game)
            {
                area = game.Configuration.Area;
            }

            var geometry = await this.areaSource.GetGeometry(area).ConfigureAwait(false);
            return GeometryUtils.GeometryToDTO(geometry, area);
        }

        [HttpPost]
        [Route("games/{gameId}/verify")]
        public bool VerifyGameExists(string gameId)
        {
            return this.storage.GameExists(gameId);
        }

        [HttpPost]
        [Route("games/{gameId}/start")]
        public void StartGame(string gameId)
        {
            Game game = this.storage.FindGame(gameId);

            lock (game)
            {
                if (game.CurrentConfigurationError != null)
                {
                    throw new InvalidOperationException("Current configuration has error");
                }

                if (game.Players.Count == 0)
                {
                    throw new InvalidOperationException("No players registered");
                }

                if (game.Starting)
                {
                    throw new InvalidOperationException("Game already starting");
                }

                if (game.Started)
                {
                    throw new InvalidOperationException("Game already started");
                }

                game.Starting = true;
            }

            this.SendGameState(gameId);

            lock (game)
            {
                game.Starting = false;

                try
                {
                    var geometry = this.areaSource.GetGeometry(game.Configuration.Area).Result;

                    if (this.taskSource.IsFinite(game.Configuration.Type))
                    {
                        var tasks = this.taskSource.CreateTasks(game.Configuration.Type, game.Configuration.Area, geometry, game.Configuration.TasksCount).Result;
                        if (tasks.Count == 0)
                        {
                            throw new PublicException("No places found in the selected area.");
                        }

                        game.Tasks = tasks.Select(t => (GameTask?)t).ToList();
                    }
                    else
                    {
                        var task = this.taskSource.CreateTask(game.Configuration.Type, game.Configuration.Area, geometry, new List<GameTask>()).Result;
                        game.Tasks = new List<GameTask?>(game.Configuration.TasksCount);
                        game.Tasks.Add(task);
                        for (int i = 0; i < game.Configuration.TasksCount - 1; ++i)
                        {
                            game.Tasks.Add(null);
                        }
                    }

                    game.CurrentTaskAnsweringStart = DateTime.UtcNow;

                    Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(game.Configuration.AnsweringTimeLimitSeconds)).ConfigureAwait(false);
                        this.FinalizeAnswering(gameId, 1);
                    });

                    game.Started = true;
                }
                catch (PublicException e)
                {
                    game.CurrentConfigurationError = e.Message;
                }
                catch
                {
                    game.CurrentConfigurationError = "An error occurred while creating game tasks.";
                    throw;
                }
                finally
                {
                    this.SendGameState(gameId);
                }
            }
        }

        [HttpPost]
        [Route("games/{gameId}/configuration")]
        public void ChangeGameConfiguration(string gameId, [FromBody] GameConfigurationChangeDTO configuration)
        {
            if (configuration.TasksCount != null)
            {
                if (configuration.TasksCount < 1 || configuration.TasksCount > 20)
                {
                    throw new ArgumentException("Invalid tasks count");
                }
            }

            if (configuration.AnsweringTimeLimitSeconds != null)
            {
                if (configuration.AnsweringTimeLimitSeconds < 1 || configuration.AnsweringTimeLimitSeconds > 300)
                {
                    throw new ArgumentException("Invalid answering time limit");
                }
            }

            if (configuration.CompletingTimeLimitSeconds != null)
            {
                if (configuration.CompletingTimeLimitSeconds < 1 || configuration.CompletingTimeLimitSeconds > 300)
                {
                    throw new ArgumentException("Invalid completing time limit");
                }
            }

            Game game = this.storage.FindGame(gameId);

            lock (game)
            {
                if (game.Starting)
                {
                    throw new InvalidOperationException("Game already starting");
                }

                if (game.Started)
                {
                    throw new InvalidOperationException("Game already started");
                }

                if (configuration.Type != null)
                {
                    game.Configuration.Type = configuration.Type;
                    this.ReloadTypeLabel(game, gameId);
                }

                if (configuration.Area != null)
                {
                    game.Configuration.Area = configuration.Area;
                    this.ReloadAreaLabel(game, gameId);
                }

                if (configuration.TasksCount != null)
                {
                    game.Configuration.TasksCount = configuration.TasksCount.Value;
                }

                if (configuration.AnsweringTimeLimitSeconds != null)
                {
                    game.Configuration.AnsweringTimeLimitSeconds = configuration.AnsweringTimeLimitSeconds.Value;
                }

                if (configuration.CompletingTimeLimitSeconds != null)
                {
                    game.Configuration.CompletingTimeLimitSeconds = configuration.CompletingTimeLimitSeconds.Value;
                }
            }

            game.CurrentConfigurationError = null;

            this.SendGameState(gameId);
        }

        [HttpPost]
        [Route("games/{gameId}/answer/{task}")]
        public void ProvideAnswer(string gameId, int task, [FromBody] ProvidedAnswerDTO providedAnswer)
        {
            if (providedAnswer.Lat < -90 || providedAnswer.Lat >= 90 || providedAnswer.Lon < -180 || providedAnswer.Lon >= 180)
            {
                throw new ArgumentException("Invalid coordinates");
            }

            Game game = this.storage.FindGame(gameId);

            lock (game)
            {
                Player currentPlayer = FindCurrentPlayer(game, this.GetSessionKeyCookie());

                if (!game.Started)
                {
                    throw new InvalidOperationException("Game not started");
                }

                if (task < 0 || task >= game.Tasks!.Count)
                {
                    throw new InvalidOperationException("Invalid task index");
                }

                if (currentPlayer.Answers.Count != task)
                {
                    throw new InvalidOperationException("Invalid task index");
                }

                if (currentPlayer.TasksCompleted != task)
                {
                    throw new InvalidOperationException("Invalid task index");
                }

                if (game.TasksCompleted != task)
                {
                    throw new InvalidOperationException("Invalid task index");
                }

                var foundTask = game.Tasks[task];
                var correctAnswer = foundTask!.Answer;
                currentPlayer.Answers.Add(new PlayerAnswer(
                    providedAnswer.Lat,
                    providedAnswer.Lon,
                    CalculationUtils.DistanceBetween(providedAnswer.Lat, providedAnswer.Lon, correctAnswer.Lat, correctAnswer.Lon),
                    CalculatePoints(providedAnswer, correctAnswer, foundTask!.Question)));

                bool allPlayersAnswered = game.TasksAnswered == task + 1;
                if (allPlayersAnswered)
                {
                    game.CurrentTaskAnsweringStart = null;
                    game.CurrentTaskCompletingStart = DateTime.UtcNow;
                    Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(game.Configuration.CompletingTimeLimitSeconds)).ConfigureAwait(false);
                        this.FinalizeCompleting(gameId, task + 1);
                    });
                }
            }

            this.SendGameState(gameId);
        }

        [HttpPost]
        [Route("games/{gameId}/next")]
        public string CreateNextGame(string gameId)
        {
            Game game = this.storage.FindGame(gameId);

            string nextGameId;

            lock (game)
            {
                if (game.TasksCompleted != game.Tasks!.Count)
                {
                    throw new InvalidOperationException("Game not finished yet");
                }

                if (game.NextGameId != null)
                {
                    throw new InvalidOperationException("Next game already created");
                }

                nextGameId = this.CreateGameInternal(game.Configuration.ShallowCopy());
                game.NextGameId = nextGameId;
            }

            this.SendGameState(gameId);

            return nextGameId;
        }

        [Route("games/{gameId}/image/{taskIndex}/{imageIndex}")]
        public FileResult GetImage(string gameId, int taskIndex, int imageIndex)
        {
            Game game = this.storage.FindGame(gameId);

            DateTime? expires = null;

            byte[]? cachedBytes = null;

            lock (game)
            {
                expires = game.ValidUntil;
                var task = game.Tasks![taskIndex];
                if (task == null)
                {
                    throw new InvalidOperationException("Task is null");
                }

                var taskImage = task.Question.Images[imageIndex];

                if (taskImage.Access == TaskImage.AccessType.HTTP)
                {
                    if (taskImage.Cached == null)
                    {
                        taskImage.Cached = ImageResizer.ResizeIfNeeded(this.downloader.HttpGet(taskImage.Url).Result);
                    }

                    cachedBytes = taskImage.Cached;
                }
            }

            this.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue() { Public = true, MustRevalidate = true };
            this.Response.GetTypedHeaders().Expires = expires.Value;

            string contentType = "image/jpeg";

            if (cachedBytes != null)
            {
                return this.File(cachedBytes, contentType);
            }

            throw new InvalidOperationException("Unhandled image serving type");
        }

        [HttpPost]
        [Route("games/{gameId}/complete/{task}")]
        public void CompleteTask(string gameId, int task)
        {
            Game game = this.storage.FindGame(gameId);

            lock (game)
            {
                Player currentPlayer = FindCurrentPlayer(game, this.GetSessionKeyCookie());

                if (!game.Started)
                {
                    throw new InvalidOperationException("Game not started");
                }

                if (task < 0 || task >= game.Tasks!.Count)
                {
                    throw new InvalidOperationException("Invalid task index");
                }

                if (currentPlayer.Answers.Count != task + 1)
                {
                    throw new InvalidOperationException("Invalid task index");
                }

                if (currentPlayer.TasksCompleted != task)
                {
                    throw new InvalidOperationException("Invalid task index");
                }

                if (game.TasksCompleted != task)
                {
                    throw new InvalidOperationException("Invalid task index");
                }

                ++currentPlayer.TasksCompleted;

                bool allPlayersCompleted = game.TasksCompleted == task + 1;
                if (allPlayersCompleted)
                {
                    game.CurrentTaskCompletingStart = null;
                    if (game.TasksCompleted < game.Tasks.Count)
                    {
                        game.CurrentTaskAnsweringStart = DateTime.UtcNow;
                        Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(game.Configuration.AnsweringTimeLimitSeconds)).ConfigureAwait(false);
                            this.FinalizeAnswering(gameId, task + 2);
                        });
                    }
                }
            }

            this.SendGameState(gameId);
        }

        [HttpPost]
        [Route("games/presets")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1822", Justification = "Cannot be static, is controller method")]
        public List<PresetDTO> GetPresets()
        {
            List<PresetDTO> result = new List<PresetDTO>();

            result.Add(new PresetDTO("Custom", "Choose any area and game type.", "Bela_river_Slovakia.jpg", "wd_places", "co_world"));
            result.Add(new PresetDTO("World - Capitals", string.Empty, "Tokyo_Tower_and_around_Skyscrapers.jpg", "wd_capitals", "co_world"));
            result.Add(new PresetDTO("Europe - Capitals", string.Empty, "Eiffel_Tower_from_the_Tour_Montparnasse_3,_Paris_May_2014.jpg", "wd_capitals", "ne_ne_10m_geography_regions_polys_1026"));
            result.Add(new PresetDTO("Europe - Railway Stations", string.Empty, "1280px-2011-03-29_Hauptbahnhof_interior_2.jpg", "wd_railway_stations", "ne_ne_10m_geography_regions_polys_1026"));
            result.Add(new PresetDTO("Towns in Slovakia", string.Empty, "Bardejov_namesti_3773.jpg", "wd_towns", "ne_ne_10m_admin_0_countries_77"));
            result.Add(new PresetDTO("Castles in Slovakia", string.Empty, "Bojnice_Bojnitz_Castle_by_Pudelek.jpg", "wd_castles", "ne_ne_10m_admin_0_countries_77"));

            return result;
        }

        [Route("images/{imageKey}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1822", Justification = "Cannot be static, is controller method")]
        public FileResult GetPresetImage(string imageKey)
        {
            if (!ResourcesProvider.GetFileNames("pages", "preset_images").Contains(imageKey))
            {
                throw new ArgumentException("Invalid image key");
            }

            var stream = ResourcesProvider.GetStream("pages", "preset_images", imageKey);

            return new FileStreamResult(stream, "image/jpeg");
        }

        [Route("config/{fileName}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1822", Justification = "Cannot be static, is controller method")]
        public ContentResult GetPublicConfigFile(string fileName, [FromQuery(Name = "currentHost")] string? currentHost)
        {
            CheckFileName(fileName);

            var stream = this.environmentConfig.GetPublicConfigFile(fileName);

            string str;
            using (StreamReader sr = new StreamReader(stream))
            {
                str = sr.ReadToEnd();
            }

            str = str.Replace("<current_host>", currentHost, StringComparison.Ordinal);

            return this.Content(str);
        }

        [Route("tiles/{zoom}/{zoom2}_{x}.i")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1822", Justification = "Cannot be static, is controller method")]
        public FileResult GetTileIndex(int zoom, int zoom2, int x)
        {
            this.CheckCanServeMapResources();

            if (zoom != zoom2)
            {
                throw new ArgumentException("Invalid zoom");
            }

            var stream = ResourcesProvider.GetStream("tiles", zoom.ToString(CultureInfo.InvariantCulture), zoom + "_" + x + ".i");

            return new FileStreamResult(stream, "application/octet-stream");
        }

        [Route("tiles/{zoom}/{zoom2}_{x}.t")]
        public FileResult GetTileData(int zoom, int zoom2, int x)
        {
            this.CheckCanServeMapResources();

            if (zoom != zoom2)
            {
                throw new ArgumentException("Invalid zoom");
            }

            string contentRange = this.Request.Headers["range"];
            string expectedPrefix = "bytes=";
            if (!contentRange.StartsWith(expectedPrefix, StringComparison.Ordinal))
            {
                throw new ArgumentException("Invalid range prefix");
            }

            string range = contentRange.Substring(expectedPrefix.Length);
            string[] rangeParts = range.Split('-');
            if (rangeParts.Length != 2)
            {
                throw new ArgumentException("Invalid range parts count");
            }

            long start = long.Parse(rangeParts[0], CultureInfo.InvariantCulture);
            long? end = rangeParts[1].Length > 0 ? long.Parse(rangeParts[1], CultureInfo.InvariantCulture) : null;

            var stream = ResourcesProvider.GetStream("tiles", zoom.ToString(CultureInfo.InvariantCulture), zoom + "_" + x + ".t");

            return new FileStreamResult(new SubStream(stream, start, end), "application/octet-stream");
        }

        [Route("fonts/{fontstack}/{file}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1822", Justification = "Cannot be static, is controller method")]
        public FileResult GetFontFile(string fontstack, string file)
        {
            this.CheckCanServeMapResources();
            CheckFileName(fontstack);
            CheckFileName(file);

            var stream = ResourcesProvider.GetStream("fonts", fontstack, file);

            return new FileStreamResult(stream, "application/octet-stream");
        }

        [Route("sprite/{file}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1822", Justification = "Cannot be static, is controller method")]
        public FileResult GetSpriteFile(string file)
        {
            this.CheckCanServeMapResources();
            CheckFileName(file);

            var stream = ResourcesProvider.GetStream("sprites", "default", file);

            return new FileStreamResult(stream, "application/octet-stream");
        }

        private static int CalculatePoints(ProvidedAnswerDTO providedAnswer, TaskAnswer correctAnswer, TaskQuestion question)
        {
            var maxDistance = CalculationUtils.DistanceBetween(question.InitialLat1, question.InitialLon1, question.InitialLat2, question.InitialLon2) / 2;

            return Math.Max(0, (int)((maxDistance - CalculationUtils.DistanceBetween(providedAnswer.Lat, providedAnswer.Lon, correctAnswer.Lat, correctAnswer.Lon)) * 1000 / maxDistance));
        }

        private static void CheckFileName(string fileName)
        {
            if (!StringCheckerForFileName.Check(fileName))
            {
                throw new ArgumentException("Invalid file name");
            }
        }

        private static Player FindCurrentPlayer(Game game, string sessionKey)
        {
            Player? result = null;

            if (sessionKey != null)
            {
                result = game.Players.SingleOrDefault(p => p.SessionKey == sessionKey);
            }

            if (result != null)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException("Current player not found");
            }
        }

        private void SendGameState(string gameId)
        {
            if (this.hubContext != null)
            {
                Game game = this.storage.FindGame(gameId);

                List<string> sessionKeys;
                lock (game)
                {
                    sessionKeys = game.Players.Select(player => player.SessionKey).ToList();
                }

                foreach (var sessionKey in sessionKeys)
                {
                    var gameState = this.gameStateGetter.GetGameState(gameId, sessionKey);
                    this.hubContext.Clients.Group("joined_" + gameId + "_" + sessionKey).SendAsync("game_state", gameState);
                }

                {
                    var gameState = this.gameStateGetter.GetGameState(gameId, null);
                    this.hubContext.Clients.Group("observer_" + gameId).SendAsync("game_state", gameState);
                }
            }
        }

        private void FinalizeAnswering(string gameId, int requiredTasksCountAnswered)
        {
            Game game = this.storage.FindGame(gameId);

            bool changed = false;

            lock (game)
            {
                foreach (var player in game.Players)
                {
                    if (player.Answers.Count == requiredTasksCountAnswered - 1)
                    {
                        player.Answers.Add(PlayerAnswer.CreateNotAnswered());
                        changed = true;
                    }
                }

                if (changed)
                {
                    game.CurrentTaskAnsweringStart = null;
                    game.CurrentTaskCompletingStart = DateTime.UtcNow;
                    Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(game.Configuration.CompletingTimeLimitSeconds)).ConfigureAwait(false);
                        this.FinalizeCompleting(gameId, requiredTasksCountAnswered);
                    });
                }
            }

            if (changed)
            {
                this.SendGameState(gameId);
            }
        }

        private void FinalizeCompleting(string gameId, int requiredTasksCountCompleted)
        {
            Game game = this.storage.FindGame(gameId);

            bool changed = false;

            lock (game)
            {
                foreach (var player in game.Players)
                {
                    if (player.TasksCompleted == requiredTasksCountCompleted - 1)
                    {
                        player.TasksCompleted = requiredTasksCountCompleted;
                        changed = true;
                    }
                }

                if (changed)
                {
                    game.CurrentTaskCompletingStart = null;
                    if (game.TasksCompleted < game.Tasks!.Count)
                    {
                        game.CurrentTaskAnsweringStart = DateTime.UtcNow;
                        Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(game.Configuration.AnsweringTimeLimitSeconds)).ConfigureAwait(false);
                            this.FinalizeAnswering(gameId, requiredTasksCountCompleted + 1);
                        });
                    }
                }
            }

            if (changed)
            {
                this.SendGameState(gameId);
            }
        }

        private string CreateGameInternal(GameConfiguration configuration)
        {
            string createdGameId = RandomGenerator.UppercaseString(4);

            Game createdGame = new Game(
                DateTime.UtcNow + GameValidity,
                configuration,
                new List<Player>());

            this.storage.AddGame(createdGameId, createdGame);

            this.ReloadTypeLabel(createdGame, createdGameId);
            this.ReloadAreaLabel(createdGame, createdGameId);

            return createdGameId;
        }

        private void CheckCanServeMapResources()
        {
            if (this.environmentConfig.GetPrivateConfigValue("ServeMapResources") != "true")
            {
                throw new InvalidOperationException("Map resources serving disabled");
            }
        }

        private string GetSessionKeyCookie()
        {
            string? result = null;
            this.Request.Cookies.TryGetValue("session_key", out result);
            if (result != null && result.Length == SessionKeyLength)
            {
                return result;
            }

            throw new InvalidOperationException("Session key not found");
        }

        private string? GetSessionKeyCookie_orNull()
        {
            string? result = null;
            this.Request.Cookies.TryGetValue("session_key", out result);
            if (result != null && result.Length == SessionKeyLength)
            {
                return result;
            }

            return null;
        }

        private void ReloadTypeLabel(Game game, string gameId)
        {
            game.LoadedTypeLabel = null;

            string gameTypeKey = game.Configuration.Type;
            Task.Run(async () =>
                {
                    string label = await this.taskSource.GetLabel(gameTypeKey).ConfigureAwait(false);
                    lock (game)
                    {
                        if (game.Configuration.Type == gameTypeKey)
                        {
                            game.LoadedTypeLabel = label;
                        }
                    }

                    this.SendGameState(gameId);
                });
        }

        private void ReloadAreaLabel(Game game, string gameId)
        {
            game.LoadedAreaLabel = null;

            string areaKey = game.Configuration.Area;
            Task.Run(async () =>
                {
                    string label = await this.areaSource.GetLabel(areaKey).ConfigureAwait(false);

                    lock (game)
                    {
                        if (game.Configuration.Area == areaKey)
                        {
                            game.LoadedAreaLabel = label;
                        }
                    }

                    this.SendGameState(gameId);
                });
        }
    }
}
