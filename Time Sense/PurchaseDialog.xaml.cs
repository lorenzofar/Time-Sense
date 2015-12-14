using Stuff;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Time_Sense
{
    public sealed partial class PurchaseDialog : ContentDialog
    {
        public PurchaseDialog()
        {           
            this.InitializeComponent();
            SecondaryButtonText = utilities.loader.GetString(utilities.STATS.Values[settings.analysis_trial] == null ? "try_btn" : "buy_btn");
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
