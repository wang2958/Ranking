using Microsoft.AspNetCore.Mvc;
using Ranking.Services;

namespace Ranking.Controllers
{
    [ApiController]
    [Route("")]
    public class RankingController : ControllerBase
    {
        private RankingService _service;

        public RankingController(RankingService service)
        {
            _service = service;
        }

        [HttpPost("init/{size}")]
        public IActionResult InitData(int size = 100 * 10000)
        {
            var random = new Random();
            var initList = Enumerable.Range(0, size).Select(x => random.Next(1, 1000)).ToArray();

            for (int i = 0; i < size; i++)
            {
                _service.UpdateScore(customerId: (ulong)i, score: initList[i]);
            }

            return Ok();
        }

        [HttpPost("customer/{customerId}/score/{score}")]
        public IActionResult UpdateScore(ulong customerId, decimal score)
        {
            var result = _service.UpdateScore(customerId, score);
            return Ok(result);
        }

        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard(int start, int end)
        {
            var result = _service.GetLeaderboard(start, end);
            return Ok(result);
        }

        [HttpGet("leaderboard/{customerId}")]
        public IActionResult GetCustomerLeaderboard(ulong customerId, int high = 0, int low = 0)
        {
            var result = _service.GetCustomerLeaderboard(customerId, high, low);
            return Ok(result);
        }
    }
}
