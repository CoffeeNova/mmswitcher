using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using mmswitcherAPI;
using mmswitcherAPI.Messengers;

namespace Click_
{
    using Gbc = GlobalBindController;

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

    internal struct MessengerControllerBinds
    {
        static MessengerControllerBinds()
        {
            KeyChanged += MessengerControllerBinds_KeyChanged;
            ModsChanged += MessengerControllerBinds_KeyChanged;
            LastUsed.PropertyChanged += (sender, e) => PropertyChanged(sender, e, SwitchBy.Recent);
            MostNew.PropertyChanged += (sender, e) => PropertyChanged(sender, e, SwitchBy.Activity);
            Order.PropertyChanged += (sender, e) => PropertyChanged(sender, e, SwitchBy.Queue);
        }

        static void MessengerControllerBinds_KeyChanged(SwitchBy switchBy, BindPair bindPair)
        {
        }

        static void PropertyChanged(object sender, PropertyChangedEventArgs e, SwitchBy switchBy)
        {
            if (e.PropertyName == "Key")
                KeyChanged(switchBy, sender as BindPair);
            if (e.PropertyName == "Mods")
                ModsChanged(switchBy, sender as BindPair);
        }

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

        public static BindPair _lastUsed = new BindPair(Keys.Z, new List<Gbc.KeyModifierStuck>() { Gbc.KeyModifierStuck.Alt });
        public static BindPair _mostNew = new BindPair(Keys.X, new List<Gbc.KeyModifierStuck>() { Gbc.KeyModifierStuck.Alt });
        public static BindPair _order = new BindPair(Keys.A, new List<Gbc.KeyModifierStuck>() { Gbc.KeyModifierStuck.Alt });

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

    internal class Bind
    {
        public Bind(Keys key, List<Gbc.KeyModifierStuck> modifiers)
        {
            _update = new BindPair(key, modifiers);
            KeyChanged += MessengerControllerBinds_KeyChanged;
            ModsChanged += MessengerControllerBinds_KeyChanged;
            BindPair.PropertyChanged += BindPair_PropertyChanged;
        }

        private static void MessengerControllerBinds_KeyChanged(BindPair bindPair)
        {
        }

        private void BindPair_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Key")
                KeyChanged(sender as BindPair);
            if (e.PropertyName == "Mods")
                ModsChanged(sender as BindPair);
        }

        public delegate void UpdateEvent(BindPair bindPair);

        public event UpdateEvent KeyChanged;
        public event UpdateEvent ModsChanged;

        private BindPair _update;

        public BindPair BindPair
        {
            get { return _update; }
            set
            {
                if (_update.Key != value.Key)
                    KeyChanged(value);
                if (_update.Mods != value.Mods)
                    ModsChanged(value);
                _update = value;
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

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private Keys _key;
        private List<Gbc.KeyModifierStuck> _mods;

        public Keys Key
        {
            get { return _key; }
            set
            {
                if (_key == value)
                    return;
                _key = value;
                OnPropertyChanged("Key");
            }
        }
        public List<Gbc.KeyModifierStuck> Mods
        {
            get { return _mods; }
            set
            {
                if (_mods == value)
                    return;
                _mods = value;
                OnPropertyChanged("Mods");
            }
        }
    }
}
