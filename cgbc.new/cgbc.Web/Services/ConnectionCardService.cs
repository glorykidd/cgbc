using cgbc.Web.Data;
using cgbc.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace cgbc.Web.Services;

public class ConnectionCardService
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;

    public ConnectionCardService(AppDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
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
            ContactReason = string.Join(", ", form.ContactReasons),
            ContactReasonOther = form.ContactReasonOther,
            PrayerRequests = form.PrayerRequests,
            SubmittedAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.ConnectionCards.Add(card);
        var saved = await _db.SaveChangesAsync() > 0;

        if (saved)
        {
            await _email.SendAdminNotificationAsync(card);

            if (card.WantsContact && card.PreferredCommunication == "Email")
                await _email.SendVisitorConfirmationAsync(card);
        }

        return saved;
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

    public async Task<ConnectionCard?> GetByIdAsync(int id)
    {
        return await _db.ConnectionCards.FindAsync(id);
    }

    public async Task<ConnectionCard?> GetByIdWithNotesAsync(int id) =>
        await _db.ConnectionCards
            .Include(c => c.Notes.OrderBy(n => n.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task SetAcknowledgedAsync(int id, bool acknowledged)
    {
        var card = await _db.ConnectionCards.FindAsync(id);
        if (card != null) { card.IsAcknowledged = acknowledged; await _db.SaveChangesAsync(); }
    }

    public async Task<ConnectionCardNote> AddNoteAsync(int cardId, string message, string createdBy)
    {
        var note = new ConnectionCardNote
        {
            ConnectionCardId = cardId,
            Message = message,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
        _db.ConnectionCardNotes.Add(note);
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _db.ConnectionCards.CountAsync();
    }

    public async Task<int> GetCountSinceAsync(DateTime since)
    {
        return await _db.ConnectionCards.CountAsync(c => c.SubmittedAt >= since);
    }

    public async Task ToggleReadAsync(int id)
    {
        var card = await _db.ConnectionCards.FindAsync(id);
        if (card != null)
        {
            card.IsRead = !card.IsRead;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var card = await _db.ConnectionCards.FindAsync(id);
        if (card != null)
        {
            _db.ConnectionCards.Remove(card);
            return await _db.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<(List<ConnectionCard> Items, int TotalCount)> SearchAsync(
        string? searchTerm, bool? isReadFilter, int page, int pageSize)
    {
        var query = _db.ConnectionCards.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.ToLower().Contains(term)));
        }

        if (isReadFilter.HasValue)
        {
            query = query.Where(c => c.IsRead == isReadFilter.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<ConnectionCard>> GetAllAsync()
    {
        return await _db.ConnectionCards
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync();
    }
}
