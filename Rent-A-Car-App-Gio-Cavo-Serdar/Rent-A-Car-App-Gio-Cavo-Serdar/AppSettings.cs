using System;
using System.Collections.Generic;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Small key/value store in the app_setting table (used for the SMTP config).
    internal static class AppSettings
    {
        public static string Get(string key, string fallback = "")
        {
            try
            {
                object v = DBHelper.getScalar("SELECT Setting_Value FROM app_setting WHERE Setting_Key = " + AuthHelper.sql(key));
                if (v == null || v == DBNull.Value) return fallback;
                return v.ToString();
            }
            catch { return fallback; }
        }

        public static void Set(string key, string value)
        {
            var p = new Dictionary<string, object> { { "@k", key }, { "@v", value ?? "" } };
            DBHelper.RunSql(
                "INSERT INTO app_setting (Setting_Key, Setting_Value) VALUES (@k, @v) " +
                "ON DUPLICATE KEY UPDATE Setting_Value = @v", p);
        }
    }
}
