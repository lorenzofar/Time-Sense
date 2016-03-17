using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Tasks
{
    public sealed class backup_task : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            StorageFile database = await ApplicationData.Current.LocalFolder.GetFileAsync("timesense_database.db");
            await database.CopyAsync(KnownFolders.DocumentsLibrary, "Time Sense automatic backup.db", NameCollisionOption.ReplaceExisting);
            _deferral.Complete();
        }
    }
}
