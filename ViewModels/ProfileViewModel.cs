namespace GotHome.ViewModels;

public class ProfileViewModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string JoinDate { get; set; } = string.Empty;
    public int EventsCreated { get; set; }
    public int RSVPsCount { get; set; }
    public int InvitesSent { get; set; }
}
