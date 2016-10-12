using SQLite;
using Stuff;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                await ConnectionDb().CreateTablesAsync<Report, Hour, Timeline, AllowedContact>();
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
                try
                {
                    var accessStatus = await Geolocator.RequestAccessAsync();
                    if (accessStatus == GeolocationAccessStatus.Allowed)
                    {
                        Geolocator geolocator = new Geolocator { DesiredAccuracy = PositionAccuracy.High };
                        // Carry out the operation
                        Geoposition pos = await geolocator.GetGeopositionAsync(new TimeSpan(0, 0, 0, 0, 3000), new TimeSpan(0, 0, 0, 0, 3100));
                        return pos.Coordinate.Point;
                    }
                    else { return null; }
                }
                catch
                {
                    return null;
                }
            }
            else { return null; }
        }

        public static async Task UpdateTimelineItem(int unlocks, int new_time, DateTime date)
        {
            string date_str = utilities.shortdate_form.Format(date);
            var item = (await ConnectionDb().Table<Timeline>().ToListAsync()).FirstOrDefault(x => x.date == date_str && x.unlocks == unlocks);
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
            for (int i = 0; i < 24; i++)
            {
                var item = (await ConnectionDb().Table<Hour>().ToListAsync()).FirstOrDefault(x => x.date == date_str && x.hour == i);
                if (item != null)
                {
                    list.Add(item);
                }
                else
                {
                    list.Add(new Hour
                    {
                        date = date_str,
                        hour = i,
                        unlocks = 0,
                        usage = 0
                    });
                }
            }
            return list;
        }

        public static async Task DeleteTimelineItem()
        {
            var list = await ConnectionDb().Table<Timeline>().ToListAsync();
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
            Report report = new Report
            {
                date = utilities.shortdate_form.Format(date),
                usage = time,
                unlocks = unlocks
            };
            var item = (await ConnectionDb().Table<Report>().ToListAsync()).FirstOrDefault(x => x.date == date_str);
            if (item == null)
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
            string date_str = utilities.shortdate_form.Format(date);
            var item = (await ConnectionDb().Table<Report>().ToListAsync()).FirstOrDefault(x => x.date == date_str);
            if (item != null)
            {
                int[] data = { item.usage, item.unlocks };
                return data;
            }
            else
            {
                int[] data = { 0, 0 };
                return data;
            }
        }

        public static async Task UpdateHourItem(DateTime date, int hour, int time, int unlocks)
        {
            string date_str = utilities.shortdate_form.Format(date);
            var item = (await ConnectionDb().Table<Hour>().ToListAsync()).FirstOrDefault(x => x.date == date_str && x.hour == hour);
            if (item != null)
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

        public static async Task<List<Report>> LoadAllData()
        {
            List<Report> list = await ConnectionDb().Table<Report>().ToListAsync();
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
                var report_query = (await ConnectionDb().Table<Report>().ToListAsync()).FirstOrDefault(x => x.date == date);
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
                var report_query = await ConnectionDb().Table<Report>().ToListAsync();
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
                i = await conn.InsertAsync(new Report
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
    }
}
