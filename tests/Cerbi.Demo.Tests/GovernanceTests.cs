using Xunit;
using Cerbi.Demo.Api;
using Cerbi.Demo.Governance;

namespace Cerbi.Demo.Tests;

public sealed class GovernanceTests
{
    private static string PolicyPath
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, "policy", "governance-profile.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not find policy/governance-profile.json from the test output directory.");
        }
    }

    [Fact]
    public void Unsafe_payment_log_is_blocked_and_not_exported()
    {
        var examples = CreateExamples();

        var result = examples.TryUnsafePaymentLog();

        Assert.False(result.IsAllowed);
        Assert.Empty(examples.AcceptedEvents);
        Assert.Equal(4, result.Violations.Count);
        Assert.Contains(result.Violations, violation => violation.RuleId == "PII.EMAIL.VALUE");
        Assert.Contains(result.Violations, violation => violation.RuleId == "PCI.PAN.VALUE");
        Assert.Contains(result.Violations, violation => violation.RuleId == "AUTH.BEARER_TOKEN.FIELD_OR_VALUE");
        Assert.Contains(result.Violations, violation => violation.RuleId == "DEBUG.RAW_PAYLOAD.FIELD");
        Assert.All(result.Violations, violation => Assert.Equal("blocked", violation.Outcome));
    }

    [Fact]
    public void Safe_payment_log_is_accepted_for_export()
    {
        var examples = CreateExamples();

        var result = examples.TrySafePaymentLog();

        Assert.True(result.IsAllowed);
        Assert.Single(examples.AcceptedEvents);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Blocked_log_includes_actionable_policy_evidence()
    {
        var examples = CreateExamples();

        var result = examples.TryUnsafePaymentLog();

        var panViolation = Assert.Single(result.Violations, violation => violation.RuleId == "PCI.PAN.VALUE");
        Assert.Equal("payment_account_number", panViolation.DataClass);
        Assert.Equal("critical", panViolation.Severity);
        Assert.Equal("block", panViolation.Action);
        Assert.Equal("blocked", panViolation.Outcome);
        Assert.Equal("cardNumber", panViolation.FieldName);
        Assert.Contains("PAN-like", panViolation.Evidence);
        Assert.Contains("PCI scope", panViolation.Risk);
        Assert.Contains("cardLast4", panViolation.Recommendation);
        Assert.Contains("Remove cardNumber", panViolation.DeveloperAction);
    }

    private static PaymentLogExamples CreateExamples()
    {
        var policy = GovernancePolicy.Load(PolicyPath);
        return new PaymentLogExamples(new CerbiGovernanceEngine(policy));
    }
}
