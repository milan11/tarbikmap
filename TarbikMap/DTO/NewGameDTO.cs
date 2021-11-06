namespace TarbikMap.DTO
{
    internal class NewGameDTO
    {
        public NewGameDTO(string gameId)
        {
            this.GameId = gameId;
        }

        public string GameId { get; private set; }
    }
}
