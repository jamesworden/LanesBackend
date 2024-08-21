using System.ComponentModel.DataAnnotations;
using LanesBackend.Interfaces;
using LanesBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace LanesBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class TestingController : ControllerBase
  {
    private readonly IGameService GameService;

    private readonly IGameBroadcaster GameBroadcaster;

    // This api key is not used after rewriting the original API to use Vertical Slice Architecture.
    const string TESTING_API_KEY = "ThisIsASecretAPIKey123!Beans";

    public TestingController(IGameBroadcaster gameBroadcaster, IGameService gameService)
    {
      GameBroadcaster = gameBroadcaster;
      GameService = gameService;
    }

    [HttpPost(Name = "UpdateGameWithTestData")]
    public async Task<ActionResult> UpdateGameWithTestData(
      [FromBody] [Required] TestingGameData testingGameData,
      [FromQuery] [Required] string gameCode,
      [FromQuery] [Required] string apiKey
    )
    {
      var incorrectApiKey = !apiKey.Equals(TESTING_API_KEY);
      if (incorrectApiKey)
      {
        return Unauthorized();
      }

      try
      {
        var game = GameService.UpdateGame(testingGameData, gameCode);

        if (game is null)
        {
          return NotFound();
        }

        await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameUpdated);

        return Ok();
      }
      catch (Exception)
      {
        return StatusCode(500, "Error updating game data.");
      }
    }
  }
}
