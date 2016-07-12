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
    public abstract class WebMessenger : MessengerBase
    {
        #region private fields
        //private WebHookManager hManager;
        #endregion

        public WebMessenger(Process browserProcess)
            : base(browserProcess)
        {
            if (browserProcess == null)
                throw new ArgumentException();
        }

        private List<IntPtr> GetWidgetHandles(int processId, string className)
        {
            return Tools.GetWidgetWindowHandles(processId, className);
        }

        /// <summary>
        /// Производит поиск вкладки веб мессенджера в процессе <paramref name="process"/>, так же возвращает дескриптор окна, в котором открыта вкладка веб мессенджера.
        /// </summary>
        /// <param name="process">Процесс браузера.</param>
        /// <param name="hWnd">Дескриптор окна, в котором открыта вкладка.</param>
        /// <returns>Вкладку веб мессенджера в виде <see cref="AutomationElement"/> и дескриптор окна, в котором открыта эта вкладка.</returns>
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


        //protected override AutomationElement GetFocusHandlerAutomationElement(IntPtr hWnd)
        //{
        //    return DefineFocusHandlerAutomationElement(hWnd);
        //}

        protected abstract AutomationElement DefineTabAutomationAelement(IntPtr widgetHandle);

        //protected abstract AutomationElement DefineFocusHandlerAutomationElement(IntPtr widgetHandle);

        //private void WebMessanger_GotFocus(Object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var tabElement = AutomationElement.FromHandle((IntPtr)sender);
        //        if (BrowserData.Process == null)
        //            return;
        //        if (tabElement.Current.ProcessId == BrowserData.Process.Id)
        //            Focused = true;
        //    }
        //    catch { }
        //}

        //private void WebMessanger_LostFocus(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var tabElement = AutomationElement.FromHandle((IntPtr)sender);
        //        if (BrowserData.Process == null)
        //            return;
        //        if (tabElement.Current.ProcessId == BrowserData.Process.Id)
        //            Focused = false;
        //    }
        //    catch { }
        //}
    }
}
