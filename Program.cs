using System.Text;
using HtmlAgilityPack;
using OpenAI.Chat;
using DotNetEnv;
using Webscaper.OpenAI;

class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();
        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("OPENAI_API_KEY not found in environment variables.");
            return;
        }

        string url = "";

        if (url.Length == 0)
        {
            Console.WriteLine("Please provide a URL to scrape.");
            return;
        }

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

            var headers = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");

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

                await SaveHeadersToDatabase(headersWithPredictions, url);
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

    // Save headers and predictions to SQLite database
    private static async Task SaveHeadersToDatabase(List<HeaderPredictionData> headersWithPredictions, string url)
    {
        using var dbContext = new AppDbContext();

        foreach (var item in headersWithPredictions)
        {
            // Check if both Header and Prediction are not null or empty
            if (!string.IsNullOrEmpty(item.Header) && !string.IsNullOrEmpty(item.Prediction))
            {
                var headerPrediction = new HeaderPrediction
                {
                    Header = item.Header,
                    Prediction = item.Prediction,
                    Source = url
                };

                // Add the new entry to the context
                dbContext.HeaderPredictions?.Add(headerPrediction);
            }
            else
            {
                Console.WriteLine("Skipping entry: Header or Prediction is empty.");
            }
        }

        // Save changes to SQLite database
        await dbContext.SaveChangesAsync();

        Console.WriteLine("Headers categorized and saved to SQLite database.");
    }
}