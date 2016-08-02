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

namespace mmswitcherAPI.Messengers
{
    internal partial class MessengerHookManager
    {
        private int _focusChangedHookHandle;
        private WinApi.WinEventHookProc _focusChangedDelegate;
        private void FocusChangedProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;
                _focusChanged.Invoke(hWnd, new EventArgs());
        }

        private void EnsureSubscribedToFocusChangedEvent()
        {
            if (_focusChangedHookHandle == 0)
            {
                _focusChangedDelegate = FocusChangedProc;
                //int processId;
                //WinApi.GetWindowThreadProcessId(HWnd, out processId);
                _focusChangedHookHandle = SetWinEventHook(EventConstants.EVENT_OBJECT_FOCUS, EventConstants.EVENT_OBJECT_FOCUS, IntPtr.Zero, _focusChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
                if (_focusChangedHookHandle == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
                if (_focusChangedHookHandle == -1)
                    _focusChangedHookHandle = 0;
            }
        }
        private void TryUnsubscribeFromFocusChangedEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (_focusChanged == null)
            {
                ForceUnsunscribeFromFocusChangedEvent();
            }
        }

        private void ForceUnsunscribeFromFocusChangedEvent()
        {
            if (_focusChangedHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(_focusChangedHookHandle);
                //reset invalid handle
                _focusChangedHookHandle = 0;
                //Free up for GC
                _focusChangedDelegate = null;
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
