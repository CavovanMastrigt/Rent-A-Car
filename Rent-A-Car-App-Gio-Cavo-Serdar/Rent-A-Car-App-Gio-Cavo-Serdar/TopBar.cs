using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Navigation bar for the top of a screen: "Rent A Car" on the left, on the right a "Log in" button
    // or, when logged in, the profile button. Clicking the profile button slides a menu in from the right
    // with Profile settings and Log out.
    public partial class TopBar : UserControl
    {
        // Raised after logging in or out, so the screen can show the right page.
        public event EventHandler LoginChanged;

        private const int MenuWidth = 250;
        private const int SlideStep = 25;

        private static readonly Color BorderColor = Color.FromArgb(229, 231, 235);
        private static readonly Color HoverColor = Color.FromArgb(243, 244, 246);
        private static readonly Color MutedColor = Color.FromArgb(107, 114, 128);

        private Panel menuPanel;
        private Label lblMenuName;
        private Label lblMenuEmail;
        private int menuVisibleWidth = 0;
        private bool menuOpen = false;

        public TopBar()
        {
            InitializeComponent();

            // Make the initials label a circle.
            var circle = new GraphicsPath();
            circle.AddEllipse(0, 0, lblInitials.Width, lblInitials.Height);
            lblInitials.Region = new Region(circle);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)
            {
                return;
            }

            createMenu();
            loadUser();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            positionProfileButton();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(BorderColor))
            {
                e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            }
        }

        // Shows "Log in" when nobody is logged in, otherwise the user's name on the button and in the menu.
        public void loadUser()
        {
            bool loggedIn = DBHelper.loggedinuserid != 0;
            btnLogin.Visible = !loggedIn;
            btnProfile.Visible = loggedIn;
            lblInitials.Visible = loggedIn;

            if (!loggedIn)
            {
                positionProfileButton();
                return;
            }

            DataTable user = DBHelper.getdataTable(
                "SELECT Name, LastName, Email FROM user WHERE ID = " + DBHelper.loggedinuserid);

            if (user.Rows.Count == 0)
            {
                return;
            }

            string name = user.Rows[0]["Name"].ToString();
            string lastName = user.Rows[0]["LastName"].ToString();

            btnProfile.Text = name + " " + lastName + "  ▾";
            lblInitials.Text = (name.Substring(0, 1) + lastName.Substring(0, 1)).ToUpper();
            lblMenuName.Text = name + " " + lastName + (AuthHelper.loggedinroleid == AuthHelper.RoleAdmin ? "  (Admin)" : "");
            lblMenuEmail.Text = user.Rows[0]["Email"].ToString();

            positionProfileButton();
        }

        private void positionProfileButton()
        {
            btnProfile.Left = Width - btnProfile.Width - 16;
            lblInitials.Left = btnProfile.Left - lblInitials.Width - 4;
            btnLogin.Left = Width - btnLogin.Width - 16;
        }

        // The menu is added to the form (not to this bar) so it can hang down over the screen's content.
        private void createMenu()
        {
            Form form = FindForm();

            menuPanel = new Panel { BackColor = Color.White, Visible = false, Width = MenuWidth };
            menuPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderColor))
                {
                    e.Graphics.DrawLine(pen, 0, 0, 0, menuPanel.Height);
                }
            };

            lblMenuName = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(MenuWidth - 40, 22),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            lblMenuEmail = new Label
            {
                Location = new Point(20, 42),
                Size = new Size(MenuWidth - 40, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = MutedColor
            };
            var separator = new Label
            {
                Location = new Point(0, 76),
                Size = new Size(MenuWidth, 1),
                BackColor = BorderColor
            };

            Button btnSettings = createMenuButton("Profile settings", 86);
            btnSettings.Click += btnSettings_Click;

            Button btnLogout = createMenuButton("Log out", 126);
            btnLogout.ForeColor = Color.FromArgb(220, 38, 38);
            btnLogout.Click += btnLogout_Click;

            menuPanel.Controls.Add(lblMenuName);
            menuPanel.Controls.Add(lblMenuEmail);
            menuPanel.Controls.Add(separator);
            menuPanel.Controls.Add(btnSettings);
            menuPanel.Controls.Add(btnLogout);

            form.Controls.Add(menuPanel);
            menuPanel.BringToFront();

            form.Resize += (s, e) => positionMenu();
            positionMenu();
        }

        private Button createMenuButton(string text, int top)
        {
            var button = new Button
            {
                Text = "   " + text,
                Location = new Point(0, top),
                Size = new Size(MenuWidth, 38),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.75F),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = HoverColor;
            return button;
        }

        private void positionMenu()
        {
            Form form = FindForm();
            menuPanel.Top = Bottom;
            menuPanel.Height = form.ClientSize.Height - Bottom;
            menuPanel.Left = form.ClientSize.Width - menuVisibleWidth;
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            menuOpen = !menuOpen;
            menuPanel.Visible = true;
            menuPanel.BringToFront();
            slideTimer.Start();
        }

        // Moves the menu a bit further in or out on every tick, until it is fully open or closed.
        private void slideTimer_Tick(object sender, EventArgs e)
        {
            if (menuOpen)
            {
                menuVisibleWidth = Math.Min(MenuWidth, menuVisibleWidth + SlideStep);
            }
            else
            {
                menuVisibleWidth = Math.Max(0, menuVisibleWidth - SlideStep);
            }

            positionMenu();

            if (menuVisibleWidth == MenuWidth || menuVisibleWidth == 0)
            {
                slideTimer.Stop();
                menuPanel.Visible = menuVisibleWidth > 0;
            }
        }

        private void hideMenuNow()
        {
            slideTimer.Stop();
            menuOpen = false;
            menuVisibleWidth = 0;
            menuPanel.Visible = false;
            positionMenu();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            using (var loginForm = new LoginForm())
            {
                if (PopupForm.show(FindForm(), loginForm) == DialogResult.OK)
                {
                    loadUser();
                    LoginChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            hideMenuNow();
            using (var profileForm = new ProfileForm())
            {
                if (PopupForm.show(FindForm(), profileForm) == DialogResult.OK)
                {
                    loadUser();
                }
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            hideMenuNow();
            DBHelper.loggedinuserid = 0;
            AuthHelper.loggedinroleid = 0;
            loadUser();
            LoginChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
