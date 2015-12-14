using Windows.UI.Xaml.Controls;

namespace Time_Sense
{
    public sealed partial class DateDialog : ContentDialog
    {
        public DateDialog()
        {
            this.InitializeComponent();
            calendar.SetDisplayDate(App.report_date.Date);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try { App.report_date = calendar.SelectedDates[0].Date; } catch {}
        }
    }
}
