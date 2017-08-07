using Oracle.DataAccess.Client;
using System.Collections.Generic;

namespace CodeGen.AccessModel.Oracle
{
    internal static class DatabaseUtils
    {
        public static bool CheckConnectionString(string connectionString)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();
            builder.ConnectionString = connectionString;

            return true;
        }

        public static string CreateBasicConnectionString(string server, string userId, string password)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();

            builder.DataSource = server;
            builder.UserID = userId;
            builder.Password = password;

            return builder.ConnectionString;
        }

        public static List<string> GetDatabaseList(string server, string userId, string password)
        {
            List<string> databaseList = new List<string>();

            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();

            builder.DataSource = server;
            builder.UserID = userId;
            builder.Password = password;

            using (OracleConnection connection = new OracleConnection(builder.ConnectionString))
            {
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SHOW DATABASES;";
                    connection.Open();
                    OracleDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        databaseList.Add(reader.GetValue(0).ToString());
                    }
                    connection.Close();
                }
            }

            databaseList.Sort();

            return databaseList;
        }
    }
}
