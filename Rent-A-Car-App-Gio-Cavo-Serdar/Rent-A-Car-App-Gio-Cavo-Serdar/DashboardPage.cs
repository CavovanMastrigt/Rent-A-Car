using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story 1 - Prestaties en omzet.
    // Manager-only dashboard: revenue per day/week/month/year, a chosen period with
    // its rental count, car occupancy, a top 5 of most-rented cars and a revenue chart.
    public class DashboardPage : UserControl
    {
        private static readonly Color CardBg = Color.White;
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private Label valDay, valWeek, valMonth, valYear;
        private Label valRevenue, valRentals, valOccupancy;
        private DateTimePicker dtpStart, dtpEnd;
        private SimpleBarChart chart;
        private DataGridView gridTop;
        private Label lblNoData;
        private Panel bottom;

        public DashboardPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => { loadKpis(); applyPeriod(); };
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(24) };
            Controls.Add(root);

            var header = new Label
            {
                Text = "Dashboard",
                Dock = DockStyle.Top,
                Height = 44,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            // --- KPI row: revenue per day / week / month / year ---
            var kpis = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 104,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0, 8, 0, 8)
            };
            for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            kpis.Controls.Add(makeKpiCard("Omzet vandaag", out valDay), 0, 0);
            kpis.Controls.Add(makeKpiCard("Omzet deze week", out valWeek), 1, 0);
            kpis.Controls.Add(makeKpiCard("Omzet deze maand", out valMonth), 2, 0);
            kpis.Controls.Add(makeKpiCard("Omzet dit jaar", out valYear), 3, 0);

            // --- Period picker ---
            var periodBar = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 10, 0, 10) };
            var lblPeriod = new Label { Text = "Periode:", Location = new Point(2, 20), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(70, 16), Width = 130 };
            var lblTo = new Label { Text = "t/m", Location = new Point(210, 20), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            dtpEnd = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(245, 16), Width = 130 };
            dtpStart.Value = new DateTime(DateTime.Today.Year, 1, 1);
            dtpEnd.Value = DateTime.Today;
            var btnApply = new Button
            {
                Text = "Toepassen",
                Location = new Point(390, 15),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += (s, e) => applyPeriod();
            periodBar.Controls.AddRange(new Control[] { lblPeriod, dtpStart, lblTo, dtpEnd, btnApply });

            // --- Period stats: revenue / rentals / occupancy ---
            var stats = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 104,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 8, 0, 8)
            };
            for (int i = 0; i < 3; i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3f));
            stats.Controls.Add(makeKpiCard("Omzet in periode", out valRevenue), 0, 0);
            stats.Controls.Add(makeKpiCard("Aantal verhuringen", out valRentals), 1, 0);
            stats.Controls.Add(makeKpiCard("Bezettingsgraad", out valOccupancy), 2, 0);

            // --- Bottom: chart (left) + top 5 (right) ---
            bottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));

            var chartCard = makeCard("Omzet over tijd", out Panel chartBody);
            chart = new SimpleBarChart { Dock = DockStyle.Fill };
            chartBody.Controls.Add(chart);

            var topCard = makeCard("Top 5 meest verhuurde auto's", out Panel topBody);
            gridTop = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            gridTop.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            gridTop.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gridTop.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            gridTop.DefaultCellStyle.SelectionForeColor = Color.Black;
            topBody.Controls.Add(gridTop);

            split.Controls.Add(chartCard, 0, 0);
            split.Controls.Add(topCard, 1, 0);

            lblNoData = new Label
            {
                Text = "Geen gegevens voor deze periode",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Muted,
                BackColor = Color.FromArgb(243, 244, 246),
                Visible = false
            };

            bottom.Controls.Add(split);
            bottom.Controls.Add(lblNoData);

            // Add bottom (Fill) first, then the Top sections from bottom to top.
            root.Controls.Add(bottom);
            root.Controls.Add(stats);
            root.Controls.Add(periodBar);
            root.Controls.Add(kpis);
            root.Controls.Add(header);
            header.BringToFront();
        }

        // A white rounded card with a bold caption and a large value label.
        private Panel makeKpiCard(string caption, out Label value)
        {
            var card = makeCard(caption, out Panel body);
            value = new Label
            {
                Text = "-",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft
            };
            body.Controls.Add(value);
            return card;
        }

        // A white card with a header strip and an empty body panel for content.
        private Panel makeCard(string title, out Panel body)
        {
            var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(6), BackColor = CardBg, Padding = new Padding(14, 10, 14, 12) };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };
            body = new Panel { Dock = DockStyle.Fill };
            var head = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Muted
            };
            card.Controls.Add(body);
            card.Controls.Add(head);
            return card;
        }

        private string euro(object value)
        {
            double d = (value == null || value == DBNull.Value)
                ? 0
                : Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return "€ " + d.ToString("N2", NL);
        }

        // Fills the four day/week/month/year revenue cards from the invoices.
        private void loadKpis()
        {
            try
            {
                valDay.Text = euro(revenueWhere("DATE(r.Date_Started) = CURDATE()"));
                valWeek.Text = euro(revenueWhere("YEARWEEK(r.Date_Started, 3) = YEARWEEK(CURDATE(), 3)"));
                valMonth.Text = euro(revenueWhere("YEAR(r.Date_Started) = YEAR(CURDATE()) AND MONTH(r.Date_Started) = MONTH(CURDATE())"));
                valYear.Text = euro(revenueWhere("YEAR(r.Date_Started) = YEAR(CURDATE())"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de omzetcijfers niet laden:\n" + ex.Message, "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private object revenueWhere(string where)
        {
            return DBHelper.getScalar(
                "SELECT COALESCE(SUM(i.Total_Price), 0) FROM rental r " +
                "JOIN invoice i ON i.Rental_ID = r.ID WHERE " + where);
        }

        // Fills the period stats, top 5 and chart for the chosen start/end dates.
        private void applyPeriod()
        {
            DateTime start = dtpStart.Value.Date;
            DateTime end = dtpEnd.Value.Date;
            if (start > end)
            {
                MessageBox.Show("De begindatum ligt na de einddatum.", "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string s = start.ToString("yyyy-MM-dd");
            string e = end.ToString("yyyy-MM-dd");
            string between = "DATE(r.Date_Started) BETWEEN '" + s + "' AND '" + e + "'";

            try
            {
                int rentals = Convert.ToInt32(DBHelper.getScalar("SELECT COUNT(*) FROM rental r WHERE " + between));

                if (rentals == 0)
                {
                    valRevenue.Text = euro(0);
                    valRentals.Text = "0";
                    valOccupancy.Text = "0%";
                    gridTop.DataSource = null;
                    chart.SetData(new List<KeyValuePair<string, double>>());
                    showNoData(true);
                    return;
                }
                showNoData(false);

                object revenue = DBHelper.getScalar(
                    "SELECT COALESCE(SUM(i.Total_Price), 0) FROM rental r " +
                    "JOIN invoice i ON i.Rental_ID = r.ID WHERE " + between);
                valRevenue.Text = euro(revenue);
                valRentals.Text = rentals.ToString();

                int totalCars = Convert.ToInt32(DBHelper.getScalar("SELECT COUNT(*) FROM car"));
                int rentedCars = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT COUNT(DISTINCT r.CarID) FROM rental r " +
                    "WHERE DATE(r.Date_Started) <= '" + e + "' " +
                    "AND (r.Date_Ended IS NULL OR DATE(r.Date_Ended) >= '" + s + "')"));
                double occ = totalCars == 0 ? 0 : (rentedCars * 100.0 / totalCars);
                valOccupancy.Text = occ.ToString("0.#", NL) + "%  (" + rentedCars + "/" + totalCars + ")";

                DataTable top = DBHelper.getdataTable(
                    "SELECT c.Brand, c.Model, c.License_Plate AS Kenteken, COUNT(*) AS Verhuringen, " +
                    "COALESCE(SUM(i.Total_Price), 0) AS Omzet " +
                    "FROM rental r JOIN car c ON c.ID = r.CarID " +
                    "LEFT JOIN invoice i ON i.Rental_ID = r.ID " +
                    "WHERE " + between + " GROUP BY r.CarID, c.Brand, c.Model, c.License_Plate " +
                    "ORDER BY Verhuringen DESC, Omzet DESC LIMIT 5");
                bindTop(top);

                DataTable series = DBHelper.getdataTable(
                    "SELECT DATE_FORMAT(r.Date_Started, '%Y-%m') AS Maand, " +
                    "COALESCE(SUM(i.Total_Price), 0) AS Omzet " +
                    "FROM rental r JOIN invoice i ON i.Rental_ID = r.ID " +
                    "WHERE " + between + " GROUP BY Maand ORDER BY Maand");
                var points = new List<KeyValuePair<string, double>>();
                foreach (DataRow row in series.Rows)
                {
                    string ym = row["Maand"].ToString();
                    string label = ym;
                    DateTime parsed;
                    if (DateTime.TryParseExact(ym + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                    {
                        label = parsed.ToString("MMM yy", NL);
                    }
                    points.Add(new KeyValuePair<string, double>(label, Convert.ToDouble(row["Omzet"], CultureInfo.InvariantCulture)));
                }
                chart.SetData(points);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de periode niet laden:\n" + ex.Message, "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bindTop(DataTable top)
        {
            // Format the revenue column as euro text for display.
            var display = new DataTable();
            display.Columns.Add("Kenteken");
            display.Columns.Add("Auto");
            display.Columns.Add("Verhuringen");
            display.Columns.Add("Omzet");
            foreach (DataRow r in top.Rows)
            {
                display.Rows.Add(
                    r["Kenteken"].ToString(),
                    r["Brand"] + " " + r["Model"],
                    r["Verhuringen"].ToString(),
                    euro(r["Omzet"]));
            }
            gridTop.DataSource = display;
            if (gridTop.Columns.Count > 0)
            {
                gridTop.Columns["Verhuringen"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                gridTop.Columns["Omzet"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void showNoData(bool show)
        {
            lblNoData.Visible = show;
            if (show) lblNoData.BringToFront();
        }

        // A lightweight bar chart drawn with GDI+ (no extra chart assembly needed).
        private class SimpleBarChart : Panel
        {
            private List<KeyValuePair<string, double>> data = new List<KeyValuePair<string, double>>();

            public SimpleBarChart()
            {
                DoubleBuffered = true;
                BackColor = Color.White;
            }

            public void SetData(List<KeyValuePair<string, double>> points)
            {
                data = points ?? new List<KeyValuePair<string, double>>();
                Invalidate();
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (data.Count == 0)
                {
                    return;
                }

                int left = 48, right = 12, top = 12, bottom = 28;
                int plotW = Width - left - right;
                int plotH = Height - top - bottom;
                if (plotW <= 10 || plotH <= 10) return;

                double max = data.Max(p => p.Value);
                if (max <= 0) max = 1;

                using (var axisPen = new Pen(Color.FromArgb(229, 231, 235)))
                using (var barBrush = new SolidBrush(Color.FromArgb(37, 99, 235)))
                using (var textBrush = new SolidBrush(Color.FromArgb(107, 114, 128)))
                using (var font = new Font("Segoe UI", 8F))
                {
                    // Gridlines + y labels (0, half, max).
                    for (int i = 0; i <= 2; i++)
                    {
                        int y = top + plotH - (int)(plotH * i / 2.0);
                        g.DrawLine(axisPen, left, y, left + plotW, y);
                        string lbl = "€" + (max * i / 2.0).ToString("0");
                        g.DrawString(lbl, font, textBrush, 2, y - 7);
                    }

                    int n = data.Count;
                    float slot = plotW / (float)n;
                    float barW = Math.Min(48f, slot * 0.6f);
                    using (var fmt = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                    {
                        for (int i = 0; i < n; i++)
                        {
                            double val = data[i].Value;
                            int h = (int)(plotH * (val / max));
                            float x = left + slot * i + (slot - barW) / 2f;
                            int y = top + plotH - h;
                            g.FillRectangle(barBrush, x, y, barW, h);
                            g.DrawString(data[i].Key, font, textBrush,
                                new RectangleF(left + slot * i, top + plotH + 6, slot, 16), fmt);
                        }
                    }
                }
            }
        }
    }
}
