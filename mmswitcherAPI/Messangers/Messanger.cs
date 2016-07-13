using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI;
using System.ComponentModel;

namespace mmswitcherAPI.Messangers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="wss"></param>
    public delegate void newMessageDelegate(IMessenger wss);

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
        event EventHandler GotFocus;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler LostFocus;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class MessengerBase : IMessenger
    {
        public string Caption { get; set; }
        public IntPtr WindowHandle { get { return _windowHandle; } }
        public static IMessenger LastMessageRecieved
        {
            get
            {
                return _lastMessageRecieved;
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
                }
                if (!value && !value.Equals(_incomeMessages))
                {
                    _incomeMessages = value;
                    MessagesGone(this);
                }
            }
        }

        public abstract Messenger Messenger { get; }
        public bool Focused
        {
            get { return _focused; }
            private set
            {
                if (value != _focused && value)
                    GotFocus(this, new EventArgs());
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
                    return _messengerAE;
                }
                catch (ElementNotAvailableException) { Dispose(true); return null; }
            }
        }

        private AutomationElement _focusableAE;

        protected AutomationElement FocusableAE
        {
            get
            {
                try
                {
                    return _focusableAE;
                }
                catch (ElementNotAvailableException) { Dispose(true); return null; }
            }
        }

        private AutomationElement _incomeMessageAE;

        protected AutomationElement IncomeMessageAE
        {
            get
            {
                try
                {
                    return _incomeMessageAE;
                }
                catch (ElementNotAvailableException) { Dispose(true); return null; }
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

        private bool _focused = false;
        private bool _incomeMessages = false;
        private static IMessenger _lastMessageRecieved = null;

        public MessengerBase(Process msgProcess)
        {
            if (msgProcess == null)
                throw new ArgumentException();
            _process = msgProcess;
            IntPtr hWnd;
            try
            {
                _messengerAE = GetAutomationElement(msgProcess, out hWnd);
                _focusableAE = GetFocusHandlerAutomationElement(hWnd);
                _incomeMessageAE = GetIncomeMessageAutomationElement(hWnd);
            }
            catch
            {
                throw new Exception(String.Format("Can't find a messenger for this process {0}", msgProcess.ProcessName));
            }
            _windowHandle = hWnd;
            //AddAutomationKeyboardFocusChangedEventHandler(_focusableAE);
            OnFocusChangedSubscribe();
            OnMessageProcessingSubscribe();
            GotNewMessage += MessengerBase_GotNewMessage;
            IncomeMessages = IncomeMessagesDetect(IncomeMessageAE) ? true : false;
            sdt = new System.Threading.Timer(Callback, null, 0, 3000);
        }

        System.Threading.Timer sdt;
        void Callback(object state)
        {
            var name = AutomationElement.NameProperty;
            var n = IncomeMessageAE.GetCurrentPropertyValue(name) as string;
            Console.WriteLine(IncomeMessageAE.Current.Name + "   |   " + n);
        }

        /// <summary>
        /// Регистрирует метод <see cref="OnPropertyChanged"/>, который будет обрабатывать события изменения свойства.<see cref="AutomationElement.HasKeyboardFocusProperty"/>
        /// </summary>
        /// <param name="aElement">Элемент модель автоматизации пользовательского элемента, с которым небходимо связать обработчик событий.</param>
        private void AddAutomationKeyboardFocusChangedEventHandler(AutomationElement aElement)
        {

        }

        protected void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            var element = sender as AutomationElement;
            if (element == FocusableAE)
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

        void MessengerBase_GotNewMessage(IMessenger wss)
        {
            _lastMessageRecieved = wss;
        }

        /// <summary>
        /// Должен получать <see cref="AutomationElement"/> главного (или нет) окна процесса <paramref name="process"/> и его дескриптор.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        protected abstract AutomationElement GetAutomationElement(Process process, out IntPtr hWnd);
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
        protected virtual void OnFocusChangedSubscribe()
        {
            var focusHandler = new AutomationFocusChangedEventHandler(OnFocusChanged);
            Automation.AddAutomationFocusChangedEventHandler(focusHandler);
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
                GotNewMessage -= MessengerBase_GotNewMessage;
                _process = null;
                _messengerAE = null;
                _focusableAE = null;
                _incomeMessageAE = null;
            }
            disposed = true;
        }
        ~MessengerBase()
        {
            Dispose(false);
        }
    }
}
