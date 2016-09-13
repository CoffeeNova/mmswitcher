using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI;
using System.ComponentModel;
using System.Reflection;
using mmswitcherAPI.winmsg;
using System.Collections.ObjectModel;
using mmswitcherAPI.Extensions;
using mmswitcherAPI.Messengers.Exceptions;

namespace mmswitcherAPI.Messengers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="wss"></param>
    public delegate void newMessageDelegate(MessengerBase wss);


    /// <summary>
    /// 
    /// </summary>
    public abstract class MessengerBase : IMessenger, IDisposable
    {
        public string Caption { get; set; }
        /// <summary>
        /// Хэндл главного окна, в котором представлена визуализация браузера, которое в данный момент содержит вкладку с веб мессенджером. 
        /// </summary>
        public IntPtr WindowHandle { get { return _windowHandle; } } //window of messenger

        /// <summary>
        /// Отображает последний получивший сообщение мессенджер.
        /// </summary>
        public static MessengerBase LastAlerted { get { return _lastAlerted; } }

        /// <summary>
        /// Последний активный мессенджер.
        /// </summary>
        public static MessengerBase LastActive { get { return _lastActive; } }

        /// <summary>
        /// Коллекция созданных и активных объектов класса <see cref="MessengerBase"/>.
        /// </summary>
        public static List<MessengerBase> MessengersCollection { get { return _messengersCollection; } }

        /// <summary>
        /// Коллекция созданных и активных объектов класса <see cref="MessengerBase"/> (те, которые имеют непрочитанные сообщения), отсортированных по колличеству полученных сообщений.
        /// </summary>
        public static List<MessengerBase> Activity { get { return _activity; } }

        /// <summary>
        /// Колличество непрочитанных сообщений в представляемом мессенджере.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <remarks>Обычно в мессенджерах изменяется графический интерфейс иконки при получении нового сообщения, или название, тулбар или переменная в памяти.</remarks>
        public int IncomeMessages
        {
            get { return _incomeMessages; }
            set
            {
                if (value > _incomeMessages)
                {
                    _incomeMessages = value;
                    PushToActivity(this);
                    if (GotNewMessage != null)
                        GotNewMessage(this);
                }
                else if (value < _incomeMessages)
                {
                    _incomeMessages = value;
                    PullFromActivity(this);
                    if (MessageGone != null)
                        MessageGone(this);
                }
            }
        }

        public abstract Messenger Messenger { get; }

        public bool Focused
        {
            get { return _focused; }
            private set
            {
                object t = this;
                if (value != _focused && value)
                {
                    if (GotFocus != null)
                        GotFocus(this, new EventArgs());
                    _lastActive = this;
                }
                if (value != _focused && !value)
                    if (LostFocus != null)
                        LostFocus(this, new EventArgs());
                _focused = value;
            }
        }

        private AutomationElement _messengerAE;

        protected AutomationElement MessengerAE
        {
            get
            {
                if (_messengerAE.IsAlive())
                    return _messengerAE;
                else
                { Dispose(true); return null; }
            }
        }

        private AutomationElement _incomeMessageAE;

        protected AutomationElement IncomeMessageAE
        {
            get
            {
                if (_incomeMessageAE.IsAlive())
                    return _incomeMessageAE;
                else
                { Dispose(true); return null; }
            }
        }

        private List<AutomationElement> _focusableAE;

        protected List<AutomationElement> FocusableAE
        {
            get
            {
                if (_focusableAE.IsAlive())
                    return _focusableAE;
                else
                { Dispose(true); return null; }
            }
        }



        public event EventHandler GotFocus;
        public event EventHandler LostFocus;
        public event newMessageDelegate GotNewMessage;
        public event newMessageDelegate MessageGone;

        #region protected fields
        protected Process _process;
        protected IntPtr _windowHandle;
        #endregion

        #region private fields
        private bool _focused = false;
        private int _incomeMessages = 0;
        private static MessengerBase _lastAlerted = null;
        private static MessengerBase _lastActive = null;
        private WindowLifeCycle _wmmon;
        private static List<MessengerBase> _messengersCollection = new List<MessengerBase>();
        private static List<MessengerBase> _activity = new List<MessengerBase>();
        private MessengerHookManager _hManager;
        protected AutomationElement _lastFocusedElement;
        #endregion

        internal protected MessengerBase(Process msgProcess)
        {
            if (msgProcess == null)
                throw new ArgumentException();
            _process = msgProcess;
            IntPtr hWnd;
            try
            {
                _messengerAE = AutomationElement.RootElement;
                var aEdel = new GetMessengerAEDel(GetMainAutomationElement);
                Focused = true;
                CacheAutomationElementProperties(msgProcess, out hWnd, ref _messengerAE, aEdel, AutomationElement.NativeWindowHandleProperty);
                CacheAutomationElementProperties(hWnd, ref _incomeMessageAE, (s) => GetIncomeMessageAutomationElement(s), AutomationElement.NativeWindowHandleProperty);
                CacheAutomationElementProperties(hWnd, ref _focusableAE, (s) => GetFocusRecieverAutomationElement(s), AutomationElement.ClassNameProperty, AutomationElement.NativeWindowHandleProperty);
            }
            catch
            {
                throw new MessengerBuildException(String.Format("Cannot build a messenger for this process {0}", msgProcess.ProcessName));
            }

            _windowHandle = hWnd;

            GotNewMessage += MessengerBase_GotNewMessage;
            _wmmon = new WindowLifeCycle();
            _wmmon.onMessageTraced += OnMessageTraced;
            _hManager = new MessengerHookManager(_windowHandle);

            _messengersCollection.Add(this);
            _activity.Add(this);
            SetForeground();

            OnFocusChangedSubscribe();
            OnMessageProcessingSubscribe();
            System.Threading.Thread.Sleep(2000);
        }

        /// <summary>
        /// Создает новый экземпляр класса типа <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Тип создаваемого клаcса (должен наследовать интерфейс <see cref="IMessenger"/>).</typeparam>
        /// <param name="process">Процесс программы мессенджера.</param>
        /// <returns>Возвращает экземляр созданного класса типа <typeparamref name="T"/>.</returns>
        /// <remarks>Пример создания экземпляра класса: Skype derivedClass = BaseClass.Create&lt;Skype&gt;(process);</remarks>
        public static T Create<T>(Process process) where T : IMessenger
        {
            try
            {
                var newMessenger = (T)Activator.CreateInstance(typeof(T), process);
                return newMessenger;
            }
            catch (TargetInvocationException)
            { throw new MessengerBuildException("Cannot build a messenger"); }
        }

        /// <summary>
        /// Регистрирует метод <see cref="OnPropertyChanged"/>, который будет обрабатывать события изменения свойства.<see cref="AutomationElement.HasKeyboardFocusProperty"/>
        /// </summary>
        /// <param name="aElement">Элемент модель автоматизации пользовательского элемента, с которым небходимо связать обработчик событий.</param>
        private void AddAutomationKeyboardFocusChangedEventHandler(AutomationElement aElement)
        {

        }

        /// <summary>
        /// Callback метод события <see cref="WindowLifeCycle.onMessageTraced"/>, который вызывает освобождение ресурсов при перехвате сообщения windows о закрытии окна мессенджера.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="hWnd">Дескриптор элемента.</param>
        /// <param name="shell">Перечисление shell событий.</param>
        protected virtual void OnMessageTraced(object sender, IntPtr hWnd, ShellEvents shell)
        {
            if (shell != ShellEvents.HSHELL_WINDOWDESTROYED)
                return;
            if (hWnd == _windowHandle || hWnd == (IntPtr)_messengerAE.Cached.NativeWindowHandle)
                Dispose(true);
        }

        protected virtual void OnFocusChanged(object sender, EventArgs e)
        {
            var focusResult = (FocusableAE.Any((p) =>
            {
                if ((IntPtr)sender == (IntPtr)p.Cached.NativeWindowHandle)
                {
                    _lastFocusedElement = p;
                    return true;
                }
                else return false;
            }));
            if (focusResult)
                Focused = true;
            else
                Focused = false;
        }

        void MessengerBase_GotNewMessage(MessengerBase wss)
        {
            _lastAlerted = wss;
        }

        private static void PullFromActivity(MessengerBase messengerBase)
        {
            if (_activity.Count > 0)
                _activity.OrderByDescending(messenger => { return messenger.IncomeMessages; }).ToList(); ;
        }

        private static void PushToActivity(MessengerBase messengerBase)
        {
            if (_activity.Count > 0)
                _activity = _activity.OrderByDescending(messenger => { return messenger.IncomeMessages; }).ToList();
        }

        protected delegate AutomationElement GetAutomationDel(IntPtr hWnd);

        /// <summary>
        /// Кэширует заданные свойства или паттерны <paramref name="cacheData"/> при создании <paramref name="element"/> методом, представленным делегатом <paramref name="getAutomationDel"/> по дескриптору окна <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="element"></param>
        /// <param name="getAutomationDel"></param>
        /// <param name="cacheData"></param>
        /// <exception cref="ArgumentException">Неправильный тим параметра <paramref name="cacheData"/>.</exception>
        /// <remarks><paramref name="cacheData"/> параметры, должны состояить только из типов <see cref="AutomationProperty"/> и <see cref="AutomationPAttern"/>.</remarks>
        protected void CacheAutomationElementProperties(IntPtr hWnd, ref AutomationElement element, GetAutomationDel getAutomationDel, params AutomationIdentifier[] cacheData)
        {

            var cacheRequest = new CacheRequest();
            foreach (var ai in cacheData)
            {
                var aPropType = typeof(AutomationProperty);
                var aPattType = typeof(AutomationPattern);
                if (ai.GetType() == aPropType)
                    cacheRequest.Add((AutomationProperty)ai);
                else if ((ai.GetType() == aPattType))
                    cacheRequest.Add((AutomationPattern)ai);
                else
                    throw new ArgumentException(string.Format("CacheData has a wrong type."));
            }
            using (cacheRequest.Activate())
            {
                element = getAutomationDel.Invoke(hWnd);
            }
        }

        protected delegate List<AutomationElement> GetFocusedAEDel(IntPtr hWnd);

        /// <summary>
        /// Кэширует заданные свойства или паттерны <paramref name="cacheData"/> при создании <paramref name="element"/> методом, представленным делегатом <paramref name="getFocusedAEDel"/> по дескриптору окна <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="element"></param>
        /// <param name="getFocusedAEDel"></param>
        /// <param name="cacheData"></param>
        protected void CacheAutomationElementProperties(IntPtr hWnd, ref List<AutomationElement> element, GetFocusedAEDel getFocusedAEDel, params AutomationIdentifier[] cacheData)
        {

            var cacheRequest = new CacheRequest();
            foreach (var ai in cacheData)
            {
                var aPropType = typeof(AutomationProperty);
                var aPattType = typeof(AutomationPattern);
                if (ai.GetType() == aPropType)
                    cacheRequest.Add((AutomationProperty)ai);
                else if ((ai.GetType() == aPattType))
                    cacheRequest.Add((AutomationPattern)ai);
                else
                    throw new ArgumentException(string.Format("CacheData has a wrong type."));
            }
            using (cacheRequest.Activate())
            {
                element = getFocusedAEDel.Invoke(hWnd);
            }
        }

        private delegate AutomationElement GetMessengerAEDel(Process process, out IntPtr hWnd);

        /// <summary>
        /// Кэширует заданные свойства <paramref name="properties"/> при создании <paramref name="element"/> методом, представленным делегатом <paramref name="getMessengerAEDel"/> по процессу окна <paramref name="process"/>.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="element"></param>
        /// <param name="getAutomationDel"></param>
        /// <param name="properties"></param>
        private void CacheAutomationElementProperties(Process process, out IntPtr hWnd, ref AutomationElement element, GetMessengerAEDel getMessengerAEDel, params AutomationProperty[] properties)
        {
            var cacheRequest = new CacheRequest();
            foreach (var property in properties)
            {
                cacheRequest.Add(property);
            }

            using (cacheRequest.Activate())
            {
                element = getMessengerAEDel.Invoke(process, out hWnd);
            }
        }

        public abstract void SetForeground();

        /// <summary>
        /// Должен получать <see cref="AutomationElement"/> главного (или нет) окна процесса <paramref name="process"/> и его дескриптор.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        protected abstract AutomationElement GetMainAutomationElement(Process process, out IntPtr hWnd);

        /// <summary>
        /// Определяет <see cref="AutomationElement"/> для <see cref="MessengerBase.FocusableAE"/>, который получает фокус при переключении на окно мессенджера.
        /// </summary>
        /// <param name="hWnd">Хэндл окна мессенджера.</param>
        /// <returns></returns>
        /// <remarks>По-умолчанию <see cref="AutomationElement"/> главного окна мессенджера служит получателем фокуса при переключении на него. </remarks>
        protected virtual List<AutomationElement> GetFocusRecieverAutomationElement(IntPtr hWnd)
        {
            return new List<AutomationElement>() { MessengerAE };
        }

        /// <summary>
        /// Определяет <see cref="AutomationElement"/> для <see cref="MessengerBase.IncomeMessageAE"/>, который получает извещение о новом сообщении путем изменения свойства.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <remarks>По-умолчанию <see cref="AutomationElement"/> главного окна мессенджера служит индикатором получения нового сообщения. </remarks>
        protected virtual AutomationElement GetIncomeMessageAutomationElement(IntPtr hWnd)
        {
            return MessengerAE;
        }

        /// <summary>
        /// Регистрирует метод <see cref="MessengerBase.OnFocusChanged"/>, который будет обрабатывать события изменения фокуса.
        /// </summary>
        /// <remarks>Можно реализовать вручную, например через winapi SetWinEventHook.</remarks>
        protected void OnFocusChangedSubscribe()
        {
            _hManager.FocusChanged += OnFocusChanged;
        }

        /// <summary>
        /// Должен реализовать функцию отслеживания изменения колличества новых сообщений.
        /// </summary>
        /// <remarks>Можно реализовать вручную, например через winapi SetWinEventHook.</remarks>
        protected abstract void OnMessageProcessingSubscribe();

        protected abstract void OnMessageProcessingUnSubscribe();


        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                GotNewMessage -= MessengerBase_GotNewMessage;
                _process = null;
                _messengerAE = null;
                _focusableAE = null;
                _wmmon.onMessageTraced -= OnMessageTraced;
                _wmmon.Dispose();
                _hManager.FocusChanged -= OnFocusChanged;
                _hManager.Dispose();
            }
            OnMessageProcessingUnSubscribe();
            _messengersCollection.Remove(this);
            _activity.Remove(this);
            _disposed = true;
        }
        ~MessengerBase()
        {
            Dispose(false);
        }
    }

    public enum Messenger
    {
        Skype,
        Telegram,
        WebSkype,
        WebWhatsApp,
        WebTelegram
    }
}
