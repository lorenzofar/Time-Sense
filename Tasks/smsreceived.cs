using Windows.ApplicationModel.Background;
using Windows.Devices.Sms;

namespace Tasks
{
    public sealed class smsreceived //: IBackgroundTask
    {
        /*private SmsDevice2 device = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            SmsMessageReceivedTriggerDetails smsDetails = taskInstance.TriggerDetails as SmsMessageReceivedTriggerDetails;
            SmsTextMessage2 message;
            if(smsDetails.MessageType == SmsMessageType.Text)
            {
                message = smsDetails.TextMessage;
                string sender = message.From;
                //sender = sender.Remove(0, 3);
                var query = await Database.Helper.ConnectionDb().Table<Database.AllowedContact>().Where(x => x.number == sender).FirstOrDefaultAsync(); //IF THE CONTACT IS ALLOWED ANALIZE THE MESSAGE, ELSE SEND IT TO THE MESSAGING APP
                if(query != null)
                {
                    string text = message.Body;
                    text = text.ToLower();
                    if (text.Contains("timesense/"))
                    {
                        smsDetails.Drop();
                        string[] str = text.Split('/');
                        string command = str[1];
                        string date_str = Stuff.utilities.shortdate_form.Format(DateTime.Now);
                        try
                        {
                            device = SmsDevice2.GetDefault();
                        }
                        catch { return; }
                        await Database.Helper.InitializeDatabase();
                        switch (command)
                        {
                            case "report":
                                //SENDS DATA ABOUT CURRENT USAGE TIME AND UNLOCKS
                                var data = await Database.Helper.ConnectionDb().Table<Database.Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
                                if (data != null)
                                {
                                    var timeline = await Database.Helper.ConnectionDb().Table<Database.Timeline>().Where(x => x.date == date_str).ToListAsync();
                                    int unlocks = timeline.Count;
                                    int usage = data.usage;
                                    string last_time = timeline.Last().time;
                                    double last_usage = timeline.Last().usage;
                                    double last_lat = timeline.Last().latitude;
                                    double last_long = timeline.Last().longitude;
                                    last_lat = Math.Round(last_lat, 3);
                                    last_long = Math.Round(last_long, 3);
                                    StringBuilder builder = new StringBuilder();
                                    builder.AppendLine("TIME SENSE REPORT");
                                    builder.AppendLine(String.Format("{0} - {1}", Stuff.utilities.shortdate_form.Format(DateTime.Now), Stuff.utilities.longtime_form.Format(DateTime.Now)));
                                    builder.AppendLine("");
                                    builder.AppendLine(String.Format("USAGE TIME: {0}", Stuff.utilities.FormatData(usage)));
                                    builder.AppendLine(String.Format("UNLOCKS: {0}", unlocks));
                                    builder.AppendLine("");
                                    builder.AppendLine("LAST USAGE:");
                                    builder.AppendLine(String.Format("TIME: {0}", last_time));
                                    builder.AppendLine(String.Format("USAGE: {0}", last_usage));
                                    builder.AppendLine(String.Format("LATITUDE: {0}", last_lat));
                                    builder.AppendLine(String.Format("LONGITUDE: {0}", last_long));
                                    string body_mess = builder.ToString();
                                    SmsTextMessage2 mess = new SmsTextMessage2();
                                    mess.To = sender;
                                    mess.Body = body_mess;
                                    SmsSendMessageResult result = await device.SendMessageAndGetResultAsync(mess);
                                }
                                break;
                            case "help":
                                // SENDS AN HELP MESSAGE
                                StringBuilder builder2 = new StringBuilder();
                                builder2.AppendLine("TIME SENSE [VERSION 2.3]");
                                builder2.AppendLine("");
                                builder2.AppendLine("timesense/report : Shows data about usage time, unlocks, last usage and map position");
                                builder2.AppendLine("");
                                builder2.AppendLine("timesense/timeline : Shows the timeline of unlocks");
                                builder2.AppendLine("");
                                builder2.AppendLine("timesense/map : Shows the last known position");
                                break;
                            case "timeline":
                                // SENDS THE TIMELINE
                                var timeline2 = await Database.Helper.ConnectionDb().Table<Database.Timeline>().Where(x => x.date == date_str).ToListAsync();
                                if(timeline2 != null)
                                {
                                    StringBuilder builder = new StringBuilder();
                                    builder.AppendLine("TIME SENSE REPORT");
                                    builder.AppendLine(String.Format("{0} - {1}", Stuff.utilities.shortdate_form.Format(DateTime.Now), Stuff.utilities.longtime_form.Format(DateTime.Now)));
                                    builder.AppendLine("DATA ARE IN FORM: UNLOCKS-TIME-USAGE-LATITUDE-LONGITUDE");
                                    builder.AppendLine("");
                                    foreach (var item in timeline2)
                                    {
                                        if(item != null)
                                        {
                                            double lat = Math.Round(item.latitude, 2);
                                            double lon = Math.Round(item.longitude, 2);
                                            builder.AppendLine(String.Format("{0}: {1} - {2} - {3} - {4}", item.unlocks, item.time, Stuff.utilities.FormatData(int.Parse(item.usage.ToString())), lat, lon));
                                        }
                                    }
                                    string body_mess = builder.ToString();
                                    SmsTextMessage2 mess = new SmsTextMessage2();
                                    mess.To = sender;
                                    mess.Body = body_mess;
                                    SmsSendMessageResult result = await device.SendMessageAndGetResultAsync(mess);
                                }
                                break;
                            case "map":
                                // SENDS THE LAST KNOWN POSITION
                                var timeline_q = await Database.Helper.ConnectionDb().Table<Database.Timeline>().ToListAsync();
                                double latitude = 0;
                                double longitude = 0;
                                string time = "";
                                string usage_q = "";
                                string unlocks_q = "";
                                timeline_q = timeline_q.OrderByDescending(x => x.unlocks).ToList();
                                foreach(var item in timeline_q)
                                {
                                    if(item.latitude != 0 || item.longitude != 0)
                                    {
                                        latitude = Math.Round(item.latitude, 3);
                                        longitude = Math.Round(item.longitude, 3);
                                        time = item.time;
                                        usage_q = Stuff.utilities.FormatData(int.Parse(item.usage.ToString()));
                                        unlocks_q = item.unlocks.ToString();
                                        break;
                                    }
                                }
                                StringBuilder builder_q = new StringBuilder();
                                builder_q.AppendLine("TIME SENSE MAP");
                                builder_q.AppendLine("");
                                builder_q.AppendLine("LAST KNOWN POSITION:");
                                if(latitude != 0 || longitude != 0)
                                {
                                    builder_q.AppendLine(String.Format("TIME: {0}", time));
                                    builder_q.AppendLine(String.Format("USAGE: {0}", usage_q));
                                    builder_q.AppendLine(String.Format("UNLOCKS: {0}", unlocks_q));
                                    builder_q.AppendLine(String.Format("LATITUDE: {0}", latitude));
                                    builder_q.AppendLine(String.Format("LONGITUDE: {0}", longitude));
                                }
                                else
                                {
                                    builder_q.AppendLine("There are no available location data");
                                }

                                break;
                            /*case "hour({0})":
                                string s = "";
                                for(int i = 5; i<command.Length; i++)
                                {
                                    s += command[i];
                                }
                                try {
                                    int hour = int.Parse(s);
                                    //LOAD DATA ABOUT SPECIFIED HOUR
                                }
                                catch { }
                                break;  */
                                /*
                        }
                    }
                    else
                    {
                        smsDetails.Accept();
                    }

                }
                else
                {
                    smsDetails.Accept();
                }
            }
            else
            {
                smsDetails.Accept();
            }
            _deferral.Complete();
        }*/
    }
}
