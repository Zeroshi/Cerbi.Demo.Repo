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
        if (Path.GetExtension(path).Equals(".yml", StringComparison.OrdinalIgnoreCase) ||
            Path.GetExtension(path).Equals(".yaml", StringComparison.OrdinalIgnoreCase))
        {
            return LoadYaml(path);
        }

        using var stream = File.OpenRead(path);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return JsonSerializer.Deserialize<GovernancePolicy>(stream, options)
            ?? throw new InvalidOperationException($"Policy file '{path}' could not be parsed.");
    }

    private static GovernancePolicy LoadYaml(string path)
    {
        var root = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var rules = new List<Dictionary<string, string>>();
        Dictionary<string, string>? currentRule = null;

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("  - ", StringComparison.Ordinal))
            {
                currentRule = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                rules.Add(currentRule);
                AddYamlPair(currentRule, line[4..]);
                continue;
            }

            if (line.StartsWith("    ", StringComparison.Ordinal) && currentRule is not null)
            {
                AddYamlPair(currentRule, line.Trim());
                continue;
            }

            if (!line.StartsWith(' '))
            {
                AddYamlPair(root, line);
            }
        }

        return new GovernancePolicy(
            Required(root, "schemaVersion"),
            Required(root, "policyId"),
            Required(root, "enforcementMode"),
            rules.Select(rule => new GovernanceRule(
                Required(rule, "id"),
                Required(rule, "dataClass"),
                Required(rule, "description"),
                Required(rule, "severity"),
                Required(rule, "action"),
                Required(rule, "matchType"),
                Required(rule, "pattern"),
                Required(rule, "evidence"),
                Required(rule, "risk"),
                Required(rule, "recommendation"),
                Required(rule, "developerAction"))).ToArray());
    }

    private static void AddYamlPair(IDictionary<string, string> target, string line)
    {
        var separator = line.IndexOf(':', StringComparison.Ordinal);
        if (separator < 0)
        {
            return;
        }

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim();
        if (value.Length >= 2 && value[0] == '\'' && value[^1] == '\'')
        {
            value = value[1..^1];
        }

        target[key] = value;
    }

    private static string Required(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Policy is missing required key '{key}'.");
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
