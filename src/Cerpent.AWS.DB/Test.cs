using Cerpent.Core.Contract;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cerpent.AWS.DB.Repositories;

[TestClass]
public class Test
{
    private static string ConnectionString =>
        "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=postgres";

    [TestMethod]
    public void SourceTest()
    {
        var aggregationRuleSource = new DbAggregationRuleSource(ConnectionString);
        
        const string ruleName = "PulseRise";
        const string firstAtomicEventName = "Pulse";
        const string secondAtomicEventName = "Pressure";
        var guidLabel = Guid.NewGuid().ToString();
        var atomicsDictionary = new Dictionary<string, int>
        {
            { firstAtomicEventName, Random.Shared.Next(1, 10) },
            { secondAtomicEventName, Random.Shared.Next(1, 10)}
        };

        var contextFields = new[] { "personId" };
        var timeSpan = Random.Shared.Next(1, 10);
     
        var newRule = new AggregationRule(ruleName, atomicsDictionary,
            contextFields, guidLabel, timeSpan);

        var newId = aggregationRuleSource.Put(newRule).Result;

        var ruleFromDb = aggregationRuleSource.Get(firstAtomicEventName)
            .Result.FirstOrDefault(rule => rule.Name == newRule.Name);

        Assert.IsTrue(ruleFromDb != null);
        Assert.IsTrue(ruleFromDb.Id == newId);
        Assert.IsTrue(ruleFromDb.Condition == newRule.Condition);
        Assert.IsTrue(ruleFromDb.ContextFields?.Count() == newRule.ContextFields?.Count());
        Assert.IsTrue(ruleFromDb.Atomics?.Count == newRule.Atomics?.Count);
        Assert.IsTrue(ruleFromDb.Atomics[firstAtomicEventName] == newRule.Atomics[firstAtomicEventName]);
        Assert.IsTrue(ruleFromDb.TimeSpan == newRule.TimeSpan);
    }
}