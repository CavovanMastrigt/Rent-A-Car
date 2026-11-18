using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story - admin controls what an employee may do, by granting or revoking
    // rights per employee. Admin-only page.
    public class PermissionsPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private DataGridView grid;
        private Label lblTitle, lblHint;
        private readonly Dictionary<string, CheckBox> boxes = new Dictionary<string, CheckBox>();
        private Button btnSave;
        private int selectedUserId = -1;

        public PermissionsPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadEmployees();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(layout);

            var header = new Label { Text = "Rechten", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 6, 0, 0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            split.Controls.Add(buildGrid(), 0, 0);
            split.Controls.Add(buildDetail(), 1, 0);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(split, 0, 1);
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

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "ID", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Naam", HeaderText = "Medewerker", DataPropertyName = "Naam", FillWeight = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "E-mail", DataPropertyName = "Email", FillWeight = 130 });

            grid.SelectionChanged += (s, e) => loadSelected();
            return grid;
        }

        private Panel buildDetail()
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20) };
            card.Paint += (s, e) => { using (var pen = new Pen(Border)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };

            lblTitle = new Label { Location = new Point(20, 18), Size = new Size(430, 26), Text = "Selecteer een medewerker", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Ink };
            lblHint = new Label { Location = new Point(20, 48), Size = new Size(430, 20), Text = "Vink aan wat deze medewerker mag gebruiken.", ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            int y = 82;
            foreach (string key in PermissionHelper.Keys)
            {
                var box = new CheckBox
                {
                    Text = PermissionHelper.Label(key),
                    Location = new Point(22, y),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F),
                    Enabled = false
                };
                boxes[key] = box;
                card.Controls.Add(box);
                y += 34;
            }

            btnSave = new Button
            {
                Text = "Opslaan",
                Location = new Point(22, y + 10),
                Size = new Size(170, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => save();

            card.Controls.Add(btnSave);
            card.Controls.Add(lblHint);
            card.Controls.Add(lblTitle);
            return card;
        }

        private void loadEmployees()
        {
            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT ID, CONCAT(Name,' ',LastName) AS Naam, Email FROM user WHERE RID = " +
                    AuthHelper.RoleEmployee + " ORDER BY Name, LastName");

                var display = new DataTable();
                display.Columns.Add("ID", typeof(int));
                display.Columns.Add("Naam");
                display.Columns.Add("Email");
                foreach (DataRow r in t.Rows)
                {
                    display.Rows.Add(Convert.ToInt32(r["ID"]), r["Naam"].ToString(), r["Email"].ToString());
                }
                grid.DataSource = display;
                loadSelected();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de medewerkers niet laden:\n" + ex.Message, "Rechten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadSelected()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["ID"].Value == null)
            {
                selectedUserId = -1;
                lblTitle.Text = "Selecteer een medewerker";
                foreach (var b in boxes.Values) { b.Checked = false; b.Enabled = false; }
                btnSave.Enabled = false;
                return;
            }

            selectedUserId = Convert.ToInt32(grid.CurrentRow.Cells["ID"].Value);
            lblTitle.Text = "Rechten van " + grid.CurrentRow.Cells["Naam"].Value;

            HashSet<string> perms = PermissionHelper.GetForUser(selectedUserId, AuthHelper.RoleEmployee);
            foreach (var pair in boxes)
            {
                pair.Value.Enabled = true;
                pair.Value.Checked = perms.Contains(pair.Key);
            }
            btnSave.Enabled = true;
        }

        private void save()
        {
            if (selectedUserId < 0) return;
            var granted = new List<string>();
            foreach (var pair in boxes)
            {
                if (pair.Value.Checked) granted.Add(pair.Key);
            }
            try
            {
                PermissionHelper.SetForUser(selectedUserId, granted);
                string name = grid.CurrentRow.Cells["Naam"].Value.ToString();
                LogHelper.Write("Rechten gewijzigd", name + ": " + (granted.Count == 0 ? "geen" : string.Join(", ", granted)));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de rechten niet opslaan:\n" + ex.Message, "Rechten", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("De rechten zijn opgeslagen. Ze gelden de volgende keer dat de medewerker inlogt.",
                "Rechten", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
