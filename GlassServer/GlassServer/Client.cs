using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GlassServer
{
    public class Client
    {
        public TcpClient TcpClient { get; private set; }
        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }

        public string UserName { get; set; }
        public string IP { get; set; }
        public string CurrentDirectory { get; set; }
        public string Identity { get { return string.Format("{0}{1}", UserName, IP); } }

        public Client(TcpClient client)
        {
            TcpClient = client;
            Reader = new BinaryReader(client.GetStream());
            Writer = new BinaryWriter(client.GetStream());
        }

        public byte ReadByte()
        {
            return Reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return Reader.ReadBytes(count);
        }

        public int ReadInt()
        {
            return Reader.ReadInt32();
        }

        public long ReadLong()
        {
            return Reader.ReadInt64();
        }

        public string ReadString()
        {
            return Reader.ReadString();
        }

        public void WriteLine(byte[] data)
        {
            Writer.Write(data);
            Writer.Flush();
        }
        public void WriteLine(GlassProtocol proto)
        {
            Writer.Write((byte)proto);
            Writer.Flush();
        }
        public void WriteLine(string text)
        {
            Writer.Write(text);
            Writer.Flush();
        }
    }
}

