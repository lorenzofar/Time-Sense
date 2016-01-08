using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Time_Sense
{
    public sealed partial class ContactDialog : ContentDialog
    {
        public ContactDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            /*if (!string.IsNullOrWhiteSpace(name.Text))
            {
                SettingsPage.c = new Database.AllowedContact
                {
                    name = name.Text,
                    number = number.Text
                };
            }
            else
            {
                args.Cancel = true;
                name.Focus(FocusState.Programmatic);
                MessageDialog error = new MessageDialog("You must insert a valid name", "Error");
                await error.ShowAsync();
            }*/
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
