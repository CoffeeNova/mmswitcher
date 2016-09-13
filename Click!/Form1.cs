using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Click_.Controls;
using System.Diagnostics;
using mmswitcherAPI;
using mmswitcherAPI.Messengers;
using mmswitcherAPI.Messengers.Desktop;
using mmswitcherAPI.Messengers.Web;
using mmswitcherAPI.Messengers.Exceptions;
using mmswitcherAPI.AltTabSimulator;



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

            Bind.KeyChanged += Bind_BindPairChanged;
            Bind.ModsChanged += Bind_ModsChanged;
            ControllerInit();

            InitMenusCondition();

            MessengersInit();

        }

        private void ManualInitializing()
        {
            this.contextMenuStrip1.Closing += ToolStripDropDownMenu_Closing;
            this.messengersToolStripMenuItem.DropDown.Closing += ToolStripDropDownMenu_Closing;
            this.desctopToolStripMenuItem.DropDown.Closing += ToolStripDropDownMenu_Closing;
            this.webVersionToolStripMenuItem.DropDown.Closing += ToolStripDropDownMenu_Closing;
            this.bindsToolStripMenuItem.DropDown.Closing += ToolStripDropDownMenu_Closing;
            this.notifyIcon1.Text = Constants._NOTIFY_ICON_TEXT;
        }



        private void Settings()
        {
            #region checked menu items
            Internal.CheckRegistrySettings(ref DetectMessenger.Skype, Constants._SKYPE_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
            Internal.CheckRegistrySettings(ref DetectMessenger.Telegram, Constants._TELEGRAM_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
            Internal.CheckRegistrySettings(ref DetectMessenger.WebSkype, Constants._WEBSKYPE_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
            Internal.CheckRegistrySettings(ref DetectMessenger.WebTelegram, Constants._WEBTELEGRAM_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
            Internal.CheckRegistrySettings(ref DetectMessenger.WebWhatsApp, Constants._WEBWHATSAPP_REGISTRYKEY_VALUENAME, Constants._SETTINGS_LOCATION, false);
            #endregion

            #region binds
            Internal.CheckRegistrySettings(ref Bind.LastUsed.Key, Constants._LAST_USED_KEY, Constants._SETTINGS_LOCATION, Bind.LastUsed.Key);
            Internal.CheckRegistrySettings(ref Bind.LastUsed.Mods, Constants._LAST_USED_MOD, Constants._SETTINGS_LOCATION, Bind.LastUsed.Mods);
            Internal.CheckRegistrySettings(ref Bind.MostNew.Key, Constants._MOST_NEW_KEY, Constants._SETTINGS_LOCATION, Bind.MostNew.Key);
            Internal.CheckRegistrySettings(ref Bind.MostNew.Mods, Constants._MOST_NEW_MOD, Constants._SETTINGS_LOCATION, Bind.MostNew.Mods);
            Internal.CheckRegistrySettings(ref Bind.Order.Key, Constants._ORDER_KEY, Constants._SETTINGS_LOCATION, Bind.Order.Key);
            Internal.CheckRegistrySettings(ref Bind.Order.Mods, Constants._ORDER_MOD, Constants._SETTINGS_LOCATION, Bind.Order.Mods);
            #endregion

            #region control menu
            Internal.CheckRegistrySettings(ref Control.LastUsed, Constants._LAST_USED_MENU_KEY, Constants._SETTINGS_LOCATION, false);
            Internal.CheckRegistrySettings(ref Control.MostNew, Constants._MOST_NEW_MENU_KEY, Constants._SETTINGS_LOCATION, false);
            Internal.CheckRegistrySettings(ref Control.Order, Constants._ORDER_MENU_KEY, Constants._SETTINGS_LOCATION, false);
            #endregion

        }

        private void InitAdditionalMenus()
        {
            if (Control.LastUsed)
                AttachBindControlToToolStripMenuItem(_lastUsedBindControl, this.lastUsedToolStripMenuItem);

            if (Control.MostNew)
                AttachBindControlToToolStripMenuItem(_mostNewBindControl, this.mostNewToolStripMenuItem);

            if (Control.Order)
                AttachBindControlToToolStripMenuItem(_orderBindControl, this.orderMessengersToolStripMenuItem);
        }

        private void BindEventsSubscribe()
        {
            _lastUsedBindControl.SelectedBindChanged += (sender, e) => bindControl_SelectedBindChanged(sender, e, SwitchBy.Recent, Constants._LAST_USED_KEY, Constants._SETTINGS_LOCATION);
            _lastUsedBindControl.SelectedModifierChanged += (sender, e) => bindControl_SelectedModifierChanged(sender, e, SwitchBy.Recent, Constants._LAST_USED_MOD, Constants._SETTINGS_LOCATION);
            _mostNewBindControl.SelectedBindChanged += (sender, e) => bindControl_SelectedBindChanged(sender, e, SwitchBy.Activity, Constants._MOST_NEW_KEY, Constants._SETTINGS_LOCATION);
            _mostNewBindControl.SelectedModifierChanged += (sender, e) => bindControl_SelectedModifierChanged(sender, e, SwitchBy.Activity, Constants._MOST_NEW_MOD, Constants._SETTINGS_LOCATION);
            _orderBindControl.SelectedBindChanged += (sender, e) => bindControl_SelectedBindChanged(sender, e, SwitchBy.Queue, Constants._ORDER_KEY, Constants._SETTINGS_LOCATION);
            _orderBindControl.SelectedModifierChanged += (sender, e) => bindControl_SelectedModifierChanged(sender, e, SwitchBy.Queue, Constants._ORDER_MOD, Constants._SETTINGS_LOCATION);
        }



        private void InitMenusCondition()
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
                MenuItemsBind(Bind.LastUsed.Key, Bind.LastUsed.Mods, ref _lastUsedBindControl);
            if (Control.MostNew)
                MenuItemsBind(Bind.MostNew.Key, Bind.MostNew.Mods, ref _mostNewBindControl);
            if (Control.Order)
                MenuItemsBind(Bind.Order.Key, Bind.Order.Mods, ref _orderBindControl);
        }

        private void ActiveWindowStackInit()
        {
            _windows = ActiveWindowStack.GetInstance();
            _windows.Start();
        }

        private void ControllerInit()
        {
            _mControl = MessengerController.GetInstance(_windows);

            ControllerSubscribeRefresh();
        }

        private void ControllerSubscribeRefresh()
        {
            if (Control.LastUsed)
                ControllerSubscribe(Bind.LastUsed.Key, Bind.LastUsed.Mods, SwitchBy.Recent);
            if (Control.MostNew)
                ControllerSubscribe(Bind.MostNew.Key, Bind.MostNew.Mods, SwitchBy.Activity);
            if (Control.Order)
                ControllerSubscribe(Bind.Order.Key, Bind.Order.Mods, SwitchBy.Queue);
        }

        private void ControllerSubscribe(Keys key, List<Gbc.KeyModifierStuck> mod, SwitchBy switchBy)
        {
            var task = new KeyValuePair<System.Windows.Forms.Keys, List<Gbc.KeyModifierStuck>>(key, mod);
            try { _mControl.SubScribe(switchBy, task); }
            catch (InvalidOperationException) { }
        }

        private void ControllerUnsubscribe(SwitchBy switchBy)
        {
            try { _mControl.UnSubscribe(switchBy); }
            catch (InvalidOperationException) { }
        }

        private void MessengersInit()
        {
            if (DetectMessenger.Skype)
                SubscribeFunctionForDesktopMessenger<Skype>(DetectMessenger.Skype, Messenger.Skype, Constants.MessengerCaption.Skype, Constants._SKYPE_PROCESSNAME);
            if (DetectMessenger.Telegram)
                SubscribeFunctionForDesktopMessenger<Telegram>(DetectMessenger.Telegram, Messenger.Telegram, Constants.MessengerCaption.Telegram, Constants._TELEGRAM_PROCESSNAME);
            if (DetectMessenger.WebSkype)
                SubscribeFunctionForWebMessenger<WebSkype>(DetectMessenger.WebSkype, Messenger.WebSkype, Constants.MessengerCaption.WebSkype);
            if (DetectMessenger.WebWhatsApp)
                SubscribeFunctionForWebMessenger<WebWhatsApp>(DetectMessenger.WebWhatsApp, Messenger.WebWhatsApp, Constants.MessengerCaption.WebWhatsApp);
            if (DetectMessenger.WebTelegram)
                SubscribeFunctionForWebMessenger<WebTelegram>(DetectMessenger.WebTelegram, Messenger.WebTelegram, Constants.MessengerCaption.WebTelegram);
        }

        //for web messengers version
        private void MessengerStart<T>(string caption) where T : WebMessenger
        {
            var processNameList = Constants._BROWSERS_PROCESSNAME;

            foreach (var processName in processNameList)
                MessengerStart<T>(processName, caption + " - " + processName);

        }

        private void MessengerStart<T>(string processName, string caption) where T : IMessenger
        {
            var exist = MessengerBase.MessengersCollection.Any((p) => p.GetType() == typeof(T));
            if (exist)
                return;
            var processes = Process.GetProcessesByName(processName);
            if (processes.Count() == 0)
                return;
            Process process = null;

            if (typeof(T).BaseType == typeof(WebMessenger))
                process = processes.FirstOrDefault((p) => p.MainWindowHandle != IntPtr.Zero);
            if (typeof(T).BaseType == typeof(DesktopMessenger))
                process = processes.FirstOrDefault();
            if (process == null)

                return;
            bool created = false;
            while (!created)
            {
                try
                {
                    var messenger = MessengerBase.Create<T>(process);
                    messenger.Caption = caption;

                    messenger.GotNewMessage += messenger_GotNewMessage;
                    messenger.MessageGone += messenger_MessageGone;
                    created = true;
                }
                catch (MessengerBuildException) { System.Threading.Thread.Sleep(2000); }
            }

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
                    bindPair = Bind.LastUsed;
                    break;
                case SwitchBy.Activity:
                    bindControl = _mostNewBindControl;
                    menuItem = this.mostNewToolStripMenuItem;
                    state = Control.MostNew;
                    bindPair = Bind.MostNew;
                    break;
                case SwitchBy.Queue:
                    bindControl = _orderBindControl;
                    menuItem = this.orderMessengersToolStripMenuItem;
                    state = Control.Order;
                    bindPair = Bind.Order;
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
                    bindPair = Bind.LastUsed;
                    break;
                case SwitchBy.Activity:
                    bindPair = Bind.MostNew;
                    break;
                case SwitchBy.Queue:
                    bindPair = Bind.Order;
                    break;
            }
            if (state)
                ControllerSubscribe(bindPair.Key, bindPair.Mods, switchBy);
            else
                ControllerUnsubscribe(switchBy);
        }

        private void SubscribeFunctionForDesktopMessenger<T>(bool isSubscribe, Messenger messenger, string messengerCaption, string processName) where T : DesktopMessenger
        {
            if (isSubscribe)
            {
                MessengerStart<T>(messengerCaption, processName);
                _windows.onActiveWindowStackChanged += (action, hWnd) => _windows_onActiveWindowStackChanged(action, hWnd, messenger);
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
                _windows.onActiveWindowStackChanged -= (action, hWnd) => _windows_onActiveWindowStackChanged(action, hWnd, messenger);
            }
        }

        private void SubscribeFunctionForWebMessenger<T>(bool isSubscribe, Messenger messenger, string messengerCaption) where T : WebMessenger
        {
            if (isSubscribe)
            {
                MessengerStart<T>(messengerCaption);
                _windows.onActiveWindowStackChanged += (action, hWnd) => _windows_onActiveWindowStackChanged(action, hWnd, messenger);
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
                _windows.onActiveWindowStackChanged -= (action, hWnd) => _windows_onActiveWindowStackChanged(action, hWnd, messenger);
            }
        }

        #region MOUSE CLICK MENUS CALLBACKS

        private void activityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(lastUsedToolStripMenuItem, ref Control.LastUsed, Constants._LAST_USED_MENU_KEY, Constants.ControlCaption.LastUsed, Constants.ControlCaption.LastUsed);
            ControllerAction(SwitchBy.Recent, Control.LastUsed);
            ControlMenuRefresh(SwitchBy.Recent);
            //lastUsedToolStripMenuItem.DropDown.Refresh();
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
            System.Environment.Exit(0);
        }

        private void skypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(sender as ToolStripMenuItem, ref DetectMessenger.Skype, Constants._SKYPE_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.Skype, Constants.MessengerCaption.Skype);
            SubscribeFunctionForDesktopMessenger<Skype>(DetectMessenger.Skype, Messenger.Skype, Constants.MessengerCaption.Skype, Constants._SKYPE_PROCESSNAME);
        }

        private void telegramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(telegramToolStripMenuItem, ref DetectMessenger.Telegram, Constants._TELEGRAM_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.Telegram, Constants.MessengerCaption.Telegram);
            SubscribeFunctionForDesktopMessenger<Telegram>(DetectMessenger.Telegram, Messenger.Telegram, Constants.MessengerCaption.Telegram, Constants._TELEGRAM_PROCESSNAME);
        }

        private void webSkypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(webSkypeToolStripMenuItem, ref DetectMessenger.WebSkype, Constants._WEBSKYPE_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.WebSkype, Constants.MessengerCaption.WebSkype);
            SubscribeFunctionForWebMessenger<WebSkype>(DetectMessenger.WebSkype, Messenger.WebSkype, Constants.MessengerCaption.WebSkype);
        }

        private void webWhatsAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(webWhatsAppToolStripMenuItem, ref DetectMessenger.WebWhatsApp, Constants._WEBWHATSAPP_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.WebWhatsApp, Constants.MessengerCaption.WebWhatsApp);
            SubscribeFunctionForWebMessenger<WebWhatsApp>(DetectMessenger.WebWhatsApp, Messenger.WebWhatsApp, Constants.MessengerCaption.WebWhatsApp);
        }

        private void webTelegramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuStateOperator(webTelegramToolStripMenuItem, ref DetectMessenger.WebTelegram, Constants._WEBTELEGRAM_REGISTRYKEY_VALUENAME, Constants.MessengerCaption.WebTelegram, Constants.MessengerCaption.WebTelegram);
            SubscribeFunctionForWebMessenger<WebTelegram>(DetectMessenger.WebTelegram, Messenger.WebTelegram, Constants.MessengerCaption.WebTelegram);
        }

        #endregion

        private void ToolStripMenuStateOperator(ToolStripMenuItem tsmi, ref bool savedValue, string regValueName, string textTrue, string textFalse)
        {
            bool tempSavedValue = savedValue;
            try
            {
                var saveAction = new Action<bool>((value) => { Internal.SaveRegistrySettings(ref tempSavedValue, regValueName, Constants._SETTINGS_LOCATION, value); });

                ToolStripMenuItemsConditionChanger(tsmi, textFalse, textTrue, saveAction, saveAction);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
                System.Environment.Exit(0);
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

        void bindControl_SelectedBindChanged(object sender, EventArgs e, SwitchBy switchBy, string valueName, string keyLocation)
        {
            BindPair bp = Bind.DefineBindPair(switchBy);
            var bindControl = sender as BindControl;

            if (bindControl.Bind == bp.Key)
                return;

            try
            {
                Internal.SaveRegistrySettings(ref bp.Key, valueName, keyLocation, bindControl.Bind);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
                System.Environment.Exit(0);
            }
        }

        void bindControl_SelectedModifierChanged(object sender, EventArgs e, SwitchBy switchBy, string keyName, string keyLocation)
        {
            BindPair bp = Bind.DefineBindPair(switchBy);
            var bindControl = sender as BindControl;
            var mod = bp.Mods;

            if (bindControl.Modifier == mod.First())
                return;
            try
            {
                Internal.SaveRegistrySettings(ref mod, keyName, keyLocation, new List<Gbc.KeyModifierStuck> { bindControl.Modifier });
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
                System.Environment.Exit(0);
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

        void _windows_onActiveWindowStackChanged(StackAction action, IntPtr hWnd, Messenger messenger)
        {
            if (action != StackAction.Added)
                return;

            var processName = Internal.DefineProcessName(hWnd);
            switch (messenger)
            {
                case (Messenger.Skype):
                    if (processName.Equals(Constants._SKYPE_PROCESSNAME))
                        MessengerStart<Skype>(processName, Constants.MessengerCaption.Skype);
                    break;
                case (Messenger.Telegram):
                    if (processName.Equals(Constants._TELEGRAM_PROCESSNAME))
                        MessengerStart<Telegram>(processName, Constants.MessengerCaption.Telegram);
                    break;
                case (Messenger.WebSkype):
                    if (Constants._BROWSERS_PROCESSNAME.Any((p) => p.Equals(processName)))
                        MessengerStart<WebSkype>(processName, Constants.MessengerCaption.WebSkype);
                    break;
                case (Messenger.WebTelegram):
                    if (Constants._BROWSERS_PROCESSNAME.Any((p) => p.Equals(processName)))
                        MessengerStart<WebTelegram>(processName, Constants.MessengerCaption.WebTelegram);
                    break;
                case (Messenger.WebWhatsApp):
                    if (Constants._BROWSERS_PROCESSNAME.Any((p) => p.Equals(processName)))
                        MessengerStart<WebWhatsApp>(processName, Constants.MessengerCaption.WebWhatsApp);
                    break;
            }

        }
        #endregion


        #region private variables
        private static MessengerController _mControl;
        private ActiveWindowStack _windows;
        //private Skype _skype;
        //private Telegram _telegram;
        //private WebSkype _webSkype;
        private BindControl _lastUsedBindControl = new BindControl();
        private BindControl _mostNewBindControl = new BindControl();
        private BindControl _orderBindControl = new BindControl();
        private System.ComponentModel.ComponentResourceManager _resources =
            new System.ComponentModel.ComponentResourceManager(typeof(Click));
        #endregion

        #region private properties
        #endregion

        #region events
        //private delegate void MessengerWaitingEventHandler(StackAction action, IntPtr hWnd, Messenger messenger);
        //private event MessengerWaitingEventHandler MessengerWaitingEvent
        //{
        //    add
        //    {
        //        if (_messengerWaitingEventSubscribed)
        //            _windows.onActiveWindowStackChanged += (action, hWnd) => value(action, hWnd, messenger);
        //        _messengerWaitingEventSubscribed = true;
        //    }
        //    remove
        //    {

        //    }
        //}
        #endregion

    }

    internal struct DetectMessenger
    {
        public static bool Skype = false;
        public static bool Telegram = false;
        public static bool WebSkype = false;
        public static bool WebWhatsApp = false;
        public static bool WebTelegram = false;
    }

    internal struct Control
    {
        public static bool LastUsed = false;
        public static bool MostNew = false;
        public static bool Order = false;
    }

    //internal enum ControlEnum
    //{
    //    LastUsed,
    //    MostNew,
    //    Order
    //}

    internal struct Bind
    {
        public static BindPair DefineBindPair(SwitchBy switchBy)
        {
            switch (switchBy)
            {
                case SwitchBy.Recent:
                    return LastUsed;
                case SwitchBy.Activity:
                    return MostNew;
                case SwitchBy.Queue:
                    return Order;
            }
            return null;
        }

        public delegate void BindEvent(SwitchBy switchBy, BindPair bindPair);

        public static event BindEvent KeyChanged;
        public static event BindEvent ModsChanged;

        private static BindPair _lastUsed = new BindPair(Keys.Z, new List<Gbc.KeyModifierStuck>() { Gbc.KeyModifierStuck.Alt });
        private static BindPair _mostNew = new BindPair(Keys.X, new List<Gbc.KeyModifierStuck>() { Gbc.KeyModifierStuck.Alt });
        private static BindPair _order = new BindPair(Keys.A, new List<Gbc.KeyModifierStuck>() { Gbc.KeyModifierStuck.Alt });

        public static BindPair LastUsed
        {
            get { return _lastUsed; }
            set
            {
                if (_mostNew.Key != value.Key)
                    KeyChanged(SwitchBy.Recent, value);
                if (_mostNew.Mods != value.Mods)
                    ModsChanged(SwitchBy.Recent, value);
                _lastUsed = value;
            }
        }

        public static BindPair MostNew
        {
            get { return _mostNew; }
            set
            {
                if (_mostNew.Key != value.Key)
                    KeyChanged(SwitchBy.Activity, value);
                if (_mostNew.Mods != value.Mods)
                    ModsChanged(SwitchBy.Activity, value);
                _mostNew = value;
            }
        }

        public static BindPair Order
        {
            get { return _order; }
            set
            {
                if (_mostNew.Key != value.Key)
                    KeyChanged(SwitchBy.Queue, value);
                if (_mostNew.Mods != value.Mods)
                    ModsChanged(SwitchBy.Queue, value);
                _order = value;
            }
        }
    }

    internal class BindPair
    {
        public BindPair(Keys key, List<Gbc.KeyModifierStuck> mods)
        {
            Key = key;
            Mods = mods;
        }

        public Keys Key;
        public List<Gbc.KeyModifierStuck> Mods;
    }
}
