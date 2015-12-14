using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Globalization.DateTimeFormatting;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Stuff
{
    public sealed class utilities
    {
        public static DateTimeFormatter shortdate_form = new DateTimeFormatter("shortdate", new[] { "it-IT"});
        public static DateTimeFormatter longtime_form = new DateTimeFormatter("longtime", new[] { "it-IT" });
        public static ApplicationDataContainer STATS = ApplicationData.Current.RoamingSettings;
        public static ResourceLoader loader { get; set; }
        //public static ResourceLoader loader = new ResourceLoader();

        public static async Task<bool> CheckDate(string setting)
        {
            bool data = false;
            Object data_obj = STATS.Values[setting];
            data = data_obj == null ? false : true;
            return data;
        }

        public static string TileXmlBuilder(string usage, int unlocks, bool badge)
        {
            loader = new ResourceLoader();
            StringBuilder builder = new StringBuilder("<tile>");
            builder.Append("<visual branding='name'>");
            builder.Append("<binding template='TileMedium'>");
            builder.Append("<image src='Assets/icon.png' placement='peek'/>");
            builder.Append("<group>");
            builder.Append("<subgroup>");
            builder.Append(String.Format("<text hint-style='captionSubtle'>{0}</text>", utilities.loader.GetString("usage")));
            builder.Append(String.Format("<text>{0}</text>", usage));
            builder.Append(String.Format("<text hint-style='captionSubtle'>{0}</text>", utilities.loader.GetString("unlocks_tile")));
            builder.Append(String.Format("<text>{0}</text>", String.Format(loader.GetString(unlocks == 1 ? "unlock" : "unlocks"), unlocks)));            
            builder.Append("</subgroup>");
            builder.Append("</group>");            
            builder.Append("</binding>");
            builder.Append("<binding template='TileWide'>");
            builder.Append("<group>");
            builder.Append("<subgroup hint-textStacking='center'>");
            builder.Append("<image src='Assets/icon.png'/>");
            builder.Append("</subgroup>");
            builder.Append("<subgroup hint-textStacking='center'>");
            builder.Append(String.Format("<text hint-style='captionSubtle'>{0}</text>", utilities.loader.GetString("usage")));
            builder.Append(String.Format("<text>{0}</text>", usage));
            builder.Append(String.Format("<text hint-style='captionSubtle'>{0}</text>", utilities.loader.GetString("unlocks_tile")));
            builder.Append(String.Format("<text>{0}</text>", String.Format(loader.GetString(unlocks == 1 ? "unlock" : "unlocks"), unlocks)));
            builder.Append("</subgroup>");
            builder.Append("</group>");            
            builder.Append("</binding>");
            builder.Append("<binding template='TileLarge' hint-textStacking='center'>");
            builder.Append("<group hint-textStacking='center'>");
            builder.Append("<subgroup hint-weight='1'/>");
            builder.Append("<subgroup hint-textStacking='top' hint-weight='2'>");
            builder.Append("<image src='Assets/icon.png'/>");
            builder.Append("</subgroup>");
            builder.Append("<subgroup hint-weight='1'/>");
            builder.Append("</group>");
            builder.Append("<group hint-textStacking='center'>");
            builder.Append("<subgroup>");
            builder.Append(String.Format("<text hint-align='center'>{0}</text>", utilities.loader.GetString("usage")));
            builder.Append(String.Format("<text hint-style='subtitle' hint-align='center'>{0}</text>", usage));
            builder.Append("</subgroup>");
            builder.Append("<subgroup>");
            builder.Append(String.Format("<text hint-align='center'>{0}</text>", utilities.loader.GetString("unlocks_tile")));
            builder.Append(String.Format("<text hint-style='subtitle' hint-align='center'>{0}</text>", unlocks));
            builder.Append("</subgroup>");
            builder.Append("</group>");
            builder.Append("</binding>");
            builder.Append("</visual>");
            builder.Append("</tile>");
            if (!badge)
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            }
            return builder.ToString();
        }

        public static string ToastUsageAlert(int limit)
        {
            loader = new ResourceLoader();
            StringBuilder builder = new StringBuilder("<toast>");
            builder.Append("<visual>");
            builder.Append("<binding template='ToastGeneric'>");
            builder.Append(String.Format("<text>{0}</text>", loader.GetString("limit_toast_title")));
            builder.Append(String.Format("<text>{0}</text>", String.Format(loader.GetString("limit_toast_content"), limit / 3600, limit / 3600 == 1 ? loader.GetString("hour") : loader.GetString("hours"))));
            builder.Append("<image placement='appLogoOverride' src='Assets/alert.png'/>");
            builder.Append("</binding>");
            builder.Append("</visual>");
            builder.Append("<actions>");
            builder.Append(String.Format("<input defaultInput='{0}' title='{1}' placeholderContent='{1}' id='time' type='selection'>", (limit / 3600).ToString(), loader.GetString("limit_toast_helper")));
            for(int i= 1; i<6; i++)
            {
                builder.Append(String.Format("<selection id='{0}' content='{1}'/>", i.ToString(), loader.GetString("limit_toast_"+i.ToString())));
            }
            builder.Append("</input>");
            builder.Append(String.Format("<action activationType='background' arguments='change_threshold' content='{0}'/>", loader.GetString("limit_toast_btn")));
            builder.Append("<action activationType='system' arguments='dismiss' content=''/>");
            builder.Append("</actions>");
            builder.Append("</toast>");
            return builder.ToString();
        }


        public static string FormatData(int usage)
        {
            int hour = usage / 3600;
            int minutes = (usage - (hour * 3600)) / 60;
            int seconds = (usage - (hour * 3600)) - (minutes * 60);
            return String.Format("{0}:{1}:{2}", hour, minutes, seconds);
        }

        public static int[] SplitData(int usage)
        {
            int[] data = new int[3];
            data[0] = usage / 3600;
            data[1] = (usage - (data[0] * 3600)) / 60;
            data[2] = (usage - (data[0] * 3600)) - (data[1] * 60);
            return data;
        }
    }
}
