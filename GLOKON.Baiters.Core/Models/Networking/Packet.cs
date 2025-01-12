using GLOKON.Baiters.GodotInterop;
using System.IO.Compression;

namespace GLOKON.Baiters.Core.Models.Networking
{
    public class Packet : Dictionary<string, object>
    {
        public Packet(string type) : base()
        {
            Add("type", type);
            Type = type;
        }

        public Packet(Dictionary<string, object> data) : base()
        {
            foreach (var item in data)
            {
                if (item.Key == "type")
                {
                    Type = (string)item.Value;
                }

                Add(item.Key, item.Value);
            }
        }

        public string Type { get; } = "unknown";

        public byte[] ToBytes()
        {
            byte[] data = GodotWriter.WritePacket(this);

            using var outputStream = new MemoryStream();

            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(data, 0, data.Length);
            }

            return outputStream.ToArray();
        }

        public static Packet Parse(byte[] data)
        {
            using var resultStream = new MemoryStream();

            using (var compressedStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(resultStream);
            }

            return new(GodotReader.ReadPacket(resultStream.ToArray()));
        }
    }
}
