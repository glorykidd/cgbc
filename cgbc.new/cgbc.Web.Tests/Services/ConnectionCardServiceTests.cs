using cgbc.Web.Data;
using cgbc.Web.Models;
using cgbc.Web.Services;
using Microsoft.EntityFrameworkCore;

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
        _service = new ConnectionCardService(_db);
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
        ContactReason = "Baptism"
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
        Assert.Equal(form.ContactReason, card.ContactReason);
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
}
