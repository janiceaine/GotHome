namespace GotHome.ViewModels;

using GotHome.Models;

public class RSVPViewModel
{
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public bool IsAttending { get; set; }
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
    public string AttendanceStatus { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}
