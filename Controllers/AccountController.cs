using GotHome.Models;
using GotHome.Services;
using GotHome.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GotHome.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly ApplicationContext _context;
    private readonly IPasswordService _passwords;
    private const string SessionUserId = "userId";

    public AccountController(ApplicationContext context, IPasswordService passwords)
    {
        _context = context;
        _passwords = passwords;
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
            ModelState.AddModelError("Username", "That UserName already exists.");
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
        return RedirectToAction("EventsIndex", "Events");
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
