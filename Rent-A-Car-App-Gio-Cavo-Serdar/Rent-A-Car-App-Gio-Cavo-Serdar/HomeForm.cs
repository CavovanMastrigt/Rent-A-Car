using System;
using System.Linq;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
            showPage();
        }

        private void topBar_LoginChanged(object sender, EventArgs e)
        {
            showPage();
        }

        private void showPage()
        {
            foreach (Control oldPage in pnlContent.Controls.Cast<Control>().ToList())
            {
                oldPage.Dispose();
            }

            pnlContent.Controls.Clear();

            if (DBHelper.loggedinuserid == 0)
            {
                Text = "Rent A Car";
                return;
            }
            UserControl page;

            // Admin and employee both get the admin area (employee folds into admin later).
            if (AuthHelper.isStaff(AuthHelper.loggedinroleid))
            {
                Text = AuthHelper.loggedinroleid == AuthHelper.RoleAdmin
                    ? "Rent A Car - Admin"
                    : "Rent A Car - Employee";
                page = new AdminPage();
            }
            else
            {
                Text = "Rent A Car";
                page = new CustomerPage();
            }

            page.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(page);
        }
    }
}
