using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Medewerkerscherm (user story 3): een reservering maken namens een klant.
    // Stappen: klant zoeken, periode kiezen, een vrije auto kiezen, extra's aanvinken en opslaan.
    // Controles: niet in het verleden, niet verder dan een jaar vooruit, binnen de openingstijden,
    // auto vrij in de periode en klant actief.
    public class NewReservationControl : UserControl
    {
        private static readonly Color AccentColor = Color.FromArgb(79, 70, 229);
        private static readonly Color HeaderColor = Color.FromArgb(243, 244, 246);
        private static readonly Color TextColor = Color.FromArgb(31, 41, 55);
        private static readonly Color MutedColor = Color.FromArgb(107, 114, 128);
        private static readonly Color ErrorColor = Color.FromArgb(185, 28, 28);

        private static readonly CultureInfo Dutch = new CultureInfo("nl-NL");

        // Betaaltermijn van de factuur in dagen.
        private const int InvoiceDays = 14;

        private class CustomerItem
        {
            public int Id;
            public string Name;
            public string Email;
            public string Status;

            public override string ToString()
            {
                return Name + " (" + Email + ")";
            }
        }

        private class CarItem
        {
            public int Id;
            public string Name;
            public decimal Price;

            public override string ToString()
            {
                return Name + " - " + Price.ToString("C", Dutch) + " per dag";
            }
        }

        private class ExtraItem
        {
            public int Id;
            public string Name;
            public decimal Price;

            public override string ToString()
            {
                return Name + " - " + Price.ToString("C", Dutch);
            }
        }

        private readonly TextBox txtSearch;
        private readonly ComboBox cmbCustomer;
        private readonly Label lblCustomerInfo;
        private readonly DateTimePicker dtpStart;
        private readonly DateTimePicker dtpEnd;
        private readonly Label lblPeriodInfo;
        private readonly ComboBox cmbCar;
        private readonly Label lblCarInfo;
        private readonly CheckedListBox clbExtras;
        private readonly TextBox txtNotes;
        private readonly Label lblSummary;
        private readonly Label lblTotal;
        private readonly Label lblMessage;
        private readonly Button btnSave;

        private bool ready = false;
        private bool adjusting = false;

        public NewReservationControl()
        {
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.75F);

            var form = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Controls.Add(form);

            // 1. Klant
            form.Controls.Add(makeHeading("1. Klant", 24, 16));
            txtSearch = new TextBox { Location = new Point(24, 46), Size = new Size(296, 27) };
            Button btnSearch = makeButton("Zoeken", 328, 44, 94);
            cmbCustomer = new ComboBox
            {
                Location = new Point(24, 84),
                Size = new Size(398, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            lblCustomerInfo = makeLabel(24, 114, 398, 22, MutedColor);
            form.Controls.Add(txtSearch);
            form.Controls.Add(btnSearch);
            form.Controls.Add(cmbCustomer);
            form.Controls.Add(lblCustomerInfo);

            // 2. Periode
            form.Controls.Add(makeHeading("2. Periode", 24, 150));
            form.Controls.Add(makeCaption("Ophalen", 24, 182));
            dtpStart = makePicker(24, 204);
            form.Controls.Add(dtpStart);
            form.Controls.Add(makeCaption("Terugbrengen", 24, 240));
            dtpEnd = makePicker(24, 262);
            form.Controls.Add(dtpEnd);
            lblPeriodInfo = makeLabel(24, 296, 398, 40, MutedColor);
            form.Controls.Add(lblPeriodInfo);

            dtpStart.Value = defaultStart();
            dtpEnd.Value = defaultStart().AddDays(1);

            // 3. Auto
            form.Controls.Add(makeHeading("3. Auto", 24, 344));
            cmbCar = new ComboBox
            {
                Location = new Point(24, 374),
                Size = new Size(398, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            lblCarInfo = makeLabel(24, 404, 398, 22, MutedColor);
            form.Controls.Add(cmbCar);
            form.Controls.Add(lblCarInfo);

            // 4. Extra's
            form.Controls.Add(makeHeading("4. Extra's", 24, 440));
            clbExtras = new CheckedListBox
            {
                Location = new Point(24, 470),
                Size = new Size(398, 84),
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            form.Controls.Add(clbExtras);

            // 5. Opmerking
            form.Controls.Add(makeHeading("5. Opmerking (optioneel)", 24, 566));
            txtNotes = new TextBox
            {
                Location = new Point(24, 596),
                Size = new Size(398, 56),
                Multiline = true,
                MaxLength = 255
            };
            form.Controls.Add(txtNotes);

            // Samenvatting rechts
            var card = new Panel
            {
                Location = new Point(470, 16),
                Size = new Size(420, 500),
                BackColor = Color.FromArgb(249, 250, 251)
            };
            card.Controls.Add(makeHeading("Samenvatting", 20, 16));
            lblSummary = new Label
            {
                Location = new Point(20, 52),
                Size = new Size(380, 250),
                ForeColor = TextColor
            };
            lblTotal = new Label
            {
                Location = new Point(20, 310),
                Size = new Size(380, 36),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = AccentColor
            };
            lblMessage = new Label
            {
                Location = new Point(20, 356),
                Size = new Size(380, 60),
                ForeColor = ErrorColor
            };
            btnSave = makeButton("Reservering opslaan", 20, 432, 380);
            btnSave.Height = 44;
            btnSave.BackColor = AccentColor;
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            card.Controls.Add(lblSummary);
            card.Controls.Add(lblTotal);
            card.Controls.Add(lblMessage);
            card.Controls.Add(btnSave);
            form.Controls.Add(card);

            // Gebeurtenissen koppelen (pas nadat de beginwaarden gezet zijn).
            btnSearch.Click += (s, e) => searchCustomers();
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    searchCustomers();
                }
            };
            cmbCustomer.SelectedIndexChanged += (s, e) => updateSummary();
            cmbCar.SelectedIndexChanged += (s, e) => updateSummary();
            clbExtras.ItemCheck += (s, e) => BeginInvoke(new Action(updateSummary));
            dtpStart.ValueChanged += (s, e) =>
            {
                if (!ready || adjusting)
                {
                    return;
                }

                adjusting = true;
                if (pickedEnd() <= pickedStart())
                {
                    dtpEnd.Value = pickedStart().AddDays(1);
                }
                adjusting = false;

                refreshCars();
                updateSummary();
            };
            dtpEnd.ValueChanged += (s, e) =>
            {
                if (!ready || adjusting)
                {
                    return;
                }

                refreshCars();
                updateSummary();
            };
            btnSave.Click += (s, e) => save();

            // Beschikbaarheid, extra's en openingstijden kunnen op andere tabbladen veranderen.
            VisibleChanged += (s, e) =>
            {
                if (ready && Visible)
                {
                    loadOpeningTimes();
                    loadExtras();
                    refreshCars();
                    updateSummary();
                }
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)
            {
                return;
            }

            ready = true;
            loadOpeningTimes();
            loadExtras();
            searchCustomers();
            refreshCars();
            updateSummary();
        }

        // ---------- Hulpmethodes voor het opbouwen van het scherm ----------

        private static Label makeHeading(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextColor
            };
        }

        private static Label makeCaption(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = MutedColor
            };
        }

        private static Label makeLabel(int x, int y, int width, int height, Color color)
        {
            return new Label
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                ForeColor = color
            };
        }

        private static Button makeButton(string text, int x, int y, int width)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 30),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = HeaderColor,
                ForeColor = TextColor
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static DateTimePicker makePicker(int x, int y)
        {
            return new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dddd d MMMM yyyy  HH:mm",
                Location = new Point(x, y),
                Size = new Size(398, 27)
            };
        }

        // ---------- Datums en tijden ----------

        private static DateTime defaultStart()
        {
            return DateTime.Today.AddDays(1).AddHours(9);
        }

        private static DateTime minutePrecision(DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        private DateTime pickedStart()
        {
            return minutePrecision(dtpStart.Value);
        }

        private DateTime pickedEnd()
        {
            return minutePrecision(dtpEnd.Value);
        }

        private static string sqlDateTime(DateTime value)
        {
            return "'" + value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
        }

        private static string sqlDecimal(decimal value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        // Verhuur per begonnen dag van 24 uur, minimaal één dag.
        private static int countDays(DateTime start, DateTime end)
        {
            double days = Math.Ceiling((end - start).TotalMinutes / 1440.0);
            return (int)Math.Max(1, days);
        }

        // ---------- Gegevens laden ----------

        private void searchCustomers()
        {
            string where = "WHERE RID = " + AuthHelper.RoleCustomer + " ";
            string text = txtSearch.Text.Trim();
            if (text.Length > 0)
            {
                string pattern = AuthHelper.sql("%" + text + "%");
                where += "AND (Name LIKE " + pattern + " OR LastName LIKE " + pattern +
                         " OR Email LIKE " + pattern + " OR CONCAT(Name, ' ', LastName) LIKE " + pattern + ") ";
            }

            try
            {
                DataTable table = DBHelper.getdataTable(
                    "SELECT ID, Name, LastName, Email, User_Status FROM user " + where +
                    "ORDER BY LastName, Name LIMIT 50");

                cmbCustomer.Items.Clear();
                foreach (DataRow row in table.Rows)
                {
                    cmbCustomer.Items.Add(new CustomerItem
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Name = Convert.ToString(row["Name"]) + " " + Convert.ToString(row["LastName"]),
                        Email = Convert.ToString(row["Email"]),
                        Status = Convert.ToString(row["User_Status"])
                    });
                }

                if (table.Rows.Count == 1)
                {
                    cmbCustomer.SelectedIndex = 0;
                }

                lblCustomerInfo.Text = table.Rows.Count == 0
                    ? "Geen klanten gevonden. De klant moet zich eerst registreren."
                    : table.Rows.Count + " klant(en) gevonden.";
            }
            catch (Exception ex)
            {
                lblCustomerInfo.Text = "De klanten konden niet worden geladen: " + ex.Message;
            }

            updateSummary();
        }

        private void loadExtras()
        {
            // Onthoud welke extra's aangevinkt waren, zodat een herlaad de keuze niet wist.
            var checkedIds = new HashSet<int>();
            foreach (object item in clbExtras.CheckedItems)
            {
                checkedIds.Add(((ExtraItem)item).Id);
            }

            try
            {
                DataTable table = DBHelper.getdataTable(
                    "SELECT ID, Name, Price FROM extra_option WHERE Is_Active = 1 ORDER BY Name");

                clbExtras.Items.Clear();
                foreach (DataRow row in table.Rows)
                {
                    var extra = new ExtraItem
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Name = Convert.ToString(row["Name"]),
                        Price = Convert.ToDecimal(row["Price"])
                    };
                    clbExtras.Items.Add(extra, checkedIds.Contains(extra.Id));
                }
            }
            catch (Exception)
            {
                // Zonder de tabel extra_option (migratie 003) zijn er gewoon geen extra's te kiezen.
                clbExtras.Items.Clear();
            }
        }

        private void loadOpeningTimes()
        {
            OpeningHours.Load();
        }

        // Toont de auto's die actief zijn en in de gekozen periode niet al gereserveerd zijn.
        private void refreshCars()
        {
            CarItem current = cmbCar.SelectedItem as CarItem;
            DateTime start = pickedStart();
            DateTime end = pickedEnd();

            cmbCar.Items.Clear();
            if (end <= start)
            {
                lblCarInfo.Text = "Kies eerst een geldige periode.";
                return;
            }

            string sql =
                "SELECT c.ID, c.Brand, c.Model, c.License_Plate, c.Daily_Price FROM car c " +
                "WHERE c.Car_Status = 'active' AND NOT EXISTS (" +
                "SELECT 1 FROM rental r WHERE r.CarID = c.ID AND r.Status <> 'cancelled' " +
                "AND r.Date_Started < " + sqlDateTime(end) + " AND r.Date_Planned_Ending > " + sqlDateTime(start) + ") " +
                "ORDER BY c.Brand, c.Model, c.License_Plate";

            try
            {
                DataTable table = DBHelper.getdataTable(sql);
                foreach (DataRow row in table.Rows)
                {
                    var car = new CarItem
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Name = Convert.ToString(row["Brand"]) + " " + Convert.ToString(row["Model"]) +
                               " (" + Convert.ToString(row["License_Plate"]) + ")",
                        Price = Convert.ToDecimal(row["Daily_Price"])
                    };
                    cmbCar.Items.Add(car);
                    if (current != null && current.Id == car.Id)
                    {
                        cmbCar.SelectedIndex = cmbCar.Items.Count - 1;
                    }
                }

                lblCarInfo.Text = table.Rows.Count == 0
                    ? "Geen auto beschikbaar in deze periode."
                    : table.Rows.Count + " auto(\u2019s) beschikbaar in deze periode.";
            }
            catch (Exception ex)
            {
                lblCarInfo.Text = "De auto\u2019s konden niet worden geladen: " + ex.Message;
            }
        }

        // ---------- Openingstijden ----------

        // Geeft een foutmelding als het moment buiten de openingstijden valt, anders null.
        private static string checkOpeningHours(DateTime moment, string what)
        {
            return OpeningHours.Check(moment, what);
        }

        // ---------- Controles ----------

        private string validatePeriod(DateTime start, DateTime end)
        {
            DateTime now = DateTime.Now;

            if (start < now.AddMinutes(-1))
            {
                return "De ophaaltijd ligt in het verleden.";
            }
            if (end <= start)
            {
                return "Het terugbrengen moet na het ophalen liggen.";
            }
            if (start > now.AddYears(1) || end > now.AddYears(1))
            {
                return "Je kunt niet verder dan één jaar vooruit reserveren.";
            }

            string hours = checkOpeningHours(start, "Ophalen");
            if (hours != null)
            {
                return hours;
            }
            return checkOpeningHours(end, "Terugbrengen");
        }

        // Geeft de eerste reden waarom opslaan nog niet kan, of null als alles klopt.
        private string validateAll()
        {
            CustomerItem customer = cmbCustomer.SelectedItem as CustomerItem;
            if (customer == null)
            {
                return "Kies een klant.";
            }
            if (customer.Status.Trim().ToLowerInvariant() != "active")
            {
                return "Deze klant heeft de status \u201c" + StatusText.Customer(customer.Status).ToLowerInvariant() +
                       "\u201d en kan geen reservering maken.";
            }

            string periodError = validatePeriod(pickedStart(), pickedEnd());
            if (periodError != null)
            {
                return periodError;
            }

            CarItem car = cmbCar.SelectedItem as CarItem;
            if (car == null)
            {
                return cmbCar.Items.Count == 0 ? "Er is geen auto beschikbaar in deze periode." : "Kies een auto.";
            }

            return null;
        }

        // ---------- Samenvatting ----------

        private decimal extrasTotal()
        {
            decimal total = 0;
            foreach (object item in clbExtras.CheckedItems)
            {
                total += ((ExtraItem)item).Price;
            }
            return total;
        }

        private void updateSummary()
        {
            if (!ready)
            {
                return;
            }

            CustomerItem customer = cmbCustomer.SelectedItem as CustomerItem;
            CarItem car = cmbCar.SelectedItem as CarItem;
            DateTime start = pickedStart();
            DateTime end = pickedEnd();

            var sb = new StringBuilder();
            sb.AppendLine("Klant: " + (customer != null ? customer.Name : "-"));
            sb.AppendLine("Auto: " + (car != null ? car.Name : "-"));
            sb.AppendLine("Ophalen: " + start.ToString("ddd d MMM yyyy HH:mm", Dutch));
            sb.AppendLine("Terugbrengen: " + end.ToString("ddd d MMM yyyy HH:mm", Dutch));
            sb.AppendLine();

            decimal total = 0;
            if (end > start)
            {
                int days = countDays(start, end);
                lblPeriodInfo.Text = "Duur: " + days + " dag(en), gerekend per begonnen dag van 24 uur." +
                                     (!OpeningHours.IsConfigured
                                         ? " Er zijn nog geen openingstijden ingesteld, dus die worden niet gecontroleerd."
                                         : "");

                if (car != null)
                {
                    decimal rent = days * car.Price;
                    total += rent;
                    sb.AppendLine("Huur: " + days + " \u00d7 " + car.Price.ToString("C", Dutch) + " = " + rent.ToString("C", Dutch));
                }
            }
            else
            {
                lblPeriodInfo.Text = "";
            }

            foreach (object item in clbExtras.CheckedItems)
            {
                ExtraItem extra = (ExtraItem)item;
                sb.AppendLine("Extra: " + extra.Name + " " + extra.Price.ToString("C", Dutch));
            }
            total += extrasTotal();

            lblSummary.Text = sb.ToString();
            lblTotal.Text = "Totaal " + total.ToString("C", Dutch);

            string error = validateAll();
            lblMessage.Text = error ?? "";
            btnSave.Enabled = error == null;
        }

        // ---------- Opslaan ----------

        private void save()
        {
            string error = validateAll();
            if (error != null)
            {
                MessageBox.Show(error, "Nieuwe reservering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CustomerItem customer = (CustomerItem)cmbCustomer.SelectedItem;
            CarItem car = (CarItem)cmbCar.SelectedItem;
            DateTime start = pickedStart();
            DateTime end = pickedEnd();
            decimal total = countDays(start, end) * car.Price + extrasTotal();

            string startSql = sqlDateTime(start);
            string endSql = sqlDateTime(end);
            string notes = txtNotes.Text.Trim();
            string notesSql = notes.Length == 0 ? "NULL" : AuthHelper.sql(notes);

            int rentalId = 0;
            try
            {
                // De controle op overlap zit in de INSERT zelf, zodat twee medewerkers niet tegelijk
                // dezelfde auto voor dezelfde periode kunnen reserveren.
                int inserted = DBHelper.RunSql(
                    "INSERT INTO rental (UserID, CarID, Created_By, Price_Per_Day, Date_Started, Date_Planned_Ending, Status, Total_Price, notes) " +
                    "SELECT " + customer.Id + ", " + car.Id + ", " + DBHelper.loggedinuserid + ", " + sqlDecimal(car.Price) + ", " +
                    startSql + ", " + endSql + ", 'reserved', " + sqlDecimal(total) + ", " + notesSql + " " +
                    "FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rental r WHERE r.CarID = " + car.Id +
                    " AND r.Status <> 'cancelled' AND r.Date_Started < " + endSql + " AND r.Date_Planned_Ending > " + startSql + ") " +
                    "AND EXISTS (SELECT 1 FROM car c WHERE c.ID = " + car.Id + " AND c.Car_Status = 'active')");

                if (inserted == 0)
                {
                    MessageBox.Show("Deze auto is net gereserveerd of niet meer beschikbaar. Kies een andere auto.",
                        "Nieuwe reservering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    refreshCars();
                    updateSummary();
                    return;
                }

                object id = DBHelper.getScalar(
                    "SELECT ID FROM rental WHERE UserID = " + customer.Id + " AND CarID = " + car.Id +
                    " AND Date_Started = " + startSql + " AND Date_Planned_Ending = " + endSql +
                    " AND Status = 'reserved' ORDER BY ID DESC LIMIT 1");
                if (id == null || id == DBNull.Value)
                {
                    throw new InvalidOperationException("De nieuwe reservering kon niet worden teruggevonden.");
                }
                rentalId = Convert.ToInt32(id);

                var rows = new List<string>();
                foreach (object item in clbExtras.CheckedItems)
                {
                    ExtraItem extra = (ExtraItem)item;
                    rows.Add("(" + rentalId + ", " + extra.Id + ", " + sqlDecimal(extra.Price) + ")");
                }
                if (rows.Count > 0)
                {
                    DBHelper.RunSql("INSERT INTO rental_option (Rental_ID, Option_ID, Option_Price) VALUES " +
                                    string.Join(", ", rows.ToArray()));
                }
            }
            catch (Exception ex)
            {
                // Is er al een reservering aangemaakt, maak die dan weer ongedaan.
                if (rentalId > 0)
                {
                    try
                    {
                        DBHelper.RunSql("DELETE FROM rental_option WHERE Rental_ID = " + rentalId);
                        DBHelper.RunSql("DELETE FROM rental WHERE ID = " + rentalId);
                    }
                    catch (Exception)
                    {
                    }
                }

                MessageBox.Show("De reservering kon niet worden opgeslagen: " + ex.Message, "Nieuwe reservering",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string invoiceNote = "";
            try
            {
                createInvoice(rentalId, total);
            }
            catch (Exception)
            {
                invoiceNote = "\n\nLet op: de factuur kon niet worden aangemaakt.";
            }

            MessageBox.Show(
                "Reservering #" + rentalId + " is opgeslagen voor " + customer.Name + ".\n" +
                car.Name + "\n" + start.ToString("d MMM yyyy HH:mm", Dutch) + " tot " + end.ToString("d MMM yyyy HH:mm", Dutch) +
                "\nTotaal: " + total.ToString("C", Dutch) + invoiceNote,
                "Nieuwe reservering", MessageBoxButtons.OK, MessageBoxIcon.Information);

            resetForm();
        }

        // TODO: vervangen door de gezamenlijke factuurfunctie zodra het team die heeft.
        // Dit gebruikt alleen de kolommen die de tabel invoice nu heeft (Rental_ID, Total_Price, Expiration_Date).
        private static void createInvoice(int rentalId, decimal total)
        {
            DBHelper.RunSql(
                "INSERT INTO invoice (Rental_ID, Total_Price, Expiration_Date) VALUES (" +
                rentalId + ", " + sqlDecimal(total) + ", DATE_ADD(CURDATE(), INTERVAL " + InvoiceDays + " DAY))");
        }

        private void resetForm()
        {
            adjusting = true;
            txtSearch.Text = "";
            txtNotes.Text = "";
            for (int i = 0; i < clbExtras.Items.Count; i++)
            {
                clbExtras.SetItemChecked(i, false);
            }
            dtpStart.Value = defaultStart();
            dtpEnd.Value = defaultStart().AddDays(1);
            adjusting = false;

            searchCustomers();
            cmbCustomer.SelectedIndex = -1;
            refreshCars();
            updateSummary();
        }
    }
}