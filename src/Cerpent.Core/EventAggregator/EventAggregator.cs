using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core.EventAggregator;

public class EventAggregator
{
    private IEventSource EventSource { get; set; }
    private IAggregationRuleSource AggregationRuleSource { get; set; }

    public EventAggregator(IEventSource eventSource, IAggregationRuleSource aggregationRuleSource)
    {
        EventSource = eventSource;
        AggregationRuleSource = aggregationRuleSource;
    }

    private IAggregationRule[] GetRules(string triggerEventName) =>
        AggregationRuleSource.Get(triggerEventName)
            .Where(rule => rule.Atomics?.ContainsKey(triggerEventName) == null)
            .ToArray();

    public IEnumerable<Event> Aggregate(Event triggerEvent)
    {
        var rules = GetRules(triggerEvent.Name);

        var dataDictionary = triggerEvent.Data != null
            ? JsonConvert.DeserializeObject<JObject>(triggerEvent.Data)
            : null;

        var contextDictionary = dataDictionary is null
            ? null
            : rules
                .Select(rule => rule.ContextFields)
                .SelectMany(contextFields => contextFields)
                .Distinct()
                .Where(field => rules.All(rule => rule.ContextFields.Contains(field)))
                .ToDictionary(field => field, field => dataDictionary[field]);

        var atomicEvents = rules
            .Select(rule => rule.Atomics)
            .SelectMany(queue => queue)
            .Distinct();

        var timeSpan = rules.MaxBy(rule => rule.TimeSpanInSec)?.TimeSpanInSec ?? 3600;
        var eventList = EventSource.Get(atomicEvents.Select(a => a.Key),
            contextDictionary, timeSpan).ToArray();

        if (eventList == null || !eventList.Any())
            return Array.Empty<Event>();

        var newEvents = new List<Event>();
        foreach (var rule in rules)
        {
            var ruleEvents = eventList
                .Where(e => rule.Atomics?.ContainsKey(e.Name) == true)
                .ToArray();

            if (rule.Atomics.Any(atomic =>
                    ruleEvents.Count(ruleEvent => ruleEvent.Name == atomic.Key) < atomic.Value))
                continue;

            var newEventData = rule.Atomics.ToDictionary(atomic => atomic.Key,
                atomic => ruleEvents
                    .Where(ruleEvent => ruleEvent.Name == atomic.Key)
                    .Select(ruleEvent => ruleEvent?.Data is null
                        ? null
                        : JsonConvert.DeserializeObject<JObject>(ruleEvent.Data))
                    .ToArray());

            newEvents.Add(new Event()
            {
                Name = rule.Name,
                DateTime = DateTime.Now,
                Data = JsonConvert.SerializeObject(newEventData)
            });
        }

        return newEvents;
    }
}
