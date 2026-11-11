using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Add or edit a customer (role 0) in a popup. Admin only.
    // Validates required fields, keeps the e-mail unique and can set the status.
    public class CustomerEditForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        // Status label shown in the UI <-> value stored in user.User_Status.
        private static readonly string[] StatusLabels = { "Actief", "Gepauzeerd", "Uitgeschakeld", "Beëindigd" };
        private static readonly Dictionary<string, string> LabelToCode = new Dictionary<string, string>
        {
            { "Actief", "active" }, { "Gepauzeerd", "paused" }, { "Uitgeschakeld", "disabled" }, { "Beëindigd", "terminated" },
        };
        private static readonly Dictionary<string, string> CodeToLabel = new Dictionary<string, string>
        {
            { "active", "Actief" }, { "paused", "Gepauzeerd" }, { "disabled", "Uitgeschakeld" }, { "terminated", "Beëindigd" },
        };

        private readonly int userId; // 0 = new

        private TextBox txtFirst, txtLast, txtEmail, txtPhone, txtAddress, txtPostal, txtCity, txtPassword;
        private ComboBox cboStatus;
        private Label lblPwHint;

        public CustomerEditForm(int userId)
        {
            this.userId = userId;
            ClientSize = new Size(460, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = userId == 0 ? "Nieuwe klant" : "Klant bewerken";
            buildUi();
            if (userId != 0) loadCustomer();
        }

        private void buildUi()
        {
            var title = new Label
            {
                Text = userId == 0 ? "Nieuwe klant" : "Klant bewerken",
                Dock = DockStyle.Top,
                Height = 54,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var btnClose = new Button
            {
                Text = "✕",
                Location = new Point(424, 6),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                ForeColor = Muted,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;

            txtFirst = field("Voornaam", 30, 70, 195);
            txtLast = field("Achternaam", 235, 70, 195);
            txtEmail = field("E-mail", 30, 128, 400);
            txtPhone = field("Telefoon", 30, 186, 195);
            cboStatus = new ComboBox { Location = new Point(235, 206), Width = 195, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboStatus.Items.AddRange(StatusLabels);
            cboStatus.SelectedIndex = 0;
            Controls.Add(new Label { Text = "Status", Location = new Point(235, 186), AutoSize = true, ForeColor = Muted });

            txtAddress = field("Adres", 30, 244, 400);
            txtPostal = field("Postcode", 30, 302, 110);
            txtCity = field("Woonplaats", 150, 302, 280);

            txtPassword = field(userId == 0 ? "Wachtwoord" : "Nieuw wachtwoord", 30, 360, 400);
            txtPassword.UseSystemPasswordChar = true;
            lblPwHint = new Label
            {
                Text = userId == 0 ? "Minimaal 6 tekens." : "Laat leeg om het wachtwoord niet te wijzigen.",
                Location = new Point(30, 384),
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5F)
            };

            var btnSave = new Button
            {
                Text = "Opslaan",
                Location = new Point(240, 500),
                Size = new Size(190, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => save();

            var btnCancel = new Button
            {
                Text = "Annuleren",
                Location = new Point(30, 500),
                Size = new Size(150, 40),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };

            Controls.AddRange(new Control[] { title, btnClose, cboStatus, lblPwHint, btnSave, btnCancel });
            title.BringToFront();
            CancelButton = btnCancel;
        }

        private TextBox field(string label, int x, int y, int width)
        {
            Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = Muted });
            var box = new TextBox { Location = new Point(x, y + 20), Width = width, Font = new Font("Segoe UI", 10F) };
            Controls.Add(box);
            return box;
        }

        private void loadCustomer()
        {
            DataTable t = DBHelper.getdataTable(
                "SELECT Name, LastName, Email, Phone_Number, Address, Postal_Code, City, User_Status FROM user WHERE ID = " + userId);
            if (t.Rows.Count == 0) return;
            DataRow r = t.Rows[0];
            txtFirst.Text = r["Name"].ToString();
            txtLast.Text = r["LastName"].ToString();
            txtEmail.Text = r["Email"].ToString();
            txtPhone.Text = r["Phone_Number"].ToString();
            txtAddress.Text = r["Address"].ToString();
            txtPostal.Text = r["Postal_Code"].ToString();
            txtCity.Text = r["City"].ToString();
            string code = r["User_Status"].ToString();
            cboStatus.SelectedItem = CodeToLabel.ContainsKey(code) ? CodeToLabel[code] : "Actief";
        }

        private void save()
        {
            string first = txtFirst.Text.Trim();
            string last = txtLast.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string address = txtAddress.Text.Trim();
            string city = txtCity.Text.Trim();

            if (first == "" || last == "" || email == "" || address == "" || city == "")
            {
                MessageBox.Show("Vul voornaam, achternaam, e-mail, adres en woonplaats in.", "Klant opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Vul een geldig e-mailadres in.", "Klant opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }
            if (userId == 0 && txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Vul een wachtwoord van minimaal 6 tekens in.", "Klant opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }
            if (userId != 0 && txtPassword.Text.Length > 0 && txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Een nieuw wachtwoord moet minimaal 6 tekens zijn.", "Klant opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // E-mail must be unique.
            try
            {
                object dup = DBHelper.getScalar(
                    "SELECT COUNT(*) FROM user WHERE Email = " + AuthHelper.sql(email) + " AND ID <> " + userId);
                if (Convert.ToInt32(dup) > 0)
                {
                    MessageBox.Show("Er bestaat al een account met dit e-mailadres.", "Klant opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het e-mailadres niet controleren:\n" + ex.Message, "Klant opslaan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string status = LabelToCode[cboStatus.SelectedItem.ToString()];
            var p = new Dictionary<string, object>
            {
                { "@fn", first }, { "@ln", last }, { "@email", email }, { "@phone", phone },
                { "@addr", address }, { "@postal", txtPostal.Text.Trim() }, { "@city", city }, { "@status", status },
            };

            try
            {
                if (userId == 0)
                {
                    p["@pw"] = AuthHelper.HashPassword(txtPassword.Text);
                    DBHelper.RunSql(
                        "INSERT INTO user (RID, Created_At, Password, Name, LastName, Address, Postal_Code, City, Email, Phone_Number, User_Status) " +
                        "VALUES (0, CURDATE(), @pw, @fn, @ln, @addr, @postal, @city, @email, @phone, @status)", p);
                }
                else
                {
                    p["@id"] = userId;
                    string pwSet = "";
                    if (txtPassword.Text.Length >= 6)
                    {
                        pwSet = ", Password = @pw";
                        p["@pw"] = AuthHelper.HashPassword(txtPassword.Text);
                    }
                    DBHelper.RunSql(
                        "UPDATE user SET Name=@fn, LastName=@ln, Email=@email, Phone_Number=@phone, " +
                        "Address=@addr, Postal_Code=@postal, City=@city, User_Status=@status" + pwSet + " WHERE ID=@id", p);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de klant niet opslaan:\n" + ex.Message, "Klant opslaan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
