using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using Windows.ApplicationModel.Background;
using Database;
using System.Diagnostics;
using Stuff;

namespace Tasks
{
    public sealed class server_task : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            Debug.WriteLine("EXECUTING SERVER TASK");
            var client = new HttpClient();
            string log = "";
            try
            {
                var logFile = await ApplicationData.Current.LocalFolder.GetFileAsync("server_log.txt");
                var stream = File.OpenText(logFile.Path);
                log = stream.ReadToEnd();
                stream.Dispose();
            }
            catch
            {
                log = "";
                await ApplicationData.Current.LocalFolder.CreateFileAsync("server_log.txt");
            }
            string date_str = utilities.shortdate_form.Format(DateTime.Now);
            var days = await Helper.ConnectionDb().Table<Report>().Where(x => x.date != date_str).ToListAsync();
            var days_uploaded = "";
            foreach (var day in days)
            {
                if (!log.Contains(day.date))
                {
                    Debug.WriteLine($"Uploading {day.date}");
                    Debug.WriteLine($"Usage: {day.usage}, unlocks: {day.unlocks}");
                    try
                    {
                        
                        var response = await client.GetAsync("http://localhost:3000/api");
                        if(response != null)
                        {
                            days_uploaded += $",{day.date}";
                        }
                    }
                    catch { }
                }
                else
                {
                    Debug.WriteLine($"{day.date} does exist");
                }
            }
            File.WriteAllText((await ApplicationData.Current.LocalFolder.GetFileAsync("server_log.txt")).Path, $"{log}{days_uploaded}");
            _deferral.Complete();
        }
    }
}
