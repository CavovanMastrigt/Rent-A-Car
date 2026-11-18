using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Modernized profile settings: edit details, change the profile photo, and
    // change the password confirmed with a 2FA code sent by e-mail.
    public partial class ProfileForm : PopupForm
    {
        private static readonly Color Ink = Color.FromArgb(17, 24, 39);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private const long MaxPhotoBytes = 3 * 1024 * 1024;

        private PictureBox picPhoto;
        private Label lblInitials, lblCodeInfo;
        private TextBox txtName, txtLastName, txtEmail, txtPhone, txtAddress, txtCity, txtNewPassword, txtConfirmNewPassword, txtCode;

        private byte[] photoBytes;
        private bool photoChanged;
        private string sentCode;
        private DateTime sentAt;

        public ProfileForm()
        {
            ClientSize = new Size(500, 730);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Profielinstellingen";
            buildUi();
            Load += (s, e) => loadProfile();
        }

        private void buildUi()
        {
            Controls.Add(new Label { Text = "Profielinstellingen", Location = new Point(30, 20), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Ink });
            var btnClose = new Button { Text = "✕", Location = new Point(464, 8), Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, ForeColor = Muted, TabStop = false, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;

            picPhoto = new PictureBox { Location = new Point(30, 70), Size = new Size(88, 88), BackColor = Color.FromArgb(79, 70, 229), SizeMode = PictureBoxSizeMode.Zoom };
            using (var path = new GraphicsPath()) { path.AddEllipse(0, 0, picPhoto.Width, picPhoto.Height); picPhoto.Region = new Region(path); }
            lblInitials = new Label { Location = new Point(30, 70), Size = new Size(88, 88), ForeColor = Color.White, Font = new Font("Segoe UI", 22F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };

            var btnPhoto = new Button { Text = "Foto wijzigen", Location = new Point(136, 82), Size = new Size(150, 32), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPhoto.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnPhoto.Click += (s, e) => pickPhoto();
            var btnRemovePhoto = new Button { Text = "Verwijderen", Location = new Point(296, 82), Size = new Size(120, 32), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(185, 28, 28), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRemovePhoto.FlatAppearance.BorderColor = Color.FromArgb(248, 180, 180);
            btnRemovePhoto.Click += (s, e) => { setPhoto(null); photoChanged = true; };

            txtName = field("Voornaam", 30, 178, 200);
            txtLastName = field("Achternaam", 270, 178, 200);
            txtEmail = field("E-mail", 30, 238, 440);
            txtPhone = field("Telefoon", 30, 298, 200);
            txtCity = field("Woonplaats", 270, 298, 200);
            txtAddress = field("Adres", 30, 358, 440);

            var sep = new Label { Location = new Point(30, 424), Size = new Size(440, 1), BackColor = Color.FromArgb(229, 231, 235) };
            Controls.Add(sep);
            Controls.Add(new Label { Text = "Wachtwoord wijzigen", Location = new Point(30, 436), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Ink });
            Controls.Add(new Label { Text = "Vul een nieuw wachtwoord in en bevestig met de code die we e-mailen.", Location = new Point(30, 462), AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 8.5F) });

            txtNewPassword = field("Nieuw wachtwoord", 30, 488, 200);
            txtNewPassword.UseSystemPasswordChar = true;
            txtConfirmNewPassword = field("Bevestig nieuw wachtwoord", 270, 488, 200);
            txtConfirmNewPassword.UseSystemPasswordChar = true;

            var btnSendCode = new Button { Text = "Verstuur code", Location = new Point(30, 556), Size = new Size(160, 32), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Accent, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSendCode.FlatAppearance.BorderColor = Color.FromArgb(191, 210, 250);
            btnSendCode.Click += (s, e) => sendCode();
            txtCode = new TextBox { Location = new Point(206, 557), Width = 150, Font = new Font("Segoe UI", 11F), Enabled = false };
            Controls.Add(new Label { Text = "Code", Location = new Point(206, 538), AutoSize = true, ForeColor = Muted });
            lblCodeInfo = new Label { Location = new Point(30, 594), Size = new Size(440, 20), ForeColor = Muted, Font = new Font("Segoe UI", 8.5F) };

            var btnSave = new Button { Text = "Opslaan", Location = new Point(310, 664), Size = new Size(160, 40), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            var btnCancel = new Button { Text = "Annuleren", Location = new Point(30, 664), Size = new Size(150, 40), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel, Cursor = Cursors.Hand };

            Controls.AddRange(new Control[] { btnClose, btnPhoto, btnRemovePhoto, txtCode, lblCodeInfo, btnSendCode, btnSave, btnCancel, lblInitials, picPhoto });
            lblInitials.BringToFront();
            CancelButton = btnCancel;
        }

        private TextBox field(string label, int x, int y, int width)
        {
            Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = Muted });
            var box = new TextBox { Location = new Point(x, y + 20), Width = width, Font = new Font("Segoe UI", 10.5F) };
            Controls.Add(box);
            return box;
        }

        private void loadProfile()
        {
            DataTable user = DBHelper.getdataTable(
                "SELECT Name, LastName, Email, Phone_Number, Address, City, Profile_Photo FROM user WHERE ID = " + DBHelper.loggedinuserid);
            if (user.Rows.Count == 0) { Close(); return; }
            DataRow row = user.Rows[0];
            txtName.Text = row["Name"].ToString();
            txtLastName.Text = row["LastName"].ToString();
            txtEmail.Text = row["Email"].ToString();
            txtPhone.Text = row["Phone_Number"].ToString();
            txtAddress.Text = row["Address"].ToString();
            txtCity.Text = row["City"].ToString();

            string initials = ((txtName.Text + " ").Substring(0, 1) + (txtLastName.Text + " ").Substring(0, 1)).ToUpper();
            lblInitials.Text = initials.Trim();
            if (row["Profile_Photo"] != DBNull.Value)
            {
                setPhoto((byte[])row["Profile_Photo"]);
            }
        }

        private void pickPhoto()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;*.bmp";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                var file = new FileInfo(dialog.FileName);
                if (!file.Exists) return;
                if (file.Length > MaxPhotoBytes)
                {
                    MessageBox.Show("De foto is groter dan 3 MB.", "Profielfoto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try { setPhoto(File.ReadAllBytes(dialog.FileName)); photoChanged = true; }
                catch (Exception ex) { MessageBox.Show("Kon de foto niet laden:\n" + ex.Message, "Profielfoto", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void setPhoto(byte[] bytes)
        {
            if (picPhoto.Image != null) { picPhoto.Image.Dispose(); picPhoto.Image = null; }
            photoBytes = bytes;
            if (bytes != null && bytes.Length > 0)
            {
                try { picPhoto.Image = Image.FromStream(new MemoryStream(bytes)); } catch { picPhoto.Image = null; }
                lblInitials.Visible = false;
            }
            else
            {
                lblInitials.Visible = true;
            }
        }

        private void sendCode()
        {
            if (!passwordFieldsValid()) return;
            if (!EmailHelper.IsConfigured())
            {
                MessageBox.Show("De e-mailinstellingen (SMTP) zijn niet ingesteld, dus er kan geen code verstuurd worden.", "Code versturen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            sentCode = new Random().Next(100000, 999999).ToString();
            sentAt = DateTime.Now;
            try
            {
                string body = "Hoi " + txtName.Text.Trim() + ",\r\n\r\n" +
                    "Je verificatiecode om je wachtwoord te wijzigen is: " + sentCode + "\r\n" +
                    "De code is 15 minuten geldig.\r\n\r\nRent A Car";
                EmailHelper.Send(txtEmail.Text.Trim(), (txtName.Text + " " + txtLastName.Text).Trim(),
                    "Verificatiecode - Rent A Car", body, null, null);
            }
            catch (Exception ex)
            {
                sentCode = null;
                MessageBox.Show("Kon de code niet versturen:\n" + ex.Message, "Code versturen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            txtCode.Enabled = true;
            txtCode.Focus();
            lblCodeInfo.Text = "Code verstuurd naar " + txtEmail.Text.Trim() + ". Vul deze hierboven in.";
        }

        private bool passwordFieldsValid()
        {
            if (txtNewPassword.Text.Length < 8)
            {
                MessageBox.Show("Het nieuwe wachtwoord moet minimaal 8 tekens zijn.", "Wachtwoord wijzigen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (txtNewPassword.Text != txtConfirmNewPassword.Text)
            {
                MessageBox.Show("De nieuwe wachtwoorden komen niet overeen.", "Wachtwoord wijzigen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Trim() == "" || txtLastName.Text.Trim() == "" || txtEmail.Text.Trim() == "" ||
                txtPhone.Text.Trim() == "" || txtAddress.Text.Trim() == "" || txtCity.Text.Trim() == "")
            {
                MessageBox.Show("Vul alle velden in.", "Profielinstellingen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!isPhoneNumber(txtPhone.Text))
            {
                MessageBox.Show("Telefoonnummer mag alleen cijfers bevatten (6 tot 15).", "Profielinstellingen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool changePassword = txtNewPassword.Text != "" || txtConfirmNewPassword.Text != "";
            if (changePassword)
            {
                if (!passwordFieldsValid()) return;
                if (sentCode == null)
                {
                    MessageBox.Show("Klik eerst op 'Verstuur code' en vul de code uit je e-mail in.", "Wachtwoord wijzigen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if ((DateTime.Now - sentAt).TotalMinutes > 15)
                {
                    MessageBox.Show("De code is verlopen. Vraag een nieuwe code aan.", "Wachtwoord wijzigen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (txtCode.Text.Trim() != sentCode)
                {
                    MessageBox.Show("De verificatiecode is onjuist.", "Wachtwoord wijzigen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                object count = DBHelper.getScalar(
                    "SELECT COUNT(*) FROM user WHERE Email = " + AuthHelper.sql(txtEmail.Text) + " AND ID <> " + DBHelper.loggedinuserid);
                if (Convert.ToInt32(count) > 0)
                {
                    MessageBox.Show("Er bestaat al een account met dit e-mailadres.", "Profielinstellingen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DBHelper.RunSql(
                    "UPDATE user SET Name = " + AuthHelper.sql(txtName.Text) +
                    ", LastName = " + AuthHelper.sql(txtLastName.Text) +
                    ", Email = " + AuthHelper.sql(txtEmail.Text) +
                    ", Phone_Number = " + AuthHelper.sql(txtPhone.Text) +
                    ", Address = " + AuthHelper.sql(txtAddress.Text) +
                    ", City = " + AuthHelper.sql(txtCity.Text) +
                    " WHERE ID = " + DBHelper.loggedinuserid);

                if (photoChanged)
                {
                    var p = new Dictionary<string, object> { { "@photo", photoBytes }, { "@id", DBHelper.loggedinuserid } };
                    DBHelper.RunSql("UPDATE user SET Profile_Photo = @photo WHERE ID = @id", p);
                }

                if (changePassword)
                {
                    DBHelper.RunSql(
                        "UPDATE user SET Password = " + AuthHelper.sql(AuthHelper.HashPassword(txtNewPassword.Text)) +
                        " WHERE ID = " + DBHelper.loggedinuserid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon je profiel niet opslaan:\n" + ex.Message, "Profielinstellingen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Je profiel is opgeslagen.", "Profielinstellingen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
