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
    // A single left sidebar lets the user cycle through all admin + employee pages;
    // the content area on the right shows the selected page.
    public partial class AdminPage : UserControl
    {
        private static readonly Color SidebarColor = Color.FromArgb(17, 24, 39);
        private static readonly Color SidebarHover = Color.FromArgb(31, 41, 55);
        private static readonly Color ActiveColor = Color.FromArgb(37, 99, 235);
        private static readonly Color NavText = Color.FromArgb(209, 213, 219);
        private static readonly Color HeaderText = Color.FromArgb(107, 114, 128);
        private const int NavItemWidth = 206;

        private Panel sidebar;
        private FlowLayoutPanel navFlow;
        private Panel content;
        private readonly List<NavEntry> entries = new List<NavEntry>();
        private readonly List<NavEntry> pageEntries = new List<NavEntry>();
        private NavEntry activeEntry;

        // One row in the sidebar: either a section header, or a button that opens a page.
        private class NavEntry
        {
            public string Text;
            public bool IsHeader;
            public bool AdminOnly;
            public Func<UserControl> CreatePage;
            public Button Button;
        }

        public AdminPage()
        {
            InitializeComponent();
            buildLayout();
            buildNav();
            if (pageEntries.Count > 0)
            {
                showPage(pageEntries[0]);
            }
        }

        private void buildLayout()
        {
            content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 244, 246) };

            sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = SidebarColor };

            // The brand sits in the (dark) top bar; the sidebar starts with the role.
            string roleName = AuthHelper.loggedinroleid == AuthHelper.RoleAdmin ? "Administrator" : "Medewerker";
            var role = new Label
            {
                Text = "  " + roleName,
                Dock = DockStyle.Top,
                Height = 44,
                ForeColor = NavText,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = SidebarColor,
                Padding = new Padding(10, 4, 10, 8)
            };

            // Fill control added first, Top control after, so the role stays on top.
            sidebar.Controls.Add(navFlow);
            sidebar.Controls.Add(role);

            Controls.Add(content);
            Controls.Add(sidebar);
        }

        private void buildNav()
        {
            bool isAdmin = AuthHelper.loggedinroleid == AuthHelper.RoleAdmin;

            // Admin pages
            entries.Add(new NavEntry { Text = "📊   Dashboard", AdminOnly = true, CreatePage = () => new DashboardPage() });
            entries.Add(new NavEntry { Text = "🚗   Auto's", AdminOnly = true, CreatePage = () => new CarsPage() });

            // Employee pages (Serdar)
            entries.Add(new NavEntry { Text = "Medewerker", IsHeader = true });
            entries.Add(new NavEntry { Text = "Dagoverzicht", CreatePage = () => new DayOverviewControl() });
            entries.Add(new NavEntry { Text = "Planning", CreatePage = () => new PlanningControl() });
            entries.Add(new NavEntry { Text = "Nieuwe reservering", CreatePage = () => new NewReservationControl() });
            entries.Add(new NavEntry { Text = "Ophalen / terugbrengen", CreatePage = () => new PickupReturnControl() });
            entries.Add(new NavEntry { Text = "Wijzigen / annuleren", CreatePage = () => new ChangeCancelControl() });
            entries.Add(new NavEntry { Text = "Extra's en prijzen", CreatePage = () => new ExtrasPricesControl() });

            // Customers - admin manages customers; staff verify and view invoices.
            entries.Add(new NavEntry { Text = "Klanten", IsHeader = true });
            entries.Add(new NavEntry { Text = "Klantbeheer", AdminOnly = true, CreatePage = () => new CustomersPage() });
            entries.Add(new NavEntry { Text = "Klantverificatie", CreatePage = () => new CustomerVerificationPage() });
            entries.Add(new NavEntry { Text = "Facturen", CreatePage = () => new InvoicesPage() });

            // Management (admin only)
            entries.Add(new NavEntry { Text = "Beheer", IsHeader = true, AdminOnly = true });
            entries.Add(new NavEntry { Text = "Medewerkers", AdminOnly = true, CreatePage = () => new EmployeesPage() });
            entries.Add(new NavEntry { Text = "Openingstijden", AdminOnly = true, CreatePage = () => new OpeningHoursPage() });

            // Employees do not see admin-only pages.
            entries.RemoveAll(e => e.AdminOnly && !isAdmin);

            foreach (NavEntry entry in entries)
            {
                if (entry.IsHeader)
                {
                    navFlow.Controls.Add(new Label
                    {
                        Text = entry.Text.ToUpper(),
                        Width = NavItemWidth,
                        Height = 22,
                        Margin = new Padding(6, 8, 0, 0),
                        ForeColor = HeaderText,
                        Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                        TextAlign = ContentAlignment.BottomLeft
                    });
                    continue;
                }

                NavEntry captured = entry;
                var button = new Button
                {
                    Text = entry.Text,
                    Width = NavItemWidth,
                    Height = 37,
                    Margin = new Padding(0, 1, 0, 1),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = NavText,
                    BackColor = SidebarColor,
                    Font = new Font("Segoe UI", 10F),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(14, 0, 0, 0),
                    Cursor = Cursors.Hand,
                    TabStop = false
                };
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = SidebarHover;
                button.Click += (s, e) => showPage(captured);
                entry.Button = button;
                navFlow.Controls.Add(button);
                pageEntries.Add(entry);
            }
        }

        // Opens the given page and highlights its sidebar button.
        private void showPage(NavEntry entry)
        {
            if (entry == null || entry.CreatePage == null || entry == activeEntry)
            {
                return;
            }

            foreach (Control old in content.Controls.Cast<Control>().ToList())
            {
                old.Dispose();
            }

            UserControl page;
            try
            {
                page = entry.CreatePage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Deze pagina kon niet geopend worden:\n" + ex.Message,
                    "Pagina", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            page.Dock = DockStyle.Fill;
            content.Controls.Add(page);

            activeEntry = entry;
            foreach (NavEntry e in pageEntries)
            {
                bool active = e == entry;
                e.Button.BackColor = active ? ActiveColor : SidebarColor;
                e.Button.ForeColor = active ? Color.White : NavText;
                e.Button.FlatAppearance.MouseOverBackColor = active ? ActiveColor : SidebarHover;
            }
        }
    }
}
