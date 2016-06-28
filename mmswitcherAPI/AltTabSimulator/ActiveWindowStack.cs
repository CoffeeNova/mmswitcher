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
        private WindowMessagesMonitor _winMesMon;
        private static ActiveWindowStack _instance;
        private static readonly object _locker = new object();
        public delegate void StackActionDelegate(StackAction action, IntPtr hWnd);

        /// <summary>
        /// Вызывается при изменении свойства <paramref name="WindowStack"/>.
        /// </summary>
        public event StackActionDelegate onActiveWindowStackChanged;

        private List<IntPtr> _windowStack;
        /// <summary>
        /// Возвращает стэк активных окон.
        /// </summary>
        public List<IntPtr> WindowStack
        {
            get { return _windowStack; }
        }

        private bool started = false;
        
        /// <summary>
        /// Возвращает состояние <paramref name="ActiveWindowStack"/> запущен.
        /// </summary>
        public bool Started
        {
            get { return started; }
        }

        private bool suspended = true;
        /// <summary>
        /// Возвращает состояние <paramref name="ActiveWindowStack"/> приостановлен.
        /// </summary>
        public bool Suspended
        {
            get { return suspended; }
        }

        //конструктор для wpf приложения
        private ActiveWindowStack(Window window)
        {
            _winMesMon = new WindowMessagesMonitor(window);
        }

        //конструктор для Windows.Forms приложения
        private ActiveWindowStack(Form window)
        {
            //todo
        }

        /// <summary>
        /// Инициализирует <paramref name="ActiveWindowStack"/> для приложения <paramref name="Windows.Forms"/>.
        /// </summary>
        /// <param name="window">Окно <paramref name="Windows.Forms"/>.</param>
        /// <returns>Экземпляр класса.</returns>
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
        /// <summary>
        /// Инициализирует <paramref name="ActiveWindowStack"/> для приложения wpf.
        /// </summary>
        /// <param name="window">Окно <paramref name="System.Windows.Window"/>.</param>
        /// <returns>Экземпляр класса.</returns>
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

        /// <summary>
        /// Запускает <paramref name="ActiveWindowStack"/>.
        /// </summary>
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

        /// <summary>
        /// Приостанавливает <paramref name="ActiveWindowStack"/>.
        /// </summary>
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
        void _winMesMon_onMessageTraced(object sender, IntPtr hWnd, Interop.ShellEvents shell)
        {
            if (shell == Interop.ShellEvents.HSHELL_WINDOWDESTROYED)
            {
                _windowStack.Remove(hWnd);
                onActiveWindowStackChanged(StackAction.Removed, hWnd);
            }

            if (shell == Interop.ShellEvents.HSHELL_WINDOWCREATED && OpenWindowGetter.KeepWindowHandleInAltTabList(hWnd))
            {
                _windowStack.Insert(0, hWnd);
                onActiveWindowStackChanged(StackAction.Added, hWnd);
            }
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

    public enum StackAction
    {
        Added,
        Removed,
        MovedToFore
        }

}
