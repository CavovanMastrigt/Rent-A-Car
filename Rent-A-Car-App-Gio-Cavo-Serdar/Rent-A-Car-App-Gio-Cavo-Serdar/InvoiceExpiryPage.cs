using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Overview of unpaid invoices that are about to expire (within
    // ReminderHelper.InvoiceWindowDays). Double-click opens the invoice on the
    // Facturen page; a button e-mails the customer a payment reminder.
    public class InvoiceExpiryPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private TextBox txtSearch;
        private DataGridView grid;
        private Label lblCount;
        private int daysColIndex = -1;
        private DataTable all;

        public InvoiceExpiryPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => load();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(layout);

            var header = new Label { Text = "Bijna verlopen facturen", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };
            var sub = new Label { Text = "Openstaande facturen die binnen " + ReminderHelper.InvoiceWindowDays + " dagen vervallen. Dubbelklik om de factuur te openen.", Dock = DockStyle.Fill, ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(sub, 0, 1);
            layout.Controls.Add(buildToolbar(), 0, 2);
            layout.Controls.Add(buildCard(), 0, 3);
        }

        private Panel buildToolbar()
        {
            var bar = new Panel { Dock = DockStyle.Fill };

            var lblSearch = new Label { Text = "Zoek op factuur, klant of kenteken", Location = new Point(2, 0), AutoSize = true, ForeColor = Muted };
            txtSearch = new TextBox { Location = new Point(2, 18), Width = 300, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();
            lblCount = new Label { Location = new Point(320, 24), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            var actions = new FlowLayoutPanel { Dock = DockStyle.Right, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 13, 0, 0) };
            var btnMail = new Button { Text = "Stuur herinnering (e-mail)", Size = new Size(210, 34), Margin = new Padding(8, 0, 0, 0), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnMail.FlatAppearance.BorderSize = 0;
            btnMail.Click += (s, e) => mailSelected();
            actions.Controls.Add(btnMail);

            bar.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblCount, actions });
            return bar;
        }

        private Panel buildCard()
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            card.Paint += (s, e) => { using (var pen = new Pen(Border)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
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
                RowTemplate = { Height = 32 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 251);

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvoiceId", DataPropertyName = "InvoiceId", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DagenNum", DataPropertyName = "DagenNum", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Factuur", HeaderText = "Factuur", DataPropertyName = "Factuur", FillWeight = 60 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Klant", HeaderText = "Klant", DataPropertyName = "Klant", FillWeight = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Auto", HeaderText = "Auto", DataPropertyName = "Auto", FillWeight = 105 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Bedrag", HeaderText = "Bedrag", DataPropertyName = "Bedrag", FillWeight = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Vervaldatum", HeaderText = "Vervaldatum", DataPropertyName = "Vervaldatum", FillWeight = 70 });
            var dc = new DataGridViewTextBoxColumn { Name = "Dagen", HeaderText = "Resterend", DataPropertyName = "Dagen", FillWeight = 60 };
            grid.Columns.Add(dc); daysColIndex = dc.Index;
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Herinnerd", HeaderText = "Bericht", DataPropertyName = "Herinnerd", FillWeight = 60 });

            grid.CellFormatting += grid_CellFormatting;
            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                object v = grid.Rows[e.RowIndex].Cells["InvoiceId"].Value;
                if (v == null || v == DBNull.Value) return;
                AdminNav.Open(PermissionHelper.Invoices, Convert.ToInt32(v));
            };

            card.Controls.Add(grid);
            return card;
        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != daysColIndex) return;
            int days;
            if (!int.TryParse(Convert.ToString(grid.Rows[e.RowIndex].Cells["DagenNum"].Value), out days)) return;
            e.CellStyle.ForeColor = days <= 2 ? Color.FromArgb(153, 27, 27) : days <= 4 ? Color.FromArgb(146, 64, 14) : Color.FromArgb(55, 65, 81);
            e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }

        private void load()
        {
            ReminderHelper.EnsureSchema();
            try
            {
                all = DBHelper.getdataTable(
                    "SELECT inv.ID AS InvoiceId, inv.Total_Price, inv.Expiration_Date, inv.Reminder_Sent, " +
                    "DATEDIFF(inv.Expiration_Date, CURDATE()) AS Dagen, " +
                    "CONCAT(u.Name,' ',u.LastName) AS Klant, u.Email, c.Brand, c.Model, c.License_Plate " +
                    "FROM invoice inv JOIN rental r ON r.ID = inv.Rental_ID " +
                    "JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID " +
                    "WHERE LOWER(inv.Status) = 'open' AND inv.Expiration_Date >= CURDATE() " +
                    "AND inv.Expiration_Date <= DATE_ADD(CURDATE(), INTERVAL " + ReminderHelper.InvoiceWindowDays + " DAY) " +
                    "ORDER BY inv.Expiration_Date");
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de facturen niet laden:\n" + ex.Message, "Bijna verlopen facturen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyFilter()
        {
            if (all == null) return;
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();

            var d = new DataTable();
            d.Columns.Add("InvoiceId", typeof(int));
            d.Columns.Add("DagenNum", typeof(int));
            d.Columns.Add("Factuur");
            d.Columns.Add("Klant");
            d.Columns.Add("Auto");
            d.Columns.Add("Bedrag");
            d.Columns.Add("Vervaldatum");
            d.Columns.Add("Dagen");
            d.Columns.Add("Herinnerd");

            foreach (DataRow r in all.Rows)
            {
                int id = Convert.ToInt32(r["InvoiceId"]);
                string invNo = ReminderHelper.InvoiceNumber(id);
                string klant = r["Klant"].ToString();
                string auto = r["Brand"] + " " + r["Model"] + " (" + r["License_Plate"] + ")";
                if (search != "" && !(invNo + " " + klant + " " + auto).ToLowerInvariant().Contains(search)) continue;

                int dagen = Convert.ToInt32(r["Dagen"]);
                DateTime exp = Convert.ToDateTime(r["Expiration_Date"]);
                double total = Convert.ToDouble(r["Total_Price"], CultureInfo.InvariantCulture);
                string herinnerd = r["Reminder_Sent"] == DBNull.Value
                    ? "Nee"
                    : Convert.ToDateTime(r["Reminder_Sent"]).ToString("dd-MM-yyyy", NL);
                d.Rows.Add(id, dagen, invNo, klant, auto, "€ " + total.ToString("N2", NL),
                    exp.ToString("dd-MM-yyyy", NL), dagen + (dagen == 1 ? " dag" : " dagen"), herinnerd);
            }

            grid.DataSource = d;
            lblCount.Text = d.Rows.Count + (d.Rows.Count == 1 ? " factuur" : " facturen");
        }

        private void mailSelected()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["InvoiceId"].Value == null)
            {
                MessageBox.Show("Selecteer eerst een factuur.", "Herinnering", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = Convert.ToInt32(grid.CurrentRow.Cells["InvoiceId"].Value);
            DataRow src = null;
            foreach (DataRow r in all.Rows) { if (Convert.ToInt32(r["InvoiceId"]) == id) { src = r; break; } }
            if (src == null) return;

            string name = src["Klant"].ToString();
            string email = Convert.ToString(src["Email"]);
            double total = Convert.ToDouble(src["Total_Price"], CultureInfo.InvariantCulture);
            DateTime exp = Convert.ToDateTime(src["Expiration_Date"]);

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Deze klant heeft geen e-mailadres.", "Herinnering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!EmailHelper.IsConfigured())
            {
                MessageBox.Show("De e-mailinstellingen (SMTP) zijn nog niet ingesteld.", "Herinnering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string invNo = ReminderHelper.InvoiceNumber(id);
            if (MessageBox.Show("Betaalherinnering voor " + invNo + " sturen naar " + email + "?", "Herinnering", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            string body = ReminderHelper.InvoiceReminderBody(name, id, total, exp);
            try
            {
                EmailHelper.Send(email, name, "Herinnering: factuur " + invNo + " vervalt binnenkort - Rent A Car", body, null, null);
                LogHelper.Write("Factuurherinnering gemaild", invNo + " naar " + email);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de e-mail niet versturen:\n" + ex.Message, "Herinnering", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("De herinnering is verstuurd naar " + email + ".", "Herinnering", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
