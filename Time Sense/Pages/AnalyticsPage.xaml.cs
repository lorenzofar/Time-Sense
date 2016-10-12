using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Stuff;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Store;
using GalaSoft.MvvmLight.Messaging;

namespace Time_Sense
{
    public sealed partial class AnalyticsPage : Page
    {
        bool rangeall = true;

        bool loading = true;

        double[] first_offset = { 0, 0 };

        const double TOP_LIST_OFFSET = 5;
        const double SCROLL_UP_OFFSET = 50;
        const double SCROLL_DOWN_OFFSET = 10;

        public AnalyticsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        public List<Timeline> query_l = new List<Timeline>();
        public List<Report> query_d = new List<Report>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Messenger.Default.Send<MessageHelper.AnalysisMessage>(new MessageHelper.AnalysisMessage());
            SetLayout();
        }

        private void SetLayout()
        {
            switch (vs_group.CurrentState.Name)
            {
                case "narrow":
                case "tablet":
                    RelativePanel.SetRightOf(endPick, null);
                    RelativePanel.SetRightOf(u_timemin, null);
                    RelativePanel.SetBelow(endPick, startPick);
                    RelativePanel.SetBelow(u_timemin, u_timemax);
                    break;
                case "wide":
                case "laptop":
                    RelativePanel.SetRightOf(endPick, startPick);
                    RelativePanel.SetRightOf(u_timemin, u_timemax);
                    RelativePanel.SetBelow(endPick, null);
                    RelativePanel.SetBelow(u_timemin, null);
                    break;
            }
        }

        private void days_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (Report)days_list.SelectedValue;
            if (item != null)
            {
                string date_str = item.date;
                int day = int.Parse(date_str[1].ToString()) * 10 + int.Parse(date_str[2].ToString());
                int month = int.Parse(date_str[6].ToString()) * 10 + int.Parse(date_str[7].ToString());
                int year = int.Parse(date_str[11].ToString()) * 1000 + int.Parse(date_str[12].ToString()) * 100 + int.Parse(date_str[13].ToString()) * 10 + int.Parse(date_str[14].ToString());
                DateTime dt = new DateTime(year, month, day);
                App.report_date = dt;
                MainPage.home.IsChecked = true;
            }
        }

        private void range_all_Checked(object sender, RoutedEventArgs e)
        {
            if (!loading)
            {
                custom_range_panel.Visibility = range_all.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
                rangeall = range_all.IsChecked == true ? true : false;
            }
            else
            {
                loading = false;
            }
        }

        private void reset_btn_Click(object sender, RoutedEventArgs e)
        {
            if (param_pivot.SelectedIndex == 0)
            {
                query_d.Clear();
                days_list.ItemsSource = null;
                days_list.ItemsSource = query_d;
                no_item_days_txt.Visibility = Visibility.Collapsed;
            }
            else
            {
                query_l.Clear();
                unlocks_list.ItemsSource = null;
                unlocks_list.ItemsSource = query_l;
                no_item_unlocks_txt.Visibility = Visibility.Collapsed;
            }
        }

        private async void search_btn_Click(object sender, RoutedEventArgs e)
        {
            search_btn.IsEnabled = false;
            reset_btn.IsEnabled = false;
            if (param_pivot.SelectedIndex == 0)
            {
                d_ring_box.Visibility = Visibility.Visible;
                d_ring.IsActive = true;
                days_list.Visibility = Visibility.Collapsed;
                query_d = new List<Report>();
                if (!rangeall)
                {
                    int days = endPick.Date.Subtract(startPick.Date).Days;
                    for (int i = 0; i <= days; i++)
                    {
                        string date_str = utilities.shortdate_form.Format(startPick.Date.AddDays(i));
                        Report d_item = await Helper.ConnectionDb().Table<Report>().Where(x => x.date == date_str && (x.unlocks != 0 || x.usage != 0)).FirstOrDefaultAsync();
                        query_d.Add(d_item);
                    }
                }
                else
                {
                    query_d = await Helper.ConnectionDb().Table<Report>().Where(x => x.unlocks != 0 && x.usage != 0).ToListAsync();

                }
                if (d_usagemax.IsChecked == true || d_usagemin.IsChecked == true || d_unlocksmax.IsChecked == true || d_unlocksmin.IsChecked == true)
                {
                    if (d_usagemax.IsChecked == true)
                    {
                        try
                        {
                            int usage_max = int.Parse(d_usage_max_box.Text);
                            usage_max = d_usage_max_combo.SelectedIndex == 0 ? usage_max : d_usage_max_combo.SelectedIndex == 1 ? usage_max * 60 : usage_max * 3600;
                            query_d = query_d.Where(x => x.usage >= usage_max).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_timemax"), utilities.loader.GetString("error")).ShowAsync();
                            d_usage_max_box.Focus(FocusState.Programmatic);
                            goto hidering_d;
                        }
                    }
                    if (d_usagemin.IsChecked == true)
                    {
                        try
                        {
                            int usage_min = int.Parse(d_usage_min_box.Text);
                            usage_min = d_usage_min_combo.SelectedIndex == 0 ? usage_min : d_usage_min_combo.SelectedIndex == 1 ? usage_min * 60 : usage_min * 3600;
                            query_d = query_d.Where(x => x.usage <= usage_min).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_timemin"), utilities.loader.GetString("error")).ShowAsync();
                            d_usage_min_box.Focus(FocusState.Programmatic);
                            goto hidering_d;
                        }
                    }
                    if (d_unlocksmax.IsChecked == true)
                    {
                        try
                        {
                            int unlocks_max = int.Parse(d_unlocks_max_box.Text);
                            query_d = query_d.Where(x => x.unlocks >= unlocks_max).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_unlocksmax"), utilities.loader.GetString("error")).ShowAsync();
                            d_unlocks_max_box.Focus(FocusState.Programmatic);
                            goto hidering_d;
                        }
                    }
                    if (d_unlocksmin.IsChecked == true)
                    {
                        try
                        {
                            int unlocks_min = int.Parse(d_unlocks_min_box.Text);
                            query_d = query_d.Where(x => x.unlocks <= unlocks_min).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_unlocksmin"), utilities.loader.GetString("error")).ShowAsync();
                            d_unlocks_min_box.Focus(FocusState.Programmatic);
                            goto hidering_d;
                        }
                    }
                    days_list.ItemsSource = null;
                    days_list.ItemsSource = query_d;
                }
                else
                {
                    query_d = null;
                    days_list.ItemsSource = null;
                }
            hidering_d:
                if (query_d != null)
                {
                    no_item_days_txt.Visibility = query_d.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                d_ring_box.Visibility = Visibility.Collapsed;
                days_list.Visibility = Visibility.Visible;
                d_ring.IsActive = false;

            }
            else
            {
                u_ring_box.Visibility = Visibility.Visible;
                unlocks_list.Visibility = Visibility.Collapsed;
                u_ring.IsActive = true;
                query_l = new List<Timeline>();
                if (!rangeall)
                {
                    int days = endPick.Date.Subtract(startPick.Date).Days;
                    for (int i = 0; i <= days; i++)
                    {
                        string date_str = Stuff.utilities.shortdate_form.Format(startPick.Date.AddDays(i));
                        var d_query = await Helper.ConnectionDb().Table<Timeline>().Where(x => x.date == date_str && (x.usage != 0 || x.unlocks != 0)).ToListAsync();
                        foreach (var item in d_query)
                        {
                            query_l.Add(item);
                        }
                    }
                }
                else
                {
                    query_l = await Helper.ConnectionDb().Table<Timeline>().Where(x => x.unlocks != 0 && x.usage != 0).ToListAsync();
                }
                if (u_batmax.IsChecked == true || u_batmin.IsChecked == true || u_batstat.IsChecked == true || u_bluetooth.IsChecked == true || u_timemax.IsChecked == true || u_timemin.IsChecked == true || u_unlocksmax.IsChecked == true || u_unlocksmin.IsChecked == true || u_usagemax.IsChecked == true || u_usagemin.IsChecked == true || u_wifi.IsChecked == true)
                {
                    if (u_usagemax.IsChecked == true)
                    {
                        try
                        {
                            int usage_max = int.Parse(u_usage_max_box.Text);
                            usage_max = u_usage_max_combo.SelectedIndex == 0 ? usage_max : u_usage_max_combo.SelectedIndex == 1 ? usage_max * 60 : usage_max * 3600;
                            query_l = query_l.Where(x => x.usage >= usage_max).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_timemax"), utilities.loader.GetString("error")).ShowAsync();
                            u_usage_max_box.Focus(FocusState.Programmatic);
                            goto hidering;
                        }
                    }
                    if (u_usagemin.IsChecked == true)
                    {
                        try
                        {
                            int usage_min = int.Parse(u_usage_min_box.Text);
                            usage_min = u_usage_min_combo.SelectedIndex == 0 ? usage_min : u_usage_min_combo.SelectedIndex == 1 ? usage_min * 60 : usage_min * 3600;
                            query_l = query_l.Where(x => x.usage <= usage_min).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_timemin"), utilities.loader.GetString("error")).ShowAsync();
                            u_usage_min_box.Focus(FocusState.Programmatic);
                            goto hidering;
                        }
                    }
                    if (u_unlocksmax.IsChecked == true)
                    {
                        try
                        {
                            int unlocks_max = int.Parse(u_unlocks_max_box.Text);
                            query_l = query_l.Where(x => x.unlocks >= unlocks_max).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_unlocksmax"), utilities.loader.GetString("error")).ShowAsync();
                            u_unlocks_max_box.Focus(FocusState.Programmatic);
                            goto hidering;
                        }
                    }
                    if (u_unlocksmin.IsChecked == true)
                    {
                        try
                        {
                            int unlocks_min = int.Parse(u_unlocks_min_box.Text);
                            query_l = query_l.Where(x => x.unlocks <= unlocks_min).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_unlocksmin"), utilities.loader.GetString("error")).ShowAsync();
                            u_unlocks_min_box.Focus(FocusState.Programmatic);
                            goto hidering;
                        }
                    }
                    if (u_batmax.IsChecked == true)
                    {
                        try
                        {
                            int bat_max = int.Parse(u_battery_max_box.Text);
                            query_l = query_l.Where(x => x.battery >= bat_max).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_batmax"), utilities.loader.GetString("error")).ShowAsync();
                            u_battery_max_box.Focus(FocusState.Programmatic);
                            goto hidering;
                        }
                    }
                    if (u_batmin.IsChecked == true)
                    {
                        try
                        {
                            int bat_min = int.Parse(u_battery_min_box.Text);
                            query_l = query_l.Where(x => x.battery <= bat_min).ToList();
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_batmin"), utilities.loader.GetString("error")).ShowAsync();
                            u_battery_min_box.Focus(FocusState.Programmatic);
                            goto hidering;
                        }
                    }
                    if (u_batstat.IsChecked == true)
                    {
                        try
                        {
                            string batstat = u_battery_combo.SelectedIndex == 0 ? "charging" : "null";
                            query_l = query_l.Where(x => x.battery_status == batstat).ToList();
                        }
                        catch { goto hidering; }
                    }
                    if (u_bluetooth.IsChecked == true)
                    {
                        try
                        {
                            string bluetooth = u_bluetooth_combo.SelectedIndex == 0 ? "on" : "off";
                            query_l = query_l.Where(x => x.bluetooth_status == bluetooth).ToList();
                        }
                        catch { goto hidering; }
                    }
                    if (u_wifi.IsChecked == true)
                    {
                        try
                        {
                            string wifi = u_wifi_combo.SelectedIndex == 0 ? "on" : "off";
                            query_l = query_l.Where(x => x.wifi_status == wifi).ToList();
                        }
                        catch { goto hidering; }
                    }
                    if (u_timemax.IsChecked == true)
                    {
                        int timemax = u_time_max_box.Time.Hours * 3600 + u_time_max_box.Time.Minutes * 60 + u_time_max_box.Time.Seconds;
                        List<Timeline> query_raw = query_l.ToList();
                        query_l.Clear();
                        foreach (var item in query_raw)
                        {
                            int h = Int32.Parse(item.time[1].ToString()) * 10 + Int32.Parse(item.time[2].ToString());
                            int m = Int32.Parse(item.time[6].ToString()) * 10 + Int32.Parse(item.time[7].ToString());
                            int s = Int32.Parse(item.time[11].ToString()) * 10 + Int32.Parse(item.time[12].ToString());
                            int tot = h * 3600 + m * 60 + s;
                            if (tot >= timemax)
                            {
                                query_l.Add(item);
                            }
                        }
                    }
                    if (u_timemin.IsChecked == true)
                    {
                        int timemin = u_time_min_box.Time.Hours * 3600 + u_time_min_box.Time.Minutes * 60 + u_time_min_box.Time.Seconds;
                        List<Timeline> query_raw = query_l.ToList();
                        query_l.Clear();
                        foreach (var item in query_raw)
                        {
                            int h = Int32.Parse(item.time[1].ToString()) * 10 + Int32.Parse(item.time[2].ToString());
                            int m = Int32.Parse(item.time[6].ToString()) * 10 + Int32.Parse(item.time[7].ToString());
                            int s = Int32.Parse(item.time[11].ToString()) * 10 + Int32.Parse(item.time[12].ToString());
                            int tot = h * 3600 + m * 60 + s;
                            if (tot <= timemin)
                            {
                                query_l.Add(item);
                            }
                        }
                    }
                    unlocks_list.ItemsSource = null;
                    unlocks_list.ItemsSource = query_l;
                }
                else
                {
                    query_l = null;
                    unlocks_list.ItemsSource = null;
                }
            hidering:
                if (query_l != null)
                {
                    no_item_unlocks_txt.Visibility = query_l.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                d_ring_box.Visibility = Visibility.Collapsed;
                unlocks_list.Visibility = Visibility.Visible;
                u_ring.IsActive = false;

                var bord = VisualTreeHelper.GetChild(this.unlocks_list, 0);
                var scroll = (ScrollViewer)VisualTreeHelper.GetChild(bord, 0);
                scroll.ViewChanged += ScrollViewer_ViewChanged;
            }
            search_btn.IsEnabled = true;
            reset_btn.IsEnabled = true;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                ScrollViewer sv = sender as ScrollViewer;
                double count = param_pivot.SelectedIndex == 0 ? days_list.Items.Count : unlocks_list.Items.Count;
                double height = root.ActualHeight;
                if (height / count <= 40)
                {
                    Visibility vis = Visibility.Visible;
                    int index = param_pivot.SelectedIndex;
                    if (sv.VerticalOffset <= TOP_LIST_OFFSET)
                    {
                        first_offset[index] = sv.VerticalOffset;
                        if (param_content_column.Height.IsStar)
                        {
                            param_r.Height = new GridLength(11, GridUnitType.Star);
                        }
                        else
                        {
                            param_r.Height = new GridLength(1, GridUnitType.Auto);
                        }
                        vis = Visibility.Collapsed;
                    }
                    else
                    {
                        vis = Visibility.Visible;
                        first_offset[index] = sv.VerticalOffset;
                        param_r.Height = new GridLength(0, GridUnitType.Pixel);
                    }
                    up_btn.Visibility = vis;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void days_list_Loaded(object sender, RoutedEventArgs e)
        {
            var b = VisualTreeHelper.GetChild(this.days_list, 0);
            var s = (ScrollViewer)VisualTreeHelper.GetChild(b, 0);
            s.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (param_pivot.SelectedIndex)
            {
                case 0:
                    days_list.ScrollIntoView(query_d.First());
                    break;
                case 1:
                    unlocks_list.ScrollIntoView(query_l.First());
                    break;
            }
        }

        private void banner_unlocks_grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            unlocks_list.ScrollIntoView(query_l.First());
        }

        private void banner_days_grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            days_list.ScrollIntoView(query_l.First());
        }

        private void expander_range_btn_Click(object sender, RoutedEventArgs e)
        {
            if (range_content_column.Height.IsStar)
            {
                range_content_column.Height = new GridLength(0, GridUnitType.Pixel);
                expand_range.Glyph = "";
                if (!param_content_column.Height.IsStar)
                {
                    param_r.Height = new GridLength(1, GridUnitType.Auto);
                }
            }
            else
            {
                range_content_column.Height = new GridLength(1, GridUnitType.Star);
                expand_range.Glyph = "";
                if (param_content_column.Height.IsStar)
                {
                    param_r.Height = new GridLength(11, GridUnitType.Star);
                }
            }
        }

        private void expander_param_btn_Click(object sender, RoutedEventArgs e)
        {
            if (param_content_column.Height.IsStar)
            {
                param_content_column.Height = new GridLength(0, GridUnitType.Pixel);
                expand_param.Glyph = "";
                param_r.Height = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                param_content_column.Height = new GridLength(1, GridUnitType.Star);
                expand_param.Glyph = "";
                param_r.Height = new GridLength(11, GridUnitType.Star);
            }
        }

        private void vs_group_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            switch (vs_group.CurrentState.Name)
            {
                case "narrow":
                case "tablet":
                    RelativePanel.SetRightOf(endPick, null);
                    RelativePanel.SetRightOf(u_timemin, null);
                    RelativePanel.SetBelow(endPick, startPick);
                    RelativePanel.SetBelow(u_timemin, u_timemax);
                    break;
                case "wide":
                case "laptop":
                    RelativePanel.SetRightOf(endPick, startPick);
                    RelativePanel.SetRightOf(u_timemin, u_timemax);
                    RelativePanel.SetBelow(endPick, null);
                    RelativePanel.SetBelow(u_timemin, null);
                    break;
            }
        }
    }
}
