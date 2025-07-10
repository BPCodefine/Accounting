using System.Data;
using Microsoft.Data.SqlClient;

namespace AccountingServer.DBAccess
{
    public sealed class DynamicsDBContext
    {
        public readonly string connString;
        public DynamicsDBContext(IConfiguration configuration)
        {
            connString = configuration.GetConnectionString("LiveDBConnectString")!;
        }
        public IDbConnection Create() => new SqlConnection(connString);
    }
}
