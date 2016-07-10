using System;
using System.IO;

namespace GlassServer.Events
{
    public class FileRecievedEventArgs : EventArgs
    {
        public Client Client { get; set; }
        public long Length { get; set; }
        public BinaryReader BinaryReader { get; set; }
    }
}

