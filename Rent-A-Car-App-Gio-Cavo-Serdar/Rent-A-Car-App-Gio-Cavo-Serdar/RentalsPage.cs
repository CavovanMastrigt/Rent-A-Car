using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Staff view of all reservations. A reservation can be cancelled (e.g. the car
    // went into maintenance); the customer is then notified by message and e-mail
    // with the reason, and any invoice for it is removed.
    public class RentalsPage : UserControl, INavTarget
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private TextBox txtSearch;
        private DataGridView grid;
        private Label lblCount;
        private int statusCol = -1;
        private DataTable all;
        private int pendingRid = -1; // reservation to auto-select after a jump from the auto-view

        public RentalsPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadRentals();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(layout);

            var header = new Label { Text = "Reserveringen", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };

            var toolbar = new Panel { Dock = DockStyle.Fill };
            txtSearch = new TextBox { Location = new Point(2, 18), Width = 300, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();
            var lblSearch = new Label { Text = "Zoek op klant of kenteken", Location = new Point(2, 0), AutoSize = true, ForeColor = Muted };
            lblCount = new Label { Location = new Point(320, 24), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            var actions = new FlowLayoutPanel { Dock = DockStyle.Right, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 13, 0, 0) };
            var btnInvoice = new Button { Text = "Factuur bekijken", Size = new Size(150, 34), Margin = new Padding(8, 0, 0, 0), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnInvoice.FlatAppearance.BorderSize = 0;
            btnInvoice.Click += (s, e) => showInvoice();
            var btnCancel = new Button { Text = "Annuleren", Size = new Size(140, 34), Margin = new Padding(8, 0, 0, 0), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(185, 28, 28), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(248, 180, 180);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => cancelSelected();
            actions.Controls.AddRange(new Control[] { btnInvoice, btnCancel });

            toolbar.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblCount, actions });

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
                RowTemplate = { Height = 30 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 251);

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rid", DataPropertyName = "Rid", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "StCode", DataPropertyName = "StCode", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nr", HeaderText = "Nr", DataPropertyName = "Nr", FillWeight = 40 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Klant", HeaderText = "Klant", DataPropertyName = "Klant", FillWeight = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Auto", HeaderText = "Auto", DataPropertyName = "Auto", FillWeight = 110 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Periode", HeaderText = "Periode", DataPropertyName = "Periode", FillWeight = 100 });
            var sc = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 70 };
            grid.Columns.Add(sc); statusCol = sc.Index;
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Factuur", HeaderText = "Factuur", DataPropertyName = "Factuur", FillWeight = 50, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });

            grid.CellFormatting += grid_CellFormatting;
            // Double-click a reservation to pop up just its invoice.
            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                object v = grid.Rows[e.RowIndex].Cells["Rid"].Value;
                if (v != null && v != DBNull.Value) openInvoice(Convert.ToInt32(v));
            };
            card.Controls.Add(grid);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(toolbar, 0, 1);
            layout.Controls.Add(card, 0, 2);
        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != statusCol) return;
            string code = Convert.ToString(grid.Rows[e.RowIndex].Cells["StCode"].Value);
            e.CellStyle.ForeColor = statusColor(code);
            e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }

        private static Color statusColor(string code)
        {
            switch ((code ?? "").ToLowerInvariant())
            {
                case "completed": return Color.FromArgb(22, 101, 52);
                case "cancelled": return Color.FromArgb(153, 27, 27);
                case "active":
                case "ongoing": return Color.FromArgb(30, 64, 175);
                default: return Color.FromArgb(146, 64, 14); // reserved
            }
        }

        private void loadRentals()
        {
            try
            {
                all = DBHelper.getdataTable(
                    "SELECT r.ID AS Rid, u.ID AS Uid, CONCAT(u.Name,' ',u.LastName) AS Klant, u.Email, " +
                    "c.Brand, c.Model, c.License_Plate, r.Date_Started, r.Date_Planned_Ending, r.Status, " +
                    "(SELECT COUNT(*) FROM invoice i WHERE i.Rental_ID = r.ID) AS HasInvoice " +
                    "FROM rental r JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID " +
                    "ORDER BY r.Date_Started DESC");
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de reserveringen niet laden:\n" + ex.Message, "Reserveringen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyFilter()
        {
            if (all == null) return;
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();

            var d = new DataTable();
            d.Columns.Add("Rid", typeof(int));
            d.Columns.Add("StCode");
            d.Columns.Add("Nr");
            d.Columns.Add("Klant");
            d.Columns.Add("Auto");
            d.Columns.Add("Periode");
            d.Columns.Add("Status");
            d.Columns.Add("Factuur");

            foreach (DataRow r in all.Rows)
            {
                string auto = r["Brand"] + " " + r["Model"] + " (" + r["License_Plate"] + ")";
                string klant = r["Klant"].ToString();
                if (search != "")
                {
                    string hay = ("#" + r["Rid"] + " " + klant + " " + r["License_Plate"]).ToLowerInvariant();
                    if (!hay.Contains(search)) continue;
                }
                string periode = Convert.ToDateTime(r["Date_Started"]).ToString("dd-MM-yyyy", NL) + " - " +
                                 Convert.ToDateTime(r["Date_Planned_Ending"]).ToString("dd-MM-yyyy", NL);
                string stCode = r["Status"].ToString();
                d.Rows.Add(Convert.ToInt32(r["Rid"]), stCode, "#" + r["Rid"], klant, auto, periode,
                    StatusText.Rental(stCode), Convert.ToInt32(r["HasInvoice"]) > 0 ? "✓" : "–");
            }

            grid.DataSource = d;
            lblCount.Text = d.Rows.Count + (d.Rows.Count == 1 ? " reservering" : " reserveringen");
            trySelectPending();
        }

        // Called by AdminNav when the user double-clicks a reservation in the auto-view.
        public void ApplyNavArg(object arg)
        {
            if (!(arg is int)) return;
            pendingRid = (int)arg;
            if (all == null) loadRentals();
            // Filter the list down to just this reservation (by its #number), then
            // select it. Clearing the search box shows everything again.
            string token = "#" + pendingRid;
            if (txtSearch.Text != token) txtSearch.Text = token; // triggers applyFilter -> trySelectPending
            else applyFilter();
        }

        // Selects and scrolls to the pending reservation within the current view.
        private void trySelectPending()
        {
            if (pendingRid < 0) return;
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["Rid"].Value != null && Convert.ToInt32(row.Cells["Rid"].Value) == pendingRid)
                {
                    pendingRid = -1;
                    grid.ClearSelection();
                    grid.CurrentCell = row.Cells["Klant"];
                    row.Selected = true;
                    try { grid.FirstDisplayedScrollingRowIndex = row.Index; } catch { }
                    return;
                }
            }
            pendingRid = -1;
        }

        // Opens the invoice popup for the selected reservation (toolbar button).
        private void showInvoice()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["Rid"].Value == null)
            {
                MessageBox.Show("Selecteer eerst een reservering.", "Factuur", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            openInvoice(Convert.ToInt32(grid.CurrentRow.Cells["Rid"].Value));
        }

        private void openInvoice(int rentalId)
        {
            bool goToInvoices = false;
            int invoiceId = -1;
            using (var dlg = new InvoiceDetailForm(rentalId))
            {
                PopupForm.show(FindForm(), dlg);
                goToInvoices = dlg.GoToInvoices;
                invoiceId = dlg.GoToInvoiceId;
            }
            // "Factuur maken": go to the Facturen page (selecting the invoice if any).
            if (goToInvoices) AdminNav.Open(PermissionHelper.Invoices, invoiceId > 0 ? (object)invoiceId : null);
        }

        private void cancelSelected()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["Rid"].Value == null)
            {
                MessageBox.Show("Selecteer eerst een reservering.", "Annuleren", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int rid = Convert.ToInt32(grid.CurrentRow.Cells["Rid"].Value);
            string stCode = Convert.ToString(grid.CurrentRow.Cells["StCode"].Value).ToLowerInvariant();
            if (stCode == "cancelled" || stCode == "completed")
            {
                MessageBox.Show("Alleen een lopende of toekomstige reservering kan geannuleerd worden.", "Annuleren", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Fetch the details we need for the notification.
            DataRow src = null;
            foreach (DataRow r in all.Rows) { if (Convert.ToInt32(r["Rid"]) == rid) { src = r; break; } }
            if (src == null) return;

            int uid = Convert.ToInt32(src["Uid"]);
            string name = src["Klant"].ToString();
            string email = src["Email"].ToString();
            string carText = src["Brand"] + " " + src["Model"] + " (" + src["License_Plate"] + ")";
            string periode = Convert.ToDateTime(src["Date_Started"]).ToString("dd-MM-yyyy", NL) + " t/m " +
                             Convert.ToDateTime(src["Date_Planned_Ending"]).ToString("dd-MM-yyyy", NL);

            string reason;
            using (var dlg = new CancelRentalForm(name + " - " + carText + " (" + periode + ")"))
            {
                if (PopupForm.show(FindForm(), dlg) != DialogResult.OK) return;
                reason = dlg.Reason;
            }

            // Was the invoice for this reservation already paid? Then the customer
            // gets a refund instead of the invoice simply being dropped.
            bool invoicePaid = false;
            try
            {
                object paid = DBHelper.getScalar(
                    "SELECT COUNT(*) FROM invoice WHERE Rental_ID = " + rid + " AND LOWER(Status) = 'paid'");
                invoicePaid = Convert.ToInt32(paid) > 0;
            }
            catch { }

            string invoiceLine = invoicePaid
                ? "De factuur voor deze reservering was al betaald. Het betaalde bedrag wordt uiteraard volledig aan je terugbetaald."
                : "Een eventueel aangemaakte factuur komt hiermee te vervallen.";

            // Identical body for the in-app message and the e-mail.
            string body =
                "Beste " + name + ",\r\n\r\n" +
                "Je reservering #" + rid + " voor " + carText + " (" + periode + ") is geannuleerd.\r\n\r\n" +
                "Reden: " + reason + "\r\n\r\n" +
                invoiceLine + " Onze excuses voor het ongemak.\r\n\r\n" +
                "Met vriendelijke groet,\r\nRent A Car";

            try
            {
                var p = new Dictionary<string, object> { { "@rid", rid }, { "@reason", reason } };
                DBHelper.RunSql("UPDATE rental SET Status = 'cancelled', Cancel_Reason = @reason WHERE ID = @rid", p);
                DBHelper.RunSql("DELETE FROM invoice WHERE Rental_ID = " + rid);

                var pm = new Dictionary<string, object>
                {
                    { "@cid", uid }, { "@sid", DBHelper.loggedinuserid }, { "@sname", staffName() }, { "@body", body },
                };
                DBHelper.RunSql(
                    "INSERT INTO message (Customer_ID, From_Staff, Sender_ID, Sender_Name, Body, Read_By_Staff) " +
                    "VALUES (@cid, 1, @sid, @sname, @body, 1)", pm);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de reservering niet annuleren:\n" + ex.Message, "Annuleren", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool emailed = false;
            string emailNote = "";
            if (!string.IsNullOrWhiteSpace(email) && EmailHelper.IsConfigured())
            {
                try
                {
                    EmailHelper.Send(email, name, "Reservering geannuleerd - Rent A Car", body, null, null);
                    emailed = true;
                }
                catch (Exception ex) { emailNote = "\r\n(E-mail niet verstuurd: " + ex.Message + ")"; }
            }
            else if (!EmailHelper.IsConfigured())
            {
                emailNote = "\r\n(E-mail niet verstuurd: SMTP niet ingesteld.)";
            }

            LogHelper.Write("Reservering geannuleerd", "#" + rid + " " + carText + " - " + reason);
            loadRentals();
            MessageBox.Show(
                "De reservering is geannuleerd en de klant is per bericht" + (emailed ? " en e-mail" : "") + " op de hoogte gebracht. " +
                "Een eventuele factuur is verwijderd." + emailNote,
                "Annuleren", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string staffName()
        {
            try
            {
                DataTable t = DBHelper.getdataTable("SELECT Name, LastName FROM user WHERE ID = " + DBHelper.loggedinuserid);
                if (t.Rows.Count > 0) return (t.Rows[0]["Name"] + " " + t.Rows[0]["LastName"]).Trim();
            }
            catch { }
            return "Medewerker";
        }
    }
}
