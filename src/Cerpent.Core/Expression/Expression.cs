using Newtonsoft.Json.Linq;

namespace Cerpent.Core.Expression;

public enum ExpressionLanguage
{
    Javascript,
    Python
}

public class Expression
{
    public Expression(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public ExpressionLanguage Language { get; set; }
    public string ArgumentName { get; set; }
    public string Body { get; set; }
    public string Condition { get; set; }
    public string Data { get; set; }

    public JToken Execute()
    {
        return JToken.FromObject(4);
    }
}
