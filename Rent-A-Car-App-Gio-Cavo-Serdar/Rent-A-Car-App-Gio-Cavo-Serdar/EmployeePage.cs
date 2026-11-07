
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    public class EmployeePage : UserControl
    {
        private readonly TabControl tabs;

        public EmployeePage()
        {
            BackColor = UITheme.Background;
            Font = UITheme.Body;
            Padding = new Padding(20);

            // Hoofdcontainer
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UITheme.Background
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 85));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            // Paginatitel
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.Background
            };

            var lblTitle = new Label
            {
                Text = "Medewerkersdashboard",
                Font = UITheme.Heading,
                ForeColor = UITheme.Text,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblSubtitle = new Label
            {
                Text = "Beheer dagelijkse verhuuractiviteiten en reserveringen.",
                Font = UITheme.SubHeading,
                ForeColor = UITheme.Muted,
                AutoSize = true,
                Location = new Point(2, 38)
            };

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSubtitle);

            // Tabbladen
            tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = UITheme.Button,
                BackColor = UITheme.Surface,
                Padding = new Point(14, 7)
            };

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(tabs, 0, 1);

            Controls.Add(layout);

            addTab("Dagoverzicht", new DayOverviewControl());
            addTab("Planning", new PlanningControl());
            addTab("Nieuwe reservering", new NewReservationControl());
            addTab("Ophalen / terugbrengen", new PickupReturnControl());
            addTab("Wijzigen / annuleren", new ChangeCancelControl());
            addTab("Extra's en prijzen", new ExtrasPricesControl());
        }

        private void addTab(string title, UserControl content)
        {
            var page = new TabPage(title)
            {
                BackColor = UITheme.Surface,
                Padding = new Padding(8)
            };

            content.Dock = DockStyle.Fill;

            page.Controls.Add(content);
            tabs.TabPages.Add(page);
        }
    }
}
