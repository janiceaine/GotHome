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
                })
                .ToListAsync(),
        };

        return View(vm);
    }
}
