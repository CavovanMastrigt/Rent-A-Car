using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Customer's chat with support. Any staff member can reply (shared inbox);
    // the customer just sends to "Rent A Car". Opened from the customer home page.
    public class CustomerMessagesForm : PopupForm
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Mine = Color.FromArgb(37, 99, 235);
        private static readonly Color Theirs = Color.FromArgb(241, 243, 245);

        private FlowLayoutPanel thread;
        private TextBox txtInput;

        public CustomerMessagesForm()
        {
            ClientSize = new Size(500, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Berichten";
            buildUi();
            loadThread();
        }

        private void buildUi()
        {
            var title = new Label { Text = "Berichten", Dock = DockStyle.Top, Height = 48, Font = new Font("Segoe UI", 15F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var sub = new Label { Text = "Stel je vraag; een medewerker reageert zo snel mogelijk.", Dock = DockStyle.Top, Height = 24, ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter };
            var btnClose = new Button { Text = "✕", Location = new Point(464, 6), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            thread = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.White, Padding = new Padding(10, 8, 10, 8) };

            var inputBar = new Panel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(10, 8, 10, 10), BackColor = Color.White };
            txtInput = new TextBox { Dock = DockStyle.Fill, Multiline = true, Font = new Font("Segoe UI", 10F) };
            var btnSend = new Button { Text = "Verzenden", Dock = DockStyle.Right, Width = 110, FlatStyle = FlatStyle.Flat, BackColor = Mine, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (s, e) => send();
            var spacer = new Panel { Dock = DockStyle.Right, Width = 8 };
            inputBar.Controls.Add(txtInput);
            inputBar.Controls.Add(spacer);
            inputBar.Controls.Add(btnSend);

            Controls.Add(thread);
            Controls.Add(inputBar);
            Controls.Add(sub);
            Controls.Add(title);
            CancelButton = btnClose;
            Controls.Add(btnClose);
            btnClose.BringToFront();
        }

        private void loadThread()
        {
            thread.Controls.Clear();
            try
            {
                int me = DBHelper.loggedinuserid;
                DataTable t = DBHelper.getdataTable(
                    "SELECT From_Staff, Sender_Name, Body, Created_At FROM message WHERE Customer_ID = " + me + " ORDER BY ID");
                if (t.Rows.Count == 0)
                {
                    thread.Controls.Add(new Label { Text = "Nog geen berichten. Stuur gerust je vraag!", AutoSize = true, ForeColor = Muted, Margin = new Padding(6, 20, 0, 0) });
                }
                foreach (DataRow r in t.Rows)
                {
                    bool fromStaff = Convert.ToInt32(r["From_Staff"]) == 1;
                    addBubble(!fromStaff, fromStaff ? r["Sender_Name"].ToString() : "Jij",
                        r["Body"].ToString(), Convert.ToDateTime(r["Created_At"]));
                }
                // Mark staff replies as read by the customer.
                DBHelper.RunSql("UPDATE message SET Read_By_Customer = 1 WHERE Customer_ID = " + me + " AND From_Staff = 1 AND Read_By_Customer = 0");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de berichten niet laden:\n" + ex.Message, "Berichten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            scrollToEnd();
        }

        // mine = the customer's own message (right, blue); otherwise a staff reply (left, grey).
        private void addBubble(bool mine, string who, string body, DateTime when)
        {
            var meta = new Label
            {
                Text = (mine ? "Jij" : who) + "  ·  " + when.ToString("dd-MM HH:mm"),
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8F),
                Margin = mine ? new Padding(270, 8, 6, 0) : new Padding(8, 8, 0, 0)
            };
            var bubble = new Label
            {
                Text = body,
                AutoSize = true,
                MaximumSize = new Size(320, 0),
                BackColor = mine ? Mine : Theirs,
                ForeColor = mine ? Color.White : Ink,
                Font = new Font("Segoe UI", 10F),
                Padding = new Padding(10, 7, 10, 7),
                Margin = mine ? new Padding(120, 2, 6, 2) : new Padding(6, 2, 120, 2)
            };
            thread.Controls.Add(meta);
            thread.Controls.Add(bubble);
        }

        private void scrollToEnd()
        {
            if (thread.Controls.Count > 0)
            {
                thread.ScrollControlIntoView(thread.Controls[thread.Controls.Count - 1]);
            }
        }

        private void send()
        {
            string body = txtInput.Text.Trim();
            if (body == "") return;
            try
            {
                var p = new Dictionary<string, object>
                {
                    { "@cid", DBHelper.loggedinuserid },
                    { "@name", myName() },
                    { "@body", body },
                };
                DBHelper.RunSql(
                    "INSERT INTO message (Customer_ID, From_Staff, Sender_ID, Sender_Name, Body, Read_By_Customer) " +
                    "VALUES (@cid, 0, @cid, @name, @body, 1)", p);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het bericht niet versturen:\n" + ex.Message, "Berichten", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            txtInput.Clear();
            loadThread();
        }

        private string myName()
        {
            try
            {
                DataTable t = DBHelper.getdataTable("SELECT Name, LastName FROM user WHERE ID = " + DBHelper.loggedinuserid);
                if (t.Rows.Count > 0) return (t.Rows[0]["Name"] + " " + t.Rows[0]["LastName"]).Trim();
            }
            catch { }
            return "Klant";
        }
    }
}
