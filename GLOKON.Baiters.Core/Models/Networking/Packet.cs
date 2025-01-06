using GLOKON.Baiters.GodotInterop;
using System.IO.Compression;

namespace GLOKON.Baiters.Core.Models.Networking
{
    public class Packet : Dictionary<string, object>
    {
        public Packet(string type) : base()
        {
            Add("type", type);
        }

        public string Type { get { return (string)(this.GetValueOrDefault("type") ?? "unknown"); } }

        public byte[] Serialize()
        {
            byte[] data = GodotWriter.WritePacket(this);

            using var outputStream = new MemoryStream();
            using var gzipStream = new GZipStream(outputStream, CompressionMode.Compress);

            gzipStream.Write(data, 0, data.Length);
            return outputStream.ToArray();
        }

        public static Packet Parse(byte[] data)
        {
            using var compressedStream = new MemoryStream(data);
            using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();

            gzipStream.CopyTo(resultStream);
            return (Packet)GodotReader.ReadPacket(resultStream.ToArray());
        }
    }
}
