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
                var candidate = Path.Combine(directory.FullName, "cerbi-policy.yml");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not find cerbi-policy.yml from the test output directory.");
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
        Assert.Contains(result.Violations, violation => violation.RuleId == "AUTH.BEARER_TOKEN.FIELD_OR_VALUE");
        Assert.Contains(result.Violations, violation => violation.RuleId == "PII.SSN.VALUE");
        Assert.Contains(result.Violations, violation => violation.RuleId == "PII.CUSTOMER_ID.FIELD");
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

        var ssnViolation = Assert.Single(result.Violations, violation => violation.RuleId == "PII.SSN.VALUE");
        Assert.Equal("national_identifier", ssnViolation.DataClass);
        Assert.Equal("critical", ssnViolation.Severity);
        Assert.Equal("block", ssnViolation.Action);
        Assert.Equal("blocked", ssnViolation.Outcome);
        Assert.Equal("ssnLastKnownValue", ssnViolation.FieldName);
        Assert.Contains("SSN-like", ssnViolation.Evidence);
        Assert.Contains("privacy exposure", ssnViolation.Risk);
        Assert.Contains("ssnPresent", ssnViolation.Recommendation);
        Assert.Contains("Remove ssnLastKnownValue", ssnViolation.DeveloperAction);
    }

    private static PaymentLogExamples CreateExamples()
    {
        var policy = GovernancePolicy.Load(PolicyPath);
        return new PaymentLogExamples(new CerbiGovernanceEngine(policy));
    }
}
