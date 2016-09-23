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

namespace mmswitcherAPI.Messengers.Web
{
    internal partial class WebMessengerHookManager
    {
        private IntPtr _tabNameChangeHookHandle;
        private WinApi.WinEventHookProc _tabNameChangeDelegate;
        private void TabNameChangeProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;
            var e = new AutomationPropertyChangedEventArgs(AutomationElement.NameProperty, String.Empty, String.Empty);
            var aElement = AutomationElement.FromHandle(hWnd);
            if (aElement != null)
                _tabNameChanged.Invoke(hWnd, e);
        }

        private void TrySubscribeToTabNameChangeEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher != null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { EnsureSubscribedToTabNameChangeEvent(); });
        }

        private void EnsureSubscribedToTabNameChangeEvent()
        {
            if (_tabNameChangeHookHandle == IntPtr.Zero)
            {
                _tabNameChangeDelegate = TabNameChangeProc;
                uint processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);
                _tabNameChangeHookHandle = WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_NAMECHANGE, EventConstants.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, _tabNameChangeDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                if (_tabNameChangeHookHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
                if (_tabNameChangeHookHandle == (IntPtr)(-1))
                    _tabNameChangeHookHandle = IntPtr.Zero;
            }
        }
        private void TryUnsubscribeFromTabNameChangeEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher == null)
                return;
            if (_tabNameChanged == null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { ForceUnsunscribeFromTabNameChangeEvent(); });
            else
                throw new InvalidOperationException("Cant unsubscribe from hook. Event still have subscribers.");
        }

        private void ForceUnsunscribeFromTabNameChangeEvent()
        {
            if (_tabNameChangeHookHandle != IntPtr.Zero)
            {
                bool result = WinApi.UnhookWinEvent(_tabNameChangeHookHandle);
                _tabNameChangeHookHandle = IntPtr.Zero;
                _tabNameChangeDelegate = null;
                if (result == false)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #region TabSelected
        private IntPtr _tabSelectedHookHandle;
        private WinApi.WinEventHookProc _tabSelectedDelegate;

        private void TabSelectedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero || hWnd != HWnd)
                return;
            var e = new EventArgs();
            _tabSelected.Invoke(hWnd, e);
        }

        private void TrySubscribeToTabSelectedEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher != null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { EnsureSubscribedToTabSelectedEvent(); });
        }

        private void EnsureSubscribedToTabSelectedEvent()
        {
            if (_tabSelectedHookHandle == IntPtr.Zero)
            {
                _tabSelectedDelegate = TabSelectedHookProc;
                uint processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);
                _tabSelectedHookHandle = WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTION, EventConstants.EVENT_OBJECT_SELECTION, IntPtr.Zero, _tabSelectedDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                if (_tabSelectedHookHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
                if (_tabSelectedHookHandle == (IntPtr)(-1))
                    _tabSelectedHookHandle = IntPtr.Zero;
            }

        }
        private void TryUnsubscribeFromTabSelectedEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher == null)
                return;
            if (_tabSelected == null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { ForceUnsunscribeFromTabSelectedEvent(); });
            else
                throw new InvalidOperationException("Cant unsubscribe from hook. Event still have subscribers.");
        }

        private void ForceUnsunscribeFromTabSelectedEvent()
        {
            if (_tabSelectedHookHandle != IntPtr.Zero)
            {
                bool result = WinApi.UnhookWinEvent(_tabSelectedHookHandle);
                _tabSelectedHookHandle = IntPtr.Zero;
                _tabSelectedDelegate = null;
                if (result == false)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #endregion

        #region TabSelectionCountChanged
        private IntPtr _tabSelectionCountChangedHookHandle;
        private WinApi.WinEventHookProc _tabSelectionCountChangedDelegate;

        private void TabSelectionCountChangedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero || hWnd != HWnd)
                return;
            var e = new EventArgs();
            _tabSelectionCountChanged.Invoke(hWnd, e);
        }

        private void TrySubscribeToTabSelectionCountChangedEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher != null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { EnsureSubscribedToTabSelectionCountChangedEvent(); });
        }

        private void EnsureSubscribedToTabSelectionCountChangedEvent()
        {
            if (_tabSelectionCountChangedHookHandle == IntPtr.Zero)
            {
                _tabSelectionCountChangedDelegate = TabSelectionCountChangedHookProc;
                uint processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);

                _tabSelectionCountChangedHookHandle = WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONADD, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, _tabSelectionCountChangedDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                if (_tabSelectionCountChangedHookHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
                if (_tabSelectionCountChangedHookHandle == (IntPtr)(-1))
                    _tabSelectionCountChangedHookHandle = IntPtr.Zero;
            }

        }
        private void TryUnsubscribeFromTabSelectionCountChangedEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher == null)
                return;
            if (_tabSelectionCountChanged == null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { ForceUnsunscribeFromTabSelectionCountChangedEvent(); });
            else
                throw new InvalidOperationException("Cant unsubscribe from hook. Event still have subscribers.");
        }

        private void ForceUnsunscribeFromTabSelectionCountChangedEvent()
        {
            if (_tabSelectionCountChangedHookHandle != IntPtr.Zero)
            {
                bool result = WinApi.UnhookWinEvent(_tabSelectionCountChangedHookHandle);
                _tabSelectionCountChangedHookHandle = IntPtr.Zero;
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

        private IntPtr _closedHookHandle;
        private WinApi.WinEventHookProc _closedDelegate;

        private void ClosedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero || hWnd != HWnd)
                return;
            var e = new EventArgs();
            _tabClosed.Invoke(hWnd, e);
        }

        private void TrySubscribeToTabClosedEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher != null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { EnsureSubscribedToTabClosedEvent(); });
        }

        private void EnsureSubscribedToTabClosedEvent()
        {
            if (_closedHookHandle == IntPtr.Zero)
            {
                _closedDelegate = ClosedHookProc;
                uint processId;
                WinApi.GetWindowThreadProcessId(HWnd, out processId);
                _closedHookHandle = WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_DESTROY, EventConstants.EVENT_OBJECT_DESTROY, IntPtr.Zero, _closedDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                if (_closedHookHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
                if (_closedHookHandle == (IntPtr)(-1))
                    _closedHookHandle = IntPtr.Zero;
            }

        }
        private void TryUnsubscribeFromTabClosedEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher == null)
                return;
            if (_tabClosed == null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { ForceUnsunscribeFromTabCLosedEvent(); });
        }

        private void ForceUnsunscribeFromTabCLosedEvent()
        {
            if (_closedHookHandle != IntPtr.Zero)
            {
                bool result = WinApi.UnhookWinEvent(_closedHookHandle);
                _closedHookHandle = IntPtr.Zero;
                _closedDelegate = null;
                if (result == false)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #endregion
    }
}
