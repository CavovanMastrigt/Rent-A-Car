using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story 8 - the customer uploads an ID card (image or PDF) for verification.
    // Opens as a popup, like the login and register screens.
    public class IdentityVerificationForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private Label lblFile;
        private PictureBox preview;
        private DateTimePicker dtpBirth;
        private DateTimePicker dtpExpiry;
        private Button btnSubmit;

        private string filePath;
        private string fileType;

        public IdentityVerificationForm()
        {
            ClientSize = new Size(440, 470);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Rent A Car - ID-verificatie";
            buildUi();
            checkExisting();
        }

        private void buildUi()
        {
            var title = new Label
            {
                Text = "ID-verificatie",
                Dock = DockStyle.Top,
                Height = 56,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var info = new Label
            {
                Text = "Upload een foto of scan van je ID-kaart (JPG, PNG of PDF, max 5 MB).\r\nEen medewerker controleert deze daarna.",
                Location = new Point(30, 62),
                Size = new Size(380, 44),
                ForeColor = Muted
            };

            var btnClose = new Button
            {
                Text = "✕",
                Location = new Point(404, 6),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                ForeColor = Muted,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;

            var btnPick = new Button
            {
                Text = "Kies bestand...",
                Location = new Point(30, 112),
                Size = new Size(140, 30),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPick.Click += btnPick_Click;

            lblFile = new Label
            {
                Text = "Geen bestand gekozen",
                Location = new Point(180, 118),
                Size = new Size(230, 20),
                ForeColor = Muted,
                AutoEllipsis = true
            };

            preview = new PictureBox
            {
                Location = new Point(30, 152),
                Size = new Size(380, 170),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(249, 250, 251)
            };

            var lblBirth = new Label { Text = "Geboortedatum (zoals op ID)", Location = new Point(30, 332), AutoSize = true };
            dtpBirth = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(30, 352),
                Width = 180,
                MaxDate = DateTime.Today
            };
            dtpBirth.Value = new DateTime(2000, 1, 1);

            var lblExpiry = new Label { Text = "Vervaldatum ID", Location = new Point(230, 332), AutoSize = true };
            dtpExpiry = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(230, 352),
                Width = 180
            };
            dtpExpiry.Value = DateTime.Today.AddYears(5);

            btnSubmit = new Button
            {
                Text = "Versturen",
                Location = new Point(230, 410),
                Size = new Size(180, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += btnSubmit_Click;

            var btnCancel = new Button
            {
                Text = "Annuleren",
                Location = new Point(30, 410),
                Size = new Size(140, 34),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };

            Controls.AddRange(new Control[]
            {
                title, info, btnClose, btnPick, lblFile, preview,
                lblBirth, dtpBirth, lblExpiry, dtpExpiry, btnSubmit, btnCancel
            });
            CancelButton = btnCancel;
        }

        // If the customer is already verified, there is nothing to do here.
        private void checkExisting()
        {
            string status = VerificationHelper.GetStatus(DBHelper.loggedinuserid);
            if (status == VerificationHelper.StatusVerified)
            {
                lblFile.Text = "Je account is al geverifieerd.";
                btnSubmit.Enabled = false;
            }
            else if (status == VerificationHelper.StatusFailed)
            {
                lblFile.Text = "Vorige poging afgekeurd - upload een nieuwe ID-kaart.";
            }
        }

        private void btnPick_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Afbeeldingen en PDF|*.jpg;*.jpeg;*.png;*.bmp;*.pdf";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var file = new FileInfo(dialog.FileName);
                if (!file.Exists)
                {
                    return;
                }
                if (file.Length > VerificationHelper.MaxFileBytes)
                {
                    MessageBox.Show("Het bestand is groter dan 5 MB.", "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string ext = file.Extension.ToLowerInvariant();
                filePath = dialog.FileName;
                fileType = ext == ".pdf" ? "pdf" : "image";
                lblFile.Text = file.Name;

                if (preview.Image != null)
                {
                    preview.Image.Dispose();
                    preview.Image = null;
                }
                if (fileType == "image")
                {
                    try
                    {
                        // Load via bytes so the file is not locked.
                        preview.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(filePath)));
                    }
                    catch
                    {
                        preview.Image = null;
                    }
                }
                else
                {
                    preview.Image = null;
                    lblFile.Text = file.Name + "  (PDF)";
                }
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Kies eerst een bestand.", "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dtpBirth.Value.Date >= DateTime.Today)
            {
                MessageBox.Show("Vul een geldige geboortedatum in.", "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                if (bytes.Length > VerificationHelper.MaxFileBytes)
                {
                    MessageBox.Show("Het bestand is groter dan 5 MB.", "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "@uid", DBHelper.loggedinuserid },
                    { "@photo", bytes },
                    { "@ftype", fileType },
                    { "@fname", Path.GetFileName(filePath) },
                    { "@dob", dtpBirth.Value.Date },
                    { "@exp", dtpExpiry.Value.Date },
                };

                // One identity row per user (UserID is unique); a re-upload replaces it
                // and resets the review.
                DBHelper.RunSql(
                    "INSERT INTO identity " +
                    "(UserID, Front_Photo, File_Type, File_Name, Date_Of_Birth, Expiration_Date, Identity_Status) " +
                    "VALUES (@uid, @photo, @ftype, @fname, @dob, @exp, 'Pending') " +
                    "ON DUPLICATE KEY UPDATE " +
                    "Front_Photo = @photo, Back_Photo = NULL, File_Type = @ftype, File_Name = @fname, " +
                    "Date_Of_Birth = @dob, Expiration_Date = @exp, Identity_Status = 'Pending', " +
                    "Reviewed_By = NULL, Reviewed_At = NULL, Document_Number = NULL, Reject_Reason = NULL",
                    parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de ID-kaart niet opslaan:\n" + ex.Message, "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(
                "Je ID-kaart is verstuurd. Een medewerker controleert deze zo snel mogelijk.",
                "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && preview != null && preview.Image != null)
            {
                preview.Image.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
