using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.IO.Compression;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using Zstandard.Net;
using ICSharpCode.SharpZipLib.Checksum;
using System.Text;

namespace CodeX.Games.RDR1.RPF6
{
    public static class Rpf6Crypto
    {
        public static byte[] AES_KEY = new byte[32]
        {
            0xB7, 0x62, 0xDF, 0xB6, 0xE2, 0xB2, 0xC6, 0xDE, 0xAF, 0x72, 0x2A, 0x32, 0xD2, 0xFB, 0x6F, 0x0C, 0x98, 0xA3, 0x21, 0x74, 0x62, 0xC9, 0xC4, 0xED, 0xAD, 0xAA, 0x2E, 0xD0, 0xDD, 0xF9, 0x2F, 0x10
        };

        public static byte[] DecryptAES(byte[] data)
        {
            return DecryptAESData(data, AES_KEY);
        }

        public static byte[] DecryptAESData(byte[] data, byte[] key)
        {
            byte[] buffer = new byte[data.Length];
            data.CopyTo(buffer, 0);

            int inputCount = buffer.Length & -16;
            if (inputCount > 0)
            {
                Rijndael rijndael = Rijndael.Create();
                rijndael.BlockSize = 128;
                rijndael.KeySize = 256;
                rijndael.Mode = CipherMode.ECB;
                rijndael.Key = key;
                rijndael.IV = new byte[16];
                rijndael.Padding = PaddingMode.None;
                ICryptoTransform decryptor = rijndael.CreateDecryptor();

                for (int index = 0; index < 16; ++index)
                {
                    decryptor.TransformBlock(buffer, 0, inputCount, buffer, 0);
                }
            }
            return buffer;
        }

        public static byte[] EncryptAES(byte[] data)
        {
            return EncryptAESData(data, AES_KEY);
        }

        public static byte[] EncryptAESData(byte[] data, byte[] key)
        {
            byte[] buffer = new byte[data.Length];
            data.CopyTo(buffer, 0);

            int inputCount = buffer.Length & -16;
            if (inputCount > 0)
            {
                Rijndael rijndael = Rijndael.Create();
                rijndael.BlockSize = 128;
                rijndael.KeySize = 256;
                rijndael.Mode = CipherMode.ECB;
                rijndael.Key = key;
                rijndael.IV = new byte[16];
                rijndael.Padding = PaddingMode.None;
                ICryptoTransform encryptor = rijndael.CreateEncryptor();

                for (int index = 0; index < 16; ++index)
                {
                    encryptor.TransformBlock(buffer, 0, inputCount, buffer, 0);
                }
            }
            return buffer;
        }

        public static byte[] DecompressDeflate(byte[] data, int decompSize, bool noHeader = true) //Decompress using ZLIB
        {
            byte[] buffer = new byte[decompSize];
            Inflater inflater = new Inflater(noHeader);
            inflater.SetInput(data);
            inflater.Inflate(buffer);
            return buffer;
        }

        public static byte[] DecompressZStandard(byte[] compressedData)
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

        public static byte[] CompressZStandard(byte[] decompressedData)
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

        public static uint Vector3ToDec3N(in Vector3 v)
        {
            var sx = (v.X < 0.0f);
            var sy = (v.Y < 0.0f);
            var sz = (v.Z < 0.0f);
            var x = Math.Min((uint)(Math.Abs(v.X) * 511.0f), 511);
            var y = Math.Min((uint)(Math.Abs(v.Y) * 511.0f), 511);
            var z = Math.Min((uint)(Math.Abs(v.Z) * 511.0f), 511);
            var ux = (x & 0x1FF) + (sx ? 0x200 : 0);
            var uy = (y & 0x1FF) + (sy ? 0x200 : 0);
            var uz = (z & 0x1FF) + (sz ? 0x200 : 0);
            var uw = 0u;
            var u = uz + (ux << 10) + (uy << 20) + (uw << 30);
            return (uint)u;
        }
    }
}
