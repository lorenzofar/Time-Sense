using SQLite;
using Stuff;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;

namespace Database
{
    public sealed class Helper
    {

        /// <summary>
        /// The basic method to connect to the database
        /// </summary>
        /// <returns></returns>
        public static SQLiteAsyncConnection ConnectionDb()
        {
            var conn = new SQLiteAsyncConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, "timesense_database.db"), true);
            return conn;
        }

        public static async Task InitializeDatabase()
        {
            bool db_exist = await CheckDatabase();
            if (!db_exist)
            {
                await ConnectionDb().CreateTablesAsync<Database.Report, Hour, Timeline, AllowedContact>();
            }
        }

        private static async Task<bool> CheckDatabase()
        {
            bool dbExist = true;
            try { StorageFile sf = await ApplicationData.Current.LocalFolder.GetFileAsync("timesense_database.db"); }
            catch { dbExist = false; }
            return dbExist;
        }


        /// <summary>
        /// Insert an unlock in the database
        /// </summary>
        /// <param name="date">The date of the unlock</param>
        /// <param name="time">The time of the unlock</param>
        /// <param name="unlocks">The current number of unlocks</param>
        /// <param name="battery">The current battery charge percentage</param>
        /// <param name="status">The status of the battery (charging or not)</param>
        /// <param name="bluetooth">The status of Bluetooth antenna (on or off)</param>
        /// <param name="wifi">The status of the Wi-Fi antenna (on or off)</param>
        /// <returns></returns>
        public static async Task AddTimelineItem(DateTime date, string time, int unlocks, int battery, string status, string bluetooth, string wifi)
        {
            Geopoint pos = await GetLocation();
            double latitude = pos == null ? 0 : pos.Position.Latitude;
            double longitude = pos == null ? 0 : pos.Position.Longitude;            
            Timeline item = new Timeline
            {
                time = time,
                usage = 0,
                unlocks = unlocks,
                battery = battery,
                latitude = latitude,
                longitude = longitude,
                date = utilities.shortdate_form.Format(DateTime.Now),
                battery_status = status, 
                bluetooth_status = bluetooth, 
                wifi_status = wifi
            };
            await ConnectionDb().InsertAsync(item);
        }

        private static async Task<Geopoint> GetLocation()
        {
            bool location_settings = utilities.STATS.Values[settings.location] == null || utilities.STATS.Values[settings.location].ToString() == "on" ? true : false;            
            if (location_settings)
            {
                Geopoint point;
                try
                {
                    var accessStatus = await Geolocator.RequestAccessAsync();
                    if (accessStatus == GeolocationAccessStatus.Allowed)
                    {
                        Geolocator geolocator = new Geolocator { DesiredAccuracy = PositionAccuracy.High };
                        // Carry out the operation
                        Geoposition pos = await geolocator.GetGeopositionAsync(new TimeSpan(0, 0, 0, 0, 3000), new TimeSpan(0, 0, 0, 0, 3100));
                        point = pos.Coordinate.Point;
                    }
                    else { point = null; }
                }
                catch
                {
                    return null;
                }
                return point;
            }
            else { return null; }
        }

        public static async Task UpdateTimelineItem(int unlocks, int new_time, DateTime date)
        {
            string date_str = utilities.shortdate_form.Format(date);
            var item = await ConnectionDb().Table<Timeline>().Where(x => x.date == date_str && x.unlocks == unlocks).FirstOrDefaultAsync();
            if (item != null)
            {
                try
                {
                    item.usage += new_time;
                    await ConnectionDb().UpdateAsync(item);
                }
                catch { }
            }
        }

        public static async Task<List<Timeline>> GetTimelineList(DateTime date)
        {
            string date_str = utilities.shortdate_form.Format(date);
            List<Timeline> list = await ConnectionDb().Table<Timeline>().Where(x => x.date == date_str).ToListAsync();
            return list;
        }

        public static async Task<List<Hour>> GetHourList(DateTime date)
        {
            List<Hour> list = new List<Hour>();
            string date_str = utilities.shortdate_form.Format(date);
            for(int i = 0; i<24; i++)
            {
                var item = await ConnectionDb().Table<Hour>().Where(x => x.date == date_str && x.hour == i).FirstOrDefaultAsync();
                if(item != null)
                {
                    list.Add(item);
                }
                else
                {
                    Hour hour = new Hour
                    {
                        date = date_str,
                        hour = i,
                        unlocks = 0,
                        usage = 0
                    };
                    list.Add(hour);
                }
            }     
            return list;
        }

        public static async Task DeleteTimelineItem()
        {
            var query = ConnectionDb().Table<Timeline>();
            var list = await query.ToListAsync();
            foreach (var item in list)
            {
                if (item != null)
                {
                    await ConnectionDb().DeleteAsync(item);
                }
            }
        }

        public static async Task UpdateUsageItem(int time, int unlocks, DateTime date)
        {
            int[] data = Formatdata(time);
            string date_str = utilities.shortdate_form.Format(date);
            Database.Report report = new Database.Report
            {
                date = utilities.shortdate_form.Format(date),
                usage = time,
                unlocks = unlocks
            };
            var item = await ConnectionDb().Table<Database.Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
            if(item == null)
            {
                await ConnectionDb().InsertAsync(report);
            }
            else
            {
                item.usage = time;
                item.unlocks = unlocks;
                await ConnectionDb().UpdateAsync(item);
            }
        }
        #region LOAD DATA
        public static async Task<int[]> LoadReportItem(DateTime date)
        {
            int[] data = new int[2];
            string date_str = utilities.shortdate_form.Format(date);
            var item = await ConnectionDb().Table<Database.Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
            if(item != null)
            {
                data[0] = item.usage;
                data[1] = item.unlocks;
            }
            else
            {
                for(int i = 0; i<2; i++)
                {
                    data[i] = 0;
                }
            }
            return data;
        }

        public static async Task UpdateHourItem(DateTime date, int hour, int time, int unlocks)
        {
            string date_str = utilities.shortdate_form.Format(date);
            var item = await ConnectionDb().Table<Hour>().Where(x => x.date == date_str && x.hour == hour).FirstOrDefaultAsync();
            if(item != null)
            {
                item.usage += time;
                item.unlocks += unlocks;
                await ConnectionDb().UpdateAsync(item); 
            }
            else
            {
                Hour hour_item = new Hour
                {
                    date = utilities.shortdate_form.Format(date),
                    usage = time,
                    unlocks = unlocks,
                    hour = hour
                };
                await ConnectionDb().InsertAsync(hour_item);
            }
        }
        #endregion

        public static async Task<List<Database.Report>> LoadAllData()
        {
            List<Database.Report> list = await ConnectionDb().Table<Database.Report>().ToListAsync();
            return list;
        }

        
        private static int[] Formatdata(int time)
        {
            int[] data = new int[3];
            data[0] = time / 3600; //HOURS
            data[1] = (time - (data[0] * 3600)) / 60; //MINUTES
            data[2] = time - (data[0] * 3600) - (data[1] * 60); // SECONDS
            return data;
        }

        #region DELETE ITEMS
        public static async Task<bool> DeleteData(DateTime dt)
        {
            bool success = true;
            try
            {
                string date = utilities.shortdate_form.Format(dt);
                var report_query = await ConnectionDb().Table<Database.Report>().Where(x => x.date == date).FirstOrDefaultAsync();
                var hour_query = await ConnectionDb().Table<Hour>().Where(x => x.date == date).ToListAsync();
                var timeline_query = await ConnectionDb().Table<Timeline>().Where(x => x.date == date).ToListAsync();
                if (report_query != null)
                {
                    await ConnectionDb().DeleteAsync(report_query);
                }
                foreach (var item in hour_query)
                {
                    if (item != null)
                    {
                        await ConnectionDb().DeleteAsync(item);
                    }
                }
                foreach (var item in timeline_query)
                {
                    if (item != null)
                    {
                        await ConnectionDb().DeleteAsync(item);
                    }
                }
            }
            catch
            {
                success = false;
            }
            return success;
        }
        public static async Task<bool> DeleteAllData()
        {
            bool success = true;
            try
            {
                var report_query = await ConnectionDb().Table<Database.Report>().ToListAsync();
                var hour_query = await ConnectionDb().Table<Hour>().ToListAsync();
                var timeline_query = await ConnectionDb().Table<Timeline>().ToListAsync();
                foreach (var item in report_query)
                {
                    if (item != null)
                    {
                        await ConnectionDb().DeleteAsync(item);
                    }
                }
                foreach (var item in hour_query)
                {
                    if (item != null)
                    {
                        await ConnectionDb().DeleteAsync(item);
                    }
                }
                foreach (var item in timeline_query)
                {
                    if (item != null)
                    {
                        await ConnectionDb().DeleteAsync(item);
                    }
                }
            }
            catch
            {
                success = false;
            }
            return success;
        }
        #endregion

        public static async Task<bool> CheckIntegrity(string filePath)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(filePath, true);
            bool valid = true;
            int i = 0;
            try
            {
                i = await conn.InsertAsync(new Database.Report
                {
                    usage = 0,
                });
            }
            catch
            {
                valid = false;
            }
            return valid;
        }

        #region DATAMIGRATION

        class Report
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string date { get; set; }
            public int usage { get; set; }
            public int unlocks { get; set; }
        }

        public static async Task<bool> DataMigration(string path, string name)
        {
            bool success = false;
            var conn = new SQLiteAsyncConnection(path, true);
            try {
                int step = 0;
                int max = 0;
                var report_list = await  conn.Table<OldData.Report>().ToListAsync();
                var timeline_list = await conn.Table<OldData.Timeline>().ToListAsync();
                var hour_list = await conn.Table<OldData.Report_oggi_time>().ToListAsync();
                max = report_list.Count + timeline_list.Count + hour_list.Count;
                //await ApplicationData.Current.LocalFolder.DeleteAsync();
                await InitializeDatabase();
                foreach (var item in report_list)
                {                    
                    int unlocks = 0;
                    try
                    {
                        string[] str = item.sblocchi.Split(' ');
                        unlocks = int.Parse(str[0]);
                    }
                    catch { }
                    int time = 0;
                    try
                    {
                        string[] str = item.utilizzo.Split('h', 'm', 's', ':');
                        time = (int.Parse(str[0]) * 3600) + (int.Parse(str[2]) * 60) + int.Parse(str[4]);
                    }
                    catch { }
                    Database.Report r = new Database.Report
                    {
                        date = item.data,
                        unlocks = unlocks,
                        usage = time
                    };
                    await ConnectionDb().InsertAsync(r);
                    step++;
                }
                foreach(var item in timeline_list)
                {
                    int unlocks = 0;
                    try
                    {
                        string[] str = item.sblocchi.Split(' ');
                        unlocks = int.Parse(str[0]);
                    }
                    catch { }
                    int time = 0;
                    try
                    {
                        string[] str = item.utilizzo.Split(':');
                        time = (int.Parse(str[0]) * 3600) + (int.Parse(str[1]) * 60) + int.Parse(str[2]);
                    }
                    catch { }
                    string b = "";
                    for(int i = 0; i< item.batteria.Length-1; i++)
                    {
                        b += item.batteria[i];
                    }
                    Timeline t = new Timeline
                    {
                        date = item.data,
                        time = item.orario,
                        usage = time,
                        unlocks = unlocks,
                        battery = int.Parse(b),
                        latitude = item.latitude,
                        longitude = item.longitude,
                    };
                    await ConnectionDb().InsertAsync(t);
                    step++;
                }
                foreach(var item in hour_list)
                {
                    Hour h = new Hour
                    {
                        date = item.data,
                        hour = int.Parse(item.fascia),
                        unlocks = item.sblocchi,
                        usage = item.uso * 60
                    };
                    await ConnectionDb().InsertAsync(h);
                    step++;
                }
                success = true;
            }
            catch { success = false; }
            return success;
        }

        public static async Task<Object[]> PrepareMigration(string path)
        {
            Object[] lists = new Object[3];
            var conn = new SQLiteAsyncConnection(path, true);
            var report_list = await conn.Table<OldData.Report>().ToListAsync();
            var timeline_list = await conn.Table<OldData.Timeline>().ToListAsync();
            var hour_list = await conn.Table<OldData.Report_oggi_time>().ToListAsync();
            lists[0] = report_list;
            lists[1] = timeline_list;
            lists[2] = hour_list;
            return lists;
        }
        #endregion
    }
}
