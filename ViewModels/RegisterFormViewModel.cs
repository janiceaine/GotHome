using System.ComponentModel.DataAnnotations;

namespace GotHome.ViewModels;

public class RegisterFormViewModel
{
    [Key]
    public int? Id { get; set; }

    [Required(ErrorMessage = "Please enter a UserName.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter an email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email.")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Email format is not valid."
    )]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Please enter your password.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
