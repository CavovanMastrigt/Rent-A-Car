using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Reminders for ID cards and invoices that are about to expire.
    //  - On staff login an in-app message is sent once per item (deduped with a
    //    "reminder sent" date column). The e-mail is sent separately from the
    //    overview pages, with a button.
    //  - The tracking columns are added automatically if missing, so a shared
    //    database updates itself (no manual migration).
    internal static class ReminderHelper
    {
        public const int IdWindowDays = 30;      // ID card "bijna verlopen" window
        public const int InvoiceWindowDays = 7;  // invoice "bijna verlopen" window

        private static readonly CultureInfo NL = new CultureInfo("nl-NL");
        private static bool schemaEnsured;

        // Adds the nullable tracking columns if they do not exist yet. Safe to call
        // repeatedly; the work happens only once per app run.
        public static void EnsureSchema()
        {
            if (schemaEnsured) return;
            try
            {
                if (!ColumnExists("identity", "ID_Reminder_Sent"))
                    DBHelper.RunSql("ALTER TABLE identity ADD COLUMN ID_Reminder_Sent DATE NULL");
                if (!ColumnExists("invoice", "Reminder_Sent"))
                    DBHelper.RunSql("ALTER TABLE invoice ADD COLUMN Reminder_Sent DATE NULL");
                schemaEnsured = true;
            }
            catch
            {
                // Best effort: the overview pages still work without the columns,
                // they just cannot remember that a reminder was already sent.
            }
        }

        private static bool ColumnExists(string table, string column)
        {
            object o = DBHelper.getScalar(
                "SELECT COUNT(*) FROM information_schema.COLUMNS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '" + table + "' AND COLUMN_NAME = '" + column + "'");
            return o != null && o != DBNull.Value && Convert.ToInt32(o) > 0;
        }

        // Sends the one-off in-app message for every ID card / invoice that just
        // entered its reminder window. Runs on staff login; never throws.
        public static void SendDueInAppReminders()
        {
            EnsureSchema();
            try { sendDueIdReminders(); } catch { }
            try { sendDueInvoiceReminders(); } catch { }
        }

        private static void sendDueIdReminders()
        {
            DataTable t = DBHelper.getdataTable(
                "SELECT i.UserID, CONCAT(u.Name,' ',u.LastName) AS Naam, i.Expiration_Date " +
                "FROM identity i JOIN user u ON u.ID = i.UserID " +
                "WHERE i.Identity_Status = 'Verified' AND i.ID_Reminder_Sent IS NULL " +
                "AND i.Expiration_Date IS NOT NULL " +
                "AND i.Expiration_Date >= CURDATE() " +
                "AND i.Expiration_Date <= DATE_ADD(CURDATE(), INTERVAL " + IdWindowDays + " DAY)");

            foreach (DataRow r in t.Rows)
            {
                int uid = Convert.ToInt32(r["UserID"]);
                string name = r["Naam"].ToString();
                DateTime exp = Convert.ToDateTime(r["Expiration_Date"]);
                sendMessage(uid, IdReminderBody(name, exp));
                DBHelper.RunSql("UPDATE identity SET ID_Reminder_Sent = CURDATE() WHERE UserID = " + uid);
            }
        }

        private static void sendDueInvoiceReminders()
        {
            DataTable t = DBHelper.getdataTable(
                "SELECT inv.ID AS InvoiceId, inv.Total_Price, inv.Expiration_Date, u.ID AS UserID, " +
                "CONCAT(u.Name,' ',u.LastName) AS Naam " +
                "FROM invoice inv JOIN rental r ON r.ID = inv.Rental_ID JOIN user u ON u.ID = r.UserID " +
                "WHERE LOWER(inv.Status) = 'open' AND inv.Reminder_Sent IS NULL " +
                "AND inv.Expiration_Date >= CURDATE() " +
                "AND inv.Expiration_Date <= DATE_ADD(CURDATE(), INTERVAL " + InvoiceWindowDays + " DAY)");

            foreach (DataRow r in t.Rows)
            {
                int uid = Convert.ToInt32(r["UserID"]);
                int invId = Convert.ToInt32(r["InvoiceId"]);
                string name = r["Naam"].ToString();
                double total = Convert.ToDouble(r["Total_Price"], CultureInfo.InvariantCulture);
                DateTime exp = Convert.ToDateTime(r["Expiration_Date"]);
                sendMessage(uid, InvoiceReminderBody(name, invId, total, exp));
                DBHelper.RunSql("UPDATE invoice SET Reminder_Sent = CURDATE() WHERE ID = " + invId);
            }
        }

        private static void sendMessage(int customerId, string body)
        {
            var p = new Dictionary<string, object>
            {
                { "@cid", customerId },
                { "@sid", DBHelper.loggedinuserid },
                { "@body", body },
            };
            DBHelper.RunSql(
                "INSERT INTO message (Customer_ID, From_Staff, Sender_ID, Sender_Name, Body, Read_By_Staff) " +
                "VALUES (@cid, 1, @sid, 'Rent A Car', @body, 1)", p);
        }

        // --- Shared text, used for both the in-app message and the e-mail ---

        public static string InvoiceNumber(int invoiceId)
        {
            return "F-" + invoiceId.ToString("D5");
        }

        public static string IdReminderBody(string name, DateTime expiry)
        {
            return
                "Beste " + name + ",\r\n\r\n" +
                "Je identiteitsbewijs bij Rent A Car verloopt op " + expiry.ToString("dd-MM-yyyy", NL) + ". " +
                "Vernieuw het op tijd en upload een nieuw ID, zodat je zonder onderbreking auto's kunt blijven huren.\r\n\r\n" +
                "Met vriendelijke groet,\r\nRent A Car";
        }

        public static string InvoiceReminderBody(string name, int invoiceId, double total, DateTime expiry)
        {
            return
                "Beste " + name + ",\r\n\r\n" +
                "Factuur " + InvoiceNumber(invoiceId) + " van € " + total.ToString("N2", NL) +
                " vervalt op " + expiry.ToString("dd-MM-yyyy", NL) + ". " +
                "Gelieve het bedrag voor de vervaldatum te voldoen om extra kosten te voorkomen.\r\n\r\n" +
                "Met vriendelijke groet,\r\nRent A Car";
        }
    }
}
