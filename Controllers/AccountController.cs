using GotHome.Models;
using GotHome.Services;
using GotHome.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GotHome.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly ApplicationContext _context;
    private readonly IPasswordService _passwords;
    private readonly IImageService _images;
    private const string SessionUserId = "userId";

    public AccountController(
        ApplicationContext context,
        IPasswordService passwords,
        IImageService images
    )
    {
        _context = context;
        _passwords = passwords;
        _images = images;
    }

    [HttpGet("register")]
    public IActionResult RegisterForm()
    {
        return View(new RegisterFormViewModel());
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

        // log User in
        HttpContext.Session.SetInt32(SessionUserId, newUser.Id);

        // Redirects to Home or Dashboard
        return RedirectToAction("UpdateProfile", "Account");
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
        };

        var profileFormViewModel = new ProfileFormViewModel { UserId = user.Profile.UserId };

        var vm = new ProfilePageViewModel
        {
            ProfileViewModel = profileViewModel,
            ProfileFormViewModel = profileFormViewModel,
        };

        return View(vm);
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
            // If validation fails, reload the page with error messages.
            // Omitted for brevity.
        }

        // Step 2: Get the user's profile from the database
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == uid);
        if (profile is null)
            return NotFound();
        if (vm.UserId != profile.UserId)
            return Forbid();

        // Step 3: Handle the image upload
        if (vm.ProfileImage is not null)
        {
            profile.ProfileImageUrl = await _images.UploadImageAsync(vm.ProfileImage);
        }

        // Step 4: Update other profile fields and save
        profile.FirstName = (vm.FirstName ?? "").Trim();
        profile.LastName = (vm.LastName ?? "").Trim();
        profile.Location = (vm.Location ?? "").Trim();
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("login")]
    public IActionResult LoginForm(string? error)
    {
        var loginFormViewModel = new LoginFormViewModel { Error = error };
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
        var maybeUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);

        if (maybeUser is null)
        {
            // manually adding error to model state
            ModelState.AddModelError("", "Email not found. Please check and try again.");
            return View(nameof(LoginForm), vm);
        }

        // verify password
        if (!_passwords.Verify(vm.Password, maybeUser.PasswordHash))
        {
            // manually adding error to model state
            ModelState.AddModelError("", "Incorrect password. Please try again.");
            return View(nameof(LoginForm), vm);
        }

        // log User in
        HttpContext.Session.SetInt32(SessionUserId, maybeUser.Id);

        // Redirects to Home or Dashboard
        return RedirectToAction("EventsIndex", "Events");
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
