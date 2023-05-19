using LanesBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace LanesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestingController : ControllerBase
    {
        [HttpPost(Name = "UpdateGameWithTestData")]
        public string UpdateGameWithTestData(string gameCode, [FromBody] TestingGameData testingGameData)
        {
            return "Test";
        }
    }
}
