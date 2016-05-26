using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VenusFiles
{
    public partial class Form1 : Form
    {
        public static bool NoCashups;
        private CreateVenusFiles venus;
        private readonly string _settingFile = Application.StartupPath + "\\";
        private string[] vendors;
        public static string FileDefinition;
        public static List<string> strList;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tmrStartVenus.Start();
            //btnCreateVenus.Enabled = true;
        }

        private void btnCreateVenus_Click(object sender, EventArgs e)
        {
            vendors = new string[1];
            vendors[0] = "KwaDukuza";
            EmailNOEmails(vendors[0]);

            //vendors = File.ReadAllText(string.Format("{0}settings\\vendors.txt", _settingFile)).Split(',');
            //vendors[0] = "KwaDukuza";
            //btnCreateVenus.Enabled = false;
            //EmailFiles("KBP");
            //UploadFiles();
            Close();
            // tmrStartVenus.Start();
            //EmailError("Testing");
            //venus = new CreateVenusFiles(_settingFile);
            //listBox1.Items.Add(venus.ExceuteVenusCreator());
        }


        private void tmrStartVenus_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrStartVenus.Stop();

                //UploadFiles();

                vendors = File.ReadAllText(string.Format("{0}settings\\vendors.txt", _settingFile)).Split(',');
                venus = new CreateVenusFiles(_settingFile);
                venus.CreateDirectories();
                venus.DeleteTempFiles();

                string strMessage = venus.ExceuteVenusCreator();

                if (strMessage == "OK")
                {
                    venus.MoveFilesToTemp();

                    EmailFiles(FileDefinition);

                    Close();
                }
                else
                {
                    EmailError(strMessage);
                    Close();
                }
            }
            catch (Exception ex)
            {
                EmailError(ex.Message);
            }
        }

        private void EmailFiles(string fileDef)
        {
            string[] attachments = Directory.GetFiles(_settingFile + @"Backups\ZipFiles\" + DateTime.Now.Date.ToString("yyyMMdd"));
            string sendTo = File.ReadAllText(_settingFile + @"Settings\emails.txt");
            string subject = string.Empty;
            string body = string.Empty;

            if (attachments != null)
            { 
            if (attachments.Length > 0)
            {
                try
                {
                    try
                    {
                        subject = string.Format("ABM CashTrack Venus Files {0} {1}", vendors[0], vendors[1]);
                    }
                    catch
                    {
                        subject = string.Format("ABM CashTrack Venus Files {0}", vendors[0]);
                    }

                    if (vendors[0] == "KwaDukuza")
                        subject = "Ilembe Files - " + DateTime.Now.ToString("yyyyMMdd");

                    body = "Please find attached the venus files for the " + DateTime.Now.Date.ToString("yyyy/MM/dd");

                    if (vendors[0] == "KwaDukuza")
                        body = "Please find attached the Ilembe files for the " +
                               DateTime.Now.Date.ToString("yyyy/MM/dd");

                    if (vendors[0] != "KwaDukuza")
                        Utility.SendEmail(sendTo, subject, body, attachments);
                    else
                    {
                        IEmailManager emailManager = new EmailManager(Application.StartupPath);

                        emailManager.SendEmail(body, subject, attachments);
                    }
                }
                catch (Exception ex)
                {
                    EmailError(ex.Message);
                }
            }
}
        }

        public static void EmailNOEmails(string vendor)
        {
            string sendTo = File.ReadAllText(Application.StartupPath + "\\" + @"Settings\NoEmails.txt");
            string subject = string.Empty;
            string body = string.Empty;

            try
            {
                if (vendor == "KwaDukuza")
                    subject = "Ilembe Files - " + DateTime.Now.ToString("yyyyMMdd");

                body = "Please ask the minder to restart the kiosk. No Cashups where found for " + DateTime.Now.Date.ToString("yyyy/MM/dd");

                string[] attachments = new string[0];
                Utility.SendEmail(sendTo, subject, body, attachments);
            }
            catch
            {

            }
        }

        private void EmailCashupsThatDoNotMatch()
        {
            string sendTo = File.ReadAllText(_settingFile + @"Settings\LogEmails.txt");
            string subject = string.Empty;
            string messageBody = string.Empty;

            try
            {
                try
                {
                    subject = string.Format("CashTrack Venus Files {0} {1}", vendors[0], vendors[1]);
                }
                catch
                {
                    subject = string.Format("CashTrack Venus Files {0}", vendors[0]);
                }


                messageBody = "Cashups and Transactions do not match" + Environment.NewLine + Environment.NewLine;

                foreach (string s in strList)
                {
                    messageBody += s + Environment.NewLine;
                }

                string[] str = new string[0];
                Utility.SendEmail(sendTo, subject, messageBody, str);
            }
            catch (Exception ex)
            {
                EmailError(ex.Message);
            }
        }

        private void EmailError(string strMessage)
        {
            string messageSubject = string.Empty;

            string sendTo = File.ReadAllText(_settingFile + @"Settings\emailError.txt");

            if (vendors[0] == "KwaDukuza")
                messageSubject = "Ilembe Files";
            else
                messageSubject = "CashTrack Venus Files";

            string messageBody = strMessage;
            string[] str = new string[0];
            Utility.SendEmail(sendTo, messageSubject, messageBody, str);
        }


        private void tmrPause_Tick(object sender, EventArgs e)
        {
            tmrPause.Stop();
            //EmailFiles(FileDefinition);
            //venus.DeleteTempFiles();
            Close();
        }
    }
}
