using System;
using System.IO;

namespace VenusFileService
{
    public class Logger
    {
        private StreamWriter LogFile;

        public void Start()
        {
            try
            {
                string LogFilePath = System.Windows.Forms.Application.StartupPath;
                if (Directory.Exists(LogFilePath))
                {
                    string fileName = Path.Combine(LogFilePath, DateTime.Now.ToString("ddMMyyyy") + ".txt");
                    LogFile = new StreamWriter(fileName, true);
                    Write("Service started");
                }
            }
            catch
            {

            }
        }

        public void Stop()
        {
            try
            {
                Write("Service stopped");
                LogFile.Close();
            }
            catch
            {

            }
        }

        public void Write(string message)
        {
            try
            {
                LogFile.WriteLine(string.Format("{0} - {1}", DateTime.Now, message));
                LogFile.Flush();
            }
            catch
            {

            }
        }
    }
}
