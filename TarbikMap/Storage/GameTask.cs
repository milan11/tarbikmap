namespace TarbikMap.Storage
{
    internal class GameTask
    {
        public GameTask(TaskQuestion question, TaskAnswer answer, TaskCreationDetails? creationDetails)
        {
            this.Question = question;
            this.Answer = answer;
            this.CreationDetails = creationDetails;
        }

        public TaskQuestion Question { get; private set; }

        public TaskAnswer Answer { get; private set; }

        public TaskCreationDetails? CreationDetails { get; private set; }
    }
}
