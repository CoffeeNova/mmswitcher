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
        AutomationElement BrowserMainWindowAutomationElement(IntPtr hWnd);
        AutomationElement BrowserTabControlWindowAutomationElement(IntPtr hWnd);
        AutomationElement MessengerTab(IntPtr hWnd);
        AutomationElement MessengerFocusAutomationElement(IntPtr hWnd);
        AutomationElement MessengerIncomeMessageAutomationElement(IntPtr hWnd);
    }

    public abstract class BrowserSet : IBrowserSet
    {
        private delegate AutomationElement MessangerTabDelegate(IntPtr hWnd);
        protected object locker = new object();

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
        protected virtual void FocusMessenger(IntPtr hWnd, AutomationElement winadowAE)
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
        protected static bool SetForegroundBrowserWindow(IntPtr hWnd, out IntPtr initialForeWindow, out bool isBrowserWindowWasMinimized)
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
        protected static bool EscMaximizedBrowserWindow(IntPtr hWnd)
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
        protected static void ReturnPreviusWindowPositions(IntPtr hWnd, IntPtr initHwnd, bool restoreMinimizedWindow)
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
        public virtual AutomationElement BrowserMainWindowAutomationElement(IntPtr hWnd)
        {
            if (hWnd == null)
                throw new ArgumentNullException("hWnd");
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("Window handle should not be IntPtr.Zero");
            AutomationElement mainWindowAe = null;
            try
            {
                var ae = AutomationElement.FromHandle(hWnd);
                mainWindowAe = TreeWalker.RawViewWalker.GetParent(ae);
                if (!TreeWalker.RawViewWalker.GetParent(mainWindowAe).Current.Name.Equals("Desktop"))
                    throw new ArgumentException("hWnd is not main window handle");
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            catch (Exception)
            {
                throw new ArgumentException("Can't get main window automation element");
            }
            return mainWindowAe;
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
            return BrowserMainWindowAutomationElement(hWnd);
        }

        public abstract AutomationElement MessengerFocusAutomationElement(IntPtr hWnd);

        public abstract AutomationElement BrowserTabControlWindowAutomationElement(IntPtr hWnd);

        /// <summary>
        /// Messenger caption in browser/
        /// </summary>
        /// <remarks>Should be like "WhatsApp - Google Chrome"</remarks>
        public abstract string MessengerCaption { get; }

        protected abstract AutomationElement SkypeTab(IntPtr hWnd);

        protected abstract AutomationElement WhatsAppTab(IntPtr hWnd);

        /// <summary>
        /// Определяет дочерний элемент, который будет получать фокус при переключении на мессенджер.
        /// </summary>
        /// <param name="parent">Родительский <see cref="AutomationElement"/>.</param>
        /// <returns></returns>
        //protected abstract AutomationElement DefineFocusHandlerChildren(AutomationElement parent);


        private int _tabSelectedHookEventConstant = EventConstants.EVENT_OBJECT_SELECTION;
        public virtual int TabSelectedHookEventConstant { get { return _tabSelectedHookEventConstant; } }

        private int _tabClosedHookEventConstant = 0x8001;
        public virtual int TabClosedHookEventConstant { get { return _tabClosedHookEventConstant; } }
    }


}
