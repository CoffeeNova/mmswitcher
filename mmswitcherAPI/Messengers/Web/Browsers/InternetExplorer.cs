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
        public override string MessengerCaption { get { return Tools.DefineWebMessengerBrowserWindowCaption(MessengerType) + Constants.IEXPLORER_BROWSER_CAPTION; } }

        public InternetExplorerSet(Messenger messenger) : base(messenger){}

        private AutomationElement DefineFocusHandlerChildren(AutomationElement parent)
        {
            if (parent == null)
                return null;
            return null; //todo
        }

        public override AutomationElement BrowserTabControlWindowAutomationElement(IntPtr hWnd)
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

        public override AutomationElement ActiveTab(IntPtr hWnd, out AutomationElementCollection tabItems)
        {
            throw new NotImplementedException();
        }
        #region Skype

        //private const int _focusHookEventConstant = EventConstants.EVENT_OBJECT_SELECTIONREMOVE; //not tested
        //public int FocusHookEventConstant { get { return _focusHookEventConstant; } }

        protected override AutomationElement SkypeTab(IntPtr hWnd)
        {
            return null; //todo
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