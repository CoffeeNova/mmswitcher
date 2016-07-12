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
        public GoogleChromeSet(Messenger messenger) : base(messenger){}

        #region Skype
        protected override AutomationElement SkypeTab(IntPtr hWnd)
        {
            try
            {
                // find the automation element
                AutomationElement windowAE = BrowserWindowAutomationElement(hWnd);
                if (windowAE == null)
                    return null;
                //situation if process is not foreground, and/or skype tab is not active
                AutomationElement tabControl = SkypeTabControl(windowAE);
                AutomationElementCollection tabItems = SkypeTabItems(tabControl);
                AutomationElement skype = SkypeTabItem(tabItems);
                return skype;
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
        /// Manual search google chrome tab control element
        /// walking path found using inspect.exe (Windows SDK) for Chrome Version 43.0.2357.65 m (currently the latest stable)
        /// </summary>
        /// <param name="chrome"></param>
        /// <returns></returns>
        private AutomationElement SkypeTabControl(AutomationElement chrome)
        {
            if (chrome == null)
                return null;
            // manually walk through the tree, searching using TreeScope.Descendants is too slow (even if it's more reliable)
            var chromeDaughter = chrome.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"));

            if (chromeDaughter == null) { return null; } // not the right chrome.exe

            // here, you can optionally check if Incognito is enabled:
            var chromeGranddaughter = TreeWalker.RawViewWalker.GetLastChild(chromeDaughter);
            var chromeGreatgranddaughter = chromeGranddaughter.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, ""))[1];
            return chromeGreatgranddaughter.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tab));
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
