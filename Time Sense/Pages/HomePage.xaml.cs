using Database;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.ApplicationInsights.DataContracts;
using Stuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
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
            List<Hour> hour_list = new List<Hour>();

            double time_helper = 0;
            double unlocks_helper = 0;
            double time_min_helper = 1000;
            double unlocks_min_helper = 1000;
            double time_total = 0;
            double unlocks_total = 0;
            double avg_t = 0;
            double avg_u = 0;
            foreach (var item in hour_list_raw)
            {
                item.usage = item.usage / 60;
                if (item.usage != 0)
                {
                    if (item.usage > time_helper)
                    {
                        time_helper = item.usage;
                    }
                    if (item.usage < time_min_helper && item.usage != 0)
                    {
                        time_min_helper = item.usage;
                    }
                    avg_t++;
                }
                if (item.unlocks != 0)
                {
                    if (item.unlocks > unlocks_helper)
                    {
                        unlocks_helper = item.unlocks;
                    }
                    if (item.unlocks < unlocks_min_helper && item.unlocks != 0)
                    {
                        unlocks_min_helper = item.unlocks;
                    }
                    avg_u++;
                }
                time_total += item.usage;
                unlocks_total += item.unlocks;
                hour_list.Add(item);
            }

            ts_data ts = new ts_data();

            ts.usage_max = time_helper == 0 ? "---" : time_helper.ToString();
            ts.unlocks_max = unlocks_helper == 0 ? "---" : unlocks_helper.ToString();
            ts.usage_min = time_min_helper == 1000 ? "---" : time_min_helper.ToString();
            ts.unlocks_min = unlocks_min_helper == 1000 ? "---" : unlocks_min_helper.ToString();
            ts.usage_avg = Math.Round((time_total / avg_t), 2).ToString();
            ts.unlocks_avg = Math.Round((unlocks_total / avg_u), 2).ToString();
            //ts.usage = FormatData(time[1]);
            ts.unlocks = unlocks[1] == 1 ? String.Format(utilities.loader.GetString("unlock"), unlocks[1]) : String.Format(utilities.loader.GetString("unlocks"), unlocks[1]);
            ts.perc_str = ((time[1] * 100) / 86400).ToString() + "%";
            ts.perc = time[1];
            ts.h_list = hour_list;

            //LOAD NOTE
            string date_str = utilities.shortdate_form.Format(App.report_date);
            var report = await Helper.ConnectionDb().Table<Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
            if (report != null)
            {
                ts.note = report.note == null ? "" : report.note;
                ts.nonote_panel = report.note == null || report.note == "" ? Visibility.Visible : Visibility.Collapsed;
                ts.note_panel = report.note == null || report.note == "" ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                ts.note = "";
                ts.nonote_panel = Visibility.Visible;
                ts.note_panel = Visibility.Collapsed;
            }

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
        }
        
        private async void export_bar_Click(object sender, RoutedEventArgs e)
        {
            try {
                var span_result = await new SpanDialog().ShowAsync();
                if (span_result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    if (App.range_start_date <= App.range_end_date)
                    {
                        FileSavePicker export_picker = new FileSavePicker();
                        export_picker.DefaultFileExtension = ".xlsx";
                        export_picker.SuggestedFileName = String.Format("timesense_{0}-{1}", utilities.shortdate_form.Format(App.range_start_date), utilities.shortdate_form.Format(App.range_end_date));
                        export_picker.FileTypeChoices.Add("Excel file", new List<string>() { ".xlsx" });
                        App.file_pick = true;
                        StorageFile export_file = await export_picker.PickSaveFileAsync();
                        if (export_file != null)
                        {
                            App.file_pick = false;
                            await new ProgressDialog(export_file).ShowAsync();
                            App.t_client.TrackEvent("Excel report created");
                        }
                    }
                    else
                    {
                        await new MessageDialog(utilities.loader.GetString("error_span"), utilities.loader.GetString("error")).ShowAsync();
                    }
                }
            }
            catch { }
        }*/
    }
}
