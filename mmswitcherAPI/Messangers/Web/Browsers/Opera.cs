using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;

namespace mmswitcherAPI.Messangers.Web.Browsers
{
    internal sealed class OperaSet : BrowserSet
    {
        public OperaSet(Messenger messenger) : base(messenger){}
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

        public override AutomationElement BrowserWindowAutomationElement(IntPtr hWnd)
        {
            try
            {
                // find the automation element
                return AutomationElement.FromHandle(hWnd);
            }
            catch { return null; }
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
                if (tab.Current.Name.Contains("Skype"))
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
    }
}
