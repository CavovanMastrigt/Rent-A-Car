using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story 3 - add or edit a car in a popup (manager only).
    // Required fields are validated, the licence plate must be unique, and an
    // optional photo can be attached. Returns DialogResult.OK when saved.
    public class CarEditForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");
        private const long MaxPhotoBytes = 5 * 1024 * 1024;

        // Dutch status label <-> stored code (same set as the overview page).
        private static readonly string[] StatusLabels = { "Beschikbaar", "Verhuurd", "In onderhoud", "Buiten gebruik" };
        private static readonly Dictionary<string, string> LabelToCode = new Dictionary<string, string>
        {
            { "Beschikbaar", "active" }, { "Verhuurd", "rented" },
            { "In onderhoud", "maintenance" }, { "Buiten gebruik", "disabled" },
        };
        private static readonly Dictionary<string, string> CodeToLabel = new Dictionary<string, string>
        {
            { "active", "Beschikbaar" }, { "rented", "Verhuurd" },
            { "maintenance", "In onderhoud" }, { "disabled", "Buiten gebruik" },
        };

        private readonly int carId; // 0 = new car

        private TextBox txtPlate, txtBrand, txtModel;
        private NumericUpDown numYear, numPrice;
        private ComboBox cboFuel, cboStatus;
        private PictureBox preview;
        private Label lblPhoto;
        private Button btnRemovePhoto;

        private byte[] photoBytes;   // currently chosen / loaded photo
        private bool photoChanged;   // true once the user picks or clears the photo

        public CarEditForm(int carId)
        {
            this.carId = carId;
            ClientSize = new Size(470, 628);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = carId == 0 ? "Nieuwe auto" : "Auto bewerken";
            buildUi();
            if (carId != 0) loadCar();
        }

        private void buildUi()
        {
            var title = new Label
            {
                Text = carId == 0 ? "Nieuwe auto" : "Auto bewerken",
                Dock = DockStyle.Top,
                Height = 54,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnClose = new Button
            {
                Text = "✕",
                Location = new Point(434, 6),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                ForeColor = Muted,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;

            txtPlate = field("Kenteken", 30, 70, 200, out _);
            cboStatus = new ComboBox { Location = new Point(250, 90), Width = 190, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboStatus.Items.AddRange(StatusLabels);
            cboStatus.SelectedIndex = 0;
            Controls.Add(new Label { Text = "Status", Location = new Point(250, 70), AutoSize = true, ForeColor = Muted });

            txtBrand = field("Merk", 30, 128, 410, out _);
            txtModel = field("Model", 30, 186, 410, out _);

            Controls.Add(new Label { Text = "Bouwjaar", Location = new Point(30, 244), AutoSize = true, ForeColor = Muted });
            numYear = new NumericUpDown
            {
                Location = new Point(30, 264),
                Width = 120,
                Minimum = 1950,
                Maximum = DateTime.Today.Year + 1,
                Value = Math.Min(DateTime.Today.Year, DateTime.Today.Year),
                Font = new Font("Segoe UI", 10F)
            };

            Controls.Add(new Label { Text = "Brandstof", Location = new Point(170, 244), AutoSize = true, ForeColor = Muted });
            cboFuel = new ComboBox { Location = new Point(170, 264), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboFuel.Items.AddRange(new object[] { "Benzine", "Diesel", "Elektrisch", "Hybride", "LPG" });
            cboFuel.SelectedIndex = 0;

            Controls.Add(new Label { Text = "Prijs per dag (€)", Location = new Point(30, 302), AutoSize = true, ForeColor = Muted });
            numPrice = new NumericUpDown
            {
                Location = new Point(30, 322),
                Width = 120,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100000,
                Increment = 5,
                Font = new Font("Segoe UI", 10F)
            };

            Controls.Add(new Label { Text = "Foto (optioneel)", Location = new Point(30, 360), AutoSize = true, ForeColor = Muted });
            var btnPick = new Button
            {
                Text = "Kies foto...",
                Location = new Point(30, 382),
                Size = new Size(130, 28),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPick.Click += btnPick_Click;

            btnRemovePhoto = new Button
            {
                Text = "Verwijder",
                Location = new Point(168, 382),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnRemovePhoto.Click += (s, e) =>
            {
                setPhoto(null);
                photoChanged = true;
            };

            lblPhoto = new Label { Text = "Geen foto gekozen", Location = new Point(280, 388), AutoSize = true, ForeColor = Muted };

            preview = new PictureBox
            {
                Location = new Point(30, 418),
                Size = new Size(220, 130),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(249, 250, 251)
            };

            var btnSave = new Button
            {
                Text = "Opslaan",
                Location = new Point(250, 568),
                Size = new Size(190, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => save();

            var btnCancel = new Button
            {
                Text = "Annuleren",
                Location = new Point(30, 568),
                Size = new Size(150, 40),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };

            Controls.AddRange(new Control[] { title, btnClose, cboStatus, numYear, cboFuel, numPrice, btnPick, btnRemovePhoto, lblPhoto, preview, btnSave, btnCancel });
            title.BringToFront();
            CancelButton = btnCancel;
        }

        // Adds a labelled text box and returns it.
        private TextBox field(string label, int x, int y, int width, out Label lbl)
        {
            lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = Muted };
            var box = new TextBox { Location = new Point(x, y + 20), Width = width, Font = new Font("Segoe UI", 10F) };
            Controls.Add(lbl);
            Controls.Add(box);
            return box;
        }

        private void loadCar()
        {
            DataTable t = DBHelper.getdataTable(
                "SELECT License_Plate, Brand, Model, Build_Date, Daily_Price, Car_Status, Fuel_Type, Photo " +
                "FROM car WHERE ID = " + carId);
            if (t.Rows.Count == 0) return;
            DataRow r = t.Rows[0];

            txtPlate.Text = r["License_Plate"].ToString();
            txtBrand.Text = r["Brand"].ToString();
            txtModel.Text = r["Model"].ToString();

            if (r["Build_Date"] != DBNull.Value)
            {
                int yr = Convert.ToDateTime(r["Build_Date"]).Year;
                numYear.Value = Math.Min(Math.Max(yr, numYear.Minimum), numYear.Maximum);
            }

            decimal price = Convert.ToDecimal(r["Daily_Price"], CultureInfo.InvariantCulture);
            numPrice.Value = Math.Min(Math.Max(price, numPrice.Minimum), numPrice.Maximum);

            string code = r["Car_Status"].ToString();
            cboStatus.SelectedItem = CodeToLabel.ContainsKey(code) ? CodeToLabel[code] : "Beschikbaar";

            string fuel = r["Fuel_Type"] == DBNull.Value ? "" : r["Fuel_Type"].ToString();
            if (fuel != "" && cboFuel.Items.Contains(fuel)) cboFuel.SelectedItem = fuel;

            if (r["Photo"] != DBNull.Value)
            {
                setPhoto((byte[])r["Photo"]);
            }
        }

        private void btnPick_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;*.bmp";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                var file = new FileInfo(dialog.FileName);
                if (!file.Exists) return;
                if (file.Length > MaxPhotoBytes)
                {
                    MessageBox.Show("De foto is groter dan 5 MB.", "Foto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    setPhoto(File.ReadAllBytes(dialog.FileName));
                    photoChanged = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kon de foto niet laden:\n" + ex.Message, "Foto", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void setPhoto(byte[] bytes)
        {
            if (preview.Image != null) { preview.Image.Dispose(); preview.Image = null; }
            photoBytes = bytes;
            if (bytes != null && bytes.Length > 0)
            {
                try { preview.Image = Image.FromStream(new MemoryStream(bytes)); } catch { preview.Image = null; }
                lblPhoto.Text = "Foto gekozen";
                btnRemovePhoto.Visible = true;
            }
            else
            {
                lblPhoto.Text = "Geen foto gekozen";
                btnRemovePhoto.Visible = false;
            }
        }

        private void save()
        {
            string plate = txtPlate.Text.Trim();
            string brand = txtBrand.Text.Trim();
            string model = txtModel.Text.Trim();

            if (plate == "" || brand == "" || model == "")
            {
                MessageBox.Show("Kenteken, merk en model zijn verplicht.", "Auto opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (numPrice.Value <= 0)
            {
                MessageBox.Show("Vul een prijs per dag in die groter is dan 0.", "Auto opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPrice.Focus();
                return;
            }

            // Licence plate must be unique (ignoring this car when editing).
            try
            {
                string esc = plate.Replace("\\", "\\\\").Replace("'", "''");
                object dup = DBHelper.getScalar(
                    "SELECT COUNT(*) FROM car WHERE License_Plate = '" + esc + "' AND ID <> " + carId);
                if (Convert.ToInt32(dup) > 0)
                {
                    MessageBox.Show("Kenteken bestaat al", "Auto opslaan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPlate.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het kenteken niet controleren:\n" + ex.Message, "Auto opslaan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var p = new Dictionary<string, object>
            {
                { "@plate", plate },
                { "@brand", brand },
                { "@model", model },
                { "@build", new DateTime((int)numYear.Value, 1, 1) },
                { "@price", (double)numPrice.Value },
                { "@status", LabelToCode[cboStatus.SelectedItem.ToString()] },
                { "@fuel", cboFuel.SelectedItem == null ? null : cboFuel.SelectedItem.ToString() },
            };

            try
            {
                if (carId == 0)
                {
                    p["@photo"] = photoBytes; // may be null
                    DBHelper.RunSql(
                        "INSERT INTO car (License_Plate, Brand, Model, Build_Date, Daily_Price, Car_Status, Fuel_Type, Photo, Archived) " +
                        "VALUES (@plate, @brand, @model, @build, @price, @status, @fuel, @photo, 0)", p);
                }
                else
                {
                    p["@id"] = carId;
                    // Only overwrite the photo when the user actually changed it.
                    string photoSet = photoChanged ? ", Photo = @photo" : "";
                    if (photoChanged) p["@photo"] = photoBytes;
                    DBHelper.RunSql(
                        "UPDATE car SET License_Plate = @plate, Brand = @brand, Model = @model, " +
                        "Build_Date = @build, Daily_Price = @price, Car_Status = @status, Fuel_Type = @fuel" +
                        photoSet + " WHERE ID = @id", p);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de auto niet opslaan:\n" + ex.Message, "Auto opslaan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && preview != null && preview.Image != null)
            {
                preview.Image.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
