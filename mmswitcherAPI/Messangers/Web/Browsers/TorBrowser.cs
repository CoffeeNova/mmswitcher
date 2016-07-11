using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;

namespace mmswitcherAPI.Messangers.Web.Browsers
{
    public class TorBrowserSet : IBrowser
    {
        #region Skype
        public AutomationElement SkypeTab(IntPtr hWnd)
        {
            return null; //todo
        }

        public AutomationElement BrowserWindowAutomationElement(IntPtr hWnd)
        {
            try
            {
                // find the automation element
                return AutomationElement.FromHandle(hWnd);
            }
            catch { return null; }
        }

        public AutomationElement SkypeFocusAutomationElement(IntPtr hWnd)
        {
            return null; //todo
        }
        #endregion
        #region WhatsApp
        public AutomationElement WhatsAppTab(IntPtr hWnd)
        {
            return null; //todo
        }
        #endregion

    }
}