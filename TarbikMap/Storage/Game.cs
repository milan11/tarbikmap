namespace TarbikMap.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Game
    {
        public Game(DateTime validUntil, GameConfiguration configuration, IList<Player> players)
        {
            this.ValidUntil = validUntil;
            this.Configuration = configuration;
            this.Players = players;
        }

        public DateTime ValidUntil { get; private set; }

        public GameConfiguration Configuration { get; private set; }

        public string? CurrentConfigurationError { get; set; }

        public IList<GameTask?>? Tasks { get; set; }

        public IList<Player> Players { get; private set; }

        public bool Starting { get; set; }

        public bool Started { get; set; }

        public string? NextGameId { get; set; }

        public DateTime? CurrentTaskAnsweringStart { get; set; }

        public DateTime? CurrentTaskCompletingStart { get; set; }

        public string? LoadedTypeLabel { get; set; }

        public string? LoadedAreaLabel { get; set; }

        public int StateCounter { get; set; }

        public int TasksCompleted
        {
            get
            {
                return this.Players.Count == 0 ? 0 : this.Players.Select(p => p.TasksCompleted).Min();
            }
        }

        public int TasksAnswered
        {
            get
            {
                return this.Players.Count == 0 ? 0 : this.Players.Select(p => p.Answers.Count).Min();
            }
        }
    }
}
