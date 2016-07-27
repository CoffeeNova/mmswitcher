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

        private int _selectionHookHandle;
        private WinApi.WinEventHookProc _selectionDelegate;

        private void SelectionHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;
            //AutomationElement aElement;
            //try
            //{
            //    aElement = AutomationElement.FromHandle(hWnd);
            //}
            //catch { return; }
            //AutomationFocusChangedEventArgs e = new AutomationFocusChangedEventArgs(idObject, idChild);

            var e = new EventArgs();
            _selection.Invoke(hWnd, e);
        }

        private void EnsureSubscribedToTabSelectionEvent()
        {
            // install Focus hook only if it is not installed and must be installed
            if (_selectionHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                _selectionDelegate = SelectionHookProc;
                int processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);
                //install hook
                _selectionHookHandle = SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTION, EventConstants.EVENT_OBJECT_SELECTIONWITHIN, IntPtr.Zero, _selectionDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                //If SetWinEventHook fails.
                if (_selectionHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (_selectionHookHandle == -1)
                    _selectionHookHandle = 0;
            }

        }
        private void TryUnsubscribeFromTabSelectionEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (_selection == null)
            {
                ForceUnsunscribeFromTabSelectionEvent();
            }
        }

        private void ForceUnsunscribeFromTabSelectionEvent()
        {
            if (_selectionHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(_selectionHookHandle);
                //reset invalid handle
                _selectionHookHandle = 0;
                //Free up for GC
                _selectionDelegate = null;
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

        #region TabClosed

        private int _closedHookHandle;
        private WinApi.WinEventHookProc _closedDelegate;

        private void ClosedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;
            //AutomationElement aElement;
            //try
            //{
            //    aElement = AutomationElement.FromHandle(hWnd);
            //}
            //catch { return; }
            //AutomationFocusChangedEventArgs e = new AutomationFocusChangedEventArgs(idObject, idChild);

            //_selected.Invoke(aElement, e);
        }

        private void EnsureSubscribedToTabClosedEvent()
        {
            // install Focus hook only if it is not installed and must be installed
            if (_selectionHookHandle == 0)
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
            if (_selection == null)
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
