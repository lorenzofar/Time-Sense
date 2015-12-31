using Database;
using Stuff;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Time_Sense
{
    public sealed class ExcelExporter
    {
        static string[] timeline_header = { "excel_banner_time", "excel_banner_usage", "excel_banner_unlocks", "excel_banner_battery", "excel_banner_charging", "excel_banner_wifi", "excel_banner_bluetooth", "excel_banner_latitude", "excel_banner_longitude" };
        static string[] timeline_alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I" };

        public static async Task CreateExcelReport(StorageFile export_file)
        {
            DateTime start_date = App.range_start_date;
            DateTime end_date = App.range_end_date;
            int days_raw = end_date.Subtract(start_date).Days;
            List<DateTime> days_list = new List<DateTime>();
            for (int i = 0; i < days_raw + 1; i++)
            {
                DateTime current_date = start_date.AddDays(i);
                string date_str = utilities.shortdate_form.Format(current_date);
                var item = await Helper.ConnectionDb().Table<Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
                if (item != null)
                {
                    days_list.Add(current_date);
                }
            }
            int days = days_list.Count;
            //INITIALIZE EXCEL ENGINE AND CREATE WORKSHEETS
            ExcelEngine excel_engine = new ExcelEngine();
            IApplication application = excel_engine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(days + 1);
            #region STYLES
            IStyle bold_style = workbook.Styles.Add("BoldStyle");
            bold_style.Font.Bold = true;

            IStyle banner_style = workbook.Styles.Add("TimelineBannerStyle");
            banner_style.Font.Bold = true;
            banner_style.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Medium;
            banner_style.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Medium;
            banner_style.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Medium;
            banner_style.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Medium;
            banner_style.HorizontalAlignment = ExcelHAlign.HAlignCenter;

            IStyle table_style = workbook.Styles.Add("TimelineTableStyle");
            table_style.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
            table_style.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            table_style.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            table_style.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            table_style.HorizontalAlignment = ExcelHAlign.HAlignCenter;

            IStyle overview_style = workbook.Styles.Add("OverviewStyle");
            overview_style.HorizontalAlignment = ExcelHAlign.HAlignRight;
            #endregion

            int total_usage = 0;
            int total_unlocks = 0;
            foreach (var current_date in days_list)
            {
                int i = days_list.IndexOf(current_date);
                string date_str = utilities.shortdate_form.Format(current_date);
                IWorksheet worksheet = workbook.Worksheets[i + 1];
                worksheet.Name = String.Format("{0}-{1}-{2}", current_date.Day, current_date.Month, current_date.Year);
                var item = await Helper.ConnectionDb().Table<Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
                if (item != null)
                {
                    var item_unlocks = await Helper.ConnectionDb().Table<Timeline>().Where(x => x.date == date_str).ToListAsync();
                    total_usage += item.usage;
                    total_unlocks += item_unlocks.Count;
                    for (int a = 0; a < 9; a++)
                    {
                        worksheet.Range[timeline_alphabet[a] + "1"].Text = utilities.loader.GetString(timeline_header[a]);
                    }
                    worksheet.Range["A1:I1"].CellStyle = banner_style;
                    var unlocks_list = await Helper.ConnectionDb().Table<Timeline>().Where(x => x.date == date_str).ToListAsync();
                    foreach (var unlock in unlocks_list)
                    {
                        //TIMELINE
                        int r = unlocks_list.IndexOf(unlock) + 2;
                        worksheet.Range[String.Format("A{0}", r.ToString())].Text = unlock.time;
                        worksheet.Range[String.Format("B{0}", r.ToString())].Text = utilities.FormatData(int.Parse(unlock.usage.ToString()));
                        worksheet.Range[String.Format("C{0}", r.ToString())].Number = unlock.unlocks;
                        worksheet.Range[String.Format("D{0}", r.ToString())].Number = unlock.battery;
                        worksheet.Range[String.Format("E{0}", r.ToString())].Text = unlock.battery_status == "charging" ? "yes" : "no";
                        worksheet.Range[String.Format("F{0}", r.ToString())].Text = unlock.wifi_status == "on" ? "on" : "off";
                        worksheet.Range[String.Format("G{0}", r.ToString())].Text = unlock.bluetooth_status == "on" ? "on" : "off";
                        worksheet.Range[String.Format("H{0}", r.ToString())].Number = Math.Round(unlock.latitude, 3);
                        worksheet.Range[String.Format("I{0}", r.ToString())].Number = Math.Round(unlock.longitude, 3);
                        worksheet.Range[String.Format("A{0}:I{0}", r.ToString())].CellStyle = table_style;
                    }
                    worksheet.Range["K1"].Text = "Total usage:";
                    worksheet.Range["K2"].Text = "Total unlocks:";
                    worksheet.Range["L1"].Text = utilities.FormatData(item.usage);
                    worksheet.Range["L2"].Number = item.unlocks;
                    worksheet.Range["K1:K2"].CellStyle = bold_style;
                    worksheet.Range["L1:L2"].CellStyle = overview_style;
                    worksheet.UsedRange.AutofitColumns();
                }
            }
            //WRITE DATA TO FIRST WORKSHEET
            IWorksheet front_worksheet = workbook.Worksheets[0];
            front_worksheet.Name = "Overview";

            front_worksheet.Range["A1:A11"].CellStyle = bold_style;

            front_worksheet.Range["A1"].Text = "TIME SENSE REPORT";
            front_worksheet.Range["A3"].Text = "Analysis range:";
            front_worksheet.Range["A4"].Text = "Creation Date:";
            front_worksheet.Range["A5"].Text = "App version:";
            front_worksheet.Range["A7"].Text = "Total usage:";
            front_worksheet.Range["A8"].Text = "Total unlocks:";
            front_worksheet.Range["A9"].Text = "Days analyzed:";
            front_worksheet.Range["A10"].Text = "Average usage:";
            front_worksheet.Range["A11"].Text = "Average unlocks:";

            front_worksheet.Range["B3"].Text = String.Format("{0} - {1}", utilities.shortdate_form.Format(start_date), utilities.shortdate_form.Format(end_date));
            front_worksheet.Range["B4"].Text = String.Format("{0} - {1}", utilities.shortdate_form.Format(DateTime.Now), utilities.longtime_form.Format(DateTime.Now));
            front_worksheet.Range["B5"].Text = "2.1.0.0";
            front_worksheet.Range["B7"].Text = utilities.FormatData(total_usage);
            front_worksheet.Range["B8"].Text = total_unlocks.ToString();
            front_worksheet.Range["B9"].Text = days.ToString();
            front_worksheet.Range["B10"].Text = utilities.FormatData((total_usage / days));
            front_worksheet.Range["B11"].Text = (total_unlocks / days).ToString();

            front_worksheet.Range["B3:B11"].CellStyle = overview_style;
            front_worksheet.UsedRange.AutofitColumns();

            await workbook.SaveAsAsync(export_file);
            // Closing the workbook.
            workbook.Close();
            // Dispose the Excel engine
            excel_engine.Dispose();

            //new MessageDialog("The Excel report has been sucesfully created").ShowAsync();
        }
    }
}
