using System.Collections.Generic;
using System.Data;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Which parts of the app an employee may use. Admins always have everything;
    // for an employee the rights are stored per user in the user_permission table.
    internal static class PermissionHelper
    {
        public const string Cars = "cars";
        public const string Reservations = "reservations";
        public const string Verification = "verification";
        public const string Invoices = "invoices";
        public const string Customers = "customers";
        public const string OpeningHours = "openinghours";

        // Rights an admin can grant to an employee, in display order.
        public static readonly string[] Keys = { Cars, Reservations, Verification, Invoices, Customers, OpeningHours };

        // Rights a brand-new employee gets by default.
        public static readonly string[] Default = { Reservations, Verification, Invoices };

        public static string Label(string key)
        {
            switch (key)
            {
                case Cars: return "Auto's";
                case Reservations: return "Reserveringen (medewerker)";
                case Verification: return "Klantverificatie";
                case Invoices: return "Facturen";
                case Customers: return "Klantbeheer";
                case OpeningHours: return "Openingstijden";
                default: return key;
            }
        }

        private static HashSet<string> current;

        // Load the logged-in user's rights. Call this right after login.
        public static void LoadCurrent()
        {
            current = GetForUser(DBHelper.loggedinuserid, AuthHelper.loggedinroleid);
        }

        public static bool Has(string perm)
        {
            if (AuthHelper.loggedinroleid == AuthHelper.RoleAdmin) return true;
            if (current == null) LoadCurrent();
            return current.Contains(perm);
        }

        public static HashSet<string> GetForUser(int userId, int roleId)
        {
            var set = new HashSet<string>();
            if (roleId == AuthHelper.RoleAdmin)
            {
                foreach (string k in Keys) set.Add(k);
                return set;
            }
            try
            {
                DataTable t = DBHelper.getdataTable("SELECT Perm FROM user_permission WHERE UserID = " + userId);
                foreach (DataRow r in t.Rows) set.Add(r["Perm"].ToString());
            }
            catch { }
            return set;
        }

        public static void SetForUser(int userId, IEnumerable<string> perms)
        {
            DBHelper.RunSql("DELETE FROM user_permission WHERE UserID = " + userId);
            foreach (string perm in perms)
            {
                var p = new Dictionary<string, object> { { "@uid", userId }, { "@perm", perm } };
                DBHelper.RunSql("INSERT INTO user_permission (UserID, Perm) VALUES (@uid, @perm)", p);
            }
        }

        public static void SeedDefault(int userId)
        {
            SetForUser(userId, Default);
        }
    }
}
