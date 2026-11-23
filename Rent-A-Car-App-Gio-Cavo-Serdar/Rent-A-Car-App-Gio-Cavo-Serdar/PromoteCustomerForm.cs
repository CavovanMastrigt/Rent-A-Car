using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Admin promotes an existing customer to staff (medewerker or beheerder).
    public class PromoteCustomerForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private ComboBox cboCustomer, cboRole;
        private TextBox txtSearch;
        private readonly List<Item> allItems = new List<Item>();

        private class Item
        {
            public int Id;
            public string Name;
            public override string ToString() { return Name; }
        }

        public PromoteCustomerForm()
        {
            ClientSize = new Size(440, 350);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Klant promoveren";
            buildUi();
            loadCustomers();
        }

        private void buildUi()
        {
            var title = new Label { Text = "Klant promoveren", Dock = DockStyle.Top, Height = 52, Font = new Font("Segoe UI", 16F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var sub = new Label { Text = "Maak van een bestaande klant een medewerker of beheerder.", Dock = DockStyle.Top, Height = 24, ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter };
            var btnClose = new Button { Text = "✕", Location = new Point(404, 6), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            Controls.Add(new Label { Text = "Zoek klant", Location = new Point(30, 88), AutoSize = true, ForeColor = Muted });
            txtSearch = new TextBox { Location = new Point(30, 108), Width = 380, Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => applyFilter();

            Controls.Add(new Label { Text = "Klant", Location = new Point(30, 144), AutoSize = true, ForeColor = Muted });
            cboCustomer = new ComboBox { Location = new Point(30, 164), Width = 380, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };

            Controls.Add(new Label { Text = "Nieuwe rol", Location = new Point(30, 204), AutoSize = true, ForeColor = Muted });
            cboRole = new ComboBox { Location = new Point(30, 224), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboRole.Items.AddRange(new object[] { "Medewerker", "Beheerder" });
            cboRole.SelectedIndex = 0;

            var btnOk = new Button { Text = "Promoveren", Location = new Point(220, 294), Size = new Size(190, 40), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) => promote();
            var btnCancel = new Button { Text = "Annuleren", Location = new Point(30, 294), Size = new Size(150, 40), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, Cursor = Cursors.Hand };

            Controls.AddRange(new Control[] { title, sub, btnClose, txtSearch, cboCustomer, cboRole, btnOk, btnCancel });
            title.BringToFront();
            CancelButton = btnCancel;
        }

        private void loadCustomers()
        {
            try
            {
                DataTable t = DBHelper.getdataTable(
                    "SELECT ID, CONCAT(Name,' ',LastName,' - ',Email) AS Naam FROM user WHERE RID = " + AuthHelper.RoleCustomer + " ORDER BY Name, LastName");
                foreach (DataRow r in t.Rows)
                {
                    allItems.Add(new Item { Id = Convert.ToInt32(r["ID"]), Name = r["Naam"].ToString() });
                }
                applyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de klanten niet laden:\n" + ex.Message, "Klant promoveren", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyFilter()
        {
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            cboCustomer.BeginUpdate();
            cboCustomer.Items.Clear();
            foreach (Item it in allItems)
            {
                if (search == "" || it.Name.ToLowerInvariant().Contains(search))
                {
                    cboCustomer.Items.Add(it);
                }
            }
            if (cboCustomer.Items.Count > 0) cboCustomer.SelectedIndex = 0;
            cboCustomer.EndUpdate();
        }

        private void promote()
        {
            if (cboCustomer.SelectedItem == null)
            {
                MessageBox.Show("Kies een klant.", "Klant promoveren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var item = (Item)cboCustomer.SelectedItem;
            int role = cboRole.SelectedItem.ToString() == "Beheerder" ? AuthHelper.RoleAdmin : AuthHelper.RoleEmployee;

            try
            {
                DBHelper.RunSql("UPDATE user SET RID = " + role + " WHERE ID = " + item.Id);
                if (role == AuthHelper.RoleEmployee)
                {
                    PermissionHelper.SeedDefault(item.Id);
                }
                LogHelper.Write("Klant gepromoveerd", item.Name + " -> " + cboRole.SelectedItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de klant niet promoveren:\n" + ex.Message, "Klant promoveren", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("De klant is nu " + cboRole.SelectedItem + ". De nieuwe rol geldt de volgende keer dat diegene inlogt.",
                "Klant promoveren", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
