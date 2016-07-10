using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GlassServer
{
    public class GlassListener
    {
        public TcpListener TcpListener { get; private set; }

        public GlassListener(string ip, int port)
        {
            TcpListener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        protected virtual void OnClientConnected(ClientConnectedEventArgs e)
        {
            var handler = ClientConnected;
            if (handler != null)
                handler(this, e);
        }

        public void ListenForConnections()
        {
            TcpListener.Start();
            new Thread(() => listenForConnections()).Start();
        }

        private void listenForConnections()
        {
            while (true)
            {
                var client = new Client(TcpListener.AcceptTcpClient());
                OnClientConnected(new ClientConnectedEventArgs { Client = client });
            }
        }
    }
}

