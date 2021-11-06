namespace TarbikMap.Hubs
{
    using Microsoft.AspNetCore.SignalR;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Hub")]
    internal class GameHub : Hub
    {
        private readonly GameStateGetter gameStateGetter;

        public GameHub(GameStateGetter gameStateGetter)
        {
            this.gameStateGetter = gameStateGetter;
        }

        public void Connect(string gameId, string sessionKey)
        {
            var state = this.gameStateGetter.GetGameState(gameId, sessionKey);

            bool isObserver = state.CurrentPlayerIndex == null;
            this.Groups.AddToGroupAsync(this.Context.ConnectionId, isObserver ? "observer_" + gameId : "joined_" + gameId + "_" + sessionKey);

            this.Clients.Caller.SendAsync("game_state", state);
        }
    }
}