
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    public class EmployeePage : UserControl
    {
        private readonly Panel sidebar;
        private readonly Panel contentPanel;
        private Button activeButton;

        public EmployeePage()
        {
            BackColor = UITheme.Background;
            Padding = new Padding(15);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UITheme.Background
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 220));

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.Background,
                Padding = new Padding(20)
            };

            var lblLogo = new Label
            {
                Text = "RENT A CAR",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = UITheme.Primary,
                Dock = DockStyle.Top,
                Height = 65,
                TextAlign = ContentAlignment.MiddleCenter
            };

            sidebar.Controls.Add(lblLogo);

            layout.Controls.Add(sidebar, 0, 0);
            layout.Controls.Add(contentPanel, 1, 0);

            Controls.Add(layout);

            AddMenuButton("Dagoverzicht",
                () => new DayOverviewControl());

            AddMenuButton("Planning",
                () => new PlanningControl());

            AddMenuButton("Nieuwe reservering",
                () => new NewReservationControl());

            AddMenuButton("Ophalen / terugbrengen",
                () => new PickupReturnControl());

            AddMenuButton("Wijzigen / annuleren",
                () => new ChangeCancelControl());

            AddMenuButton("Extra's en prijzen",
                () => new ExtrasPricesControl());
        }

        private void AddMenuButton(
            string title,
            Func<UserControl> createPage)
        {
            var button = new Button
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 48,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Font = UITheme.Body,
                BackColor = Color.White,
                ForeColor = UITheme.Text,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;

            button.Click += (s, e) =>
            {
                if (activeButton != null)
                {
                    activeButton.BackColor = Color.White;
                    activeButton.ForeColor = UITheme.Text;
                }

                activeButton = button;
                button.BackColor = UITheme.Selection;
                button.ForeColor = UITheme.Primary;

                contentPanel.Controls.Clear();

                UserControl page = createPage();
                page.Dock = DockStyle.Fill;

                contentPanel.Controls.Add(page);
            };

            sidebar.Controls.Add(button);
            sidebar.Controls.SetChildIndex(button, 0);

            // Open eerste pagina automatisch.
            if (activeButton == null)
                button.PerformClick();
        }
    }
}
