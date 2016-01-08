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
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Stuff;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Time_Sense
{
    public sealed partial class ProgressDialog : ContentDialog
    {
        public ProgressDialog(StorageFile export_file)
        {
            this.InitializeComponent();
            Export(export_file);
        }

        private async void Export(StorageFile export_file)
        {
            bool success = true;
            try
            {
                await ExcelExporter.CreateExcelReport(export_file);
            }
            catch
            {
                success = false;
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                new MessageDialog(success == true ? utilities.loader.GetString("excel_success") : utilities.loader.GetString("excel_error")).ShowAsync();
            });
            this.Hide();
        }
    }
}
