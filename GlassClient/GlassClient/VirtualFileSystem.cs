using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GlassClient
{
    public class VirtualFileSystem
    {
        public const long BEGIN_DATA_POSITION = 500;

        public string RealLocation { get; private set; }
        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }

        public long Position { get { return Reader.BaseStream.Position; } set { Reader.BaseStream.Position = value; } }

        public long DataPosition { get; private set; }
        public long FileCount { get; private set; }

        public List<VirtualFile> Files { get; private set; }

        public VirtualFileSystem(string location)
        {
            RealLocation = location;
            var stream = new FileStream(location, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            Reader = new BinaryReader(stream);
            Writer = new BinaryWriter(stream);
            Files = new List<VirtualFile>();

            if (Reader.BaseStream.Length > 0)
            {
                Reader.ReadByte();
                FileCount = Reader.ReadInt64();
                for (int i = 0; i < FileCount; i++)
                    Files.Add(new VirtualFile(Reader.ReadString(), Reader.ReadInt64(), Reader.ReadInt64()));
                DataPosition = FileCount > 0 ? Files[Files.Count - 1].EndPosition + 1 : BEGIN_DATA_POSITION;
            }
            else
            {
                FileCount = 0;
                DataPosition = BEGIN_DATA_POSITION;
            }
        }

        public void Close()
        {
            Reader.Close();
            Writer.Close();
        }

        public void Create(long size)
        {
            Writer.Write((byte)1);
            for (long i = 1; i < size; i++)
                Writer.Write(0x00);
            Writer.Flush();
        }

        public void AddFile(string name, byte[] data)
        {
            VirtualFile file = new VirtualFile(name, DataPosition, DataPosition + data.Length);
            incrementFileCount();
            Writer.Write(file.Name);
            Writer.Write(file.BeginPosition);
            Writer.Write(file.EndPosition);
            Writer.Flush();
            writeFileContents(data);
            Files.Add(file);
        }

        public byte[] GetFile(string name)
        {
            VirtualFile file = null;
            foreach (var f in Files)
                if (f.Name == name)
                    return GetFile(f);
            throw new Exception("File " + name + " not found!");
        }
        public byte[] GetFile(VirtualFile file)
        {
            byte[] data = new byte[file.EndPosition - file.BeginPosition];

            long temp = Position;
            Position = file.BeginPosition;
            data = Reader.ReadBytes((int)(file.EndPosition - file.BeginPosition));
            Position = temp;

            return data;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            long temp = Position;
            Position = 0;
            sb.AppendLine("Byte: " + Reader.ReadByte());
            sb.AppendLine("Long: " + Reader.ReadInt64());
            for (int i = 0; i < (int)FileCount; i++)
                sb.AppendLine(string.Format("String: {0}, Long {1}, Long {2}", Reader.ReadString(), Reader.ReadInt64(), Reader.ReadInt64()));
            Position = Files[0].BeginPosition;
            for (int i = 0; i < 100; i++)
                sb.Append((int)Reader.ReadByte());
            Position = temp;

            return sb.ToString();
        }

        private void incrementFileCount(int amount = 1)
        {
            long temp = Position;
            Position = 1;
            Writer.Write(FileCount + amount);
            Position = temp;
        }

        private void writeFileContents(byte[] data)
        {
            long temp = Position;
            Position = DataPosition;
            foreach (byte b in data)
                Writer.Write(b);
            Writer.Flush();
            Position = temp;
            DataPosition += data.Length + 1;
        }
    }

    public class VirtualFile
    {
        public string Name { get; private set; }
        public long BeginPosition { get; private set; }
        public long EndPosition { get; private set; }

        public VirtualFile(string name, long beginPos, long endPos)
        {
            Name = name;
            BeginPosition = beginPos;
            EndPosition = endPos;
        }
    }
}