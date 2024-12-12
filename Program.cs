using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using System.Text.Encodings.Web;

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
    // Fetch HTML content
    var response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();
    var htmlBytes = await response.Content.ReadAsByteArrayAsync();
    // Decode as UTF-8
    string html = Encoding.UTF8.GetString(htmlBytes);

    // Parse HTML with HtmlAgilityPack
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

        // Serialize the list to JSON
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Prevents escaping of non-ASCII characters
        };

        string json = JsonSerializer.Serialize(headerList, options);

        // Save JSON to a file with UTF-8 encoding
        File.WriteAllText("headers.json", json, Encoding.UTF8);
        Console.WriteLine("Headers found and saved to headers.json");
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
    Console.WriteLine($"Expection: {ex.Message}");
}
