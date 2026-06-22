using Cerbi.Demo.Governance;

namespace Cerbi.Demo.Api;

public sealed class PaymentLogExamples
{
    private readonly GovernedLogger _logger;

    public PaymentLogExamples(CerbiGovernanceEngine engine) => _logger = new GovernedLogger(engine);

    public IReadOnlyList<LogEvent> AcceptedEvents => _logger.AcceptedEvents;

    public IReadOnlyList<GovernanceViolation> Violations => _logger.Violations;

    public GovernanceResult TryUnsafePaymentLog() => _logger.Write(LogEvent.Create(
        "PaymentAttemptFailed.Unsafe",
        ("customerEmail", "demo.customer@example.invalid"),
        ("cardNumber", "4111 1111 1111 1111"),
        ("authorization", "Bearer demo-token-not-real"),
        ("debugPayload", "{ \"note\": \"unreviewed free-form request body\" }")));

    public GovernanceResult TrySafePaymentLog() => _logger.Write(LogEvent.Create(
        "PaymentAttemptFailed.Safe",
        ("customerRef", "cust_demo_001"),
        ("paymentInstrumentType", "card"),
        ("cardLast4", "1111"),
        ("tokenPresent", true),
        ("failureCode", "issuer_declined")));
}
