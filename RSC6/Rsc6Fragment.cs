using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using System;
using System.Linq;
using System.Numerics;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6Fragment : Rsc6Block, MetaNode
    {
        /*
         * These will not do anything unless the game utilizes m_UnbrokenElasticity :
         * - DampingLinearC
         * - DampingLinearV
         * - DampingLinearV2
         * - DampingAngularC
         * - DampingAngularV
         * - DampingAngularV2
         */

        public ulong FilePosition { get; set; }
        public ulong BlockLength => 348 + 164; //Actually sagFragtype + rage::fragType
        public bool IsPhysical => false;
        public uint VFT { get; set; }
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public float SmallestAngInertia { get; set; } //m_SmallestAngInertia
        public float LargestAngInertia { get; set; } //m_LargestAngInertia
        public Vector4 BoundingSphere { get; set; } //m_BoundingSphere
        public Vector4 RootCGOffset { get; set; } //m_RootCGOffset
        public Vector4 OriginalRootCGOffset { get; set; } //m_OriginalRootCGOffset
        public Vector4 UnbrokenCGOffset { get; set; } //m_UnbrokenCGOffset
        public Vector4 DampingLinearC { get; set; }
        public Vector4 DampingLinearV { get; set; }
        public Vector4 DampingLinearV2 { get; set; }
        public Vector4 DampingAngularC { get; set; }
        public Vector4 DampingAngularV { get; set; }
        public Vector4 DampingAngularV2 { get; set; }
        public Rsc6Str NameRef { get; set; } //m_TuneName
        public Rsc6Ptr<Rsc6FragDrawable> Drawable { get; set; } //m_CommonDrawable, contains data common to all the parts of the fragment type, the shader groups, etc.
        public Rsc6Ptr<Rsc6FragDrawable> ExtraDrawables { get; set; } //m_ExtraDrawables
        public uint Unk1 { get; set; } //m_NumExtraDrawables
        public int UnkInt { get; set; } //m_ExtraDrawableNames
        public int UnkInt2 { get; set; }
        public uint DamagedDrawable { get; set; } //m_DamagedDrawable, rage::fragTypeChild, when health value reaches zero, the piece can be swapped for a damaged version, which can also take more damage for further mesh deformation and texture adjustment
        public Rsc6RawLst<Rsc6String> GroupNames { get; set; } //m_GroupNames
        public Rsc6RawPtrArr<Rsc6FragPhysGroup> Groups { get; set; } //m_Groups, rage::fragTypeGroup
        public Rsc6RawPtrArr<Rsc6FragPhysChild> Children { get; set; } //m_Children, rage::fragTypeChild
        public uint UnkArray { get; set; } //m_EnvCloth, rage::fragTypeEnvCloth
        public ushort UnkArrayCount { get; set; } //m_Count
        public ushort UnkArraySize { get; set; } //m_Capacity
        public uint CharCloth { get; set; } //m_CharCloth, rage::fragTypeCharCloth
        public Rsc6Ptr<Rsc6FragArchetypeDamp> Archetype1 { get; set; } //rage::phArchetypeDamp
        public Rsc6Ptr<Rsc6FragArchetypeDamp> Archetype2 { get; set; } //rage::phArchetypeDamp
        public Rsc6Ptr<Rsc6Bounds> Bounds { get; set; } //m_CompositeBounds
        public Rsc6RawArr<Vector4> UndamagedAngInertia { get; set; } //m_UndamagedAngInertia
        public Rsc6RawArr<Vector4> DamagedAngInertia { get; set; } //m_DamagedAngInertia
        public Rsc6RawArr<Matrix3x4> LinkAttachments { get; set; } //m_LinkAttachments
        public uint MinBreakingImpulses { get; set; } //m_MinBreakingImpulses
        public Rsc6RawArr<byte> SelfCollisionA { get; set; } //m_SelfCollisionA
        public Rsc6RawArr<byte> SelfCollisionB { get; set; } //m_SelfCollisionB
        public uint UserData { get; set; } //m_UserData
        public uint JointParams { get; set; } //m_JointParams, rage::fragType
        public uint CollisionEventset { get; set; } //m_CollisionEventset, rage::evtSet
        public uint CollisionEventPlayer { get; set; } //m_CollisionEventPlayer, rage::evtPlayer
        public uint AnimFrame { get; set; } //m_AnimFrame, rage::crFrame
        public uint Skeleton1 { get; set; } //rage::crSkeleton
        public uint Skeleton2 { get; set; } //rage::crSkeleton
        public uint SharedMatrixSet { get; set; } //m_SharedMatrixSet
        public uint EstimatedCacheSize { get; set; } //m_EstimatedCacheSizes
        public uint EstimatedArticulatedCacheSize { get; set; } //m_EstimatedArticulatedCacheSize
        public byte NumSelfCollisions { get; set; } //m_NumSelfCollisions
        public byte MaxNumSelfCollisions { get; set; } //m_MaxNumSelfCollisions
        public byte GroupCount { get; set; } //m_NumGroups
        public byte ChildCount { get; set; } //m_NumChildren
        public byte FragTypeGroupCount { get; set; } //m_RootGroupCount
        public byte DamageRegions { get; set; } //m_NumRootDamageRegions
        public byte ChildCount2 { get; set; } //m_NumBonyChildren
        public byte EntityClass { get; set; } //m_EntityClass
        public byte ARTAssetID { get; set; } //m_ARTAssetID
        public byte AttachBottomEnd { get; set; } //m_AttachBottomEnd
        public Rsc6FragTypeFlags Flags { get; set; } //m_Flags
        public int ClientClassID { get; set; } //m_ClientClassID
        public float MinMoveForce { get; set; } //m_MinMoveForce
        public float UnbrokenElasticity { get; set; } //m_UnbrokenElasticity
        public float BuoyancyFactor { get; set; } //m_BuoyancyFactor
        public uint Unknown_14Ch { get; set; } //Always 0
        public byte GlassAttachmentBone { get; set; } //m_GlassAttachmentBone, seems to always be 0 (root)
        public byte NumGlassPaneModelInfos { get; set; } //m_NumGlassPaneModelInfos
        public ushort Unknown_152h { get; set; } //Always 0
        public uint GlassPaneModelInfos { get; set; } //m_GlassPaneModelInfos, rage::bgPaneModelInfoBase, used for windows (p_win), array of pointers using 'NumGlassPaneModelInfos'
        public float GravityFactor { get; set; } = 1.0f; //m_GravityFactor
        public uint Unknown_15Ch { get; set; }
        public Rsc6PtrToPtrArr<Rsc6AssociationInfo> AssociatedFragments { get; set; } //m_AssociatedFragments
        public uint Unknown_164h { get; set; }
        public uint ChildDataSet { get; set; } //m_ChildDataSet, sagFragTypeChildDataSet
        public uint Unknown_16Ch { get; set; }
        public bool HasTextureLOD { get; set; } //m_HasTextureLOD
        public bool HasFragLOD { get; set; } //m_HasFragLOD
        public bool HasAnimNormalMap { get; set; } //m_HasAnimNrmMap
        public byte[] Unknown_173h { get; set; }
        public Rsc6Str ParentTextureLOD { get; set; } //m_ParentTextureLOD, used for hats
        public int GlassGlowShaderIndex { get; set; } = -1; //m_GlassGlowShaderIndex
        public uint TargetManager { get; set; } //m_TargetManager, rage::grbTargetManager
        public byte[] VariableMeshArray1 { get; set; } //Used in scripts with 'ACTOR_ENABLE_VARIABLE_MESH' for enabling mesh components
        public byte[] VariableMeshArray2 { get; set; }
        public byte[] VariableMeshArray3 { get; set; }
        public byte VariableMeshCount { get; set; } //m_VariableMeshCount
        public byte AlwaysAddToShadow { get; set; } = 0xCD; //m_AlwayAddToShadow
        public byte InnerSorting { get; set; } = 0xCD; //m_InnerSorting
        public Rsc6Ptr<Rsc6TextureDictionary> Textures { get; set; } //m_BuiltInTextureDictionary
        public uint PlacedLightsGroup { get; set; } //m_PlacedLightsGroup
        public JenkHash TuneNameHash { get; set; } //m_TuneNameHash
        public ushort Unknown_1FCh { get; set; } = 0xCDCD; //Padding
        public bool NoSnow { get; set; } //m_NoSnow
        public bool ForceOutSide { get; set; } //m_ForceOutSide

        public void Read(Rsc6DataReader reader) //sagFragType
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();

            //Loading textures before drawables
            reader.Position += 0x1E8;
            Textures = Files.WfdFile.TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>();
            PlacedLightsGroup = reader.ReadUInt32();
            TuneNameHash = reader.ReadUInt32();
            Unknown_1FCh = reader.ReadUInt16();
            NoSnow = reader.ReadBoolean();
            ForceOutSide = reader.ReadBoolean();
            reader.Position -= 0x1F8;

            SmallestAngInertia = reader.ReadSingle();
            LargestAngInertia = reader.ReadSingle();
            BoundingSphere = reader.ReadVector4();
            RootCGOffset = reader.ReadVector4();
            OriginalRootCGOffset = reader.ReadVector4();
            UnbrokenCGOffset = reader.ReadVector4();
            DampingLinearC = reader.ReadVector4();
            DampingLinearV = reader.ReadVector4();
            DampingLinearV2 = reader.ReadVector4();
            DampingAngularC = reader.ReadVector4();
            DampingAngularV = reader.ReadVector4();
            DampingAngularV2 = reader.ReadVector4();
            NameRef = reader.ReadStr();
            Drawable = reader.ReadPtr<Rsc6FragDrawable>();
            ExtraDrawables = reader.ReadPtr<Rsc6FragDrawable>();
            Unk1 = reader.ReadUInt32();
            UnkInt = reader.ReadInt32();
            UnkInt2 = reader.ReadInt32();
            DamagedDrawable = reader.ReadUInt32();
            GroupNames = reader.ReadRawLstPtr<Rsc6String>();
            Groups = reader.ReadRawPtrArrPtr<Rsc6FragPhysGroup>();
            Children = reader.ReadRawPtrArrPtr<Rsc6FragPhysChild>();
            UnkArray = reader.ReadUInt32();
            UnkArrayCount = reader.ReadUInt16();
            UnkArraySize = reader.ReadUInt16();
            CharCloth = reader.ReadUInt32();
            Archetype1 = reader.ReadPtr<Rsc6FragArchetypeDamp>();
            Archetype2 = reader.ReadPtr<Rsc6FragArchetypeDamp>();
            Bounds = reader.ReadPtr(Rsc6Bounds.Create);
            UndamagedAngInertia = reader.ReadRawArrPtr<Vector4>();
            DamagedAngInertia = reader.ReadRawArrPtr<Vector4>();
            LinkAttachments = reader.ReadRawArrPtr<Matrix3x4>();
            MinBreakingImpulses = reader.ReadUInt32();
            SelfCollisionA = reader.ReadRawArrPtr<byte>();
            SelfCollisionB = reader.ReadRawArrPtr<byte>();
            UserData = reader.ReadUInt32();
            JointParams = reader.ReadUInt32();
            CollisionEventset = reader.ReadUInt32();
            CollisionEventPlayer = reader.ReadUInt32();
            AnimFrame = reader.ReadUInt32();
            Skeleton1 = reader.ReadUInt32();
            Skeleton2 = reader.ReadUInt32();
            SharedMatrixSet = reader.ReadUInt32();
            EstimatedCacheSize = reader.ReadUInt32();
            EstimatedArticulatedCacheSize = reader.ReadUInt32();
            NumSelfCollisions = reader.ReadByte();
            MaxNumSelfCollisions = reader.ReadByte();
            GroupCount = reader.ReadByte();
            ChildCount = reader.ReadByte();
            FragTypeGroupCount = reader.ReadByte();
            DamageRegions = reader.ReadByte();
            ChildCount2 = reader.ReadByte();
            EntityClass = reader.ReadByte();
            ARTAssetID = reader.ReadByte();
            AttachBottomEnd = reader.ReadByte();
            Flags = (Rsc6FragTypeFlags)reader.ReadUInt16();
            ClientClassID = reader.ReadInt32();
            MinMoveForce = reader.ReadSingle();
            UnbrokenElasticity = reader.ReadSingle();
            BuoyancyFactor = reader.ReadSingle();
            Unknown_14Ch = reader.ReadUInt32();
            GlassAttachmentBone = reader.ReadByte();
            NumGlassPaneModelInfos = reader.ReadByte();
            Unknown_152h = reader.ReadUInt16();
            GlassPaneModelInfos = reader.ReadUInt32();
            GravityFactor = reader.ReadSingle();
            Unknown_15Ch = reader.ReadUInt32();
            AssociatedFragments = reader.ReadPtrToItem<Rsc6AssociationInfo>();
            Unknown_164h = reader.ReadUInt32();
            ChildDataSet = reader.ReadUInt32();
            Unknown_16Ch = reader.ReadUInt32();
            HasTextureLOD = reader.ReadBoolean();
            HasFragLOD = reader.ReadBoolean();
            HasAnimNormalMap = reader.ReadBoolean();
            Unknown_173h = reader.ReadBytes(17); //???
            ParentTextureLOD = reader.ReadStr();
            GlassGlowShaderIndex = reader.ReadInt32();
            TargetManager = reader.ReadUInt32();
            VariableMeshArray1 = reader.ReadBytes(31);
            VariableMeshArray2 = reader.ReadBytes(31);
            VariableMeshArray3 = reader.ReadBytes(31);
            VariableMeshCount = reader.ReadByte();
            AlwaysAddToShadow = reader.ReadByte();
            InnerSorting = reader.ReadByte();

            AssociatedFragments = reader.ReadItems(AssociatedFragments);
            Groups = reader.ReadRawPtrArrItem(Groups, GroupCount);
            Children = reader.ReadRawPtrArrItem(Children, ChildCount);
            UndamagedAngInertia = reader.ReadRawArrItems(UndamagedAngInertia, ChildCount);
            DamagedAngInertia = reader.ReadRawArrItems(DamagedAngInertia, ChildCount);
            LinkAttachments = reader.ReadRawArrItems(LinkAttachments, ChildCount);
            SelfCollisionA = reader.ReadRawArrItems(SelfCollisionA, NumSelfCollisions);
            SelfCollisionB = reader.ReadRawArrItems(SelfCollisionB, NumSelfCollisions);
        }

        public void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            writer.WriteSingle("SmallestAngInertia", SmallestAngInertia);
            writer.WriteSingle("LargestAngInertia", LargestAngInertia);
            writer.WriteVector4("BoundingSphere", BoundingSphere);
            writer.WriteVector4("RootCGOffset", RootCGOffset);
            writer.WriteVector4("OriginalRootCGOffset", OriginalRootCGOffset);
            writer.WriteVector4("UnbrokenCGOffset", UnbrokenCGOffset);
            writer.WriteVector4("DampingLinearC", DampingLinearC);
            writer.WriteVector4("DampingLinearV", DampingLinearV);
            writer.WriteVector4("DampingLinearV2", DampingLinearV2);
            writer.WriteVector4("DampingAngularC", DampingAngularC);
            writer.WriteVector4("DampingAngularV", DampingAngularV);
            writer.WriteVector4("DampingAngularV2", DampingAngularV2);
            if (NameRef.Value != null) writer.WriteString("Name", NameRef.Value);
            if (Drawable.Item != null) writer.WriteNode("Drawable", Drawable.Item);
            if (ExtraDrawables.Item != null) writer.WriteNode("ExtraDrawables", ExtraDrawables.Item);
            writer.WriteUInt32("Unk1", Unk1);
            writer.WriteInt32("UnkInt", UnkInt);
            writer.WriteInt32("UnkInt2", UnkInt2);
            writer.WriteUInt32("DamagedDrawable", DamagedDrawable);
            if (GroupNames.Items != null) writer.WriteStringArray("GroupNames", GroupNames.Items.Select(s => s.Value).ToArray());
            if (Groups.Items != null) writer.WriteNodeArray("Groups", Groups.Items);
            if (Children.Items != null) writer.WriteNodeArray("Children", Children.Items);
            writer.WriteUInt32("UnkArray", UnkArray);
            writer.WriteUInt16("UnkArrayCount", UnkArrayCount);
            writer.WriteUInt16("UnkArraySize", UnkArraySize);
            writer.WriteUInt32("CharCloth", CharCloth);
            if (Archetype1.Item != null) writer.WriteNode("Archetype1", Archetype1.Item);
            if (Archetype2.Item != null) writer.WriteNode("Archetype2", Archetype2.Item);
            //if (Bounds.Item != null) writer.WriteNode("Bounds", Bounds.Item); //not yet
            if (UndamagedAngInertia.Items != null) writer.WriteVector4Array("UndamagedAngInertia", UndamagedAngInertia.Items);
            if (DamagedAngInertia.Items != null) writer.WriteVector4Array("DamagedAngInertia", DamagedAngInertia.Items);
            if (LinkAttachments.Items != null) writer.WriteMatrix3x4Array("LinkAttachments", LinkAttachments.Items);
            writer.WriteUInt32("MinBreakingImpulses", MinBreakingImpulses);
            if (SelfCollisionA.Items != null) writer.WriteByteArray("SelfCollisionA", SelfCollisionA.Items);
            if (SelfCollisionB.Items != null) writer.WriteByteArray("SelfCollisionB", SelfCollisionB.Items);
            writer.WriteUInt32("UserData", UserData);
            writer.WriteUInt32("JointParams", JointParams);
            writer.WriteUInt32("CollisionEventset", CollisionEventset);
            writer.WriteUInt32("CollisionEventPlayer", CollisionEventPlayer);
            writer.WriteUInt32("AnimFrame", AnimFrame);
            writer.WriteUInt32("Skeleton1", Skeleton1);
            writer.WriteUInt32("Skeleton2", Skeleton2);
            writer.WriteUInt32("SharedMatrixSet", SharedMatrixSet);
            writer.WriteUInt32("EstimatedCacheSize", EstimatedCacheSize);
            writer.WriteUInt32("EstimatedArticulatedCacheSize", EstimatedArticulatedCacheSize);
            writer.WriteByte("NumSelfCollisions", NumSelfCollisions);
            writer.WriteByte("MaxNumSelfCollisions", MaxNumSelfCollisions);
            writer.WriteByte("GroupCount", GroupCount);
            writer.WriteByte("ChildCount", ChildCount);
            writer.WriteByte("FragTypeGroupCount", FragTypeGroupCount);
            writer.WriteByte("DamageRegions", DamageRegions);
            writer.WriteByte("ChildCount2", ChildCount2);
            writer.WriteByte("EntityClass", EntityClass);
            writer.WriteByte("ARTAssetID", ARTAssetID);
            writer.WriteByte("AttachBottomEnd", AttachBottomEnd);
            writer.WriteUInt16("Flags", (ushort)Flags);
            writer.WriteInt32("ClientClassID", ClientClassID);
            writer.WriteSingle("MinMoveForce", MinMoveForce);
            writer.WriteSingle("UnbrokenElasticity", UnbrokenElasticity);
            writer.WriteSingle("BuoyancyFactor", BuoyancyFactor);
            writer.WriteUInt32("Unknown_14Ch", Unknown_14Ch);
            writer.WriteByte("GlassAttachmentBone", GlassAttachmentBone);
            writer.WriteByte("NumGlassPaneModelInfos", NumGlassPaneModelInfos);
            writer.WriteUInt16("Unknown_152h", Unknown_152h);
            writer.WriteUInt32("GlassPaneModelInfos", GlassPaneModelInfos);
            writer.WriteSingle("GravityFactor", GravityFactor);
            writer.WriteUInt32("Unknown_15Ch", Unknown_15Ch);
            if (AssociatedFragments.Array.Items != null) writer.WriteNodeArray("AssociatedFragments", AssociatedFragments.Array.Items);
            writer.WriteUInt32("Unknown_164h", Unknown_164h);
            writer.WriteUInt32("ChildDataSet", ChildDataSet);
            writer.WriteUInt32("Unknown_16Ch", Unknown_16Ch);
            writer.WriteBool("HasTextureLOD", HasTextureLOD);
            writer.WriteBool("HasFragLOD", HasFragLOD);
            writer.WriteBool("HasAnimNormalMap", HasAnimNormalMap);
            writer.WriteByteArray("Unknown_173h", Unknown_173h);
            if (ParentTextureLOD.Value != null) writer.WriteString("ParentTextureLOD", ParentTextureLOD.Value);
            writer.WriteInt32("GlassGlowShaderIndex", GlassGlowShaderIndex);
            writer.WriteUInt32("TargetManager", TargetManager);
            writer.WriteByteArray("VariableMeshArray1", VariableMeshArray1);
            writer.WriteByteArray("VariableMeshArray2", VariableMeshArray2);
            writer.WriteByteArray("VariableMeshArray3", VariableMeshArray3);
            writer.WriteByte("VariableMeshCount", VariableMeshCount);
            writer.WriteByte("AlwaysAddToShadow", AlwaysAddToShadow);
            writer.WriteByte("InnerSorting", InnerSorting);
        }

        public void SetFlags(ushort bits, bool set)
        {
            if (set)
                Flags |= (Rsc6FragTypeFlags)(bits & ushort.MaxValue);
            else
                Flags &= (Rsc6FragTypeFlags)(~bits & ushort.MaxValue);
        }

        public bool IsBreakingDisabled()
        {
	        return (Flags & Rsc6FragTypeFlags.DISABLE_BREAKING) != 0;
        }

        public bool IsActivationDisabled()
        {
	        return (Flags & Rsc6FragTypeFlags.DISABLE_ACTIVATION) != 0;
        }

        public bool GetNeedsCacheEntryToActivate()
        {
	        return (Flags & Rsc6FragTypeFlags.NEEDS_CACHE_ENTRY_TO_ACTIVATE) != 0;
        }

        public bool GetHasAnyArticulatedParts()
        {
	        return (Flags & Rsc6FragTypeFlags.HAS_ANY_ARTICULATED_PARTS) != 0;
        }

        public bool GetIsUserModified()
        {
	        return (Flags & Rsc6FragTypeFlags.IS_USER_MODIFIED) != 0;
        }

        public bool GetAllocateTypeAndIncludeFlags()
        {
            return (Flags & Rsc6FragTypeFlags.ALLOCATE_TYPE_AND_INCLUDE_FLAGS) != 0;
        }
    }

    public class Rsc6FragDrawable : Rsc6Drawable, MetaNode //rage::fragDrawable
    {
        /*
         * Handles the loading of the drawing and bounds data for each piece of a fragment type
         * Each fragTypeChild owns one fragDrawable.
         * The fragDrawable also loads other data, such as "locators" which are used to indicate the positions of the characters in vehicle seats, the entry
         * and exit positions from every door, the position of particle effects, etc.
         */

        public new ulong BlockLength => base.BlockLength + 120;
        public uint Unknow_78h { get; set; } //Padding
        public uint Unknow_7Ch { get; set; } //Padding
        public Matrix4x4 BoundMatrix { get; set; } = Matrix4x4.Identity; //m_BoundMatrix
        public Rsc6Ptr<Rsc6Bounds> Bound { get; set; } //m_Bound
        public Rsc6Arr<uint> ExtraBounds { get; set; } //m_ExtraBounds
        public Rsc6RawArr<Matrix4x4> ExtraBoundsMatrices { get; set; } //m_ExtraBoundsMatrices
        public ushort NumExtraBounds { get; set; } //m_NumExtraBounds
        public bool LoadSkeleton { get; set; } = true; //m_LoadSkeleton
        public byte Pad { get; set; } //m_Pad0
        public Rsc6Ptr<Rsc6FragDrawableLocator> Locators { get; set; } //m_Locators, rage::fragDrawable::LocatorData
        public int NumLocatorNodes { get; set; } //m_NumNodes
        public uint NodePool { get; set; } //m_NodePool
        public Rsc6Str SkeletonTypeName { get; set; } //m_SkeletonTypeName, always NULL
        public uint BoneOffsets { get; set; } //m_BoneOffsets, rage::crBoneOffsets, always NULL
        public Rsc6PtrArr<Rsc6FragAnimation> Animations { get; set; } //m_Animations, rage::fragAnimation, used for clocks & vehicles

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //rmcDrawable
            Unknow_78h = reader.ReadUInt32();
            Unknow_7Ch = reader.ReadUInt32();
            BoundMatrix = reader.ReadMatrix4x4();
            Bound = reader.ReadPtr(Rsc6Bounds.Create);
            ExtraBounds = reader.ReadArr<uint>();
            ExtraBoundsMatrices = reader.ReadRawArrPtr<Matrix4x4>();
            NumExtraBounds = reader.ReadUInt16();
            LoadSkeleton = reader.ReadBoolean();
            Pad = reader.ReadByte();
            Locators = reader.ReadPtr<Rsc6FragDrawableLocator>();
            NumLocatorNodes = reader.ReadInt32();
            NodePool = reader.ReadUInt32();
            SkeletonTypeName = reader.ReadStr();
            BoneOffsets = reader.ReadUInt32();
            Animations = reader.ReadPtrArr<Rsc6FragAnimation>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer); //rmcDrawable
            writer.WriteUInt32(Unknow_78h);
            writer.WriteUInt32(Unknow_7Ch);
            writer.WriteMatrix4x4(BoundMatrix);
            writer.WritePtr(Bound);
            writer.WriteArr(ExtraBounds);
            writer.WriteRawArrPtr(ExtraBoundsMatrices);
            writer.WriteUInt16(NumExtraBounds);
            writer.WriteBoolean(LoadSkeleton);
            writer.WriteByte(Pad);
            writer.WritePtr(Locators);
            writer.WriteInt32(NumLocatorNodes);
            writer.WriteUInt32(NodePool);
            writer.WriteStr(SkeletonTypeName);
            writer.WriteUInt32(BoneOffsets);
            writer.WritePtrArr(Animations);
        }

        public new void Read(MetaNodeReader reader)
        {
            BoundMatrix = reader.ReadMatrix4x4("BoundMatrix");
            Bound = new(reader.ReadNode<Rsc6Bounds>("Bound"));
            ExtraBounds = new(reader.ReadUInt32Array("ExtraBounds"));
            ExtraBoundsMatrices = new(reader.ReadMatrix4x4Array("ExtraBoundsMatrices"));
            NumExtraBounds = reader.ReadUInt16("NumExtraBounds");
            LoadSkeleton = reader.ReadBool("LoadSkeleton");
            Locators = new(reader.ReadNode<Rsc6FragDrawableLocator>("Locators"));
            NumLocatorNodes = reader.ReadInt32("NumLocatorNodes");
            NodePool = reader.ReadUInt32("NodePool");
            SkeletonTypeName = new(reader.ReadString("SkeletonTypeName"));
            Animations = new(reader.ReadNodeArray<Rsc6FragAnimation>("Animations"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            writer.WriteMatrix4x4("BoundMatrix", BoundMatrix);
            if (Bound.Item != null) writer.WriteNode("Bound", Bound.Item);
            if (ExtraBounds.Items != null) writer.WriteUInt32Array("ExtraBounds", ExtraBounds.Items);
            if (ExtraBoundsMatrices.Items != null) writer.WriteMatrix4x4Array("ExtraBoundsMatrices", ExtraBoundsMatrices.Items);
            writer.WriteUInt16("NumExtraBounds", NumExtraBounds);
            writer.WriteBool("LoadSkeleton", LoadSkeleton);
            if (Locators.Item != null) writer.WriteNode("Locators", Locators.Item);
            writer.WriteInt32("NumLocatorNodes", NumLocatorNodes);
            writer.WriteUInt32("NodePool", NodePool);
            if (SkeletonTypeName.Value != null) writer.WriteString("SkeletonTypeName", SkeletonTypeName.Value);
            if (Animations.Items != null) writer.WriteNodeArray("Animations", Animations.Items);
        }
    }

    public class Rsc6FragAnimation : Rsc6Block, MetaNode //rage::fragAnimation
    {
        /*
         * Owns a crAnimation, for handling of animated parts
         * 
         * Animated parts can optionally control a bound part
         * If they do, that part can be broken off if its force limit is exceeded
         */

        public ulong BlockLength => 24;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6Ptr<Rsc6Animation> Animation { get; set; } //Animation
        public int BoneCount { get; set; } //BoneCount
        public Rsc6RawArr<int> BoneIndices { get; set; } //BoneIndices
        public Rsc6Str Name { get; set; } //Name
        public float PhaseLastFrame { get; set; } //PhaseLastFrame
        public bool AutoPlay { get; set; } //AutoPlay
        public bool AffectsOnlyRootNodes { get; set; } //AffectsOnlyRootNodes
        public byte Pad0 { get; set; } //Padding
        public byte Pad1 { get; set; } //Padding

        public void Read(Rsc6DataReader reader)
        {
            Animation = reader.ReadPtr<Rsc6Animation>();
            BoneIndices = reader.ReadRawArrPtr<int>();
            BoneCount = reader.ReadInt32();
            Name = reader.ReadStr();
            PhaseLastFrame = reader.ReadSingle();
            AutoPlay = reader.ReadBoolean();
            AffectsOnlyRootNodes = reader.ReadBoolean();
            Pad0 = reader.ReadByte();
            Pad1 = reader.ReadByte();

            BoneIndices = reader.ReadRawArrItems(BoneIndices, (uint)BoneCount);
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WritePtr(Animation);
            writer.WriteInt32(BoneCount);
            writer.WriteRawArrPtr(BoneIndices);
            writer.WriteStr(Name);
            writer.WriteSingle(PhaseLastFrame);
            writer.WriteBoolean(AutoPlay);
            writer.WriteBoolean(AffectsOnlyRootNodes);
            writer.WriteByte(Pad0);
            writer.WriteByte(Pad1);
        }

        public void Read(MetaNodeReader reader)
        {
            Animation = new(reader.ReadNode<Rsc6Animation>("Animation"));
            BoneCount = reader.ReadInt32("BoneCount");
            BoneIndices = new(reader.ReadInt32Array("BoneIndices"));
            Name = new(reader.ReadString("Name"));
            PhaseLastFrame = reader.ReadSingle("PhaseLastFrame");
            AutoPlay = reader.ReadBool("AutoPlay");
            AffectsOnlyRootNodes = reader.ReadBool("AffectsOnlyRootNodes");
        }

        public void Write(MetaNodeWriter writer)
        {
            //if (Animation.Item != null) writer.WriteNode<Rsc6Animation>("Animation");
            writer.WriteInt32("BoneCount", BoneCount);
            if (BoneIndices.Items != null) writer.WriteInt32Array("BoneIndices", BoneIndices.Items);
            if (Name.Value != null) writer.WriteString("Name", Name.Value);
            writer.WriteSingle("PhaseLastFrame", PhaseLastFrame);
            writer.WriteBool("AutoPlay", AutoPlay);
            writer.WriteBool("AffectsOnlyRootNodes", AffectsOnlyRootNodes);
        }
    }

    public class Rsc6FragDrawableLocator : Rsc6Block, MetaNode //rage::fragDrawable::Locator
    {
        public ulong FilePosition { get; set; }
        public ulong BlockLength => 48;
        public bool IsPhysical => false;
        public Vector4 Offset { get; set; } //Offset
        public Vector4 Eulers { get; set; } //Eulers
        public int BoneIndex { get; set; } //BoneIndex
        public uint Pad0 { get; set; } //Pad0
        public uint Pad1 { get; set; } //Pad1
        public uint Pad2 { get; set; } //Pad2

        public void Read(Rsc6DataReader reader)
        {
            Offset = reader.ReadVector4();
            Eulers = reader.ReadVector4();
            BoneIndex = reader.ReadInt32();
            Pad0 = reader.ReadUInt32();
            Pad1 = reader.ReadUInt32();
            Pad2 = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(Offset);
            writer.WriteVector4(Eulers);
            writer.WriteInt32(BoneIndex);
            writer.WriteUInt32(Pad0);
            writer.WriteUInt32(Pad1);
            writer.WriteUInt32(Pad2);
        }

        public void Read(MetaNodeReader reader)
        {
            Offset = reader.ReadVector4("Offset");
            Eulers = reader.ReadVector4("Eulers");
            BoneIndex = reader.ReadInt32("BoneIndex");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("Offset", Offset);
            writer.WriteVector4("Eulers", Eulers);
            writer.WriteInt32("BoneIndex", BoneIndex);
        }
    }

    public class Rsc6FragArchetypeDamp : Rsc6FragArchetypePhys, MetaNode //rage::phArchetypeDamp
    {
        /*
         * Each phArchetype contains a bound and flags for culling collision tests
         * Each phArchetypePhys also contains mass and angular inertia and their inverses
         * Each phArchetypeDamp also contains damping constants for active objects
         * 
         * Holds the physical properties for any physical object. It contains a pointer to a phBound (the physical
         * boundary of the object), type flags to specify the kind of object it is for collisions, include flags to specify
         * the kinds of objects it can collide with, a reference count to keep track of the number of physics instances
         * using this archetype, and a name for sharing, debugging and loading from resources
         * 
         * phArchetype is only for physical objects that do not move, the derived class phArchetypePhys contains physical properties used for motion
         */
        public override ulong BlockLength => base.BlockLength + 100;
        public Vector4[] DampingConstants { get; set; } //m_DampingConstant

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            DampingConstants = reader.ReadVector4Arr(6);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            DampingConstants = reader.ReadVector4Array("DampingConstants");
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            if (DampingConstants != null) writer.WriteVector4Array("DampingConstants", DampingConstants);
        }

        public override string ToString()
        {
            return NameRef.Value;
        }
    }

    public class Rsc6FragArchetypePhys : Rsc6FragArchetype, MetaNode //rage::phArchetypePhys
    {
        /*
         * Each phArchetype contains a bound and flags for culling collision tests
         * Each phArchetypePhys also contains mass and angular inertia and their inverses
         * Each phArchetypeDamp also contains damping constants for active objects
         * 
         * Holds the physical properties for any physical object. It contains a pointer to a phBound (the physical
         * boundary of the object), type flags to specify the kind of object it is for collisions, include flags to specify
         * the kinds of objects it can collide with, a reference count to keep track of the number of physics instances
         * using this archetype, and a name for sharing, debugging and loading from resources
         * 
         * phArchetype is only for physical objects that do not move, the derived class phArchetypePhys contains physical properties used for motion
         */
        public override ulong BlockLength => base.BlockLength + 64;
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD; //Padding
        public ulong Unknown_20h { get; set; } = 0xCDCDCDCDCDCDCDCD; //Padding
        public float Mass { get; set; } //m_Mass
        public float InvMass { get; set; } //m_InvMass
        public float GravityFactor { get; set; } //m_GravityFactor
        public float MaxSpeed { get; set; } //m_MaxSpeed
        public float MaxAngSpeed { get; set; } //m_MaxAngSpeed
        public float BuoyancyFactor { get; set; } //m_BuoyancyFactor
        public Vector4 AngleInertia { get; set; } //m_AngInertia
        public Vector4 InverseAngleInertia { get; set; } //m_InvAngInertia

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt64();
            Mass = reader.ReadSingle();
            InvMass = reader.ReadSingle();
            GravityFactor = reader.ReadSingle();
            MaxSpeed = reader.ReadSingle();
            MaxAngSpeed = reader.ReadSingle();
            BuoyancyFactor = reader.ReadSingle();
            AngleInertia = reader.ReadVector4();
            InverseAngleInertia = reader.ReadVector4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_1Ch);
            writer.WriteUInt64(Unknown_20h);
            writer.WriteSingle(Mass);
            writer.WriteSingle(InvMass);
            writer.WriteSingle(GravityFactor);
            writer.WriteSingle(MaxSpeed);
            writer.WriteSingle(MaxAngSpeed);
            writer.WriteSingle(BuoyancyFactor);
            writer.WriteVector4(AngleInertia);
            writer.WriteVector4(InverseAngleInertia);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Mass = reader.ReadSingle("Mass");
            InvMass = reader.ReadSingle("InvMass");
            GravityFactor = reader.ReadSingle("GravityFactor");
            MaxSpeed = reader.ReadSingle("MaxSpeed");
            MaxAngSpeed = reader.ReadSingle("MaxAngSpeed");
            BuoyancyFactor = reader.ReadSingle("BuoyancyFactor");
            AngleInertia = reader.ReadVector4("AngleInertia");
            InverseAngleInertia = reader.ReadVector4("InverseAngleInertia");
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle("Mass", Mass);
            writer.WriteSingle("InvMass", InvMass);
            writer.WriteSingle("GravityFactor", GravityFactor);
            writer.WriteSingle("MaxSpeed", MaxSpeed);
            writer.WriteSingle("MaxAngSpeed", MaxAngSpeed);
            writer.WriteSingle("BuoyancyFactor", BuoyancyFactor);
            writer.WriteVector4("AngleInertia", AngleInertia);
            writer.WriteVector4("InverseAngleInertia", InverseAngleInertia);
        }
    }

    public class Rsc6FragArchetype : Rsc6FileBase, MetaNode //rage::phArchetype
    {
        /*
         * Each phArchetype contains a bound and flags for culling collision tests
         * Each phArchetypePhys also contains mass and angular inertia and their inverses
         * Each phArchetypeDamp also contains damping constants for active objects
         * 
         * Holds the physical properties for any physical object. It contains a pointer to a phBound (the physical
         * boundary of the object), type flags to specify the kind of object it is for collisions, include flags to specify
         * the kinds of objects it can collide with, a reference count to keep track of the number of physics instances
         * using this archetype, and a name for sharing, debugging and loading from resources
         * 
         * phArchetype is only for physical objects that do not move, the derived class phArchetypePhys contains physical properties used for motion
         */

        public override ulong BlockLength => 28;
        public Rsc6ArchetypeType Type { get; set; } //m_Type, the type of archetype (phArchetype, phArchetypePhys, phArchetypeDamp)
        public Rsc6Str NameRef { get; set; } //m_Filename, filename for the bank saving
        public Rsc6Ptr<Rsc6Bounds> Bounds { get; set; } //m_Bound, actual bound
        public Rsc6ObjectTypeFlags TypeFlags { get; set; } //m_TypeFlags, tells what type of object this is (prop, water, creature, player, vehicle, etc)
        public int IncludeFlags { get; set; } = -1; //m_IncludeFlags, tell what types of object this can collide with
        public ushort PropertyFlags { get; set; } //m_PropertyFlags, used to assign physical properties to objects, such as whether or not they can have contact forces
        public ushort RefCount { get; set; } = 1; //m_RefCount, number of references to this archetype

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Type = (Rsc6ArchetypeType)reader.ReadInt32(); //2
            NameRef = reader.ReadStr();
            Bounds = reader.ReadPtr(Rsc6Bounds.Create);
            TypeFlags = (Rsc6ObjectTypeFlags)reader.ReadUInt32();
            IncludeFlags = reader.ReadInt32(); //-1
            PropertyFlags = reader.ReadUInt16();
            RefCount = reader.ReadUInt16();

            if (IncludeFlags != -1)
            {
                throw new Exception("Rsc6FragArchetype: Unknown IncludeFlags");
            }
            if (PropertyFlags != 0)
            {
                throw new Exception("Rsc6FragArchetype: Unknown PropertyFlags");
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x00E6AEE8);
            writer.WriteStr(NameRef);
            writer.WritePtr(Bounds);
            writer.WriteUInt32((uint)TypeFlags);
            writer.WriteInt32(IncludeFlags);
            writer.WriteUInt16(PropertyFlags);
            writer.WriteUInt32(RefCount);
        }

        public void Read(MetaNodeReader reader)
        {
            NameRef = new(reader.ReadString("Name"));
            //Bounds = new(reader.ReadNode("Bounds"));
            TypeFlags = (Rsc6ObjectTypeFlags)reader.ReadUInt32("TypeFlags");
            IncludeFlags = reader.ReadInt32("IncludeFlags");
            PropertyFlags = reader.ReadUInt16("PropertyFlags");
            RefCount = reader.ReadUInt16("RefCount");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("Type", (int)Type);
            if (NameRef.Value != null) writer.WriteString("Name", NameRef.Value);
            if (Bounds.Item != null) writer.WriteNode("Bounds", Bounds.Item);
            writer.WriteUInt32("TypeFlags", (uint)TypeFlags);
            writer.WriteInt32("IncludeFlags", IncludeFlags);
            writer.WriteUInt16("PropertyFlags", PropertyFlags);
            writer.WriteUInt16("RefCount", RefCount);
        }

        public bool MatchFlags(uint includeFlags, uint typeFlags)
        {
            return ((includeFlags & (uint)TypeFlags) != 0) && ((IncludeFlags & typeFlags) != 0);
        }
    }

    public class Rsc6FragPhysGroup : Rsc6BlockBase, MetaNode //rage::fragTypeGroup
    {
        /*
         * fragTypeGroup's are tied together in a tree structure, and the links between them are the breaking points of the fragment instances
         * Each fragTypeGroup owns one or more fragTypeChild's, which contain the drawable and physical properties of the breakable piece it controls
         */

        public override ulong BlockLength => 128;
        public uint DeathEventset { get; set; } //m_DeathEventset, evtSet, always NULL?
        public uint DeathEventPlayer { get; set; } //m_DeathEventPlayer, fragCollisionEventPlayer, always NULL?
        public float Strength { get; set; } //m_Strength, magnitude of force required to separate this group from its parent
        public float ForceTransmissionScaleUp { get; set; } //m_ForceTransmissionScaleUp, how much of a force applied gets passed on to our parent. Before we send force up the chain, it is first scaled by this value
        public float ForceTransmissionScaleDown { get; set; } //m_ForceTransmissionScaleDown, how much of a force applied to us from our parent applies to us
        public float JointStiffness { get; set; } //m_JointStiffness, the articulated stiffness of the joint where this group is attached, if it becomes articulated
        public float MinSoftAngle1 { get; set; } //m_MinSoftAngle1, the minimum soft angle limit for 1 DOF joints or the first limit for 3 DOF joints
        public float MaxSoftAngle1 { get; set; } //m_MaxSoftAngle1, the maximum soft angle limit for 1 DOF joints or the first limit for 3 DOF joints
        public float MaxSoftAngle2 { get; set; } //m_MaxSoftAngle2, the second maximum soft angle limit for 3 DOF joints
        public float MaxSoftAngle3 { get; set; } //m_MaxSoftAngle3, the third maximum soft angle limit for 3 DOF joints
        public float RotationSpeed { get; set; } //m_RotationSpeed, the speed this articulated joint attached here will attempt to achieve using a muscle
        public float RotationStrength { get; set; } //m_RotationStrength, the strength this articulated joint will use to try to match the rotation speed
        public float RestoringStrength { get; set; } //m_RestoringStrength, the strength with with this articulated joint will try to reach the zero pose
        public float RestoringMaxTorque { get; set; } //m_RestoringMaxTorque, the maximum torque this articulated joint can use to return to the zero pose
        public float LatchStrength { get; set; } //m_LatchStrength, the breaking strength of the latch on this joint, if there is one
        public float TotalUndamagedMass { get; set; } //m_TotalUndamagedMass, the total mass of all child fragments, undamaged
        public float TotalDamagedMass { get; set; } //m_TotalDamagedMass, the total mass of all child fragments, damaged
        public byte ChildGroupsPointersIndex { get; set; } = 0xFF; //m_ChildGroupsPointersIndex, index in the fragType to the start of the list of child groups we own - 0xFF if we own no groups
        public byte ParentGroupPointerIndex { get; set; } = 0xFF; //m_ParentGroupPointerIndex, index in the fragType to the child we own - 0xFF if we don't own one
        public byte ChildIndex { get; set; } = 0xFF; //m_ChildIndex, index in the fragType to the child we own - 0xFF if we don't own one
        public byte NumChildren { get; set; } //m_NumChildren, the number of children we own, starting with m_ChildIndex
        public byte NumChildGroups { get; set; } //m_NumChildGroups, the number of child groups we own, starting with child groups pointers index
        public byte GlassModelAndType { get; set; } = 0xFF; //m_GlassModelAndType, upper bits are glass type index, lower are geometry index
        public byte GlassPaneModelInfoIndex { get; set; } //m_GlassPaneModelInfoIndex, geometry index
        public Rsc6FragTypeGroupFlag Flags { get; set; } //m_Flags
        public float MinDamageForce { get; set; } //m_MinDamageForce
        public float DamageHealth { get; set; } //m_DamageHealth
        public string DebugName { get; set; } //m_DebugName

        public override void Read(Rsc6DataReader reader)
        {
            DeathEventset = reader.ReadUInt32();
            DeathEventPlayer = reader.ReadUInt32();
            Strength = reader.ReadSingle();
            ForceTransmissionScaleUp = reader.ReadSingle();
            ForceTransmissionScaleDown = reader.ReadSingle();
            JointStiffness = reader.ReadSingle();
            MinSoftAngle1 = reader.ReadSingle();
            MaxSoftAngle1 = reader.ReadSingle();
            MaxSoftAngle2 = reader.ReadSingle();
            MaxSoftAngle3 = reader.ReadSingle();
            RotationSpeed = reader.ReadSingle();
            RotationStrength = reader.ReadSingle();
            RestoringStrength = reader.ReadSingle();
            RestoringMaxTorque = reader.ReadSingle();
            LatchStrength = reader.ReadSingle();
            TotalUndamagedMass = reader.ReadSingle();
            TotalDamagedMass = reader.ReadSingle();
            ChildGroupsPointersIndex = reader.ReadByte();
            ParentGroupPointerIndex = reader.ReadByte();
            ChildIndex = reader.ReadByte();
            NumChildren = reader.ReadByte();
            NumChildGroups = reader.ReadByte();
            GlassModelAndType = reader.ReadByte();
            GlassPaneModelInfoIndex = reader.ReadByte();
            Flags = (Rsc6FragTypeGroupFlag)reader.ReadByte();
            MinDamageForce = reader.ReadSingle();
            DamageHealth = reader.ReadSingle();
            DebugName = reader.ReadString();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            ulong pos = writer.Position;
            writer.WriteUInt32(DeathEventset);
            writer.WriteUInt32(DeathEventPlayer);
            writer.WriteSingle(Strength);
            writer.WriteSingle(ForceTransmissionScaleUp);
            writer.WriteSingle(ForceTransmissionScaleDown);
            writer.WriteSingle(JointStiffness);
            writer.WriteSingle(MinSoftAngle1);
            writer.WriteSingle(MaxSoftAngle1);
            writer.WriteSingle(MaxSoftAngle2);
            writer.WriteSingle(MaxSoftAngle3);
            writer.WriteSingle(RotationSpeed);
            writer.WriteSingle(RotationStrength);
            writer.WriteSingle(RestoringStrength);
            writer.WriteSingle(RestoringMaxTorque);
            writer.WriteSingle(LatchStrength);
            writer.WriteSingle(TotalUndamagedMass);
            writer.WriteSingle(TotalDamagedMass);
            writer.WriteByte(ChildGroupsPointersIndex);
            writer.WriteByte(ParentGroupPointerIndex);
            writer.WriteByte(ChildIndex);
            writer.WriteByte(NumChildren);
            writer.WriteByte(NumChildGroups);
            writer.WriteByte(GlassModelAndType);
            writer.WriteByte(GlassPaneModelInfoIndex);
            writer.WriteByte((byte)Flags);
            writer.WriteSingle(MinDamageForce);
            writer.WriteSingle(DamageHealth);
            writer.WriteStringNullTerminated(DebugName);

            while (writer.Position < pos + BlockLength)
            {
                writer.WriteByte(0xCD); //Padding
            }
        }

        public void Read(MetaNodeReader reader)
        {
            DeathEventset = reader.ReadUInt32("DeathEventset");
            DeathEventPlayer = reader.ReadUInt32("DeathEventPlayer");
            Strength = reader.ReadSingle("Strength");
            ForceTransmissionScaleUp = reader.ReadSingle("ForceTransmissionScaleUp");
            ForceTransmissionScaleDown = reader.ReadSingle("ForceTransmissionScaleDown");
            JointStiffness = reader.ReadSingle("JointStiffness");
            MinSoftAngle1 = reader.ReadSingle("MinSoftAngle1");
            MaxSoftAngle1 = reader.ReadSingle("MaxSoftAngle1");
            MaxSoftAngle2 = reader.ReadSingle("MaxSoftAngle2");
            MaxSoftAngle3 = reader.ReadSingle("MaxSoftAngle3");
            RotationSpeed = reader.ReadSingle("RotationSpeed");
            RotationStrength = reader.ReadSingle("RotationStrength");
            RestoringStrength = reader.ReadSingle("RestoringStrength");
            RestoringMaxTorque = reader.ReadSingle("RestoringMaxTorque");
            LatchStrength = reader.ReadSingle("LatchStrength");
            TotalUndamagedMass = reader.ReadSingle("TotalUndamagedMass");
            TotalDamagedMass = reader.ReadSingle("TotalDamagedMass");
            ChildGroupsPointersIndex = reader.ReadByte("ChildGroupsPointersIndex");
            ParentGroupPointerIndex = reader.ReadByte("ParentGroupPointerIndex");
            ChildIndex = reader.ReadByte("ChildIndex");
            NumChildren = reader.ReadByte("NumChildren");
            NumChildGroups = reader.ReadByte("NumChildGroups");
            GlassModelAndType = reader.ReadByte("GlassModelAndType");
            GlassPaneModelInfoIndex = reader.ReadByte("GlassPaneModelInfoIndex");
            Flags = (Rsc6FragTypeGroupFlag)reader.ReadByte("Flags");
            MinDamageForce = reader.ReadSingle("MinDamageForce");
            DamageHealth = reader.ReadSingle("DamageHealth");
            DebugName = reader.ReadString("DebugName");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("DeathEventset", DeathEventset);
            writer.WriteUInt32("DeathEventPlayer", DeathEventPlayer);
            writer.WriteSingle("Strength", Strength);
            writer.WriteSingle("ForceTransmissionScaleUp", ForceTransmissionScaleUp);
            writer.WriteSingle("ForceTransmissionScaleDown", ForceTransmissionScaleDown);
            writer.WriteSingle("JointStiffness", JointStiffness);
            writer.WriteSingle("MinSoftAngle1", MinSoftAngle1);
            writer.WriteSingle("MaxSoftAngle1", MaxSoftAngle1);
            writer.WriteSingle("MaxSoftAngle2", MaxSoftAngle2);
            writer.WriteSingle("MaxSoftAngle3", MaxSoftAngle3);
            writer.WriteSingle("RotationSpeed", RotationSpeed);
            writer.WriteSingle("RotationStrength", RotationStrength);
            writer.WriteSingle("RestoringStrength", RestoringStrength);
            writer.WriteSingle("RestoringMaxTorque", RestoringMaxTorque);
            writer.WriteSingle("DeathEventset", LatchStrength);
            writer.WriteSingle("TotalUndamagedMass", TotalUndamagedMass);
            writer.WriteSingle("TotalDamagedMass", TotalDamagedMass);
            writer.WriteByte("ChildGroupsPointersIndex", ChildGroupsPointersIndex);
            writer.WriteByte("ParentGroupPointerIndex", ParentGroupPointerIndex);
            writer.WriteByte("ChildIndex", ChildIndex);
            writer.WriteByte("NumChildren", NumChildren);
            writer.WriteByte("NumChildGroups", NumChildGroups);
            writer.WriteByte("GlassModelAndType", GlassModelAndType);
            writer.WriteByte("GlassPaneModelInfoIndex", GlassPaneModelInfoIndex);
            writer.WriteByte("Flags", (byte)Flags);
            writer.WriteSingle("MinDamageForce", MinDamageForce);
            writer.WriteSingle("DamageHealth", DamageHealth);
            writer.WriteString("DebugName", DebugName);
        }
    }

    public class Rsc6FragPhysChild : Rsc6BlockBase, MetaNode
    {
        /*
         * Holds type data related to one atomic piece of a fragment type.
         * Simple fragType's have only one of these, complex fragType's have one of these for each piece they can break into.
         * fragTypeChild's are linked together in a complex type by fragTypeGroup's
         */

        public override ulong BlockLength => 192;
        public uint VFT { get; set; } = 0x00F23A34;
        public float UndamagedMass { get; set; } //m_UndamagedMass
        public float DamagedMass { get; set; } //m_DamagedMass
        public byte OwnerGroupPointerIndex { get; set; } //m_OwnerGroupPointerIndex, index in the fragType to the group that owns us
        public byte Flags { get; set; } //m_Flags
        public ushort BoneID { get; set; } //m_BoneID, the bone of the main entity's skeleton this child follows, 0 if it's parented to the entity itself
        public Matrix4x4 BoneAttachment { get; set; } = Matrix4x4.Identity; //m_BoneAttachment
        public Matrix4x4 LinkAttachment { get; set; } = Matrix4x4.Identity; //m_LinkAttachment
        public Rsc6Ptr<Rsc6FragDrawable> UndamagedEntity { get; set; } //m_UndamagedEntity
        public Rsc6Ptr<Rsc6FragDrawable> DamagedEntity { get; set; } //DamagedEntity
        public uint ContinuousEventset { get; set; } //m_ContinuousEventset
        public uint CollisionEventset { get; set; } //m_CollisionEventset
        public uint BreakEventset { get; set; } //m_BreakEventset
        public uint BreakFromRootEventset { get; set; } //m_BreakFromRootEventset
        public uint CollisionEventPlayer { get; set; } //m_CollisionEventPlayer
        public uint BreakEventPlayer { get; set; } //m_BreakEventPlayer
        public uint BreakFromRootEventPlayer { get; set; } //m_BreakFromRootEventPlayer
        public uint Unknown_B4h { get; set; } //Padding
        public uint Unknown_B8h { get; set; } //Padding
        public uint Unknown_BCh { get; set; } //Padding

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            UndamagedMass = reader.ReadSingle();
            DamagedMass = reader.ReadSingle();
            OwnerGroupPointerIndex = reader.ReadByte();
            Flags = reader.ReadByte();
            BoneID = reader.ReadUInt16();
            BoneAttachment = reader.ReadMatrix4x4();
            LinkAttachment = reader.ReadMatrix4x4();
            UndamagedEntity = reader.ReadPtr<Rsc6FragDrawable>();
            DamagedEntity = reader.ReadPtr<Rsc6FragDrawable>();
            ContinuousEventset = reader.ReadUInt32();
            CollisionEventset = reader.ReadUInt32();
            BreakEventset = reader.ReadUInt32();
            BreakFromRootEventset = reader.ReadUInt32();
            CollisionEventPlayer = reader.ReadUInt32();
            BreakEventPlayer = reader.ReadUInt32();
            BreakFromRootEventPlayer = reader.ReadUInt32();
            Unknown_B4h = reader.ReadUInt32();
            Unknown_B8h = reader.ReadUInt32();
            Unknown_BCh = reader.ReadUInt32();

            if (DamagedEntity.Item != null)
            {
                throw new Exception("Rsc6FragPhysChild: Found a damageable fragDrawable!");
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(VFT);
            writer.WriteSingle(UndamagedMass);
            writer.WriteSingle(DamagedMass);
            writer.WriteByte(OwnerGroupPointerIndex);
            writer.WriteByte(Flags);
            writer.WriteUInt16(BoneID);
            writer.WriteMatrix4x4(BoneAttachment);
            writer.WriteMatrix4x4(LinkAttachment);
            writer.WritePtr(UndamagedEntity);
            writer.WritePtr(DamagedEntity);
            writer.WriteUInt32(ContinuousEventset);
            writer.WriteUInt32(CollisionEventset);
            writer.WriteUInt32(BreakEventset);
            writer.WriteUInt32(BreakFromRootEventset);
            writer.WriteUInt32(CollisionEventPlayer);
            writer.WriteUInt32(BreakEventPlayer);
            writer.WriteUInt32(BreakFromRootEventPlayer);
            writer.WriteUInt32(Unknown_B4h);
            writer.WriteUInt32(Unknown_B8h);
            writer.WriteUInt32(Unknown_BCh);
        }

        public void Read(MetaNodeReader reader)
        {
            //UndamagedEntity = new(reader.ReadNode("UndamagedEntity"));
            //DamagedEntity = new(reader.ReadNode("DamagedEntity"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteSingle("UndamagedMass", UndamagedMass);
            writer.WriteSingle("DamagedMass", DamagedMass);
            writer.WriteByte("OwnerGroupPointerIndex", OwnerGroupPointerIndex);
            writer.WriteByte("Flags", Flags);
            writer.WriteUInt16("BoneID", BoneID);
            writer.WriteMatrix4x4("BoneAttachment", BoneAttachment);
            writer.WriteMatrix4x4("LinkAttachment", LinkAttachment);
            if (UndamagedEntity.Item != null) writer.WriteNode("UndamagedEntity", UndamagedEntity.Item);
            if (DamagedEntity.Item != null) writer.WriteNode("DamagedEntity", DamagedEntity.Item);
            writer.WriteUInt32("ContinuousEventset", ContinuousEventset);
            writer.WriteUInt32("CollisionEventset", CollisionEventset);
            writer.WriteUInt32("BreakEventset", BreakEventset);
            writer.WriteUInt32("BreakFromRootEventset", BreakFromRootEventset);
            writer.WriteUInt32("CollisionEventPlayer", CollisionEventPlayer);
            writer.WriteUInt32("BreakEventPlayer", BreakEventPlayer);
            writer.WriteUInt32("BreakFromRootEventPlayer", BreakFromRootEventPlayer);
        }
    }

    public class Rsc6AssociationInfo : Rsc6Block, MetaNode
    {
        public ulong BlockLength => 20;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6Str FragmentName { get; set; } //m_FragmentName
        public Rsc6Str NonMangledName { get; set; } //m_NonMangledName
        public Rsc6Str LocatorName { get; set; } //m_LocatorName
        public JenkHash DesignTagHash { get; set; } //m_DesignTagHash
        public bool Default { get; set; } //m_bDefault
        public bool HardLink { get; set; } //m_bHardLink
        public bool Detachable { get; set; } //m_bDetachable
        public byte Pad { get; set; } //m_Pad

        public void Read(Rsc6DataReader reader)
        {
            FragmentName = reader.ReadStr();
            NonMangledName = reader.ReadStr();
            LocatorName = reader.ReadStr();
            DesignTagHash = reader.ReadUInt32();
            Default = reader.ReadBoolean();
            HardLink = reader.ReadBoolean();
            Detachable = reader.ReadBoolean();
            Pad = reader.ReadByte();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteStr(FragmentName);
            writer.WriteStr(NonMangledName);
            writer.WriteStr(LocatorName);
            writer.WriteUInt32(DesignTagHash);
            writer.WriteBoolean(Default);
            writer.WriteBoolean(HardLink);
            writer.WriteBoolean(Detachable);
            writer.WriteByte(Pad);
        }

        public void Read(MetaNodeReader reader)
        {
            FragmentName = new(reader.ReadString("FragmentName"));
            NonMangledName = new(reader.ReadString("NonMangledName"));
            LocatorName = new(reader.ReadString("LocatorName"));
            DesignTagHash = reader.ReadUInt32("DesignTagHash");
            Default = reader.ReadBool("Default");
            HardLink = reader.ReadBool("HardLink");
            Detachable = reader.ReadBool("Detachable");
        }

        public void Write(MetaNodeWriter writer)
        {
            if (FragmentName.Value != null) writer.WriteString("FragmentName", FragmentName.Value);
            if (NonMangledName.Value != null) writer.WriteString("NonMangledName", NonMangledName.Value);
            if (LocatorName.Value != null) writer.WriteString("LocatorName", LocatorName.Value);
            writer.WriteUInt32("DesignTagHash", DesignTagHash);
            writer.WriteBool("Default", Default);
            writer.WriteBool("HardLink", HardLink);
            writer.WriteBool("Detachable", Detachable);
        }
    }

    public enum Rsc6ArchetypeType : int
    {
        ARCHETYPE = 0,
        ARCHETYPE_PHYS = 1,
        ARCHETYPE_DAMP = 2
    }

    public enum Rsc6FragTypeFlags : int
    {
        NEEDS_CACHE_ENTRY_TO_ACTIVATE = 1 << 0,
        HAS_ANY_ARTICULATED_PARTS = 1 << 1,
        UNUSED = 1 << 2,
        CLONE_BOUND_PARTS_IN_CACHE = 1 << 3,
        ALLOCATE_TYPE_AND_INCLUDE_FLAGS = 1 << 4,
        BECOME_ROPE = 1 << 10,  //Some nasty RDR hack
        IS_USER_MODIFIED = 1 << 11,   //Flag to help the user keep track of fragments they modified
        DISABLE_ACTIVATION = 1 << 12,   //Disables activation on instances until the user enables
        DISABLE_BREAKING = 1 << 13  //Disables activation on instances until the user enables
    };

    public enum Rsc6ObjectTypeFlags : uint
    {
        OBJ_SKINNED = 1, //Peds, all p_gen_ropesm's, p_gen_blood03x, p_blk_cityhall_clock01x
        OBJ_STATIC_STANDARD = 1572869, //All p_gen_doorstandard's, p_gen_basin01x, p_gen_cranedock01x, p_gen_chairtheater01x, all debris_rockclusters
        OBJ_SPECIAL = 1572999, //Most trees, p_gen_streetclock01x
        OBJ_WEAPONS = 1572865, //revolver_lemat01x
        OBJ_PROP_STANDARD = 1572871, //p_gen_cart01x, p_gen_barrel01x, p_gen_chair05x, p_gen_chaircomfy01x, p_gen_debrisboard02x, p_gen_coffin02x, car01x etc...
        OBJ_SPECIAL2 = 1572997, //st_whitepine01x
        OBJ_PROP_STANDARD2 = 3670021,
        OBJ_SPECIAL3 = 3670023, //p_gen_milkcan02x, p_gen_trunk01x
        OBJ_WINDOW = 3671047,
        OBJ_SEPERATED_PROPS = 3670149, //p_gen_boxcar0101x
        OBJ_CARS1 = 18350087, //armoredcar01x, flatcar01x
        OBJ_CARS2 = 18350215, //northboxcar01x
        OBJ_WATERTROUGH = 20447237, //p_gen_watertrough01x
        OBJ_CARTS = 1075314695, //cart001x
    }

    public enum Rsc6FragTypeGroupFlag : byte
    {
        DISAPPEARS_WHEN_DEAD = 1 << 0, //When health reaches zero, this group disappears
        MADE_OF_GLASS = 1 << 1, //This group is made out of glass and will shatter when broken
        DAMAGE_WHEN_BROKEN = 1 << 2, //When this group breaks off its parent, it will become damaged
        DOESNT_AFFECT_VEHICLES = 1 << 3, //When colliding with vehicles, the vehicle is treated as infinitely massive
        DOESNT_PUSH_VEHICLES_DOWN = 1 << 4,
        HAS_CLOTH = 1 << 5, //This group has the cloth (can't have more than one cloth per fragment)
    }
}