using System;
using System.Numerics;
using CodeX.Core.Engine;
using CodeX.Core.Utilities;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6ClipDictionary : Rsc6FileBase
    {
        public override ulong BlockLength => 28;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public uint RefCount { get; set; } = 1; //m_RefCount
        public Rsc6Ptr<Rsc6AnimDictionary> AnimDictionaryOwner { get; set; } //m_AnimDictionaryOwner
        public uint BaseNameKeys { get; set; } = 1; //m_BaseNameKeys

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            RefCount = reader.ReadUInt32();
            AnimDictionaryOwner = reader.ReadPtr<Rsc6AnimDictionary>();
            BaseNameKeys = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            
        }
    }

    public class Rsc6Clip : Rsc6FileBase
    {
        public override ulong BlockLength => 28;

        public override void Read(Rsc6DataReader reader)
        {
            
        }

        public override void Write(Rsc6DataWriter writer)
        {

        }
    }

    public class Rsc6AnimDictionary : Rsc6FileBase
    {
        public override ulong BlockLength => 24;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public uint RefCount { get; set; } //m_RefCount, always 0?
        public Rsc6PtrArr<Rsc6AnimDictionaryEntry> Animations { get; set; } //m_Animations
        public ushort Unknown_14h { get; set; } = 0xCDCD; //Pad
        public byte Unknown_16h { get; set; } = 0xCD; //Pad
        public byte BaseNameKeys { get; set; } //m_BaseNameKeys, always 1?

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            RefCount = reader.ReadUInt32();
            Animations = reader.ReadPtrArr<Rsc6AnimDictionaryEntry>(); //atMap<rage::crAnimation>
            Unknown_14h = reader.ReadUInt16();
            Unknown_16h = reader.ReadByte();
            BaseNameKeys = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x01322FA4);
        }
    }

    public class Rsc6AnimDictionaryEntry : Rsc6BlockBase
    {
        public override ulong BlockLength => 12;
        public uint MapKey { get => Hash; set => Hash = value; }
        public Rsc6AnimDictionaryEntry MapNext { get => Next.Item; set => Next = new(value); }

        public JenkHash Hash; //Name hash of the anim
        public Rsc6Ptr<Rsc6Animation> Anim;
        public Rsc6Ptr<Rsc6AnimDictionaryEntry> Next;

        public override void Read(Rsc6DataReader reader)
        {
            Hash = reader.ReadUInt32();
            Anim = reader.ReadPtr<Rsc6Animation>();
            Next = reader.ReadPtr<Rsc6AnimDictionaryEntry>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            
        }

        public override string ToString()
        {
            return $"Animation: {Anim.Item?.Name}, Duration: {Anim.Item?.Duration}";
        }
    }

    public class Rsc6Animation : Animation, Rsc6Block, MetaNode //rage::crAnimation
    {
        /*
         * Animations represent the change in a series of values (tracks) over a period of time (duration)
         * The crAnimation class hides the internal storage of these tracks, the channels and compression used
         * The animation also constructs a parallel structure of blocks and chunks, to help organize the memory layout of the animation data in a more temporal fashion
         */

        public ulong BlockLength => 48;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public uint VFT { get; set; }
        public Rsc6AnimationFlags Flags { get; set; } //m_Flags, RAGE flags
        public ushort ProjectFlags { get; set; } //m_ProjectFlags, project specific flags
        public ushort NumFrames { get; set; } //m_NumFrames, internal frame count (including terminating zero duration frame)
        public ushort FramesPerChunk { get; set; } = 127; //m_FramesPerChunk, frames per chunk (excluding any terminating frames)
        public uint Signature { get; set; } //m_Signature, animation signature (a hash of the animation structure)
        public Rsc6PtrArr<Rsc6AnimBlock> Blocks { get; set; } //m_Blocks, rage::crAnimBlock
        public Rsc6Str NameRef { get; set; } //m_Name
        public Rsc6ManagedArr<Rsc6AnimTrack> Tracks { get; set; } //m_Tracks, animation tracks, structurally organized view of the compressed animation data
        public ulong MaxBlockSize { get; set; } //m_MaxBlockSize, always NULL, size of the largest block in the animation (in bytes, 16 byte aligned) if the data is packed
        public ulong RefCount { get; set; } //m_RefCount

        public static uint DefaultFramesPerChunk = 127;

        public void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Flags = (Rsc6AnimationFlags)reader.ReadUInt16();
            ProjectFlags = reader.ReadUInt16();
            NumFrames = reader.ReadUInt16();
            FramesPerChunk = reader.ReadUInt16();
            Duration = reader.ReadSingle(); //Inherited from Animation, duration of the animation in seconds
            Signature = reader.ReadUInt32();
            Blocks = reader.ReadPtrArr<Rsc6AnimBlock>();
            NameRef = reader.ReadStr();
            Tracks = reader.ReadArr<Rsc6AnimTrack>();
            MaxBlockSize = reader.ReadUInt32();
            RefCount = reader.ReadUInt32();

            Name = NameRef.Value;
        }

        public void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? (uint)0x00FA2AF0 : 0x011C2AF0);
        }

        public void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public void Write(MetaNodeWriter writer)
        {
            throw new NotImplementedException();
        }

        //Estimated total storage used, in bytes
        //Memory alignment/fragmentation can cause the animation to use more than this estimated total
        public uint ComputeSize()
        {
            uint size = (uint)BlockLength;
            foreach (var block in Blocks.Items)
            {
                //size += block.GetBlockSize();
            }

            foreach (var track in Tracks.Items)
            {
                //size += track.ComputeSize() + 16; //16 = Rsc6AnimTrack.BlockLength
            }

            if (Name != null)
            {
                size += (uint)Name.Length + 1;
            }
            return size;
        }

        //Calculates a signature (a hash) representing all the tracks (their tracks/ids/types)
        //Anim signatures allow frames to be quickly compared to determine if their structures, but not their values, are equal
        //This allows operations between animations and frames to have optimized paths, when they are structurally identical
        public void CalcSignature()
        {
            int idx = 0;
            uint len = Tracks.Count;
            uint sum1 = 0xFFFF, sum2 = 0xFFFF;

            while (len > 0)
            {
                uint tlen = len > 180 ? 180 : len;
                len -= tlen;
                do
                {
                    var track = Tracks[idx++];
                    //ushort data = (ushort)((track.GetTrack() << 8) | (track.GetType() | 0x80));

                    //sum1 += data;
                    sum2 += sum1;
                    //data = track.ID;
                    //sum1 += data;
                    sum2 += sum1;
                }
                while (--tlen > 0);

                sum1 = (sum1 & 0xFFFF) + (sum1 >> 16);
                sum2 = (sum2 & 0xFFFF) + (sum2 >> 16);
            }
            sum1 = (sum1 & 0xFFFF) + (sum1 >> 16);
            sum2 = (sum2 & 0xFFFF) + (sum2 >> 16);
            Signature = (sum2 << 16) | sum1;
        }
    }

    public class Rsc6AnimBlock : Rsc6Block //rage::crAnimBlock
    {
        public ulong BlockLength => 32;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public uint Offset { get; set; } //Points to the current block, if ((Flags & 1) == 0) we read Chunks
        public Rsc6PtrArr<Rsc6AnimChunk> Chunks { get; set; } //m_Chunks, rage::crAnimChunk
        public uint BlockSize { get; set; } //m_BlockSize
        public ushort NumFrames { get; set; } //m_NumFrames
        public ushort Flags { get; set; } //m_Flags
        public uint CompactBlockSize { get; set; } //m_CompactBlockSize
        public uint CompactSlopSize { get; set; } //m_CompactSlopSize
        public uint Unknown_1Ch { get; set; } //Always 0? 31750

        public void Read(Rsc6DataReader reader)
        {
            Offset = reader.ReadUInt32();
            Chunks = reader.ReadPtrArr<Rsc6AnimChunk>();
            BlockSize = reader.ReadUInt32();
            NumFrames = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            CompactBlockSize = reader.ReadUInt32();
            CompactSlopSize = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Offset);
            writer.WritePtrArr(Chunks);
            writer.WriteUInt32(BlockSize);
            writer.WriteUInt16(NumFrames);
            writer.WriteUInt16(Flags);
            writer.WriteUInt32(CompactBlockSize);
            writer.WriteUInt32(CompactSlopSize);
            writer.WriteUInt32(Unknown_1Ch);
        }
    }

    public class Rsc6AnimTrack : Rsc6Block //rage::crAnimTrack
    {
        /*
         * Internal storage class of crAnimation
         * Tracks represent change in single value, which may be of type float/vector3/quaternion etc over the entire duration of an animation
         * Internally tracks hold their values in a series of one of more chunks, which then in turn compress their data within one or more channels
         * Chunking and compressing of data is deliberately hidden from the end user.
         */

        public ulong BlockLength => 16;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public byte Track { get; set; } //m_Track, track index
        public byte Type { get; set; } //m_Type, track ID
        public ushort ID { get; set; } //m_Id, track ID
        public ushort FramesPerChunk { get; set; } //m_FramesPerChunk, number of internal frames per chunk
        public ushort Flags { get; set; } //m_Flags
        public Rsc6ManagedArr<Rsc6AnimChunk> Chunks { get; set; } //m_Chunks, rage::crAnimChunk

        public void Read(Rsc6DataReader reader)
        {
            Track = reader.ReadByte();
            Type = reader.ReadByte();
            ID = reader.ReadUInt16();
            //FramesPerChunk = reader.ReadByte();
            //Flags = reader.ReadUInt16();
            Chunks = reader.ReadArr<Rsc6AnimChunk>();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteByte(Track);
            writer.WriteByte(Type);
            writer.WriteUInt16(ID);
            writer.WriteUInt16(FramesPerChunk);
            writer.WriteUInt16(Flags);
            writer.WriteArr(Chunks);
        }

        public uint CalcNumChunks(uint numFrames)
        {
	        if(numFrames > 1)
	        {
		        return ((numFrames - 2) / FramesPerChunk) + 1;
	        }
	        return 1;
        }

        public override string ToString()
        {
            return $"ID: {ID}, {FramesPerChunk} channels";
        }
    }

    public class Rsc6AnimChunk : Rsc6Block //rage::crAnimChunk
    {
        /*
         * Animation chunks represent the change in a value over a short period of time
         * They may store compound types (ie vectors, quaternions) or basic types (ie floats, integers etc)
         * The period of time may be that of the entire animation, or some short subsection of it
         * Internally they pack this changing value using one or more channels, which can use a variety of different compression techniques
         */

        public ulong FilePosition { get; set; }
        public ulong BlockLength => 16;
        public bool IsPhysical => false;
        public byte Track { get; set; } //m_Track
        public byte Format { get; set; } //m_Format
        public ushort ID { get; set; } //m_Id
        public Rsc6ManagedArr<Rsc6AnimChannel> Channels { get; set; } //m_Channels

        public void Read(Rsc6DataReader reader)
        {
            Track = reader.ReadByte();
            Format = reader.ReadByte();
            ID = reader.ReadUInt16();
            //Channels = reader.ReadArr<Rsc6AnimChannel>();
        }

        public void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6AnimChannel : Rsc6ChannelAttribute, Rsc6Block //rage::crAnimChannel
    {
        /*
         * Animation channels represent the change in value over time of a single type.
         * They may store compound types (ie vectors, quaternions) or basic types (ie floats, integers etc)
         * There are many different types of channel, for storing all the different value types, and using different compression techniques
         */

        public Rsc6ChannelAttribute Attribute;
        public static int attempt = 0;

        public override void Read(Rsc6DataReader reader)
        {
            try
            {
                base.Read(reader);
                Attribute = Create(ChannelType);
                Attribute.Read(reader);
                attempt++;
            }
            catch { }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            Attribute.Write(writer);
        }

        public Rsc6ChannelAttribute Create(Rsc6AnimChannelType type)
        {
            return type switch
            {
                Rsc6AnimChannelType.StaticQuaternion => new Rsc6ChannelStaticQuaternion(),
                Rsc6AnimChannelType.StaticVector3 => new Rsc6ChannelStaticVector3(),
                Rsc6AnimChannelType.StaticFloat => new Rsc6ChannelStaticFloat(),
                //Rsc6AnimChannelType.RawFloat => new Rsc6ChannelRawFloat(),
                Rsc6AnimChannelType.QuantizeFloat => new Rsc6ChannelQuantizeFloat(),
                /*case Rsc6AnimChannelType.IndirectQuantizeFloat: return new Rsc6ChannelIndirectQuantizeFloat();
                case Rsc6AnimChannelType.LinearFloat: return new Rsc6ChannelLinearFloat();*/
                _ => throw new NotImplementedException($"Rsc6ChannelAttribute: Unknown type: {type}")
            };
        }
    }

    public abstract class Rsc6ChannelAttribute : Rsc6Block
    {
        public virtual ulong FilePosition { get; set; }
        public virtual ulong BlockLength => 8;
        public virtual bool IsPhysical => false;

        public byte Flags;
        public byte Type;
        public byte Unknown_2h;
        public ushort Unknown_3h; //Padding?
        public Rsc6AnimChannelType ChannelType;
        public byte CompressCost;
        public byte DecompressCost;

        public virtual void Read(Rsc6DataReader reader)
        {
            Flags = reader.ReadByte();
            Type = reader.ReadByte();
            Unknown_2h = reader.ReadByte();
            Unknown_3h = reader.ReadUInt16();
            ChannelType = (Rsc6AnimChannelType)reader.ReadByte();
            CompressCost = reader.ReadByte();
            DecompressCost = reader.ReadByte();
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            writer.WriteByte(Flags);
            writer.WriteByte(Type);
            writer.WriteByte(Unknown_2h);
            writer.WriteUInt16(Unknown_3h);
            writer.WriteByte((byte)ChannelType);
            writer.WriteByte(CompressCost);
            writer.WriteByte(DecompressCost);
        }
    }

    public class Rsc6ChannelStaticVector3 : Rsc6ChannelAttribute //rage::crAnimChannelStaticVector3
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6PtrUnmanaged<Vector3> Vector { get; set; }
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            Vector = reader.ReadPtrUnmanaged<Vector3>();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrUnmanaged(Vector);
            writer.WriteUInt32(Unknown_Ch);
        }
    }

    public class Rsc6ChannelStaticFloat : Rsc6ChannelAttribute //rage::crAnimChannelStaticFloat
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6PtrUnmanaged<float> Value { get; set; }
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            Value = reader.ReadPtrUnmanaged<float>();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrUnmanaged(Value);
            writer.WriteUInt32(Unknown_Ch);
        }
    }

    public class Rsc6ChannelStaticQuaternion : Rsc6ChannelAttribute //rage::crAnimChannelStaticQuaternion
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6PtrUnmanaged<Quaternion> Quaternion { get; set; }
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            Quaternion = reader.ReadPtrUnmanaged<Quaternion>();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrUnmanaged(Quaternion);
            writer.WriteUInt32(Unknown_Ch);
        }
    }

    public class Rsc6ChannelRawFloat : Rsc6ChannelAttribute //rage::crAnimChannelRawFloat
    {
        public float Value { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            Value = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteSingle(Value);
        }
    }

    public class Rsc6ChannelQuantizeFloat : Rsc6ChannelAttribute //rage::crAnimChannelQuantizeFloat
    {
        public override ulong BlockLength => base.BlockLength + 24;
        public Rsc6Arr<uint> QuantizedValues { get; set; } //m_QuantizedValues
        public float Scale { get; set; } //m_Scale
        public float Offset { get; set; } //m_Offset
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            QuantizedValues = reader.ReadArr<uint>(true); //atPackedArray
            Scale = reader.ReadSingle();
            Offset = reader.ReadSingle();
            Unknown_Ch = reader.ReadUInt32();

            var floats = new float[QuantizedValues.Count];
            for (int i = 0; i < QuantizedValues.Count; i++)
            {
                var value = QuantizedValues[i];
                float test = (value * Scale) + Offset;
                floats[i] = test;
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(QuantizedValues, true); //TODO: When initializing 'Values', need to set count/capacity as 4 bytes values instead of 2
            writer.WriteSingle(Scale);
            writer.WriteSingle(Offset);
            writer.WriteUInt32(Unknown_Ch);
        }
    }

    public enum Rsc6AnimChannelType : byte
    {
        None = 0,
        RawFloat = 1,
        Vector3 = 2,
        Quaternion = 3,
        StaticFloat = 4,
        CurveFloat = 5,
        QuantizeFloat = 6,
        RawInt = 7,
        RawBool = 8,
        StaticQuaternion = 9,
        DeltaFloat = 10,
        StaticInt = 11,
        RleInt = 12,
        StaticVector3 = 13,
        SmallestThreeQuaternion = 14,
        VariableQuantizeFloat = 15,
        IndirectQuantizeFloat = 16,
        LinearFloat = 17,
        QuadraticBSpline = 18,
        CubicBSpline = 19,
        StaticSmallestThreeQuaternion = 20,
        Num = 21
    }

    public enum Rsc6AnimationFlags
    {
        kLooped = 1 << 0,
        kRaw = 1 << 3,
        kMoverTracks = 1 << 4,
        kPacked = 1 << 8,
        kCompact = 1 << 10,
        kNonSerializableMask = kPacked | kCompact
    }
}