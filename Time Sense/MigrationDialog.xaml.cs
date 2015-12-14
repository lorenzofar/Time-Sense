using System;
using System.Collections.Generic;
using System.Linq;
using Stuff;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Time_Sense
{
    public sealed partial class MigrationDialog : ContentDialog
    {
        public MigrationDialog()
        {
            this.InitializeComponent();
            Start();
        }
        
        private async void Start()
        {
            App.dialog = true;
            var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
            var file = files.Where(x => x.Name.Contains(".db") && x.Name != "timesense_database.db").FirstOrDefault();
            if (file != null)
            {
                try
                {
                    Object[] lists = await Database.Helper.PrepareMigration(file.Path);
                    List<OldData.Report> list_r = lists[0] as List<OldData.Report>;
                    List<OldData.Timeline> list_t = lists[1] as List<OldData.Timeline>;
                    List<OldData.Report_oggi_time> list_h = lists[2] as List<OldData.Report_oggi_time>;
                    int max = list_r.Count + list_t.Count + list_h.Count;
                    bar.Maximum = max;
                    int step = 0;
                    foreach (var item in list_r)
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

                        var f = await Database.Helper.ConnectionDb().Table<Database.Report>().Where(x => x.date == item.data).FirstOrDefaultAsync();
                        if (f == null)
                        {
                            Database.Report r = new Database.Report
                            {
                                date = item.data,
                                unlocks = unlocks,
                                usage = time
                            };
                            await Database.Helper.ConnectionDb().InsertAsync(r);
                        }
                        else
                        {
                            f.unlocks = unlocks;
                            f.usage = time;
                            await Database.Helper.ConnectionDb().UpdateAsync(f);
                        }
                        step++;
                        bar.Value = step;
                        migrate_progress.Text = String.Format(utilities.loader.GetString("migrate_dialog_progress"), step, max);
                    }
                    foreach (var item in list_t)
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
                        for (int i = 0; i < item.batteria.Length - 1; i++)
                        {
                            b += item.batteria[i];
                        }
                        var f = await Database.Helper.ConnectionDb().Table<Database.Timeline>().Where(x => x.date == item.data && x.unlocks == unlocks).FirstOrDefaultAsync();
                        if (f == null)
                        {
                            Database.Timeline t = new Database.Timeline
                            {
                                date = item.data,
                                time = item.orario,
                                usage = time,
                                unlocks = unlocks,
                                battery = int.Parse(b),
                                latitude = item.latitude,
                                longitude = item.longitude,
                            };
                            await Database.Helper.ConnectionDb().InsertAsync(t);
                        }
                        else
                        {
                            f.time = item.orario;
                            f.usage = time;
                            f.unlocks = unlocks;
                            f.battery = int.Parse(b);
                            f.latitude = item.latitude;
                            f.longitude = item.longitude;
                            await Database.Helper.ConnectionDb().UpdateAsync(f);
                        }
                        step++;
                        bar.Value = step;
                        migrate_progress.Text = String.Format(utilities.loader.GetString("migrate_dialog_progress"), step, max);
                    }
                    foreach (var item in list_h)
                    {
                        int fasc = int.Parse(item.fascia);
                        var f = await Database.Helper.ConnectionDb().Table<Database.Hour>().Where(x => x.date == item.data && x.hour == fasc).FirstOrDefaultAsync();
                        if (f == null)
                        {
                            Database.Hour h = new Database.Hour
                            {
                                date = item.data,
                                hour = int.Parse(item.fascia),
                                unlocks = item.sblocchi,
                                usage = item.uso * 60
                            };
                            await Database.Helper.ConnectionDb().InsertAsync(h);
                        }
                        else
                        {
                            f.hour = int.Parse(item.fascia);
                            f.unlocks = item.sblocchi;
                            f.usage = item.uso * 60;
                            await Database.Helper.ConnectionDb().UpdateAsync(f);
                        }
                        step++;
                        bar.Value = step;
                        migrate_progress.Text = String.Format(utilities.loader.GetString("migrate_dialog_progress"), step, max);
                    }
                    MessageDialog success = new MessageDialog(utilities.loader.GetString("migrate_dialog_success"), utilities.loader.GetString("success"));
                    await success.ShowAsync();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageDialog error = new MessageDialog(utilities.loader.GetString("migrate_dialog_error"), utilities.loader.GetString("error"));
                    await error.ShowAsync();
                    this.Hide();
                }
            }
            else
            {
                MessageDialog success = new MessageDialog(utilities.loader.GetString("migrate_dialog_unpresent"), utilities.loader.GetString("error"));
                await success.ShowAsync();
                this.Hide();
            }
        }
    }
}
