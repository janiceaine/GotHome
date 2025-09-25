using System.ComponentModel.DataAnnotations;
using GotHome.Models;
using GotHome.ViewModels;

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

    public List<InviteViewModel> Invites { get; set; } = [];
    public List<RSVPViewModel> RSVPs { get; set; } = [];
    public List<LocationPingViewModel> LocationPings { get; set; } = [];
}
