using System.ComponentModel.DataAnnotations;
using GotHome.Attributes;

namespace GotHome.ViewModels;

public class ProfileFormViewModel
{
    [Required]
    public int UserId { get; set; }

    [MinLength(1, ErrorMessage = "First name must be at least one character.")]
    public string? FirstName { get; set; }

    [MinLength(1, ErrorMessage = "Last name must be at least one character.")]
    public string? LastName { get; set; }

    [MinLength(2, ErrorMessage = "Location must be at least two characters.")]
    public string? Location { get; set; }

    [ImageFile]
    public IFormFile? ProfileImage { get; set; }
}
