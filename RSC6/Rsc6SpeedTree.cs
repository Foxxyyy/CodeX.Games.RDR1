using System;
using System.Numerics;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6TreeForestGrid : Rsc6TreeForest //rage::TreeForestGrid
    {
        //Manages a forest using a grid culling system.
        //Each instance is put into a grid cell based on its position.
        //Entire grid cells are rejected with a single sphere visiblity

        public override ulong BlockLength => base.BlockLength + 112;
        public Vector4 GridMin { get; set; }
        public Vector4 GridSize { get; set; }
        public BoundingSphere BoundSphere { get; set; } //m_BoundSphere, W is radius
        public Rsc6ManagedArr<Rsc6TreeForestGridCell> GridCells { get; set; } //m_GridCells
        public Rsc6Arr<short> IndexList { get; set; }
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

        public BoundingBox BoundingBox => new(GridMin.XYZ(), GridMin.XYZ() + GridSize.XYZ());

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            GridMin = reader.ReadVector4();
            GridSize = reader.ReadVector4();
            var vector = reader.ReadVector4();
            BoundSphere = new BoundingSphere(vector.XYZ(), vector.W);
            GridCells = reader.ReadArr<Rsc6TreeForestGridCell>(); //Rsc6TreeForestGridCell[CellsWidth][CellsHeight]
            IndexList = reader.ReadArr<short>();
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

            foreach (var cell in GridCells.Items)
            {
                var instances = cell.CombinedInstanceListPos.Items;
                var indices = cell.IndexList.Items;

                if (instances != null)
                {
                    var size = BoundingBox.Size;
                    var min = BoundingBox.Minimum;

                    for (int i = 0; i < instances.Length; i++)
                    {
                        var inst = instances[i];
                        var pos = new Vector3(inst.Z, inst.X, inst.Y) / 65535.0f; //[0..1]
                        var scaledPos = Vector3.Multiply(size, pos);
                        var treeIndex = 0;

                        for (int index = 2; index < indices.Length; index += 2)
                        {
                            if (i < indices[index])
                            {
                                treeIndex = indices[index - 1];
                                break;
                            }
                        }
                        inst.Position = min + scaledPos;
                        inst.TreeIndex = treeIndex;
                    }
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4(GridMin);
            writer.WriteVector4(GridSize);
            writer.WriteVector4(new Vector4(BoundSphere.Center, BoundSphere.Radius));
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

    [TC(typeof(EXP))] public class Rsc6TreeForestGridCell : Rsc6BlockBase //rage::speedTreeForestGridCell
    {
        public override ulong BlockLength => 48;
        public BoundingSphere BoundSphere { get; set; } //m_BoundSphere, W is the max distance from the center to any instance (radius)
        public Rsc6ManagedArr<Rsc6PackedInstancePos> CombinedInstanceListPos { get; set; } //m_CombinedInstanceListPos
        public Rsc6ManagedArr<Rsc6InstanceMatrix> CombinedInstanceListMatrix { get; set; } //m_CombinedInstanceListMtx
        public Rsc6Arr<short> IndexList { get; set; }
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            var vector = reader.ReadVector4();
            BoundSphere = new BoundingSphere(vector.XYZ(), vector.W);
            CombinedInstanceListPos = reader.ReadArr<Rsc6PackedInstancePos>();
            CombinedInstanceListMatrix = reader.ReadArr<Rsc6InstanceMatrix>();
            IndexList = reader.ReadArr<short>();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(new Vector4(BoundSphere.Center, BoundSphere.Radius));
            writer.WriteArr(CombinedInstanceListPos);
            writer.WriteArr(CombinedInstanceListMatrix);
            writer.WriteArr(IndexList);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public override string ToString()
        {
            return BoundSphere.ToString() + ", instances: " + (CombinedInstanceListPos.Count + CombinedInstanceListMatrix.Count).ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6TreeForest : Rsc6BlockBaseMap //rage::TreeForest
    {
        public override ulong BlockLength => 96;
        public override uint VFT { get; set; } = 0x04CA9264;
        public Rsc6Arr<uint> Trees { get; set; } //m_Trees - rage::instanceTreeData
        public Rsc6Arr<JenkHash> TreeHashes { get; set; } //m_TreeSourceHashes, different from TreeNames, they generate a hash based on the tree filename + seed + tree size
        public Rsc6PtrStr TreeNames { get; set; } //m_TreeDebugNames, fixed-size strings
        public Rsc6ManagedArr<Rsc6TreeInstancePos> TreeInstancePos { get; set; } //m_TreeInstancesPos
        public Rsc6ManagedArr<Rsc6PackedInstancePos> CombinedVisibleInstancePos { get; set; } //m_CombinedVisibleTreeInstancesPos
        public Rsc6ManagedArr<Rsc6InstanceMatrix> TreeInstanceMatrix { get; set; } //m_TreeInstancesMtx
        public Rsc6ManagedArr<Rsc6InstanceMatrix> CombinedVisibleTreeInstanceMatrix { get; set; } //m_CombinedVisibleTreeInstancesMtx
        public int Unknown_40h { get; set; } //Always 0
        public int MaxBillboardsPerFrame { get; set; } //m_MaxBillboardsPerFrame
        public float BillboardBlendRange { get; set; } //m_billboardBlendRange
        public bool RegenerateAll { get; set; } //m_RegenerateAll
        public bool UseBillboardSizeSelection { get; set; } //m_useBillboardSizeSelection
        public ushort Unknown_4Eh { get; set; } = 0xCDCD; //Padding
        public uint Unknown_50h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_54h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_58h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_5Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Trees = reader.ReadArr<uint>();
            TreeHashes = reader.ReadArr<JenkHash>();
            TreeNames = reader.ReadPtrStr(32);
            TreeInstancePos = reader.ReadArr<Rsc6TreeInstancePos>();
            CombinedVisibleInstancePos = reader.ReadArr<Rsc6PackedInstancePos>();
            TreeInstanceMatrix = reader.ReadArr<Rsc6InstanceMatrix>();
            CombinedVisibleTreeInstanceMatrix = reader.ReadArr<Rsc6InstanceMatrix>();
            Unknown_40h = reader.ReadInt32();
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

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteArr(Trees);
            writer.WriteArr(TreeHashes);
            writer.WritePtrStr(TreeNames, 0x10);
            writer.WriteArr(TreeInstancePos);
            writer.WriteArr(CombinedVisibleInstancePos);
            writer.WriteArr(TreeInstanceMatrix);
            writer.WriteArr(CombinedVisibleTreeInstanceMatrix);
            writer.WriteInt32(Unknown_40h);
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

    [TC(typeof(EXP))] public class Rsc6PackedInstancePos : Rsc6BlockBase //rage::speedTreePackedInstancePos
    {
        public override ulong BlockLength => 8;
        public ushort X { get; set; } //x
        public ushort Y { get; set; } //y
        public ushort Z { get; set; } //z
        public byte Fade { get; set; } //fade
        public byte Seed { get; set; } //seed

        public Vector3 Position;
        public int TreeIndex;

        public Rsc6PackedInstancePos()
        {
        }

        public Rsc6PackedInstancePos(Vector3 pos)
        {
            Position = pos;
            Fade = 80;
            Seed = 27;
        }

        public override void Read(Rsc6DataReader reader)
        {
            X = reader.ReadUInt16();
            Y = reader.ReadUInt16();
            Z = reader.ReadUInt16();
            Fade = reader.ReadByte();
            Seed = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt16(X);
            writer.WriteUInt16(Y);
            writer.WriteUInt16(Z);
            writer.WriteByte(Fade);
            writer.WriteByte(Seed);
        }

        public override string ToString()
        {
            return Position.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6InstanceMatrix : Rsc6TreeInstanceBase //rage::speedTreeInstanceMtx
    {
        public override ulong BlockLength => base.BlockLength + 64;
        public Matrix4x4 Transform { get; set; } //m_mTransform

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Transform = reader.ReadMatrix4x4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteMatrix4x4(Transform);
        }

        public override string ToString()
        {
            return Transform.Translation.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6TreeInstanceBase : Rsc6BlockBase //rage::speedTreeInstanceBase
    {
        //This is the data needed to exist an instance of a speedtree in the world

        public override ulong BlockLength => 16;
        public float LODDist { get; set; } //m_fLOD, lod value between 0.0f (lowest) and 1.0f (highest)
        public byte LODLevels { get; set; } //m_uLodLevels, LOD Levels[0-1] - 4 bits each
        public Rsc6TreeInstanceFlags Flags { get; set; } //m_uFlags
        public byte TreeTypeID { get; set; } //m_TreeTypeID
        public byte Pad0 { get; set; } = 0xCD; //m_Pad0
        public float FadeOut { get; set; } = 0xCDCDCDCD; //m_fadeOut
        public uint Pad1 { get; set; } = 0xCDCDCDCD; //m_Pad1

        public override void Read(Rsc6DataReader reader)
        {
            LODDist = reader.ReadSingle();
            LODLevels = reader.ReadByte();
            Flags = (Rsc6TreeInstanceFlags)reader.ReadByte();
            TreeTypeID = reader.ReadByte();
            Pad0 = reader.ReadByte();
            FadeOut = reader.ReadSingle();
            Pad1 = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteSingle(LODDist);
            writer.WriteByte(LODLevels);
            writer.WriteByte((byte)Flags);
            writer.WriteByte(TreeTypeID);
            writer.WriteByte(Pad0);
            writer.WriteSingle(FadeOut);
            writer.WriteUInt32(Pad1);
        }

        public int GetBranchLod() //Get the descreet branch level of detail from the speedtree runtime
        {
            return (int)(LODLevels & 0xF);
        }

        public int GetFrondLod() //Get the descreet frond level of detail from the speedtree runtime
        {
            return (int)(LODLevels >> 4);
        }

        public int GetLeafLod() //Get the descreet leaf level of detail from the speedtree runtime
        {
            return (int)((byte)Flags >> 4);
        }

        public bool IsValid() //Test to see if this instance is valid
        {
            return (Flags & Rsc6TreeInstanceFlags.FLAG_VALID) != 0;
        }

        public bool IsVisible() //Test to see if this instance is visible
        {
            return (Flags & Rsc6TreeInstanceFlags.FLAG_VISIBLE) != 0;
        }

        public bool IsBillboardActive() //Test to see if this instance should draw a billboard
        {
            return (Flags & Rsc6TreeInstanceFlags.FLAG_BILLBOARD_ACTIVE) != 0;
        }

        public bool IsBillboardOnly() //Test to see if this instance is only a billboard
        {
            return (Flags & Rsc6TreeInstanceFlags.FLAG_BILLBOARD_ONLY) != 0;
        }

        public int DiscreetLod(int numLevels, float lodLevel)
        {
            short sLevel = 0;
            int nNumLodLevels = numLevels;

            sLevel = (short)((1.0f - lodLevel) * nNumLodLevels);
            if (sLevel == nNumLodLevels)
            {
                sLevel--;
            }

            if (sLevel < 0 || sLevel >= numLevels)
            {
                throw new InvalidOperationException("Invalid operation: sLevel >= 0 && sLevel < NumLevels");
            }

            return sLevel;
        }

        public void SetLod(Vector4 lods, float fLod)
        {
            LODDist = fLod;
            LODLevels = (byte)((byte)DiscreetLod((int)lods.X, fLod) | ((byte)DiscreetLod((int)lods.Y, fLod) << 4));
            Flags &= (Rsc6TreeInstanceFlags)0xF0;
            Flags |= (Rsc6TreeInstanceFlags)DiscreetLod((int)lods.Z, fLod);
        }
    }

    [TC(typeof(EXP))] public class Rsc6TreeInstancePos : Rsc6BlockBase //rage::speedTreeInstancePos
    {
        public override ulong BlockLength => 48;
        public Rsc6TreeInstanceBase InstanceBase { get; set; }
        public Vector4 Position { get; set; } //m_vPosition, Position.W is the rotation in the Y axis, other axis are 0.0f (radians)
        public ushort Tilt { get; set; } //m_tilt
        public ushort Width { get; set; } //m_width
        public byte Rotation { get; set; } //m_rot
        public ushort Unknown_19h { get; set; } = 0xCDCD; //Padding
        public byte Unknown_1Bh { get; set; } = 0xCD; //Padding
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_20h { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
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

        public override void Write(Rsc6DataWriter writer)
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

    [TC(typeof(EXP))] public struct TreeItem
    {
        public object Instance;
        public Vector3 Position;
        public JenkHash Hash;

        public override string ToString()
        {
            return Hash.ToString();
        }
    }

    [Flags] public enum Rsc6TreeInstanceFlags
    {
        FLAG_VISIBLE = 1 << 4,
        FLAG_BILLBOARD_ONLY = 1 << 5,
        FLAG_BILLBOARD_ACTIVE = 1 << 6,
        FLAG_VALID = 1 << 7
    };
}