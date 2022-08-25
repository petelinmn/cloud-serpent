using Cerpent.AWS.DB.Repositories;
using Cerpent.Core.Contract;
using Microsoft.AspNetCore.Mvc;

namespace Cerpent.AWS.ControlStand.Controllers;

[Route("api/[controller]")]
public class AggregationRulesController : ControllerBase
{
    private IAggregationRuleSource AggregationRuleSource { get; set; }
    
    public AggregationRulesController(IAggregationRuleSource aggregationRuleSource)
    {
        AggregationRuleSource = aggregationRuleSource;
    }
    
    // GET api/aggregationRules
    //[HttpGet]
    //public IEnumerable<string> Get()
    //{
    //    return new string[] {"value1", "value2"};
    //}

    // GET api/aggregationRules/test-rule
    [HttpGet("{ruleName}")]
    public async Task<IEnumerable<AggregationRule>> Get(string ruleName)
    {
        var rules = await AggregationRuleSource.Get(ruleName);
        return rules;
    }

    // POST api/values
    //[HttpPost]
    //public void Post([FromBody] string value)
    //{
    //}

    // PUT api/values
    [HttpPut]
    public async Task<int> Put([FromBody] AggregationRule rule)
    {
        return await AggregationRuleSource.Put(rule);
    }
    
    // DELETE api/values/5
    //[HttpDelete("{id}")]
    //public void Delete(int id)
    //{
    //}
}