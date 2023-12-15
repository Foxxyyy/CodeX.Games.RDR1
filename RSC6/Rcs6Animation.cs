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
        public Rsc6Ptr<Rsc6Anim> Anim;
        public Rsc6Ptr<Rsc6AnimDictionaryEntry> Next;

        public override void Read(Rsc6DataReader reader)
        {
            Hash = reader.ReadUInt32();
            Anim = reader.ReadPtr<Rsc6Anim>();
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

    public class Rsc6Anim : Animation, Rsc6Block
    {
        public ulong BlockLength => 48;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public uint VFT;
        public ushort Flags; //m_Flags
        public ushort ProjectFlags; //m_ProjectFlags
        public ushort NumFrames; //m_NumFrames
        public ushort FramesPerChunk; //m_FramesPerChunk
        public uint Signature; //m_Signature
        public Rsc6PtrArr<Rsc6AnimBlock> Blocks; //m_Blocks, rage::crAnimBlock
        public Rsc6Str NameRef; //m_Name
        public uint Tracks; //m_Tracks, rage::crAnimTrack
        public ulong MaxBlockSize; //m_MaxBlockSize
        public ulong RefCount; //m_RefCount

        public void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Flags = reader.ReadUInt16();
            ProjectFlags = reader.ReadUInt16();
            NumFrames = reader.ReadUInt16();
            FramesPerChunk = reader.ReadUInt16();
            Duration = reader.ReadSingle();
            Signature = reader.ReadUInt32(); //2435658708, 303367174, 84017665, etc
            Blocks = reader.ReadPtrArr<Rsc6AnimBlock>(); //rage::crAnimBlock
            NameRef = reader.ReadStr();
            Tracks = reader.ReadUInt32(); //rage::crAnimTrack
            var tracksCount = reader.ReadUInt16();
            var tracksCapacity = reader.ReadUInt16();
            MaxBlockSize = reader.ReadUInt32();
            RefCount = reader.ReadUInt32();

            Name = NameRef.Value;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x011C2AF0);
        }
    }

    public class Rsc6AnimBlock : Rsc6Block
    {
        public ulong BlockLength => 32;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public uint Offset { get; set; } //Points to the current block, if ((Flags & 1) == 0) we read Chunks
        public Rsc6PtrArr<Rsc6AnimTrack> Chunks { get; set; } //m_Chunks, rage::crAnimChunk
        public uint BlockSize { get; set; } //m_BlockSize
        public ushort NumFrames { get; set; } //m_NumFrames
        public ushort Flags { get; set; } //m_Flags
        public uint CompactBlockSize { get; set; } //m_CompactBlockSize
        public uint CompactSlopSize { get; set; } //m_CompactSlopSize
        public uint Unknown_1Ch { get; set; } //Always 0?

        public void Read(Rsc6DataReader reader)
        {
            Offset = reader.ReadUInt32();
            Chunks = reader.ReadPtrArr<Rsc6AnimTrack>();
            BlockSize = reader.ReadUInt32();
            NumFrames = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            CompactBlockSize = reader.ReadUInt32();
            CompactSlopSize = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            
        }
    }

    public class Rsc6AnimTrack : Rsc6FileBase
    {
        public override ulong BlockLength => 16;

        public byte ID { get; set; } //m_Id
        public byte FramesPerChunk { get; set; } //m_FramesPerChunk
        public ushort Flags { get; set; } //m_Flags
        public ushort Chunks { get; set; } //m_Chunks, rage::crAnimChunk
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD; //Padding

        public Rsc6AnimChannel[] Channels { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            FramesPerChunk = reader.ReadByte();
            Flags = reader.ReadUInt16();
            //Chunks = reader.ReadPtr<Rsc6AnimChunk>();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x011C4468);
        }

        public override string ToString()
        {
            return $"{FramesPerChunk} channels";
        }
    }
    public abstract class Rsc6AnimChannel
    {
        /*public Rsc6AnimChannelType Type;
        public int Track;
        public int Index;
        public int DataOffset;
        public int FrameOffset;
        public int FrameBits;
        public virtual int RefIndex => Index;//used for root channel refs

        public virtual void ReadHeader(Rsc6AnimSequenceReader reader) { }
        public virtual void ReadFrame(Rsc6AnimSequenceReader reader) { }
        public virtual void WriteHeader(Rsc6AnimSequenceWriter writer) { }
        public virtual void WriteFrame(Rsc6AnimSequenceWriter writer) { }

        public static Rsc6AnimChannel Create(Rsc6AnimChannelType type)
        {
            switch (type)
            {
                case Rsc6AnimChannelType.StaticQuaternion: return new Rsc6AnimChannelStaticQuaternion();
                case Rsc6AnimChannelType.StaticVector3: return new Rsc6AnimChannelStaticVector3();
                case Rsc6AnimChannelType.StaticFloat: return new Rsc6AnimChannelStaticFloat();
                case Rsc6AnimChannelType.RawFloat: return new Rsc6AnimChannelRawFloat();
                case Rsc6AnimChannelType.QuantizeFloat: return new Rsc6AnimChannelQuantizeFloat();
                case Rsc6AnimChannelType.IndirectQuantizeFloat: return new Rsc6AnimChannelIndirectQuantizeFloat();
                case Rsc6AnimChannelType.LinearFloat: return new Rsc6AnimChannelLinearFloat();
                case Rsc6AnimChannelType.CachedQuaternion1: return new Rsc6AnimChannelCachedQuaternion(type);
                case Rsc6AnimChannelType.CachedQuaternion2: return new Rsc6AnimChannelCachedQuaternion(type);
                case Rsc6AnimChannelType.Unknown1: return new Rsc6AnimChannelUnknown1();
                default: return null;
            }
        }*/
    }


    public enum Rsc6AnimChannelType : byte //From RDR2
    {
        StaticQuaternion = 0,
        StaticVector3 = 1,
        StaticFloat = 2,
        RawFloat = 3,
        QuantizeFloat = 4,
        IndirectQuantizeFloat = 5,
        LinearFloat = 6,
        CachedQuaternion1 = 7,
        CachedQuaternion2 = 8,
        Unknown1 = 9,
        _Count = 10,
    }
}