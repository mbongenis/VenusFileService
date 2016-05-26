namespace VenusFiles
{
    public interface ISettings
    {
        string Ipaddress { get; set; }
        int Port { get; set; }

    }


    public class Settings : ISettings
    {
        public Configurator SettingsIni;

        public string Ipaddress { get; set; }
        public int Port { get; set; }
    }
}
