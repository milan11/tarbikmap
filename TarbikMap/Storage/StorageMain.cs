namespace TarbikMap.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class StorageMain
    {
        private Dictionary<string, Game> games = new Dictionary<string, Game>();

        public Game FindGame(string gameId)
        {
            lock (this.games)
            {
                if (this.games.TryGetValue(gameId, out Game? value))
                {
                    return value!;
                }
                else
                {
                    throw new InvalidOperationException("Game not found: " + gameId);
                }
            }
        }

        public bool GameExists(string gameId)
        {
            lock (this.games)
            {
                return this.games.ContainsKey(gameId);
            }
        }

        public void AddGame(string createdGameId, Game game)
        {
            lock (this.games)
            {
                foreach (var key in this.games.Where(kv => kv.Value.ValidUntil < DateTime.UtcNow).Select(kv => kv.Key).ToList())
                {
                    this.games.Remove(key);
                }

                if (this.games.ContainsKey(createdGameId))
                {
                    throw new InvalidOperationException("Game ID already exists");
                }

                this.games.Add(createdGameId, game);
            }
        }
    }
}
