using System;

namespace GlassServer
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public Client Client { get; set; }
    }
}

