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
        #region DocusChanged
        private IntPtr _focusChangedHookHandle;
        private WinApi.WinEventHookProc _focusChangedDelegate;
        private void FocusChangedProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;
            try
            {
                _focusChanged.Invoke(hWnd, new EventArgs());
            }
            catch { }
        }

        private void TrySubscribeToFocusChangedEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher != null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { EnsureSubscribedToFocusChangedEvent(); });
        }

        private void EnsureSubscribedToFocusChangedEvent()
        {
            if (_focusChangedHookHandle == IntPtr.Zero)
            {
                _focusChangedDelegate = FocusChangedProc;
                //int processId;
                //WinApi.GetWindowThreadProcessId(HWnd, out processId);
                _focusChangedHookHandle = WinApi.SetWinEventHook(EventConstants.EVENT_OBJECT_FOCUS, EventConstants.EVENT_OBJECT_FOCUS, IntPtr.Zero, _focusChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
                if (_focusChangedHookHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
                if (_focusChangedHookHandle == (IntPtr)(-1))
                    _focusChangedHookHandle = IntPtr.Zero;
            }
        }
        private void TryUnsubscribeFromFocusChangedEvent()
        {
            if (WindowsMessagesTrapper.Dispatcher == null)
                return;
            if (_focusChanged == null) //unhook win event from WindowsMessagesTrapper Form thread
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { ForceUnsunscribeFromFocusChangedEvent(); });
            else
                throw new InvalidOperationException("Cant unsubscribe from hook. Event still have subscribers.");
        }

        private void ForceUnsunscribeFromFocusChangedEvent()
        {
            if (_focusChangedHookHandle != IntPtr.Zero)
            {
                bool result = WinApi.UnhookWinEvent(_focusChangedHookHandle);
                _focusChangedHookHandle = IntPtr.Zero;
                _focusChangedDelegate = null;
                if (result == false)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }
        #endregion

        #region EventListener

        private IntPtr _eventsListenerHookHandle;
        private WinApi.WinEventHookProc _eventsListenerDelegate;
        private void EventsListenerProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;
            _eventsListener.Invoke(hWnd, new EventArgs());
        }

        private void TrySubscribeToEventsListener()
        {
            if (WindowsMessagesTrapper.Dispatcher != null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { EnsureSubscribedToEventsListener(); });
        }

        private void EnsureSubscribedToEventsListener()
        {
            if (_eventsListenerHookHandle == IntPtr.Zero)
            {
                _eventsListenerDelegate = EventsListenerProc;
                uint processId;
                var threadId = WinApi.GetWindowThreadProcessId(HWnd, out processId);
                _eventsListenerHookHandle = WinApi.SetWinEventHook(0x00000001, 0x7FFFFFFF, IntPtr.Zero, _eventsListenerDelegate, processId, 0, WINEVENT_OUTOFCONTEXT);
                if (_eventsListenerHookHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
                if (_eventsListenerHookHandle == (IntPtr)(-1))
                    _eventsListenerHookHandle = IntPtr.Zero;
            }
        }
        private void TryUnsubscribeFromEventsListener()
        {
            if (WindowsMessagesTrapper.Dispatcher == null)
                return;
            if (_eventsListener == null)
                WindowsMessagesTrapper.Dispatcher.Invoke(() => { ForceUnsunscribeFromEventsListener(); });
            else
                throw new InvalidOperationException("Cant unsubscribe from hook. Event still have subscribers.");
        }

        private void ForceUnsunscribeFromEventsListener()
        {
            if (_eventsListenerHookHandle != IntPtr.Zero)
            {
                bool result = WinApi.UnhookWinEvent(_eventsListenerHookHandle);
                _eventsListenerHookHandle = IntPtr.Zero;
                _eventsListenerDelegate = null;
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
