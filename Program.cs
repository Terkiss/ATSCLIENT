using System;
using System.CodeDom;
using System.Threading;
using System.Windows.Forms;
using MapleATS.Windows;

using MapleATS.Util;
using MapleATS.CLI;

namespace ATS
{
    internal static class Program
    {



        private static MainForm mainForm;
        private static Thread uiThread;
        private static Thread workThread;


        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            mainForm = new MainForm();

            AppMemory.Instance.MainForm = mainForm;

            uiThread = new Thread(() =>
            {
                Application.Run(mainForm);
            });

            workThread = new Thread(() =>
            {
                ATS_CLI.Start();
            });




            // Windows Forms는 반드시 STA(Single-Threaded Apartment) 모드 스레드에서 생성되어야 합니다.
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            workThread.Start();

            // CLI 초기화가 `ATS_CLI.Start()`에서 수행됩니다.
            workThread.Join();
            uiThread.Join();
        }
    }
}
