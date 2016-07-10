using System;

namespace GlassServer.Events
{
    public class CurrentDirectoryRecievedEventArgs : EventArgs
    {
        public Client Client { get; set; }
    }
}

