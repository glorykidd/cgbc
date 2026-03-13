using cgbc.Web.Data;
using cgbc.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace cgbc.Web.Services;

public class ConnectionCardService
{
    private readonly AppDbContext _db;

    public ConnectionCardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> SubmitAsync(ConnectionCardForm form)
    {
        var card = new ConnectionCard
        {
            Email = form.Email,
            Name = form.Name,
            VisitStatus = form.VisitStatus,
            WantsContact = form.WantsContact ?? false,
            PreferredCommunication = form.PreferredCommunication,
            Address = form.Address,
            Phone = form.Phone,
            ContactReason = form.ContactReason,
            ContactReasonOther = form.ContactReasonOther,
            PrayerRequests = form.PrayerRequests,
            SubmittedAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.ConnectionCards.Add(card);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<List<ConnectionCard>> GetSubmissionsAsync(int page, int pageSize)
    {
        return await _db.ConnectionCards
            .OrderByDescending(c => c.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return await _db.ConnectionCards.CountAsync(c => !c.IsRead);
    }

    public async Task MarkAsReadAsync(int id)
    {
        var card = await _db.ConnectionCards.FindAsync(id);
        if (card != null)
        {
            card.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }
}
