using Cerbi.Demo.Governance;

namespace Cerbi.Demo.Api;

public sealed class DemoEvidence
{
    private readonly PaymentLogExamples _examples;

    public DemoEvidence(PaymentLogExamples examples) => _examples = examples;

    public DemoEvidenceResult Capture()
    {
        var unsafeResult = _examples.TryUnsafePaymentLog();
        var safeResult = _examples.TrySafePaymentLog();

        return new DemoEvidenceResult(
            UnsafeAttempt: new DemoAttempt(
                "PaymentAttemptFailed.Unsafe",
                unsafeResult.IsAllowed ? "allowed" : "blocked",
                unsafeResult.IsAllowed,
                "Unsafe event was evaluated before export and blocked because policy violations were found.",
                unsafeResult.Violations.Select(DemoViolation.From).ToArray()),
            SafeAttempt: new DemoAttempt(
                "PaymentAttemptFailed.Safe",
                safeResult.IsAllowed ? "allowed" : "blocked",
                safeResult.IsAllowed,
                "Safe event contains reviewed structured fields and is accepted for export.",
                safeResult.Violations.Select(DemoViolation.From).ToArray()));
    }
}

public sealed record DemoEvidenceResult(DemoAttempt UnsafeAttempt, DemoAttempt SafeAttempt);

public sealed record DemoAttempt(
    string EventName,
    string Outcome,
    bool IsAllowed,
    string Summary,
    IReadOnlyList<DemoViolation> Violations);

public sealed record DemoViolation(
    string FieldName,
    string Outcome,
    string RuleId,
    string DataClass,
    string Severity,
    string Action,
    string Evidence,
    string Risk,
    string Recommendation,
    string DeveloperAction)
{
    public static DemoViolation From(GovernanceViolation violation) => new(
        violation.FieldName,
        violation.Outcome,
        violation.RuleId,
        violation.DataClass,
        violation.Severity,
        violation.Action,
        violation.Evidence,
        violation.Risk,
        violation.Recommendation,
        violation.DeveloperAction);
}
