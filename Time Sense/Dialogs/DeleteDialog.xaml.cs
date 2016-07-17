using System;
using System.Collections.Generic;
using Database;
using Stuff;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Time_Sense
{
    public sealed partial class DeleteDialog : ContentDialog
    {

        public DeleteDialog(int mode)
        {
            this.InitializeComponent();
            Start(mode);
        }

        private async void Start(int mode)
        {
            App.dialog = true;
            if (mode == 0)
            {
                try
                {
                    int step = 0;
                    var report_query = await Helper.ConnectionDb().Table<Report>().ToListAsync();
                    var hour_query = await Helper.ConnectionDb().Table<Hour>().ToListAsync();
                    var timeline_query = await Helper.ConnectionDb().Table<Timeline>().ToListAsync();
                    int max = report_query.Count + hour_query.Count + timeline_query.Count;
                    bar.Maximum = max;
                    foreach (var item in report_query)
                    {
                        if (item != null)
                        {
                            await Helper.ConnectionDb().DeleteAsync(item);
                            step++;
                            bar.Value = step;
                            delete_progress.Text = String.Format(utilities.loader.GetString("delete_dialog_progress"), step, max);
                        }
                    }
                    foreach (var item in hour_query)
                    {
                        if (item != null)
                        {
                            await Helper.ConnectionDb().DeleteAsync(item);
                            step++;
                            bar.Value = step;
                            delete_progress.Text = String.Format(utilities.loader.GetString("delete_dialog_progress"), step, max);
                        }
                    }
                    foreach (var item in timeline_query)
                    {
                        if (item != null)
                        {
                            await Helper.ConnectionDb().DeleteAsync(item);
                            step++;
                            bar.Value = step;
                            delete_progress.Text = String.Format(utilities.loader.GetString("delete_dialog_progress"), step, max);
                        }
                    }
                    await new MessageDialog(utilities.loader.GetString("delete_dialog_success"), utilities.loader.GetString("success")).ShowAsync();
                    this.Hide();
                }
                catch
                {
                    await new MessageDialog(utilities.loader.GetString("delete_dialog_error"), utilities.loader.GetString("error")).ShowAsync();
                    this.Hide();
                }
            }
            else
            {
                try
                {
                    //DELETE RANGE
                    int days = App.range_end_date.Date.Subtract(App.range_start_date.Date).Days;
                    int step = 0;
                    var t = new List<Timeline>();
                    var r = new List<Report>();
                    var h = new List<Hour>();
                    for (int i = 0; i <= days; i++)
                    {
                        string date_str = utilities.shortdate_form.Format(App.range_start_date.Date.AddDays(i));
                        Report d_item = await Helper.ConnectionDb().Table<Report>().Where(x => x.date == date_str).FirstOrDefaultAsync();
                        var t_list = await Helper.ConnectionDb().Table<Timeline>().Where(x => x.date == date_str).ToListAsync();
                        var h_list = await Helper.ConnectionDb().Table<Hour>().Where(x => x.date == date_str).ToListAsync();
                        if (d_item != null)
                        {
                            r.Add(d_item);
                        }
                        foreach (var item in t_list)
                        {
                            if (item != null)
                            {
                                t.Add(item);
                            }
                        }
                        foreach (var item in h_list)
                        {
                            if (item != null)
                            {
                                h.Add(item);
                            }
                        }
                    }
                    int max = r.Count + t.Count + h.Count;
                    bar.Maximum = max;
                    foreach (var item in r)
                    {
                        if (item != null)
                        {
                            await Helper.ConnectionDb().DeleteAsync(item);
                            step++;
                            bar.Value = step;
                            delete_progress.Text = String.Format(utilities.loader.GetString("delete_dialog_progress"), step, max);
                        }
                    }
                    foreach (var item in t)
                    {
                        if (item != null)
                        {
                            await Helper.ConnectionDb().DeleteAsync(item);
                            step++;
                            bar.Value = step;
                            delete_progress.Text = String.Format(utilities.loader.GetString("delete_dialog_progress"), step, max);
                        }
                    }
                    foreach (var item in h)
                    {
                        if (item != null)
                        {
                            await Helper.ConnectionDb().DeleteAsync(item);
                            step++;
                            bar.Value = step;
                            delete_progress.Text = String.Format(utilities.loader.GetString("delete_dialog_progress"), step, max);
                        }
                    }
                    await new MessageDialog(utilities.loader.GetString("delete_dialog_success"), utilities.loader.GetString("success")).ShowAsync();
                    this.Hide();
                }
                catch
                {
                    await new MessageDialog(utilities.loader.GetString("delete_dialog_error"), utilities.loader.GetString("error")).ShowAsync();
                    this.Hide();
                }

            }

        }
    }
}
