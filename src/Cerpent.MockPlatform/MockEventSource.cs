using Cerpent.Core.Contract;
using Newtonsoft.Json.Linq;

namespace Cerpent.MockPlatform;

public class MockEventSource : IEventSource
{
    private Event[] Events { get; }
    
    public MockEventSource(Event[] events)
    {
        Events = events;
    }

    public async Task<IEnumerable<Event>> Get(IEnumerable<string> names,
        Dictionary<string, JToken?>? contextDictionary,
        double? timeSpanInSec = null) =>
        await Task.Run(() => Events
            .Where(@event => names.Contains(@event.Name))
            .Where(@event =>
                !timeSpanInSec.HasValue ||
                timeSpanInSec.Value > (DateTime.Now - @event.DateTime).Seconds));
}