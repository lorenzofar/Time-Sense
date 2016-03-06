using GalaSoft.MvvmLight.Messaging;
using Stuff;
using System;
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
        public ReportPage()
        {
            this.InitializeComponent();
            Messenger.Default.Register<MessageHelper.ShowReportMessage>(this, message =>
            {
                ShowMenu();
            });
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.t_client.TrackPageView("Report page");
            Messenger.Default.Send<MessageHelper.ReportMessage>(new MessageHelper.ReportMessage());
        }

        #region CONTEXT MENU
        private async void ShowMenu()
        {
            try {
                var menu = new PopupMenu();
                menu.Commands.Add(new UICommand(utilities.loader.GetString("range_7"), (command) =>
                {
                    App.t_client.TrackEvent("7 days report");
                    Messenger.Default.Send<MessageHelper.LoadReportMessage>(new MessageHelper.LoadReportMessage() { days = 6 });
                }));
                menu.Commands.Add(new UICommand(utilities.loader.GetString("range_14"), (command) =>
                {
                    App.t_client.TrackEvent("14 days report");
                    Messenger.Default.Send<MessageHelper.LoadReportMessage>(new MessageHelper.LoadReportMessage() { days = 13 });
                }));
                menu.Commands.Add(new UICommand(utilities.loader.GetString("range_30"), (command) =>
                {
                    App.t_client.TrackEvent("30 days report");
                    Messenger.Default.Send<MessageHelper.LoadReportMessage>(new MessageHelper.LoadReportMessage() { days = 29 });
                }));
                menu.Commands.Add(new UICommand(utilities.loader.GetString("range_all"), (command) =>
                {
                    App.t_client.TrackEvent("All data report");
                    Messenger.Default.Send<MessageHelper.LoadReportMessage>(new MessageHelper.LoadReportMessage() { days = 0 });
                }));
                var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)range_btn));
            }
            catch(Exception ex) { App.t_client.TrackException(new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex)); }          
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }
        #endregion
    }
}
