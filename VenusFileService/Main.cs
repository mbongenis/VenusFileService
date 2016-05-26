using System;
using System.ServiceProcess;
using System.Threading;

namespace VenusFileService
{
    public partial class VenusFiles : ServiceBase
    {

        private const int FAST_PERIOD = 1000 * 60; //1 Minute
        private readonly Mutex InsertMutex;
        private System.Timers.Timer InsertTimer;
        private DateTime UploadIPAddressTimer;
        private readonly Logger log;

        public VenusFiles()
        {
            InitializeComponent();
            InsertMutex = new Mutex();
            log = new Logger();
        }

        protected override void OnStart(string[] args)
        {
            UploadIPAddressTimer = DateTime.Now;
            InsertTimer = new System.Timers.Timer { Interval = FAST_PERIOD };
            InsertTimer.Elapsed += InsertTimer_Elapsed;
            InsertTimer.Enabled = true;
        }

        void InsertTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                InsertTimer.Enabled = false;
                InsertMutex.WaitOne();

                //do Work here

            }
            catch (Exception ex)
            {
                log.Write("Error: " + ex.Message);
            }
            finally
            {
                InsertMutex.ReleaseMutex();
                InsertTimer.Enabled = true;
            }
        }

        protected override void OnStop()
        {
            log.Stop();
        }
    }
}
