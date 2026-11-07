
using System.Drawing;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    public static class UITheme
    {
        // Achtergronden
        public static readonly Color Background =
            Color.FromArgb(243, 244, 246);

        public static readonly Color Surface =
            Color.White;

        // Kleuren
        public static readonly Color Primary =
            Color.FromArgb(79, 70, 229);

        public static readonly Color PrimaryHover =
            Color.FromArgb(67, 56, 202);

        public static readonly Color Text =
            Color.FromArgb(31, 41, 55);

        public static readonly Color Muted =
            Color.FromArgb(107, 114, 128);

        public static readonly Color Border =
            Color.FromArgb(229, 231, 235);

        public static readonly Color TableHeader =
            Color.FromArgb(249, 250, 251);

        public static readonly Color RowAlternate =
            Color.FromArgb(249, 250, 251);

        public static readonly Color Selection =
            Color.FromArgb(224, 231, 255);

        // Lettertypes
        public static readonly Font Heading =
            new Font("Segoe UI", 18, FontStyle.Bold);

        public static readonly Font SubHeading =
            new Font("Segoe UI", 11, FontStyle.Regular);

        public static readonly Font Body =
            new Font("Segoe UI", 9.75F);

        public static readonly Font Button =
            new Font("Segoe UI", 9.5F, FontStyle.Bold);
    }
}
