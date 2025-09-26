using System.ComponentModel.DataAnnotations;

namespace GotHome.Models;

public class LocationPing
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    public string LocationStatus { get; set; } = string.Empty;
    public bool IsHost { get; set; } = false;
}
