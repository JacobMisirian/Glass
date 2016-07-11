using System;
using System.Collections.Generic;
using System.Linq;
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
        static void Main()
        {
            while (true)
            {
                GlassConnection connection = new GlassConnection();
                while (!connection.Connect("127.0.0.1", 1337)) Thread.Sleep(5000);
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
