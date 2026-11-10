using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Medewerkerscherm (user story 8): klanten zoeken en hun gegevens met alle reserveringen bekijken.
    // Dit scherm is alleen om te lezen; het wijzigen van de klantstatus hoort bij het beheer.
    public class CustomersControl : UserControl
    {
        private const string Title = "Klanten";

        private readonly TextBox txtSearch = new TextBox { Location = new Point(16, 14), Size = new Size(380, 27) };
        private readonly DataGridView grdCustomers = Ui.Grid();
        private readonly DataGridView grdRentals = Ui.Grid();
        private readonly Label lblName = new Label
        {
            Location = new Point(24, 14),
            AutoSize = true,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Ui.Ink,
            Text = "Kies links een klant"
        };
        private readonly Label lblInfo = new Label { Location = new Point(24, 52), Size = new Size(560, 112), ForeColor = Ui.Ink };
        private readonly Label lblStats = Ui.Note(24, 168, 560, 24);
        private int selectedId = 0;
        private bool ready = false;
        private bool loadingList = false;

        public CustomersControl()
        {
            Ui.Style(this);

            // Links: zoeken en de lijst met klanten.
            Button btnSearch = Ui.Btn("Zoeken", 404, 12, 90);
            Button btnRefresh = Ui.Btn("Vernieuwen", 502, 12, 100);
            var search = new Panel { Dock = DockStyle.Top, Height = 56 };
            search.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh });
            var left = new Panel { Dock = DockStyle.Left, Width = 620 };
            left.Controls.Add(grdCustomers);
            left.Controls.Add(search);

            // Rechts: de gegevens van de gekozen klant en zijn reserveringen.
            var details = new Panel { Dock = DockStyle.Top, Height = 200 };
            details.Controls.AddRange(new Control[] { lblName, lblInfo, lblStats });
            var right = new Panel { Dock = DockStyle.Fill };
            right.Controls.Add(Ui.Section("Reserveringen", grdRentals));
            right.Controls.Add(details);

            Controls.Add(right);
            Controls.Add(left);

            btnSearch.Click += (s, e) => LoadList();
            btnRefresh.Click += (s, e) => LoadList();
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadList();
                }
            };
            grdCustomers.SelectionChanged += (s, e) =>
            {
                DataRowView row = Ui.Row(grdCustomers);
                if (ready && !loadingList && row != null)
                {
                    ShowCustomer(Convert.ToInt32(row["Nr"]));
                }
            };

            // Klanten en reserveringen kunnen op andere tabbladen veranderen, dus laad opnieuw bij het tonen.
            VisibleChanged += (s, e) =>
            {
                if (ready && Visible)
                {
                    LoadList();
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
            LoadList();
        }

        private void LoadList()
        {
            string where = "WHERE RID = " + AuthHelper.RoleCustomer + " ";
            string text = txtSearch.Text.Trim();
            if (text.Length > 0)
            {
                string like = Sql.Like(text);
                where += "AND (Name LIKE " + like + " OR LastName LIKE " + like + " OR Email LIKE " + like +
                         " OR Phone_Number LIKE " + like + " OR CONCAT(Name, ' ', LastName) LIKE " + like + ") ";
            }

            try
            {
                DataTable table = DBHelper.getdataTable(
                    "SELECT ID AS `Nr`, CONCAT(Name, ' ', LastName) AS `Klant`, Email AS `E-mail`, " +
                    "Phone_Number AS `Telefoon`, User_Status AS `Status` FROM user " + where +
                    "ORDER BY LastName, Name LIMIT 200");
                foreach (DataRow row in table.Rows)
                {
                    row["Status"] = StatusText.Customer(Convert.ToString(row["Status"]));
                }

                loadingList = true;
                Ui.Bind(grdCustomers, table);
                bool found = Ui.Reselect(grdCustomers, selectedId);
                loadingList = false;

                if (found)
                {
                    ShowCustomer(selectedId);
                }
                else
                {
                    Clear(table.Rows.Count == 0 ? "Geen klanten gevonden." : "Kies links een klant");
                }
            }
            catch (Exception ex)
            {
                loadingList = false;
                Ui.Warn("De klanten konden niet worden geladen: " + ex.Message, Title);
            }
        }

        private void Clear(string title)
        {
            selectedId = 0;
            lblName.Text = title;
            lblInfo.Text = "";
            lblStats.Text = "";
            grdRentals.DataSource = null;
        }

        private void ShowCustomer(int id)
        {
            try
            {
                DataTable user = DBHelper.getdataTable(
                    "SELECT Name, LastName, Email, Phone_Number, Address, City, Created_At, User_Status FROM user WHERE ID = " + id);
                if (user.Rows.Count == 0)
                {
                    Clear("Deze klant bestaat niet meer.");
                    return;
                }

                DataRow u = user.Rows[0];
                selectedId = id;
                lblName.Text = Convert.ToString(u["Name"]) + " " + Convert.ToString(u["LastName"]);
                lblInfo.Text =
                    "E-mail: " + Convert.ToString(u["Email"]) + Environment.NewLine +
                    "Telefoon: " + Convert.ToString(u["Phone_Number"]) + Environment.NewLine +
                    "Adres: " + Convert.ToString(u["Address"]) + ", " + Convert.ToString(u["City"]) + Environment.NewLine +
                    "Klant sinds: " + Convert.ToDateTime(u["Created_At"]).ToString("d MMMM yyyy", Ui.Dutch) + Environment.NewLine +
                    "Status: " + StatusText.Customer(Convert.ToString(u["User_Status"]));

                DataTable rentals = DBHelper.getdataTable(
                    "SELECT r.ID AS `Nr`, CONCAT(c.Brand, ' ', c.Model, ' (', c.License_Plate, ')') AS `Auto`, " +
                    "r.Date_Started AS `Ophalen`, r.Date_Planned_Ending AS `Terugbrengen`, r.Status AS `Status`, " +
                    "r.Total_Price AS `Totaal` FROM rental r JOIN car c ON c.ID = r.CarID " +
                    "WHERE r.UserID = " + id + " ORDER BY r.Date_Started DESC");

                // Geannuleerde reserveringen tellen niet mee in het aantal en het totaalbedrag.
                int count = 0;
                decimal sum = 0;
                foreach (DataRow row in rentals.Rows)
                {
                    string status = Convert.ToString(row["Status"]);
                    if (status.ToLowerInvariant() != "cancelled")
                    {
                        count++;
                        sum += Convert.ToDecimal(row["Totaal"]);
                    }
                    row["Status"] = StatusText.Rental(status);
                }

                Ui.Bind(grdRentals, rentals);
                Ui.Format(grdRentals, "Ophalen", "ddd d MMM yyyy HH:mm");
                Ui.Format(grdRentals, "Terugbrengen", "ddd d MMM yyyy HH:mm");
                Ui.Format(grdRentals, "Totaal", "C");
                lblStats.Text = count + " reservering(en), totaal " + Ui.Euro(sum) +
                                (rentals.Rows.Count > count ? " (geannuleerde niet meegeteld)" : "");
            }
            catch (Exception ex)
            {
                Ui.Warn("De klantgegevens konden niet worden geladen: " + ex.Message, Title);
            }
        }
    }
}
