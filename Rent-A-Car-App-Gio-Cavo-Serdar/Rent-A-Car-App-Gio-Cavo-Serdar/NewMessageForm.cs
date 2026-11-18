using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Staff starts a message to a specific customer (e.g. a warning or update).
    public class NewMessageForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private ComboBox cboCustomer;
        private TextBox txtBody;

        public int CustomerId { get; private set; } = -1;
        public string Body { get; private set; } = "";

        private class Item
        {
            public int Id;
            public string Name;
            public override string ToString() { return Name; }
        }

        public NewMessageForm()
        {
            ClientSize = new Size(440, 360);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Nieuw bericht";
            buildUi();
            loadCustomers();
        }

        private void buildUi()
        {
            var title = new Label { Text = "Nieuw bericht", Dock = DockStyle.Top, Height = 52, Font = new Font("Segoe UI", 16F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var btnClose = new Button { Text = "✕", Location = new Point(404, 6), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            Controls.Add(new Label { Text = "Aan welke klant?", Location = new Point(30, 70), AutoSize = true, ForeColor = Muted });
            cboCustomer = new ComboBox { Location = new Point(30, 90), Width = 380, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };

            Controls.Add(new Label { Text = "Bericht", Location = new Point(30, 130), AutoSize = true, ForeColor = Muted });
            txtBody = new TextBox { Location = new Point(30, 150), Width = 380, Height = 110, Multiline = true, Font = new Font("Segoe UI", 10F) };

            var btnSend = new Button { Text = "Verzenden", Location = new Point(220, 300), Size = new Size(190, 40), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (s, e) => send();
            var btnCancel = new Button { Text = "Annuleren", Location = new Point(30, 300), Size = new Size(150, 40), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, Cursor = Cursors.Hand };

            Controls.AddRange(new Control[] { title, btnClose, cboCustomer, txtBody, btnSend, btnCancel });
            title.BringToFront();
            CancelButton = btnCancel;
        }

        private void loadCustomers()
        {
            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT ID, CONCAT(Name,' ',LastName) AS Naam FROM user WHERE RID = " + AuthHelper.RoleCustomer + " ORDER BY Name, LastName");
                foreach (DataRow r in t.Rows)
                {
                    cboCustomer.Items.Add(new Item { Id = Convert.ToInt32(r["ID"]), Name = r["Naam"].ToString() });
                }
                if (cboCustomer.Items.Count > 0) cboCustomer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de klanten niet laden:\n" + ex.Message, "Nieuw bericht", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void send()
        {
            if (cboCustomer.SelectedItem == null)
            {
                MessageBox.Show("Kies een klant.", "Nieuw bericht", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtBody.Text.Trim() == "")
            {
                MessageBox.Show("Typ een bericht.", "Nieuw bericht", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            CustomerId = ((Item)cboCustomer.SelectedItem).Id;
            Body = txtBody.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
