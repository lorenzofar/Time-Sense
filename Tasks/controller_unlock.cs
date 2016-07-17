using Database;
using Stuff;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class controller_unlock : IBackgroundTask
    {
        int time = new int();
        int unlocks = new int();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            utilities.STATS.Values[settings.date] = DateTime.Now.ToString();
            string date_str = utilities.shortdate_form.Format(DateTime.Now);
            var item = await Helper.ConnectionDb().Table<Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
            time = item == null ? 0 : item.usage;
            unlocks = ((await Helper.ConnectionDb().Table<Timeline>().Where(x => x.date == date_str).ToListAsync()).Count) + 1;
            await Helper.UpdateHourItem(DateTime.Now, DateTime.Now.Hour, 0, 1);
            var radios = await Windows.Devices.Radios.Radio.GetRadiosAsync();
            var bluetooth_device = radios.Where(x => x.Kind == Windows.Devices.Radios.RadioKind.Bluetooth).FirstOrDefault();
            var wifi_device = radios.Where(x => x.Kind == Windows.Devices.Radios.RadioKind.WiFi).FirstOrDefault();
            string bluetooth = bluetooth_device == null ? "off" : bluetooth_device.State == Windows.Devices.Radios.RadioState.On ? "on" : "off";
            string wifi = wifi_device == null ? "off" : wifi_device.State == Windows.Devices.Radios.RadioState.On ? "on" : "off";
            string battery = Windows.Devices.Power.Battery.AggregateBattery.GetReport().Status == Windows.System.Power.BatteryStatus.Charging ? "charging" : "null";
            await Helper.AddTimelineItem(DateTime.Now, utilities.longtime_form.Format(DateTime.Now), unlocks, Windows.System.Power.PowerManager.RemainingChargePercent, battery, bluetooth, wifi);
            CheckLimit();
            UpdateTile();
            //REGISTER NEW BACKGROUND TASK IF THE PREVIOUS WAS CANCELED (ONLY ON PCS)
            if (!Windows.Foundation.Metadata.ApiInformation.IsEventPresent("Windows.Phone.UI.Input.HardwareButtons", "BackPressed"))
            {
                try { RegisterTaskTimer(1, 15); } catch { }
                try { RegisterTaskTimer(2, 20); } catch { }
                try { RegisterTaskTimer(3, 25); } catch { }
                try { RegisterTaskTimer(4, 35); } catch { }
                try { RegisterTaskTimer(5, 50); } catch { }
            }
            _deferral.Complete();
        }

        private void UpdateTile()
        {
            XmlDocument doc = new XmlDocument();
            bool badge = utilities.STATS.Values[settings.unlocks] == null ? true : utilities.STATS.Values[settings.unlocks].ToString() == "badge" ? true : false;            
            doc.LoadXml(utilities.TileXmlBuilder(utilities.FormatData(time), unlocks, badge));
            TileNotification tile = new TileNotification(doc);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tile);
            if (badge)
            {
                string badgeXmlString = "<badge value='" + unlocks.ToString() + "'/>";
                XmlDocument badgeDOM = new XmlDocument();
                badgeDOM.LoadXml(badgeXmlString);
                BadgeNotification badge_not = new BadgeNotification(badgeDOM);
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge_not);
            }
        }

        private void CheckLimit()
        {
            int limit = utilities.STATS.Values[settings.limit] == null ? 7200 : int.Parse(utilities.STATS.Values[settings.limit].ToString());
            int span = limit - time;
            if(limit != 0 && span >= 0 && DateTime.Now.AddSeconds(span).Date == DateTime.Now.Date)
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

        #region TIMER TASKS

        private async void RegisterTaskTimer(int index, int span)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == $"timesense_timer_{index}")
                {
                    return;
                }
            }            
            var builder = new BackgroundTaskBuilder();
            builder.Name = string.Format("timesense_timer_{0}", index);
            builder.TaskEntryPoint = "Tasks.timer_task";
            builder.SetTrigger(new TimeTrigger(uint.Parse(span.ToString()), false));
            builder.CancelOnConditionLoss = true;
            SystemCondition user_present_condition = new SystemCondition(SystemConditionType.UserPresent);
            builder.AddCondition(user_present_condition);
            BackgroundExecutionManager.RemoveAccess();
            BackgroundAccessStatus access_status = await BackgroundExecutionManager.RequestAccessAsync();
            if (access_status != BackgroundAccessStatus.Denied)
            {
                BackgroundTaskRegistration mytask = builder.Register();
            }
        }
        
        #endregion
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  