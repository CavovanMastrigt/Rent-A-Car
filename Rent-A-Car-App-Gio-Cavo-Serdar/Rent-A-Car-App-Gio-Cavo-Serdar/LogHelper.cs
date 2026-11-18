using System;
using System.Collections.Generic;
using System.Data;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Writes activity-log entries (who did what) that the admin can review.
    internal static class LogHelper
    {
        // Records one action for the current user. Never throws - logging must not
        // break the action it is logging.
        public static void Write(string action, string details)
        {
            try
            {
                string name = "";
                try
                {
                    DataTable t = DBHelper.getdataTable("SELECT Name, LastName FROM user WHERE ID = " + DBHelper.loggedinuserid);
                    if (t.Rows.Count > 0) name = (t.Rows[0]["Name"] + " " + t.Rows[0]["LastName"]).Trim();
                }
                catch { }

                var p = new Dictionary<string, object>
                {
                    { "@uid", DBHelper.loggedinuserid == 0 ? (object)DBNull.Value : DBHelper.loggedinuserid },
                    { "@name", name },
                    { "@action", action ?? "" },
                    { "@details", details ?? "" },
                };
                DBHelper.RunSql(
                    "INSERT INTO activity_log (User_ID, User_Name, Action, Details) VALUES (@uid, @name, @action, @details)", p);
            }
            catch { }
        }
    }
}
