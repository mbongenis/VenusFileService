using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using G4S.Deposita.TransactionData;

namespace VenusFiles
{
    public class CreateVenusFiles
    {
        private VenusData _venus;
        private readonly SmartDatabase _sdb = new SmartDatabase(@"" + System.Configuration.ConfigurationManager.AppSettings["SmartDatabase"].ToString() + "");
        private int _transactionType;
        public DateTime DateToPass = DateTime.Now;
        private string _venusFileName = "";
        private readonly string _settingFile;
        private List<string> _stringList = new List<string>();
        private SmartDatabase.VedorName _vendorName;
        private string _whereToAreFileFor;
        private int _sqno;
        private string _ilembeSequenceNumber;
        private bool _isEkur;
        private bool _iscashup;

        public CreateVenusFiles(string settingsFilePath)
        {
            _settingFile = settingsFilePath;
        }

        public string ExceuteVenusCreator()
        {
            var strMessage = string.Empty;

            try
            {
                //Everything happens here
                string venusCashupSp = File.ReadAllText(string.Format("{0}settings\\venusCashupSP.txt", _settingFile));
                string[] vendors = File.ReadAllText(string.Format("{0}settings\\vendors.txt", _settingFile)).Split(',');
                int vendorCount = vendors.Length;
                int i = 0;
                while (i < vendorCount)
                {
                    _stringList.Clear();

                    string vn = vendors[i];
                    switch (vn)
                    {
                        case "MangaungBP":
                            _transactionType = 14;
                            _vendorName = SmartDatabase.VedorName.MangaungBP;
                            _whereToAreFileFor = "MBP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "BelaBela":
                            _transactionType = 11;
                            _vendorName = SmartDatabase.VedorName.BelaBela;
                            _whereToAreFileFor = "BCP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "CashPower":
                            _transactionType = 3;
                            _vendorName = SmartDatabase.VedorName.CashPower;
                            _whereToAreFileFor = "358115";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = true;
                            break;

                        case "Actaris":
                            _transactionType = 10;
                            _vendorName = SmartDatabase.VedorName.Actaris;
                            _whereToAreFileFor = "APP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "IMS":
                            _transactionType = 1;
                            _vendorName = SmartDatabase.VedorName.Ims;
                            _whereToAreFileFor = "178101";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = true;
                            break;

                        case "JHBBP":
                            _transactionType = 7;
                            _vendorName = SmartDatabase.VedorName.JHBBP;
                            _whereToAreFileFor = "JBP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "EkurhuleniBP":
                            _transactionType = 8;
                            _vendorName = SmartDatabase.VedorName.EkurhuleniBP;
                            _whereToAreFileFor = "EBP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = true;
                            break;

                        case "MadibengBP":
                            _transactionType = 9;
                            _vendorName = SmartDatabase.VedorName.MadibengBP;
                            _whereToAreFileFor = "BPM";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "CityPowerCP":
                            _transactionType = 2;
                            _vendorName = SmartDatabase.VedorName.CityPowerCP;
                            _whereToAreFileFor = "CPP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "Grintek":
                            _transactionType = 6;
                            _vendorName = SmartDatabase.VedorName.Grintek;
                            _whereToAreFileFor = "GPP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "Syntell":
                            _transactionType = 5;
                            _vendorName = SmartDatabase.VedorName.Syntell;
                            _whereToAreFileFor = "SPP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "Airtime":
                            _transactionType = 12;
                            _vendorName = SmartDatabase.VedorName.Airtime;
                            _whereToAreFileFor = "SAT";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "CigiCell":
                            _transactionType = 22;
                            _vendorName = SmartDatabase.VedorName.CigiCell;
                            _whereToAreFileFor = "CGC";
                            Form1.FileDefinition = _whereToAreFileFor;
                            _isEkur = false;
                            break;

                        case "KwaDukuza":
                            _transactionType = 15;
                            _vendorName = SmartDatabase.VedorName.KwaDukuza;
                            _isEkur = false;
                            _sqno =
                                Convert.ToInt32(
                                    File.ReadAllText(string.Format("{0}settings\\IlembeSequenceNumber.txt", _settingFile)));
                            _sqno++;
                            _ilembeSequenceNumber = _sqno < 10
                                ? string.Format("0{0}", _sqno)
                                : _sqno.ToString(CultureInfo.InvariantCulture);

                            _whereToAreFileFor = "KBP";
                            Form1.FileDefinition = _whereToAreFileFor;
                            break;
                    }

                    List<string> strList;

                    var cashupList = _sdb.ExecuteVenusCashupStoredProcedure(venusCashupSp, _transactionType.ToString(CultureInfo.InvariantCulture), out strList);

                    Form1.strList = strList;

                    if (cashupList.Count > 0)
                    {
                        strMessage = "OK";
                        var tdl = GetTransactions(cashupList);

                        if (tdl.Count > 0)
                        {
                            CreateVenusFile(tdl);

                            new Reports().CreateReports(cashupList, tdl, _venusFileName,
                                _settingFile);
                        }

                        MarkAsProcessed(cashupList);

                        if (_vendorName == SmartDatabase.VedorName.KwaDukuza)
                            File.WriteAllText(string.Format("{0}settings\\IlembeSequenceNumber.txt", _settingFile),
                                _sqno.ToString(CultureInfo.InvariantCulture));
                        tdl.Clear();
                    }
                    else
                    {
                        Form1.EmailNOEmails(vendors[0]);

                        if (!_iscashup)
                        {
                            if (i == vendorCount - 1)
                            {
                                Form1.NoCashups = true;
                                strMessage = "OK";
                            }
                        }
                    }

                    cashupList.Clear();
                    i++;
                }
            }
            catch (Exception ex)
            {
                strMessage = ex.ToString();
            }

            return strMessage;
        }

        public void MarkAsProcessed(IEnumerable<SmartDatabase.CashupStruct> cul)
        {
            var transactionIds = cul.Aggregate(string.Empty, (current, cashupStruct) => current + (cashupStruct.CashupsId + ","));

            transactionIds = transactionIds.Substring(0, transactionIds.Length - 1);

            _sdb.MarkCashupAsProcessed(transactionIds);
        }

        public void MarkAsProcessed(List<SmartDatabase.TransactionDataStruct> tdl)
        {
            var transactionIds = string.Empty;

            foreach (var t in tdl)
            {
                transactionIds += t.TransactionId + ",";
            }

            transactionIds = transactionIds.Substring(0, transactionIds.Length - 1);

            _sdb.MarkCashupAsProcessed(transactionIds);
        }

        public void CreateVenusFile(List<SmartDatabase.TransactionDataStruct> vtl)
        {
            string passThisDate = DateToPass.ToString("yyyyMMdd");
            bool keepLooping = true;
            int count = 1;

            if (_vendorName == SmartDatabase.VedorName.KwaDukuza)
            {
                keepLooping = false;
                _venusFileName = string.Format("ABSA{0}{1}", _whereToAreFileFor, _ilembeSequenceNumber) +
                                 DateToPass.ToString("yyyyMMdd");
            }
            else
                _venusFileName = string.Format("ABSA{0}S0{1}", _whereToAreFileFor, count) +
                             DateToPass.ToString("MMdd");

            while (keepLooping)
            {
                if (
                    File.Exists(_settingFile + @"Backups\ZipFiles\" + DateToPass.ToString("yyyyMMdd") + "\\" +
                                _venusFileName + ".txt"))
                {
                    count++;
                    var incremented = count < 10 ? string.Format("0{0}", count) : count.ToString(CultureInfo.InvariantCulture);

                    if (_vendorName == SmartDatabase.VedorName.KwaDukuza)
                        _venusFileName = string.Format("ABSA{0}{1}", _whereToAreFileFor, incremented) +
                                         DateToPass.ToString("yyyyMMdd");
                    else
                        _venusFileName = string.Format("ABSA{0}S{1}", _whereToAreFileFor, incremented) +
                                         DateToPass.ToString("MMdd");
                }
                else
                {
                    keepLooping = false;
                }
            }

            int i = 1;

            foreach (SmartDatabase.TransactionDataStruct vt in vtl)
            {
                _venus = new VenusData(_settingFile + _venusFileName + ".txt", _stringList);

                int paymentType;

                if (vt.VendSource == 0)
                    paymentType = 1;
                else
                {
                    paymentType = 2;
                }

                var decimalTemp = decimal.Round(vt.CashAmount, 2);

                var decimalArray = decimalTemp.ToString(CultureInfo.InvariantCulture).Split('.');

                var rands = decimalArray[0];
                string cents;

                try
                {
                    cents = decimalArray[1];
                }
                catch
                {
                    cents = "00";
                }

                try
                {
                    if (_vendorName == SmartDatabase.VedorName.MangaungBP)
                    {
                        if (i >= 100)
                            i = 1;
                        _stringList = (_venus.PassVenusData(Convert.ToInt32("0045"),
                            vt.VenusTrace,
                            i,
                            "T4",
                            1,
                            vt.AccountNumber,
                            1,
                            Convert.ToInt32(rands),
                            cents,
                            "000",
                            "000018",
                            "0",
                            0,
                            0,
                            "0",
                            0,
                            passThisDate));
                        i++;
                    }
                    else if (_vendorName == SmartDatabase.VedorName.KwaDukuza)
                    {
                        _stringList = (_venus.PassVenusData(Convert.ToInt32(vt.CashierId),
                            vt.VenusTrace,
                            1,
                            "T7",
                            1,
                            vt.AccountNumber,
                            1,
                            Convert.ToInt32(rands),
                            cents,
                            "000",
                            "000018",
                            "0",
                            0,
                            0,
                            "0",
                            0,
                            passThisDate));
                    }
                    else if (_vendorName == SmartDatabase.VedorName.EkurhuleniBP)
                    {
                        _stringList = (_venus.PassVenusData(Convert.ToInt32(vt.CashierId),
                            vt.VenusTrace,
                            1,
                            "T4",
                            paymentType,
                            vt.AccountNumber,
                            1,
                            Convert.ToInt32(rands),
                            cents,
                            "000",
                            "000018",
                            "0",
                            0,
                            0,
                            "0",
                            0,
                            passThisDate));
                    }
                    else if (_vendorName == SmartDatabase.VedorName.CashPower ||
                             _vendorName == SmartDatabase.VedorName.Ims)
                    {
                        _stringList = (_venus.PassVenusData(Convert.ToInt32(vt.CashierId),
                            vt.VenusTrace,
                            1,
                            "T7",
                            1,
                            vt.AccountNumber,
                            1,
                            Convert.ToInt32(rands),
                            cents,
                            "000",
                            "000009",
                            "0",
                            0,
                            0,
                            "0",
                            0,
                            passThisDate));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("CreateVenusFile - " + ex.Message);
                }
            }

            if (_vendorName != SmartDatabase.VedorName.MangaungBP)
            {
                File.WriteAllLines(_settingFile + @"CreatedFiles\" + _venusFileName + ".txt", _stringList.ToArray());
                File.WriteAllLines(_settingFile + @"Backups\VenusFiles\" + _venusFileName + ".txt", _stringList.ToArray());
            }
            else
            {
                File.WriteAllLines(_settingFile + @"CreatedFiles\" + _venusFileName + ".txt", _stringList.ToArray());
                File.WriteAllLines(_settingFile + @"Backups\VenusFiles\" + _venusFileName + ".txt", _stringList.ToArray());
            }
        }

        public SmartDatabase.VedorName GetVendorName(int i)
        {
            var vn = new SmartDatabase.VedorName();

            if (i == 0)
                vn = SmartDatabase.VedorName.CashPower;
            else if (i == 1)
                vn = SmartDatabase.VedorName.Ims;

            return vn;
        }

        public void CreateDirectories()
        {
            if (!Directory.Exists(_settingFile))
                Directory.CreateDirectory(_settingFile);
            if (!Directory.Exists(_settingFile + "CreatedFiles"))
                Directory.CreateDirectory(_settingFile + "CreatedFiles");
            if (!Directory.Exists(_settingFile + "Backups"))
                Directory.CreateDirectory(_settingFile + "Backups");
            if (!Directory.Exists(_settingFile + @"Backups\VenusFiles"))
                Directory.CreateDirectory(_settingFile + @"Backups\VenusFiles");
            if (!Directory.Exists(_settingFile + "Backups\\ZipFiles"))
                Directory.CreateDirectory(_settingFile + @"Backups\ZipFiles");
            if (!Directory.Exists(_settingFile + "Backups\\ZipFiles\\" + DateToPass.ToString("yyyyMMdd")))
                Directory.CreateDirectory(_settingFile + "Backups\\ZipFiles\\" + DateToPass.ToString("yyyyMMdd"));
            if (!Directory.Exists(_settingFile + "LogFiles"))
                Directory.CreateDirectory(_settingFile + "LogFiles");
            if (!Directory.Exists(_settingFile + @"Backups\VenusFiles\Reports"))
                Directory.CreateDirectory(_settingFile + @"Backups\VenusFiles\Reports");
            if (!Directory.Exists(_settingFile + "Temp"))
                Directory.CreateDirectory(_settingFile + "Temp");
            if (!Directory.Exists(_settingFile + "Settings"))
                Directory.CreateDirectory(_settingFile + "Settings");
        }

        public List<SmartDatabase.TransactionDataStruct> GetTransactions(List<SmartDatabase.CashupStruct> cul)
        {
            var tdl = new List<SmartDatabase.TransactionDataStruct>();

            var venusTrace = 0;

            foreach (var cu in cul)
            {
                venusTrace = ReadLastTrace();
                tdl.AddRange(!_isEkur
                    ? _sdb.GetTransactionData(cu, venusTrace,
                        out venusTrace, (int)cu.FkTerminalsId)
                    : _sdb.GetTransactionData(cu, venusTrace,
                        out venusTrace, (int)cu.FkTerminalsId));
                WriteTraceNumber(venusTrace);
            }

            WriteTraceNumber(venusTrace);

            return tdl;
        }

        private void WriteTraceNumber(int lastTrace)
        {
            var trace = new string[1];
            trace[0] = Convert.ToString(lastTrace);
            File.WriteAllLines(_settingFile + @"Settings\Trace.txt", trace);
        }

        private int ReadLastTrace()
        {
            int venusTrace;

            try
            {
                venusTrace = Convert.ToInt32(File.ReadAllText(_settingFile + @"Settings\Trace.txt"));
            }
            catch
            {
                venusTrace = 1;
            }

            return venusTrace;
        }

        public void CleanupComplete()
        {
            MoveFilesToTemp();
        }

        public void MoveFilesToTemp()
        {
            var dirList = Directory.GetFiles(_settingFile + "CreatedFiles");

            foreach (var str in dirList)
            {
                var p = str.LastIndexOf("\\", StringComparison.Ordinal);
                var fileName = str.Substring(p);

                File.Move(str, string.Format(@"{0}Backups\Zipfiles\{1}\{2}", _settingFile, DateToPass.ToString("yyyyMMdd"), fileName));
            }
        }

        public void DeleteTempFiles()
        {
            var dirList = Directory.GetFiles(_settingFile + @"Temp");

            foreach (var str in dirList)
            {
                var p = str.LastIndexOf("\\", StringComparison.Ordinal);
                var fileName = str.Substring(p);

                File.Delete(_settingFile + @"Temp\" + fileName);
            }
        }
    }
}