/*
  Copyright 2024 DrMeepso
  Copyright 2025 DanielMcAssey - Additional improvements

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.

  Source: https://github.com/DrMeepso/WebFishingCove/blob/main/Cove/GodotFormat/GDWriter.cs
*/

using System.Text;
using System.Numerics;

namespace GLOKON.Baiters.GodotInterop
{
    public static class GodotWriter
    {
        public static byte[] WritePacket(Dictionary<string, object> packet)
        {
            using var stream = new MemoryStream();

            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                Write(packet, writer);
            }

            return stream.ToArray();
        }

        private static void WriteVariant(object packet, BinaryWriter writer)
        {
            switch (packet)
            {
                case null:
                    writer.Write(BitConverter.GetBytes(0));
                    break;
                case Dictionary<string, object> pktStrDict:
                    Write(pktStrDict, writer);
                    break;
                case string pktString:
                    Write(pktString, writer);
                    break;
                case int pktInt:
                    Write(pktInt, writer);
                    break;
                case long pktLong:
                    Write(pktLong, writer);
                    break;
                case float pktSingle:
                    Write(pktSingle, writer);
                    break;
                case double pktDouble:
                    Write(pktDouble, writer);
                    break;
                case bool pktBool:
                    Write(pktBool, writer);
                    break;
                case Dictionary<int, object> pktIntDict:
                    Write(pktIntDict, writer);
                    break;
                case Vector3 pktVect3:
                    Write(pktVect3, writer);
                    break;
                case Vector2 pktVect2:
                    Write(pktVect2, writer);
                    break;
                default:
                    throw new Exception($"Unknown type: {packet.GetType()}");
            }
        }

        private static void Write(GodotTypes type, BinaryWriter writer)
        {
            writer.Write(BitConverter.GetBytes((uint)type));
        }

        private static void Write(Vector2 packet, BinaryWriter writer)
        {
            Write(GodotTypes.Vector2, writer);
            writer.Write(BitConverter.GetBytes(packet.X));
            writer.Write(BitConverter.GetBytes(packet.Y));
        }

        private static void Write(Vector3 packet, BinaryWriter writer)
        {
            Write(GodotTypes.Vector3, writer);
            writer.Write(BitConverter.GetBytes(packet.X));
            writer.Write(BitConverter.GetBytes(packet.Y));
            writer.Write(BitConverter.GetBytes(packet.Z));
        }

        private static void Write(bool packet, BinaryWriter writer)
        {
            Write(GodotTypes.Bool, writer);
            writer.Write(BitConverter.GetBytes((uint)(packet ? 1 : 0)));
        }

        private static void Write(int packet, BinaryWriter writer)
        {
            Write(GodotTypes.Int, writer);
            writer.Write(BitConverter.GetBytes((uint)packet));
        }

        private static void Write(long packet, BinaryWriter writer)
        {
            Write(GodotTypes.Long, writer);
            writer.Write(BitConverter.GetBytes((ulong)packet));
        }

        private static void Write(float packet, BinaryWriter writer)
        {
            Write(GodotTypes.Float, writer);
            writer.Write(BitConverter.GetBytes(packet));
        }

        private static void Write(double packet, BinaryWriter writer)
        {
            Write(GodotTypes.Double, writer);
            writer.Write(BitConverter.GetBytes(packet));
        }

        private static void Write(string packet, BinaryWriter writer)
        {
            Write(GodotTypes.String, writer);
            writer.Write(BitConverter.GetBytes((uint)packet.Length)); // Length
            writer.Write(Encoding.UTF8.GetBytes(packet)); // String Content

            // Step 4: Calculate padding needed to make the total length a multiple of 4
            int padding = (4 - (packet.Length % 4)) % 4;

            // Step 5: Write padding bytes (if needed)
            for (int i = 0; i < padding; i++)
            {
                writer.Write((byte)0); // Write padding as zero bytes
            }
        }

        private static void Write(Dictionary<int, object> packet, BinaryWriter writer)
        {
            Write(GodotTypes.Array, writer);
            writer.Write(BitConverter.GetBytes((uint)packet.Count));

            for (int i = 0; i < packet.Count; i++)
            {
                WriteVariant(packet[i], writer);
            }
        }

        private static void Write(Dictionary<string, object> packet, BinaryWriter writer)
        {
            Write(GodotTypes.Dictionary, writer);
            writer.Write(BitConverter.GetBytes((uint)packet.Count));

            foreach (var pair in packet)
            {
                WriteVariant(pair.Key, writer);
                WriteVariant(pair.Value, writer);
            }
        }
    }
}
