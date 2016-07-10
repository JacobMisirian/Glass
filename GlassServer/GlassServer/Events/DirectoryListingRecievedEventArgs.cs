using System;

namespace GlassServer.Events
{
    public class DirectoryListingRecievedEventArgs : EventArgs
    {
        public Client Client { get; set; }
        public string[] Directories { get; set; }
    }
}

