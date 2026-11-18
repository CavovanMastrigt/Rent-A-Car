using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Registration popup, styled like the rest of the app.
    public partial class RegisterForm : PopupForm
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private TextBox txtName, txtLastName, txtEmail, txtPhone, txtAddress, txtCity, txtPassword, txtConfirmPassword;
        private Button btnShowPassword, btnShowConfirmPassword;

        public string RegisteredEmail { get; private set; }

        public RegisterForm()
        {
            ClientSize = new Size(460, 632);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Registreren";
            buildUi();
        }

        private void buildUi()
        {
            Controls.Add(new Label { Text = "Account aanmaken", Location = new Point(0, 28), Size = new Size(460, 36), Font = new Font("Segoe UI", 19F, FontStyle.Bold), ForeColor = Ink, TextAlign = ContentAlignment.MiddleCenter });
            Controls.Add(new Label { Text = "Maak een account om een auto te huren", Location = new Point(0, 66), Size = new Size(460, 20), ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter });

            var btnClose = new Button { Text = "✕", Location = new Point(424, 8), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            txtName = field("Voornaam", 30, 104, 185);
            txtLastName = field("Achternaam", 245, 104, 185);
            txtEmail = field("E-mail", 30, 162, 400);
            txtPhone = field("Telefoon", 30, 220, 400);
            txtAddress = field("Adres", 30, 278, 400);
            txtCity = field("Woonplaats", 30, 336, 400);

            Controls.Add(new Label { Text = "Wachtwoord", Location = new Point(30, 394), AutoSize = true, ForeColor = Muted });
            txtPassword = new TextBox { Location = new Point(30, 414), Width = 358, Font = new Font("Segoe UI", 10.5F), UseSystemPasswordChar = true };
            btnShowPassword = toggle(390, 413);
            btnShowPassword.Click += (s, e) => { txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar; btnShowPassword.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🙈"; };

            Controls.Add(new Label { Text = "Bevestig wachtwoord", Location = new Point(30, 450), AutoSize = true, ForeColor = Muted });
            txtConfirmPassword = new TextBox { Location = new Point(30, 470), Width = 358, Font = new Font("Segoe UI", 10.5F), UseSystemPasswordChar = true };
            btnShowConfirmPassword = toggle(390, 469);
            btnShowConfirmPassword.Click += (s, e) => { txtConfirmPassword.UseSystemPasswordChar = !txtConfirmPassword.UseSystemPasswordChar; btnShowConfirmPassword.Text = txtConfirmPassword.UseSystemPasswordChar ? "👁" : "🙈"; };

            var btnRegister = new Button
            {
                Text = "Registreren",
                Location = new Point(30, 520),
                Size = new Size(400, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += btnRegister_Click;

            var btnBack = new Button
            {
                Text = "Al een account?  Inloggen",
                Location = new Point(30, 576),
                Size = new Size(400, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Accent,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] { btnClose, txtPassword, btnShowPassword, txtConfirmPassword, btnShowConfirmPassword, btnRegister, btnBack });
            AcceptButton = btnRegister;
            CancelButton = btnClose;
        }

        private TextBox field(string label, int x, int y, int width)
        {
            Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = Muted });
            var box = new TextBox { Location = new Point(x, y + 20), Width = width, Font = new Font("Segoe UI", 10.5F) };
            Controls.Add(box);
            return box;
        }

        private Button toggle(int x, int y)
        {
            var b = new Button { Text = "👁", Location = new Point(x, y), Size = new Size(40, 27), FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand, TabStop = false };
            b.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            return b;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Trim() == "" || txtLastName.Text.Trim() == "" || txtEmail.Text.Trim() == "" ||
                txtPhone.Text.Trim() == "" || txtAddress.Text.Trim() == "" || txtCity.Text.Trim() == "" || txtPassword.Text == "")
            {
                MessageBox.Show("Vul alle velden in.", "Registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtPassword.Text.Length < 8)
            {
                MessageBox.Show("Het wachtwoord moet minimaal 8 tekens zijn.", "Registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!isPhoneNumber(txtPhone.Text))
            {
                MessageBox.Show("Telefoonnummer mag alleen cijfers bevatten (6 tot 15).", "Registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("De wachtwoorden komen niet overeen.", "Registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                object count = DBHelper.getScalar("SELECT COUNT(*) FROM user WHERE Email = " + AuthHelper.sql(txtEmail.Text));
                if (Convert.ToInt32(count) > 0)
                {
                    MessageBox.Show("Er bestaat al een account met dit e-mailadres.", "Registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DBHelper.RunSql(
                    "INSERT INTO user (RID, Created_At, Password, Name, LastName, Address, City, Email, Phone_Number, User_Status) " +
                    "VALUES (0, CURDATE(), " +
                    AuthHelper.sql(AuthHelper.HashPassword(txtPassword.Text)) + ", " +
                    AuthHelper.sql(txtName.Text) + ", " +
                    AuthHelper.sql(txtLastName.Text) + ", " +
                    AuthHelper.sql(txtAddress.Text) + ", " +
                    AuthHelper.sql(txtCity.Text) + ", " +
                    AuthHelper.sql(txtEmail.Text) + ", " +
                    AuthHelper.sql(txtPhone.Text) + ", 'active')");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het account niet aanmaken:\n" + ex.Message, "Registreren", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Je account is aangemaakt. Je kunt nu inloggen.", "Registreren", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RegisteredEmail = txtEmail.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
