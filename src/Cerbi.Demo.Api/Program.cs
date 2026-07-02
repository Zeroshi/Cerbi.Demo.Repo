using Cerbi.Demo.Api;
using Cerbi.Demo.Governance;

var builder = WebApplication.CreateBuilder(args);

var policyPath = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "cerbi-policy.yml");
var policy = GovernancePolicy.Load(Path.GetFullPath(policyPath));
builder.Services.AddSingleton(new CerbiGovernanceEngine(policy));
builder.Services.AddScoped<PaymentLogExamples>();
builder.Services.AddScoped<DemoEvidence>();

var app = builder.Build();

app.Urls.Add("http://localhost:5000");

app.MapGet("/", () => Results.Ok(new { service = "cerbi-undeniable-demo", purpose = "logging governance demo" }));

app.MapPost("/demo/unsafe", (PaymentLogExamples examples) =>
{
    var result = examples.TryUnsafePaymentLog();
    return Results.BadRequest(new
    {
        message = "Cerbi blocked unsafe log before it reached observability tools.",
        result.Violations
    });
});

app.MapPost("/demo/safe", (PaymentLogExamples examples) =>
{
    var result = examples.TrySafePaymentLog();
    return Results.Ok(new
    {
        message = "Safe structured log accepted.",
        result.IsAllowed
    });
});

app.MapGet("/demo/evidence", (DemoEvidence evidence) => Results.Ok(evidence.Capture()));

app.Run();
