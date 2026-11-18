using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Shows one log entry and lets an admin delete it after confirming with their
    // own password. Opened from the Logboek by double-clicking a row.
    public class LogDetailForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);

        private readonly int logId;
        private TextBox txtPassword;

        public LogDetailForm(int logId, string tijd, string who, string action, string details)
        {
            this.logId = logId;
            ClientSize = new Size(440, 440);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Logregel";
            buildUi(tijd, who, action, details);
        }

        private void buildUi(string tijd, string who, string action, string details)
        {
            var title = new Label { Text = "Logregel", Dock = DockStyle.Top, Height = 52, Font = new Font("Segoe UI", 16F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var btnClose = new Button { Text = "✕", Location = new Point(404, 6), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            field("Tijdstip", tijd, 70);
            field("Door", who, 118);
            field("Actie", action, 166);

            Controls.Add(new Label { Text = "Details", Location = new Point(30, 214), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) });
            Controls.Add(new Label { Text = string.IsNullOrEmpty(details) ? "-" : details, Location = new Point(30, 232), Size = new Size(380, 44), ForeColor = Ink, Font = new Font("Segoe UI", 10F) });

            var sep = new Label { Location = new Point(0, 288), Size = new Size(440, 1), BackColor = Color.FromArgb(229, 231, 235) };
            Controls.Add(sep);

            Controls.Add(new Label { Text = "Bevestig met je wachtwoord om te verwijderen", Location = new Point(30, 302), AutoSize = true, ForeColor = Muted });
            txtPassword = new TextBox { Location = new Point(30, 324), Width = 380, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10F) };
            Controls.Add(txtPassword);

            var btnDelete = new Button
            {
                Text = "Verwijderen",
                Location = new Point(230, 380),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) => delete();

            var btnCancel = new Button { Text = "Sluiten", Location = new Point(30, 380), Size = new Size(150, 40), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, Cursor = Cursors.Hand };

            Controls.AddRange(new Control[] { title, btnClose, btnDelete, btnCancel });
            title.BringToFront();
            CancelButton = btnCancel;
        }

        private void field(string label, string value, int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(30, y), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) });
            Controls.Add(new Label { Text = string.IsNullOrEmpty(value) ? "-" : value, Location = new Point(30, y + 18), Size = new Size(380, 22), ForeColor = Ink, Font = new Font("Segoe UI", 10F) });
        }

        private void delete()
        {
            if (txtPassword.Text.Length == 0)
            {
                MessageBox.Show("Vul je wachtwoord in om te bevestigen.", "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // Confirm with the logged-in admin's own password.
            try
            {
                object stored = DBHelper.getScalar("SELECT Password FROM user WHERE ID = " + DBHelper.loggedinuserid);
                if (stored == null || stored == DBNull.Value || !AuthHelper.VerifyPassword(txtPassword.Text, stored.ToString()))
                {
                    MessageBox.Show("Onjuist wachtwoord.", "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon het wachtwoord niet controleren:\n" + ex.Message, "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                DBHelper.RunSql("DELETE FROM activity_log WHERE ID = " + logId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de logregel niet verwijderen:\n" + ex.Message, "Verwijderen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
