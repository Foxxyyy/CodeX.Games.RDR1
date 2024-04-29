using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.IO.Compression;
using System.IO;
using System.Security.Cryptography;
using Zstandard.Net;
using System.Collections.Generic;
using CodeX.Core.Utilities;
using System.Numerics;
using System.Xml;
using CodeX.Core.Numerics;
using CodeX.Core.Engine;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.RPF6
{
    public static class Rpf6Crypto
    {
        public const ulong VIRTUAL_BASE  = 0x50000000;
        public const ulong PHYSICAL_BASE = 0x60000000;

        static Aes AesAlg;
        static readonly byte[] AES_KEY = new byte[32]
        {
            0xB7, 0x62, 0xDF, 0xB6, 0xE2, 0xB2, 0xC6, 0xDE, 0xAF, 0x72, 0x2A, 0x32, 0xD2, 0xFB, 0x6F, 0x0C, 0x98, 0xA3, 0x21, 0x74, 0x62, 0xC9, 0xC4, 0xED, 0xAD, 0xAA, 0x2E, 0xD0, 0xDD, 0xF9, 0x2F, 0x10
        };

        public static bool Init()
        {
            AesAlg = Aes.Create();
            AesAlg.BlockSize = 128;
            AesAlg.KeySize = 256;
            AesAlg.Mode = CipherMode.ECB;
            AesAlg.Key = AES_KEY;
            AesAlg.IV = new byte[16];
            AesAlg.Padding = PaddingMode.None;
            return true;
        }

        public static byte[] DecryptAES(byte[] data)
        {
            var rijndael = Aes.Create();
            rijndael.KeySize = 256;
            rijndael.Key = AES_KEY;
            rijndael.BlockSize = 128;
            rijndael.Mode = CipherMode.ECB;
            rijndael.Padding = PaddingMode.None;

            var buffer = (byte[])data.Clone();
            var length = data.Length & -16;

            if (length > 0)
            {
                var decryptor = rijndael.CreateDecryptor();
                for (var roundIndex = 0; roundIndex < 16; roundIndex++)
                    decryptor.TransformBlock(buffer, 0, length, buffer, 0);
            }
            return buffer;
        }

        public static byte[] EncryptAES(byte[] data)
        {
            var rijndael = Aes.Create();
            rijndael.KeySize = 256;
            rijndael.Key = AES_KEY;
            rijndael.BlockSize = 128;
            rijndael.Mode = CipherMode.ECB;
            rijndael.Padding = PaddingMode.None;

            var buffer = (byte[])data.Clone();
            var length = data.Length & -16;

            if (length > 0)
            {
                var encryptor = rijndael.CreateEncryptor();
                for (var roundIndex = 0; roundIndex < 16; roundIndex++)
                    encryptor.TransformBlock(buffer, 0, length, buffer, 0);
            }
            return buffer;
        }

        public static byte[] DecompressDeflate(byte[] data, int decompSize, bool noHeader = true) //ZLIB
        {
            byte[] buffer = new byte[decompSize];
            var inflater = new Inflater(noHeader);
            inflater.SetInput(data);
            inflater.Inflate(buffer);
            return buffer;
        }

        public static byte[] DecompressZStandard(byte[] compressedData) //ZStandard
        {
            byte[] decompressedData = null;

            using (var memoryStream = new MemoryStream(compressedData))
            using (var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Decompress))
            using (var temp = new MemoryStream())
            {
                compressionStream.CopyTo(temp);
                decompressedData = temp.ToArray();
            }
            return decompressedData;
        }

        public static byte[] CompressZStandard(byte[] decompressedData) //ZStandard
        {
            byte[] compressedData = null;

            using (var memoryStream = new MemoryStream())
            using (var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Compress))
            {
                compressionStream.Write(decompressedData, 0, decompressedData.Length);
                compressionStream.Close();
                compressedData = memoryStream.ToArray();
            }
            return compressedData;
        }

        public static int SetBit(int val, int bit, bool trueORfalse)
        {
            bool flag = (uint)(val & 1 << bit) > 0U;
            if (trueORfalse)
            {
                if (!flag)
                    return val |= 1 << bit;
            }
            else if (flag)
            {
                return val ^ 1 << bit;
            }
            return val;
        }

        public static int TrailingZeroes(int n)
        {
            int num1 = 1;
            int num2 = 0;

            while (num2 < 32)
            {
                if ((uint)(n & num1) > 0U)
                {
                    return num2;
                }
                ++num2;
                num1 <<= 1;
            }
            return 32;
        }

        public static void RemoveDictValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TValue value)
        {
            //Find the key associated with the specified value
            TKey keyToRemove = default;
            foreach (var kvp in dictionary)
            {
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                {
                    keyToRemove = kvp.Key;
                    break;
                }
            }

            //Remove the key-value pair
            if (!EqualityComparer<TKey>.Default.Equals(keyToRemove, default))
            {
                dictionary.Remove(keyToRemove);
            }
        }

        public static long RoundUp(long num, long multiple)
        {
            if (multiple == 0L)
            {
                return 0;
            }
            long num1 = multiple / Math.Abs(multiple);
            return (num + multiple - num1) / multiple * multiple;
        }

        public static int Swap(int value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public static uint Swap(uint value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public static short Swap(short value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public static ushort Swap(ushort value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public static long Swap(long value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public static ulong Swap(ulong value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        public static float Swap(float value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }

        //Swap the axis from XYZ to ZXY
        public static Vector3 ToZXY(Vector3 vec)
        {
            return new Vector3(vec.Z, vec.X, vec.Y);
        }

        //Swap the axis from XYZ to ZXY
        public static Vector4 ToZXY(Vector4 vec)
        {
            return new Vector4(vec.Z, vec.X, vec.Y, vec.W);
        }

        //Swap the axis from XYZ to ZXY
        public static BoundingBox4 ToZXY(BoundingBox4 bb)
        {
            var newBB = new BoundingBox()
            {
                Minimum = ToZXY(bb.Min.XYZ()),
                Maximum = ToZXY(bb.Max.XYZ())
            };
            return new BoundingBox4(newBB);
        }

        //Swap the axis from XYZ to ZXY
        public static BoundingBox4[] ToZXY(BoundingBox4[] bb)
        {
            for (int i = 0; i < bb.Length; i++)
            {
                bb[i] = ToZXY(bb[i]);
            }
            return bb;
        }

        //Swap the axis from XYZ to ZXY
        public static Quaternion ToZXY(Quaternion quat)
        {
            return new Quaternion(quat.Z, quat.X, quat.Y, quat.W);
        }

        //Swap the axis from XYZ to ZXY
        public static Matrix3x4 ToZXY(Matrix3x4 m)
        {
            m.Translation = ToZXY(m.Translation);
            m.Orientation = ToZXY(m.Orientation);
            return m;
        }

        //Swap the axis from XYZ to ZXY
        public static Matrix3x4[] ToZXY(Matrix3x4[] m)
        {
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = ToZXY(m[i]);
            }
            return m;
        }

        //Swap the axis from XYZ to ZXY
        public static Matrix4x4 ToZXY(Matrix4x4 m, bool write = false)
        {
            var m14 = write ? GetNaN() : m.M14;
            var m24 = write ? GetNaN() : m.M24;
            var m34 = write ? GetNaN() : m.M34;
            var m44 = write ? GetNaN() : (float.IsNaN(m.M44) ? 0.0f : m.M44);
            var translation = m.Translation;
            m.Decompose(out var scale, out var rot, out var trans);

            return new Matrix4x4(
                m.M11, m.M12, m.M13, m14,
                m.M21, m.M22, m.M23, m24,
                m.M31, m.M32, m.M33, m34,
                translation.Z, translation.X, translation.Y, m44
            );
        }

        //Swap the axis from XYZ to YZX
        public static Matrix4x4 ToYZX(Matrix4x4 m, bool write = false)
        {
            var m14 = write ? GetNaN() : m.M14;
            var m24 = write ? GetNaN() : m.M24;
            var m34 = write ? GetNaN() : m.M34;
            var m44 = write ? GetNaN() : (float.IsNaN(m.M44) ? 0.0f : m.M44);
            var translation = m.Translation;

            return new Matrix4x4(
                m.M11, m.M12, m.M13, m14,
                m.M21, m.M22, m.M23, m24,
                m.M31, m.M32, m.M33, m34,
                translation.Y, translation.Z, translation.X, m44
            );
        }

        //Swap the axis from XYZ to ZXY
        public static Matrix4x4[] ToZXY(Matrix4x4[] m)
        {
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = ToZXY(m[i]);
            }
            return m;
        }

        public static Vector3 ToYZX(Vector3 vector)
        {
            return new Vector3(vector.Y, vector.Z, vector.X);
        }

        public static Vector4 ToYZX(Vector4 vector, bool wNaN = false)
        {
            return new Vector4(vector.Y, vector.Z, vector.X, vector.W);
        }

        public static Vector4 GetXmlVector4(XmlNode node, string name)
        {
            var vector = Xml.GetChildVector4Attributes(node, name);
            return new Vector4(vector.Y, vector.Z, vector.X, vector.W);
        }

        //Swap the axis and writes a Vector3 at the given offset in a buffer
        public static void WriteVector3AtIndex(Vector3 vec, byte[] buffer, int offset, bool zxy = true)
        {
            var x = BitConverter.GetBytes(vec.X);
            var y = BitConverter.GetBytes(vec.Y);
            var z = BitConverter.GetBytes(vec.Z);

            if (zxy) //XYZ > ZXY (RDR1 to CX)
            {
                Buffer.BlockCopy(z, 0, buffer, offset, sizeof(float));
                Buffer.BlockCopy(x, 0, buffer, offset + 4, sizeof(float));
                Buffer.BlockCopy(y, 0, buffer, offset + 8, sizeof(float));
            }
            else //XYZ > YZX (CX to RDR1)
            {
                Buffer.BlockCopy(y, 0, buffer, offset, sizeof(float));
                Buffer.BlockCopy(z, 0, buffer, offset + 4, sizeof(float));
                Buffer.BlockCopy(x, 0, buffer, offset + 8, sizeof(float));
            }
        }

        //Swap the axis and writes a Vector4 at the given offset in a buffer
        public static void WriteVector4AtIndex(Vector4 vec, byte[] buffer, int offset, bool zxy = true)
        {
            if (float.IsNaN(vec.W))
            {
                vec.W = 0.0f;
            }

            var x = BitConverter.GetBytes(vec.X);
            var y = BitConverter.GetBytes(vec.Y);
            var z = BitConverter.GetBytes(vec.Z);
            var w = BitConverter.GetBytes(vec.W);

            if (zxy) //XYZ > ZXY (RDR1 to CX)
            {
                Buffer.BlockCopy(z, 0, buffer, offset, sizeof(float));
                Buffer.BlockCopy(x, 0, buffer, offset + 4, sizeof(float));
                Buffer.BlockCopy(y, 0, buffer, offset + 8, sizeof(float));
                Buffer.BlockCopy(w, 0, buffer, offset + 12, sizeof(float));
            }
            else //XYZ > YZX (CX to RDR1)
            {
                Buffer.BlockCopy(y, 0, buffer, offset, sizeof(float));
                Buffer.BlockCopy(z, 0, buffer, offset + 4, sizeof(float));
                Buffer.BlockCopy(x, 0, buffer, offset + 8, sizeof(float));
                Buffer.BlockCopy(w, 0, buffer, offset + 12, sizeof(float));
            }
        }

        //Rescale Half2 texcoords (used for the terrain tiles)
        public static Vector2 RescaleHalf2(Half2 val, float scale)
        {
            return new Half2((float)val.X * scale, (float)val.Y * scale);
        }

        //Reads UShort2N texcoords and rescale the values depending of the current model LOD
        public static float[] ReadRescaleUShort2N(byte[] buffer, int offset, bool highLOD)
        {
            var xBuf = BufferUtil.ReadArray<byte>(buffer, offset, 2);
            var yBuf = BufferUtil.ReadArray<byte>(buffer, offset + 2, 2);
            var xVal = BitConverter.ToUInt16(xBuf, 0) * 3.05185094e-005f;
            var yVal = BitConverter.ToUInt16(yBuf, 0) * 3.05185094e-005f;

            float[] values;
            if (!highLOD)
            {
                xVal *= 2f;
                yVal *= 2f;
                values = new float[2] { float.NaN, float.NaN };
            }
            else
            {
                values = new float[2] { xVal, yVal };
            }

            //We can write the values anyway, for 'resource_0' they will be overwritten later...
            BufferUtil.WriteArray(buffer, offset, BitConverter.GetBytes((ushort)(xVal / 3.05185094e-005f)));
            BufferUtil.WriteArray(buffer, offset + 2, BitConverter.GetBytes((ushort)(yVal / 3.05185094e-005f)));
            return values;
        }

        //Creates a Colour from a string representing RGB values, ie "255, 255, 255"
        public static Colour ParseRGBString(string rgbString)
        {
            string[] rgbValues = rgbString.Split(',');
            if (rgbValues.Length == 3 && int.TryParse(rgbValues[0], out int red) && int.TryParse(rgbValues[1], out int green) && int.TryParse(rgbValues[2], out int blue))
            {
                return new Colour(red, green, blue);
            }
            return Colour.Red;
        }

        //A hacky function to adjust the bounds correctly for fragments, used for the prefabs
        public static void ResizeBoundsForPeds(Piece piece)
        {
            if (piece == null) return;
            var min = ((Rsc6Drawable)piece).BoundingBoxMin.XYZ();
            var max = ((Rsc6Drawable)piece).BoundingBoxMax.XYZ();

            min = new Vector3(min.X, min.Y, min.Z - 2.0f);
            max = new Vector3(max.X, max.Y, max.Z - 2.0f);

            piece.BoundingBox = new BoundingBox(min, max);
            piece.BoundingSphere = new BoundingSphere(piece.BoundingBox.Center, piece.BoundingBox.Size.Length() * 0.5f);
        }

        //Convert a Vector4 to a byte array
        public static byte[] Vector4ToByteArray(Vector4 vector)
        {
            var buffer = new byte[16];
            Buffer.BlockCopy(new float[]
            {
                vector.X,
                vector.Y,
                vector.Z,
                vector.W
            }, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        //Checks if a specified value is present in an enum
        public static bool IsDefinedInEnumRange<TEnum>(byte value) where TEnum : Enum
        {
            foreach (byte enumValue in Enum.GetValues(typeof(TEnum)))
            {
                if (enumValue == value)
                {
                    return true;
                }
            }
            return false;
        }

        //Returns NaN as 0x0100807F, (float.NaN = 0x0000C0FF)
        public static float GetNaN()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(0x7F800001), 0);
        }

        //Returns a Vector4 of NaN as 0x0100807F, (float.NaN = 0x0000C0FF)
        public static Vector4 GetVec4NaN()
        {
            return new Vector4(GetNaN(), GetNaN(), GetNaN(), GetNaN());
        }

        //Converts a floating point number into a fixed point number
        //It's simply multiplying the float by a constant value and discarding the extra bits
        public static uint PackFixedPoint(float value, uint size, uint shift) //Vector3::Pack1010102
        {
            float scale = ((1u << (int)(size - 1)) - 1);
            return ((uint)(value * scale) & ((1u << (int)size) - 1)) << (int)shift;
        }

        //Converts a Vector4 (XYZ to ZXY/YZX) to Dec3N
        public static uint GetDec3N(Vector4 val, bool zxy = true)
        {
            var wPack = val.W > 0.5f ? 1 << 30 : val.W < -0.5f ? -1 << 30 : 0;
            if (zxy) //XYZ > ZXY (RDR1 to CX)
                return PackFixedPoint(val.Z, 10, 0) | PackFixedPoint(val.X, 10, 10) | PackFixedPoint(val.Y, 10, 20) | (uint)wPack;
            else     //XYZ > YZX (CX to RDR1)
                return PackFixedPoint(val.Y, 10, 0) | PackFixedPoint(val.Z, 10, 10) | PackFixedPoint(val.X, 10, 20) | (uint)wPack;
        }
    }
}