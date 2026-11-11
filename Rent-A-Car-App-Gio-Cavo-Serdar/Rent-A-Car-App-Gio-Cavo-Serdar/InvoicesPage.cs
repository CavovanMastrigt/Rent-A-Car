using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Invoices overview for admin and employee. Shows every invoice, the details
    // as labels, and generates a PDF on the fly to open or download.
    public class InvoicesPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private TextBox txtSearch;
        private DataGridView grid;
        private Label lblCount;
        private Label lblDetailTitle, lblDetail;
        private Button btnOpen, btnDownload;

        private DataTable all;              // every invoice with all columns
        private PdfInvoice.Data current;    // currently selected invoice

        public InvoicesPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadInvoices();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(layout);

            var header = new Label { Text = "Facturen", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };

            var toolbar = new Panel { Dock = DockStyle.Fill };
            txtSearch = new TextBox { Location = new Point(2, 8), Width = 300, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();
            lblCount = new Label { Location = new Point(320, 12), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9.5F) };
            toolbar.Controls.AddRange(new Control[] { txtSearch, lblCount });

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 6, 0, 0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            split.Controls.Add(buildGrid(), 0, 0);
            split.Controls.Add(buildDetail(), 1, 0);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(toolbar, 0, 1);
            layout.Controls.Add(split, 0, 2);
        }

        private DataGridView buildGrid()
        {
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 30 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 251);

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvoiceId", DataPropertyName = "InvoiceId", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Factuur", HeaderText = "Factuur", DataPropertyName = "Factuur", FillWeight = 70 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Klant", HeaderText = "Klant", DataPropertyName = "Klant", FillWeight = 110 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Auto", HeaderText = "Auto", DataPropertyName = "Auto", FillWeight = 110 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Bedrag", HeaderText = "Bedrag", DataPropertyName = "Bedrag", FillWeight = 70, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Vervaldatum", HeaderText = "Vervaldatum", DataPropertyName = "Vervaldatum", FillWeight = 80 });

            grid.SelectionChanged += (s, e) => loadDetail();
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) openPdf(); };
            return grid;
        }

        private Panel buildDetail()
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18) };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            lblDetailTitle = new Label { Dock = DockStyle.Top, Height = 32, Text = "Selecteer een factuur", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Ink };
            lblDetail = new Label { Dock = DockStyle.Top, Height = 300, Text = "", Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(55, 65, 81) };

            var buttons = new Panel { Dock = DockStyle.Bottom, Height = 52 };
            btnOpen = new Button
            {
                Text = "PDF openen",
                Size = new Size(150, 36),
                Location = new Point(0, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnOpen.FlatAppearance.BorderSize = 0;
            btnOpen.Click += (s, e) => openPdf();

            btnDownload = new Button
            {
                Text = "PDF downloaden",
                Size = new Size(160, 36),
                Location = new Point(160, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(31, 41, 55),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnDownload.FlatAppearance.BorderColor = Border;
            btnDownload.FlatAppearance.BorderSize = 1;
            btnDownload.Click += (s, e) => downloadPdf();

            buttons.Controls.AddRange(new Control[] { btnOpen, btnDownload });

            card.Controls.Add(buttons);
            card.Controls.Add(lblDetail);
            card.Controls.Add(lblDetailTitle);
            return card;
        }

        private void loadInvoices()
        {
            try
            {
                all = DBHelper.getdataTable(
                    "SELECT i.ID AS InvoiceId, i.Total_Price, i.Expiration_Date, " +
                    "r.ID AS RentalId, r.Date_Started, r.Date_Planned_Ending, r.Price_Per_Day, r.Created_At, " +
                    "CONCAT(u.Name,' ',u.LastName) AS Klant, u.Address, u.City, u.Email, " +
                    "c.Brand, c.Model, c.License_Plate " +
                    "FROM invoice i " +
                    "JOIN rental r ON r.ID = i.Rental_ID " +
                    "JOIN user u ON u.ID = r.UserID " +
                    "JOIN car c ON c.ID = r.CarID " +
                    "ORDER BY i.ID DESC");
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de facturen niet laden:\n" + ex.Message, "Facturen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyFilter()
        {
            if (all == null) return;
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();

            var display = new DataTable();
            display.Columns.Add("InvoiceId", typeof(int));
            display.Columns.Add("Factuur");
            display.Columns.Add("Klant");
            display.Columns.Add("Auto");
            display.Columns.Add("Bedrag");
            display.Columns.Add("Vervaldatum");

            foreach (DataRow r in all.Rows)
            {
                string invNo = "F-" + Convert.ToInt32(r["InvoiceId"]).ToString("D5");
                string car = r["Brand"] + " " + r["Model"] + " (" + r["License_Plate"] + ")";
                string klant = r["Klant"].ToString();
                if (search != "")
                {
                    string hay = (invNo + " " + klant + " " + car).ToLowerInvariant();
                    if (!hay.Contains(search)) continue;
                }
                display.Rows.Add(
                    Convert.ToInt32(r["InvoiceId"]),
                    invNo,
                    klant,
                    car,
                    euro(Convert.ToDouble(r["Total_Price"], CultureInfo.InvariantCulture)),
                    Convert.ToDateTime(r["Expiration_Date"]).ToString("dd-MM-yyyy", NL));
            }

            grid.DataSource = display;
            lblCount.Text = display.Rows.Count + (display.Rows.Count == 1 ? " factuur" : " facturen");
            loadDetail();
        }

        private void loadDetail()
        {
            current = null;
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["InvoiceId"].Value == null)
            {
                lblDetailTitle.Text = "Selecteer een factuur";
                lblDetail.Text = "";
                btnOpen.Enabled = false;
                btnDownload.Enabled = false;
                return;
            }

            int id = Convert.ToInt32(grid.CurrentRow.Cells["InvoiceId"].Value);
            DataRow r = null;
            foreach (DataRow row in all.Rows)
            {
                if (Convert.ToInt32(row["InvoiceId"]) == id) { r = row; break; }
            }
            if (r == null) return;

            DateTime start = Convert.ToDateTime(r["Date_Started"]);
            DateTime end = Convert.ToDateTime(r["Date_Planned_Ending"]);
            int days = Math.Max(1, (int)Math.Round((end.Date - start.Date).TotalDays));
            double pricePerDay = Convert.ToDouble(r["Price_Per_Day"], CultureInfo.InvariantCulture);
            double total = Convert.ToDouble(r["Total_Price"], CultureInfo.InvariantCulture);
            DateTime date = r["Created_At"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(r["Created_At"]);

            current = new PdfInvoice.Data
            {
                InvoiceId = id,
                Date = date,
                Expiration = Convert.ToDateTime(r["Expiration_Date"]),
                CustomerName = r["Klant"].ToString(),
                CustomerAddress = r["Address"].ToString(),
                CustomerCity = r["City"].ToString(),
                CustomerEmail = r["Email"].ToString(),
                RentalId = Convert.ToInt32(r["RentalId"]),
                CarText = r["Brand"] + " " + r["Model"] + " (" + r["License_Plate"] + ")",
                Start = start,
                End = end,
                Days = days,
                PricePerDay = pricePerDay,
                Total = total,
            };

            lblDetailTitle.Text = "Factuur F-" + id.ToString("D5");
            lblDetail.Text =
                "Factuurdatum:   " + date.ToString("dd-MM-yyyy", NL) + "\r\n" +
                "Vervaldatum:    " + current.Expiration.ToString("dd-MM-yyyy", NL) + "\r\n\r\n" +
                "Klant:          " + current.CustomerName + "\r\n" +
                "Adres:          " + current.CustomerAddress + "\r\n" +
                "Woonplaats:     " + current.CustomerCity + "\r\n" +
                "E-mail:         " + current.CustomerEmail + "\r\n\r\n" +
                "Auto:           " + current.CarText + "\r\n" +
                "Periode:        " + start.ToString("dd-MM-yyyy", NL) + " t/m " + end.ToString("dd-MM-yyyy", NL) + "\r\n" +
                "Aantal dagen:   " + days + "\r\n" +
                "Prijs per dag:  " + euro(pricePerDay) + "\r\n\r\n" +
                "Totaal:         " + euro(total);

            btnOpen.Enabled = true;
            btnDownload.Enabled = true;
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
                MessageBox.Show("Kon de PDF niet openen:\n" + ex.Message, "Facturen", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("De factuur is opgeslagen.", "Facturen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kon de PDF niet opslaan:\n" + ex.Message, "Facturen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string euro(double value)
        {
            return "€ " + value.ToString("N2", NL);
        }
    }
}
