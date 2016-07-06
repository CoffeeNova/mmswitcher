using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace mmswitcherAPI.Messangers.Web.Browsers
{
    public interface IBrowser
    {
        AutomationElement SkypeTab(IntPtr hWnd);
        AutomationElement WhatsUpTab(IntPtr hWnd);
    }
}
