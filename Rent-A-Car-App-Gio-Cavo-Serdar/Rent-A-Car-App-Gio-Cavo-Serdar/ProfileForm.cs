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
    public partial class ProfileForm : PopupForm
    {
        public ProfileForm()
        {
            InitializeComponent();
        }

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            DataTable user = DBHelper.getdataTable(
                "SELECT Name, LastName, Email, Phone_Number, Address, City FROM user WHERE ID = @id",
                new Dictionary<string, object> { { "@id", DBHelper.loggedinuserid } });

            if (user.Rows.Count == 0)
            {
                Close();
                return;
            }

            DataRow row = user.Rows[0];
            txtName.Text = row["Name"].ToString();
            txtLastName.Text = row["LastName"].ToString();
            txtEmail.Text = row["Email"].ToString();
            txtPhone.Text = row["Phone_Number"].ToString();
            txtAddress.Text = row["Address"].ToString();
            txtCity.Text = row["City"].ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Trim() == "" || txtLastName.Text.Trim() == "" || txtEmail.Text.Trim() == "" ||
                txtPhone.Text.Trim() == "" || txtAddress.Text.Trim() == "" || txtCity.Text.Trim() == "")
            {
                MessageBox.Show("Please fill in all fields.", "Profile settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!isPhoneNumber(txtPhone.Text))
            {
                MessageBox.Show("Phone number may only contain digits (6 to 15).", "Profile settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool changePassword =txtNewPassword.Text != "" || txtConfirmNewPassword.Text != "";
            if (changePassword && !checkNewPassword())
            {
                return;
            }

            try
            {
                object count = DBHelper.getScalar(
                    "SELECT COUNT(*) FROM user WHERE Email = @email AND ID <> @id",
                    new Dictionary<string, object> { { "@email", txtEmail.Text.Trim() }, { "@id", DBHelper.loggedinuserid } });

                if (Convert.ToInt32(count) > 0)
                {
                    MessageBox.Show("There is already an account with this email.", "Profile settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DBHelper.RunSql(
                    "UPDATE user SET Name = @name, LastName = @lastName, Email = @email, Phone_Number = @phone, " +
                    "Address = @address, City = @city WHERE ID = @id",
                    new Dictionary<string, object>
                    {
                        { "@name", txtName.Text.Trim() },
                        { "@lastName", txtLastName.Text.Trim() },
                        { "@email", txtEmail.Text.Trim() },
                        { "@phone", txtPhone.Text.Trim() },
                        { "@address", txtAddress.Text.Trim() },
                        { "@city", txtCity.Text.Trim() },
                        { "@id", DBHelper.loggedinuserid }
                    });

                if (changePassword)
                {
                    DBHelper.RunSql(
                        "UPDATE user SET Password = @password WHERE ID = @id",
                        new Dictionary<string, object>
                        {
                            { "@password", DBHelper.HashPassword(txtNewPassword.Text) },
                            { "@id", DBHelper.loggedinuserid }
                        });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save your profile:\n" + ex.Message, "Profile settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Your profile has been saved.", "Profile settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        // The current password must be correct before a new one is accepted.
        private bool checkNewPassword()
        {
            object storedHash = DBHelper.getScalar(
                "SELECT Password FROM user WHERE ID = @id",
                new Dictionary<string, object> { { "@id", DBHelper.loggedinuserid } });

            if (storedHash == null || !DBHelper.VerifyPassword(txtCurrentPassword.Text, storedHash.ToString()))
            {
                MessageBox.Show("Your current password is wrong.", "Profile settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (txtNewPassword.Text.Length < 8)
            {
                MessageBox.Show("New password must be at least 8 characters.", "Profile settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (txtNewPassword.Text != txtConfirmNewPassword.Text)
            {
                MessageBox.Show("New passwords do not match.", "Profile settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
