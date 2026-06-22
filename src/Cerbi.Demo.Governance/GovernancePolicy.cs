using System.Text.Json;

namespace Cerbi.Demo.Governance;

public sealed record GovernancePolicy(
    string SchemaVersion,
    string PolicyId,
    string EnforcementMode,
    IReadOnlyList<GovernanceRule> Rules)
{
    public static GovernancePolicy Load(string path)
    {
        using var stream = File.OpenRead(path);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return JsonSerializer.Deserialize<GovernancePolicy>(stream, options)
            ?? throw new InvalidOperationException($"Policy file '{path}' could not be parsed.");
    }
}

public sealed record GovernanceRule(
    string Id,
    string DataClass,
    string Description,
    string Severity,
    string Action,
    string MatchType,
    string Pattern,
    string Evidence,
    string Risk,
    string Recommendation,
    string DeveloperAction);
