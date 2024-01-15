namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6ExpressionDictionary : Rsc6FileBase //pgDictionary<crExpressions>
    {
        public override ulong BlockLength => 32;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public uint Unknown_8h { get; set; } //Always 0
        public uint RefCount { get; set; } = 1; //m_RefCount
        public Rsc6Arr<uint> Hashes { get; set; }
        public Rsc6PtrArr<Rsc6Expressions> Expressions { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            Unknown_8h = reader.ReadUInt32();
            RefCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<uint>();
            Expressions = reader.ReadPtrArr<Rsc6Expressions>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x00D0E590);
            writer.WritePtr(BlockMap);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(RefCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Expressions);
        }
    }

    public class Rsc6Expressions : Rsc6FileBase //rage::crExpressions
    {
        public override ulong BlockLength => 36;
        public uint RefCount { get; set; } //m_RefCount
        public Rsc6PtrArr<Rsc6Expression> Expressions { get; set; } //m_Expressions
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } = 1; //Always 1, might be the expression version
        public uint Unknown_18h { get; set; } //Always 0
        public uint ExpressionFilter { get; set; } //m_ExpressionFilter
        public uint MaxPackedSize { get; set; } //m_MaxPackedSize, max length of any item

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            RefCount = reader.ReadUInt32();
            Expressions = reader.ReadPtrArr<Rsc6Expression>();
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            ExpressionFilter = reader.ReadUInt32();
            MaxPackedSize = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x00D4E824);
            writer.WriteUInt32(RefCount);
            writer.WritePtrArr(Expressions);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(ExpressionFilter);
            writer.WriteUInt32(MaxPackedSize);
        }
    }

    public class Rsc6Expression : Rsc6Block //rage::crExpression
    {
        public ulong BlockLength => 32;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public ushort Enabled { get; set; } //m_Enabled
        public ushort StackDepth { get; set; } //m_StackDepth
        public Rsc6Ptr<Rsc6ExpressionOp> ExpressionOp { get; set; } //m_ExpressionOp
        public uint Signature { get; set; } //m_Signature
        public uint PackedSize { get; set; } //m_PackedSize
        public Rsc6ManagedArr<Rsc6ExpressionIODof> InputOutputDofs { get; set; } //m_InputOutputDofs
        public ushort NumAcceleratedIndices { get; set; } //m_NumAcceleratedIndices
        public uint Unknown_1Ah { get; set; } //Always 0, padding
        public ushort Unknown_1Eh { get; set; } //Always 0, padding

        public void Read(Rsc6DataReader reader)
        {
            Enabled = reader.ReadUInt16();
            StackDepth = reader.ReadUInt16();
            ExpressionOp = reader.ReadPtr<Rsc6ExpressionOp>();
            Signature = reader.ReadUInt32();
            PackedSize = reader.ReadUInt32();
            InputOutputDofs = reader.ReadArr<Rsc6ExpressionIODof>();
            NumAcceleratedIndices = reader.ReadUInt16();
            Unknown_1Ah = reader.ReadUInt32();
            Unknown_1Eh = reader.ReadUInt16();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt16(Enabled);
            writer.WriteUInt16(StackDepth);
            writer.WritePtr(ExpressionOp);
            writer.WriteUInt32(Signature);
            writer.WriteUInt32(PackedSize);
            writer.WriteArr(InputOutputDofs);
            writer.WriteUInt16(NumAcceleratedIndices);
            writer.WriteUInt32(Unknown_1Ah);
            writer.WriteUInt16(Unknown_1Eh);
        }
    }

    public class Rsc6ExpressionOp : Rsc6Block //rage::crExpressionOp
    {
        public ulong BlockLength => 4;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public void Read(Rsc6DataReader reader) //TODO:
        {
            
        }

        public void Write(Rsc6DataWriter writer)
        {
            
        }
    }

    public class Rsc6ExpressionIODof : Rsc6Block //rage::crExpression::InputOutputDof
    {
        public ulong BlockLength => 12;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6Arr<uint> InputTrackIDs { get; set; } //m_InputTrackIds
        public uint OutputTrackID { get; set; } //m_OutputTrackId

        public void Read(Rsc6DataReader reader)
        {
            OutputTrackID = reader.ReadUInt32();
            InputTrackIDs = reader.ReadArr<uint>();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(OutputTrackID);
            writer.WriteArr(InputTrackIDs);
        }

        public override string ToString()
        {
            return $"InputTrackIDs count: {InputTrackIDs.Count}, OutputTrackID: {OutputTrackID}";
        }
    }
}