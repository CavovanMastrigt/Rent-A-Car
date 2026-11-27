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
    // De zijbalk heeft zeven knoppen. Wat bij elkaar hoort (bijvoorbeeld alle reserveringsschermen)
    // staat als tabbladen binnen één knop (zie TabbedPage).
    public partial class AdminPage : UserControl
    {
        private static readonly Color SidebarColor = Color.FromArgb(17, 24, 39);
        private static readonly Color SidebarHover = Color.FromArgb(31, 41, 55);
        private static readonly Color ActiveColor = Color.FromArgb(37, 99, 235);
        private static readonly Color NavText = Color.FromArgb(209, 213, 219);
        private const int NavItemWidth = 184;

        private Panel sidebar;
        private FlowLayoutPanel navFlow;
        private Panel content;
        private readonly List<NavEntry> pageEntries = new List<NavEntry>();
        private NavEntry activeEntry;
        private UserControl activePage;

        // One row in the sidebar: a button that opens one page, or a group that opens its pages as tabs.
        private class NavEntry
        {
            public string Text;
            public bool AdminOnly;       // only an admin ever sees it
            public string Perm;          // or: an employee sees it only with this right
            public Func<UserControl> CreatePage;
            public List<NavEntry> Children;   // bij een groep: de tabbladen binnen deze knop
            public Button Button;
        }

        public AdminPage()
        {
            InitializeComponent();
            buildLayout();
            buildNav();
            if (pageEntries.Count > 0)
            {
                showPage(pageEntries[0], null);
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
        // Zit de pagina als tabblad in een groep, dan opent de groep met dat tabblad.
        private void navigateTo(string perm, object arg)
        {
            if (string.IsNullOrEmpty(perm) || !IsHandleCreated) return;
            NavEntry target = pageEntries.FirstOrDefault(x => x.Perm == perm ||
                (x.Children != null && x.Children.Any(c => c.Perm == perm)));
            if (target == null) return; // hidden for this user: ignore silently

            // Defer the swap: the page that asked for it is still running an event
            // handler and is about to be disposed, so let its stack unwind first.
            BeginInvoke((Action)(() =>
            {
                showPage(target, perm);

                // Bij een groep krijgt het geopende tabblad het argument, niet de groep zelf.
                var tabbed = activePage as TabbedPage;
                var nt = (tabbed != null ? tabbed.CurrentPage : activePage) as INavTarget;
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
                Padding = new Padding(10, 6, 10, 4)
            };

            // Fill control added first, Top control after, so the role stays on top.
            sidebar.Controls.Add(navFlow);
            sidebar.Controls.Add(role);

            Controls.Add(content);
            Controls.Add(sidebar);
        }

        // Een knop die één pagina opent.
        private static NavEntry Item(string text, string perm, Func<UserControl> createPage)
        {
            return new NavEntry { Text = text, Perm = perm, CreatePage = createPage };
        }

        // Een knop die meerdere pagina's als tabbladen opent.
        private static NavEntry Group(string text, params NavEntry[] pages)
        {
            return new NavEntry { Text = text, Children = new List<NavEntry>(pages) };
        }

        // Mag deze gebruiker de pagina zien? Beheerders zien alles, medewerkers alleen met het recht.
        private static bool Allowed(NavEntry entry, bool isAdmin)
        {
            return isAdmin || (!entry.AdminOnly && (entry.Perm == null || PermissionHelper.Has(entry.Perm)));
        }

        private void buildNav()
        {
            bool isAdmin = AuthHelper.loggedinroleid == AuthHelper.RoleAdmin;

            // Every page has its own right, so the admin can grant/revoke each one
            // per employee. Admins always see everything regardless of these.
            var all = new List<NavEntry>
            {
                Item("📊   Dashboard", PermissionHelper.Dashboard, () => new DashboardPage()),
                Group("🚗   Auto's",
                    Item("Wagenpark", PermissionHelper.Cars, () => new CarsPage()),
                    Item("Extra's en prijzen", PermissionHelper.Extras, () => new ExtrasPricesControl())),
                Group("📅   Reserveringen",
                    Item("Dagoverzicht", PermissionHelper.DayOverview, () => new DayOverviewControl()),
                    Item("Planning", PermissionHelper.Planning, () => new PlanningControl()),
                    Item("Nieuwe reservering", PermissionHelper.NewReservation, () => new NewReservationControl()),
                    Item("Ophalen / terugbrengen", PermissionHelper.PickupReturn, () => new PickupReturnControl()),
                    Item("Wijzigen / annuleren", PermissionHelper.ChangeCancel, () => new ChangeCancelControl()),
                    Item("Alle reserveringen", PermissionHelper.Rentals, () => new RentalsPage())),
                Group("👥   Klanten",
                    Item("Klantbeheer", PermissionHelper.Customers, () => new CustomersPage()),
                    Item("Klantverificatie", PermissionHelper.Verification, () => new CustomerVerificationPage()),
                    Item("Bijna verlopen ID's", PermissionHelper.Verification, () => new VerificationExpiryPage())),
                Group("🧾   Facturen",
                    Item("Facturen", PermissionHelper.Invoices, () => new InvoicesPage()),
                    Item("Bijna verlopen facturen", PermissionHelper.Invoices, () => new InvoiceExpiryPage())),
                Item("✉   Berichten", PermissionHelper.Messages, () => new MessagesPage()),
                Group("⚙   Beheer",
                    Item("Medewerkers", PermissionHelper.Employees, () => new EmployeesPage()),
                    Item("Rechten", PermissionHelper.Permissions, () => new PermissionsPage()),
                    Item("Openingstijden", PermissionHelper.OpeningHours, () => new OpeningHoursPage()),
                    Item("Logboek", PermissionHelper.Logs, () => new LogsPage()))
            };

            // Keep only what this user may see. A group keeps just the tabs the user may open.
            if (!isAdmin) PermissionHelper.LoadCurrent();
            foreach (NavEntry entry in all)
            {
                if (entry.Children == null)
                {
                    if (Allowed(entry, isAdmin)) addNavButton(entry);
                    continue;
                }

                entry.Children.RemoveAll(child => !Allowed(child, isAdmin));
                if (entry.Children.Count == 0) continue;

                // Blijft er maar één tabblad over, dan tonen we die pagina zelf, zonder tabbalk.
                if (entry.Children.Count == 1)
                {
                    entry.Perm = entry.Children[0].Perm;
                    entry.CreatePage = entry.Children[0].CreatePage;
                    entry.Children = null;
                }
                addNavButton(entry);
            }
        }

        private void addNavButton(NavEntry entry)
        {
            var button = new Button
            {
                Text = entry.Text,
                Width = NavItemWidth,
                Height = 36,
                Margin = new Padding(0, 2, 0, 2),
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
            button.Click += (s, e) => showPage(entry, null);
            entry.Button = button;
            navFlow.Controls.Add(button);
            pageEntries.Add(entry);
        }

        // Opens the given page and highlights its sidebar button.
        // selectPerm: bij een groep het tabblad dat open moet gaan (null = het eerste).
        private void showPage(NavEntry entry, string selectPerm)
        {
            if (entry == null || (entry.CreatePage == null && entry.Children == null))
            {
                return;
            }

            // Staat deze knop al open, dan wisselen we hooguit van tabblad.
            if (entry == activeEntry)
            {
                var open = activePage as TabbedPage;
                if (open != null && selectPerm != null) open.SelectByPerm(selectPerm);
                return;
            }

            foreach (Control old in content.Controls.Cast<Control>().ToList())
            {
                old.Dispose();
            }

            UserControl page;
            try
            {
                page = entry.Children != null
                    ? new TabbedPage(entry.Children.Select(c => new TabbedPage.Tab(c.Text, c.Perm, c.CreatePage)).ToList(), selectPerm)
                    : entry.CreatePage();
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