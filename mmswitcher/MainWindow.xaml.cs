using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using mmswitcherAPI;

namespace mmswitcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int _msgNotify;
        private MsgMonitor _msgMon;
        private WindowConditionMonitor wcm;
        private int m = 0;

        public MainWindow()
        {
            InitializeComponent();
            
            
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            wcm = new WindowConditionMonitor(this);
            wcm.onMessageTraced += wcm_onMessageTraced;
        }

        void wcm_onMessageTraced(object sender, IntPtr hWnd, Interop.ShellEvents shell)
        {
            Console.WriteLine(m = m+1);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return IntPtr.Zero;

        }

        
    }


}
