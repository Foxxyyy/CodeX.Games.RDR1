using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using CodeX.Core.Numerics;

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

        public Half ReadHalf()
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

        public Vector3 ReadVector3(bool toZXY = false)
        {
            var v = BufferUtil.ReadVector3(Data, GetDataOffset());
            Position += 12;
            if (toZXY)
            {
                v = new Vector3(v.Z, v.X, v.Y);
            }
            return v;
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

        public Vector4 ReadVector4(bool toZXYW = false)
        {
            var v = BufferUtil.ReadVector4(Data, GetDataOffset());
            Position += 16;

            if (float.IsNaN(v.W))
                v = new Vector4(v.X, v.Y, v.Z, 0.0f);
            if (toZXYW)
                v = new Vector4(v.Z, v.X, v.Y, v.W);
            return v;
        }

        public Vector4[] ReadVector4Arr(int count, bool toZXYW = false)
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
            return matrix;
        }

        public BoundingBox4 ReadBoundingBox4()
        {
            var bb = new BoundingBox4
            {
                Min = BufferUtil.ReadVector4(Data, GetDataOffset()),
                Max = BufferUtil.ReadVector4(Data, GetDataOffset())
            };
            Position += 32;
            return bb;
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

        public Rsc6Arr<T> ReadArr<T>(bool size64 = false) where T : unmanaged
        {
            var arr = new Rsc6Arr<T>();
            arr.Read(this, size64);
            return arr;
        }

        public Rsc6CustomArr<T> ReadArr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : Rsc6Block, new()
        {
            var arr = new Rsc6CustomArr<T>();
            arr.Read(this, createFunc);
            return arr;
        }

        public Rsc6PoolArr<T> ReadPoolArr<T>() where T : unmanaged
        {
            var arr = new Rsc6PoolArr<T>();
            arr.Read(this);
            return arr;
        }

        public Rsc6RawArr<T> ReadRawArrItems<T>(Rsc6RawArr<T> arr, uint count, bool uShort = false) where T : unmanaged
        {
            arr.ReadItems(this, count, uShort);
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

        public Rsc6RawArr<T> Rsc6RawVector4Items<T>(Rsc6RawArr<T> arr, uint count, bool toZXYW = false) where T : unmanaged
        {
            arr.ReadVector4Items(this, count, toZXYW);
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

        public Rsc6RawPtrArr<T> ReadRawPtrArrPtr<T>() where T : Rsc6Block, new()
        {
            var arr = new Rsc6RawPtrArr<T>();
            arr.ReadPtr(this);
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
            if (PhysicalBlocks.Contains(block.Block))
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
            var blockmap = (Rsc6BlockMap)null;
            var blockmapdata = (byte[])null;

            foreach (var block in BlockList)
            {
                BlockDataDict.TryGetValue(block, out var bdata);

                if (bdata == null)
                    continue;

                var bblock = new BuilderBlock(block, (ulong)bdata.Length);
                blocks[block] = bblock;

                if (PhysicalBlocks.Contains(block))
                    pblocks.Add(bblock);
                else
                    vblocks.Add(bblock);

                if ((block is Rsc6Block) || (block is Array))
                {
                    bblock.Align = 16;
                }

                if (block is Rsc6BlockMap)
                {
                    blockmap = block as Rsc6BlockMap;
                    blockmapdata = bdata;
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
            flags.SetTotalSize((int)vlen, (int)plen);
            int flag = flags.GetFlags((int)vlen, (int)plen);
            flags.IsExtendedFlags = false;

            var compressed = Rpf6Crypto.CompressZStandard(data);
            var output = Rpf6ResourceFileEntry.AddResourceHeader(compressed, version, flag, flags.Flag2, flags);

            return output;
        }

        private new void BuildPointers(Dictionary<object, BuilderBlock> blocks)
        {
            foreach (var pref in PointerRefs)
            {
                if (pref.Data == null) continue;
                if (pref.Object == null) continue;
                if (blocks.TryGetValue(pref.Object, out var bblock) == false) continue;

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

            Data = new byte[size];
            Position = 0;

            AddBlock(block, Data);
            block.Write(this);

            if (block.IsPhysical)
            {
                PhysicalBlocks.Add(block);
            }
            Data = exdata;
            Position = expos;
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

            Data = new byte[size];
            Position = 0;
            AddBlock(blocks, Data);

            if (b0.IsPhysical)
            {
                PhysicalBlocks.Add(blocks);
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block == null) continue;
                Position = (ulong)(i * bs);
                block.Write(this);
            }
            Data = exdata;
            Position = expos;
        }

        public void WriteBoolean(bool value)
        {
            WriteByte(value ? (byte)1 : (byte)0);
        }

        public void WriteHalf(Half value)
        {
            byte[] buffer = new byte[2];
            BufferUtil.WriteStruct(buffer, 0, ref value);
            WriteBytes(buffer);
        }

        public void WriteHalf2(Half2 value)
        {
            byte[] buffer = new byte[4];
            BufferUtil.WriteStruct(buffer, 0, ref value);
            WriteBytes(buffer);
        }

        public void WriteHalf4(Half4 value)
        {
            byte[] buffer = new byte[8];
            BufferUtil.WriteStruct(buffer, 0, ref value);
            WriteBytes(buffer);
        }

        public void WriteArr<T>(Rsc6CustomArr<T> arr) where T : Rsc6Block, new()
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

        public void WriteArr<T>(Rsc6Arr<T> arr) where T : unmanaged
        {
            arr.Write(this);
        }

        public void WritePoolArr<T>(Rsc6PoolArr<T> arr) where T : unmanaged
        {
            arr.Write(this);
        }

        public void WritePtr<T>(Rsc6Ptr<T> ptr) where T : Rsc6Block, new()
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

    public interface Rsc6Block
    {
        ulong FilePosition { get; set; }
        ulong BlockLength { get; }
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

    public struct Rsc6CustomArr<T> where T : Rsc6Block, new()
    {
        public uint Position;
        public ushort Count;
        public ushort Capacity;
        public T[] Items;

        public Rsc6CustomArr(T[] items)
        {
            Position = 0;
            Items = items;
            Count = (ushort)items.Length;
            Capacity = Count;
        }

        public void Read(Rsc6DataReader reader, Func<Rsc6DataReader, T> createFunc = null) //Big-endian
        {
            Position = reader.ReadUInt32();
            Count = reader.ReadUInt16();
            Capacity = reader.ReadUInt16();

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

    public struct Rsc6Arr<T> where T : unmanaged
    {
        public uint Position;
        public uint Count;
        public uint Capacity;
        public T[] Items;
        public bool Uint32;

        public Rsc6Arr(T[] items, bool size32 = false)
        {
            Position = 0;
            Count = (ushort)items.Length;
            Capacity = Count;
            Items = items;
            Uint32 = size32;
        }

        public void Read(Rsc6DataReader reader, bool size32 = false)
        {
            Uint32 = size32;
            Position = reader.ReadUInt32();
            Count = Uint32 ? reader.ReadUInt32() : reader.ReadUInt16();
            Capacity = Uint32 ? reader.ReadUInt32() : reader.ReadUInt16();

            var p = reader.Position;
            reader.Position = Position;
            Items = reader.ReadArray<T>(Count);
            reader.Position = p;
        }

        public void Read(Rsc6DataReader reader, uint count)
        {
            Position = reader.ReadUInt32();
            Count = count;
            Capacity = count;
            var p = reader.Position;
            reader.Position = Position;
            Items = reader.ReadArray<T>(count);
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);

            if (Uint32)
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
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

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

    public struct Rsc6PtrToPtrArr<T> where T : Rsc6Block, new()
    {
        public uint Position;
        public Rsc6CustomArr<T> Array;

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
                Array = reader.ReadArr<T>(createFunc);
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
                var p = reader.Position;
                reader.Position = Position;
                Value = reader.ReadString();
                reader.Position = p;
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
            return Value;
        }
    }

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

        public void Write(Rsc6DataWriter writer, uint pad) //TODO: make it working with custom padding
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

    public struct Rsc6RawArr<T> where T : unmanaged
    {
        public ulong Position;
        public T[] Items;

        public Rsc6RawArr(T[] items)
        {
            Position = 0;
            Items = items;
        }

        public Rsc6RawArr(ulong position)
        {
            Position = position;
        }

        public void ReadPtr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
        }

        public void ReadItems(Rsc6DataReader reader, uint count, bool uShort = false)
        {
            var p = reader.Position;
            reader.Position = Position;

            if (uShort)
                Items = reader.ReadUInt16Arr((int)count) as T[];
            else
                Items = reader.ReadArray<T>(count);
            reader.Position = p;
        }

        public void ReadVector4Items(Rsc6DataReader reader, uint count, bool toZXYW = false)
        {
            reader.Position = Position;
            Items = reader.ReadVector4Arr((int)count, toZXYW) as T[];
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
                Items[i] = reader.ReadBlock<T>(Pointers[i], createFunc);
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

    public abstract class Rsc6FileBase : Rsc6BlockBase
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
}