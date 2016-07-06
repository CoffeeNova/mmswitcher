using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace mmswitcherAPI.Messangers.Hooks
{
    internal partial class TabNameHookManager
    {
        private event EventHandler _tabNameChange;
        public IntPtr HWnd { get { return _hWnd; } }

        private IntPtr _hWnd;

        public TabNameHookManager(IntPtr hWnd)
        {
            _hWnd = hWnd;
        }

        public event EventHandler TabNameChange
        {
            add
            {
                EnsureSubscribedToTabNameChangeEvent();
                _tabNameChange += value;
            }
            remove
            {
                _tabNameChange -= value;
                TryUnsubscribeFromTabNameChangeEvent();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinApi.WinEventHookProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);
    }

    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx
    /// </summary>
    internal struct EventConstants
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
}
