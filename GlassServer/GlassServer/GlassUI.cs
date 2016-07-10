using System;
using System.Collections.Generic;
using System.IO;

using GlassServer.Events;

namespace GlassServer
{
    public class GlassUI
    {
        public GlassListener GlassListener { get; private set; }
        public GlassClientManager GlassClientManager { get; private set; }

        public Dictionary<string, Client> IdentifiedClients = new Dictionary<string, Client>();

        public GlassUI(string ip, int port)
        {
            GlassListener = new GlassListener(ip, port);
            GlassClientManager = new GlassClientManager();

            GlassListener.ClientConnected += listener_ClientConnected;

            GlassClientManager.CurrentDirectoryRecieved += manager_DirectoryRecieved;
            GlassClientManager.DirectoryListingRecieved += manager_DirectoryListingRecieved;
            GlassClientManager.FileListingRecieved += manager_FileListingRecieved;
            GlassClientManager.FileRecieved += manager_FileRecieved;
            GlassClientManager.IdentifyRecieved += manager_IdentifyRecieved;
        }

        public void Start()
        {
            GlassListener.ListenForConnections();
            Client selectedClient = null;
            while (true)
            {
                string command = Console.ReadLine();
                string[] parts = command.Split(' ');
                switch (parts[0].ToLower())
                {
                    case "select":
                        selectedClient = IdentifiedClients[parts[1]];
                        break;
                    case "pwd":
                        selectedClient.WriteLine(GlassProtocol.RequestCurrentDirectory);
                        break;
                    case "ls":
                        selectedClient.WriteLine(GlassProtocol.RequestDirectoryListing);
                        selectedClient.WriteLine(parts.Length < 2 ? selectedClient.CurrentDirectory : parts[1]);
                        selectedClient.WriteLine(GlassProtocol.RequestFileListing);
                        selectedClient.WriteLine(parts.Length < 2 ? selectedClient.CurrentDirectory : parts[1]);
                        break;
                    case "dl":
                        selectedClient.WriteLine(GlassProtocol.RequestFileDownload);
                        selectedClient.WriteLine(parts[1]);
                        selectedClient.WriteLine(parts[2]);
                        break;
                    case "get":
                        selectedClient.WriteLine(GlassProtocol.RequestFile);
                        selectedClient.WriteLine(parts[1]);
                        break;
                }
            }
        }

        private void listener_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            GlassClientManager.RegisterClient(e.Client);
            e.Client.WriteLine(GlassProtocol.RequestCurrentDirectory);
        }

        private void manager_IdentifyRecieved(object sender, IdentifyRecievedEventArgs e)
        {
            Console.WriteLine("IDENTIFY {0} from {1}", e.Client.UserName, e.Client.IP);
            Console.WriteLine("Identity is {0}.", e.Client.Identity);
            if (IdentifiedClients.ContainsKey(e.Client.Identity))
                IdentifiedClients.Remove(e.Client.Identity);
            IdentifiedClients.Add(e.Client.Identity, e.Client);
        }
        private void manager_DirectoryRecieved(object sender, CurrentDirectoryRecievedEventArgs e)
        {
            Console.WriteLine(e.Client.CurrentDirectory);
        }
        private void manager_DirectoryListingRecieved(object sender, DirectoryListingRecievedEventArgs e)
        {
            foreach (string dir in e.Directories)
                Console.Write("{0}\t", dir);
            Console.WriteLine();
        }
        private void manager_FileListingRecieved(object sender, FileListingRecievedEventArgs e)
        {
            foreach (string file in e.Files)
                Console.Write("{0}\t", file);
            Console.WriteLine();
        }
        private void manager_FileRecieved(object sender, FileRecievedEventArgs e)
        {
            Console.Write("File recieved {0} bytes. Enter path for saving: ", e.Length);
            string path = Console.ReadLine();
            BinaryWriter writer = new BinaryWriter(new StreamWriter(path).BaseStream);
            for (int i = 0; i < e.Length; i++)
                writer.Write(e.BinaryReader.ReadByte());
            writer.Close();
            Console.WriteLine("File saved!");
            GlassClientManager.ReadInput = true;
        }
    }
}

