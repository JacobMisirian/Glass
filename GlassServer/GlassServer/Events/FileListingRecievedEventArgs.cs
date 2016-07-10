using System;

namespace GlassServer
{
    public class FileListingRecievedEventArgs : EventArgs
    {
        public Client Client { get; set; }
        public string[] Files { get; set; }
    }
}

