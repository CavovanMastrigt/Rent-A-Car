using System;
using System.Data;
using System.Globalization;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Controleert of een moment binnen de openingstijden valt (tabel openingtimes).
    // Gebruikt door Nieuwe reservering en Ophalen / terugbrengen.
    // Is er voor een dag niets ingesteld (of is de tabel leeg), dan wordt er niet gecontroleerd.
    internal static class OpeningHours
    {
        private static readonly CultureInfo Dutch = new CultureInfo("nl-NL");

        // Mogelijke schrijfwijzen van een weekdag in de tabel, geïndexeerd op (int)DayOfWeek (zondag = 0).
        private static readonly string[] EnglishDays = { "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };
        private static readonly string[] EnglishShort = { "sun", "mon", "tue", "wed", "thu", "fri", "sat" };
        private static readonly string[] DutchDays = { "zondag", "maandag", "dinsdag", "woensdag", "donderdag", "vrijdag", "zaterdag" };
        private static readonly string[] DutchShort = { "zo", "ma", "di", "wo", "do", "vr", "za" };

        private static DataTable table = new DataTable();

        // True als er openingstijden zijn ingesteld.
        public static bool IsConfigured
        {
            get { return table.Rows.Count > 0; }
        }

        // Haalt de openingstijden opnieuw uit de database.
        public static void Load()
        {
            try
            {
                table = DBHelper.getdataTable("SELECT Weekday, Time_Open, Time_Closed, Is_Closed FROM openingtimes");
            }
            catch (Exception)
            {
                // Zonder de kolom Is_Closed (migratie 003) of de tabel zelf wordt er niet gecontroleerd.
                table = new DataTable();
            }
        }

        // Geeft een foutmelding als het moment buiten de openingstijden valt, anders null.
        // "what" is bijvoorbeeld "Ophalen" of "Terugbrengen".
        public static string Check(DateTime moment, string what)
        {
            DataRow row = findRow(moment.DayOfWeek);
            if (row == null)
            {
                return null;
            }

            string dayName = Dutch.DateTimeFormat.GetDayName(moment.DayOfWeek);
            if (Convert.ToBoolean(row["Is_Closed"]))
            {
                return what + " kan niet op " + dayName + ": dan zijn we gesloten.";
            }

            TimeSpan open = toTimeSpan(row["Time_Open"]);
            TimeSpan closed = toTimeSpan(row["Time_Closed"]);
            TimeSpan time = moment.TimeOfDay;
            if (time < open || time > closed)
            {
                return what + " kan op " + dayName + " alleen tussen " +
                       open.ToString(@"hh\:mm") + " en " + closed.ToString(@"hh\:mm") + ".";
            }

            return null;
        }

        private static DataRow findRow(DayOfWeek day)
        {
            int d = (int)day;
            string iso = (d == 0 ? 7 : d).ToString();

            foreach (DataRow row in table.Rows)
            {
                string weekday = Convert.ToString(row["Weekday"]).Trim().ToLowerInvariant();
                if (weekday == EnglishDays[d] || weekday == EnglishShort[d] ||
                    weekday == DutchDays[d] || weekday == DutchShort[d] ||
                    weekday == iso || (d == 0 && weekday == "0"))
                {
                    return row;
                }
            }
            return null;
        }

        private static TimeSpan toTimeSpan(object value)
        {
            if (value is TimeSpan)
            {
                return (TimeSpan)value;
            }
            if (value is DateTime)
            {
                return ((DateTime)value).TimeOfDay;
            }
            return TimeSpan.Parse(Convert.ToString(value), CultureInfo.InvariantCulture);
        }
    }
}
