using Stuff;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Time_Sense
{
    public sealed partial class BatterySaverDialog : ContentDialog
    {
        public BatterySaverDialog()
        {
            this.InitializeComponent();
        }
        
        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void show_check_Checked(object sender, RoutedEventArgs e)
        {
            utilities.STATS.Values[settings.battery_dialog] = show_check.IsChecked == true ? "yes" : null;
        }
    }
}
