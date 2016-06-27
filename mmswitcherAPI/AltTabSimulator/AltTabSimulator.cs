using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using mmswitcherAPI;
using mmswitcherAPI.winmsg;
using System.Windows;
using System.Windows.Forms;

namespace mmswitcherAPI.AltTabSimulator
{
    /// <summary>
    /// Класс реализует функционал, аналогичный (максимально приближенный) работе с переключением окон сочетанием клавиш alt+tab
    ///<remarks>Singleton</remarks>
    /// </summary>
    internal sealed class ActiveWindowStack
    {
        private WindowMessagesMonitor _winMesMon;
        private static ActiveWindowStack _instance;
        private static readonly object _locker = new object();

        private List<IntPtr> _windowStack;
        public List<IntPtr> WindowStack
        {
            get { return _windowStack; }
        }

        private bool started = false;
        public bool Started
        {
            get { return started; }
        }

        private bool suspended = true;
        public bool Suspended
        {
            get { return suspended; }
        }

        private ActiveWindowStack(Window window)
        {
            _winMesMon = new WindowMessagesMonitor(window);
        }

        private ActiveWindowStack(Form window)
        {
            //todo
        }

        public static ActiveWindowStack Instance(Form window)
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new ActiveWindowStack(window);
                }
            }
            return _instance;
        }

        public static ActiveWindowStack Instance(Window window)
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new ActiveWindowStack(window);
                }
            }
            return _instance;
        }

        public void Start()
        {
            if (!started && suspended)
            {
                RefreshStack();
                _winMesMon.onMessageTraced += _winMesMon_onMessageTraced;
                HookManager.ForegroundChanged += HookManager_ForegroundChanged;
                started = true;
                suspended = false;
            }
        }

        public void Suspend()
        {
            if (!suspended && started)
            {
                ClearAltTabList();
                _winMesMon.onMessageTraced -= _winMesMon_onMessageTraced;
                HookManager.ForegroundChanged -= HookManager_ForegroundChanged;
                suspended = true;
                started = false;
            }
        }
        /// <summary>
        ///Обновляет список видимых окон, как в списке альт таба (почти как, еще добавлет закрытые окна, окторые в вин10 почему-то остаются висеть в процессах, типа calc.exe)
        /// </summary>
        private void RefreshStack()
        {
            _windowStack = OpenWindowGetter.GetAltTabWindowsHandles();
        }

        private void ClearAltTabList()
        {
            _windowStack.Clear();
        }
        private void HookManager_ForegroundChanged(object sender, EventArgs e)
        {
            bool newWindow = true;
            IntPtr fore = (IntPtr)sender;

            try
            {
                // try to find new foreground window in alt tab list
                foreach (IntPtr hWnd in _windowStack)
                    if (hWnd == fore)
                    {
                        newWindow = false;
                        break;
                    }
                if (!newWindow)
                {
                    IntPtr windowHWnd = _windowStack.Find(x => x == fore);
                    if (windowHWnd != IntPtr.Zero)
                    {
                        _windowStack.Remove(windowHWnd);
                        _windowStack.Insert(0, windowHWnd);
                    }
                }
                //check if window exists, remove from list if not

            }
            catch { }

        }

        void _winMesMon_onMessageTraced(object sender, IntPtr hWnd, Interop.ShellEvents shell)
        {
            if (shell == Interop.ShellEvents.HSHELL_WINDOWDESTROYED)
                _windowStack.Remove(hWnd);

            if (shell == Interop.ShellEvents.HSHELL_WINDOWCREATED && OpenWindowGetter.KeepWindowHandleInAltTabList(hWnd))
                _windowStack.Insert(0, hWnd);
        }
        private void Dispose()
        {
            try
            {
                _winMesMon.onMessageTraced -= _winMesMon_onMessageTraced;
                HookManager.ForegroundChanged -= HookManager_ForegroundChanged;
            }
            catch { }
        }
        ~ActiveWindowStack()
        {
            Dispose();
        }
    }
    public enum SwitchTo
    {
        Tab = 0,
        Window = 1
    }

}
