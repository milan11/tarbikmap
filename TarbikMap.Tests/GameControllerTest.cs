namespace TarbikMap.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using TarbikMap.AreaSources;
    using TarbikMap.Controllers;
    using TarbikMap.DTO;
    using TarbikMap.Storage;
    using TarbikMap.TaskSources;
    using Xunit;

    public class GameControllerTest
    {
        [Fact]
        public async void Test()
        {
            var logger = new Mock<ILogger<GameController>>();
            var storage = new StorageMain();
            var areaSource = new CommonAreaSource();
            var taskSource = new WikidataTaskSource();
            var gameStateGetter = new GameStateGetter(storage, areaSource, taskSource);
            var environmentConfig = new EnvironmentConfig();
            var controller = new GameController(logger.Object, null, storage, areaSource, taskSource, Factories.CreateDownloader(environmentConfig), environmentConfig, gameStateGetter);

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext()
            {
                HttpContext = new DefaultHttpContext(),
            };

            var cookies_observer = new RequestCookies(new Dictionary<string, string>() { });

            string gameId = (await controller.CreateGame().ConfigureAwait(false)).GameId;
            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_observer;

                Assert.Throws<InvalidOperationException>(() => controller.StartGame(gameId));

                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.False(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Null(state.TotalTasks);
                Assert.Null(state.Questions);
                Assert.Null(state.CorrectAnswers);
                Assert.Empty(state.Players);
                Assert.Null(state.CurrentTaskCompleted);
                Assert.Null(state.CurrentPlayerIndex);
            }

            controller.JoinGame(gameId, "Player1");
            var cookies_player1 = new RequestCookies(new Dictionary<string, string>() { { "session_key", GameController.LastSetCookieForTests! } });

            controller.JoinGame(gameId, "Player2");
            var cookies_player2 = new RequestCookies(new Dictionary<string, string>() { { "session_key", GameController.LastSetCookieForTests! } });
            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_player1;
                Assert.Throws<InvalidOperationException>(() => controller.ProvideAnswer(gameId, 0, new ProvidedAnswerDTO(1, 1)));
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 0));
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_observer;
                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.False(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Null(state.TotalTasks);
                Assert.Null(state.Questions);
                Assert.Null(state.CorrectAnswers);
                Assert.Equal(2, state.Players.Count);
                Assert.Null(state.CurrentTaskCompleted);
                Assert.Null(state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_observer;
                controller.StartGame(gameId);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_observer;
                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Single(state.Questions!);
                Assert.Empty(state.CorrectAnswers);
                Assert.Equal(2, state.Players.Count);
                Assert.Null(state.CurrentTaskCompleted);
                Assert.Null(state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_player1;
                Assert.Throws<InvalidOperationException>(() => controller.ProvideAnswer(gameId, 1, new ProvidedAnswerDTO(1, 1)));
                Assert.Throws<InvalidOperationException>(() => controller.ProvideAnswer(gameId, 2, new ProvidedAnswerDTO(1, 1)));
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 0));
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 1));
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 2));
                controller.ProvideAnswer(gameId, 0, new ProvidedAnswerDTO(1, 1));

                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Single(state.Questions!);
                Assert.Single(state.CorrectAnswers!);
                Assert.Equal(2, state.Players.Count);
                Assert.Single(state.Players[0].Answers);
                Assert.Empty(state.Players[1].Answers);
                Assert.False(state.CurrentTaskCompleted);
                Assert.Equal(0, state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_player2;

                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Single(state.Questions!);
                Assert.Empty(state.CorrectAnswers);
                Assert.Equal(2, state.Players.Count);
                Assert.Empty(state.Players[0].Answers);
                Assert.Empty(state.Players[1].Answers);
                Assert.False(state.CurrentTaskCompleted);
                Assert.Equal(1, state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_observer;
                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Single(state.Questions!);
                Assert.Empty(state.CorrectAnswers);
                Assert.Equal(2, state.Players.Count);
                Assert.Empty(state.Players[0].Answers);
                Assert.Empty(state.Players[1].Answers);
                Assert.Null(state.CurrentTaskCompleted);
                Assert.Null(state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_player2;
                controller.ProvideAnswer(gameId, 0, new ProvidedAnswerDTO(2, 2));

                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Single(state.Questions!);
                Assert.Single(state.CorrectAnswers!);
                Assert.Equal(2, state.Players.Count);
                Assert.Single(state.Players[0].Answers);
                Assert.Single(state.Players[1].Answers);
                Assert.False(state.CurrentTaskCompleted);
                Assert.Equal(1, state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_observer;
                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Single(state.Questions!);
                Assert.Single(state.CorrectAnswers!);
                Assert.Equal(2, state.Players.Count);
                Assert.Null(state.CurrentTaskCompleted);
                Assert.Null(state.CurrentTaskCompleted);
                Assert.Null(state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_player1;
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 1));
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 2));
                controller.CompleteTask(gameId, 0);
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 0));

                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Single(state.Questions!);
                Assert.Single(state.CorrectAnswers!);
                Assert.Equal(2, state.Players.Count);
                Assert.Single(state.Players[1].Answers);
                Assert.Single(state.Players[1].Answers);
                Assert.True(state.CurrentTaskCompleted);
                Assert.Equal(0, state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_player2;

                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(0, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Single(state.Questions!);
                Assert.Single(state.CorrectAnswers!);
                Assert.Equal(2, state.Players.Count);
                Assert.Single(state.Players[0].Answers);
                Assert.Single(state.Players[1].Answers);
                Assert.False(state.CurrentTaskCompleted);
                Assert.Equal(1, state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_player2;
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 1));
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 2));
                controller.CompleteTask(gameId, 0);
                Assert.Throws<InvalidOperationException>(() => controller.CompleteTask(gameId, 0));

                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(1, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Equal(2, state.Questions!.Count);
                Assert.Single(state.CorrectAnswers!);
                Assert.Equal(2, state.Players.Count);
                Assert.Single(state.Players[1].Answers);
                Assert.Single(state.Players[1].Answers);
                Assert.False(state.CurrentTaskCompleted);
                Assert.Equal(1, state.CurrentPlayerIndex);
            }

            {
                controller.ControllerContext.HttpContext.Request.Cookies = cookies_observer;
                var state = gameStateGetter.GetGameState(gameId, controller.ControllerContext.HttpContext.Request.Cookies["session_key"]);
                Assert.True(state.Started);
                Assert.Equal(1, state.TasksCompleted);
                Assert.Equal(5, state.TotalTasks);
                Assert.Equal(2, state.Questions!.Count);
                Assert.Single(state.CorrectAnswers!);
                Assert.Equal(2, state.Players.Count);
                Assert.Null(state.CurrentTaskCompleted);
                Assert.Null(state.CurrentPlayerIndex);
            }
        }

        private class RequestCookies : IRequestCookieCollection
        {
            private Dictionary<string, string> cookies;

            public RequestCookies(Dictionary<string, string> cookies)
            {
                this.cookies = cookies;
            }

            public int Count => this.cookies.Count;

            public ICollection<string> Keys => this.cookies.Keys;

            public string this[string key] => this.cookies[key];

            public bool ContainsKey(string key)
            {
                return this.cookies.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return this.cookies.GetEnumerator();
            }

            public bool TryGetValue(string key, out string value)
            {
                return this.cookies.TryGetValue(key, out value!);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.cookies.GetEnumerator();
            }
        }
    }
}
