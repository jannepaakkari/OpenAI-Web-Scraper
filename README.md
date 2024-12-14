
### WebScraper Implementation with AI

This WebScraper application is designed to scrape selected websites and collect topics. It leverages AI to analyze and predict the categorization of the collected topics. The analysis and scraped data are stored in an SQLite database. Additionally, the application provides an API that can be used to extract data for frontend applications and other purposes.

## Prerequisites

- .NET 9.0 SDK
- SQLite

## Installation

1. Clone the repository:
```bash
   git clone https://github.com/jannepaakkari/Web-scraper.git
   cd WebScraper
```

2. Set up settings
- Set up .env file and add OPENAI_API_KEY='your_key'
- Modify appsettings.json:
```bash
    "Url": Set url you want to scape,
    "RunAPI": Set true you want to run APIs, not neccessary for scraper itself,
    "ScrapingNodes": "Nodes you want to scape, depends on site,
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Build the project:
```bash
dotnet build
```

5. Run the application:
```bash
dotnet run
```

6. To apply the latest database migrations, use the following command:
```bash
dotnet ef database update
```