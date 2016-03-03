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

        /*public async void ShowData()
        {          
            //BATTERY DATA
            if (Windows.Devices.Power.Battery.AggregateBattery.GetReport().Status == Windows.System.Power.BatteryStatus.Charging)
            {
                ts.notch_panel = Visibility.Collapsed;
                ts.ch_panel = Visibility.Visible;
            }
            else
            {
                ts.notch_panel = Visibility.Visible;
                ts.ch_panel = Visibility.Collapsed;
                var list = await Helper.ConnectionDb().Table<Timeline>().ToListAsync();
                int list_count = list.Count - 1;
                int batt_time = 0;
                int battery = 0;
                int batt_unlocks = 0;
                if (list_count > 0)
                {
                    do
                    {
                        try
                        {
                            var item = list.ElementAt(list_count);
                            if (item != null && item.battery_status != "charging")
                            {
                                batt_time += int.Parse(item.usage.ToString());
                                batt_unlocks++;
                                battery = item.battery;
                            }
                        }
                        catch (Exception e)
                        {
                            App.t_client.TrackException(new ExceptionTelemetry(e));
                        }
                        finally
                        {
                            list_count--;
                        }
                    }
                    while (CheckBattery(list, battery, list_count));
                }
                else
                {
                    batt_time = time[1];
                    batt_unlocks = unlocks[1];
                }
                //ts.battery_usage = FormatData(batt_time);
                ts.battery_unlocks = batt_unlocks == 1 ? String.Format(utilities.loader.GetString("unlock"), batt_unlocks) : String.Format(utilities.loader.GetString("unlocks"), batt_unlocks);
            }

            if (App.report_date.Date == DateTime.Now.Date)
            {
                UpdateTile();
            }

            await Task.Delay(1);
            MainPage.title.Text = App.report_date.Date == DateTime.Now.Date ? utilities.loader.GetString("today") : App.report_date.Date == DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0, 0)).Date ? utilities.loader.GetString("yesterday") : utilities.shortdate_form.Format(App.report_date);
            this.DataContext = ts;
        }

        private bool CheckBattery(List<Timeline> list, int battery, int count )
        {
            count = count--;
            if (count >= 0)
            {
                var item = list.ElementAt(count);
                if (item != null)
                {
                    if (item.battery < battery || item.battery_status == "charging") { return false; }
                    else { return true; }
                } else { return false; }
            } else { return false; }
        }

        private void UpdateTile()
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
