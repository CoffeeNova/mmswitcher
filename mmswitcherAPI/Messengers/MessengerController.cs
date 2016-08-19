using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmswitcherAPI.AltTabSimulator;
using mmswitcherAPI.Messengers.Web;
using mmswitcherAPI.Messengers.Desktop;
using System.Windows.Forms;
using System.Threading;

namespace mmswitcherAPI.Messengers
{
    using Gbc = GlobalBindController;

    public sealed class MessengerController
    {

        private MessengerController(ActiveWindowStack aws)
        {
            _activeWindowStack = aws;
            _activeWindowStack.onActiveWindowStackChanged += _activeWindowStack_onActiveWindowStackChanged;
        }

        void _activeWindowStack_onActiveWindowStackChanged(StackAction action, IntPtr hWnd)
        {
            if (action != StackAction.MovedToFore)
                return;
            _foregroundChangedTime = DateTime.Now;
        }

        public static MessengerController Instance(ActiveWindowStack aws)
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new MessengerController(aws);
                }
            }
            return _instance;
        }

        public void SubScribe(SwitchBy by, KeyValuePair<Keys, Gbc.KeyModifierStuck> key)
        {
            switch (by)
            {
                case SwitchBy.Activity:
                    SwitchSubscribeA(key);
                    break;
                case SwitchBy.Recent:
                    SwitchSubscribeR(key);
                    break;
                case SwitchBy.Queue:
                    SwitchSubscribeQ(key);
                    break;
            }

        }

        public void UnSubscribe(SwitchBy by)
        {
            switch (by)
            {
                case SwitchBy.Activity:
                    _switchByActGBC.Dispose();
                    Subsribed.Activity = false;
                    break;
                case SwitchBy.Recent:
                    _switchByRecGBC.Dispose();
                    Subsribed.Recent = false;
                    break;
                case SwitchBy.Queue:
                    _switchByQueGBC.Dispose();
                    Subsribed.Queue = false;
                    break;
            }
        }

        private void SwitchSubscribeA(KeyValuePair<Keys, Gbc.KeyModifierStuck> key)
        {
            if (Subsribed.Activity)
                throw new InvalidOperationException("Subscribed allready");
            ActivityBind = key;
            Action bindAction = Activity;
            var list = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
            list.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(key.Value, bindAction));
            _switchByActGBC = new Gbc(key.Key, Gbc.BindMethod.RegisterHotKey, Gbc.HookBehaviour.Replacement, list);
            _switchByActGBC.Execute = true;
            Subsribed.Activity = true;
        }

        private void SwitchSubscribeR(KeyValuePair<Keys, Gbc.KeyModifierStuck> key)
        {
            if (Subsribed.Recent)
                throw new InvalidOperationException("Subscribed allready");
            RecentBind = key;
            Action bindAction = Recent;
            var list = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
            list.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(key.Value, bindAction));
            _switchByRecGBC = new Gbc(key.Key, Gbc.BindMethod.RegisterHotKey, Gbc.HookBehaviour.Replacement, list);
            _switchByRecGBC.Execute = true;
            Subsribed.Recent = true;
        }

        private void SwitchSubscribeQ(KeyValuePair<Keys, Gbc.KeyModifierStuck> key)
        {
            if (Subsribed.Queue)
                throw new InvalidOperationException("Subscribed allready");
            QueueBind = key;
            Action bindAction = Queue;
            var list = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
            list.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(key.Value, bindAction));
            _switchByQueGBC = new Gbc(key.Key, Gbc.BindMethod.RegisterHotKey, Gbc.HookBehaviour.Replacement, list);
            _switchByQueGBC.Execute = true;
            Subsribed.Queue = true;
        }

        private void Recent()
        {
            if (_activeWindowStack.Suspended)
                throw new InvalidOperationException("Cannot do method MessengerController.Recent() properly while _activeWindowStack.Suspended is true.");
            if (MessengerBase.MessengersCollection.Count == 0)
                return;

            var recentMessenger = MessengerBase.LastActive;
            if (recentMessenger.Focused)
                ReturnPreviousForeground(recentMessenger);
            else
                MessengerBase.LastActive.SetForeground();
        }

        private void Activity()
        {
            var messengersCount = MessengerBase.MessengersCollection.Count;
            if (messengersCount == 0)
                return;
            if (MessengerBase.Activity.First().NewMessagesCount == 0)
            {
                var focusedMessenger = MessengerBase.Activity.First((x) => x.Focused);
                if (focusedMessenger == null)
                {
                    Recent();
                    return;
                }
                var nextMessengerIndex = MessengerBase.Activity.IndexOf(focusedMessenger) + 1;
                if (nextMessengerIndex < messengersCount)
                    MessengerBase.Activity[nextMessengerIndex].SetForeground();
                else
                    MessengerBase.Activity.First().SetForeground();
            }
            else
                MessengerBase.Activity.First().SetForeground();
        }

        private void Queue()
        {
            var messengersCount = MessengerBase.MessengersCollection.Count;
            if (messengersCount == 0)
                return;

            var focusedMessenger = MessengerBase.MessengersCollection.First((x) => x.Focused);
            if (focusedMessenger == null)
            {
                Recent();
                return;
            }
            var nextMessengerIndex = MessengerBase.MessengersCollection.IndexOf(focusedMessenger) + 1;
            if (nextMessengerIndex < messengersCount)
                MessengerBase.MessengersCollection[nextMessengerIndex].SetForeground();
            else
                MessengerBase.MessengersCollection.First().SetForeground();
        }

        private void ReturnPreviousForeground(MessengerBase messenger)
        {
            if (messenger is WebMessenger)
            {
                var webMess = messenger as WebMessenger;
                if (webMess.TabSelectedTime > _foregroundChangedTime)
                     webMess.ReturnPreviousSelectedTab();
                else
                    ReturnPreviousWindow();
            }
            else
                ReturnPreviousWindow();
        }

        private void ReturnPreviousWindow()
        {
            if (_activeWindowStack.WindowStack.Count > 1)
                WinApi.SetForegroundWindow(_activeWindowStack.WindowStack[1]);
        }

        private static MessengerController _instance;
        private static readonly object _locker = new object();
        private ActiveWindowStack _activeWindowStack;
        private Gbc _switchByActGBC;
        private Gbc _switchByRecGBC;
        private Gbc _switchByQueGBC;
        private DateTime _foregroundChangedTime = DateTime.Now;

        public KeyValuePair<Keys, Gbc.KeyModifierStuck> RecentBind { get; set; }
        public KeyValuePair<Keys, Gbc.KeyModifierStuck> ActivityBind { get; set; }
        public KeyValuePair<Keys, Gbc.KeyModifierStuck> QueueBind { get; set; }

        private struct Subsribed
        {
            public static bool Activity = false;
            public static bool Recent = false;
            public static bool Queue = false;
        }

        public enum SwitchBy
        {
            Activity,
            Recent,
            Queue
        }

        //private enum SwitchTo
        //{
        //    Tab = 0,
        //    Window = 1
        //}
    }
}
