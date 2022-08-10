using Newtonsoft.Json.Linq;

namespace Cerpent.Core.EventAggregator;

public class Event
{
    public string Name { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string? Data { get; set; }
}

public interface IEventSource
{
    IEnumerable<Event> Get(IEnumerable<string> names, Dictionary<string, JToken?>? contextDictionary, double? timeSpanInSec = null);
}

public interface IAggregationRule
{
    public string Name { get; set; }
    /// <summary>
    /// Event names those are triggers for rule
    /// </summary>
    IDictionary<string, int> Atomics { get; set; }
    IEnumerable<string> ContextFields { get; set; }
    string? Condition { get; set; }
    public double? TimeSpanInSec { get; set; }
}

public interface IAggregationRuleSource
{
    IEnumerable<IAggregationRule> Get(string triggerEvent);
}
