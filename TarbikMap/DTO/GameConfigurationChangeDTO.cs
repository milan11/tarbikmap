namespace TarbikMap.DTO
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from ASP.NET)")]
    internal class GameConfigurationChangeDTO
    {
        public string? Type { get; set; }

        public string? Area { get; set; }

        public int? TasksCount { get; set; }

        public int? AnsweringTimeLimitSeconds { get; set; }

        public int? CompletingTimeLimitSeconds { get; set; }
    }
}
