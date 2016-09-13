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

        public static MessengerController GetInstance(ActiveWindowStack aws)
        {
            if (aws == null)
                throw new ArgumentNullException("aws");

            if (Instance == null)
            {
                lock (_locker)
                {
                    if (Instance == null)
                        Instance = new MessengerController(aws);
                }
            }
            return Instance;
        }

        public void SubScribe(SwitchBy by, KeyValuePair<Keys, List<Gbc.KeyModifierStuck>> key)
        {
            switch (by)
            {
                case SwitchBy.Activity:
                    SwitchSubscribe(ref _switchByActGBC, key, Activity, ref Subsribed.Activity);
                    break;
                case SwitchBy.Recent:
                    SwitchSubscribe(ref _switchByRecGBC, key, Recent, ref Subsribed.Recent);
                    break;
                case SwitchBy.Queue:
                    SwitchSubscribe(ref _switchByQueGBC, key, Queue, ref Subsribed.Queue);
                    break;
            }

        }

        public void UnSubscribe(SwitchBy by)
        {
            switch (by)
            {
                case SwitchBy.Activity:
                    SwitchUnSubscribe(ref _switchByActGBC, ref Subsribed.Activity);
                    break;
                case SwitchBy.Recent:
                    SwitchUnSubscribe(ref _switchByRecGBC, ref Subsribed.Recent);
                    break;
                case SwitchBy.Queue:
                    SwitchUnSubscribe(ref _switchByQueGBC, ref Subsribed.Queue);
                    break;
            }
        }

        private void SwitchSubscribe(ref GlobalBindController gbc, KeyValuePair<Keys, List<Gbc.KeyModifierStuck>> key, Action action, ref bool subscribed)
        {
            if (subscribed)
                throw new InvalidOperationException("Subscribed allready");

            var list = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
            foreach(var keyModStuck in key.Value)
                list.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(keyModStuck, action));

            gbc = new Gbc(key.Key, Gbc.BindMethod.RegisterHotKey, Gbc.HookBehaviour.Replacement, list);
            gbc.Execute = true;
            subscribed = true;
        }

        private void SwitchUnSubscribe(ref GlobalBindController gbc, ref bool subscribed)
        {
            if (!subscribed)
                throw new InvalidOperationException("Unsubscribed allready");
            gbc.Dispose();
            subscribed = false;
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
            if (MessengerBase.Activity.First().IncomeMessages == 0)
            {
                var focusedMessenger = MessengerBase.Activity.FirstOrDefault((x) => x.Focused);
                if (focusedMessenger == null)
                {
                    MessengerBase.Activity.First().SetForeground();
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

            var focusedMessenger = MessengerBase.MessengersCollection.FirstOrDefault((x) => x.Focused);
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

        private static readonly object _locker = new object();
        private ActiveWindowStack _activeWindowStack;
        private Gbc _switchByActGBC;
        private Gbc _switchByRecGBC;
        private Gbc _switchByQueGBC;
        private DateTime _foregroundChangedTime = DateTime.Now;

        public static MessengerController Instance { get; private set; }

        //public KeyValuePair<Keys, Gbc.KeyModifierStuck> RecentBind { get; set; }
        //public KeyValuePair<Keys, Gbc.KeyModifierStuck> ActivityBind { get; set; }
        //public KeyValuePair<Keys, Gbc.KeyModifierStuck> QueueBind { get; set; }

        public Keys ActivityKey
        {
            get
            {
                if (!Subsribed.Activity) throw new InvalidOperationException("Should subscribe to switch by activity first");
                return _switchByActGBC.Key;
            }
            set
            {
                if (!Subsribed.Activity) throw new InvalidOperationException("Should subscribe to switch by activity first");
                _switchByActGBC.Key = value;
            }
        }

        public Keys RecentKey
        {
            get
            {
                if (!Subsribed.Recent) throw new InvalidOperationException("Should subscribe to switch by recent first");
                return _switchByRecGBC.Key;
            }
            set
            {
                if (!Subsribed.Recent) throw new InvalidOperationException("Should subscribe to switch by recent first");
                _switchByRecGBC.Key = value;
            }
        }

        public Keys QueueKey
        {
            get
            {
                if (!Subsribed.Queue) throw new InvalidOperationException("Should subscribe to switch by queue first");
                return _switchByQueGBC.Key;
            }
            set
            {
                if (!Subsribed.Queue) throw new InvalidOperationException("Should subscribe to switch by queue first");
                _switchByQueGBC.Key = value;
            }
        }

        public List<Gbc.KeyModifierStuck> ActivityModifiers
        {
            get
            {
                if (!Subsribed.Activity) throw new InvalidOperationException("Should subscribe to switch by activity first");
                return _switchByActGBC.Tasks.Select<KeyValuePair<Gbc.KeyModifierStuck, Action>, Gbc.KeyModifierStuck>((pair, keymod) => { return pair.Key; }).ToList();
            }
            set
            {
                if (!Subsribed.Activity) throw new InvalidOperationException("Should subscribe to switch by activity first");
                var kvp = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
                foreach (Gbc.KeyModifierStuck mod in value)
                    kvp.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(mod, Activity));
                _switchByActGBC.Tasks = kvp;

            }
        }

        public List<Gbc.KeyModifierStuck> RecentModifiers
        {
            get
            {
                if (!Subsribed.Recent) throw new InvalidOperationException("Should subscribe to switch by recent first");
                return _switchByRecGBC.Tasks.Select<KeyValuePair<Gbc.KeyModifierStuck, Action>, Gbc.KeyModifierStuck>((pair, keymod) => { return pair.Key; }).ToList();
            }
            set
            {
                if (!Subsribed.Recent) throw new InvalidOperationException("Should subscribe to switch by recent first");
                var kvp = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
                foreach (Gbc.KeyModifierStuck mod in value)
                    kvp.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(mod, Recent));
                _switchByRecGBC.Tasks = kvp;

            }
        }

        public List<Gbc.KeyModifierStuck> QueueModifiers
        {
            get
            {
                if (!Subsribed.Queue) throw new InvalidOperationException("Should subscribe to switch by queue first");
                return _switchByQueGBC.Tasks.Select<KeyValuePair<Gbc.KeyModifierStuck, Action>, Gbc.KeyModifierStuck>((pair, keymod) => { return pair.Key; }).ToList();
            }
            set
            {
                if (!Subsribed.Queue) throw new InvalidOperationException("Should subscribe to switch by queue first");
                var kvp = new List<KeyValuePair<Gbc.KeyModifierStuck, Action>>();
                foreach (Gbc.KeyModifierStuck mod in value)
                    kvp.Add(new KeyValuePair<Gbc.KeyModifierStuck, Action>(mod, Queue));
                _switchByQueGBC.Tasks = kvp;

            }
        }

        private struct Subsribed
        {
            public static bool Activity = false;
            public static bool Recent = false;
            public static bool Queue = false;
        }

        //private enum SwitchTo
        //{
        //    Tab = 0,
        //    Window = 1
        //}
    }

    public enum SwitchBy
    {
        Activity,
        Recent,
        Queue
    }
}
