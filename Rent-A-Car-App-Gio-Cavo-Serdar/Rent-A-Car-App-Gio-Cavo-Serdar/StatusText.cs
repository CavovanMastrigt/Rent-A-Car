namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // De database bewaart statussen in het Engels (reserved, ongoing, ...).
    // Deze klasse vertaalt ze naar de Nederlandse tekst die op het scherm getoond wordt.
    internal static class StatusText
    {
        // Status van een reservering (rental.Status).
        public static string Rental(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "reserved":
                    return "Gereserveerd";
                case "ongoing":
                    return "Verhuurd";
                case "completed":
                    return "Afgerond";
                case "cancelled":
                    return "Geannuleerd";
                default:
                    return status;
            }
        }

        // Status van een auto (car.Car_Status).
        public static string Car(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "active":
                    return "Beschikbaar";
                case "maintenance":
                    return "In onderhoud";
                case "disabled":
                    return "Buiten gebruik";
                default:
                    return status;
            }
        }
    }
}
