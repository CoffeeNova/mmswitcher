using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using mmswitcherAPI;
using System.Windows.Automation;

namespace mmswitcherAPI.Messangers.Web
{
    internal partial class WebMessengerHookManager
    {
        private int _tabNameChangeHookHandle;
        private WinApi.WinEventHookProc _tabNameChangeDelegate;
        private void TabNameChangeProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;
            var e = new AutomationPropertyChangedEventArgs(AutomationElement.NameProperty, String.Empty, String.Empty);
            _tabNameChanged.Invoke(hWnd, e);
        }

        //protected void

        private void EnsureSubscribedToTabNameChangeEvent()
        {
            // install Focus hook only if it is not installed and must be installed
            if (_tabNameChangeHookHandle == 0)
            {
                _tabNameChangeDelegate = TabNameChangeProc;
                int processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);
                //install hook
                _tabNameChangeHookHandle = SetWinEventHook(EventConstants.EVENT_OBJECT_NAMECHANGE, EventConstants.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, _tabNameChangeDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                //If SetWinEventHook fails.
                if (_tabNameChangeHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (_tabNameChangeHookHandle == -1)
                    _tabNameChangeHookHandle = 0;
            }
        }
        private void TryUnsubscribeFromTabNameChangeEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (_tabNameChanged == null)
            {
                ForceUnsunscribeFromTabNameChangeEvent();
            }
        }

        private void ForceUnsunscribeFromTabNameChangeEvent()
        {
            if (_tabNameChangeHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(_tabNameChangeHookHandle);
                //reset invalid handle
                _tabNameChangeHookHandle = 0;
                //Free up for GC
                _tabNameChangeDelegate = null;
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

        private int _focusHookHandle;
        private WinApi.WinEventHookProc _focusDelegate;

        private void FocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;
            AutomationElement aElement;
            try
            {
                aElement = AutomationElement.FromHandle(hWnd);
            }
            catch { return; }
            AutomationFocusChangedEventArgs e = new AutomationFocusChangedEventArgs(idObject, idChild);

            _focusChanged.Invoke(aElement, e);
        }

        private void EnsureSubscribedToFocusEvent()
        {
            // install Focus hook only if it is not installed and must be installed
            if (_focusHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                _focusDelegate = FocusHookProc;
                int processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);
                //install hook
                _focusHookHandle = SetWinEventHook(_browserSet.FocusHookEventConstant, _browserSet.FocusHookEventConstant, IntPtr.Zero, _focusDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
                //If SetWinEventHook fails.
                if (_focusHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (_focusHookHandle == -1)
                    _focusHookHandle = 0;
            }

        }
        private void TryUnsubscribeFromFocusEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (_focusChanged == null)
            {
                ForceUnsunscribeFromFocusEvent();
            }
        }

        private void ForceUnsunscribeFromFocusEvent()
        {
            if (_focusHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(_focusHookHandle);
                //reset invalid handle
                _focusHookHandle = 0;
                //Free up for GC
                _focusDelegate = null;
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
    }
}
