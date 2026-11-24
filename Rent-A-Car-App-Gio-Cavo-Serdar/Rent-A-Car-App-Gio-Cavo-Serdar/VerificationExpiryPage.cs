using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Overview of customers whose verified ID card is about to expire (within
    // ReminderHelper.IdWindowDays). Double-click a customer to open their ID form
    // (the verification review popup). A button e-mails the customer a reminder.
    public class VerificationExpiryPage : UserControl
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

        public VerificationExpiryPage()
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

            var header = new Label { Text = "Bijna verlopen ID-kaarten", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };
            var sub = new Label { Text = "ID-kaarten die binnen " + ReminderHelper.IdWindowDays + " dagen verlopen. Dubbelklik een klant om het ID-formulier te openen.", Dock = DockStyle.Fill, ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(sub, 0, 1);
            layout.Controls.Add(buildToolbar(), 0, 2);
            layout.Controls.Add(buildCard(), 0, 3);
        }

        private Panel buildToolbar()
        {
            var bar = new Panel { Dock = DockStyle.Fill };

            var lblSearch = new Label { Text = "Zoek op naam of e-mail", Location = new Point(2, 0), AutoSize = true, ForeColor = Muted };
            txtSearch = new TextBox { Location = new Point(2, 18), Width = 280, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();
            lblCount = new Label { Location = new Point(300, 24), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

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

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserID", DataPropertyName = "UserID", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DagenNum", DataPropertyName = "DagenNum", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Naam", HeaderText = "Klant", DataPropertyName = "Naam", FillWeight = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "E-mail", DataPropertyName = "Email", FillWeight = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Vervaldatum", HeaderText = "Vervaldatum", DataPropertyName = "Vervaldatum", FillWeight = 75 });
            var dc = new DataGridViewTextBoxColumn { Name = "Dagen", HeaderText = "Resterend", DataPropertyName = "Dagen", FillWeight = 65 };
            grid.Columns.Add(dc); daysColIndex = dc.Index;
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Herinnerd", HeaderText = "Bericht", DataPropertyName = "Herinnerd", FillWeight = 70 });

            grid.CellFormatting += grid_CellFormatting;
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) openIdForm(); };

            card.Controls.Add(grid);
            return card;
        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != daysColIndex) return;
            int days;
            if (!int.TryParse(Convert.ToString(grid.Rows[e.RowIndex].Cells["DagenNum"].Value), out days)) return;
            // The closer to expiry, the more urgent the colour.
            e.CellStyle.ForeColor = days <= 7 ? Color.FromArgb(153, 27, 27) : days <= 14 ? Color.FromArgb(146, 64, 14) : Color.FromArgb(55, 65, 81);
            e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }

        private void load()
        {
            ReminderHelper.EnsureSchema();
            try
            {
                all = DBHelper.getdataTable(
                    "SELECT u.ID AS UserID, CONCAT(u.Name,' ',u.LastName) AS Naam, u.Email, " +
                    "i.Expiration_Date, DATEDIFF(i.Expiration_Date, CURDATE()) AS Dagen, i.ID_Reminder_Sent " +
                    "FROM identity i JOIN user u ON u.ID = i.UserID " +
                    "WHERE i.Identity_Status = 'Verified' AND i.Expiration_Date IS NOT NULL " +
                    "AND i.Expiration_Date >= CURDATE() " +
                    "AND i.Expiration_Date <= DATE_ADD(CURDATE(), INTERVAL " + ReminderHelper.IdWindowDays + " DAY) " +
                    "ORDER BY i.Expiration_Date");
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de ID-kaarten niet laden:\n" + ex.Message, "Bijna verlopen ID-kaarten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyFilter()
        {
            if (all == null) return;
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();

            var d = new DataTable();
            d.Columns.Add("UserID", typeof(int));
            d.Columns.Add("DagenNum", typeof(int));
            d.Columns.Add("Naam");
            d.Columns.Add("Email");
            d.Columns.Add("Vervaldatum");
            d.Columns.Add("Dagen");
            d.Columns.Add("Herinnerd");

            foreach (DataRow r in all.Rows)
            {
                string naam = r["Naam"].ToString();
                string email = r["Email"].ToString();
                if (search != "" && !(naam + " " + email).ToLowerInvariant().Contains(search)) continue;

                int dagen = Convert.ToInt32(r["Dagen"]);
                DateTime exp = Convert.ToDateTime(r["Expiration_Date"]);
                string herinnerd = r["ID_Reminder_Sent"] == DBNull.Value
                    ? "Nee"
                    : Convert.ToDateTime(r["ID_Reminder_Sent"]).ToString("dd-MM-yyyy", NL);
                d.Rows.Add(Convert.ToInt32(r["UserID"]), dagen, naam, email,
                    exp.ToString("dd-MM-yyyy", NL), dagen + (dagen == 1 ? " dag" : " dagen"), herinnerd);
            }

            grid.DataSource = d;
            lblCount.Text = d.Rows.Count + (d.Rows.Count == 1 ? " ID-kaart" : " ID-kaarten");
        }

        // Opens the customer's ID form - the same review popup as Klantverificatie.
        private void openIdForm()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["UserID"].Value == null) return;
            int uid = Convert.ToInt32(grid.CurrentRow.Cells["UserID"].Value);
            string name = grid.CurrentRow.Cells["Naam"].Value.ToString();
            string email = grid.CurrentRow.Cells["Email"].Value.ToString();
            using (var dlg = new VerificationReviewForm(uid, name, email))
            {
                if (PopupForm.show(FindForm(), dlg) == DialogResult.OK) load();
            }
        }

        private void mailSelected()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["UserID"].Value == null)
            {
                MessageBox.Show("Selecteer eerst een klant.", "Herinnering", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string name = grid.CurrentRow.Cells["Naam"].Value.ToString();
            string email = Convert.ToString(grid.CurrentRow.Cells["Email"].Value);
            DateTime exp = DateTime.ParseExact(grid.CurrentRow.Cells["Vervaldatum"].Value.ToString(), "dd-MM-yyyy", NL);

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
            if (MessageBox.Show("Herinnering sturen naar " + email + "?", "Herinnering", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            string body = ReminderHelper.IdReminderBody(name, exp);
            try
            {
                EmailHelper.Send(email, name, "Je ID-kaart verloopt binnenkort - Rent A Car", body, null, null);
                LogHelper.Write("ID-herinnering gemaild", name + " (" + email + ")");
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
