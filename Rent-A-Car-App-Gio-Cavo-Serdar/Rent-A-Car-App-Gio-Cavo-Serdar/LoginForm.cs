using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Login popup, styled like the rest of the app.
    public partial class LoginForm : PopupForm
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private TextBox txtEmail, txtPassword;
        private Button btnShowPassword;

        public LoginForm()
        {
            ClientSize = new Size(380, 430);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Inloggen";
            buildUi();
        }

        private void buildUi()
        {
            Controls.Add(new Label { Text = "Inloggen", Location = new Point(0, 34), Size = new Size(380, 36), Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Ink, TextAlign = ContentAlignment.MiddleCenter });
            Controls.Add(new Label { Text = "Welkom terug bij Rent A Car", Location = new Point(0, 72), Size = new Size(380, 20), ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter });

            var btnClose = new Button { Text = "✕", Location = new Point(344, 8), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            Controls.Add(new Label { Text = "E-mail", Location = new Point(40, 118), AutoSize = true, ForeColor = Muted });
            txtEmail = new TextBox { Location = new Point(40, 138), Width = 300, Font = new Font("Segoe UI", 10.5F) };

            Controls.Add(new Label { Text = "Wachtwoord", Location = new Point(40, 186), AutoSize = true, ForeColor = Muted });
            txtPassword = new TextBox { Location = new Point(40, 206), Width = 258, Font = new Font("Segoe UI", 10.5F), UseSystemPasswordChar = true };
            btnShowPassword = new Button { Text = "👁", Location = new Point(300, 205), Size = new Size(40, 27), FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand, TabStop = false };
            btnShowPassword.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnShowPassword.Click += btnShowPassword_Click;

            var btnLogin = new Button
            {
                Text = "Inloggen",
                Location = new Point(40, 262),
                Size = new Size(300, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += btnLogin_Click;

            var btnRegister = new Button
            {
                Text = "Nog geen account?  Registreren",
                Location = new Point(40, 318),
                Size = new Size(300, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Accent,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += btnRegister_Click;

            Controls.AddRange(new Control[] { btnClose, txtEmail, txtPassword, btnShowPassword, btnLogin, btnRegister });
            AcceptButton = btnLogin;
            CancelButton = btnClose;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtEmail.Text.Trim() == "" || txtPassword.Text == "")
            {
                MessageBox.Show("Vul je e-mail en wachtwoord in.", "Inloggen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable user;
            try
            {
                user = DBHelper.getdataTable(
                    "SELECT ID, RID, Password FROM user WHERE Email = " + AuthHelper.sql(txtEmail.Text) + " AND User_Status = 'active'");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kan geen verbinding maken met de database:\n" + ex.Message, "Inloggen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (user.Rows.Count == 0 || !AuthHelper.VerifyPassword(txtPassword.Text, user.Rows[0]["Password"].ToString()))
            {
                MessageBox.Show("Onjuist e-mailadres of wachtwoord.", "Inloggen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }

            int roleId = Convert.ToInt32(user.Rows[0]["RID"]);
            if (roleId != AuthHelper.RoleCustomer && roleId != AuthHelper.RoleAdmin && roleId != AuthHelper.RoleEmployee)
            {
                MessageBox.Show("Je account heeft geen geldige rol. Neem contact op met een beheerder.", "Inloggen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DBHelper.loggedinuserid = Convert.ToInt32(user.Rows[0]["ID"]);
            AuthHelper.loggedinroleid = roleId;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnShowPassword_Click(object sender, EventArgs e)
        {
            bool show = txtPassword.UseSystemPasswordChar;
            txtPassword.UseSystemPasswordChar = !show;
            btnShowPassword.Text = show ? "🙈" : "👁";
            txtPassword.Focus();
            txtPassword.SelectionStart = txtPassword.TextLength;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            using (var registerForm = new RegisterForm())
            {
                if (PopupForm.show(this, registerForm) == DialogResult.OK)
                {
                    txtEmail.Text = registerForm.RegisteredEmail;
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
        }
    }
}
