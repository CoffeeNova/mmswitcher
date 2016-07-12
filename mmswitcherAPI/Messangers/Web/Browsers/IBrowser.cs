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
        private void FocusMessenger(IntPtr hWnd, AutomationElement winadowAE)
        {
            if (hWnd == IntPtr.Zero || winadowAE == null)
                return;
            //simulate mouse click
            var mtd = DefineTab(MessengerType);
            Tools.SimulateClickUIAutomation(mtd.Invoke(hWnd), winadowAE, hWnd);

        }

        /// <summary>
        /// Выводит на передний план окно браузера.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
        /// <param name="initialForeWindow">Хэндл окна, которое на переднем плане перед выполнением метода.</param>
        /// <param name="isBrowserWindowWasMinimized">Указывает было ли окно браузера свернутым.</param>
        /// <returns><see cref="true"/>, если операция завершилась успешно. <see cref="false"/>, если операция завершилась неуспешно, или окно браузера уже на переднем плане.</returns>
        private bool SetForegroundBrowserWindow(IntPtr hWnd, out IntPtr initialForeWindow, out bool isBrowserWindowWasMinimized)
        {
            isBrowserWindowWasMinimized = false;
            initialForeWindow = WinApi.GetForegroundWindow();
            bool newForegroundSet = false; 

            if (initialForeWindow != hWnd)
            {
                isBrowserWindowWasMinimized = Tools.RestoreMinimizedWindow(hWnd);
                newForegroundSet = WinApi.SetForegroundWindow(hWnd);
            }
            return newForegroundSet;
        }

        //офигенный метод от батяни - если процесс на фулл экран, нажмем Esc
        private bool EscMaximizedBrowserWindow(IntPtr hWnd)
        {
            if (Tools.FullscreenProcess(hWnd))
            {
                WinApi.PostMessage(hWnd, Constants.WM_KEYDOWN, (IntPtr)WindowsVirtualKey.VK_ESCAPE, IntPtr.Zero);
                WinApi.PostMessage(hWnd, Constants.WM_KEYUP, (IntPtr)WindowsVirtualKey.VK_ESCAPE, IntPtr.Zero);
                System.Threading.Thread.Sleep(50); // пауза ( на быстрых пк не успевает свернуться интерфейс, а условие необходимо для клика мыши по вкладке
                return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает положения окон (начального и окна браузера) в исходное состояние.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
        /// <param name="initHwnd">Хэндл предыдущего (начального) окна на переднем плане.</param>
        /// <param name="restoreMinimizedWindow">Указывает надо ли сворачивать окно браузера.</param>
        private void ReturnPreviusWindowPositions(IntPtr hWnd, IntPtr initHwnd, bool restoreMinimizedWindow)
        {
            if (restoreMinimizedWindow)
                Tools.MinimizeWindow(hWnd);
            if (hWnd != initHwnd)
                WinApi.SetForegroundWindow(initHwnd);
        }
        /// <summary>
        /// Получает <see cref="AutomationElement"/>, которые будет получать фокус при переключении на мессенджер.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
        /// <returns></returns>
        public AutomationElement MessengerFocusAutomationElement(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return null;
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
                    IntPtr initForeHwnd;
                    bool minimWind;
                    bool setFore = SetForegroundBrowserWindow(hWnd, out initForeHwnd, out minimWind);
                    EscMaximizedBrowserWindow(hWnd);
                    FocusMessenger(hWnd, windowAE);
                    var focusAE =  windowAE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_RenderWidgetHostHWND"));
                    
                    if (setFore)
                        ReturnPreviusWindowPositions(hWnd, initForeHwnd, minimWind);
                    return focusAE;
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
