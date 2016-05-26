using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace VenusFiles
{
    /// <summary>
    /// This class was written to handle all database interaction on the kiosk
    /// </summary>

    public class SmartDatabase
    {
        #region Variables
        //Database variables
        readonly SqlConnection _dbConnection = new SqlConnection();
        private SqlDataReader _sdr;
        private SqlCommand _sc;
        private readonly string _settingFile = Application.StartupPath + "\\";

        public struct CashupStruct
        {
            public string TerminalCode;
            public int TransactionType;
            public Int64 FkTerminalsId;
            public string Vendor;
            public int Batch;
            public DateTime CashupDateTime;
            public int CashTotal;
            public string SealNumber;
            public string ReferenceNumber;
            public string Location;
            public int CashupsId;
        }

        public struct CheckCashupStruct
        {
            public int FkTerminalId;
            public string TerminalCode;
            public string Location;
            public bool DidCashup;
            public DateTime LastCashupReceived;
            public int LastBatch;
            public decimal CashupAmount;
            public decimal TransactionAmount;
        }
        
        public enum VedorName
        {
            CashPower,
            Ims,
            Actaris,
            MangaungBP,
            JHBBP,
            EkurhuleniBP,
            CityPowerCP,
            Grintek,
            Syntell,
            BelaBela,
            KwaDukuza,
            Airtime,
            MadibengBP,
            CigiCell
        }

        public struct TransactionDataStruct
        {
            public string CashierId;
            public string UnitId;
            public string LocationName;
            public int VenusTrace;
            public int Batch;
            public int Trace;
            public string TerminalCode;
            public string AccountNumber;
            public decimal CashAmount;
            public DateTime TransactionDateTime;
            public int VendSource { get; set; }
            public int TransactionId { get; set; }
        }

        #endregion

        #region Constructor Destructor

        public SmartDatabase(string dbConnectionString)
        {
            try
            {
                _dbConnection.ConnectionString = dbConnectionString;
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - Create - " + ex.Message);
            }
        }

        public bool TestDatabaseConnection(ref string error)
        {
            try
            {
                _dbConnection.Open();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }
        }

        #endregion

        #region Program Settings Methods

        public decimal TotalCashAmountByBatch(Int64 fkTerminalsId, int batchNumber, int TransactionType)
        {
            decimal cashAmount = 0;
            try
            {
                _dbConnection.Open();

                var ssql = @"SELECT  [Total] = SUM(Amount) FROM FilterTransactions WHERE fkTerminalID = '" +
                       Convert.ToString(fkTerminalsId) + "' AND Batch = " + Convert.ToString(batchNumber) +
                       " AND TransactionType=" + TransactionType + " AND VendSource=0";

                SqlCommand sc = new SqlCommand(ssql, _dbConnection);
                SqlDataReader sdr = sc.ExecuteReader();

                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        try
                        {
                            cashAmount = Convert.ToDecimal(sdr["Total"]);
                        }
                        catch
                        { }
                    }
                }
                else
                    cashAmount = 0;
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - TotalCashAmountByBatch - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            return cashAmount;
        }

        public bool MarkAsProcessed(int fkTerminalsId, string transactionDateTime, int batch, int trace, string accountNumber, decimal amount)
        {
            try
            {
                _dbConnection.Open();

                _sc = new SqlCommand(@"UPDATE Transactions SET Processed=" + "1" + " WHERE fkTerminalsId='" + fkTerminalsId + "' AND Batch='" + batch + "' AND Trace='" + trace + "' AND AccountNumber='" + accountNumber + "' AND CashAmount='" + amount + "'", _dbConnection);

                _sc.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - SaveEkurReceiptNumber - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            return true;
        }

        public bool MarkCashupAsProcessed(string transactionIds)
        {
            try
            {
                _dbConnection.Open();

                _sc = new SqlCommand(string.Format(@"UPDATE CashupsPerTransactionType SET Venus = 1 WHERE CashupsID IN ({0})", transactionIds), _dbConnection);

                _sc.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - MarkCashupAsProcessed - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            return true;
        }

        #endregion Program Settings Methods

        #region SQL Interaction Methods

        public List<CashupStruct> ExecuteVenusCashupStoredProcedure(string storedProcedureName, string sqlparam, out List<string> strList)
        {
            var cashupList = new List<CashupStruct>();
            var cashups = new List<CashupStruct>();
            var cu = new CashupStruct();

            try
            {
                _dbConnection.Open();

                _sc = new SqlCommand(storedProcedureName, _dbConnection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                _sc.Parameters.Add(new SqlParameter("@TransactionType", sqlparam));
                _sdr = _sc.ExecuteReader();

                int i = 1;

                while (_sdr.Read())
                {
                    if (i > 500)
                        break;
                    if (sqlparam == "7")
                    {
                        if ((Convert.ToInt64(_sdr["fkTerminalKey"]) == 10) ||
                            (Convert.ToInt64(_sdr["fkTerminalKey"]) == 101))
                        {
                            cu.Batch = Convert.ToInt32(_sdr["Batch"]);
                            cu.CashTotal = Convert.ToInt32(_sdr["CashTotal"]);
                            cu.FkTerminalsId = Convert.ToInt32(_sdr["fkTerminalKey"]);

                            cu.CashupDateTime = Convert.ToDateTime(_sdr["CashupDateTime"]);
                            cu.CashupsId = Convert.ToInt32(_sdr["CashupsID"]);
                            cu.Location = Convert.ToString(_sdr["LocationName"]);
                            cu.TerminalCode = Convert.ToString(_sdr["TerminalID"]);
                            cu.SealNumber = Convert.ToString(_sdr["SealNumber"]);
                            cu.ReferenceNumber = Convert.ToString(_sdr["BankReference"]);
                            cu.TransactionType = Convert.ToInt32(_sdr["TransactionType"]);

                            if (cu.TransactionType == 1)
                                cu.Vendor = "Ims";
                            else if (cu.TransactionType == 3)
                                cu.Vendor = "CashPower";
                            else if (cu.TransactionType == 7)
                                cu.Vendor = "JHBBP";

                            if (cu.SealNumber != "0")
                                cashupList.Add(cu);
                        }
                    }
                    else
                    {
                        cu.Batch = Convert.ToInt32(_sdr["Batch"]);
                        cu.CashTotal = Convert.ToInt32(_sdr["CashTotal"]);
                        cu.FkTerminalsId = Convert.ToInt64(_sdr["fkTerminalKey"]);

                        cu.CashupDateTime = Convert.ToDateTime(_sdr["CashupDateTime"]);
                        cu.Location = Convert.ToString(_sdr["LocationName"]);
                        cu.TerminalCode = Convert.ToString(_sdr["TerminalID"]);
                        cu.SealNumber = Convert.ToString(_sdr["SealNumber"]);
                        cu.ReferenceNumber = Convert.ToString(_sdr["BankReference"]);
                        cu.TransactionType = Convert.ToInt32(_sdr["TransactionType"]);
                        cu.CashupsId = Convert.ToInt32(_sdr["CashupsID"]);

                        if (cu.TransactionType == 1)
                            cu.Vendor = "Ims";
                        if (cu.TransactionType == 10)
                            cu.Vendor = "Actaris";
                        else if (cu.TransactionType == 3)
                            cu.Vendor = "CashPower";
                        else if (cu.TransactionType == 7)
                            cu.Vendor = "JHBBP";
                        else if (cu.TransactionType == 2)
                            cu.Vendor = "CityPowerCP";
                        else if (cu.TransactionType == 6)
                            cu.Vendor = "Grintek";
                        else if (cu.TransactionType == 5)
                            cu.Vendor = "Syntell";
                        else if (cu.TransactionType == 11)
                            cu.Vendor = "BelaBela";
                        else if (cu.TransactionType == 15)
                            cu.Vendor = "KwaDukuza";
                        else if (cu.TransactionType == 12)
                            cu.Vendor = "Airtime";
                        else if (cu.TransactionType == 9)
                            cu.Vendor = "MadibengBP";
                        else if (cu.TransactionType == 22)
                            cu.Vendor = "CigiCell";

                        if (cu.SealNumber != "0")
                            cashupList.Add(cu);

                        if ((cu.SealNumber == "0") && (cu.TransactionType == 2))
                            cashupList.Add(cu);
                    }

                    i++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - ExecuteVenusStoredProcedure - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            strList = new List<string>();
            strList.Add("TerminalCode,FkTerminalKey,CashupDateTime,Batch,CashTotal,Total,TransactionType");

            foreach (CashupStruct cs in cashupList)
            {
                decimal tot = 0;

                try
                {
                    tot = TotalCashAmountByBatch(cs.FkTerminalsId, cs.Batch, cs.TransactionType);
                }
                catch { }


                if ((tot == cs.CashTotal) && (tot != 0))
                {
                    cashups.Add(cs);
                }
                else
                {
                    strList.Add(cs.TerminalCode + "," + cs.FkTerminalsId + "," + cs.CashupDateTime + "," + cs.Batch + "," + cs.CashTotal + "," + tot + "," + cs.TransactionType);
                }
            }

            if (strList.Count > 1)
            {
                string errorLogPath = _settingFile + @"LogFiles\ErrorLog" + cu.Vendor + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                File.WriteAllLines(errorLogPath, strList.ToArray());
            }

            return cashups;
        }

        public List<TransactionDataStruct> GetTransactionData(CashupStruct cashupStruct,int venusTraceNumber, out int lastTransactionTrace, int fkTerminalKey)
        {
            var transactionList = new List<TransactionDataStruct>();
            var td = new TransactionDataStruct();

            try
            {
                _dbConnection.Open();

                var ssql =
                    string.Format(@"SELECT * FROM ABM_vwGetVenusTransactions WHERE Batch = {0} and fkTerminalID = {2} AND TransactionType = {1} AND Amount > 0 AND CardPaymentInfo IS NULL", cashupStruct.Batch, cashupStruct.TransactionType, fkTerminalKey);

                _sc = new SqlCommand(ssql, _dbConnection);

                _sdr = _sc.ExecuteReader();

                while (_sdr.Read())
                {
                    td.VendSource = string.IsNullOrEmpty(Convert.ToString(_sdr["CardPaymentInfo"])) ? 0 : 1;

                    td.AccountNumber = Convert.ToString(_sdr["AccountNumber"]);
                    td.Batch = Convert.ToInt32(_sdr["Batch"]);
                    td.CashAmount = Convert.ToDecimal(_sdr["Amount"]);
                    td.TerminalCode = Convert.ToString(_sdr["TerminalId"]);
                    td.TransactionDateTime = Convert.ToDateTime(_sdr["TransactionDateTime"]);
                    td.CashierId = Convert.ToString(_sdr["TellerID"]);
                    td.LocationName = Convert.ToString(_sdr["LocationName"]);
                    td.TransactionId = Convert.ToInt32(_sdr["TransactionId"]);
                    td.UnitId = Convert.ToString(_sdr["UnitID"]);

                    td.VendSource = Convert.ToInt32(_sdr["VendSource"]);

                    venusTraceNumber++;
                    td.VenusTrace = venusTraceNumber;
                    if (td.VenusTrace > 999999)
                    {
                        DeleteVenusTraces();
                        td.VenusTrace = 1;
                        venusTraceNumber = 1;
                    }

                    transactionList.Add(td);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - ExecuteVenusStoredProcedure - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            lastTransactionTrace = td.VenusTrace;

            return transactionList;
        }

        public List<TransactionDataStruct> ExecuteVenusTransactionsStoredProcedure(string storedProcedureName, CashupStruct cu, int venusTraceNumber, out int lastTransactionTrace)
        {
            List<TransactionDataStruct> transactionList = new List<TransactionDataStruct>();
            TransactionDataStruct td = new TransactionDataStruct();

            try
            {
                _dbConnection.Open();

                _sc = new SqlCommand(storedProcedureName, _dbConnection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };


                _sc.Parameters.Add(new SqlParameter("@TerminalCode", cu.TerminalCode));
                _sc.Parameters.Add(new SqlParameter("@BatchNumber", cu.Batch));
                _sc.Parameters.Add(new SqlParameter("@TransactionType", cu.TransactionType));
                _sc.Parameters.Add(new SqlParameter("@CashupDateTime", cu.CashupDateTime));
                _sdr = _sc.ExecuteReader();

                while (_sdr.Read())
                {
                    td.VendSource = Convert.ToInt32(_sdr["VendSource"]);


                    td.AccountNumber = Convert.ToString(_sdr["AccountNumber"]);
                    td.Batch = Convert.ToInt32(_sdr["Batch"]);
                    td.CashAmount = Convert.ToDecimal(_sdr["Amount"]);
                    td.TerminalCode = cu.TerminalCode;
                    td.TransactionDateTime = Convert.ToDateTime(_sdr["TransactionDateTime"]);
                    td.CashierId = Convert.ToString(_sdr["TellerID"]);
                    td.LocationName = Convert.ToString(_sdr["LocationName"]);

                    td.UnitId = cu.Vendor == "CashPower" ? Convert.ToString(_sdr["UnitID"]) : string.Empty;

                    td.VendSource = Convert.ToInt32(_sdr["VendSource"]);

                    venusTraceNumber++;
                    td.VenusTrace = venusTraceNumber;
                    if (td.VenusTrace > 999999)
                    {
                        DeleteVenusTraces();
                        td.VenusTrace = 1;
                        venusTraceNumber = 1;
                    }

                    transactionList.Add(td);

                }
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - ExecuteVenusStoredProcedure - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            lastTransactionTrace = td.VenusTrace;

            return transactionList;
        }

        public List<CheckCashupStruct>
            ExecuteCheckCashupsStoredProcedure()
        {
            List<CheckCashupStruct> ccList = new List<CheckCashupStruct>();
            CheckCashupStruct cc = new CheckCashupStruct();
            const string storedProcedureName = "spCheckCashupDates";

            try
            {
                _dbConnection.Open();

                _sc = new SqlCommand(storedProcedureName, _dbConnection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                //sc.Parameters.Add(new SqlParameter("@DateTimeFrom", DateTime.Today.Date));
                //sc.Parameters.Add(new SqlParameter("@DateTimeTo", DateTime.Today.Date.AddDays(1)));

                _sdr = _sc.ExecuteReader();

                while (_sdr.Read())
                {
                    cc.FkTerminalId = Convert.ToInt32(_sdr["TerminalsID"]);
                    cc.TerminalCode = Convert.ToString(_sdr["TerminalCode"]);
                    cc.Location = Convert.ToString(_sdr["Location"]);
                    cc.LastCashupReceived = Convert.ToDateTime(_sdr["LastCashup"]);
                    cc.LastBatch = Convert.ToInt32(_sdr["LastBatch"]);
                    cc.CashupAmount = Convert.ToInt32(_sdr["CashupAmount"]);
                    try
                    {
                        cc.TransactionAmount = Convert.ToInt32(_sdr["TransactionAmount"]);
                    }
                    catch
                    {
                        cc.TransactionAmount = 0;
                    }

                    cc.DidCashup = cc.LastCashupReceived.Date >= DateTime.Today.Date.AddDays(-1);

                    ccList.Add(cc);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - ExecuteVenusStoredProcedure - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            return ccList;
        }

        public bool SaveTrasaction(TransactionDataStruct td, string venusFileName)
        {
            try
            {
                _dbConnection.Open();

                _sc = new SqlCommand(@"INSERT INTO VenusTransactions " +
                                    "(TellerID" +
                                    ",VenusTrace" +
                                    ",MeterNumber" +
                                    ",Batch" +
                                    ",Amount" +
                                    ",DateofVenusFile" +
                                    ",DateofTransaction" +
                                    ",TerminalCode" +
                                    ",Location" +
                                    ",UnitID" +
                                    ",VenusFilName)" +
                                    "VALUES ('" + td.CashierId + "'," +
                                    "'" + td.VenusTrace + "'," +
                                    "'" + td.AccountNumber + "'," +
                                    "'" + td.Batch + "'," +
                                    "'" + td.CashAmount + "'," +
                                    "'" + DateTime.Now.Date.ToString("yyyyMMdd") + "'," +
                                    "'" + td.TransactionDateTime + "'," +
                                    "'" + td.TerminalCode + "'," +
                                    "'" + td.LocationName + "'," +
                                    "'" + td.UnitId + "'," +
                                    "'" + venusFileName + "')"
                                    , _dbConnection);
                _sc.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - SaveVenusTrace - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            return true;
        }

        public bool SaveCashup(CashupStruct cu, string venusFileName)
        {
            try
            {
                _dbConnection.Open();



                string ssql = @"INSERT INTO VenusCashups " +
                              "(VenusFileName" +
                              ",TerminalCode" +
                              ",Location" +
                              ",Batch" +
                              ",CashupTotal" +
                              ",Reference" +
                              ",SealNumber" +
                              ",CashupDateTime)" +
                              "VALUES ('" + venusFileName + "'" +
                              ",'" + cu.TerminalCode + "'" +
                              ",'" + cu.Location + "'" +
                              ",'" + cu.Batch + "'" +
                              ",'" + cu.CashTotal + "'" +
                              ",'" + cu.ReferenceNumber + "'" +
                              ",'" + cu.SealNumber + "'" +
                              ",'" + cu.CashupDateTime + "')";


                _sc = new SqlCommand(ssql, _dbConnection);

                _sc.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - SaveVenusTrace - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            return true;
        }

        public bool SaveTerminalData(CheckCashupStruct cc)
        {
            try
            {
                _dbConnection.Open();

                string ssql = @"UPDATE Terminals SET LastCashup='" + cc.LastCashupReceived + "', LastBatch='" + cc.LastBatch +
                              "' WHERE TerminalID='" + cc.FkTerminalId + "'";

                _sc = new SqlCommand(ssql, _dbConnection);

                _sc.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - SaveVenusTrace - " + ex.Message);
            }
            finally
            {
                if (_dbConnection != null)
                    _dbConnection.Close();
            }

            return true;
        }

        public bool DeleteVenusTraces()
        {
            const bool result = false;

            try
            {
                string[] trace = new string[1];
                trace[0] = "";
                File.WriteAllLines(_settingFile + @"Settings\Trace.txt", trace);
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseInteraction - DeleteVenusTraces - " + ex.Message);
            }

            return result;
        }

        #endregion SQL Interaction Methods
    }
}
