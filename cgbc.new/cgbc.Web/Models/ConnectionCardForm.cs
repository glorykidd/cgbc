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

    [Required(ErrorMessage = "Please select a reason for contact.")]
    public string ContactReason { get; set; } = "";

    public string? ContactReasonOther { get; set; }

    public string? PrayerRequests { get; set; }
}
