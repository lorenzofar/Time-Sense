using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Database;
using GalaSoft.MvvmLight.Command;
using Stuff;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI;
using GalaSoft.MvvmLight.Messaging;

namespace Time_Sense.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        private static string[] pivot_strings = { "usage_chart_title", "battery_chart_title" };

        public TimelineViewModel()
        {
            Messenger.Default.Register<MessageHelper.TimelineMessage>(this, message => {
                Refresh();
            });
        }

        private List<Timeline> _timelineList;
        public List<Timeline> timelineList
        {
            get
            {
                return _timelineList;
            }
            set
            {
                Set(ref _timelineList, value);
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

        private int _listIndex;
        public int listIndex
        {
            get
            {
                return _listIndex;
            }
            set
            {
                Set(ref _listIndex, value);
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

        #region COMMANDS
        private RelayCommand _RefreshData;
        public RelayCommand RefreshData
        {
            get
            {
                if(_RefreshData == null)
                {
                    _RefreshData = new RelayCommand(() =>
                    {
                        Refresh();
                    });
                }
                return _RefreshData;
            }
        }

        private RelayCommand<object> _ChangeDate;
        public RelayCommand<object> ChangeDate
        {
            get
            {
                if (_ChangeDate == null)
                {
                    _ChangeDate = new RelayCommand<object>((object parameter) =>
                    {
                        int days = int.Parse(parameter.ToString());
                        App.report_date = App.report_date.AddDays(days);
                        Refresh();
                    });
                }
                return _ChangeDate;
            }
        }

        private RelayCommand _PickDate;
        public RelayCommand PickDate
        {
            get
            {
                if (_PickDate == null)
                {
                    _PickDate = new RelayCommand(async () => {
                        if (await new DateDialog().ShowAsync() == ContentDialogResult.Primary)
                        {
                            Refresh();
                        }
                    });
                }
                return _PickDate;
            }
        }

        private RelayCommand<object> _ItemClick;
        public RelayCommand<object> ItemClick
        {
            get
            {
                if(_ItemClick == null)
                {
                    _ItemClick = new RelayCommand<object>((e) =>
                    {
                        var args = e as ItemClickEventArgs;
                        var unlock = args.ClickedItem as Timeline;
                        if (unlock != null)
                        {
                            Messenger.Default.Send<MessageHelper.ShowMapMessage>(new MessageHelper.ShowMapMessage() { unlock = unlock });
                        }
                    });
                }
                return _ItemClick;
            }
        }

        private RelayCommand _CloseMap;
        public RelayCommand CloseMap
        {
            get
            {
                if(_CloseMap == null)
                {
                    _CloseMap = new RelayCommand(() =>
                    {
                        Messenger.Default.Send<MessageHelper.CloseMapMessage>(new MessageHelper.CloseMapMessage());
                    });
                }
                return _CloseMap;
            }
        }
        #endregion


        private async void Refresh()
        {
            loading = true;
            string date_str = utilities.shortdate_form.Format(App.report_date);
            timelineList = await Helper.ConnectionDb().Table<Timeline>().Where(x => x.date == date_str).ToListAsync();
            MainPage.title.Text = App.report_date.Date == DateTime.Now.Date ? utilities.loader.GetString("today") : App.report_date.Date == DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0, 0)).Date ? utilities.loader.GetString("yesterday") : utilities.shortdate_form.Format(App.report_date);
            loading = false;
        }

        public UIElement Marker()
        {
            Canvas marker = new Canvas();
            object brush = Application.Current.Resources["SystemControlBackgroundAccentBrush"];
            Ellipse outer = new Ellipse() { Width = 25, Height = 25 };
            outer.Fill = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
            outer.Margin = new Thickness(-12.5, -12.5, 0, 0);
            Ellipse inner = new Ellipse() { Width = 20, Height = 20 };
            inner.Fill = brush as SolidColorBrush;
            inner.Margin = new Thickness(-10, -10, 0, 0);
            Ellipse core = new Ellipse() { Width = 10, Height = 10 };
            core.Fill = new SolidColorBrush(Colors.White);
            core.Margin = new Thickness(-5, -5, 0, 0);
            marker.Children.Add(outer);
            marker.Children.Add(inner);
            marker.Children.Add(core);
            return marker;
        }
    }
}
