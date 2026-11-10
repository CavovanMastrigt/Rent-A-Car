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
    // Page shown under the top bar for staff (role 1 = admin, role 2 = employee).
    // A left sidebar lets the admin cycle through all the admin pages; the content
    // area on the right shows the selected page.
    public partial class AdminPage : UserControl
    {
        private static readonly Color SidebarColor = Color.FromArgb(17, 24, 39);
        private static readonly Color SidebarHover = Color.FromArgb(31, 41, 55);
        private static readonly Color ActiveColor = Color.FromArgb(37, 99, 235);
        private static readonly Color NavText = Color.FromArgb(209, 213, 219);

        private Panel sidebar;
        private Panel content;
        private readonly List<NavItem> navItems = new List<NavItem>();
        private readonly List<Button> navButtons = new List<Button>();
        private int activeIndex = -1;

        // One entry in the sidebar: a label, the page it opens, and whether it is admin-only.
        private class NavItem
        {
            public string Text;
            public Func<UserControl> CreatePage;
            public bool AdminOnly;
        }

        public AdminPage()
        {
            InitializeComponent();
            buildLayout();
            buildNav();
            if (navItems.Count > 0)
            {
                showPage(0);
            }
        }

        private void buildLayout()
        {
            content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 244, 246) };

            sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = SidebarColor };

            var brand = new Label
            {
                Text = "  Rent A Car",
                Dock = DockStyle.Top,
                Height = 64,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            string roleName = AuthHelper.loggedinroleid == AuthHelper.RoleAdmin ? "Administrator" : "Employee";
            var role = new Label
            {
                Text = "  " + roleName,
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = NavText,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Controls docked Top render top-to-bottom in reverse add order.
            sidebar.Controls.Add(role);
            sidebar.Controls.Add(brand);

            Controls.Add(content);
            Controls.Add(sidebar);
        }

        private void buildNav()
        {
            navItems.Add(new NavItem { Text = "📊   Dashboard", AdminOnly = true, CreatePage = () => new DashboardPage() });
            navItems.Add(new NavItem { Text = "🚗   Auto's", AdminOnly = false, CreatePage = () => new CarsPage() });

            // Employees do not see admin-only pages.
            bool isAdmin = AuthHelper.loggedinroleid == AuthHelper.RoleAdmin;
            navItems.RemoveAll(n => n.AdminOnly && !isAdmin);

            // Add buttons bottom-up so the first item ends up on top.
            for (int i = navItems.Count - 1; i >= 0; i--)
            {
                int index = i;
                var button = new Button
                {
                    Text = navItems[i].Text,
                    Dock = DockStyle.Top,
                    Height = 46,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = NavText,
                    BackColor = SidebarColor,
                    Font = new Font("Segoe UI", 10.5F),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(16, 0, 0, 0),
                    Cursor = Cursors.Hand,
                    TabStop = false
                };
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = SidebarHover;
                button.Click += (s, e) => showPage(index);
                sidebar.Controls.Add(button);
                button.BringToFront();
                navButtons.Insert(0, button);
            }
        }

        // Opens the page at the given index and highlights its sidebar button.
        private void showPage(int index)
        {
            if (index < 0 || index >= navItems.Count || index == activeIndex)
            {
                return;
            }

            foreach (Control old in content.Controls.Cast<Control>().ToList())
            {
                old.Dispose();
            }

            UserControl page = navItems[index].CreatePage();
            page.Dock = DockStyle.Fill;
            content.Controls.Add(page);

            activeIndex = index;
            for (int i = 0; i < navButtons.Count; i++)
            {
                bool active = i == index;
                navButtons[i].BackColor = active ? ActiveColor : SidebarColor;
                navButtons[i].ForeColor = active ? Color.White : NavText;
                navButtons[i].FlatAppearance.MouseOverBackColor = active ? ActiveColor : SidebarHover;
            }
        }
    }
}
