using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;
using mmswitcherAPI.Messangers.Web.Browsers;
using mmswitcherAPI.winmsg;

namespace mmswitcherAPI.Messangers.Web
{
    /// <summary>
    /// Представляет функционал для отслеживания состояния и управления веб версии популярных мессенджеров.
    /// </summary>
    public abstract class WebMessenger : MessengerBase, IDisposable
    {
        #region protected fields

        protected BrowserSet _browserSet;
        protected WebMessengerHookManager _hManager;

        #endregion

        private IntPtr _renderWidgetHandle;
        /// <summary>
        /// Хэндл окна, которое является дочерним главному окну визуализации процесса. Непосредственно отображает рабочие элементы веб мессенджера и создается/уничтожается при создании/закрытии вкладки.
        /// </summary>
        protected IntPtr RenderWidgetHandle { get { return _renderWidgetHandle; } } 
        
        private WindowLifeCycle _wmmon;

        public WebMessenger(Process browserProcess)
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
            //_renderWidgetHandle = _browserSet.
            _wmmon = new WindowLifeCycle();
            _wmmon.onMessageTraced += _wmmon_onMessageTraced;
        }

        void _wmmon_onMessageTraced(object sender, IntPtr hWnd, ShellEvents shell)
        {
            if (hWnd != RenderWidgetHandle || shell != ShellEvents.HSHELL_WINDOWDESTROYED)
                return;
            Console.WriteLine(hWnd.ToString("X"));
            return;
            try
            {
                var aElement = AutomationElement.FromHandle(hWnd);
                var aElementName = aElement.Current.Name; 
                InitBrowserSet(base._process);
                if (aElementName.Contains(_browserSet.MessengerCaption))
                    Dispose(true);
            }
                //окно успело задестроится и не доступно, чтобы убедится, что это нужная 
            catch
            {

            }
            
            //var browserWindowHwnd =_browserSet.BrowserWindowAutomationElement(WindowHandle);
            //if(browserWindowHwnd == null)
            //    return;
                
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
            return _browserSet.MessengerTab(widgetHandle);
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

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                _hManager.Dispose();
                _browserSet = null;
            }
            disposed = true;
            base.Dispose(disposing);
        }
    }
}
