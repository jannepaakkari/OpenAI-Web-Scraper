using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebScraper.Models;

namespace WebScraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public ContentController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET api/content
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HeaderPrediction>>> GetPredictions()
        {
            if (_dbContext.HeaderPredictions == null)
            {
                return NotFound("No predictions found.");
            }

            // Fetch the last 100 predictions
            var predictions = await _dbContext.HeaderPredictions
                                              .OrderByDescending(p => p.Date)
                                              .Take(100)
                                              .ToListAsync();

            if (predictions == null || predictions.Count == 0)
            {
                return NotFound("No predictions found.");
            }

            return Ok(predictions);
        }
    }
}
