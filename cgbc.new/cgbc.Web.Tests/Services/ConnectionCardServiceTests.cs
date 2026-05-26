using cgbc.Web.Data;
using cgbc.Web.Models;
using cgbc.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace cgbc.Web.Tests.Services;

public class ConnectionCardServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ConnectionCardService _service;

    public ConnectionCardServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        // EmailService with empty config so sends are skipped (no SMTP configured)
        var config = new ConfigurationBuilder().Build();
        var email = new EmailService(config, NullLogger<EmailService>.Instance);
        _service = new ConnectionCardService(_db, email);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    private static ConnectionCardForm CreateValidForm() => new()
    {
        Email = "test@example.com",
        Name = "John Doe",
        VisitStatus = "1st Time Guest",
        WantsContact = true,
        PreferredCommunication = "Email",
        ContactReasons = ["Baptism"]
    };

    [Fact]
    public async Task SubmitAsync_SavesCard()
    {
        var form = CreateValidForm();
        var result = await _service.SubmitAsync(form);

        Assert.True(result);
        Assert.Equal(1, await _db.ConnectionCards.CountAsync());
    }

    [Fact]
    public async Task SubmitAsync_SetsSubmittedAtUtc()
    {
        var before = DateTime.UtcNow;
        await _service.SubmitAsync(CreateValidForm());
        var after = DateTime.UtcNow;

        var card = await _db.ConnectionCards.FirstAsync();
        Assert.InRange(card.SubmittedAt, before, after);
    }

    [Fact]
    public async Task SubmitAsync_DefaultsIsReadToFalse()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();
        Assert.False(card.IsRead);
    }

    [Fact]
    public async Task SubmitAsync_MapsAllFields()
    {
        var form = CreateValidForm();
        form.Address = "123 Main St";
        form.Phone = "555-1234";
        form.ContactReasonOther = "Other reason";
        form.PrayerRequests = "Please pray";

        await _service.SubmitAsync(form);
        var card = await _db.ConnectionCards.FirstAsync();

        Assert.Equal(form.Email, card.Email);
        Assert.Equal(form.Name, card.Name);
        Assert.Equal(form.VisitStatus, card.VisitStatus);
        Assert.True(card.WantsContact);
        Assert.Equal(form.PreferredCommunication, card.PreferredCommunication);
        Assert.Equal(form.Address, card.Address);
        Assert.Equal(form.Phone, card.Phone);
        Assert.Equal(string.Join(", ", form.ContactReasons), card.ContactReason);
        Assert.Equal(form.ContactReasonOther, card.ContactReasonOther);
        Assert.Equal(form.PrayerRequests, card.PrayerRequests);
    }

    [Fact]
    public async Task GetSubmissionsAsync_ReturnsPaginated()
    {
        for (int i = 0; i < 5; i++)
        {
            var form = CreateValidForm();
            form.Name = $"Person {i}";
            await _service.SubmitAsync(form);
        }

        var page1 = await _service.GetSubmissionsAsync(1, 2);
        var page2 = await _service.GetSubmissionsAsync(2, 2);

        Assert.Equal(2, page1.Count);
        Assert.Equal(2, page2.Count);
    }

    [Fact]
    public async Task GetSubmissionsAsync_OrdersBySubmittedAtDesc()
    {
        await _service.SubmitAsync(CreateValidForm());
        await _service.SubmitAsync(CreateValidForm());

        var results = await _service.GetSubmissionsAsync(1, 10);
        Assert.True(results[0].SubmittedAt >= results[1].SubmittedAt);
    }

    [Fact]
    public async Task GetUnreadCountAsync_CountsUnreadOnly()
    {
        await _service.SubmitAsync(CreateValidForm());
        await _service.SubmitAsync(CreateValidForm());

        var card = await _db.ConnectionCards.FirstAsync();
        card.IsRead = true;
        await _db.SaveChangesAsync();

        Assert.Equal(1, await _service.GetUnreadCountAsync());
    }

    [Fact]
    public async Task MarkAsReadAsync_SetsIsReadTrue()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();

        await _service.MarkAsReadAsync(card.Id);
        await _db.Entry(card).ReloadAsync();

        Assert.True(card.IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_NonExistentId_DoesNotThrow()
    {
        await _service.MarkAsReadAsync(999);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCard_WhenExists()
    {
        await _service.SubmitAsync(CreateValidForm());
        var saved = await _db.ConnectionCards.FirstAsync();

        var result = await _service.GetByIdAsync(saved.Id);

        Assert.NotNull(result);
        Assert.Equal(saved.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        await _service.SubmitAsync(CreateValidForm());
        await _service.SubmitAsync(CreateValidForm());

        Assert.Equal(2, await _service.GetTotalCountAsync());
    }

    [Fact]
    public async Task GetCountSinceAsync_CountsOnlyRecentCards()
    {
        var old = new ConnectionCard
        {
            Name = "Old", Email = "old@test.com", VisitStatus = "1st Time Guest",
            SubmittedAt = DateTime.UtcNow.AddDays(-10)
        };
        _db.ConnectionCards.Add(old);
        await _db.SaveChangesAsync();

        await _service.SubmitAsync(CreateValidForm());

        var cutoff = DateTime.UtcNow.AddDays(-1);
        Assert.Equal(1, await _service.GetCountSinceAsync(cutoff));
    }

    [Fact]
    public async Task ToggleReadAsync_TogglesIsRead()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();
        Assert.False(card.IsRead);

        await _service.ToggleReadAsync(card.Id);
        await _db.Entry(card).ReloadAsync();
        Assert.True(card.IsRead);

        await _service.ToggleReadAsync(card.Id);
        await _db.Entry(card).ReloadAsync();
        Assert.False(card.IsRead);
    }

    [Fact]
    public async Task ToggleReadAsync_NonExistentId_DoesNotThrow()
    {
        await _service.ToggleReadAsync(999);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCard_ReturnsTrue()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();

        var result = await _service.DeleteAsync(card.Id);

        Assert.True(result);
        Assert.Equal(0, await _db.ConnectionCards.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(999);
        Assert.False(result);
    }

    [Fact]
    public async Task SearchAsync_FiltersBySearchTerm_MatchesName()
    {
        var form1 = CreateValidForm(); form1.Name = "Alice Smith";
        var form2 = CreateValidForm(); form2.Name = "Bob Jones";
        await _service.SubmitAsync(form1);
        await _service.SubmitAsync(form2);

        var (items, total) = await _service.SearchAsync("alice", null, 1, 10);

        Assert.Equal(1, total);
        Assert.Equal("Alice Smith", items[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FiltersBySearchTerm_MatchesEmail()
    {
        var form = CreateValidForm(); form.Email = "unique@domain.org";
        await _service.SubmitAsync(form);
        await _service.SubmitAsync(CreateValidForm());

        var (items, total) = await _service.SearchAsync("unique@domain", null, 1, 10);

        Assert.Equal(1, total);
        Assert.Equal("unique@domain.org", items[0].Email);
    }

    [Fact]
    public async Task SearchAsync_FiltersByIsRead()
    {
        await _service.SubmitAsync(CreateValidForm());
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();
        card.IsRead = true;
        await _db.SaveChangesAsync();

        var (unread, unreadTotal) = await _service.SearchAsync(null, false, 1, 10);
        var (read, readTotal) = await _service.SearchAsync(null, true, 1, 10);

        Assert.Equal(1, unreadTotal);
        Assert.Equal(1, readTotal);
    }

    [Fact]
    public async Task SearchAsync_NoFilter_ReturnsAll()
    {
        await _service.SubmitAsync(CreateValidForm());
        await _service.SubmitAsync(CreateValidForm());

        var (_, total) = await _service.SearchAsync(null, null, 1, 10);

        Assert.Equal(2, total);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrderedByDateDesc()
    {
        await _service.SubmitAsync(CreateValidForm());
        await _service.SubmitAsync(CreateValidForm());

        var all = await _service.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.True(all[0].SubmittedAt >= all[1].SubmittedAt);
    }

    [Fact]
    public async Task AddNoteAsync_SavesNoteWithCorrectFields()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();

        var before = DateTime.UtcNow;
        var note = await _service.AddNoteAsync(card.Id, "Test note", "Admin");
        var after = DateTime.UtcNow;

        Assert.Equal(card.Id, note.ConnectionCardId);
        Assert.Equal("Test note", note.Message);
        Assert.Equal("Admin", note.CreatedBy);
        Assert.InRange(note.CreatedAt, before, after);
    }

    [Fact]
    public async Task AddNoteAsync_PersistsToDatabase()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();

        await _service.AddNoteAsync(card.Id, "Persisted note", "Admin");

        Assert.Equal(1, await _db.ConnectionCardNotes.CountAsync());
    }

    [Fact]
    public async Task AddNoteAsync_DoesNotDuplicateNote()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();

        await _service.AddNoteAsync(card.Id, "Single note", "Admin");

        Assert.Equal(1, await _db.ConnectionCardNotes.CountAsync());
    }

    [Fact]
    public async Task GetByIdWithNotesAsync_IncludesNotes()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();
        await _service.AddNoteAsync(card.Id, "Note 1", "Admin");
        await _service.AddNoteAsync(card.Id, "Note 2", "Admin");

        var result = await _service.GetByIdWithNotesAsync(card.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result.Notes.Count);
    }

    [Fact]
    public async Task GetByIdWithNotesAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdWithNotesAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdWithNotesAsync_OrdersNotesAscending()
    {
        await _service.SubmitAsync(CreateValidForm());
        var card = await _db.ConnectionCards.FirstAsync();

        // Insert in reverse chronological order to prove the query re-orders them
        _db.ConnectionCardNotes.Add(new ConnectionCardNote
        {
            ConnectionCardId = card.Id, Message = "Second", CreatedBy = "Admin",
            CreatedAt = DateTime.UtcNow
        });
        _db.ConnectionCardNotes.Add(new ConnectionCardNote
        {
            ConnectionCardId = card.Id, Message = "First", CreatedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await _db.SaveChangesAsync();

        // Use a fresh context read (detach tracked entity so the query actually runs)
        _db.ChangeTracker.Clear();
        var result = await _service.GetByIdWithNotesAsync(card.Id);

        Assert.Equal("First", result!.Notes[0].Message);
        Assert.Equal("Second", result.Notes[1].Message);
    }
}
