using System;
using System.Linq;
using System.Numerics;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6TreeForestGrid : Rsc6TreeForest, MetaNode //rage::TreeForestGrid
    {
        //Manages a forest using a grid culling system.
        //Each instance is put into a grid cell based on its position.
        //Entire grid cells are rejected with a single sphere visiblity

        public override ulong BlockLength => base.BlockLength + 112;
        public Vector4 GridMin { get; set; }
        public Vector4 GridSize { get; set; }
        public Vector4 BoundSphere { get; set; } //m_BoundSphere, W is radius
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
            BoundSphere = reader.ReadVector4();
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

        public new void Read(MetaNodeReader reader)
        {
            GridMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("GridMin"));
            GridSize = Rpf6Crypto.ToXYZ(reader.ReadVector4("GridMax"));
            BoundSphere = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundSphere"));
            Left = reader.ReadInt32("Left");
            Right = reader.ReadInt32("Right");
            Top = reader.ReadInt32("Top");
            Bottom = reader.ReadInt32("Bottom");
            WidthStep = reader.ReadInt32("WidthStep");
            HeightStep = reader.ReadInt32("HeightStep");
            Width = reader.ReadInt32("Width");
            Height = reader.ReadInt32("Height");
            CellsWidth = reader.ReadInt32("CellsWidth");
            CellsHeight = reader.ReadInt32("CellsHeight");
            YUp = reader.ReadBool("YUp");
            EnableGrid = reader.ReadBool("EnableGrid");
            EntireCellCull = reader.ReadBool("EntireCellCull");
            LoadedMeshes = reader.ReadBool("LoadedMeshes");
            StreamRadius = reader.ReadSingle("StreamRadius");
            base.Read(reader);
            GridCells = new(reader.ReadNodeArray<Rsc6TreeForestGridCell>("GridCells"));

            var iList = reader.ReadInt16Array("IndexList");
            if (iList != null)
            {
                IndexList = new(iList);
            }
        }

        public new void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("GridMin", GridMin);
            writer.WriteVector4("GridMax", GridSize);
            writer.WriteVector4("BoundSphere", BoundSphere);
            writer.WriteInt32("Left", Left);
            writer.WriteInt32("Right", Right);
            writer.WriteInt32("Top", Top);
            writer.WriteInt32("Bottom", Bottom);
            writer.WriteInt32("WidthStep", WidthStep);
            writer.WriteInt32("HeightStep", HeightStep);
            writer.WriteInt32("Width", Width);
            writer.WriteInt32("Height", Height);
            writer.WriteInt32("CellsWidth", CellsWidth);
            writer.WriteInt32("CellsHeight", CellsHeight);
            writer.WriteBool("YUp", YUp);
            writer.WriteBool("EnableGrid", EnableGrid);
            writer.WriteBool("EntireCellCull", EntireCellCull);
            writer.WriteBool("LoadedMeshes", LoadedMeshes);
            writer.WriteSingle("StreamRadius", StreamRadius);
            base.Write(writer);
            writer.WriteNodeArray("GridCells", GridCells.Items);
            writer.WriteInt16Array("IndexList", IndexList.Items);
        }

        //Get the coordinates of the grid cell where a position is in.
        public void GetGridCell(Vector3 vPosition, out int nCellW, out int nCellH)
        {
            if (YUp)
            {
                nCellW = (int)((vPosition.X - Left) / WidthStep);
                nCellH = (int)((vPosition.Z - Top) / HeightStep);
            }
            else
            {
                nCellW = ((int)vPosition.X - Left) / WidthStep;
                nCellH = ((int)vPosition.Y - Top) / HeightStep;
            }
        }
    }

    [TC(typeof(EXP))] public class Rsc6TreeForestGridCell : Rsc6BlockBase, MetaNode //rage::speedTreeForestGridCell
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

        public void Read(MetaNodeReader reader)
        {
            var vector = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundSphere"));
            BoundSphere = new BoundingSphere(vector.XYZ(), vector.W);
            CombinedInstanceListPos = new(reader.ReadNodeArray<Rsc6PackedInstancePos>("CombinedInstanceListPos"));
            CombinedInstanceListMatrix = new(reader.ReadNodeArray<Rsc6InstanceMatrix>("CombinedInstanceListMatrix"));

            var iList = reader.ReadInt16Array("IndexList");
            if (iList != null)
            {
                IndexList = new(iList);
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("BoundSphere", new Vector4(BoundSphere.Center, BoundSphere.Radius));
            writer.WriteNodeArray("CombinedInstanceListPos", CombinedInstanceListPos.Items);
            writer.WriteNodeArray("CombinedInstanceListMatrix", CombinedInstanceListMatrix.Items);
            writer.WriteInt16Array("IndexList", IndexList.Items);
        }

        public override string ToString()
        {
            return BoundSphere.ToString() + ", instances: " + (CombinedInstanceListPos.Count + CombinedInstanceListMatrix.Count).ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6TreeForest : Rsc6BlockBaseMap, MetaNode //rage::TreeForest
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
            TreeNames = reader.ReadPtrStr(0x10);
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

        public void Read(MetaNodeReader reader)
        {
            var names = reader.ReadStringArray("TreeNames") ?? Array.Empty<string>();
            TreeNames = new(names.Select(s => new Rsc6Str(s)).ToArray());
            TreeHashes = new(reader.ReadJenkHashArray("TreeHashes"));
            TreeInstancePos = new(reader.ReadNodeArray<Rsc6TreeInstancePos>("TreeInstancePos"));
            CombinedVisibleInstancePos = new(reader.ReadNodeArray<Rsc6PackedInstancePos>("CombinedVisibleInstancePos"));
            TreeInstanceMatrix = new(reader.ReadNodeArray<Rsc6InstanceMatrix>("TreeInstanceMatrix"));
            CombinedVisibleTreeInstanceMatrix = new(reader.ReadNodeArray<Rsc6InstanceMatrix>("CombinedVisibleTreeInstanceMatrix"));
            MaxBillboardsPerFrame = reader.ReadInt32("MaxBillboardsPerFrame");
            BillboardBlendRange = reader.ReadSingle("BillboardBlendRange");
            RegenerateAll = reader.ReadBool("RegenerateAll");
            UseBillboardSizeSelection = reader.ReadBool("UseBillboardSizeSelection");

            var treeValues = new uint[names.Length];
            for (int i = 0; i < treeValues.Length; i++)
            {
                treeValues[i] = uint.MaxValue;
            }
            Trees = new(treeValues);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteStringArray("TreeNames", TreeNames.Items.Select(s => s.Value).ToArray());
            writer.WriteJenkHashArray("TreeHashes", TreeHashes.Items);
            writer.WriteNodeArray("TreeInstancePos", TreeInstancePos.Items);
            writer.WriteNodeArray("CombinedVisibleInstancePos", CombinedVisibleInstancePos.Items);
            writer.WriteNodeArray("TreeInstanceMatrix", TreeInstanceMatrix.Items);
            writer.WriteNodeArray("CombinedVisibleTreeInstanceMatrix", CombinedVisibleTreeInstanceMatrix.Items);
            writer.WriteInt32("MaxBillboardsPerFrame", MaxBillboardsPerFrame);
            writer.WriteSingle("BillboardBlendRange", BillboardBlendRange);
            writer.WriteBool("RegenerateAll", RegenerateAll);
            writer.WriteBool("UseBillboardSizeSelection", UseBillboardSizeSelection);
        }
    }

    [TC(typeof(EXP))] public class Rsc6PackedInstancePos : Rsc6BlockBase, MetaNode //rage::speedTreePackedInstancePos
    {
        public override ulong BlockLength => 8;
        public ushort X { get; set; } //x
        public ushort Y { get; set; } //y
        public ushort Z { get; set; } //z
        public byte Fade { get; set; } //fade
        public byte Seed { get; set; } //seed

        public Vector3 Position;
        public int TreeIndex;

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

        public void Read(MetaNodeReader reader)
        {
            X = reader.ReadUInt16("Y");
            Y = reader.ReadUInt16("Z");
            Z = reader.ReadUInt16("X");
            Fade = reader.ReadByte("Fade");
            Seed = reader.ReadByte("Seed");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt16("X", Z);
            writer.WriteUInt16("Y", X);
            writer.WriteUInt16("Z", Y);
            writer.WriteByte("Fade", Fade);
            writer.WriteByte("Seed", Seed);
        }

        public override string ToString()
        {
            return $"Seed: {Seed} - X: {X}, Y: {Y}, Z: {Z}";
        }
    }

    [TC(typeof(EXP))] public class Rsc6InstanceMatrix : Rsc6TreeInstanceBase, MetaNode //rage::speedTreeInstanceMtx
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

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Transform = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("Transform"), true);
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteMatrix4x4("Transform", Transform);
        }

        public override string ToString()
        {
            return Transform.Translation.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6TreeInstanceBase : Rsc6BlockBase, MetaNode //rage::speedTreeInstanceBase
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

        public void Read(MetaNodeReader reader)
        {
            LODDist = reader.ReadSingle("LODDist");
            LODLevels = reader.ReadByte("LODLevels");
            Flags = reader.ReadEnum<Rsc6TreeInstanceFlags>("Flags");
            TreeTypeID = reader.ReadByte("TreeTypeID");
            FadeOut = reader.ReadSingle("FadeOut");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteSingle("LODDist", LODDist);
            writer.WriteByte("LODLevels", LODLevels);
            writer.WriteEnum("Flags", Flags);
            writer.WriteByte("TreeTypeID", TreeTypeID);
            writer.WriteSingle("FadeOut", FadeOut);
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

    [TC(typeof(EXP))] public class Rsc6TreeInstancePos : Rsc6BlockBase, MetaNode //rage::speedTreeInstancePos
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

        public void Read(MetaNodeReader reader)
        {
            InstanceBase = reader.ReadNode<Rsc6TreeInstanceBase>("InstanceBase");
            Position = Rpf6Crypto.ToXYZ(reader.ReadVector4("Position"));
            Tilt = reader.ReadUInt16("Tilt");
            Width = reader.ReadUInt16("Width");
            Rotation = reader.ReadByte("Rotation");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("InstanceBase", InstanceBase);
            writer.WriteVector4("Position", Position);
            writer.WriteUInt16("Tilt", Tilt);
            writer.WriteUInt16("Width", Width);
            writer.WriteByte("Rotation", Rotation);
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