namespace WebScraper.Models
{
    public class HeaderPrediction
    {
        public int Id { get; set; }
        public required string Header { get; set; }
        public required string Prediction { get; set; }
        public required string Source { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}