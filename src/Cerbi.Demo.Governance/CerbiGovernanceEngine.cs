using System.Text.RegularExpressions;

namespace Cerbi.Demo.Governance;

public sealed class CerbiGovernanceEngine
{
    private readonly GovernancePolicy _policy;
    private readonly IReadOnlyList<CompiledRule> _rules;

    public CerbiGovernanceEngine(GovernancePolicy policy)
    {
        _policy = policy;
        _rules = policy.Rules.Select(CompiledRule.Create).ToArray();
    }

    public GovernanceResult Evaluate(LogEvent logEvent)
    {
        var violations = new List<GovernanceViolation>();

        foreach (var property in logEvent.Properties)
        {
            var value = Convert.ToString(property.Value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            foreach (var rule in _rules)
            {
                if (!rule.IsMatch(property.Key, value))
                {
                    continue;
                }

                violations.Add(new GovernanceViolation(
                    rule.Id,
                    rule.DataClass,
                    rule.Severity,
                    rule.Action,
                    rule.Action.Equals("block", StringComparison.OrdinalIgnoreCase) && _policy.EnforcementMode.Equals("block", StringComparison.OrdinalIgnoreCase)
                        ? "blocked"
                        : rule.Action,
                    logEvent.EventName,
                    property.Key,
                    rule.Evidence,
                    rule.Risk,
                    rule.Recommendation,
                    rule.DeveloperAction));
            }
        }

        return violations.Count == 0
            ? GovernanceResult.Allowed
            : new GovernanceResult(_policy.EnforcementMode.Equals("block", StringComparison.OrdinalIgnoreCase) is false, violations);
    }

    private sealed record CompiledRule(
        string Id,
        string DataClass,
        string Severity,
        string Action,
        string MatchType,
        Regex Regex,
        string Evidence,
        string Risk,
        string Recommendation,
        string DeveloperAction)
    {
        public static CompiledRule Create(GovernanceRule rule) => new(
            rule.Id,
            rule.DataClass,
            rule.Severity,
            rule.Action,
            rule.MatchType,
            new Regex(rule.Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            rule.Evidence,
            rule.Risk,
            rule.Recommendation,
            rule.DeveloperAction);

        public bool IsMatch(string fieldName, string value) => MatchType switch
        {
            "field-name" => Regex.IsMatch(fieldName),
            "field-value" => Regex.IsMatch(value),
            "field-name-or-value" => Regex.IsMatch(fieldName) || Regex.IsMatch(value),
            _ => throw new InvalidOperationException($"Unsupported rule match type '{MatchType}'.")
        };
    }
}
