using System;
using System.IO.Compression;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using CodeX.Core.Utilities;
using System.Numerics;
using CodeX.Core.Numerics;
using CodeX.Core.Engine;
using CodeX.Games.RDR1.RSC6;
using MI = System.Runtime.CompilerServices.MethodImplAttribute;
using MO = System.Runtime.CompilerServices.MethodImplOptions;

namespace CodeX.Games.RDR1.RPF6
{
    public static class Rpf6Crypto
    {
        public static Aes AesAlg;
        public static byte[] AES_KEY;
        public static readonly int[] AES_KEY_OFFSETS = new int[] { 0x22a2300, 0x2293500 };//TODO: more offsets?
        public static readonly byte[] AES_KEY_HASH = new byte[20] { 0x87, 0x86, 0x24, 0x97, 0xEE, 0x46, 0x85, 0x53, 0x72, 0xB5, 0x1C, 0x7A, 0x32, 0x4A, 0x2B, 0xB5, 0xCD, 0x66, 0xF4, 0xAF };

        public static bool Init(string folder)
        {
            if (AES_KEY == null)
            {
                if (FindKey(folder) == false)
                {
                    return false;
                }
            }

            AesAlg = Aes.Create();
            AesAlg.BlockSize = 128;
            AesAlg.KeySize = 256;
            AesAlg.Mode = CipherMode.ECB;
            AesAlg.Key = AES_KEY;
            AesAlg.IV = new byte[16];
            AesAlg.Padding = PaddingMode.None;
            return true;
        }

        private static bool FindKey(string folder)
        {
            byte[] exedata = File.ReadAllBytes(folder + "\\rdr.exe");
            if (exedata == null) return false;
            AES_KEY = SearchUtil.HashSearch(exedata, AES_KEY_OFFSETS, 32, AES_KEY_HASH);
            if (AES_KEY != null) return true;
            using (var exestr = new MemoryStream(exedata))
            {
                AES_KEY = SearchUtil.HashSearch(exestr, AES_KEY_HASH, 32, 1048576, 4);
                return (AES_KEY != null);
            }
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


        public static byte[] DecompressZStandard(byte[] compressedData) //ZStandard
        {
            try
            {
                using (var memoryStream = new MemoryStream(compressedData))
                using (var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Decompress))
                using (var temp = new MemoryStream())
                {
                    compressionStream.CopyTo(temp);
                    return temp.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        public static byte[] CompressZStandard(byte[] decompressedData) //ZStandard
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                using (var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Compress))
                {
                    compressionStream.Write(decompressedData, 0, decompressedData.Length);
                    compressionStream.Close();
                    return memoryStream.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }












        ///<summary>Swap the axis from XYZ to ZXY</summary>
        [MI(MO.AggressiveInlining)] public static Vector3 ToZXY(Vector3 vec)
        {
            var x = float.IsNaN(vec.X) ? 0.0f : vec.X;
            var y = float.IsNaN(vec.Y) ? 0.0f : vec.Y;
            var z = float.IsNaN(vec.Z) ? 0.0f : vec.Z;
            return new Vector3(z, x, y);
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        [MI(MO.AggressiveInlining)] public static Vector4 ToZXY(Vector4 vec)
        {
            var x = float.IsNaN(vec.X) ? 0.0f : vec.X;
            var y = float.IsNaN(vec.Y) ? 0.0f : vec.Y;
            var z = float.IsNaN(vec.Z) ? 0.0f : vec.Z;
            var w = float.IsNaN(vec.W) ? 0.0f : vec.W;
            return new Vector4(z, x, y, w);
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        [MI(MO.AggressiveInlining)] public static BoundingBox4 ToZXY(BoundingBox4 bb)
        {
            var newBB = new BoundingBox()
            {
                Minimum = ToZXY(bb.Min.XYZ()),
                Maximum = ToZXY(bb.Max.XYZ())
            };
            return new BoundingBox4(newBB);
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        [MI(MO.AggressiveInlining)] public static BoundingBox4 ToXYZ(BoundingBox4 bb)
        {
            var newBB = new BoundingBox()
            {
                Minimum = ToXYZ(bb.Min.XYZ()),
                Maximum = ToXYZ(bb.Max.XYZ())
            };
            return new BoundingBox4(newBB);
        }


        ///<summary>Swap the axis from XYZ to ZXY</summary>
        [MI(MO.AggressiveInlining)] public static Quaternion ToZXY(Quaternion quat)
        {
            return new Quaternion(quat.Z, quat.X, quat.Y, quat.W);
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        [MI(MO.AggressiveInlining)] public static Matrix3x4 ToZXY(Matrix3x4 m)
        {
            m.Translation = ToZXY(m.Translation);
            m.Orientation = ToZXY(m.Orientation);
            return m;
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        [MI(MO.AggressiveInlining)] public static Matrix3x4 ToXYZ(Matrix3x4 m, bool write = false)
        {
            var r1 = ToXYZ(m.Row1);
            var r2 = ToXYZ(m.Row2);
            var r3 = ToXYZ(m.Row3);
            r1.W = write ? FNaN : (float.IsNaN(r1.W) ? 0.0f : r1.W);
            r2.W = write ? FNaN : (float.IsNaN(r2.W) ? 0.0f : r2.W);
            r3.W = write ? FNaN : (float.IsNaN(r3.W) ? 0.0f : r3.W);
            return new Matrix3x4(r1, r2, r3);
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        [MI(MO.AggressiveInlining)] public static Matrix3x4[] ToXYZ(Matrix3x4[] m, bool write = false)
        {
            if (m == null) return null;
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = ToXYZ(m[i], write);
            }
            return m;
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        [MI(MO.AggressiveInlining)] public static Matrix4x4 ToZXY(Matrix4x4 m, bool write = false)
        {
            var m14 = write ? FNaN : (float.IsNaN(m.M14) ? 0.0f : m.M14);
            var m24 = write ? FNaN : (float.IsNaN(m.M24) ? 0.0f : m.M24);
            var m34 = write ? FNaN : (float.IsNaN(m.M34) ? 0.0f : m.M34);
            var m44 = write ? FNaN : (float.IsNaN(m.M44) ? 0.0f : m.M44);
            var translation = m.Translation;

            return new Matrix4x4(
                m.M11, m.M12, m.M13, m14,
                m.M21, m.M22, m.M23, m24,
                m.M31, m.M32, m.M33, m34,
                translation.Z, translation.X, translation.Y, m44
            );
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        [MI(MO.AggressiveInlining)] public static Matrix4x4 ToXYZ(Matrix4x4 m, bool write = false)
        {
            var m14 = write ? FNaN : (float.IsNaN(m.M14) ? 0.0f : m.M14);
            var m24 = write ? FNaN : (float.IsNaN(m.M24) ? 0.0f : m.M24);
            var m34 = write ? FNaN : (float.IsNaN(m.M34) ? 0.0f : m.M34);
            var m44 = write ? FNaN : (float.IsNaN(m.M44) ? 0.0f : m.M44);
            var translation = m.Translation;

            return new Matrix4x4(
                m.M11, m.M12, m.M13, m14,
                m.M21, m.M22, m.M23, m24,
                m.M31, m.M32, m.M33, m34,
                translation.Y, translation.Z, translation.X, m44
            );
        }

        ///<summary>Swap the axis from XYZ to ZXY</summary>
        [MI(MO.AggressiveInlining)] public static Matrix4x4[] ToZXY(Matrix4x4[] m)
        {
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = ToZXY(m[i]);
            }
            return m;
        }

        ///<summary>Swap the axis from ZXY to XYZ</summary>
        [MI(MO.AggressiveInlining)] public static Matrix4x4[] ToXYZ(Matrix4x4[] m, bool write = false)
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
        [MI(MO.AggressiveInlining)] public static Vector3 ToXYZ(Vector3 vector)
        {
            return new Vector3(vector.Y, vector.Z, vector.X);
        }

        ///<summary>
        ///Converts a <see cref="System.Numerics.Vector4" /> from ZXYW to XYZW format.
        ///</summary>
        ///<param name="vector">The input Vector4 in ZXYW format.</param>
        ///<returns>A new Vector4 in XYZW format.</returns>
        [MI(MO.AggressiveInlining)] public static Vector4 ToXYZ(Vector4 vector)
        {
            return new Vector4(vector.Y, vector.Z, vector.X, (vector.W == 0.0f) ? FNaN : vector.W);
        }

        [MI(MO.AggressiveInlining)] public static Vector2 Half2ToVector2(Half2 value)
        {
            return new Vector2((float)value.X, (float)value.Y);
        }

        [MI(MO.AggressiveInlining)] public static Vector4 Half4ToVector4(Half4 value)
        {
            return new Vector4((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
        }

        ///<summary>
        ///Converts a <see cref="System.Numerics.Vector4" />[] from ZXYW to XYZW format.
        ///</summary>
        ///<param name="vector">The input Vector4[] in ZXYW format.</param>
        ///<returns>A new Vector4[] in XYZW format.</returns>
        [MI(MO.AggressiveInlining)] public static Vector4[] ToXYZ(Vector4[] vector)
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
        [MI(MO.AggressiveInlining)] public static void WriteVector3AtIndex(Vector3 vec, byte[] buffer, int offset, bool zxy = true)
        {
            if (zxy) //XYZ > ZXY (RDR1 to CX)
            {
                var vzxy = new Vector3(vec.Z, vec.X, vec.Y);
                BufferUtil.WriteVector3(buffer, offset, ref vzxy);
            }
            else //XYZ > YZX (CX to RDR1)
            {
                var vyzx = new Vector3(vec.Y, vec.Z, vec.X);
                BufferUtil.WriteVector3(buffer, offset, ref vyzx);
            }
        }

        ///<summary>
        ///Swaps the axis and writes a <see cref="System.Numerics.Vector4" /> at the given offset in a buffer
        ///</summary>
        ///<param name="vec">The Vector4 to be written.</param>
        ///<param name="buffer">The buffer to write to.</param>
        ///<param name="offset">The offset in the buffer where writing starts.</param>
        ///<param name="zxy">Determines whether to swap the axis according to the ZXY (RDR1>CX) or YZX (CX>RDR1) convention. Default is true (ZXY).</param>
        [MI(MO.AggressiveInlining)] public static void WriteVector4AtIndex(Vector4 vec, byte[] buffer, int offset, bool zxy = true)
        {
            if (float.IsNaN(vec.W))
            {
                vec.W = 0.0f;
            }
            if (zxy) //XYZ > ZXY (RDR1 to CX)
            {
                var vzxyw = new Vector4(vec.Z, vec.X, vec.Y, vec.W);
                BufferUtil.WriteVector4(buffer, offset, ref vzxyw);
            }
            else //XYZ > YZX (CX to RDR1)
            {
                var vyzxw = new Vector4(vec.Y, vec.Z, vec.X, vec.W);
                BufferUtil.WriteVector4(buffer, offset, ref vyzxw);
            }
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








        /// <summary>
        /// Converts a floating point number into a fixed point number.
        /// </summary>
        /// <remarks>
        /// <format type="text/markdown">
        /// <![CDATA[It's multiplying the float by a constant value and discarding the extra bits (Vector3::Pack1010102)]]>
        /// </format>
        /// </remarks>
        [MI(MO.AggressiveInlining)] public static uint PackFixedPoint(float value, uint size, uint shift)
        {
            float scale = ((1u << (int)(size - 1)) - 1);
            return ((uint)(value * scale) & ((1u << (int)size) - 1)) << (int)shift;
        }

        ///<summary>Converts a <see cref="System.Numerics.Vector4" /> to the Dec3N format.</summary>
        [MI(MO.AggressiveInlining)] public static uint GetDec3N(Vector4 val, bool zxy = true)
        {
            var wPack = (val.W > 0.5f) ? 1 << 30 : (val.W < -0.5f) ? -1 << 30 : 0;
            if (zxy) //XYZ > ZXY (RDR1 to CX)
                return PackFixedPoint(val.Z, 10, 0) | PackFixedPoint(val.X, 10, 10) | PackFixedPoint(val.Y, 10, 20) | (uint)wPack;
            else     //ZXY > XYZ (CX to RDR1)
                return PackFixedPoint(val.Y, 10, 0) | PackFixedPoint(val.Z, 10, 10) | PackFixedPoint(val.X, 10, 20) | (uint)wPack;
        }

        [MI(MO.AggressiveInlining)] public static Vector4 Dec3NToVector4(uint u)
        {
            var ux = (int)((u & 0x3FF) << 22);
            var uy = (int)(((u >> 10) & 0x3FF) << 22);
            var uz = (int)(((u >> 20) & 0x3FF) << 22);
            var uw = (int)u;
            var fx = (float)(ux >> 22);
            var fy = (float)(uy >> 22);
            var fz = (float)(uz >> 22);
            var fw = (float)(uw >> 30);
            var scale = 0.001956947162f;
            var v = new Vector4(fx * scale, fy * scale, fz * scale, fw);
            return v;
        }

        [MI(MO.AggressiveInlining)] public static uint Vector4ToDec3N(in Vector4 v)
        {
            var sx = (v.X >= 0.0f);
            var sy = (v.Y >= 0.0f);
            var sz = (v.Z >= 0.0f);
            var sw = (v.W >= 0.0f);
            var x = Math.Min((uint)(Math.Abs(v.X) * 511.0f), 511);
            var y = Math.Min((uint)(Math.Abs(v.Y) * 511.0f), 511);
            var z = Math.Min((uint)(Math.Abs(v.Z) * 511.0f), 511);
            var w = (v.W == 0.0f) ? 0 : (v.W == 1.0f) ? 1 : (v.W == -1.0f) ? 2 : 3;
            var ux = ((sx ? x : ~x) & 0x1FF) + (sx ? 0x200 : 0);
            var uy = ((sy ? y : ~y) & 0x1FF) + (sy ? 0x200 : 0);
            var uz = ((sz ? z : ~z) & 0x1FF) + (sz ? 0x200 : 0);
            var uw = ((sw ? w : ~w) & 0x3) + (sw ? 0x200 : 0);
            var u = ux + (uy << 10) + (uz << 20) + (uw << 30);
            return (uint)u;
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



        ///<summary>Returns a <see cref="System.Numerics.Vector4" /> with NaN values.</summary>
        [MI(MO.AggressiveInlining)] public static Vector4 GetVec4NaN()
        {
            return new Vector4(FNaN, FNaN, FNaN, FNaN);
        }

        ///<summary>Returns a <see cref="System.Numerics.Matrix4x4" /> with NaN values.</summary>
        [MI(MO.AggressiveInlining)] public static Matrix4x4 GetMatrix4x4NaN()
        {
            return new Matrix4x4(FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN, FNaN);
        }

        public static readonly float FNaN = BufferUtil.GetUintFloat(0x7F800001); //NaN as 0x0100807F (float.NaN = 0x0000C0FF).

    }
}