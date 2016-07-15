using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassServer.Events
{
    public class StdoutRecievedEventArgs : EventArgs
    {
        public string Line { get; set; }
    }
}
