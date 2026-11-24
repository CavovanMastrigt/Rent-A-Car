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
    // Manager dashboard in two clear zones:
    //   1. "Vandaag" - fixed revenue snapshot for day / week / month / year.
    //   2. "Analyse per periode" - revenue, rentals, occupancy, a revenue chart and
    //      a top 10 for a date range you choose and can cycle through.
    // The whole page sits in one root grid (TableLayoutPanel) so bands never overlap.
    public class DashboardPage : UserControl
    {
        private static readonly Color PageBg = Color.FromArgb(243, 244, 246);
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        private Label valDay, valWeek, valMonth, valYear;
        private Label valRevenue, valRentals, valOccupancy, subOccupancy, subRevenue;
        private Label lblPeriodHeading, lblChartTitle;
        private DateTimePicker dtpStart, dtpEnd;
        private SimpleBarChart chart;
        private DataGridView gridTop;
        private Label lblNoData;

        // Per-day view (toggled with the period view).
        private bool dayMode;
        private Button btnModePeriod, btnModeDay;
        private Control periodControls, periodTiles, periodBottom;
        private Control dayControls, dayTiles, dayBottom;
        private DateTimePicker dtpDay;
        private Label valDayRevenue, valDayStarts, valDayActive, lblDayTitle, lblDayEmpty;
        private DataGridView gridDay;

        public DashboardPage()
        {
            BackColor = PageBg;
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => { loadKpis(); applyPeriod(); };
        }

        private void buildUi()
        {
            // One root grid with explicit rows - fully deterministic, no docking games.
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(24, 16, 24, 20),
                BackColor = PageBg
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));   // 0 header
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));   // 1 "VANDAAG"
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104f));  // 2 snapshot tiles
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));   // 3 analysis heading
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));   // 4 period controls
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104f));  // 5 period tiles
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // 6 chart + top 10
            Controls.Add(root);

            var header = new Label
            {
                Text = "Dashboard",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Ink,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Zone 1 - fixed "today" snapshot.
            var snapRow = tileRow(4);
            snapRow.Controls.Add(makeKpi("Omzet vandaag", out valDay, out _), 0, 0);
            snapRow.Controls.Add(makeKpi("Omzet deze week", out valWeek, out _), 1, 0);
            snapRow.Controls.Add(makeKpi("Omzet deze maand", out valMonth, out _), 2, 0);
            snapRow.Controls.Add(makeKpi("Omzet dit jaar", out valYear, out _), 3, 0);

            // Zone 2 - analysis, with a "Per periode / Per dag" switch.
            lblPeriodHeading = sectionLabel("ANALYSE PER PERIODE");
            var headingRow = buildHeadingRow();

            // Row 4 - controls: period range pickers OR a single-day navigator.
            periodControls = buildPeriodBar();
            dayControls = buildDayBar();
            var controlsHost = new Panel { Dock = DockStyle.Fill };
            controlsHost.Controls.Add(dayControls);
            controlsHost.Controls.Add(periodControls);

            // Row 5 - tiles: period figures OR that day's figures.
            var statsRow = tileRow(3);
            statsRow.Controls.Add(makeKpi("Omzet in periode", out valRevenue, out subRevenue), 0, 0);
            statsRow.Controls.Add(makeKpi("Aantal verhuringen", out valRentals, out _), 1, 0);
            statsRow.Controls.Add(makeKpi("Bezettingsgraad", out valOccupancy, out subOccupancy), 2, 0);
            periodTiles = statsRow;
            dayTiles = buildDayTiles();
            var tilesHost = new Panel { Dock = DockStyle.Fill };
            tilesHost.Controls.Add(dayTiles);
            tilesHost.Controls.Add(periodTiles);

            // Row 6 - bottom: chart + top 10 OR that day's reservation list.
            var bottom = buildBottom();

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(sectionLabel("VANDAAG"), 0, 1);
            root.Controls.Add(snapRow, 0, 2);
            root.Controls.Add(headingRow, 0, 3);
            root.Controls.Add(controlsHost, 0, 4);
            root.Controls.Add(tilesHost, 0, 5);
            root.Controls.Add(bottom, 0, 6);

            styleSeg(btnModePeriod, true);
            styleSeg(btnModeDay, false);
        }

        // Heading text on the left, the period/day toggle on the right.
        private Control buildHeadingRow()
        {
            var p = new Panel { Dock = DockStyle.Fill };
            var toggle = new FlowLayoutPanel { Dock = DockStyle.Right, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0) };
            btnModePeriod = segBtn("Per periode");
            btnModePeriod.Click += (s, e) => setMode(false);
            btnModeDay = segBtn("Per dag");
            btnModeDay.Click += (s, e) => setMode(true);
            toggle.Controls.Add(btnModePeriod);
            toggle.Controls.Add(btnModeDay);

            lblPeriodHeading.Dock = DockStyle.Fill;
            p.Controls.Add(toggle);
            p.Controls.Add(lblPeriodHeading);
            return p;
        }

        private Button segBtn(string text)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(94, 24),
                Margin = new Padding(4, 2, 0, 0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private void styleSeg(Button b, bool active)
        {
            b.BackColor = active ? Accent : Color.White;
            b.ForeColor = active ? Color.White : Ink;
        }

        // Switches the whole analysis zone between the period view and the day view.
        private void setMode(bool day)
        {
            dayMode = day;
            styleSeg(btnModeDay, day);
            styleSeg(btnModePeriod, !day);

            periodControls.Visible = !day;
            periodTiles.Visible = !day;
            periodBottom.Visible = !day;
            dayControls.Visible = day;
            dayTiles.Visible = day;
            dayBottom.Visible = day;
            if (!day) { } // period view manages its own empty state
            else lblNoData.Visible = false;

            if (day) applyDay();
            else applyPeriod();
        }

        private FlowLayoutPanel buildPeriodBar()
        {
            // The pickers accept typing (click a part, type) and a calendar dropdown.
            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0, 2, 0, 8) };
            dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Margin = new Padding(0, 4, 4, 0) };
            var lblTo = new Label { Text = "t/m", AutoSize = true, Margin = new Padding(2, 8, 2, 0), Font = new Font("Segoe UI", 10F) };
            dtpEnd = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Margin = new Padding(0, 4, 8, 0) };
            dtpStart.Value = new DateTime(DateTime.Today.Year, 1, 1);
            dtpEnd.Value = DateTime.Today;

            var btnApply = primaryBtn("Toepassen", 96);
            btnApply.Click += (s, e) => applyPeriod();

            var btnPrev = miniBtn("◀", 32);
            btnPrev.Click += (s, e) => shiftWeek(-1);
            var btnNext = miniBtn("▶", 32);
            btnNext.Click += (s, e) => shiftWeek(+1);
            var tip = new ToolTip();
            tip.SetToolTip(btnPrev, "Vorige week");
            tip.SetToolTip(btnNext, "Volgende week");

            var cboPreset = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130, Margin = new Padding(8, 4, 0, 0), Font = new Font("Segoe UI", 9F) };
            cboPreset.Items.AddRange(new object[] { "Snel kiezen…", "Deze week", "Deze maand", "Dit jaar" });
            cboPreset.SelectedIndex = 0;
            cboPreset.SelectedIndexChanged += (s, e) =>
            {
                DateTime today = DateTime.Today;
                switch (cboPreset.SelectedIndex)
                {
                    case 1: DateTime m = mondayOf(today); setRange(m, m.AddDays(6)); break;
                    case 2: DateTime f = new DateTime(today.Year, today.Month, 1); setRange(f, f.AddMonths(1).AddDays(-1)); break;
                    case 3: setRange(new DateTime(today.Year, 1, 1), new DateTime(today.Year, 12, 31)); break;
                }
                if (cboPreset.SelectedIndex != 0) cboPreset.SelectedIndex = 0;
            };

            bar.Controls.AddRange(new Control[] { dtpStart, lblTo, dtpEnd, btnApply, spacer(14), btnPrev, btnNext, spacer(10), cboPreset });
            return bar;
        }

        private Panel buildBottom()
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = PageBg, Margin = new Padding(0, 4, 0, 0) };

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));

            var chartCard = makeCard("Omzet per maand", out Panel chartBody, out lblChartTitle);
            chart = new SimpleBarChart { Dock = DockStyle.Fill };
            chartBody.Controls.Add(chart);

            var topCard = makeCard("Top 10 meest verhuurde auto's", out Panel topBody, out _);
            gridTop = buildTopGrid();
            // Double-click a car to open the Auto's page filtered to just that car.
            gridTop.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0 || gridTop.Columns["Kenteken"] == null) return;
                object v = gridTop.Rows[e.RowIndex].Cells["Kenteken"].Value;
                if (v == null || v == DBNull.Value) return;
                AdminNav.Open(PermissionHelper.Cars, Convert.ToString(v));
            };
            var tipTop = new ToolTip();
            tipTop.SetToolTip(gridTop, "Dubbelklik een auto om deze te openen bij Auto's.");
            topBody.Controls.Add(gridTop);

            split.Controls.Add(chartCard, 0, 0);
            split.Controls.Add(topCard, 1, 0);
            periodBottom = split;

            lblNoData = new Label
            {
                Text = "Geen gegevens voor deze periode",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Muted,
                BackColor = PageBg,
                Visible = false
            };

            dayBottom = buildDayList();

            host.Controls.Add(dayBottom);
            host.Controls.Add(split);
            host.Controls.Add(lblNoData);
            return host;
        }

        // Row 4 in day mode: ◀ [day] ▶  Vandaag
        private Control buildDayBar()
        {
            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Visible = false, Margin = new Padding(0, 2, 0, 8) };

            var btnPrev = miniBtn("◀", 32);
            btnPrev.Click += (s, e) => dtpDay.Value = dtpDay.Value.Date.AddDays(-1);
            dtpDay = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 140, Margin = new Padding(4, 4, 4, 0) };
            dtpDay.Value = DateTime.Today;
            dtpDay.ValueChanged += (s, e) => { if (dayMode) applyDay(); };
            var btnNext = miniBtn("▶", 32);
            btnNext.Click += (s, e) => dtpDay.Value = dtpDay.Value.Date.AddDays(1);

            var btnToday = primaryBtn("Vandaag", 90);
            btnToday.Margin = new Padding(10, 4, 2, 0);
            btnToday.Click += (s, e) => dtpDay.Value = DateTime.Today;

            var tip = new ToolTip();
            tip.SetToolTip(btnPrev, "Vorige dag");
            tip.SetToolTip(btnNext, "Volgende dag");

            bar.Controls.AddRange(new Control[] { btnPrev, dtpDay, btnNext, btnToday });
            return bar;
        }

        // Row 5 in day mode: three figures for the chosen day.
        private Control buildDayTiles()
        {
            var t = tileRow(3);
            t.Visible = false;
            t.Controls.Add(makeKpi("Omzet op deze dag", out valDayRevenue, out _), 0, 0);
            t.Controls.Add(makeKpi("Nieuwe verhuringen", out valDayStarts, out _), 1, 0);
            t.Controls.Add(makeKpi("Auto's op de weg", out valDayActive, out _), 2, 0);
            return t;
        }

        // Row 6 in day mode: the reservations active on the chosen day.
        private Control buildDayList()
        {
            var card = makeCard("Reserveringen op deze dag", out Panel body, out lblDayTitle);
            card.Visible = false;

            gridDay = buildTopGrid();
            gridDay.ScrollBars = ScrollBars.Vertical;
            gridDay.RowTemplate.Height = 28;
            gridDay.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rid", DataPropertyName = "Rid", Visible = false });
            gridDay.Columns.Add(new DataGridViewTextBoxColumn { Name = "StCode", DataPropertyName = "StCode", Visible = false });
            gridDay.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nr", HeaderText = "Nr", DataPropertyName = "Nr", FillWeight = 32 });
            gridDay.Columns.Add(new DataGridViewTextBoxColumn { Name = "Klant", HeaderText = "Klant", DataPropertyName = "Klant", FillWeight = 85 });
            gridDay.Columns.Add(new DataGridViewTextBoxColumn { Name = "Auto", HeaderText = "Auto", DataPropertyName = "Auto", FillWeight = 110 });
            gridDay.Columns.Add(new DataGridViewTextBoxColumn { Name = "Periode", HeaderText = "Periode", DataPropertyName = "Periode", FillWeight = 95 });
            gridDay.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 65 });
            gridDay.CellFormatting += gridDay_CellFormatting;
            // Double-click a reservation to open it on the Reserveringen page.
            gridDay.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                object v = gridDay.Rows[e.RowIndex].Cells["Rid"].Value;
                if (v == null || v == DBNull.Value) return;
                AdminNav.Open(PermissionHelper.Rentals, Convert.ToInt32(v));
            };

            lblDayEmpty = new Label
            {
                Text = "Geen reserveringen op deze dag",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Muted,
                Visible = false
            };

            body.Controls.Add(gridDay);
            body.Controls.Add(lblDayEmpty);
            return card;
        }

        private void gridDay_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || gridDay.Columns[e.ColumnIndex].Name != "Status") return;
            string code = Convert.ToString(gridDay.Rows[e.RowIndex].Cells["StCode"].Value);
            switch ((code ?? "").ToLowerInvariant())
            {
                case "completed": e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52); break;
                case "cancelled": e.CellStyle.ForeColor = Color.FromArgb(153, 27, 27); break;
                case "active":
                case "ongoing": e.CellStyle.ForeColor = Color.FromArgb(30, 64, 175); break;
                default: e.CellStyle.ForeColor = Color.FromArgb(146, 64, 14); break;
            }
            e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        // Fills the day tiles and the reservation list for the chosen day.
        private void applyDay()
        {
            DateTime day = dtpDay.Value.Date;
            string ds = day.ToString("yyyy-MM-dd");
            lblDayTitle.Text = "Reserveringen op " + day.ToString("dddd d MMM yyyy", NL);

            // Rentals whose period covers this day (and that were not cancelled).
            string activeOnDay =
                "r.Status <> 'cancelled' AND DATE(r.Date_Started) <= '" + ds + "' " +
                "AND DATE(COALESCE(r.Date_Ended, r.Date_Planned_Ending)) >= '" + ds + "'";

            try
            {
                valDayRevenue.Text = euro(revenueWhere("DATE(r.Date_Started) = '" + ds + "'"));
                valDayStarts.Text = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT COUNT(*) FROM rental r WHERE r.Status <> 'cancelled' AND DATE(r.Date_Started) = '" + ds + "'")).ToString();
                valDayActive.Text = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT COUNT(DISTINCT r.CarID) FROM rental r WHERE " + activeOnDay)).ToString();

                DataTable t = DBHelper.getdataTable(
                    "SELECT r.ID AS Rid, CONCAT(u.Name,' ',u.LastName) AS Klant, c.Brand, c.Model, c.License_Plate, " +
                    "r.Date_Started, r.Date_Planned_Ending, r.Status " +
                    "FROM rental r JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID " +
                    "WHERE " + activeOnDay + " ORDER BY r.Date_Started");

                var d = new DataTable();
                d.Columns.Add("Rid", typeof(int));
                d.Columns.Add("StCode");
                d.Columns.Add("Nr");
                d.Columns.Add("Klant");
                d.Columns.Add("Auto");
                d.Columns.Add("Periode");
                d.Columns.Add("Status");
                foreach (DataRow r in t.Rows)
                {
                    string auto = r["Brand"] + " " + r["Model"] + " (" + r["License_Plate"] + ")";
                    string periode = Convert.ToDateTime(r["Date_Started"]).ToString("dd-MM-yyyy", NL) + " - " +
                                     Convert.ToDateTime(r["Date_Planned_Ending"]).ToString("dd-MM-yyyy", NL);
                    string stCode = r["Status"].ToString();
                    d.Rows.Add(Convert.ToInt32(r["Rid"]), stCode, "#" + r["Rid"], r["Klant"].ToString(),
                        auto, periode, StatusText.Rental(stCode));
                }
                gridDay.DataSource = d;
                lblDayEmpty.Visible = d.Rows.Count == 0;
                if (d.Rows.Count == 0) lblDayEmpty.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de dag niet laden:\n" + ex.Message, "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataGridView buildTopGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 28,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ScrollBars = ScrollBars.None,
                GridColor = Color.FromArgb(240, 241, 243),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 26 }
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 251);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            g.DefaultCellStyle.SelectionForeColor = Ink;
            return g;
        }

        // --- Small building blocks ---

        private Label sectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 0, 0, 0),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Muted,
                TextAlign = ContentAlignment.BottomLeft
            };
        }

        private TableLayoutPanel tileRow(int cols)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = cols, RowCount = 1, Margin = new Padding(0, 2, 0, 2) };
            for (int i = 0; i < cols; i++) t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            return t;
        }

        // A KPI card: muted caption, large value, optional sub-line underneath.
        private Card makeKpi(string caption, out Label value, out Label sub)
        {
            var card = makeCard(caption, out Panel body, out _);
            sub = new Label { Dock = DockStyle.Bottom, Height = 16, Text = "", Font = new Font("Segoe UI", 8.5F), ForeColor = Muted };
            value = new Label
            {
                Text = "–",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                ForeColor = Ink,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            body.Controls.Add(value);
            body.Controls.Add(sub);
            return card;
        }

        // A rounded white card that fills its grid cell, with a caption strip and body.
        private Card makeCard(string title, out Panel body, out Label head)
        {
            var card = new Card { Dock = DockStyle.Fill };
            body = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            head = new Label
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

        private Button primaryBtn(string text, int width)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(width, 28),
                Margin = new Padding(0, 4, 2, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Button miniBtn(string text, int width)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(width, 28),
                Margin = new Padding(0, 4, 4, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Ink,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private Control spacer(int width)
        {
            return new Panel { Size = new Size(width, 1), Margin = new Padding(0) };
        }

        // --- Period picker logic ---

        private static DateTime mondayOf(DateTime d)
        {
            int diff = ((int)d.DayOfWeek + 6) % 7; // Monday = 0
            return d.Date.AddDays(-diff);
        }

        // Step one whole week back/forward, snapping to a Monday–Sunday window.
        private void shiftWeek(int dir)
        {
            DateTime weekStart = mondayOf(dtpStart.Value.Date).AddDays(dir * 7);
            setRange(weekStart, weekStart.AddDays(6));
        }

        private void setRange(DateTime start, DateTime end)
        {
            if (start < dtpStart.MinDate) start = dtpStart.MinDate;
            if (end > dtpEnd.MaxDate) end = dtpEnd.MaxDate;
            dtpStart.Value = start;
            dtpEnd.Value = end;
            applyPeriod();
        }

        private string euro(object value)
        {
            double d = (value == null || value == DBNull.Value)
                ? 0
                : Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return "€ " + d.ToString("N2", NL);
        }

        // Fills the four fixed day/week/month/year revenue cards.
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

        // Fills the period stats, chart and top 10 for the chosen start/end dates.
        private void applyPeriod()
        {
            DateTime start = dtpStart.Value.Date;
            DateTime end = dtpEnd.Value.Date;
            if (start > end)
            {
                MessageBox.Show("De begindatum ligt na de einddatum.", "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblPeriodHeading.Text = "ANALYSE PER PERIODE   ·   " +
                start.ToString("d MMM yyyy", NL) + "  t/m  " + end.ToString("d MMM yyyy", NL);

            string s = start.ToString("yyyy-MM-dd");
            string e = end.ToString("yyyy-MM-dd");
            string between = "DATE(r.Date_Started) BETWEEN '" + s + "' AND '" + e + "'";

            try
            {
                int rentals = Convert.ToInt32(DBHelper.getScalar("SELECT COUNT(*) FROM rental r WHERE " + between));

                if (rentals == 0)
                {
                    valRevenue.Text = euro(0);
                    subRevenue.Text = "";
                    valRentals.Text = "0";
                    valOccupancy.Text = "0%";
                    subOccupancy.Text = "";
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
                double rev = Convert.ToDouble(revenue, CultureInfo.InvariantCulture);
                subRevenue.Text = "≈ " + euro(rev / rentals).Trim() + " per verhuring";
                valRentals.Text = rentals.ToString();

                int totalCars = Convert.ToInt32(DBHelper.getScalar("SELECT COUNT(*) FROM car"));
                int rentedCars = Convert.ToInt32(DBHelper.getScalar(
                    "SELECT COUNT(DISTINCT r.CarID) FROM rental r " +
                    "WHERE DATE(r.Date_Started) <= '" + e + "' " +
                    "AND (r.Date_Ended IS NULL OR DATE(r.Date_Ended) >= '" + s + "')"));
                double occ = totalCars == 0 ? 0 : (rentedCars * 100.0 / totalCars);
                valOccupancy.Text = occ.ToString("0.#", NL) + "%";
                subOccupancy.Text = rentedCars + " van " + totalCars + " auto's";

                DataTable top = DBHelper.getdataTable(
                    "SELECT c.Brand, c.Model, c.License_Plate AS Kenteken, COUNT(*) AS Verhuringen, " +
                    "COALESCE(SUM(i.Total_Price), 0) AS Omzet " +
                    "FROM rental r JOIN car c ON c.ID = r.CarID " +
                    "LEFT JOIN invoice i ON i.Rental_ID = r.ID " +
                    "WHERE " + between + " GROUP BY r.CarID, c.Brand, c.Model, c.License_Plate " +
                    "ORDER BY Verhuringen DESC, Omzet DESC LIMIT 10");
                bindTop(top);

                loadChart(start, end, between);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de periode niet laden:\n" + ex.Message, "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Daily bars for short ranges (so cycling weeks shows real daily detail),
        // monthly bars for longer ranges.
        private void loadChart(DateTime start, DateTime end, string between)
        {
            bool daily = (end - start).Days <= 45;
            string fmt = daily ? "%Y-%m-%d" : "%Y-%m";
            lblChartTitle.Text = daily ? "Omzet per dag" : "Omzet per maand";

            DataTable series = DBHelper.getdataTable(
                "SELECT DATE_FORMAT(r.Date_Started, '" + fmt + "') AS P, " +
                "COALESCE(SUM(i.Total_Price), 0) AS Omzet " +
                "FROM rental r JOIN invoice i ON i.Rental_ID = r.ID " +
                "WHERE " + between + " GROUP BY P ORDER BY P");

            var points = new List<KeyValuePair<string, double>>();
            foreach (DataRow row in series.Rows)
            {
                string key = row["P"].ToString();
                string label = key;
                DateTime parsed;
                if (daily && DateTime.TryParseExact(key, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                    label = parsed.ToString("d MMM", NL);
                else if (!daily && DateTime.TryParseExact(key + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                    label = parsed.ToString("MMM yy", NL);
                points.Add(new KeyValuePair<string, double>(label, Convert.ToDouble(row["Omzet"], CultureInfo.InvariantCulture)));
            }
            chart.SetData(points);
        }

        private void bindTop(DataTable top)
        {
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
                gridTop.Columns["Kenteken"].FillWeight = 85;
                gridTop.Columns["Auto"].FillWeight = 120;
                gridTop.Columns["Verhuringen"].FillWeight = 70;
                gridTop.Columns["Omzet"].FillWeight = 85;
            }
        }

        private void showNoData(bool show)
        {
            lblNoData.Visible = show;
            if (show) lblNoData.BringToFront();
        }

        // A rounded white card drawn with GDI+.
        private class Card : Panel
        {
            public Card()
            {
                DoubleBuffered = true;
                Margin = new Padding(6);
                Padding = new Padding(16, 12, 16, 12);
                BackColor = Color.Transparent;
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                if (Width > 0 && Height > 0)
                {
                    using (var path = Rounded(0, 0, Width, Height, 12))
                        Region = new Region(path);
                }
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var fill = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(229, 231, 235)))
                using (var path = Rounded(0, 0, Width - 1, Height - 1, 12))
                {
                    e.Graphics.FillPath(fill, path);
                    e.Graphics.DrawPath(pen, path);
                }
            }

            private static GraphicsPath Rounded(int x, int y, int w, int h, int r)
            {
                var p = new GraphicsPath();
                if (w <= 0 || h <= 0) { p.AddRectangle(new Rectangle(x, y, Math.Max(1, w), Math.Max(1, h))); return p; }
                int d = Math.Min(r * 2, Math.Min(w, h));
                p.AddArc(x, y, d, d, 180, 90);
                p.AddArc(x + w - d, y, d, d, 270, 90);
                p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
                p.AddArc(x, y + h - d, d, d, 90, 90);
                p.CloseFigure();
                return p;
            }
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

            private static string Money(double v)
            {
                if (v >= 1000) return "€" + (v / 1000.0).ToString("0.#", NL) + "k";
                return "€" + v.ToString("0", NL);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (data.Count == 0) return;

                int left = 52, right = 14, top = 14, bottom = 30;
                int plotW = Width - left - right;
                int plotH = Height - top - bottom;
                if (plotW <= 10 || plotH <= 10) return;

                double max = data.Max(p => p.Value);
                if (max <= 0) max = 1;

                int n = data.Count;
                int labelStep = (int)Math.Ceiling(n / 12.0);

                using (var axisPen = new Pen(Color.FromArgb(236, 238, 241)))
                using (var barBrush = new SolidBrush(Accent))
                using (var textBrush = new SolidBrush(Muted))
                using (var font = new Font("Segoe UI", 8F))
                using (var fmt = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                {
                    for (int i = 0; i <= 2; i++)
                    {
                        int y = top + plotH - (int)(plotH * i / 2.0);
                        g.DrawLine(axisPen, left, y, left + plotW, y);
                        g.DrawString(Money(max * i / 2.0), font, textBrush, 2, y - 7);
                    }

                    float slot = plotW / (float)n;
                    float barW = Math.Min(46f, slot * 0.62f);
                    for (int i = 0; i < n; i++)
                    {
                        double val = data[i].Value;
                        int h = (int)(plotH * (val / max));
                        float x = left + slot * i + (slot - barW) / 2f;
                        int y = top + plotH - h;
                        using (var path = RoundedTop(x, y, barW, Math.Max(h, 2), 4))
                            g.FillPath(barBrush, path);
                        if (i % labelStep == 0)
                            g.DrawString(data[i].Key, font, textBrush,
                                new RectangleF(left + slot * i, top + plotH + 6, slot, 16), fmt);
                    }
                }
            }

            private static GraphicsPath RoundedTop(float x, float y, float w, float h, float r)
            {
                var p = new GraphicsPath();
                if (h < r * 2) { p.AddRectangle(new RectangleF(x, y, w, h)); return p; }
                p.AddArc(x, y, r * 2, r * 2, 180, 90);
                p.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
                p.AddLine(x + w, y + r, x + w, y + h);
                p.AddLine(x + w, y + h, x, y + h);
                p.CloseFigure();
                return p;
            }
        }
    }
}
