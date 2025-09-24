using GotHome.Models;
using GotHome.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GotHome.Controllers;

[Route("events")]
public class EventsController : Controller
{
    private readonly ApplicationContext _context;
    private const string SessionUserId = "userId";

    public EventsController(ApplicationContext context)
    {
        _context = context;
    }

    // View all Events
    [HttpGet]
    public async Task<IActionResult> EventsIndex()
    {
        // getting current user's id fron session
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        // Redirect to SignIn if user is not signed in
        if (userId is not int uid)
        {
            return RedirectToAction("LoginForm", "Account", new { message = "not-authenticated" });
        }

        // Displays all Events in a list
        var vm = new EventsIndexViewModel
        {
            Events = await _context
                .Events.AsNoTracking()
                .Include(e => e.Invites)
                .Include(e => e.User)
                .OrderByDescending(e => e.CreatedAt) // Show newest first
                .Select(e => new EventsRowViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    UploadedBy = e.User!.UserName,
                    UploaderId = e.UserId,
                    CreateDate = e.CreatedAt.ToString("MMMM dd, yyyy"),
                    InviteCount = e.Invites.Count(),
                })
                .ToListAsync(),
        };
        return View(vm);
    }

    // Get new event
    [HttpGet("new")]
    public IActionResult NewEventsForm()
    {
        var vm = new EventFormViewModel();
        return View(vm);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormViewModel vm)
    {
        // Normalize input
        vm.Title = (vm.Title ?? string.Empty).Trim();
        vm.Description = (vm.Description ?? string.Empty).Trim();
        vm.Location = (vm.Location ?? string.Empty).Trim();

        // Check authentication
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
        {
            return RedirectToAction(
                nameof(AccountController.LoginForm),
                "Account",
                new { message = "not-authenticated" }
            );
        }

        // Validate model
        if (!ModelState.IsValid)
        {
            return View(nameof(NewEventsForm), vm);
        }

        // Extra business rule: prevent past events
        if (vm.StartTime < DateTime.UtcNow)
        {
            ModelState.AddModelError(nameof(vm.StartTime), "Start time cannot be in the past.");
            return View(nameof(NewEventsForm), vm);
        }

        var newEvent = new Event
        {
            Title = vm.Title,
            Description = vm.Description,
            Location = vm.Location,
            StartTime = vm.StartTime,
            UserId = userId.Value,
            CreatedAt = DateTime.UtcNow,
        };

        await _context.Events.AddAsync(newEvent);
        await _context.SaveChangesAsync();

        TempData["ToastMessage"] = "🎉 Event created!";
        return RedirectToAction(nameof(EventDetails), new { id = newEvent.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> EventDetails(int id)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
            return RedirectToAction("LoginForm", "Account");

        var evt = await _context
            .Events.Include(e => e.User)
            .Include(e => e.Invites)
            .Include(e => e.RSVPs)
            .ThenInclude(r => r.User)
            .Include(e => e.LocationPings)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evt is null)
            return NotFound();

        var vm = new EventDetailsViewModel
        {
            Id = evt.Id,
            Title = evt.Title,
            Description = evt.Description,
            Location = evt.Location,
            StartTime = evt.StartTime,
            UploadedBy = evt.User?.UserName ?? "Unknown",
            Invitees = evt.Invites.Select(i => i.RecipientEmail).ToList(),
            RSVPs = evt.RSVPs.Select(r => r.User?.UserName ?? "Unknown").ToList(),
            LocationPings = evt.LocationPings.OrderByDescending(p => p.Timestamp).ToList(),
        };

        return View(vm);
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> EditEvent(int id)
    {
        // getting current user's id fron session
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        // Redirect to Sign In if user is not signed in
        if (userId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { message = "not-authenticated" });
        }

        var maybeEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (maybeEvent is null)
        {
            return NotFound("No Event was Found.");
        }

        // check if User matches event.UserId

        var vm = new EventFormViewModel
        {
            Id = maybeEvent.Id,
            Title = maybeEvent.Title,
            Description = maybeEvent.Description,
            Location = maybeEvent.Location,
            StartTime = maybeEvent.StartTime,
        };

        // return View(vm);
        return View(vm);
    }

    [HttpPost("{id}/edit/process")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProcess(int id, EventFormViewModel vm)
    {
        // Normalize input
        vm.Title = (vm.Title ?? string.Empty).Trim();
        vm.Description = (vm.Description ?? string.Empty).Trim();
        vm.Location = (vm.Location ?? string.Empty).Trim();

        // Authentication check
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
        {
            return RedirectToAction(
                nameof(AccountController.LoginForm),
                "Account",
                new { message = "not-authenticated" }
            );
        }

        // Model validation
        if (!ModelState.IsValid)
        {
            return View(nameof(EditEvent), vm);
        }

        // Load event and enforce ownership
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (evt is null)
        {
            return NotFound("No Event was Found.");
        }

        // Update fields
        evt.Title = vm.Title;
        evt.Description = vm.Description;
        evt.Location = vm.Location;
        evt.StartTime = vm.StartTime;
        evt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["ToastMessage"] = "Event updated!";
        return RedirectToAction(nameof(EventDetails), new { id });
    }

    // POST /events/{id}/rsvp
    [HttpPost("{id}/rsvp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RSVP(int id)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
        {
            return RedirectToAction(
                nameof(AccountController.LoginForm),
                "Account",
                new { message = "not-authenticated" }
            );
        }

        // Prevent duplicate RSVP
        var alreadyRSVPed = await _context.RSVPs.AnyAsync(r =>
            r.EventId == id && r.UserId == userId
        );

        if (!alreadyRSVPed)
        {
            _context.RSVPs.Add(
                new RSVP
                {
                    EventId = id,
                    UserId = userId.Value,
                    Timestamp = DateTime.UtcNow,
                }
            );
            await _context.SaveChangesAsync();
        }

        TempData["ToastMessage"] = "✅ RSVP confirmed!";
        return RedirectToAction(nameof(EventDetails), new { id });
    }

    // POST /events/{id}/invite
    [HttpPost("{id}/invite")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(int id, InviteFormViewModel vm)
    {
        // Normalize input
        vm.RecipientEmail = (vm.RecipientEmail ?? string.Empty).Trim();
        vm.Message = (vm.Message ?? string.Empty).Trim();

        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
        {
            return RedirectToAction(
                nameof(AccountController.LoginForm),
                "Account",
                new { message = "not-authenticated" }
            );
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(EventDetails), new { id });
        }

        var invite = new Invite
        {
            EventId = id,
            SenderId = userId.Value,
            RecipientEmail = vm.RecipientEmail,
            Message = vm.Message,
            SentAt = DateTime.UtcNow,
        };

        _context.Invites.Add(invite);
        await _context.SaveChangesAsync();

        TempData["ToastMessage"] = "✉️ Invite sent!";
        return RedirectToAction(nameof(EventDetails), new { id });
    }

    // POST /events/{id}/ping
    [HttpPost("{id}/ping")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PingLocation(int id, LocationPingViewModel vm)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
        {
            return Unauthorized();
        }

        var ping = new LocationPing
        {
            EventId = id,
            UserId = userId.Value,
            Latitude = vm.Latitude,
            Longitude = vm.Longitude,
            Timestamp = DateTime.UtcNow,
        };

        _context.LocationPings.Add(ping);
        await _context.SaveChangesAsync();

        // Since this is more API-like, return a simple result
        return Ok(new { success = true, message = "Location ping saved." });
    }

    // POST /events/{id}/wrap
    [HttpPost("{id}/wrap")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WrapEvent(int id)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
        {
            return RedirectToAction(
                nameof(AccountController.LoginForm),
                "Account",
                new { message = "not-authenticated" }
            );
        }

        // Only the owner can wrap the event
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (evt is null)
        {
            return NotFound("No Event was Found.");
        }

        evt.IsWrappedUp = true;
        evt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["ToastMessage"] = "🎉 Event wrapped up!";
        return RedirectToAction(nameof(EventDetails), new { id });
    }

    [HttpGet("{id}/delete")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        // Authentication check
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
        {
            return RedirectToAction(
                nameof(AccountController.LoginForm),
                "Account",
                new { message = "not-authenticated" }
            );
        }

        // Load event and enforce ownership
        var evt = await _context
            .Events.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (evt is null)
        {
            return NotFound("No Event was Found.");
        }

        var vm = new ConfirmDeleteViewModel { Id = evt.Id };
        return View(nameof(ConfirmDelete), vm);
    }

    [HttpPost("{id}/destroy")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id, ConfirmDeleteViewModel vm)
    {
        // Route id vs. form id mismatch
        if (id != vm.Id)
        {
            return BadRequest("Mismatched event IDs.");
        }

        // Authentication check
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is null)
        {
            return RedirectToAction(
                nameof(AccountController.LoginForm),
                "Account",
                new { message = "not-authenticated" }
            );
        }

        // Load event and enforce ownership
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (evt is null)
        {
            return NotFound("No Event was Found.");
        }

        // Delete + persist
        _context.Events.Remove(evt);
        await _context.SaveChangesAsync();

        TempData["ToastMessage"] = "🗑️ Event deleted!";
        return RedirectToAction(nameof(EventsIndex));
    }
}
