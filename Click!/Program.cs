using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using NLog;

namespace Click_
{
    static class Program
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            _log.Debug("START DEBUGGING!");
#endif
            _log.Info(string.Format("{0} starting.", Constants._APPLICATION_NAME));
            Process curProc;
            Process[] proc;
            proc = Process.GetProcesses();
            curProc = Process.GetCurrentProcess();

            foreach (Process pr in proc)
            {
                if (pr.ProcessName == curProc.ProcessName && pr.Id != curProc.Id)
                {
                    _log.Info(string.Format("Attempt to start another instance of the application. {0} closing.", Constants._APPLICATION_NAME));
                    pr.Kill();
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Click());
        }
    }
}
