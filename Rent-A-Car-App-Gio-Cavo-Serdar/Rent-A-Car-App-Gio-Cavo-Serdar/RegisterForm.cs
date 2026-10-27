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
    public partial class RegisterForm : PopupForm
    {
        // Filled in after a successful registration, so the login form can prefill it.
        public string RegisteredEmail { get; private set; }

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Trim() == "" || txtLastName.Text.Trim() == "" || txtEmail.Text.Trim() == "" ||
                txtPhone.Text.Trim() == "" || txtAddress.Text.Trim() == "" || txtCity.Text.Trim() == "" ||
                txtPassword.Text == "")
            {
                MessageBox.Show("Please fill in all fields.", "Register", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters.", "Register", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!isPhoneNumber(txtPhone.Text))
            {
                MessageBox.Show("Phone number may only contain digits (6 to 15).", "Register", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Register", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                object count = DBHelper.getScalar(
                    "SELECT COUNT(*) FROM user WHERE Email = @email",
                    new Dictionary<string, object> { { "@email", txtEmail.Text.Trim() } });

                if (Convert.ToInt32(count) > 0)
                {
                    MessageBox.Show("There is already an account with this email.", "Register", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // RID 0 = standard user
                DBHelper.RunSql(
                    "INSERT INTO user (RID, Created_At, Password, Name, LastName, Address, City, Email, Phone_Number, User_Status) " +
                    "VALUES (0, CURDATE(), @password, @name, @lastName, @address, @city, @email, @phone, 'active')",
                    new Dictionary<string, object>
                    {
                        { "@password", DBHelper.HashPassword(txtPassword.Text) },
                        { "@name", txtName.Text.Trim() },
                        { "@lastName", txtLastName.Text.Trim() },
                        { "@address", txtAddress.Text.Trim() },
                        { "@city", txtCity.Text.Trim() },
                        { "@email", txtEmail.Text.Trim() },
                        { "@phone", txtPhone.Text.Trim() }
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save your account:\n" + ex.Message, "Register", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Your account has been created. You can now log in.", "Register", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RegisteredEmail = txtEmail.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
