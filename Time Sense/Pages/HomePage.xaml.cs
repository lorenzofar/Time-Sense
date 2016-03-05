using GalaSoft.MvvmLight.Messaging;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Time_Sense
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.t_client.TrackPageView("Home Page");
            Messenger.Default.Send<MessageHelper.HomeMessage>(new MessageHelper.HomeMessage());
        }        
    }
}
