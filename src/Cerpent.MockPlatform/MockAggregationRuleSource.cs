
using Cerpent.Core.Contract;

namespace Cerpent.MockPlatform;

public class MockAggregationRuleSource : IAggregationRuleSource
{
    private IEnumerable<AggregationRule> AggregationRules { get; set; }
    
    public MockAggregationRuleSource(IEnumerable<AggregationRule> aggregationRules)
    {
        AggregationRules = aggregationRules;
    }

    public async Task<IEnumerable<AggregationRule>> Get(string triggerEventName) =>
        await Task.Run(() =>
            AggregationRules.Where(rule => rule.Atomics?.ContainsKey(triggerEventName) == true));

    public async Task<int> Put(AggregationRule rule)
    {
        throw new NotImplementedException();
        /*var newList = new List<AggregationRule>();
        newList.AddRange(AggregationRules);
        
        
        
        if (rule.Id == 0)
        {
            rule.Id = newList.Max(i => i.Id) + 1;
            newList.Add(rule);
            return rule.Id;
        }
        else
        {
            var existing = newList.FirstOrDefault(i => i.Id == )
            
        }

        AggregationRules = newList;
        return 0;*/
    }
}
