namespace cgbc.Web.Models;

public class ConnectionCardNote
{
    public int Id { get; set; }
    public int ConnectionCardId { get; set; }
    public string Message { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public ConnectionCard ConnectionCard { get; set; } = default!;
}
