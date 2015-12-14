﻿using System;
using System.Linq;
using Stuff;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using UniversalRateReminder;

namespace Time_Sense
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        public static DateTime report_date = DateTime.Now;

        public MediaElement m_element = new MediaElement();

        public static bool dialog = false;

        public static string jump_arguments;

        public static Microsoft.ApplicationInsights.TelemetryClient t_client = new Microsoft.ApplicationInsights.TelemetryClient();
        
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            utilities.loader = new Windows.ApplicationModel.Resources.ResourceLoader();
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
            
            Object password_obj = utilities.STATS.Values[settings.password];
            string password = password_obj == null ? "" : password_obj.ToString();

            if (e.Arguments != null && e.Arguments != "")
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

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }
                
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                if (password != "")
                {
                    rootFrame.Navigate(typeof(PasswordPage), password);
                    goto activate;
                }
                else
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
            }
            activate:
            Window.Current.Activate();
            /*var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TimeSenseCommands.xml"));
            await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);
            */
            
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

            void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }


        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), null);
            }
            Window.Current.Activate();
            if (args.Kind == ActivationKind.Protocol)
            {
                App.t_client.TrackEvent("URI activated");
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                Object password_obj = utilities.STATS.Values[settings.password];
                string password = password_obj == null ? "" : password_obj.ToString();
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
                            default:
                                jump_arguments = null;
                                break;
                        }
                        if (args.PreviousExecutionState == ApplicationExecutionState.Running)
                        {
                            rootFrame.Navigate(typeof(MainPage), jump_arguments);
                        }
                        else
                        {
                            if (password != "")
                            {
                                rootFrame.Navigate(typeof(PasswordPage), password);
                                return;
                            }
                            else
                            {
                                rootFrame.Navigate(typeof(MainPage), jump_arguments);
                            }
                        }
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
                    }
                }
            }
            else if (args.Kind == ActivationKind.VoiceCommand)
            {
                App.t_client.TrackEvent("URI activated");
                VoiceCommandActivatedEventArgs eventArgs = args as VoiceCommandActivatedEventArgs;
                SpeechRecognitionResult result = eventArgs.Result;
                Object password_obj = utilities.STATS.Values[settings.password];
                string password = password_obj == null ? "" : password_obj.ToString();
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
                    if (args.PreviousExecutionState == ApplicationExecutionState.Running)
                    {
                        rootFrame.Navigate(typeof(MainPage), jump_arguments);
                    }
                    else
                    {
                        if (password != "")
                        {
                            rootFrame.Navigate(typeof(PasswordPage), password);
                            return;
                        }
                        else
                        {
                            rootFrame.Navigate(typeof(MainPage), jump_arguments);
                        }
                    }
                }
            }
        }
    }
}
