using Microsoft.AspNetCore.SignalR;

namespace LanesBackend.Hubs
{
    public class GameHub : Hub
    {

        private readonly ICacheService cacheService;

        public GameHub(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        public async Task CreateGame()
        {
            string gameCode = Guid.NewGuid().ToString()[..4];
            Console.WriteLine(gameCode);
        }

        public async Task OnDisconnectedAsync()
        {
            // Clear gamestate from cache
            // Emit a gameover message to the other person who is still connected saying they won because the opponent disconnected.
            Console.WriteLine("TODO: Client disconnected.");
        }
    }
}