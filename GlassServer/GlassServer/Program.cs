﻿using System;
using System.IO;

using GlassServer.Events;

namespace GlassServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new GlassUI("127.0.0.1", 1337).Start();
        }
    }
}

