using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
            showPage();
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

            pnlContent.Controls.Clear();

            if (DBHelper.loggedinuserid == 0)
            {
                Text = "Rent A Car";
                showHomePage();
                return;
            }

            UserControl page;

            if (AuthHelper.loggedinroleid == AuthHelper.RoleAdmin)
            {
                Text = "Rent A Car - Admin";
                page = new AdminPage();
            }
            else
            {
                Text = "Rent A Car";
                page = new CustomerPage();
            }

            page.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(page);
        }

        private void showHomePage()
        {
            Panel homePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(40, 30, 40, 30)
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 115,
                BackColor = Color.Transparent
            };

            Label titleLabel = new Label
            {
                AutoSize = true,
                Text = "Vind jouw ideale huurauto",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Location = new Point(0, 0)
            };

            Label subtitleLabel = new Label
            {
                AutoSize = true,
                Text = "Bekijk onze beschikbare auto's en kies een auto die bij jou past.",
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(3, 48)
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);

            FlowLayoutPanel carsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 10),
                BackColor = Color.Transparent
            };

            List<CarInfo> availableCars = loadAvailableCars();

            if (availableCars.Count == 0)
            {
                Label noCarsLabel = new Label
                {
                    AutoSize = true,
                    Text = "Er zijn momenteel geen beschikbare auto's.",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Margin = new Padding(5, 20, 0, 0)
                };

                carsPanel.Controls.Add(noCarsLabel);
            }
            else
            {
                foreach (CarInfo car in availableCars)
                {
                    carsPanel.Controls.Add(createCarCard(car));
                }
            }

            homePanel.Controls.Add(carsPanel);
            homePanel.Controls.Add(headerPanel);
            pnlContent.Controls.Add(homePanel);
        }

        private List<CarInfo> loadAvailableCars()
        {
            List<CarInfo> cars = new List<CarInfo>();

            try
            {
                string sql = @"
                    SELECT
                        ID,
                        License_Plate,
                        Brand,
                        Model,
                        Build_Date,
                        Daily_Price,
                        Car_Status
                    FROM car
                    WHERE ID >= 1
                      AND LOWER(Car_Status) = 'active'
                    ORDER BY ID
                    LIMIT 8";

                DataTable carTable = DBHelper.getdataTable(sql);

                foreach (DataRow row in carTable.Rows)
                {
                    cars.Add(new CarInfo
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        LicensePlate = Convert.ToString(row["License_Plate"]),
                        Brand = Convert.ToString(row["Brand"]),
                        Model = Convert.ToString(row["Model"]),
                        BuildDate = formatBuildDate(row["Build_Date"]),
                        DailyPrice = formatDailyPrice(row["Daily_Price"]),
                        CarStatus = Convert.ToString(row["Car_Status"])
                    });
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "De auto's konden niet worden geladen.\n\n" + exception.Message,
                    "Databasefout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return cars;
        }

        private string formatBuildDate(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "Onbekend";
            }

            DateTime buildDate;

            if (DateTime.TryParse(Convert.ToString(value), out buildDate))
            {
                return buildDate.ToString("dd-MM-yyyy");
            }

            return Convert.ToString(value);
        }

        private string formatDailyPrice(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "Onbekend";
            }

            decimal dailyPrice;

            if (decimal.TryParse(
                Convert.ToString(value),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out dailyPrice))
            {
                return dailyPrice.ToString("C2", CultureInfo.GetCultureInfo("nl-NL")) + " per dag";
            }

            return Convert.ToString(value) + " per dag";
        }

        private Panel createCarCard(CarInfo car)
        {
            Panel card = new Panel
            {
                Width = 285,
                Height = 365,
                Margin = new Padding(0, 0, 22, 22),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Tag = car
            };

            Panel imagePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 145,
                BackColor = Color.Black,
                Cursor = Cursors.Hand
            };

            Label imageLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "AFBEELDING",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                BackColor = Color.Black,
                Cursor = Cursors.Hand
            };

            Label brandLabel = new Label
            {
                AutoSize = true,
                Text = car.Brand,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(18, 163),
                Cursor = Cursors.Hand
            };

            Label modelLabel = new Label
            {
                AutoSize = true,
                Text = car.Model,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Location = new Point(17, 183),
                Cursor = Cursors.Hand
            };

            Label informationLabel = new Label
            {
                AutoSize = false,
                Width = 245,
                Height = 65,
                Text =
                    "ID: " + car.Id + Environment.NewLine +
                    "Kenteken: " + car.LicensePlate + Environment.NewLine +
                    "Bouwdatum: " + car.BuildDate,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(18, 222),
                Cursor = Cursors.Hand
            };

            Label priceLabel = new Label
            {
                AutoSize = true,
                Text = car.DailyPrice,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(18, 292),
                Cursor = Cursors.Hand
            };

            Label statusLabel = new Label
            {
                AutoSize = true,
                Text = "Status: " + car.CarStatus,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 163, 74),
                Location = new Point(18, 325),
                Cursor = Cursors.Hand
            };

            imagePanel.Controls.Add(imageLabel);
            card.Controls.Add(statusLabel);
            card.Controls.Add(priceLabel);
            card.Controls.Add(informationLabel);
            card.Controls.Add(modelLabel);
            card.Controls.Add(brandLabel);
            card.Controls.Add(imagePanel);

            addClickHandlerToControls(card, car);

            return card;
        }

        private void addClickHandlerToControls(Control control, CarInfo car)
        {
            control.Click += (sender, e) => showCarDetails(car);

            foreach (Control childControl in control.Controls)
            {
                addClickHandlerToControls(childControl, car);
            }
        }

        private void showCarDetails(CarInfo car)
        {
            Panel detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(50)
            };

            Button backButton = new Button
            {
                Text = "? Terug naar auto's",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(31, 41, 55),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(50, 35),
                Cursor = Cursors.Hand
            };

            backButton.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);

            // Laad de HomeForm opnieuw, zodat de gebruiker terugkomt op het startscherm.
            backButton.Click += (sender, e) => showPage();

            Panel imagePanel = new Panel
            {
                Width = 420,
                Height = 280,
                BackColor = Color.Black,
                Location = new Point(50, 100)
            };

            Label imageLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "AFBEELDING",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                BackColor = Color.Black
            };

            imagePanel.Controls.Add(imageLabel);

            Label titleLabel = new Label
            {
                AutoSize = true,
                Text = car.Brand + " " + car.Model,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Location = new Point(510, 100)
            };

            Label detailsLabel = new Label
            {
                AutoSize = false,
                Width = 480,
                Height = 180,
                Text =
                    "ID: " + car.Id + Environment.NewLine +
                    "Kenteken: " + car.LicensePlate + Environment.NewLine +
                    "Merk: " + car.Brand + Environment.NewLine +
                    "Model: " + car.Model + Environment.NewLine +
                    "Bouwdatum: " + car.BuildDate + Environment.NewLine +
                    "Huurprijs: " + car.DailyPrice + Environment.NewLine +
                    "Status: " + car.CarStatus,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(510, 165)
            };

            Button rentButton = new Button
            {
                Text = "Inhuren",
                Width = 150,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(510, 365),
                Cursor = Cursors.Hand
            };

            rentButton.FlatAppearance.BorderSize = 0;

            // De button krijgt voorlopig nog geen functionaliteit.
            detailsPanel.Controls.Add(backButton);
            detailsPanel.Controls.Add(imagePanel);
            detailsPanel.Controls.Add(titleLabel);
            detailsPanel.Controls.Add(detailsLabel);
            detailsPanel.Controls.Add(rentButton);

            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(detailsPanel);
        }

        private sealed class CarInfo
        {
            public int Id { get; set; }

            public string LicensePlate { get; set; }

            public string Brand { get; set; }

            public string Model { get; set; }

            public string BuildDate { get; set; }

            public string DailyPrice { get; set; }

            public string CarStatus { get; set; }
        }
    }
}
