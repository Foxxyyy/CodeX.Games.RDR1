using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.IO.Compression;
using System.IO;
using System.Security.Cryptography;
using Zstandard.Net;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.RPF6
{
    public static class Rpf6Crypto
    {
        public static Aes AesAlg;
        public static byte[] AES_KEY = new byte[32]
        {
            0xB7, 0x62, 0xDF, 0xB6, 0xE2, 0xB2, 0xC6, 0xDE, 0xAF, 0x72, 0x2A, 0x32, 0xD2, 0xFB, 0x6F, 0x0C, 0x98, 0xA3, 0x21, 0x74, 0x62, 0xC9, 0xC4, 0xED, 0xAD, 0xAA, 0x2E, 0xD0, 0xDD, 0xF9, 0x2F, 0x10
        };

        public static bool Init(string folder)
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
    }
}
