using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Admin-only customer management: CRUD, status changes and uploading an ID
    // on behalf of a customer. Matches the look of the Auto's / verification pages.
    public class CustomersPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private static readonly string[] StatusLabels = { "Actief", "Gepauzeerd", "Uitgeschakeld", "Beëindigd" };
        private static readonly Dictionary<string, string> LabelToCode = new Dictionary<string, string>
        {
            { "Actief", "active" }, { "Gepauzeerd", "paused" }, { "Uitgeschakeld", "disabled" }, { "Beëindigd", "terminated" },
        };

        private TextBox txtSearch;
        private ComboBox cboStatus;
        private DataGridView grid;
        private Label lblCount;
        private int statusCol = -1, verifCol = -1;
        private DataTable all;

        public CustomersPage()
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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(layout);

            var header = new Label { Text = "Klanten", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(buildToolbar(), 0, 1);
            layout.Controls.Add(buildCard(), 0, 2);
        }

        private Panel buildToolbar()
        {
            var bar = new Panel { Dock = DockStyle.Fill };

            txtSearch = new TextBox { Location = new Point(2, 18), Width = 260, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();
            var lblSearch = new Label { Text = "Zoek op naam, e-mail of stad", Location = new Point(2, 0), AutoSize = true, ForeColor = Muted };

            cboStatus = new ComboBox { Location = new Point(280, 18), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboStatus.Items.Add("Alle statussen");
            cboStatus.Items.AddRange(StatusLabels);
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += (s, e) => applyFilter();
            var lblFilter = new Label { Text = "Status", Location = new Point(280, 0), AutoSize = true, ForeColor = Muted };

            lblCount = new Label { Location = new Point(478, 24), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            var actions = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 13, 0, 0) };
            var btnAdd = actionButton("Nieuwe klant", Accent, Color.White);
            btnAdd.Click += (s, e) => openEditor(0);
            var btnEdit = actionButton("Bewerken", Color.White, Color.FromArgb(31, 41, 55));
            btnEdit.FlatAppearance.BorderColor = Border; btnEdit.FlatAppearance.BorderSize = 1;
            btnEdit.Click += (s, e) => editSelected();
            var btnId = actionButton("ID uploaden", Color.White, Color.FromArgb(31, 41, 55));
            btnId.FlatAppearance.BorderColor = Border; btnId.FlatAppearance.BorderSize = 1;
            btnId.Click += (s, e) => uploadId();
            var btnDelete = actionButton("Verwijderen", Color.White, Color.FromArgb(185, 28, 28));
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(248, 180, 180); btnDelete.FlatAppearance.BorderSize = 1;
            btnDelete.Click += (s, e) => deleteSelected();
            actions.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnId, btnDelete });

            bar.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilter, cboStatus, lblCount, actions });
            return bar;
        }

        private Button actionButton(string text, Color back, Color fore)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(120, 34),
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
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VerifCode", DataPropertyName = "VerifCode", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Naam", HeaderText = "Naam", DataPropertyName = "Naam", FillWeight = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "E-mail", DataPropertyName = "Email", FillWeight = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Telefoon", HeaderText = "Telefoon", DataPropertyName = "Telefoon", FillWeight = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stad", HeaderText = "Woonplaats", DataPropertyName = "Stad", FillWeight = 80 });
            var sc = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 80 };
            grid.Columns.Add(sc); statusCol = sc.Index;
            var vc = new DataGridViewTextBoxColumn { Name = "Verificatie", HeaderText = "Verificatie", DataPropertyName = "Verificatie", FillWeight = 90 };
            grid.Columns.Add(vc); verifCol = vc.Index;

            grid.CellFormatting += grid_CellFormatting;
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) openEditor(Convert.ToInt32(grid.Rows[e.RowIndex].Cells["ID"].Value)); };

            card.Controls.Add(grid);
            return card;
        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == statusCol)
            {
                string code = Convert.ToString(grid.Rows[e.RowIndex].Cells["StCode"].Value);
                e.CellStyle.ForeColor = customerStatusColor(code);
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
            else if (e.ColumnIndex == verifCol)
            {
                string code = Convert.ToString(grid.Rows[e.RowIndex].Cells["VerifCode"].Value);
                e.CellStyle.ForeColor = VerificationHelper.StatusForeColor(code);
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
        }

        private static Color customerStatusColor(string code)
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

        private void loadCustomers()
        {
            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT u.ID, CONCAT(u.Name,' ',u.LastName) AS Naam, u.Email, u.Phone_Number, u.City, u.User_Status, " +
                    "COALESCE(i.Identity_Status,'None') AS Verif " +
                    "FROM user u LEFT JOIN identity i ON i.UserID = u.ID " +
                    "WHERE u.RID = " + AuthHelper.RoleCustomer + " ORDER BY u.Name, u.LastName");

                all = new DataTable();
                all.Columns.Add("ID", typeof(int));
                all.Columns.Add("Naam");
                all.Columns.Add("Email");
                all.Columns.Add("Telefoon");
                all.Columns.Add("Stad");
                all.Columns.Add("StCode");
                all.Columns.Add("Status");
                all.Columns.Add("VerifCode");
                all.Columns.Add("Verificatie");
                foreach (DataRow r in t.Rows)
                {
                    string stCode = r["User_Status"].ToString();
                    string verif = r["Verif"].ToString();
                    all.Rows.Add(
                        Convert.ToInt32(r["ID"]), r["Naam"].ToString(), r["Email"].ToString(),
                        r["Phone_Number"].ToString(), r["City"].ToString(),
                        stCode, StatusText.Customer(stCode),
                        verif, VerificationHelper.StatusLabel(verif));
                }
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de klanten niet laden:\n" + ex.Message, "Klanten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyFilter()
        {
            if (all == null) return;
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            string wanted = cboStatus.SelectedIndex > 0 ? LabelToCode[cboStatus.SelectedItem.ToString()] : null;

            var view = all.Clone();
            foreach (DataRow r in all.Rows)
            {
                if (wanted != null && r["StCode"].ToString() != wanted) continue;
                if (search != "")
                {
                    string hay = (r["Naam"] + " " + r["Email"] + " " + r["Stad"]).ToString().ToLowerInvariant();
                    if (!hay.Contains(search)) continue;
                }
                view.ImportRow(r);
            }
            grid.DataSource = view;
            lblCount.Text = view.Rows.Count + (view.Rows.Count == 1 ? " klant" : " klanten");
        }

        private int selectedId()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["ID"].Value == null) return -1;
            return Convert.ToInt32(grid.CurrentRow.Cells["ID"].Value);
        }

        private void openEditor(int id)
        {
            using (var dlg = new CustomerEditForm(id))
            {
                if (PopupForm.show(FindForm(), dlg) == DialogResult.OK) loadCustomers();
            }
        }

        private void editSelected()
        {
            int id = selectedId();
            if (id < 0) { MessageBox.Show("Selecteer eerst een klant.", "Bewerken", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            openEditor(id);
        }

        private void uploadId()
        {
            int id = selectedId();
            if (id < 0) { MessageBox.Show("Selecteer eerst een klant.", "ID uploaden", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string name = grid.CurrentRow.Cells["Naam"].Value.ToString();
            using (var dlg = new CustomerIdUploadForm(id, name))
            {
                if (PopupForm.show(FindForm(), dlg) == DialogResult.OK) loadCustomers();
            }
        }

        private void deleteSelected()
        {
            int id = selectedId();
            if (id < 0) { MessageBox.Show("Selecteer eerst een klant.", "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string name = grid.CurrentRow.Cells["Naam"].Value.ToString();

            try
            {
                object cnt = DBHelper.getScalar("SELECT COUNT(*) FROM rental WHERE UserID = " + id);
                if (Convert.ToInt32(cnt) > 0)
                {
                    MessageBox.Show(
                        "Deze klant heeft reserveringen en kan niet verwijderd worden.\n" +
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

            if (MessageBox.Show("Weet je het zeker? Klant " + name + " wordt definitief verwijderd.",
                "Verwijderen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                DBHelper.RunSql("DELETE FROM identity WHERE UserID = " + id);
                DBHelper.RunSql("DELETE FROM user WHERE ID = " + id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de klant niet verwijderen:\n" + ex.Message, "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            loadCustomers();
        }
    }
}
