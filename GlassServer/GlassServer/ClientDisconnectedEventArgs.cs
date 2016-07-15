using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassServer
{
    public class ClientDisconnectedEventArgs : EventArgs
    {
        public Client Client { get; set; }
    }
}
