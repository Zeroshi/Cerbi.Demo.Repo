namespace Cerbi.Demo.Governance;

public sealed class GovernedLogger
{
    private readonly CerbiGovernanceEngine _engine;
    private readonly List<LogEvent> _acceptedEvents = [];
    private readonly List<GovernanceViolation> _violations = [];

    public GovernedLogger(CerbiGovernanceEngine engine) => _engine = engine;

    public IReadOnlyList<LogEvent> AcceptedEvents => _acceptedEvents;

    public IReadOnlyList<GovernanceViolation> Violations => _violations;

    public GovernanceResult Write(LogEvent logEvent)
    {
        var result = _engine.Evaluate(logEvent);
        if (result.IsAllowed)
        {
            _acceptedEvents.Add(logEvent);
            return result;
        }

        _violations.AddRange(result.Violations);
        return result;
    }
}
