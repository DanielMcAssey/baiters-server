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
                WriteDictionary(packet, writer);
            }

            return stream.ToArray();
        }

        private static void WriteAny(object packet, BinaryWriter writer)
        {
            if (packet == null)
            {
                writer.Write(0);
            }
            else if (packet is Dictionary<string, object> pktStrDict)
            {
                WriteDictionary(pktStrDict, writer);
            }
            else if (packet is string pktString)
            {
                WriteString(pktString, writer);
            }
            else if (packet is int pktInt)
            {
                WriteInt(pktInt, writer);
            }
            else if (packet is long pktLong)
            {
                WriteLong(pktLong, writer);
            }
            else if (packet is float pktSingle)
            {
                WriteSingle(pktSingle, writer);
            }
            else if (packet is double pktDouble)
            {
                WriteDouble(pktDouble, writer);
            }
            else if (packet is bool pktBool)
            {
                WriteBool(pktBool, writer);
            }
            else if (packet is Dictionary<int, object> pktIntDict)
            {
                WriteArray(pktIntDict, writer);
            }
            else if (packet is Vector3 pktVect3)
            {
                WriteVector3(pktVect3, writer);
            }
            else if (packet is Vector2 pktVect2)
            {
                WriteVector2(pktVect2, writer);
            }
            else
            {
                throw new Exception("Unknown type: " + packet.GetType());
            }
        }

        private static void WriteVector2(Vector2 packet, BinaryWriter writer)
        {
            writer.Write((int)GodotTypes.Vector2);
            writer.Write(packet.X);
            writer.Write(packet.Y);
        }

        private static void WriteVector3(Vector3 packet, BinaryWriter writer)
        {
            writer.Write((int)GodotTypes.Vector3);
            writer.Write(packet.X);
            writer.Write(packet.Y);
            writer.Write(packet.Z);
        }

        private static void WriteBool(bool packet, BinaryWriter writer)
        {
            writer.Write((int)GodotTypes.Bool);
            writer.Write(packet ? 1 : 0);
        }

        private static void WriteInt(int packet, BinaryWriter writer)
        {
            writer.Write((int)GodotTypes.Int);
            writer.Write(packet);
        }

        private static void WriteLong(long packet, BinaryWriter writer)
        {
            writer.Write((int)65538); // write the int value header! this is the same as above but with the 64 bit header!
            writer.Write(packet);
        }

        private static void WriteSingle(float packet, BinaryWriter writer)
        {
            writer.Write((int)GodotTypes.Float);
            writer.Write((float)packet);
        }

        private static void WriteDouble(double packet, BinaryWriter writer)
        {
            writer.Write((int)65539);// write the float value header! this is the same as above but with the 64 bit header!
            writer.Write((double)packet);
        }

        private static void WriteString(string packet, BinaryWriter writer)
        {
            writer.Write((int)GodotTypes.String);

            byte[] bytes = Encoding.UTF8.GetBytes(packet);

            writer.Write((int)bytes.Length);
            // get the ammount to pad by!

            // Step 3: Write the actual bytes of the string
            writer.Write(bytes);

            // Step 4: Calculate padding needed to make the total length a multiple of 4
            int padding = (4 - (bytes.Length % 4)) % 4; // Calculate padding

            // Step 5: Write padding bytes (if needed)
            for (int i = 0; i < padding; i++)
            {
                writer.Write((byte)0); // Write padding as zero bytes
            }
        }

        private static void WriteArray(Dictionary<int, object> packet, BinaryWriter writer)
        {
            // because we have a dic we need to write the correct byte info!
            writer.Write((int)GodotTypes.Array);
            writer.Write((int)packet.Count);

            for (int i = 0; i < packet.Count; i++)
            {
                WriteAny(packet[i], writer);
            }
        }

        private static void WriteDictionary(Dictionary<string, object> packet, BinaryWriter writer)
        {
            writer.Write((int)GodotTypes.Dictionary);
            writer.Write((int)packet.Count);

            foreach (var pair in packet)
            {
                WriteAny(pair.Key, writer);
                WriteAny(pair.Value, writer);
            }
        }
    }
}
