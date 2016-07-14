﻿using System;
using System.Collections.Generic;
using System.IO;

using GlassServer.Events;

namespace GlassServer
{
    public class GlassUI
    {
        public GlassListener GlassListener { get; private set; }
        public GlassClientManager GlassClientManager { get; private set; }

        public Dictionary<int, Client> IdentifiedClients = new Dictionary<int, Client>();

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
            int selectedClientID = 0;
            while (true)
            {
                string command = Console.ReadLine();
                string[] parts = command.Split(' ');
                string remainder = null;
                if (parts.Length >= 2)
                    remainder = command.Substring(command.IndexOf(" ") + 1);
                switch (parts[0].ToLower())
                {
                    case "select":
                        try
                        {
                            int id = Convert.ToInt32(parts[1]);
                            selectedClient = IdentifiedClients[id];
                            selectedClientID = id;
                        }
                        catch
                        {
                            Console.WriteLine("Was not a number!");
                        }
                        break;
                    case "selected":
                        Console.WriteLine("{0}:\t{1} {2}", selectedClientID, selectedClient.UserName, selectedClient.IP);
                        break;
                    case "info":
                        var cl = IdentifiedClients[Convert.ToInt32(parts[1])];
                        Console.WriteLine("{0}:\t{1} {2}", parts[1], cl.UserName, cl.IP);
                        break;
                    case "list":
                        foreach (var pair in IdentifiedClients)
                            Console.WriteLine("{0}:\t{1} {2}", pair.Key, pair.Value.UserName, pair.Value.IP);
                        break;
                    case "pwd":
                        selectedClient.WriteLine(GlassProtocol.RequestCurrentDirectory);
                        break;
                    case "ls":
                        selectedClient.WriteLine(GlassProtocol.RequestDirectoryListing);
                        selectedClient.WriteLine(parts.Length < 2 ? selectedClient.CurrentDirectory : remainder);
                        selectedClient.WriteLine(GlassProtocol.RequestFileListing);
                        selectedClient.WriteLine(parts.Length < 2 ? selectedClient.CurrentDirectory : remainder);
                        break;
                    case "dl":
                        selectedClient.WriteLine(GlassProtocol.RequestFileDownload);
                        selectedClient.WriteLine(parts[1]);
                        selectedClient.WriteLine(parts[2]);
                        break;
                    case "cd":
                        selectedClient.WriteLine(GlassProtocol.SetCurrentDirectory);
                        selectedClient.WriteLine(remainder);
                        selectedClient.CurrentDirectory = remainder;
                        break;
                    case "get":
                        selectedClient.WriteLine(GlassProtocol.RequestFile);
                        selectedClient.WriteLine(remainder);
                        selectedClient.FileJobs.Push(remainder);
                        break;
                    case "put":
                        selectedClient.WriteLine(GlassProtocol.SendingFile);
                        selectedClient.WriteLine(parts[2]);
                        selectedClient.WriteLine(new FileInfo(parts[1]).Length);
                        BinaryReader file = new BinaryReader(new StreamReader(parts[1]).BaseStream);
                        while (file.BaseStream.Position < file.BaseStream.Length)
                            selectedClient.Writer.Write(file.ReadByte());
                        file.Close();
                        selectedClient.Writer.Flush();
                        break;
                    case "rm":
                        selectedClient.WriteLine(GlassProtocol.RequestDeleteFile);
                        selectedClient.WriteLine(remainder);
                        break;
                    case "rmf":
                        selectedClient.WriteLine(GlassProtocol.RequestDeleteDir);
                        selectedClient.WriteLine(remainder);
                        break;
                    case "msg":
                        selectedClient.WriteLine(GlassProtocol.RequestMessageDisplay);
                        selectedClient.WriteLine(remainder);
                        break;
                }
            }
        }

        private void listener_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            GlassClientManager.RegisterClient(e.Client);
            e.Client.WriteLine(GlassProtocol.RequestCurrentDirectory);
        }

        private int id = 0;
        private void manager_IdentifyRecieved(object sender, IdentifyRecievedEventArgs e)
        {
            Console.WriteLine("IDENTIFY {0} from {1}", e.Client.UserName, e.Client.IP);
            Console.WriteLine("Identity is {0}.", e.Client.Identity = ++id);
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
            Console.WriteLine("File recieved {0} size {1} bytes.", e.Client.FileJobs.Count > 0 ? e.Client.FileJobs.Peek() : "unknown", e.Length);
            string path = e.Client.FileJobs.Pop();
            BinaryWriter writer = new BinaryWriter(new StreamWriter(path).BaseStream);
            for (int i = 0; i < e.Length; i++)
                writer.Write(e.BinaryReader.ReadByte());
            writer.Close();
            Console.WriteLine("File saved!");
            GlassClientManager.ReadInput = true;
        }
    }
}

