
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    public class DayOverviewControl : UserControl
    {
        private readonly DateTimePicker dtpDay;
        private readonly Label lblSummary;
        private readonly DataGridView grid;
        private bool ready = false;

        public DayOverviewControl()
        {
            BackColor = UITheme.Surface;
            Font = UITheme.Body;
            Padding = new Padding(10);

            // Reserveringentabel
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
                BackgroundColor = UITheme.Surface,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 42 },
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = UITheme.Border
            };

            // Kolomkoppen
            grid.ColumnHeadersDefaultCellStyle.BackColor =
                UITheme.TableHeader;

            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                UITheme.Text;

            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                UITheme.TableHeader;

            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                UITheme.Text;

            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9.75F, FontStyle.Bold);

            grid.ColumnHeadersHeight = 45;
            grid.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Cellen
            grid.DefaultCellStyle.Font = UITheme.Body;
            grid.DefaultCellStyle.ForeColor = UITheme.Text;
            grid.DefaultCellStyle.BackColor = UITheme.Surface;
            grid.DefaultCellStyle.SelectionBackColor =
                UITheme.Selection;
            grid.DefaultCellStyle.SelectionForeColor =
                UITheme.Text;

            grid.DefaultCellStyle.Padding =
                new Padding(8, 4, 8, 4);

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                UITheme.RowAlternate;

            // Bovenpaneel
            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = UITheme.Surface,
                Padding = new Padding(5, 12, 5, 8),
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };

            Button btnPrev = makeButton("‹", 38);
            Button btnNext = makeButton("›", 38);

            dtpDay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Long,
                Width = 240,
                Height = 34,
                Value = DateTime.Today,
                Font = UITheme.Body,
                Margin = new Padding(5, 2, 5, 2)
            };

            Button btnToday = makeButton("Vandaag", 90);
            btnToday.BackColor = UITheme.Primary;
            btnToday.ForeColor = Color.White;

            btnToday.FlatAppearance.MouseOverBackColor =
                UITheme.PrimaryHover;

            lblSummary = new Label
            {
                AutoSize = true,
                ForeColor = UITheme.Muted,
                Font = UITheme.Body,
                Margin = new Padding(15, 10, 5, 0)
            };

            top.Controls.Add(btnPrev);
            top.Controls.Add(dtpDay);
            top.Controls.Add(btnNext);
            top.Controls.Add(btnToday);
            top.Controls.Add(lblSummary);

            Controls.Add(grid);
            Controls.Add(top);

            // Gebeurtenissen
            btnPrev.Click += (s, e) =>
                dtpDay.Value = dtpDay.Value.AddDays(-1);

            btnNext.Click += (s, e) =>
                dtpDay.Value = dtpDay.Value.AddDays(1);

            btnToday.Click += (s, e) =>
                dtpDay.Value = DateTime.Today;

            dtpDay.ValueChanged += (s, e) =>
            {
                if (ready)
                    loadData();
            };

            VisibleChanged += (s, e) =>
            {
                if (ready && Visible)
                    loadData();
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            ready = true;
            loadData();
        }

        private static Button makeButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(width, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = UITheme.TableHeader,
                ForeColor = UITheme.Text,
                Font = UITheme.Button,
                Margin = new Padding(5, 2, 5, 2)
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor =
                UITheme.Selection;

            return button;
        }

        private void loadData()
        {
            string day = "'" +
                dtpDay.Value.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture) + "'";

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

                foreach (DataRow row in table.Rows)
                {
                    row["Status"] =
                        StatusText.Rental(
                            Convert.ToString(row["Status"]));
                }

                grid.DataSource = table;

                if (grid.Columns.Contains("Nr"))
                    grid.Columns["Nr"].FillWeight = 30;

                if (grid.Columns.Contains("Tijd"))
                    grid.Columns["Tijd"].FillWeight = 45;

                if (grid.Columns.Contains("Soort"))
                    grid.Columns["Soort"].FillWeight = 65;

                int pickUps = 0;
                int returns = 0;

                foreach (DataRow row in table.Rows)
                {
                    if (row["Soort"].ToString() == "Ophalen")
                        pickUps++;
                    else
                        returns++;
                }

                lblSummary.Text =
                    pickUps + " ophaling(en)  |  " +
                    returns + " terugbrenging(en)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "De reserveringen konden niet worden geladen: " +
                    ex.Message,
                    "Dagoverzicht",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
