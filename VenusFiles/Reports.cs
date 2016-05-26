using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using GemBox.Spreadsheet;

namespace VenusFiles
{
    public class Reports
    {
        string _settingFile = "";
        readonly SmartDatabase _sdb = new SmartDatabase("Data Source=Smartsql;Initial Catalog=EkurhuleniVenus;User ID=sa;Password=''");
        //SmartDatabase sdb = new SmartDatabase(@"Data Source=Stephen\SQLExpress;Initial Catalog=EkurhuleniVenus;User ID=kiosk;Password='sm@rt3c'");

        public void CreateReports(IEnumerable<SmartDatabase.CashupStruct> cul, IEnumerable<SmartDatabase.TransactionDataStruct> vtdl, string venusFileName, string settingsFilePath)
        {
            _settingFile = settingsFilePath;
            CashupReport(cul, venusFileName);
            //TransactionReport(vtdl, dateToPass, venusFileName);
            TransactionReport(vtdl, venusFileName);
        }

        private void CashupReport(IEnumerable<SmartDatabase.CashupStruct> cul, string venusFileName)
        {
            try
            {
                SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

                var file = new ExcelFile();
                var worksheet = file.Worksheets.Add("New worksheet");
                worksheet.Columns["A"].Width = 0x1770;
                worksheet.Columns["B"].Width = 0xfa0;
                worksheet.Columns["C"].Width = 0x1f40;
                worksheet.Columns["D"].Width = 0xfa0;
                worksheet.Columns["E"].Width = 0x1770;
                worksheet.Cells["A1"].SetBorders(MultipleBorders.Bottom, Color.Black, LineStyle.Medium);
                worksheet.Cells["B1"].SetBorders(MultipleBorders.Bottom, Color.Black, LineStyle.Medium);
                worksheet.Cells["A3"].SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Medium);
                worksheet.Cells["B3"].SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Medium);
                worksheet.Cells["C3"].SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Medium);
                worksheet.Cells["D3"].SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Medium);
                worksheet.Cells["E3"].SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Medium);
                worksheet.Cells["F3"].SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Medium);
                worksheet.Cells["G3"].SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Medium);
                worksheet.Cells["A1"].Value = "Cashup Report - VenusFiles Name: " + venusFileName;
                worksheet.Cells["A3"].Value = "DateTime";
                worksheet.Cells["B3"].Value = "TerminalCode";
                worksheet.Cells["C3"].Value = "Location";
                worksheet.Cells["D3"].Value = "Batch";
                worksheet.Cells["E3"].Value = "Total";
                int num = 4;

                int cashTotal = 0;

                foreach (SmartDatabase.CashupStruct cu in cul)
                {
                    worksheet.Cells["A" + Convert.ToString(num)].Value = Convert.ToString(cu.CashupDateTime);
                    worksheet.Cells["B" + Convert.ToString(num)].Value = cu.TerminalCode;
                    worksheet.Cells["C" + Convert.ToString(num)].Value = Convert.ToString(cu.Location);
                    worksheet.Cells["D" + Convert.ToString(num)].Value = Convert.ToString(cu.Batch);
                    worksheet.Cells["E" + Convert.ToString(num)].Value = Convert.ToInt32(cu.CashTotal);

                    cashTotal += cu.CashTotal;

                    _sdb.SaveCashup(cu, venusFileName);

                    num++;
                }
                var num3 = num + 2;
                worksheet.Cells["A" + num3].Value = "Total Amount: R " + Convert.ToString(cashTotal);

                file.SaveXls(_settingFile + @"CreatedFiles\CashupReport" + venusFileName + ".xls");
                file.SaveXls(_settingFile + @"Backups\VenusFiles\Reports\CashupReport" + venusFileName + ".xls");
            }
            catch (Exception exception)
            {
                throw new Exception("CashupReport " + exception.Message);
            }

        }

        private void TransactionReport(IEnumerable<SmartDatabase.TransactionDataStruct> vtdl, string venusFileName)
        {
            var strList = new List<string>
            {
                "TerminalCode,Location Name,TellerID,UnitID,VenusTrace,MeterNumber,Batch,CashAmount,DateTime"
            };

            foreach (var vtd in vtdl)
            {
                if (vtd.UnitId == string.Empty)
                    strList.Add(vtd.TerminalCode + "," + vtd.LocationName + "," + vtd.CashierId + "," + "" + "," + Convert.ToString(vtd.VenusTrace) + "," + Convert.ToString(vtd.AccountNumber) + "," + Convert.ToString(vtd.Batch) + "," + Convert.ToString("R " + vtd.CashAmount) + "," + Convert.ToString(vtd.TransactionDateTime));
                else
                    strList.Add(vtd.TerminalCode + "," + vtd.LocationName + "," + vtd.CashierId + "," + vtd.UnitId + "," + Convert.ToString(vtd.VenusTrace) + "," + Convert.ToString(vtd.AccountNumber) + "," + Convert.ToString(vtd.Batch) + "," + Convert.ToString("R " + vtd.CashAmount) + "," + Convert.ToString(vtd.TransactionDateTime));

                _sdb.SaveTrasaction(vtd, venusFileName);
            }

            File.WriteAllLines(_settingFile + @"CreatedFiles\Transactions" + venusFileName + ".csv", strList.ToArray());
            File.WriteAllLines(_settingFile + @"Backups\VenusFiles\Reports\Transactions" + venusFileName + ".csv", strList.ToArray());
        }


    }
}
