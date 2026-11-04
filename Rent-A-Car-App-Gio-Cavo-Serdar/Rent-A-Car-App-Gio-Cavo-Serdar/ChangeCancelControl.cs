using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Medewerkerscherm (user story 4): een reservering namens een klant wijzigen of annuleren.
    // Alleen reserveringen die nog niet zijn opgehaald (status reserved) staan in de lijst.
    //   Wijzigen:  periode, auto, extra's en opmerking. De beschikbaarheid wordt opnieuw gecontroleerd en de prijs
    //              herberekend. Bestaande prijzen blijven staan: de dagprijs en extra-prijzen die bij het reserveren
    //              zijn vastgelegd, veranderen alleen als de auto of de extra zelf wordt gewijzigd.
    //   Annuleren: kan tot 24 uur voor het ophalen, met een bevestigingsvraag.
    public class ChangeCancelControl : UserControl
    {
        private static readonly Color AccentColor = Color.FromArgb(79, 70, 229);
        private static readonly Color HeaderColor = Color.FromArgb(243, 244, 246);
        private static readonly Color TextColor = Color.FromArgb(31, 41, 55);
        private static readonly Color MutedColor = Color.FromArgb(107, 114, 128);
        private static readonly Color ErrorColor = Color.FromArgb(185, 28, 28);
        private static readonly Color DangerBackColor = Color.FromArgb(254, 226, 226);

        private static readonly CultureInfo Dutch = new CultureInfo("nl-NL");

        // Zoveel uur voor het ophalen kan een reservering nog geannuleerd worden.
        private const int CancelHours = 24;

        // De reservering zoals hij in de database staat op het moment dat hij is geopend.
        private class RentalInfo
        {
            public int Id;
            public string Customer;
            public int CarId;
            public decimal PricePerDay;
            public decimal TotalPrice;
            public DateTime Start;
            public DateTime End;
            public string Notes;
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
        private readonly DataGridView grid;
        private readonly Label lblTitle;
        private readonly Label lblCustomer;
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
        private readonly Label lblCancelInfo;
        private readonly Button btnSave;
        private readonly Button btnCancelRental;
        private readonly Panel editPanel;

        private RentalInfo current = null;
        private Dictionary<int, decimal> existingOptions = new Dictionary<int, decimal>();
        private DateTime lastStart;
        private bool ready = false;
        private bool adjusting = false;
        private bool loadingList = false;

        public ChangeCancelControl()
        {
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.75F);

            // ---------- Rechts: het formulier om de gekozen reservering aan te passen ----------
            var right = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            lblTitle = new Label
            {
                Location = new Point(24, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextColor,
                Text = "Kies links een reservering"
            };
            lblCustomer = new Label { Location = new Point(24, 46), Size = new Size(400, 22), ForeColor = MutedColor };
            right.Controls.Add(lblTitle);
            right.Controls.Add(lblCustomer);

            editPanel = new Panel { Location = new Point(0, 76), Size = new Size(960, 640), Enabled = false };
            right.Controls.Add(editPanel);

            editPanel.Controls.Add(makeCaption("Ophalen", 24, 0));
            dtpStart = makePicker(24, 22);
            editPanel.Controls.Add(dtpStart);
            editPanel.Controls.Add(makeCaption("Terugbrengen", 24, 58));
            dtpEnd = makePicker(24, 80);
            editPanel.Controls.Add(dtpEnd);
            lblPeriodInfo = makeLabel(24, 112, 400, 36, MutedColor);
            editPanel.Controls.Add(lblPeriodInfo);

            editPanel.Controls.Add(makeCaption("Auto", 24, 156));
            cmbCar = new ComboBox
            {
                Location = new Point(24, 178),
                Size = new Size(400, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            lblCarInfo = makeLabel(24, 208, 400, 22, MutedColor);
            editPanel.Controls.Add(cmbCar);
            editPanel.Controls.Add(lblCarInfo);

            editPanel.Controls.Add(makeCaption("Extra's", 24, 242));
            clbExtras = new CheckedListBox
            {
                Location = new Point(24, 264),
                Size = new Size(400, 84),
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            editPanel.Controls.Add(clbExtras);

            editPanel.Controls.Add(makeCaption("Opmerking", 24, 360));
            txtNotes = new TextBox
            {
                Location = new Point(24, 382),
                Size = new Size(400, 56),
                Multiline = true,
                MaxLength = 255
            };
            editPanel.Controls.Add(txtNotes);

            // Samenvatting en knoppen.
            var card = new Panel
            {
                Location = new Point(470, 0),
                Size = new Size(420, 520),
                BackColor = Color.FromArgb(249, 250, 251)
            };
            card.Controls.Add(makeHeading("Samenvatting", 20, 16));
            lblSummary = new Label { Location = new Point(20, 52), Size = new Size(380, 190), ForeColor = TextColor };
            lblTotal = new Label
            {
                Location = new Point(20, 248),
                Size = new Size(380, 36),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = AccentColor
            };
            lblMessage = new Label { Location = new Point(20, 292), Size = new Size(380, 50), ForeColor = ErrorColor };
            btnSave = makeButton("Wijzigingen opslaan", 20, 350, 380);
            btnSave.Height = 44;
            btnSave.BackColor = AccentColor;
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnCancelRental = makeButton("Reservering annuleren", 20, 406, 380);
            btnCancelRental.Height = 40;
            btnCancelRental.BackColor = DangerBackColor;
            btnCancelRental.ForeColor = ErrorColor;
            lblCancelInfo = new Label { Location = new Point(20, 452), Size = new Size(380, 44), ForeColor = MutedColor };
            card.Controls.Add(lblSummary);
            card.Controls.Add(lblTotal);
            card.Controls.Add(lblMessage);
            card.Controls.Add(btnSave);
            card.Controls.Add(btnCancelRental);
            card.Controls.Add(lblCancelInfo);
            editPanel.Controls.Add(card);

            // ---------- Links: zoeken en de lijst met reserveringen ----------
            var left = new Panel { Dock = DockStyle.Left, Width = 620 };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 32 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextColor;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 36;
            grid.DefaultCellStyle.ForeColor = TextColor;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
            grid.DefaultCellStyle.SelectionForeColor = TextColor;

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 56 };
            txtSearch = new TextBox { Location = new Point(16, 14), Size = new Size(380, 27) };
            Button btnSearch = makeButton("Zoeken", 404, 12, 90);
            Button btnRefresh = makeButton("Vernieuwen", 502, 12, 100);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnSearch);
            searchPanel.Controls.Add(btnRefresh);

            left.Controls.Add(grid);
            left.Controls.Add(searchPanel);

            Controls.Add(right);
            Controls.Add(left);

            // ---------- Gebeurtenissen ----------
            btnSearch.Click += (s, e) => loadList();
            btnRefresh.Click += (s, e) => loadList();
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    loadList();
                }
            };
            grid.SelectionChanged += (s, e) =>
            {
                if (!ready || loadingList)
                {
                    return;
                }

                DataRowView row = selectedRow();
                if (row != null)
                {
                    loadReservation(Convert.ToInt32(row["Nr"]));
                }
            };
            dtpStart.ValueChanged += (s, e) =>
            {
                if (!ready || adjusting || current == null)
                {
                    return;
                }

                // Schuif het terugbrengen mee, zodat de duur gelijk blijft.
                adjusting = true;
                dtpEnd.Value = pickedEnd() + (pickedStart() - lastStart);
                lastStart = pickedStart();
                adjusting = false;

                refreshCars();
                updateSummary();
            };
            dtpEnd.ValueChanged += (s, e) =>
            {
                if (!ready || adjusting || current == null)
                {
                    return;
                }

                refreshCars();
                updateSummary();
            };
            cmbCar.SelectedIndexChanged += (s, e) => updateSummary();
            clbExtras.ItemCheck += (s, e) =>
            {
                if (!adjusting)
                {
                    BeginInvoke(new Action(updateSummary));
                }
            };
            txtNotes.TextChanged += (s, e) =>
            {
                if (!adjusting)
                {
                    updateSummary();
                }
            };
            btnSave.Click += (s, e) => saveChanges();
            btnCancelRental.Click += (s, e) => cancelReservation();

            // Reserveringen kunnen op andere tabbladen veranderen, dus laad opnieuw zodra dit tabblad weer getoond wordt.
            VisibleChanged += (s, e) =>
            {
                if (ready && Visible)
                {
                    OpeningHours.Load();
                    loadList();
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
            OpeningHours.Load();
            loadList();
        }

        // ---------- Hulpmethodes ----------

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
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = MutedColor };
        }

        private static Label makeLabel(int x, int y, int width, int height, Color color)
        {
            return new Label { Location = new Point(x, y), Size = new Size(width, height), ForeColor = color };
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
                Size = new Size(400, 27)
            };
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

        private DataRowView selectedRow()
        {
            return grid.CurrentRow == null ? null : grid.CurrentRow.DataBoundItem as DataRowView;
        }

        // ---------- De lijst met reserveringen ----------

        private void loadList()
        {
            int keepId = current != null ? current.Id : 0;

            string where = "WHERE r.Status = 'reserved' ";
            string text = txtSearch.Text.Trim();
            if (text.Length > 0)
            {
                string pattern = AuthHelper.sql("%" + text + "%");
                where += "AND (u.Name LIKE " + pattern + " OR u.LastName LIKE " + pattern +
                         " OR u.Email LIKE " + pattern + " OR CONCAT(u.Name, ' ', u.LastName) LIKE " + pattern +
                         " OR CAST(r.ID AS CHAR) = " + AuthHelper.sql(text) + ") ";
            }

            try
            {
                DataTable table = DBHelper.getdataTable(
                    "SELECT r.ID AS `Nr`, CONCAT(u.Name, ' ', u.LastName) AS `Klant`, " +
                    "CONCAT(c.Brand, ' ', c.Model, ' (', c.License_Plate, ')') AS `Auto`, " +
                    "r.Date_Started AS `Ophalen`, r.Date_Planned_Ending AS `Terugbrengen`, r.Total_Price AS `Totaal` " +
                    "FROM rental r JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID " +
                    where + "ORDER BY r.Date_Started LIMIT 200");

                loadingList = true;
                grid.DataSource = table;
                formatColumn("Ophalen", "ddd d MMM HH:mm");
                formatColumn("Terugbrengen", "ddd d MMM HH:mm");
                formatColumn("Totaal", "C");
                if (grid.Columns.Contains("Nr"))
                {
                    grid.Columns["Nr"].FillWeight = 30;
                }

                // Selecteer de reservering die open stond weer, als hij nog in de lijst staat.
                bool found = false;
                foreach (DataGridViewRow gridRow in grid.Rows)
                {
                    DataRowView view = gridRow.DataBoundItem as DataRowView;
                    if (view != null && keepId != 0 && Convert.ToInt32(view["Nr"]) == keepId)
                    {
                        grid.CurrentCell = gridRow.Cells[0];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    grid.ClearSelection();
                    grid.CurrentCell = null;
                }
                loadingList = false;

                if (found)
                {
                    loadReservation(keepId);
                }
                else
                {
                    clearForm(table.Rows.Count == 0
                        ? "Geen reserveringen gevonden die nog gewijzigd kunnen worden."
                        : "Kies links een reservering");
                }
            }
            catch (Exception ex)
            {
                loadingList = false;
                MessageBox.Show("De reserveringen konden niet worden geladen: " + ex.Message, "Wijzigen / annuleren",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void formatColumn(string name, string format)
        {
            if (grid.Columns.Contains(name))
            {
                grid.Columns[name].DefaultCellStyle.Format = format;
                grid.Columns[name].DefaultCellStyle.FormatProvider = Dutch;
            }
        }

        private void clearForm(string title)
        {
            current = null;
            existingOptions = new Dictionary<int, decimal>();
            lblTitle.Text = title;
            lblCustomer.Text = "";
            lblSummary.Text = "";
            lblTotal.Text = "";
            lblMessage.Text = "";
            lblCancelInfo.Text = "";
            lblPeriodInfo.Text = "";
            lblCarInfo.Text = "";
            cmbCar.Items.Clear();
            clbExtras.Items.Clear();
            txtNotes.Text = "";
            editPanel.Enabled = false;
        }

        // ---------- Een reservering openen ----------

        private void loadReservation(int id)
        {
            try
            {
                DataTable table = DBHelper.getdataTable(
                    "SELECT r.ID, r.CarID, r.Price_Per_Day, r.Total_Price, r.Date_Started, r.Date_Planned_Ending, r.notes, " +
                    "r.Status, CONCAT(u.Name, ' ', u.LastName) AS Customer " +
                    "FROM rental r JOIN user u ON u.ID = r.UserID WHERE r.ID = " + id);

                if (table.Rows.Count == 0 || Convert.ToString(table.Rows[0]["Status"]).ToLowerInvariant() != "reserved")
                {
                    clearForm("Deze reservering kan niet meer gewijzigd worden.");
                    return;
                }

                DataRow row = table.Rows[0];
                current = new RentalInfo
                {
                    Id = Convert.ToInt32(row["ID"]),
                    Customer = Convert.ToString(row["Customer"]),
                    CarId = Convert.ToInt32(row["CarID"]),
                    PricePerDay = Convert.ToDecimal(row["Price_Per_Day"]),
                    TotalPrice = Convert.ToDecimal(row["Total_Price"]),
                    Start = minutePrecision(Convert.ToDateTime(row["Date_Started"])),
                    End = minutePrecision(Convert.ToDateTime(row["Date_Planned_Ending"])),
                    Notes = Convert.ToString(row["notes"])
                };

                existingOptions = new Dictionary<int, decimal>();
                DataTable options = DBHelper.getdataTable(
                    "SELECT Option_ID, Option_Price FROM rental_option WHERE Rental_ID = " + id);
                foreach (DataRow option in options.Rows)
                {
                    existingOptions[Convert.ToInt32(option["Option_ID"])] = Convert.ToDecimal(option["Option_Price"]);
                }

                adjusting = true;
                dtpStart.Value = current.Start;
                dtpEnd.Value = current.End;
                lastStart = current.Start;
                txtNotes.Text = current.Notes;
                loadExtras();
                adjusting = false;

                lblTitle.Text = "Reservering #" + current.Id;
                lblCustomer.Text = "Klant: " + current.Customer;
                editPanel.Enabled = true;

                refreshCars();
                updateSummary();
            }
            catch (Exception ex)
            {
                adjusting = false;
                MessageBox.Show("De reservering kon niet worden geladen: " + ex.Message, "Wijzigen / annuleren",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Toont de actieve extra's, plus extra's die al op de reservering staan (ook als ze intussen uit gebruik zijn).
        // Extra's die al op de reservering staan houden de prijs van toen.
        private void loadExtras()
        {
            DataTable table = DBHelper.getdataTable("SELECT ID, Name, Price, Is_Active FROM extra_option ORDER BY Name");

            clbExtras.Items.Clear();
            foreach (DataRow row in table.Rows)
            {
                int id = Convert.ToInt32(row["ID"]);
                bool active = Convert.ToBoolean(row["Is_Active"]);
                bool onRental = existingOptions.ContainsKey(id);
                if (!active && !onRental)
                {
                    continue;
                }

                clbExtras.Items.Add(new ExtraItem
                {
                    Id = id,
                    Name = Convert.ToString(row["Name"]),
                    Price = onRental ? existingOptions[id] : Convert.ToDecimal(row["Price"])
                }, onRental);
            }
        }

        // Auto's die actief zijn en in de gekozen periode vrij zijn (deze reservering zelf telt niet mee).
        // De auto die al op de reservering staat houdt de dagprijs van toen.
        private void refreshCars()
        {
            if (current == null)
            {
                return;
            }

            CarItem selected = cmbCar.SelectedItem as CarItem;
            int wantedId = selected != null ? selected.Id : current.CarId;
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
                "SELECT 1 FROM rental r WHERE r.CarID = c.ID AND r.ID <> " + current.Id + " AND r.Status <> 'cancelled' " +
                "AND r.Date_Started < " + sqlDateTime(end) + " AND r.Date_Planned_Ending > " + sqlDateTime(start) + ") " +
                "ORDER BY c.Brand, c.Model, c.License_Plate";

            try
            {
                DataTable table = DBHelper.getdataTable(sql);
                foreach (DataRow row in table.Rows)
                {
                    int id = Convert.ToInt32(row["ID"]);
                    cmbCar.Items.Add(new CarItem
                    {
                        Id = id,
                        Name = Convert.ToString(row["Brand"]) + " " + Convert.ToString(row["Model"]) +
                               " (" + Convert.ToString(row["License_Plate"]) + ")",
                        Price = id == current.CarId ? current.PricePerDay : Convert.ToDecimal(row["Daily_Price"])
                    });
                    if (id == wantedId)
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

        // ---------- Controles en samenvatting ----------

        private decimal extrasTotal()
        {
            decimal total = 0;
            foreach (object item in clbExtras.CheckedItems)
            {
                total += ((ExtraItem)item).Price;
            }
            return total;
        }

        private HashSet<int> checkedExtraIds()
        {
            var ids = new HashSet<int>();
            foreach (object item in clbExtras.CheckedItems)
            {
                ids.Add(((ExtraItem)item).Id);
            }
            return ids;
        }

        private bool hasChanges()
        {
            if (current == null)
            {
                return false;
            }

            CarItem car = cmbCar.SelectedItem as CarItem;
            var oldIds = new HashSet<int>(existingOptions.Keys);

            return pickedStart() != current.Start ||
                   pickedEnd() != current.End ||
                   (car != null && car.Id != current.CarId) ||
                   txtNotes.Text.Trim() != (current.Notes ?? "").Trim() ||
                   !oldIds.SetEquals(checkedExtraIds());
        }

        // Geeft de eerste reden waarom opslaan nog niet kan, of null als het kan.
        private string validateEdit()
        {
            if (current == null)
            {
                return "Kies links een reservering.";
            }

            DateTime now = DateTime.Now;
            DateTime start = pickedStart();
            DateTime end = pickedEnd();
            bool startChanged = start != current.Start;
            bool endChanged = end != current.End;

            if (startChanged && start < now.AddMinutes(-1))
            {
                return "De ophaaltijd ligt in het verleden.";
            }
            if (end <= start)
            {
                return "Het terugbrengen moet na het ophalen liggen.";
            }
            if ((startChanged || endChanged) && (start > now.AddYears(1) || end > now.AddYears(1)))
            {
                return "Je kunt niet verder dan één jaar vooruit reserveren.";
            }

            // De openingstijden gelden alleen voor tijden die je nu wijzigt.
            if (startChanged)
            {
                string hours = OpeningHours.Check(start, "Ophalen");
                if (hours != null)
                {
                    return hours;
                }
            }
            if (endChanged)
            {
                string hours = OpeningHours.Check(end, "Terugbrengen");
                if (hours != null)
                {
                    return hours;
                }
            }

            CarItem chosenCar = cmbCar.SelectedItem as CarItem;
            if (chosenCar == null)
            {
                return cmbCar.Items.Count == 0
                    ? "Er is geen auto beschikbaar in deze periode."
                    : "Kies een auto.";
            }

            if (!hasChanges())
            {
                return "Er is nog niets gewijzigd.";
            }

            return null;
        }

        private void updateSummary()
        {
            if (!ready || current == null)
            {
                return;
            }

            CarItem car = cmbCar.SelectedItem as CarItem;
            DateTime start = pickedStart();
            DateTime end = pickedEnd();

            var sb = new StringBuilder();
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

            sb.AppendLine();
            sb.AppendLine("Oorspronkelijk totaal: " + current.TotalPrice.ToString("C", Dutch));

            lblSummary.Text = sb.ToString();
            lblTotal.Text = "Totaal " + total.ToString("C", Dutch);

            string error = validateEdit();
            lblMessage.Text = error ?? "";
            btnSave.Enabled = error == null;

            // Annuleren kan tot 24 uur voor het ophalen (gerekend vanaf het opgeslagen ophaalmoment).
            DateTime deadline = current.Start.AddHours(-CancelHours);
            bool canCancel = DateTime.Now <= deadline;
            btnCancelRental.Enabled = canCancel;
            lblCancelInfo.Text = canCancel
                ? "Annuleren kan nog tot " + deadline.ToString("ddd d MMM HH:mm", Dutch) + "."
                : "Annuleren is niet meer mogelijk: het ophalen is binnen " + CancelHours + " uur.";
        }

        // ---------- Wijzigingen opslaan ----------

        private void saveChanges()
        {
            string error = validateEdit();
            if (error != null)
            {
                MessageBox.Show(error, "Wijzigen / annuleren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CarItem car = (CarItem)cmbCar.SelectedItem;
            DateTime start = pickedStart();
            DateTime end = pickedEnd();
            decimal total = countDays(start, end) * car.Price + extrasTotal();
            string startSql = sqlDateTime(start);
            string endSql = sqlDateTime(end);
            string notes = txtNotes.Text.Trim();
            string notesSql = notes.Length == 0 ? "NULL" : AuthHelper.sql(notes);
            int id = current.Id;

            string invoiceNote = "";
            try
            {
                // Controleer vlak voor het opslaan nog een keer of de auto vrij is.
                int busy = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT COUNT(*) FROM rental WHERE CarID = " + car.Id + " AND ID <> " + id +
                    " AND Status <> 'cancelled' AND Date_Started < " + endSql + " AND Date_Planned_Ending > " + startSql));
                int active = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT COUNT(*) FROM car WHERE ID = " + car.Id + " AND Car_Status = 'active'"));
                if (busy > 0 || active == 0)
                {
                    MessageBox.Show("Deze auto is in de tussentijd gereserveerd of niet meer beschikbaar. Kies een andere auto.",
                        "Wijzigen / annuleren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    refreshCars();
                    updateSummary();
                    return;
                }

                int changed = DBHelper.RunSql(
                    "UPDATE rental SET CarID = " + car.Id + ", Price_Per_Day = " + sqlDecimal(car.Price) +
                    ", Date_Started = " + startSql + ", Date_Planned_Ending = " + endSql +
                    ", Total_Price = " + sqlDecimal(total) + ", notes = " + notesSql +
                    " WHERE ID = " + id + " AND Status = 'reserved'");
                if (changed == 0)
                {
                    MessageBox.Show("Deze reservering is intussen gewijzigd of opgehaald. De lijst wordt vernieuwd.",
                        "Wijzigen / annuleren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    loadList();
                    return;
                }

                DBHelper.RunSql("DELETE FROM rental_option WHERE Rental_ID = " + id);
                var rows = new List<string>();
                foreach (object item in clbExtras.CheckedItems)
                {
                    ExtraItem extra = (ExtraItem)item;
                    rows.Add("(" + id + ", " + extra.Id + ", " + sqlDecimal(extra.Price) + ")");
                }
                if (rows.Count > 0)
                {
                    DBHelper.RunSql("INSERT INTO rental_option (Rental_ID, Option_ID, Option_Price) VALUES " +
                                    string.Join(", ", rows.ToArray()));
                }

                // TODO: vervangen door de gezamenlijke factuurfunctie zodra het team die heeft.
                try
                {
                    DBHelper.RunSql("UPDATE invoice SET Total_Price = " + sqlDecimal(total) + " WHERE Rental_ID = " + id);
                }
                catch (Exception)
                {
                    invoiceNote = "\n\nLet op: de factuur kon niet worden bijgewerkt.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("De wijziging kon niet (volledig) worden opgeslagen: " + ex.Message +
                                "\n\nControleer de reservering in de lijst.", "Wijzigen / annuleren",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                loadList();
                return;
            }

            MessageBox.Show("Reservering #" + id + " is bijgewerkt.\nNieuw totaal: " + total.ToString("C", Dutch) + invoiceNote,
                "Wijzigen / annuleren", MessageBoxButtons.OK, MessageBoxIcon.Information);
            loadList();
        }

        // ---------- Annuleren ----------

        private void cancelReservation()
        {
            if (current == null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            if (current.Start < now.AddHours(CancelHours))
            {
                MessageBox.Show("Annuleren kan alleen tot " + CancelHours + " uur voor het ophalen.",
                    "Reservering annuleren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                updateSummary();
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Reservering #" + current.Id + " van " + current.Customer + " annuleren?\n\n" +
                "De bijbehorende factuur wordt verwijderd.",
                "Reservering annuleren", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
            {
                return;
            }

            int id = current.Id;
            string invoiceNote = "";
            try
            {
                int changed = DBHelper.RunSql(
                    "UPDATE rental SET Status = 'cancelled' WHERE ID = " + id + " AND Status = 'reserved' " +
                    "AND Date_Started >= " + sqlDateTime(now.AddHours(CancelHours)));
                if (changed == 0)
                {
                    MessageBox.Show("Deze reservering is intussen gewijzigd of kan niet meer geannuleerd worden. De lijst wordt vernieuwd.",
                        "Reservering annuleren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    loadList();
                    return;
                }

                // TODO: vervangen door de gezamenlijke factuurfunctie (bijvoorbeeld een status of creditfactuur).
                try
                {
                    DBHelper.RunSql("DELETE FROM invoice WHERE Rental_ID = " + id);
                }
                catch (Exception)
                {
                    invoiceNote = "\n\nLet op: de factuur kon niet worden verwijderd.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("De reservering kon niet worden geannuleerd: " + ex.Message, "Reservering annuleren",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Reservering #" + id + " is geannuleerd." + invoiceNote, "Reservering annuleren",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            current = null;
            loadList();
        }
    }
}
