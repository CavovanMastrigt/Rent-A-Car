using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // User story 8 - admin-only page to review customer ID verifications.
    // The admin compares the ID with the customer's name/birth date, enters the
    // document number and approves or rejects (with a mandatory reason).
    public class CustomerVerificationPage : UserControl
    {
        private static readonly Color Border = Color.FromArgb(229, 231, 235);
        private static readonly Color Muted = Color.FromArgb(107, 114, 128);
        private static readonly Color Green = Color.FromArgb(22, 101, 52);
        private static readonly Color Red = Color.FromArgb(185, 28, 28);

        private DataGridView grid;
        private PictureBox photo;
        private Label lblPhotoNote;
        private Label lblAccount, lblIdData, lblReviewInfo;
        private TextBox txtDocument, txtReason;
        private Button btnOpenPdf, btnApprove, btnReject;
        private Panel detailPanel;

        private int selectedUserId = -1;

        public CustomerVerificationPage()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            Dock = DockStyle.Fill;
            buildUi();
            Load += (s, e) => loadCustomers();
        }

        private void buildUi()
        {
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Controls.Add(root);

            var header = new Label
            {
                Text = "Klantverificatie",
                Dock = DockStyle.Top,
                Height = 44,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 30 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserID", DataPropertyName = "UserID", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Naam", HeaderText = "Naam", DataPropertyName = "Naam", FillWeight = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "E-mail", DataPropertyName = "Email", FillWeight = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 90 });
            grid.SelectionChanged += (s, e) => loadSelectedDetail();

            detailPanel = buildDetailPanel();

            split.Controls.Add(grid, 0, 0);
            split.Controls.Add(detailPanel, 1, 0);

            root.Controls.Add(split);
            root.Controls.Add(header);
            header.BringToFront();
        }

        private Panel buildDetailPanel()
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18), AutoScroll = true };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            lblAccount = new Label { Location = new Point(18, 16), Size = new Size(520, 46), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            lblIdData = new Label { Location = new Point(18, 66), Size = new Size(520, 46), ForeColor = Muted, Font = new Font("Segoe UI", 9.5F) };

            photo = new PictureBox
            {
                Location = new Point(18, 118),
                Size = new Size(520, 230),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(249, 250, 251)
            };
            lblPhotoNote = new Label { Location = new Point(18, 352), Size = new Size(380, 20), ForeColor = Muted, Font = new Font("Segoe UI", 9F) };
            btnOpenPdf = new Button
            {
                Text = "PDF openen",
                Location = new Point(410, 350),
                Size = new Size(128, 26),
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Cursor = Cursors.Hand
            };
            btnOpenPdf.Click += (s, e) => openPdf();

            var lblDoc = new Label { Text = "Documentnummer", Location = new Point(18, 384), AutoSize = true };
            txtDocument = new TextBox { Location = new Point(18, 404), Width = 260 };

            var lblReason = new Label { Text = "Reden (verplicht bij afkeuren)", Location = new Point(18, 436), AutoSize = true };
            txtReason = new TextBox { Location = new Point(18, 456), Width = 520, Height = 48, Multiline = true };

            btnApprove = new Button
            {
                Text = "Goedkeuren",
                Location = new Point(18, 516),
                Size = new Size(160, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += (s, e) => approve();

            btnReject = new Button
            {
                Text = "Afkeuren",
                Location = new Point(188, 516),
                Size = new Size(160, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += (s, e) => reject();

            lblReviewInfo = new Label { Location = new Point(18, 560), Size = new Size(520, 60), ForeColor = Muted, Font = new Font("Segoe UI", 9F) };

            card.Controls.AddRange(new Control[]
            {
                lblAccount, lblIdData, photo, lblPhotoNote, btnOpenPdf,
                lblDoc, txtDocument, lblReason, txtReason, btnApprove, btnReject, lblReviewInfo
            });
            showDetail(false);
            return card;
        }

        private void loadCustomers()
        {
            try
            {
                DataTable customers = DBHelper.getdataTable(
                    "SELECT u.ID AS UserID, CONCAT(u.Name, ' ', u.LastName) AS Naam, u.Email, " +
                    "COALESCE(i.Identity_Status, 'None') AS St " +
                    "FROM user u LEFT JOIN identity i ON i.UserID = u.ID " +
                    "WHERE u.RID = " + AuthHelper.RoleCustomer + " " +
                    "ORDER BY (COALESCE(i.Identity_Status,'None') = 'Pending') DESC, u.Name");

                var display = new DataTable();
                display.Columns.Add("UserID", typeof(int));
                display.Columns.Add("Naam");
                display.Columns.Add("Email");
                display.Columns.Add("Status");
                foreach (DataRow r in customers.Rows)
                {
                    display.Rows.Add(
                        Convert.ToInt32(r["UserID"]),
                        r["Naam"].ToString(),
                        r["Email"].ToString(),
                        VerificationHelper.StatusLabel(r["St"].ToString()));
                }
                grid.DataSource = display;
                loadSelectedDetail();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de klanten niet laden:\n" + ex.Message, "Klantverificatie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadSelectedDetail()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["UserID"].Value == null)
            {
                showDetail(false);
                return;
            }
            selectedUserId = Convert.ToInt32(grid.CurrentRow.Cells["UserID"].Value);

            DataTable t = DBHelper.getdataTable(
                "SELECT Front_Photo, File_Type, File_Name, Date_Of_Birth, Expiration_Date, " +
                "Identity_Status, Reviewed_By, Reviewed_At, Document_Number, Reject_Reason " +
                "FROM identity WHERE UserID = " + selectedUserId);

            string name = grid.CurrentRow.Cells["Naam"].Value.ToString();
            string email = grid.CurrentRow.Cells["Email"].Value.ToString();

            clearPhoto();

            if (t.Rows.Count == 0)
            {
                showDetail(true);
                lblAccount.Text = name + "\r\n" + email;
                lblIdData.Text = "Deze klant heeft nog geen ID-kaart geüpload.";
                lblPhotoNote.Text = "";
                btnOpenPdf.Visible = false;
                txtDocument.Text = "";
                txtReason.Text = "";
                setActionsEnabled(false);
                lblReviewInfo.Text = "";
                return;
            }

            DataRow row = t.Rows[0];
            string status = row["Identity_Status"].ToString();
            string dob = row["Date_Of_Birth"] == DBNull.Value ? "-" : Convert.ToDateTime(row["Date_Of_Birth"]).ToString("dd-MM-yyyy");
            string exp = row["Expiration_Date"] == DBNull.Value ? "-" : Convert.ToDateTime(row["Expiration_Date"]).ToString("dd-MM-yyyy");

            showDetail(true);
            lblAccount.Text = name + "\r\n" + email;
            lblIdData.Text = "Geboortedatum (ID): " + dob + "     Vervaldatum ID: " + exp +
                "\r\nBestand: " + (row["File_Name"] == DBNull.Value ? "-" : row["File_Name"].ToString());

            bool reviewed = status == VerificationHelper.StatusVerified || status == VerificationHelper.StatusFailed;

            // Show the scan only while it is still pending; it is wiped after review.
            byte[] bytes = row["Front_Photo"] == DBNull.Value ? null : (byte[])row["Front_Photo"];
            string fileType = row["File_Type"] == DBNull.Value ? "image" : row["File_Type"].ToString();
            if (bytes != null && bytes.Length > 0)
            {
                if (fileType == "pdf")
                {
                    lblPhotoNote.Text = "PDF-document geüpload.";
                    btnOpenPdf.Visible = true;
                }
                else
                {
                    try { photo.Image = Image.FromStream(new MemoryStream(bytes)); }
                    catch { lblPhotoNote.Text = "Afbeelding kon niet getoond worden."; }
                    btnOpenPdf.Visible = false;
                }
            }
            else
            {
                lblPhotoNote.Text = reviewed ? "ID-scan is na verificatie verwijderd." : "Geen scan aanwezig.";
                btnOpenPdf.Visible = false;
            }

            txtDocument.Text = row["Document_Number"] == DBNull.Value ? "" : row["Document_Number"].ToString();
            txtReason.Text = row["Reject_Reason"] == DBNull.Value ? "" : row["Reject_Reason"].ToString();

            // Only pending submissions can be actioned.
            setActionsEnabled(status == VerificationHelper.StatusPending);

            if (reviewed)
            {
                string who = row["Reviewed_By"] == DBNull.Value ? "-" : row["Reviewed_By"].ToString();
                string when = row["Reviewed_At"] == DBNull.Value ? "-" : Convert.ToDateTime(row["Reviewed_At"]).ToString("dd-MM-yyyy HH:mm");
                lblReviewInfo.Text = VerificationHelper.StatusLabel(status) + " door " + who + " op " + when +
                    (status == VerificationHelper.StatusFailed ? "\r\nReden: " + txtReason.Text : "");
                lblReviewInfo.ForeColor = status == VerificationHelper.StatusVerified ? Green : Red;
            }
            else
            {
                lblReviewInfo.Text = "In behandeling - controleer de ID-kaart en keur goed of af.";
                lblReviewInfo.ForeColor = Muted;
            }
        }

        private void approve()
        {
            if (selectedUserId < 0) return;
            if (txtDocument.Text.Trim() == "")
            {
                MessageBox.Show("Vul eerst het documentnummer in.", "Goedkeuren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var p = new Dictionary<string, object>
                {
                    { "@uid", selectedUserId },
                    { "@doc", VerificationHelper.MaskDocument(txtDocument.Text) },
                    { "@rev", reviewerName() },
                };
                // Verified, reviewer + time recorded, document number stored masked, scan wiped.
                DBHelper.RunSql(
                    "UPDATE identity SET Identity_Status = 'Verified', Document_Number = @doc, " +
                    "Reviewed_By = @rev, Reviewed_At = NOW(), Reject_Reason = NULL, " +
                    "Front_Photo = NULL, Back_Photo = NULL WHERE UserID = @uid", p);
                DBHelper.RunSql("UPDATE user SET User_Status = 'active' WHERE ID = " + selectedUserId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon niet goedkeuren:\n" + ex.Message, "Goedkeuren", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("De klant is geverifieerd.", "Goedkeuren", MessageBoxButtons.OK, MessageBoxIcon.Information);
            loadCustomers();
        }

        private void reject()
        {
            if (selectedUserId < 0) return;
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
                    { "@uid", selectedUserId },
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
            loadCustomers();
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

        private void openPdf()
        {
            try
            {
                DataTable t = DBHelper.getdataTable("SELECT Front_Photo FROM identity WHERE UserID = " + selectedUserId);
                if (t.Rows.Count == 0 || t.Rows[0]["Front_Photo"] == DBNull.Value)
                {
                    return;
                }
                byte[] bytes = (byte[])t.Rows[0]["Front_Photo"];
                string path = Path.Combine(Path.GetTempPath(), "id_" + selectedUserId + "_" + Guid.NewGuid().ToString("N") + ".pdf");
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

        private void showDetail(bool visible)
        {
            foreach (Control c in detailPanel.Controls)
            {
                c.Visible = visible;
            }
        }

        private void clearPhoto()
        {
            if (photo.Image != null)
            {
                photo.Image.Dispose();
                photo.Image = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                clearPhoto();
            }
            base.Dispose(disposing);
        }
    }
}
