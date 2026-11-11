using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story 8 - the customer uploads the FRONT and BACK of an ID card
    // (image or PDF) for verification. Opens as a popup, like login/register.
    public class IdentityVerificationForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        // One upload slot (front or back): a pick button, a file-name label and a preview.
        private class Slot
        {
            public Label FileLabel;
            public PictureBox Preview;
            public string Path;
            public string Type;
        }

        private Slot front;
        private Slot back;
        private DateTimePicker dtpBirth;
        private DateTimePicker dtpExpiry;
        private Button btnSubmit;

        public IdentityVerificationForm()
        {
            ClientSize = new Size(700, 540);
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
                Text = "Upload een foto of scan van de VOORKANT en de ACHTERKANT van je ID-kaart (JPG, PNG of PDF, max 5 MB per bestand). Een medewerker controleert deze daarna.",
                Location = new Point(30, 62),
                Size = new Size(640, 44),
                ForeColor = Muted
            };

            var btnClose = new Button
            {
                Text = "✕",
                Location = new Point(664, 6),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                ForeColor = Muted,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;

            front = addSlot("Voorkant van je ID", 30);
            back = addSlot("Achterkant van je ID", 360);

            var lblBirth = new Label { Text = "Geboortedatum (zoals op ID)", Location = new Point(30, 366), AutoSize = true };
            dtpBirth = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(30, 386),
                Width = 180,
                MaxDate = DateTime.Today
            };
            dtpBirth.Value = new DateTime(2000, 1, 1);

            var lblExpiry = new Label { Text = "Vervaldatum ID", Location = new Point(360, 366), AutoSize = true };
            dtpExpiry = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(360, 386),
                Width = 180
            };
            dtpExpiry.Value = DateTime.Today.AddYears(5);

            btnSubmit = new Button
            {
                Text = "Versturen",
                Location = new Point(520, 470),
                Size = new Size(150, 36),
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
                Location = new Point(30, 470),
                Size = new Size(140, 36),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };

            Controls.AddRange(new Control[]
            {
                title, info, btnClose, lblBirth, dtpBirth, lblExpiry, dtpExpiry, btnSubmit, btnCancel
            });
            CancelButton = btnCancel;
        }

        // Builds a labelled pick button + file-name label + preview at column x.
        private Slot addSlot(string caption, int x)
        {
            var slot = new Slot();

            var lbl = new Label
            {
                Text = caption,
                Location = new Point(x, 112),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var btnPick = new Button
            {
                Text = "Kies bestand...",
                Location = new Point(x, 136),
                Size = new Size(140, 30),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            slot.FileLabel = new Label
            {
                Text = "Geen bestand gekozen",
                Location = new Point(x + 148, 142),
                Size = new Size(160, 20),
                ForeColor = Muted,
                AutoEllipsis = true
            };

            slot.Preview = new PictureBox
            {
                Location = new Point(x, 172),
                Size = new Size(310, 184),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(249, 250, 251)
            };

            btnPick.Click += (s, e) => pick(slot);

            Controls.AddRange(new Control[] { lbl, btnPick, slot.FileLabel, slot.Preview });
            return slot;
        }

        // If the customer is already verified, there is nothing to do here.
        private void checkExisting()
        {
            string status = VerificationHelper.GetStatus(DBHelper.loggedinuserid);
            if (status == VerificationHelper.StatusVerified)
            {
                front.FileLabel.Text = "Je account is al geverifieerd.";
                btnSubmit.Enabled = false;
            }
            else if (status == VerificationHelper.StatusFailed)
            {
                front.FileLabel.Text = "Vorige poging afgekeurd - upload opnieuw.";
            }
        }

        private void pick(Slot slot)
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
                slot.Path = dialog.FileName;
                slot.Type = ext == ".pdf" ? "pdf" : "image";

                if (slot.Preview.Image != null)
                {
                    slot.Preview.Image.Dispose();
                    slot.Preview.Image = null;
                }

                if (slot.Type == "image")
                {
                    slot.FileLabel.Text = file.Name;
                    try { slot.Preview.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(slot.Path))); }
                    catch { slot.Preview.Image = null; }
                }
                else
                {
                    slot.FileLabel.Text = file.Name + "  (PDF)";
                    slot.Preview.Image = null;
                }
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(front.Path) || !File.Exists(front.Path))
            {
                MessageBox.Show("Kies een bestand voor de voorkant van je ID.", "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(back.Path) || !File.Exists(back.Path))
            {
                MessageBox.Show("Kies een bestand voor de achterkant van je ID.", "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dtpBirth.Value.Date >= DateTime.Today)
            {
                MessageBox.Show("Vul een geldige geboortedatum in.", "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] frontBytes = File.ReadAllBytes(front.Path);
                byte[] backBytes = File.ReadAllBytes(back.Path);
                if (frontBytes.Length > VerificationHelper.MaxFileBytes || backBytes.Length > VerificationHelper.MaxFileBytes)
                {
                    MessageBox.Show("Een van de bestanden is groter dan 5 MB.", "ID-verificatie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "@uid", DBHelper.loggedinuserid },
                    { "@front", frontBytes },
                    { "@ftype", front.Type },
                    { "@fname", Path.GetFileName(front.Path) },
                    { "@back", backBytes },
                    { "@btype", back.Type },
                    { "@bname", Path.GetFileName(back.Path) },
                    { "@dob", dtpBirth.Value.Date },
                    { "@exp", dtpExpiry.Value.Date },
                };

                // One identity row per user (UserID is unique); a re-upload replaces it
                // and resets the review.
                DBHelper.RunSql(
                    "INSERT INTO identity " +
                    "(UserID, Front_Photo, File_Type, File_Name, Back_Photo, Back_File_Type, Back_File_Name, " +
                    "Date_Of_Birth, Expiration_Date, Identity_Status) " +
                    "VALUES (@uid, @front, @ftype, @fname, @back, @btype, @bname, @dob, @exp, 'Pending') " +
                    "ON DUPLICATE KEY UPDATE " +
                    "Front_Photo = @front, File_Type = @ftype, File_Name = @fname, " +
                    "Back_Photo = @back, Back_File_Type = @btype, Back_File_Name = @bname, " +
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
            if (disposing)
            {
                if (front != null && front.Preview != null && front.Preview.Image != null)
                {
                    front.Preview.Image.Dispose();
                }
                if (back != null && back.Preview != null && back.Preview.Image != null)
                {
                    back.Preview.Image.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
