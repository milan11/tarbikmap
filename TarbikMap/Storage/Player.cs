namespace TarbikMap.Storage
{
    using System.Collections.Generic;

    internal class Player
    {
        public Player(string name, string sessionKey, List<PlayerAnswer> answers, int tasksCompleted)
        {
            this.Name = name;
            this.SessionKey = sessionKey;
            this.Answers = answers;
            this.TasksCompleted = tasksCompleted;
        }

        public string Name { get; private set; }

        public string SessionKey { get; private set; }

        public List<PlayerAnswer> Answers { get; private set; }

        public int TasksCompleted { get; set; }
    }
}
