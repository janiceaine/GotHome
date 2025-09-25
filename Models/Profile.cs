using System.ComponentModel.DataAnnotations;

namespace GotHome.Models;

public class Profile
{
    [Key]
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string FullName
    {
        get { return $"{FirstName} {LastName}"; }
    }
    public string Email { get; set; } = string.Empty;
    public string? Location { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } =
        "https://ik.imagekit.io/Janice/default-image.jpg?updatedAt=1758640819082";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string JoinDate { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User? User { get; set; }
    public int EventsCreated { get; set; }
    public int RSVPsCount { get; set; }
    public int InvitesSent { get; set; }
}
