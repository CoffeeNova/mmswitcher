using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;
using mmswitcherAPI.Messangers.Web.Browsers;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace mmswitcherAPI.Messangers.Web
{
    /// <summary>
    /// Представляет функционал для отслеживания состояния и управления веб версии популярных мессенджеров.
    /// </summary>
    public abstract class WebMessenger : MessengerBase, IDisposable
    {
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

        //protected AutomationElement PreviousTab
        //{
        //    get
        //    {
        //        try
        //        {
        //            var t = _previousTab.Current.Name;
        //            return _previousTab;
        //        }
        //        catch { return null; }
        //    }
        //}

        //protected AutomationElement CurrentTab
        //{
        //    get
        //    {
        //        try
        //        {
        //            var t = _currentTab.Current.Name;
        //            return _currentTab;
        //        }
        //        catch { return null; }
        //    }
        //}

        protected AutomationElementCollection TabContainer
        {
            get
            {
                return _tabContainer;
            }
        }
        #region protected fields
        protected BrowserSet _browserSet;
        #endregion

        #region private fields
        private WebMessengerHookManager _hManager;
        private AutomationElement _renderTabWidgetAE;
        private IntPtr _renderTabWidgetHandle;
        private IntPtr _previousForegroundWindow = IntPtr.Zero;
        //private AutomationElement _previousTab;
        //private AutomationElement _currentTab;
        private AutomationElementCollection _tabContainer;
        private int _currentTabNumber = 0;
        private int _previousTabNumber = 0;
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

            _hManager.TabSelection += _hManager_TabSelected;
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
        protected override AutomationElement GetMainAutomationElement(Process process, out IntPtr hWnd)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Cчитаю что, для обычного мессенджера достаточно было бы проверить один единственный элемент (контрол или окно в целом) в окне на получение фокуса для того чтобы удостовериться, что фокус получен нашим месседжером. В браузере же происходит следущее: необходимый нам элемент (окно, контрол) получает фокус, но затем его передает на общее окно браузера, или же другая ситуация, которая приводит к зажиганию лишний раз метода OnFocusChanged. Необходимо отфильтровать "ложные срабатывания", для каждого из браузеров исключения будут свои. Передадим хэндл и убедимся, что он не является хэндлом элемента, который получает "паразитный" фокус.</remarks>
        protected override void OnFocusChanged(object sender, EventArgs e)
        {

            if (!_browserSet.OnFocusLostPermission((IntPtr)sender))
                return;
            base.OnFocusChanged(sender, e);
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

        private void _hManager_TabSelected(object sender, EventArgs e)
        {
            //try
            //{
            //    _previousTabNumber = _currentTabNumber;
            //    //var element = sender as AutomationElement;
            //    //try { var name = element.Current.Name; }
            //    //catch { return; }
            //    var hWnd = (IntPtr)sender;
            //    AutomationElementCollection tabItems;
            //    var activeTab = _browserSet.ActiveTab(hWnd, out tabItems);
            //    _tabContainer = tabItems;
            //    _currentTabNumber = (_tabContainer as IEnumerable<AutomationElement>).TakeWhile(x => x.Equals(activeTab)).Count();
            //}
            //catch { Dispose(true); }
        }


        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                if (_hManager != null)
                {
                    _hManager.TabSelection -= _hManager_TabSelected;
                    _hManager.Dispose();
                }
                _browserSet = null;

            }
            disposed = true;
            base.Dispose(disposing);
        }

    }


}
