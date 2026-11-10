using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Medewerkerscherm (user story 5): registreren dat een auto is opgehaald of teruggebracht.
    // Links staan de reserveringen die klaar zijn om op te halen, rechts de auto's die nu verhuurd zijn.
    //   Ophalen:       reserved -> ongoing, Date_Picked_Up wordt gezet (de auto telt dan als verhuurd).
    //   Terugbrengen:  ongoing -> completed, Date_Ended wordt gezet. Bij schade gaat de auto op buiten gebruik.
    // Beide kunnen alleen binnen de openingstijden.
    public class PickupReturnControl : UserControl
    {
        private static readonly Color AccentColor = Color.FromArgb(79, 70, 229);
        private static readonly Color HeaderColor = Color.FromArgb(243, 244, 246);
        private static readonly Color TextColor = Color.FromArgb(31, 41, 55);
        private static readonly Color MutedColor = Color.FromArgb(107, 114, 128);
        private static readonly Color ErrorColor = Color.FromArgb(185, 28, 28);
        private static readonly Color SuccessColor = Color.FromArgb(21, 128, 61);
        private static readonly Color LateColor = Color.FromArgb(254, 226, 226);

        private static readonly CultureInfo Dutch = new CultureInfo("nl-NL");

        private DataGridView grdPickups;
        private DataGridView grdReturns;
        private Button btnPickup;
        private Button btnReturn;
        private Label lblPickupInfo;
        private Label lblReturnInfo;
        private bool ready = false;

        public PickupReturnControl()
        {
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.75F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            layout.Controls.Add(createSection("Op te halen", "Registreer ophalen",
                out grdPickups, out btnPickup, out lblPickupInfo), 0, 0);
            layout.Controls.Add(createSection("Verhuurd, terug te brengen", "Registreer terugbrengen",
                out grdReturns, out btnReturn, out lblReturnInfo), 1, 0);

            // Bovenbalk met de knop om te vernieuwen.
            var top = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            Button btnRefresh = makeButton("Vernieuwen", 16, 12, 110);
            var lblHint = new Label
            {
                AutoSize = true,
                Location = new Point(140, 19),
                ForeColor = MutedColor,
                Text = "Alleen reserveringen van vandaag of eerder staan links. Rode rijen zijn te laat."
            };
            top.Controls.Add(btnRefresh);
            top.Controls.Add(lblHint);

            Controls.Add(layout);
            Controls.Add(top);

            btnRefresh.Click += (s, e) => loadData();
            btnPickup.Click += (s, e) => registerPickup();
            btnReturn.Click += (s, e) => registerReturn();

            // Reserveringen kunnen op andere tabbladen veranderen, dus laad opnieuw zodra dit tabblad weer getoond wordt.
            VisibleChanged += (s, e) =>
            {
                if (ready && Visible)
                {
                    OpeningHours.Load();
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
            OpeningHours.Load();
            loadData();
        }

        // ---------- Opbouw van het scherm ----------

        private static Button makeButton(string text, int x, int y, int width)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 32),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = HeaderColor,
                ForeColor = TextColor
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        // Eén helft van het scherm: titel, tabel, en onderaan de knop met een infotekst.
        private Panel createSection(string title, string buttonText,
            out DataGridView grid, out Button button, out Label info)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 16) };

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

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 56 };
            button = makeButton(buttonText, 0, 12, 200);
            button.BackColor = AccentColor;
            button.ForeColor = Color.White;
            info = new Label
            {
                Location = new Point(212, 8),
                Size = new Size(300, 40),
                ForeColor = MutedColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            bottom.Controls.Add(button);
            bottom.Controls.Add(info);

            var heading = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(grid);
            panel.Controls.Add(bottom);
            panel.Controls.Add(heading);
            return panel;
        }

        // ---------- Gegevens laden ----------

        private void loadData()
        {
            try
            {
                string customerAndCar =
                    "CONCAT(u.Name, ' ', u.LastName) AS `Klant`, " +
                    "CONCAT(c.Brand, ' ', c.Model, ' (', c.License_Plate, ')') AS `Auto` ";
                string joins = "FROM rental r JOIN user u ON u.ID = r.UserID JOIN car c ON c.ID = r.CarID ";

                DataTable pickups = DBHelper.getdataTable(
                    "SELECT r.ID AS `Nr`, r.Date_Started AS `Ophalen`, r.Date_Planned_Ending AS `Terugbrengen`, " +
                    customerAndCar + ", c.Car_Status AS `Autostatus`, c.Car_Status AS `CarStatusRaw`, r.CarID AS `CarID` " +
                    joins +
                    "WHERE r.Status = 'reserved' AND DATE(r.Date_Started) <= CURDATE() " +
                    "ORDER BY r.Date_Started");

                // De database bewaart de autostatus in het Engels; op het scherm tonen we hem in het Nederlands.
                foreach (DataRow row in pickups.Rows)
                {
                    row["Autostatus"] = StatusText.Car(Convert.ToString(row["CarStatusRaw"]));
                }

                grdPickups.DataSource = pickups;
                hideColumns(grdPickups, "CarStatusRaw", "CarID");
                formatDates(grdPickups, "Ophalen", "Terugbrengen");
                setWeight(grdPickups, "Nr", 30);

                DataTable returns = DBHelper.getdataTable(
                    "SELECT r.ID AS `Nr`, COALESCE(r.Date_Picked_Up, r.Date_Started) AS `Opgehaald`, " +
                    "r.Date_Planned_Ending AS `Verwacht terug`, " +
                    customerAndCar + ", r.CarID AS `CarID` " +
                    joins +
                    "WHERE r.Status = 'ongoing' " +
                    "ORDER BY r.Date_Planned_Ending");

                grdReturns.DataSource = returns;
                hideColumns(grdReturns, "CarID");
                formatDates(grdReturns, "Opgehaald", "Verwacht terug");
                setWeight(grdReturns, "Nr", 30);

                int late = 0;
                foreach (DataGridViewRow gridRow in grdReturns.Rows)
                {
                    DataRowView view = gridRow.DataBoundItem as DataRowView;
                    if (view != null && Convert.ToDateTime(view["Verwacht terug"]) < DateTime.Now)
                    {
                        gridRow.DefaultCellStyle.BackColor = LateColor;
                        late++;
                    }
                }

                setInfo(lblPickupInfo, pickups.Rows.Count + " reservering(en) klaar om op te halen.", MutedColor);
                setInfo(lblReturnInfo, returns.Rows.Count + " auto(\u2019s) verhuurd" +
                                       (late > 0 ? ", waarvan " + late + " te laat." : "."), MutedColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show("De reserveringen konden niet worden geladen: " + ex.Message, "Ophalen / terugbrengen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void hideColumns(DataGridView grid, params string[] names)
        {
            foreach (string name in names)
            {
                if (grid.Columns.Contains(name))
                {
                    grid.Columns[name].Visible = false;
                }
            }
        }

        private static void formatDates(DataGridView grid, params string[] names)
        {
            foreach (string name in names)
            {
                if (grid.Columns.Contains(name))
                {
                    grid.Columns[name].DefaultCellStyle.Format = "ddd d MMM HH:mm";
                    grid.Columns[name].DefaultCellStyle.FormatProvider = Dutch;
                }
            }
        }

        private static void setWeight(DataGridView grid, string name, int weight)
        {
            if (grid.Columns.Contains(name))
            {
                grid.Columns[name].FillWeight = weight;
            }
        }

        private static void setInfo(Label label, string text, Color color)
        {
            label.Text = text;
            label.ForeColor = color;
        }

        private static DataRowView selectedRow(DataGridView grid)
        {
            return grid.CurrentRow == null ? null : grid.CurrentRow.DataBoundItem as DataRowView;
        }

        private static string sqlDateTime(DateTime value)
        {
            return "'" + value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
        }

        private static string describe(TimeSpan span)
        {
            if (span.TotalDays >= 1)
            {
                return (int)span.TotalDays + " dag(en) en " + span.Hours + " uur";
            }
            return span.Hours + " uur en " + span.Minutes + " min";
        }

        // ---------- Ophalen registreren ----------

        private void registerPickup()
        {
            DataRowView row = selectedRow(grdPickups);
            if (row == null)
            {
                setInfo(lblPickupInfo, "Kies eerst een reservering in de lijst.", ErrorColor);
                return;
            }

            int rentalId = Convert.ToInt32(row["Nr"]);
            int carId = Convert.ToInt32(row["CarID"]);
            string carStatus = Convert.ToString(row["CarStatusRaw"]).Trim().ToLowerInvariant();

            if (carStatus != "active")
            {
                MessageBox.Show("Deze auto kan niet worden meegegeven. Status: " +
                                StatusText.Car(carStatus).ToLowerInvariant() + ".",
                    "Ophalen registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hours = OpeningHours.Check(DateTime.Now, "Ophalen");
            if (hours != null)
            {
                MessageBox.Show(hours, "Ophalen registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                object stillOut = DBHelper.getScalar(
                    "SELECT COUNT(*) FROM rental WHERE CarID = " + carId + " AND Status = 'ongoing'");
                if (Convert.ToInt32(stillOut) > 0)
                {
                    MessageBox.Show("Deze auto is nog niet teruggebracht door de vorige klant.",
                        "Ophalen registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult answer = MessageBox.Show(
                    "Ophalen registreren voor reservering #" + rentalId + "?\n\n" +
                    Convert.ToString(row["Klant"]) + "\n" + Convert.ToString(row["Auto"]),
                    "Ophalen registreren", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes)
                {
                    return;
                }

                int changed = DBHelper.RunSql(
                    "UPDATE rental SET Status = 'ongoing', Date_Picked_Up = " + sqlDateTime(DateTime.Now) +
                    " WHERE ID = " + rentalId + " AND Status = 'reserved'");

                if (changed == 0)
                {
                    MessageBox.Show("Deze reservering is intussen gewijzigd. De lijst wordt vernieuwd.",
                        "Ophalen registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    loadData();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Het ophalen kon niet worden geregistreerd: " + ex.Message, "Ophalen registreren",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            loadData();
            setInfo(lblPickupInfo, "Ophalen geregistreerd voor reservering #" + rentalId + ".", SuccessColor);
        }

        // ---------- Terugbrengen registreren ----------

        private void registerReturn()
        {
            DataRowView row = selectedRow(grdReturns);
            if (row == null)
            {
                setInfo(lblReturnInfo, "Kies eerst een reservering in de lijst.", ErrorColor);
                return;
            }

            int rentalId = Convert.ToInt32(row["Nr"]);
            int carId = Convert.ToInt32(row["CarID"]);
            DateTime plannedEnd = Convert.ToDateTime(row["Verwacht terug"]);
            DateTime now = DateTime.Now;

            string hours = OpeningHours.Check(now, "Terugbrengen");
            if (hours != null)
            {
                MessageBox.Show(hours, "Terugbrengen registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string lateText = now > plannedEnd
                ? "Te laat: " + describe(now - plannedEnd) + " na het geplande einde."
                : "";
            string details =
                "Reservering #" + rentalId + "\n" +
                "Klant: " + Convert.ToString(row["Klant"]) + "\n" +
                "Auto: " + Convert.ToString(row["Auto"]) + "\n" +
                "Verwacht terug: " + plannedEnd.ToString("ddd d MMM HH:mm", Dutch);

            bool damage;
            string remark;
            using (var dialog = new ReturnDialog(details, lateText))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
                {
                    return;
                }
                damage = dialog.Damage;
                remark = dialog.Remark;
            }

            string notesSql = remark.Length == 0
                ? "notes"
                : "LEFT(CONCAT(IF(COALESCE(notes, '') = '', '', CONCAT(notes, ' | ')), " +
                  AuthHelper.sql("Teruggebracht: " + remark) + "), 255)";

            string followUp = "";
            try
            {
                int changed = DBHelper.RunSql(
                    "UPDATE rental SET Status = 'completed', Date_Ended = " + sqlDateTime(now) +
                    ", notes = " + notesSql + " WHERE ID = " + rentalId + " AND Status = 'ongoing'");

                if (changed == 0)
                {
                    MessageBox.Show("Deze reservering is intussen gewijzigd. De lijst wordt vernieuwd.",
                        "Terugbrengen registreren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    loadData();
                    return;
                }

                if (damage)
                {
                    DBHelper.RunSql("UPDATE car SET Car_Status = 'disabled' WHERE ID = " + carId);

                    object future = DBHelper.getScalar(
                        "SELECT COUNT(*) FROM rental WHERE CarID = " + carId +
                        " AND Status = 'reserved' AND Date_Started > " + sqlDateTime(now));
                    int futureCount = Convert.ToInt32(future);

                    followUp = "\n\nDe auto is op buiten gebruik gezet." +
                               (futureCount > 0
                                   ? "\nLet op: deze auto heeft nog " + futureCount + " toekomstige reservering(en). Regel een andere auto."
                                   : "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Het terugbrengen kon niet worden geregistreerd: " + ex.Message, "Terugbrengen registreren",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                loadData();
                return;
            }

            loadData();
            setInfo(lblReturnInfo, "Terugbrengen geregistreerd voor reservering #" + rentalId + ".", SuccessColor);

            if (followUp.Length > 0 || lateText.Length > 0)
            {
                MessageBox.Show(
                    "Terugbrengen geregistreerd voor reservering #" + rentalId + "." +
                    (lateText.Length > 0 ? "\n" + lateText : "") + followUp,
                    "Terugbrengen registreren", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Klein venster waarin de medewerker bij het terugbrengen schade en een opmerking kan vastleggen.
        private class ReturnDialog : Form
        {
            private readonly CheckBox chkDamage;
            private readonly TextBox txtRemark;

            public bool Damage
            {
                get { return chkDamage.Checked; }
            }

            public string Remark
            {
                get { return txtRemark.Text.Trim(); }
            }

            public ReturnDialog(string details, string lateText)
            {
                Text = "Terugbrengen registreren";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                Font = new Font("Segoe UI", 9.75F);
                BackColor = Color.White;
                ClientSize = new Size(460, 360);

                var lblDetails = new Label
                {
                    Location = new Point(20, 16),
                    Size = new Size(420, 88),
                    ForeColor = TextColor,
                    Text = details
                };
                var lblLate = new Label
                {
                    Location = new Point(20, 108),
                    Size = new Size(420, 24),
                    ForeColor = ErrorColor,
                    Text = lateText
                };
                chkDamage = new CheckBox
                {
                    Location = new Point(20, 142),
                    Size = new Size(420, 24),
                    Text = "Er is schade aan de auto (auto buiten gebruik zetten)"
                };
                var lblRemark = new Label
                {
                    Location = new Point(20, 180),
                    AutoSize = true,
                    ForeColor = MutedColor,
                    Text = "Opmerking (optioneel)"
                };
                txtRemark = new TextBox
                {
                    Location = new Point(20, 204),
                    Size = new Size(420, 84),
                    Multiline = true,
                    MaxLength = 200
                };

                var btnOk = new Button
                {
                    Text = "Registreer terugbrengen",
                    DialogResult = DialogResult.OK,
                    Location = new Point(170, 308),
                    Size = new Size(180, 36),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = AccentColor,
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnCancel = new Button
                {
                    Text = "Annuleren",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(360, 308),
                    Size = new Size(80, 36),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = HeaderColor,
                    ForeColor = TextColor,
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                Controls.Add(lblDetails);
                Controls.Add(lblLate);
                Controls.Add(chkDamage);
                Controls.Add(lblRemark);
                Controls.Add(txtRemark);
                Controls.Add(btnOk);
                Controls.Add(btnCancel);

                AcceptButton = btnOk;
                CancelButton = btnCancel;
            }
        }
    }
}
