using Cerpent.Core.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core;

public class EventAggregator
{
    private IEventSource EventSource { get; set; }
    private IAggregationRuleSource AggregationRuleSource { get; set; }

    public EventAggregator(IEventSource eventSource, IAggregationRuleSource aggregationRuleSource)
    {
        EventSource = eventSource;
        AggregationRuleSource = aggregationRuleSource;
    }

    private async Task<AggregationRule[]> GetRules(string triggerEventName) =>
        (await AggregationRuleSource.Get(triggerEventName))
            .Where(rule => rule.Atomics?.ContainsKey(triggerEventName) == true)
            .ToArray();

    public async Task<IEnumerable<Event>> Aggregate(Event triggerAtomicEvent)
    {
        var rules = await GetRules(triggerAtomicEvent.Name);

        var dataDictionary = triggerAtomicEvent.Data;

        var contextDictionary = rules
            .Select(rule => rule.ContextFields)
            .SelectMany(contextFields => contextFields)
            .Distinct()
            .Where(field => rules.All(rule => rule.ContextFields.Contains(field)))
            .ToDictionary(field => field, field => dataDictionary[field]);

        var atomicEvents = rules
            .Select(rule => rule.Atomics)
            .SelectMany(queue => queue)
            .Distinct()
            .Select(a => a.Key)
            .ToArray();

        var timeSpan = rules.MaxBy(rule => rule.TimeSpan)?.TimeSpan ?? 3600;
        var eventList = (await EventSource.Get(atomicEvents,
            contextDictionary, timeSpan)).ToList();

        eventList = eventList
            .Where(e => contextDictionary
                .All(c => e.Data[c.Key]?.ToString() == c.Value?.ToString()))
            .ToList();
        
        if (eventList.All(e => e.Id != triggerAtomicEvent.Id))
            eventList.Add(triggerAtomicEvent);

        eventList = eventList.OrderByDescending(o => o.DateTime).ToList();

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
                    .Where(ruleEvent => ruleEvent.Name == atomic.Key).Take(atomic.Value)
                    .Select(ruleEvent => ruleEvent?.Data)
                    .ToArray());

            newEvents.Add(new Event()
            {
                Id = Guid.NewGuid(),
                Name = rule.Name,
                DateTime = DateTime.Now,
                Data = JToken.FromObject(newEventData)
            });
        }

        return newEvents;
    }
}
