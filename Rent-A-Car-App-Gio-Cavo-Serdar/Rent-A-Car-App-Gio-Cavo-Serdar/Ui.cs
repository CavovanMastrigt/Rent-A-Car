using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Shared UI helpers used by the staff screens (gebruikers/rollen en klanten).
    // Rebuilt from how UsersRolesControl and CustomersControl use it.
    internal static class Ui
    {
        public static readonly Color Ink = Color.FromArgb(17, 24, 39);
        public static readonly Color Muted = Color.FromArgb(107, 114, 128);
        public static readonly Color Error = Color.FromArgb(220, 38, 38);
        public static readonly Color Success = Color.FromArgb(22, 163, 74);
        public static readonly Color Accent = Color.FromArgb(37, 99, 235);
        public static readonly Color LineColor = Color.FromArgb(209, 213, 219);
        public static readonly CultureInfo Dutch = new CultureInfo("nl-NL");

        // Applies the standard page background and padding to a staff screen.
        public static void Style(Control control)
        {
            control.BackColor = Color.FromArgb(243, 244, 246);
            control.Padding = new Padding(16);
        }

        // A read-only grid that fills its parent, with one-row selection.
        public static DataGridView Grid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 28 }
            };
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            return grid;
        }

        // A filled primary (blue) button.
        public static Button Primary(string text, int x, int y, int width)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.75F),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        // A secondary (outlined) button.
        public static Button Btn(string text, int x, int y, int width)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Ink,
                Font = new Font("Segoe UI", 9.75F),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = LineColor;
            return button;
        }

        // A small muted note/status label.
        public static Label Note(int x, int y, int width, int height)
        {
            return new Label
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9F)
            };
        }

        // A bold section heading.
        public static Label Heading(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Ink
            };
        }

        // A small caption above a field.
        public static Label Caption(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Muted
            };
        }

        // Wraps a content control under a titled header, filling the parent.
        public static Control Section(string title, Control content)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            content.Dock = DockStyle.Fill;
            var head = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Ink
            };
            panel.Controls.Add(content);
            panel.Controls.Add(head);
            return panel;
        }

        // Binds a table to a grid; optionally hides one column (e.g. a raw id).
        public static void Bind(DataGridView grid, DataTable table, string hideColumn = null)
        {
            grid.DataSource = table;
            if (hideColumn != null)
            {
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    if (column.Name == hideColumn || column.DataPropertyName == hideColumn)
                    {
                        column.Visible = false;
                    }
                }
            }
        }

        // The data row behind the grid's current row, or null.
        public static DataRowView Row(DataGridView grid)
        {
            if (grid.CurrentRow == null)
            {
                return null;
            }
            return grid.CurrentRow.DataBoundItem as DataRowView;
        }

        // Reselects the row whose "Nr" column equals id; returns whether it was found.
        public static bool Reselect(DataGridView grid, int id)
        {
            if (id == 0)
            {
                return false;
            }
            foreach (DataGridViewRow row in grid.Rows)
            {
                DataRowView view = row.DataBoundItem as DataRowView;
                if (view == null || !view.Row.Table.Columns.Contains("Nr"))
                {
                    continue;
                }
                if (Convert.ToInt32(view["Nr"]) == id)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.Visible)
                        {
                            grid.CurrentCell = cell;
                            break;
                        }
                    }
                    row.Selected = true;
                    return true;
                }
            }
            return false;
        }

        // Applies a display format to one grid column.
        public static void Format(DataGridView grid, string column, string format)
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Name == column || col.DataPropertyName == column)
                {
                    col.DefaultCellStyle.Format = format;
                    col.DefaultCellStyle.FormatProvider = Dutch;
                }
            }
        }

        // Sets a label's text and colour in one call.
        public static void Say(Label label, string text, Color color)
        {
            label.Text = text;
            label.ForeColor = color;
        }

        public static string Euro(decimal value)
        {
            return value.ToString("C2", Dutch);
        }

        public static void Warn(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Fail(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Ask(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }

    // Small SQL string helpers. User input is escaped before it is put in a query.
    internal static class Sql
    {
        // A quoted, escaped string literal, e.g. Smith -> 'Smith'.
        public static string Text(string value)
        {
            if (value == null)
            {
                value = "";
            }
            return "'" + value.Trim().Replace("\\", "\\\\").Replace("'", "''") + "'";
        }

        // A quoted LIKE pattern, e.g. smi -> '%smi%', with wildcards escaped.
        public static string Like(string value)
        {
            if (value == null)
            {
                value = "";
            }
            string escaped = value.Trim()
                .Replace("\\", "\\\\")
                .Replace("'", "''")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            return "'%" + escaped + "%'";
        }

        // Runs a COUNT(*)-style query and returns the number.
        public static int Count(string sql)
        {
            object value = DBHelper.getScalar(sql);
            return (value == null || value == DBNull.Value) ? 0 : Convert.ToInt32(value);
        }
    }
}
