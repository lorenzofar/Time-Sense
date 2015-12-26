using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Stuff;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Time_Sense
{
    public sealed partial class ReportPage : Page
    {
        public class Stats
        {
            public DateTime date_raw { get; set; }
            public string date { get; set; }
            public double usage { get; set; }
            public int unlocks { get; set; }
        }

        public class ts_data
        {
            public string usage { get; set; }
            public string unlocks { get; set; }
            public string usage_max { get; set; }
            public string usage_min { get; set; }
            public string usage_avg { get; set; }
            public string unlocks_max { get; set; }
            public string unlocks_min { get; set; }
            public string unlocks_avg { get; set; }
            public string usage_max_date { get; set; }
            public string usage_min_date { get; set; }
            public string usage_avg_date { get; set; }
            public string unlocks_max_date { get; set; }
            public string unlocks_min_date { get; set; }
            public string unlocks_avg_date { get; set; }
            public List<Stats> r_list { get; set; }
        }
        
        public List<Report> list = new List<Report>();
        public List<Stats> stats_list = new List<Stats>();
        public static DateTime startDate = new DateTime();
        public static DateTime endDate = new DateTime();

        public ReportPage()
        {
            this.InitializeComponent();
            App.report_date = DateTime.Now;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.t_client.TrackPageView("Report page");
            LoadData(6, DateTime.Now.Subtract(new TimeSpan(6, 0, 0, 0, 0)));
        }

        #region CONTEXT MENU
        private async void range_btn_Click_1(object sender, RoutedEventArgs e)
        {
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand(utilities.loader.GetString("range_7"), (command) =>
            {
                App.t_client.TrackEvent("7 days report");
                LoadData(6, DateTime.Now.Subtract(new TimeSpan(6, 0, 0, 0, 0)));
            }));
            menu.Commands.Add(new UICommand(utilities.loader.GetString("range_14"), (command) =>
            {
                App.t_client.TrackEvent("14 days report");
                LoadData(13, DateTime.Now.Subtract(new TimeSpan(13, 0, 0, 0, 0)));
            }));
            menu.Commands.Add(new UICommand(utilities.loader.GetString("range_30"), (command) =>
            {
                App.t_client.TrackEvent("30 days report");
                LoadData(29, DateTime.Now.Subtract(new TimeSpan(29,0,0,0,0)));
            }));
            menu.Commands.Add(new UICommand(utilities.loader.GetString("range_all"), (command) =>
            {
                App.t_client.TrackEvent("All data report");
                LoadData(0, DateTime.Now);
            }));
            var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));            
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }
        #endregion

        private async void LoadData(int span, DateTime startDate)
        {
            root.Visibility = Visibility.Collapsed;
            ring.IsActive = true;
            range_btn.IsEnabled = false;
            ring_box.Visibility = Visibility.Visible;
            int usage = 0;
            int unlocks = 0;
            int time_max = 0;
            int time_min = 86400;
            int unlocks_max = 0;
            int unlocks_min = 10000;
            int avg_t = 0;
            double avg_u = 0;
            string time_max_date = "---";
            string time_min_date = "---";
            string unlocks_max_date = "---";
            string unlocks_min_date = "---";
            list.Clear();
            stats_list.Clear();
            if (span == 0)
            {
                list = await Helper.LoadAllData();
                foreach (var item in list)
                {
                    usage += item.usage;
                    unlocks += item.unlocks;
                    if (item.unlocks > unlocks_max)
                    {
                        unlocks_max = item.unlocks;
                        unlocks_max_date = item.date;
                    }
                    if (item.usage > time_max)
                    {
                        time_max = item.usage;
                        time_max_date = item.date;
                    }
                    if (item.unlocks < unlocks_min && item.unlocks != 0)
                    {
                        unlocks_min = item.unlocks;
                        unlocks_min_date = item.date;
                    }
                    if (item.usage < time_min && item.usage != 0)
                    {
                        time_min = item.usage;
                        time_min_date = item.date;
                    }
                    avg_t++;
                    avg_u++;
                    double s_in_h = 3600;
                    DateTime dt = new DateTime(2000, 1, 2);
                    DateTime dt_control = new DateTime(2000, 1, 1);
                    try {
                        string[] date_str = item.date.Split('/');
                        int y = 0;
                        int m = 0;
                        int d = 0;
                        d = (int.Parse(date_str[0][1].ToString()))*10 + int.Parse(date_str[0][2].ToString());
                        m = (int.Parse(date_str[1][1].ToString())) * 10 + int.Parse(date_str[1][2].ToString());
                        y = (int.Parse(date_str[2][1].ToString())) * 1000 + int.Parse(date_str[2][2].ToString())*100 + int.Parse(date_str[2][3].ToString())*10 + int.Parse(date_str[2][4].ToString());
                        dt = new DateTime(y, m, d);
                        Stats s = new Stats
                        {
                            date = item.date,
                            date_raw = dt,
                            unlocks = item.unlocks,
                            usage = double.Parse(item.usage.ToString()) / s_in_h
                        };
                        s.usage = Math.Round(s.usage, 1);
                        stats_list.Add(s);
                    }
                    catch { }
                }
                stats_list = stats_list.OrderBy(z => z.date_raw).ToList();
                startDate = stats_list.First().date_raw.Date;
                endDate = stats_list.Last().date_raw.Date;
            }
            else
            {
                for (int i = 0; i <= span; i++)
                {
                    DateTime dt = startDate.AddDays(i);
                    int[] data = await Database.Helper.LoadReportItem(dt);
                    usage += data[0];
                    unlocks += data[1];
                    if (data[1] > unlocks_max)
                    {
                        unlocks_max = data[1];
                        unlocks_max_date = utilities.shortdate_form.Format(dt);
                    }
                    if (data[0] > time_max)
                    {
                        time_max = data[0];
                        time_max_date = utilities.shortdate_form.Format(dt);
                    }
                    if (data[1] < unlocks_min && data[1] != 0)
                    {
                        unlocks_min = data[1];
                        unlocks_min_date = utilities.shortdate_form.Format(dt);
                    }
                    if (data[0] < time_min && data[0] != 0)
                    {
                        time_min = data[0];
                        time_min_date = utilities.shortdate_form.Format(dt);
                    }
                    avg_t++;
                    avg_u++;
                    double s_in_h = 3600;
                    Stats item = new Stats
                    {
                        date = utilities.shortdate_form.Format(dt),
                        usage = double.Parse(data[0].ToString()) / s_in_h,
                        unlocks = data[1]
                    };
                    item.usage = Math.Round(item.usage, 1);
                    stats_list.Add(item);
                    endDate = startDate.AddDays(span);
                }
            }
            ts_data ts = new ts_data();
            ts.usage = FormatData(usage);
            ts.unlocks = unlocks == 1 ? String.Format(utilities.loader.GetString("unlock"), unlocks) : String.Format(utilities.loader.GetString("unlocks"), unlocks);
            ts.usage_max = time_max == 0 ? "---" : FormatData(time_max);
            ts.usage_max_date = time_max_date;
            ts.unlocks_max = unlocks_max == 0 ? "---" : unlocks_max.ToString();
            ts.unlocks_max_date = unlocks_max_date;
            ts.usage_min = time_min == 86400 ? "---" : FormatData(time_min);
            ts.usage_min_date = time_min_date;
            ts.unlocks_min = unlocks_min == 10000 ? "---" : unlocks_min.ToString();
            ts.usage_avg = avg_t != 0 ? FormatData(usage/avg_t) : "---";
            double unlocks_avg = unlocks / avg_u;
            ts.unlocks_avg = Math.Round(unlocks_avg, 2).ToString();
            ts.usage_avg_date = avg_t == 1 ? String.Format(utilities.loader.GetString("avg_day"), avg_t) : String.Format(utilities.loader.GetString("avg_days"), avg_t);
            ts.unlocks_avg_date = avg_u == 1 ? String.Format(utilities.loader.GetString("avg_day"), avg_u) : String.Format(utilities.loader.GetString("avg_days"), avg_u);
            ts.unlocks_min_date = unlocks_min_date;
            ts.r_list = stats_list;
            this.DataContext = ts;
            MainPage.title.Text = utilities.shortdate_form.Format(startDate) + " - " + utilities.shortdate_form.Format(endDate);
            root.Visibility = Visibility.Visible;
            ring.IsActive = false;
            ring_box.Visibility = Visibility.Collapsed;
            range_btn.IsEnabled = true;
        }

        private String FormatData(int usage)
        {
            int hour = usage / 3600;
            int minutes = (usage - (hour * 3600)) / 60;
            int seconds = (usage - (hour * 3600)) - (minutes * 60);
            string letter = utilities.STATS.Values[settings.letters] == null ? "{0}:{1}:{2}" : "{0}h:{1}m:{2}s";
            return String.Format(letter, hour, minutes, seconds);
        }

        private void statistics_chart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.t_client.TrackEvent("Report chart swipe");
            chart_helper.Text = statistics_chart.SelectedIndex == 0 ? utilities.loader.GetString("hours_report") : utilities.loader.GetString("unlocks_tile");
        }
    }
}
