using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;
using mmswitcherAPI.Messengers.Web.Browsers;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using mmswitcherAPI.Extensions;


namespace mmswitcherAPI.Messengers.Web
{
    /// <summary>
    /// Представляет функционал для отслеживания состояния и управления веб версии популярных мессенджеров.
    /// </summary>
    public abstract class WebMessenger : MessengerBase, IDisposable
    {

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

            _selectedTab = _tabArray[0];
            _previousSelectedTab = _tabArray[1];
            IncomeMessages = GetMessagesCount(base.IncomeMessageAE);
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
        }

        private void InitBrowserComponents()
        {
            if (_browserComponentsInitialized)
                return;
            InitBrowserSet(base._process);

            CacheAutomationElementProperties(base._windowHandle, ref _browserWindowAE, (s) => _browserSet.BrowserMainWindowAutomationElement(s), AutomationElement.NativeWindowHandleProperty);

            _tabControl = _browserSet.BrowserTabControl(_browserWindowAE);
            _tabCollection = CacheAutomationElementProperties(_tabControl, (s) => _browserSet.TabItems(s), SelectionItemPattern.Pattern);

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
                bool isRestored = Tools.RestoreWindow(widgetHandle);
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
            catch (ElementNotAvailableException)
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
        protected override AutomationElement GetFocusRecieverAutomationElement(IntPtr hWnd)
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
            bool isMinim;
            IntPtr previousForegroundWindow;
            try
            {
                _browserSet.SetForegroundMessengerTab(base.WindowHandle, out previousForegroundWindow, ref _selectedTab, out isMinim);
            }
            catch { Dispose(true); }
        }

        public void ReturnPreviousSelectedTab()
        {
            if (_previousSelectedTab != null && !_previousSelectedTab.Current.BoundingRectangle.Equals(System.Windows.Rect.Empty))
                    _browserSet.FocusBrowserTab(_windowHandle, _previousSelectedTab);
        }

        private delegate AutomationElementCollection GetAutomationCollectionDel(AutomationElement hWnd);
        /// <summary>
        /// Кэширует заданные свойства или паттерны <paramref name="cacheData"/> при создании <see cref="AutomationElementCollection"/> методом, представленным делегатом <paramref name="getAutomationCollectionDel"/> из родительского <paramref name="parrent"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Неправильный тим параметра <paramref name="cacheData"/>.</exception>
        /// <remarks><paramref name="cacheData"/> параметры, должны состояить только из типов <see cref="AutomationProperty"/> и <see cref="AutomationPattern"/>.</remarks>
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
                else if ((ai.GetType() == aPattType))
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

        protected void OnMessageProcessing(object sender, AutomationPropertyChangedEventArgs e)
        {
            var element = sender as AutomationElement;
            if (element == IncomeMessageAE)
                IncomeMessages = GetMessagesCount(element);
        }

        protected abstract int GetMessagesCount(AutomationElement element);

        protected override void OnMessageProcessingSubscribe()
        {
            propertyHandler = new AutomationPropertyChangedEventHandler(OnMessageProcessing);
            Automation.AddAutomationPropertyChangedEventHandler(IncomeMessageAE, TreeScope.Element, propertyHandler, AutomationElement.NameProperty);
            _onMessageProcesseongSubsribed = true;
        }

        protected override void OnMessageProcessingUnSubscribe()
        {
            if (propertyHandler != null && _onMessageProcesseongSubsribed)
                Automation.RemoveAutomationPropertyChangedEventHandler(IncomeMessageAE, propertyHandler);
        }

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
                _tabArray[1] = _tabArray[0];
                _tabCollection = CacheAutomationElementProperties(_tabControl, (s) => _browserSet.TabItems(s), SelectionItemPattern.Pattern);
                _tabArray[0] = _browserSet.SelectedTab(_tabCollection); ;

                if (_tabArray[0]!=null)
                Debug.WriteLine(String.Format("current: {0}", _tabArray[0].Current.BoundingRectangle));
                if (_tabArray[1] != null)
                    Debug.WriteLine(String.Format("prev: {0}", _tabArray[1].Current.BoundingRectangle));

                TabSelectedTime = DateTime.Now;
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

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                if (_hManager != null)
                {
                    //_incomeMessageAE = null;
                    _hManager.TabSelected -= _hManager_TabSelected;
                    //_hManager.TabSelectionCountChanged -= _hManager_TabSelectionCountChanged;
                    _hManager.Dispose();
                }
                _browserSet = null;

            }
            _disposed = true;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Посоеднее время переключения вкладки браузера.
        /// </summary>
        public DateTime TabSelectedTime { get; private set; }

        protected override int IncomeMessages { get; set; }

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
        #endregion

        #region private fields
        private WebMessengerHookManager _hManager;
        private AutomationElement _browserWindowAE;
        private AutomationElement _renderTabWidgetAE;
        private AutomationElement _tabControl;
        private AutomationElementCollection _tabCollection;
        private AutomationElement[] _tabArray = new AutomationElement[2];
        private AutomationElement _selectedTab;
        private AutomationElement _previousSelectedTab;
        private bool _browserComponentsInitialized = false;
        private bool _disposed = false;
        private AutomationPropertyChangedEventHandler propertyHandler = null;
        private bool _onMessageProcesseongSubsribed = false;
        #endregion
    }
}
