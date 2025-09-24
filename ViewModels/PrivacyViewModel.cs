namespace GotHome.Models;

public class PrivacyViewModel
{
    public string GoogleMapsAPIKey { get; set; } = string.Empty;
    public string GoogleMapsMapId { get; set; } = string.Empty;
    public List<MarkerData> Markers { get; set; } = [];
}

public class MarkerData
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Color { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
