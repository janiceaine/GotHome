using System.ComponentModel.DataAnnotations;

namespace GotHome.ViewModels;

public class NewRSVPFormViewModel
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Required")]
    public string UserAttendanceValue { get; set; } = string.Empty;
    public int UserId { get; set; }
}
