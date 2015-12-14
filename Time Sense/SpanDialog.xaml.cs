using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Time_Sense
{
    public sealed partial class SpanDialog : ContentDialog
    {
        public SpanDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SettingsPage.delete_date[0] = startPick.Date.DateTime;
            SettingsPage.delete_date[1] = endPick.Date.DateTime;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
