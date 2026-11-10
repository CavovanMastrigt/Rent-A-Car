using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Beheerdersscherm (user story 7): rollen van gebruikers wijzigen en nieuwe medewerker-accounts aanmaken.
    // Rollen: klant (0), beheerder (1), medewerker (2). Een nieuwe rol geldt vanaf de volgende keer inloggen.
    // Alleen beheerders mogen dit scherm gebruiken. Dat wordt hier bij elke actie nogmaals gecontroleerd,
    // zodat een medewerker zichzelf nooit beheerder kan maken.
    public class UsersRolesControl : UserControl
    {
        private const string Title = "Gebruikers en rollen";

        private class RoleItem
        {
            public int Id;
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }

        // Filter in de lijst: index 0 = alle rollen.
        private static readonly string[] FilterNames = { "Alle rollen", "Klanten", "Medewerkers", "Beheerders" };
        private static readonly int[] FilterRoles =
            { -1, AuthHelper.RoleCustomer, AuthHelper.RoleEmployee, AuthHelper.RoleAdmin };

        private readonly TextBox txtSearch = new TextBox { Location = new Point(16, 14), Size = new Size(250, 27) };
        private readonly ComboBox cmbFilter = new ComboBox
        {
            Location = new Point(274, 14),
            Size = new Size(130, 27),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        private readonly DataGridView grid = Ui.Grid();

        // Rol wijzigen
        private readonly Label lblUser = new Label
        {
            Location = new Point(24, 46),
            Size = new Size(440, 24),
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Ui.Ink,
            Text = "Kies links een gebruiker"
        };
        private readonly ComboBox cmbRole = NewRoleBox(24, 98);
        private readonly Button btnSaveRole = Ui.Primary("Rol opslaan", 286, 96, 120);
        private readonly Label lblRole = Ui.Note(24, 132, 440, 40);

        // Nieuw account
        private readonly TextBox txtFirst, txtLast, txtEmail, txtPhone, txtCity, txtAddress, txtPassword;
        private readonly ComboBox cmbNewRole = NewRoleBox(244, 478);
        private readonly Button btnCreate = Ui.Primary("Account aanmaken", 24, 530, 180);
        private readonly Label lblCreate = Ui.Note(24, 572, 440, 40);

        private int userId = 0;
        private int userRole = 0;
        private bool ready = false;
        private bool loadingList = false;

        private static bool IsAdmin
        {
            get { return AuthHelper.loggedinroleid == AuthHelper.RoleAdmin; }
        }

        public UsersRolesControl()
        {
            Ui.Style(this);

            // Links: zoeken, filteren en de lijst.
            Button btnSearch = Ui.Btn("Zoeken", 412, 12, 90);
            Button btnRefresh = Ui.Btn("Vernieuwen", 510, 12, 100);
            var search = new Panel { Dock = DockStyle.Top, Height = 56 };
            search.Controls.AddRange(new Control[] { txtSearch, cmbFilter, btnSearch, btnRefresh });
            var left = new Panel { Dock = DockStyle.Left, Width = 640 };
            left.Controls.Add(grid);
            left.Controls.Add(search);
            cmbFilter.Items.AddRange(FilterNames);
            cmbFilter.SelectedIndex = 0;

            // Rechts: rol wijzigen en een nieuw account aanmaken.
            var right = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            right.Controls.AddRange(new Control[]
            {
                Ui.Heading("Rol wijzigen", 24, 14), lblUser, Ui.Caption("Rol", 24, 76), cmbRole, btnSaveRole, lblRole,
                Ui.Heading("Nieuw account", 24, 190)
            });
            txtFirst = AddField(right, "Voornaam", 24, 224, 200);
            txtLast = AddField(right, "Achternaam", 244, 224, 200);
            txtEmail = AddField(right, "E-mail", 24, 282, 420);
            txtPhone = AddField(right, "Telefoon", 24, 340, 200);
            txtCity = AddField(right, "Plaats", 244, 340, 200);
            txtAddress = AddField(right, "Adres", 24, 398, 420);
            txtPassword = AddField(right, "Wachtwoord (minimaal 8 tekens)", 24, 456, 200);
            txtPassword.UseSystemPasswordChar = true;
            right.Controls.AddRange(new Control[] { Ui.Caption("Rol", 244, 456), cmbNewRole, btnCreate, lblCreate });
            cmbNewRole.SelectedIndex = 1;   // standaard: medewerker

            Controls.Add(right);
            Controls.Add(left);

            btnSearch.Click += (s, e) => LoadList();
            btnRefresh.Click += (s, e) => LoadList();
            cmbFilter.SelectedIndexChanged += (s, e) => LoadList();
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadList();
                }
            };
            grid.SelectionChanged += (s, e) =>
            {
                DataRowView row = Ui.Row(grid);
                if (ready && !loadingList && row != null)
                {
                    ShowUser(row);
                }
            };
            btnSaveRole.Click += (s, e) => SaveRole();
            btnCreate.Click += (s, e) => CreateAccount();
            btnSaveRole.Enabled = false;
            cmbRole.Enabled = false;
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

        // Keuzelijst met de drie rollen: klant, medewerker, beheerder.
        private static ComboBox NewRoleBox(int x, int y)
        {
            var box = new ComboBox { Location = new Point(x, y), Size = new Size(200, 27), DropDownStyle = ComboBoxStyle.DropDownList };
            box.Items.Add(new RoleItem { Id = AuthHelper.RoleCustomer, Name = "Klant" });
            box.Items.Add(new RoleItem { Id = AuthHelper.RoleEmployee, Name = "Medewerker" });
            box.Items.Add(new RoleItem { Id = AuthHelper.RoleAdmin, Name = "Beheerder" });
            return box;
        }

        // Een invoerveld met een kopje erboven.
        private static TextBox AddField(Panel panel, string caption, int x, int y, int width)
        {
            var box = new TextBox { Location = new Point(x, y + 22), Size = new Size(width, 27), MaxLength = 100 };
            panel.Controls.Add(Ui.Caption(caption, x, y));
            panel.Controls.Add(box);
            return box;
        }

        // ---------- De lijst ----------

        private void LoadList()
        {
            string where = "WHERE 1 = 1 ";
            if (cmbFilter.SelectedIndex > 0)
            {
                where += "AND RID = " + FilterRoles[cmbFilter.SelectedIndex] + " ";
            }
            string text = txtSearch.Text.Trim();
            if (text.Length > 0)
            {
                string like = Sql.Like(text);
                where += "AND (Name LIKE " + like + " OR LastName LIKE " + like + " OR Email LIKE " + like +
                         " OR CONCAT(Name, ' ', LastName) LIKE " + like + ") ";
            }

            try
            {
                DataTable table = DBHelper.getdataTable(
                    "SELECT ID AS `Nr`, CONCAT(Name, ' ', LastName) AS `Naam`, Email AS `E-mail`, " +
                    "CASE RID WHEN " + AuthHelper.RoleCustomer + " THEN 'Klant' WHEN " + AuthHelper.RoleEmployee +
                    " THEN 'Medewerker' WHEN " + AuthHelper.RoleAdmin + " THEN 'Beheerder' ELSE 'Onbekend' END AS `Rol`, " +
                    "RID AS `RoleRaw`, User_Status AS `Status` FROM user " + where + "ORDER BY LastName, Name LIMIT 300");
                foreach (DataRow row in table.Rows)
                {
                    row["Status"] = StatusText.Customer(Convert.ToString(row["Status"]));
                }

                loadingList = true;
                Ui.Bind(grid, table, "RoleRaw");
                bool found = Ui.Reselect(grid, userId);
                loadingList = false;

                if (found)
                {
                    ShowUser(Ui.Row(grid));
                }
                else
                {
                    userId = 0;
                    lblUser.Text = "Kies links een gebruiker";
                    cmbRole.Enabled = false;
                    btnSaveRole.Enabled = false;
                    lblRole.Text = "";
                }
            }
            catch (Exception ex)
            {
                loadingList = false;
                Ui.Warn("De gebruikers konden niet worden geladen: " + ex.Message, Title);
            }
        }

        private void ShowUser(DataRowView row)
        {
            userId = Convert.ToInt32(row["Nr"]);
            userRole = Convert.ToInt32(row["RoleRaw"]);
            lblUser.Text = Convert.ToString(row["Naam"]) + " (" + Convert.ToString(row["E-mail"]) + ")";

            foreach (object item in cmbRole.Items)
            {
                if (((RoleItem)item).Id == userRole)
                {
                    cmbRole.SelectedItem = item;
                }
            }

            // Je eigen rol wijzigen kan niet: zo sluit je jezelf nooit buiten.
            bool self = userId == DBHelper.loggedinuserid;
            cmbRole.Enabled = !self;
            btnSaveRole.Enabled = !self;
            Ui.Say(lblRole, self
                ? "Je kunt je eigen rol niet wijzigen."
                : "De nieuwe rol geldt vanaf de volgende keer dat deze gebruiker inlogt.", Ui.Muted);
        }

        // ---------- Rol opslaan ----------

        private void SaveRole()
        {
            RoleItem role = cmbRole.SelectedItem as RoleItem;
            if (userId == 0 || role == null)
            {
                return;
            }
            if (!IsAdmin)
            {
                Ui.Warn("Alleen beheerders kunnen rollen wijzigen.", Title);
                return;
            }
            if (userId == DBHelper.loggedinuserid)
            {
                Ui.Say(lblRole, "Je kunt je eigen rol niet wijzigen.", Ui.Error);
                return;
            }
            if (role.Id == userRole)
            {
                Ui.Say(lblRole, "Deze gebruiker heeft die rol al.", Ui.Error);
                return;
            }

            try
            {
                // Er moet altijd minstens één andere beheerder overblijven.
                if (userRole == AuthHelper.RoleAdmin &&
                    Sql.Count("SELECT COUNT(*) FROM user WHERE RID = " + AuthHelper.RoleAdmin + " AND ID <> " + userId) == 0)
                {
                    Ui.Say(lblRole, "Er moet minstens één beheerder overblijven.", Ui.Error);
                    return;
                }
                if (!Ui.Ask("De rol van " + lblUser.Text + " wijzigen naar " + role.Name.ToLowerInvariant() + "?", Title))
                {
                    return;
                }

                DBHelper.RunSql("UPDATE user SET RID = " + role.Id + " WHERE ID = " + userId);
                LoadList();
                Ui.Say(lblRole, "De rol is gewijzigd naar " + role.Name.ToLowerInvariant() + ".", Ui.Success);
            }
            catch (Exception ex)
            {
                Ui.Fail("De rol kon niet worden gewijzigd: " + ex.Message, Title);
            }
        }

        // ---------- Nieuw account ----------

        private void CreateAccount()
        {
            RoleItem role = cmbNewRole.SelectedItem as RoleItem;
            if (!IsAdmin || role == null)
            {
                Ui.Warn("Alleen beheerders kunnen accounts aanmaken.", Title);
                return;
            }

            string email = txtEmail.Text.Trim();
            string error = null;
            if (txtFirst.Text.Trim().Length == 0 || txtLast.Text.Trim().Length == 0)
            {
                error = "Vul een voor- en achternaam in.";
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                error = "Vul een geldig e-mailadres in.";
            }
            else if (txtPassword.Text.Length < 8)
            {
                error = "Het wachtwoord moet minimaal 8 tekens hebben.";
            }
            if (error != null)
            {
                Ui.Say(lblCreate, error, Ui.Error);
                return;
            }

            try
            {
                if (Sql.Count("SELECT COUNT(*) FROM user WHERE LOWER(Email) = LOWER(" + Sql.Text(email) + ")") > 0)
                {
                    Ui.Say(lblCreate, "Dit e-mailadres is al in gebruik.", Ui.Error);
                    return;
                }

                DBHelper.RunSql(
                    "INSERT INTO user (RID, Created_At, Password, Name, LastName, Address, City, Email, Phone_Number, User_Status) VALUES (" +
                    role.Id + ", CURDATE(), " + Sql.Text(AuthHelper.HashPassword(txtPassword.Text)) + ", " +
                    Sql.Text(txtFirst.Text) + ", " + Sql.Text(txtLast.Text) + ", " + Sql.Text(txtAddress.Text) + ", " +
                    Sql.Text(txtCity.Text) + ", " + Sql.Text(email) + ", " + Sql.Text(txtPhone.Text) + ", 'active')");

                string name = txtFirst.Text.Trim() + " " + txtLast.Text.Trim();
                foreach (TextBox box in new[] { txtFirst, txtLast, txtEmail, txtPhone, txtCity, txtAddress, txtPassword })
                {
                    box.Text = "";
                }
                LoadList();
                Ui.Say(lblCreate, "Account aangemaakt voor " + name + " (" + role.Name.ToLowerInvariant() + "). Hij of zij kan nu inloggen.", Ui.Success);
            }
            catch (Exception ex)
            {
                Ui.Fail("Het account kon niet worden aangemaakt: " + ex.Message, Title);
            }
        }
    }
}
