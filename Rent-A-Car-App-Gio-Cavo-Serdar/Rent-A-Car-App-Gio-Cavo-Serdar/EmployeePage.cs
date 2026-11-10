using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Pagina onder de topbalk voor rol 2 (medewerker).
    public class EmployeePage : UserControl
    {
        private readonly TabControl tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Name = nameof(tabs)
        };

        public EmployeePage()
        {
            BackColor = Color.White;
            ForeColor = Color.FromArgb(31, 41, 55);
            Font = new Font("Segoe UI", 9.75F);
            Padding = new Padding(16);

            SuspendLayout();

            AddCommonTabs();

            if (AuthHelper.loggedinroleid == AuthHelper.RoleAdmin)
            {
                AddAdminTabs();
            }

            Controls.Add(tabs);

            if (tabs.TabPages.Count > 0)
            {
                tabs.SelectedIndex = 0;
            }

            ResumeLayout(false);
        }

        private void AddCommonTabs()
        {
            AddTab("Dagoverzicht", new DayOverviewControl());
            AddTab("Planning", new PlanningControl());
            AddTab("Nieuwe reservering", new NewReservationControl());
            AddTab("Ophalen / terugbrengen", new PickupReturnControl());
            AddTab("Wijzigen / annuleren", new ChangeCancelControl());
            AddTab("Klanten", new CustomersControl());
        }

        private void AddAdminTabs()
        {
            AddTab("Extra's en prijzen", new ExtrasPricesControl());
            AddTab("Gebruikers en rollen", new UsersRolesControl());
        }

        // Voegt een UserControl toe als een nieuw tabblad.
        public void AddTab(string title, Control content)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Een tabblad moet een titel hebben.", nameof(title));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            TabPage page = new TabPage(title)
            {
                BackColor = Color.White,
                Padding = new Padding(4)
            };

            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            tabs.TabPages.Add(page);
        }
    }
}