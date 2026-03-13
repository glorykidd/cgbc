using System.ComponentModel.DataAnnotations;
using cgbc.Web.Models;

namespace cgbc.Web.Tests.Models;

public class ConnectionCardFormTests
{
    private static List<ValidationResult> ValidateModel(ConnectionCardForm form)
    {
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, context, results, true);
        return results;
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
    public void ValidForm_HasNoErrors()
    {
        var results = ValidateModel(CreateValidForm());
        Assert.Empty(results);
    }

    [Fact]
    public void MissingEmail_HasValidationError()
    {
        var form = CreateValidForm();
        form.Email = "";
        var results = ValidateModel(form);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void InvalidEmail_HasValidationError()
    {
        var form = CreateValidForm();
        form.Email = "not-an-email";
        var results = ValidateModel(form);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void MissingName_HasValidationError()
    {
        var form = CreateValidForm();
        form.Name = "";
        var results = ValidateModel(form);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void MissingVisitStatus_HasValidationError()
    {
        var form = CreateValidForm();
        form.VisitStatus = "";
        var results = ValidateModel(form);
        Assert.Contains(results, r => r.MemberNames.Contains("VisitStatus"));
    }

    [Fact]
    public void MissingWantsContact_HasValidationError()
    {
        var form = CreateValidForm();
        form.WantsContact = null;
        var results = ValidateModel(form);
        Assert.Contains(results, r => r.MemberNames.Contains("WantsContact"));
    }

    [Fact]
    public void MissingPreferredCommunication_HasValidationError()
    {
        var form = CreateValidForm();
        form.PreferredCommunication = "";
        var results = ValidateModel(form);
        Assert.Contains(results, r => r.MemberNames.Contains("PreferredCommunication"));
    }

    [Fact]
    public void MissingContactReason_HasValidationError()
    {
        var form = CreateValidForm();
        form.ContactReason = "";
        var results = ValidateModel(form);
        Assert.Contains(results, r => r.MemberNames.Contains("ContactReason"));
    }

    [Fact]
    public void OptionalFields_CanBeNull()
    {
        var form = CreateValidForm();
        form.Address = null;
        form.Phone = null;
        form.ContactReasonOther = null;
        form.PrayerRequests = null;
        var results = ValidateModel(form);
        Assert.Empty(results);
    }
}
