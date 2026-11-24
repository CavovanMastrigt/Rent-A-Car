using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Sends e-mail through the SMTP server configured in app_setting.
    // No external package needed - uses System.Net.Mail.
    internal static class EmailHelper
    {
        public static bool IsConfigured()
        {
            return AppSettings.Get("smtp_host") != "" && AppSettings.Get("smtp_from") != "";
        }

        // Sends a message with one attachment. Throws on failure; the caller reports it.
        public static void Send(string toEmail, string toName, string subject, string body, byte[] attachment, string attachmentName)
        {
            string host = AppSettings.Get("smtp_host");
            int port; if (!int.TryParse(AppSettings.Get("smtp_port", "587"), out port)) port = 587;
            string user = AppSettings.Get("smtp_user");
            string pass = AppSettings.Get("smtp_pass");
            string from = AppSettings.Get("smtp_from");
            bool ssl = AppSettings.Get("smtp_ssl", "1") == "1";

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                throw new InvalidOperationException("De e-mailinstellingen (SMTP) zijn nog niet ingesteld.");
            }

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(from, "Rent A Car");
                message.To.Add(new MailAddress(toEmail, toName));
                message.Subject = subject;
                message.Body = body;
                if (attachment != null)
                {
                    message.Attachments.Add(new Attachment(new MemoryStream(attachment), attachmentName, "application/pdf"));
                }

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = ssl;
                    if (!string.IsNullOrEmpty(user))
                    {
                        client.Credentials = new NetworkCredential(user, pass);
                    }
                    client.Send(message);
                }
            }
        }
    }
}
