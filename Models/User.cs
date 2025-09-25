using System.ComponentModel.DataAnnotations;

namespace GotHome.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter your username.")]
    [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<Event> Events { get; set; } = [];
    public List<RSVP> RSVPs { get; set; } = [];
    public List<Invite> SentInvites { get; set; } = [];
    public List<LocationPing> LocationPings { get; set; } = [];

    public Profile? Profile { get; set; }
}
