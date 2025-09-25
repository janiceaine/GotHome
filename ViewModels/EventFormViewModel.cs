using System.ComponentModel.DataAnnotations;
using GotHome.Models;

namespace GotHome.ViewModels;

public class EventFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Please enter a title.")]
    [MinLength(3, ErrorMessage = "Title cannot be less than 3 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a description.")]
    [MinLength(8, ErrorMessage = "Description cannot be less than 8 characters.")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Please select a start time.")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public int? UserId { get; set; }
    public string GoogleMapsAPIKey { get; set; } = string.Empty;
    public string GoogleMapsMapId { get; set; } = string.Empty;
}
