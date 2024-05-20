using LanesBackend.Hubs;
using LanesBackend.Interfaces;
using LanesBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LanesBackend.Broadcasters
{
    public class GameBroadcaster : IGameBroadcaster
    {
        private readonly IPlayerGameViewMapper PlayerGameViewMapper;

        private readonly IHubContext<GameHub> GameHubContext;

        public GameBroadcaster(
            IPlayerGameViewMapper playerGameViewMapper,
            IHubContext<GameHub> gameHubContext
        )
        {
            PlayerGameViewMapper = playerGameViewMapper;
            GameHubContext = gameHubContext;
        }

        public async Task BroadcastPlayerGameViews(Game game, string messageType)
        {
            var hostGameView = PlayerGameViewMapper.MapToHostPlayerGameView(game);
            var guestGameView = PlayerGameViewMapper.MapToGuestPlayerGameView(game);

            var serializedHostGameView = JsonConvert.SerializeObject(
                hostGameView,
                new StringEnumConverter()
            );
            var serializedGuestGameView = JsonConvert.SerializeObject(
                guestGameView,
                new StringEnumConverter()
            );

            await GameHubContext
                .Clients.Client(game.HostConnectionId)
                .SendAsync(messageType, serializedHostGameView);
            await GameHubContext
                .Clients.Client(game.GuestConnectionId)
                .SendAsync(messageType, serializedGuestGameView);
        }
    }
}
