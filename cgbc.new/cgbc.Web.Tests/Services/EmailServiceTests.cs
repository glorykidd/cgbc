using cgbc.Web.Models;
using cgbc.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace cgbc.Web.Tests.Services;

public class EmailServiceTests
{
    private static ConnectionCard MakeCard(bool wantsContact = true, string? email = "visitor@test.com") => new()
    {
        Id = 42,
        Name = "Jane Doe",
        Email = email ?? "",
        Phone = "555-9876",
        Address = "123 Oak St",
        VisitStatus = "1st Time Guest",
        WantsContact = wantsContact,
        PreferredCommunication = "Email",
        ContactReason = "Baptism, General Info",
        PrayerRequests = "Family health",
        SubmittedAt = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc)
    };

    private static EmailService MakeService(Dictionary<string, string?>? settings = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings ?? [])
            .Build();
        return new EmailService(config, NullLogger<EmailService>.Instance);
    }

    // --- BuildAdminNotificationHtml ---

    [Fact]
    public void BuildAdminNotificationHtml_ContainsName()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.Contains("Jane Doe", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ContainsEmail()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.Contains("visitor@test.com", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ContainsPhone()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.Contains("555-9876", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ContainsAddress()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.Contains("123 Oak St", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ContainsVisitStatus()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.Contains("1st Time Guest", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ContainsContactReason()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.Contains("Baptism, General Info", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ContainsPrayerRequests()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.Contains("Family health", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ContainsAdminLink()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.Contains("/admin/submissions/42", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ShowsDashForNullPhone()
    {
        var card = MakeCard();
        card.Phone = null;
        var html = EmailService.BuildAdminNotificationHtml(card);
        Assert.Contains("—", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_ShowsNoneForNullContactReason()
    {
        var card = MakeCard();
        card.ContactReason = "";
        var html = EmailService.BuildAdminNotificationHtml(card);
        Assert.Contains(">None<", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_HtmlEncodesSpecialChars()
    {
        var card = MakeCard();
        card.Name = "<script>alert('xss')</script>";
        var html = EmailService.BuildAdminNotificationHtml(card);
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void BuildAdminNotificationHtml_IsValidHtmlStructure()
    {
        var html = EmailService.BuildAdminNotificationHtml(MakeCard());
        Assert.StartsWith("<!DOCTYPE html>", html.TrimStart());
        Assert.Contains("</html>", html);
    }

    // --- BuildVisitorConfirmationHtml ---

    [Fact]
    public void BuildVisitorConfirmationHtml_ContainsVisitorName()
    {
        var html = EmailService.BuildVisitorConfirmationHtml(MakeCard());
        Assert.Contains("Jane Doe", html);
    }

    [Fact]
    public void BuildVisitorConfirmationHtml_ContainsChurchWebsite()
    {
        var html = EmailService.BuildVisitorConfirmationHtml(MakeCard());
        Assert.Contains("cedargrovebaptist.church", html);
    }

    [Fact]
    public void BuildVisitorConfirmationHtml_IncludesContactPromise_WhenWantsContact()
    {
        var html = EmailService.BuildVisitorConfirmationHtml(MakeCard(wantsContact: true));
        Assert.Contains("reaching out to you soon", html);
    }

    [Fact]
    public void BuildVisitorConfirmationHtml_ExcludesContactPromise_WhenNoContact()
    {
        var html = EmailService.BuildVisitorConfirmationHtml(MakeCard(wantsContact: false));
        Assert.DoesNotContain("reaching out to you soon", html);
    }

    [Fact]
    public void BuildVisitorConfirmationHtml_HtmlEncodesName()
    {
        var card = MakeCard();
        card.Name = "O'Brien & Co <Ltd>";
        var html = EmailService.BuildVisitorConfirmationHtml(card);
        Assert.DoesNotContain("<Ltd>", html);
        Assert.Contains("&lt;Ltd&gt;", html);
    }

    // --- SendAdminNotificationAsync (skips when not configured) ---

    [Fact]
    public async Task SendAdminNotificationAsync_SkipsWhenNoAdminEmailConfigured()
    {
        var svc = MakeService();
        // Should not throw even though no SMTP is configured
        await svc.SendAdminNotificationAsync(MakeCard());
    }

    // --- SendVisitorConfirmationAsync (skips when email is blank) ---

    [Fact]
    public async Task SendVisitorConfirmationAsync_SkipsWhenCardEmailIsEmpty()
    {
        var svc = MakeService();
        var card = MakeCard(email: "");
        // Should not throw; returns early due to empty email
        await svc.SendVisitorConfirmationAsync(card);
    }
}
