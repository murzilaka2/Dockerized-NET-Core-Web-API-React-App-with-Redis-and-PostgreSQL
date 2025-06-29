using Microsoft.AspNetCore.Mvc;
using VotingSystem.API.Data;
using VotingSystem.API.Models;

namespace VotingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoteController : ControllerBase
    {
        private readonly ApplicationContext _context;
        public VoteController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostVote([FromBody] Vote vote)
        {
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public IActionResult GetVotes()
        {
            var result = _context.Votes
                .GroupBy(v => v.Option)
                .Select(g => new { Option = g.Key, Count = g.Count() })
                .ToList();
            return Ok(result);
        }
    }
}
