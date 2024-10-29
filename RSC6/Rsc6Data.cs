using System;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using SharpDX.DirectWrite;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6DataReader : BlockReader
    {
        public Rpf6ResourceFileEntry FileEntry;
        public DataEndianess Endianess;
        public int VirtualSize;
        public int PhysicalSize;

        private static ulong VIRTUAL_BASE => Rpf6Crypto.VIRTUAL_BASE;
        private static ulong PHYSICAL_BASE => Rpf6Crypto.PHYSICAL_BASE;
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

        public float[] ReadSingleArr(int count)
        {
            var floats = new float[count];
            for (int i = 0; i < count; i++)
            {
                floats[i] = ReadSingle();
            }
            return floats;
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

        public int[] ReadInt32Arr(int count)
        {
            var array = new int[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = BufferUtil.ReadInt(Data, GetDataOffset());
                Position += 4;
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

        public T ReadBlock<T>(Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
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

        public T ReadBlock<T>(ulong position, Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            if (position == 0) return default(T);
            var p = Position;
            Position = position;
            var b = ReadBlock<T>(createFunc);
            Position = p;
            return b;
        }

        public Rsc6Ptr<T> ReadPtr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            var ptr = new Rsc6Ptr<T>();
            ptr.Read(this, createFunc);
            return ptr;
        }

        public Rsc6PtrUnmanaged<T> ReadPtrUnmanaged<T>() where T : unmanaged
        {
            var ptr = new Rsc6PtrUnmanaged<T>();
            ptr.Read(this);
            return ptr;
        }

        public Rsc6PtrArr<T> ReadPtrArr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            var arr = new Rsc6PtrArr<T>();
            arr.Read(this, createFunc);
            return arr;
        }

        public Rsc6PtrToPtrArr<T> ReadPtrToItem<T>() where T : IRsc6Block, new()
        {
            var ptr = new Rsc6PtrToPtrArr<T>();
            ptr.ReadPtr(this);
            return ptr;
        }

        public Rsc6PtrToPtrArr<T> ReadItems<T>(Rsc6PtrToPtrArr<T> arr, Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            arr.ReadItems(this, createFunc);
            return arr;
        }

        public Rsc6Arr<T> ReadArr<T>(bool useArraySizeOf64 = false) where T : unmanaged
        {
            var arr = new Rsc6Arr<T>();
            arr.Read(this, useArraySizeOf64);
            return arr;
        }

        public Rsc6PackedArr ReadPackedArr()
        {
            var arr = new Rsc6PackedArr();
            arr.Read(this);
            return arr;
        }

        public Rsc6ManagedArr<T> ReadArr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            var arr = new Rsc6ManagedArr<T>();
            arr.Read(this, createFunc);
            return arr;
        }

        public Rsc6AtMapArr<T> ReadAtMapArr<T>(Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            var mapArr = new Rsc6AtMapArr<T>();
            mapArr.Read(this, createFunc);
            return mapArr;
        }

        public Rsc6ManagedSizedArr<T> ReadSizedArrPtr<T>() where T : IRsc6Block, new()
        {
            var arr = new Rsc6ManagedSizedArr<T>();
            arr.ReadArr(this);
            return arr;
        }

        public Rsc6ManagedSizedArr<T> ReadSizedArrItems<T>(Rsc6ManagedSizedArr<T> arr, ushort size, Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
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

        public Rsc6RawLst<T> ReadRawLstPtr<T>() where T : IRsc6Block, new()
        {
            var arr = new Rsc6RawLst<T>();
            arr.ReadPtr(this);
            return arr;
        }

        public Rsc6RawLst<T> ReadRawLstItems<T>(Rsc6RawLst<T> arr, uint count, Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            arr.ReadItems(this, count, createFunc);
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

        public Rsc6RawPtrArr<T> ReadRawPtrArrPtr<T>() where T : IRsc6Block, new()
        {
            var arr = new Rsc6RawPtrArr<T>();
            arr.ReadPtr(this);
            return arr;
        }

        public Rsc6RawPtrArr<T> ReadRawPtrArrItem<T>(Rsc6RawPtrArr<T> arr, uint count, Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            arr.ReadItems(this, count, createFunc);
            return arr;
        }

        public Rsc6Ptr<T> ReadPtrOnly<T>() where T : IRsc6Block, new()
        {
            var ptr = new Rsc6Ptr<T>();
            ptr.ReadPtr(this);
            return ptr;
        }

        public Rsc6Ptr<T> ReadPtrItem<T>(Rsc6Ptr<T> ptr, Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
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

        public Rsc6StrArr ReadPtr()
        {
            var arr = new Rsc6StrArr();
            arr.ReadPtr(this);
            return arr;
        }

        public Rsc6StrArr ReadItems(Rsc6StrArr arr, uint count)
        {
            arr.ReadItems(this, count);
            return arr;
        }

        public Rsc6PtrStr ReadPtrStr(uint size = 0) //Optional size for fixed-size string block
        {
            var str = new Rsc6PtrStr();
            str.Read(this, size);
            return str;
        }

        public static BlockAnalyzer Analyze<T>(Rpf6ResourceFileEntry rfe, byte[] data, Func<Rsc6DataReader, T> createFunc = null) where T : IRsc6Block, new()
        {
            var r = new Rsc6DataReader(rfe, data);
            var block = r.ReadBlock(createFunc);
            var analyzer = new BlockAnalyzer(r, rfe);
            return analyzer;
        }
    }

    public class Rsc6DataWriter : BlockWriter
    {
        public static bool UseProjectExplorer; //Should only be true when using the project explorer
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

                if ((block is IRsc6Block) || (block is Array))
                {
                    bblock.Align = 16;
                }
            }
            if (vblocks.Count > 0)
            {
                vblocks[0].IsRoot = true;
            }

            var vpages = BuildPages(vblocks, 9, 5, 4096);
            var ppages = BuildPages(pblocks, 9, 2, 4096);
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

            int flag = FlagInfo.GetFlags((int)vlen, (int)plen);
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
                    if (pref.Object is Rsc6BoneData)
                    {
                        var kv = blocks.FirstOrDefault(e => e.Key is Rsc6SkeletonBoneData);
                        if (kv.Value != null)
                        {
                            bblock = kv.Value;
                        }
                    }
                    else if (pref.Object is Rsc6DrawableInstance)
                    {
                        var found = false;
                        var kvs = blocks.Where(e => e.Key is Rsc6DrawableInstance[]).ToArray();

                        for (int i = 0; i < kvs.Length; i++)
                        {
                            if (found) break;
                            var kv = kvs[i];

                            if (kv.Key != null)
                            {
                                if (kv.Key is Rsc6DrawableInstance[] instances)
                                {
                                    foreach (var inst in instances)
                                    {
                                        if (inst == pref.Object)
                                        {
                                            bblock = kv.Value;
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (pref.Object is Rsc6DrawableInstanceBase instBase)
                    {
                        var found = false;
                        var kvs = blocks.Where(e => e.Key is Rsc6DrawableInstanceBase[]).ToArray();

                        for (int i = 0; i < kvs.Length; i++)
                        {
                            if (found) break;
                            var kv = kvs[i];

                            if (kv.Key is Rsc6DrawableInstanceBase[] array)
                            {
                                foreach (var inst in array)
                                {
                                    if (inst.Node == instBase.Node)
                                    {
                                        bblock = kv.Value;
                                        found = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (pref.Object is Rsc6PolygonBlock pBlock)
                    {
                        var found = false;
                        var kvs = blocks.Where(e => e.Key is Rsc6PolygonBlock[]).ToArray();

                        for (int i = 0; i < kvs.Length; i++)
                        {
                            if (found) break;
                            var kv = kvs[i];

                            if (kv.Key != null && kv.Key is Rsc6PolygonBlock[] instances)
                            {
                                foreach (var inst in instances)
                                {
                                    if (inst == pBlock.Block.Target)
                                    {
                                        bblock = kv.Value;
                                        found = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                var ptr = GetPointer(bblock);
                BufferUtil.WriteUint(pref.Data, (int)pref.Pos, (uint)(ptr + pref.EmbedOffset));
            }
        }

        public void WriteBlock<T>(T block) where T : IRsc6Block, new()
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

        public void WriteBlocks<T>(T[] blocks) where T : IRsc6Block, new()
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

        public new void WriteVector3(Vector3 value)
        {
            if (UseProjectExplorer)
            {
                value = Rpf6Crypto.ToXYZ(value);
            }

            byte[] buffer = new byte[12];
            BufferUtil.WriteVector3(buffer, 0, ref value);
            this.WriteBytes(buffer);
        }

        public new void WriteVector4(Vector4 value)
        {
            if (UseProjectExplorer)
            {
                value = Rpf6Crypto.ToXYZ(value);
            }

            byte[] buffer = new byte[16];
            BufferUtil.WriteVector4(buffer, 0, ref value);
            this.WriteBytes(buffer);
        }

        public void WriteUInt16Array(ushort[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                this.WriteUInt16(arr[i]);
            }
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

        public void WriteSingleArray(float[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                this.WriteSingle(arr[i]);
            }
        }

        public void WriteVector2Array(Vector2[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                this.WriteVector2(arr[i]);
            }
        }

        public void WriteVector3Array(Vector3[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                this.WriteVector3(arr[i]);
            }
        }

        public void WriteVector4Array(Vector4[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                this.WriteVector4(arr[i]);
            }
        }

        public void WriteColourArray(Colour[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                this.WriteColour(arr[i]);
            }
        }

        public void WriteArr<T>(Rsc6Arr<T> arr, bool useArraySizeOf64 = false) where T : unmanaged
        {
            arr.Write(this, useArraySizeOf64);
        }

        public void WritePackedArr(Rsc6PackedArr arr)
        {
            arr.Write(this);
        }

        public void WriteArr<T>(Rsc6ManagedArr<T> arr) where T : IRsc6Block, new()
        {
            arr.Write(this);
        }

        public void WriteAtMapArr<T>(Rsc6AtMapArr<T> mapArr) where T : IRsc6Block, new()
        {
            mapArr.Write(this);
        }

        public void WriteSizedArr<T>(Rsc6ManagedSizedArr<T> arr) where T : IRsc6Block, new()
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

        public void WritePtr<T>(Rsc6Ptr<T> ptr) where T : IRsc6Block, new()
        {
            ptr.Write(this);
        }

        public void WritePtrUnmanaged<T>(Rsc6PtrUnmanaged<T> ptr) where T : unmanaged
        {
            ptr.Write(this);
        }

        public void WritePtrArr<T>(Rsc6PtrArr<T> ptr, bool inverseCapacity = false) where T : IRsc6Block, new()
        {
            ptr.Write(this, inverseCapacity);
        }

        public void WriteRawArr<T>(Rsc6RawArr<T> ptr) where T : unmanaged
        {
            ptr.Write(this);
        }

        public void WriteRawLst<T>(Rsc6RawLst<T> lst) where T : IRsc6Block, new()
        {
            lst.Write(this);
        }

        public void WriteStrArr(Rsc6StrArr arr)
        {
            arr.Write(this);
        }

        public void WriteRawPtrArr<T>(Rsc6RawPtrArr<T> arr) where T : IRsc6Block, new()
        {
            arr.Write(this);
        }

        public void WritePtrToPtrArr<T>(Rsc6PtrToPtrArr<T> arr) where T : IRsc6Block, new()
        {
            arr.Write(this);
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

    public struct Rsc6Ptr<T> where T : IRsc6Block, new()
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

    public struct Rsc6Arr<T> where T : unmanaged
    {
        public uint Position;
        public uint Count;
        public uint Capacity;
        public T[] Items;
        public bool Size64; //Store position + count + capacity on 12 bytes or 8 bytes

        public Rsc6Arr(T[] items, bool useArraySizeOf64 = false, uint capacity = 0)
        {
            Position = 0;
            Count = (ushort)(items?.Length ?? 0);
            Capacity = (capacity > 0) ? capacity : Count;
            Items = items;
            Size64 = useArraySizeOf64;
        }

        public void Read(Rsc6DataReader reader, bool useArraySizeOf64 = false)
        {
            Size64 = useArraySizeOf64;
            Position = reader.ReadUInt32();

            if (useArraySizeOf64)
            {
                Count = reader.ReadUInt32();
                Capacity = reader.ReadUInt32();
            }
            else
            {
                Count = reader.ReadUInt16();
                Capacity = reader.ReadUInt16();
            }

            var p = reader.Position;
            reader.Position = Position;
            Items = reader.ReadArray<T>(Count);
            Rpf6Crypto.TransformToZXY(Items);
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer, bool useArraySizeOf64 = false)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);

            if (useArraySizeOf64 || Size64)
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

        public readonly T this[int index]
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
            ElementMax = reader.ReadUInt32(); //Also number of frames

            var p = reader.Position;
            reader.Position = Position;

            var numElements = GetElementCount();
            Items = reader.ReadArray<uint>(numElements + 1);
            reader.Position = p;
        }

        public readonly void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32(Position);
            writer.WriteUInt32(ElementSize);
            writer.WriteUInt32(ElementMax);
            writer.WriteArray(Items);
        }

        public readonly uint GetElementCount()
        {
            return (ElementSize * ElementMax + 31) >> 5;
        }

        public readonly uint GetElement(uint index)
        {
            var address = index * ElementSize;
            var block = address >> 5;
            var bit = address & 31;

            var word = Items[block] | ((ulong)Items[block + 1] << 32);
            return (uint)((word >> (int)bit) & MaskTable[ElementSize]);
        }

        public readonly uint this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + GetElementCount().ToString();
        }
    }

    public struct Rsc6ManagedArr<T> where T : IRsc6Block, new()
    {
        public uint Position;
        public ushort Count;
        public ushort Capacity;
        public T[] Items;

        public Rsc6ManagedArr(T[] items)
        {
            Position = 0;
            Items = items;
            Count = (ushort)(items?.Length ?? 0);
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

        public readonly T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

    public struct Rsc6ManagedSizedArr<T> where T : IRsc6Block, new()
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

    public struct Rsc6AtMapArr<T> where T : IRsc6Block, new()
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
            Items = new T[Count];
            Pointers = reader.ReadArray<uint>(Count);

            for (int i = 0; i < Count; i++)
            {
                Items[i] = reader.ReadBlock(Pointers[i], createFunc);;
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

    public struct Rsc6PtrArr<T> where T : IRsc6Block, new()
    {
        public uint Position;
        public ushort Count;
        public ushort Capacity;
        public uint[] Pointers;
        public T[] Items;

        public Rsc6PtrArr(T[] items)
        {
            Count = (ushort)(items?.Length ?? 0);
            Capacity = Count;
            InitBlock(items);
        }

        public Rsc6PtrArr(T[] items, ushort capacity, ushort count)
        {
            Count = count;
            Capacity = capacity;
            InitBlock(items);
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
                if (Pointers[i] == 0) continue;
                byte[] buffer = BitConverter.GetBytes(Pointers[i]);
                Items[i] = reader.ReadBlock(BitConverter.ToUInt32(buffer, 0), createFunc);
            }
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer, bool inverseCapacity = false)
        {
            var ptrs = new uint[inverseCapacity ? Count : Capacity];
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
                for (int i = 0; i < (inverseCapacity ? Count : Capacity); i++)
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

        private void InitBlock(T[] items)
        {
            Position = 0;
            Pointers = new uint[Count];
            Items = items;
        }

        public readonly T this[int index]
        {
            get => index < Items.Length ? Items[index] : default;
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }
    }

    public struct Rsc6RawLst<T> where T : IRsc6Block, new()
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

        public void ReadItems(Rsc6DataReader reader, uint count, Func<Rsc6DataReader, T> createFunc = null)
        {
            if (Position != 0)
            {
                var p = reader.Position;
                reader.Position = Position;
                Items = new T[count];

                for (int i = 0; i < count; i++)
                {
                    Items[i] = reader.ReadBlock(createFunc);
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

        public void ReadPtr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
        }

        public void ReadItems(Rsc6DataReader reader, uint count)
        {
            var p = reader.Position;
            reader.Position = Position;
            Items = reader.ReadArray<T>(count);
            Rpf6Crypto.TransformToZXY(Items);
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.AddPointerRef(Items);
            writer.WriteUInt32((uint)Position);
            writer.WriteArray(Items);
        }

        public readonly T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + (Items?.Length.ToString() ?? "0");
        }
    }

    public struct Rsc6RawPtrArr<T> where T : IRsc6Block, new()
    {
        public uint Position;
        public uint[] Pointers;
        public T[] Items;

        public Rsc6RawPtrArr(T[] items)
        {
            Position = 0;
            if (items != null && items.Length > 0)
            {
                Items = items;
            }
        }
    
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

            var data = writer.WriteArray(ptrs);
            var offset = 0u;

            if (Items != null)
            {
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

    public struct Rsc6PtrToPtrArr<T> where T : IRsc6Block, new()
    {
        public uint Position;
        public Rsc6ManagedArr<T> Array;

        public Rsc6PtrToPtrArr(Rsc6ManagedArr<T> arr)
        {
            Array = arr;
        }

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

        public void ReadAsArray(Rsc6DataReader reader, uint size)
        {
            var block = reader.ReadBytes((int)size);
            Value = Encoding.UTF8.GetString(block);
            Value = Value[..Value.LastIndexOf('\0')];
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

        public override string ToString()
        {
            return Value ?? "NULL";
        }
    }

    public struct Rsc6StrArr
    {
        public ulong Position;
        public Rsc6Str[] Items;

        public Rsc6StrArr(Rsc6Str[] items)
        {
            Position = 0;
            Items = items;
        }

        public Rsc6StrArr(string[] strings)
        {
            Position = 0;
            if (strings != null)
            {
                Items = new Rsc6Str[strings.Length];
                for (int i = 0; i < strings.Length; i++)
                {
                    Items[i] = new Rsc6Str(strings[i]);
                }
            }
        }

        public void ReadPtr(Rsc6DataReader reader)
        {
            Position = reader.ReadUInt32();
        }

        public void ReadItems(Rsc6DataReader reader, uint count)
        {
            var p = reader.Position;
            reader.Position = Position;

            Items = new Rsc6Str[count];
            for (int i = 0; i < count; i++)
            {
                Items[i] = reader.ReadStr();
            }
            reader.Position = p;
        }

        public void Write(Rsc6DataWriter writer)
        {
            var ptrs = (Items != null) ? new uint[Items.Length + 1] : null;
            writer.AddPointerRef(ptrs);
            writer.WriteUInt32((uint)Position);

            if (Items != null)
            {
                var ptrData = writer.WriteArray(ptrs);
                var offset = 0u;

                foreach (var item in Items)
                {
                    if (item.Value == null) continue;

                    var encoding = Encoding.UTF8;
                    var b = encoding.GetBytes(item.Value);
                    var len = b.Length + 1;
                    var data = new byte[len];
                    if (b != null)
                    {
                        Buffer.BlockCopy(b, 0, data, 0, b.Length);
                    }

                    writer.AddBlock(item, data);
                    writer.AddPointerRef(item, ptrData, offset);
                    offset += 4;
                }
            }
        }

        public readonly Rsc6Str this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public override string ToString()
        {
            return "Count: " + (Items?.Length.ToString() ?? "0");
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

        public void Read(Rsc6DataReader reader, uint size)
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
                    if (size > 0) //Used for rage::speedTreeDebugName (fixed-size string)
                    {
                        Items[i] = new Rsc6Str(reader.Position);
                        Items[i].ReadAsArray(reader, size);
                    }
                    else Items[i] = reader.ReadStr();
                }
                reader.Position = p;
            }
        }

        public void Write(Rsc6DataWriter writer, uint pad)
        {
            var ptrs = new uint[Capacity];
            if (pad > 0)
                writer.AddPointerRef(Items);
            else if (ptrs.Length > 0)
                writer.AddPointerRef(ptrs);

            writer.WriteUInt32((uint)Position);
            writer.WriteUInt16(Count);
            writer.WriteUInt16(Capacity);

            if (Items != null && Count > 0)
            {
                var encoding = Encoding.UTF8;
                var offset = 0ul;
                var data = writer.WriteArray(ptrs);

                if (pad > 0)
                {
                    var list = new List<byte>();
                    foreach (var item in Items)
                    {
                        if (item.Value == null) continue;
                        var temp = encoding.GetBytes(item.Value);
                        var len = temp.Length + 1;

                        if ((len % pad) != 0)
                        {
                            len += (ushort)(pad - (ushort)(len % pad));
                        }

                        var bytes = new byte[len];
                        Array.Copy(temp, bytes, temp.Length);
                        bytes[temp.Length] = 0; //Null terminator

                        for (int i = temp.Length + 1; i < len; i++)
                        {
                            bytes[i] = 0xCD; //Padding
                        }
                        list.AddRange(bytes);
                    }
                    writer.AddBlock(Items, list.ToArray());
                }
                else
                {
                    foreach (var item in Items)
                    {
                        if (item.Value == null) continue;

                        var bytes = encoding.GetBytes(item.Value);
                        var len = bytes.Length + 1;
                        var strData = new byte[len];

                        Array.Copy(bytes, strData, bytes.Length);
                        writer.AddBlock(item, strData);
                        writer.AddPointerRef(item, data, offset);
                        offset += 4ul;
                    }
                }
            }
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
            Rpf6Crypto.TransformToZXY(Items);
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

    public class Rsc6BlockMap : Rsc6BlockBase
    {
        public override ulong BlockLength => 4;
        public uint Block { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            Block = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Block);
        }
    }

    public abstract class Rsc6BlockBase : IRsc6Block
    {
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public abstract ulong BlockLength { get; }
        public abstract void Read(Rsc6DataReader reader);
        public abstract void Write(Rsc6DataWriter writer);
    }

    public abstract class Rsc6FileBase : Rsc6BlockBase
    {
        public abstract uint VFT { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(VFT);
        }
    }

    public abstract class Rsc6BlockBaseMap : Rsc6FileBase //rage::pgBase
    {
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(BlockMap);
        }
    }

    public abstract class Rsc6BlockBaseMapRef : Rsc6BlockBaseMap //rage::pgBaseRefCounted
    {
        public uint RefCount { get; set; } //m_RefCount

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            RefCount = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(RefCount);
        }

        public override string ToString()
        {
            return "References: " + RefCount.ToString();
        }
    }

    public static class Rsc6DataMap
    {
        public static List<T> Build<T>(List<T> entries, bool sortbuckets = false, bool reversebuckets = false, T[] extest = null) where T : IRsc6DataMapEntry<T>
        {
            if (entries.Count < 1 || ((entries.Count == 1) && (entries[0].MapKey == 0)))
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
                    if (b.Count > 1)
                    {
                        if (sortbuckets)
                        {
                            if (reversebuckets)
                            {
                                b.Sort((a, b) => b.MapKey.CompareTo(a.MapKey));
                            }
                            else
                            {
                                b.Sort((a, b) => a.MapKey.CompareTo(b.MapKey));
                            }
                        }
                        else if (reversebuckets)
                        {
                            b.Reverse();
                        }
                    }
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

            if (extest != null) //Just testing
            {
                var numtest = extest.Length;
                for (int i = 0; i < numtest; i++)
                {
                    var ot = extest[i];
                    var nt = result[i];

                    while (ot != null)
                    {
                        ot = ot.MapNext;
                        nt = nt.MapNext;
                    }
                }
            }
            return result;
        }

        public static List<U> Flatten<T, U>(T[] entries, Func<T, U> yieldFunc, Comparison<U> sortFunc = null) where T : IRsc6DataMapEntry<T>
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

        public static Dictionary<uint, U> GetDictionary<T, U>(T[] entries, Func<T, U> yieldFunc) where T : IRsc6DataMapEntry<T>
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

    public interface IRsc6Block : BlockBase
    {
        bool IsPhysical { get; }
        void Read(Rsc6DataReader reader);
        void Write(Rsc6DataWriter writer);
    }

    public interface IRsc6DataMapEntry<T>
    {
        uint MapKey { get; }
        T MapNext { get; set; }
    }
}