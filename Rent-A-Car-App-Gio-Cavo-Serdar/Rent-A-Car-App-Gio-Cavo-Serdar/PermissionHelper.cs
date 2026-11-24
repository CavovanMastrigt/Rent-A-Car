using System.Collections.Generic;
using System.Data;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Which parts of the app an employee may use. Admins always have everything;
    // for an employee the rights are stored per user in the user_permission table.
    // There is one right per page, so the admin can grant/revoke every single page
    // an employee could otherwise see on the Rechten page.
    internal static class PermissionHelper
    {
        // One key per page in the sidebar.
        public const string Dashboard = "dashboard";
        public const string Cars = "cars";
        public const string DayOverview = "dayoverview";
        public const string Planning = "planning";
        public const string NewReservation = "newreservation";
        public const string PickupReturn = "pickupreturn";
        public const string ChangeCancel = "changecancel";
        public const string Extras = "extras";
        public const string Rentals = "rentals";
        public const string Customers = "customers";
        public const string Verification = "verification";
        public const string Invoices = "invoices";
        public const string Messages = "messages";
        public const string Employees = "employees";
        public const string Permissions = "permissions";
        public const string OpeningHours = "openinghours";
        public const string Logs = "logs";

        // Old grouped key that used to cover all of the "Medewerker" pages at once.
        // Kept only so existing data keeps working (see GetForUser).
        private const string LegacyReservations = "reservations";

        // The keys that the old grouped "reservations" right now maps to.
        private static readonly string[] LegacyReservationsMapsTo =
        {
            DayOverview, Planning, NewReservation, PickupReturn, ChangeCancel, Extras, Rentals, Messages
        };

        // A section of rights, shown together on the Rechten page.
        public sealed class Group
        {
            public string Title;
            public string[] Keys;
        }

        // Rights grouped the same way as the sidebar, in display order.
        public static readonly Group[] Groups =
        {
            new Group { Title = "Algemeen",   Keys = new[] { Dashboard, Cars } },
            new Group { Title = "Medewerker", Keys = new[] { DayOverview, Planning, NewReservation, PickupReturn, ChangeCancel, Extras, Rentals } },
            new Group { Title = "Klanten",    Keys = new[] { Customers, Verification, Invoices, Messages } },
            new Group { Title = "Beheer",     Keys = new[] { Employees, Permissions, OpeningHours, Logs } },
        };

        // Flat list of every grantable right, in display order.
        public static readonly string[] Keys;

        // Rights a brand-new employee gets by default (the everyday work pages).
        public static readonly string[] Default =
        {
            DayOverview, Planning, NewReservation, PickupReturn, ChangeCancel, Extras,
            Rentals, Verification, Invoices, Messages
        };

        static PermissionHelper()
        {
            var all = new List<string>();
            foreach (Group g in Groups) all.AddRange(g.Keys);
            Keys = all.ToArray();
        }

        public static string Label(string key)
        {
            switch (key)
            {
                case Dashboard: return "Dashboard";
                case Cars: return "Auto's";
                case DayOverview: return "Dagoverzicht";
                case Planning: return "Planning";
                case NewReservation: return "Nieuwe reservering";
                case PickupReturn: return "Ophalen / terugbrengen";
                case ChangeCancel: return "Wijzigen / annuleren";
                case Extras: return "Extra's en prijzen";
                case Rentals: return "Reserveringen";
                case Customers: return "Klantbeheer";
                case Verification: return "Klantverificatie";
                case Invoices: return "Facturen";
                case Messages: return "Berichten";
                case Employees: return "Medewerkers";
                case Permissions: return "Rechten";
                case OpeningHours: return "Openingstijden";
                case Logs: return "Logboek";
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

            // Backward compatibility: an employee who was set up before the rights
            // were split per page still has the old grouped "reservations" right.
            // Expand it to the individual pages so nothing is lost (no migration needed).
            if (set.Remove(LegacyReservations))
            {
                foreach (string k in LegacyReservationsMapsTo) set.Add(k);
            }
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
