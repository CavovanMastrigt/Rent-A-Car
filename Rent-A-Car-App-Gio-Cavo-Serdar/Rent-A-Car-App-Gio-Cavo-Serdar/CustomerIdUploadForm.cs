using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Admin / employee uploads an ID (front + back) on behalf of a customer.
    // Stores it as a new 'Pending' identity so it shows up in Klantverificatie.
    public class CustomerIdUploadForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private class Slot
        {
            public string Path;
            public string Type = "image";
            public Label FileLabel;
            public PictureBox Preview;
        }

        private readonly int userId;
        private readonly string customerName;
        private Slot front, back;
        private DateTimePicker dtpBirth, dtpExpiry;

        public CustomerIdUploadForm(int userId, string customerName)
        {
            this.userId = userId;
            this.customerName = customerName;
            ClientSize = new Size(720, 470);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "ID uploaden";
            buildUi();
        }

        private void buildUi()
        {
            var title = new Label
            {
                Text = "ID uploaden voor " + customerName,
                Dock = DockStyle.Top,
                Height = 50,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var btnClose = new Button
            {
                Text = "✕",
                Location = new Point(684, 6),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                ForeColor = Muted,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;

            front = addSlot("Voorkant van de ID", 30);
            back = addSlot("Achterkant van de ID", 370);

            Controls.Add(new Label { Text = "Geboortedatum", Location = new Point(30, 320), AutoSize = true, ForeColor = Muted });
            dtpBirth = new DateTimePicker { Location = new Point(30, 340), Width = 150, Format = DateTimePickerFormat.Short, MaxDate = DateTime.Today };
            dtpBirth.Value = new DateTime(2000, 1, 1);

            Controls.Add(new Label { Text = "Vervaldatum", Location = new Point(200, 320), AutoSize = true, ForeColor = Muted });
            dtpExpiry = new DateTimePicker { Location = new Point(200, 340), Width = 150, Format = DateTimePickerFormat.Short };
            dtpExpiry.Value = DateTime.Today.AddYears(5);

            var btnSubmit = new Button
            {
                Text = "Uploaden",
                Location = new Point(540, 410),
                Size = new Size(150, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, e) => submit();

            var btnCancel = new Button
            {
                Text = "Annuleren",
                Location = new Point(30, 410),
                Size = new Size(150, 40),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };

            Controls.AddRange(new Control[] { title, btnClose, dtpBirth, dtpExpiry, btnSubmit, btnCancel });
            title.BringToFront();
            CancelButton = btnCancel;
        }

        private Slot addSlot(string caption, int x)
        {
            var slot = new Slot();
            Controls.Add(new Label { Text = caption, Location = new Point(x, 60), AutoSize = true, ForeColor = Muted });

            var btnPick = new Button
            {
                Text = "Kies bestand...",
                Location = new Point(x, 80),
                Size = new Size(140, 28),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPick.Click += (s, e) => pick(slot);

            slot.FileLabel = new Label { Text = "Geen bestand gekozen", Location = new Point(x + 150, 86), Size = new Size(150, 20), ForeColor = Muted, AutoEllipsis = true };
            slot.Preview = new PictureBox
            {
                Location = new Point(x, 116),
                Size = new Size(300, 180),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(249, 250, 251)
            };

            Controls.AddRange(new Control[] { btnPick, slot.FileLabel, slot.Preview });
            return slot;
        }

        private void pick(Slot slot)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Afbeeldingen en PDF|*.jpg;*.jpeg;*.png;*.bmp;*.pdf";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                var file = new FileInfo(dialog.FileName);
                if (!file.Exists) return;
                if (file.Length > VerificationHelper.MaxFileBytes)
                {
                    MessageBox.Show("Het bestand is groter dan 5 MB.", "ID uploaden", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string ext = file.Extension.ToLowerInvariant();
                slot.Path = dialog.FileName;
                slot.Type = ext == ".pdf" ? "pdf" : "image";

                if (slot.Preview.Image != null) { slot.Preview.Image.Dispose(); slot.Preview.Image = null; }
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

        private void submit()
        {
            if (string.IsNullOrEmpty(front.Path) || string.IsNullOrEmpty(back.Path))
            {
                MessageBox.Show("Kies zowel een voorkant als een achterkant.", "ID uploaden", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dtpBirth.Value.Date >= DateTime.Today)
            {
                MessageBox.Show("Vul een geldige geboortedatum in.", "ID uploaden", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] frontBytes = File.ReadAllBytes(front.Path);
                byte[] backBytes = File.ReadAllBytes(back.Path);
                if (frontBytes.Length > VerificationHelper.MaxFileBytes || backBytes.Length > VerificationHelper.MaxFileBytes)
                {
                    MessageBox.Show("Een bestand is groter dan 5 MB.", "ID uploaden", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var p = new Dictionary<string, object>
                {
                    { "@uid", userId },
                    { "@front", frontBytes }, { "@ftype", front.Type }, { "@fname", Path.GetFileName(front.Path) },
                    { "@back", backBytes }, { "@btype", back.Type }, { "@bname", Path.GetFileName(back.Path) },
                    { "@dob", dtpBirth.Value.Date }, { "@exp", dtpExpiry.Value.Date },
                };

                DBHelper.RunSql(
                    "INSERT INTO identity " +
                    "(UserID, Front_Photo, File_Type, File_Name, Back_Photo, Back_File_Type, Back_File_Name, Date_Of_Birth, Expiration_Date, Identity_Status) " +
                    "VALUES (@uid, @front, @ftype, @fname, @back, @btype, @bname, @dob, @exp, 'Pending') " +
                    "ON DUPLICATE KEY UPDATE " +
                    "Front_Photo=@front, File_Type=@ftype, File_Name=@fname, " +
                    "Back_Photo=@back, Back_File_Type=@btype, Back_File_Name=@bname, " +
                    "Date_Of_Birth=@dob, Expiration_Date=@exp, Identity_Status='Pending', " +
                    "Reviewed_By=NULL, Reviewed_At=NULL, Document_Number=NULL, Reject_Reason=NULL, " +
                    "ID_First_Name=NULL, ID_Last_Name=NULL",
                    p);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de ID niet opslaan:\n" + ex.Message, "ID uploaden", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("De ID is geüpload en staat klaar voor verificatie.", "ID uploaden", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (front != null && front.Preview != null && front.Preview.Image != null) front.Preview.Image.Dispose();
                if (back != null && back.Preview != null && back.Preview.Image != null) back.Preview.Image.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
