using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace mmswitcherAPI.Messangers
{
    internal partial class WebHookManager
    {
        #region GotFocus
        private int _gotFocusHookHandle;
        private WinApi.WinEventHookProc _gotFocusDelegate;

        private void GotFocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            EventArgs e = new EventArgs();
            _gotFocus.Invoke(hWnd, e);
        }

        private void EnsureSubscribedToGotFocusEvent(InternetBrowser browser, Process process)
        {
            // install Focus hook only if it is not installed and must be installed
            if (_gotFocusHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                _gotFocusDelegate = GotFocusHookProc;
                //install hook
                _gotFocusHookHandle = WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, _gotFocusDelegate, process.Id, 0, WINEVENT_OUTOFCONTEXT);
                //_gotFocusHookHandle = ChooseWinEventHook(browser, process, _gotFocusDelegate);
                //If SetWinEventHook fails.
                if (_gotFocusHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (_gotFocusHookHandle == -1)
                    _gotFocusHookHandle = 0;
            }

        }
        private void TryUnsubscribeFromGotFocusEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (_gotFocus == null)
            {
                ForceUnsunscribeFromGotFocusEvent();
            }
        }

        private void ForceUnsunscribeFromGotFocusEvent()
        {
            if (_gotFocusHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(_gotFocusHookHandle);
                //reset invalid handle
                _gotFocusHookHandle = 0;
                //Free up for GC
                _gotFocusDelegate = null;
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }
        #endregion

        #region LostFocus
        private int _lostFocusHookHandle;
        private WinApi.WinEventHookProc _lostFocusDelegate;

        private void LostFocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            EventArgs e = new EventArgs();
            _lostFocus.Invoke(hWnd, e);
        }

        private void EnsureSubscribedToLostFocusEvent(InternetBrowser browser, Process process)
        {
            // install Focus hook only if it is not installed and must be installed
            if (_lostFocusHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                _lostFocusDelegate = LostFocusHookProc;
                //install hook
                _lostFocusHookHandle = ChooseWinEventHook(browser, process, _lostFocusDelegate);
                //If SetWinEventHook fails.
                if (_lostFocusHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (_lostFocusHookHandle == -1)
                    _lostFocusHookHandle = 0;
            }

        }
        private void TryUnsubscribeFromLostFocusEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (_gotFocus == null)
            {
                ForceUnsunscribeFromLostFocusEvent();
            }
        }

        private void ForceUnsunscribeFromLostFocusEvent()
        {
            if (_lostFocusHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(_lostFocusHookHandle);
                //reset invalid handle
                _lostFocusHookHandle = 0;
                //Free up for GC
                _lostFocusDelegate = null;
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }
        #endregion
        //!!! NOT TESTED FOR OTHER BROWSERS THEN CHROME
        private int ChooseWinEventHook(InternetBrowser browser, Process process, WinApi.WinEventHookProc wehp)
        {
            switch (browser)
            {
                case InternetBrowser.GoogleChrome:
                    return WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, wehp, process.Id, 0, WINEVENT_OUTOFCONTEXT);
                case InternetBrowser.Opera:
                    return WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_SHOW, EventConstants.EVENT_OBJECT_SHOW, IntPtr.Zero, wehp, process.Id, 0, WINEVENT_OUTOFCONTEXT);
                case InternetBrowser.Firefox:
                    return WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, wehp, process.Id, 0, WINEVENT_OUTOFCONTEXT);
                case InternetBrowser.TorBrowser:
                    return WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, wehp, process.Id, 0, WINEVENT_OUTOFCONTEXT);
                case InternetBrowser.InternetExplorer:
                    return WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, wehp, process.Id, 0, WINEVENT_OUTOFCONTEXT);
                default:
                    return -1;
            }
        }
        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx
        /// </summary>
        private struct EventConstants
        {
            public const int EVENT_OBJECT_CREATE = 0x8000;
            public const int EVENT_OBJECT_SHOW = 0x8002;
            public const int EVENT_OBJECT_REORDER = 0x8004;
            public const int EVENT_OBJECT_FOCUS = 0x8005;
            public const int EVENT_OBJECT_CONTENTSCROLLED = 0x8015;
            public const int EVENT_OBJECT_SELECTION = 0x8006;
            public const int EVENT_OBJECT_SELECTIONREMOVE = 0x8008;
            public const int EVENT_OBJECT_SELECTIONWITHIN = 0x8009;
            public const int EVENT_SYSTEM_FOREGROUND = 0x0003;
            public const int EVENT_OBJECT_SELECTIONADD = 0x0007;
            public const int EVENT_OBJECT_STATECHANGE = 0x800A;
            public const int EVENT_OBJECT_NAMECHANGE = 0x800C;
        }
        /// <summary>
        /// The callback function is not mapped into the address space of the process that generates the event. Because the hook function is called across process boundaries, the system must queue events. Although this method is asynchronous, events are guaranteed to be in sequential order.
        /// </summary>
        private const int WINEVENT_OUTOFCONTEXT = 0;
    }
}
