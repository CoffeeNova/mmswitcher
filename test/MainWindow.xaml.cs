﻿using System;
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
using mmswitcherAPI.AltTabSimulator;

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
        ActiveWindowStack aws;


        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _wmm = new WindowMessagesMonitor();
            _wmm.onMessageTraced += _wmm_onMessageTraced;
            aws = ActiveWindowStack.Instance(this);
            aws.onActiveWindowStackChanged += aws_onActiveWindowStackChanged;
            aws.Start();

        }

        void aws_onActiveWindowStackChanged(StackAction action, IntPtr hWnd)
        {
            //int length = Interop.GetWindowTextLength(hWnd);
            //StringBuilder builder = new StringBuilder(length);
            //Interop.GetWindowText(hWnd, builder, length + 1);
            //Console.WriteLine(builder.ToString() + " " + action.ToString());
            Console.WriteLine("----------------------------------------------");
            foreach (IntPtr handle in aws.WindowStack)
            {
                int len = Interop.GetWindowTextLength(handle);
                StringBuilder buid = new StringBuilder(len);
                Interop.GetWindowText(handle, buid, len + 1);
                Console.WriteLine(buid.ToString());
            }
        }


        void _wmm_onMessageTraced(object sender, IntPtr hWnd, Interop.ShellEvents shell)
        {
            //Console.WriteLine(m = m + 1);
        }

    }
}
