using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Medewerkerscherm (user story 1): alle ophalingen en terugbrengingen van één dag, gesorteerd op tijd.
    public class DayOverviewControl : UserControl
    {
        private static readonly Color AccentColor = Color.FromArgb(79, 70, 229);
        private static readonly Color HeaderColor = Color.FromArgb(243, 244, 246);
        private static readonly Color TextColor = Color.FromArgb(31, 41, 55);
        private static readonly Color MutedColor = Color.FromArgb(107, 114, 128);

        private readonly DateTimePicker dtpDay;
        private readonly Label lblSummary;
        private readonly DataGridView grid;
        private bool ready = false;

        public DayOverviewControl()
        {
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.75F);

            // Tabel met de reserveringen. Eerst toegevoegd, zodat hij de ruimte vult die het bovenpaneel overlaat.
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
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            // Bovenpaneel met de dagkiezer.
            var top = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };

            Button btnPrev = makeButton("‹", 16, 36);
            dtpDay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Long,
                Location = new Point(58, 15),
                Width = 240,
                Value = DateTime.Today
            };
            Button btnNext = makeButton("›", 304, 36);
            Button btnToday = makeButton("Vandaag", 346, 80);
            btnToday.BackColor = AccentColor;
            btnToday.ForeColor = Color.White;

            lblSummary = new Label
            {
                AutoSize = true,
                Location = new Point(444, 19),
                ForeColor = MutedColor
            };

            top.Controls.Add(btnPrev);
            top.Controls.Add(dtpDay);
            top.Controls.Add(btnNext);
            top.Controls.Add(btnToday);
            top.Controls.Add(lblSummary);

            Controls.Add(grid);
            Controls.Add(top);

            btnPrev.Click += (s, e) => dtpDay.Value = dtpDay.Value.AddDays(-1);
            btnNext.Click += (s, e) => dtpDay.Value = dtpDay.Value.AddDays(1);
            btnToday.Click += (s, e) => dtpDay.Value = DateTime.Today;
            dtpDay.ValueChanged += (s, e) =>
            {
                if (ready)
                {
                    loadData();
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
            loadData();
        }

        private static Button makeButton(string text, int left, int width)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(left, 12),
                Size = new Size(width, 32),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = HeaderColor,
                ForeColor = TextColor
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        // Ophalingen gebruiken de geplande start van een reservering, terugbrengingen het geplande einde. Geannuleerde reserveringen blijven weg.
        private void loadData()
        {
            string day = "'" + dtpDay.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";

            string sql =
                "SELECT r.ID AS `Nr`, TIME_FORMAT(r.Date_Started, '%H:%i') AS `Tijd`, 'Ophalen' AS `Soort`, " +
                "CONCAT(u.Name, ' ', u.LastName) AS `Klant`, " +
                "CONCAT(c.Brand, ' ', c.Model, ' (', c.License_Plate, ')') AS `Auto`, r.Status AS `Status` " +
                "FROM rental r JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID " +
                "WHERE DATE(r.Date_Started) = " + day + " AND r.Status <> 'cancelled' " +
                "UNION ALL " +
                "SELECT r.ID, TIME_FORMAT(r.Date_Planned_Ending, '%H:%i'), 'Terugbrengen', " +
                "CONCAT(u.Name, ' ', u.LastName), " +
                "CONCAT(c.Brand, ' ', c.Model, ' (', c.License_Plate, ')'), r.Status " +
                "FROM rental r JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID " +
                "WHERE DATE(r.Date_Planned_Ending) = " + day + " AND r.Status <> 'cancelled' " +
                "ORDER BY `Tijd`, `Soort`";

            try
            {
                DataTable table = DBHelper.getdataTable(sql);

                // De database bewaart de statussen in het Engels; op het scherm tonen we ze in het Nederlands.
                foreach (DataRow row in table.Rows)
                {
                    row["Status"] = StatusText.Rental(Convert.ToString(row["Status"]));
                }

                grid.DataSource = table;

                if (grid.Columns.Contains("Nr"))
                {
                    grid.Columns["Nr"].FillWeight = 30;
                }
                if (grid.Columns.Contains("Tijd"))
                {
                    grid.Columns["Tijd"].FillWeight = 40;
                }
                if (grid.Columns.Contains("Soort"))
                {
                    grid.Columns["Soort"].FillWeight = 60;
                }

                int pickUps = 0;
                int returns = 0;
                foreach (DataRow row in table.Rows)
                {
                    if (row["Soort"].ToString() == "Ophalen")
                    {
                        pickUps++;
                    }
                    else
                    {
                        returns++;
                    }
                }

                lblSummary.Text = pickUps + " ophaling(en), " + returns + " terugbrenging(en)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("De reserveringen konden niet worden geladen: " + ex.Message, "Dagoverzicht",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}