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
        internal const string _LAST_USED_MENU_KEY = "lastused";
        internal const string _MOST_NEW_MENU_KEY = "mostnew";
        internal const string _ORDER_MENU_KEY = "order";
        internal const string _NOTIFY_ICON_TEXT = "Click!";

        internal const string _SKYPE_PROCESSNAME = "Skype";
        internal const string _TELEGRAM_PROCESSNAME = "Telegram";
        internal  static readonly string[] _BROWSERS_PROCESSNAME = new string[4] { "chrome", "opera", "firefox", "iexplore" };
        
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
