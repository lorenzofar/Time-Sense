using Database;
using NdefLibrary.Ndef;
using Stuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Email;
using Windows.Data.Xml.Dom;
using Windows.Networking.Proximity;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Time_Sense
{
    public sealed partial class SettingsPage : Page
    {
        public bool loading = false;
        //private List<AllowedContact> contacts_list = new List<AllowedContact>();
        ProximityDevice nfc_device;
        long messageId0;
        long subscriptionId;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainPage.title.Text = utilities.loader.GetString("settings");
            LoadSettings();
        }

        public async void SendMessage()
        {
            await ChatMessageManager.RegisterTransportAsync();
            ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();
            ChatMessage message = new ChatMessage();
        }

        private void LoadSettings()
        {
            loading = true;
            int limit = utilities.STATS.Values[settings.limit] == null ? 7200 : int.Parse(utilities.STATS.Values[settings.limit].ToString());
            location_switch.IsOn = utilities.STATS.Values[settings.location] == null ? true : utilities.STATS.Values[settings.location].ToString() == "on" ? true : false;
            string unlocks = utilities.STATS.Values[settings.unlocks] == null ? "badge" : utilities.STATS.Values[settings.unlocks].ToString();
            bool password = utilities.STATS.Values[settings.password] == null ? false : true;
            letters_switch.IsOn = utilities.STATS.Values[settings.letters] == null ? false : true;
            hello_switch.IsOn = utilities.STATS.Values[settings.windows_hello] == null ? false : true;
            auto_backup_switch.IsOn = utilities.STATS.Values[settings.automatic_backup] == null ? false : true;
            unlocks += "_radio";
            threshold_box.SelectedIndex = limit == 0 ? 5 : (limit/3600) - 1;
            password_switch.IsOn = password;
            if (password)
            {
                password_box.Text = utilities.STATS.Values[settings.password].ToString();
            }
            RadioButton radio_btn = root_panel.FindName(unlocks) as RadioButton;
            radio_btn.IsChecked = true;
            nfc_device = ProximityDevice.GetDefault();
            if(nfc_device != null)
            {
                nfc_box.IsEnabled = true;
                nfc_btn.IsEnabled = true;
                nfc_device.DeviceArrived += Nfc_device_DeviceArrived;
                nfc_device.DeviceDeparted += Nfc_device_DeviceDeparted;
                subscriptionId = nfc_device.SubscribeForMessage("NDEF", MessageReceivedHandler);
            }
            else
            {
                nfc_first.Content = utilities.loader.GetString("nfc_unavailable");
                nfc_box.IsEnabled = false;
                nfc_btn.IsEnabled = false;
            }
            //await Helper.InitializeDatabase();
            //contacts_list = await Helper.ConnectionDb().Table<AllowedContact>().ToListAsync();
            //sms_list.ItemsSource = null;
            //sms_list.ItemsSource = contacts_list;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            try
            {
                nfc_device.StopPublishingMessage(messageId0);
            }
            catch { }
        }


        private void threshold_box_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (threshold_box.SelectedIndex == 5)
            {
                //DISABLES NOTIFICATIONS AND REMOVES SCHEDULED ONES
                utilities.STATS.Values[settings.limit] = 0;
                try
                {
                    IReadOnlyList<ScheduledToastNotification> list = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
                    foreach (var toast in list)
                    {
                        ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
                    }
                }
                catch(Exception ex)
                {
                }
            }
            else
            {
                utilities.STATS.Values[settings.limit] = (threshold_box.SelectedIndex + 1) * 3600;
            }
            // GETS THE USAGE DATA AND THEN SCHEDULES THE TOAST NOTIFICATION
            RegisterUsageToast();
            
        }

        private async void RegisterUsageToast()
        {
            int[] data = await Helper.LoadReportItem(DateTime.Now);
            int time = data[0];
            int limit = (threshold_box.SelectedIndex + 1) * 3600;
            int span = limit - time;
            if (limit != 0 && span >= 0 && DateTime.Now.AddSeconds(span).Date == DateTime.Now.Date)
            {
                IReadOnlyList<ScheduledToastNotification> list = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
                foreach (var toast in list)
                {
                    ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
                }
                XmlDocument document = new XmlDocument();
                document.LoadXml(utilities.ToastUsageAlert(limit));
                ScheduledToastNotification scheduled_toast = new ScheduledToastNotification(document, DateTime.Now.AddSeconds(span)) { Tag = utilities.shortdate_form.Format(DateTime.Now) };
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduled_toast);
            }
        }

        private void location_switch_toggled(object sender, RoutedEventArgs e)
        {
            utilities.STATS.Values[settings.location] = location_switch.IsOn ? "on" : "off";
        }

        private void unlocks_radio_checked(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            string[] name = btn.Name.Split('_');
            utilities.STATS.Values[settings.unlocks] = name[0];
        }

        private async void password_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!password_box.IsEnabled)
            {
                password_box.IsEnabled = true;
                password_box.Focus(FocusState.Programmatic);
                password_icon.Glyph = "";
            }
            else
            {
                bool valid = true;
                try
                {
                    int a = int.Parse(password_box.Text);
                }
                catch
                {
                    valid = false;
                }
                if (!string.IsNullOrWhiteSpace(password_box.Text.ToString()) && valid)
                {
                    utilities.STATS.Values[settings.password] = password_box.Text.ToString();
                    password_box.IsEnabled = false;
                    password_icon.Glyph = "";
                }
                else
                {
                    await new MessageDialog(utilities.loader.GetString("password_error"), utilities.loader.GetString("error")).ShowAsync();
                    password_box.Focus(FocusState.Programmatic);
                }
            }
        }

        private void password_switch_Toggled(object sender, RoutedEventArgs e)
        {
            switch (password_switch.IsOn)
            {
                case false:
                    utilities.STATS.Values[settings.password] = null;
                    password_box.Text = "";
                    password_box.IsEnabled = false;
                    password_icon.Glyph = "";
                    password_edit_grid.Visibility = Visibility.Collapsed;
                    break;
                case true:
                    password_icon.Glyph = "";
                    password_edit_grid.Visibility = Visibility.Visible;
                    Object password_obj = utilities.STATS.Values[settings.password];
                    bool password = password_obj == null ? false : true;
                    if (!password)
                    {
                        password_box.IsEnabled = true;
                        password_box.Focus(FocusState.Programmatic);
                        password_icon.Glyph = "";
                    }
                    else
                    {
                        password_box.Text = password_obj.ToString();
                    }
                    break;
            }
        }

        private void password_box_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(password_box.Text.ToString()))
            {
                password_box.IsEnabled = false;
                password_icon.Glyph = "";
                password_switch.IsOn = false;
            }
        }

        private async void hello_switch_Toggled(object sender, RoutedEventArgs e)
        {
            switch (hello_switch.IsOn)
            {
                case true:
                    UserConsentVerifierAvailability consentAvailability = await UserConsentVerifier.CheckAvailabilityAsync();
                    switch (consentAvailability)
                    {
                        case UserConsentVerifierAvailability.Available:
                        case UserConsentVerifierAvailability.DeviceBusy:
                        case UserConsentVerifierAvailability.DeviceNotPresent:
                            utilities.STATS.Values[settings.windows_hello] = "on";
                            break;
                        case UserConsentVerifierAvailability.DisabledByPolicy:
                        case UserConsentVerifierAvailability.NotConfiguredForUser:
                            await new MessageDialog(utilities.loader.GetString("hello_not_available"), utilities.loader.GetString("error")).ShowAsync();
                            break;
                    }
                    break;
                case false:
                    utilities.STATS.Values[settings.windows_hello] = null;
                    break;
            }
        }

        private async void auto_backup_switch_Toggled(object sender, RoutedEventArgs e)
        {
            utilities.STATS.Values[settings.automatic_backup] = auto_backup_switch.IsOn ? "on" : null;
            if (auto_backup_switch.IsOn)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == "timesense_backup")
                    {
                        return;
                    }
                }
                var builder = new BackgroundTaskBuilder();
                builder.Name = "timesense_backup";
                builder.TaskEntryPoint = "Tasks.backup_task";
                builder.SetTrigger(new TimeTrigger(720, false));
                BackgroundExecutionManager.RemoveAccess();
                BackgroundAccessStatus access_status = await BackgroundExecutionManager.RequestAccessAsync();
                if (access_status != BackgroundAccessStatus.Denied)
                {
                    BackgroundTaskRegistration mytask = builder.Register();
                }
            }
            else
            {
                BackgroundTaskRegistration.AllTasks.Where(x => x.Value.Name == "timesense_backup").FirstOrDefault().Value.Unregister(true);
            }
        }

        private async void resetOne_btn_Click(object sender, RoutedEventArgs e)
        {
            if((await new SpanDialog().ShowAsync()) == ContentDialogResult.Primary)
            {
                if (App.range_start_date <= App.range_end_date)
                {
                    if ((App.range_end_date.Date.Subtract(DateTime.Now.Date)).Days <= 0)
                    {
                        utilities.STATS.Values[settings.date] = DateTime.Now.ToString();
                    }
                    await new DeleteDialog(1).ShowAsync();
                    try
                    {
                        IReadOnlyList<ScheduledToastNotification> list = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
                        foreach (var toast in list)
                        {
                            ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
                        }
                    }
                    catch { }
                    RegisterUsageToast();
                }
                else { await new MessageDialog(utilities.loader.GetString("error_span"), utilities.loader.GetString("error")).ShowAsync(); }
            }
        }

        private async void resetAll_btn_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog reset_confirm = new MessageDialog(utilities.loader.GetString("delete_confirm_content"), utilities.loader.GetString("delete_confirm_title"));
            reset_confirm.Commands.Add(new UICommand(utilities.loader.GetString("yes"), null, "yes"));
            reset_confirm.Commands.Add(new UICommand(utilities.loader.GetString("no"),null, "no"));
            IUICommand reset_command = await reset_confirm.ShowAsync();
            if (reset_command.Id.ToString() == "yes")
            {
                utilities.STATS.Values[settings.date] = DateTime.Now.ToString();
                await new DeleteDialog(0).ShowAsync();
                try
                {
                    IReadOnlyList<ScheduledToastNotification> list = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
                    foreach (var toast in list)
                    {
                        ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
                    }
                }
                catch { }
                RegisterUsageToast();
            }
        }

        private async void backup_btn_Click(object sender, RoutedEventArgs e)
        {
            try {
                FolderPicker fold_picker = new FolderPicker();
                fold_picker.FileTypeFilter.Add(".tsb");
                fold_picker.FileTypeFilter.Add(".db");
                App.file_pick = true;
                StorageFolder folder = await fold_picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    App.file_pick = false;
                    StorageFile database = await ApplicationData.Current.LocalFolder.GetFileAsync("timesense_database.db");
                    await database.CopyAsync(folder, "timesense_backup.tsb", NameCollisionOption.GenerateUniqueName);
                    await new MessageDialog(utilities.loader.GetString("backup_dialog_success"), utilities.loader.GetString("success")).ShowAsync();
                }
            }
            catch
            {
                await new MessageDialog(utilities.loader.GetString("error_transaction"), utilities.loader.GetString("error")).ShowAsync();
            }
        }

        private async void restore_btn_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker file_picker = new FileOpenPicker();
            file_picker.FileTypeFilter.Add(".tsb");
            file_picker.FileTypeFilter.Add(".db");
            App.file_pick = true;
            StorageFile file = await file_picker.PickSingleFileAsync();
            if (file != null)
            {
                App.file_pick = false;
                file = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, "temp", NameCollisionOption.GenerateUniqueName);
                bool valid = await Helper.CheckIntegrity(file.Path);
                if (valid)
                {
                    await file.CopyAsync(ApplicationData.Current.LocalFolder, "timesense_database.db", NameCollisionOption.ReplaceExisting);
                    utilities.STATS.Values[settings.date] = null;
                    await new MessageDialog(utilities.loader.GetString("restore_dialog_success"), utilities.loader.GetString("success")).ShowAsync();
                }
                else
                {
                    await new MessageDialog(utilities.loader.GetString("restore_dialog_error"), utilities.loader.GetString("error")).ShowAsync();
                }
            }
        }
        
        private void password_box_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                password_btn_Click(password_btn, new RoutedEventArgs());
            }
        }

        private async void feedback_btn_Click(object sender, RoutedEventArgs e)
        {
            String recipient = "lorenzo.farinelli@outlook.it";
            EmailMessage feedback = new EmailMessage();
            feedback.Subject = "Time Sense";
            feedback.Body = "Version 2.5";
            var emailRecipient = new EmailRecipient(recipient);
            feedback.To.Add(emailRecipient);
            await EmailManager.ShowComposeNewEmailAsync(feedback);
        }

        private async void rate_btn_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9wzdncrcwwmn"));
        }

        private async void guide_settngs_btn_Click(object sender, RoutedEventArgs e)
        {
            await new WelcomeDialog().ShowAsync();
        }

        private void letters_switch_Toggled(object sender, RoutedEventArgs e)
        {
            utilities.STATS.Values[settings.letters] = letters_switch.IsOn ? "yes" : null;
        }

        #region NFC SUPPORT
        long publishedMessageId = -1;

        private void nfc_btn_Click(object sender, RoutedEventArgs e)
        {
            if(nfc_device != null)
            {
                nfc_device.StopSubscribingForMessage(subscriptionId);
                string s = "";
                switch (nfc_box.SelectedIndex)
                {
                    case 0:
                        s = "timesense:usage";
                        //USAGE
                        break;
                    case 1:
                        s = "timesense:timeline";
                        break;
                    case 2:
                        //LOCON
                        s = "timesense:locon";
                        break;
                    case 3:
                        //LOCOFF
                        s = "timesense:locoff";
                        break;
                }
                var record = new NdefUriRecord
                {
                    Uri = s
                };
                var ndefMessage = new NdefMessage { record };
                publishedMessageId = nfc_device.PublishBinaryMessage("NDEF:WriteTag", ndefMessage.ToByteArray().AsBuffer(), messageWrittenHandler);
            }
        }

        private async void messageWrittenHandler(ProximityDevice sender, long messageId)
        {
            nfc_device.StopPublishingMessage(messageId);
            messageId0 = messageId;
            subscriptionId = nfc_device.SubscribeForMessage("NDEF", MessageReceivedHandler);
            await new MessageDialog(utilities.loader.GetString("nfc_success_content"), utilities.loader.GetString("nfc_success_title")).ShowAsync();
        }

        private void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {

        }

        private void Nfc_device_DeviceDeparted(ProximityDevice sender)
        {
        }

        private void Nfc_device_DeviceArrived(ProximityDevice sender)
        {
        }
        #endregion
    }
}
