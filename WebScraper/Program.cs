using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using OpenAI.Chat;
using System.Text;
using HtmlAgilityPack;
using Webscaper.OpenAI;
using WebScraper.Models;

/*
- Make .env file at root of /WebScraper and add your OPENAI_API_KEY="key"
- Configure /WebScraper/appsettings.json to set up which nodes you want to scrape, url and if you want to run apis (running apis is not neccessary)
*/
class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        Env.Load();

        // Start the web server (ASP.NET Core)
        var host = CreateHostBuilder(args).Build();

        var config = host.Services.GetRequiredService<IConfiguration>();

        // Set this at .env file (make file at root). You need your own OPENAI_API_KEY.
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;

        bool runAPI = bool.TryParse(config["ScraperSettings:RunAPI"], out bool result) && result;

        if (!string.IsNullOrEmpty(apiKey))
        {
            string url = config["ScraperSettings:Url"] ?? string.Empty;
            // This depends on site you are scrabing.
            string scrabingNodes = config["ScraperSettings:ScrapingNodes"] ?? "//h1|//h2|//h3|//h4|//h5|//h6";

            // Run the scraper in the background
            if (url != string.Empty)
            {
                await RunScraperAsync(apiKey, url, scrabingNodes);
            }
            else
            {
                Console.WriteLine("URL not found in appsettings.json.");
            }
        }
        else
        {
            Console.WriteLine("OPENAI_API_KEY not found in environment variables.");
        }

        // Run the web API
        if (runAPI)
        {
            await host.RunAsync();
        }
    }

    // Configure and build the web API
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    // DbContext configuration
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite("Data Source=HeadersDatabase.db"));

                    // Add API controllers
                    services.AddControllers();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            });

    private static async Task RunScraperAsync(string apiKey, string url, string scrabingNodes)
    {
        using HttpClient client = new HttpClient(new HttpClientHandler
        {
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12
        })
        {
            DefaultRequestVersion = new Version(1, 1)
        };

        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; WebScraper/1.0)");

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var htmlBytes = await response.Content.ReadAsByteArrayAsync();
            string html = Encoding.UTF8.GetString(htmlBytes);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var headers = doc.DocumentNode.SelectNodes(scrabingNodes);

            if (headers != null)
            {
                var headerList = new List<string>();
                foreach (var header in headers)
                {
                    headerList.Add(header.InnerText.Trim());
                }

                // Create OpenAI client
                ChatClient openAIClient = new(model: "gpt-4o", apiKey: apiKey);

                var headersWithPredictions = new List<HeaderPredictionData>();

                foreach (var header in headerList)
                {
                    var prediction = await OpenAIClassifier.ClassifyHeaderUsingOpenAIAsync(openAIClient, header);
                    headersWithPredictions.Add(new HeaderPredictionData
                    {
                        Header = header,
                        Prediction = prediction
                    });
                }

                // Manually configure DbContext options and pass to AppDbContext
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite("Data Source=HeadersDatabase.db");

                var dbContext = new AppDbContext(optionsBuilder.Options);

                await SaveHeadersToDatabase(dbContext, headersWithPredictions, url);
            }
            else
            {
                Console.WriteLine("No headers found.");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
        }
    }

    private static async Task SaveHeadersToDatabase(AppDbContext dbContext, List<HeaderPredictionData> headersWithPredictions, string url)
    {
        foreach (var item in headersWithPredictions)
        {
            if (!string.IsNullOrEmpty(item.Header) && !string.IsNullOrEmpty(item.Prediction))
            {
                var headerPrediction = new HeaderPrediction
                {
                    Header = item.Header,
                    Prediction = item.Prediction,
                    Source = url
                };

                dbContext.HeaderPredictions?.Add(headerPrediction);
            }
            else
            {
                Console.WriteLine("Skipping entry: Header or Prediction is empty.");
            }
        }

        try
        {
            await dbContext.SaveChangesAsync();
            Console.WriteLine("Headers categorized and saved to SQLite database.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save changes to the database: {ex}");
        }
    }
}
