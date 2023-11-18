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

            game.Lanes = testingGameData.Lanes;
            game.HostPlayer.Hand = testingGameData.HostHand;
            game.GuestPlayer.Hand = testingGameData.GuestHand;
            game.HostPlayer.Deck = testingGameData.HostDeck;
            game.HostPlayer.Deck = testingGameData.GuestDeck;
            game.RedJokerLaneIndex = testingGameData.RedJokerLaneIndex;
            game.BlackJokerLaneIndex = testingGameData.BlackJokerLaneIndex;
            game.IsHostPlayersTurn = testingGameData.IsHostPlayersTurn;

            await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameUpdated);

            return Ok();
        }
    }
}
