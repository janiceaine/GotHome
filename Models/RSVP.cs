using System.ComponentModel.DataAnnotations;

namespace GotHome.Models;

public class RSVP
{
    [Key]
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime Timestamp { get; set; }
}
