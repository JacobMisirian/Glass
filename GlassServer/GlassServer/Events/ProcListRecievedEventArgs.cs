using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassServer.Events
{
    public class ProcListRecievedEventArgs : EventArgs
    {
        public string ProcList { get; set; }
    }
}
