using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using GlassServer.Events;

namespace GlassServer
{
    public class Program
    {
        public static X509Certificate2 Cert = new X509Certificate2("server.pfx", Constants.SSL_CERT_PASSWD);
        public static void Main(string[] args)
        {
            new GlassUI(args[0], Convert.ToInt32(args[1])).Start();
        }
    }   
}