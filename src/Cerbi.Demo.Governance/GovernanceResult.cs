namespace Cerbi.Demo.Governance;

public sealed record GovernanceResult(bool IsAllowed, IReadOnlyList<GovernanceViolation> Violations)
{
    public static GovernanceResult Allowed { get; } = new(true, Array.Empty<GovernanceViolation>());
}

public sealed record GovernanceViolation(
    string RuleId,
    string DataClass,
    string Severity,
    string Action,
    string Outcome,
    string EventName,
    string FieldName,
    string Evidence,
    string Risk,
    string Recommendation,
    string DeveloperAction);
