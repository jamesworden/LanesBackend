using LanesBackend.Interfaces;
using LanesBackend.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LanesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestingController : ControllerBase
    {
        private readonly IGameCache GameCache;

        private readonly IGameBroadcaster GameBroadcaster;

        const string TESTING_API_KEY = "ThisIsASecretAPIKey123!Beans";

        public TestingController(IGameCache gameCache, IGameBroadcaster gameBroadcaster)
        { 
            GameCache = gameCache;
            GameBroadcaster = gameBroadcaster;
        }

        [HttpPost(Name = "UpdateGameWithTestData")]
        public async Task<ActionResult> UpdateGameWithTestData(
            [FromBody] [Required] TestingGameData testingGameData, 
            [FromQuery] [Required] string gameCode, 
            [FromQuery] [Required] string apiKey)
        {
            var incorrectApiKey = !apiKey.Equals(TESTING_API_KEY);

            if (incorrectApiKey)
            {
                return Unauthorized();
            }

            var game = GameCache.FindGameByGameCode(gameCode);

            if (game is null)
            {
                return NotFound();
            }

            // TODO: Add other game properties necessary for testing.
            game.Lanes = testingGameData.Lanes;

            // TODO: Ensure the game that is stored in the cache actually gets updated.
            await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameUpdated);

            return Ok();
        }
    }
}
