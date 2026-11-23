using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Asks the staff member for a cancellation reason. The reason is sent to the
    // customer by message and e-mail.
    public class CancelRentalForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);

        private TextBox txtReason;
        public string Reason { get; private set; } = "";

        public CancelRentalForm(string summary)
        {
            ClientSize = new Size(440, 320);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Reservering annuleren";
            buildUi(summary);
        }

        private void buildUi(string summary)
        {
            var title = new Label { Text = "Reservering annuleren", Dock = DockStyle.Top, Height = 48, Font = new Font("Segoe UI", 16F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var sub = new Label { Text = summary, Dock = DockStyle.Top, Height = 40, ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter };
            var btnClose = new Button { Text = "✕", Location = new Point(404, 6), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            Controls.Add(new Label { Text = "Reden (gaat naar de klant per bericht en e-mail)", Location = new Point(30, 104), AutoSize = true, ForeColor = Muted });
            txtReason = new TextBox { Location = new Point(30, 124), Width = 380, Height = 110, Multiline = true, Font = new Font("Segoe UI", 10F) };

            var btnConfirm = new Button { Text = "Annuleren & melden", Location = new Point(210, 258), Size = new Size(200, 42), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(220, 38, 38), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += (s, e) => confirm();
            var btnBack = new Button { Text = "Terug", Location = new Point(30, 258), Size = new Size(140, 42), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, Cursor = Cursors.Hand };

            Controls.AddRange(new Control[] { title, sub, btnClose, txtReason, btnConfirm, btnBack });
            title.BringToFront();
            CancelButton = btnBack;
        }

        private void confirm()
        {
            if (txtReason.Text.Trim() == "")
            {
                MessageBox.Show("Vul een reden in.", "Reservering annuleren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReason.Focus();
                return;
            }
            Reason = txtReason.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
