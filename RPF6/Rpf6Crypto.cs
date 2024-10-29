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
using System.ComponentModel;

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

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static Vector3 ToZXY(Vector3 vec)
        {
            var x = float.IsNaN(vec.X) ? 0.0f : vec.X;
            var y = float.IsNaN(vec.Y) ? 0.0f : vec.Y;
            var z = float.IsNaN(vec.Z) ? 0.0f : vec.Z;
            return new Vector3(z, x, y);
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static Vector4 ToZXY(Vector4 vec)
        {
            var x = float.IsNaN(vec.X) ? 0.0f : vec.X;
            var y = float.IsNaN(vec.Y) ? 0.0f : vec.Y;
            var z = float.IsNaN(vec.Z) ? 0.0f : vec.Z;
            var w = float.IsNaN(vec.W) ? 0.0f : vec.W;
            return new Vector4(z, x, y, w);
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static BoundingBox4 ToZXY(BoundingBox4 bb)
        {
            var newBB = new BoundingBox()
            {
                Minimum = ToZXY(bb.Min.XYZ()),
                Maximum = ToZXY(bb.Max.XYZ())
            };
            return new BoundingBox4(newBB);
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        public static BoundingBox4 ToXYZ(BoundingBox4 bb)
        {
            var newBB = new BoundingBox()
            {
                Minimum = ToXYZ(bb.Min.XYZ()),
                Maximum = ToXYZ(bb.Max.XYZ())
            };
            return new BoundingBox4(newBB);
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static BoundingBox4[] ToXYZ(BoundingBox4[] bb)
        {
            for (int i = 0; i < bb.Length; i++)
            {
                bb[i] = ToXYZ(bb[i]);
            }
            return bb;
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static Quaternion ToZXY(Quaternion quat)
        {
            return new Quaternion(quat.Z, quat.X, quat.Y, quat.W);
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static Matrix3x4 ToZXY(Matrix3x4 m)
        {
            m.Translation = ToZXY(m.Translation);
            m.Orientation = ToZXY(m.Orientation);
            return m;
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        public static Matrix3x4 ToXYZ(Matrix3x4 m, bool write = false)
        {
            var r1 = ToXYZ(m.Row1);
            var r2 = ToXYZ(m.Row2);
            var r3 = ToXYZ(m.Row3);
            r1.W = write ? NaN() : (float.IsNaN(r1.W) ? 0.0f : r1.W);
            r2.W = write ? NaN() : (float.IsNaN(r2.W) ? 0.0f : r2.W);
            r3.W = write ? NaN() : (float.IsNaN(r3.W) ? 0.0f : r3.W);
            return new Matrix3x4(r1, r2, r3);
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static Matrix3x4[] ToZXY(Matrix3x4[] m)
        {
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = ToZXY(m[i]);
            }
            return m;
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        public static Matrix3x4[] ToXYZ(Matrix3x4[] m, bool write = false)
        {
            if (m == null) return null;
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = ToXYZ(m[i], write);
            }
            return m;
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static Matrix4x4 ToZXY(Matrix4x4 m, bool write = false)
        {
            var m14 = write ? NaN() : (float.IsNaN(m.M14) ? 0.0f : m.M14);
            var m24 = write ? NaN() : (float.IsNaN(m.M24) ? 0.0f : m.M24);
            var m34 = write ? NaN() : (float.IsNaN(m.M34) ? 0.0f : m.M34);
            var m44 = write ? NaN() : (float.IsNaN(m.M44) ? 0.0f : m.M44);
            var translation = m.Translation;

            return new Matrix4x4(
                m.M11, m.M12, m.M13, m14,
                m.M21, m.M22, m.M23, m24,
                m.M31, m.M32, m.M33, m34,
                translation.Z, translation.X, translation.Y, m44
            );
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        public static Matrix4x4 ToXYZ(Matrix4x4 m, bool write = false)
        {
            var m14 = write ? NaN() : (float.IsNaN(m.M14) ? 0.0f : m.M14);
            var m24 = write ? NaN() : (float.IsNaN(m.M24) ? 0.0f : m.M24);
            var m34 = write ? NaN() : (float.IsNaN(m.M34) ? 0.0f : m.M34);
            var m44 = write ? NaN() : (float.IsNaN(m.M44) ? 0.0f : m.M44);
            var translation = m.Translation;

            return new Matrix4x4(
                m.M11, m.M12, m.M13, m14,
                m.M21, m.M22, m.M23, m24,
                m.M31, m.M32, m.M33, m34,
                translation.Y, translation.Z, translation.X, m44
            );
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        public static Matrix4x4[] ToZXY(Matrix4x4[] m)
        {
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = ToZXY(m[i]);
            }
            return m;
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        public static Matrix4x4[] ToXYZ(Matrix4x4[] m, bool write = false)
        {
            if (m == null) return null;
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = ToXYZ(m[i], write);
            }
            return m;
        }

        ///<summary>
        ///Converts a <see cref="System.Numerics.Vector3" /> from ZXY to XYZ format.
        ///</summary>
        ///<param name="vector">The input Vector3 in ZXY format.</param>
        ///<returns>A new Vector3 in XYZ format.</returns>
        public static Vector3 ToXYZ(Vector3 vector)
        {
            return new Vector3(vector.Y, vector.Z, vector.X);
        }

        ///<summary>
        ///Converts a <see cref="System.Numerics.Vector4" /> from ZXYW to XYZW format.
        ///</summary>
        ///<param name="vector">The input Vector4 in ZXYW format.</param>
        ///<returns>A new Vector4 in XYZW format.</returns>
        public static Vector4 ToXYZ(Vector4 vector)
        {
            return new Vector4(vector.Y, vector.Z, vector.X, (vector.W == 0.0f) ? NaN() : vector.W);
        }

        public static Vector2 Half2ToVector2(Half2 value)
        {
            return new Vector2((float)value.X, (float)value.Y);
        }

        public static Vector4 Half4ToVector4(Half4 value)
        {
            return new Vector4((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
        }

        ///<summary>
        ///Converts a <see cref="System.Numerics.Vector4" />[] from ZXYW to XYZW format.
        ///</summary>
        ///<param name="vector">The input Vector4[] in ZXYW format.</param>
        ///<returns>A new Vector4[] in XYZW format.</returns>
        public static Vector4[] ToXYZ(Vector4[] vector)
        {
            if (vector == null) return null;
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = ToXYZ(vector[i]);
            }
            return vector;
        }

        ///<summary>
        ///Swaps the axis and writes a <see cref="System.Numerics.Vector3" /> at the given offset in a buffer
        ///</summary>
        ///<param name="vec">The Vector3 to be written.</param>
        ///<param name="buffer">The buffer to write to.</param>
        ///<param name="offset">The offset in the buffer where writing starts.</param>
        ///<param name="zxy">Determines whether to swap the axis according to the ZXY (RDR1>CX) or YZX (CX>RDR1) convention. Default is true (ZXY).</param>
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

        ///<summary>
        ///Swaps the axis and writes a <see cref="System.Numerics.Vector4" /> at the given offset in a buffer
        ///</summary>
        ///<param name="vec">The Vector4 to be written.</param>
        ///<param name="buffer">The buffer to write to.</param>
        ///<param name="offset">The offset in the buffer where writing starts.</param>
        ///<param name="zxy">Determines whether to swap the axis according to the ZXY (RDR1>CX) or YZX (CX>RDR1) convention. Default is true (ZXY).</param>
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

        ///<summary>
        ///Rescales <see cref="CodeX.Core.Numerics.Half2" /> values (used for #vd terrain resources)
        ///</summary>
        ///<param name="val">The Half2 value to be rescaled.</param>
        ///<param name="scale">The scaling factor.</param>
        ///<returns>A new <see cref="Vector2" /> with the rescaled values.</returns>
        public static Vector2 RescaleHalf2(Half2 val, float scale)
        {
            return new Half2((float)val.X * scale, (float)val.Y * scale);
        }

        ///<summary>
        ///Reads & rescales UShort2N values
        ///</summary>
        ///<param name="buffer">The buffer containing the UShort2N values.</param>
        ///<param name="offset">The offset in the buffer where the UShort2N values start.</param>
        public static void ReadRescaleUShort2N(byte[] buffer, int offset)
        {
            var xBuf = BufferUtil.ReadArray<byte>(buffer, offset, 2);
            var yBuf = BufferUtil.ReadArray<byte>(buffer, offset + 2, 2);
            var xVal = BitConverter.ToUInt16(xBuf, 0) * 3.05185094e-005f;
            var yVal = BitConverter.ToUInt16(yBuf, 0) * 3.05185094e-005f;

            xVal *= 2.0f;
            yVal *= 2.0f;

            BufferUtil.WriteArray(buffer, offset, BitConverter.GetBytes((ushort)(xVal / 3.05185094e-005f)));
            BufferUtil.WriteArray(buffer, offset + 2, BitConverter.GetBytes((ushort)(yVal / 3.05185094e-005f)));
        }

        ///<summary>Creates a <see cref="CodeX.Core.Numerics.Colour" /> from a string representing RGB values, e.g., "255, 255, 255".</summary>
        public static Colour ParseRGBString(string rgbString)
        {
            string[] rgbValues = rgbString.Split(',');
            if (rgbValues.Length == 3
                && int.TryParse(rgbValues[0], out int red)
                && int.TryParse(rgbValues[1], out int green)
                && int.TryParse(rgbValues[2], out int blue))
            {
                return new Colour(red, green, blue);
            }
            return Colour.Red;
        }

        ///<summary>A hacky function to adjust the bounds for fragments, used in the prefabs.</summary>
        public static void ResizeBoundsForPeds(Piece piece)
        {
            if (piece == null) return;
            var min = ((Rsc6Drawable)piece).BoundingBoxMin.XYZ();
            var max = ((Rsc6Drawable)piece).BoundingBoxMax.XYZ();

            min = new Vector3(min.X, min.Y, min.Z - 3.0f);
            max = new Vector3(max.X, max.Y, max.Z - 3.0f);

            piece.BoundingBox = new BoundingBox(min, max);
            piece.BoundingSphere = new BoundingSphere(piece.BoundingBox.Center, piece.BoundingBox.Size.Length() * 0.5f);
        }

        ///<summary>Convert a <see cref="System.Numerics.Vector4" /> to a <see cref="System.Byte" />[].</summary>
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

        ///<summary>Checks if a specified value is present in an enum.</summary>
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

        ///<summary>Returns NaN as 0x0100807F (float.NaN = 0x0000C0FF).</summary>
        public static float NaN()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(0x7F800001), 0);
        }

        ///<summary>Returns a <see cref="System.Numerics.Vector4" /> with NaN values.</summary>
        public static Vector4 GetVec4NaN()
        {
            return new Vector4(NaN(), NaN(), NaN(), NaN());
        }

        ///<summary>Returns a <see cref="System.Numerics.Matrix4x4" /> with NaN values.</summary>
        public static Matrix4x4 GetMatrix4x4NaN()
        {
            return new Matrix4x4(NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN(), NaN());
        }

        /// <summary>
        /// Converts a floating point number into a fixed point number.
        /// </summary>
        /// <remarks>
        /// <format type="text/markdown">
        /// <![CDATA[It's multiplying the float by a constant value and discarding the extra bits (Vector3::Pack1010102)]]>
        /// </format>
        /// </remarks>
        public static uint PackFixedPoint(float value, uint size, uint shift)
        {
            float scale = ((1u << (int)(size - 1)) - 1);
            return ((uint)(value * scale) & ((1u << (int)size) - 1)) << (int)shift;
        }

        ///<summary>Converts a <see cref="System.Numerics.Vector4" /> to the Dec3N format.</summary>
        public static uint GetDec3N(Vector4 val, bool zxy = true)
        {
            var wPack = (val.W > 0.5f) ? 1 << 30 : (val.W < -0.5f) ? -1 << 30 : 0;
            if (zxy) //XYZ > ZXY (RDR1 to CX)
                return PackFixedPoint(val.Z, 10, 0) | PackFixedPoint(val.X, 10, 10) | PackFixedPoint(val.Y, 10, 20) | (uint)wPack;
            else     //ZXY > XYZ (CX to RDR1)
                return PackFixedPoint(val.Y, 10, 0) | PackFixedPoint(val.Z, 10, 10) | PackFixedPoint(val.X, 10, 20) | (uint)wPack;
        }

        /// <summary>
        /// Transforms an array of items of type T from XYZ to ZXY space. This method should be used for unmanaged types only.
        /// </summary>
        /// <typeparam name="T">The type of items in the array. Must be an unmanaged type.</typeparam>
        /// <param name="items">The array of items to transform.</param>
        public static void TransformToZXY<T>(T[] items) where T : unmanaged
        {
            if (items == null) return;
            if (typeof(T) == typeof(Vector4))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToZXY((Vector4)(object)items[i]);
                }
            }
            else if(typeof(T) == typeof(BoundingBox4))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToZXY((BoundingBox4)(object)items[i]);
                }
            }
            else if (typeof(T) == typeof(Vector3))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToZXY((Vector3)(object)items[i]);
                }
            }
            else if (typeof(T) == typeof(Matrix3x4))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToZXY((Matrix3x4)(object)items[i]);
                }
            }
            else if (typeof(T) == typeof(Matrix4x4))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToZXY((Matrix4x4)(object)items[i]);
                }
            }
        }


        /// <summary>
        /// Transforms an array of items of type T from ZXY to XYZ space. This method should be used for unmanaged types only.
        /// </summary>
        /// <typeparam name="T">The type of items in the array. Must be an unmanaged type.</typeparam>
        /// <param name="items">The array of items to transform.</param>
        public static void TransformFromZXY<T>(T[] items) where T : unmanaged
        {
            if (items == null) return;
            if (typeof(T) == typeof(Vector4))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToXYZ((Vector4)(object)items[i]);
                }
            }
            else if(typeof(T) == typeof(BoundingBox4))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToXYZ((BoundingBox4)(object)items[i]);
                }
            }
            else if (typeof(T) == typeof(Vector3))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToXYZ((Vector3)(object)items[i]);
                }
            }
            else if (typeof(T) == typeof(Matrix3x4))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToXYZ((Matrix3x4)(object)items[i]);
                }
            }
            else if (typeof(T) == typeof(Matrix4x4))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (T)(object)ToXYZ((Matrix4x4)(object)items[i]);
                }
            }
        }

        ///<summary>rage::intA<2></summary>
        public struct IntA
        {
            public int Int1; //m_ints[0]
            public int Int2; //m_ints[1]

            public override readonly string ToString()
            {
                return $"Int1: {Int1}, Int2: {Int2}";
            }
        }
    }
}