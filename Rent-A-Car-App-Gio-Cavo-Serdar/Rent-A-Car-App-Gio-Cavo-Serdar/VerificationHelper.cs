using System;
using System.Drawing;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Helpers for ID verification (user story 8).
    internal static class VerificationHelper
    {
        public const string StatusNone = "None";
        public const string StatusPending = "Pending";
        public const string StatusVerified = "Verified";
        public const string StatusFailed = "Failed";

        public const long MaxFileBytes = 5 * 1024 * 1024; // 5 MB

        // Returns the identity status for a user, or "None" if they never submitted one.
        public static string GetStatus(int userId)
        {
            object value = DBHelper.getScalar(
                "SELECT Identity_Status FROM identity WHERE UserID = " + userId);
            if (value == null || value == DBNull.Value)
            {
                return StatusNone;
            }
            return value.ToString();
        }

        // A customer can only rent once their ID has been verified.
        public static bool IsVerified(int userId)
        {
            return GetStatus(userId) == StatusVerified;
        }

        // Keeps only the last two characters, e.g. "AB1234567" -> "*******67".
        // The full document number is never stored.
        public static string MaskDocument(string documentNumber)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                return "";
            }
            string trimmed = documentNumber.Trim();
            if (trimmed.Length <= 2)
            {
                return new string('*', trimmed.Length);
            }
            return new string('*', trimmed.Length - 2) + trimmed.Substring(trimmed.Length - 2);
        }

        // Dutch label for a status.
        public static string StatusLabel(string status)
        {
            switch (status)
            {
                case StatusVerified: return "Geverifieerd";
                case StatusPending: return "In behandeling";
                case StatusFailed: return "Afgekeurd";
                default: return "Niet geverifieerd";
            }
        }

        // Soft background colour for a status chip / grid cell.
        public static Color StatusBackColor(string status)
        {
            switch (status)
            {
                case StatusVerified: return Color.FromArgb(220, 252, 231); // green
                case StatusPending: return Color.FromArgb(254, 243, 199);  // amber
                case StatusFailed: return Color.FromArgb(254, 226, 226);   // red
                default: return Color.FromArgb(237, 239, 242);             // grey
            }
        }

        // Matching text colour for a status chip / grid cell.
        public static Color StatusForeColor(string status)
        {
            switch (status)
            {
                case StatusVerified: return Color.FromArgb(22, 101, 52);
                case StatusPending: return Color.FromArgb(146, 64, 14);
                case StatusFailed: return Color.FromArgb(153, 27, 27);
                default: return Color.FromArgb(75, 85, 99);
            }
        }
    }
}
