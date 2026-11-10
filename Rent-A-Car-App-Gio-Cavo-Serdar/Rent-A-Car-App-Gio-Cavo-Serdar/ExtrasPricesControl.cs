using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Scherm (user story 6): extra's beheren en dagprijzen van auto's wijzigen.
    // Links de extra's (navigatie, kinderstoeltje, luxe-pakket, ...): toevoegen, bewerken, in of uit gebruik zetten.
    // Rechts de dagprijzen van de auto's.
    // Prijswijzigingen gelden alleen voor nieuwe reserveringen: bij het reserveren worden de dagprijs en de
    // extra-prijzen vastgelegd in rental en rental_option, dus bestaande reserveringen veranderen niet.
    // Extra's worden nooit echt verwijderd, anders verdwijnt de historie van oude reserveringen.
    public class ExtrasPricesControl : UserControl
    {
        private static readonly Color AccentColor = Color.FromArgb(79, 70, 229);
        private static readonly Color HeaderColor = Color.FromArgb(243, 244, 246);
        private static readonly Color TextColor = Color.FromArgb(31, 41, 55);
        private static readonly Color MutedColor = Color.FromArgb(107, 114, 128);
        private static readonly Color ErrorColor = Color.FromArgb(185, 28, 28);
        private static readonly Color SuccessColor = Color.FromArgb(21, 128, 61);

        private static readonly CultureInfo Dutch = new CultureInfo("nl-NL");

        // Extra's
        private DataGridView grdExtras;
        private TextBox txtExtraName;
        private TextBox txtExtraDescription;
        private NumericUpDown nudExtraPrice;
        private Button btnExtraNew;
        private Button btnExtraSave;
        private Button btnExtraToggle;
        private Label lblExtraInfo;
        private int selectedExtraId = 0;          // 0 = een nieuwe extra
        private bool selectedExtraActive = true;
        private bool loadingExtras = false;

        // Auto's
        private DataGridView grdCars;
        private Label lblCarName;
        private NumericUpDown nudCarPrice;
        private Button btnCarSave;
        private Label lblCarInfo;
        private int selectedCarId = 0;
        private bool loadingCars = false;

        private bool ready = false;

        public ExtrasPricesControl()
        {
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.75F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            layout.Controls.Add(createExtrasSection(), 0, 0);
            layout.Controls.Add(createCarsSection(), 1, 0);
            Controls.Add(layout);

            // Beide lijsten kunnen op andere tabbladen veranderen, dus laad opnieuw zodra dit tabblad weer getoond wordt.
            VisibleChanged += (s, e) =>
            {
                if (ready && Visible)
                {
                    loadExtras(selectedExtraId);
                    loadCars(selectedCarId);
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
            loadExtras(0);
            loadCars(0);
        }

        // ---------- Opbouw van het scherm ----------

        private static Button makeButton(string text, int x, int y, int width)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 32),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = HeaderColor,
                ForeColor = TextColor
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Label makeCaption(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = MutedColor };
        }

        private static NumericUpDown makePriceBox(int x, int y)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(140, 27),
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100000,
                Increment = 1,
                ThousandsSeparator = false,
                TextAlign = HorizontalAlignment.Right
            };
        }

        private static DataGridView makeGrid()
        {
            var grid = new DataGridView
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
            return grid;
        }

        private static Label makeHeading(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private Panel createExtrasSection()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 8, 16, 16) };

            grdExtras = makeGrid();
            grdExtras.SelectionChanged += (s, e) =>
            {
                if (!ready || loadingExtras)
                {
                    return;
                }

                DataRowView row = selectedRow(grdExtras);
                if (row != null)
                {
                    showExtra(row);
                }
            };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 270 };
            bottom.Controls.Add(makeCaption("Naam", 0, 12));
            txtExtraName = new TextBox
            {
                Location = new Point(0, 34),
                Size = new Size(420, 27),
                MaxLength = 100,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            bottom.Controls.Add(txtExtraName);
            bottom.Controls.Add(makeCaption("Omschrijving (optioneel)", 0, 70));
            txtExtraDescription = new TextBox
            {
                Location = new Point(0, 92),
                Size = new Size(420, 27),
                MaxLength = 255,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            bottom.Controls.Add(txtExtraDescription);
            bottom.Controls.Add(makeCaption("Prijs per reservering (\u20ac)", 0, 128));
            nudExtraPrice = makePriceBox(0, 150);
            bottom.Controls.Add(nudExtraPrice);

            btnExtraNew = makeButton("Nieuw", 0, 194, 90);
            btnExtraSave = makeButton("Opslaan", 100, 194, 110);
            btnExtraSave.BackColor = AccentColor;
            btnExtraSave.ForeColor = Color.White;
            btnExtraToggle = makeButton("Uit gebruik nemen", 220, 194, 170);
            lblExtraInfo = new Label
            {
                Location = new Point(0, 234),
                Size = new Size(420, 36),
                ForeColor = MutedColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            bottom.Controls.Add(btnExtraNew);
            bottom.Controls.Add(btnExtraSave);
            bottom.Controls.Add(btnExtraToggle);
            bottom.Controls.Add(lblExtraInfo);

            btnExtraNew.Click += (s, e) => newExtra();
            btnExtraSave.Click += (s, e) => saveExtra();
            btnExtraToggle.Click += (s, e) => toggleExtra();

            panel.Controls.Add(grdExtras);
            panel.Controls.Add(bottom);
            panel.Controls.Add(makeHeading("Extra\u2019s"));
            return panel;
        }

        private Panel createCarsSection()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 8, 16, 16) };

            grdCars = makeGrid();
            grdCars.SelectionChanged += (s, e) =>
            {
                if (!ready || loadingCars)
                {
                    return;
                }

                DataRowView row = selectedRow(grdCars);
                if (row != null)
                {
                    showCar(row);
                }
            };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 270 };
            lblCarName = new Label
            {
                Location = new Point(0, 12),
                Size = new Size(420, 24),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = TextColor,
                Text = "Kies een auto in de lijst"
            };
            bottom.Controls.Add(lblCarName);
            bottom.Controls.Add(makeCaption("Nieuwe dagprijs (\u20ac)", 0, 50));
            nudCarPrice = makePriceBox(0, 72);
            nudCarPrice.Enabled = false;
            bottom.Controls.Add(nudCarPrice);
            btnCarSave = makeButton("Dagprijs opslaan", 160, 70, 160);
            btnCarSave.BackColor = AccentColor;
            btnCarSave.ForeColor = Color.White;
            btnCarSave.Enabled = false;
            bottom.Controls.Add(btnCarSave);
            lblCarInfo = new Label
            {
                Location = new Point(0, 116),
                Size = new Size(420, 70),
                ForeColor = MutedColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            bottom.Controls.Add(lblCarInfo);

            btnCarSave.Click += (s, e) => saveCarPrice();

            panel.Controls.Add(grdCars);
            panel.Controls.Add(bottom);
            panel.Controls.Add(makeHeading("Dagprijzen auto\u2019s"));
            return panel;
        }

        // ---------- Hulpmethodes ----------

        private static DataRowView selectedRow(DataGridView grid)
        {
            return grid.CurrentRow == null ? null : grid.CurrentRow.DataBoundItem as DataRowView;
        }

        // Selecteert de rij met dit nummer (kolom Nr). Geeft false als hij niet in de lijst staat.
        private static bool selectRow(DataGridView grid, int id)
        {
            foreach (DataGridViewRow gridRow in grid.Rows)
            {
                DataRowView view = gridRow.DataBoundItem as DataRowView;
                if (view != null && Convert.ToInt32(view["Nr"]) == id)
                {
                    grid.CurrentCell = gridRow.Cells[0];
                    return true;
                }
            }
            return false;
        }

        private static void formatColumn(DataGridView grid, string name, string format)
        {
            if (grid.Columns.Contains(name))
            {
                grid.Columns[name].DefaultCellStyle.Format = format;
                grid.Columns[name].DefaultCellStyle.FormatProvider = Dutch;
            }
        }

        private static void setInfo(Label label, string text, Color color)
        {
            label.Text = text;
            label.ForeColor = color;
        }

        private static string sqlDecimal(decimal value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static decimal clampPrice(NumericUpDown box, decimal value)
        {
            return Math.Max(box.Minimum, Math.Min(box.Maximum, value));
        }

        // ---------- Extra's ----------

        private void loadExtras(int keepId)
        {
            try
            {
                DataTable table = DBHelper.getdataTable(
                    "SELECT ID AS `Nr`, Name AS `Naam`, Description AS `Omschrijving`, Price AS `Prijs`, " +
                    "IF(Is_Active = 1, 'Actief', 'Uit gebruik') AS `Status`, Is_Active AS `IsActive` " +
                    "FROM extra_option ORDER BY Name");

                loadingExtras = true;
                grdExtras.DataSource = table;
                if (grdExtras.Columns.Contains("IsActive"))
                {
                    grdExtras.Columns["IsActive"].Visible = false;
                }
                formatColumn(grdExtras, "Prijs", "C");
                if (grdExtras.Columns.Contains("Nr"))
                {
                    grdExtras.Columns["Nr"].FillWeight = 30;
                }

                // Extra's die uit gebruik zijn staan grijs.
                foreach (DataGridViewRow gridRow in grdExtras.Rows)
                {
                    DataRowView view = gridRow.DataBoundItem as DataRowView;
                    if (view != null && !Convert.ToBoolean(view["IsActive"]))
                    {
                        gridRow.DefaultCellStyle.ForeColor = MutedColor;
                    }
                }

                bool found = keepId != 0 && selectRow(grdExtras, keepId);
                if (!found)
                {
                    grdExtras.ClearSelection();
                    grdExtras.CurrentCell = null;
                }
                loadingExtras = false;

                if (found)
                {
                    showExtra(selectedRow(grdExtras));
                }
                else
                {
                    newExtra();
                }
            }
            catch (Exception ex)
            {
                loadingExtras = false;
                MessageBox.Show("De extra\u2019s konden niet worden geladen: " + ex.Message + "\n\nIs migratie 003 uitgevoerd?",
                    "Extra\u2019s en prijzen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void newExtra()
        {
            selectedExtraId = 0;
            selectedExtraActive = true;
            txtExtraName.Text = "";
            txtExtraDescription.Text = "";
            nudExtraPrice.Value = 0;
            btnExtraToggle.Enabled = false;
            btnExtraToggle.Text = "Uit gebruik nemen";
            setInfo(lblExtraInfo, "Vul de gegevens van een nieuwe extra in en klik op Opslaan.", MutedColor);
        }

        private void showExtra(DataRowView row)
        {
            if (row == null)
            {
                return;
            }

            selectedExtraId = Convert.ToInt32(row["Nr"]);
            selectedExtraActive = Convert.ToBoolean(row["IsActive"]);
            txtExtraName.Text = Convert.ToString(row["Naam"]);
            txtExtraDescription.Text = Convert.ToString(row["Omschrijving"]);
            nudExtraPrice.Value = clampPrice(nudExtraPrice, Convert.ToDecimal(row["Prijs"]));
            btnExtraToggle.Enabled = true;
            btnExtraToggle.Text = selectedExtraActive ? "Uit gebruik nemen" : "Weer in gebruik nemen";
            setInfo(lblExtraInfo,
                "Een prijswijziging geldt alleen voor nieuwe reserveringen. Bestaande reserveringen houden hun prijs.",
                MutedColor);
        }

        private void saveExtra()
        {
            string name = txtExtraName.Text.Trim();
            if (name.Length == 0)
            {
                setInfo(lblExtraInfo, "Vul een naam in.", ErrorColor);
                return;
            }

            decimal price = nudExtraPrice.Value;
            string description = txtExtraDescription.Text.Trim();
            string descriptionSql = description.Length == 0 ? "NULL" : AuthHelper.sql(description);

            try
            {
                int duplicates = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT COUNT(*) FROM extra_option WHERE LOWER(Name) = LOWER(" + AuthHelper.sql(name) + ") " +
                    "AND ID <> " + selectedExtraId));
                if (duplicates > 0)
                {
                    setInfo(lblExtraInfo, "Er bestaat al een extra met deze naam.", ErrorColor);
                    return;
                }

                int keepId = selectedExtraId;
                if (selectedExtraId == 0)
                {
                    DBHelper.RunSql(
                        "INSERT INTO extra_option (Name, Description, Price, Is_Active) VALUES (" +
                        AuthHelper.sql(name) + ", " + descriptionSql + ", " + sqlDecimal(price) + ", 1)");
                    keepId = Convert.ToInt32(DBHelper.getScalar(
                        "SELECT ID FROM extra_option WHERE Name = " + AuthHelper.sql(name) + " ORDER BY ID DESC LIMIT 1"));
                }
                else
                {
                    DBHelper.RunSql(
                        "UPDATE extra_option SET Name = " + AuthHelper.sql(name) + ", Description = " + descriptionSql +
                        ", Price = " + sqlDecimal(price) + " WHERE ID = " + selectedExtraId);
                }

                loadExtras(keepId);
                setInfo(lblExtraInfo,
                    "\u201c" + name + "\u201d is opgeslagen. Geldt alleen voor nieuwe reserveringen.", SuccessColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show("De extra kon niet worden opgeslagen: " + ex.Message, "Extra\u2019s en prijzen",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Een extra wordt niet verwijderd maar uit gebruik genomen: klanten kunnen hem dan niet meer kiezen,
        // maar bestaande reserveringen behouden hem.
        private void toggleExtra()
        {
            if (selectedExtraId == 0)
            {
                return;
            }

            bool newActive = !selectedExtraActive;
            try
            {
                DBHelper.RunSql("UPDATE extra_option SET Is_Active = " + (newActive ? 1 : 0) +
                                " WHERE ID = " + selectedExtraId);
                loadExtras(selectedExtraId);
                setInfo(lblExtraInfo,
                    newActive
                        ? "De extra is weer in gebruik."
                        : "De extra is uit gebruik genomen. Bestaande reserveringen behouden hem.",
                    SuccessColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show("De status kon niet worden gewijzigd: " + ex.Message, "Extra\u2019s en prijzen",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- Dagprijzen van auto's ----------

        private void loadCars(int keepId)
        {
            try
            {
                DataTable table = DBHelper.getdataTable(
                    "SELECT ID AS `Nr`, CONCAT(Brand, ' ', Model, ' (', License_Plate, ')') AS `Auto`, " +
                    "Car_Status AS `Status`, Daily_Price AS `Dagprijs` " +
                    "FROM car ORDER BY Brand, Model, License_Plate");

                // De database bewaart de autostatus in het Engels; op het scherm tonen we hem in het Nederlands.
                foreach (DataRow row in table.Rows)
                {
                    row["Status"] = StatusText.Car(Convert.ToString(row["Status"]));
                }

                loadingCars = true;
                grdCars.DataSource = table;
                formatColumn(grdCars, "Dagprijs", "C");
                if (grdCars.Columns.Contains("Nr"))
                {
                    grdCars.Columns["Nr"].FillWeight = 30;
                }

                bool found = keepId != 0 && selectRow(grdCars, keepId);
                if (!found)
                {
                    grdCars.ClearSelection();
                    grdCars.CurrentCell = null;
                }
                loadingCars = false;

                if (found)
                {
                    showCar(selectedRow(grdCars));
                }
                else
                {
                    clearCar();
                }
            }
            catch (Exception ex)
            {
                loadingCars = false;
                MessageBox.Show("De auto\u2019s konden niet worden geladen: " + ex.Message, "Extra\u2019s en prijzen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void clearCar()
        {
            selectedCarId = 0;
            lblCarName.Text = "Kies een auto in de lijst";
            nudCarPrice.Value = 0;
            nudCarPrice.Enabled = false;
            btnCarSave.Enabled = false;
            setInfo(lblCarInfo, "", MutedColor);
        }

        private void showCar(DataRowView row)
        {
            if (row == null)
            {
                return;
            }

            selectedCarId = Convert.ToInt32(row["Nr"]);
            lblCarName.Text = Convert.ToString(row["Auto"]);
            nudCarPrice.Value = clampPrice(nudCarPrice, Convert.ToDecimal(row["Dagprijs"]));
            nudCarPrice.Enabled = true;
            btnCarSave.Enabled = true;

            string text = "Een nieuwe dagprijs geldt alleen voor nieuwe reserveringen.";
            try
            {
                int running = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT COUNT(*) FROM rental WHERE CarID = " + selectedCarId +
                    " AND Status IN ('reserved', 'ongoing')"));
                if (running > 0)
                {
                    text += "\n" + running + " reservering(en) (gereserveerd of verhuurd) houden hun huidige dagprijs.";
                }
            }
            catch (Exception)
            {
                // Het aantal is alleen informatief.
            }
            setInfo(lblCarInfo, text, MutedColor);
        }

        private void saveCarPrice()
        {
            if (selectedCarId == 0)
            {
                return;
            }

            decimal price = nudCarPrice.Value;
            if (price <= 0)
            {
                setInfo(lblCarInfo, "Vul een dagprijs in die groter is dan 0.", ErrorColor);
                return;
            }

            try
            {
                DBHelper.RunSql("UPDATE car SET Daily_Price = " + sqlDecimal(price) + " WHERE ID = " + selectedCarId);
                int keepId = selectedCarId;
                loadCars(keepId);
                setInfo(lblCarInfo,
                    "De dagprijs is opgeslagen: " + price.ToString("C", Dutch) + ". Geldt alleen voor nieuwe reserveringen.",
                    SuccessColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show("De dagprijs kon niet worden opgeslagen: " + ex.Message, "Extra\u2019s en prijzen",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
