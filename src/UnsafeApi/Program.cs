var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://localhost:5000");

app.MapPost("/checkout/fail", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("UnsafeCheckout");

    // BAD: these values are intentionally present so Cerbi Scanner can fail the pipeline.
    logger.LogWarning(
        "Payment failed for {Email} {Token} {SsnLastKnownValue} {CustomerId}",
        "alex.customer@example.invalid",
        "Bearer demo-token-not-real",
        "123-45-6789",
        "customerId-78441");

    return Results.Accepted();
});

app.Run();
