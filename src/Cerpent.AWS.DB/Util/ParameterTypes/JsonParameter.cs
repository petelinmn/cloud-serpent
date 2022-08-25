using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace Cerpent.AWS.DB.Repositories.Util.ParameterTypes;

public class JsonParameter : SqlMapper.ICustomQueryParameter
{
    private readonly string _value;

    public JsonParameter(string value) => _value = value;

    public void AddParameter(IDbCommand command, string name) =>
        command.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Json)
        {
            Value = _value
        });
}
