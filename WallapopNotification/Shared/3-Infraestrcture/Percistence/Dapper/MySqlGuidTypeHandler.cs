using System.Data;
using Dapper;

namespace WallapopNotification.Shared._3_Infraestrcture.Percistence.Dapper;

public class MySqlGuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid guid)
    {
        parameter.Value = guid.ToString();
    }

    public override Guid Parse(object value)
    {
        return new Guid((string)value);
    }
}