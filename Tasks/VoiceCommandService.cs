using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Stuff;

namespace Tasks
{
    public sealed class VoiceCommandService : IBackgroundTask
    {
        VoiceCommandServiceConnection voiceServiceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            var details = taskInstance.TriggerDetails as Windows.ApplicationModel.AppService.AppServiceTriggerDetails;  
            if(details != null && details.Name == "TimeSenseVoiceCommandService")
            {
                try
                {
                    voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(details);
                    voiceServiceConnection.VoiceCommandCompleted += VoiceServiceConnection_VoiceCommandCompleted;
                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                    switch (voiceCommand.CommandName)
                    {
                        case "usage":
                            SendUsageData();
                            break;
                        case "resetone":
                            ResetData(0);
                            break;
                        case "resetall":
                            ResetData(1);
                            break;
                        case "locon":
                            SwitchLocation(0);
                            break;
                        case "locoff":
                            SwitchLocation(1);
                            break;
                        default:
                            LaunchAppInForeground();
                            break;
                    }
                }
                finally
                {
                    if(_deferral != null)
                    {
                        _deferral.Complete();
                    }
                }

            }
            _deferral.Complete();
        }

        private async void SwitchLocation(int i)
        {
            Stuff.utilities.STATS.Values[Stuff.settings.location] = i == 0 ? "on" : "off";

            var location_success = new VoiceCommandUserMessage();
            location_success.DisplayMessage = location_success.SpokenMessage = String.Format(utilities.loader.GetString("voice_location"), i == 0 ? utilities.loader.GetString("on") : utilities.loader.GetString("off"));
            var location_response = VoiceCommandResponse.CreateResponse(location_success);
            await voiceServiceConnection.ReportSuccessAsync(location_response);

        }

        private async void ResetData(int i)
        {
            var userPrompt = new VoiceCommandUserMessage();
            userPrompt.DisplayMessage = userPrompt.SpokenMessage = i == 0 ? "Do you want to delete the data of today?" : "Do you want to delete all data?";
            var userReprompt = new VoiceCommandUserMessage();
            userPrompt.DisplayMessage = userReprompt.SpokenMessage = i == 0 ? "Did you want to delete data of today?" : "Did you want to delete all data?";
            var response = VoiceCommandResponse.CreateResponseForPrompt(userPrompt, userReprompt);
            var confirmation = await voiceServiceConnection.RequestConfirmationAsync(response);
            if (confirmation.Confirmed == true)
            {
                var userProgressMessage = new VoiceCommandUserMessage();
                userProgressMessage.DisplayMessage = "Deleting data...";
                userProgressMessage.SpokenMessage = "I'm deleting your data";
                VoiceCommandResponse delete_progress = VoiceCommandResponse.CreateResponse(userProgressMessage);
                await voiceServiceConnection.ReportProgressAsync(delete_progress);
                string date_str = Stuff.utilities.shortdate_form.Format(DateTime.Now);
                switch (i)
                {
                    case 0:                        
                        var query_r = await Database.Helper.ConnectionDb().Table<Database.Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
                        var query_t = await Database.Helper.ConnectionDb().Table<Database.Timeline>().Where(x => x.date == date_str).ToListAsync();
                        var query_h = await Database.Helper.ConnectionDb().Table<Database.Hour>().Where(x => x.date == date_str).ToListAsync();
                        await Database.Helper.ConnectionDb().DeleteAsync(query_r);
                        foreach(var item in query_t)
                        {
                            await Database.Helper.ConnectionDb().DeleteAsync(item);
                        }
                        foreach (var item in query_h)
                        {
                            await Database.Helper.ConnectionDb().DeleteAsync(item);
                        }
                        break;
                    case 1:
                        var query_r2 = await Database.Helper.ConnectionDb().Table<Database.Report>().ToListAsync();
                        var query_t2 = await Database.Helper.ConnectionDb().Table<Database.Timeline>().ToListAsync();
                        var query_h2 = await Database.Helper.ConnectionDb().Table<Database.Hour>().ToListAsync();
                        foreach (var item in query_r2)
                        {
                            await Database.Helper.ConnectionDb().DeleteAsync(item);
                        }
                        foreach (var item in query_t2)
                        {
                            await Database.Helper.ConnectionDb().DeleteAsync(item);
                        }
                        foreach (var item in query_h2)
                        {
                            await Database.Helper.ConnectionDb().DeleteAsync(item);
                        }
                        break;
                }
                var delete_success = new VoiceCommandUserMessage();
                delete_success.DisplayMessage = delete_success.SpokenMessage = "Your data have been succesfully deleted";
                var delete_response = VoiceCommandResponse.CreateResponse(delete_success);
                await voiceServiceConnection.ReportSuccessAsync(delete_response);
            }

        }

        private async void LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = "Launching Time Sense";
            var response = VoiceCommandResponse.CreateResponse(userMessage); // When launching the app in the foreground, pass an app // specific launch parameter to indicate what page to show.
            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        private async void SendUsageData()
        {
            string date_str = Stuff.utilities.shortdate_form.Format(DateTime.Now);
            //Database.Report item = new Database.Report();
            await Database.Helper.InitializeDatabase();
            try {
                //var list = await Database.Helper.ConnectionDb().Table<Database.Report>().ToListAsync();
                //var item = await Database.Helper.ConnectionDb().Table<Database.Report>().Where(x => x.date == date_str1).FirstOrDefaultAsync();
                //var item = list.Last();
                var item = new Database.Report
                {
                    date = date_str,
                    usage = 45600,
                    unlocks = 45
                };
               int[] data = Stuff.utilities.SplitData(item.usage);
               string short_answer = Stuff.utilities.FormatData(item.usage);
               string answer = String.Format("{0} {1}, {2} {3}, {4} {5}", data[0], data[0] == 1 ? "hour" : "hours", data[1], data[1] == 1 ? "minute" : "minutes", data[2], data[2] == 1 ? "second" : "seconds");
               var userMessage = new VoiceCommandUserMessage();
               userMessage.DisplayMessage = "Here's your usage.";
               userMessage.SpokenMessage = "You used the phone for " + answer;
               var usageTiles = new List<VoiceCommandContentTile>();
               var usageTile = new VoiceCommandContentTile();
               var unlocksTile = new VoiceCommandContentTile();
               usageTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;
               usageTile.Title = "USAGE:";
               usageTile.TextLine2 = short_answer;
               unlocksTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;
               unlocksTile.Title = "UNLOCKS:";
               unlocksTile.TextLine2 = String.Format("{0} {1}", item.unlocks, item.unlocks == 1 ? "unlock" : "unlocks");
               usageTiles.Add(usageTile);
               usageTiles.Add(unlocksTile);
               var response = VoiceCommandResponse.CreateResponse(userMessage, usageTiles);
               await voiceServiceConnection.ReportSuccessAsync(response);
            }
            catch(Exception ex)
            {
            }
        }

        private void VoiceServiceConnection_VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
        }
    }
}
