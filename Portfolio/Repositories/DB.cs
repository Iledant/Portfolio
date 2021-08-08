using Npgsql;
using System;

namespace Portfolio.Repositories
{
    public class DBConnexionParams
    {
        public string Host { get; }
        public string Port { get; }
        public string Username { get; }
        public string Password { get; }

        public DBConnexionParams(string host = "localhost",
            string port = "5432",
            string username = "",
            string password = "")
        {
            Host = host;
            Port = port;
            Username = username;
            Password = password;
        }
    }

    public enum DBState
    {
        OK,
        AlreadyExists,
        NotFound,
        Error
    }

    public static class DB
    {
        private static NpgsqlConnection Connection;

        public static bool Ok { get; private set; } = false;

        public static DBState State { get; set; }

        private static string GetConnectionString()
        {
            Config.FetchDBConnexionParams();

            return $"Host={Config.DBConnexionParams.Host};" +
                $"Port={Config.DBConnexionParams.Port};" +
                $"Username={Config.DBConnexionParams.Username};" +
                $"Password={Config.DBConnexionParams.Password};" +
                $"Database=hfunds";
        }

        public static bool ChangeParamsAndCheckConnection()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection = null;
            }
            _ = GetConnection();
            return Ok;
        }

        public static NpgsqlConnection GetConnection()
        {
            if (Connection is null)
            {
                string connectionString = GetConnectionString();
                try
                {
                    Connection = new NpgsqlConnection(connectionString);
                    Connection.Open();
                    Ok = true;
                }
                catch (Exception)
                {
                    Ok = false;
                }
            }
            return Connection;
        }
    }
}
