using System;
using System.Collections.Generic;
using System.Threading;

using GlassServer.Events;

namespace GlassServer
{
    public class GlassClientManager
    {
        public List<Client> Clients { get; private set; }
        public bool ReadInput { get; set; }

        public GlassClientManager()
        {
            Clients = new List<Client>();
            ReadInput = true;
        }

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        protected virtual void OnClientDisconnected(ClientDisconnectedEventArgs e)
        {
            var handler = ClientDisconnected;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<CurrentDirectoryRecievedEventArgs> CurrentDirectoryRecieved;
        protected virtual void OnCurrentDirectoryRecieved(CurrentDirectoryRecievedEventArgs e)
        {
            var handler = CurrentDirectoryRecieved;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<IdentifyRecievedEventArgs> IdentifyRecieved;
        protected virtual void OnIdentifyRecieved(IdentifyRecievedEventArgs e)
        {
            var handler = IdentifyRecieved;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<DirectoryListingRecievedEventArgs> DirectoryListingRecieved;
        protected virtual void OnDirectoryListingRecieved(DirectoryListingRecievedEventArgs e)
        {
            var handler = DirectoryListingRecieved;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<FileRecievedEventArgs> FileRecieved;
        protected virtual void OnFileRecieved(FileRecievedEventArgs e)
        {
            var handler = FileRecieved;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<FileListingRecievedEventArgs> FileListingRecieved;
        protected virtual void OnFileListingRecieved(FileListingRecievedEventArgs e)
        {
            var handler = FileListingRecieved;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<StdoutRecievedEventArgs> StdoutRecieved;
        protected virtual void OnStdoutReecieved(StdoutRecievedEventArgs e)
        {
            var handler = StdoutRecieved;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<ProcListRecievedEventArgs> ProcListRecieved;
        protected virtual void OnProcListRecieved(ProcListRecievedEventArgs e)
        {
            var handler = ProcListRecieved;
            if (handler != null)
                handler(this, e);
        }

        public void RegisterClient(Client client)
        {
            Clients.Add(client);
            new Thread(() => protocolListener(client)).Start();
        }

        private void protocolListener(Client client)
        {
            while (true)
            {
                try
                {
                    if (!ReadInput)
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                    int len;
                    byte b = client.ReadByte();
                    switch (b)
                    {
                        case (byte)GlassProtocol.Identify:
                            client.UserName = client.ReadString();
                            client.IP = client.ReadString();
                            OnIdentifyRecieved(new IdentifyRecievedEventArgs { Client = client });
                            break;
                        case (byte)GlassProtocol.SendingCurrentDirectory:
                            client.CurrentDirectory = client.ReadString();
                            OnCurrentDirectoryRecieved(new CurrentDirectoryRecievedEventArgs { Client = client });
                            break;
                        case (byte)GlassProtocol.SendingDirectoryListing:
                            len = client.ReadInt();
                            string[] dirs = new string[len];
                            for (int i = 0; i < len; i++)
                                dirs[i] = client.ReadString();
                            OnDirectoryListingRecieved(new DirectoryListingRecievedEventArgs { Client = client, Directories = dirs });
                            break;
                        case (byte)GlassProtocol.SendingError:
                            Console.WriteLine("ERROR! {0}", client.ReadString());
                            break;
                        case (byte)GlassProtocol.SendingFile:
                            ReadInput = false;
                            OnFileRecieved(new FileRecievedEventArgs { Client = client, Length = client.ReadLong(), BinaryReader = client.Reader });
                            break;
                        case (byte)GlassProtocol.SendingFileListing:
                            len = client.ReadInt();
                            string[] files = new string[len];
                            for (int i = 0; i < len; i++)
                                files[i] = client.ReadString();
                            OnFileListingRecieved(new FileListingRecievedEventArgs { Client = client, Files = files });
                            break;
                        case (byte)GlassProtocol.SendingProgramStdout:
                            OnStdoutReecieved(new StdoutRecievedEventArgs { Line = client.ReadString() });
                            break;
                        case (byte)GlassProtocol.SendingProcList:
                            OnProcListRecieved(new ProcListRecievedEventArgs { ProcList = client.ReadString() });
                            break;
                    }
                }
                catch (System.IO.IOException)
                {
                    OnClientDisconnected(new ClientDisconnectedEventArgs { Client = client });
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Local error: {0}", ex.Message);
                }
            }
        }
    }
}

