using System.ServiceProcess;

namespace VenusFileService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun = new ServiceBase[] 
                                              { 
                                                  new VenusFiles() 
                                              };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
