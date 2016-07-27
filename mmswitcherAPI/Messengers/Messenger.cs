﻿using System;
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

namespace mmswitcherAPI.Messangers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="wss"></param>
    public delegate void newMessageDelegate(MessengerBase wss);

    /// <summary>
    /// 
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// 
        /// </summary>
        string Caption { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// 
        /// </summary>
        Messenger Messenger { get; }

        /// <summary>
        /// 
        /// </summary>
        bool Focused { get; }

        /// <summary>
        /// 
        /// </summary>
        event newMessageDelegate GotNewMessage;

        /// <summary>
        /// 
        /// </summary>
        event newMessageDelegate MessagesGone;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler GotFocus;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler LostFocus;

    }

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
        public static Collection<MessengerBase> MessengersCollection { get { return _messengersCollection; } }

        /// <summary>
        /// Коллекция созданных и активных объектов класса <see cref="MessengerBase"/> (те, которые имеют непрочитанные сообщения), отсортированных по колличеству полученных сообщений.
        /// </summary>
        public static Collection<MessengerBase> Activity { get { return _activity; } }

        private static AutomationElement _previousNonMessenger;
        /// <summary>
        /// Элемент модели автоматизации, которая служит индикатором получения фокуса для предыдущего активного окна, не являющимся мессенджером.
        /// </summary>
        protected static AutomationElement PreviousNonMessenger
        {
            get { return _previousNonMessenger; }
            private set
            {
                if (MessengerBase.MessengersCollection.SingleOrDefault((m) => m._focusableAE == value) == null)
                    _previousNonMessenger = value;
            }
        }

        protected bool IncomeMessages
        {
            get { return _incomeMessages; }
            set
            {
                if (value && !value.Equals(_incomeMessages))
                {
                    _incomeMessages = value;
                    GotNewMessage(this);
                    NewMessagesCount++;
                    PushToActivity(this);
                }
                if (!value && !value.Equals(_incomeMessages))
                {
                    _incomeMessages = value;
                    MessagesGone(this);
                    NewMessagesCount = 0;
                    PullFromActivity(this);
                }
            }
        }

        public int NewMessagesCount { get; private set; }



        public abstract Messenger Messenger { get; }


        public bool Focused
        {
            get { return _focused; }
            private set
            {
                object t = this;
                if (value != _focused && value)
                {
                    GotFocus(this, new EventArgs());
                    _lastActive = this;
                }
                if (value != _focused && !value)
                    LostFocus(this, new EventArgs());
                _focused = value;
            }
        }

        private AutomationElement _messengerAE;

        protected AutomationElement MessengerAE
        {
            get
            {
                try
                {
                    var t = _incomeMessageAE.Current.Name;
                    return _messengerAE;
                }
                catch { Dispose(true); return null; }
            }
        }

        private AutomationElement _focusableAE;

        protected AutomationElement FocusableAE
        {
            get
            {
                try
                {
                    var t = _incomeMessageAE.Current.Name;
                    return _focusableAE;
                }
                catch { Dispose(true); return null; }
            }
        }

        private AutomationElement _incomeMessageAE;

        protected AutomationElement IncomeMessageAE
        {
            get
            {
                try
                {
                    var t = _incomeMessageAE.Current.Name;
                    return _incomeMessageAE;
                }
                catch { Dispose(true); return null; }
            }
        }

        public event EventHandler GotFocus;
        public event EventHandler LostFocus;
        public event newMessageDelegate GotNewMessage;
        public event newMessageDelegate MessagesGone;

        #region protected fields
        protected Process _process;
        protected IntPtr _windowHandle;
        #endregion

        #region private fields
        private bool _focused = false;
        private bool _incomeMessages = false;
        private static MessengerBase _lastAlerted = null;
        private static MessengerBase _lastActive = null;
        private WindowLifeCycle _wmmon;
        private static Collection<MessengerBase> _messengersCollection = new Collection<MessengerBase>();
        private static Collection<MessengerBase> _activity = new Collection<MessengerBase>();
        private MessengerHookManager _hManager;
        #endregion

        internal protected MessengerBase(Process msgProcess)
        {
            if (msgProcess == null)
                throw new ArgumentException();
            _process = msgProcess;
            IntPtr hWnd;
            try
            {
                var aEdel = new GetMessengerAEDel(GetMainAutomationElement);
                CacheAutomationElementProperties(msgProcess, out hWnd, ref _messengerAE, aEdel, AutomationElement.NativeWindowHandleProperty);
                CacheAutomationElementProperties(hWnd, ref _focusableAE, (s) => GetFocusHandlerAutomationElement(s), AutomationElement.ClassNameProperty, AutomationElement.NativeWindowHandleProperty);
                CacheAutomationElementProperties(hWnd, ref _incomeMessageAE, (s) => GetIncomeMessageAutomationElement(s), AutomationElement.NativeWindowHandleProperty);
            }
            catch
            {
                throw new Exception(String.Format("Can't find a messenger for this process {0}", msgProcess.ProcessName));
            }

            _windowHandle = hWnd;
            _hManager = new MessengerHookManager(_windowHandle);
            GotNewMessage += MessengerBase_GotNewMessage;
            IncomeMessages = IncomeMessagesDetect(IncomeMessageAE) ? true : false;
            //if (FocusableAE.Current.HasKeyboardFocus)
            //    _focused = true;
            _wmmon = new WindowLifeCycle();
            _wmmon.onMessageTraced += OnMessageTraced;
            _messengersCollection.Add(this);
            _activity.Add(this);

            SetForeground();
            _focused = true;
        }

        /// <summary>
        /// Создает новый экземпляр класса типа <paramref name="derivedType"/>.
        /// </summary>
        /// <param name="derivedType">Тип класса.</param>
        /// <param name="process">Процесс программы мессенджера.</param>
        /// <returns></returns>
        /// <remarks>Пример создания экземпляра класса: DerivedClass derivedClass = (DerivedClass)BaseClass.Create(typeof(DerivedClass));</remarks>
        public static MessengerBase Create(Type derivedType, Process process)
        {
            var newMessenger = (MessengerBase)Activator.CreateInstance(derivedType, process);
            return newMessenger;
        }

        public static void SetToForeground(MessengerBase messenger)
        {

        }

        public static void SelLastAlertedToForeground()
        {
            SetToForeground(LastAlerted);
        }
        /// <summary>
        /// Регистрирует метод <see cref="OnPropertyChanged"/>, который будет обрабатывать события изменения свойства.<see cref="AutomationElement.HasKeyboardFocusProperty"/>
        /// </summary>
        /// <param name="aElement">Элемент модель автоматизации пользовательского элемента, с которым небходимо связать обработчик событий.</param>
        private void AddAutomationKeyboardFocusChangedEventHandler(AutomationElement aElement)
        {

        }

        /// <summary>
        /// Callback метод события <see cref="WindowLifeCycle.onMessageTraced"/>, который вызывает освобождение ресурсов при перехвате сообщения windows о закрытии окна мессенджера или других элементов, помогающих в отслеживании состояния или управления мессенджеромю.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="hWnd">Дескриптор элемента.</param>
        /// <param name="shell">Перечисление shell событий.</param>
        protected virtual void OnMessageTraced(object sender, IntPtr hWnd, ShellEvents shell)
        {
            if (shell != ShellEvents.HSHELL_WINDOWDESTROYED)
                return;
            if (hWnd == _windowHandle || hWnd == (IntPtr)_messengerAE.Cached.NativeWindowHandle || hWnd == (IntPtr)_focusableAE.Cached.NativeWindowHandle || hWnd == (IntPtr)_incomeMessageAE.Cached.NativeWindowHandle)
                Dispose(true);
        }


        protected virtual void OnFocusChanged(object sender, EventArgs e)
        {
            if ((IntPtr)sender == (IntPtr)FocusableAE.Cached.NativeWindowHandle)
                Focused = true;
            else
                Focused = false;
        }

        protected void OnMessageProcessing(object sender, AutomationPropertyChangedEventArgs e)
        {
            var element = sender as AutomationElement;
            if (element == IncomeMessageAE)
                IncomeMessages = IncomeMessagesDetect(element) ? true : false;
        }

        void MessengerBase_GotNewMessage(MessengerBase wss)
        {
            _lastAlerted = wss;
        }

        private void OnBoundingRectangleChanged(object sender, AutomationPropertyChangedEventArgs e)
        {
            //if(e.Property)
            Dispose(true);
        }

        private static void PullFromActivity(MessengerBase messengerBase)
        {
            if (_activity.Count > 0)
                _activity.OrderByDescending((messenger) => { return messenger.NewMessagesCount; });
        }

        private static void PushToActivity(MessengerBase messengerBase)
        {
            //_activity.Add(messengerBase);
            if (_activity.Count > 0)
                _activity.OrderByDescending((messenger) => { return messenger.NewMessagesCount; });
        }

        private delegate AutomationElement GetAutomationDel(IntPtr hWnd);

        /// <summary>
        /// Кэширует заданные свойства <paramref name="properties"/> при поиске <see cref="AutomationElement"/> методом, представленным делегатом <paramref name="getAutomationDel"/> по дескриптору окна <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="element"></param>
        /// <param name="getAutomationDel"></param>
        /// <param name="properties"></param>
        private void CacheAutomationElementProperties(IntPtr hWnd, ref AutomationElement element, GetAutomationDel getAutomationDel, params AutomationProperty[] properties)
        {
            var cacheRequest = new CacheRequest();
            foreach (var property in properties)
            {
                cacheRequest.Add(property);
            }

            using (cacheRequest.Activate())
            {
                element = getAutomationDel.Invoke(hWnd);
            }
        }

        private delegate AutomationElement GetMessengerAEDel(Process process, out IntPtr hWnd);

        /// <summary>
        /// Кэширует заданные свойства <paramref name="properties"/> при поиске <see cref="AutomationElement"/> методом, представленным делегатом <paramref name="getAutomationDel"/> по процессу окна <paramref name="process"/>.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="element"></param>
        /// <param name="getAutomationDel"></param>
        /// <param name="properties"></param>
        private void CacheAutomationElementProperties(Process process, out IntPtr hWnd, ref AutomationElement element, GetMessengerAEDel getAutomationDel, params AutomationProperty[] properties)
        {
            var cacheRequest = new CacheRequest();
            foreach (var property in properties)
            {
                cacheRequest.Add(property);
            }

            using (cacheRequest.Activate())
            {
                element = getAutomationDel.Invoke(process, out hWnd);
            }
        }

        //public static void SwitchToMostActive()
        //{
        //    if (_activity.Count > 0)
        //    {
        //        var messenger = _activity.First((m) => { return m.NewMessagesCount > 0; }); //
        //        if (messenger != null)
        //            messenger.SetForeground();
        //        //else
        //        //    LastAlerted.ReturnForeground();
        //    }
        //}

        ///// <summary>
        ///// Выводит на передний план
        ///// </summary>
        //public static void SwitchToLastAlerted()
        //{
        //    if (LastAlerted == null)
        //        return;
        //    if (!LastAlerted.Focused)
        //        LastAlerted.SetForeground();
        //    else
        //        LastAlerted.ReturnForeground();

        //}

        //public static void SwitchToLastActive()
        //{
        //    if (!LastAlerted.Focused)
        //    LastActive.SetForeground();
        //    else
        //        LastActive.ReturnForeground();
        //}

        public abstract void SetForeground();

        //public abstract void ReturnForeground();

        /// <summary>
        /// Должен получать <see cref="AutomationElement"/> главного (или нет) окна процесса <paramref name="process"/> и его дескриптор.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        protected abstract AutomationElement GetMainAutomationElement(Process process, out IntPtr hWnd);
        /// <summary>
        /// Должен проверять любое изменение в объекте <paramref name="element"/>, подтверждающее или опровергающее сигнализацию о новом сообщении.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <remarks>Обычно в мессенджерах изменяется графический интерфейс иконки при получении нового сообщения, или название, тулбар.</remarks>
        protected abstract bool IncomeMessagesDetect(AutomationElement element);

        /// <summary>
        /// Определяет <see cref="AutomationElement"/> для <see cref="MessengerBase.FocusableAE"/>, который получает фокус при переключении на окно мессенджера.
        /// </summary>
        /// <param name="hWnd">Хэндл окна мессенджера.</param>
        /// <returns></returns>
        /// <remarks>По-умолчанию <see cref="AutomationElement"/> главного окна мессенджера служит получателем фокуса при переключении на него. </remarks>
        protected virtual AutomationElement GetFocusHandlerAutomationElement(IntPtr hWnd)
        {
            return MessengerAE;
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
        /// Регистрирует метод, который будет обрабатывать события изменения свойства <see cref="AutomationElement.NameProperty"/> 
        /// </summary>
        /// <remarks>Можно реализовать вручную, например через winapi SetWinEventHook.</remarks>
        protected virtual void OnMessageProcessingSubscribe()
        {
            var propertyHandler = new AutomationPropertyChangedEventHandler(OnMessageProcessing);
            Automation.AddAutomationPropertyChangedEventHandler(IncomeMessageAE, TreeScope.Element, propertyHandler, AutomationElement.NameProperty);
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                Automation.RemoveAllEventHandlers();
                GotNewMessage -= MessengerBase_GotNewMessage;
                _process = null;
                _messengerAE = null;
                _focusableAE = null;
                _incomeMessageAE = null;
                _wmmon.onMessageTraced -= OnMessageTraced;
                _wmmon = null;
                _hManager.FocusChanged -= OnFocusChanged;
                _hManager.Dispose();
            }
            _messengersCollection.Remove(this);
            _activity.Remove(this);
            disposed = true;
        }
        ~MessengerBase()
        {
            Dispose(false);
        }

    }
}
