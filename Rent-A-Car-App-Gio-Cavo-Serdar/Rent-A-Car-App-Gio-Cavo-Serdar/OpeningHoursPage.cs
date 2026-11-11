using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Admin-only CRUD on the opening hours (table openingtimes).
    // One row per weekday: open time, close time and a "gesloten" flag.
    // Saving rewrites all seven rows and refreshes the OpeningHours cache that
    // the reservation screens use.
    public class OpeningHoursPage : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        // Stored weekday value (Dutch, recognised by OpeningHours) and its UI label.
        private static readonly string[] DayKeys = { "maandag", "dinsdag", "woensdag", "donderdag", "vrijdag", "zaterdag", "zondag" };
        private static readonly string[] DayLabels = { "Maandag", "Dinsdag", "Woensdag", "Donderdag", "Vrijdag", "Zaterdag", "Zondag" };

        private readonly DateTimePicker[] opens = new DateTimePicker[7];
        private readonly DateTimePicker[] closes = new DateTimePicker[7];
        private readonly CheckBox[] closedBoxes = new CheckBox[7];

        public OpeningHoursPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadTimes();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(layout);

            var header = new Label { Text = "Openingstijden", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink };
            var sub = new Label { Text = "Stel per dag de openingstijden in. Vink 'Gesloten' aan als we die dag dicht zijn.", Dock = DockStyle.Fill, ForeColor = Muted };

            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20) };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            var tbl = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 5 };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));

            tbl.Controls.Add(colHead("Dag"), 0, 0);
            tbl.Controls.Add(colHead("Open"), 1, 0);
            tbl.Controls.Add(new Label(), 2, 0);
            tbl.Controls.Add(colHead("Sluit"), 3, 0);
            tbl.Controls.Add(colHead("Gesloten"), 4, 0);

            for (int i = 0; i < 7; i++)
            {
                int row = i + 1;
                int day = i;

                tbl.Controls.Add(new Label
                {
                    Text = DayLabels[i],
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 12, 0, 6),
                    Font = new Font("Segoe UI", 10F)
                }, 0, row);

                opens[i] = timePicker();
                closes[i] = timePicker();
                tbl.Controls.Add(opens[i], 1, row);
                tbl.Controls.Add(new Label { Text = "tot", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(8, 13, 0, 0), ForeColor = Muted }, 2, row);
                tbl.Controls.Add(closes[i], 3, row);

                closedBoxes[i] = new CheckBox { Text = "Gesloten", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(12, 12, 0, 0) };
                closedBoxes[i].CheckedChanged += (s, e) => applyClosed(day);
                tbl.Controls.Add(closedBoxes[i], 4, row);
            }

            var buttons = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(0, 14, 0, 0) };
            var btnSave = new Button
            {
                Text = "Opslaan",
                Size = new Size(160, 36),
                Location = new Point(0, 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => save();

            var btnClear = new Button
            {
                Text = "Alles verwijderen",
                Size = new Size(170, 36),
                Location = new Point(172, 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(185, 28, 28),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(248, 180, 180);
            btnClear.FlatAppearance.BorderSize = 1;
            btnClear.Click += (s, e) => clearAll();

            buttons.Controls.AddRange(new Control[] { btnSave, btnClear });

            card.Controls.Add(buttons);
            card.Controls.Add(tbl);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(sub, 0, 1);
            layout.Controls.Add(card, 0, 2);
        }

        private static Label colHead(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 0, 0, 8),
                ForeColor = Color.FromArgb(75, 85, 99),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
        }

        private static DateTimePicker timePicker()
        {
            return new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Width = 92,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 4)
            };
        }

        private void applyClosed(int i)
        {
            bool open = !closedBoxes[i].Checked;
            opens[i].Enabled = open;
            closes[i].Enabled = open;
        }

        private void loadTimes()
        {
            // Defaults for any day that is not configured yet.
            for (int i = 0; i < 7; i++)
            {
                opens[i].Value = DateTime.Today.AddHours(9);
                closes[i].Value = DateTime.Today.AddHours(17);
                closedBoxes[i].Checked = false;
                applyClosed(i);
            }

            try
            {
                DataTable t = DBHelper.getdataTable("SELECT Weekday, Time_Open, Time_Closed, Is_Closed FROM openingtimes");
                foreach (DataRow r in t.Rows)
                {
                    int i = dayIndex(Convert.ToString(r["Weekday"]));
                    if (i < 0) continue;
                    opens[i].Value = DateTime.Today.Add(toTimeSpan(r["Time_Open"]));
                    closes[i].Value = DateTime.Today.Add(toTimeSpan(r["Time_Closed"]));
                    closedBoxes[i].Checked = Convert.ToBoolean(r["Is_Closed"]);
                    applyClosed(i);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de openingstijden niet laden:\n" + ex.Message, "Openingstijden", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void save()
        {
            // Validate open < close for any day that is not closed.
            for (int i = 0; i < 7; i++)
            {
                if (!closedBoxes[i].Checked && opens[i].Value.TimeOfDay >= closes[i].Value.TimeOfDay)
                {
                    MessageBox.Show("Op " + DayLabels[i].ToLower() + " moet de sluitingstijd na de openingstijd liggen.",
                        "Openingstijden", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                // Rewrite all seven rows (create + update in one go).
                DBHelper.RunSql("DELETE FROM openingtimes");
                for (int i = 0; i < 7; i++)
                {
                    var p = new Dictionary<string, object>
                    {
                        { "@o", opens[i].Value.TimeOfDay },
                        { "@c", closes[i].Value.TimeOfDay },
                        { "@w", DayKeys[i] },
                        { "@closed", closedBoxes[i].Checked ? 1 : 0 },
                    };
                    DBHelper.RunSql(
                        "INSERT INTO openingtimes (Time_Open, Time_Closed, Weekday, Is_Closed) " +
                        "VALUES (@o, @c, @w, @closed)", p);
                }
                OpeningHours.Load(); // refresh the cache used by the reservation screens
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de openingstijden niet opslaan:\n" + ex.Message, "Openingstijden", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("De openingstijden zijn opgeslagen.", "Openingstijden", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void clearAll()
        {
            DialogResult ok = MessageBox.Show(
                "Weet je het zeker? Alle openingstijden worden verwijderd.\n" +
                "Er wordt dan niet meer op openingstijden gecontroleerd.",
                "Openingstijden", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) return;

            try
            {
                DBHelper.RunSql("DELETE FROM openingtimes");
                OpeningHours.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de openingstijden niet verwijderen:\n" + ex.Message, "Openingstijden", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            loadTimes();
            MessageBox.Show("De openingstijden zijn verwijderd.", "Openingstijden", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Maps a stored weekday (various spellings) to 0=Monday .. 6=Sunday, or -1.
        private static int dayIndex(string stored)
        {
            string w = (stored ?? "").Trim().ToLowerInvariant();
            string[][] variants =
            {
                new[] { "maandag", "monday", "mon", "ma", "1" },
                new[] { "dinsdag", "tuesday", "tue", "di", "2" },
                new[] { "woensdag", "wednesday", "wed", "wo", "3" },
                new[] { "donderdag", "thursday", "thu", "do", "4" },
                new[] { "vrijdag", "friday", "fri", "vr", "5" },
                new[] { "zaterdag", "saturday", "sat", "za", "6" },
                new[] { "zondag", "sunday", "sun", "zo", "7", "0" },
            };
            for (int i = 0; i < 7; i++)
            {
                if (Array.IndexOf(variants[i], w) >= 0) return i;
            }
            return -1;
        }

        private static TimeSpan toTimeSpan(object value)
        {
            if (value is TimeSpan) return (TimeSpan)value;
            if (value is DateTime) return ((DateTime)value).TimeOfDay;
            return TimeSpan.Parse(Convert.ToString(value), CultureInfo.InvariantCulture);
        }
    }
}
