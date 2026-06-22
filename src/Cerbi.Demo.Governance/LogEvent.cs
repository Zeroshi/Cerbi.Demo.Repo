namespace Cerbi.Demo.Governance;

public sealed record LogEvent(
    string EventName,
    IReadOnlyDictionary<string, object?> Properties)
{
    public static LogEvent Create(string eventName, params (string Key, object? Value)[] properties) =>
        new(eventName, properties.ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase));
}
