namespace GotHome.ViewModels;

public class UserEventStatsViewModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int EventsCount { get; set; }

    public int EventsRated { get; set; }
}
