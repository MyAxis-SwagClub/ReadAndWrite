using System.Data;
using Microsoft.Data.SqlClient;

namespace ReadAndWrite
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString =
    "Server=MYAXIS-SIGMA\\SQLEXPRESS01;Database=ReadAndWrite;Integrated Security=True;" +
    "TrustServerCertificate=True;";
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            var table = new DataTable();
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                using (var adapter = new SqlDataAdapter(command))
                    adapter.Fill(table);
            }
            return table;
        }
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteScalar();
            }
        }
    }
}