using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Employee screen (user story 1): all pick-ups and returns planned for one day, sorted by time.
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

            // Table with the reservations. Added first so it fills what the top panel leaves over.
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
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 36;
            grid.DefaultCellStyle.ForeColor = TextColor;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
            grid.DefaultCellStyle.SelectionForeColor = TextColor;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            // Top panel with the day picker.
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
            Button btnToday = makeButton("Today", 346, 80);
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

        // Pick-ups use the planned start of a reservation, returns the planned end. Cancelled reservations are left out.
        private void loadData()
        {
            string day = "'" + dtpDay.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";

            string sql =
                "SELECT r.ID AS `No`, TIME_FORMAT(r.Date_Started, '%H:%i') AS `Time`, 'Pick-up' AS `Type`, " +
                "CONCAT(u.Name, ' ', u.LastName) AS `Customer`, " +
                "CONCAT(c.Brand, ' ', c.Model, ' (', c.License_Plate, ')') AS `Car`, r.Status AS `Status` " +
                "FROM rental r JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID " +
                "WHERE DATE(r.Date_Started) = " + day + " AND r.Status <> 'cancelled' " +
                "UNION ALL " +
                "SELECT r.ID, TIME_FORMAT(r.Date_Planned_Ending, '%H:%i'), 'Return', " +
                "CONCAT(u.Name, ' ', u.LastName), " +
                "CONCAT(c.Brand, ' ', c.Model, ' (', c.License_Plate, ')'), r.Status " +
                "FROM rental r JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID " +
                "WHERE DATE(r.Date_Planned_Ending) = " + day + " AND r.Status <> 'cancelled' " +
                "ORDER BY `Time`, `Type`";

            try
            {
                DataTable table = DBHelper.getdataTable(sql);
                grid.DataSource = table;

                if (grid.Columns.Contains("No"))
                {
                    grid.Columns["No"].FillWeight = 30;
                }
                if (grid.Columns.Contains("Time"))
                {
                    grid.Columns["Time"].FillWeight = 40;
                }
                if (grid.Columns.Contains("Type"))
                {
                    grid.Columns["Type"].FillWeight = 60;
                }

                int pickUps = 0;
                int returns = 0;
                foreach (DataRow row in table.Rows)
                {
                    if (row["Type"].ToString() == "Pick-up")
                    {
                        pickUps++;
                    }
                    else
                    {
                        returns++;
                    }
                }

                lblSummary.Text = pickUps + " pick-up(s), " + returns + " return(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The reservations could not be loaded: " + ex.Message, "Day overview",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
