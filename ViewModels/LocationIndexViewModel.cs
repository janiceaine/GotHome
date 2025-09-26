namespace GotHome.ViewModels;

using GotHome.Models;

public class LocationIndexViewModel
{
    // a list of all the events displayed in each cards
    public List<LocationPingViewModel> LocationPings { get; set; } = [];
    public string GoogleMapsAPIKey { get; set; } = string.Empty;
    public string GoogleMapsMapId { get; set; } = string.Empty;
    public List<MarkerDataClass> Markers { get; set; } = [];
}

public class MarkerDataClass
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Color { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
