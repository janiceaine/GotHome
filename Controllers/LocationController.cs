using GotHome.Models;
using GotHome.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GotHome.Controllers;

public class LocationController : Controller
{
    private readonly ApplicationContext _context;
    private const string SessionUserId = "userId";
    private readonly IConfiguration _config;

    public LocationController(ApplicationContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // View all Events for a user
    [HttpGet]
    [Route("location")]
    public async Task<IActionResult> LiveTracking()
    {
        // getting current user's id fron session
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        // Redirect to SignIn if user is not signed in
        if (userId is not int uid)
        {
            return RedirectToAction("LoginForm", "Account", new { message = "not-authenticated" });
        }

        var eventLive = await _context
            .Events.AsNoTracking()
            .FirstOrDefaultAsync(eventMarkers => eventMarkers.IsLiveTracking == true);

        var eventMarkers = new MarkerDataClass();
        if (eventLive is not null)
        {
            eventMarkers.Lat = 32.912366;
            eventMarkers.Lng = -96.890316;
            eventMarkers.Color = "purple";
            eventMarkers.Title = eventLive.Title;
        }

        // Displays all Locations in a list
        var vm = new LocationIndexViewModel
        {
            LocationPings = await _context
                .LocationPings.AsNoTracking()
                .Include(p => p.User)
                .ThenInclude(u => u!.Profile)
                .Include(p => p.Event)
                .Where(p => p.Event!.IsLiveTracking == true)
                .OrderByDescending(p => p.Timestamp) // Show newest first
                .Select(p => new LocationPingViewModel
                {
                    Id = p.Id,
                    EventId = p.EventId,
                    UserId = p.UserId,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Timestamp = p.Timestamp,
                    LocationStatus = p.LocationStatus,
                    Profile = p.User!.Profile,
                    Event = p.Event,
                })
                .ToListAsync(),

            GoogleMapsAPIKey = _config["GoogleMapsJSKey"] ?? "",
            GoogleMapsMapId = _config["GoogleMapId"] ?? "",
            Markers = new List<MarkerDataClass> { eventMarkers },
        };

        return View(vm);
    }
}
