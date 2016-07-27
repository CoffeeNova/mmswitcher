using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;

namespace mmswitcherAPI.Messangers.Web.Browsers
{
    internal sealed class GoogleChromeSet : BrowserSet
    {
        public override string MessengerCaption { get { return Tools.DefineWebMessengerBrowserWindowCaption(MessengerType) + Constants.CHROME_BROWSER_CAPTION; } }


        public GoogleChromeSet(Messenger messenger) : base(messenger) { }


        /// <summary>
        /// Определяет дочерний элемент, который будет получать фокус при переключении на мессенджер.
        /// </summary>
        /// <param name="parent">Родительский <see cref="AutomationElement"/>.</param>
        /// <returns></returns>
        private AutomationElement DefineFocusHandlerChildren(AutomationElement parent)
        {
            if (parent == null)
                return null;
            return parent.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_RenderWidgetHostHWND"));
        }

        /// <summary>
        /// Manual search google chrome tab control element
        /// walking path found using inspect.exe (Windows SDK) for Chrome Version 43.0.2357.65 m (currently the latest stable)
        /// </summary>
        /// <param name="chrome"></param>
        /// <returns></returns>
        private AutomationElement TabControl(AutomationElement chrome)
        {
            if (chrome == null)
                return null;
            var asdasd = TreeWalker.RawViewWalker.GetFirstChild(chrome);
            // manually walk through the tree, searching using TreeScope.Descendants is too slow (even if it's more reliable)
            var chromeDaughter = chrome.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"));

            if (chromeDaughter == null) { return null; } // not the right chrome.exe

            // here, you can optionally check if Incognito is enabled:
            var chromeGranddaughter = TreeWalker.RawViewWalker.GetLastChild(chromeDaughter);
            var chromeGreatgranddaughter = chromeGranddaughter.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, ""))[1];
            return chromeGreatgranddaughter.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tab));
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
            if (hWnd == null)
                throw new ArgumentNullException("hWnd");
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("Window handle should not be IntPtr.Zero");
            lock (_locker)
            {
                try
                {
                    //при переключении вкладок этот контрол перерисовывается, поэтому чтобы получить нужный, нам необходимо задать фокус на наш мессенджер
                    IntPtr initForeHwnd;
                    AutomationElement initControl;
                    bool minimWind;
                    bool setForeTab = SetForegroundMessengerTab(hWnd, out initForeHwnd, out initControl, out minimWind);
                    if (!setForeTab)
                        return null;
                    System.Threading.Thread.Sleep(50);
                    var windowAE = BrowserMainWindowAutomationElement(hWnd);
                    var focusAE = DefineFocusHandlerChildren(windowAE);
                    //if (setFore)
                    //    ReturnPreviusWindowPositions(hWnd, initForeHwnd, minimWind);
                    return focusAE;
                }
                catch { return null; }
            }
        }

        public override AutomationElement BrowserTabControlWindowAutomationElement(IntPtr hWnd)
        {
            return MessengerFocusAutomationElement(hWnd); //the same AE as for focus for chrome
        }

        public override bool OnFocusLostPermission(IntPtr hWnd)
        {
            var element = AutomationElement.FromHandle(hWnd);
            try
            {
                if (element.Current.ClassName == Constants.CHROME_CLASS_NAME)
                    return false;
                return true;
            }
            catch { return true; }
        }

        public override AutomationElement ActiveTab(IntPtr hWnd, out AutomationElementCollection tabItems)
        {
            tabItems = null;
            try
            {
                AutomationElement windowAE = BrowserMainWindowAutomationElement(hWnd);
                if (windowAE == null)
                    return null;
                AutomationElement tabControl = TabControl(windowAE);
                tabItems = TabItems(tabControl);
                AutomationElement currentTab = ActiveTabItem(tabItems, windowAE);
                return currentTab;
            }
            catch { return null; }
        }

        /// <summary>
        /// Retrieve active tab from google chrome tab collecion.
        /// </summary>
        /// <param name="tabItems"></param>
        /// <param name="windowAE">Окно браузера.</param>
        /// <returns></returns>
        /// <remarks>this method is pretty shitty cos idk why the hell AutomationElement.GetSupportedPatterns doesn't returns any paterns. Using inspect.exe we can find that tabitem has IsSelectionItemPatternAvailable. </remarks>
        private AutomationElement ActiveTabItem(AutomationElementCollection tabItems, AutomationElement windowAE)
        {
            foreach (AutomationElement tab in tabItems)
            {
                if (windowAE.Current.Name.StartsWith(tab.Current.Name))
                    return tab;
            }
            return null;
        }

        #region Skype
        protected override AutomationElement SkypeTab(IntPtr hWnd)
        {
            try
            {
                // find the automation element
                AutomationElement windowAE = BrowserMainWindowAutomationElement(hWnd);

                if (windowAE == null)
                    return null;
                //situation if process is not foreground, and/or skype tab is not active
                AutomationElement tabControl = TabControl(windowAE);
                AutomationElementCollection tabItems = TabItems(tabControl);
                AutomationElement skype = SkypeTabItem(tabItems);
                return skype;
            }
            catch 
            {
                throw new ElementNotAvailableException("Skype tab is not available in a Google Chrome.");
            }
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
                if (tab.Current.Name.Contains(Constants.SKYPE_BROWSER_TAB_CAPTION))
                    return tab;
            }
            return null;
        }
        #endregion

        #region WhatsApp
        protected override AutomationElement WhatsAppTab(IntPtr hWnd)
        {
            try
            {
                // find the automation element
                AutomationElement windowAE = BrowserMainWindowAutomationElement(hWnd);
                if (windowAE == null)
                    return null;
                //situation if process is not foreground, and/or skype tab is not active
                AutomationElement tabControl = TabControl(windowAE);
                AutomationElementCollection tabItems = TabItems(tabControl);
                AutomationElement skype = WhatsAppTabItem(tabItems);
                return skype;
            }
            catch { return null; }
        }

        /// <summary>
        /// Retrieve web skype tab from google chrome tab collecion
        /// </summary>
        /// <param name="tabItems"></param>
        /// <returns></returns>
        private AutomationElement WhatsAppTabItem(AutomationElementCollection tabItems)
        {
            foreach (AutomationElement tab in tabItems)
            {
                if (tab.Current.Name.Contains(Constants.WHATSAPP_BROWSER_TAB_CAPTION))
                    return tab;
            }
            return null;
        }
        #endregion

    }
}
