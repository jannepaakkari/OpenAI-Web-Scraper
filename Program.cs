using System.Text;
using System.Text.Json;
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

        // Doesnt work without API key, so better inform user.
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("OPENAI_API_KEY not found in environment variables.");
        }

        string url = "";

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
            // Fetch HTML content with proper encoding
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var htmlBytes = await response.Content.ReadAsByteArrayAsync();
            string html = Encoding.UTF8.GetString(htmlBytes);

            // Parse HTML with HtmlAgilityPack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Select all headers (h1, h2 etc.)
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

                var headersWithPredictions = new List<object>();

                foreach (var header in headerList)
                {
                    var prediction = await OpenAIClassifier.ClassifyHeaderUsingOpenAIAsync(openAIClient, header);
                    headersWithPredictions.Add(new { header, prediction });
                }

                // Serialize the categorized headers to JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string json = JsonSerializer.Serialize(headersWithPredictions, options);
                File.WriteAllText("categorized_headers.json", json, Encoding.UTF8);
                Console.WriteLine("Headers categorized and saved to categorized_headers.json");
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
}
