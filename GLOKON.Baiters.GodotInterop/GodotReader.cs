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

  Source: https://github.com/DrMeepso/WebFishingCove/blob/main/Cove/GodotFormat/GDReader.cs
*/
using GLOKON.Baiters.GodotInterop.Models;
using System.Numerics;
using System.Text;

namespace GLOKON.Baiters.GodotInterop
{
    public static class GodotReader
    {
        public static Dictionary<string, object> ReadPacket(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            // Read header
            var type = (GodotTypes)reader.ReadInt32();

            if (type != GodotTypes.Dictionary)
            {
                throw new Exception("Unable to decode a non-dictionary godot packet!");
            }

            return ReadDictionary(reader);
        }

        private static object ReadNext(BinaryReader reader)
        {
            int typeHeader = reader.ReadInt32();

            GodotTypes type = (GodotTypes)(typeHeader & 0xFFFF);
            int flags = typeHeader >> 16;

            // TODO: Do we need to add missing types?
            return type switch
            {
                GodotTypes.Null => null,
                GodotTypes.Dictionary => ReadDictionary(reader),
                GodotTypes.Array => ReadArray(reader),
                GodotTypes.String => ReadString(reader),
                GodotTypes.Int => ReadInt(reader, flags),
                GodotTypes.Vector3 => ReadVector3(reader),
                GodotTypes.Quaternion => ReadQuaternion(reader),
                GodotTypes.Bool => ReadBool(reader),
                GodotTypes.Float => ReadFloat(reader, flags),
                GodotTypes.Plane => ReadPlane(reader),
                GodotTypes.Vector2 => ReadVector2(reader),
                _ => new ReadError($"Unable to handel object of type: {type}"),
            };
        }

        private static Plane ReadPlane(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float dist = reader.ReadSingle();

            return new(x, y, z, dist);
        }

        private static double ReadFloat(BinaryReader reader, int flags)
        {
            if ((flags & 1) == 1)
            {
                return reader.ReadDouble();
            }
            else
            {
                return reader.ReadSingle();
            }
        }

        private static bool ReadBool(BinaryReader reader)
        {
            return reader.ReadUInt32() != 0;
        }

        private static Quaternion ReadQuaternion(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            return new(x, y, z, w);
        }

        private static Vector2 ReadVector2(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            return new(x, y);
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return new(x, y, z);
        }

        private static long ReadInt(BinaryReader reader, int flags)
        {
            if ((flags & 1) == 1)
            {
                return (long)reader.ReadUInt64();
            }
            else
            {
                return (int)reader.ReadUInt32();
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            int stringLength = (int)reader.ReadUInt32();
            string stringValue = Encoding.UTF8.GetString(reader.ReadBytes(stringLength));

            // this field is padded to 4 bytes
            if (4 - ((int)reader.BaseStream.Position % 4) != 4)
            {
                reader.ReadBytes(4 - ((int)reader.BaseStream.Position % 4));
            }

            return stringValue;
        }

        private static Dictionary<int, object> ReadArray(BinaryReader reader)
        {
            Dictionary<int, object> array = [];

            int elementCount = (int)reader.ReadUInt32();
            elementCount &= 0x7FFFFFFF;

            for (int i = 0; i < elementCount; i++)
            {
                array[i] = ReadNext(reader);
            }

            return array;
        }

        private static Dictionary<string, object> ReadDictionary(BinaryReader reader)
        {
            Dictionary<string, object> dic = [];

            int elementCount = (int)reader.ReadUInt32();
            elementCount &= 0x7FFFFFFF;

            for (int i = 0; i < elementCount; i++)
            {
                if (ReadNext(reader) is string key)
                {
                    dic[key] = ReadNext(reader);
                }
                else
                {
                    // if the value is not a string (bad read) break the loop.
                    break; //break from the loop to save the server!
                }
            }

            return dic;
        }
    }
}
