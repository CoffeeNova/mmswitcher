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
        AutomationElement BrowserTabControl(AutomationElement windowAe);
        AutomationElement MessengerTab(IntPtr hWnd);
        AutomationElement MessengerFocusAutomationElement(IntPtr hWnd);
        AutomationElement MessengerIncomeMessageAutomationElement(IntPtr hWnd);
    }

    public abstract class BrowserSet : IBrowserSet
    {
        private delegate AutomationElement MessangerTabDelegate(IntPtr hWnd);

        /// <summary>
        /// 
        /// </summary>
        protected static object _locker = new object();

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
            var mesTab = mtd.Invoke(hWnd);
            return mesTab;
        }

        /// <summary>
        /// Возвращает <see cref="AutomationElement"/> контрола вкладок окна браузера с дескриптором <paramref name="hWnd"/>
        /// </summary>
        /// <param name="hWnd">Хэндл окна.</param>
        /// <exception cref="ArgumentNullException">Значение параметра <paramref name="hWnd"/> равно <see langword="null"/>.</exception>
        /// <exception cref= "ArgumentException">Значение параметра <paramref name="hWnd"/> равно <see langword="IntPtr.Zero"/>.</exception>
        /// <returns></returns>
        //public AutomationElement BrowserTabControl(IntPtr hWnd)
        //{
        //    if (hWnd == null)
        //        throw new ArgumentNullException("hWnd");
        //    if (hWnd == IntPtr.Zero)
        //        throw new ArgumentException("Window handle should not be IntPtr.Zero");

        //    MessangerTabDelegate mtd = DefineTab(MessengerType);
        //    if (mtd == null)
        //        return null;
        //    var mesTab = mtd.Invoke(hWnd);
        //    return mesTab;
        //}

        /// <summary>
        /// Выводит на передний план окно браузера и переключает на вкладку с веб мессенджером.
        /// </summary>
        /// <param name="hWnd">Хэндл окна браузера.</param>
        /// <param name="initialFore">Хэндл окна, которое на переднем плане перед выполнением метода.</param>
        /// <param name="initialTab"><see cref="AutomationElement"/> вкладки, которая была активна перед выполнением миетода.</param>
        /// <param name="isBrowserWindowWasMinimized">Возвращает <see langword="true"/>, если окно браузера было свернуто.</param>
        /// <param name="onlyTab">Возвращает <see langword="true"/>, если окно браузера переместилось на передний план.</param>
        /// <returns></returns>
        public virtual bool SetForegroundMessengerTab(IntPtr hWnd, out IntPtr initialFore, out AutomationElement initialTab, out bool isBrowserWindowWasMinimized)
        {
            initialFore = IntPtr.Zero;
            initialTab = null;
            isBrowserWindowWasMinimized = false;
            var windowAE = BrowserMainWindowAutomationElement(hWnd);
            if (windowAE == null)
                return false;
            SetForegroundBrowserWindow(hWnd, out initialFore, out isBrowserWindowWasMinimized);
            EscMaximizedBrowserWindow(hWnd);
            return FocusMessenger(hWnd, windowAE, out initialTab);
        }

        //пока что кривая реализация через костыль (разворачиваем окно хрома, определяем положение границы вкладки мессенджера и нажимаем на нее мышкой
        protected virtual bool FocusMessenger(IntPtr hWnd, AutomationElement windowAE, out AutomationElement initialTab)
        {
            initialTab = null;
            AutomationElement messengerTab = null;
            if (hWnd == IntPtr.Zero || windowAE == null)
                return false;
            try
            {
                AutomationElementCollection items;
                initialTab = ActiveTab(hWnd, out items);
            }
            catch (Exception ex)
            {
                throw new ElementNotAvailableException("Impossible to define current active browser tab.", ex);
            }
            try
            {
                var mtd = DefineTab(MessengerType);
                messengerTab = mtd.Invoke(hWnd);
            }
            catch (Exception ex)
            {
                throw new ElementNotAvailableException("Messenger tab is not available in a browser.", ex);
            }
            //simulate mouse click
            Tools.SimulateClickUIAutomation(messengerTab, windowAE, hWnd);
            return true;
        }

        /// <summary>
        /// Переключает указанную вкладку <paramref name="tab"/> в окне браузера с дескриптором <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        public virtual bool FocusBrowserTab(IntPtr hWnd, AutomationElement tab)
        {
            var windowAE = BrowserMainWindowAutomationElement(hWnd);
            if (windowAE == null)
                return false;
            Tools.SimulateClickUIAutomation(tab, windowAE, hWnd);
            return true;
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
                mainWindowAe = AutomationElement.FromHandle(hWnd);
                if (!TreeWalker.RawViewWalker.GetParent(mainWindowAe).Current.ClassName.Equals("#32769"))
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

        /// <summary>
        /// Collection of browser tab items
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        public virtual AutomationElementCollection TabItems(AutomationElement tab)
        {
            return tab.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
        }

        /// <summary>
        /// Возвращает текущую активную вкладку (TabControl.Item), как <see cref="AutomationElement"/>.
        /// </summary>
        /// <param name="hWnd">Дескриптор окна браузера.</param>
        /// <returns></returns>
        protected virtual AutomationElement ActiveTab(IntPtr hWnd, out AutomationElementCollection tabItems)
        {
            if (hWnd == null)
                throw new ArgumentNullException("hWnd");
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("Window handle should not be IntPtr.Zero");

            tabItems = null;
            try
            {
                AutomationElement windowAE = BrowserMainWindowAutomationElement(hWnd);
                if (windowAE == null)
                    return null;
                AutomationElement tabControl = BrowserTabControl(windowAE);
                tabItems = TabItems(tabControl);
                AutomationElement currentTab = ActiveTabItem(tabItems, windowAE);
                return currentTab;
            }
            catch { return null; }
        }

        public AutomationElement ActiveTab(AutomationElementCollection tabItems, AutomationElement windowAE)
        {
            if (tabItems == null)
                throw new ArgumentNullException("tabItems");
            if (windowAE == null)
                throw new ArgumentNullException("windowAE");
            
            var currentTab = ActiveTabItem(tabItems, windowAE);
            
            return currentTab;
        }

        public abstract AutomationElement MessengerFocusAutomationElement(IntPtr hWnd);

        public abstract AutomationElement BrowserTabControlWindowAutomationElement(IntPtr hWnd);

        /// <summary>
        /// Messenger caption in browser/
        /// </summary>
        /// <remarks>Should be like "WhatsApp - Google Chrome"</remarks>
        public abstract string MessengerCaption { get; }

        /// <summary>
        /// Проверяет, что <paramref name="hWnd"/> является хэндлом элемента, который действительно сигнализирует о потери фокуса веб мессенджером.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        /// <remarks>Бывает так, что после получения фокуса элементом, который служит индикатором получения фокуса веб мессенджера, он передает фокус другому элементу, который является общим для других функций браузера. Этот метод призван выявлять такие элементы и возвращать <see langword="false"/>, если <paramref name="hWnd"/> является хэндлом такого элемента.</remarks>
        public abstract bool OnFocusLostPermission(IntPtr hWnd);

        /// <summary>
        /// Возвращает <see cref="AutomationElement"/> контрола вкладок родительского элемента <paramref name="mainWindowAE"/>
        /// </summary>
        /// <param name="mainWindowAE"></param>
        /// <returns></returns>
        public abstract AutomationElement BrowserTabControl(AutomationElement mainWindowAE);

        protected abstract AutomationElement ActiveTabItem(AutomationElementCollection tabItems, AutomationElement windowAE);

        protected abstract AutomationElement SkypeTab(IntPtr hWnd);

        protected abstract AutomationElement WhatsAppTab(IntPtr hWnd);

        /// <summary>
        /// Определяет дочерний элемент, который будет получать фокус при переключении на мессенджер.
        /// </summary>
        /// <param name="parent">Родительский <see cref="AutomationElement"/>.</param>
        /// <returns></returns>
        //protected abstract AutomationElement DefineFocusHandlerChildren(AutomationElement parent);


        //private int _tabSelectedHookEventConstant = EventConstants.EVENT_OBJECT_SELECTION;
        //public virtual int TabSelectedHookEventConstant { get { return _tabSelectedHookEventConstant; } }

        //private int _tabClosedHookEventConstant = 0x8001;
        //public virtual int TabClosedHookEventConstant { get { return _tabClosedHookEventConstant; } }
    }


}
