using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmswitcherAPI
{
    public static class Constants
    {
        //public const byte VK_TAB = 0x09;
        //public const byte VK_CONTROL = 0x11;
        //public const byte VK_MENU = 0x12;
        //public const byte VK_ESCAPE = 0x1B;
        //public const byte V = 0x56;
        public const int KEYEVENTF_EXTENDEDKEY = 0x01;
        public const int KEYEVENTF_KEYUP = 0x02;
        public const int UIA_ControlTypePropertyId = 30003;
        public const int UIA_EditControlTypeId = 50004;
        public const int KEY_REGISTER_ID = 0;
        public const int SHIFTKEY_REGISTER_ID = 1;
        public const int CTRLKEY_REGISTER_ID = 2;
        public const int ALTKEY_REGISTER_ID = 3;
        public const int WINKEY_REGISTER_ID = 3;
        public const int SW_RESTORE = 9;
        public const int DddRawTargetPath = 0x00000001;
        public const int MAX_ALLOWED_MESSAGES = 10;
        public const int WM_GETTEXT = 0x000D;
        public const int WM_GETTEXTLENGTH = 0x000E;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_HOTKEY = 0x312;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;
        public const int WM_SYSCHAR = 0x106;
        public const int WM_SYSDEADCHAR = 0x107;
        public const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public const int WM_INPUTLANGCHANGE = 0x0051;
        public const int MK_LBUTTON = 0x0001;
        public const int MK_RBUTTON = 0x0002;
        public const int MA_ACTIVATE = 1;
        public const int MA_ACTIVATEANDEAT = 2;
        public const int BM_CLICK = 0x00F5;
        public const int CTRL_F = 0xA20046; //ctrl+F 
        public const int MAXTITLE = 255;
        public const int ENG_LANG_KEYB_LAYOUT = 1033;
        public const int RUS_LANG_KEYB_LAYOUT = 1049;
        public const uint FileAnyAccess = 0;
        public const uint MethodBuffered = 0;
        public const uint FileDeviceKeyboard = 0x0000000b;
        public const uint KLF_ACTIVATE = 0x00000001;
        public const uint KLF_REORDER = 0x00000008;
        public const uint KLF_SUBSTITUTE_OK = 0x00000002;
        public const uint KLF_NOTELLSHELL = 0x00000080;
        public const uint KLF_REPLACELANG = 0x00000010;
        public const uint KLF_SETFORPROCESS = 0x00000100;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        public const uint MOUSEEVENTF_LEFTUP = 0x04;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const uint MOUSEEVENTF_RIGHTUP = 0x10;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x00008000;



        //public const string DEFAUL_WOW_PROCESS_NAME = "Wow-64";
        //public const string DEFAULT_BROWSER_PROCESS_NAME = "iexplore";
        //public const string CHROME_PROCESS_NAME = "chrome";
        //public const string OPERA_PROCESS_NAME = "opera";
        //public const string FIREFOX_PROCESS_NAME = "firefox";
        //public const string TOR_PROCESS_NAME = "firefox";
        //public const string IE_PROCESS_NAME = "iexplore";
        //public const string SETTINGS_LOCATION = @"SOFTWARE\WowDisableWinKey";
        //public const string WOW_PROCESS_KEY_NAME = @"WowProcessName";
        //public const string CHROME_PROCESS_KEYNAME = @"ChromeProcessName";
        //public const string BLOCK_WINKEY = "BlockWinKey";
        //public const string DISABLE_ALL = "DisableAll";
        //public const string ADR_BAR_LANG = "AdressBarLanguage";
        //public const string CAPS_SWITCH = "CapsSwitch";
        //public const string WSS = "wss";
        //public const string WSS_CHROME = "wss_gc";
        //public const string WSS_OPERA = "wss_o";
        //public const string WSS_FIREFOX = "wss_ff";
        //public const string WSS_TOR = "wss_tor";
        //public const string WSS_IE = "wss_ie";
        //public const string CHROME_CLASS_NAME = "Chrome_WidgetWin_1";
        //public const string FIREFOX_CLASS_NAME = "";
        //public const string IE_CLASS_NAME = "";
        //public const string BROWSER_PROCESS_KEYNAME = "BrowserProcess";
        //public const string WEBSKYPE_MODIFIER_CHROME = "wsgc_mod";
        //public const string WEBSKYPE_KEY_CHROME = "wsgc_key";
        //public const string WEBSKYPE_MODIFIER_OPERA = "wso_mod";
        //public const string WEBSKYPE_KEY_OPERA = "wso_key";
        //public const string WEBSKYPE_MODIFIER_FIREFOX = "wsff_mod";
        //public const string WEBSKYPE_KEY_FIREFOX = "wsff_key";
        //public const string WEBSKYPE_MODIFIER_TOR = "wst_mod";
        //public const string WEBSKYPE_KEY_TOR = "wst_key";
        //public const string WEBSKYPE_MODIFIER_IE = "wsie_mod";
        //public const string WEBSKYPE_KEY_IE = "wsie_key";
        //public const string WEBSKYPE_BROWSERS = "wsBrowsers";
        //public const string NOTIFY_ICON_TEXT = "donky\'s tool";
        //public const string SHELL_TRAYWND = "Shell_TrayWnd";
        //public const string GOOGLE_CHROME_
    }
}
