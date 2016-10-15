using Database;
using Stuff;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class controller_lock : IBackgroundTask
    {
        DateTime[] date = new DateTime[2];

        int[] time = new int[2];
        int[] unlocks = new int[2];
        int[] total_seconds = new int[2];
        int diff = 0;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            bool date1_exist = utilities.CheckDate(settings.date);
            date[1] = DateTime.Now;
            if (date1_exist)
            {
                try
                {
                    date[0] = DateTime.Parse(utilities.STATS.Values[settings.date].ToString());
                }
                catch
                {
                    goto error;
                }
                // LOAD TIME FROM DATABASE
                for (int i = date[0].Date == date[1].Date ? 1 : 0; i < 2; i++)
                {
                    string date_str = utilities.shortdate_form.Format(date[i]);
                    var item = (await Helper.ConnectionDb().Table<Report>().ToListAsync()).Find(x => x.date == date_str);
                    time[i] = item == null ? 0 : item.usage;
                    unlocks[i] = await Helper.ConnectionDb().Table<Timeline>().Where(x => x.date == date_str).CountAsync();
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
                        if (unlocks[1] < 5 && diff >= 9000)
                        {
                        }
                        else {
                            time[1] += diff;
                            if (date[0].Hour < date[1].Hour)
                            {
                                await Helper.UpdateHourItem(date[1], date[0].Hour, ((date[0].Hour + 1) * 3600) - total_seconds[0], 0);
                                for (int i = 1; i < date[1].Hour - date[0].Hour; i++)
                                {
                                    await Helper.UpdateHourItem(date[1], date[0].Hour + i, 3600, 0);
                                }
                                await Helper.UpdateHourItem(date[1], date[1].Hour, total_seconds[1] - (date[1].Hour * 3600), 0);
                            }
                            else if (date[0].Hour == date[1].Hour)
                            {
                                await Helper.UpdateHourItem(date[1], date[1].Hour, diff, 0);
                            }
                            await Helper.UpdateUsageItem(time[1], unlocks[1], date[1]);
                            await Helper.UpdateTimelineItem(unlocks[1], diff, date[1]);
                        }
                    }
                    else if (date[1].DayOfYear - date[0].DayOfYear == 1)
                    {
                        ConsecutiveDaysDiff();
                    }
                }
            }
            error:
            utilities.STATS.Values[settings.date] = date[1].ToString();
            try
            {
                IReadOnlyList<ScheduledToastNotification> list = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
                foreach(var toast in list)
                {
                    ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
                }
            }
            catch { }
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name.Contains("timesense_timer"))
                {
                    task.Value.Unregister(true);
                }
            }
            _deferral.Complete();
        }

        private async void ConsecutiveDaysDiff()
        {
            if (total_seconds[1] <= 9000)
            {
                time[0] += (86400 - total_seconds[0]);
                time[1] += total_seconds[1];
                await Helper.UpdateHourItem(date[0], date[0].Hour, (((date[0].Hour + 1) * 3600) - total_seconds[0]), 0);
                for (int i = 1; i < 24 - date[0].Hour; i++)
                {
                    await Helper.UpdateHourItem(date[0], date[0].Hour + i, 3600, 0);
                }
                for (int i = 0; i < date[1].Hour; i++)
                {
                    await Helper.UpdateHourItem(date[1], i, 3600, i == 0 ? 1 : 0);
                }
                await Helper.UpdateHourItem(date[1], date[1].Hour, total_seconds[1] - (date[1].Hour * 3600), 0);
                for (int i = 0; i < 2; i++)
                {
                    await Helper.UpdateUsageItem(time[i], unlocks[i], date[i]);
                }
                await Helper.UpdateTimelineItem(unlocks[0], (86400 - total_seconds[0]), date[0]);
                var radios = await Windows.Devices.Radios.Radio.GetRadiosAsync();
                var bluetooth_device = radios.Where(x => x.Kind == Windows.Devices.Radios.RadioKind.Bluetooth).FirstOrDefault();
                var wifi_device = radios.Where(x => x.Kind == Windows.Devices.Radios.RadioKind.WiFi).FirstOrDefault();
                string bluetooth = bluetooth_device == null ? "off" : bluetooth_device.State == Windows.Devices.Radios.RadioState.On ? "on" : "off";
                string wifi = wifi_device == null ? "off" : wifi_device.State == Windows.Devices.Radios.RadioState.On ? "on" : "off";
                string battery = Windows.Devices.Power.Battery.AggregateBattery.GetReport().Status == Windows.System.Power.BatteryStatus.Charging ? "charging" : "null";
                await Helper.AddTimelineItem(date[1], "00:00:00", 1, Windows.System.Power.PowerManager.RemainingChargePercent, battery, bluetooth, wifi);
                await Helper.UpdateTimelineItem(1, time[1], date[1]);
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
    }
}
