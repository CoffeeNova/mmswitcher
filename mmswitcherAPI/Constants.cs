using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmswitcherAPI
{
    internal static class Constants
    {
        internal const int MAXTITLE = 255;

        internal const int KEYEVENTF_EXTENDEDKEY = 0x01;
        internal const int KEYEVENTF_KEYUP = 0x02;
        internal const int UIA_ControlTypePropertyId = 30003;
        internal const int UIA_EditControlTypeId = 50004;
        internal const int KEY_REGISTER_ID = 0;
        internal const int SHIFTKEY_REGISTER_ID = 1;
        internal const int CTRLKEY_REGISTER_ID = 2;
        internal const int ALTKEY_REGISTER_ID = 3;
        internal const int WINKEY_REGISTER_ID = 3;
        internal const int SW_RESTORE = 9;
        internal const int DddRawTargetPath = 0x00000001;
        internal const int MAX_ALLOWED_MESSAGES = 10;
        internal const int WM_GETTEXT = 0x000D;
        internal const int WM_GETTEXTLENGTH = 0x000E;
        internal const int WM_LBUTTONDOWN = 0x0201;
        internal const int WM_LBUTTONUP = 0x0202;
        internal const int WM_LBUTTONDBLCLK = 0x0203;
        internal const int WM_MOUSEACTIVATE = 0x0021;
        internal const int WM_HOTKEY = 0x312;
        internal const int WM_KEYDOWN = 0x100;
        internal const int WM_KEYUP = 0x101;
        internal const int WM_SYSKEYDOWN = 0x104;
        internal const int WM_SYSKEYUP = 0x105;
        internal const int WM_SYSCHAR = 0x106;
        internal const int WM_SYSDEADCHAR = 0x107;
        internal const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        internal const int WM_INPUTLANGCHANGE = 0x0051;
        internal const int MK_LBUTTON = 0x0001;
        internal const int MK_RBUTTON = 0x0002;
        internal const int MA_ACTIVATE = 1;
        internal const int MA_ACTIVATEANDEAT = 2;
        internal const int BM_CLICK = 0x00F5;
        internal const int CTRL_F = 0xA20046; //ctrl+F 

        internal const string CHROME_PROCESS_NAME = "chrome";
        internal const string OPERA_PROCESS_NAME = "opera";
        internal const string FIREFOX_PROCESS_NAME = "firefox";
        internal const string TOR_PROCESS_NAME = "firefox";
        internal const string IE_PROCESS_NAME = "iexplore";

        internal const string CHROME_CLASS_NAME = "Chrome_WidgetWin_1";
        internal const string FIREFOX_CLASS_NAME = "";
        internal const string IE_CLASS_NAME = "";

        internal const string SKYPE_BROWSER_WINDOW_CAPTION = "Skype - ";
        internal const string WHATSAPP_BROWSER_WINDOW_CAPTION = "WhatsApp - ";
        internal const string CHROME_BROWSER_CAPTION = "Google Chrome";
        internal const string OPERA_BROWSER_CAPTION = "Opera";
        internal const string FIREFOX_BROWSER_CAPTION = "";//todo
        internal const string IEXPLORER_BROWSER_CAPTION = "";//todo
        internal const string TOR_BROWSER_CAPTION = "";//todo
        internal const string SKYPE_BROWSER_TAB_CAPTION = "Skype";
        internal const string WHATSAPP_BROWSER_TAB_CAPTION = "WhatsApp";
    }
}
