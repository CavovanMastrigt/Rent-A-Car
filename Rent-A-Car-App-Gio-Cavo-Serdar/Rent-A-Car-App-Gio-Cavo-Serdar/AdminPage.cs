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
        private const int NavItemWidth = 184;

        private Panel sidebar;
        private FlowLayoutPanel navFlow;
        private Panel content;
        private readonly List<NavEntry> entries = new List<NavEntry>();
        private readonly List<NavEntry> pageEntries = new List<NavEntry>();
        private NavEntry activeEntry;
        private UserControl activePage;

        // One row in the sidebar: either a section header, or a button that opens a page.
        private class NavEntry
        {
            public string Text;
            public bool IsHeader;
            public bool AdminOnly;       // only an admin ever sees it
            public string Perm;          // or: an employee sees it only with this right
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

            // Let other pages/popups send the user to a page by its permission key.
            AdminNav.Handler = navigateTo;
            Disposed += (s, e) => { if (AdminNav.Handler == (Action<string, object>)navigateTo) AdminNav.Handler = null; };

            // On staff login: send the one-off in-app reminders for ID cards and
            // invoices that just entered their "bijna verlopen" window.
            Load += (s, e) => { try { ReminderHelper.SendDueInAppReminders(); } catch { } };
        }

        // Opens the page with the given permission key (if this user may see it)
        // and hands it an optional argument. Used for cross-page jumps, e.g.
        // "Medewerker -> Rechten" or "Auto -> die reservering".
        private void navigateTo(string perm, object arg)
        {
            if (string.IsNullOrEmpty(perm) || !IsHandleCreated) return;
            NavEntry target = pageEntries.FirstOrDefault(x => x.Perm == perm);
            if (target == null) return; // hidden for this user: ignore silently

            // Defer the swap: the page that asked for it is still running an event
            // handler and is about to be disposed, so let its stack unwind first.
            BeginInvoke((Action)(() =>
            {
                showPage(target);
                var nt = activePage as INavTarget;
                if (nt != null) nt.ApplyNavArg(arg);
            }));
        }

        private void buildLayout()
        {
            content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 244, 246) };

            sidebar = new Panel { Dock = DockStyle.Left, Width = 208, BackColor = SidebarColor };

            // The brand sits in the (dark) top bar; the sidebar starts with the role.
            string roleName = AuthHelper.loggedinroleid == AuthHelper.RoleAdmin ? "Administrator" : "Medewerker";
            var role = new Label
            {
                Text = "  " + roleName,
                Dock = DockStyle.Top,
                Height = 28,
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
                Padding = new Padding(10, 2, 10, 4)
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

            // Every page has its own right, so the admin can grant/revoke each one
            // per employee. Admins always see everything regardless of these.
            entries.Add(new NavEntry { Text = "📊   Dashboard", Perm = PermissionHelper.Dashboard, CreatePage = () => new DashboardPage() });
            entries.Add(new NavEntry { Text = "🚗   Auto's", Perm = PermissionHelper.Cars, CreatePage = () => new CarsPage() });

            // Employee pages (Serdar)
            entries.Add(new NavEntry { Text = "Medewerker", IsHeader = true });
            entries.Add(new NavEntry { Text = "Dagoverzicht", Perm = PermissionHelper.DayOverview, CreatePage = () => new DayOverviewControl() });
            entries.Add(new NavEntry { Text = "Planning", Perm = PermissionHelper.Planning, CreatePage = () => new PlanningControl() });
            entries.Add(new NavEntry { Text = "Nieuwe reservering", Perm = PermissionHelper.NewReservation, CreatePage = () => new NewReservationControl() });
            entries.Add(new NavEntry { Text = "Ophalen / terugbrengen", Perm = PermissionHelper.PickupReturn, CreatePage = () => new PickupReturnControl() });
            entries.Add(new NavEntry { Text = "Wijzigen / annuleren", Perm = PermissionHelper.ChangeCancel, CreatePage = () => new ChangeCancelControl() });
            entries.Add(new NavEntry { Text = "Extra's en prijzen", Perm = PermissionHelper.Extras, CreatePage = () => new ExtrasPricesControl() });
            entries.Add(new NavEntry { Text = "Reserveringen", Perm = PermissionHelper.Rentals, CreatePage = () => new RentalsPage() });

            // Customers - admin manages customers; staff verify and view invoices.
            entries.Add(new NavEntry { Text = "Klanten", IsHeader = true });
            entries.Add(new NavEntry { Text = "Klantbeheer", Perm = PermissionHelper.Customers, CreatePage = () => new CustomersPage() });
            entries.Add(new NavEntry { Text = "Klantverificatie", Perm = PermissionHelper.Verification, CreatePage = () => new CustomerVerificationPage() });
            entries.Add(new NavEntry { Text = "Bijna verlopen ID's", Perm = PermissionHelper.Verification, CreatePage = () => new VerificationExpiryPage() });
            entries.Add(new NavEntry { Text = "Facturen", Perm = PermissionHelper.Invoices, CreatePage = () => new InvoicesPage() });
            entries.Add(new NavEntry { Text = "Bijna verlopen facturen", Perm = PermissionHelper.Invoices, CreatePage = () => new InvoiceExpiryPage() });
            entries.Add(new NavEntry { Text = "Berichten", Perm = PermissionHelper.Messages, CreatePage = () => new MessagesPage() });

            // Management
            entries.Add(new NavEntry { Text = "Beheer", IsHeader = true });
            entries.Add(new NavEntry { Text = "Medewerkers", Perm = PermissionHelper.Employees, CreatePage = () => new EmployeesPage() });
            entries.Add(new NavEntry { Text = "Rechten", Perm = PermissionHelper.Permissions, CreatePage = () => new PermissionsPage() });
            entries.Add(new NavEntry { Text = "Openingstijden", Perm = PermissionHelper.OpeningHours, CreatePage = () => new OpeningHoursPage() });
            entries.Add(new NavEntry { Text = "Logboek", Perm = PermissionHelper.Logs, CreatePage = () => new LogsPage() });

            // Keep only what this user may see: admins see everything, employees see
            // the pages they have the right for. Then drop empty section headers.
            if (!isAdmin) PermissionHelper.LoadCurrent();
            var visible = new List<NavEntry>();
            foreach (NavEntry e in entries)
            {
                if (e.IsHeader) { visible.Add(e); continue; }
                bool ok = isAdmin || (!e.AdminOnly && (e.Perm == null || PermissionHelper.Has(e.Perm)));
                if (ok) visible.Add(e);
            }
            var shown = new List<NavEntry>();
            for (int i = 0; i < visible.Count; i++)
            {
                if (visible[i].IsHeader)
                {
                    bool hasChild = i + 1 < visible.Count && !visible[i + 1].IsHeader;
                    if (hasChild) shown.Add(visible[i]);
                }
                else shown.Add(visible[i]);
            }
            entries.Clear();
            entries.AddRange(shown);

            foreach (NavEntry entry in entries)
            {
                if (entry.IsHeader)
                {
                    navFlow.Controls.Add(new Label
                    {
                        Text = entry.Text.ToUpper(),
                        Width = NavItemWidth,
                        Height = 16,
                        Margin = new Padding(6, 3, 0, 0),
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
                    Height = 28,
                    Margin = new Padding(0, 1, 0, 1),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = NavText,
                    BackColor = SidebarColor,
                    Font = new Font("Segoe UI", 9.5F),
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
            activePage = page;

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
