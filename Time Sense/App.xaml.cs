using Database;
using Microsoft.HockeyApp;
using Stuff;
using System;
using System.Linq;
using UniversalRateReminder;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Time_Sense
{
    sealed partial class App : Application
    {

        public static DateTime report_date = DateTime.Now;

        public static DateTime range_start_date = DateTime.Now;
        public static DateTime range_end_date = DateTime.Now;

        public MediaElement m_element = new MediaElement();

        public static bool dialog = false;

        public static string jump_arguments = null;

        public static bool file_pick = false;

        public static Type current_page = null;
        
        public App()
        {
            HockeyClient.Current.Configure("175f59265eec419aa9526ba89f726658 ");
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += App_Resuming;
        }

        private void App_Resuming(object sender, object e)
        {
            if (!file_pick)
            {
                Object password_obj = utilities.STATS.Values[settings.password];
                string password = password_obj == null ? "" : password_obj.ToString();
                if (password != "" || utilities.STATS.Values[settings.windows_hello] != null)
                {
                    jump_arguments = current_page == typeof(MainPage) ? null : current_page == typeof(TimelinePage) ? "timeline" : current_page == typeof(ReportPage) ? "report" : current_page == typeof(AnalyticsPage) ? "analysis" : current_page == typeof(SettingsPage) ? "settings" : null;
                    Frame rootFrame = Window.Current.Content as Frame;
                    rootFrame.Navigate(typeof(PasswordPage), password);
                }
            }
            else { file_pick = false; }
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            RatePopup.LaunchLimit = 7;
            RatePopup.Title = utilities.loader.GetString("rate_title");
            RatePopup.Content = utilities.loader.GetString("rate_message");
            RatePopup.CancelButtonText = utilities.loader.GetString("rate_later");
            RatePopup.RateButtonText = utilities.loader.GetString("rate_rate");
            RateReminderResult result = await RatePopup.CheckRateReminderAsync();
            if (result == RateReminderResult.Dismissed)
            {
                RatePopup.ResetLaunchCount();
            }

            Frame rootFrame = Window.Current.Content as Frame;
            Helper.InitializeDatabase();

            string password = utilities.STATS.Values[settings.password] == null ? "" : utilities.STATS.Values[settings.password].ToString();

            if (e != null && (e.Arguments != null && e.Arguments != ""))
            {
                jump_arguments = e.Arguments;
                if (e.PreviousExecutionState == ApplicationExecutionState.Running)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
            }
            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e != null && e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                if (password != "" || utilities.STATS.Values[settings.windows_hello] != null)
                {
                    rootFrame.Navigate(typeof(PasswordPage), password);
                    goto activate;
                }
                else
                {
                    rootFrame.Navigate(typeof(MainPage), e == null ? null : e.Arguments);
                }
            }
            activate:
            Window.Current.Activate();
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.StartScreen.JumpList"))
            {
                var jump_list = await JumpList.LoadCurrentAsync();
                jump_list.Items.Clear();
                jump_list.SystemGroupKind = JumpListSystemGroupKind.None;
                JumpListItem j_timeline = JumpListItem.CreateWithArguments("timeline", utilities.loader.GetString("jump_timeline"));
                JumpListItem j_report = JumpListItem.CreateWithArguments("report", utilities.loader.GetString("jump_report"));
                JumpListItem j_settings = JumpListItem.CreateWithArguments("settings", utilities.loader.GetString("jump_settings"));
                j_settings.Logo = new Uri("ms-appx:///Assets/settings.png");
                j_report.Logo = new Uri("ms-appx:///Assets/report.png");
                j_timeline.Logo = new Uri("ms-appx:///Assets/list.png");
                jump_list.Items.Add(j_timeline);
                jump_list.Items.Add(j_report);
                jump_list.Items.Add(j_settings);
                await jump_list.SaveAsync();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Frame rootFrame = MainPage.main_fr;
            current_page = rootFrame.SourcePageType;
            deferral.Complete();
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            FileActivatedEventArgs fileArgs = args as FileActivatedEventArgs;
            if (fileArgs.Files.Count != 0)
            {
                var file = fileArgs.Files[0] as StorageFile;
                if (file != null)
                {
                    file = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, "temp", NameCollisionOption.GenerateUniqueName);
                    bool valid = await Helper.CheckIntegrity(file.Path);
                    if (valid)
                    {
                        var restoreDialog = new MessageDialog(utilities.loader.GetString("backup_restore_confirm"), utilities.loader.GetString("backup_restore_title"));
                        restoreDialog.Commands.Add(new UICommand(utilities.loader.GetString("yes"), async (command) =>
                        {
                            try
                            {
                                await file.CopyAsync(ApplicationData.Current.LocalFolder, "timesense_database.db", NameCollisionOption.ReplaceExisting);
                                utilities.STATS.Values[settings.date] = null;
                                await new MessageDialog(utilities.loader.GetString("restore_dialog_success"), utilities.loader.GetString("success")).ShowAsync();
                            }
                            catch
                            {
                                await new MessageDialog(utilities.loader.GetString("error_transaction"), utilities.loader.GetString("error")).ShowAsync();
                            }
                        }));
                        restoreDialog.Commands.Add(new UICommand(utilities.loader.GetString("no")));
                        restoreDialog.DefaultCommandIndex = 0;
                        restoreDialog.CancelCommandIndex = 1;
                        await restoreDialog.ShowAsync();
                    }
                    else
                    {
                        await new MessageDialog(utilities.loader.GetString("restore_dialog_error"), utilities.loader.GetString("error")).ShowAsync();
                    }
                }
                OnLaunched(null);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            Helper.InitializeDatabase();

            Object password_obj = utilities.STATS.Values[settings.password];
            string password = password_obj == null ? "" : password_obj.ToString();
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                if (eventArgs.Uri != null)
                {
                    if (eventArgs.Uri.ToString() != "timesense:locon" && eventArgs.Uri.ToString() != "timesense:locoff")
                    {
                        switch (eventArgs.Uri.ToString())
                        {
                            case "timesense:timeline":
                                jump_arguments = "timeline";
                                break;
                            case "timesense:usage":
                                jump_arguments = "usage";
                                break;
                            case "timesense:report":
                                jump_arguments = "report";
                                break;
                            case "timesense:settings":
                                jump_arguments = "settings";
                                break;
                            case "timesense:analysis":
                                jump_arguments = "analysis";
                                break;
                            default:
                                jump_arguments = null;
                                break;
                        }
                        if (rootFrame == null)
                        {
                            rootFrame = new Frame();

                            rootFrame.NavigationFailed += OnNavigationFailed;

                            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                            {
                            }

                            Window.Current.Content = rootFrame;
                        }

                        if (rootFrame.Content == null)
                        {
                            if (password != "" || utilities.STATS.Values[settings.windows_hello] != null)
                            {
                                rootFrame.Navigate(typeof(PasswordPage), password);
                                goto activate_1;
                            }
                            else
                            {
                                rootFrame.Navigate(typeof(MainPage), jump_arguments);
                            }
                        }
                        else
                        {
                            rootFrame.Navigate(typeof(MainPage), jump_arguments);
                        }
                        activate_1:
                        Window.Current.Activate();
                    }
                    else
                    {
                        switch (eventArgs.Uri.ToString())
                        {
                            case "timesense:locon":
                                utilities.STATS.Values[settings.location] = "on";
                                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                                synthesizer.Voice = SpeechSynthesizer.DefaultVoice;
                                SpeechSynthesisStream stream = await synthesizer.SynthesizeTextToStreamAsync(String.Format(utilities.loader.GetString("voice_location"), utilities.loader.GetString("on")));
                                m_element.SetSource(stream, stream.ContentType);
                                m_element.Play();
                                break;
                            case "timesense:locoff":
                                utilities.STATS.Values[settings.location] = "off";
                                SpeechSynthesizer synthesizer_off = new SpeechSynthesizer();
                                SpeechSynthesisStream stream_off = await synthesizer_off.SynthesizeTextToStreamAsync(String.Format(utilities.loader.GetString("voice_location"), utilities.loader.GetString("off")));
                                m_element.SetSource(stream_off, stream_off.ContentType);
                                m_element.Play();
                                break;
                        }
                        OnLaunched(null);
                    }
                }
            }
            else if (args.Kind == ActivationKind.ToastNotification)
            {
                OnLaunched(null);
            }
            else if (args.Kind == ActivationKind.VoiceCommand)
            {
                VoiceCommandActivatedEventArgs eventArgs = args as VoiceCommandActivatedEventArgs;
                SpeechRecognitionResult result = eventArgs.Result;
                if (eventArgs != null)
                {
                    string result_str = result.RulePath.FirstOrDefault();
                    switch (result_str)
                    {
                        case "timeline":
                            jump_arguments = "timeline";
                            break;
                        case "usage":
                            jump_arguments = "usage";
                            break;
                        case "report":
                            jump_arguments = "report";
                            break;
                        case "settings":
                            jump_arguments = "settings";
                            break;
                        default:
                            jump_arguments = null;
                            break;
                    }
                }
            }
            else
            {
                jump_arguments = null;
            }
            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                if (password != "" || utilities.STATS.Values[settings.windows_hello] != null)
                {
                    rootFrame.Navigate(typeof(PasswordPage), password);
                    goto activate_2;
                }
                else
                {
                    rootFrame.Navigate(typeof(MainPage), jump_arguments);
                }
            }
            activate_2:
            Window.Current.Activate();
        }
    }
}
