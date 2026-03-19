using Stripe.Checkout;

namespace cgbc.Web.Endpoints;

public static class StripeEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/create-checkout-session", async (
            HttpContext context,
            IConfiguration config) =>
        {
            var form = await context.Request.ReadFromJsonAsync<DonationRequest>();
            if (form is null || form.Amount < 1)
            {
                return Results.BadRequest(new { error = "Amount must be at least $1.00" });
            }

            Stripe.StripeConfiguration.ApiKey = config["Stripe:SecretKey"];

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                UiMode = "embedded",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(form.Amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Donation to Cedar Grove Baptist Church"
                            }
                        },
                        Quantity = 1
                    }
                ],
                ReturnUrl = $"{context.Request.Scheme}://{context.Request.Host}/donate?session_id={{CHECKOUT_SESSION_ID}}"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Results.Ok(new { clientSecret = session.ClientSecret });
        });

        app.MapGet("/api/checkout-status", async (string session_id, IConfiguration config) =>
        {
            Stripe.StripeConfiguration.ApiKey = config["Stripe:SecretKey"];

            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            return Results.Ok(new { status = session.Status, paymentStatus = session.PaymentStatus });
        });
    }

    private record DonationRequest(decimal Amount);
}
