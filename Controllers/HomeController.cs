using System.Diagnostics;
using GotHome.Models;
using Microsoft.AspNetCore.Mvc;

namespace GotHome.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;

    public HomeController(ILogger<HomeController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        var vm = new PrivacyViewModel
        {
            GoogleMapsAPIKey = _config["GoogleMapsJSKey"] ?? "",
            GoogleMapsMapId = _config["GoogleMapId"] ?? "",
            Markers = new List<MarkerData>
            {
                new MarkerData
                {
                    Lat = 32.5358053807361,
                    Lng = -97.09577927978566,
                    Color = "green",
                    Title = "Home",
                },
                new MarkerData
                {
                    Lat = 32.747519,
                    Lng = -97.092994,
                    Color = "orange",
                    Title = "Event location",
                },
            },
        };
        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
