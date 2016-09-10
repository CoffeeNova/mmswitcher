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
    /// Реализует перечень активных окон Windows, аналогично перечню Alt+Tab :>
    ///<remarks>Singleton</remarks>
    /// </summary>
    public sealed class ActiveWindowStack
    {
        public static ActiveWindowStack Instance { get; private set; }

        private WindowLifeCycle _winMesMon;
        private static readonly object _locker = new object();
        private AltTabHookManager _hManager;
        private bool _disposed = false;
        public delegate void StackActionDelegate(StackAction action, IntPtr hWnd);

        /// <summary>
        /// Вызывается при изменении свойства <see cref="ActiveWindowStack"/>.
        /// </summary>
        public event StackActionDelegate onActiveWindowStackChanged;

        private static List<IntPtr> _windowStack;
        /// <summary>
        /// Возвращает стэк активных окон.
        /// </summary>
        public List<IntPtr> WindowStack
        {
            get { return _windowStack; }
        }

        private bool _started = false;

        /// <summary>
        /// Возвращает состояние <see cref="ActiveWindowStack"/> запущен.
        /// </summary>
        public bool Started
        {
            get { return _started; }
        }

        private bool _suspended = true;
        /// <summary>
        /// Возвращает состояние <see cref="ActiveWindowStack"/> приостановлен.
        /// </summary>
        public bool Suspended
        {
            get { return _suspended; }
        }

        ////конструктор для wpf приложения
        //private ActiveWindowStack(Window window)
        //{
        //    _winMesMon = new WindowLifeCycle(window);
        //    _hManager = new AltTabHookManager();
        //}

        private ActiveWindowStack()
        {
            _winMesMon = new WindowLifeCycle();
            _hManager = new AltTabHookManager();
        }

        /// <summary>
        /// Инициализирует <see cref="ActiveWindowStack"/>..
        /// </summary>
        /// <returns>Экземпляр класса.</returns>
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
        ///// <summary>
        ///// Инициализирует <see cref="ActiveWindowStack"/> для приложения wpf.
        ///// </summary>
        ///// <param name="window">Окно <see cref="System.Windows.Window"/>.</param>
        ///// <returns>Экземпляр класса.</returns>
        ///// <remarks>Должен быть выполняться в потоке графического интерфейса окна <paramref name="window"/>.</remarks>
        //public static ActiveWindowStack GetInstance(Window window)
        //{
        //    if (Instance == null)
        //    {
        //        lock (_locker)
        //        {
        //            if (Instance == null)
        //                Instance = new ActiveWindowStack(window);
        //        }
        //    }
        //    return Instance;
        //}

        /// <summary>
        /// Запускает <see cref="ActiveWindowStack"/>.
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
        /// Приостанавливает <see cref="ActiveWindowStack"/>.
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
        ///Обновляет список видимых окон, как в списке альт таба (почти как, еще добавлет закрытые окна, окторые в вин10 почему-то остаются висеть в процессах, типа calc.exe)
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

        //Callback функция хука, который отслеживает изменение активного окна Windows
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
                    IntPtr hWnd = _windowStack.Find(x => x == fore);
                    if (hWnd != IntPtr.Zero)
                    {
                        _windowStack.Remove(hWnd);
                        _windowStack.Insert(0, hWnd);
                        onActiveWindowStackChanged(StackAction.MovedToFore, hWnd);
                    }
                }
                //check if window exists, remove from list if not

            }
            catch { }

        }

        //Вызывается при создании или закрытии любого окна Windows
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
    }

    public enum StackAction
    {
        Added,
        Removed,
        MovedToFore
    }

}
