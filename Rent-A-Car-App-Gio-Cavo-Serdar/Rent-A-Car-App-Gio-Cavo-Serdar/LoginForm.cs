using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    public partial class LoginForm : PopupForm
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtEmail.Text.Trim() == "" || txtPassword.Text == "")
            {
                MessageBox.Show("Please enter your email and password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Could not connect to the database:\n" + ex.Message, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (user.Rows.Count == 0 || !AuthHelper.VerifyPassword(txtPassword.Text, user.Rows[0]["Password"].ToString()))
            {
                MessageBox.Show("Wrong email or password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }

            int roleId = Convert.ToInt32(user.Rows[0]["RID"]);
            if (roleId != AuthHelper.RoleCustomer && roleId != AuthHelper.RoleAdmin)
            {
                MessageBox.Show("Your account has no valid role. Please contact an admin.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DBHelper.loggedinuserid = Convert.ToInt32(user.Rows[0]["ID"]);
            AuthHelper.loggedinroleid = roleId;

            // The top bar shows the customer or admin page when this popup closes with OK.
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            using (var registerForm = new RegisterForm())
            {
                if (registerForm.ShowDialog(this) == DialogResult.OK)
                {
                    txtEmail.Text = registerForm.RegisteredEmail;
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
        }
    }
}
