using System.ComponentModel.DataAnnotations;

namespace cgbc.Web.Models;

public class ConnectionCardForm
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Please select your visit status.")]
    public string VisitStatus { get; set; } = "";

    [Required(ErrorMessage = "Please select yes or no.")]
    public bool? WantsContact { get; set; }

    [Required(ErrorMessage = "Please select a communication preference.")]
    public string PreferredCommunication { get; set; } = "";

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public List<string> ContactReasons { get; set; } = [];

    public string? ContactReasonOther { get; set; }

    public string? PrayerRequests { get; set; }

    /// <summary>
    /// Hidden honeypot field. Real visitors never see or fill this in; bots that
    /// blindly fill every input do. Any non-empty value here means reject the submission.
    /// </summary>
    public string? Website { get; set; }
}
