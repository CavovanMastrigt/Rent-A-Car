using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Large popup opened by double-clicking a car. Shows all car info, its rental
    // history, and lets staff record the condition: odometer, tyre pressure,
    // damage notes and a condition status (Goed / Beschadigd / Gerepareerd).
    public class CarDetailForm : PopupForm
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private static readonly string[] CondLabels = { "Goed", "Beschadigd", "Gerepareerd" };
        private static readonly Dictionary<string, string> LabelToCond = new Dictionary<string, string>
        {
            { "Goed", "ok" }, { "Beschadigd", "damaged" }, { "Gerepareerd", "repaired" },
        };
        private static readonly Dictionary<string, string> CondToLabel = new Dictionary<string, string>
        {
            { "ok", "Goed" }, { "damaged", "Beschadigd" }, { "repaired", "Gerepareerd" },
        };

        private readonly int carId;
        private string plate = "";

        private Label lblPlate, lblBrandModel, lblInfo, lblBadge, lblLast;
        private NumericUpDown numOdo;
        private TextBox txtTire, txtNote;
        private ComboBox cboCondition;
        private DataGridView gridRentals, gridMaint;

        public CarDetailForm(int carId)
        {
            this.carId = carId;
            ClientSize = new Size(900, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Auto-details";
            buildUi();
            loadCar();
            loadRentals();
            loadMaintenance();
        }

        private void buildUi()
        {
            var outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(outer);

            // Header
            var head = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            lblPlate = new Label { Location = new Point(24, 12), AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Ink };
            lblBrandModel = new Label { Location = new Point(26, 42), AutoSize = true, Font = new Font("Segoe UI", 10F), ForeColor = Muted };
            lblBadge = new Label { Location = new Point(700, 20), Size = new Size(160, 28), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            var btnClose = new Button { Text = "✕", Location = new Point(864, 6), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            head.Controls.AddRange(new Control[] { lblPlate, lblBrandModel, lblBadge, btnClose });

            // Body: left = info + condition form, right = tabs with history
            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(16, 8, 16, 16) };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            body.Controls.Add(buildLeft(), 0, 0);
            body.Controls.Add(buildRight(), 1, 0);

            outer.Controls.Add(head, 0, 0);
            outer.Controls.Add(body, 0, 1);

            CancelButton = btnClose;
        }

        private Panel buildLeft()
        {
            var p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };

            lblInfo = new Label { Location = new Point(0, 4), Size = new Size(360, 78), ForeColor = Color.FromArgb(55, 65, 81), Font = new Font("Segoe UI", 9.5F) };
            var sep = new Label { Location = new Point(0, 90), Size = new Size(360, 1), BackColor = Border };
            var hdr = new Label { Text = "Toestand bijwerken", Location = new Point(0, 102), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Ink };

            p.Controls.Add(new Label { Text = "Km-teller", Location = new Point(0, 134), AutoSize = true, ForeColor = Muted });
            numOdo = new NumericUpDown { Location = new Point(0, 154), Width = 150, Minimum = 0, Maximum = 5000000, Increment = 100, ThousandsSeparator = true, Font = new Font("Segoe UI", 10F) };

            p.Controls.Add(new Label { Text = "Bandenspanning", Location = new Point(170, 134), AutoSize = true, ForeColor = Muted });
            txtTire = new TextBox { Location = new Point(170, 154), Width = 190, Font = new Font("Segoe UI", 10F) };

            p.Controls.Add(new Label { Text = "Conditie", Location = new Point(0, 192), AutoSize = true, ForeColor = Muted });
            cboCondition = new ComboBox { Location = new Point(0, 212), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboCondition.Items.AddRange(CondLabels);
            cboCondition.SelectedIndex = 0;

            p.Controls.Add(new Label { Text = "Schade / opmerking", Location = new Point(0, 250), AutoSize = true, ForeColor = Muted });
            txtNote = new TextBox { Location = new Point(0, 270), Width = 360, Height = 86, Multiline = true, Font = new Font("Segoe UI", 10F) };

            var btnSave = new Button
            {
                Text = "Opslaan",
                Location = new Point(0, 372),
                Size = new Size(200, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => saveCondition();

            lblLast = new Label { Location = new Point(0, 420), Size = new Size(360, 40), ForeColor = Muted, Font = new Font("Segoe UI", 8.5F) };

            p.Controls.AddRange(new Control[] { lblInfo, sep, hdr, numOdo, txtTire, cboCondition, txtNote, btnSave, lblLast });
            return p;
        }

        private Control buildRight()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F) };

            var tabRent = new TabPage("Verhuringen");
            gridRentals = makeGrid();
            gridRentals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Klant", HeaderText = "Klant", DataPropertyName = "Klant", FillWeight = 85 });
            gridRentals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Periode", HeaderText = "Periode", DataPropertyName = "Periode", FillWeight = 105 });
            gridRentals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 65 });
            var condCol = new DataGridViewTextBoxColumn { Name = "CondCode", DataPropertyName = "CondCode", Visible = false };
            gridRentals.Columns.Add(condCol);
            gridRentals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Conditie", HeaderText = "Conditie (toen)", DataPropertyName = "Conditie", FillWeight = 75 });
            gridRentals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Bedrag", HeaderText = "Bedrag", DataPropertyName = "Bedrag", FillWeight = 55, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            gridRentals.CellFormatting += gridRentals_CellFormatting;
            tabRent.Controls.Add(gridRentals);

            var tabMaint = new TabPage("Onderhoud / schade");
            gridMaint = makeGrid();
            gridMaint.Columns.Add(new DataGridViewTextBoxColumn { Name = "Datum", HeaderText = "Datum", DataPropertyName = "Datum", FillWeight = 85 });
            gridMaint.Columns.Add(new DataGridViewTextBoxColumn { Name = "Door", HeaderText = "Door", DataPropertyName = "Door", FillWeight = 85 });
            gridMaint.Columns.Add(new DataGridViewTextBoxColumn { Name = "Conditie", HeaderText = "Conditie", DataPropertyName = "Conditie", FillWeight = 75 });
            gridMaint.Columns.Add(new DataGridViewTextBoxColumn { Name = "Km", HeaderText = "Km", DataPropertyName = "Km", FillWeight = 55 });
            gridMaint.Columns.Add(new DataGridViewTextBoxColumn { Name = "Band", HeaderText = "Band", DataPropertyName = "Band", FillWeight = 55 });
            gridMaint.Columns.Add(new DataGridViewTextBoxColumn { Name = "Opmerking", HeaderText = "Opmerking", DataPropertyName = "Opmerking", FillWeight = 150 });
            tabMaint.Controls.Add(gridMaint);

            tabs.TabPages.Add(tabRent);
            tabs.TabPages.Add(tabMaint);
            return tabs;
        }

        private DataGridView makeGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 28 }
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 251);
            return g;
        }

        private void loadCar()
        {
            DataTable t = DBHelper.getdataTable(
                "SELECT License_Plate, Brand, Model, Build_Date, Fuel_Type, Daily_Price, Car_Status, " +
                "Odometer, Tire_Pressure, Car_Condition FROM car WHERE ID = " + carId);
            if (t.Rows.Count == 0) return;
            DataRow r = t.Rows[0];

            plate = r["License_Plate"].ToString();
            lblPlate.Text = plate;
            lblBrandModel.Text = r["Brand"] + " " + r["Model"];

            string year = r["Build_Date"] == DBNull.Value ? "-" : Convert.ToDateTime(r["Build_Date"]).Year.ToString();
            string fuel = r["Fuel_Type"] == DBNull.Value || r["Fuel_Type"].ToString() == "" ? "-" : r["Fuel_Type"].ToString();
            double price = Convert.ToDouble(r["Daily_Price"], CultureInfo.InvariantCulture);
            lblInfo.Text =
                "Bouwjaar:        " + year + "\r\n" +
                "Brandstof:       " + fuel + "\r\n" +
                "Prijs per dag:   € " + price.ToString("N2", NL) + "\r\n" +
                "Status:          " + carStatusLabel(r["Car_Status"].ToString());

            string cond = r["Car_Condition"] == DBNull.Value ? "ok" : r["Car_Condition"].ToString();
            lblBadge.Text = CondToLabel.ContainsKey(cond) ? CondToLabel[cond] : "Goed";
            lblBadge.BackColor = condBack(cond);
            lblBadge.ForeColor = condFore(cond);
            cboCondition.SelectedItem = CondToLabel.ContainsKey(cond) ? CondToLabel[cond] : "Goed";

            if (r["Odometer"] != DBNull.Value)
            {
                decimal odo = Convert.ToDecimal(r["Odometer"]);
                numOdo.Value = Math.Min(Math.Max(odo, numOdo.Minimum), numOdo.Maximum);
            }
            txtTire.Text = r["Tire_Pressure"] == DBNull.Value ? "" : r["Tire_Pressure"].ToString();
        }

        private void loadRentals()
        {
            try
            {
                // Condition the car was in at the time of each rental: the most recent
                // maintenance entry logged on or before the rental's start date.
                DataTable t = DBHelper.getdataTable(
                    "SELECT CONCAT(u.Name,' ',u.LastName) AS Klant, r.Date_Started, r.Date_Planned_Ending, r.Status, r.Total_Price, " +
                    "(SELECT cm.Car_Condition FROM car_maintenance cm WHERE cm.CarID = r.CarID AND cm.Created_At <= r.Date_Started ORDER BY cm.ID DESC LIMIT 1) AS CondAtTime " +
                    "FROM rental r JOIN user u ON u.ID = r.UserID WHERE r.CarID = " + carId + " ORDER BY r.Date_Started DESC");

                var d = new DataTable();
                d.Columns.Add("Klant"); d.Columns.Add("Periode"); d.Columns.Add("Status");
                d.Columns.Add("CondCode"); d.Columns.Add("Conditie"); d.Columns.Add("Bedrag");
                foreach (DataRow r in t.Rows)
                {
                    string periode = Convert.ToDateTime(r["Date_Started"]).ToString("dd-MM-yyyy", NL) + " - " +
                                     Convert.ToDateTime(r["Date_Planned_Ending"]).ToString("dd-MM-yyyy", NL);
                    double total = Convert.ToDouble(r["Total_Price"], CultureInfo.InvariantCulture);
                    string condCode = r["CondAtTime"] == DBNull.Value ? "ok" : r["CondAtTime"].ToString();
                    string condLabel = CondToLabel.ContainsKey(condCode) ? CondToLabel[condCode] : "Goed";
                    d.Rows.Add(r["Klant"].ToString(), periode, StatusText.Rental(r["Status"].ToString()),
                        condCode, condLabel, "€ " + total.ToString("N2", NL));
                }
                gridRentals.DataSource = d;
            }
            catch { }
        }

        private void loadMaintenance()
        {
            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT Created_At, By_Name, Car_Condition, Odometer, Tire_Pressure, Note " +
                    "FROM car_maintenance WHERE CarID = " + carId + " ORDER BY ID DESC");

                var d = new DataTable();
                d.Columns.Add("Datum"); d.Columns.Add("Door"); d.Columns.Add("Conditie");
                d.Columns.Add("Km"); d.Columns.Add("Band"); d.Columns.Add("Opmerking");
                foreach (DataRow r in t.Rows)
                {
                    string datum = r["Created_At"] == DBNull.Value ? "" : Convert.ToDateTime(r["Created_At"]).ToString("dd-MM-yyyy HH:mm", NL);
                    string cond = r["Car_Condition"] == DBNull.Value ? "" : r["Car_Condition"].ToString();
                    d.Rows.Add(
                        datum,
                        r["By_Name"] == DBNull.Value ? "" : r["By_Name"].ToString(),
                        CondToLabel.ContainsKey(cond) ? CondToLabel[cond] : cond,
                        r["Odometer"] == DBNull.Value ? "" : r["Odometer"].ToString(),
                        r["Tire_Pressure"] == DBNull.Value ? "" : r["Tire_Pressure"].ToString(),
                        r["Note"] == DBNull.Value ? "" : r["Note"].ToString());
                }
                gridMaint.DataSource = d;
            }
            catch { }
        }

        private void saveCondition()
        {
            string condCode = LabelToCond[cboCondition.SelectedItem.ToString()];
            string note = txtNote.Text.Trim();
            try
            {
                var p = new Dictionary<string, object>
                {
                    { "@cid", carId },
                    { "@by", reviewerName() },
                    { "@note", note },
                    { "@odo", (int)numOdo.Value },
                    { "@tp", txtTire.Text.Trim() },
                    { "@cond", condCode },
                };
                DBHelper.RunSql(
                    "INSERT INTO car_maintenance (CarID, By_Name, Note, Odometer, Tire_Pressure, Car_Condition) " +
                    "VALUES (@cid, @by, @note, @odo, @tp, @cond)", p);
                DBHelper.RunSql(
                    "UPDATE car SET Odometer = @odo, Tire_Pressure = @tp, Car_Condition = @cond WHERE ID = @cid", p);
                LogHelper.Write("Auto bijgewerkt", plate + " - " + cboCondition.SelectedItem +
                    (note == "" ? "" : " (" + note + ")"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de toestand niet opslaan:\n" + ex.Message, "Auto-details", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            txtNote.Text = "";
            loadCar();
            loadMaintenance();
            lblLast.Text = "Laatst bijgewerkt: zojuist.";
        }

        private string reviewerName()
        {
            try
            {
                DataTable t = DBHelper.getdataTable("SELECT Name, LastName FROM user WHERE ID = " + DBHelper.loggedinuserid);
                if (t.Rows.Count > 0) return (t.Rows[0]["Name"] + " " + t.Rows[0]["LastName"]).Trim();
            }
            catch { }
            return "";
        }

        private void gridRentals_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || gridRentals.Columns[e.ColumnIndex].Name != "Conditie") return;
            string code = Convert.ToString(gridRentals.Rows[e.RowIndex].Cells["CondCode"].Value);
            e.CellStyle.ForeColor = condFore(code);
            e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private static string carStatusLabel(string code)
        {
            switch (code)
            {
                case "active": return "Beschikbaar";
                case "rented": return "Verhuurd";
                case "maintenance": return "In onderhoud";
                case "disabled": return "Buiten gebruik";
                default: return code;
            }
        }

        private static Color condBack(string code)
        {
            switch (code)
            {
                case "damaged": return Color.FromArgb(254, 226, 226);
                case "repaired": return Color.FromArgb(219, 234, 254);
                default: return Color.FromArgb(220, 252, 231);
            }
        }

        private static Color condFore(string code)
        {
            switch (code)
            {
                case "damaged": return Color.FromArgb(153, 27, 27);
                case "repaired": return Color.FromArgb(30, 64, 175);
                default: return Color.FromArgb(22, 101, 52);
            }
        }
    }
}
