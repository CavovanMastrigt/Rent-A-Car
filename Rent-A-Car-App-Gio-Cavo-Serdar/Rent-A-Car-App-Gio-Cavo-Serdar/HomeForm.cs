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
    // The one main window. Starts as an empty home page with "Log in" in the top bar;
    // after logging in it shows the customer or admin page under the top bar.
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
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

            UserControl page = null;
            if (DBHelper.loggedinuserid == 0)
            {
                Text = "Rent A Car";
            }
            else if (AuthHelper.loggedinroleid == AuthHelper.RoleAdmin)
            {
                Text = "Rent A Car - Admin";
                page = new AdminPage();
            }
            else
            {
                Text = "Rent A Car";
                page = new CustomerPage();
            }

            if (page != null)
            {
                page.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(page);
            }
        }
    }
}
