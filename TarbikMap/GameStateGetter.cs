namespace TarbikMap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TarbikMap.AreaSources;
    using TarbikMap.DTO;
    using TarbikMap.Storage;
    using TarbikMap.TaskSources;

    internal class GameStateGetter
    {
        private StorageMain storage;
        private IAreaSource areaSource;
        private ITaskSource taskSource;

        public GameStateGetter(StorageMain storage, IAreaSource areaSource, ITaskSource taskSource)
        {
            this.storage = storage;
            this.areaSource = areaSource;
            this.taskSource = taskSource;
        }

        public GameStateDTO GetGameState(string gameId, string? sessionKey)
        {
            Game game = this.storage.FindGame(gameId);

            lock (game)
            {
                Player? currentPlayer = FindCurrentPlayer_orNull(game, sessionKey);

                int questionsCountToShow = game.Started ? Math.Min(game.TasksCompleted + 1, game.Tasks!.Count) : 0;
                int answersCountToShow = (currentPlayer != null) ? currentPlayer.Answers.Count : game.TasksAnswered;

                List<TaskQuestionDTO>? questions = null;
                List<TaskAnswerDTO>? correctAnswers = null;
                List<PlayerDTO>? players = null;

                if (game.Tasks != null)
                {
                    this.EnsureTasksExist_ForNotFinite(game.Configuration.Type, game.Configuration.Area, game.Tasks!, questionsCountToShow).Wait();

                    questions = game.Tasks.Take(questionsCountToShow).Select(task => new TaskQuestionDTO(
                        task!.Question.InitialLat1,
                        task!.Question.InitialLon1,
                        task!.Question.InitialLat2,
                        task!.Question.InitialLon2,
                        task!.Question.Images.Count)).ToList();

                    correctAnswers = game.Tasks.Take(answersCountToShow).Select(task => new TaskAnswerDTO(
                        task!.Answer.Lat,
                        task!.Answer.Lon,
                        task!.Answer.Description)).ToList();
                }

                players = game.Players.Select(player =>
                {
                    return new PlayerDTO(
                        player.Name,
                        player.Answers.Take(answersCountToShow).Select(answer =>
                            new PlayerAnswerDTO(
                                answer.Lat,
                                answer.Lon,
                                answer.Distance,
                                answer.Points)).ToList(),
                        player.Answers.Take(answersCountToShow).Select(answer => answer.Points).Sum());
                }).ToList();

                return new GameStateDTO(
                    new GameConfigurationDTO(
                        game.Configuration.Type,
                        game.Configuration.Area,
                        game.Configuration.TasksCount,
                        game.Configuration.AnsweringTimeLimitSeconds,
                        game.Configuration.CompletingTimeLimitSeconds),
                    game.CurrentConfigurationError,
                    game.LoadedTypeLabel,
                    game.LoadedAreaLabel,
                    game.Starting,
                    game.Started,
                    game.TasksCompleted,
                    game.Tasks != null ? game.Tasks.Count : null,
                    questions,
                    correctAnswers,
                    players,
                    (currentPlayer != null) ? (currentPlayer.TasksCompleted == game.TasksCompleted + 1) : null,
                    (currentPlayer != null) ? game.Players.IndexOf(currentPlayer) : null,
                    game.NextGameId,
                    game.CurrentTaskAnsweringStart.HasValue ? (int?)Math.Max((game.CurrentTaskAnsweringStart.Value + TimeSpan.FromSeconds(game.Configuration.AnsweringTimeLimitSeconds) - DateTime.UtcNow).TotalMilliseconds, 0) : null,
                    game.CurrentTaskCompletingStart.HasValue ? (int?)Math.Max((game.CurrentTaskCompletingStart.Value + TimeSpan.FromSeconds(game.Configuration.CompletingTimeLimitSeconds) - DateTime.UtcNow).TotalMilliseconds, 0) : null,
                    game.StateCounter++);
            }
        }

        private static Player? FindCurrentPlayer_orNull(Game game, string? sessionKey)
        {
            Player? result = null;

            if (sessionKey != null)
            {
                result = game.Players.SingleOrDefault(p => p.SessionKey == sessionKey);
            }

            return result;
        }

        private async Task EnsureTasksExist_ForNotFinite(string gameType, string area, IList<GameTask> existingTasksToModify, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                if (existingTasksToModify[i] == null)
                {
                    existingTasksToModify[i] = await this.taskSource.CreateTask(gameType, area, await this.areaSource.GetGeometry(area).ConfigureAwait(false), existingTasksToModify).ConfigureAwait(false);
                }
            }
        }
    }
}