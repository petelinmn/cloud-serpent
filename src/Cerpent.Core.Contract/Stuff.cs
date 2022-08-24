using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core.Contract;

public class Event
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public JToken Data { get; set; }
}

public interface IEventSource
{
    Task<IEnumerable<Event>> Get(IEnumerable<string> names, Dictionary<string,
        JToken?>? contextDictionary, double? timeSpanInSec = null);
}

public class AggregationRule
{
    public AggregationRule()
    {
        
    }
    
    public AggregationRule(string name, IDictionary<string, int> atomics,
        IEnumerable<string> contextFields, string? condition, double? timeSpan)
    {
        Name = name;
        Atomics = atomics;
        ContextFields = contextFields.ToList();
        Condition = condition;
        TimeSpan = timeSpan;
    }

    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    /// <summary>
    /// Event names those are triggers for rule
    /// </summary>
    [JsonProperty("atomics")]
    public IDictionary<string, int>? Atomics { get; set; }
    [JsonProperty("fields")]
    public IEnumerable<string>? ContextFields { get; set; }
    [JsonProperty("condition")]
    public string? Condition { get; set; }
    [JsonProperty("timespan")]
    public double? TimeSpan { get; set; }
}

public interface IAggregationRuleSource
{
    Task<IEnumerable<AggregationRule>> Get(string triggerEvent);

    Task<int> Put(AggregationRule rule);
}






public class StereotypeDescription
{
    public StereotypeDescription(string name, string triggerEvent)
    {
        Name = name;
        TriggerEvent = triggerEvent;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string TriggerEvent { get; set; }
    public Dictionary<string, string> UpperBounds { get; set; }
    public Dictionary<string, string> LowerBounds { get; set; }
    public double Accuracy { get; set; }
}

public class StereotypeConfirmedResult
{
    public string Name { get; set; }
}

public interface IStereotypeDefinitionSource
{
    IEnumerable<StereotypeDescription> Get(string triggerName);
}
