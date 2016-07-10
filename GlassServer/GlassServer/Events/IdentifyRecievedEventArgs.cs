using System;

namespace GlassServer.Events
{
    public class IdentifyRecievedEventArgs : EventArgs
    {
        public Client Client { get; set; }
    }
}

