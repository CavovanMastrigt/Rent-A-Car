using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story 8 - admin-only overview of customer ID verifications.
    // The admin searches/filters the list, sees each status by colour, and
    // double-clicks (or uses "Beoordelen") to review one customer in a popup.
    public class CustomerVerificationPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);

        private DataGridView grid;
        private TextBox txtSearch;
        private ComboBox cboStatus;
        private Label lblCount;
        private int statusColIndex = -1;

        private DataTable allRows;   // UserID, St (raw), Naam, Email, Status (label)

        public CustomerVerificationPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadCustomers();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(layout);

            var header = new Label
            {
                Text = "Klantverificatie",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Ink
            };

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(buildToolbar(), 0, 1);
            layout.Controls.Add(buildGrid(), 0, 2);
        }

        private Panel buildToolbar()
        {
            var bar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var lblSearch = new Label { Text = "Zoek op naam of e-mail", Location = new Point(0, 2), AutoSize = true, ForeColor = Muted };
            txtSearch = new TextBox { Location = new Point(0, 22), Width = 280, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();

            var lblFilter = new Label { Text = "Status", Location = new Point(300, 2), AutoSize = true, ForeColor = Muted };
            cboStatus = new ComboBox { Location = new Point(300, 22), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboStatus.Items.AddRange(new object[] { "Alle statussen", "In behandeling", "Geverifieerd", "Afgekeurd", "Niet geverifieerd" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += (s, e) => applyFilter();

            var btnReview = new Button
            {
                Text = "Beoordelen",
                Location = new Point(530, 21),
                Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReview.FlatAppearance.BorderSize = 0;
            btnReview.Click += (s, e) => openReview();

            lblCount = new Label { Location = new Point(700, 28), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            // Legend: a coloured chip per status so the colours are self-explaining.
            var lblLegend = new Label { Text = "Kleuren:", Location = new Point(0, 70), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            var legend = new FlowLayoutPanel { Location = new Point(62, 64), Size = new Size(640, 36), WrapContents = false, AutoSize = false };
            legend.Controls.Add(makeChip("In behandeling", VerificationHelper.StatusPending));
            legend.Controls.Add(makeChip("Geverifieerd", VerificationHelper.StatusVerified));
            legend.Controls.Add(makeChip("Afgekeurd", VerificationHelper.StatusFailed));
            legend.Controls.Add(makeChip("Niet geverifieerd", VerificationHelper.StatusNone));

            var hint = new Label { Text = "Dubbelklik op een klant om te beoordelen.", Location = new Point(0, 100), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9F, FontStyle.Italic) };

            bar.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilter, cboStatus, btnReview, lblCount, lblLegend, legend, hint });
            return bar;
        }

        private static Label makeChip(string text, string status)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 4, 10, 4),
                Padding = new Padding(10, 5, 10, 5),
                BackColor = VerificationHelper.StatusBackColor(status),
                ForeColor = VerificationHelper.StatusForeColor(status),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        private DataGridView buildGrid()
        {
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 0, 0),
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
                RowTemplate = { Height = 32 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Ink;

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserID", DataPropertyName = "UserID", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "St", DataPropertyName = "St", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Naam", HeaderText = "Naam", DataPropertyName = "Naam", FillWeight = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "E-mail", DataPropertyName = "Email", FillWeight = 130 });
            var statusCol = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 85 };
            grid.Columns.Add(statusCol);
            statusColIndex = statusCol.Index;

            grid.CellFormatting += grid_CellFormatting;
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) openReview(); };
            return grid;
        }

        // Colour the Status cell by the raw status in the hidden "St" column.
        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != statusColIndex) return;
            var raw = grid.Rows[e.RowIndex].Cells["St"].Value;
            if (raw == null) return;
            string st = raw.ToString();
            e.CellStyle.BackColor = VerificationHelper.StatusBackColor(st);
            e.CellStyle.ForeColor = VerificationHelper.StatusForeColor(st);
            e.CellStyle.SelectionBackColor = VerificationHelper.StatusBackColor(st);
            e.CellStyle.SelectionForeColor = VerificationHelper.StatusForeColor(st);
            e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void loadCustomers()
        {
            try
            {
                DataTable customers = DBHelper.getdataTable(
                    "SELECT u.ID AS UserID, CONCAT(u.Name, ' ', u.LastName) AS Naam, u.Email, " +
                    "COALESCE(i.Identity_Status, 'None') AS St " +
                    "FROM user u LEFT JOIN identity i ON i.UserID = u.ID " +
                    "WHERE u.RID = " + AuthHelper.RoleCustomer + " " +
                    "ORDER BY (COALESCE(i.Identity_Status,'None') = 'Pending') DESC, u.Name");

                allRows = new DataTable();
                allRows.Columns.Add("UserID", typeof(int));
                allRows.Columns.Add("St");
                allRows.Columns.Add("Naam");
                allRows.Columns.Add("Email");
                allRows.Columns.Add("Status");
                foreach (DataRow r in customers.Rows)
                {
                    string st = r["St"].ToString();
                    allRows.Rows.Add(
                        Convert.ToInt32(r["UserID"]),
                        st,
                        r["Naam"].ToString(),
                        r["Email"].ToString(),
                        VerificationHelper.StatusLabel(st));
                }
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de klanten niet laden:\n" + ex.Message, "Klantverificatie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyFilter()
        {
            if (allRows == null) return;

            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            string wanted = statusForSelection();

            var view = allRows.Clone();
            foreach (DataRow r in allRows.Rows)
            {
                if (wanted != null && r["St"].ToString() != wanted) continue;
                if (search != "")
                {
                    string hay = (r["Naam"].ToString() + " " + r["Email"].ToString()).ToLowerInvariant();
                    if (!hay.Contains(search)) continue;
                }
                view.ImportRow(r);
            }

            grid.DataSource = view;
            lblCount.Text = view.Rows.Count + (view.Rows.Count == 1 ? " klant" : " klanten");
        }

        // Maps the filter combo to a raw status, or null for "all".
        private string statusForSelection()
        {
            switch (cboStatus.SelectedIndex)
            {
                case 1: return VerificationHelper.StatusPending;
                case 2: return VerificationHelper.StatusVerified;
                case 3: return VerificationHelper.StatusFailed;
                case 4: return VerificationHelper.StatusNone;
                default: return null;
            }
        }

        private void openReview()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["UserID"].Value == null)
            {
                return;
            }
            int uid = Convert.ToInt32(grid.CurrentRow.Cells["UserID"].Value);
            string name = grid.CurrentRow.Cells["Naam"].Value.ToString();
            string email = grid.CurrentRow.Cells["Email"].Value.ToString();

            using (var dlg = new VerificationReviewForm(uid, name, email))
            {
                DialogResult res = PopupForm.show(FindForm(), dlg);
                if (res == DialogResult.OK)
                {
                    loadCustomers();
                }
            }
        }
    }
}
