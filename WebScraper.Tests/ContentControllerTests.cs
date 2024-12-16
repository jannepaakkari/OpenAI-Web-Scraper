using Microsoft.EntityFrameworkCore;
using WebScraper.Controllers;
using WebScraper.Models;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.Tests
{
    public class ContentControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public ContentControllerTests()
        {
            // Setup InMemoryDatabase for testing
            _options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseInMemoryDatabase("TestDb")
                        .Options;
        }

        [Fact]
        public async Task GetPredictions_ReturnsNotFound_WhenNoPredictionsExist()
        {
            // Arrange
            using (var context = new AppDbContext(_options))
            {
                // Ensure the database is empty
                context.HeaderPredictions.RemoveRange(context.HeaderPredictions);
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ContentController(context);

                var result = await controller.GetPredictions();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<HeaderPrediction>>>(result);
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
                Assert.Equal("No predictions found.", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task GetPredictions_ReturnsOkResult_WhenPredictionsExist()
        {
            using (var context = new AppDbContext(_options))
            {
                // Add 2 predictions of the topics to the database
                context.HeaderPredictions.Add(new HeaderPrediction
                {
                    Id = 1,
                    Date = DateTime.UtcNow,
                    Prediction = "Test Prediction 1",
                    Header = "Test Header 1",
                    Source = "https://aijdsfjsdkgslgsdf.com"
                });
                context.HeaderPredictions.Add(new HeaderPrediction
                {
                    Id = 2,
                    Date = DateTime.UtcNow.AddMinutes(1),
                    Prediction = "Test Prediction 2",
                    Header = "Test Header 2",
                    Source = "https://jdsfsdlgjksdjgkldsgks.com"
                });
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ContentController(context);

                var result = await controller.GetPredictions();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<HeaderPrediction>>>(result);
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var predictions = Assert.IsAssignableFrom<List<HeaderPrediction>>(okResult.Value);

                // Check if there are 2 predictions in the response
                Assert.Equal(2, predictions.Count);
            }
        }
    }
}
