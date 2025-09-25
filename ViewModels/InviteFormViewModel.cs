using System.ComponentModel.DataAnnotations;

public class InviteFormViewModel
{
    [Required(ErrorMessage = "Please enter recipient email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string RecipientEmail { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
    public string? Message { get; set; }
}
