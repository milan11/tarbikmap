namespace TarbikMap.DTO
{
    internal class GameConfigurationDTO
    {
        public GameConfigurationDTO(string type, string area, int tasksCount, int answeringTimeLimitSeconds, int completingTimeLimitSeconds)
        {
            this.Type = type;
            this.Area = area;
            this.TasksCount = tasksCount;
            this.AnsweringTimeLimitSeconds = answeringTimeLimitSeconds;
            this.CompletingTimeLimitSeconds = completingTimeLimitSeconds;
        }

        public string Type { get; private set; }

        public string Area { get; private set; }

        public int TasksCount { get; private set; }

        public int AnsweringTimeLimitSeconds { get; private set; }

        public int CompletingTimeLimitSeconds { get; private set; }
    }
}
