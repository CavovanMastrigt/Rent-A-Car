using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Employee screen (user story 2): week planning with every car as a row and the days of the week as columns.
    // A cell shows who has the car that day, or that the car is in maintenance / out of service.
    public class PlanningControl : UserControl
    {
        private static readonly Color AccentColor = Color.FromArgb(79, 70, 229);
        private static readonly Color HeaderColor = Color.FromArgb(243, 244, 246);
        private static readonly Color TextColor = Color.FromArgb(31, 41, 55);
        private static readonly Color MutedColor = Color.FromArgb(107, 114, 128);
        private static readonly Color BorderColor = Color.FromArgb(229, 231, 235);

        private static readonly Color ReservedColor = Color.FromArgb(224, 231, 255);
        private static readonly Color OngoingColor = Color.FromArgb(209, 250, 229);
        private static readonly Color CompletedColor = Color.FromArgb(229, 231, 235);
        private static readonly Color MaintenanceColor = Color.FromArgb(254, 243, 199);
        private static readonly Color OutOfServiceColor = Color.FromArgb(254, 226, 226);

        // One reservation, kept in the Tag of every cell it covers.
        private class Booking
        {
            public int RentalId;
            public int CarId;
            public string Customer;
            public string Status;
            public DateTime Start;
            public DateTime End;
        }

        private readonly Label lblRange;
        private readonly Label lblDetails;
        private readonly DataGridView grid;
        private DateTime weekStart;
        private bool ready = false;

        public PlanningControl()
        {
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.75F);
            weekStart = mondayOf(DateTime.Today);

            // Table: first column is the car, then one column per day. Added first so it fills what is left over.
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                GridColor = BorderColor,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 46 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextColor;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersHeight = 38;
            grid.DefaultCellStyle.ForeColor = TextColor;
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.CellClick += (s, e) => showDetails(e.RowIndex, e.ColumnIndex);

            // Details of the clicked cell, below the table.
            lblDetails = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Color.FromArgb(249, 250, 251),
                ForeColor = TextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 16, 0),
                Text = "Click a cell to see the details."
            };

            // Top panel with the week navigation and the legend.
            var top = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };

            Button btnPrev = makeButton("‹", 16, 36);
            lblRange = new Label
            {
                AutoSize = false,
                Location = new Point(58, 12),
                Size = new Size(250, 32),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            Button btnNext = makeButton("›", 314, 36);
            Button btnThisWeek = makeButton("This week", 356, 96);
            btnThisWeek.BackColor = AccentColor;
            btnThisWeek.ForeColor = Color.White;
            Button btnRefresh = makeButton("Refresh", 460, 80);

            var legend = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Padding = new Padding(0, 14, 12, 0)
            };
            legend.Controls.Add(makeLegend("Reserved", ReservedColor));
            legend.Controls.Add(makeLegend("Ongoing", OngoingColor));
            legend.Controls.Add(makeLegend("Completed", CompletedColor));
            legend.Controls.Add(makeLegend("Maintenance", MaintenanceColor));
            legend.Controls.Add(makeLegend("Out of service", OutOfServiceColor));

            top.Controls.Add(btnPrev);
            top.Controls.Add(lblRange);
            top.Controls.Add(btnNext);
            top.Controls.Add(btnThisWeek);
            top.Controls.Add(btnRefresh);
            top.Controls.Add(legend);

            Controls.Add(grid);
            Controls.Add(lblDetails);
            Controls.Add(top);

            btnPrev.Click += (s, e) => changeWeek(-7);
            btnNext.Click += (s, e) => changeWeek(7);
            btnThisWeek.Click += (s, e) =>
            {
                weekStart = mondayOf(DateTime.Today);
                loadData();
            };
            btnRefresh.Click += (s, e) => loadData();

            // Reservations can change on other tabs, so reload every time this tab is shown again.
            VisibleChanged += (s, e) =>
            {
                if (ready && Visible)
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

        private static DateTime mondayOf(DateTime date)
        {
            return date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
        }

        private static string sqlDate(DateTime date)
        {
            return "'" + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";
        }

        private static string shortDateTime(DateTime date)
        {
            return date.ToString("d MMM HH:mm", CultureInfo.InvariantCulture);
        }

        private static Color darker(Color color)
        {
            return Color.FromArgb(Math.Max(0, color.R - 22), Math.Max(0, color.G - 22), Math.Max(0, color.B - 22));
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

        private static Label makeLegend(string text, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                BackColor = color,
                ForeColor = TextColor,
                Padding = new Padding(8, 4, 8, 4),
                Margin = new Padding(4, 0, 4, 0),
                Font = new Font("Segoe UI", 9F)
            };
        }

        private void changeWeek(int days)
        {
            weekStart = weekStart.AddDays(days);
            loadData();
        }

        // Loads all cars and all reservations that touch the shown week (cancelled ones are left out).
        private void loadData()
        {
            lblRange.Text = weekStart.ToString("d MMM", CultureInfo.InvariantCulture) + " – " +
                            weekStart.AddDays(6).ToString("d MMM yyyy", CultureInfo.InvariantCulture);

            string start = sqlDate(weekStart);
            string endExclusive = sqlDate(weekStart.AddDays(7));

            try
            {
                DataTable cars = DBHelper.getdataTable(
                    "SELECT ID, License_Plate, Brand, Model, Car_Status FROM car " +
                    "ORDER BY Brand, Model, License_Plate");

                DataTable rentals = DBHelper.getdataTable(
                    "SELECT r.ID AS RentalID, r.CarID AS CarID, r.Date_Started AS StartAt, " +
                    "r.Date_Planned_Ending AS EndAt, r.Status AS RentalStatus, " +
                    "CONCAT(u.Name, ' ', u.LastName) AS Customer " +
                    "FROM rental r JOIN user u ON u.ID = r.UserID " +
                    "WHERE r.Status <> 'cancelled' " +
                    "AND DATE(r.Date_Started) < " + endExclusive + " " +
                    "AND DATE(r.Date_Planned_Ending) >= " + start);

                fillGrid(cars, rentals);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The planning could not be loaded: " + ex.Message, "Planning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void fillGrid(DataTable cars, DataTable rentals)
        {
            var bookings = new List<Booking>();
            foreach (DataRow row in rentals.Rows)
            {
                bookings.Add(new Booking
                {
                    RentalId = Convert.ToInt32(row["RentalID"]),
                    CarId = Convert.ToInt32(row["CarID"]),
                    Customer = Convert.ToString(row["Customer"]),
                    Status = Convert.ToString(row["RentalStatus"]),
                    Start = Convert.ToDateTime(row["StartAt"]),
                    End = Convert.ToDateTime(row["EndAt"])
                });
            }

            grid.Rows.Clear();
            grid.Columns.Clear();

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "car",
                HeaderText = "Car",
                FillWeight = 150,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            for (int i = 0; i < 7; i++)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "day" + i,
                    HeaderText = weekStart.AddDays(i).ToString("ddd d MMM", CultureInfo.InvariantCulture),
                    FillWeight = 100,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }

            foreach (DataRow car in cars.Rows)
            {
                int carId = Convert.ToInt32(car["ID"]);
                string carStatus = Convert.ToString(car["Car_Status"]).ToLowerInvariant();

                int rowIndex = grid.Rows.Add();
                DataGridViewRow row = grid.Rows[rowIndex];

                DataGridViewCell carCell = row.Cells[0];
                carCell.Value = Convert.ToString(car["Brand"]) + " " + Convert.ToString(car["Model"]) +
                                "\n" + Convert.ToString(car["License_Plate"]);
                carCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                carCell.Style.Padding = new Padding(8, 0, 0, 0);
                carCell.Style.BackColor = Color.White;
                carCell.Style.SelectionBackColor = HeaderColor;
                carCell.Style.SelectionForeColor = TextColor;

                for (int i = 0; i < 7; i++)
                {
                    DateTime day = weekStart.AddDays(i).Date;
                    DataGridViewCell cell = row.Cells[i + 1];
                    Booking booking = findBooking(bookings, carId, day);

                    if (booking != null)
                    {
                        setCell(cell, booking.Customer, colorFor(booking.Status));
                        cell.Tag = booking;
                    }
                    else if (carStatus == "maintenance")
                    {
                        setCell(cell, "Maintenance", MaintenanceColor);
                        cell.Tag = "The car is in maintenance.";
                    }
                    else if (carStatus == "disabled")
                    {
                        setCell(cell, "Out of service", OutOfServiceColor);
                        cell.Tag = "The car is out of service.";
                    }
                    else
                    {
                        setCell(cell, "", Color.White);
                    }
                }
            }

            grid.ClearSelection();
            lblDetails.Text = cars.Rows.Count == 0
                ? "There are no cars yet."
                : "Click a cell to see the details.";
        }

        private static Booking findBooking(List<Booking> bookings, int carId, DateTime day)
        {
            foreach (Booking booking in bookings)
            {
                if (booking.CarId == carId && booking.Start.Date <= day && booking.End.Date >= day)
                {
                    return booking;
                }
            }
            return null;
        }

        private static Color colorFor(string rentalStatus)
        {
            switch ((rentalStatus ?? "").ToLowerInvariant())
            {
                case "ongoing":
                    return OngoingColor;
                case "completed":
                    return CompletedColor;
                default:
                    return ReservedColor;
            }
        }

        private static void setCell(DataGridViewCell cell, string text, Color back)
        {
            cell.Value = text;
            cell.Style.BackColor = back;
            cell.Style.SelectionBackColor = back == Color.White ? HeaderColor : darker(back);
            cell.Style.SelectionForeColor = TextColor;
        }

        private void showDetails(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || columnIndex < 0)
            {
                return;
            }

            string carName = Convert.ToString(grid[0, rowIndex].Value).Replace("\n", " ");
            if (columnIndex == 0)
            {
                lblDetails.Text = carName;
                return;
            }

            DateTime day = weekStart.AddDays(columnIndex - 1);
            string dayText = day.ToString("ddd d MMM", CultureInfo.InvariantCulture);
            object tag = grid[columnIndex, rowIndex].Tag;
            Booking booking = tag as Booking;

            if (booking != null)
            {
                lblDetails.Text = carName + "  |  " + dayText + "  |  Reservation #" + booking.RentalId + ": " +
                                  booking.Customer + ", " + booking.Status + ", " +
                                  shortDateTime(booking.Start) + " to " + shortDateTime(booking.End);
            }
            else if (tag is string)
            {
                lblDetails.Text = carName + "  |  " + dayText + "  |  " + (string)tag;
            }
            else
            {
                lblDetails.Text = carName + "  |  " + dayText + "  |  Free.";
            }
        }
    }
}
