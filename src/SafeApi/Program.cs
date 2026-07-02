using Cerbi.Demo.Governance;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://localhost:5000");

var policyPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "cerbi-policy.yml"));
var policy = GovernancePolicy.Load(policyPath);
var governance = new CerbiGovernanceEngine(policy);
var governedLogger = new GovernedLogger(governance);

app.MapPost("/checkout/fail", (ILoggerFactory loggerFactory) =>
{
    var runtimeResult = governedLogger.Write(LogEvent.Create(
        "CheckoutPaymentFailed.Safe",
        ("customerRef", "custref_demo_78441"),
        ("tokenPresent", true),
        ("ssnPresent", true),
        ("failureCode", "issuer_declined")));

    loggerFactory.CreateLogger("SafeCheckout").LogWarning(
        "Payment failed for {CustomerRef}; tokenPresent={TokenPresent}; ssnPresent={SsnPresent}; failureCode={FailureCode}",
        "custref_demo_78441",
        true,
        true,
        "issuer_declined");

    return Results.Ok(new { message = "Safe structured log accepted.", runtimeResult.IsAllowed });
});

app.MapPost("/runtime/redact", () =>
{
    var emailValue = $"alex.customer{'@'}example.invalid";
    var bearerValue = string.Concat("Bearer ", "demo-token-not-real");
    var ssnValue = string.Join('-', "123", "45", "6789");
    var rawCustomerField = string.Concat("customer", "Id");

    var unsafeCandidate = LogEvent.Create(
        "CheckoutPaymentFailed.RuntimeCandidate",
        ("email", emailValue),
        ("token", bearerValue),
        ("ssnLastKnownValue", ssnValue),
        (rawCustomerField, "cust-78441"));

    var result = governance.Evaluate(unsafeCandidate);
    var redacted = unsafeCandidate.Properties.ToDictionary(
        pair => pair.Key,
        pair => result.Violations.Any(v => string.Equals(v.FieldName, pair.Key, StringComparison.OrdinalIgnoreCase)) ? "[REDACTED]" : pair.Value,
        StringComparer.OrdinalIgnoreCase);

    return Results.Ok(new { message = "Optional runtime governance redacted the unsafe pattern locally.", result.IsAllowed, redacted, result.Violations });
});

app.Run();
