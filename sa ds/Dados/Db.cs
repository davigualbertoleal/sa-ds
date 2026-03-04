using MySql.Data.MySqlClient;

namespace revisao.Dados
{
    public static class Db
    {
        private static string ConnectionString = "Server=localhost;Database=controleEstoque;Uid=root;Pwd=;";

        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(ConnectionString);

            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            return connection;
        }
    }
}