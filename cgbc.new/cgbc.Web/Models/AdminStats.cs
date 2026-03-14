namespace cgbc.Web.Models;

public class AdminStats
{
    public int TotalSubmissions { get; set; }
    public int UnreadCount { get; set; }
    public int ThisWeekCount { get; set; }
    public int ThisMonthCount { get; set; }
    public List<ConnectionCard> RecentSubmissions { get; set; } = [];
}
