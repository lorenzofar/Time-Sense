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
using GalaSoft.MvvmLight.Messaging;

namespace Time_Sense
{
    public sealed partial class TimelinePage : Page
    {
        public TimelinePage()
        {
            this.InitializeComponent();
            Messenger.Default.Register<MessageHelper.ShowMapMessage>(this, message => {
                ShowMap(message.unlock);
            });
            Messenger.Default.Register<MessageHelper.CloseMapMessage>(this, message =>
            {
                map_close.Begin();
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Messenger.Default.Send<MessageHelper.TimelineMessage>(new MessageHelper.TimelineMessage());
        }

        private void ShowMap(Timeline unlock)
        {
            if (unlock.latitude != 0 || unlock.longitude != 0)
            {
                map_row.Height = new GridLength(1, GridUnitType.Auto);
                map_open.Begin();
                BasicGeoposition pos = new BasicGeoposition() { Latitude = double.Parse(unlock.latitude.ToString()), Longitude = double.Parse(unlock.longitude.ToString()) };
                Geopoint point = new Geopoint(pos);
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

        private void DoubleAnimation_Completed(object sender, object e)
        {
            map_row.Height = new GridLength(0, GridUnitType.Star);
        }
    }
}
