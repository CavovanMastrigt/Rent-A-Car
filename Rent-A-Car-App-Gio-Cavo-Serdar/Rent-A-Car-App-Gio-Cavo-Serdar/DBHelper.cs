using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    internal class DBHelper
    {
        public static int loggedinuserid = 0;
        public static int loggedinroleid = 0; // 0 = customer, 1 = admin

        public const int RoleCustomer = 0;
        public const int RoleAdmin = 1;
        public static string ConnectionString = "Server=localhost;Port=3306;Database=RentACar;Uid=root;Pwd=;SslMode=None;AllowPublicKeyRetrieval=True;";

        // Pass user input through parameters (e.g. "@email") instead of putting it in the sql string,
        // so it can't break the query (SQL injection).
        public static int RunSql(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = createConnection())
            using (var command = createCommand(connection, sql, parameters))
            {
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static DataTable getdataTable(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = createConnection())
            using (var command = createCommand(connection, sql, parameters))
            {
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    var table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }

        public static object getScalar(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = createConnection())
            using (var command = createCommand(connection, sql, parameters))
            {
                connection.Open();
                return command.ExecuteScalar();
            }
        }

        // Turns a password into "salt.hash" so it is never stored as plain text.
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }
            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash(password, salt));
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            string[] parts = storedHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }
            byte[] salt = Convert.FromBase64String(parts[0]);
            return Convert.ToBase64String(hash(password, salt)) == parts[1];
        }

        private static byte[] hash(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32);
            }
        }

        private static DbCommand createCommand(DbConnection connection, string sql, Dictionary<string, object> parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (var pair in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = pair.Key;
                    parameter.Value = pair.Value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        private static DbConnection createConnection()
        {
            var type = Type.GetType("MySqlConnector.MySqlConnection, MySqlConnector", throwOnError: true);
            return (DbConnection)Activator.CreateInstance(type, ConnectionString);
        }
    }
}
