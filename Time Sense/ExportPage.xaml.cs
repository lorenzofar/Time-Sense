using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Syncfusion.XlsIO;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Time_Sense
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExportPage : Page
    {
        public ExportPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ExcelEngine excel_engine = new ExcelEngine();
            IApplication application = excel_engine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];
            //Adding text data
            worksheet.Range["A1"].Text = "Month";
            worksheet.Range["B1"].Text = "Sales";
            worksheet.Range["A6"].Text = "Total";
            //Adding DateTime data
            worksheet.Range["A2"].DateTime = new DateTime(2015, 1, 10);
            worksheet.Range["A3"].DateTime = new DateTime(2015, 2, 10);
            worksheet.Range["A4"].DateTime = new DateTime(2015, 3, 10);
            //Applying number format for date value cells A2 to A4
            worksheet.Range["A2:A4"].NumberFormat = "mmmm, yyyy";
            //Auto-size the first column to fit the content
            worksheet.AutofitColumn(1);
            //Adding numeric data
            worksheet.Range["B2"].Number = 68878;
            worksheet.Range["B3"].Number = 71550;
            worksheet.Range["B4"].Number = 72808;
            //Adding formula
            worksheet.Range["B6"].Formula = "SUM(B2:B4)";
            StorageFile doc = await KnownFolders.VideosLibrary.CreateFileAsync("timesense_export_test.xlsx", CreationCollisionOption.ReplaceExisting);
            await workbook.SaveAsAsync(doc);
            // Closing the workbook.
            workbook.Close();
            // Dispose the Excel engine
            excel_engine.Dispose();

        }

        private void save_btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
