using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace mmswitcherAPI.Messangers.Web.Browsers
{
    public interface IBrowserSet
    {
        AutomationElement BrowserWindowAutomationElement(IntPtr hWnd);
        AutomationElement MessengerTab(IntPtr hWnd);
        AutomationElement MessengerFocusAutomationElement(IntPtr hWnd);
        AutomationElement MessengerIncomeMessageAutomationElement(IntPtr hWnd);
    }

    public abstract class BrowserSet : IBrowserSet
    {
        private delegate AutomationElement MessangerTabDelegate(IntPtr hWnd);
        protected static object locker = new object();

        public Messenger MessengerType { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BrowserSet"/>.
        /// </summary>
        /// <param name="messenger">Тип мессенджера.</param>
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
        /// <summary>
        /// Возвращает <see cref="AutomationElement"/> вкладки браузера, в которой открыт мессенджер.
        /// </summary>
        /// <param name="hWnd">Хэндл окна.</param>
        /// <exception cref="ArgumentNullException">Значение параметра <paramref name="hWnd"/> равно <see langword="null"/>.</exception>
        /// <exception cref= "ArgumentException">Значение параметра <paramref name="hWnd"/> равно <see langword="IntPtr.Zero"/>.</exception>
        /// <returns></returns>
        public AutomationElement MessengerTab(IntPtr hWnd)
        {
            if (hWnd == null)
                throw new ArgumentNullException("hWnd");
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("Window handle should not be IntPtr.Zero");

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
        /// <returns><see langword="true"/>, если операция завершилась успешно. <see langword="true"/>, если операция завершилась неуспешно, или окно браузера уже на переднем плане.</returns>
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
        /// Получает <see cref="AutomationElement"/> окна браузера.
        /// </summary>
        /// <param name="hWnd">Хэндл окна.</param>
        /// <exception cref="ArgumentNullException">Значение параметра <paramref name="hWnd"/> равно <see langword="null"/>.</exception>
        /// <exception cref= "ArgumentException">Значение параметра <paramref name="hWnd"/> равно <see langword="IntPtr.Zero"/>.</exception>
        /// <returns></returns>
        public virtual AutomationElement BrowserWindowAutomationElement(IntPtr hWnd)
        {
            if (hWnd == null)
                throw new ArgumentNullException("hWnd");
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("Window handle should not be IntPtr.Zero");
            return AutomationElement.FromHandle(hWnd);
        }

        /// <summary>
        /// Получает <see cref="AutomationElement"/>, которые будет получать фокус при переключении на мессенджер.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Значение параметра <paramref name="hWnd"/> равно <see langword="null"/>.</exception>
        /// <exception cref= "ArgumentException">Значение параметра <paramref name="hWnd"/> равно <see langword="IntPtr.Zero"/>.</exception>
        /// <remarks></remarks>
        public virtual AutomationElement MessengerFocusAutomationElement(IntPtr hWnd)
        {
            if (hWnd == null)
                throw new ArgumentNullException("hWnd");
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("Window handle should not be IntPtr.Zero");
            lock (locker)
            {
                try
                {
                    //при переключении вкладок этот контрол перерисовывается, поэтому чтобы получить нужный, нам необходимо задать фокус на наш мессенджер
                    // find the automation element
                    var windowAE = BrowserWindowAutomationElement(hWnd);
                    if (windowAE == null)
                        return null;
                    IntPtr initForeHwnd;
                    bool minimWind;
                    bool setFore = SetForegroundBrowserWindow(hWnd, out initForeHwnd, out minimWind);
                    EscMaximizedBrowserWindow(hWnd);
                    FocusMessenger(hWnd, windowAE);
                    var focusAE = DefineFocusHandlerChildren(windowAE);
                    if (setFore)
                        ReturnPreviusWindowPositions(hWnd, initForeHwnd, minimWind);
                    return focusAE;
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// Возвращает <see cref="AutomationElement"/> который изменяет свойство <see cref="AutomationElement.PropertyName"/> при получении сообщения.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
         /// <exception cref="ArgumentNullException">Значение параметра <paramref name="hWnd"/> равно <see langword="null"/>.</exception>
        /// <exception cref= "ArgumentException">Значение параметра <paramref name="hWnd"/> равно <see langword="IntPtr.Zero"/>.</exception>
        /// <returns></returns>
        public virtual AutomationElement MessengerIncomeMessageAutomationElement(IntPtr hWnd)
        {
            if (hWnd == null)
                throw new ArgumentNullException("hWnd");
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("Window handle should not be IntPtr.Zero");
            return BrowserWindowAutomationElement(hWnd);
        }

        protected abstract AutomationElement SkypeTab(IntPtr hWnd);

        protected abstract AutomationElement WhatsAppTab(IntPtr hWnd);

        /// <summary>
        /// Определяет дочерний элемент, который будет получать фокус при переключении на мессенджер.
        /// </summary>
        /// <param name="parent">Родительский <see cref="AutomationElement"/>.</param>
        /// <returns></returns>
        protected abstract AutomationElement DefineFocusHandlerChildren(AutomationElement parent);


        private int _tabSelectedHookEventConstant = EventConstants.EVENT_OBJECT_SELECTION;
        public virtual int TabSelectedHookEventConstant { get { return _tabSelectedHookEventConstant; } }
    }


}
