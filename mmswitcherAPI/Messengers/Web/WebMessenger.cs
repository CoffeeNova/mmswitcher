using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;
using mmswitcherAPI.Messangers.Web.Browsers;


namespace mmswitcherAPI.Messangers.Web
{
    /// <summary>
    /// Представляет функционал для отслеживания состояния и управления веб версии популярных мессенджеров.
    /// </summary>
    public abstract class WebMessenger : MessengerBase, IDisposable
    {
        private AutomationElement _renderTabWidgetAE;
        /// <summary>
        /// <see cref="AutomationElement"/> окна, которое непосредственно отображает рабочие элементы веб мессенджера и создается/уничтожается при создании/закрытии вкладки.
        /// </summary>
        protected AutomationElement RenderTabWidgetAE
        {
            get
            {
                try
                {
                    var t = _renderTabWidgetAE.Current.Name;
                    return _renderTabWidgetAE;
                }
                catch { Dispose(true); return null; }
            }
        }

        #region protected fields
        protected BrowserSet _browserSet;
        protected WebMessengerHookManager _hManager;
        #endregion

        #region private fields
        private IntPtr _renderTabWidgetHandle;
        private IntPtr _previousForegroundWindow = IntPtr.Zero;
        private AutomationElement _previousTab;
        #endregion

        protected WebMessenger(Process browserProcess)
            : base(browserProcess)
        {
            if (browserProcess == null)
                throw new ArgumentException();
            if (_browserSet == null)
                InitBrowserSet(browserProcess);
            if (_hManager == null)
                InitHookManager(browserProcess);
            // _hManager = new WebMessengerHookManager(base.WindowHandle, _browserSet);
            // _hManager.TabClosed += _hManager_TabClosed;
            _renderTabWidgetAE = _browserSet.BrowserTabControlWindowAutomationElement(base._windowHandle);
            _renderTabWidgetHandle = (IntPtr)_renderTabWidgetAE.Current.NativeWindowHandle;

        }

        /// <summary>
        /// Добавляет к методу <see cref="BaseMessenger.OnMessageTraced"/> отслеживаение закрытия вкладки браузера и перенос вкладки в новое окно. Вызывает деструктор.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="hWnd">Дескриптор элемента.</param>
        /// <param name="shell">Перечисление shell событий.</param>
        protected override void OnMessageTraced(object sender, IntPtr hWnd, ShellEvents shell)
        {
            if (shell != ShellEvents.HSHELL_WINDOWDESTROYED)
                return;
            if (hWnd == _renderTabWidgetHandle)
                Dispose(true);
            base.OnMessageTraced(sender, hWnd, shell);
        }

        void _hManager_TabClosed(object sender, AutomationFocusChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private List<IntPtr> GetWidgetHandles(int processId, string className)
        {
            return Tools.GetWidgetWindowHandles(processId, className);
        }

        private void InitBrowserSet(Process browserProcess)
        {
            if (_browserSet != null) return;

            var browser = Tools.DefineBrowserByProcessName(browserProcess.ProcessName);
            switch (browser)
            {
                case InternetBrowser.GoogleChrome:
                    _browserSet = new GoogleChromeSet(Messenger);
                    break;
                case InternetBrowser.Opera:
                    _browserSet = new OperaSet(Messenger);
                    break;
                case InternetBrowser.Firefox:
                    _browserSet = new FirefoxSet(Messenger);
                    break;
                case InternetBrowser.TorBrowser:
                    _browserSet = new TorBrowserSet(Messenger);
                    break;
            }
        }

        private void InitHookManager(Process browserProcess)
        {
            if (_hManager != null) return;
            _hManager = new WebMessengerHookManager(base._windowHandle, _browserSet);
        }

        /// <summary>
        /// Производит поиск вкладки веб мессенджера в процессе <paramref name="process"/>, так же возвращает дескриптор окна, в котором открыта вкладка веб мессенджера.
        /// </summary>
        /// <param name="process">Процесс браузера.</param>
        /// <param name="hWnd">Дескриптор окна, в котором открыта вкладка.</param>
        /// <returns>Вкладку веб мессенджера в виде <see cref="AutomationElement"/> и дескриптор окна, в котором открыта эта вкладка.</returns>
        /// В роли <see cref="AutomationElement"/> для веб версии мессенджеров является контрол вкладки браузера, это позволит управлять переключением. 
        protected override AutomationElement GetAutomationElement(Process process, out IntPtr hWnd)
        {
            AutomationElement messengerElement = null;
            hWnd = IntPtr.Zero;
            //узнаeм имя класса которому принадлежит главное окно (для браузеров имя класса будет одинаково для всех окон)
            string className = Tools.GetClassName(process.MainWindowHandle);
            var widgetHandles = GetWidgetHandles(process.Id, className);
            //произведем поиск по виджетам браузера в поиске вкладки мессенджера
            foreach (IntPtr widgetHandle in widgetHandles)
            {
                bool isRestored = Tools.RestoreMinimizedWindow(widgetHandle);
                var tabElement = DefineTabAutomationAelement(widgetHandle);

                if (isRestored)
                    Tools.MinimizeWindow(widgetHandle);

                if (tabElement != null)
                {
                    messengerElement = tabElement;
                    hWnd = widgetHandle;
                    break;
                }
            }
            return messengerElement;
        }

        protected virtual AutomationElement DefineTabAutomationAelement(IntPtr widgetHandle)
        {
            InitBrowserSet(base._process);
            AutomationElement mesTab;
            try
            {
                mesTab = _browserSet.MessengerTab(widgetHandle);
            }
            catch (ElementNotAvailableException ex)
            {
                return null;
            }
            return mesTab;
        }

        /// <summary>
        /// Получает модель автоматизации пользовательского интерфейса, которая служит индикатором получения фокуса при переключении на вкладку мессенджера в браузере.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
        /// <returns></returns>
        protected override AutomationElement GetFocusHandlerAutomationElement(IntPtr hWnd)
        {
            if (_browserSet == null)
                InitBrowserSet(base._process);

            return _browserSet.MessengerFocusAutomationElement(hWnd);
        }

        /// <summary>
        /// Получает модель автоматизации пользовательского интерфейса, которая служит индикатором получения нового сообщения мессенджером.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
        /// <returns></returns>
        protected override AutomationElement GetIncomeMessageAutomationElement(IntPtr hWnd)
        {
            return _browserSet.MessengerIncomeMessageAutomationElement(hWnd);
        }

        public override void SetForeground()
        {
            IntPtr initFore;
            AutomationElement initTab;
            bool isMinim;
            try
            {
                _browserSet.SetForegroundMessengerTab(base.WindowHandle, out initFore, out initTab, out isMinim);
                //_previousForegroundWindow = initFore;
                //_previousTab = initTab;
            }
            catch { Dispose(true); }
        }

        //public override void ReturnForeground()
        //{
        //    if (_previousForegroundWindow == IntPtr.Zero || _previousTab == null)
        //        return;
        //    try
        //    {
        //        if (_previousForegroundWindow != base.WindowHandle)
        //            WinApi.SetForegroundWindow(_previousForegroundWindow);
        //        else
        //            _browserSet.FocusBrowserTab(base.WindowHandle, _previousTab);
        //        _previousForegroundWindow = IntPtr.Zero;
        //        _previousTab = null;
        //    }
        //    catch { Dispose(true); }
        //}

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                if (_hManager != null)
                    _hManager.Dispose();
                _browserSet = null;

            }
            disposed = true;
            base.Dispose(disposing);
        }
    }
}
