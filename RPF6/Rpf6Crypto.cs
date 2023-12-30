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
using System.Linq;

namespace CodeX.Games.RDR1.RPF6
{
    public static class Rpf6Crypto
    {
        static Aes AesAlg;
        static byte[] AES_KEY = new byte[32]
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

        public static Vector3 GetXmlVector3(XmlNode node, string name)
        {
            var vector = Xml.GetChildVector3Attributes(node, name);
            return new Vector3(vector.Y, vector.Z, vector.X);
        }

        public static Vector4 GetXmlVector4(XmlNode node, string name)
        {
            var vector = Xml.GetChildVector4Attributes(node, name);
            return new Vector4(vector.Y, vector.Z, vector.X, vector.W);
        }

        //Converts a Vector3 (XYZ to ZXY) to Dec3N
        public static uint GetDec3N(Vector3 val)
        {
            return PackFixedPoint(val.Z, 10, 0) | PackFixedPoint(val.X, 10, 10) | PackFixedPoint(val.Y, 10, 20);
        }

        //Writes a Vector3 (XYZ to ZXY) at the given offset in a buffer
        public static void WriteVector3AtIndex(Vector3 vec, byte[] buffer, int offset)
        {
            var x = BitConverter.GetBytes(vec.X);
            var y = BitConverter.GetBytes(vec.Y);
            var z = BitConverter.GetBytes(vec.Z);
            Buffer.BlockCopy(z, 0, buffer, offset, sizeof(float));
            Buffer.BlockCopy(x, 0, buffer, offset + 4, sizeof(float));
            Buffer.BlockCopy(y, 0, buffer, offset + 8, sizeof(float));
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

        //Rescale and writes UV coords in a given byte array
        //UV coords are normalized by dividing each coordinate by the range (difference between max and min values)
        //After this step, they should lie in the range [0, 1].
        public static void NormalizeUVs(List<float> uvX, List<float> uvY, List<int> uvOffset, ref byte[] buffer)
        {
            if (uvX.Count <= 0 || uvY.Count <= 0 || uvOffset.Count <= 0) return;

            var minU = uvX.Min();
            var maxU = uvX.Max();
            var minV = uvY.Min();
            var maxV = uvY.Max();

            //Shift the values to the origin
            for (int i = 0; i < uvX.Count; i++)
            {
                uvX[i] -= minU;
                uvY[i] -= minV;
            }

            //Normalize the values
            for (int i = 0; i < uvX.Count; i++)
            {
                uvX[i] /= maxU - minU;
                uvY[i] /= maxV - minV;
            }

            //Convert to bytes and update buffer
            for (int i = 0; i < uvX.Count; i++)
            {
                var x1 = (ushort)(uvX[i] / 3.05185094e-005f);
                var y1 = (ushort)(uvY[i] / 3.05185094e-005f);
                var xBuf = BitConverter.GetBytes(x1);
                var yBuf = BitConverter.GetBytes(y1);
                BufferUtil.WriteArray(buffer, uvOffset[i], xBuf);
                BufferUtil.WriteArray(buffer, uvOffset[i] + 2, yBuf);
            }
        }

        //Converts a floating point number into a fixed point number, it's simply multiplying the float by a constant value and discarding the extra bits
        public static uint PackFixedPoint(float value, uint size, uint shift) //Vector3::Pack1010102
        {
            float scale = ((1u << (int)(size - 1)) - 1);
            return ((uint)(value * scale) & ((1u << (int)size) - 1)) << (int)shift;
        }
    }
}