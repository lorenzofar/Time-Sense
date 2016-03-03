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
            Messenger.Default.Send<MessageHelper.HomeMessage>(new MessageHelper.HomeMessage());
        }

        /*private void UpdateTile()
        {
            // UPDATE THE TILE
            bool badge = utilities.STATS.Values[settings.unlocks] == null ? true : utilities.STATS.Values[settings.unlocks].ToString() == "badge" ? true : false;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(utilities.TileXmlBuilder(utilities.FormatData(time[1]), unlocks[1], badge));
            TileNotification tile = new TileNotification(doc);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tile);
            if (badge)
            {
                string badgeXmlString = "<badge value='" + unlocks[1].ToString() + "'/>";
                XmlDocument badgeDOM = new XmlDocument();
                badgeDOM.LoadXml(badgeXmlString);
                BadgeNotification badge_not = new BadgeNotification(badgeDOM);
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge_not);
            }
        }

        private void home_charts_pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.t_client.TrackEvent("Home pivot swipe");
            try
            {
                switch (home_charts_pivot.SelectedIndex)
                {
                    case 0:
                        home_chart_helper.Text = utilities.loader.GetString("usage_hour_title");
                        break;
                    case 1:
                        home_chart_helper.Text = utilities.loader.GetString("unlocks_hour_title");
                        break;
                    case 3:
                        home_chart_helper.Text = utilities.loader.GetString("charge_banner");
                        break;
                    case 2:
                        home_chart_helper.Text = utilities.loader.GetString("note_banner");
                        break;
                }
            }
            catch { }
        }*/      
        
    }
}
