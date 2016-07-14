using System;
using System.IO;

using GlassServer.Events;

namespace GlassServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new GlassUI("192.168.1.31", 1337).Start();
        }
    }
}

