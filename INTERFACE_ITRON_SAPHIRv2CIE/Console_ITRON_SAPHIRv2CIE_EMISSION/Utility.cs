using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Console_ITRON_SAPHIRv2CIE_EMISSION
{
    public class Utility
    {
        

        public void SendEMail(string emailid, string subject, string body, ref string msgErr)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.EnableSsl = true;
                client.EnableSsl = false;
                client.Host = System.Configuration.ConfigurationManager.AppSettings["serverSMTP"];
                // client.Port = 587;

                //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("xxxxx", "yyyy");
                //client.UseDefaultCredentials = false;
                //client.Credentials = credentials;

                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.From = new MailAddress("adminDDI@cie.ci");
                msg.To.Add(new MailAddress(emailid));

                msg.Subject = subject;
                msg.IsBodyHtml = true;
                msg.Body = body;

                client.Send(msg);
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
            }
        }
    }
}
