using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmswitcherAPI.Messangers
{
    internal partial class WebHookManager
    {
        public System.Diagnostics.Process Process { get { return _process; } }
        public InternetBrowser Browser { get { return _browser;} }

        private System.Diagnostics.Process _process;
        private InternetBrowser _browser;

        public WebHookManager(System.Diagnostics.Process process, InternetBrowser browser)
        {
            _process = process;
            _browser = browser;
        }

    }
}
