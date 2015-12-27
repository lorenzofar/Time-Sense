using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stuff;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Store;
using Windows.Devices.Geolocation;
using Windows.Devices.Sms;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Time_Sense
{
    public sealed partial class MainPage : Page
    {
        public static TextBlock title;
        public static RadioButton home;
        bool control = false;

        LicenseInformation license;

        public static string parameter = null;

        public MainPage()
        {
            this.InitializeComponent();
            try { RegisterTaskLock(); } catch { }
            try { RegisterTaskUnlock(); } catch { }
            try { RegisterTaskAlert(); } catch { }
            title = app_title;
            home = home_btn;
            if (Windows.Foundation.Metadata.ApiInformation.IsEventPresent("Windows.Phone.UI.Input.HardwareButtons", "BackPressed"))
            {
                HideBar();
            }
            else
            {
                try { RegisterTaskTimer(); } catch { }
                try { RegisterTaskTimer2(); } catch { }
                try { RegisterTaskTimer3(); } catch { }
            }
            InitializeLocation();
            CheckDialogs();
        }

        private async void HideBar()
        {
            var view = ApplicationView.GetForCurrentView();
            var bar = StatusBar.GetForCurrentView();
            await bar.HideAsync();
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }
        

        private async void CheckDialogs()
        {
            await CheckUpdate();
            await CheckBattery();
        }

        private async Task CheckUpdate()
        {
            if (utilities.STATS.Values[settings.version] == null)
            {
                await new WelcomeDialog().ShowAsync();
                utilities.STATS.Values[settings.version] = "10";
            }
        }

        private async Task CheckBattery()
        {
            Windows.System.Power.EnergySaverStatus saver = Windows.System.Power.PowerManager.EnergySaverStatus;
            if (saver == Windows.System.Power.EnergySaverStatus.On && utilities.STATS.Values[settings.battery_dialog] == null)
            {
                App.t_client.TrackEvent("Battery dialog shown");
                await new BatterySaverDialog().ShowAsync();
            }
        }

        private async void InitializeLocation()
        {
            try
            {
                var accessStatus = await Geolocator.RequestAccessAsync();
            }
            catch { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                switch (e.Parameter.ToString())
                {
                    case "usage":
                        parameter = "true";
                        home_btn.IsChecked = false;
                        home_btn.IsChecked = true;
                        break;
                    case "timeline":
                        parameter = "true";
                        timeline_btn.IsChecked = false;
                        timeline_btn.IsChecked = true;
                        break;
                    case "report":
                        parameter = "true";
                        total_btn.IsChecked = false;
                        total_btn.IsChecked = true;
                        break;
                    case "settings":
                        parameter = "true";
                        settings_btn.IsChecked = false;
                        home_btn.IsChecked = false;
                        settings_btn.IsChecked = true;
                        break;
                }
            }
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (fr.SourcePageType != typeof(HomePage))
            {
                home_btn.IsChecked = true;
                splitview.IsPaneOpen = false;
                e.Handled = true;
            }
        }

        #region BACKGROUND TASK
        private async void RegisterTaskLock()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "timesense_unlock")
                {
                    return;
                }
            }
            var builder = new BackgroundTaskBuilder();
            builder.Name = "timesense_unlock";
            builder.TaskEntryPoint = "Tasks.controller_unlock";
            builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserPresent, false));
            BackgroundExecutionManager.RemoveAccess();
            BackgroundAccessStatus x = await BackgroundExecutionManager.RequestAccessAsync();
            if (x != BackgroundAccessStatus.Denied)
            {
                BackgroundTaskRegistration mytask = builder.Register();
                mytask.Completed += Mytask_Completed;
            }
        }

        private async void RegisterTaskUnlock()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "timesense_lock")
                {
                    return;
                }
            }
            var builder = new BackgroundTaskBuilder();
            builder.Name = "timesense_lock";
            builder.TaskEntryPoint = "Tasks.controller_lock";
            builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserAway, false));
            BackgroundExecutionManager.RemoveAccess();
            BackgroundAccessStatus x = await BackgroundExecutionManager.RequestAccessAsync();
            if (x != BackgroundAccessStatus.Denied)
            {
                BackgroundTaskRegistration mytask = builder.Register();
                mytask.Completed += Mytask_Completed;
            }
        }

        private async void RegisterTaskAlert()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "timesense_alert")
                {
                    return;
                }
            }
            var builder = new BackgroundTaskBuilder();
            builder.Name = "timesense_alert";
            builder.TaskEntryPoint = "Tasks.alert";
            builder.SetTrigger(new ToastNotificationActionTrigger());
            BackgroundExecutionManager.RemoveAccess();
            BackgroundAccessStatus x = await BackgroundExecutionManager.RequestAccessAsync();
            if (x != BackgroundAccessStatus.Denied)
            {
                BackgroundTaskRegistration mytask = builder.Register();
                mytask.Completed += Mytask_Completed;
            }
        }
        #region TIMER TASKS
        private async void RegisterTaskTimer()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "timesense_timer")
                {
                    return;
                }
            }
            var builder = new BackgroundTaskBuilder();
            builder.Name = "timesense_timer";
            builder.TaskEntryPoint = "Tasks.timer_task";
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.CancelOnConditionLoss = true;
            SystemCondition user_present_condition = new SystemCondition(SystemConditionType.UserPresent);
            builder.AddCondition(user_present_condition);
            BackgroundExecutionManager.RemoveAccess();
            BackgroundAccessStatus x = await BackgroundExecutionManager.RequestAccessAsync();
            if (x != BackgroundAccessStatus.Denied)
            {
                BackgroundTaskRegistration mytask = builder.Register();
                mytask.Completed += Mytask_Completed;
            }
        }

        private async void RegisterTaskTimer2()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "timesense_timer2")
                {
                    return;
                }
            }
            var builder = new BackgroundTaskBuilder();
            builder.Name = "timesense_timer2";
            builder.TaskEntryPoint = "Tasks.timer_task";
            builder.SetTrigger(new TimeTrigger(20, false));
            builder.CancelOnConditionLoss = true;
            SystemCondition user_present_condition = new SystemCondition(SystemConditionType.UserPresent);
            builder.AddCondition(user_present_condition);
            BackgroundExecutionManager.RemoveAccess();
            BackgroundAccessStatus x = await BackgroundExecutionManager.RequestAccessAsync();
            if (x != BackgroundAccessStatus.Denied)
            {
                BackgroundTaskRegistration mytask = builder.Register();
                mytask.Completed += Mytask_Completed;
            }
        }

        private async void RegisterTaskTimer3()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "timesense_timer3")
                {
                    return;
                }
            }
            var builder = new BackgroundTaskBuilder();
            builder.Name = "timesense_timer3";
            builder.TaskEntryPoint = "Tasks.timer_task";
            builder.SetTrigger(new TimeTrigger(25, false));
            builder.CancelOnConditionLoss = true;
            SystemCondition user_present_condition = new SystemCondition(SystemConditionType.UserPresent);
            builder.AddCondition(user_present_condition);
            BackgroundExecutionManager.RemoveAccess();
            BackgroundAccessStatus x = await BackgroundExecutionManager.RequestAccessAsync();
            if (x != BackgroundAccessStatus.Denied)
            {
                BackgroundTaskRegistration mytask = builder.Register();
                mytask.Completed += Mytask_Completed;
            }
        }
        #endregion

        private void RegisterTaskSMS()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "timesense_sms")
                {
                    return;
                }
            }
            try
            {
                SmsMessageType messageType = SmsMessageType.Text; // set as Text as default

                // Create new filter rule (individual)
                SmsFilterRule filter = new SmsFilterRule(messageType);

                // Set filter action type
                SmsFilterActionType actionType = SmsFilterActionType.Peek; // set as Accept as default

                // Created set of filters for this application
                SmsFilterRules filterRules = new SmsFilterRules(actionType);
                IList<SmsFilterRule> rules = filterRules.Rules;
                rules.Add(filter);

                // Create a new background task builder.
                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();

                // Create a new SmsMessageReceivedTrigger.
                SmsMessageReceivedTrigger trigger = new SmsMessageReceivedTrigger(filterRules);

                // Associate the SmsReceived trigger with the background task builder.
                taskBuilder.SetTrigger(trigger);

                // Specify the background task to run when the trigger fires.
                taskBuilder.TaskEntryPoint = "Tasks.smsreceived";

                // Name the background task.
                taskBuilder.Name = "timesense_sms";

                // Register the background task.
                BackgroundTaskRegistration taskRegistration = taskBuilder.Register();
            }
            catch { }
        }

        private void Mytask_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.t_client.TrackEvent("Hamburger_btn");
            splitview.IsPaneOpen = !splitview.IsPaneOpen;
        }

        private async void nav_btn_checked(object sender, RoutedEventArgs e)
        {
            RadioButton btn = (RadioButton)sender;
            switch (btn.Name)
            {
                case "home_btn":
                    fr.Navigate(typeof(HomePage), parameter);
                    break;
                case "timeline_btn":
                    fr.Navigate(typeof(TimelinePage), parameter);
                    break;
                case "settings_btn":
                    fr.Navigate(typeof(SettingsPage), null);
                    break;
                case "about_btn":
                    break;
                case "total_btn":
                    fr.Navigate(typeof(ReportPage), null);
                    break;
                case "export_btn":
                    fr.Navigate(typeof(ExportPage), null);
                    break;
                case "analytics_btn":
                    try
                    {
                        license = CurrentApp.LicenseInformation;
                        if (license.ProductLicenses["ts_analytics"].IsActive)
                        {
                            fr.Navigate(typeof(AnalyticsPage), null);
                        }
                        else
                        {                            
                            var result = await new PurchaseDialog().ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                try
                                {
                                    //TRY TO CONTACT THE STORE  
                                    PurchaseResults p = await CurrentApp.RequestProductPurchaseAsync("ts_analytics");
                                    if (p.Status == ProductPurchaseStatus.AlreadyPurchased || p.Status == ProductPurchaseStatus.Succeeded)
                                    {
                                        fr.Navigate(typeof(AnalyticsPage), null);
                                    }
                                    else
                                    {
                                        home_btn.IsChecked = true;
                                        analytics_btn.IsChecked = false;
                                    }
                                    if (license.ProductLicenses["ts_analytics"].IsActive)
                                    {
                                        fr.Navigate(typeof(AnalyticsPage), null);
                                    }
                                    else
                                    {
                                        home_btn.IsChecked = true;
                                        analytics_btn.IsChecked = false;
                                    }
                                }
                                catch
                                {
                                    await new MessageDialog(utilities.loader.GetString("error_transaction"), utilities.loader.GetString("error")).ShowAsync();
                                    home_btn.IsChecked = true;
                                    analytics_btn.IsChecked = false;
                                }
                            }
                            else
                            {
                                if (utilities.STATS.Values[settings.analysis_trial] == null)
                                {
                                    utilities.STATS.Values[settings.analysis_trial] = "tried";
                                    fr.Navigate(typeof(AnalyticsPage), null);
                                }
                                else
                                {
                                    home_btn.IsChecked = true;
                                    analytics_btn.IsChecked = false;
                                }
                            }
                        }
                    }
                    catch
                    {
                        await new MessageDialog(utilities.loader.GetString("error_transaction_internet"), utilities.loader.GetString("error")).ShowAsync();
                        home_btn.IsChecked = true;
                        analytics_btn.IsChecked = false;
                        home_btn.IsChecked = true;
                        analytics_btn.IsChecked = false;
                    }
                    break;
            }
            try
            {
                if (vs_group.CurrentState.Name != "wide")
                {
                    splitview.IsPaneOpen = false;
                }
            }
            catch { }
        }


        #region KEYBOARD SUPPORT
        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Tab:
                    splitview.IsPaneOpen = !splitview.IsPaneOpen;
                    break;
                case Windows.System.VirtualKey.Control:
                    control = true;
                    break;
                case Windows.System.VirtualKey.Number1:
                    if (control)
                    {
                        home_btn.IsChecked = true;
                    }
                    break;
                case Windows.System.VirtualKey.Number2:
                    if (control)
                    {
                        timeline_btn.IsChecked = true;
                    }
                    break;
                case Windows.System.VirtualKey.Number3:
                    if (control)
                    {
                        total_btn.IsChecked = true;
                    }
                    break;
                case Windows.System.VirtualKey.Number4:
                    if (control)
                    {
                        analytics_btn.IsChecked = true;
                    }
                    break;
                case Windows.System.VirtualKey.Number5:
                    if (control)
                    {
                        settings_btn.IsChecked = true;
                    }
                    break;
            }
        }

        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control)
            {
                control = false;
            }
        }
        #endregion
    }
}
