using System;
using System.Data;
using System.IO;
using System.Linq;

namespace VenusFiles
{
    public sealed class Configurator
    {

        #region Declarations

        public DataTable Settings;

        private string fileName;

        public enum FileType
        {
            Ini,
            Xml
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// creates the settings
        /// </summary>
        public Configurator()
        {
            initializeDataTable();
        }

        /// <summary>
        /// loads settings from a file (xml or ini)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ft"></param>
        public void LoadFromFile(string file, FileType ft)
        {
            fileName = Path.GetFullPath(file); //saves the filename for future use

            if (ft == FileType.Ini)
                LoadFromIni();
            else
                LoadFromXml();
        }


        /// <summary>
        /// adds a new setting to the table
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="OverwriteExisting"></param>
        public void AddValue(string Category, string Key, string Value, bool OverwriteExisting)
        {
            if (OverwriteExisting)
            {
                foreach (DataRow row in Settings.Rows.Cast<DataRow>().Where(row => (string)row[0] == Category && (string)row[1] == Key))
                {
                    row[2] = Value;
                    return;
                }

                Settings.Rows.Add(Category, Key, Value);
            }
            else
                Settings.Rows.Add(Category, Key, Value);
        }


        /// <summary>
        /// gets a value or returns a default value
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="Key"></param>
        /// <param name="DefaultValue"></param>
        /// <returns>Value or Default Value</returns>
        public string GetValue(string Category, string Key, string DefaultValue) //
        {
            foreach (DataRow row in Settings.Rows.Cast<DataRow>().Where(row => (string)row[0] == Category && (string)row[1] == Key))
            {
                return (string)row[2];
            }

            return DefaultValue;
        }

        /// <summary>
        /// saves the file to the previously loaded file
        /// </summary>
        /// <param name="ft"></param>
        public void Save(FileType ft)
        {
            //sorts the table for saving

            if (fileName == "") throw new FileNotFoundException("The file name was not previously defined");

            DataView dv = Settings.DefaultView;
            dv.Sort = "Category asc";
            DataTable sortedDT = dv.ToTable();

            if (ft == FileType.Xml)
                sortedDT.WriteXml(fileName);
            else
            {
                StreamWriter sw = new StreamWriter(fileName);

                string lastCategory = "";

                foreach (DataRow row in sortedDT.Rows)
                {
                    if ((string)row[0] != lastCategory)
                    {
                        lastCategory = (string)row[0];
                        sw.WriteLine("[" + lastCategory + "]");
                    }

                    sw.WriteLine((string)row[1] + "=" + (string)row[2]);
                }

                sw.Close();
            }
        }

        /// <summary>
        /// saves the file to a file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ft"></param>
        public void Save(string file, FileType ft)
        {
            fileName = Path.GetFullPath(file); //saves the filename for future use

            Save(ft);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// loads settings from ini
        /// </summary>
        private void LoadFromIni()
        {
            if (!File.Exists(fileName)) return;

            StreamReader sr = new StreamReader(fileName); //stream reader that will read the settings

            string currentCategory = ""; //holds the category we're at

            while (!sr.EndOfStream) //goes through the file
            {
                string currentLine = sr.ReadLine(); //reads the current file

                if (currentLine.Length < 3) continue; //checks that the line is usable

                if (currentLine.StartsWith("[") && currentLine.EndsWith("]")) //checks if the line is a category marker
                {
                    currentCategory = currentLine.Substring(1, currentLine.Length - 2);
                    continue;
                }

                if (!currentLine.Contains("=")) continue; //or an actual setting

                string currentKey = currentLine.Substring(0, currentLine.IndexOf("=", StringComparison.Ordinal));

                string currentValue = currentLine.Substring(currentLine.IndexOf("=", StringComparison.Ordinal) + 1);

                AddValue(currentCategory, currentKey, currentValue, true);
            }

            sr.Close(); //closes the stream
        }

        /// <summary>
        /// loads the settings from an xml file
        /// </summary>
        private void LoadFromXml()
        {
            Settings.ReadXml(fileName);
        }

        /// <summary>
        /// re-initializes the table with the proper columns
        /// </summary>
        private void initializeDataTable()
        {
            Settings = new DataTable { TableName = "Settings" };

            Settings.Columns.Add("Category", typeof(string));
            Settings.Columns.Add("SettingKey", typeof(string));
            Settings.Columns.Add("SettingsValue", typeof(string));
        }

        #endregion
    }
}
