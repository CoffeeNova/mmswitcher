﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Messengers.Web.Browsers;

namespace mmswitcherAPI.Messengers.Web
{
    /// <summary>
    /// Класс отслеживает глобальную активность веб мессенджеров
    /// </summary>
    internal partial class WebMessengerHookManager : IDisposable
    {
        public WebMessengerHookManager()
        {
            WindowsMessagesTrapper.Start();
        }

        //private event EventHandler _messangerTabFocusChanged;
        public IntPtr HWnd { get { return _hWnd; } }
        public BrowserSet _browserSet;

        private IntPtr _hWnd;

        public WebMessengerHookManager(IntPtr hWnd, BrowserSet browserSet)
        {
            _hWnd = hWnd;
            _browserSet = browserSet;
        }

        private event AutomationPropertyChangedEventHandler _tabNameChanged;

        /// <summary>
        /// Происходит при изменении названия любого обекта в процессе веб мессенджера
        /// </summary>
        public event AutomationPropertyChangedEventHandler ObjectNameChange
        {
            add
            {
                TrySubscribeToTabNameChangeEvent();
                _tabNameChanged += value;
            }
            remove
            {
                _tabNameChanged -= value;
                TryUnsubscribeFromTabNameChangeEvent();
            }
        }

        private event EventHandler _tabSelected;

        public event EventHandler TabSelected
        {
            add
            {
                TrySubscribeToTabSelectedEvent();
                _tabSelected += value;
            }
            remove
            {
                _tabSelected -= value;
                TryUnsubscribeFromTabSelectedEvent();
            }
        }

        private event EventHandler _tabSelectionCountChanged;

        public event EventHandler TabSelectionCountChanged
        {
            add
            {
                TrySubscribeToTabSelectionCountChangedEvent();
                _tabSelectionCountChanged += value;
            }
            remove
            {
                _tabSelectionCountChanged -= value;
                TryUnsubscribeFromTabSelectionCountChangedEvent();
            }
        }
        private event EventHandler _tabClosed;

        public event EventHandler TabClosed
        {
            add
            {
                TrySubscribeToTabClosedEvent();
                _tabClosed += value;
            }
            remove
            {
                _tabClosed -= value;
                TryUnsubscribeFromTabClosedEvent();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinApi.WinEventHookProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _browserSet = null;
                _hWnd = IntPtr.Zero;

                try
                {
                    TryUnsubscribeFromTabNameChangeEvent();
                    TryUnsubscribeFromTabSelectedEvent();
                    TryUnsubscribeFromTabSelectionCountChangedEvent();
                    TryUnsubscribeFromTabClosedEvent();
                }
                catch (InvalidOperationException) { }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WebMessengerHookManager()
        {
            _tabNameChanged = null;
            _tabSelected = null;
            _tabSelectionCountChanged = null;
            _tabClosed = null;
            Dispose(false);
        }
    }

    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx
    /// </summary>
    internal struct EventConstants
    {
        public const int EVENT_OBJECT_CREATE = 0x8000;
        public const int EVENT_OBJECT_DESTROY = 0x8001;
        public const int EVENT_OBJECT_SHOW = 0x8002;
        public const int EVENT_OBJECT_REORDER = 0x8004;
        public const int EVENT_OBJECT_FOCUS = 0x8005;
        public const int EVENT_OBJECT_CONTENTSCROLLED = 0x8015;
        public const int EVENT_OBJECT_SELECTION = 0x8006;
        public const int EVENT_OBJECT_SELECTIONADD = 0x0007;
        public const int EVENT_OBJECT_SELECTIONREMOVE = 0x8008;
        public const int EVENT_OBJECT_SELECTIONWITHIN = 0x8009;
        public const int EVENT_SYSTEM_FOREGROUND = 0x0003;
        public const int EVENT_OBJECT_STATECHANGE = 0x800A;
        public const int EVENT_OBJECT_NAMECHANGE = 0x800C;
        public const int EVENT_OBJECT_INVOKED = 0x8013;
    }
}
