namespace GotHome.ViewModels;

public class InviteViewModel
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? Message { get; set; }
    public DateTime SentAt { get; set; }
}
