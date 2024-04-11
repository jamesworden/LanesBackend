using LanesBackend.Exceptions;
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
        private readonly IGameService GameService;

        private readonly IGameBroadcaster GameBroadcaster;

        // TODO: Remove from commit history.
        const string TESTING_API_KEY = "ThisIsASecretAPIKey123!Beans";

        public TestingController(
            IGameCache gameCache, 
            IGameBroadcaster gameBroadcaster, 
            IGameService gameService)
        { 
            GameCache = gameCache;
            GameBroadcaster = gameBroadcaster;
            GameService = gameService;
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

            try
            {
                var game = GameService.UpdateGame(testingGameData, gameCode);
                await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameUpdated);

                return Ok();
            } catch (GameNotExistsException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error updating game data.");
            }
        }
    }
}
