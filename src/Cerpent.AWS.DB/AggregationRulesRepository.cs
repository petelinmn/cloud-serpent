using System.Collections;
using Cerpent.AWS.DB.Stuff;
using Cerpent.AWS.DB.Util.ParameterTypes;
using Cerpent.Core.Contract;
using Dapper;
using Newtonsoft.Json;

namespace Cerpent.AWS.DB;

public class AggregationRulesRepository : BaseRepository
{
    public AggregationRulesRepository(string connectionString) : base(connectionString)
    {
        
    }
    public async Task<IEnumerable<AggregationRule>> GetByAtomic(string? atomicEventName = null)
    {
        var whereClause = atomicEventName is null ? "" : $"where (atomics->>'{atomicEventName}') is not null";

        var sql = $@"
            select Id, Name, Condition, Timespan, ContextFields, Id, Atomics as atomicsJson
            from AggregationRules
            {whereClause};";
        //            --where 'lunch' = ANY(ContextFields) --bar = any (array[1, 2, 3]);
        var result = await Connection.QueryAsync<AggregationRule, AggregationRuleAtomics, AggregationRule>(sql,
            ((rule, ruleAtomics) =>
            {
                if (ruleAtomics is not null)
                    rule.Atomics = JsonConvert.DeserializeObject<Dictionary<string, int>>(ruleAtomics.AtomicsJson);

                return rule;
            }),
            //new {}, 
            //Transaction, 
            splitOn: "Id");

        return result;
    }
    
    private async Task<int?> GetId(string ruleName)
    {
        var result = await Connection.ExecuteScalarAsync<int?>($@"
            select Id from AggregationRules where Name = @{nameof(ruleName)}",
            new { ruleName });

        return result;
    }
    
    public async Task<int> Put(AggregationRule rule)
    {
        if (rule is null)
        {
            throw new Exception("Rule can't be null");
        }

        var currentId = await GetId(rule.Name);
        if (!currentId.HasValue)
        {
            currentId = await Insert(rule);
            
        }
        else
        {
            rule.Id = currentId.Value;
            currentId = await Update(rule);
        }

        return currentId.Value;
    }

    private async Task<int> Insert(AggregationRule rule)
    {
        var jsonText = JsonConvert.SerializeObject(rule.Atomics);
        var atomicsJson = new JsonParameter(jsonText);

        var contextFields = (IEnumerable) rule.ContextFields! ?? new[] {"personId"};

        try
        {
            var result = await Connection.ExecuteAsync($@"
               INSERT INTO AggregationRules (Name, Atomics, ContextFields, Condition, Timespan)
                VALUES (@{nameof(rule.Name)},@{nameof(atomicsJson)},@{nameof(rule.ContextFields)},
                    @{nameof(rule.Condition)},@{nameof(rule.TimeSpan)});
            SELECT currval('aggregationrules_id_seq');
            ", new
            {
                rule.Name, atomicsJson, rule.ContextFields,
                rule.Condition, rule.TimeSpan
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return -1;
    }
    
    //public object ToAnonymousObject(Dictionary<string, object> dict) =>
    //    dict.Aggregate(new ExpandoObject() as IDictionary<string, object>,
    //        (a, p) => { a.Add(p.Key, p.Value); return a; });
    
    private async Task<int> Update(AggregationRule rule)
    {
        if (rule.Id == 0)
            throw new Exception("Rule doesn't exist");
        
        var jsonText = JsonConvert.SerializeObject(rule.Atomics);
        var atomicsJson = new JsonParameter(jsonText);

        await Connection.ExecuteAsync($@"
           UPDATE AggregationRules
           SET Name=@{nameof(rule.Name)},
               Atomics=@{nameof(atomicsJson)},
               ContextFields=@{nameof(rule.ContextFields)},
               Condition=@{nameof(rule.Condition)},
               Timespan=@{nameof(rule.TimeSpan)}
           WHERE Id = @{rule.Id} 
        ", new { rule.Id, rule.Name, atomicsJson, rule.ContextFields,
            rule.Condition, rule.TimeSpan });

        return rule.Id;
    }
    
    class AggregationRuleAtomics
    {
        public int Id { get; set; }
        public string? AtomicsJson { get; set; }
    }
}
