using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Shown when staff set a car back to "Beschikbaar" (returned). They must enter
    // the car's current state (km, tyre pressure, condition, damage) before it is
    // made available again. On save it logs the state and sets the status to active.
    public class CarReturnForm : PopupForm
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

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
        private readonly string plate;
        private int previousOdo;

        private NumericUpDown numOdo;
        private TextBox txtTire, txtNote;
        private ComboBox cboCondition;

        public CarReturnForm(int carId, string plate)
        {
            this.carId = carId;
            this.plate = plate;
            ClientSize = new Size(440, 440);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Auto teruggebracht";
            buildUi();
            loadCurrent();
        }

        private void buildUi()
        {
            var title = new Label { Text = "Auto teruggebracht", Dock = DockStyle.Top, Height = 48, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Ink, TextAlign = ContentAlignment.MiddleCenter };
            var sub = new Label { Text = "Voer de huidige staat van " + plate + " in voordat de auto weer beschikbaar wordt.", Dock = DockStyle.Top, Height = 38, ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter };
            var btnClose = new Button { Text = "✕", Location = new Point(404, 6), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            Controls.Add(new Label { Text = "Km-teller", Location = new Point(30, 108), AutoSize = true, ForeColor = Muted });
            numOdo = new NumericUpDown { Location = new Point(30, 128), Width = 150, Minimum = 0, Maximum = 5000000, Increment = 100, ThousandsSeparator = true, Font = new Font("Segoe UI", 10F) };

            Controls.Add(new Label { Text = "Bandenspanning", Location = new Point(200, 108), AutoSize = true, ForeColor = Muted });
            txtTire = new TextBox { Location = new Point(200, 128), Width = 200, Font = new Font("Segoe UI", 10F) };

            Controls.Add(new Label { Text = "Conditie", Location = new Point(30, 170), AutoSize = true, ForeColor = Muted });
            cboCondition = new ComboBox { Location = new Point(30, 190), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboCondition.Items.AddRange(CondLabels);
            cboCondition.SelectedIndex = 0;

            Controls.Add(new Label { Text = "Schade / opmerking", Location = new Point(30, 232), AutoSize = true, ForeColor = Muted });
            txtNote = new TextBox { Location = new Point(30, 252), Width = 370, Height = 90, Multiline = true, Font = new Font("Segoe UI", 10F) };

            var btnSave = new Button { Text = "Opslaan & beschikbaar", Location = new Point(190, 372), Size = new Size(210, 42), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => save();
            var btnCancel = new Button { Text = "Annuleren", Location = new Point(30, 372), Size = new Size(140, 42), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, Cursor = Cursors.Hand };

            Controls.AddRange(new Control[] { title, sub, btnClose, numOdo, txtTire, cboCondition, txtNote, btnSave, btnCancel });
            title.BringToFront();
            CancelButton = btnCancel;
        }

        private void loadCurrent()
        {
            try
            {
                DataTable t = DBHelper.getdataTable("SELECT Odometer, Tire_Pressure, Car_Condition FROM car WHERE ID = " + carId);
                if (t.Rows.Count == 0) return;
                DataRow r = t.Rows[0];
                if (r["Odometer"] != DBNull.Value)
                {
                    previousOdo = Convert.ToInt32(r["Odometer"]);
                    numOdo.Value = Math.Min(Math.Max(previousOdo, numOdo.Minimum), numOdo.Maximum);
                }
                txtTire.Text = r["Tire_Pressure"] == DBNull.Value ? "" : r["Tire_Pressure"].ToString();
                string cond = r["Car_Condition"] == DBNull.Value ? "ok" : r["Car_Condition"].ToString();
                cboCondition.SelectedItem = CondToLabel.ContainsKey(cond) ? CondToLabel[cond] : "Goed";
            }
            catch { }
        }

        private void save()
        {
            // The km-teller and tyre pressure must be recorded on return.
            if ((int)numOdo.Value <= 0)
            {
                MessageBox.Show("Vul de km-teller in.", "Auto teruggebracht", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numOdo.Focus();
                return;
            }
            if ((int)numOdo.Value < previousOdo)
            {
                MessageBox.Show("De km-stand kan niet lager zijn dan de vorige stand (" + previousOdo.ToString("N0") + " km).",
                    "Auto teruggebracht", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numOdo.Focus();
                return;
            }
            if (txtTire.Text.Trim() == "")
            {
                MessageBox.Show("Vul de bandenspanning in.", "Auto teruggebracht", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTire.Focus();
                return;
            }

            string condCode = LabelToCond[cboCondition.SelectedItem.ToString()];
            string note = txtNote.Text.Trim();
            try
            {
                var p = new Dictionary<string, object>
                {
                    { "@cid", carId },
                    { "@by", reviewerName() },
                    { "@note", note == "" ? "Teruggebracht" : note },
                    { "@odo", (int)numOdo.Value },
                    { "@tp", txtTire.Text.Trim() },
                    { "@cond", condCode },
                };
                DBHelper.RunSql(
                    "INSERT INTO car_maintenance (CarID, By_Name, Note, Odometer, Tire_Pressure, Car_Condition) " +
                    "VALUES (@cid, @by, @note, @odo, @tp, @cond)", p);
                DBHelper.RunSql(
                    "UPDATE car SET Odometer = @odo, Tire_Pressure = @tp, Car_Condition = @cond, Car_Status = 'active' WHERE ID = @cid", p);
                LogHelper.Write("Auto teruggebracht", plate + " - " + cboCondition.SelectedItem + (note == "" ? "" : " (" + note + ")"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de staat niet opslaan:\n" + ex.Message, "Auto teruggebracht", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
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
    }
}
