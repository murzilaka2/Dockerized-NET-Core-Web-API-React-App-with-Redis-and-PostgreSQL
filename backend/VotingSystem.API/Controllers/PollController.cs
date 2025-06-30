using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingSystem.API.Data;
using VotingSystem.API.Models;
using VotingSystem.API.Services;

namespace VotingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IRedisService _redis;
        public PollController(ApplicationContext context, IRedisService redis)
        {
            _context = context;
            _redis = redis;
        }

        [HttpGet]
        public IActionResult GetPolls()
        {
            var polls = _context.Polls
                .Select(p => new { p.Id, p.Title })
                .ToList();
            return Ok(polls);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPollDetails(int id)
        {
            string cacheKey = $"poll:{id}:details";

            // 1. Попробовать получить из Redis
            var cached = await _redis.GetCachedValueAsync(cacheKey);
            if (cached != null)
            {
                var pollFromCache = System.Text.Json.JsonSerializer.Deserialize<Poll>(cached);
                return Ok(pollFromCache);
            }

            // 2. Получить из базы данных
            var poll = _context.Polls
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    Questions = p.Questions.Select(q => new
                    {
                        q.Id,
                        q.Text,
                        Options = q.Options.Select(o => new { o.Id, o.Text })
                    })
                }).FirstOrDefault();

            if (poll == null)
                return NotFound();

            // 3. Сериализовать и сохранить в Redis
            var json = System.Text.Json.JsonSerializer.Serialize(poll);
            await _redis.SetCachedValueAsync(cacheKey, json, TimeSpan.FromMinutes(10)); // кэш на 10 минут
            return Ok(poll);
        }

        [HttpPost("vote")]
        public async Task<IActionResult> SubmitVote([FromBody] List<int> answerOptionIds)
        {
            var votes = answerOptionIds.Select(id => new Vote { AnswerOptionId = id }).ToList();
            _context.Votes.AddRange(votes);
            await _context.SaveChangesAsync();

            // Удаление кэша по связанным опросам
            var pollIds = _context.AnswerOptions
                .Where(o => answerOptionIds.Contains(o.Id))
                .Include(o => o.Question)
                .Select(o => o.Question.PollId)
                .Distinct()
                .ToList();

            foreach (var pollId in pollIds)
            {
                string cacheKey = $"poll:{pollId}:results";
                await _redis.RemoveAsync(cacheKey);
            }

            return Ok();
        }

        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(int id)
        {
            string cacheKey = $"poll:{id}:results";
            var cached = await _redis.GetCachedValueAsync(cacheKey);
            if (cached != null)
            {
                var pollFromCache = System.Text.Json.JsonSerializer.Deserialize<Poll>(cached);
                return Ok(pollFromCache);
            }

            var poll = _context.Polls
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Options)
                    .ThenInclude(o => o.Votes)
                .FirstOrDefault(p => p.Id == id);

            if (poll == null)
                return NotFound();

            var results = poll.Questions.Select(q => new {
                q.Text,
                Options = q.Options.Select(o => new {
                    o.Text,
                    Votes = o.Votes.Count,
                    Percentage = o.Votes.Count == 0 ? 0 :
                                 (double)o.Votes.Count / q.Options.Sum(opt => opt.Votes.Count) * 100
                })
            });

            var json = System.Text.Json.JsonSerializer.Serialize(results);
            await _redis.SetCachedValueAsync(cacheKey, json, TimeSpan.FromMinutes(5)); // Кэшируем на 5 минут

            return Ok(results);
        }

        [HttpPost("clear-cache")]
        public async Task<IActionResult> ClearCache()
        {
            await _redis.ClearAllAsync();
            return Ok(new { message = "The cache has been successfully cleared." });
        }
    }
}
