public class HeaderPrediction
{
    public int Id { get; set; }
    public required string Header { get; set; }
    public required string Prediction { get; set; }
    public DateTime Date { get; set; }

    public HeaderPrediction()
    {
        var finlandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
        Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, finlandTimeZone);
    }
}