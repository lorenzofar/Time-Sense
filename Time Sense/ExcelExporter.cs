using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Syncfusion.XlsIO;
using Windows.Storage.Pickers;
using Windows.Storage;
using Stuff;
using Windows.UI;

namespace Time_Sense
{
    public sealed class ExcelExporter
    {
        public static async Task CreateExcelReport()
        {
            var span_result = await new SpanDialog().ShowAsync();
            if (span_result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                FileSavePicker export_picker = new FileSavePicker();
                export_picker.DefaultFileExtension = ".xlsx";
                export_picker.FileTypeChoices.Add("Excel file", new List<string>() { ".xlsx" });
                StorageFile export_file = await export_picker.PickSaveFileAsync();
                if (export_file != null)
                {
                    DateTime start_date = App.range_start_date;
                    DateTime end_date = App.range_end_date;
                    int days = end_date.Subtract(start_date).Days;
                    //INITIALIZE EXCEL ENGINE AND CREATE WORKSHEETS
                    ExcelEngine excel_engine = new ExcelEngine();
                    IApplication application = excel_engine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2013;
                    IWorkbook workbook = application.Workbooks.Create(days+2);

                    IStyle bold_style = workbook.Styles.Add("BoldStyle");
                    bold_style.Color = Colors.Black;
                    bold_style.Font.Bold = true;

                    for (int i = days; i >=0; i--)
                    {
                        DateTime current_date = start_date.AddDays(i);
                        string date_str = utilities.shortdate_form.Format(current_date);
                        IWorksheet worksheet = workbook.Worksheets[i+1];
                        worksheet.Name = String.Format("{0}-{1}-{2}", current_date.Day, current_date.Month, current_date.Year);
                    }
                    //WRITE DATA TO FIRST WORKSHEET
                    IWorksheet front_worksheet = workbook.Worksheets[0];
                    front_worksheet.Name = "Overview";

                    //front_worksheet.Range["A1:A11"].CellStyle = bold_style;

                    front_worksheet.Range["A1"].Text = "TIME SENSE REPORT";
                    front_worksheet.Range["A3"].Text = "Analysis range:";
                    front_worksheet.Range["A4"].Text = "Creation Date:";
                    front_worksheet.Range["A5"].Text = "App version:";
                    front_worksheet.Range["A7"].Text = "Total usage:";
                    front_worksheet.Range["A8"].Text = "Total unlocks:";
                    front_worksheet.Range["A9"].Text = "Days analyzed:";
                    front_worksheet.Range["A10"].Text = "Usage average:";
                    front_worksheet.Range["A11"].Text = "Unlocks average:";

                    front_worksheet.AutofitColumn(1);

                    await workbook.SaveAsAsync(export_file);
                    // Closing the workbook.
                    workbook.Close();
                    // Dispose the Excel engine
                    excel_engine.Dispose();
                }
            }
        }
    }
}
