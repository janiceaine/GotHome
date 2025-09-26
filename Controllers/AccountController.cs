using GotHome.Models;
using GotHome.Services;
using GotHome.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GotHome.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly ApplicationContext _context;
    private readonly IPasswordService _passwords;
    private readonly IImageService _images;
    private readonly IConfiguration _config;
    private const string SessionUserId = "userId";

    public AccountController(
        ApplicationContext context,
        IPasswordService passwords,
        IImageService images,
        IConfiguration config
    )
    {
        _context = context;
        _passwords = passwords;
        _images = images;
        _config = config;
    }

    [HttpGet("register")]
    public IActionResult RegisterForm(int? eventId)
    {
        return View(new RegisterFormViewModel { EventId = eventId });
    }

    [ValidateAntiForgeryToken]
    [HttpPost("register/process")]
    public async Task<IActionResult> ProcessRegister(RegisterFormViewModel vm)
    {
        // normalize input
        vm.UserName = (vm.UserName ?? "").Trim();
        vm.Email = (vm.Email ?? "").Trim().ToLowerInvariant();
        vm.Password = (vm.Password ?? "").Trim();
        vm.ConfirmPassword = (vm.ConfirmPassword ?? "").Trim();

        // check if input is valid
        if (!ModelState.IsValid)
        {
            return View(nameof(RegisterForm), vm);
        }

        if (_context.Users.Any((u) => u.UserName == vm.UserName))
        {
            // Manually adding error to model state
            ModelState.AddModelError("UserName", "That UserName already exists.");
            return View(nameof(RegisterForm), vm);
        }

        if (_context.Users.Any((u) => u.Email == vm.Email))
        {
            // Manually adding error to model state
            ModelState.AddModelError("Email", "That Email is in use. Please Login.");
            return View(nameof(RegisterForm), vm);
        }

        // hash the password
        var hashedPassword = _passwords.Hash(vm.Password);

        // create new user
        var newUser = new User
        {
            UserName = vm.UserName,
            Email = vm.Email,
            PasswordHash = hashedPassword,
        };

        // add the user to the DataBase
        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        var newProfile = new Profile { UserId = newUser.Id };
        await _context.Profiles.AddAsync(newProfile);
        await _context.SaveChangesAsync();

        // log User in
        HttpContext.Session.SetInt32(SessionUserId, newUser.Id);
        HttpContext.Session.SetString("UserName", newUser.UserName);
        HttpContext.Session.SetString("ProfileImage", newProfile.ProfileImageUrl);

        //if the login is via an invite url, add the invitee to the rsvp table
        //check for rsvp in database
        if (vm.EventId is not null)
        {
            var foundRSVP = await _context
                .RSVPs.Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.EventId == vm.EventId && r.UserId == newUser.Id);

            //populate viewmodel and return view

            if (foundRSVP == null)
            {
                var newRSVP = new RSVP
                {
                    EventId = vm.EventId ?? 0,
                    AttendanceStatus = "No Response",
                    UserId = newUser.Id,
                    RespondedAt = DateTime.UtcNow,
                };
                await _context.RSVPs.AddAsync(newRSVP);
                await _context.SaveChangesAsync();
            }
        }
        // Redirects to Home or Dashboard
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userid = HttpContext.Session.GetInt32(SessionUserId);
        if (userid is not int uid)
        {
            return Unauthorized();
        }

        var user = await _context
            .Users.AsNoTracking()
            .Include((u) => u.Profile)
            .Include((u) => u.Events)
            .Include((u) => u.RSVPs)
            .Include((u) => u.SentInvites)
            .FirstOrDefaultAsync((u) => u.Id == uid);

        if (user is null)
        {
            return NotFound();
        }

        if (user.Profile is null)
        {
            user.Profile = new Profile
            {
                UserId = user.Id,
                ProfileImageUrl =
                    "https://ik.imagekit.io/Janice/default-image.jpg?updatedAt=1758640819082",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Profiles.Add(user.Profile);
            await _context.SaveChangesAsync();
        }

        var profileViewModel = new ProfileViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            JoinDate = user.CreatedAt.Humanize(),
            FullName = user.Profile!.FullName,
            Location = user.Profile!.Location,
            ProfileImageUrl = user.Profile!.ProfileImageUrl,
            UserId = user.Id,
            EventsCreated = user.Events.Count(),
            RSVPsCount = user.RSVPs.Count(),
            InvitesSent = user.SentInvites.Count,
        };

        var profileFormViewModel = new ProfileFormViewModel { UserId = user.Id };

        var vm = new ProfilePageViewModel
        {
            ProfileViewModel = profileViewModel,
            ProfileFormViewModel = profileFormViewModel,
        };

        return View(vm);
    }

    [HttpGet("profile/update/{id}")]
    public async Task<IActionResult> EditProfile(int id)
    {
        var userid = HttpContext.Session.GetInt32("userId");
        if (userid is not int uid)
        {
            return Unauthorized();
        }

        var user = await _context
            .Users.AsNoTracking()
            .Include((u) => u.Profile)
            .FirstOrDefaultAsync((u) => u.Id == uid);

        if (user is null)
        {
            return NotFound();
        }

        var profileFormViewModel = new ProfileFormViewModel { UserId = id };

        if (user.Profile is not null)
        {
            profileFormViewModel.FirstName = user.Profile!.FirstName ?? "";
            profileFormViewModel.LastName = user.Profile!.LastName ?? "";
            profileFormViewModel.Location = user.Profile!.Location ?? "";
            profileFormViewModel.GoogleMapsAPIKey = _config["GoogleMapsJSKey"] ?? "";
            profileFormViewModel.GoogleMapsMapId = _config["GoogleMapId"] ?? "";
        }

        return View("_ProfileForm", profileFormViewModel);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("profile/update")]
    public async Task<IActionResult> UpdateProfile(ProfileFormViewModel vm)
    {
        var userid = HttpContext.Session.GetInt32("userId");
        if (userid is not int uid)
        {
            return Unauthorized();
        }

        // Step 1: Check for validation errors
        if (!ModelState.IsValid)
        {
            var user = await _context
                .Users.AsNoTracking()
                .Include((u) => u.Profile)
                .FirstOrDefaultAsync((u) => u.Id == uid);

            if (user is null)
            {
                return NotFound();
            }
            var profileViewModel = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                JoinDate = user.CreatedAt.Humanize(),
                FullName = user.Profile!.FullName ?? "",
                Location = user.Profile!.Location ?? "",
                ProfileImageUrl = user.Profile!.ProfileImageUrl ?? "",
                UserId = user.Id,
            };

            var viewmodel = new ProfilePageViewModel
            {
                ProfileViewModel = profileViewModel,
                ProfileFormViewModel = vm,
            };

            // If validation fails, reload the page with error messages.
            return View(nameof(Profile), viewmodel);
        }

        // Step 2: Get the user's profile from the database
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == uid);
        if (profile is null)
            return NotFound();
        if (vm.UserId != profile.UserId)
            return Forbid();

        Console.WriteLine("************************ update profile");
        Console.WriteLine(vm.ProfileImage?.FileName);
        // Step 3: Handle the image upload
        if (vm.ProfileImage is not null)
        {
            profile.ProfileImageUrl = await _images.UploadImageAsync(vm.ProfileImage);

            HttpContext.Session.SetString("ProfileImage", profile.ProfileImageUrl);
        }

        // Step 4: Update other profile fields and save
        profile.FirstName = (vm.FirstName ?? "").Trim();
        profile.LastName = (vm.LastName ?? "").Trim();
        profile.Location = (vm.Location ?? "").Trim();
        profile.UpdatedAt = DateTime.UtcNow;

        Console.WriteLine(
            profile.FirstName,
            profile.LastName,
            profile.Location,
            profile.ProfileImageUrl
        );

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("login")]
    public IActionResult LoginForm(string? error, int? eventId)
    {
        var loginFormViewModel = new LoginFormViewModel { Error = error, EventId = eventId };
        return View(loginFormViewModel);
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessLoginForm(LoginFormViewModel vm)
    {
        // normalize input
        vm.Email = (vm.Email ?? "").Trim().ToLowerInvariant();
        vm.Password = (vm.Password ?? "").Trim();

        // check if model is valid
        if (!ModelState.IsValid)
        {
            return View(nameof(LoginForm), vm);
        }

        // Find User in DataBase
        if (!_context.Users.Any((u) => u.Email == vm.Email))
        {
            // manually adding error to model state
            ModelState.AddModelError(
                "",
                "Invalid user Credentials. Please Register and try again."
            );
            return View(nameof(LoginForm), vm);
        }

        // email exists, find user
        var maybeUser = await _context
            .Users.Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == vm.Email);

        if (maybeUser is null)
        {
            // manually adding error to model state
            ModelState.AddModelError(
                "",
                "Credentials could not be authenticated. Please check and try again."
            );
            return View(nameof(LoginForm), vm);
        }

        // verify password
        if (!_passwords.Verify(vm.Password, maybeUser.PasswordHash))
        {
            // manually adding error to model state
            ModelState.AddModelError(
                "",
                "Credentials could not be authenticated. Please try again."
            );
            return View(nameof(LoginForm), vm);
        }

        // log User in
        HttpContext.Session.SetInt32(SessionUserId, maybeUser.Id);
        HttpContext.Session.SetString("UserName", maybeUser.UserName);

        // Console.WriteLine("******************************");
        // Console.WriteLine(maybeUser.Profile?.ProfileImageUrl);
        var profileImageUrl =
            maybeUser.Profile?.ProfileImageUrl
            ?? "https://ik.imagekit.io/Janice/default-image.jpg?updatedAt=1758640819082";

        HttpContext.Session.SetString("ProfileImage", profileImageUrl);

        //if the login is via an invite url, add the invitee to the rsvp table
        //check for rsvp in database
        if (vm.EventId is not null)
        {
            var foundRSVP = await _context
                .RSVPs.Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.EventId == vm.EventId && r.UserId == maybeUser.Id);

            //populate viewmodel and return view

            if (foundRSVP == null)
            {
                var newRSVP = new RSVP
                {
                    EventId = vm.EventId ?? 0,
                    AttendanceStatus = "No Response",
                    UserId = maybeUser.Id,
                    RespondedAt = DateTime.UtcNow,
                };
                await _context.RSVPs.AddAsync(newRSVP);
                await _context.SaveChangesAsync();
            }
        }
        // Redirects to Home or Dashboard
        return RedirectToAction(nameof(EventsController.EventsIndex), "Events");
    }

    [HttpGet("logout")]
    public IActionResult ConfirmLogout()
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);

        if (userId is null)
        {
            return Unauthorized();
        }
        return View();
    }

    [ValidateAntiForgeryToken]
    [HttpPost("logout/process")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Clears all session data for the current user
        // TempData message that will survive the redirect
        TempData["LogoutMessage"] = "You have successfully logged out!";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("redirect-to-privacy")]
    public IActionResult RedirectToPrivacy()
    {
        return RedirectToAction(nameof(HomeController.Privacy), "Home");
    }
}
