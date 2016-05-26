using System;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace VenusFiles
{
    public class Utility
    {
        private static string smtpServerIP = System.Configuration.ConfigurationManager.AppSettings["smtpServerIP"].ToString();
        private static string smtpServerUsername = System.Configuration.ConfigurationManager.AppSettings["smtpServerUsername"].ToString();
        private static string smtpServerPassword = System.Configuration.ConfigurationManager.AppSettings["smtpServerPassword"].ToString();
        private static string smtpFromAddress = System.Configuration.ConfigurationManager.AppSettings["smtpFromAddress"].ToString();
        public static void SendEmail(string messageTo, string subject, string emailMessage, string[] attachments)
        {
            try
            {
                //messageTo = "khuli.mpanza@ilembe.gov.za";

                var message = new MailMessage { From = new MailAddress(smtpFromAddress, "VenusFiles") };

                message.To.Add(messageTo);

                message.Subject = subject;
                message.Body = emailMessage;

                foreach (var attach in attachments.Select(attachment => new Attachment(attachment)))
                {
                    message.Attachments.Add(attach);
                }

                var smtp = new SmtpClient(smtpServerIP, 25)
                {
                    Credentials = new System.Net.NetworkCredential(@""+ smtpServerUsername, smtpServerPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                smtp.Send(message);
            }
            catch(SmtpException ex)
            {
                
            }
        }

        public static void SendEmailInd(string messageTo, string subject, string emailMessage, string[] attachments)
        {
            try
            {
                var message = new MailMessage { From = new MailAddress(smtpFromAddress, "VenusFiles") };

                string[] strArray = messageTo.Split(',');

                message.Subject = "PLEASE DON'T USE THIS FILE IT IS A TEST"; //subject;
                message.Body = "PLEASE EMAIL Stephen.Jacobs@deposita.co.za IF YOU receive this email, THANK YOU"; //emailMessage;

                foreach (var attach in attachments.Select(attachment => new Attachment(attachment)))
                {
                    message.Attachments.Add(attach);
                }

                var smtp = new SmtpClient(smtpServerIP, 25)
                {
                    Credentials = new System.Net.NetworkCredential(@"" + smtpServerUsername, smtpServerPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };


                foreach (var mt in strArray)
                {
                    message.To.Add(mt);
                    
                    smtp.Send(message);
                }
            }
            catch (SmtpException ex)
            {

            }
        }
    }
}
