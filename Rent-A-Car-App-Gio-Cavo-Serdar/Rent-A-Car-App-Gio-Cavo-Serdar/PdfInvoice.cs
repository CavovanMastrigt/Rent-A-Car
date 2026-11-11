using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Minimal, dependency-free PDF generator for a one-page rental invoice.
    // Writes a valid PDF 1.4 by hand (Helvetica, WinAnsi) so no NuGet package is
    // needed. Used by the invoices page to download / open an invoice.
    internal static class PdfInvoice
    {
        public class Data
        {
            public int InvoiceId;
            public DateTime Date;
            public DateTime Expiration;
            public string CustomerName = "";
            public string CustomerAddress = "";
            public string CustomerCity = "";
            public string CustomerEmail = "";
            public int RentalId;
            public string CarText = "";
            public DateTime Start;
            public DateTime End;
            public int Days;
            public double PricePerDay;
            public double Total;
        }

        private static readonly Encoding Win = Encoding.GetEncoding(1252);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        public static byte[] Build(Data d)
        {
            var body = new StringBuilder();

            // Header
            text(body, 60, 60, 24, true, "FACTUUR");
            text(body, 60, 96, 13, true, "Rent A Car");
            text(body, 60, 114, 9, false, "Autostraat 1, 7500 AA Enschede");
            text(body, 60, 127, 9, false, "info@rentacar.nl  -  www.rentacar.nl");

            // Invoice meta (right column)
            text(body, 360, 96, 9, true, "Factuurnummer");
            text(body, 470, 96, 9, false, "F-" + d.InvoiceId.ToString("D5"));
            text(body, 360, 112, 9, true, "Factuurdatum");
            text(body, 470, 112, 9, false, d.Date.ToString("dd-MM-yyyy", NL));
            text(body, 360, 128, 9, true, "Vervaldatum");
            text(body, 470, 128, 9, false, d.Expiration.ToString("dd-MM-yyyy", NL));

            // Bill-to block
            text(body, 60, 180, 10, true, "Factuuradres");
            text(body, 60, 198, 10, false, d.CustomerName);
            text(body, 60, 213, 10, false, d.CustomerAddress);
            text(body, 60, 228, 10, false, d.CustomerCity);
            if (!string.IsNullOrEmpty(d.CustomerEmail))
            {
                text(body, 60, 243, 10, false, d.CustomerEmail);
            }

            // Table header
            line(body, 60, 285, 535, 285);
            text(body, 60, 278, 10, true, "Omschrijving");
            text(body, 330, 278, 10, true, "Aantal");
            text(body, 400, 278, 10, true, "Prijs/dag");
            text(body, 500, 278, 10, true, "Totaal");
            line(body, 60, 292, 535, 292);

            // Line item
            text(body, 60, 312, 10, false, "Autohuur: " + d.CarText);
            text(body, 60, 328, 9, false, "Periode: " + d.Start.ToString("dd-MM-yyyy", NL) + " t/m " + d.End.ToString("dd-MM-yyyy", NL));
            text(body, 330, 312, 10, false, d.Days.ToString());
            text(body, 400, 312, 10, false, euro(d.PricePerDay));
            text(body, 500, 312, 10, false, euro(d.PricePerDay * d.Days));

            // Totals
            line(body, 60, 360, 535, 360);
            text(body, 400, 376, 11, true, "Totaal");
            text(body, 500, 376, 11, true, euro(d.Total));

            // Footer
            text(body, 60, 430, 9, false, "Gelieve het totaalbedrag voor de vervaldatum over te maken onder vermelding van het");
            text(body, 60, 443, 9, false, "factuurnummer. Reservering " + d.RentalId + ".");
            text(body, 60, 800, 8, false, "Rent A Car - automatisch gegenereerde factuur");

            byte[] content = Win.GetBytes(body.ToString());

            string[] objs =
            {
                "<</Type/Catalog/Pages 2 0 R>>",
                "<</Type/Pages/Kids[3 0 R]/Count 1>>",
                "<</Type/Page/Parent 2 0 R/MediaBox[0 0 595 842]/Resources<</Font<</F1 4 0 R/F2 5 0 R>>>>/Contents 6 0 R>>",
                "<</Type/Font/Subtype/Type1/BaseFont/Helvetica/Encoding/WinAnsiEncoding>>",
                "<</Type/Font/Subtype/Type1/BaseFont/Helvetica-Bold/Encoding/WinAnsiEncoding>>",
            };

            var ms = new MemoryStream();
            var offsets = new long[7];
            Action<string> w = s => { byte[] b = Win.GetBytes(s); ms.Write(b, 0, b.Length); };

            w("%PDF-1.4\n%âãÏÓ\n");
            for (int i = 0; i < objs.Length; i++)
            {
                offsets[i + 1] = ms.Length;
                w((i + 1) + " 0 obj\n" + objs[i] + "\nendobj\n");
            }
            // Content stream object (6)
            offsets[6] = ms.Length;
            w("6 0 obj\n<</Length " + content.Length + ">>\nstream\n");
            ms.Write(content, 0, content.Length);
            w("\nendstream\nendobj\n");

            long xref = ms.Length;
            w("xref\n0 7\n0000000000 65535 f \n");
            for (int i = 1; i <= 6; i++)
            {
                w(offsets[i].ToString("D10") + " 00000 n \n");
            }
            w("trailer\n<</Size 7/Root 1 0 R>>\nstartxref\n" + xref + "\n%%EOF");

            return ms.ToArray();
        }

        private static string euro(double value)
        {
            return "€ " + value.ToString("N2", NL);
        }

        // Adds a line of text. y is measured from the top of an A4 page (842 pt tall).
        private static void text(StringBuilder sb, int x, int yFromTop, int size, bool bold, string value)
        {
            int y = 842 - yFromTop;
            sb.Append("BT /").Append(bold ? "F2" : "F1").Append(' ').Append(size).Append(" Tf ")
              .Append(x).Append(' ').Append(y).Append(" Td (").Append(escape(value)).Append(") Tj ET\n");
        }

        private static void line(StringBuilder sb, int x1, int yTop1, int x2, int yTop2)
        {
            sb.Append("0.8 0.8 0.8 RG 0.7 w ")
              .Append(x1).Append(' ').Append(842 - yTop1).Append(" m ")
              .Append(x2).Append(' ').Append(842 - yTop2).Append(" l S\n");
        }

        private static string escape(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }
    }
}
