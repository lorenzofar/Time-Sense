using System;
using System.Text;
using Windows.ApplicationModel.Resources;
using Windows.Globalization.DateTimeFormatting;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Stuff
{
    public sealed class utilities
    {
        public static DateTimeFormatter shortdate_form = new DateTimeFormatter("shortdate", new[] { "it-IT"});
        public static DateTimeFormatter longtime_form = new DateTimeFormatter("longtime", new[] { "it-IT" });
        public static ApplicationDataContainer STATS = ApplicationData.Current.LocalSettings;
        public static ResourceLoader loader
        {
            get { return new ResourceLoader(); }
        }

        public static bool CheckDate(string setting)
        {
            return STATS.Values[setting] == null ? false : true;
        }

        public static string TileXmlBuilder(string usage, int unlocks, bool badge)
        {
            StringBuilder builder = new StringBuilder("<tile>");
            builder.Append("<visual branding='name'>");
            builder.Append("<binding template='TileMedium'>");
            builder.Append("<image src='Assets/icon.png' placement='peek'/>");
            builder.Append("<group>");
            builder.Append("<subgroup>");
            builder.Append($"<text hint-style='captionSubtle'>{utilities.loader.GetString("usage")}</text>");
            builder.Append($"<text>{usage}</text>");
            builder.Append($"<text hint-style='captionSubtle'>{utilities.loader.GetString("unlocks_tile")}</text>");
            builder.Append($"<text>{String.Format(loader.GetString(unlocks == 1 ? "unlock" : "unlocks"), unlocks)}</text>");            
            builder.Append("</subgroup>");
            builder.Append("</group>");            
            builder.Append("</binding>");
            builder.Append("<binding template='TileWide'>");
            builder.Append("<group>");
            builder.Append("<subgroup hint-textStacking='center'>");
            builder.Append("<image src='Assets/icon.png'/>");
            builder.Append("</subgroup>");
            builder.Append("<subgroup hint-textStacking='center'>");
            builder.Append($"<text hint-style='captionSubtle'>{utilities.loader.GetString("usage")}</text>");
            builder.Append($"<text>{usage}</text>");
            builder.Append($"<text hint-style='captionSubtle'>{utilities.loader.GetString("unlocks_tile")}</text>");
            builder.Append($"<text>{String.Format(loader.GetString(unlocks == 1 ? "unlock" : "unlocks"), unlocks)}</text>");
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
            builder.Append($"<text hint-align='center'>{utilities.loader.GetString("usage")}</text>");
            builder.Append($"<text hint-style='subtitle' hint-align='center'>{usage}</text>");
            builder.Append("</subgroup>");
            builder.Append("<subgroup>");
            builder.Append($"<text hint-align='center'>{utilities.loader.GetString("unlocks_tile")}</text>");
            builder.Append($"<text hint-style='subtitle' hint-align='center'>{unlocks}</text>");
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
            StringBuilder builder = new StringBuilder("<toast launch=''>");
            builder.Append("<visual>");
            builder.Append("<binding template='ToastGeneric'>");
            builder.Append($"<text>{loader.GetString("limit_toast_title")}</text>");
            builder.Append($"<text>{String.Format(loader.GetString("limit_toast_content"), limit / 3600, limit / 3600 == 1 ? loader.GetString("hour") : loader.GetString("hours"))}</text>");
            builder.Append("<image placement='appLogoOverride' src='Assets/alert.png'/>");
            builder.Append("</binding>");
            builder.Append("</visual>");
            builder.Append("<actions>");
            builder.Append($"<input defaultInput='{(limit / 3600)}' title='{loader.GetString("limit_toast_helper")}' placeholderContent='{loader.GetString("limit_toast_helper")}' id='time' type='selection'>");
            for(int i= 1; i<6; i++)
            {
                builder.Append($"<selection id='{i}' content='{loader.GetString("limit_toast_" + i.ToString())}'/>");
            }
            builder.Append("</input>");
            builder.Append($"<action activationType='background' arguments='change_threshold' content='{ loader.GetString("limit_toast_btn")}'/>");
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
            return $"{hour}:{minutes}:{seconds}";
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