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
        private AutomationElement _browserWindowAE;
        private AutomationElement _renderTabWidgetAE;
        private AutomationElement _tabControl;
        private AutomationElementCollection _tabCollection;
        //private IntPtr _renderTabWidgetHandle;
        private IntPtr _previousForegroundWindow = IntPtr.Zero;
        //private AutomationElement _previousTab;
        //private AutomationElement _currentTab;
        private AutomationElementCollection _tabContainer;
        private System.Windows.Rect _currentTabBounding = new System.Windows.Rect();
        private System.Windows.Rect _previousTabBounding = new System.Windows.Rect();
        private bool _browserComponentsInitialized = false;
        #endregion

        protected WebMessenger(Process browserProcess)
            : base(browserProcess)
        {
            if (browserProcess == null)
                throw new ArgumentException();
            if (_browserSet == null)
                InitBrowserSet(browserProcess);

            InitBrowserComponents();

            if (_hManager == null)
                InitHookManager();

            // _hManager = new WebMessengerHookManager(base.WindowHandle, _browserSet);
            // _hManager.TabClosed += _hManager_TabClosed;



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
            if (hWnd == (IntPtr)_renderTabWidgetAE.Cached.NativeWindowHandle)
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

        private void InitHookManager()
        {
            InitBrowserSet(base._process);

            if (_hManager != null) return;
            _hManager = new WebMessengerHookManager(base._windowHandle, _browserSet);
            _hManager.TabSelected += _hManager_TabSelected;
            //_hManager.TabSelectionCountChanged += _hManager_TabSelectionCountChanged;
        }

        private void InitBrowserComponents()
        {
            if (_browserComponentsInitialized)
                return;
            InitBrowserSet(base._process);

            //_tabControl = TreeWalker.RawViewWalker.GetParent(base.MessengerAE);
            //_browserWindowAE = _browserSet.BrowserMainWindowAutomationElement(base._windowHandle);
            CacheAutomationElementProperties(base._windowHandle, ref _browserWindowAE, (s) => _browserSet.BrowserMainWindowAutomationElement(s), AutomationElement.NativeWindowHandleProperty);
            _tabControl = _browserSet.BrowserTabControl(_browserWindowAE);
            _tabCollection = CacheAutomationElementProperties(_tabControl, (s) => _browserSet.TabItems(s), SelectionItemPattern.Pattern);
            //_tabCollection = _browserSet.TabItems(_tabControl);
            //_renderTabWidgetAE = _browserSet.BrowserTabControlWindowAutomationElement(base._windowHandle);
            CacheAutomationElementProperties(base._windowHandle, ref _renderTabWidgetAE, (s) => _browserSet.BrowserTabControlWindowAutomationElement(s), AutomationElement.NativeWindowHandleProperty);

            _browserComponentsInitialized = true;
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

        private delegate AutomationElementCollection GetAutomationCollectionDel(AutomationElement hWnd);
        /// <summary>
        /// Кэширует заданные свойства или паттерны <paramref name="cacheData"/> при создании <see cref="AutomationElementCollection"/> методом, представленным делегатом <paramref name="getAutomationCollectionDel"/> из родительского <paramref name="parrent"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Неправильный тим параметра <paramref name="cacheData"/>.</exception>
        /// <remarks><paramref name="cacheData"/> параметры, должны состояить только из типов <see cref="AutomationProperty"/> и <see cref="AutomationPAttern"/>.</remarks>
        private AutomationElementCollection CacheAutomationElementProperties(AutomationElement parrent, GetAutomationCollectionDel getAutomationCollectionDel, params AutomationIdentifier[] cacheData)
        {
            AutomationElementCollection aeCollection;
            var cacheRequest = new CacheRequest();
            foreach (var ai in cacheData)
            {
                var aPropType = typeof(AutomationProperty);
                var aPattType = typeof(AutomationPattern);
                if (ai.GetType() == aPropType)
                    cacheRequest.Add((AutomationProperty)ai);
                else if((ai.GetType() == aPattType))
                    cacheRequest.Add((AutomationPattern)ai);
                else
                    throw new ArgumentException(string.Format("CacheData has a wrong type."));
            }
            using (cacheRequest.Activate())
            {
                aeCollection = getAutomationCollectionDel.Invoke(parrent);
            }
            return aeCollection;
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
            if (!_browserComponentsInitialized)
                InitBrowserComponents();

            var hWnd = (IntPtr)sender;
            var browserHandle = (IntPtr)_browserWindowAE.Cached.NativeWindowHandle;
            if (!hWnd.Equals(browserHandle))
                return;

            try
            {
                Console.WriteLine("");

                _previousTabBounding = _currentTabBounding;

                _tabCollection = CacheAutomationElementProperties(_tabControl, (s) => _browserSet.TabItems(s), SelectionItemPattern.Pattern);

                var t = _tabCollection[0].GetCachedPattern(SelectionItemPattern.Pattern);
                var activeTab = _browserSet.ActiveTab(_tabCollection, _browserWindowAE);
                if (activeTab == null)
                    return;
                _currentTabBounding = activeTab.Current.BoundingRectangle;

                Console.WriteLine(String.Format("current: {0} | prev: {1}", _currentTabBounding, _previousTabBounding));
            }
            catch { Dispose(true); }
        }

        void _hManager_TabSelectionCountChanged(object sender, EventArgs e)
        {
            if (!_browserComponentsInitialized)
                InitBrowserComponents();

            var browserHandle = (IntPtr)_browserWindowAE.Cached.NativeWindowHandle;
            if (!((IntPtr)sender).Equals(browserHandle))
                return;
            _tabCollection = _browserSet.TabItems(_tabControl);
            Console.WriteLine(_tabCollection.Count);
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
                    _hManager.TabSelected -= _hManager_TabSelected;
                    _hManager.TabSelectionCountChanged -= _hManager_TabSelectionCountChanged;
                    _hManager.Dispose();
                }
                _browserSet = null;

            }
            disposed = true;
            base.Dispose(disposing);
        }

    }


}
