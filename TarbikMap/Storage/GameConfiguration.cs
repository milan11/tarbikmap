namespace TarbikMap.Storage
{
    internal class GameConfiguration
    {
        public GameConfiguration(string type, string area, bool mapLabels, int tasksCount, int answeringTimeLimitSeconds, int completingTimeLimitSeconds)
        {
            this.Type = type;
            this.Area = area;
            this.MapLabels = mapLabels;
            this.TasksCount = tasksCount;
            this.AnsweringTimeLimitSeconds = answeringTimeLimitSeconds;
            this.CompletingTimeLimitSeconds = completingTimeLimitSeconds;
        }

        public string Type { get; set; }

        public string Area { get; set; }

        public bool MapLabels { get; set; }

        public int TasksCount { get; set; }

        public int AnsweringTimeLimitSeconds { get; set; }

        public int CompletingTimeLimitSeconds { get; set; }

        public GameConfiguration ShallowCopy()
        {
            return (GameConfiguration)this.MemberwiseClone();
        }
    }
}
