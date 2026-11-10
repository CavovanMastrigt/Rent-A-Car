using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story 8 - admin reviews one customer's ID in a popup (like login/register).
    // Shows the front and back scan, lets the admin fill in the document number and
    // approve or reject (with a mandatory reason). Returns DialogResult.OK when the
    // customer was approved or rejected, so the list behind it can refresh.
    public class VerificationReviewForm : PopupForm
    {
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);

        private readonly int userId;
        private readonly string custName;
        private readonly string custEmail;

        private Label lblAccount, lblIdData, lblBadge, lblReviewInfo;
        private PictureBox photoFront, photoBack;
        private Label lblFrontNote, lblBackNote;
        private Button btnOpenFront, btnOpenBack, btnApprove, btnReject;
        private TextBox txtDocument, txtReason;

        private string status = VerificationHelper.StatusNone;

        public VerificationReviewForm(int userId, string name, string email)
        {
            this.userId = userId;
            this.custName = name;
            this.custEmail = email;

            ClientSize = new Size(760, 646);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Text = "Klant beoordelen";
            buildUi();
            loadDetail();
        }

        private void buildUi()
        {
            var btnClose = new Button
            {
                Text = "✕",
                Location = new Point(724, 8),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                ForeColor = Muted,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;

            lblAccount = new Label
            {
                Location = new Point(24, 18),
                Size = new Size(470, 44),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            };

            lblBadge = new Label
            {
                Location = new Point(560, 20),
                Size = new Size(170, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            lblIdData = new Label
            {
                Location = new Point(24, 66),
                Size = new Size(710, 24),
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9.5F)
            };

            var lblFront = new Label { Text = "Voorkant", Location = new Point(24, 96), AutoSize = true, ForeColor = Muted };
            var lblBack = new Label { Text = "Achterkant", Location = new Point(388, 96), AutoSize = true, ForeColor = Muted };

            photoFront = makePhoto(new Point(24, 116));
            photoBack = makePhoto(new Point(388, 116));

            lblFrontNote = new Label { Location = new Point(24, 322), Size = new Size(240, 20), ForeColor = Muted, Font = new Font("Segoe UI", 9F) };
            lblBackNote = new Label { Location = new Point(388, 322), Size = new Size(240, 20), ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            btnOpenFront = makePdfButton(new Point(244, 320));
            btnOpenFront.Click += (s, e) => openPdf("Front_Photo");
            btnOpenBack = makePdfButton(new Point(608, 320));
            btnOpenBack.Click += (s, e) => openPdf("Back_Photo");

            var lblDoc = new Label { Text = "Documentnummer", Location = new Point(24, 350), AutoSize = true };
            txtDocument = new TextBox { Location = new Point(24, 370), Width = 340, Font = new Font("Segoe UI", 10F) };

            var lblReason = new Label { Text = "Reden (verplicht bij afkeuren)", Location = new Point(24, 404), AutoSize = true };
            txtReason = new TextBox { Location = new Point(24, 424), Width = 712, Height = 60, Multiline = true };

            btnApprove = new Button
            {
                Text = "Goedkeuren",
                Location = new Point(24, 500),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += (s, e) => approve();

            btnReject = new Button
            {
                Text = "Afkeuren",
                Location = new Point(216, 500),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += (s, e) => reject();

            lblReviewInfo = new Label { Location = new Point(24, 552), Size = new Size(712, 70), ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            Controls.AddRange(new Control[]
            {
                btnClose, lblAccount, lblBadge, lblIdData, lblFront, lblBack,
                photoFront, photoBack, lblFrontNote, lblBackNote, btnOpenFront, btnOpenBack,
                lblDoc, txtDocument, lblReason, txtReason, btnApprove, btnReject, lblReviewInfo
            });
            CancelButton = btnClose;
        }

        private static PictureBox makePhoto(Point loc)
        {
            return new PictureBox
            {
                Location = loc,
                Size = new Size(348, 200),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(249, 250, 251)
            };
        }

        private static Button makePdfButton(Point loc)
        {
            return new Button
            {
                Text = "PDF openen",
                Location = loc,
                Size = new Size(120, 26),
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Cursor = Cursors.Hand
            };
        }

        private void loadDetail()
        {
            lblAccount.Text = custName + "\r\n" + custEmail;
            lblAccount.Font = new Font("Segoe UI", 13F, FontStyle.Bold);

            DataTable t = DBHelper.getdataTable(
                "SELECT Front_Photo, Back_Photo, File_Type, Back_File_Type, File_Name, Back_File_Name, " +
                "Date_Of_Birth, Expiration_Date, Identity_Status, Reviewed_By, Reviewed_At, " +
                "Document_Number, Reject_Reason FROM identity WHERE UserID = " + userId);

            if (t.Rows.Count == 0)
            {
                status = VerificationHelper.StatusNone;
                setBadge();
                lblIdData.Text = "Deze klant heeft nog geen ID-kaart geüpload.";
                lblFrontNote.Text = "Geen scan";
                lblBackNote.Text = "Geen scan";
                setActionsEnabled(false);
                lblReviewInfo.Text = "";
                return;
            }

            DataRow row = t.Rows[0];
            status = row["Identity_Status"].ToString();
            setBadge();

            string dob = row["Date_Of_Birth"] == DBNull.Value ? "-" : Convert.ToDateTime(row["Date_Of_Birth"]).ToString("dd-MM-yyyy");
            string exp = row["Expiration_Date"] == DBNull.Value ? "-" : Convert.ToDateTime(row["Expiration_Date"]).ToString("dd-MM-yyyy");
            string fname = row["File_Name"] == DBNull.Value ? "-" : row["File_Name"].ToString();
            lblIdData.Text = "Geboortedatum (ID): " + dob + "      Vervaldatum ID: " + exp + "      Bestand: " + fname;

            bool reviewed = status == VerificationHelper.StatusVerified || status == VerificationHelper.StatusFailed;

            showScan(photoFront, lblFrontNote, btnOpenFront, row, "Front_Photo", "File_Type", reviewed, true);
            showScan(photoBack, lblBackNote, btnOpenBack, row, "Back_Photo", "Back_File_Type", reviewed, false);

            txtDocument.Text = row["Document_Number"] == DBNull.Value ? "" : row["Document_Number"].ToString();
            txtReason.Text = row["Reject_Reason"] == DBNull.Value ? "" : row["Reject_Reason"].ToString();

            // Only a pending submission can still be approved or rejected.
            setActionsEnabled(status == VerificationHelper.StatusPending);

            if (reviewed)
            {
                string who = row["Reviewed_By"] == DBNull.Value ? "-" : row["Reviewed_By"].ToString();
                string when = row["Reviewed_At"] == DBNull.Value ? "-" : Convert.ToDateTime(row["Reviewed_At"]).ToString("dd-MM-yyyy HH:mm");
                lblReviewInfo.Text = VerificationHelper.StatusLabel(status) + " door " + who + " op " + when +
                    (status == VerificationHelper.StatusFailed ? "\r\nReden: " + txtReason.Text : "");
                lblReviewInfo.ForeColor = VerificationHelper.StatusForeColor(status);
            }
            else
            {
                lblReviewInfo.Text = "In behandeling - controleer de ID-kaart en keur goed of af.";
                lblReviewInfo.ForeColor = Muted;
            }
        }

        private void showScan(PictureBox box, Label note, Button pdf, DataRow row,
                              string photoCol, string typeCol, bool reviewed, bool isFront)
        {
            byte[] bytes = row[photoCol] == DBNull.Value ? null : (byte[])row[photoCol];
            string type = row[typeCol] == DBNull.Value ? "image" : row[typeCol].ToString();
            pdf.Visible = false;

            if (bytes != null && bytes.Length > 0)
            {
                if (type == "pdf")
                {
                    note.Text = "PDF-document geüpload.";
                    pdf.Visible = true;
                }
                else
                {
                    try { box.Image = Image.FromStream(new MemoryStream(bytes)); note.Text = ""; }
                    catch { note.Text = "Kon afbeelding niet tonen."; }
                }
            }
            else
            {
                note.Text = reviewed ? "Na verificatie verwijderd." : (isFront ? "Geen scan" : "Geen achterkant");
            }
        }

        private void setBadge()
        {
            lblBadge.Text = VerificationHelper.StatusLabel(status);
            lblBadge.BackColor = VerificationHelper.StatusBackColor(status);
            lblBadge.ForeColor = VerificationHelper.StatusForeColor(status);
        }

        private void approve()
        {
            if (txtDocument.Text.Trim() == "")
            {
                MessageBox.Show("Vul eerst het documentnummer in.", "Goedkeuren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDocument.Focus();
                return;
            }
            try
            {
                var p = new Dictionary<string, object>
                {
                    { "@uid", userId },
                    { "@doc", VerificationHelper.MaskDocument(txtDocument.Text) },
                    { "@rev", reviewerName() },
                };
                // Verified, reviewer + time recorded, document number stored masked, scans wiped.
                DBHelper.RunSql(
                    "UPDATE identity SET Identity_Status = 'Verified', Document_Number = @doc, " +
                    "Reviewed_By = @rev, Reviewed_At = NOW(), Reject_Reason = NULL, " +
                    "Front_Photo = NULL, Back_Photo = NULL WHERE UserID = @uid", p);
                DBHelper.RunSql("UPDATE user SET User_Status = 'active' WHERE ID = " + userId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon niet goedkeuren:\n" + ex.Message, "Goedkeuren", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("De klant is geverifieerd.", "Goedkeuren", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void reject()
        {
            if (txtReason.Text.Trim() == "")
            {
                MessageBox.Show("Een reden is verplicht bij afkeuren.", "Afkeuren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReason.Focus();
                return;
            }
            try
            {
                var p = new Dictionary<string, object>
                {
                    { "@uid", userId },
                    { "@reason", txtReason.Text.Trim() },
                    { "@rev", reviewerName() },
                };
                DBHelper.RunSql(
                    "UPDATE identity SET Identity_Status = 'Failed', Reject_Reason = @reason, " +
                    "Reviewed_By = @rev, Reviewed_At = NOW(), " +
                    "Front_Photo = NULL, Back_Photo = NULL WHERE UserID = @uid", p);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon niet afkeuren:\n" + ex.Message, "Afkeuren", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("De klant is afgekeurd.", "Afkeuren", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private string reviewerName()
        {
            try
            {
                DataTable t = DBHelper.getdataTable("SELECT Name, LastName FROM user WHERE ID = " + DBHelper.loggedinuserid);
                if (t.Rows.Count > 0)
                {
                    return (t.Rows[0]["Name"] + " " + t.Rows[0]["LastName"]).Trim();
                }
            }
            catch { }
            return "Admin #" + DBHelper.loggedinuserid;
        }

        private void openPdf(string column)
        {
            try
            {
                DataTable t = DBHelper.getdataTable("SELECT " + column + " AS Doc FROM identity WHERE UserID = " + userId);
                if (t.Rows.Count == 0 || t.Rows[0]["Doc"] == DBNull.Value)
                {
                    return;
                }
                byte[] bytes = (byte[])t.Rows[0]["Doc"];
                string path = Path.Combine(Path.GetTempPath(), "id_" + userId + "_" + Guid.NewGuid().ToString("N") + ".pdf");
                File.WriteAllBytes(path, bytes);
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de PDF niet openen:\n" + ex.Message, "Klantverificatie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void setActionsEnabled(bool enabled)
        {
            btnApprove.Enabled = enabled;
            btnReject.Enabled = enabled;
            txtDocument.Enabled = enabled;
            txtReason.Enabled = enabled;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (photoFront != null && photoFront.Image != null) photoFront.Image.Dispose();
                if (photoBack != null && photoBack.Image != null) photoBack.Image.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
