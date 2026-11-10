using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    public partial class CustomerPage : UserControl
    {
        private readonly Color backgroundColor = Color.FromArgb(243, 244, 246);
        private readonly Color darkColor = Color.FromArgb(31, 41, 55);
        private readonly Color mutedColor = Color.FromArgb(107, 114, 128);
        private readonly Color blueColor = Color.FromArgb(37, 99, 235);

        private CarInfo selectedCar;
        private DateTimePicker startDatePicker;
        private DateTimePicker endDatePicker;
        private CheckedListBox optionsList;
        private Label totalPriceLabel;

        public CustomerPage()
        {
            InitializeComponent();
            showOverview();
        }

        private void showOverview()
        {
            clearPage();

            Panel pagePanel = createPagePanel();

            Panel headerPanel = createHeader(
                "Vind jouw ideale huurauto",
                "Bekijk beschikbare auto's en maak direct een reservering.");

            FlowLayoutPanel carsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 10),
                BackColor = Color.Transparent
            };

            List<CarInfo> cars = loadAvailableCars();

            if (cars.Count == 0)
            {
                carsPanel.Controls.Add(createMessageLabel(
                    "Er zijn momenteel geen beschikbare auto's."));
            }
            else
            {
                foreach (CarInfo car in cars)
                {
                    carsPanel.Controls.Add(createCarCard(car));
                }
            }

            pagePanel.Controls.Add(carsPanel);

            Panel banner = createVerificationBanner();
            if (banner != null)
            {
                pagePanel.Controls.Add(banner);
            }

            pagePanel.Controls.Add(headerPanel);
            Controls.Add(pagePanel);
        }

        // Shows an ID-verification notice above the car overview until the customer
        // is verified (user story 8).
        private Panel createVerificationBanner()
        {
            string status = VerificationHelper.GetStatus(DBHelper.loggedinuserid);
            if (status == VerificationHelper.StatusVerified)
            {
                return null;
            }

            Color bg;
            string message;
            bool showButton = true;
            if (status == VerificationHelper.StatusPending)
            {
                bg = Color.FromArgb(254, 249, 195);
                message = "Je ID-kaart is in behandeling. Je kunt pas een auto huren nadat deze is goedgekeurd.";
                showButton = false;
            }
            else if (status == VerificationHelper.StatusFailed)
            {
                bg = Color.FromArgb(254, 226, 226);
                message = "Je ID-verificatie is afgekeurd. Upload een nieuwe ID-kaart om een auto te kunnen huren.";
            }
            else
            {
                bg = Color.FromArgb(254, 226, 226);
                message = "Je account is nog niet geverifieerd. Verifieer je ID-kaart om een auto te kunnen huren.";
            }

            Panel banner = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = bg, Padding = new Padding(16, 8, 16, 8) };
            Label label = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F),
                ForeColor = darkColor
            };
            banner.Controls.Add(label);

            if (showButton)
            {
                Button verify = new Button
                {
                    Text = "Verifieer nu",
                    Dock = DockStyle.Right,
                    Width = 150,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = blueColor,
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                verify.FlatAppearance.BorderSize = 0;
                verify.Click += (s, e) =>
                {
                    if (PopupForm.show(FindForm(), new IdentityVerificationForm()) == DialogResult.OK)
                    {
                        showOverview();
                    }
                };
                banner.Controls.Add(verify);
                verify.BringToFront();
            }

            return banner;
        }

        private List<CarInfo> loadAvailableCars()
        {
            List<CarInfo> cars = new List<CarInfo>();

            try
            {
                DataTable table = DBHelper.getdataTable(@"
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
                    LIMIT 8");

                foreach (DataRow row in table.Rows)
                {
                    cars.Add(new CarInfo
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        LicensePlate = Convert.ToString(row["License_Plate"]),
                        Brand = Convert.ToString(row["Brand"]),
                        Model = Convert.ToString(row["Model"]),
                        BuildDate = formatDate(row["Build_Date"]),
                        DailyPrice = Convert.ToDecimal(row["Daily_Price"]),
                        Status = Convert.ToString(row["Car_Status"])
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

        private Panel createCarCard(CarInfo car)
        {
            Panel card = new Panel
            {
                Width = 285,
                Height = 365,
                Margin = new Padding(0, 0, 22, 22),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
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

            Label brandLabel = createLabel(
                car.Brand,
                new Point(18, 163),
                new Font("Segoe UI", 10F, FontStyle.Bold),
                mutedColor);

            Label modelLabel = createLabel(
                car.Model,
                new Point(17, 183),
                new Font("Segoe UI", 16F, FontStyle.Bold),
                darkColor);

            Label informationLabel = new Label
            {
                AutoSize = false,
                Width = 245,
                Height = 68,
                Text =
                    "ID: " + car.Id + Environment.NewLine +
                    "Kenteken: " + car.LicensePlate + Environment.NewLine +
                    "Bouwdatum: " + car.BuildDate,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(18, 222),
                Cursor = Cursors.Hand
            };

            Label priceLabel = createLabel(
                formatPrice(car.DailyPrice) + " per dag",
                new Point(18, 295),
                new Font("Segoe UI", 11F, FontStyle.Bold),
                blueColor);

            Label statusLabel = createLabel(
                "Status: " + car.Status,
                new Point(18, 328),
                new Font("Segoe UI", 9F, FontStyle.Bold),
                Color.FromArgb(22, 163, 74));

            imagePanel.Controls.Add(imageLabel);
            card.Controls.Add(statusLabel);
            card.Controls.Add(priceLabel);
            card.Controls.Add(informationLabel);
            card.Controls.Add(modelLabel);
            card.Controls.Add(brandLabel);
            card.Controls.Add(imagePanel);

            addClickHandler(card, car);

            return card;
        }

        private void addClickHandler(Control control, CarInfo car)
        {
            control.Click += (sender, e) => showCarDetails(car);

            foreach (Control child in control.Controls)
            {
                addClickHandler(child, car);
            }
        }

        private void showCarDetails(CarInfo car)
        {
            clearPage();

            Panel pagePanel = createPagePanel();

            Button backButton = createButton(
                "? Terug naar auto's",
                new Size(170, 38),
                new Point(40, 30),
                Color.White,
                darkColor);

            backButton.Click += (sender, e) => showOverview();

            Panel imagePanel = new Panel
            {
                Width = 420,
                Height = 280,
                BackColor = Color.Black,
                Location = new Point(40, 95)
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

            Label titleLabel = createLabel(
                car.Brand + " " + car.Model,
                new Point(510, 95),
                new Font("Segoe UI", 26F, FontStyle.Bold),
                darkColor);

            Label detailsLabel = new Label
            {
                AutoSize = false,
                Width = 500,
                Height = 220,
                Text =
                    "ID: " + car.Id + Environment.NewLine +
                    "Kenteken: " + car.LicensePlate + Environment.NewLine +
                    "Merk: " + car.Brand + Environment.NewLine +
                    "Model: " + car.Model + Environment.NewLine +
                    "Bouwdatum: " + car.BuildDate + Environment.NewLine +
                    "Huurprijs: " + formatPrice(car.DailyPrice) + " per dag" + Environment.NewLine +
                    "Status: " + car.Status,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(510, 160)
            };

            Button rentButton = createButton(
                "Inhuren",
                new Size(150, 45),
                new Point(510, 385),
                blueColor,
                Color.White);

            rentButton.Click += (sender, e) => showReservationPage(car);

            pagePanel.Controls.Add(backButton);
            pagePanel.Controls.Add(imagePanel);
            pagePanel.Controls.Add(titleLabel);
            pagePanel.Controls.Add(detailsLabel);
            pagePanel.Controls.Add(rentButton);

            Controls.Add(pagePanel);
        }

        private void showReservationPage(CarInfo car)
        {
            selectedCar = car;
            clearPage();

            Panel pagePanel = createPagePanel();

            Button backButton = createButton(
                "? Terug naar auto",
                new Size(160, 38),
                new Point(40, 25),
                Color.White,
                darkColor);

            backButton.Click += (sender, e) => showCarDetails(car);

            Label titleLabel = createLabel(
                "Auto reserveren",
                new Point(40, 80),
                new Font("Segoe UI", 24F, FontStyle.Bold),
                darkColor);

            Label carLabel = createLabel(
                car.Brand + " " + car.Model + " - " + formatPrice(car.DailyPrice) + " per dag",
                new Point(43, 125),
                new Font("Segoe UI", 11F, FontStyle.Bold),
                mutedColor);

            Label startLabel = createLabel(
                "Ophaalmoment",
                new Point(43, 180),
                new Font("Segoe UI", 10F, FontStyle.Bold),
                darkColor);

            startDatePicker = createDatePicker(
                new Point(40, 210),
                DateTime.Now.AddDays(1).Date.AddHours(9));

            Label endLabel = createLabel(
                "Terugbrengmoment",
                new Point(43, 265),
                new Font("Segoe UI", 10F, FontStyle.Bold),
                darkColor);

            endDatePicker = createDatePicker(
                new Point(40, 295),
                DateTime.Now.AddDays(2).Date.AddHours(9));

            Label optionsLabel = createLabel(
                "Extra opties",
                new Point(360, 180),
                new Font("Segoe UI", 10F, FontStyle.Bold),
                darkColor);

            optionsList = new CheckedListBox
            {
                Location = new Point(360, 210),
                Width = 300,
                Height = 150,
                Font = new Font("Segoe UI", 10F),
                CheckOnClick = true
            };

            loadOptions();

            optionsList.ItemCheck += (sender, e) =>
            {
                BeginInvoke(new Action(updateTotalPrice));
            };

            totalPriceLabel = createLabel(
                "Totaalprijs: " + formatPrice(car.DailyPrice),
                new Point(360, 380),
                new Font("Segoe UI", 14F, FontStyle.Bold),
                blueColor);

            Button reserveButton = createButton(
                "Reservering plaatsen",
                new Size(210, 45),
                new Point(40, 380),
                blueColor,
                Color.White);

            reserveButton.Click += (sender, e) => saveReservation();

            pagePanel.Controls.Add(backButton);
            pagePanel.Controls.Add(titleLabel);
            pagePanel.Controls.Add(carLabel);
            pagePanel.Controls.Add(startLabel);
            pagePanel.Controls.Add(startDatePicker);
            pagePanel.Controls.Add(endLabel);
            pagePanel.Controls.Add(endDatePicker);
            pagePanel.Controls.Add(optionsLabel);
            pagePanel.Controls.Add(optionsList);
            pagePanel.Controls.Add(totalPriceLabel);
            pagePanel.Controls.Add(reserveButton);

            Controls.Add(pagePanel);
            updateTotalPrice();
        }

        private void loadOptions()
        {
            try
            {
                DataTable table = DBHelper.getdataTable(@"
                    SELECT ID, Name, Description, Price
                    FROM extra_option
                    WHERE Is_Active = 1
                    ORDER BY Name");

                foreach (DataRow row in table.Rows)
                {
                    optionsList.Items.Add(new ExtraOption
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Name = Convert.ToString(row["Name"]),
                        Description = Convert.ToString(row["Description"]),
                        Price = Convert.ToDecimal(row["Price"])
                    });
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "De extra opties konden niet worden geladen.\n\n" + exception.Message,
                    "Databasefout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void updateTotalPrice()
        {
            if (selectedCar == null || startDatePicker == null || endDatePicker == null)
            {
                return;
            }

            double rentalDays = Math.Ceiling(
                (endDatePicker.Value - startDatePicker.Value).TotalDays);

            if (rentalDays < 1)
            {
                rentalDays = 1;
            }

            decimal optionsPrice = 0;

            if (optionsList != null)
            {
                foreach (ExtraOption option in optionsList.CheckedItems)
                {
                    optionsPrice += option.Price;
                }
            }

            decimal totalPrice =
                selectedCar.DailyPrice * Convert.ToDecimal(rentalDays) + optionsPrice;

            totalPriceLabel.Text = "Totaalprijs: " + formatPrice(totalPrice);
        }

        private void saveReservation()
        {
            // User story 8: a customer whose ID is not verified cannot rent a car.
            if (!VerificationHelper.IsVerified(DBHelper.loggedinuserid))
            {
                MessageBox.Show(
                    "Je account is nog niet geverifieerd. Verifieer eerst je ID-kaart voordat je een auto huurt.",
                    "Niet geverifieerd",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DateTime start = startDatePicker.Value;
            DateTime end = endDatePicker.Value;

            if (end <= start)
            {
                MessageBox.Show(
                    "Het terugbrengmoment moet na het ophaalmoment liggen.",
                    "Ongeldige periode",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!isWithinOpeningHours(start) || !isWithinOpeningHours(end))
            {
                MessageBox.Show(
                    "Het ophaal- en terugbrengmoment moeten binnen de openingstijden vallen.",
                    "Ongeldige tijd",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!isCarAvailable(selectedCar.Id, start, end))
                {
                    MessageBox.Show(
                        "Deze auto is in deze periode niet meer beschikbaar.",
                        "Auto niet beschikbaar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                decimal totalPrice = calculateTotalPrice(start, end);

                string insertSql =
                    "INSERT INTO rental " +
                    "(UserID, CarID, Price_Per_Day, Date_Started, Date_Planned_Ending, " +
                    "Date_Ended, notes, Rental_Status) VALUES (" +
                    DBHelper.loggedinuserid + ", " +
                    selectedCar.Id + ", " +
                    selectedCar.DailyPrice.ToString(CultureInfo.InvariantCulture) + ", " +
                    AuthHelper.sql(start.ToString("yyyy-MM-dd HH:mm:ss")) + ", " +
                    AuthHelper.sql(end.ToString("yyyy-MM-dd HH:mm:ss")) + ", " +
                    "NULL, " +
                    AuthHelper.sql("Totaalprijs: " + totalPrice.ToString(CultureInfo.InvariantCulture)) + ", " +
                    "'reserved')";

                DBHelper.RunSql(insertSql);

                int rentalId = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT LAST_INSERT_ID()"));

                foreach (ExtraOption option in optionsList.CheckedItems)
                {
                    DBHelper.RunSql(
                        "INSERT INTO rental_option " +
                        "(Rental_ID, Option_ID, Option_Price) VALUES (" +
                        rentalId + ", " +
                        option.Id + ", " +
                        option.Price.ToString(CultureInfo.InvariantCulture) + ")");
                }

                MessageBox.Show(
                    "De reservering is succesvol opgeslagen.",
                    "Reservering bevestigd",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                showMyReservations();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "De reservering kon niet worden opgeslagen.\n\n" + exception.Message,
                    "Databasefout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool isCarAvailable(int carId, DateTime start, DateTime end)
        {
            object result = DBHelper.getScalar(
                "SELECT COUNT(*) FROM rental " +
                "WHERE CarID = " + carId +
                " AND Rental_Status IN ('reserved', 'confirmed', 'ongoing') " +
                " AND Date_Started < " + AuthHelper.sql(end.ToString("yyyy-MM-dd HH:mm:ss")) +
                " AND Date_Planned_Ending > " + AuthHelper.sql(start.ToString("yyyy-MM-dd HH:mm:ss")));

            return Convert.ToInt32(result) == 0;
        }

        private bool isWithinOpeningHours(DateTime dateTime)
        {
            string weekday = dateTime.DayOfWeek.ToString();

            DataTable table = DBHelper.getdataTable(
                "SELECT Time_Open, Time_Closed " +
                "FROM openingtimes " +
                "WHERE Weekday = " + AuthHelper.sql(weekday));

            if (table.Rows.Count == 0)
            {
                return false;
            }

            TimeSpan openTime = TimeSpan.Parse(table.Rows[0]["Time_Open"].ToString());
            TimeSpan closeTime = TimeSpan.Parse(table.Rows[0]["Time_Closed"].ToString());

            return dateTime.TimeOfDay >= openTime &&
                   dateTime.TimeOfDay <= closeTime;
        }

        private decimal calculateTotalPrice(DateTime start, DateTime end)
        {
            decimal days = Convert.ToDecimal(Math.Ceiling(
                (end - start).TotalDays));

            decimal optionsPrice = 0;

            foreach (ExtraOption option in optionsList.CheckedItems)
            {
                optionsPrice += option.Price;
            }

            return selectedCar.DailyPrice * days + optionsPrice;
        }

        private void showMyReservations()
        {
            clearPage();

            Panel pagePanel = createPagePanel();

            Button backButton = createButton(
                "? Terug naar auto's",
                new Size(170, 38),
                new Point(40, 30),
                Color.White,
                darkColor);

            backButton.Click += (sender, e) => showOverview();

            Label titleLabel = createLabel(
                "Mijn reserveringen",
                new Point(40, 85),
                new Font("Segoe UI", 24F, FontStyle.Bold),
                darkColor);

            DataGridView reservationsGrid = new DataGridView
            {
                Location = new Point(40, 145),
                Width = 900,
                Height = 400,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            try
            {
                reservationsGrid.DataSource = DBHelper.getdataTable(@"
                    SELECT
                        rental.ID AS Reservering,
                        CONCAT(car.Brand, ' ', car.Model) AS Auto,
                        rental.Date_Started AS Ophalen,
                        rental.Date_Planned_Ending AS Terugbrengen,
                        rental.Price_Per_Day AS Prijs_Per_Dag,
                        rental.Rental_Status AS Status
                    FROM rental
                    INNER JOIN car ON car.ID = rental.CarID
                    WHERE rental.UserID = " + DBHelper.loggedinuserid + @"
                    ORDER BY rental.Date_Started DESC");
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "De reserveringen konden niet worden geladen.\n\n" + exception.Message,
                    "Databasefout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            pagePanel.Controls.Add(backButton);
            pagePanel.Controls.Add(titleLabel);
            pagePanel.Controls.Add(reservationsGrid);

            Controls.Add(pagePanel);
        }

        private Panel createHeader(string title, string subtitle)
        {
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 115,
                BackColor = Color.Transparent
            };

            Label titleLabel = createLabel(
                title,
                new Point(0, 0),
                new Font("Segoe UI", 24F, FontStyle.Bold),
                darkColor);

            Label subtitleLabel = createLabel(
                subtitle,
                new Point(3, 48),
                new Font("Segoe UI", 11F),
                mutedColor);

            Button accountButton = createButton(
                "Account",
                new Size(120, 38),
                new Point(720, 10),
                Color.White,
                darkColor);

            accountButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            accountButton.Left = 800;

            accountButton.Click += (sender, e) => openAccount();

            Button reservationsButton = createButton(
                "Mijn reserveringen",
                new Size(170, 38),
                new Point(600, 58),
                blueColor,
                Color.White);

            reservationsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            reservationsButton.Left = 750;

            reservationsButton.Click += (sender, e) => showMyReservations();

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Controls.Add(accountButton);
            headerPanel.Controls.Add(reservationsButton);

            return headerPanel;
        }

        private void openAccount()
        {
            using (ProfileForm profileForm = new ProfileForm())
            {
                if (PopupForm.show(FindForm(), profileForm) == DialogResult.OK)
                {
                    Invalidate();
                }
            }
        }

        private Panel createPagePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backgroundColor,
                Padding = new Padding(40, 25, 40, 25)
            };
        }

        private Label createLabel(
            string text,
            Point location,
            Font font,
            Color color)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                Location = location,
                Font = font,
                ForeColor = color,
                Cursor = Cursors.Hand
            };
        }

        private Button createButton(
            string text,
            Size size,
            Point location,
            Color backColor,
            Color foreColor)
        {
            Button button = new Button
            {
                Text = text,
                Size = size,
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            button.FlatAppearance.BorderSize = 1;

            if (backColor == blueColor)
            {
                button.FlatAppearance.BorderSize = 0;
            }

            return button;
        }

        private DateTimePicker createDatePicker(Point location, DateTime value)
        {
            DateTimePicker picker = new DateTimePicker
            {
                Location = location,
                Width = 260,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd-MM-yyyy HH:mm",
                ShowUpDown = true,
                MinDate = DateTime.Now,
                Value = value
            };

            picker.ValueChanged += (sender, e) => updateTotalPrice();

            return picker;
        }

        private Label createMessageLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", 12F),
                ForeColor = mutedColor,
                Margin = new Padding(5, 20, 0, 0)
            };
        }

        private string formatDate(object value)
        {
            DateTime date;

            if (value != null &&
                value != DBNull.Value &&
                DateTime.TryParse(Convert.ToString(value), out date))
            {
                return date.ToString("dd-MM-yyyy");
            }

            return "Onbekend";
        }

        private string formatPrice(decimal price)
        {
            return price.ToString(
                "C2",
                CultureInfo.GetCultureInfo("nl-NL"));
        }

        private void clearPage()
        {
            foreach (Control control in Controls.Cast<Control>().ToList())
            {
                control.Dispose();
            }

            Controls.Clear();
        }

        private sealed class CarInfo
        {
            public int Id { get; set; }

            public string LicensePlate { get; set; }

            public string Brand { get; set; }

            public string Model { get; set; }

            public string BuildDate { get; set; }

            public decimal DailyPrice { get; set; }

            public string Status { get; set; }
        }

        private sealed class ExtraOption
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public decimal Price { get; set; }

            public override string ToString()
            {
                return Name + " - " + priceText();
            }

            private string priceText()
            {
                return Price.ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("nl-NL"));
            }
        }
    }
}
