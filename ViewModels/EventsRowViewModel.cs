namespace GotHome.ViewModels;

public class EventsRowViewModel
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Invite { get; set; }
    public int InviteCount { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public int UploaderId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreateDate { get; set; } = string.Empty;

    public int? RSVP { get; set; }
    public int RSVPCount { get; set; }
    public bool IsWrappedUp { get; set; } = false;
    public bool IsLiveTracking { get; set; } = false;
}
