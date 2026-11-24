using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Admin-only activity log: who did what and when (verifications, deletions,
    // rights changes, invoices, ...). Read-only.
    public class LogsPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private TextBox txtSearch;
        private ComboBox cboAction, cboUser;
        private DataGridView grid;
        private Label lblCount;
        private DataTable all;
        private bool loadingFilters;

        public LogsPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadLogs();
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

            var header = new Label { Text = "Logboek", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };

            var toolbar = new Panel { Dock = DockStyle.Fill };
            txtSearch = new TextBox { Location = new Point(2, 9), Width = 220, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();

            cboAction = new ComboBox { Location = new Point(232, 9), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboAction.SelectedIndexChanged += (s, e) => { if (!loadingFilters) applyFilter(); };
            cboUser = new ComboBox { Location = new Point(412, 9), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboUser.SelectedIndexChanged += (s, e) => { if (!loadingFilters) applyFilter(); };

            var btnRefresh = new Button { Text = "Vernieuwen", Location = new Point(594, 8), Size = new Size(110, 28), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRefresh.FlatAppearance.BorderColor = Border;
            btnRefresh.Click += (s, e) => loadLogs();
            lblCount = new Label { Location = new Point(716, 14), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9.5F) };
            toolbar.Controls.AddRange(new Control[] { txtSearch, cboAction, cboUser, btnRefresh, lblCount });

            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1), Margin = new Padding(0, 6, 0, 0) };
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

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tijd", HeaderText = "Tijdstip", DataPropertyName = "Tijd", FillWeight = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Wie", HeaderText = "Door", DataPropertyName = "Wie", FillWeight = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Actie", HeaderText = "Actie", DataPropertyName = "Actie", FillWeight = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Details", HeaderText = "Details", DataPropertyName = "Details", FillWeight = 160 });

            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) openLog(e.RowIndex); };

            card.Controls.Add(grid);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(toolbar, 0, 1);
            layout.Controls.Add(card, 0, 2);
        }

        private void loadLogs()
        {
            try
            {
                all = DBHelper.getdataTable(
                    "SELECT ID, Created_At, User_Name, Action, Details FROM activity_log ORDER BY ID DESC LIMIT 1000");
                populateFilters();
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het logboek niet laden:\n" + ex.Message, "Logboek", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Fills the Action and User dropdowns with the distinct values in the log,
        // keeping the current choice if it is still present.
        private void populateFilters()
        {
            loadingFilters = true;
            try
            {
                string keepAction = cboAction.SelectedIndex > 0 ? cboAction.SelectedItem.ToString() : null;
                string keepUser = cboUser.SelectedIndex > 0 ? cboUser.SelectedItem.ToString() : null;

                var actions = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
                var users = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
                foreach (DataRow r in all.Rows)
                {
                    if (r["Action"] != DBNull.Value && r["Action"].ToString() != "") actions.Add(r["Action"].ToString());
                    if (r["User_Name"] != DBNull.Value && r["User_Name"].ToString() != "") users.Add(r["User_Name"].ToString());
                }

                cboAction.Items.Clear();
                cboAction.Items.Add("Alle acties");
                foreach (string a in actions) cboAction.Items.Add(a);
                cboAction.SelectedIndex = keepAction != null && cboAction.Items.Contains(keepAction) ? cboAction.Items.IndexOf(keepAction) : 0;

                cboUser.Items.Clear();
                cboUser.Items.Add("Alle gebruikers");
                foreach (string u in users) cboUser.Items.Add(u);
                cboUser.SelectedIndex = keepUser != null && cboUser.Items.Contains(keepUser) ? cboUser.Items.IndexOf(keepUser) : 0;
            }
            finally
            {
                loadingFilters = false;
            }
        }

        private void applyFilter()
        {
            if (all == null) return;
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            string wantAction = cboAction.SelectedIndex > 0 ? cboAction.SelectedItem.ToString() : null;
            string wantUser = cboUser.SelectedIndex > 0 ? cboUser.SelectedItem.ToString() : null;

            var display = new DataTable();
            display.Columns.Add("Id", typeof(int));
            display.Columns.Add("Tijd");
            display.Columns.Add("Wie");
            display.Columns.Add("Actie");
            display.Columns.Add("Details");

            foreach (DataRow r in all.Rows)
            {
                string who = r["User_Name"] == DBNull.Value ? "-" : r["User_Name"].ToString();
                string action = r["Action"].ToString();
                string details = r["Details"] == DBNull.Value ? "" : r["Details"].ToString();
                if (wantAction != null && action != wantAction) continue;
                if (wantUser != null && who != wantUser) continue;
                if (search != "")
                {
                    string hay = (who + " " + action + " " + details).ToLowerInvariant();
                    if (!hay.Contains(search)) continue;
                }
                string tijd = r["Created_At"] == DBNull.Value ? "" : Convert.ToDateTime(r["Created_At"]).ToString("dd-MM-yyyy HH:mm", NL);
                display.Rows.Add(Convert.ToInt32(r["ID"]), tijd, who, action, details);
            }

            grid.DataSource = display;
            lblCount.Text = display.Rows.Count + (display.Rows.Count == 1 ? " regel" : " regels");
        }

        private void openLog(int rowIndex)
        {
            DataGridViewRow row = grid.Rows[rowIndex];
            if (row.Cells["Id"].Value == null) return;
            int id = Convert.ToInt32(row.Cells["Id"].Value);
            using (var dlg = new LogDetailForm(
                id,
                Convert.ToString(row.Cells["Tijd"].Value),
                Convert.ToString(row.Cells["Wie"].Value),
                Convert.ToString(row.Cells["Actie"].Value),
                Convert.ToString(row.Cells["Details"].Value)))
            {
                if (PopupForm.show(FindForm(), dlg) == DialogResult.OK) loadLogs();
            }
        }
    }
}
