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
    /// It provides a list of active windows for Windows, similar to Alt + Tab list.
    ///<remarks>Singleton</remarks>
    /// </summary>
    public sealed class ActiveWindowStack
    {
        private ActiveWindowStack()
        {
            _winMesMon = new WindowLifeCycle();
            _hManager = new AltTabHookManager();
        }

        /// <summary>
        /// Initialize new instance of <see cref="ActiveWindowStack"/>.
        /// </summary>
        /// <returns><see cref="ActiveWindowStack.Instance"/>.</returns>
        public static ActiveWindowStack GetInstance()
        {
            if (Instance == null)
            {
                lock (_locker)
                {
                    if (Instance == null)
                        Instance = new ActiveWindowStack();
                }
            }
            return Instance;
        }

        /// <summary>
        /// Starts <see cref="ActiveWindowStack"/>.
        /// </summary>
        public void Start()
        {
            if (!_started && _suspended)
            {
                RefreshStack();
                _winMesMon.onMessageTraced += _winMesMon_onMessageTraced;
                _hManager.ForegroundChanged += HookManager_ForegroundChanged;
                _started = true;
                _suspended = false;
            }
        }

        /// <summary>
        /// Suspends <see cref="ActiveWindowStack"/>.
        /// </summary>
        public void Suspend()
        {
            if (!_suspended && _started)
            {
                _hManager.ForegroundChanged -= HookManager_ForegroundChanged;
                _winMesMon.onMessageTraced -= _winMesMon_onMessageTraced;
                ClearAltTabList();
                _suspended = true;
                _started = false;
            }
        }

        /// <summary>
        ///Refreshes the list of visible windows, similar to the alt+tab list.
        /// </summary>
        private void RefreshStack()
        {
            _windowStack = OpenWindowGetter.GetAltTabWindowsHandles();
#if DEBUG
            var windows = OpenWindowGetter.GetAltTabWindows();
            var testHexStack = new List<string>();
            testHexStack = _windowStack.Select((IntPtr p) => { return p.ToString("X"); }).ToList();
#endif
        }

        private void ClearAltTabList()
        {
            _windowStack.Clear();
        }

        //Callback hook function which monitors change of the active window.
        private void HookManager_ForegroundChanged(object sender, EventArgs e)
        {
            bool newWindow = true;
            IntPtr fore = (IntPtr)sender;

            // try to find new foreground window in alt tab list
            newWindow = !_windowStack.Any(w => w == fore);
            if (newWindow)
                return;

            IntPtr hWnd = _windowStack.Find(w => w == fore);
            if (hWnd != IntPtr.Zero)
            {
                _windowStack.Remove(hWnd);
                _windowStack.Insert(0, hWnd);
                onActiveWindowStackChanged(StackAction.MovedToFore, hWnd);
            }
        }

        void _winMesMon_onMessageTraced(object sender, IntPtr hWnd, ShellEvents shell)
        {
            if (shell == ShellEvents.HSHELL_WINDOWDESTROYED)
            {
                _windowStack.Remove(hWnd);
                if (onActiveWindowStackChanged != null)
                    onActiveWindowStackChanged(StackAction.Removed, hWnd);
            }

            if (shell == ShellEvents.HSHELL_WINDOWCREATED && OpenWindowGetter.KeepWindowHandleInAltTabList(hWnd))
            {
                _windowStack.Insert(0, hWnd);
                if (onActiveWindowStackChanged != null)
                    onActiveWindowStackChanged(StackAction.Added, hWnd);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                try
                {
                    _hManager.ForegroundChanged -= HookManager_ForegroundChanged;
                    _hManager.Dispose();
                    _winMesMon.onMessageTraced -= _winMesMon_onMessageTraced;
                    _winMesMon.Dispose();
                }
                catch { }
            }
            _disposed = true;
        }
        ~ActiveWindowStack()
        {
            Dispose(false);
        }

        private WindowLifeCycle _winMesMon;
        private static readonly object _locker = new object();
        private AltTabHookManager _hManager;
        private bool _disposed = false;
        private static List<IntPtr> _windowStack;
        private bool _started = false;
        private bool _suspended = true;
        public delegate void StackActionDelegate(StackAction action, IntPtr hWnd);

        /// <summary>
        /// The instance of a class.
        /// </summary>
        public static ActiveWindowStack Instance { get; private set; }

        /// <summary>
        /// Fired when a property <see cref="ActiveWindowStack.WindowStack"/> changes.
        /// </summary>
        public event StackActionDelegate onActiveWindowStackChanged;

        /// <summary>
        /// Returns the stack of active windows.
        /// </summary>
        public List<IntPtr> WindowStack
        {
            get { return _windowStack; }
        }

        /// <summary>
        /// Returns if  <see cref="ActiveWindowStack"/> has started condition.
        /// </summary>
        public bool Started
        {
            get { return _started; }
        }

        /// <summary>
        /// Returns if  <see cref="ActiveWindowStack"/> has suspended condition.
        /// </summary>
        public bool Suspended
        {
            get { return _suspended; }
        }
    }

    public enum StackAction
    {
        Added,
        Removed,
        MovedToFore
    }

}
