using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Infrastructure.Helpers
{
    public static class ConnectionStringsHelper
    {
        public static string GetDbConnectionString()
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:SystemDbConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                var serverName = Environment.GetEnvironmentVariable("DB_SERVER_NAME") ?? "localhost";
                var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "productsdb";
                var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
                var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "admin123";
                var defaultConnectionString = $"Server={serverName};Database={dbName};User Id={dbUser};Password={dbPassword};Encrypt=False;TrustServerCertificate=True";
                return defaultConnectionString;
            }
            return connectionString;
        }
    }
}
