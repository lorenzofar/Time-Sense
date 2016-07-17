using Database;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Stuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Time_Sense.ViewModels
{
    public class ReportViewModel : ViewModelBase
    {
        public class Stats
        {
            public DateTime date_raw { get; set; }
            public string date { get; set; }
            public double usage { get; set; }
            public int unlocks { get; set; }
        }

        public class ReportData
        {
            public int usage { get; set;}
            public int unlocks { get; set; }
            public int usage_max { get; set; }
            public int usage_min { get; set; }
            public int usage_avg { get; set; }
            public int unlocks_max { get; set; }
            public int unlocks_min { get; set; }
            public double unlocks_avg { get; set; }
            public string usage_max_date { get; set; }
            public string usage_min_date { get; set; }
            public string usage_avg_date { get; set; }
            public string unlocks_max_date { get; set; }
            public string unlocks_min_date { get; set; }
            public string unlocks_avg_date { get; set; }
        }

        public static DateTime startDate = new DateTime();
        public static DateTime endDate = new DateTime();

        private static string[] pivot_strings = { "hours_report", "unlocks_tile" };

        public ReportViewModel()
        {
            Messenger.Default.Register<MessageHelper.ReportMessage>(this, message =>
            {
                App.report_date = DateTime.Now;
                LoadData(6, DateTime.Now.Subtract(new TimeSpan(6, 0, 0, 0, 0)));
            });
            Messenger.Default.Register<MessageHelper.LoadReportMessage>(this, message =>
            {
                LoadData(message.days, DateTime.Now.Subtract(new TimeSpan(message.days, 0, 0, 0)));
            });
        }

        #region VARIABLES
        private List<Stats> _reportList;
        public List<Stats> reportList
        {
            get
            {
                return _reportList;
            }
            set
            {
                Set(ref _reportList, value);
            }
        }

        private ReportData _data;
        public ReportData data
        {
            get
            {
                return _data;
            }
            set
            {
                Set(ref _data, value);
            }
        }

        private bool _loading;
        public bool loading
        {
            get
            {
                return _loading;
            }
            set
            {
                Set(ref _loading, value);
            }
        }

        private int _pivotIndex = 0;
        public int pivotIndex
        {
            get
            {
                return _pivotIndex;
            }
            set
            {
                Set(ref _pivotIndex, value);
                pivotBanner = utilities.loader.GetString(pivot_strings[_pivotIndex]);
            }
        }

        private string _pivotBanner = utilities.loader.GetString(pivot_strings[0]);
        public string pivotBanner
        {
            get
            {
                return _pivotBanner;
            }
            set
            {
                Set(ref _pivotBanner, value);
            }
        }
        #endregion

        #region COMMANDS
        private RelayCommand _ChangeRange;
        public RelayCommand ChangeRange
        {
            get
            {
                if(_ChangeRange == null)
                {
                    _ChangeRange = new RelayCommand(() =>
                    {
                        Messenger.Default.Send<MessageHelper.ShowReportMessage>(new MessageHelper.ShowReportMessage());
                    });
                }
                return _ChangeRange;
            }
        }

        private RelayCommand _ExportData;
        public RelayCommand ExportData
        {
            get
            {
                if(_ExportData == null)
                {
                    _ExportData = new RelayCommand(() =>
                    {
                        Export();
                    });
                }
                return _ExportData;
            }
        }
        #endregion

        private async void LoadData(int span, DateTime startDt)
        {
            loading = true;
            await Task.Delay(100);
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
            var list = new List<Report>();
            var rawList = new List<Stats>();
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
                    try
                    {
                        string[] date_str = item.date.Split('/');
                        int y = 0;
                        int m = 0;
                        int d = 0;
                        d = (int.Parse(date_str[0][1].ToString())) * 10 + int.Parse(date_str[0][2].ToString());
                        m = (int.Parse(date_str[1][1].ToString())) * 10 + int.Parse(date_str[1][2].ToString());
                        y = (int.Parse(date_str[2][1].ToString())) * 1000 + int.Parse(date_str[2][2].ToString()) * 100 + int.Parse(date_str[2][3].ToString()) * 10 + int.Parse(date_str[2][4].ToString());
                        dt = new DateTime(y, m, d);
                        Stats s = new Stats
                        {
                            date = item.date,
                            date_raw = dt,
                            unlocks = item.unlocks,
                            usage = double.Parse(item.usage.ToString()) / s_in_h
                        };
                        s.usage = Math.Round(s.usage, 1);
                        rawList.Add(s);
                    }
                    catch { }
                }
                rawList = rawList.OrderBy(z => z.date_raw).ToList();
                startDate = rawList.First().date_raw.Date;
                endDate = rawList.Last().date_raw.Date;
            }
            else
            {
                for (int i = 0; i <= span; i++)
                {
                    DateTime dt = startDt.AddDays(i);
                    int[] data = await Helper.LoadReportItem(dt);
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
                    rawList.Add(item);
                    rawList = rawList.OrderBy(z => z.date_raw).ToList();
                    startDate = startDt;
                    endDate = startDt.AddDays(span);
                }
            }
            data = new ReportData
            {
                usage = usage,
                unlocks = unlocks,
                usage_max = time_max,
                usage_max_date = time_max_date,
                usage_min = time_min == 86400 ? 0 : time_min,
                usage_min_date = time_min_date,
                usage_avg = avg_t != 0 ? usage / avg_t : 0,
                usage_avg_date = String.Format(utilities.loader.GetString(avg_t == 1 ? "avg_day" : "avg_days"), avg_t),
                unlocks_max = unlocks_max,
                unlocks_max_date = unlocks_max_date,
                unlocks_min = unlocks_min == 10000 ? 0 : unlocks_min,
                unlocks_min_date = unlocks_min_date,
                unlocks_avg = Math.Round(unlocks / avg_u, 2),
                unlocks_avg_date = String.Format(utilities.loader.GetString(avg_u == 1 ? "avg_day" : "avg_days"), avg_u)
            };
            reportList = rawList;
            MainPage.title.Text = utilities.shortdate_form.Format(startDate) + " - " + utilities.shortdate_form.Format(endDate);
            loading = false;         
        }

        private String FormatData(int usage)
        {
            int hour = usage / 3600;
            int minutes = (usage - (hour * 3600)) / 60;
            int seconds = (usage - (hour * 3600)) - (minutes * 60);
            string letter = utilities.STATS.Values[settings.letters] == null ? "{0}:{1}:{2}" : "{0}h:{1}m:{2}s";
            return String.Format(letter, hour, minutes, seconds);
        }

        private async void Export()
        {
            try
            {
                var span_result = await new SpanDialog().ShowAsync();
                if (span_result == ContentDialogResult.Primary)
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
                        }
                    }
                    else
                    {
                        await new MessageDialog(utilities.loader.GetString("error_span"), utilities.loader.GetString("error")).ShowAsync();
                    }
                }
            }
            catch { }
        }
    }
}
