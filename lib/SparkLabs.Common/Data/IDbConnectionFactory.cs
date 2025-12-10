using System.Data;

namespace SparkLabs.Common.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
