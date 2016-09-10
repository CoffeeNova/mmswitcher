using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;

namespace mmswitcherAPI.Messengers.Web.Browsers
{
    internal sealed class OperaSet : BrowserSet
    {
        public override string MessengerCaption { get { return Tools.DefineWebMessengerBrowserWindowCaption(MessengerType) + Constants.OPERA_BROWSER_CAPTION; } }

        public OperaSet(Messenger messenger) : base(messenger){}

        private AutomationElement DefineFocusHandlerChildren(AutomationElement parent)
        {
            if (parent == null)
                return null;
            return parent.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_RenderWidgetHostHWND"));
        }

        public override AutomationElement BrowserTabControlWindowAutomationElement(IntPtr hWnd)
        {
            throw new NotImplementedException();
        }

        public override AutomationElement BrowserTabControl(AutomationElement mainWindowAE)
        {
            throw new NotImplementedException();
        }

        public override AutomationElement SelectedTab(AutomationElementCollection tabItems)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Получает <see cref="AutomationElement"/>, которые будет получать фокус при переключении на мессенджер.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Значение параметра <paramref name="hWnd"/> равно <see langword="null"/>.</exception>
        /// <exception cref= "ArgumentException">Значение параметра <paramref name="hWnd"/> равно <see langword="IntPtr.Zero"/>.</exception>
        /// <remarks></remarks>
        public override AutomationElement MessengerFocusAutomationElement(IntPtr hWnd)
        {
            throw new NotImplementedException();
        }

        protected override AutomationElement ActiveTab(IntPtr hWnd, out AutomationElementCollection tabItems)
        {
            throw new NotImplementedException();
        }
        #region Skype

        //private const int _focusHookEventConstant = EventConstants.EVENT_OBJECT_SHOW;
        //public int FocusHookEventConstant { get { return _focusHookEventConstant; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        protected override AutomationElement SkypeTab(IntPtr handle)
        {
            string windowName = "";

            try
            {
                // find the automation element
                AutomationElement windowAE = AutomationElement.FromHandle(handle);
                windowName = windowAE.Current.Name;
                //situation if process is not foreground, and/or skype tab is not active
                AutomationElement tabControl = SkypeTabControl(windowAE);
                if (tabControl == null)
                    return null;
                AutomationElementCollection tabItems = SkypeTabItems(tabControl);
                AutomationElement skype = SkypeTabItem(tabItems);
                return skype == null ? null : skype;
            }
            catch { return null; }
        }

        public override AutomationElement BrowserMainWindowAutomationElement(IntPtr hWnd)
        {
            try
            {
                // find the automation element
                return AutomationElement.FromHandle(hWnd);
            }
            catch { return null; }
        }

        public override bool OnFocusLostPermission(IntPtr hWnd)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Manual search Opera tab control element
        /// walking path found using inspect.exe (Windows SDK) for Opera Version 33.0.1990.115 (currently the latest stable)
        /// </summary>
        /// <param name="opera"></param>
        /// <returns></returns>
        private AutomationElement SkypeTabControl(AutomationElement opera)
        {
            if (opera == null)
                return null;
            // manually walk through the tree, searching using TreeScope.Descendants is too slow (even if it's more reliable)
            // var operaDaughter = opera.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.HelpTextProperty, ""));
            var operaDaughter = opera.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Browser container"));

            if (operaDaughter == null) { return null; } // not the right opera.exe

            var operaGranddaughter = operaDaughter.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Browser client"));
            var operaGreatgranddaughter = operaGranddaughter.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Browser contents"));
            var operasBelovedChild = operaGreatgranddaughter.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Top bar container"));
            var operasUnlovedChild = operasBelovedChild.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Tab bar"));
            //OPERA STRONG AND YOUNG!
            return operasUnlovedChild;
        }
        /// <summary>
        /// Collection of google chrome tab items
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        private AutomationElementCollection SkypeTabItems(AutomationElement tab)
        {
            return tab.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
        }
        /// <summary>
        /// Retrieve web skype tab from google chrome tab collecion
        /// </summary>
        /// <param name="tabItems"></param>
        /// <returns></returns>
        private AutomationElement SkypeTabItem(AutomationElementCollection tabItems)
        {
            foreach (AutomationElement tab in tabItems)
            {
                if (tab.Current.Name.Contains(Constants.SKYPE_BROWSER_WINDOW_CAPTION))
                    return tab;
            }
            return null;
        }

        private AutomationElement SkypeFocusAutomationElement(IntPtr hWnd)
        {
            return null; //todo
        }
        #endregion

        #region WhatsApp
        protected override AutomationElement WhatsAppTab(IntPtr hWnd)
        {
            //todo
            return null;
        }
        #endregion

        #region Telegram
        protected override AutomationElement TelegramTab(IntPtr hWnd)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
