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
        ("customerEmail", "alex.customer@example.invalid"),
        ("token", "Bearer demo-token-not-real"),
        ("ssnLastKnownValue", "123-45-6789"),
        ("customerId", "customerId-78441")));

    public GovernanceResult TrySafePaymentLog() => _logger.Write(LogEvent.Create(
        "PaymentAttemptFailed.Safe",
        ("customerRef", "custref_demo_78441"),
        ("tokenPresent", true),
        ("ssnPresent", true),
        ("failureCode", "issuer_declined")));
}
