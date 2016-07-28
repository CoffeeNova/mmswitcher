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
            var aElement = AutomationElement.FromHandle(hWnd);
            if (aElement != null)
            {
                Console.WriteLine(aElement.Current.ClassName);
                _tabNameChanged.Invoke(hWnd, e);
            }
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

        #region TabSelected
        private int _tabSelectedHookHandle;
        private WinApi.WinEventHookProc _tabSelectedDelegate;

        private void TabSelectedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero || hWnd != HWnd)
                return;
            var e = new EventArgs();
            _tabSelected.Invoke(hWnd, e);
        }

        private void EnsureSubscribedToTabSelectedEvent()
        {
            // install Focus hook only if it is not installed and must be installed
            if (_tabSelectedHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                _tabSelectedDelegate = TabSelectedHookProc;
                int processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);
                //install hook
                _tabSelectedHookHandle = SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTION, EventConstants.EVENT_OBJECT_SELECTION, IntPtr.Zero, _tabSelectedDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                //If SetWinEventHook fails.
                if (_tabSelectedHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (_tabSelectedHookHandle == -1)
                    _tabSelectedHookHandle = 0;
            }

        }
        private void TryUnsubscribeFromTabSelectedEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (_tabSelected == null)
            {
                ForceUnsunscribeFromTabSelectedEvent();
            }
        }

        private void ForceUnsunscribeFromTabSelectedEvent()
        {
            if (_tabSelectedHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(_tabSelectedHookHandle);
                //reset invalid handle
                _tabSelectedHookHandle = 0;
                //Free up for GC
                _tabSelectedDelegate = null;
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

        #region TabSelectionCountChanged
        private int _tabSelectionCountChangedHookHandle;
        private WinApi.WinEventHookProc _tabSelectionCountChangedDelegate;

        private void TabSelectionCountChangedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero || hWnd != HWnd)
                return;
            var e = new EventArgs();
            _tabSelectionCountChanged.Invoke(hWnd, e);
        }

        private GCHandle hnd;

        private void EnsureSubscribedToTabSelectionCountChangedEvent()
        {
            if (_tabSelectionCountChangedHookHandle == 0)
            {
                _tabSelectionCountChangedDelegate = TabSelectionCountChangedHookProc;
                int processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);

                hnd = GCHandle.Alloc(_tabSelectionCountChangedHookHandle);
                _tabSelectionCountChangedHookHandle = SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONADD, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, _tabSelectionCountChangedDelegate, processId, 0, WINEVENT_INCONTEXT);
                if (_tabSelectionCountChangedHookHandle == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
                if (_tabSelectionCountChangedHookHandle == -1)
                    _tabSelectionCountChangedHookHandle = 0;
            }

        }
        private void TryUnsubscribeFromTabSelectionCountChangedEvent()
        {
            if (_tabSelectionCountChanged == null)
            {
                ForceUnsunscribeFromTabSelectionCountChangedEvent();
            }
        }

        private void ForceUnsunscribeFromTabSelectionCountChangedEvent()
        {
            if (_tabSelectionCountChangedHookHandle != 0)
            {
                bool result = WinApi.UnhookWinEvent(_tabSelectionCountChangedHookHandle);
                hnd.Free();
                _tabSelectionCountChangedHookHandle = 0;
                _tabSelectionCountChangedDelegate = null;
                if (result == false)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #endregion

        #region TabClosed

        private int _closedHookHandle;
        private WinApi.WinEventHookProc _closedDelegate;

        private void ClosedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
           if (hWnd == IntPtr.Zero || hWnd != HWnd)
                return;
            var e = new EventArgs();
            _tabClosed.Invoke(hWnd, e);
        }

        private void EnsureSubscribedToTabClosedEvent()
        {
            // install Focus hook only if it is not installed and must be installed
            if (_closedHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                _closedDelegate = ClosedHookProc;
                int processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);
                //install hook
                _closedHookHandle = SetWinEventHook(EventConstants.EVENT_OBJECT_DESTROY, EventConstants.EVENT_OBJECT_DESTROY, IntPtr.Zero, _closedDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                //If SetWinEventHook fails.
                if (_closedHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (_closedHookHandle == -1)
                    _closedHookHandle = 0;
            }

        }
        private void TryUnsubscribeFromTabClosedEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (_tabClosed == null)
            {
                ForceUnsunscribeFromTabCLosedEvent();
            }
        }

        private void ForceUnsunscribeFromTabCLosedEvent()
        {
            if (_closedHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(_closedHookHandle);
                //reset invalid handle
                _closedHookHandle = 0;
                //Free up for GC
                _closedDelegate = null;
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
    }
}
