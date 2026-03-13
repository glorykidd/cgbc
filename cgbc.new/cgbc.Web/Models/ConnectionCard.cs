namespace cgbc.Web.Models;

public class ConnectionCard
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string VisitStatus { get; set; } = "";
    public bool WantsContact { get; set; }
    public string PreferredCommunication { get; set; } = "";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string ContactReason { get; set; } = "";
    public string? ContactReasonOther { get; set; }
    public string? PrayerRequests { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsRead { get; set; }
}
