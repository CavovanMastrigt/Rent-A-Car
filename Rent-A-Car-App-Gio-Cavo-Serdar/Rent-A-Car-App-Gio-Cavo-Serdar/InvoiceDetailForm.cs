using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Small popup that shows just the invoice for one reservation, opened from the
    // Reserveringen page. Read-only: view the details, open or download the PDF.
    public class InvoiceDetailForm : PopupForm
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private readonly int rentalId;
        private PdfInvoice.Data current;

        // Set when the user clicks "Factuur maken": the caller then opens the
        // Facturen page (preselecting this invoice when one exists).
        public bool GoToInvoices;
        public int GoToInvoiceId = -1;

        public InvoiceDetailForm(int rentalId)
        {
            this.rentalId = rentalId;
            ClientSize = new Size(560, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Factuur";
            load();
            buildUi();
        }

        private void load()
        {
            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT i.ID AS InvoiceId, i.Total_Price, i.Expiration_Date, i.Status, " +
                    "r.ID AS RentalId, r.Date_Started, r.Date_Planned_Ending, r.Price_Per_Day, r.Created_At, " +
                    "CONCAT(u.Name,' ',u.LastName) AS Klant, u.Address, u.Postal_Code AS Postcode, u.City, u.Email, " +
                    "c.Brand, c.Model, c.License_Plate " +
                    "FROM invoice i " +
                    "JOIN rental r ON r.ID = i.Rental_ID " +
                    "JOIN user u ON u.ID = r.UserID " +
                    "JOIN car c ON c.ID = r.CarID " +
                    "WHERE i.Rental_ID = " + rentalId + " LIMIT 1");
                if (t.Rows.Count == 0) return;
                DataRow r = t.Rows[0];

                DateTime start = Convert.ToDateTime(r["Date_Started"]);
                DateTime end = Convert.ToDateTime(r["Date_Planned_Ending"]);
                int days = Math.Max(1, (int)Math.Round((end.Date - start.Date).TotalDays));
                double pricePerDay = Convert.ToDouble(r["Price_Per_Day"], CultureInfo.InvariantCulture);
                double total = Convert.ToDouble(r["Total_Price"], CultureInfo.InvariantCulture);
                DateTime date = r["Created_At"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(r["Created_At"]);
                DateTime exp = Convert.ToDateTime(r["Expiration_Date"]);
                string key = statusKey(r["Status"].ToString(), exp);

                current = new PdfInvoice.Data
                {
                    InvoiceId = Convert.ToInt32(r["InvoiceId"]),
                    Date = date,
                    Expiration = exp,
                    CustomerName = r["Klant"].ToString(),
                    CustomerAddress = r["Address"].ToString(),
                    CustomerPostalCode = r["Postcode"].ToString(),
                    CustomerCity = r["City"].ToString(),
                    CustomerEmail = r["Email"].ToString(),
                    RentalId = Convert.ToInt32(r["RentalId"]),
                    CarText = r["Brand"] + " " + r["Model"] + " (" + r["License_Plate"] + ")",
                    Start = start,
                    End = end,
                    Days = days,
                    PricePerDay = pricePerDay,
                    Total = total,
                    StatusLabel = statusLabel(key),
                };

                try
                {
                    DataTable opt = DBHelper.getdataTable(
                        "SELECT eo.Name, ro.Option_Price FROM rental_option ro " +
                        "LEFT JOIN extra_option eo ON eo.ID = ro.Option_ID WHERE ro.Rental_ID = " + current.RentalId);
                    foreach (DataRow o in opt.Rows)
                    {
                        string name = o["Name"] == DBNull.Value ? "Optie" : o["Name"].ToString();
                        double price = Convert.ToDouble(o["Option_Price"], CultureInfo.InvariantCulture);
                        current.Options.Add(new PdfInvoice.Option { Name = name, Price = price });
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de factuur niet laden:\n" + ex.Message, "Factuur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buildUi()
        {
            var outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0) };
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));
            Controls.Add(outer);

            // Header
            var head = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var title = new Label
            {
                Text = current == null ? "Geen factuur" : "Factuur F-" + current.InvoiceId.ToString("D5"),
                Location = new Point(20, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Ink
            };
            var btnClose = new Button { Text = "✕", Location = new Point(ClientSize.Width - 36, 10), Size = new Size(28, 28), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            var sep = new Label { Location = new Point(0, 57), Size = new Size(ClientSize.Width, 1), BackColor = Border };
            head.Controls.AddRange(new Control[] { title, btnClose, sep });

            // Body
            var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(22, 12, 22, 8) };
            var detail = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.5F),
                ForeColor = Color.FromArgb(55, 65, 81),
                Text = current == null
                    ? "Er is nog geen factuur voor deze reservering.\r\n\r\n" +
                      "Een factuur wordt aangemaakt wanneer de reservering wordt verwerkt."
                    : invoiceText(current)
            };
            body.Controls.Add(detail);

            // Footer with actions
            var footer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 10, 16, 12) };
            var row = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 2, 0, 0) };

            var btnMake = footerBtn("Factuur maken", true, 130);
            btnMake.Click += (s, e) =>
            {
                GoToInvoices = true;
                GoToInvoiceId = current == null ? -1 : current.InvoiceId;
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnOpen = footerBtn("PDF openen", false, 110);
            btnOpen.Enabled = current != null;
            btnOpen.Click += (s, e) => openPdf();

            var btnDownload = footerBtn("PDF downloaden", false, 140);
            btnDownload.Enabled = current != null;
            btnDownload.Click += (s, e) => downloadPdf();

            var btnSluit = footerBtn("Sluiten", false, 90);
            btnSluit.DialogResult = DialogResult.Cancel;

            row.Controls.AddRange(new Control[] { btnMake, btnOpen, btnDownload, btnSluit });
            footer.Controls.Add(row);

            outer.Controls.Add(head, 0, 0);
            outer.Controls.Add(body, 0, 1);
            outer.Controls.Add(footer, 0, 2);

            CancelButton = btnClose;
        }

        private string invoiceText(PdfInvoice.Data d)
        {
            double carSubtotal = d.PricePerDay * d.Days;
            string optionsText = "";
            foreach (PdfInvoice.Option o in d.Options)
                optionsText += "Extra " + o.Name + ":  " + euro(o.Price) + "\r\n";

            return
                "Status:         " + d.StatusLabel + "\r\n" +
                "Factuurdatum:   " + d.Date.ToString("dd-MM-yyyy", NL) + "\r\n" +
                "Vervaldatum:    " + d.Expiration.ToString("dd-MM-yyyy", NL) + "\r\n\r\n" +
                "Klant:          " + d.CustomerName + "\r\n" +
                "Adres:          " + d.CustomerAddress + "\r\n" +
                "Postcode:       " + d.CustomerPostalCode + "\r\n" +
                "Woonplaats:     " + d.CustomerCity + "\r\n" +
                "E-mail:         " + d.CustomerEmail + "\r\n\r\n" +
                "Auto:           " + d.CarText + "\r\n" +
                "Periode:        " + d.Start.ToString("dd-MM-yyyy", NL) + " t/m " + d.End.ToString("dd-MM-yyyy", NL) + "\r\n" +
                "Aantal dagen:   " + d.Days + "\r\n" +
                "Prijs per dag:  " + euro(d.PricePerDay) + "\r\n" +
                "Huur subtotaal: " + euro(carSubtotal) + "\r\n" +
                optionsText +
                "Totaal:         " + euro(d.Total) + "\r\n\r\n" +
                "Betaling naar:  " + PdfInvoice.Iban + " (" + PdfInvoice.Beneficiary + ")";
        }

        private Button footerBtn(string text, bool primary, int width)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(width, 36),
                Margin = new Padding(0, 0, 8, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? Accent : Color.White,
                ForeColor = primary ? Color.White : Color.FromArgb(31, 41, 55),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            if (primary) b.FlatAppearance.BorderSize = 0;
            else { b.FlatAppearance.BorderColor = Border; b.FlatAppearance.BorderSize = 1; }
            return b;
        }

        private void openPdf()
        {
            if (current == null) return;
            try
            {
                string path = Path.Combine(Path.GetTempPath(), "Factuur-F" + current.InvoiceId.ToString("D5") + ".pdf");
                File.WriteAllBytes(path, PdfInvoice.Build(current));
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de PDF niet openen:\n" + ex.Message, "Factuur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void downloadPdf()
        {
            if (current == null) return;
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF-bestand|*.pdf";
                dialog.FileName = "Factuur-F" + current.InvoiceId.ToString("D5") + ".pdf";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllBytes(dialog.FileName, PdfInvoice.Build(current));
                    MessageBox.Show("De factuur is opgeslagen.", "Factuur", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kon de PDF niet opslaan:\n" + ex.Message, "Factuur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 'paid' -> paid; 'open' past expiry -> overdue; otherwise open.
        private static string statusKey(string raw, DateTime expiration)
        {
            if (raw == "paid") return "paid";
            return expiration.Date < DateTime.Today ? "overdue" : "open";
        }

        private static string statusLabel(string key)
        {
            switch (key)
            {
                case "paid": return "Betaald";
                case "overdue": return "Verlopen";
                default: return "Openstaand";
            }
        }

        private static string euro(double value)
        {
            return "€ " + value.ToString("N2", NL);
        }
    }
}
