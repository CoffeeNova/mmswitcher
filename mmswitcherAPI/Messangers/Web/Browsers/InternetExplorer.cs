using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;

namespace mmswitcherAPI.Messangers.Web.Browsers
{
    internal sealed class InternetExplorerSet : BrowserSet
    {
        public InternetExplorerSet(Messenger messenger) : base(messenger){}

        protected override AutomationElement DefineFocusHandlerChildren(AutomationElement parent)
        {
            if (parent == null)
                return null;
            return null; //todo
        }
        #region Skype

        //private const int _focusHookEventConstant = EventConstants.EVENT_OBJECT_SELECTIONREMOVE; //not tested
        //public int FocusHookEventConstant { get { return _focusHookEventConstant; } }

        protected override AutomationElement SkypeTab(IntPtr hWnd)
        {
            return null; //todo
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

        private AutomationElement SkypeFocusAutomationElement(IntPtr hWnd)
        {
            return null; //todo
        }
        #endregion
        #region WhatsApp
        protected override AutomationElement WhatsAppTab(IntPtr hWnd)
        {
            return null; //todo
        }
        #endregion

    }
}