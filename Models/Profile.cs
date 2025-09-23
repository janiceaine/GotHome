using System.ComponentModel.DataAnnotations;

namespace GotHome.Models;

public class Profile
{
    [Key]
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName
    {
        get { return $"{FirstName} {LastName}"; }
    }
    public string? Location { get; set; }
    public string ProfileImageUrl { get; set; } =
        "https://ik.imagekit.io/Janice/default-image.jpg?updatedAt=1758640819082";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User? User { get; set; }
}
