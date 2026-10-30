using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    internal class DBHelper
    {
        public static int loggedinuserid = 0;
        public static string ConnectionString = "Server=localhost;Port=3306;Database=RentACar;Uid=root;Pwd=;SslMode=None;AllowPublicKeyRetrieval=True;";

        public static int RunSql(string sql)
        {
            using (var connection = createConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static DataTable getdataTable(string sql)
        {
            using (var connection = createConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    var table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }

        public static object getScalar(string sql)
        {
            using (var connection = createConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                connection.Open();
                return command.ExecuteScalar();
            }
        }

        private static DbConnection createConnection()
        {
            var type = Type.GetType("MySqlConnector.MySqlConnection, MySqlConnector", throwOnError: true);
            return (DbConnection)Activator.CreateInstance(type, ConnectionString);
        }
    }
}
