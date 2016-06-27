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
using mmswitcherAPI;
using mmswitcherAPI.winmsg;

namespace test
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int _msgNotify;
        private int m = 0;
        WindowMessagesMonitor _wmm;
        

        public MainWindow()
        {
            InitializeComponent();
            _wmm.onMessageTraced += _wmm_onMessageTraced;
        }


        void _wmm_onMessageTraced(object sender, IntPtr hWnd, Interop.ShellEvents shell)
        {
            Console.WriteLine(m = m + 1);
        }

    }
}
