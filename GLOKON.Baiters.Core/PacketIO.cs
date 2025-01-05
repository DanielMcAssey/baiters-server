using GLOKON.Baiters.GodotInterop;
using Steamworks;
using System.IO.Compression;

namespace GLOKON.Baiters.Core
{
    internal static class PacketIO
    {
        public static byte[] WritePacket(Dictionary<string, object> packet)
        {
            byte[] data = GodotWriter.WritePacket(packet);

            using var outputStream = new MemoryStream();
            using var gzipStream = new GZipStream(outputStream, CompressionMode.Compress);

            gzipStream.Write(data, 0, data.Length);
            return outputStream.ToArray();
        }

        public static Dictionary<string, object> ReadPacket(byte[] data)
        {
            using var compressedStream = new MemoryStream(data);
            using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();

            gzipStream.CopyTo(resultStream);
            return GodotReader.ReadPacket(resultStream.ToArray());
        }
    }
}
