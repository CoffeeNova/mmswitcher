using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Messangers.Web.Browsers;

namespace mmswitcherAPI.Messangers.Web
{
    public sealed class WebSkype : WebMessenger, IDisposable
    {
        public override Messenger Messenger
        {
            get { return Messenger.Skype; }
        }

        WebMessengerHookManager _hManager;
        private IBrowserSet _browserSet;
        private AutomationElement _browserWindow;

        public WebSkype(Process browserProcess)
            : base(browserProcess)
        {
            if (browserProcess == null)
                throw new ArgumentException();
            if (_browserSet == null)
                InitBrowserSet(browserProcess);
            if (_hManager == null)
                InitHookManager(browserProcess);
            _browserWindow = _browserSet.BrowserWindowAutomationElement(base._windowHandle);
            _hManager = new WebMessengerHookManager(base.WindowHandle, _browserSet);
        }

        ///// <summary>
        ///// Callback функция события изменения имени любого обекта внутри веб скайпа.
        ///// </summary>
        ///// <param name="sender">Хэндл объекта, у которого поменялось название</param>
        ///// <param name="e"></param>
        //private void _hManager_TabNameChange(IntPtr sender, EventArgs e)
        //{
        //    try
        //    {
        //        //Console.WriteLine("GOT");
        //        AutomationElement lync = AutomationElement.FromHandle(sender);
        //        //определим по названию, что это нужный нам объект - окно в котором открыт веб скайп, прочитаем есть ли новые сообщения и передадим в свойство базового класса.
        //        if (lync.Current.Name.Contains("Skype - "))
        //            base.IncomeMessages = IncomeMessagesDetect(base._messengerAE) ? true : false;
        //    }
        //    catch { }
        //}

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
            InitBrowserSet(browserProcess);
            if (_hManager != null) return;

            _hManager = new WebMessengerHookManager(base._windowHandle, _browserSet);
        }

        protected override bool IncomeMessagesDetect(AutomationElement tab)
        {
            return tab.Current.Name.StartsWith("(") ? true : false;
        }

        protected override AutomationElement DefineTabAutomationAelement(IntPtr widgetHandle)
        {
            if (_browserSet == null)
                InitBrowserSet(base._process);
            return _browserSet.SkypeTab(widgetHandle);
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

            return _browserSet.SkypeFocusAutomationElement(hWnd);
        }

        //подпишемся на событие отслеживающее изменение названия вкладки
        //protected override void OnMessageProcessingSubscribe()
        //{
        //    _hManager = new WebMessengerHookManager(base._windowHandle);
        //    _hManager.ObjectNameChange += OnMessageProcessing;
        //}

        protected override void OnFocusChangedSubscribe()
        {
            //InitHookManager(base._process);
            //_hManager.FocusChange += _hManager_FocusChange;

        }

        void _hManager_FocusChange(object sender, AutomationFocusChangedEventArgs e)
        {
            var element = sender as AutomationElement;
            if (element == _focusableAE)
            {
                if (_browserWindow.Current.Name.Contains("Skype - "))
                    base.OnFocusChanged(sender, e);
                else
                    base.OnFocusChanged(null, null);
            }
            else
                base.OnFocusChanged(null, null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool disposed = false;
        private void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                _hManager.Dispose();
            }
            disposed = true;
        }
        ~WebSkype() { Dispose(false); }

    }
}
