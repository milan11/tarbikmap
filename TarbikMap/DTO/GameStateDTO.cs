namespace TarbikMap.DTO
{
    using System.Collections.Generic;

    internal class GameStateDTO
    {
        public GameStateDTO(GameConfigurationDTO configuration, string? currentConfigurationError, string? loadedTypeLabel, string? loadedAreaLabel, bool starting, bool started, int tasksCompleted, int? totalTasks, IList<TaskQuestionDTO>? questions, IList<TaskAnswerDTO>? correctAnswers, IList<PlayerDTO> players, bool? currentTaskCompleted, int? currentPlayerIndex, string? nextGameId, int? currentTaskAnsweringRemainingMs, int? currentTaskCompletingRemainingMs, int stateCounter)
        {
            this.Configuration = configuration;
            this.CurrentConfigurationError = currentConfigurationError;
            this.LoadedTypeLabel = loadedTypeLabel;
            this.LoadedAreaLabel = loadedAreaLabel;
            this.Starting = starting;
            this.Started = started;
            this.TasksCompleted = tasksCompleted;
            this.TotalTasks = totalTasks;
            this.Questions = questions;
            this.CorrectAnswers = correctAnswers;
            this.Players = players;
            this.CurrentTaskCompleted = currentTaskCompleted;
            this.CurrentPlayerIndex = currentPlayerIndex;
            this.NextGameId = nextGameId;
            this.CurrentTaskAnsweringRemainingMs = currentTaskAnsweringRemainingMs;
            this.CurrentTaskCompletingRemainingMs = currentTaskCompletingRemainingMs;
            this.StateCounter = stateCounter;
        }

        public GameConfigurationDTO Configuration { get; private set; }

        public string? CurrentConfigurationError { get; private set; }

        public string? LoadedTypeLabel { get; private set; }

        public string? LoadedAreaLabel { get; private set; }

        public bool Starting { get; private set; }

        public bool Started { get; private set; }

        public int TasksCompleted { get; private set; }

        public int? TotalTasks { get; private set; }

        public IList<TaskQuestionDTO>? Questions { get; private set; }

        public IList<TaskAnswerDTO>? CorrectAnswers { get; private set; }

        public IList<PlayerDTO> Players { get; private set; }

        public bool? CurrentTaskCompleted { get; private set; }

        public int? CurrentPlayerIndex { get; private set; }

        public string? NextGameId { get; private set; }

        public int? CurrentTaskAnsweringRemainingMs { get; private set; }

        public int? CurrentTaskCompletingRemainingMs { get; private set; }

        public int StateCounter { get; private set; }
    }
}
