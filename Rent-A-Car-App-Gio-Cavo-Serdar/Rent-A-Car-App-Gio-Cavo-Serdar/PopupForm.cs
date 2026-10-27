using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Base for small screens (login, register, profile) that open as a popup:
    // no window border, a thin outline with a shadow, and the screen behind it dimmed.
    public class PopupForm : Form
    {
        private static readonly Color BorderColor = Color.FromArgb(209, 213, 219);
        private const int DropShadow = 0x20000;

        public PopupForm()
        {
            BackColor = Color.White;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= DropShadow;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(BorderColor))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }

        // KeyPress handler for phone number boxes: only digits, and a "+" as the first character.
        protected void onlyPhoneNumber(object sender, KeyPressEventArgs e)
        {
            var box = (TextBox)sender;
            bool isPlusAtStart = e.KeyChar == '+' && box.SelectionStart == 0 && !box.Text.Contains("+");
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !isPlusAtStart)
            {
                e.Handled = true;
            }
        }

        // Also catches letters that were pasted in.
        public static bool isPhoneNumber(string text)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(text.Trim(), @"^\+?[0-9]{6,15}$");
        }

        // Dims the owner window, shows the popup in the middle of it and waits until it is closed.
        public static DialogResult show(Form owner, Form popup)
        {
            using (var dim = new Form())
            {
                dim.FormBorderStyle = FormBorderStyle.None;
                dim.BackColor = Color.Black;
                dim.Opacity = 0.4;
                dim.ShowInTaskbar = false;
                dim.StartPosition = FormStartPosition.Manual;
                dim.Bounds = owner.RectangleToScreen(owner.ClientRectangle);
                dim.Show(owner);

                popup.StartPosition = FormStartPosition.CenterParent;
                return popup.ShowDialog(dim);
            }
        }
    }
}
