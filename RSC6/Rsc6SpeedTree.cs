using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using System.Collections.Generic;
using System.Numerics;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6TreeForestGrid : Rsc6FileBase
    {
        public override ulong BlockLength => 208;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6TreeForest TreeForest { get; set; }
        public Vector4 GridMin { get; set; }
        public Vector4 GridMax { get; set; }
        public Vector4 BoundSphere { get; set; } //m_BoundSphere
        public Rsc6CustomArr<Rsc6TreeForestGridCell> GridCells { get; set; } //m_GridCells
        public Rsc6Arr<ushort> IndexList { get; set; }
        public int Left { get; set; } //m_nLeft
        public int Right { get; set; } //m_nRight
        public int Top { get; set; } //m_nTop
        public int Bottom { get; set; } //m_nBottom
        public int WidthStep { get; set; } //m_nWidthStep
        public int HeightStep { get; set; } //m_nHeightStep
        public int Width { get; set; } //m_nWidth
        public int Height { get; set; } //m_nHeight
        public int CellsWidth { get; set; } //m_nCellsW
        public int CellsHeight { get; set; } //m_nCellsH
        public bool YUp { get; set; } //m_bYUp
        public bool EnableGrid { get; set; } //m_bEnableGrid
        public bool EntireCellCull { get; set; } //m_bEntireCellCull
        public bool LoadedMeshes { get; set; } //m_LoadedMeshes
        public float StreamRadius { get; set; } = 0xCDCDCDCD; //m_streamRadius, always NULL

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            TreeForest = reader.ReadBlock<Rsc6TreeForest>();
            GridMin = reader.ReadVector4();
            GridMax = reader.ReadVector4();
            BoundSphere = reader.ReadVector4();
            GridCells = reader.ReadArr<Rsc6TreeForestGridCell>();
            IndexList = reader.ReadArr<ushort>();
            Left = reader.ReadInt32();
            Right = reader.ReadInt32();
            Top = reader.ReadInt32();
            Bottom = reader.ReadInt32();
            WidthStep = reader.ReadInt32();
            HeightStep = reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            CellsWidth = reader.ReadInt32();
            CellsHeight = reader.ReadInt32();
            YUp = reader.ReadBoolean();
            EnableGrid = reader.ReadBoolean();
            EntireCellCull = reader.ReadBoolean();
            LoadedMeshes = reader.ReadBoolean();
            StreamRadius = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x04CA9264);
            writer.WritePtr(BlockMap);
            writer.WriteBlock(TreeForest);
            writer.WriteVector4(GridMin);
            writer.WriteVector4(GridMax);
            writer.WriteVector4(BoundSphere);
            writer.WriteArr(GridCells);
            writer.WriteArr(IndexList);
            writer.WriteInt32(Left);
            writer.WriteInt32(Right);
            writer.WriteInt32(Top);
            writer.WriteInt32(Bottom);
            writer.WriteInt32(WidthStep);
            writer.WriteInt32(HeightStep);
            writer.WriteInt32(Width);
            writer.WriteInt32(Height);
            writer.WriteInt32(CellsWidth);
            writer.WriteInt32(CellsHeight);
            writer.WriteBoolean(YUp);
            writer.WriteBoolean(EnableGrid);
            writer.WriteBoolean(EntireCellCull);
            writer.WriteBoolean(LoadedMeshes);
            writer.WriteSingle(StreamRadius);
        }
    }

    public class Rsc6TreeForestGridCell : Rsc6Block //rage::speedTreeForestGridCell
    {
        public ulong BlockLength => 48;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Vector4 BoundSphere { get; set; } //m_BoundSphere
        public Rsc6CustomArr<Rsc6PackedInstancePos> CombinedInstanceListPos { get; set; } //m_CombinedInstanceListPos
        public Rsc6CustomArr<Rsc6InstanceMatrix> CombinedInstanceListMatrix { get; set; } //m_CombinedInstanceListMtx
        public Rsc6Arr<short> IndexList { get; set; }
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Padding

        public void Read(Rsc6DataReader reader)
        {
            BoundSphere = reader.ReadVector4();
            CombinedInstanceListPos = reader.ReadArr<Rsc6PackedInstancePos>();
            CombinedInstanceListMatrix = reader.ReadArr<Rsc6InstanceMatrix>();
            IndexList = reader.ReadArr<short>();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(BoundSphere);
            writer.WriteArr(CombinedInstanceListPos);
            writer.WriteArr(CombinedInstanceListMatrix);
            writer.WriteArr(IndexList);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }
    }

    public class Rsc6TreeForest : Rsc6Block
    {
        public ulong BlockLength => 96;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6Arr<uint> Trees { get; set; } //m_Trees - rage::instanceTreeData
        public Rsc6Arr<JenkHash> TreeHashes { get; set; } //m_TreeSourceHashes
        public Rsc6PtrStr TreeNames { get; set; } //m_TreeDebugNames
        public Rsc6CustomArr<Rsc6TreeInstancePos> TreeInstancePos { get; set; } //m_TreeInstancesPos
        public Rsc6CustomArr<Rsc6PackedInstancePos> CombinedVisibleInstancePos { get; set; } //m_CombinedVisibleTreeInstancesPos
        public Rsc6CustomArr<Rsc6InstanceMatrix> TreeInstanceMatrix { get; set; } //m_TreeInstancesMtx
        public Rsc6CustomArr<Rsc6InstanceMatrix> CombinedVisibleTreeInstanceMatrix { get; set; } //m_CombinedVisibleTreeInstancesMtx
        public uint Unknown_40h { get; set; }
        public int MaxBillboardsPerFrame { get; set; } //m_MaxBillboardsPerFrame
        public float BillboardBlendRange { get; set; } //m_billboardBlendRange
        public bool RegenerateAll { get; set; } //m_RegenerateAll
        public bool UseBillboardSizeSelection { get; set; } //m_useBillboardSizeSelection
        public ushort Unknown_4Eh { get; set; } = 0xCDCD; //Padding
        public uint Unknown_50h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_54h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_58h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_5Ch { get; set; } = 0xCDCDCDCD; //Padding

        public void Read(Rsc6DataReader reader)
        {
            Trees = reader.ReadArr<uint>();
            TreeHashes = reader.ReadArr<JenkHash>();
            TreeNames = reader.ReadPtrStr(0x10);
            TreeInstancePos = reader.ReadArr<Rsc6TreeInstancePos>();
            CombinedVisibleInstancePos = reader.ReadArr<Rsc6PackedInstancePos>();
            TreeInstanceMatrix = reader.ReadArr<Rsc6InstanceMatrix>();
            CombinedVisibleTreeInstanceMatrix = reader.ReadArr<Rsc6InstanceMatrix>();
            Unknown_40h = reader.ReadUInt32();
            MaxBillboardsPerFrame = reader.ReadInt32();
            BillboardBlendRange = reader.ReadSingle();
            RegenerateAll = reader.ReadBoolean();
            UseBillboardSizeSelection = reader.ReadBoolean();
            Unknown_4Eh = reader.ReadUInt16();
            Unknown_50h = reader.ReadUInt32();
            Unknown_54h = reader.ReadUInt32();
            Unknown_58h = reader.ReadUInt32();
            Unknown_5Ch = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(Trees);
            writer.WriteArr(TreeHashes);
            writer.WritePtrStr(TreeNames);
            writer.WriteArr(TreeInstancePos);
            writer.WriteArr(CombinedVisibleInstancePos);
            writer.WriteArr(TreeInstanceMatrix);
            writer.WriteArr(CombinedVisibleTreeInstanceMatrix);
            writer.WriteUInt32(Unknown_40h);
            writer.WriteInt32(MaxBillboardsPerFrame);
            writer.WriteSingle(BillboardBlendRange);
            writer.WriteBoolean(RegenerateAll);
            writer.WriteBoolean(UseBillboardSizeSelection);
            writer.WriteUInt16(Unknown_4Eh);
            writer.WriteUInt32(Unknown_50h);
            writer.WriteUInt32(Unknown_54h);
            writer.WriteUInt32(Unknown_58h);
            writer.WriteUInt32(Unknown_5Ch);
        }
    }

    public class Rsc6PackedInstancePos : Rsc6Block //rage::speedTreePackedInstancePos
    {
        public ulong BlockLength => 8;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public ushort X { get; set; } //x
        public ushort Y { get; set; } //y
        public ushort Z { get; set; } //z
        public byte Fade { get; set; } //fade
        public byte Seed { get; set; } //seed

        public byte SeedValue //rage::speedTreePackedInstancePos::SetRot(byte)
        {
            get => Seed;
            set => Seed = (byte)((Seed & 0x3F) | (value << 6));
        }

        public Vector3 Position { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            X = reader.ReadUInt16();
            Y = reader.ReadUInt16();
            Z = reader.ReadUInt16();
            Fade = reader.ReadByte();
            Seed = reader.ReadByte();

            Position = new Vector3(X / 65535.0f, Y / 65535.0f, Z / 65535.0f);
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt16(X);
            writer.WriteUInt16(Y);
            writer.WriteUInt16(Z);
            writer.WriteByte(Fade);
            writer.WriteByte(Seed);
        }

        public override string ToString()
        {
            return $"Seed: {Seed} - X: {X}, Y: {Y}, Z: {Z}";
        }
    }

    public class Rsc6InstanceMatrix : Rsc6Block //rage::speedTreeInstanceMtx
    {
        public ulong BlockLength => 80;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6TreeInstanceBase InstanceBase { get; set; }
        public Matrix4x4 Transform { get; set; } //m_mTransform

        public void Read(Rsc6DataReader reader)
        {
            InstanceBase = reader.ReadBlock<Rsc6TreeInstanceBase>();
            Transform = reader.ReadMatrix4x4();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteBlock(InstanceBase);
            writer.WriteMatrix4x4(Transform);
        }

        public override string ToString()
        {
            return Transform.Translation.ToString();
        }
    }

    public class Rsc6TreeInstanceBase : Rsc6Block //rage::speedTreeInstanceBase
    {
        public ulong BlockLength => 16;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public float LODDist { get; set; } //m_fLOD
        public byte LODLevels { get; set; } //m_uLodLevels
        public byte Flags { get; set; } //m_uFlags
        public byte TreeTypeID { get; set; } //m_TreeTypeID
        public byte Pad0 { get; set; } //m_Pad0
        public float FadeOut { get; set; } //m_fadeOut
        public uint Pad1 { get; set; } //m_Pad1

        public void Read(Rsc6DataReader reader)
        {
            LODDist = reader.ReadSingle();
            LODLevels = reader.ReadByte();
            Flags = reader.ReadByte();
            TreeTypeID = reader.ReadByte();
            Pad0 = reader.ReadByte();
            FadeOut = reader.ReadSingle();
            Pad1 = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteSingle(LODDist);
            writer.WriteByte(LODLevels);
            writer.WriteByte(Flags);
            writer.WriteByte(TreeTypeID);
            writer.WriteByte(Pad0);
            writer.WriteSingle(FadeOut);
            writer.WriteUInt32(Pad1);
        }
    }

    public class Rsc6TreeInstancePos : Rsc6Block //rage::speedTreeInstancePos
    {
        public ulong BlockLength => 48;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6TreeInstanceBase InstanceBase { get; set; }
        public Vector4 Position { get; set; } //m_vPosition
        public ushort Tilt { get; set; } //m_tilt
        public ushort Width { get; set; } //m_width
        public byte Rotation { get; set; } //m_rot
        public ushort Unknown_19h { get; set; } = 0xCDCD; //Padding
        public byte Unknown_1Bh { get; set; } = 0xCD; //Padding
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_20h { get; set; } = 0xCDCDCDCD; //Padding

        public void Read(Rsc6DataReader reader)
        {
            InstanceBase = reader.ReadBlock<Rsc6TreeInstanceBase>();
            Position = reader.ReadVector4();
            Tilt = reader.ReadUInt16();
            Width = reader.ReadUInt16();
            Rotation = reader.ReadByte();
            Unknown_19h = reader.ReadUInt16();
            Unknown_1Bh = reader.ReadByte();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteBlock(InstanceBase);
            writer.WriteVector4(Position);
            writer.WriteUInt16(Tilt);
            writer.WriteUInt16(Width);
            writer.WriteByte(Rotation);
            writer.WriteUInt16(Unknown_19h);
            writer.WriteByte(Unknown_1Bh);
            writer.WriteUInt32(Unknown_1Ch);
            writer.WriteUInt32(Unknown_20h);
        }
    }
}