using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using CodeX.Core.Numerics;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Net;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6DataReader : BlockReader
    {
        private const ulong VIRTUAL_BASE = 0x50000000;
        private const ulong PHYSICAL_BASE = 0x60000000;

        public Rpf6ResourceFileEntry FileEntry;
        public DataEndianess Endianess;
        public int VirtualSize;
        public int PhysicalSize;

        public int Offset => GetDataOffset();

        public Rsc6DataReader(Rpf6ResourceFileEntry entry, byte[] data, DataEndianess endianess = DataEndianess.LittleEndian)
        {
            FileEntry = entry;
            Endianess = endianess;
            Data = data;
            VirtualSize = entry.VirtualSize;
            PhysicalSize = entry.PhysicalSize;
            Position = 0x50000000;
        }

        public override int GetDataOffset()
        {
            if ((Position & VIRTUAL_BASE) == VIRTUAL_BASE)
            {
                return (int)(Position & 0x0FFFFFFF);
            }
            if ((Position & PHYSICAL_BASE) == PHYSICAL_BASE)
            {
                return (int)(Position & 0x1FFFFFFF) + VirtualSize;
            }
            throw new Exception("Invalid Position. Possibly the file is corrupted.");
        }

        public bool ReadBoolean()
        {
            return ReadByte() > 0;
        }

        public new short ReadInt16()
        {
            var v = BufferUtil.ReadShort(Data, GetDataOffset());
            Position += 2;
            return (Endianess == DataEndianess.LittleEndian) ? v : Rpf6Crypto.Swap(v);
        }

        public new ushort ReadUInt16()
        {
            var v = BufferUtil.ReadUshort(Data, GetDataOffset());
            Position += 2;
            return (Endianess == DataEndianess.LittleEndian) ? v : Rpf6Crypto.Swap(v);
        }

        public new int ReadInt32()
        {
            var v = BufferUtil.ReadInt(Data, GetDataOffset());
            Position += 4;
            return (Endianess == DataEndianess.LittleEndian) ? v : Rpf6Crypto.Swap(v);
        }

        public new uint ReadUInt32()
        {
            var v = BufferUtil.ReadUint(Data, GetDataOffset());
            Position += 4;
            return (Endianess == DataEndianess.LittleEndian) ? v : Rpf6Crypto.Swap(v);
        }

        public new long ReadInt64()
        {
            var v = BufferUtil.ReadLong(Data, GetDataOffset());
            Position += 8;
            return (Endianess == DataEndianess.LittleEndian) ? v : Rpf6Crypto.Swap(v);
        }

        public new ulong ReadUInt64()
        {
            var v = BufferUtil.ReadUlong(Data, GetDataOffset());
            Position += 8;
            return (Endianess == DataEndianess.LittleEndian) ? v : Rpf6Crypto.Swap(v);
        }

        public new float ReadSingle()
        {
            var v = BufferUtil.ReadSingle(Data, GetDataOffset());
            Position += 4;
            return (Endianess == DataEndianess.LittleEndian) ? v : Rpf6Crypto.Swap(v);
        }

        public string ReadString()
        {
            var bytes = new List<byte>();
            var o = GetDataOffset();
            var temp = Data[o];
            uint i = 1;
            while (temp != 0)
            {
                bytes.Add(temp);
                temp = Data[o + i];
                i++;
            }
            Position += i;
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public new Half ReadHalf()
        {
            return BufferUtil.GetUshortHalf(ReadUInt16());
        }

        public Half2 ReadHalf2()
        {
            byte[] buffer = ReadBytes(4);
            return BufferUtil.ReadStruct<Half2>(buffer, 0);
        }

        public Half4 ReadHalf4()
        {
            byte[] buffer = ReadBytes(8);
            return BufferUtil.ReadStruct<Half4>(buffer, 0);
        }

        public new Vector2 ReadVector2()
        {
            var v = BufferUtil.ReadVector2(Data, GetDataOffset());
            Position += 8;
            return v;
        }

        public Vector2[] ReadVector2Arr(int count)
        {
            var vectors = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                vectors[i] = BufferUtil.ReadVector2(Data, GetDataOffset());
                Position += 8;
            }
            return vectors;
        }

        public new Vector3 ReadVector3()
        {
            var v = BufferUtil.ReadVector3(Data, GetDataOffset());
            Position += 12;
            return new Vector3(v.Z, v.X, v.Y);
        }

        public Vector3[] ReadVector3Arr(int count)
        {
            var vectors = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                vectors[i] = ReadVector3();
            }
            return vectors;
        }

        public Vector4 ReadVector4(bool toZXYW = true)
        {
            var v = BufferUtil.ReadVector4(Data, GetDataOffset());
            Position += 16;

            if (float.IsNaN(v.W))
            {
                v = new Vector4(v.XYZ(), 0.0f);
            }
            return toZXYW ? new Vector4(v.Z, v.X, v.Y, v.W) : v;
        }

        public Vector4[] ReadVector4Arr(int count, bool toZXYW = true)
        {
            var vectors = new Vector4[count];
            for (int i = 0; i < count; i++)
            {
                vectors[i] = ReadVector4(toZXYW);
            }
            return vectors;
        }

        public new Matrix4x4 ReadMatrix4x4()
        {
            var matrix = BufferUtil.ReadMatrix4x4(Data, GetDataOffset());
            Position += 64;

            if (float.IsNaN(matrix.M14))
                matrix.M14 = 0.0f;
            if (float.IsNaN(matrix.M24))
                matrix.M24 = 0.0f;
            if (float.IsNaN(matrix.M34))
                matrix.M34 = 0.0f;
            if (float.IsNaN(matrix.M44))
                matrix.M44 = 0.0f;
            return Rpf6Crypto.ToZXY(matrix);
        }

        public BoundingBox4 ReadBoundingBox4()
        {
            var bb = new BoundingBox4
            {
                Min = BufferUtil.ReadVector4(Data, GetDataOffset()),
                Max = BufferUtil.ReadVector4(Data, GetDataOffset())
            };
            Position += 32;
            return Rpf6Crypto.ToZXY(bb);
        }

        public ushort[] ReadUInt16Arr(int count)
        {
            var array = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BufferUtil.ReadUshort(Data, GetDataOffset());
                Position += 2;
            }
            return array;
        }

        public uint[] ReadUInt32Arr(int count)
        {
            var array = new uint[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BufferUtil.ReadUint(Data, GetDataOffset());
                Position += 4;
            }
            return array;
        }

        public T ReadBlock<T>(Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            if (Position == 0) return default(T);
            if (BlockPool.TryGetValue(Position, out var exitem))
            {
                if (exitem is T exblock)
                {
                    Position += exblock.BlockLength;
                    return exblock;
                }
            }
            var block = (createFunc != null) ? createFunc(this) : new T();
            BlockPool[Position] = block;
            block.FilePosition = Position;
            block.Read(this);
            return block;
        }

        public T ReadBlock<T>(ulong position, Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            if (position == 0) return default(T);
            var p = Position;
            Position = position;
            var b = ReadBlock<T>(createFunc);
            Position = p;
            return b;
        }

        public Rsc6Ptr<T> ReadPtr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            var ptr = new Rsc6Ptr<T>();
            ptr.Read(this, createFunc);
            return ptr;
        }

        public Rsc6PtrUnmanaged<T> ReadPtr<T>() where T : unmanaged
        {
            var ptr = new Rsc6PtrUnmanaged<T>();
            ptr.ReadPtr(this);
            return ptr;
        }

        public Rsc6PtrUnmanaged<T> ReadItem<T>(Rsc6PtrUnmanaged<T> ptr) where T : unmanaged
        {
            ptr.ReadItem(this);
            return ptr;
        }

        public Rsc6PtrUnmanaged<T> ReadPtrUnmanaged<T>() where T : unmanaged
        {
            var ptr = new Rsc6PtrUnmanaged<T>();
            ptr.Read(this);
            return ptr;
        }

        public Rsc6PtrArr<T> ReadPtrArr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            var arr = new Rsc6PtrArr<T>();
            arr.Read(this, createFunc);
            return arr;
        }

        public Rsc6PtrToPtrArr<T> ReadPtrToItem<T>() where T : Rsc6Block, new()
        {
            var ptr = new Rsc6PtrToPtrArr<T>();
            ptr.ReadPtr(this);
            return ptr;
        }

        public Rsc6PtrToPtrArr<T> ReadItems<T>(Rsc6PtrToPtrArr<T> arr, Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            arr.ReadItems(this, createFunc);
            return arr;
        }

        public Rsc6Arr<T> ReadArr<T>(bool size32 = false) where T : unmanaged
        {
            var arr = new Rsc6Arr<T>();
            arr.Read(this, size32);
            return arr;
        }

        public Rsc6PackedArr ReadPackedArr()
        {
            var arr = new Rsc6PackedArr();
            arr.Read(this);
            return arr;
        }

        public Rsc6ManagedArr<T> ReadArr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            var arr = new Rsc6ManagedArr<T>();
            arr.Read(this, createFunc);
            return arr;
        }

        public Rsc6AtMapArr<T> ReadAtMapArr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            var mapArr = new Rsc6AtMapArr<T>();
            mapArr.Read(this, createFunc);
            return mapArr;
        }

        public Rsc6ManagedSizedArr<T> ReadSizedArrPtr<T>() where T : Rsc6Block, new()
        {
            var arr = new Rsc6ManagedSizedArr<T>();
            arr.ReadArr(this);
            return arr;
        }

        public Rsc6ManagedSizedArr<T> ReadSizedArrItems<T>(Rsc6ManagedSizedArr<T> arr, ushort size, Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            arr.ReadItems(this, size, createFunc);
            return arr;
        }

        public Rsc6PoolArr<T> ReadPoolArr<T>() where T : unmanaged
        {
            var arr = new Rsc6PoolArr<T>();
            arr.Read(this);
            return arr;
        }

        public Rsc6RawLst<T> ReadRawLstPtr<T>() where T : Rsc6Block, new()
        {
            var arr = new Rsc6RawLst<T>();
            arr.ReadPtr(this);
            return arr;
        }

        public Rsc6RawLst<T> ReadRawLstItems<T>(Rsc6RawLst<T> arr, uint count) where T : Rsc6Block, new()
        {
            arr.ReadItems(this, count);
            return arr;
        }

        public Rsc6RawArr<T> ReadRawArrPtr<T>(int virtualSize = -1) where T : unmanaged
        {
            var arr = new Rsc6RawArr<T>();
            arr.ReadPtr(this);

            if (virtualSize != -1)
                arr.Position += (ulong)virtualSize;

            return arr;
        }

        public Rsc6RawArr<T> ReadRawArrItems<T>(Rsc6RawArr<T> arr, uint count) where T : unmanaged
        {
            arr.ReadItems(this, count);
            return arr;
        }

        public Rsc6RawPtrArr<T> ReadRawPtrArrPtr<T>() where T : Rsc6Block, new()
        {
            var arr = new Rsc6RawPtrArr<T>();
            arr.ReadPtr(this);
            return arr;
        }

        public Rsc6RawPtrArr<T> ReadRawPtrArrItem<T>(Rsc6RawPtrArr<T> arr, uint count, Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            arr.ReadItems(this, count, createFunc);
            return arr;
        }

        public Rsc6Ptr<T> ReadPtrPtr<T>() where T : Rsc6Block, new()
        {
            var ptr = new Rsc6Ptr<T>();
            ptr.ReadPtr(this);
            return ptr;
        }

        public Rsc6Ptr<T> ReadPtrItem<T>(Rsc6Ptr<T> ptr, Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            ptr.ReadItem(this, createFunc);
            return ptr;
        }

        public Rsc6Str ReadStr()
        {
            var str = new Rsc6Str();
            str.Read(this);
            return str;
        }

        public Rsc6PtrStr ReadPtrStr(uint pad = 0)
        {
            var str = new Rsc6PtrStr();
            str.Read(this, pad);
            return str;
        }

        public static BlockAnalyzer Analyze<T>(Rpf6ResourceFileEntry rfe, byte[] data, Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            var r = new Rsc6DataReader(rfe, data);
            var block = r.ReadBlock(createFunc);
            var analyzer = new BlockAnalyzer(r, rfe);
            return analyzer;
        }
    }

    public class Rsc6DataWriter : BlockWriter
    {
        public HashSet<object> PhysicalBlocks = new HashSet<object>();

        protected override bool ValidatePaging(uint[] pageCounts, uint baseShift, uint pageCount, ulong currentPosition, ulong currentTotalSize)
        {
            return true;
        }

        protected override ulong GetPointer(BuilderBlock block)
        {
            if (block == null) return 0;
            if (block.Block == null) return 0;
            if (this.PhysicalBlocks.Contains(block.Block))
            {
                return 0x60000000 + block.Position;
            }
            return 0x50000000 + block.Position;
        }

        public byte[] Build(uint version)
        {
            var blocks = new Dictionary<object, BuilderBlock>();
            var vblocks = new List<BuilderBlock>();
            var pblocks = new List<BuilderBlock>();

            foreach (var block in BlockList)
            {
                this.BlockDataDict.TryGetValue(block, out var bdata);

                if (bdata == null)
                    continue;

                var bblock = new BuilderBlock(block, (ulong)bdata.Length);
                blocks[block] = bblock;

                if (this.PhysicalBlocks.Contains(block))
                    pblocks.Add(bblock);
                else
                    vblocks.Add(bblock);

                if ((block is Rsc6Block) || (block is Array))
                {
                    bblock.Align = 16;
                }
            }
            if (vblocks.Count > 0)
            {
                vblocks[0].IsRoot = true;
            }

            var vpages = BuildPages(vblocks, 9, 5, 32);
            var ppages = BuildPages(pblocks, 9, 2, 32);
            var vlen = vpages.TotalSize;
            var plen = ppages.TotalSize;
            var tlen = vlen + plen;
            var data = new byte[tlen];
            Array.Fill(data, (byte)0xCD);

            BuildPointers(blocks);
            BuildData(vblocks, data, 0);
            BuildData(pblocks, data, vlen);

            var flags = new FlagInfo
            {
                IsCompressed = true,
                IsResource = true,
                IsExtendedFlags = false
            };

            int flag = flags.GetFlags((int)vlen, (int)plen);
            flags.IsExtendedFlags = false;

            if (flags.IsExtendedFlags)
                flags.SetTotalSize((int)vlen, (int)plen);
            else
                flags.RSC05_SetMemSizes((int)vlen, (int)plen);

            var compressed = Rpf6Crypto.CompressZStandard(data);
            var output = Rpf6ResourceFileEntry.AddResourceHeader(compressed, version, flag, flags.Flag2, flags);

            return output;
        }

        private new void BuildPointers(Dictionary<object, BuilderBlock> blocks)
        {
            foreach (var pref in this.PointerRefs)
            {
                if (pref.Data == null || pref.Object == null) continue;
                if (blocks.TryGetValue(pref.Object, out var bblock) == false)
                {
                    if (pref.Object is Rsc6BoneData bobject)
                    {
                        var kv = blocks.FirstOrDefault(e => e.Key is Rsc6SkeletonBoneData data);
                        if (kv.Value != null)
                        {
                            bblock = kv.Value;
                        }
                    }
                }
                var ptr = GetPointer(bblock);
                BufferUtil.WriteUint(pref.Data, (int)pref.Pos, (uint)(ptr + pref.EmbedOffset));
            }
        }

        public void WriteBlock<T>(T block) where T : Rsc6Block, new()
        {
            if (block == null)
                return;

            var exdata = Data;
            var expos = Position;
            var size = block.BlockLength;

            this.Data = new byte[size];
            this.Position = 0;

            AddBlock(block, Data);
            block.Write(this);

            if (block.IsPhysical)
            {
                this.PhysicalBlocks.Add(block);
            }
            this.Data = exdata;
            this.Position = expos;
        }

        public void WriteBlocks<T>(T[] blocks) where T : Rsc6Block, new()
        {
            if (blocks == null)
                return;
            if (blocks.Length == 0)
                return;

            var b0 = blocks[0];
            var bs = (int)(b0?.BlockLength ?? 0);

            if (bs == 0)
                return;

            var exdata = Data;
            var expos = Position;
            var size = blocks.Length * bs;

            this.Data = new byte[size];
            this.Position = 0;
            this.AddBlock(blocks, Data);

            if (b0.IsPhysical)
            {
                this.PhysicalBlocks.Add(blocks);
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block == null) continue;
                this.Position = (ulong)(i * bs);
                block.Write(this);
            }
            this.Data = exdata;
            this.Position = expos;
        }

        public void WriteBoolean(bool value)
        {
            this.WriteByte(value ? (byte)1 : (byte)0);
        }

        public new void WriteHalf(Half value)
        {
            byte[] buffer = new byte[2];
            BufferUtil.WriteStruct(buffer, 0, ref value);
            this.WriteBytes(buffer);
        }

        public void WriteHalf2(Half2 value)
        {
            byte[] buffer = new byte[4];
            BufferUtil.WriteStruct(buffer, 0, ref value);
            this.WriteBytes(buffer);
        }

        public void WriteHalf4(Half4 value)
        {
            byte[] buffer = new byte[8];
            BufferUtil.WriteStruct(buffer, 0, ref value);
            this.WriteBytes(buffer);
        }

        public void WriteInt32Array(int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                this.WriteInt32(arr[i]);
            }
        }

        public void WriteUInt32Array(uint[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                this.WriteUInt32(arr[i]);
            }
        }

        public void WriteArr<T>(Rsc6Arr<T> arr, bool size32 = false) where T : unmanaged
        {
            arr.Write(this, size32);
        }

        public void WritePackedArr(Rsc6PackedArr arr)
        {
            arr.Write(this);
        }

        public void WriteArr<T>(Rsc6ManagedArr<T> arr) where T : Rsc6Block, new()
        {
            arr.Write(this);
        }

        public void WriteAtMapArr<T>(Rsc6AtMapArr<T> mapArr) where T : Rsc6Block, new()
        {
            mapArr.Write(this);
        }

        public void WriteSizedArr<T>(Rsc6ManagedSizedArr<T> arr) where T : Rsc6Block, new()
        {
            arr.Write(this);
        }

        public void WriteStr(Rsc6Str str)
        {
            str.Write(this);
        }

        public void WritePtrStr(Rsc6PtrStr str, uint pad = 0)
        {
            str.Write(this, pad);
        }

        public void WritePoolArr<T>(Rsc6PoolArr<T> arr) where T : unmanaged
        {
            arr.Write(this);
        }

        public void WritePtr<T>(Rsc6Ptr<T> ptr) where T : Rsc6Block, new()
        {
            ptr.Write(this);
        }

        public void WritePtrUnmanaged<T>(Rsc6PtrUnmanaged<T> ptr) where T : unmanaged
        {
            ptr.Write(this);
        }

        public void WritePtrArr<T>(Rsc6PtrArr<T> ptr) where T : Rsc6Block, new()
        {
            ptr.Write(this);
        }

        public void WriteRawArrPtr<T>(Rsc6RawArr<T> ptr) where T : unmanaged
        {
            ptr.Write(this);
        }

        public void WritePtrEmbed(object target, object owner, ulong offset)
        {
            //target is the object this is a pointer to - will be 0 pointer if target is null
            //owner is the object that the final pointer will be offset from
            //offset is added to the owner's pointer to get the final pointer.
            if (target != null)
            {
                AddPointerRef(owner, offset);
            }
            WriteUInt32(0);//this data will get updated later if the object isn't null
        }
    }

    public interface Rsc6Block : BlockBase
    {
        bool IsPhysical { get; }
        void Read(Rsc6DataReader reader);
        void Write(Rsc6DataWriter writer);
    }

    public abstract class Rsc6BlockBase : Rsc6Block
    {
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public abstract ulong BlockLength { get; }
        public abstract void Read(Rsc6DataReader reader);
        public abstract void Write(Rsc6DataWriter writer);
    }

    public class Rsc6BlockMap : Rsc6BlockBase
    {
        public override ulong BlockLength => 4;

        public uint Blockmap { get; set; }

        public Rsc6BlockMap()
        {
        }

        public Rsc6BlockMap(ulong blockmap)
        {
            Blockmap = (uint)blockmap;
        }

        public override void Read(Rsc6DataReader reader)
        {
            Blockmap = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Blockmap);
        }
    }

    //Pointer to a single managed object
    public struct Rsc6Ptr<T> where T : Rsc6Block, new()
    {
        public ulong Position;
        public T Item;

        public Rsc6Ptr(T item)
        {
            Position = 0;
            Item = item;
        }

        public Rsc6Ptr(ulong pos)
        {
            Position = pos;
        }

        public void ReadPtr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
        }

        public void Read(Rsc6DataReader reader, Func<Rsc6DataReader, T> createFunc = null)
        {
            Position = reader.ReadUInt32();
            Item = reader.ReadBlock(Position, createFunc);
        }

        public void ReadItem(Rsc6DataReader reader, Func<Rsc6DataReader, T> createFunc = null)
        {
            Item = reader.ReadBlock(Position, createFunc);
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Item);
            writer.WriteUInt32((uint)Position);
            writer.WriteBlock(Item);
        }

        public override string ToString()
        {
            return Item?.ToString() ?? Position.ToString();
        }
    }

    //Pointer to a single unmanaged object
    public struct Rsc6PtrUnmanaged<T> where T : unmanaged
    {
        public ulong Position;
        public T Item;

        public Rsc6PtrUnmanaged(T item)
        {
            this.Position = 0;
            this.Item = item;
        }

        public void ReadPtr(Rsc6DataReader reader)
        {
            this.Position = reader.ReadUInt32();
        }

        public void ReadItem(Rsc6DataReader reader)
        {
            this.Read(reader, false);
        }

        public void Read(Rsc6DataReader reader, bool readPos = true)
        {
            if (readPos) this.Position = reader.ReadUInt32();
            var p = reader.Position;
            reader.Position = this.Position;

            if (typeof(T) == typeof(Vector3))
            {
                dynamic item = reader.ReadArray<T>(1)[0];
                this.Item = (T)(ValueType)new Vector3(item.Z, item.X, item.Y);
            }
            else if (typeof(T) == typeof(Vector4))
            {
                dynamic item = reader.ReadArray<T>(1)[0];
                this.Item = (T)(ValueType)new Vector4(item.Z, item.X, item.Y, item.W);
            }
            else if (typeof(T) == typeof(Quaternion))
            {
                dynamic item = reader.ReadArray<T>(1)[0];
                this.Item = (T)(ValueType)new Quaternion(item.Z, item.X, item.Y, item.W);
            }
            else
            {
                this.Item = reader.ReadArray<T>(1)[0];
            }
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(this.Item);
            writer.WriteUInt32((uint)this.Position);
            writer.WriteArray(new T[] { this.Item });
        }

        public override string ToString()
        {
            return this.Item.ToString();
        }
    }

    //Array of unmanaged objects
    //Sometimes count/capacity can be stored on 4 bytes
    public struct Rsc6Arr<T> where T : unmanaged
    {
        public uint Position;
        public uint Count;
        public uint Capacity;
        public T[] Items;
        public bool Size32;

        public Rsc6Arr(T[] items, bool size32 = false)
        {
            Position = 0;
            Count = (ushort)items.Length;
            Capacity = Count;
            Items = items;
            Size32 = size32;
        }

        public void Read(Rsc6DataReader reader, bool size32 = false)
        {
            Size32 = size32;
            Position = reader.ReadUInt32();
            Count = Size32 ? reader.ReadUInt32() : reader.ReadUInt16();
            Capacity = Size32 ? reader.ReadUInt32() : reader.ReadUInt16();

            var p = reader.Position;
            reader.Position = Position;

            Items = reader.ReadArray<T>(Count);
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer, bool size32 = false)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);

            if (size32 || Size32)
            {
                writer.WriteUInt32(Count);
                writer.WriteUInt32(Capacity);
            }
            else
            {
                writer.WriteUInt16((ushort)Count);
                writer.WriteUInt16((ushort)Capacity);
            }
            writer.WriteArray(Items);
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

    public struct Rsc6PackedArr
    {
        public uint Position;
        public uint ElementSize; //32 is max
        public uint ElementMax;
        public uint[] Items;

        public static uint[] MaskTable = new uint[]
        {
            0x0, 0x1, 0x3, 0x7, 0xF, 0x1F, 0x3F, 0x7F, 0xFF, 0x1FF, 0x3FF,
            0x7FF, 0xFFF, 0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF, 0x1FFFF,
            0x3FFFF, 0x7FFFF, 0xFFFFF, 0x1FFFFF, 0x3FFFFF, 0x7FFFFF,
            0xFFFFFF, 0x1FFFFFF, 0x3FFFFFF, 0x7FFFFFF, 0xFFFFFFF,
            0x1FFFFFFF, 0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF
        };

        public Rsc6PackedArr(uint[] items)
        {
            Position = 0;
            ElementSize = (ushort)items.Length;
            ElementMax = ElementSize;
            Items = items;
        }

        public void Read(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
            ElementSize = reader.ReadUInt32();
            ElementMax = reader.ReadUInt32();

            var p = reader.Position;
            reader.Position = Position;

            var numElements = GetElementCount();
            Items = reader.ReadArray<uint>(ElementMax);

            /*uint addr = 0;
            Items = new uint[ElementMax];

            for (uint i = 0; i < ElementMax; i++)
            {
                var val = elements[i];
                var word = addr >> 5;
                var bit = addr & 31;

                Items[word] |= val << (int)bit;
                Items[word + 1] |= (uint)((ulong)val >> (32 - (int)bit));

                addr += ElementSize;
            }*/
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);
            writer.WriteUInt32(ElementSize);
            writer.WriteUInt32(ElementMax);
            writer.WriteArray(Items);
        }

        public uint GetElementCount()
        {
            return (ElementSize * ElementMax + 31) >> 5;
        }

        public uint GetElement(uint index)
        {
            var address = index * ElementSize;
            var block = address >> 5;
            var bit = address & 31;

            var word = Items[block] | ((ulong)Items[block + 1] << 32);
            return (uint)((word >> (int)bit) & MaskTable[ElementSize]);

            //var blockValues = new uint[] { Convert.ToUInt32(Items[block]), Convert.ToUInt32(Items[block + 1]) };
            //var word = BitConverter.ToUInt64(BitConverter.GetBytes(blockValues[0]).Concat(BitConverter.GetBytes(blockValues[1])).ToArray(), 0);
            //return (uint)((word >> (int)bit) & MaskTable[ElementSize]);
        }

        public uint GetQuantizedValue(uint address, uint[] data, uint elemSize)
        {
            var block = address >> 5;
            var bit = address & 31;
            var word = data[block] | (ulong)((data[block + 1]) << 32);
	        return (uint)((word >> (int)bit) & MaskTable[elemSize]);
        }

        public uint this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + GetElementCount().ToString();
        }
    }

    //Array of managed objects
    public struct Rsc6ManagedArr<T> where T : Rsc6Block, new()
    {
        public uint Position;
        public ushort Count;
        public ushort Capacity;
        public T[] Items;

        public Rsc6ManagedArr(T[] items)
        {
            Position = 0;
            Items = items;
            Count = (ushort)items.Length;
            Capacity = Count;
        }

        public void Read(Rsc6DataReader reader, Func<Rsc6DataReader, T> createFunc = null)
        {
            Position = reader.ReadUInt32();
            Count = reader.ReadUInt16();
            Capacity = reader.ReadUInt16();

            //TODO: remove this temporary hack
            if (Position > 0 && Count == 0 && Capacity == 0)
            {
                Count = 1;
                Capacity = 1;
            }

            var p = reader.Position;
            reader.Position = Position;
            Items = new T[Count];

            for (int i = 0; i < Count; i++)
            {
                Items[i] = reader.ReadBlock(createFunc);
            }
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);
            writer.WriteUInt16(Count);
            writer.WriteUInt16(Capacity);
            writer.WriteBlocks(Items);
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

    //Array of managed objects but with a given size different than count/capacity
    public struct Rsc6ManagedSizedArr<T> where T : Rsc6Block, new()
    {
        public uint Position;
        public ushort Count;
        public ushort Capacity;
        public T[] Items;

        public Rsc6ManagedSizedArr(T[] items)
        {
            Position = 0;
            Items = items;
            Count = (ushort)items.Length;
            Capacity = Count;
        }

        public void ReadArr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
            Count = reader.ReadUInt16();
            Capacity = reader.ReadUInt16();
        }

        public void ReadItems(Rsc6DataReader reader, ushort size, Func<Rsc6DataReader, T> createFunc = null)
        {
            var p = reader.Position;
            reader.Position = Position;

            Items = new T[size];
            Count = size;
            Capacity = size;

            for (int i = 0; i < size; i++)
            {
                Items[i] = reader.ReadBlock(createFunc);
            }
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);
            writer.WriteUInt16(Count);
            writer.WriteUInt16(Capacity);
            writer.WriteBlocks(Items);
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

    //Array of key/value objects with different purpose (atMap uses a hash to associate an arbitrary key with arbitrary data)
    public struct Rsc6AtMapArr<T> where T : Rsc6Block, new()
    {
        public uint Position;
        public ushort Count; //m_Slots, number of slots in toplevel hash
        public ushort Capacity; //m_Used, number of those slots currently in use
        public byte Unknown_8h = 0xCD;
        public byte Unknown_9h = 0xCD;
        public byte Unknown_Ah = 0xCD;
        public bool AllowRecompute; //m_AllowReCompute, for dynamic memory allocation

        public uint[] Pointers;
        public T[] Items;

        public Rsc6AtMapArr(T[] items)
        {
            Position = 0;
            Items = items;
            Count = (ushort)items.Length;
            Capacity = (ushort)items.Length;
            Pointers = new uint[items.Length];
        }

        public void Read(Rsc6DataReader reader, Func<Rsc6DataReader, T> createFunc = null)
        {
            Position = reader.ReadUInt32();
            Count = reader.ReadUInt16();
            Capacity = reader.ReadUInt16();
            Unknown_8h = reader.ReadByte();
            Unknown_9h = reader.ReadByte();
            Unknown_Ah = reader.ReadByte();
            AllowRecompute = reader.ReadBoolean();

            var p = reader.Position;
            reader.Position = Position;
            Pointers = reader.ReadArray<uint>(Count);
            Items = new T[Count];

            for (int i = 0; i < Count; i++)
            {
                Items[i] = reader.ReadBlock(Pointers[i], createFunc);
            }
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            var ptrs = new uint[Capacity];
            writer.AddPointerRef(ptrs);
            writer.WriteUInt32(Position);
            writer.WriteUInt16(Count);
            writer.WriteUInt16(Capacity);
            writer.WriteByte(Unknown_8h);
            writer.WriteByte(Unknown_9h);
            writer.WriteByte(Unknown_Ah);
            writer.WriteBoolean(AllowRecompute);

            var data = writer.WriteArray(ptrs);
            var offset = 0u;
            if (Items != null)
            {
                for (int i = 0; i < Capacity; i++)
                {
                    var item = (i < Items.Length) ? Items[i] : default;
                    if (item != null)
                    {
                        writer.WriteBlock(item);
                        writer.AddPointerRef(item, data, offset);
                    }
                    offset += 48;
                }
            }
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

    //Pointer to an array of managed objects
    public struct Rsc6PtrArr<T> where T : Rsc6Block, new()
    {
        public uint Position;
        public ushort Count;
        public ushort Capacity;
        public uint[] Pointers;
        public T[] Items;

        public Rsc6PtrArr(T[] items)
        {
            Position = 0;
            Count = (ushort)items.Length;
            Capacity = Count;
            Pointers = new uint[Count];
            Items = items;
        }

        public Rsc6PtrArr(uint count)
        {
            Position = 0;
            Count = (ushort)count;
            Capacity = (ushort)count;
            Pointers = new uint[count];
            Items = new T[count];
        }

        public void Read(Rsc6DataReader reader, Func<Rsc6DataReader, T> createFunc = null)
        {
            Position = reader.ReadUInt32();
            Count = reader.ReadUInt16();
            Capacity = reader.ReadUInt16();

            var p = reader.Position;
            reader.Position = Position;
            Pointers = reader.ReadArray<uint>(Count);

            Items = new T[Count];
            for (int i = 0; i < Count; i++)
            {
                byte[] buffer = BitConverter.GetBytes(Pointers[i]);
                Items[i] = reader.ReadBlock(BitConverter.ToUInt32(buffer, 0), createFunc);
            }
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            var ptrs = new uint[Capacity];
            if (Count > 0)
            {
                writer.AddPointerRef(ptrs);
            }
            writer.WriteUInt32(Position);
            writer.WriteUInt16(Count);
            writer.WriteUInt16(Capacity);

            if (Items != null)
            {
                var data = writer.WriteArray(ptrs);
                var offset = 0u;
                for (int i = 0; i < Capacity; i++)
                {
                    var item = (i < Items.Length) ? Items[i] : default;
                    if (item != null)
                    {
                        writer.WriteBlock(item);
                        writer.AddPointerRef(item, data, offset);
                    }
                    offset += 4;
                }
            }
        }

        public T this[int index]
        {
            get => index < Items.Length ? Items[index] : default;
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

    //Pointer to an array of blocks with a given size
    public struct Rsc6RawLst<T> where T : Rsc6Block, new()
    {
        public uint Position { get; set; }
        public T[] Items { get; set; }

        public Rsc6RawLst(T[] items)
        {
            Position = 0;
            Items = items;
        }

        public void ReadPtr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
        }

        public void ReadItems(Rsc6DataReader reader, uint count)
        {
            if (Position != 0)
            {
                var p = reader.Position;
                reader.Position = Position;
                Items = new T[count];

                for (int i = 0; i < count; i++)
                {
                    Items[i] = reader.ReadBlock<T>();
                }
                reader.Position = p;
            }
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);
            writer.WriteBlocks(Items);
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + (Items?.Length.ToString() ?? "0");
        }
    }

    //Pointer to an array of unmanaged data with a given size
    public struct Rsc6RawArr<T> where T : unmanaged
    {
        public ulong Position;
        public T[] Items;

        public Rsc6RawArr(ulong position, T[] items)
        {
            Position = position;
            Items = items;
        }

        public Rsc6RawArr(ulong position)
        {
            Position = position;
        }

        public Rsc6RawArr(T[] items)
        {
            Position = 0;
            Items = items;
        }

        public void ReadPtr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
        }

        public void ReadItems(Rsc6DataReader reader, uint count)
        {
            var p = reader.Position;
            reader.Position = Position;
            Items = reader.ReadArray<T>(count);

            /*if (tems != null && typeof(T) == typeof(Matrix4x4)) //Convert each matrix from XYZ to ZXY
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    Matrix4x4 matrix = (Matrix4x4)(object)Items[i];
                    matrix = Rpf6Crypto.ToZXY(matrix);
                    Items[i] = (T)(object)matrix;
                }
            }*/
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32((uint)Position);
            writer.WriteArray(Items);
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + (Items?.Length.ToString() ?? "0");
        }
    }

    //Pointer to an array of raw pointers to managed objects
    public struct Rsc6RawPtrArr<T> where T : Rsc6Block, new()
    {
        public uint Position;
        public uint[] Pointers;
        public T[] Items;

        public void ReadPtr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
        }

        public void ReadItems(Rsc6DataReader reader, uint count, Func<Rsc6DataReader, T> createFunc = null)
        {
            var p = reader.Position;
            reader.Position = Position;
            Pointers = reader.ReadArray<uint>(count);
            Items = new T[count];

            for (int i = 0; i < count; i++)
            {
                Items[i] = reader.ReadBlock(Pointers[i], createFunc);
            }
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            var ptrs = (Items != null) ? new uint[Items.Length] : null;
            writer.AddPointerRef(ptrs);
            writer.WriteUInt32(Position);

            if (Items != null)
            {
                var data = writer.WriteArray(ptrs);
                var offset = 0u;
                for (int i = 0; i < Items.Length; i++)
                {
                    var item = Items[i];
                    if (item != null)
                    {
                        writer.WriteBlock(item);
                        writer.AddPointerRef(item, data, offset);
                    }
                    offset += 4;
                }
            }
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + (Items?.Length.ToString() ?? "0");
        }
    }

    //Pointer to an array of 'pointers/count/capacity' to managed objects
    //Basically Rsc6Ptr + Rsc6Arr
    public struct Rsc6PtrToPtrArr<T> where T : Rsc6Block, new()
    {
        public uint Position;
        public Rsc6ManagedArr<T> Array;

        public void ReadPtr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
        }

        public void ReadItems(Rsc6DataReader reader, Func<Rsc6DataReader, T> createFunc = null)
        {
            if (Position != 0)
            {
                var p = reader.Position;
                reader.Position = Position;
                Array = reader.ReadArr(createFunc);
                reader.Position = p;
            }
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Array);
            writer.WriteUInt32(Position);
            writer.WriteArr(Array);
        }

        public T this[int index]
        {
            get => Array[index];
            set => Array[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + (Array.Items?.Length.ToString() ?? "0");
        }
    }

    //Pointer to a string
    public struct Rsc6Str
    {
        public ulong Position;
        public string Value;

        public Rsc6Str(string str)
        {
            Position = 0;
            Value = str;
        }

        public Rsc6Str(ulong pos)
        {
            Position = pos;
        }

        public void Read(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
            if (Position != 0)
            {
                var blockexists = reader.BlockPool.TryGetValue(Position, out var exblock);
                if (blockexists && (exblock is string str))
                {
                    Value = str;
                    return;
                }

                var p = reader.Position;
                reader.Position = Position;
                Value = reader.ReadString();
                reader.Position = p;

                if (blockexists == false)
                {
                    reader.BlockPool[Position] = Value;
                }
            }
        }

        public void ReadAsArray(Rsc6DataReader reader, uint pad)
        {
            Value = reader.ReadString();
            if (Value.Length <= pad) pad += 16;
            if ((reader.Position % pad) != 0)
            {
                reader.Position += (ushort)(pad - (ushort)(reader.Position % pad));
            }
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Value);
            writer.WriteUInt32((uint)Position);

            if (Value != null)
            {
                var encoding = Encoding.UTF8;
                var b = encoding.GetBytes(Value);
                var len = b.Length + 1;
                var data = new byte[len];
                if (b != null)
                {
                    Buffer.BlockCopy(b, 0, data, 0, b.Length);
                }
                writer.AddBlock(Value, data);
            }
        }

        public void WriteAsArray(Rsc6DataReader reader, uint pad)
        {
            if (Value != null)
            {
                var encoding = Encoding.UTF8;
                var b = encoding.GetBytes(Value);
                var len = b.Length + 1;

                if ((len % pad) != 0)
                {
                    len += (ushort)(pad - (ushort)(len % pad));
                }

                var data = new byte[len];
                Array.Fill(data, (byte)0xCD);

                if (b != null)
                {
                    Buffer.BlockCopy(b, 0, data, 0, b.Length);
                }
            }
        }

        public override string ToString()
        {
            return Value ?? "NULL";
        }
    }

    //A rarely specific-case structure consisting of a pointer + count + capacity pointing to an array of strings
    //Each string have a maximum size per block, the remaining bytes are padding until the start of the next block
    public struct Rsc6PtrStr
    {
        public ulong Position;
        public ushort Count;
        public ushort Capacity;
        public Rsc6Str[] Items;

        public Rsc6PtrStr()
        {
            Items = Array.Empty<Rsc6Str>();
        }

        public Rsc6PtrStr(Rsc6Str[] items)
        {
            Position = 0;
            Count = (ushort)items.Length;
            Capacity = Count;
            Items = items;
        }

        public void Read(Rsc6DataReader reader, uint pad)
        {
            Position = reader.ReadUInt32();
            Count = reader.ReadUInt16();
            Capacity = reader.ReadUInt16();

            if (Position != 0)
            {
                var p = reader.Position;
                reader.Position = Position;
                Items = new Rsc6Str[Count];

                for (int i = 0; i < Count; i++)
                {
                    if (pad > 0) //Used for rage::speedTreeDebugName...
                    {
                        Items[i] = new Rsc6Str(reader.Position);
                        Items[i].ReadAsArray(reader, pad);
                    }
                    else Items[i] = reader.ReadStr();
                }
                reader.Position = p;
            }
        }

        public void Write(Rsc6DataWriter writer, uint pad) //TODO: make it work with custom padding
        {
            var ptrs = new uint[Capacity];
            if (ptrs.Length > 0)
            {
                writer.AddPointerRef(ptrs);
            }
            writer.WriteUInt32((uint)Position);
            writer.WriteUInt16(Count);
            writer.WriteUInt16(Capacity);

            if (Items != null && Count > 0)
            {
                var data = writer.WriteArray(ptrs);
                var offset = 0u;
                for (int i = 0; i < Capacity; i++)
                {
                    var item = (i < Items.Length) ? Items[i] : default;
                    if (item.Value != null)
                    {
                        var encoding = Encoding.UTF8;
                        var b = encoding.GetBytes(item.Value);
                        var len = b.Length + 1;
                        var strdata = new byte[len];

                        if (b != null)
                        {
                            Buffer.BlockCopy(b, 0, strdata, 0, b.Length);
                        }

                        writer.AddBlock(item.Value, strdata);
                        writer.AddPointerRef(item, data, offset);
                    }
                    offset += 4;
                }
            }
        }
    }

    //A rarely specific-case structure used for flash files (.wsf)
    public struct Rsc6PoolArr<T> where T : unmanaged //Flash files - .wsf
    {
        public uint Position;
        public ushort Count; //m_Size
        public ushort Capacity; //m_FreeCount
        public uint Size; //m_ElemSize
        public uint FirstFree; //m_FirstFree, 2nd pointer
        public ushort PeakUse; //m_PeakUse
        public ushort Dummy = 0xCDCD; //m_Dummy
        public byte OwnMem = 1; //m_OwnMem
        public T[] Items;

        public Rsc6PoolArr(T[] items)
        {
            Position = 0;
            Count = (ushort)items.Length;
            Capacity = Count;
            Items = items;
        }

        public void Read(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
            Count = reader.ReadUInt16();
            Capacity = reader.ReadUInt16();
            Size = reader.ReadUInt32();
            FirstFree = reader.ReadUInt32();
            PeakUse = reader.ReadUInt16();
            Dummy = reader.ReadUInt16();
            OwnMem = reader.ReadByte();

            var p = reader.Position;
            reader.Position = Position;
            Items = reader.ReadArray<T>(Count);
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);
            writer.WriteUInt16(Count);
            writer.WriteUInt16(Capacity);
            writer.WriteUInt32(Size);
            writer.WriteUInt32(FirstFree);
            writer.WriteUInt16(PeakUse);
            writer.WriteUInt16(Dummy);
            writer.WriteByte(OwnMem);
            writer.WriteArray(Items);
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

    public class Rsc6String : Rsc6BlockBase
    {
        public override ulong BlockLength => 8;
        public uint Position;
        public string Value;
        public uint FixedLength;

        public Rsc6String() { }
        public Rsc6String(uint fixedLength) { FixedLength = fixedLength; }

        public override void Read(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
            if (Position != 0)
            {
                var p = reader.Position;
                reader.Position = Position;

                if (FixedLength != 0)
                    Value = Encoding.ASCII.GetString(reader.ReadArray<byte>(40, false)).Trim('\0');
                else
                    Value = reader.ReadString();
                reader.Position = p;
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Value);
            writer.WriteUInt32(Position);

            if (Value != null)
            {
                var encoding = Encoding.UTF8;
                var b = encoding.GetBytes(Value);
                var len = b.Length + 1;
                var data = new byte[len];
                if (b != null)
                {
                    Buffer.BlockCopy(b, 0, data, 0, b.Length);
                }
                writer.AddBlock(Value, data);
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public abstract class Rsc6FileBase : Rsc6BlockBase //I guess I should rework this to include blockmap and then update all classes...
    {
        public ulong VFT { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32((uint)VFT);
        }
    }

    public static class Rsc6DataMap
    {
        public static List<T> Build<T>(List<T> entries, bool reversebuckets = false, T[] extest = null) where T : Rsc6DataMapEntry<T>
        {
            if (entries.Count < 1)
            {
                return null;
            }

            var numbuckets = 0;
            var numkeys = entries.Count;
            if (numkeys < 11) numbuckets = 11;
            else if (numkeys < 29) numbuckets = 29;
            else if (numkeys < 59) numbuckets = 59;
            else if (numkeys < 107) numbuckets = 107;
            else if (numkeys < 191) numbuckets = 191;
            else if (numkeys < 331) numbuckets = 331;
            else if (numkeys < 563) numbuckets = 563;
            else if (numkeys < 953) numbuckets = 953;
            else if (numkeys < 1609) numbuckets = 1609;
            else if (numkeys < 2729) numbuckets = 2729;
            else if (numkeys < 4621) numbuckets = 4621;
            else if (numkeys < 7841) numbuckets = 7841;
            else if (numkeys < 13297) numbuckets = 13297;
            else if (numkeys < 22571) numbuckets = 22571;
            else if (numkeys < 38351) numbuckets = 38351;
            else if (numkeys < 65167) numbuckets = 65167;
            else numbuckets = 65521;

            var buckets = new List<T>[numbuckets];
            foreach (var entry in entries)
            {
                var b = entry.MapKey % numbuckets;
                var bucket = buckets[b];
                if (bucket == null)
                {
                    bucket = new List<T>();
                    buckets[b] = bucket;
                }
                bucket.Add(entry);
            }

            var result = new List<T>();
            foreach (var b in buckets)
            {
                if ((b?.Count ?? 0) == 0)
                    result.Add(default);
                else
                {
                    if (reversebuckets && (b.Count > 1)) b.Reverse();
                    result.Add(b[0]);
                    var p = b[0];
                    for (int i = 1; i < b.Count; i++)
                    {
                        var c = b[i];
                        c.MapNext = default;
                        p.MapNext = c;
                        p = c;
                    }
                }
            }
            return result;
        }

        public static List<U> Flatten<T, U>(T[] entries, Func<T, U> yieldFunc, Comparison<U> sortFunc = null) where T : Rsc6DataMapEntry<T>
        {
            var result = new List<U>();
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    var e = entry;
                    while (e != null)
                    {
                        result.Add(yieldFunc(e));
                        e = e.MapNext;
                    }
                }
            }
            if (sortFunc != null)
            {
                result.Sort(sortFunc);
            }
            return result;
        }

        public static Dictionary<uint, U> GetDictionary<T, U>(T[] entries, Func<T, U> yieldFunc) where T : Rsc6DataMapEntry<T>
        {
            var result = new Dictionary<uint, U>();
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    var e = entry;
                    while (e != null)
                    {
                        result[e.MapKey] = yieldFunc(e);
                        e = e.MapNext;
                    }
                }
            }
            return result;
        }
    }

    public interface Rsc6DataMapEntry<T>
    {
        uint MapKey { get; }
        T MapNext { get; set; }
    }
}