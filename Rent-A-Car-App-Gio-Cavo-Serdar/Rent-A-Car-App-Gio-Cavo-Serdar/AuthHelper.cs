using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    internal class AuthHelper
    {
        public static int loggedinroleid = 0; // 0 = customer, 1 = admin

        public const int RoleCustomer = 0;
        public const int RoleAdmin = 1;

        // Puts user input in quotes for a DBHelper query, e.g. "WHERE Email = " + AuthHelper.sql(txtEmail.Text).
        // Quotes and backslashes are escaped, so the input can't break out of the string (SQL injection).
        public static string sql(string value)
        {
            return "'" + value.Trim().Replace("\\", "\\\\").Replace("'", "''") + "'";
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
    }
}
