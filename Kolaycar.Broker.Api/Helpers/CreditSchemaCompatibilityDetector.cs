using Microsoft.Data.SqlClient;
using System;

namespace KolayCAR.Broker.API.Helpers
{
    public static class CreditSchemaCompatibilityDetector
    {
        public static bool UsesLegacySchema(string connectionString)
        {
            const string query = @"
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM sys.columns c
        INNER JOIN sys.tables t ON t.object_id = c.object_id
        WHERE t.name = 'AGENCY'
          AND c.name = 'FULLCREDITPERMISSION'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM sys.columns c
        INNER JOIN sys.tables t ON t.object_id = c.object_id
        WHERE t.name = 'AGENCY'
          AND c.name = 'CREDITTYPE'
    )
    THEN 1
    ELSE 0
END";

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection);
            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar()) == 1;
        }
    }
}
