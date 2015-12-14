using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class limitreach : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            ToastNotificationHistoryChangedTriggerDetail detail = taskInstance.TriggerDetails as ToastNotificationHistoryChangedTriggerDetail;
            Stuff.utilities.STATS.Values[Stuff.settings.limit_reached] = "yes";
            _deferral.Complete();
        }
    }
}
