using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Pagina onder de topbalk voor rol 2 (medewerker).
    public class EmployeePage : UserControl
    {
        private static readonly Color AccentColor = Color.FromArgb(37, 99, 235);
        private static readonly Color DarkColor = Color.FromArgb(31, 41, 55);
        private static readonly Color MutedColor = Color.FromArgb(107, 114, 128);
        private static readonly Color BorderColor = Color.FromArgb(229, 231, 235);
        private static readonly Color TabHeaderColor = Color.FromArgb(243, 244, 246);

        private readonly TabControl tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Name = nameof(tabs),
            ItemSize = new Size(0, 48),
            DrawMode = TabDrawMode.OwnerDrawFixed
        };

        public EmployeePage()
        {
            // Zorg ervoor dat oude instellingen worden verwijderd
            this.DoubleBuffered = true;
            
            BackColor = Color.White;
            ForeColor = DarkColor;
            Font = new Font("Segoe UI", 9.75F);
            Padding = new Padding(0);

            SuspendLayout();

            // Verwijder oude tabs
            tabs.TabPages.Clear();
            tabs.Controls.Clear();
            
            tabs.ItemSize = new Size(0, 48);
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem += TabControl_DrawItem;
            
            AddCommonTabs();

            if (AuthHelper.loggedinroleid == AuthHelper.RoleAdmin)
            {
                AddAdminTabs();
            }

            Controls.Add(tabs);

            if (tabs.TabPages.Count > 0)
            {
                tabs.SelectedIndex = 0;
            }

            ResumeLayout(false);
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabs.TabPages[e.Index];
            Rectangle tabRect = tabs.GetTabRect(e.Index);

            bool isSelected = e.Index == tabs.SelectedIndex;
            Color bgColor = isSelected ? Color.White : TabHeaderColor;
            Color textColor = isSelected ? AccentColor : MutedColor;
            Font tabFont = isSelected ? new Font("Segoe UI", 11F, FontStyle.Bold) : new Font("Segoe UI", 10F);

            e.Graphics.Clear(bgColor);
            e.Graphics.FillRectangle(new SolidBrush(bgColor), tabRect);

            // Underline voor actieve tab
            if (isSelected)
            {
                using (var pen = new Pen(AccentColor, 4))
                {
                    e.Graphics.DrawLine(pen, tabRect.Left, tabRect.Bottom - 4, tabRect.Right, tabRect.Bottom - 4);
                }
            }

            // Border tussen tabs
            using (var pen = new Pen(BorderColor, 1))
            {
                e.Graphics.DrawLine(pen, tabRect.Right - 1, tabRect.Top, tabRect.Right - 1, tabRect.Bottom);
            }

            // Text met emoji's
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.DrawString(page.Text, tabFont, new SolidBrush(textColor), tabRect, stringFormat);
        }

        private void AddCommonTabs()
        {
            AddTab("📅 Dagoverzicht", new DayOverviewControl());
            AddTab("📊 Planning", new PlanningControl());
            AddTab("🆕 Nieuwe reservering", new NewReservationControl());
            AddTab("🚗 Ophalen / terugbrengen", new PickupReturnControl());
            AddTab("✏️ Wijzigen / annuleren", new ChangeCancelControl());
            AddTab("👥 Klanten", new CustomersControl());
        }

        private void AddAdminTabs()
        {
            AddTab("⚙️ Extra's en prijzen", new ExtrasPricesControl());
            AddTab("🔐 Gebruikers en rollen", new UsersRolesControl());
        }

        public void AddTab(string title, Control content)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Een tabblad moet een titel hebben.", nameof(title));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            TabPage page = new TabPage(title)
            {
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            tabs.TabPages.Add(page);
        }
    }
}