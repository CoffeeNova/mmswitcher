using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmswitcherAPI.AltTabSimulator;
using System.Windows.Forms;

namespace mmswitcherAPI.Messengers
{
    using Gbc = GlobalBindController;

    public sealed class MessengerController
    {
        private static MessengerController _instance;
        private static readonly object _locker = new object();

        private ActiveWindowStack _activeWindowStack;
        private KeyValuePair<Keys, Gbc.KeyModifierStuck> _switchByRecentBind;
        private KeyValuePair<Keys, Gbc.KeyModifierStuck> _switchByActivityBind;
        private Gbc controller;

        private MessengerController(ActiveWindowStack aws,
                                                   KeyValuePair<Keys, Gbc.KeyModifierStuck> switchByRecentBind,
                                                   KeyValuePair<Keys, Gbc.KeyModifierStuck> switchByActivityBind) 
        {
            _activeWindowStack = aws;
            _switchByRecentBind = switchByRecentBind;
            _switchByActivityBind = switchByActivityBind;
        }

        public static MessengerController Instance(ActiveWindowStack aws,
                                                   KeyValuePair<Keys, Gbc.KeyModifierStuck> switchByRecentBind,
                                                   KeyValuePair<Keys, Gbc.KeyModifierStuck> switchByActivityBind)
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new MessengerController(aws, switchByRecentBind, switchByActivityBind);
                }
            }
            return _instance;
        }

    }
}
