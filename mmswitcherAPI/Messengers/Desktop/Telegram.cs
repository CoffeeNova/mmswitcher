using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmswitcherAPI.Messengers;
using System.Diagnostics;

namespace mmswitcherAPI.Messengers.Desktop
{
    public class Telegram : MessengerBase, IDisposable
    {
        public Telegram(Process process) : base(process)
        {

        }
    }
}
