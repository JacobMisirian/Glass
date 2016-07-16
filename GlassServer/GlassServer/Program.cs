using System;
using System.IO;

using GlassServer.Events;

namespace GlassServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new GlassUI(args[0], Convert.ToInt32(args[1])).Start();
        }
    }   
}