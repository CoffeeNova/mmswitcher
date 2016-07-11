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
        AutomationElement BrowserWindowAutomationElement(IntPtr hWnd);
        AutomationElement SkypeTab(IntPtr hWnd);
        AutomationElement WhatsAppTab(IntPtr hWnd);
        AutomationElement SkypeFocusAutomationElement(IntPtr hWnd);
    }
}
