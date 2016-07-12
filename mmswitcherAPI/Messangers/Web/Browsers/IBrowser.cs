using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace mmswitcherAPI.Messangers.Web.Browsers
{
    internal interface IBrowserSet
    {
        AutomationElement BrowserWindowAutomationElement(IntPtr hWnd);
        AutomationElement MessengerTab(IntPtr hWnd);
        AutomationElement MessengerFocusAutomationElement(IntPtr hWnd);
    }

    internal abstract class BrowserSet : IBrowserSet
    {
        private delegate AutomationElement MessangerTabDelegate(IntPtr hWnd);
        private static object locker = new object();

        public Messenger MessengerType { get; private set; }

        public BrowserSet(Messenger messenger)
        {
            MessengerType = messenger;
        }

        private MessangerTabDelegate DefineTab(Messenger messenger)
        {
            MessangerTabDelegate mtd;

            switch (messenger)
            {
                case Messenger.Skype:
                    mtd = new MessangerTabDelegate(SkypeTab);
                    break;
                case Messenger.WhatsApp:
                    mtd = new MessangerTabDelegate(WhatsAppTab);
                    break;
                default:
                    mtd = null;
                    break;
            }
            return mtd;
        }

        public AutomationElement MessengerTab(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return null;
            MessangerTabDelegate mtd = DefineTab(MessengerType);
            if (mtd == null)
                return null;
            return mtd.Invoke(hWnd);
        }

        //пока что кривая реализация через костыль (разворачиваем окно хрома, определяем положение границы вкладки мессенджера и нажимаем на нее мышкой
        protected void FocusMessenger(IntPtr hWnd, AutomationElement winadowAE)
        {
            if (hWnd == IntPtr.Zero || winadowAE == null)
                return;
            bool returnInitForeWindow = false;
            bool restoreMinimizedWindow = false;

            var initialForeWindow = WinApi.GetForegroundWindow();
            if (initialForeWindow != hWnd)
            {
                restoreMinimizedWindow = Tools.RestoreMinimizedWindow(hWnd);
                WinApi.SetForegroundWindow(hWnd);
                returnInitForeWindow = true;
            }
            //офигенный метод от батяни - если процесс на фулл экран, нажмем Esc
            if (Tools.FullscreenProcess(hWnd))
            {
                WinApi.PostMessage(initialForeWindow, Constants.WM_KEYDOWN, (IntPtr)WindowsVirtualKey.VK_ESCAPE, IntPtr.Zero);
                WinApi.PostMessage(initialForeWindow, Constants.WM_KEYUP, (IntPtr)WindowsVirtualKey.VK_ESCAPE, IntPtr.Zero);
                System.Threading.Thread.Sleep(50); // пауза ( на быстрых пк не успевает свернуться интерфейс, а условие необходимо для клика мыши по вкладке
            }
            //simulate mouse click
            var mtd = DefineTab(MessengerType);
            Tools.SimulateClickUIAutomation(mtd.Invoke(hWnd), winadowAE, hWnd);

            if (restoreMinimizedWindow)
                Tools.MinimizeWindow(hWnd);
            if (returnInitForeWindow)
                WinApi.SetForegroundWindow(initialForeWindow);
        }

        public AutomationElement MessengerFocusAutomationElement(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return null;
            //try
            //{
            //    // find the automation element
            //    return BrowserWindowAutomationElement(hWnd);
            //}
            lock (locker)
            {
                try
                {
                    //при переключении вкладок хрома, или операции SetForegroundWindow окна хрома фокус получает контрол "document", его имя класса "Chrome_RenderWidgetHostHWND"
                    //при переключении вкладок этот контрол перерисовывается, поэтому чтобы получить нужный, нам необходимо задать фокус на наш мессенджер
                    // find the automation element

                    var windowAE = AutomationElement.FromHandle(hWnd);
                    if (windowAE == null)
                        return null;
                    FocusMessenger(hWnd, windowAE);
                    return windowAE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_RenderWidgetHostHWND"));
                }
                catch { return null; }
            }
        }

        protected abstract AutomationElement SkypeTab(IntPtr hWnd);

        protected abstract AutomationElement WhatsAppTab(IntPtr hWnd);

        public abstract AutomationElement BrowserWindowAutomationElement(IntPtr hWnd);

        private int _tabSelectedHookEventConstant = EventConstants.EVENT_OBJECT_SELECTION;
        public virtual int TabSelectedHookEventConstant { get { return _tabSelectedHookEventConstant; } }
    }


}
