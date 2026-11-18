using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Shared inbox for staff (admin + employee): every customer conversation,
    // any staff member can reply, and you can start a message to a customer.
    public class MessagesPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly Color Theirs = Color.FromArgb(241, 243, 245);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private DataGridView grid;
        private FlowLayoutPanel thread;
        private TextBox txtInput;
        private Button btnSend;
        private Label lblThreadTitle;
        private int selectedCustomer = -1;

        public MessagesPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadConversations();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(layout);

            var header = new Label { Text = "Berichten", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 6, 0, 0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            split.Controls.Add(buildList(), 0, 0);
            split.Controls.Add(buildThread(), 1, 0);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(split, 0, 1);
        }

        private Panel buildList()
        {
            var p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };

            var btnNew = new Button
            {
                Text = "Nieuw bericht",
                Dock = DockStyle.Top,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.Click += (s, e) => newMessage();

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 0),
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
                RowTemplate = { Height = 34 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Ink;

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cid", DataPropertyName = "Cid", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unread", DataPropertyName = "Unread", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nieuw", HeaderText = "", DataPropertyName = "Nieuw", FillWeight = 12, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, ForeColor = Accent, Font = new Font("Segoe UI", 12F, FontStyle.Bold) } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Klant", HeaderText = "Klant", DataPropertyName = "Klant", FillWeight = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Laatste", HeaderText = "Laatste", DataPropertyName = "Laatste", FillWeight = 70 });

            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int unread = 0;
                int.TryParse(Convert.ToString(grid.Rows[e.RowIndex].Cells["Unread"].Value), out unread);
                if (unread > 0 && grid.Columns[e.ColumnIndex].Name == "Klant")
                {
                    e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }
            };
            grid.SelectionChanged += (s, e) => loadThread();

            p.Controls.Add(grid);
            p.Controls.Add(btnNew);
            return p;
        }

        private Panel buildThread()
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            card.Paint += (s, e) => { using (var pen = new Pen(Border)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };

            lblThreadTitle = new Label { Dock = DockStyle.Top, Height = 38, Text = "Selecteer een gesprek", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Ink, Padding = new Padding(12, 8, 0, 0) };

            thread = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.White, Padding = new Padding(10, 6, 10, 6) };

            var inputBar = new Panel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(10, 8, 10, 10), BackColor = Color.White };
            txtInput = new TextBox { Dock = DockStyle.Fill, Multiline = true, Font = new Font("Segoe UI", 10F), Enabled = false };
            btnSend = new Button { Text = "Verzenden", Dock = DockStyle.Right, Width = 110, FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (s, e) => reply();
            var spacer = new Panel { Dock = DockStyle.Right, Width = 8 };
            inputBar.Controls.Add(txtInput);
            inputBar.Controls.Add(spacer);
            inputBar.Controls.Add(btnSend);

            card.Controls.Add(thread);
            card.Controls.Add(inputBar);
            card.Controls.Add(lblThreadTitle);
            return card;
        }

        private void loadConversations()
        {
            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT m.Customer_ID AS Cid, CONCAT(u.Name,' ',u.LastName) AS Klant, MAX(m.Created_At) AS Laatste, " +
                    "SUM(CASE WHEN m.From_Staff = 0 AND m.Read_By_Staff = 0 THEN 1 ELSE 0 END) AS Unread " +
                    "FROM message m JOIN user u ON u.ID = m.Customer_ID GROUP BY m.Customer_ID, Klant ORDER BY Laatste DESC");

                var d = new DataTable();
                d.Columns.Add("Cid", typeof(int));
                d.Columns.Add("Unread", typeof(int));
                d.Columns.Add("Nieuw");
                d.Columns.Add("Klant");
                d.Columns.Add("Laatste");
                foreach (DataRow r in t.Rows)
                {
                    int unread = Convert.ToInt32(r["Unread"]);
                    string last = r["Laatste"] == DBNull.Value ? "" : Convert.ToDateTime(r["Laatste"]).ToString("dd-MM HH:mm", NL);
                    d.Rows.Add(Convert.ToInt32(r["Cid"]), unread, unread > 0 ? "●" : "", r["Klant"].ToString(), last);
                }
                int keep = selectedCustomer;
                grid.DataSource = d;
                if (keep > 0) reselect(keep); else loadThread();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de gesprekken niet laden:\n" + ex.Message, "Berichten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void reselect(int cid)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["Cid"].Value != null && Convert.ToInt32(row.Cells["Cid"].Value) == cid)
                {
                    row.Selected = true;
                    grid.CurrentCell = row.Cells["Klant"];
                    return;
                }
            }
            loadThread();
        }

        private void loadThread()
        {
            thread.Controls.Clear();
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["Cid"].Value == null)
            {
                selectedCustomer = -1;
                lblThreadTitle.Text = "Selecteer een gesprek";
                txtInput.Enabled = btnSend.Enabled = false;
                return;
            }

            selectedCustomer = Convert.ToInt32(grid.CurrentRow.Cells["Cid"].Value);
            lblThreadTitle.Text = Convert.ToString(grid.CurrentRow.Cells["Klant"].Value);
            txtInput.Enabled = btnSend.Enabled = true;

            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT From_Staff, Sender_Name, Body, Created_At FROM message WHERE Customer_ID = " + selectedCustomer + " ORDER BY ID");
                foreach (DataRow r in t.Rows)
                {
                    bool fromStaff = Convert.ToInt32(r["From_Staff"]) == 1;
                    addBubble(fromStaff, r["Sender_Name"] == DBNull.Value ? "" : r["Sender_Name"].ToString(),
                        r["Body"].ToString(), Convert.ToDateTime(r["Created_At"]));
                }
                // Mark the customer's messages as read by staff.
                DBHelper.RunSql("UPDATE message SET Read_By_Staff = 1 WHERE Customer_ID = " + selectedCustomer + " AND From_Staff = 0 AND Read_By_Staff = 0");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het gesprek niet laden:\n" + ex.Message, "Berichten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (thread.Controls.Count > 0) thread.ScrollControlIntoView(thread.Controls[thread.Controls.Count - 1]);
        }

        // mine = staff message (right, blue); otherwise the customer (left, grey).
        private void addBubble(bool mine, string who, string body, DateTime when)
        {
            var meta = new Label
            {
                Text = (mine ? (who == "" ? "Medewerker" : who) : who) + "  ·  " + when.ToString("dd-MM HH:mm"),
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8F),
                Margin = mine ? new Padding(300, 8, 6, 0) : new Padding(8, 8, 0, 0)
            };
            var bubble = new Label
            {
                Text = body,
                AutoSize = true,
                MaximumSize = new Size(360, 0),
                BackColor = mine ? Accent : Theirs,
                ForeColor = mine ? Color.White : Ink,
                Font = new Font("Segoe UI", 10F),
                Padding = new Padding(10, 7, 10, 7),
                Margin = mine ? new Padding(140, 2, 6, 2) : new Padding(6, 2, 140, 2)
            };
            thread.Controls.Add(meta);
            thread.Controls.Add(bubble);
        }

        private void reply()
        {
            if (selectedCustomer < 0) return;
            string body = txtInput.Text.Trim();
            if (body == "") return;
            sendStaffMessage(selectedCustomer, body);
            txtInput.Clear();
            loadConversations();
        }

        private void newMessage()
        {
            using (var dlg = new NewMessageForm())
            {
                if (PopupForm.show(FindForm(), dlg) == DialogResult.OK && dlg.CustomerId > 0)
                {
                    sendStaffMessage(dlg.CustomerId, dlg.Body);
                    selectedCustomer = dlg.CustomerId;
                    loadConversations();
                }
            }
        }

        private void sendStaffMessage(int customerId, string body)
        {
            try
            {
                var p = new Dictionary<string, object>
                {
                    { "@cid", customerId },
                    { "@sid", DBHelper.loggedinuserid },
                    { "@name", staffName() },
                    { "@body", body },
                };
                DBHelper.RunSql(
                    "INSERT INTO message (Customer_ID, From_Staff, Sender_ID, Sender_Name, Body, Read_By_Staff) " +
                    "VALUES (@cid, 1, @sid, @name, @body, 1)", p);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het bericht niet versturen:\n" + ex.Message, "Berichten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
