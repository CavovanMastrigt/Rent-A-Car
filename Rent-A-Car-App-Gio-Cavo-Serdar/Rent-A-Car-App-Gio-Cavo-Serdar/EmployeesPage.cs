using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Admin-only staff management: change roles and statuses of admins/employees
    // and create new staff accounts. Same look as the Klanten / Auto's pages.
    public class EmployeesPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private TextBox txtSearch;
        private DataGridView grid;
        private Label lblCount;
        private int roleCol = -1, statusCol = -1;
        private DataTable all;

        public EmployeesPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadStaff();
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

            var header = new Label { Text = "Medewerkers", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(buildToolbar(), 0, 1);
            layout.Controls.Add(buildCard(), 0, 2);
        }

        private Panel buildToolbar()
        {
            var bar = new Panel { Dock = DockStyle.Fill };

            txtSearch = new TextBox { Location = new Point(2, 18), Width = 280, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();
            var lblSearch = new Label { Text = "Zoek op naam of e-mail", Location = new Point(2, 0), AutoSize = true, ForeColor = Muted };

            lblCount = new Label { Location = new Point(300, 24), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            var actions = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 13, 0, 0) };
            var btnAdd = actionButton("Nieuwe medewerker", Accent, Color.White, 160);
            btnAdd.Click += (s, e) => openEditor(0);
            var btnEdit = actionButton("Bewerken", Color.White, Color.FromArgb(31, 41, 55), 120);
            btnEdit.FlatAppearance.BorderColor = Border; btnEdit.FlatAppearance.BorderSize = 1;
            btnEdit.Click += (s, e) => editSelected();
            var btnDelete = actionButton("Verwijderen", Color.White, Color.FromArgb(185, 28, 28), 120);
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(248, 180, 180); btnDelete.FlatAppearance.BorderSize = 1;
            btnDelete.Click += (s, e) => deleteSelected();
            actions.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            bar.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblCount, actions });
            return bar;
        }

        private Button actionButton(string text, Color back, Color fore, int width)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(width, 34),
                Margin = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
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
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(75, 85, 99);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 40;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(237, 239, 242);
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 251);

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "ID", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "StCode", DataPropertyName = "StCode", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Naam", HeaderText = "Naam", DataPropertyName = "Naam", FillWeight = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "E-mail", DataPropertyName = "Email", FillWeight = 130 });
            var rc = new DataGridViewTextBoxColumn { Name = "Rol", HeaderText = "Rol", DataPropertyName = "Rol", FillWeight = 80 };
            grid.Columns.Add(rc); roleCol = rc.Index;
            var scStatus = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 80 };
            grid.Columns.Add(scStatus); statusCol = scStatus.Index;

            grid.CellFormatting += grid_CellFormatting;
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) openEditor(Convert.ToInt32(grid.Rows[e.RowIndex].Cells["ID"].Value)); };

            card.Controls.Add(grid);
            return card;
        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == roleCol)
            {
                string role = Convert.ToString(grid.Rows[e.RowIndex].Cells["Rol"].Value);
                e.CellStyle.ForeColor = role == "Beheerder" ? Color.FromArgb(30, 64, 175) : Color.FromArgb(55, 65, 81);
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
            else if (e.ColumnIndex == statusCol)
            {
                string code = Convert.ToString(grid.Rows[e.RowIndex].Cells["StCode"].Value);
                e.CellStyle.ForeColor = statusColor(code);
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
        }

        private static Color statusColor(string code)
        {
            switch (code)
            {
                case "active": return Color.FromArgb(22, 101, 52);
                case "paused": return Color.FromArgb(146, 64, 14);
                case "disabled": return Color.FromArgb(75, 85, 99);
                case "terminated": return Color.FromArgb(153, 27, 27);
                default: return Color.FromArgb(75, 85, 99);
            }
        }

        private void loadStaff()
        {
            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT ID, CONCAT(Name,' ',LastName) AS Naam, Email, RID, User_Status " +
                    "FROM user WHERE RID IN (" + AuthHelper.RoleAdmin + "," + AuthHelper.RoleEmployee + ") " +
                    "ORDER BY RID, Name");

                all = new DataTable();
                all.Columns.Add("ID", typeof(int));
                all.Columns.Add("Naam");
                all.Columns.Add("Email");
                all.Columns.Add("Rol");
                all.Columns.Add("StCode");
                all.Columns.Add("Status");
                foreach (DataRow r in t.Rows)
                {
                    string role = Convert.ToInt32(r["RID"]) == AuthHelper.RoleAdmin ? "Beheerder" : "Medewerker";
                    string stCode = r["User_Status"].ToString();
                    all.Rows.Add(Convert.ToInt32(r["ID"]), r["Naam"].ToString(), r["Email"].ToString(),
                        role, stCode, StatusText.Customer(stCode));
                }
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de medewerkers niet laden:\n" + ex.Message, "Medewerkers", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyFilter()
        {
            if (all == null) return;
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();

            var view = all.Clone();
            foreach (DataRow r in all.Rows)
            {
                if (search != "")
                {
                    string hay = (r["Naam"] + " " + r["Email"]).ToString().ToLowerInvariant();
                    if (!hay.Contains(search)) continue;
                }
                view.ImportRow(r);
            }
            grid.DataSource = view;
            lblCount.Text = view.Rows.Count + (view.Rows.Count == 1 ? " medewerker" : " medewerkers");
        }

        private int selectedId()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["ID"].Value == null) return -1;
            return Convert.ToInt32(grid.CurrentRow.Cells["ID"].Value);
        }

        private void openEditor(int id)
        {
            using (var dlg = new EmployeeEditForm(id))
            {
                if (PopupForm.show(FindForm(), dlg) == DialogResult.OK) loadStaff();
            }
        }

        private void editSelected()
        {
            int id = selectedId();
            if (id < 0) { MessageBox.Show("Selecteer eerst een medewerker.", "Bewerken", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            openEditor(id);
        }

        private void deleteSelected()
        {
            int id = selectedId();
            if (id < 0) { MessageBox.Show("Selecteer eerst een medewerker.", "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (id == DBHelper.loggedinuserid)
            {
                MessageBox.Show("Je kunt je eigen account niet verwijderen.", "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string name = grid.CurrentRow.Cells["Naam"].Value.ToString();

            try
            {
                object cnt = DBHelper.getScalar("SELECT COUNT(*) FROM rental WHERE Created_By = " + id);
                if (Convert.ToInt32(cnt) > 0)
                {
                    MessageBox.Show(
                        "Deze medewerker heeft reserveringen aangemaakt en kan niet verwijderd worden.\n" +
                        "Zet de status op 'Beëindigd' om het account te sluiten.",
                        "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de reserveringen niet controleren:\n" + ex.Message, "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Weet je het zeker? Medewerker " + name + " wordt definitief verwijderd.",
                "Verwijderen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                DBHelper.RunSql("DELETE FROM identity WHERE UserID = " + id);
                DBHelper.RunSql("DELETE FROM user WHERE ID = " + id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de medewerker niet verwijderen:\n" + ex.Message, "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            loadStaff();
        }
    }
}
