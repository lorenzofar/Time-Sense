﻿using Stuff;
using System;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace Tasks
{
    public sealed class service : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private AppServiceConnection appServiceConnection;

        int[] total_seconds = new int[2];
        int[] time = new int[2];
        int[] unlocks = new int[2];
        DateTime[] date = new DateTime[2];
        int diff = new int();

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            appServiceConnection = details.AppServiceConnection;
            appServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
        }

        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();
            ValueSet message = args.Request.Message;
            ValueSet returnData = new ValueSet();
            string command = message["Command"] as string;
            Refresh();
            //string date_str = Stuff.utilities.shortdate_form.Format(DateTime.Now);
            //var item = await Database.Helper.ConnectionDb().Table<Database.Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
            double usage = double.Parse(time[1].ToString());
            switch (command)
            {
                case "seconds":
                    break;
                case "minutes":
                    usage = usage / 60;
                    break;
                case "hours":
                    usage = usage / 3600;
                    break;
            }
            returnData.Add("Result", usage);
            returnData.Add("Status", "OK");
            await args.Request.SendResponseAsync(returnData);
            messageDeferral.Complete();
            this._deferral.Complete();

        }

        private async void Refresh()
        {
            bool date1_exist = utilities.CheckDate(settings.date);
            date[1] = DateTime.Now;
            if (date1_exist)
            {
                date[0] = DateTime.Parse(utilities.STATS.Values[settings.date].ToString());
                // LOAD TIME FROM DATABASE
                for (int i = date[0].Date == date[1].Date ? 1 : 0; i < 2; i++)
                {
                    string date_str = utilities.shortdate_form.Format(date[i]);
                    var item = await Database.Helper.ConnectionDb().Table<Database.Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
                    time[i] = item == null ? 0 : item.usage;
                    unlocks[i] = (await Database.Helper.ConnectionDb().Table<Database.Timeline>().Where(x => x.date == date_str).ToListAsync()).Count;
                }
                //END LOADING
                ConvertSeconds();
                if (date[0].Year != date[1].Year)
                {
                    if (date[0].Day == 31 && date[1].Day == 1)
                    {
                        ConsecutiveDaysDiff();
                    }
                }
                else
                {
                    if (date[0].Date == date[1].Date)
                    {
                        time[1] += diff;
                        if (date[0].Hour < date[1].Hour)
                        {
                            await Database.Helper.UpdateHourItem(date[1], date[0].Hour, ((date[0].Hour + 1) * 3600) - total_seconds[0], 0);
                            for (int i = 1; i < date[1].Hour - date[0].Hour; i++)
                            {
                                await Database.Helper.UpdateHourItem(date[1], date[0].Hour + i, 3600, 0);
                            }
                            await Database.Helper.UpdateHourItem(date[1], date[1].Hour, total_seconds[1] - (date[1].Hour * 3600), 0);
                        }
                        else if (date[0].Hour == date[1].Hour)
                        {
                            await Database.Helper.UpdateHourItem(date[1], date[1].Hour, diff, 0);
                        }
                        await Database.Helper.UpdateUsageItem(time[1], unlocks[1], date[1]);
                        await Database.Helper.UpdateTimelineItem(unlocks[1], diff, date[1]);
                    }
                    else if (date[1].DayOfYear - date[0].DayOfYear == 1)
                    {
                        ConsecutiveDaysDiff();
                    }
                }
            }
            utilities.STATS.Values[settings.date] = date[1].ToString();
        }

        private async void ConsecutiveDaysDiff()
        {
            if (total_seconds[1] <= 9000)
            {
                time[0] += (86400 - total_seconds[0]);
                time[1] += total_seconds[1];
                await Database.Helper.UpdateHourItem(date[0], date[0].Hour, (((date[0].Hour + 1) * 3600) - total_seconds[0]), 0);
                for (int i = 1; i < 24 - date[0].Hour; i++)
                {
                    await Database.Helper.UpdateHourItem(date[0], date[0].Hour + i, 3600, 0);
                }
                for (int i = 0; i < date[1].Hour; i++)
                {
                    await Database.Helper.UpdateHourItem(date[1], i, 3600, i == 0 ? 1 : 0);
                }
                await Database.Helper.UpdateHourItem(date[1], date[1].Hour, total_seconds[1] - (date[1].Hour * 3600), 0);
                for (int i = 0; i < 2; i++)
                {
                    await Database.Helper.UpdateUsageItem(time[i], unlocks[i], date[i]);
                }
                await Database.Helper.UpdateTimelineItem(unlocks[0], (86400 - total_seconds[0]), date[0]);
                var radios = await Windows.Devices.Radios.Radio.GetRadiosAsync();
                var bluetooth_device = radios.Where(x => x.Kind == Windows.Devices.Radios.RadioKind.Bluetooth).FirstOrDefault();
                var wifi_device = radios.Where(x => x.Kind == Windows.Devices.Radios.RadioKind.WiFi).FirstOrDefault();
                string bluetooth = bluetooth_device == null ? "off" : bluetooth_device.State == Windows.Devices.Radios.RadioState.On ? "on" : "off";
                string wifi = wifi_device == null ? "off" : wifi_device.State == Windows.Devices.Radios.RadioState.On ? "on" : "off";
                string battery = Windows.Devices.Power.Battery.AggregateBattery.GetReport().Status == Windows.System.Power.BatteryStatus.Charging ? "charging" : "null";
                await Database.Helper.AddTimelineItem(date[1], "00:00:00", 1, Windows.System.Power.PowerManager.RemainingChargePercent, battery, bluetooth, wifi);
                await Database.Helper.UpdateTimelineItem(1, time[1], date[1]);
            }
        }

        private void ConvertSeconds()
        {
            for (int i = 0; i < 2; i++)
            {
                total_seconds[i] = (date[i].Hour * 3600) + (date[i].Minute * 60) + date[i].Second;
            }
            diff = total_seconds[1] - total_seconds[0];
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this._deferral != null)
            {
                this._deferral.Complete();
            }
        }
    }
}