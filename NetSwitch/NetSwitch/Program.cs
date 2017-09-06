using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

//fgrando@outlook.com
// thanks to:
// http://wutils.com/wmi/root/cimv2/win32_networkadapter/cs-samples.html
// https://msdn.microsoft.com/en-us/library/aa394216(v=vs.85).aspx
// https://stackoverflow.com/questions/70272/single-form-hide-on-startup
// https://stackoverflow.com/questions/2818179/how-do-i-force-my-net-application-to-run-as-administrator
// and google that made all of this possible :)


namespace NetSwitch
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
