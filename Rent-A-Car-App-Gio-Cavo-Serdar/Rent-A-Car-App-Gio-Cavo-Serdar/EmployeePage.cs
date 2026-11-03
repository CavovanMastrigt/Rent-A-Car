using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Page shown under the top bar for role 2 (employee).
    // Every feature is its own UserControl on its own tab, so features can be added without touching each other.
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

            addTab("Day overview", new DayOverviewControl());
            // Next tabs: Planning, New reservation, Pick-up / return, Extras & prices, Customers.
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
