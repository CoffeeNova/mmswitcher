using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using NLog;
using Click_.Controls;

using mmswitcherAPI;
using mmswitcherAPI.Messengers;
using mmswitcherAPI.Messengers.Desktop;
using mmswitcherAPI.Messengers.Web;
using mmswitcherAPI.Messengers.Exceptions;
using mmswitcherAPI.AltTabSimulator;
using mmswitcherAPI.winmsg;

namespace Click_
{
    using Gbc = GlobalBindController;

    public partial class Click : Form
    {
        public Click()
        {
            InitializeComponent();
            ManualInitializing();
            Settings();

            InitAdditionalMenus();
            BindEventsSubscribe();

            ActiveWindowStackInit();

            MessengerControllerBinds.KeyChanged += Bind_BindPairChanged;
            MessengerControllerBinds.ModsChanged += Bind_ModsChanged;

            ControllersInit();

            InitMenusCondition();

            MessengersInit();
            _log.Info(string.Format("{0} is started successfully.", Constants._APPLICATION_NAME));
        }

        private void ManualInitializing()
        {
            this.contextMenuStrip1.Closing += ToolStripDropDownMenu_Closing;
            this.messengersToolStripMenuItem.DropDown.Closing += ToolStripDropDownMenu_Closing;
            this.desctopToolStripMenuItem.DropDown.Closing += ToolStripDropDownMenu_Closing;
            this.webVersionToolStripMenuItem.DropDown.Closing += ToolStripDropDownMenu_Closing;
            this.bindsToolStripMenuItem.DropDown.Closing += ToolStripDropDownMenu_Closing;
            this.notifyIcon1.Text = Constants._NOTIFY_ICON_TEXT;
            UpdateBind = new Bind(_updateBindKey, _updateBindMods);
            UpdateBind.KeyChanged += UpdateBind_KeyChanged;
            UpdateBind.ModsChanged += UpdateBind_ModsChanged;

            var list = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
            foreach (var keyModStuck in UpdateBind.BindPair.Mods)
                list.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(keyModStuck, () =>
                {
                    if (StartMessengerManually != null)
                        StartMessengerManually();
                }));
            updateGBC = new Gbc(UpdateBind.BindPair.Key, Gbc.BindMethod.RegisterHotKey, Gbc.HookBehaviour.Replacement, list);
            updateGBC.KeyProcessingDelay = Constants._CHECK_FOR_WEB_MESSENGERS_DELAY;
        }

        private void Settings()
        {
            try
            {
                #region checked menu items
                Internal.CheckRegistrySettings(ref DetectMessenger.Skype, Constants._SKYPE_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
                Internal.CheckRegistrySettings(ref DetectMessenger.Telegram, Constants._TELEGRAM_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
                Internal.CheckRegistrySettings(ref DetectMessenger.WebSkype, Constants._WEBSKYPE_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
                Internal.CheckRegistrySettings(ref DetectMessenger.WebTelegram, Constants._WEBTELEGRAM_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
                Internal.CheckRegistrySettings(ref DetectMessenger.WebWhatsApp, Constants._WEBWHATSAPP_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
                #endregion

                #region binds
                MessengerControllerBinds.LastUsed.Key = Internal.CheckRegistrySettings(Constants._LAST_USED_KEY, Constants._SETTINGS_LOCATION, MessengerControllerBinds.LastUsed.Key);
                MessengerControllerBinds.LastUsed.Mods = Internal.CheckRegistrySettings(Constants._LAST_USED_MOD, Constants._SETTINGS_LOCATION, MessengerControllerBinds.LastUsed.Mods);
                MessengerControllerBinds.MostNew.Key = Internal.CheckRegistrySettings(Constants._MOST_NEW_KEY, Constants._SETTINGS_LOCATION, MessengerControllerBinds.MostNew.Key);
                MessengerControllerBinds.MostNew.Mods = Internal.CheckRegistrySettings(Constants._MOST_NEW_MOD, Constants._SETTINGS_LOCATION, MessengerControllerBinds.MostNew.Mods);
                MessengerControllerBinds.Order.Key = Internal.CheckRegistrySettings(Constants._ORDER_KEY, Constants._SETTINGS_LOCATION, MessengerControllerBinds.Order.Key);
                MessengerControllerBinds.Order.Mods = Internal.CheckRegistrySettings(Constants._ORDER_MOD, Constants._SETTINGS_LOCATION, MessengerControllerBinds.Order.Mods);
                UpdateBind.BindPair.Key = Internal.CheckRegistrySettings(Constants._UPDATE_KEY, Constants._SETTINGS_LOCATION, UpdateBind.BindPair.Key);
                UpdateBind.BindPair.Mods = Internal.CheckRegistrySettings(Constants._UPDATE_MOD, Constants._SETTINGS_LOCATION, UpdateBind.BindPair.Mods);
                #endregion

                #region control menu
                Internal.CheckRegistrySettings(ref Control.LastUsed, Constants._LAST_USED_MENU_KEY, Constants._SETTINGS_LOCATION, false);
                Internal.CheckRegistrySettings(ref Control.MostNew, Constants._MOST_NEW_MENU_KEY, Constants._SETTINGS_LOCATION, false);
                Internal.CheckRegistrySettings(ref Control.Order, Constants._ORDER_MENU_KEY, Constants._SETTINGS_LOCATION, false);
                #endregion

            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, ex.InnerException, true);
            }
        }

        private void InitAdditionalMenus()
        {
            if (Control.LastUsed)
                AttachBindControlToToolStripMenuItem(_lastUsedBindControl, this.lastUsedToolStripMenuItem);

            if (Control.MostNew)
                AttachBindControlToToolStripMenuItem(_mostNewBindControl, this.mostNewToolStripMenuItem);

            if (Control.Order)
                AttachBindControlToToolStripMenuItem(_orderBindControl, this.orderMessengersToolStripMenuItem);

            AttachBindControlToToolStripMenuItem(_updateBindControl, this.updateToolStripMenuItem);
        }

        private void BindEventsSubscribe()
        {
            _lastUsedBindControl.SelectedBindChanged += (sender, e) => messengerBindControl_SelectedBindChanged(sender, e, SwitchBy.Recent, Constants._LAST_USED_KEY, Constants._SETTINGS_LOCATION);
            _lastUsedBindControl.SelectedModifierChanged += (sender, e) => messengerBindControl_SelectedModifierChanged(sender, e, SwitchBy.Recent, Constants._LAST_USED_MOD, Constants._SETTINGS_LOCATION);
            _mostNewBindControl.SelectedBindChanged += (sender, e) => messengerBindControl_SelectedBindChanged(sender, e, SwitchBy.Activity, Constants._MOST_NEW_KEY, Constants._SETTINGS_LOCATION);
            _mostNewBindControl.SelectedModifierChanged += (sender, e) => messengerBindControl_SelectedModifierChanged(sender, e, SwitchBy.Activity, Constants._MOST_NEW_MOD, Constants._SETTINGS_LOCATION);
            _orderBindControl.SelectedBindChanged += (sender, e) => messengerBindControl_SelectedBindChanged(sender, e, SwitchBy.Queue, Constants._ORDER_KEY, Constants._SETTINGS_LOCATION);
            _orderBindControl.SelectedModifierChanged += (sender, e) => messengerBindControl_SelectedModifierChanged(sender, e, SwitchBy.Queue, Constants._ORDER_MOD, Constants._SETTINGS_LOCATION);
            _updateBindControl.SelectedBindChanged += (sender, e) => bindControl_SelectedBindChanged(sender, e, UpdateBind, Constants._UPDATE_KEY, Constants._SETTINGS_LOCATION);
            _updateBindControl.SelectedModifierChanged += (sender, e) => bindControl_SelectedModifierChanged(sender, e, UpdateBind, Constants._UPDATE_MOD, Constants._SETTINGS_LOCATION);

        }

        void InitMenusCondition()
        {
            //1.Messengers menus.
            MenuItemsCondition(DetectMessenger.Skype, ref skypeToolStripMenuItem, Constants.MessengerCaption.Skype, Constants.MessengerCaption.Skype);
            MenuItemsCondition(DetectMessenger.Telegram, ref telegramToolStripMenuItem, Constants.MessengerCaption.Telegram, Constants.MessengerCaption.Telegram);
            MenuItemsCondition(DetectMessenger.WebSkype, ref webSkypeToolStripMenuItem, Constants.MessengerCaption.WebSkype, Constants.MessengerCaption.WebSkype);
            MenuItemsCondition(DetectMessenger.WebWhatsApp, ref webWhatsAppToolStripMenuItem, Constants.MessengerCaption.WebWhatsApp, Constants.MessengerCaption.WebWhatsApp);
            MenuItemsCondition(DetectMessenger.WebTelegram, ref webTelegramToolStripMenuItem, Constants.MessengerCaption.WebTelegram, Constants.MessengerCaption.WebTelegram);
            //2.Control menus.
            MenuItemsCondition(Control.LastUsed, ref lastUsedToolStripMenuItem, Constants.ControlCaption.LastUsed, Constants.ControlCaption.LastUsed);
            MenuItemsCondition(Control.MostNew, ref mostNewToolStripMenuItem, Constants.ControlCaption.MostNew, Constants.ControlCaption.MostNew);
            MenuItemsCondition(Control.Order, ref orderMessengersToolStripMenuItem, Constants.ControlCaption.Order, Constants.ControlCaption.Order);
            //3.Binds menus.
            if (Control.LastUsed)
                MenuItemsBind(MessengerControllerBinds.LastUsed.Key, MessengerControllerBinds.LastUsed.Mods, ref _lastUsedBindControl);
            if (Control.MostNew)
                MenuItemsBind(MessengerControllerBinds.MostNew.Key, MessengerControllerBinds.MostNew.Mods, ref _mostNewBindControl);
            if (Control.Order)
                MenuItemsBind(MessengerControllerBinds.Order.Key, MessengerControllerBinds.Order.Mods, ref _orderBindControl);
            MenuItemsBind(UpdateBind.BindPair.Key, UpdateBind.BindPair.Mods, ref _updateBindControl);
        }

        private void ActiveWindowStackInit()
        {
            _windows = ActiveWindowStack.GetInstance();
            _windows.Start();
            _windowLifeCycle = new WindowLifeCycle();
        }

        private void ControllersInit()
        {
            _mControl = MessengerController.GetInstance(_windows);
            _mControl.ActionProcessingDelay = Constants._SWITCHING_DELAY;
            MessengerControllerSubscribeRefresh();

        }

        private void MessengerControllerSubscribeRefresh()
        {
            if (Control.LastUsed)
                ControllerSubscribe(MessengerControllerBinds.LastUsed.Key, MessengerControllerBinds.LastUsed.Mods, SwitchBy.Recent);
            if (Control.MostNew)
                ControllerSubscribe(MessengerControllerBinds.MostNew.Key, MessengerControllerBinds.MostNew.Mods, SwitchBy.Activity);
            if (Control.Order)
                ControllerSubscribe(MessengerControllerBinds.Order.Key, MessengerControllerBinds.Order.Mods, SwitchBy.Queue);
        }

        private void ControllerSubscribe(Keys key, List<Gbc.KeyModifierStuck> mod, SwitchBy switchBy)
        {
            var task = new KeyValuePair<System.Windows.Forms.Keys, List<Gbc.KeyModifierStuck>>(key, mod);
            try { _mControl.SubScribe(switchBy, task); }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        private void ControllerUnsubscribe(SwitchBy switchBy)
        {
            try { _mControl.UnSubscribe(switchBy); }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        private void MessengersInit()
        {
            if (DetectMessenger.Skype)
                SubscribeFunctionForMessenger<Skype>(DetectMessenger.Skype, Messenger.Skype, Constants.MessengerCaption.Skype, Constants._SKYPE_PROCESSNAME);
            if (DetectMessenger.Telegram)
                SubscribeFunctionForMessenger<Telegram>(DetectMessenger.Telegram, Messenger.Telegram, Constants.MessengerCaption.Telegram, Constants._TELEGRAM_PROCESSNAME);
            if (DetectMessenger.WebSkype)
                SubscribeFunctionForMessenger<WebSkype>(DetectMessenger.WebSkype, Messenger.WebSkype, Constants.MessengerCaption.WebSkype, Constants.CHROME_PROCESS_NAME);
            if (DetectMessenger.WebWhatsApp)
                SubscribeFunctionForMessenger<WebWhatsApp>(DetectMessenger.WebWhatsApp, Messenger.WebWhatsApp, Constants.MessengerCaption.WebWhatsApp, Constants.CHROME_PROCESS_NAME);
            if (DetectMessenger.WebTelegram)
                SubscribeFunctionForMessenger<WebTelegram>(DetectMessenger.WebTelegram, Messenger.WebTelegram, Constants.MessengerCaption.WebTelegram, Constants.CHROME_PROCESS_NAME);
        }

        private Task MessengerStartAsync<T>(string processName, string caption) where T : IMessenger
        {
            var exist = MessengerBase.MessengersCollection.Any((p) => p.GetType() == typeof(T));
            if (exist)
                return null;
            return Task.Run(() => MessengerStart<T>(processName, caption));
        }

        private bool MessengerStart<T>(string processName, string caption) where T : IMessenger
        {
            var exist = MessengerBase.MessengersCollection.Any((p) => p.GetType() == typeof(T));
            if (exist)
                return false;

            var processes = Process.GetProcessesByName(processName);
            if (processes.Count() == 0)
                return false;
            Process process = null;

            if (typeof(T).BaseType == typeof(WebMessenger))
                process = processes.FirstOrDefault((p) => p.MainWindowHandle != IntPtr.Zero);
            if (typeof(T).BaseType == typeof(DesktopMessenger))
                process = processes.FirstOrDefault();
            if (process == null)
                return false;

            Thread.Sleep(1000); //delay to load window 
            try
            {
                var messenger = MessengerBase.Create<T>(process);
                messenger.Caption = caption;
                messenger.GotNewMessage += messenger_GotNewMessage;
                messenger.MessageGone += messenger_MessageGone;
            }
            catch (MessengerBuildException ex)
            {
                ExceptionHandler.Handle(ex, typeof(T).ToString(), false);
                return false;
            }
            return true;
        }

        void messenger_MessageGone(MessengerBase wss)
        {
            var icon = Properties.Resources.winlogo_yellow;
            if (this.notifyIcon1.Icon.Equals(icon))
                return;

            if (!MessengerBase.MessengersCollection.Any((p) => p.IncomeMessages > 0))
                this.notifyIcon1.Icon = icon;
        }

        void messenger_GotNewMessage(MessengerBase wss)
        {
            var icon = Properties.Resources.winlogo_yellow_with_message;
            if (this.notifyIcon1.Icon.Equals(icon))
                return;

            if (MessengerBase.MessengersCollection.Any((p) => p.IncomeMessages > 0))
                this.notifyIcon1.Icon = icon;
        }

        private void ToolStripMenuItemsConditionChanger(ToolStripMenuItem menuItem, string itemText1, string itemText2, Action<bool> func1, Action<bool> func2)
        {
            if (!menuItem.Checked)
            {
                menuItem.Text = itemText1;
                func1(false);
            }
            else
            {
                menuItem.Text = itemText2;
                func2(true);
            }
        }

        private void AttachBindControlToToolStripMenuItem(BindControl bControl, ToolStripMenuItem menuItem)
        {
            if (menuItem.DropDownItems.Count > 0)
                return;

            System.Windows.Forms.ToolStripControlHost tsHost;

            tsHost = new System.Windows.Forms.ToolStripControlHost(bControl);
            menuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            tsHost});
        }

        private void DettachBindControlToToolStripMenuItem(ToolStripMenuItem menuItem)
        {
            menuItem.DropDownItems.Clear();
            menuItem.DropDown = null;
        }

        private void ControlMenuRefresh(SwitchBy switchBy)
        {
            bool state = false;
            BindControl bindControl = null;
            ToolStripMenuItem menuItem = null;
            BindPair bindPair = null;

            switch (switchBy)
            {
                case SwitchBy.Recent:
                    bindControl = _lastUsedBindControl;
                    menuItem = this.lastUsedToolStripMenuItem;
                    state = Control.LastUsed;
                    bindPair = MessengerControllerBinds.LastUsed;
                    break;
                case SwitchBy.Activity:
                    bindControl = _mostNewBindControl;
                    menuItem = this.mostNewToolStripMenuItem;
                    state = Control.MostNew;
                    bindPair = MessengerControllerBinds.MostNew;
                    break;
                case SwitchBy.Queue:
                    bindControl = _orderBindControl;
                    menuItem = this.orderMessengersToolStripMenuItem;
                    state = Control.Order;
                    bindPair = MessengerControllerBinds.Order;
                    break;
            }

            if (state)
            {
                AttachBindControlToToolStripMenuItem(bindControl, menuItem);
                MenuItemsBind(bindPair.Key, bindPair.Mods, ref bindControl);
                var parent = menuItem.DropDown.OwnerItem;
                menuItem.DropDown.Show(parent.Bounds.Location, ToolStripDropDownDirection.Default);
            }
            else
            {
                menuItem.DropDown.Hide();
                DettachBindControlToToolStripMenuItem(menuItem);
            }
        }

        private void ControllerAction(SwitchBy switchBy, bool state)
        {
            BindPair bindPair = null;

            switch (switchBy)
            {
                case SwitchBy.Recent:
                    bindPair = MessengerControllerBinds.LastUsed;
                    break;
                case SwitchBy.Activity:
                    bindPair = MessengerControllerBinds.MostNew;
                    break;
                case SwitchBy.Queue:
                    bindPair = MessengerControllerBinds.Order;
                    break;
            }
            if (state)
                ControllerSubscribe(bindPair.Key, bindPair.Mods, switchBy);
            else
                ControllerUnsubscribe(switchBy);
        }

        private async void SubscribeFunctionForMessenger<T>(bool isSubscribe, Messenger messenger, string messengerCaption, string processName) where T : IMessenger
        {
            if (isSubscribe)
            {
                await MessengerStartAsync<T>(processName, messengerCaption);
                if (typeof(T).BaseType.Equals(typeof(DesktopMessenger)))
                    _windowLifeCycle.onMessageTraced += (sender, hWnd, shell) => _windowLifeCycle_onMessageTraced(sender, hWnd, shell, messenger);
                if (typeof(T).BaseType.Equals(typeof(WebMessenger)))
                {
                    StartMessengerManually += () => Click_StartMessengerManually<T>(processName, messengerCaption);
                    updateGBC.Execute = true;
                }
            }
            else
            {
                MessengerBase.MessengersCollection.RemoveAll((x) =>
                {
                    if (x.GetType() == typeof(T))
                    {
                        x.Dispose();
                        return true;
                    }
                    return false;
                });
                if (typeof(T).BaseType.Equals(typeof(DesktopMessenger)))
                    _windowLifeCycle.onMessageTraced -= (sender, hWnd, shell) => _windowLifeCycle_onMessageTraced(sender, hWnd, shell, messenger);
                if (typeof(T).BaseType.Equals(typeof(WebMessenger)))
                {
                    StartMessengerManually -= () => Click_StartMessengerManually<T>(processName, messengerCaption);
                    if (StartMessengerManually == null)
                        updateGBC.Execute = false;
                }
            }
        }

        private void Click_StartMessengerManually<T>(string processName, string caption) where T : IMessenger
        {
            MessengerStartAsync<T>(processName, caption);
        }

        #region MOUSE CLICK MENUS CALLBACKS

        private void activityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(lastUsedToolStripMenuItem, ref Control.LastUsed, Constants._LAST_USED_MENU_KEY, Constants.ControlCaption.LastUsed, Constants.ControlCaption.LastUsed);
            ControllerAction(SwitchBy.Recent, Control.LastUsed);
            ControlMenuRefresh(SwitchBy.Recent);
        }

        private void lastActiveMessengerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(mostNewToolStripMenuItem, ref Control.MostNew, Constants._MOST_NEW_MENU_KEY, Constants.ControlCaption.MostNew, Constants.ControlCaption.MostNew);
            ControllerAction(SwitchBy.Activity, Control.MostNew);
            ControlMenuRefresh(SwitchBy.Activity);
        }

        private void orderMessengersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(orderMessengersToolStripMenuItem, ref Control.Order, Constants._ORDER_MENU_KEY, Constants.ControlCaption.Order, Constants.ControlCaption.Order);
            ControllerAction(SwitchBy.Queue, Control.Order);
            ControlMenuRefresh(SwitchBy.Queue);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _log.Info(string.Format("{0} is closed.", Constants._APPLICATION_NAME));
            Application.Exit();
        }

        private void skypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(sender as ToolStripMenuItem, ref DetectMessenger.Skype, Constants._SKYPE_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.Skype, Constants.MessengerCaption.Skype);
            SubscribeFunctionForMessenger<Skype>(DetectMessenger.Skype, Messenger.Skype, Constants.MessengerCaption.Skype, Constants._SKYPE_PROCESSNAME);
        }

        private void telegramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(telegramToolStripMenuItem, ref DetectMessenger.Telegram, Constants._TELEGRAM_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.Telegram, Constants.MessengerCaption.Telegram);
            SubscribeFunctionForMessenger<Telegram>(DetectMessenger.Telegram, Messenger.Telegram, Constants.MessengerCaption.Telegram, Constants._TELEGRAM_PROCESSNAME);
        }

        private void webSkypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(webSkypeToolStripMenuItem, ref DetectMessenger.WebSkype, Constants._WEBSKYPE_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.WebSkype, Constants.MessengerCaption.WebSkype);
            SubscribeFunctionForMessenger<WebSkype>(DetectMessenger.WebSkype, Messenger.WebSkype, Constants.MessengerCaption.WebSkype, Constants.CHROME_PROCESS_NAME);
        }

        private void webWhatsAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(webWhatsAppToolStripMenuItem, ref DetectMessenger.WebWhatsApp, Constants._WEBWHATSAPP_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.WebWhatsApp, Constants.MessengerCaption.WebWhatsApp);
            SubscribeFunctionForMessenger<WebWhatsApp>(DetectMessenger.WebWhatsApp, Messenger.WebWhatsApp, Constants.MessengerCaption.WebWhatsApp, Constants.CHROME_PROCESS_NAME);
        }

        private void webTelegramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(webTelegramToolStripMenuItem, ref DetectMessenger.WebTelegram, Constants._WEBTELEGRAM_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.WebTelegram, Constants.MessengerCaption.WebTelegram);
            SubscribeFunctionForMessenger<WebTelegram>(DetectMessenger.WebTelegram, Messenger.WebTelegram, Constants.MessengerCaption.WebTelegram, Constants.CHROME_PROCESS_NAME);
        }

        #endregion

        private void ToolStripMenuStateOperator(ToolStripMenuItem tsmi, ref bool savedValue, string regValueName, string textTrue, string textFalse)
        {
            bool tempSavedValue = savedValue;
            try
            {
                var saveAction = new Action<bool>((value) =>
                {
                    Internal.SaveRegistrySettings(ref tempSavedValue, regValueName, Constants._SETTINGS_LOCATION, value);
                    _log.Info("Now value name \"{0}\" of registry key \"{1}\" has a \"{2}\" value.", regValueName, Constants._SETTINGS_LOCATION, value);
                });

                ToolStripMenuItemsConditionChanger(tsmi, textFalse, textTrue, saveAction, saveAction);
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
            savedValue = tempSavedValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="menuItem"></param>
        /// <param name="enabledText"></param>
        /// <param name="disabledText"></param>
        private void MenuItemsCondition(bool condition, ref ToolStripMenuItem menuItem, string enabledText, string disabledText)
        {
            if (condition == true)
            {
                menuItem.Checked = true;
                menuItem.Text = enabledText;
            }
            else
            {
                menuItem.Checked = false;
                menuItem.Text = disabledText;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mod"></param>
        /// <param name="menuItem"></param>
        private void MenuItemsBind(Keys key, List<Gbc.KeyModifierStuck> mod, ref Controls.BindControl menuItem)
        {
            menuItem.Bind = key;
            if (mod != null && mod.Count > 0)
                menuItem.Modifier = mod.First();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        #region binds changed callbacks

        private delegate Keys KeyGetter();
        private delegate void KeySetter(Keys key);
        private delegate List<Gbc.KeyModifierStuck> ModGetter();
        private delegate void ModSetter(List<Gbc.KeyModifierStuck> mod);

        private class KeyProp
        {
            public KeyProp(KeyGetter get, KeySetter set)
            {
                KeyGetter = get;
                KeySetter = set;
            }
            public KeyGetter KeyGetter;
            public KeySetter KeySetter;
        }

        private class ModProp
        {
            public ModProp(ModGetter get, ModSetter set)
            {
                ModGetter = get;
                ModSetter = set;
            }
            public ModGetter ModGetter;
            public ModSetter ModSetter;
        }

        private KeyProp CaseKey(SwitchBy sb)
        {
            switch (sb)
            {
                case SwitchBy.Recent:
                    return new KeyProp(() => _mControl.RecentKey, (s) => _mControl.RecentKey = s);
                case SwitchBy.Activity:
                    return new KeyProp(() => _mControl.ActivityKey, (s) => _mControl.ActivityKey = s);
                case SwitchBy.Queue:
                    return new KeyProp(() => _mControl.QueueKey, (s) => _mControl.QueueKey = s);
            }

            throw new ArgumentException("Not described BindControl");
        }

        private ModProp CaseMod(SwitchBy sb)
        {
            switch (sb)
            {
                case SwitchBy.Recent:
                    return new ModProp(() => _mControl.RecentModifiers, (s) => _mControl.RecentModifiers = s);
                case SwitchBy.Activity:
                    return new ModProp(() => _mControl.ActivityModifiers, (s) => _mControl.ActivityModifiers = s);
                case SwitchBy.Queue:
                    return new ModProp(() => _mControl.QueueModifiers, (s) => _mControl.QueueModifiers = s);
            }

            throw new ArgumentException("Not described BindControl");
        }

        void messengerBindControl_SelectedBindChanged(object sender, EventArgs e, SwitchBy switchBy, string valueName, string keyLocation)
        {
            BindPair bp = MessengerControllerBinds.DefineBindPair(switchBy);
            var bindControl = sender as BindControl;
            if (bindControl.Bind == bp.Key)
                return;

            try
            {
                bp.Key = Internal.SaveRegistrySettings(valueName, keyLocation, bindControl.Bind);
                _log.Info("Now value name \"{0}\" of registry key \"{1}\" has a \"{2}\" value.", valueName, keyLocation, bindControl.Bind);
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        void messengerBindControl_SelectedModifierChanged(object sender, EventArgs e, SwitchBy switchBy, string valueName, string keyLocation)
        {
            BindPair bp = MessengerControllerBinds.DefineBindPair(switchBy);
            var bindControl = sender as BindControl;

            if (bindControl.Modifier == bp.Mods.First())
                return;
            try
            {
                bp.Mods = Internal.SaveRegistrySettings(valueName, keyLocation, new List<Gbc.KeyModifierStuck> { bindControl.Modifier });
                _log.Info("Now value name \"{0}\" of registry key \"{1}\" has a \"{2}\" value.", valueName, keyLocation, bindControl.Modifier.ToString());
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        void bindControl_SelectedBindChanged(object sender, EventArgs e, Bind bind, string valueName, string keyLocation)
        {
            var bindControl = sender as BindControl;
            if (bindControl.Bind == bind.BindPair.Key)
                return;

            try
            {
                bind.BindPair.Key = Internal.SaveRegistrySettings(valueName, keyLocation, bindControl.Bind);
                _log.Info("Now value name \"{0}\" of registry key \"{1}\" has a \"{2}\" value.", valueName, keyLocation, bindControl.Bind);
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        void bindControl_SelectedModifierChanged(object sender, EventArgs e, Bind bind, string valueName, string keyLocation)
        {
            var bindControl = sender as BindControl;
            if (bindControl.Modifier == bind.BindPair.Mods.First())
                return;

            try
            {
                bind.BindPair.Mods = Internal.SaveRegistrySettings(valueName, keyLocation, new List<Gbc.KeyModifierStuck> { bindControl.Modifier });
                _log.Info("Now value name \"{0}\" of registry key \"{1}\" has a \"{2}\" value.", valueName, keyLocation, bindControl.Modifier.ToString());
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        #endregion

        #region other events callbacks

        void ToolStripDropDownMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                e.Cancel = true;
        }

        void Bind_BindPairChanged(SwitchBy switchBy, BindPair bindPair)
        {
            KeyProp kp = CaseKey(switchBy);
            kp.KeySetter(bindPair.Key);
        }

        void Bind_ModsChanged(SwitchBy switchBy, BindPair bindPair)
        {
            ModProp mp = CaseMod(switchBy);
            mp.ModSetter(bindPair.Mods);
        }

        void UpdateBind_KeyChanged(BindPair bindPair)
        {
            updateGBC.Key = bindPair.Key;
        }

        void UpdateBind_ModsChanged(BindPair bindPair)
        {
            var list = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
            foreach (var keyModStuck in UpdateBind.BindPair.Mods)
                list.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(keyModStuck, () =>
                {
                    if (StartMessengerManually != null)
                        StartMessengerManually();
                }));
            updateGBC.Tasks = list;
        }

        void _windowLifeCycle_onMessageTraced(object sender, IntPtr hWnd, ShellEvents shell, Messenger messenger)
        {
            if (shell != ShellEvents.HSHELL_WINDOWCREATED)
                return;
            var processName = Internal.DefineProcessName(hWnd);
            switch (messenger)
            {
                case (Messenger.Skype):
                    if (processName.Equals(Constants._SKYPE_PROCESSNAME))
                        MessengerStartAsync<Skype>(processName, Constants.MessengerCaption.Skype);
                    break;
                case (Messenger.Telegram):
                    if (processName.Equals(Constants._TELEGRAM_PROCESSNAME))
                        MessengerStartAsync<Telegram>(processName, Constants.MessengerCaption.Telegram);
                    break;
            }

        }
        #endregion

        #region events
        private delegate void StartMessengerDel();
        private static event StartMessengerDel StartMessengerManually;
        #endregion

        #region private variables
        private static MessengerController _mControl;
        private ActiveWindowStack _windows;
        private WindowLifeCycle _windowLifeCycle;
        private BindControl _lastUsedBindControl = new BindControl();
        private BindControl _mostNewBindControl = new BindControl();
        private BindControl _orderBindControl = new BindControl();
        private BindControl _updateBindControl = new BindControl();
        private System.ComponentModel.ComponentResourceManager _resources =
            new System.ComponentModel.ComponentResourceManager(typeof(Click));
        private static Logger _log = LogManager.GetCurrentClassLogger();
        private GlobalBindController updateGBC;
        private Bind UpdateBind;
        private static readonly Keys _updateBindKey = Keys.C;
        private static readonly List<Gbc.KeyModifierStuck> _updateBindMods = new List<Gbc.KeyModifierStuck>() { Gbc.KeyModifierStuck.Alt};
        #endregion

        #region private properties
        #endregion

        #region events
        #endregion

    }

}
