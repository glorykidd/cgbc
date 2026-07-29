using System.Text;
using cgbc.Web.Services;

namespace cgbc.Web.Endpoints;

public static class ExportEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/admin/export/csv", async (ConnectionCardService service) =>
        {
            var submissions = await service.GetAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Email,VisitStatus,WantsContact,PreferredCommunication,Address,Phone,ContactReason,ContactReasonOther,PrayerRequests,SubmittedAt,IsRead");

            foreach (var c in submissions)
            {
                sb.AppendLine($"{c.Id},{Escape(c.Name)},{Escape(c.Email)},{Escape(c.VisitStatus)},{c.WantsContact},{Escape(c.PreferredCommunication)},{Escape(c.Address)},{Escape(c.Phone)},{Escape(c.ContactReason)},{Escape(c.ContactReasonOther)},{Escape(c.PrayerRequests)},{c.SubmittedAt:yyyy-MM-dd HH:mm:ss},{c.IsRead}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv", $"connection-cards-{DateTime.UtcNow:yyyyMMdd}.csv");
        }).RequireAuthorization();
    }

    // Characters that spreadsheet apps (Excel, Sheets) interpret as the start
    // of a formula. A public visitor can submit one of these via the
    // Connection Card form; without neutralizing it here, opening the export
    // in a spreadsheet app can execute it (CSV/formula injection, CWE-1236).
    private static readonly char[] FormulaTriggerChars = ['=', '+', '-', '@', '\t', '\r'];

    internal static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        if (FormulaTriggerChars.Contains(value[0]))
        {
            value = "'" + value;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
