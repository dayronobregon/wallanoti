using System.Data;
using System.Text.Json;
using Dapper;

namespace WallapopNotification.Shared._3_Infraestrcture.Percistence.Dapper;

public class StringListTypeHandler : SqlMapper.TypeHandler<List<string>>
{
    public override void SetValue(IDbDataParameter parameter, List<string> value)
    {
        parameter.Value = JsonSerializer.Serialize(value);
    }

    public override List<string> Parse(object value)
    {
        return value == null ? new List<string>() : JsonSerializer.Deserialize<List<string>>(value.ToString());
    }
}