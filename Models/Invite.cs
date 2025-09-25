using System.ComponentModel.DataAnnotations;

namespace GotHome.Models;

public class Invite
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required]
    public int SenderId { get; set; }
    public User? Sender { get; set; }

    [Required(ErrorMessage = "Please enter recipient email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string RecipientEmail { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
    public string Message { get; set; } = string.Empty;

    [Required]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool Accepted { get; set; } = false;
}
