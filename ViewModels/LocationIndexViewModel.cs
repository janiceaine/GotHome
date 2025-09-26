namespace GotHome.ViewModels;

using GotHome.Models;

public class LocationIndexViewModel
{
    // a list of all the events displayed in each cards
    public List<LocationPingViewModel> LocationPings { get; set; } = [];
}
