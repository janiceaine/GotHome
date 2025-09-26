using System;
using System.ComponentModel.DataAnnotations;

namespace GotHome.Models
{
    public class EventChatMessage
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; } // foreign key to the event
        public int UserId { get; set; } // who sent the message
        public User? User { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
