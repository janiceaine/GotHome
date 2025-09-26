namespace GotHome.ViewModels;

public class RSVPFormViewModel
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string? RSVPAttendanceStatus { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
}
