using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Add or edit a staff member (admin / employee) in a popup. Admin only.
    // The admin sets the role and status; you cannot change your own role here.
    public class EmployeeEditForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private static readonly string[] RoleLabels = { "Beheerder", "Medewerker" };
        private static readonly Dictionary<string, int> LabelToRole = new Dictionary<string, int>
        {
            { "Beheerder", AuthHelper.RoleAdmin }, { "Medewerker", AuthHelper.RoleEmployee },
        };

        private static readonly string[] StatusLabels = { "Actief", "Gepauzeerd", "Uitgeschakeld", "Beëindigd" };
        private static readonly Dictionary<string, string> LabelToCode = new Dictionary<string, string>
        {
            { "Actief", "active" }, { "Gepauzeerd", "paused" }, { "Uitgeschakeld", "disabled" }, { "Beëindigd", "terminated" },
        };
        private static readonly Dictionary<string, string> CodeToLabel = new Dictionary<string, string>
        {
            { "active", "Actief" }, { "paused", "Gepauzeerd" }, { "disabled", "Uitgeschakeld" }, { "terminated", "Beëindigd" },
        };

        private readonly int userId;    // 0 = new
        private readonly bool isSelf;

        private TextBox txtFirst, txtLast, txtEmail, txtPhone, txtAddress, txtCity, txtPassword;
        private ComboBox cboRole, cboStatus;

        public EmployeeEditForm(int userId)
        {
            this.userId = userId;
            this.isSelf = userId != 0 && userId == DBHelper.loggedinuserid;
            ClientSize = new Size(460, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = userId == 0 ? "Nieuwe medewerker" : "Medewerker bewerken";
            buildUi();
            if (userId != 0) loadStaff();
        }

        private void buildUi()
        {
            var title = new Label
            {
                Text = userId == 0 ? "Nieuwe medewerker" : "Medewerker bewerken",
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
            txtPhone = field("Telefoon", 30, 186, 400);

            Controls.Add(new Label { Text = "Rol", Location = new Point(30, 244), AutoSize = true, ForeColor = Muted });
            cboRole = new ComboBox { Location = new Point(30, 264), Width = 195, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboRole.Items.AddRange(RoleLabels);
            cboRole.SelectedIndex = 1; // Medewerker by default
            if (isSelf)
            {
                cboRole.Enabled = false; // you cannot change your own role
            }

            Controls.Add(new Label { Text = "Status", Location = new Point(235, 244), AutoSize = true, ForeColor = Muted });
            cboStatus = new ComboBox { Location = new Point(235, 264), Width = 195, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboStatus.Items.AddRange(StatusLabels);
            cboStatus.SelectedIndex = 0;

            txtAddress = field("Adres", 30, 302, 400);
            txtCity = field("Woonplaats", 30, 360, 195);

            txtPassword = field(userId == 0 ? "Wachtwoord" : "Nieuw wachtwoord", 235, 360, 195);
            txtPassword.UseSystemPasswordChar = true;
            Controls.Add(new Label
            {
                Text = userId == 0 ? "Min. 6 tekens." : "Leeg = ongewijzigd.",
                Location = new Point(235, 404),
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5F)
            });
            if (isSelf)
            {
                Controls.Add(new Label { Text = "Dit is je eigen account; je rol kun je hier niet wijzigen.", Location = new Point(30, 430), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 8.5F) });
            }

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

            Controls.AddRange(new Control[] { title, btnClose, cboRole, cboStatus, btnSave, btnCancel });
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

        private void loadStaff()
        {
            DataTable t = DBHelper.getdataTable(
                "SELECT Name, LastName, Email, Phone_Number, Address, City, RID, User_Status FROM user WHERE ID = " + userId);
            if (t.Rows.Count == 0) return;
            DataRow r = t.Rows[0];
            txtFirst.Text = r["Name"].ToString();
            txtLast.Text = r["LastName"].ToString();
            txtEmail.Text = r["Email"].ToString();
            txtPhone.Text = r["Phone_Number"].ToString();
            txtAddress.Text = r["Address"].ToString();
            txtCity.Text = r["City"].ToString();
            cboRole.SelectedItem = Convert.ToInt32(r["RID"]) == AuthHelper.RoleAdmin ? "Beheerder" : "Medewerker";
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
                MessageBox.Show("Vul voornaam, achternaam, e-mail, adres en woonplaats in.", "Medewerker opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Vul een geldig e-mailadres in.", "Medewerker opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }
            if (userId == 0 && txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Vul een wachtwoord van minimaal 6 tekens in.", "Medewerker opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }
            if (userId != 0 && txtPassword.Text.Length > 0 && txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Een nieuw wachtwoord moet minimaal 6 tekens zijn.", "Medewerker opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                object dup = DBHelper.getScalar(
                    "SELECT COUNT(*) FROM user WHERE Email = " + AuthHelper.sql(email) + " AND ID <> " + userId);
                if (Convert.ToInt32(dup) > 0)
                {
                    MessageBox.Show("Er bestaat al een account met dit e-mailadres.", "Medewerker opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het e-mailadres niet controleren:\n" + ex.Message, "Medewerker opslaan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int role = LabelToRole[cboRole.SelectedItem.ToString()];
            string status = LabelToCode[cboStatus.SelectedItem.ToString()];
            var p = new Dictionary<string, object>
            {
                { "@rid", role }, { "@fn", first }, { "@ln", last }, { "@email", email }, { "@phone", phone },
                { "@addr", address }, { "@city", city }, { "@status", status },
            };

            try
            {
                if (userId == 0)
                {
                    p["@pw"] = AuthHelper.HashPassword(txtPassword.Text);
                    DBHelper.RunSql(
                        "INSERT INTO user (RID, Created_At, Password, Name, LastName, Address, City, Email, Phone_Number, User_Status) " +
                        "VALUES (@rid, CURDATE(), @pw, @fn, @ln, @addr, @city, @email, @phone, @status)", p);
                }
                else
                {
                    p["@id"] = userId;
                    // Never change your own role here (guards against self-lockout).
                    string roleSet = isSelf ? "" : "RID=@rid, ";
                    string pwSet = "";
                    if (txtPassword.Text.Length >= 6)
                    {
                        pwSet = ", Password=@pw";
                        p["@pw"] = AuthHelper.HashPassword(txtPassword.Text);
                    }
                    DBHelper.RunSql(
                        "UPDATE user SET " + roleSet + "Name=@fn, LastName=@ln, Email=@email, Phone_Number=@phone, " +
                        "Address=@addr, City=@city, User_Status=@status" + pwSet + " WHERE ID=@id", p);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de medewerker niet opslaan:\n" + ex.Message, "Medewerker opslaan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
