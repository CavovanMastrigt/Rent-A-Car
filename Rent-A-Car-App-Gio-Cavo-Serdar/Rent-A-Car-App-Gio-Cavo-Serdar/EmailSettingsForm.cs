using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Admin configures the SMTP server used to e-mail invoices. Stored in app_setting.
    public class EmailSettingsForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private TextBox txtHost, txtPort, txtFrom, txtUser, txtPass;
        private CheckBox chkSsl;

        public EmailSettingsForm()
        {
            ClientSize = new Size(440, 470);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "E-mailinstellingen";
            buildUi();
            load();
        }

        private void buildUi()
        {
            var title = new Label { Text = "E-mailinstellingen", Dock = DockStyle.Top, Height = 54, Font = new Font("Segoe UI", 16F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var btnClose = new Button { Text = "✕", Location = new Point(404, 6), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            var info = new Label { Text = "SMTP-server waarmee facturen verstuurd worden.", Location = new Point(30, 62), Size = new Size(380, 20), ForeColor = Muted };

            txtHost = field("SMTP-server (host)", 30, 92, 400);
            txtPort = field("Poort", 30, 150, 110);
            chkSsl = new CheckBox { Text = "SSL/TLS gebruiken", Location = new Point(160, 172), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            Controls.Add(chkSsl);

            txtFrom = field("Afzender (e-mail)", 30, 208, 400);
            txtUser = field("Gebruikersnaam", 30, 266, 400);
            txtPass = field("Wachtwoord", 30, 324, 400);
            txtPass.UseSystemPasswordChar = true;

            var btnSave = new Button { Text = "Opslaan", Location = new Point(220, 410), Size = new Size(190, 40), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => save();

            var btnCancel = new Button { Text = "Annuleren", Location = new Point(30, 410), Size = new Size(150, 40), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, Cursor = Cursors.Hand };

            Controls.AddRange(new Control[] { title, btnClose, info, chkSsl, btnSave, btnCancel });
            title.BringToFront();
            CancelButton = btnCancel;
        }

        private TextBox field(string label, int x, int y, int width)
        {
            Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = Muted });
            var box = new TextBox { Location = new Point(x, y + 20), Width = width, Font = new Font("Segoe UI", 10F) };
            Controls.Add(box);
            return box;
        }

        private void load()
        {
            txtHost.Text = AppSettings.Get("smtp_host", "smtp.gmail.com");
            txtPort.Text = AppSettings.Get("smtp_port", "587");
            txtFrom.Text = AppSettings.Get("smtp_from");
            txtUser.Text = AppSettings.Get("smtp_user");
            txtPass.Text = AppSettings.Get("smtp_pass");
            chkSsl.Checked = AppSettings.Get("smtp_ssl", "1") == "1";
        }

        private void save()
        {
            int port;
            if (txtPort.Text.Trim() != "" && !int.TryParse(txtPort.Text.Trim(), out port))
            {
                MessageBox.Show("Vul een geldig poortnummer in.", "E-mailinstellingen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                AppSettings.Set("smtp_host", txtHost.Text.Trim());
                AppSettings.Set("smtp_port", txtPort.Text.Trim() == "" ? "587" : txtPort.Text.Trim());
                AppSettings.Set("smtp_from", txtFrom.Text.Trim());
                AppSettings.Set("smtp_user", txtUser.Text.Trim());
                AppSettings.Set("smtp_pass", txtPass.Text);
                AppSettings.Set("smtp_ssl", chkSsl.Checked ? "1" : "0");
                LogHelper.Write("E-mailinstellingen gewijzigd", txtHost.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de instellingen niet opslaan:\n" + ex.Message, "E-mailinstellingen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
