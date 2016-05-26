using System;
using System.Linq;
using System.Net.Mail;

namespace VenusFiles
{
    public interface IEmailManager
    {
        void SendEmail(string emailMessage, string emailSubject, string[] attachments);
    }

    class EmailManager : IEmailManager
    {
        private readonly Settings _settings;
        private static string smtpServerIP = System.Configuration.ConfigurationManager.AppSettings["smtpServerIP"].ToString();
        private static string smtpServerUsername = System.Configuration.ConfigurationManager.AppSettings["smtpServerUsername"].ToString();
        private static string smtpServerPassword = System.Configuration.ConfigurationManager.AppSettings["smtpServerPassword"].ToString();
        private static string smtpFromAddress = System.Configuration.ConfigurationManager.AppSettings["smtpFromAddress"].ToString();

        public EmailManager(string applicationStartupPath)
        {
            _settings = new Settings { SettingsIni = new Configurator() };
            _settings.SettingsIni.LoadFromFile(applicationStartupPath + @"\Settings.ini", Configurator.FileType.Ini);
        }
        
        public void SendEmail(string emailMessage, string emailSubject, string[] attachments)
        {            
            _settings.Ipaddress = _settings.SettingsIni.GetValue("SMTP", "IPAddress", smtpServerIP);
            _settings.Port = Convert.ToInt32(_settings.SettingsIni.GetValue("SMTP", "Port", "25"));

            try
            {
                SendInternal(emailMessage, emailSubject, attachments);
            }
            catch (Exception ex)
            {
                SendInternal(ex.Message, "ErrorSending Email", attachments);
            }
        }

        private void SendTheMail(MailMessage message)
        {
            var smtp = new SmtpClient(_settings.Ipaddress, _settings.Port)
            {
                Credentials = new System.Net.NetworkCredential(smtpServerUsername, smtpServerPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            smtp.Send(message);
        }

        public string SendInternal(string emailMessage, string emailSubject, string[] attachments)
        {
            var sendTo = _settings.SettingsIni.GetValue("Email", "UserEmail", string.Empty);

            var ccTo = string.Empty;

            if (emailSubject != "ErrorSending Email")
            {
                var emailCount = Convert.ToInt32(_settings.SettingsIni.GetValue("Email", "UserEmailsCount", "0"));

                for (var i = 0; i < emailCount; i++)
                {
                    ccTo += _settings.SettingsIni.GetValue("Email", "CCEmails" + i, string.Empty) + ",";
                }

                var p = ccTo.LastIndexOf(',');

                if (ccTo.Length > 0)
                    ccTo = ccTo.Substring(0, p);
            }

            var message = new MailMessage();

            message.To.Add(sendTo);
            if (ccTo.Length > 0)
                message.CC.Add(ccTo);
            message.Subject = emailSubject;
            message.From = new MailAddress(smtpFromAddress, "Deposita Systems");
            message.Body = emailMessage;

            foreach (var attach in attachments.Select(attachment => new Attachment(attachment)))
            {
                message.Attachments.Add(attach);
            }

            message.Subject = "PLEASE DO NOT USE THIS FILE IT IS A TEST"; //subject;
            message.Body = "PLEASE EMAIL " + smtpFromAddress + " IF YOU receive this email, THANK YOU"; //emailMessage;

            SendTheMail(message);

            return sendTo;
        }
    }
}
