using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
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
        public int Identity { get; set; }

        public Stack<string> FileJobs { get; private set; }

        public Client(TcpClient client)
        {
            TcpClient = client;
            SslStream ssl = new SslStream(client.GetStream(), false);
            ssl.AuthenticateAsServer(Program.Cert, false, System.Security.Authentication.SslProtocols.Tls, true);
            Reader = new BinaryReader(ssl);
            Writer = new BinaryWriter(ssl);
            FileJobs = new Stack<string>();
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

        public void WriteLine(byte b)
        {
            Writer.Write(b);
            Writer.Flush();
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
        public void WriteLine(long num)
        {
            Writer.Write(num);
            Writer.Flush();
        }
        public void WriteLine(string text)
        {
            Writer.Write(text);
            Writer.Flush();
        }
    }
}

