using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Stuff;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class timer_task : IBackgroundTask
    {
        DateTime[] date = new DateTime[2];
        int[] time = new int[2];
        int[] unlocks = new int[2];
        int diff = 0;
        int[] total_seconds = new int[2];

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            
            date[1] = DateTime.Now;
            if (await utilities.CheckDate(settings.date))
            {
                try
                {
                    date[0] = DateTime.Parse(utilities.STATS.Values[settings.date].ToString());
                }
                catch
                {
                    goto save;
                }
                // LOAD TIME FROM DATABASE
                //IF THE DAY HASN'T CHANGED, LOAD ONLY ONE COUPLE OF DATA
                await Database.Helper.InitializeDatabase();
                for (int i = date[0].Date == date[1].Date ? 1 : 0; i < 2; i++)
                {
                    string date_str = utilities.shortdate_form.Format(date[i]);
                    var item = await Database.Helper.ConnectionDb().Table<Database.Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
                    time[i] = item == null ? 0 : item.usage;
                    var unlocks_list = await Database.Helper.ConnectionDb().Table<Database.Timeline>().Where(x => x.date == date_str).ToListAsync();
                    unlocks[i] = unlocks_list.Count;
                }
                //END LOADING
                ConvertSeconds();
                if (date[0].Year != date[1].Year)
                {
                    if (date[0].Day == 31 && date[1].Day == 1)
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
                            await Database.Helper.UpdateHourItem(date[1], i, 3600, 0);
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
                        await Database.Helper.UpdateTimelineItem(unlocks[1], diff, date[1]); //AGGIORNA TIMELINE
                    }
                    else if (date[1].DayOfYear - date[0].DayOfYear == 1)
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
                            await Database.Helper.UpdateHourItem(date[1], i, 3600, 0);
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
                    }
                }
            }
            save:
            utilities.STATS.Values[settings.date] = date[1].ToString();
            SendNotifications();
            _deferral.Complete();
        }


        private void ConvertSeconds()
        {
            for (int i = 0; i < 2; i++)
            {
                total_seconds[i] = (date[i].Hour * 3600) + (date[i].Minute * 60) + date[i].Second;
            }
            diff = total_seconds[1] - total_seconds[0];
        }

        private void SendNotifications()
        {
            bool badge = utilities.STATS.Values[settings.unlocks] == null ? true : utilities.STATS.Values[settings.unlocks].ToString() == "badge" ? true : false;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(utilities.TileXmlBuilder(utilities.FormatData(time[1]), unlocks[1], badge));
            TileNotification tile = new TileNotification(doc);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tile);
        }
    }
}
