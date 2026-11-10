using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story 2 - Overzicht auto's en status aanpassen.
    // Shows all cars (plate, brand, model, status), lets staff change the status inline,
    // and supports filtering by status and searching on plate/brand/model.
    public class CarsPage : UserControl
    {
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly CultureInfo NL = new CultureInfo("nl-NL");

        // The four fixed statuses: Dutch label shown in the UI <-> code stored in the database.
        private static readonly string[] StatusLabels = { "Beschikbaar", "Verhuurd", "In onderhoud", "Buiten gebruik" };
        private static readonly Dictionary<string, string> LabelToCode = new Dictionary<string, string>
        {
            { "Beschikbaar", "active" },
            { "Verhuurd", "rented" },
            { "In onderhoud", "maintenance" },
            { "Buiten gebruik", "disabled" },
        };
        private static readonly Dictionary<string, string> CodeToLabel = new Dictionary<string, string>
        {
            { "active", "Beschikbaar" },
            { "rented", "Verhuurd" },
            { "maintenance", "In onderhoud" },
            { "disabled", "Buiten gebruik" },
        };

        private TextBox txtSearch;
        private ComboBox cboFilter;
        private DataGridView grid;
        private Label lblCount;
        private bool loading;

        public CarsPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadCars();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var header = new Label
            {
                Text = "Auto's",
                Dock = DockStyle.Top,
                Height = 44,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(0, 8, 0, 10) };
            txtSearch = new TextBox { Location = new Point(2, 12), Width = 240 };
            setPlaceholder(txtSearch, "Zoek op kenteken, merk of model");
            txtSearch.TextChanged += (s, e) => { if (!loading) loadCars(); };

            cboFilter = new ComboBox
            {
                Location = new Point(255, 11),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboFilter.Items.Add("Alle statussen");
            cboFilter.Items.AddRange(StatusLabels);
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += (s, e) => { if (!loading) loadCars(); };

            lblCount = new Label
            {
                Location = new Point(450, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 9.5F)
            };
            toolbar.Controls.AddRange(new Control[] { txtSearch, cboFilter, lblCount });

            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 32 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 38;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "ID", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kenteken", HeaderText = "Kenteken", DataPropertyName = "Kenteken", ReadOnly = true, FillWeight = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Merk", HeaderText = "Merk", DataPropertyName = "Merk", ReadOnly = true, FillWeight = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Model", HeaderText = "Model", DataPropertyName = "Model", ReadOnly = true, FillWeight = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Prijs",
                HeaderText = "Prijs/dag",
                DataPropertyName = "Prijs",
                ReadOnly = true,
                FillWeight = 70,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            var statusCol = new DataGridViewComboBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataPropertyName = "Status",
                FillWeight = 110,
                FlatStyle = FlatStyle.Flat
            };
            statusCol.Items.AddRange(StatusLabels);
            grid.Columns.Add(statusCol);
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Active", DataPropertyName = "Active", Visible = false });

            grid.CellValueChanged += grid_CellValueChanged;
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                // Commit a combobox pick immediately so CellValueChanged fires.
                if (grid.IsCurrentCellDirty)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            grid.DataError += (s, e) => { e.ThrowException = false; };
            grid.CellFormatting += grid_CellFormatting;

            card.Controls.Add(grid);

            root.Controls.Add(card);
            root.Controls.Add(toolbar);
            root.Controls.Add(header);
            header.BringToFront();
        }

        private void loadCars()
        {
            loading = true;
            try
            {
                var where = new List<string>();

                string search = realText(txtSearch).Trim();
                if (search != "")
                {
                    string like = "'%" + search.Replace("\\", "\\\\").Replace("'", "''").Replace("%", "\\%").Replace("_", "\\_") + "%'";
                    where.Add("(c.License_Plate LIKE " + like + " OR c.Brand LIKE " + like + " OR c.Model LIKE " + like + ")");
                }

                if (cboFilter.SelectedIndex > 0)
                {
                    string label = cboFilter.SelectedItem.ToString();
                    where.Add("c.Car_Status = '" + LabelToCode[label] + "'");
                }

                string sql =
                    "SELECT c.ID, c.License_Plate, c.Brand, c.Model, c.Daily_Price, c.Car_Status, " +
                    "(SELECT COUNT(*) FROM rental r WHERE r.CarID = c.ID AND r.Date_Ended IS NULL AND r.Status <> 'cancelled') AS ActiveRentals " +
                    "FROM car c";
                if (where.Count > 0)
                {
                    sql += " WHERE " + string.Join(" AND ", where);
                }
                sql += " ORDER BY c.License_Plate";

                DataTable cars = DBHelper.getdataTable(sql);

                var display = new DataTable();
                display.Columns.Add("ID", typeof(int));
                display.Columns.Add("Kenteken");
                display.Columns.Add("Merk");
                display.Columns.Add("Model");
                display.Columns.Add("Prijs");
                display.Columns.Add("Status");
                display.Columns.Add("Active", typeof(int));

                foreach (DataRow r in cars.Rows)
                {
                    string code = r["Car_Status"].ToString();
                    string label = CodeToLabel.ContainsKey(code) ? CodeToLabel[code] : "Beschikbaar";
                    double price = Convert.ToDouble(r["Daily_Price"], CultureInfo.InvariantCulture);
                    display.Rows.Add(
                        Convert.ToInt32(r["ID"]),
                        r["License_Plate"].ToString(),
                        r["Brand"].ToString(),
                        r["Model"].ToString(),
                        "€ " + price.ToString("N2", NL),
                        label,
                        Convert.ToInt32(r["ActiveRentals"]));
                }

                grid.DataSource = display;
                lblCount.Text = display.Rows.Count + (display.Rows.Count == 1 ? " auto" : " auto's");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de auto's niet laden:\n" + ex.Message, "Auto's", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loading = false;
            }
        }

        private void grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (loading || e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Status")
            {
                return;
            }

            DataGridViewRow row = grid.Rows[e.RowIndex];
            int carId = Convert.ToInt32(row.Cells["ID"].Value);
            int active = Convert.ToInt32(row.Cells["Active"].Value);
            string newLabel = row.Cells["Status"].Value == null ? "" : row.Cells["Status"].Value.ToString();
            if (!LabelToCode.ContainsKey(newLabel))
            {
                return;
            }
            string newCode = LabelToCode[newLabel];

            // A car with an active rental cannot be set to "Beschikbaar".
            if (newCode == "active" && active > 0)
            {
                MessageBox.Show(
                    "Deze auto heeft een actieve verhuring en kan niet op 'Beschikbaar' gezet worden.",
                    "Status aanpassen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                loadCars();
                return;
            }

            try
            {
                DBHelper.RunSql("UPDATE car SET Car_Status = '" + newCode + "' WHERE ID = " + carId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de status niet opslaan:\n" + ex.Message, "Status aanpassen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                loadCars();
            }
        }

        // Colours the status cell so each status is easy to spot.
        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Status" || e.Value == null)
            {
                return;
            }
            switch (e.Value.ToString())
            {
                case "Beschikbaar": e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52); break;
                case "Verhuurd": e.CellStyle.ForeColor = Color.FromArgb(30, 64, 175); break;
                case "In onderhoud": e.CellStyle.ForeColor = Color.FromArgb(146, 64, 14); break;
                case "Buiten gebruik": e.CellStyle.ForeColor = Color.FromArgb(153, 27, 27); break;
            }
        }

        // --- Simple placeholder support for the search box ---
        private void setPlaceholder(TextBox box, string placeholder)
        {
            box.Tag = placeholder;
            box.ForeColor = Color.Gray;
            box.Text = placeholder;
            box.GotFocus += (s, e) =>
            {
                if (box.Text == placeholder)
                {
                    box.Text = "";
                    box.ForeColor = Color.Black;
                }
            };
            box.LostFocus += (s, e) =>
            {
                if (box.Text.Trim() == "")
                {
                    box.Text = placeholder;
                    box.ForeColor = Color.Gray;
                }
            };
        }

        private string realText(TextBox box)
        {
            return (box.Tag != null && box.Text == box.Tag.ToString()) ? "" : box.Text;
        }
    }
}
