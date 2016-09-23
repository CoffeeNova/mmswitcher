using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Click_
{
    internal static class Constants
    {
        internal const string _SKYPE_REGISTRYKEY_VALUENAME = "skype";
        internal const string _TELEGRAM_REGISTRYKEY_VALUENAME = "telegram";
        internal const string _WEBSKYPE_REGISTRYKEY_VALUENAME = "webskype";
        internal const string _WEBWHATSAPP_REGISTRYKEY_VALUENAME = "webwhatsapp";
        internal const string _WEBTELEGRAM_REGISTRYKEY_VALUENAME = "webtelegram";
        internal const string _SETTINGS_LOCATION = @"SOFTWARE\Click!";
        internal const string _LAST_USED_KEY = "luk";
        internal const string _MOST_NEW_KEY = "mnk";
        internal const string _ORDER_KEY = "ok";
        internal const string _LAST_USED_MOD = "lum";
        internal const string _MOST_NEW_MOD = "mnm";
        internal const string _ORDER_MOD = "om";
        internal const string _UPDATE_KEY = "uk";
        internal const string _UPDATE_MOD = "um";
        internal const string _LAST_USED_MENU_KEY = "lastused";
        internal const string _MOST_NEW_MENU_KEY = "mostnew";
        internal const string _ORDER_MENU_KEY = "order";
        internal const string _APPLICATION_NAME = "Click!";
        internal const string _NOTIFY_ICON_TEXT = _APPLICATION_NAME;

        internal const string _SKYPE_PROCESSNAME = "Skype";
        internal const string _TELEGRAM_PROCESSNAME = "Telegram";
        internal const string CHROME_PROCESS_NAME = "chrome";
        internal const string OPERA_PROCESS_NAME = "opera";
        internal const string FIREFOX_PROCESS_NAME = "firefox";
        internal const string TOR_PROCESS_NAME = "firefox";
        internal const string IE_PROCESS_NAME = "iexplore";
        internal static readonly string[] _BROWSERS_PROCESSNAMES = new string[4] { "chrome", "opera", "firefox", "iexplore" };
        internal static readonly string[] _BROWSERS_CLASS_NAMES = new string[3] { "Chrome_WidgetWin_1", "firefox_CLASS_NAME", "iexplore_CLASS_NAME" };


        internal const int _SWITCHING_DELAY = 50;
        internal const int _MAXTITLE = 255;
        internal const int _CHECK_FOR_WEB_MESSENGERS_DELAY = 500;

        internal struct MessengerCaption
        {
            public const string Skype = "Skype";
            public const string Telegram = "Telegram";
            public const string WebSkype = "Skype (web version)";
            public const string WebWhatsApp = "WhatsApp (web version)";
            public const string WebTelegram = "Telegram (web version)";
        }

        internal struct ControlCaption
        {
            public const string LastUsed = "Last Used";
            public const string MostNew = "Most New Messages";
            public const string Order = "Order";
        }
    }
}
