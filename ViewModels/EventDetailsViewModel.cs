using System.ComponentModel.DataAnnotations;
using GotHome.Models;

public class EventDetailsViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; }
    public string UploadedBy { get; set; } = string.Empty;

    public List<string> Invitees { get; set; } = [];
    public List<string> RSVPs { get; set; } = [];
    public List<LocationPing> LocationPings { get; set; } = [];
}
