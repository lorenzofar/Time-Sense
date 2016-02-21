using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Stuff;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using System.Threading.Tasks;

namespace Time_Sense
{
    public sealed partial class TimelinePage : Page
    {
        public List<Timeline> list_raw = new List<Timeline>();
        public List<Timeline> list = new List<Timeline>();

        public Pointer pointer { get; set; }

        private class ts_data
        {
            public List<Timeline> t_list { get; set; }
            public Visibility no_item { get; set; }
        }

        public TimelinePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.t_client.TrackPageView("Timeline page");
            LoadData();
        }

        private async void LoadData()
        {
            Button_switch(false);
            timeline_list.ItemsSource = null;
            timeline_list.Visibility = Visibility.Collapsed;
            ring_box.Visibility = Visibility.Visible;
            ring.IsActive = true;
            list_raw = await Helper.GetTimelineList(App.report_date);
            list_raw = list_raw.OrderBy(z => z.unlocks).ToList();
            await Task.Delay(1);
            this.DataContext = new ts_data
            {
                t_list = list_raw,
                no_item = list_raw.Count == 0 ? Visibility.Visible : Visibility.Collapsed
            };
            ring.IsActive = false;
            ring_box.Visibility = Visibility.Collapsed;
            timeline_list.Visibility = Visibility.Visible;
            timeline_list.ItemsSource = list_raw;
            MainPage.title.Text = App.report_date.Date == DateTime.Now.Date ? utilities.loader.GetString("today") : App.report_date.Date == DateTime.Now.Subtract(new TimeSpan(1,0,0,0,0)).Date ? utilities.loader.GetString("yesterday") : utilities.shortdate_form.Format(App.report_date);            
            Button_switch(true);
        }

        private async void date_timeline_bar_Click(object sender, RoutedEventArgs e)
        {
            App.t_client.TrackEvent("Calendar shown");
            if ((await new DateDialog().ShowAsync()) == ContentDialogResult.Primary)
            {
                LoadData();
            }
        }
        private void forward_timeline_bar_Click(object sender, RoutedEventArgs e)
        {
            App.t_client.TrackEvent("Next day");
            App.report_date = App.report_date.AddDays(1);
            LoadData();
        }

        private void back_timeline_bar_Click(object sender, RoutedEventArgs e)
        {
            App.t_client.TrackEvent("Previous day");
            App.report_date = App.report_date.Subtract(new TimeSpan(1, 0, 0, 0));
            LoadData();
        }

        private void refresh_timeline_bar_Click(object sender, RoutedEventArgs e)
        {
            App.t_client.TrackEvent("Timeline refreshed");
            LoadData();
        }

        private void chart_pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.t_client.TrackEvent("Timeline chart swipe");
            charts_header.Text = chart_pivot.SelectedIndex == 0 ? utilities.loader.GetString("usage_chart_title") : utilities.loader.GetString("battery_chart_title");
        }

        private void timeline_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (Timeline)timeline_list.SelectedValue;
            if (item != null)
            {
                if (item.latitude != 0 || item.longitude != 0)
                {
                    App.t_client.TrackEvent("Map shown");
                    map_row.Height = new GridLength(1, GridUnitType.Auto);
                    map_open.Begin();
                    BasicGeoposition pos = new BasicGeoposition() { Latitude = double.Parse(item.latitude.ToString()), Longitude = double.Parse(item.longitude.ToString()) };
                    Geopoint point = new Geopoint(pos);
                    map.PedestrianFeaturesVisible = true;
                    map.ManipulationMode = ManipulationModes.All;
                    DependencyObject marker = Marker();
                    map.Children.Clear();
                    map.Children.Add(marker);
                    Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(marker, point);
                    Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(marker, new Point(0.5, 0.5));
                    map.ZoomLevel = 16;
                    map.Center = point;
                }
                else
                {
                    map_row.Height = new GridLength(0, GridUnitType.Star);
                }
            }
            timeline_list.SelectedItem = null;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            map_close.Begin();
        }

        public UIElement Marker()
        {
            Canvas marker = new Canvas();
            object brush = Windows.UI.Xaml.Application.Current.Resources["SystemControlBackgroundAccentBrush"];
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

        private void DoubleAnimation_Completed(object sender, object e)
        {
            map_row.Height = new GridLength(0, GridUnitType.Star);
        }

        public void Button_switch(bool enabled)
        {
            bottom_bar.IsEnabled = enabled;
        }
    }
}
