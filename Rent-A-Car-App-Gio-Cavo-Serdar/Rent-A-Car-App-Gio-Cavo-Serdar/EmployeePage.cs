using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Pagina onder de topbalk voor rol 2 (medewerker).
    // Elke functie is een eigen UserControl op een eigen tabblad, zodat functies los van elkaar toegevoegd kunnen worden.
    public class EmployeePage : UserControl
    {
        private readonly TabControl tabs;

        public EmployeePage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Font = new Font("Segoe UI", 9.75F);
            Padding = new Padding(16);

            tabs = new TabControl { Dock = DockStyle.Fill };
            Controls.Add(tabs);

            addTab("Dagoverzicht", new DayOverviewControl());
            addTab("Planning", new PlanningControl());
            // Volgende tabbladen: Nieuwe reservering, Ophalen / terugbrengen, Extra's en prijzen, Klanten.
        }

        private void addTab(string title, UserControl content)
        {
            var page = new TabPage(title) { BackColor = Color.White };
            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            tabs.TabPages.Add(page);
        }
    }
}