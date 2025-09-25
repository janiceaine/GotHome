using System.ComponentModel.DataAnnotations;

namespace GotHome.Models;

public class Event
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a title for the event.")]
    [MinLength(2, ErrorMessage = "Title cannot be less than 2 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a description.")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Please select a start time.")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; }

    public bool IsWrappedUp { get; set; } = false;

    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public List<RSVP> RSVPs { get; set; } = [];
    public List<Invite> Invites { get; set; } = [];
    public List<LocationPing> LocationPings { get; set; } = [];
}
