using GotHome.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GotHome.Hubs;

public class EventChatHub : Hub
{
    private readonly ApplicationContext _context;

    public EventChatHub(ApplicationContext context) => _context = context;

    public async Task SendMessage(int eventId, string userName, string message, int userId)
    {
        // Save message to DB
        var chatMessage = new EventChatMessage
        {
            EventId = eventId,
            UserId = userId,
            UserName = userName,
            Message = message,
            CreatedAt = DateTime.UtcNow,
        };
        _context.EventChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        // Get profile image
        var profileImage =
            await _context
                .Users.Where(u => u.Id == userId)
                .Select(u => u.Profile.ProfileImageUrl)
                .FirstOrDefaultAsync() ?? "/images/default-avatar.png";

        // Broadcast
        await Clients
            .Group($"Event-{eventId}")
            .SendAsync("ReceiveMessage", userName, message, profileImage, userId);
    }

    public async Task JoinEvent(int eventId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Event-{eventId}");

    public async Task LeaveEvent(int eventId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Event-{eventId}");
}
