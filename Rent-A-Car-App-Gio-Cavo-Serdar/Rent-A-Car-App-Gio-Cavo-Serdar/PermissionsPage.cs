using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story - admin controls what an employee may do, by granting or revoking
    // rights per employee. Admin-only page.
    public class PermissionsPage : UserControl, INavTarget
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private DataGridView grid;
        private TextBox txtSearch;
        private DataTable allEmployees;
        private Label lblTitle, lblHint;
        private readonly Dictionary<string, CheckBox> boxes = new Dictionary<string, CheckBox>();
        private Button btnSave;
        private int selectedUserId = -1;
        private int pendingUserId = -1; // employee to auto-select after a jump from Medewerkers

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
            split.Controls.Add(buildEmployeeList(), 0, 0);
            split.Controls.Add(buildDetail(), 1, 0);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(split, 0, 1);
        }

        // The employee list with a search box above it.
        private Control buildEmployeeList()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 0, 12, 0) };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var searchPanel = new Panel { Dock = DockStyle.Fill };
            var lbl = new Label { Text = "Zoek medewerker", Location = new Point(0, 0), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 8.5F) };
            txtSearch = new TextBox { Location = new Point(0, 18), Width = 260, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyEmployeeFilter();
            searchPanel.Controls.Add(lbl);
            searchPanel.Controls.Add(txtSearch);

            panel.Controls.Add(searchPanel, 0, 0);
            panel.Controls.Add(buildGrid(), 0, 1);
            return panel;
        }

        private DataGridView buildGrid()
        {
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
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

            var inner = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));  // title
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));  // hint
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // scrollable rights
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));  // save
            card.Controls.Add(inner);

            lblTitle = new Label { Dock = DockStyle.Fill, Text = "Selecteer een medewerker", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Ink };
            lblHint = new Label { Dock = DockStyle.Fill, Text = "Vink aan wat deze medewerker mag zien en gebruiken.", ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            // Two columns so every right fits on one screen without scrolling.
            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            content.Controls.Add(buildColumn(new[] { PermissionHelper.Groups[0], PermissionHelper.Groups[1] }), 0, 0);
            content.Controls.Add(buildColumn(new[] { PermissionHelper.Groups[2], PermissionHelper.Groups[3] }), 1, 0);

            btnSave = new Button
            {
                Text = "Opslaan",
                Size = new Size(170, 38),
                Margin = new Padding(2, 10, 0, 0),
                Anchor = AnchorStyles.Left,
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => save();

            inner.Controls.Add(lblTitle, 0, 0);
            inner.Controls.Add(lblHint, 0, 1);
            inner.Controls.Add(content, 0, 2);
            inner.Controls.Add(btnSave, 0, 3);
            return card;
        }

        // One vertical column of section headers and their checkboxes.
        private FlowLayoutPanel buildColumn(PermissionHelper.Group[] groups)
        {
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            foreach (PermissionHelper.Group g in groups)
            {
                flow.Controls.Add(new Label
                {
                    Text = g.Title.ToUpper(),
                    AutoSize = true,
                    Margin = new Padding(2, 12, 0, 4),
                    ForeColor = Muted,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold)
                });
                foreach (string key in g.Keys)
                {
                    var box = new CheckBox
                    {
                        Text = PermissionHelper.Label(key),
                        AutoSize = true,
                        Margin = new Padding(4, 2, 0, 2),
                        Font = new Font("Segoe UI", 10F),
                        Enabled = false
                    };
                    boxes[key] = box;
                    flow.Controls.Add(box);
                }
            }
            return flow;
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
                allEmployees = display;
                applyEmployeeFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de medewerkers niet laden:\n" + ex.Message, "Rechten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Filters the employee list on name / e-mail and keeps the selection logic.
        private void applyEmployeeFilter()
        {
            if (allEmployees == null) return;
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();

            var view = allEmployees.Clone();
            foreach (DataRow r in allEmployees.Rows)
            {
                if (search != "")
                {
                    string hay = (r["Naam"] + " " + r["Email"]).ToString().ToLowerInvariant();
                    if (!hay.Contains(search)) continue;
                }
                view.ImportRow(r);
            }
            grid.DataSource = view;
            if (!trySelectPending()) loadSelected();
        }

        // Called by AdminNav when the admin jumps here from the Medewerkers page.
        public void ApplyNavArg(object arg)
        {
            if (!(arg is int)) return;
            pendingUserId = (int)arg;
            if (allEmployees == null) loadEmployees();
            // Filter the list down to just this employee (by e-mail), then select them.
            // Clearing the search box shows the whole list again.
            string token = emailFor(pendingUserId);
            if (string.IsNullOrEmpty(token)) { trySelectPending(); return; }
            if (txtSearch.Text != token) txtSearch.Text = token; // triggers applyEmployeeFilter -> trySelectPending
            else applyEmployeeFilter();
        }

        private string emailFor(int uid)
        {
            if (allEmployees == null) return null;
            foreach (DataRow r in allEmployees.Rows)
                if (Convert.ToInt32(r["ID"]) == uid) return r["Email"].ToString();
            return null;
        }

        // Selects the pending employee's row (which loads their rights). Returns
        // true when it found and selected the row.
        private bool trySelectPending()
        {
            if (pendingUserId < 0) return false;
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["ID"].Value != null && Convert.ToInt32(row.Cells["ID"].Value) == pendingUserId)
                {
                    pendingUserId = -1;
                    grid.ClearSelection();
                    grid.CurrentCell = row.Cells["Naam"];
                    row.Selected = true;
                    loadSelected();
                    return true;
                }
            }
            pendingUserId = -1;
            return false;
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
