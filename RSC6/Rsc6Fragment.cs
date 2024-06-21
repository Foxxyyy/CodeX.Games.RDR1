using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Collections.Generic;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using static CodeX.Games.RDR1.RPF6.Rpf6Crypto;
using CodeX.Games.RDR1.Files;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6Fragment : Rsc6BlockBaseMap, MetaNode //rage::fragType
    {
        public override ulong BlockLength => 348 + 164; //sagFragtype + rage::fragType
        public override uint VFT { get; set; } = 0x00DDB8E4;
        public float SmallestAngInertia { get; set; } //m_SmallestAngInertia
        public float LargestAngInertia { get; set; } //m_LargestAngInertia
        public Vector4 BoundingSphere { get; set; } //m_BoundingSphere
        public Vector4 RootCGOffset { get; set; } //m_RootCGOffset
        public Vector4 OriginalRootCGOffset { get; set; } //m_OriginalRootCGOffset
        public Vector4 UnbrokenCGOffset { get; set; } //m_UnbrokenCGOffset
        public Vector4[] DampingConstant { get; set; } //m_DampingConstant, won't do anything unless the game utilizes 'UnbrokenElasticity'
        public Rsc6Str NameRef { get; set; } //m_TuneName
        public Rsc6Ptr<Rsc6FragmentDrawable> Drawable { get; set; } //m_CommonDrawable, contains data common to all the parts of the fragment type, the shader groups, etc.
        public Rsc6RawPtrArr<Rsc6FragmentDrawable> ExtraDrawables { get; set; } //m_ExtraDrawables
        public Rsc6StrArr ExtraDrawableNames { get; set; } //m_ExtraDrawableNames
        public uint NumExtraDrawables { get; set; } //m_NumExtraDrawables
        public uint DamagedDrawable { get; set; } = uint.MaxValue; //m_DamagedDrawable, rage::fragTypeChild, when health value reaches zero, the piece can be swapped for a damaged version, which can also take more damage for further mesh deformation and texture adjustment, always 4294967295
        public uint RootChild { get; set; } //m_RootChild, used when undamaged, and the bound is used when there are no children, always 0
        public Rsc6StrArr GroupNames { get; set; } //m_GroupNames
        public Rsc6RawPtrArr<Rsc6FragPhysGroup> Groups { get; set; } //m_Groups, rage::fragTypeGroup
        public Rsc6RawPtrArr<Rsc6FragPhysChild> Childrens { get; set; } //m_Children, rage::fragTypeChild
        public uint UnkArray { get; set; } //m_EnvCloth, rage::fragTypeEnvCloth
        public ushort UnkArrayCount { get; set; } //m_Count
        public ushort UnkArraySize { get; set; } //m_Capacity
        public Rsc6Ptr<Rsc6FragmentCloth> Clothes { get; set; } //m_CharCloth
        public Rsc6Ptr<Rsc6FragArchetypeDamp> Archetype1 { get; set; }
        public Rsc6Ptr<Rsc6FragArchetypeDamp> Archetype2 { get; set; }
        public Rsc6Ptr<Rsc6Bounds> Bounds { get; set; } //m_CompositeBounds
        public Rsc6RawArr<Vector4> UndamagedAngInertia { get; set; } //m_UndamagedAngInertia, precomputed angular inertia values for complex fragments
        public Rsc6RawArr<Vector4> DamagedAngInertia { get; set; } //m_DamagedAngInertia, precomputed angular inertia values for complex fragments
        public Rsc6RawArr<Matrix4x4> LinkAttachments { get; set; } //m_LinkAttachments, the initial position of the center of gravity of the top-most fragment part that is part of the link of which that fragment part is a part, the orientation part will always be identity
        public Rsc6RawArr<float> MinBreakingImpulses { get; set; } //m_MinBreakingImpulses, 
        public Rsc6RawArr<byte> SelfCollisionIndicesA { get; set; } //m_SelfCollisionA, group indices for collision detection (max=255), to index all m_Groups's child groups consecutively
        public Rsc6RawArr<byte> SelfCollisionIndicesB { get; set; } //m_SelfCollisionB, group indices for collision detection (max=255), to index all m_Groups's child groups consecutively
        public uint UserData { get; set; } //m_UserData
        public Rsc6RawLst<Rsc6FragmentJointParams> JointParams { get; set; } //m_JointParams
        public Rsc6Ptr<Rsc6CollisionEventSet> CollisionEventSet { get; set; } //m_CollisionEventset, for certain 'p_gen' props
        public Rsc6Ptr<Rsc6CollisionEventPlayer> CollisionEventPlayer { get; set; } //m_CollisionEventPlayer
        public Rsc6Ptr<Rsc6Frame> AnimFrame { get; set; } //m_AnimFrame
        public Rsc6Ptr<Rsc6Skeleton> SkeletonA { get; set; }
        public Rsc6Ptr<Rsc6Skeleton> SkeletonB { get; set; }
        public Rsc6Ptr<Rsc6MatrixSet> SharedMatrixSet { get; set; } //m_SharedMatrixSet, only NULL when fragment has no skeleton
        public uint EstimatedCacheSize { get; set; } //m_EstimatedCacheSizes
        public uint EstimatedArticulatedCacheSize { get; set; } //m_EstimatedArticulatedCacheSize
        public byte NumSelfCollisions { get; set; } //m_NumSelfCollisions
        public byte MaxNumSelfCollisions { get; set; } //m_MaxNumSelfCollisions
        public byte GroupCount { get; set; } //m_NumGroups
        public byte ChildCount { get; set; } //m_NumChildren, total number of children
        public byte FragTypeGroupCount { get; set; } //m_RootGroupCount
        public byte NumRootDamageRegions { get; set; } //m_NumRootDamageRegions, number of damageable regions of the root child
        public byte NumBonyChildren { get; set; } = 1; //m_NumBonyChildren, number of children controlled by skeleton bones
        public byte EntityClass { get; set; } //m_EntityClass
        public byte ARTAssetID { get; set; } //m_ARTAssetID
        public byte AttachBottomEnd { get; set; } //m_AttachBottomEnd
        public Rsc6FragTypeFlags Flags { get; set; } //m_Flags
        public int ClientClassID { get; set; } //m_ClientClassID
        public float MinMoveForce { get; set; } //m_MinMoveForce
        public float UnbrokenElasticity { get; set; } //m_UnbrokenElasticity
        public float BuoyancyFactor { get; set; } //m_BuoyancyFactor
        public uint Unknown_14Ch { get; set; } //Always 0
        public byte GlassAttachmentBone { get; set; } //m_GlassAttachmentBone, always 0 (root)
        public byte NumGlassPaneModelInfos { get; set; } //m_NumGlassPaneModelInfos
        public ushort Unknown_152h { get; set; } //Always 0
        public uint GlassPaneModelInfos { get; set; } //m_GlassPaneModelInfos, rage::bgPaneModelInfoBase, used for windows (p_win), array of pointers using 'NumGlassPaneModelInfos'
        public float GravityFactor { get; set; } = 1.0f; //m_GravityFactor
        public uint Unknown_15Ch { get; set; }
        public Rsc6PtrToPtrArr<Rsc6AssociationInfo> AssociatedFragments { get; set; } //m_AssociatedFragments
        public uint Unknown_164h { get; set; } //Always 0
        public Rsc6Ptr<Rsc6FragChildDataSet> ChildDataSet { get; set; } //m_ChildDataSet
        public uint Unknown_16Ch { get; set; } //Always 0
        public bool HasTextureLOD { get; set; } //m_HasTextureLOD
        public bool HasFragLOD { get; set; } //m_HasFragLOD
        public bool HasAnimNormalMap { get; set; } //m_HasAnimNrmMap
        public byte[] Unknown_173h { get; set; }
        public Rsc6Str ParentTextureLOD { get; set; } //m_ParentTextureLOD, used for hats
        public int GlassGlowShaderIndex { get; set; } = -1; //m_GlassGlowShaderIndex, mostly -1, can be 0, 1 or 2 for lamps
        public Rsc6Ptr<Rsc6TargetManager> TargetManager { get; set; } //m_TargetManager
        public Rsc6VariableMeshArray VariableMeshArray { get; set; } //m_VariableMeshArray
        public byte VariableMeshCount { get; set; } //m_VariableMeshCount
        public byte AlwaysAddToShadow { get; set; } = 0xCD; //m_AlwayAddToShadow, always 0xCD
        public byte InnerSorting { get; set; } = 0xCD; //m_InnerSorting, always 0xCD
        public Rsc6Ptr<Rsc6TextureDictionary> Textures { get; set; } //m_BuiltInTextureDictionary
        public uint PlacedLightsGroup { get; set; } //m_PlacedLightsGroup, always 0
        public JenkHash TuneNameHash { get; set; } //m_TuneNameHash
        public ushort Unknown_1FCh { get; set; } = 0xCDCD; //Padding
        public bool NoSnow { get; set; } //m_NoSnow
        public bool ForceOutSide { get; set; } //m_ForceOutSide

        public static float SkinnedHeightPos = 1.0f;
        public const int NUM_DAMP_TYPES = (int)(Rsc6MotionDampingType.ANGULAR_V2 + 1);

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);

            //Loading textures before drawables
            reader.Position += 0x1E8;
            Textures = WfdFile.TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>();
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
            DampingConstant = reader.ReadVector4Arr(NUM_DAMP_TYPES);
            NameRef = reader.ReadStr();
            Drawable = reader.ReadPtr<Rsc6FragmentDrawable>();
            ExtraDrawables = reader.ReadRawPtrArrPtr<Rsc6FragmentDrawable>();
            ExtraDrawableNames = reader.ReadPtr();
            NumExtraDrawables = reader.ReadUInt32();
            DamagedDrawable = reader.ReadUInt32();
            RootChild = reader.ReadUInt32();
            GroupNames = reader.ReadPtr();
            Groups = reader.ReadRawPtrArrPtr<Rsc6FragPhysGroup>();
            Childrens = reader.ReadRawPtrArrPtr<Rsc6FragPhysChild>();
            UnkArray = reader.ReadUInt32(); //TODO (p_gen_glider_cs01x)
            UnkArrayCount = reader.ReadUInt16();
            UnkArraySize = reader.ReadUInt16();
            Clothes = reader.ReadPtr<Rsc6FragmentCloth>(); //TODO (horsemangy01)
            Archetype1 = reader.ReadPtr<Rsc6FragArchetypeDamp>();
            Archetype2 = reader.ReadPtr<Rsc6FragArchetypeDamp>();
            Bounds = reader.ReadPtr(Rsc6Bounds.Create);
            UndamagedAngInertia = reader.ReadRawArrPtr<Vector4>();
            DamagedAngInertia = reader.ReadRawArrPtr<Vector4>();
            LinkAttachments = reader.ReadRawArrPtr<Matrix4x4>();
            MinBreakingImpulses = reader.ReadRawArrPtr<float>();
            SelfCollisionIndicesA = reader.ReadRawArrPtr<byte>();
            SelfCollisionIndicesB = reader.ReadRawArrPtr<byte>();
            UserData = reader.ReadUInt32();
            JointParams = reader.ReadRawLstPtr<Rsc6FragmentJointParams>();
            CollisionEventSet = reader.ReadPtr<Rsc6CollisionEventSet>();
            CollisionEventPlayer = reader.ReadPtr<Rsc6CollisionEventPlayer>();
            AnimFrame = reader.ReadPtr<Rsc6Frame>();
            SkeletonA = reader.ReadPtr<Rsc6Skeleton>();
            SkeletonB = reader.ReadPtr<Rsc6Skeleton>();
            SharedMatrixSet = reader.ReadPtr<Rsc6MatrixSet>();
            EstimatedCacheSize = reader.ReadUInt32();
            EstimatedArticulatedCacheSize = reader.ReadUInt32();
            NumSelfCollisions = reader.ReadByte();
            MaxNumSelfCollisions = reader.ReadByte();
            GroupCount = reader.ReadByte(); 
            ChildCount = reader.ReadByte();
            FragTypeGroupCount = reader.ReadByte();
            NumRootDamageRegions = reader.ReadByte();
            NumBonyChildren = reader.ReadByte();
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
            ChildDataSet = reader.ReadPtr<Rsc6FragChildDataSet>();
            Unknown_16Ch = reader.ReadUInt32();
            HasTextureLOD = reader.ReadBoolean();
            HasFragLOD = reader.ReadBoolean();
            HasAnimNormalMap = reader.ReadBoolean();
            Unknown_173h = reader.ReadBytes(17);
            ParentTextureLOD = reader.ReadStr();
            GlassGlowShaderIndex = reader.ReadInt32();
            TargetManager = reader.ReadPtr<Rsc6TargetManager>();
            VariableMeshArray = reader.ReadBlock<Rsc6VariableMeshArray>();
            VariableMeshCount = reader.ReadByte();
            AlwaysAddToShadow = reader.ReadByte();
            InnerSorting = reader.ReadByte();
            AssociatedFragments = reader.ReadItems(AssociatedFragments);
            Groups = reader.ReadRawPtrArrItem(Groups, GroupCount);
            GroupNames = reader.ReadItems(GroupNames, GroupCount);
            Childrens = reader.ReadRawPtrArrItem(Childrens, ChildCount);
            UndamagedAngInertia = reader.ReadRawArrItems(UndamagedAngInertia, ChildCount);
            DamagedAngInertia = reader.ReadRawArrItems(DamagedAngInertia, ChildCount);
            LinkAttachments = reader.ReadRawArrItems(LinkAttachments, ChildCount);
            MinBreakingImpulses = reader.ReadRawArrItems(MinBreakingImpulses, ChildCount);
            JointParams = reader.ReadRawLstItems(JointParams, ChildCount);
            SelfCollisionIndicesA = reader.ReadRawArrItems(SelfCollisionIndicesA, NumSelfCollisions);
            SelfCollisionIndicesB = reader.ReadRawArrItems(SelfCollisionIndicesB, NumSelfCollisions);
            ExtraDrawables = reader.ReadRawPtrArrItem(ExtraDrawables, NumExtraDrawables);
            ExtraDrawableNames = reader.ReadItems(ExtraDrawableNames, NumExtraDrawables);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle(SmallestAngInertia);
            writer.WriteSingle(LargestAngInertia);
            writer.WriteVector4(BoundingSphere);
            writer.WriteVector4(RootCGOffset);
            writer.WriteVector4(OriginalRootCGOffset);
            writer.WriteVector4(UnbrokenCGOffset);
            writer.WriteVector4Array(DampingConstant);
            writer.WriteStr(NameRef);
            writer.WritePtr(Drawable);
            writer.WriteRawPtrArr(ExtraDrawables);
            writer.WriteStrArr(ExtraDrawableNames);
            writer.WriteUInt32(NumExtraDrawables);
            writer.WriteUInt32(DamagedDrawable);
            writer.WriteUInt32(RootChild);
            writer.WriteStrArr(GroupNames);
            writer.WriteRawPtrArr(Groups);
            writer.WriteRawPtrArr(Childrens);
            writer.WriteUInt32(UnkArray);
            writer.WriteUInt16(UnkArrayCount);
            writer.WriteUInt16(UnkArraySize);
            writer.WritePtr(Clothes);
            writer.WritePtr(Archetype1);
            writer.WritePtr(Archetype2);
            writer.WritePtr(Bounds);
            writer.WriteRawArr(UndamagedAngInertia);
            writer.WriteRawArr(DamagedAngInertia);
            writer.WriteRawArr(LinkAttachments);
            writer.WriteRawArr(MinBreakingImpulses);
            writer.WriteRawArr(SelfCollisionIndicesA);
            writer.WriteRawArr(SelfCollisionIndicesB);
            writer.WriteUInt32(UserData);
            writer.WriteRawLst(JointParams);
            writer.WritePtr(CollisionEventSet);
            writer.WritePtr(CollisionEventPlayer);
            writer.WritePtr(AnimFrame);
            writer.WritePtr(SkeletonA);
            writer.WritePtr(SkeletonB);
            writer.WritePtr(SharedMatrixSet);
            writer.WriteUInt32(EstimatedCacheSize);
            writer.WriteUInt32(EstimatedArticulatedCacheSize);
            writer.WriteByte(NumSelfCollisions);
            writer.WriteByte(MaxNumSelfCollisions);
            writer.WriteByte(GroupCount);
            writer.WriteByte(ChildCount);
            writer.WriteByte(FragTypeGroupCount);
            writer.WriteByte(NumRootDamageRegions);
            writer.WriteByte(NumBonyChildren);
            writer.WriteByte(EntityClass);
            writer.WriteByte(ARTAssetID);
            writer.WriteByte(AttachBottomEnd);
            writer.WriteUInt16((ushort)Flags);
            writer.WriteInt32(ClientClassID);
            writer.WriteSingle(MinMoveForce);
            writer.WriteSingle(UnbrokenElasticity);
            writer.WriteSingle(BuoyancyFactor);
            writer.WriteUInt32(Unknown_14Ch);
            writer.WriteByte(GlassAttachmentBone);
            writer.WriteByte(NumGlassPaneModelInfos);
            writer.WriteUInt16(Unknown_152h);
            writer.WriteUInt32(GlassPaneModelInfos);
            writer.WriteSingle(GravityFactor);
            writer.WriteUInt32(Unknown_15Ch);
            writer.WriteUInt32(0u); //TODO: Fix WritePtrToPtrArr (AssociatedFragments)
            writer.WriteUInt32(Unknown_164h);
            writer.WritePtr(ChildDataSet);
            writer.WriteUInt32(Unknown_16Ch);
            writer.WriteBoolean(HasTextureLOD);
            writer.WriteBoolean(HasFragLOD);
            writer.WriteBoolean(HasAnimNormalMap);
            writer.WriteBytes(Unknown_173h);
            writer.WriteStr(ParentTextureLOD);
            writer.WriteInt32(GlassGlowShaderIndex);
            writer.WritePtr(TargetManager);
            VariableMeshArray?.Write(writer);
            writer.WriteByte(VariableMeshCount);
            writer.WriteByte(AlwaysAddToShadow);
            writer.WriteByte(InnerSorting);
            writer.WritePtr(Textures);
            writer.WriteUInt32(PlacedLightsGroup);
            writer.WriteUInt32(TuneNameHash);
            writer.WriteUInt16(Unknown_1FCh);
            writer.WriteBoolean(NoSnow);
            writer.WriteBoolean(ForceOutSide);
        }

        public void Read(MetaNodeReader reader)
        {
            Textures = new(reader.ReadNode<Rsc6TextureDictionary>("Textures"));
            var drawable = reader.ReadNode<Rsc6FragmentDrawable>("Drawable");
            var archetype = reader.ReadNode<Rsc6FragArchetypeDamp>("Archetype1");
            var childs = reader.ReadNodeArray<Rsc6FragPhysChild>("Childrens");
            var bounds = reader.ReadNode("Bounds", Rsc6Bounds.Create);

            if (childs != null )
            {
                if (drawable != null)
                {
                    for (int i = 0; i < childs.Length; i++) //Use the current skeleton for childs (so we don't rewrite skeleton each time)
                    {
                        var c = childs[i];
                        var ent = c.UndamagedEntity.Item;
                        if (ent == null || ent.Drawable.Skeleton == null) continue;

                        ent.Drawable.UseFragmentPointers = true;
                        ent.Drawable.SkeletonRef = drawable.Drawable.SkeletonRef;
                    }
                }
                if (archetype != null)
                {
                    for (int i = 0; i < childs.Length; i++) //Use the current bounds for childs (so we don't rewrite bounds each time)
                    {
                        var c = childs[i];
                        var ent = c.UndamagedEntity.Item;
                        if (ent == null || ent.Bound.Item == null) continue;

                        ent.UseBoundsArchetype = true;
                        ent.Bound = archetype.Bounds;
                    }
                }
            }

            if (bounds != null && archetype != null)
                Bounds = archetype.Bounds;
            else
                Bounds = new(bounds);

            TuneNameHash = reader.ReadJenkHash("TuneNameHash");
            NoSnow = reader.ReadBool("NoSnow");
            ForceOutSide = reader.ReadBool("ForceOutSide");
            SmallestAngInertia = reader.ReadSingle("SmallestAngInertia");
            LargestAngInertia = reader.ReadSingle("LargestAngInertia");
            BoundingSphere = ToXYZ(reader.ReadVector4("BoundingSphere"));
            RootCGOffset = ToXYZ(reader.ReadVector4("RootCGOffset"));
            OriginalRootCGOffset = ToXYZ(reader.ReadVector4("OriginalRootCGOffset"));
            UnbrokenCGOffset = ToXYZ(reader.ReadVector4("UnbrokenCGOffset"));
            DampingConstant = ToXYZ(reader.ReadVector4Array("DampingConstant"));
            NameRef = new(reader.ReadString("Name"));
            ExtraDrawables = new(reader.ReadNodeArray<Rsc6FragmentDrawable>("ExtraDrawables"));
            ExtraDrawableNames = new(reader.ReadStringArray("ExtraDrawableNames"));
            NumExtraDrawables = reader.ReadUInt32("NumExtraDrawables");
            DamagedDrawable = reader.ReadUInt32("DamagedDrawable", uint.MaxValue);
            RootChild = reader.ReadUInt32("RootChild");
            GroupNames = new(reader.ReadStringArray("GroupNames"));
            Groups = new(reader.ReadNodeArray<Rsc6FragPhysGroup>("Groups"));

            var clothes = reader.ReadNode<Rsc6FragmentCloth>("Clothes");
            if (clothes != null)
            {
                clothes.ReferencedDrawable = Drawable.Item;
                Clothes = new(clothes);
            }

            Archetype2 = new(reader.ReadNode<Rsc6FragArchetypeDamp>("Archetype2"));
            UndamagedAngInertia = new(ToXYZ(reader.ReadVector4Array("UndamagedAngInertia")));
            DamagedAngInertia = new(ToXYZ(reader.ReadVector4Array("DamagedAngInertia")));
            LinkAttachments = new(ToXYZ(reader.ReadMatrix4x4Array("LinkAttachments"), true));
            MinBreakingImpulses = new(reader.ReadSingleArray("MinBreakingImpulses"));
            SelfCollisionIndicesA = new(reader.ReadByteArray("SelfCollisionIndicesA"));
            SelfCollisionIndicesB = new(reader.ReadByteArray("SelfCollisionIndicesB"));
            UserData = reader.ReadUInt32("UserData");
            JointParams = new(reader.ReadNodeArray<Rsc6FragmentJointParams>("JointParams"));
            CollisionEventSet = new(reader.ReadNode<Rsc6CollisionEventSet>("CollisionEventSet"));
            CollisionEventPlayer = new(reader.ReadNode<Rsc6CollisionEventPlayer>("CollisionEventPlayer"));
            AnimFrame = new(reader.ReadNode<Rsc6Frame>("AnimFrame"));
            SkeletonA = new(reader.ReadNode<Rsc6Skeleton>("SkeletonA"));
            SkeletonB = new(reader.ReadNode<Rsc6Skeleton>("SkeletonB"));
            EstimatedCacheSize = reader.ReadUInt32("EstimatedCacheSize");
            EstimatedArticulatedCacheSize = reader.ReadUInt32("EstimatedArticulatedCacheSize");
            FragTypeGroupCount = reader.ReadByte("FragTypeGroupCount");
            NumRootDamageRegions = reader.ReadByte("NumDamageRegions");
            NumBonyChildren = reader.ReadByte("NumBonyChildren");
            EntityClass = reader.ReadByte("EntityClass");
            ARTAssetID = reader.ReadByte("ARTAssetID");
            AttachBottomEnd = reader.ReadByte("AttachBottomEnd");
            Flags = reader.ReadEnum<Rsc6FragTypeFlags>("Flags");
            ClientClassID = reader.ReadInt32("ClientClassID");
            MinMoveForce = reader.ReadSingle("MinMoveForce");
            UnbrokenElasticity = reader.ReadSingle("UnbrokenElasticity");
            BuoyancyFactor = reader.ReadSingle("BuoyancyFactor");
            GlassAttachmentBone = reader.ReadByte("GlassAttachmentBone");
            NumGlassPaneModelInfos = reader.ReadByte("NumGlassPaneModelInfos");
            GlassPaneModelInfos = reader.ReadUInt32("GlassPaneModelInfos");
            GravityFactor = reader.ReadSingle("GravityFactor");

            var associations = reader.ReadNodeArray<Rsc6AssociationInfo>("AssociatedFragments");
            if (associations != null)
            {
                var associationsBlock = new Rsc6ManagedArr<Rsc6AssociationInfo>(associations);
                AssociatedFragments = new(associationsBlock);
            }

            ChildDataSet = new(reader.ReadNode<Rsc6FragChildDataSet>("ChildDataSet"));
            HasTextureLOD = reader.ReadBool("HasTextureLOD");
            HasFragLOD = reader.ReadBool("HasFragLOD");
            HasAnimNormalMap = reader.ReadBool("HasAnimNormalMap");
            Unknown_173h = reader.ReadByteArray("Unknown_173h");
            ParentTextureLOD = new(reader.ReadString("ParentTextureLOD"));
            GlassGlowShaderIndex = reader.ReadInt32("GlassGlowShaderIndex");
            TargetManager = new(reader.ReadNode<Rsc6TargetManager>("TargetManager"));
            VariableMeshArray = reader.ReadNode<Rsc6VariableMeshArray>("VariableMeshArray");
            AlwaysAddToShadow = reader.ReadByte("AlwaysAddToShadow");
            InnerSorting = reader.ReadByte("InnerSorting");

            //Create an Rsc6MatrixSet instance if the fragment uses a skeleton
            if (SkeletonA.Item != null)
            {
                var mSet = new Rsc6MatrixSet(SkeletonA.Item.NumBones);
                SharedMatrixSet = new(mSet);
            }

            Drawable = new(drawable);
            Childrens = new(childs);
            Archetype1 = new(archetype);
            WfdFile.TextureDictionary = Textures;

            VariableMeshCount = (byte)(VariableMeshArray.UsedIndices?.Length ?? 0); //Unsure
            GroupCount = (byte)(Groups.Items?.Length ?? 0);
            ChildCount = (byte)(Childrens.Items?.Length ?? 0);
            NumSelfCollisions = (byte)(SelfCollisionIndicesA.Items?.Length ?? 0);
            MaxNumSelfCollisions = NumSelfCollisions;
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
            writer.WriteVector4Array("DampingConstant", DampingConstant);
            writer.WriteString("Name", NameRef.ToString());
            writer.WriteNode("Drawable", Drawable.Item);
            writer.WriteNodeArray("ExtraDrawables", ExtraDrawables.Items);
            if (ExtraDrawableNames.Items != null) writer.WriteStringArray("ExtraDrawableNames", ExtraDrawableNames.Items.Select(s => s.Value).ToArray());
            if (DamagedDrawable != uint.MaxValue) writer.WriteUInt32("DamagedDrawable", DamagedDrawable);
            writer.WriteStringArray("GroupNames", GroupNames.Items.Select(s => s.Value).ToArray());
            writer.WriteNodeArray("Groups", Groups.Items);
            writer.WriteNodeArray("Childrens", Childrens.Items);
            writer.WriteNode("Archetype1", Archetype1.Item);
            writer.WriteNode("Archetype2", Archetype2.Item);
            writer.WriteNode("Bounds", Bounds.Item);
            writer.WriteVector4Array("UndamagedAngInertia", UndamagedAngInertia.Items);
            writer.WriteVector4Array("DamagedAngInertia", DamagedAngInertia.Items);
            writer.WriteMatrix4x4Array("LinkAttachments", LinkAttachments.Items);
            writer.WriteSingleArray("MinBreakingImpulses", MinBreakingImpulses.Items);
            writer.WriteByteArray("SelfCollisionIndicesA", SelfCollisionIndicesA.Items);
            writer.WriteByteArray("SelfCollisionIndicesB", SelfCollisionIndicesB.Items);
            writer.WriteUInt32("UserData", UserData);
            writer.WriteNodeArray("JointParams", JointParams.Items);
            writer.WriteNode("CollisionEventSet", CollisionEventSet.Item);
            writer.WriteNode("CollisionEventPlayer", CollisionEventPlayer.Item);
            writer.WriteNode("AnimFrame", AnimFrame.Item);
            writer.WriteNode("SkeletonA", SkeletonA.Item);
            writer.WriteNode("SkeletonB", SkeletonB.Item);
            writer.WriteUInt32("EstimatedCacheSize", EstimatedCacheSize);
            writer.WriteUInt32("EstimatedArticulatedCacheSize", EstimatedArticulatedCacheSize);
            writer.WriteByte("FragTypeGroupCount", FragTypeGroupCount);
            writer.WriteByte("NumDamageRegions", NumRootDamageRegions);
            writer.WriteByte("NumBonyChildren", NumBonyChildren);
            writer.WriteByte("EntityClass", EntityClass);
            writer.WriteByte("ARTAssetID", ARTAssetID);
            writer.WriteByte("AttachBottomEnd", AttachBottomEnd);
            writer.WriteEnum("Flags", Flags);
            writer.WriteInt32("ClientClassID", ClientClassID);
            writer.WriteSingle("MinMoveForce", MinMoveForce);
            writer.WriteSingle("UnbrokenElasticity", UnbrokenElasticity);
            writer.WriteSingle("BuoyancyFactor", BuoyancyFactor);
            writer.WriteByte("NumGlassPaneModelInfos", NumGlassPaneModelInfos);
            writer.WriteUInt32("GlassPaneModelInfos", GlassPaneModelInfos);
            writer.WriteSingle("GravityFactor", GravityFactor);
            writer.WriteNodeArray("AssociatedFragments", AssociatedFragments.Array.Items);
            writer.WriteNode("ChildDataSet", ChildDataSet.Item);
            writer.WriteBool("HasTextureLOD", HasTextureLOD);
            writer.WriteBool("HasFragLOD", HasFragLOD);
            writer.WriteBool("HasAnimNormalMap", HasAnimNormalMap);
            writer.WriteByteArray("Unknown_173h", Unknown_173h);
            if (ParentTextureLOD.Value != null) writer.WriteString("ParentTextureLOD", ParentTextureLOD.ToString());
            writer.WriteInt32("GlassGlowShaderIndex", GlassGlowShaderIndex);
            writer.WriteNode("TargetManager", TargetManager.Item);
            writer.WriteNode("VariableMeshArray", VariableMeshArray);
            writer.WriteByte("AlwaysAddToShadow", AlwaysAddToShadow);
            writer.WriteByte("InnerSorting", InnerSorting);
            writer.WriteNode("Textures", Textures.Item);
            writer.WriteJenkHash("TuneNameHash", TuneNameHash);
            writer.WriteBool("NoSnow", NoSnow);
            writer.WriteBool("ForceOutSide", ForceOutSide);
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

    public class Rsc6FragmentJointParams : Rsc6BlockBase, MetaNode //rage::fragType::JointParams
    {
        public override ulong BlockLength => 96;
        public int DOF { get; set; } //dof
        public int Parent { get; set; } //parent
        public float Limit1Min { get; set; } //limit1min
        public float Limit1Max { get; set; } //limit1max
        public float Limit1Soft { get; set; } //limit1soft
        public float Limit2Min { get; set; } //limit2min
        public float Limit2Max { get; set; } //limit2max
        public float Limit2Soft { get; set; } //limit2soft
        public float Limit3Min { get; set; } //limit3min
        public float Limit3Max { get; set; } //limit3max
        public float Limit3Soft { get; set; } //limit3soft
        public float Stiffness { get; set; } //stiffness
        public Vector4 Position { get; set; } //axisPos
        public Vector4 AxisDirection { get; set; } //fixedAxisDir
        public Vector4 LeanDirection { get; set; } //fixedLeanDir

        public override void Read(Rsc6DataReader reader)
        {
            DOF = reader.ReadInt32();
            Parent = reader.ReadInt32();
            Limit1Min = reader.ReadSingle();
            Limit1Max = reader.ReadSingle();
            Limit1Soft = reader.ReadSingle();
            Limit2Min = reader.ReadSingle();
            Limit2Max = reader.ReadSingle();
            Limit2Soft = reader.ReadSingle();
            Limit3Min = reader.ReadSingle();
            Limit3Max = reader.ReadSingle();
            Limit3Soft = reader.ReadSingle();
            Stiffness = reader.ReadSingle();
            Position = reader.ReadVector4();
            AxisDirection = reader.ReadVector4();
            LeanDirection = reader.ReadVector4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteInt32(DOF);
            writer.WriteInt32(Parent);
            writer.WriteSingle(Limit1Min);
            writer.WriteSingle(Limit1Max);
            writer.WriteSingle(Limit1Soft);
            writer.WriteSingle(Limit2Min);
            writer.WriteSingle(Limit2Max);
            writer.WriteSingle(Limit2Soft);
            writer.WriteSingle(Limit3Min);
            writer.WriteSingle(Limit3Max);
            writer.WriteSingle(Limit3Soft);
            writer.WriteSingle(Stiffness);
            writer.WriteVector4(Position);
            writer.WriteVector4(AxisDirection);
            writer.WriteVector4(LeanDirection);
        }

        public void Read(MetaNodeReader reader)
        {
            DOF = reader.ReadInt32("DOF", DOF);
            Parent = reader.ReadInt32("Parent", Parent);
            Limit1Min = reader.ReadSingle("Limit1Min", Limit1Min);
            Limit1Max = reader.ReadSingle("Limit1Max", Limit1Max);
            Limit1Soft = reader.ReadSingle("Limit1Soft", Limit1Soft);
            Limit2Min = reader.ReadSingle("Limit2Min", Limit2Min);
            Limit2Max = reader.ReadSingle("Limit2Max", Limit2Max);
            Limit2Soft = reader.ReadSingle("Limit2Soft", Limit2Soft);
            Limit3Min = reader.ReadSingle("Limit3Min", Limit3Min);
            Limit3Max = reader.ReadSingle("Limit3Max", Limit3Max);
            Limit3Soft = reader.ReadSingle("Limit3Soft", Limit3Soft);
            Stiffness = reader.ReadSingle("Stiffness", Stiffness);
            Position = ToXYZ(reader.ReadVector4("Position", Position));
            AxisDirection = ToXYZ(reader.ReadVector4("AxisDirection", AxisDirection));
            LeanDirection = ToXYZ(reader.ReadVector4("LeanDirection", LeanDirection));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("DOF", DOF);
            writer.WriteInt32("Parent", Parent);
            writer.WriteSingle("Limit1Min", Limit1Min);
            writer.WriteSingle("Limit1Max", Limit1Max);
            writer.WriteSingle("Limit1Soft", Limit1Soft);
            writer.WriteSingle("Limit2Min", Limit2Min);
            writer.WriteSingle("Limit2Max", Limit2Max);
            writer.WriteSingle("Limit2Soft", Limit2Soft);
            writer.WriteSingle("Limit3Min", Limit3Min);
            writer.WriteSingle("Limit3Max", Limit3Max);
            writer.WriteSingle("Limit3Soft", Limit3Soft);
            writer.WriteSingle("Stiffness", Stiffness);
            writer.WriteVector4("Position", Position);
            writer.WriteVector4("AxisDirection", AxisDirection);
            writer.WriteVector4("LeanDirection", LeanDirection);
        }
    }

    public class Rsc6FragmentDrawable : Rsc6BlockBase, MetaNode //rage::fragDrawable
    {
        /*
         * Handles the loading of the drawing and bounds data for each piece of a fragment type
         * Each fragTypeChild owns one fragDrawable.
         * The fragDrawable also loads other data, such as "locators" which are used to indicate the positions of the characters in vehicle seats, the entry
         * and exit positions from every door, the position of particle effects, etc.
         */

        public override ulong BlockLength => 240;
        public Rsc6Drawable Drawable { get; set; } //rmcDrawable
        public uint Unknown_78h { get; set; } //Padding
        public uint Unknown_7Ch { get; set; } //Padding
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

        public bool UseBoundsArchetype { get; set; } //To point to the archetype's bounds

        public override void Read(Rsc6DataReader reader)
        {
            Drawable = reader.ReadBlock<Rsc6Drawable>();
            Unknown_78h = reader.ReadUInt32();
            Unknown_7Ch = reader.ReadUInt32();
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
            ExtraBoundsMatrices = reader.ReadRawArrItems(ExtraBoundsMatrices, NumExtraBounds);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            Drawable?.Write(writer);
            writer.WriteUInt32(Unknown_78h);
            writer.WriteUInt32(Unknown_7Ch);
            writer.WriteMatrix4x4(BoundMatrix);

            if (UseBoundsArchetype)
                writer.WritePtrEmbed(Bound, Bound, 0);
            else
                writer.WritePtr(Bound);

            writer.WriteArr(ExtraBounds);
            writer.WriteRawArr(ExtraBoundsMatrices);
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

        public void Read(MetaNodeReader reader)
        {
            Drawable = new Rsc6Drawable();
            Drawable.Read(reader);
            BoundMatrix = ToXYZ(reader.ReadMatrix4x4("BoundMatrix"), true);
            Bound = new(reader.ReadNode<Rsc6Bounds>("Bound"));
            ExtraBounds = new(reader.ReadUInt32Array("ExtraBounds"));
            ExtraBoundsMatrices = new(ToXYZ(reader.ReadMatrix4x4Array("ExtraBoundsMatrices"), true));
            LoadSkeleton = reader.ReadBool("LoadSkeleton");
            Locators = new(reader.ReadNode<Rsc6FragDrawableLocator>("Locators"));
            NumLocatorNodes = reader.ReadInt32("NumLocatorNodes");
            NodePool = reader.ReadUInt32("NodePool");
            SkeletonTypeName = new(reader.ReadString("SkeletonTypeName"));
            Animations = new(reader.ReadNodeArray<Rsc6FragAnimation>("Animations"));
            NumExtraBounds = (ushort)(ExtraBoundsMatrices.Items?.Length ?? 0);
        }

        public void Write(MetaNodeWriter writer)
        {
            Drawable.Write(writer);
            writer.WriteMatrix4x4("BoundMatrix", BoundMatrix);
            if (Bound.Item != null) writer.WriteNode("Bound", Bound.Item);
            if (ExtraBounds.Items != null) writer.WriteUInt32Array("ExtraBounds", ExtraBounds.Items);
            if (ExtraBoundsMatrices.Items != null) writer.WriteMatrix4x4Array("ExtraBoundsMatrices", ExtraBoundsMatrices.Items);
            writer.WriteBool("LoadSkeleton", LoadSkeleton);
            if (Locators.Item != null) writer.WriteNode("Locators", Locators.Item);
            writer.WriteInt32("NumLocatorNodes", NumLocatorNodes);
            writer.WriteUInt32("NodePool", NodePool);
            if (SkeletonTypeName.Value != null) writer.WriteString("SkeletonTypeName", SkeletonTypeName.Value);
            if (Animations.Items != null) writer.WriteNodeArray("Animations", Animations.Items);
        }
    }

    public class Rsc6FragAnimation : Rsc6BlockBase, MetaNode //rage::fragAnimation
    {
        /*
         * Owns a crAnimation, for handling of animated parts
         * Animated parts can optionally control a bound part
         * If they do, that part can be broken off if its force limit is exceeded
         */

        public override ulong BlockLength => 24;
        public Rsc6Ptr<Rsc6Animation> Animation { get; set; } //Animation
        public int BoneCount { get; set; } //BoneCount
        public Rsc6RawArr<int> BoneIndices { get; set; } //BoneIndices
        public Rsc6Str Name { get; set; } //Name
        public float PhaseLastFrame { get; set; } //PhaseLastFrame
        public bool AutoPlay { get; set; } //AutoPlay
        public bool AffectsOnlyRootNodes { get; set; } //AffectsOnlyRootNodes
        public byte Pad0 { get; set; } //Padding
        public byte Pad1 { get; set; } //Padding

        public override void Read(Rsc6DataReader reader)
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

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtr(Animation);
            writer.WriteInt32(BoneCount);
            writer.WriteRawArr(BoneIndices);
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
            writer.WriteNode("Animation", Animation.Item);
            writer.WriteInt32("BoneCount", BoneCount);
            writer.WriteInt32Array("BoneIndices", BoneIndices.Items);
            writer.WriteString("Name", Name.Value);
            writer.WriteSingle("PhaseLastFrame", PhaseLastFrame);
            writer.WriteBool("AutoPlay", AutoPlay);
            writer.WriteBool("AffectsOnlyRootNodes", AffectsOnlyRootNodes);
        }
    }

    public class Rsc6FragDrawableLocator : Rsc6BlockBase, MetaNode //rage::fragDrawable::Locator
    {
        public override ulong BlockLength => 48;
        public Vector4 Offset { get; set; } //Offset
        public Vector4 Eulers { get; set; } //Eulers
        public int BoneIndex { get; set; } //BoneIndex
        public uint Pad0 { get; set; } //Pad0
        public uint Pad1 { get; set; } //Pad1
        public uint Pad2 { get; set; } //Pad2

        public override void Read(Rsc6DataReader reader)
        {
            Offset = reader.ReadVector4();
            Eulers = reader.ReadVector4();
            BoneIndex = reader.ReadInt32();
            Pad0 = reader.ReadUInt32();
            Pad1 = reader.ReadUInt32();
            Pad2 = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
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
            Offset = ToXYZ(reader.ReadVector4("Offset"));
            Eulers = ToXYZ(reader.ReadVector4("Eulers"));
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
            DampingConstants = reader.ReadVector4Arr(Rsc6Fragment.NUM_DAMP_TYPES);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4Array(DampingConstants);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            DampingConstants = ToXYZ(reader.ReadVector4Array("DampingConstants"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4Array("DampingConstants", DampingConstants);
        }

        public override string ToString()
        {
            return Name.ToString();
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
        public uint Unknown_20h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD; //Padding
        public float Mass { get; set; } //m_Mass, mass of the bound (set or calculated from density and bound shape)
        public float InvMass { get; set; } //m_InvMass, 1.0f / m_Mass
        public float GravityFactor { get; set; } //m_GravityFactor, a factor by which you multiply the global gravity to get the gravity for this object
        public float MaxSpeed { get; set; } //m_MaxSpeed, maximum speed of this object
        public float MaxAngSpeed { get; set; } //m_MaxAngSpeed, maximum angular speed of this object (radians per second)
        public float BuoyancyFactor { get; set; } //m_BuoyancyFactor, degree to which buoyant forces should be scaled to compensate for the bound + mass
        public Vector4 AngleInertia { get; set; } //m_AngInertia, angular inertia vector (set or calculated from mass and bound shape)
        public Vector4 InverseAngleInertia { get; set; } //m_InvAngInertia, inverse of m_AngInertia

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
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
            writer.WriteUInt32(Unknown_20h);
            writer.WriteUInt32(Unknown_24h);
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
            AngleInertia = ToXYZ(reader.ReadVector4("AngleInertia"));
            InverseAngleInertia = ToXYZ(reader.ReadVector4("InverseAngleInertia"));
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
        public override uint VFT { get; set; } = 0x00E6AEE8;
        public Rsc6ArchetypeType Type { get; set; } //m_Type, the type of archetype (phArchetype, phArchetypePhys, phArchetypeDamp)
        public Rsc6Str Name { get; set; } //m_Filename, filename for the bank saving
        public Rsc6Ptr<Rsc6Bounds> Bounds { get; set; } //m_Bound, the actual bound
        public Rsc6ObjectTypeFlags TypeFlags { get; set; } //m_TypeFlags, specifying the kind of archetype (prop, water, creature, player, vehicle, etc)
        public int IncludeFlags { get; set; } = -1; //m_IncludeFlags, specifying types of archetypes to interact with
        public ushort PropertyFlags { get; set; } //m_PropertyFlags, specifying physical properties (contact forces)
        public ushort RefCount { get; set; } = 1; //m_RefCount, number of references to this archetype

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Type = (Rsc6ArchetypeType)reader.ReadInt32();
            Name = reader.ReadStr();
            Bounds = reader.ReadPtr(Rsc6Bounds.Create);
            TypeFlags = (Rsc6ObjectTypeFlags)reader.ReadUInt32();
            IncludeFlags = reader.ReadInt32();
            PropertyFlags = reader.ReadUInt16();
            RefCount = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteInt32((int)Type);
            writer.WriteStr(Name);
            writer.WritePtr(Bounds);
            writer.WriteUInt32((uint)TypeFlags);
            writer.WriteInt32(IncludeFlags);
            writer.WriteUInt16(PropertyFlags);
            writer.WriteUInt16(RefCount);
        }

        public void Read(MetaNodeReader reader)
        {
            Type = reader.ReadEnum<Rsc6ArchetypeType>("@type");
            Name = new(reader.ReadString("Name"));
            Bounds = new(reader.ReadNode("Bounds", Rsc6Bounds.Create));
            TypeFlags = reader.ReadEnum<Rsc6ObjectTypeFlags>("TypeFlags");
            IncludeFlags = reader.ReadInt32("IncludeFlags");
            PropertyFlags = reader.ReadUInt16("PropertyFlags");
            RefCount = reader.ReadUInt16("RefCount");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteEnum("@type", Type);
            writer.WriteString("Name", Name.ToString());
            writer.WriteNode("Bounds", Bounds.Item);
            writer.WriteEnum("TypeFlags", TypeFlags);
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
            Flags = reader.ReadEnum<Rsc6FragTypeGroupFlag>("Flags");
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
            writer.WriteEnum("Flags", Flags);
            writer.WriteSingle("MinDamageForce", MinDamageForce);
            writer.WriteSingle("DamageHealth", DamageHealth);
            writer.WriteString("DebugName", DebugName);
        }

        public override string ToString()
        {
            return $"{DebugName}, ParentIndex: {ParentGroupPointerIndex}, ChildIndex: {ChildGroupsPointersIndex}";
        }
    }

    public class Rsc6FragPhysChild : Rsc6FileBase, MetaNode //rage::fragTypeChild
    {
        /*
         * Holds type data related to one atomic piece of a fragment type.
         * Simple fragType's have only one of these, complex fragType's have one of these for each piece they can break into.
         * fragTypeChild's are linked together in a complex type by fragTypeGroup's
         */

        public override ulong BlockLength => 192;
        public override uint VFT { get; set; } = 0x00F23A34;
        public float UndamagedMass { get; set; } //m_UndamagedMass
        public float DamagedMass { get; set; } //m_DamagedMass
        public byte OwnerGroupPointerIndex { get; set; } //m_OwnerGroupPointerIndex, index in the fragType to the group that owns us
        public byte Flags { get; set; } //m_Flags
        public ushort BoneID { get; set; } //m_BoneID, the bone of the main entity's skeleton this child follows, 0 if it's parented to the entity itself
        public Matrix4x4 BoneAttachment { get; set; } = Matrix4x4.Identity; //m_BoneAttachment
        public Matrix4x4 LinkAttachment { get; set; } = Matrix4x4.Identity; //m_LinkAttachment
        public Rsc6Ptr<Rsc6FragmentDrawable> UndamagedEntity { get; set; } //m_UndamagedEntity
        public Rsc6Ptr<Rsc6FragmentDrawable> DamagedEntity { get; set; } //DamagedEntity
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
            base.Read(reader);
            UndamagedMass = reader.ReadSingle();
            DamagedMass = reader.ReadSingle();
            OwnerGroupPointerIndex = reader.ReadByte();
            Flags = reader.ReadByte();
            BoneID = reader.ReadUInt16();
            BoneAttachment = reader.ReadMatrix4x4();
            LinkAttachment = reader.ReadMatrix4x4();
            UndamagedEntity = reader.ReadPtr<Rsc6FragmentDrawable>();
            DamagedEntity = reader.ReadPtr<Rsc6FragmentDrawable>();
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
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
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
            UndamagedMass = reader.ReadSingle("UndamagedEntity");
            DamagedMass = reader.ReadSingle("DamagedEntity");
            OwnerGroupPointerIndex = reader.ReadByte("OwnerGroupPointerIndex");
            Flags = reader.ReadByte("Flags");
            BoneID = reader.ReadUInt16("BoneID");
            BoneAttachment = ToXYZ(reader.ReadMatrix4x4("BoneAttachment"), true);
            LinkAttachment = ToXYZ(reader.ReadMatrix4x4("LinkAttachment"), true);
            UndamagedEntity = new(reader.ReadNode<Rsc6FragmentDrawable>("UndamagedEntity"));
            DamagedEntity = new(reader.ReadNode<Rsc6FragmentDrawable>("DamagedEntity"));
            ContinuousEventset = reader.ReadUInt32("ContinuousEventset");
            CollisionEventset = reader.ReadUInt32("CollisionEventset");
            BreakEventset = reader.ReadUInt32("BreakEventset");
            BreakFromRootEventset = reader.ReadUInt32("BreakFromRootEventset");
            CollisionEventPlayer = reader.ReadUInt32("CollisionEventPlayer");
            BreakEventPlayer = reader.ReadUInt32("BreakEventPlayer");
            BreakFromRootEventPlayer = reader.ReadUInt32("BreakFromRootEventPlayer");
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
            writer.WriteNode("UndamagedEntity", UndamagedEntity.Item);
            writer.WriteNode("DamagedEntity", DamagedEntity.Item);
            writer.WriteUInt32("ContinuousEventset", ContinuousEventset);
            writer.WriteUInt32("CollisionEventset", CollisionEventset);
            writer.WriteUInt32("BreakEventset", BreakEventset);
            writer.WriteUInt32("BreakFromRootEventset", BreakFromRootEventset);
            writer.WriteUInt32("CollisionEventPlayer", CollisionEventPlayer);
            writer.WriteUInt32("BreakEventPlayer", BreakEventPlayer);
            writer.WriteUInt32("BreakFromRootEventPlayer", BreakFromRootEventPlayer);
        }
    }

    public class Rsc6PhysicsInstance : Rsc6FileBase, MetaNode //rage::phInst
    {
        /*
         * Basic instance class for all physical objects, storing a matrix for position and
         * orientation, a pointer to the physics archetype (which contains physical information),
         * flags and the index used by the physics level
         */

        public override ulong BlockLength => 80;
        public override uint VFT { get; set; } = 0x04E0A528;
        public Rsc6Ptr<Rsc6FragArchetype> Archetype { get; set; } //m_Archetype
        public ushort LevelIndex { get; set; } //m_LevelIndex, the "handle" for this instance in the level
        public ushort Flags { get; set; } //m_Flags
        public uint UserData { get; set; } //m_UserData
        public Matrix4x4 Matrix { get; set; } //m_Matrix, orientation (3x3 part) and position (d-vector) for the physics instance

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Archetype = reader.ReadPtr<Rsc6FragArchetype>();
            LevelIndex = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            UserData = reader.ReadUInt32();
            Matrix = reader.ReadMatrix4x4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Archetype);
            writer.WriteUInt16(LevelIndex);
            writer.WriteUInt16(Flags);
            writer.WriteUInt32(UserData);
            writer.WriteMatrix4x4(Matrix);
        }

        public void Read(MetaNodeReader reader)
        {
            Archetype = new(reader.ReadNode<Rsc6FragArchetype>("Archetype"));
            LevelIndex = reader.ReadUInt16("LevelIndex");
            Flags = reader.ReadUInt16("Flags");
            UserData = reader.ReadUInt32("UserData");
            Matrix = ToXYZ(reader.ReadMatrix4x4("Matrix"), true);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("Archetype", Archetype.Item);
            writer.WriteUInt16("LevelIndex", LevelIndex);
            writer.WriteUInt16("Flags", Flags);
            writer.WriteUInt32("UserData", UserData);
            writer.WriteMatrix4x4("Matrix", Matrix);
        }
    }

    public class Rsc6BoundInstance : Rsc6BlockBaseMap //sagBoundInstance, TODO: finish this
    {
        public override ulong BlockLength => 80;
        public override uint VFT { get; set; } = 0x01909C20;
        public Rsc6Ptr<Rsc6FragArchetype> Archetype { get; set; } //m_archetype
        public Rsc6Ptr<Rsc6FragArchetype> PhysInstance { get; set; } //m_physInstance

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Archetype = reader.ReadPtr<Rsc6FragArchetype>();
            PhysInstance = reader.ReadPtr<Rsc6FragArchetype>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    public class Rsc6AssociationInfo : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 20;
        public Rsc6Str FragmentName { get; set; } //m_FragmentName
        public Rsc6Str NonMangledName { get; set; } //m_NonMangledName
        public Rsc6Str LocatorName { get; set; } //m_LocatorName
        public JenkHash DesignTagHash { get; set; } //m_DesignTagHash
        public bool Default { get; set; } //m_bDefault
        public bool HardLink { get; set; } //m_bHardLink
        public bool Detachable { get; set; } //m_bDetachable
        public byte Pad { get; set; } //m_Pad

        public override void Read(Rsc6DataReader reader)
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

        public override void Write(Rsc6DataWriter writer)
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

    public class Rsc6FragEnvCloth : Rsc6FileBase
    {
        public override ulong BlockLength => 0;
        public override uint VFT { get; set; } = 0x00F2017C;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    public class Rsc6CollisionEventSet : Rsc6FileBase, MetaNode //rage::evtSet
    {
        /*
         * A set of events that should all be started, stopped and updated in unison.
         * This might be used to signal to a number of systems that some discrete event has occured
         * (for example all the event instances in an event set might be triggered when an object breaks)
         */

        public override ulong BlockLength => 24;
        public override uint VFT { get; set; } = 0x00D55540;
        public Rsc6PtrArr<Rsc6EventInstance> Instances { get; set; } //m_Instances
        public int NewInstanceType { get; set; } //m_NewInstanceType, always 0
        public uint Bank { get; set; } //m_Bank, always NULL (bkBank = bank of widgets)
        public uint Group { get; set; } //m_Group, always NULL (bkGroup = group of widgets)

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Instances = reader.ReadPtrArr<Rsc6EventInstance>();
            NewInstanceType = reader.ReadInt32();
            Bank = reader.ReadUInt32();
            Group = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtrArr(Instances);
            writer.WriteInt32(NewInstanceType);
            writer.WriteUInt32(Bank);
            writer.WriteUInt32(Group);
        }

        public void Read(MetaNodeReader reader)
        {
            Instances = new(reader.ReadNodeArray<Rsc6EventInstance>("Instances"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Instances", Instances.Items);
        }
    }

    public class Rsc6EventInstance : Rsc6FileBase, MetaNode //rage::evtInstance
    {
        public override ulong BlockLength => 36;
        public override uint VFT { get; set; } = 0x00D0EB6C;
        public float MinT { get; set; } //m_MinT, always 0.0f
        public float MaxT { get; set; } //m_MaxT, 0.0f or 1.0f
        public int EventType { get; set; } //m_EventType, always 0
        public JenkHash EventTypeHash { get; set; } //m_EventTypeHash, always 0 or 0x05927AD8 or 0x0C9CD783
        public short Flags { get; set; } = 15; //m_Flags, always 15
        public byte TrackNumber { get; set; } //m_TrackNumber, always 0
        public byte Priority { get; set; } //m_Priority, always 0
        public int WidgetGroup { get; set; } //m_WidgetGroup, always NULL
        public int EditorWidget { get; set; } //m_EditorWidget, always NULL
        public Rsc6Ptr<Rsc6CollisionEventSet> Set { get; set; } //m_Set, always NULL

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            MinT = reader.ReadSingle();
            MaxT = reader.ReadSingle();
            EventType = reader.ReadInt32();
            EventTypeHash = reader.ReadUInt32();
            Flags = reader.ReadInt16();
            TrackNumber = reader.ReadByte();
            Priority = reader.ReadByte();
            WidgetGroup = reader.ReadInt32();
            EditorWidget = reader.ReadInt32();
            Set = reader.ReadPtr<Rsc6CollisionEventSet>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle(MinT);
            writer.WriteSingle(MaxT);
            writer.WriteInt32(EventType);
            writer.WriteUInt32(EventTypeHash);
            writer.WriteInt16(Flags);
            writer.WriteByte(TrackNumber);
            writer.WriteByte(Priority);
            writer.WriteInt32(WidgetGroup);
            writer.WriteInt32(EditorWidget);
            writer.WritePtr(Set);
        }

        public void Read(MetaNodeReader reader)
        {
            MinT = reader.ReadSingle("Min");
            MaxT = reader.ReadSingle("Max");
            EventTypeHash = reader.ReadJenkHash("EventTypeHash");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteSingle("Min", MinT);
            writer.WriteSingle("Max", MaxT);
            writer.WriteJenkHash("EventTypeHash", EventTypeHash);
        }
    }

    public class Rsc6CollisionEventPlayer : Rsc6BlockBase, MetaNode //rage::evtPlayer
    {
        public override ulong BlockLength => 44;
        public Rsc6Arr<uint> Params { get; set; } //m_Params, atArray<atAny>, always NULL
        public Rsc6Arr<int> Scheme { get; set; } //m_Scheme, evtParamScheme* -> atArray<int> m_ParamIdToSlotNum, always NULL
        public Rsc6Ptr<Rsc6EventTimeline> Timeline { get; set; } //m_Timeline
        public Rsc6Ptr<Rsc6CollisionEventSet> EventSet { get; set; } //m_Eventset, always NULL
        public float LastT { get; set; } //m_LastT
        public Rsc6EventDirection LastDirection { get; set; } = Rsc6EventDirection.DIR_FORWARD; //m_LastDirection
        public int StartInstance { get; set; } = -1; //m_StartInstance, where to start when scanning over the list of instances
        public int LastKnownModification { get; set; } //m_LastKnownModification
        public bool Playing { get; set; } //m_Playing
        public bool ControlTimelinePlayhead { get; set; } //m_ControlTimelinePlayhead
        public ushort Unknown_1Ah { get; set; } //Padding

        public override void Read(Rsc6DataReader reader)
        {
            Params = reader.ReadArr<uint>();
            Scheme = reader.ReadArr<int>();
            Timeline = reader.ReadPtr<Rsc6EventTimeline>();
            EventSet = reader.ReadPtr<Rsc6CollisionEventSet>();
            LastT = reader.ReadSingle();
            LastDirection = (Rsc6EventDirection)reader.ReadInt32();
            StartInstance = reader.ReadInt32();
            LastKnownModification = reader.ReadInt32();
            Playing = reader.ReadBoolean();
            ControlTimelinePlayhead = reader.ReadBoolean();
            Unknown_1Ah = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(Params);
            writer.WriteArr(Scheme);
            writer.WritePtr(Timeline);
            writer.WritePtr(EventSet);
            writer.WriteSingle(LastT);
            writer.WriteInt32((int)LastDirection);
            writer.WriteInt32(StartInstance);
            writer.WriteInt32(LastKnownModification);
            writer.WriteBoolean(Playing);
            writer.WriteBoolean(ControlTimelinePlayhead);
            writer.WriteUInt16(Unknown_1Ah);
        }

        public void Read(MetaNodeReader reader)
        {
            Timeline = new(reader.ReadNode<Rsc6EventTimeline>("Timeline"));
            LastT = reader.ReadSingle("LastT");
            LastDirection = reader.ReadEnum<Rsc6EventDirection>("LastDirection");
            LastKnownModification = reader.ReadInt32("LastKnownModification");
            Playing = reader.ReadBool("Playing");
            ControlTimelinePlayhead = reader.ReadBool("ControlTimelinePlayhead");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("Timeline", Timeline.Item);
            writer.WriteSingle("LastT", LastT);
            writer.WriteEnum("LastDirection", LastDirection);
            writer.WriteInt32("LastKnownModification", LastKnownModification);
            writer.WriteBool("Playing", Playing);
            writer.WriteBool("ControlTimelinePlayhead", ControlTimelinePlayhead);
        }
    }

    public class Rsc6EventTimeline : Rsc6FileBase, MetaNode //rage::evtTimeline
    {
        /*
         * A collection of event instances that each have start and stop times.
         * An evtPlayer can play the timeline, and events in the timeline will be started, updated and stopped based on the player's current position.
         */

        public override ulong BlockLength => 24;
        public override uint VFT { get; set; } = 0x00D55540;
        public Rsc6PtrArr<Rsc6EventInstance> Instances { get; set; } //m_Instances
        public Rsc6Arr<int> MaxTimeOrder { get; set; } //m_MaxTimeOrder
        public bool SortStatus { get; set; } //m_SortStatus
        public byte Unknown_15h { get; set; } //Padding
        public ushort Unknown_16h { get; set; } //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Instances = reader.ReadPtrArr<Rsc6EventInstance>();
            MaxTimeOrder = reader.ReadArr<int>();
            SortStatus = reader.ReadBoolean();
            Unknown_15h = reader.ReadByte();
            Unknown_16h = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtrArr(Instances);
            writer.WriteArr(MaxTimeOrder);
            writer.WriteBoolean(SortStatus);
            writer.WriteByte(Unknown_15h);
            writer.WriteUInt16(Unknown_16h);
        }

        public void Read(MetaNodeReader reader)
        {
            Instances = new(reader.ReadNodeArray<Rsc6EventInstance>("Instances"));
            MaxTimeOrder = new(reader.ReadInt32Array("MaxTimeOrder"));
            SortStatus = reader.ReadBool("SortStatus");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Instances", Instances.Items);
            writer.WriteInt32Array("MaxTimeOrder", MaxTimeOrder.Items);
            writer.WriteBool("SortStatus", SortStatus);
        }
    }

    public class Rsc6Frame : Rsc6FileBase, MetaNode //rage::crFrame
    {
        /*
         * Used to hold a snapshot of values from an animation at a particular moment in time.
         * This snapshot can then be manipulated and combined with others.
         * Frames contain a collection of degrees of freedom (DOFs).
         */

        public override ulong BlockLength => 20;
        public override uint VFT { get; set; } = 0x00FA3E4C;
        public uint Accelerator { get; set; } //m_Accelerator, always NULL
        public uint Signature { get; set; } //m_Signature, hash representing all the DoFs (their tracks/ids/types) using Fletcher's algorithm
        public Rsc6PtrArr<Rsc6FrameDof> Dofs { get; set; } //m_Dofs, crFrameDof

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Accelerator = reader.ReadUInt32();
            Signature = reader.ReadUInt32();
            Dofs = reader.ReadPtrArr<Rsc6FrameDof>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Accelerator);
            writer.WriteUInt32(Signature);
            writer.WritePtrArr(Dofs);
        }

        public void Read(MetaNodeReader reader)
        {
            Signature = reader.ReadUInt32("Signature");
            Dofs = new(reader.ReadNodeArray<Rsc6FrameDof>("Dofs"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Signature", Signature);
            writer.WriteNodeArray("Dofs", Dofs.Items);
        }
    }

    public class Rsc6FrameDof : Rsc6FileBase, MetaNode //rage::crFrameDof
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x00FA2934;
        public ushort Offset { get; set; } //m_Offset
        public Rsc6BoneIdEnum ID { get; set; } //m_Id
        public uint Unknown_8h { get; set; } //Always 0
        public uint Unknown_Ch { get; set; } //Always 0
        public Vector4 Transform { get; set; } = Rpf6Crypto.GetVec4NaN(); //Always NULL (NaN)

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Offset = reader.ReadUInt16();
            ID = (Rsc6BoneIdEnum)reader.ReadUInt16();
            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            Transform = reader.ReadVector4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt16(Offset);
            writer.WriteUInt16((ushort)ID);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteVector4(Transform);
        }

        public void Read(MetaNodeReader reader)
        {
            Offset = reader.ReadUInt16("Offset");
            ID = reader.ReadEnum<Rsc6BoneIdEnum>("ID");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt16("Offset", Offset);
            writer.WriteEnum("ID", ID);
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }

    public class Rsc6MatrixSet : Rsc6BlockBase
    {
        public override ulong BlockLength => 48;
        public uint D3DCommon { get; set; } = 0xCDCDCDCD; //D3DCommon, Xbox 360 only
        public uint D3DReferenceCount { get; set; } = 0xCDCDCDCD; //D3DReferenceCount, Xbox 360 only
        public uint D3DFence { get; set; } = 0xCDCDCDCD; //D3DFence, Xbox 360 only
        public uint D3DReadFence { get; set; } = 0xCDCDCDCD; //D3DReadFence, Xbox 360 only
        public byte MatrixCount { get; set; } //m_MatrixCount
        public byte MaxMatrixCount { get; set; } //m_MaxMatrixCount
        public bool IsSkinned { get; set; } //m_IsSkinned
        public byte Padding { get; set; } //m_Padding, always 0
        public uint D3DBaseFlush { get; set; } = 0xCDCDCDCD; //D3DBaseFlush, Xbox 360 only
        public uint D3DBaseAddress { get; set; } = 0xCDCDCDCD; //D3DBaseAddress, Xbox 360 only
        public uint D3DSize { get; set; } = 0xCDCDCDCD; //D3DSize, Xbox 360 only

        public Rsc6MatrixSet()
        {
        }

        public Rsc6MatrixSet(int matrixCount)
        {
            MatrixCount = (byte)matrixCount;
            MaxMatrixCount = (byte)matrixCount;
        }

        public override void Read(Rsc6DataReader reader)
        {
            D3DCommon = reader.ReadUInt32();
            D3DReferenceCount = reader.ReadUInt32();
            D3DFence = reader.ReadUInt32();
            D3DReadFence = reader.ReadUInt32();
            MatrixCount = reader.ReadByte();
            MaxMatrixCount = reader.ReadByte();
            IsSkinned = reader.ReadBoolean();
            Padding = reader.ReadByte();
            D3DBaseFlush = reader.ReadUInt32();
            D3DBaseAddress = reader.ReadUInt32();
            D3DSize = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(D3DCommon);
            writer.WriteUInt32(D3DReferenceCount);
            writer.WriteUInt32(D3DFence);
            writer.WriteUInt32(D3DReadFence);
            writer.WriteByte(MatrixCount);
            writer.WriteByte(MaxMatrixCount);
            writer.WriteBoolean(IsSkinned);
            writer.WriteByte(Padding);
            writer.WriteUInt32(D3DBaseFlush);
            writer.WriteUInt32(D3DBaseAddress);
            writer.WriteUInt32(D3DSize);
        }
    }

    public class Rsc6FragChildDataSet : Rsc6FileBase, MetaNode //sagFragTypeChildDataSet
    {
        public override ulong BlockLength => 12;
        public override uint VFT { get; set; } = 0x0105F6F8;
        public Rsc6ManagedArr<Rsc6FragChildData> DataArray { get; set; } //m_DataArray

        public int[] ClassItems
        {
            get
            {
                if (DataArray.Items == null)
                {
                    return null;
                }
                else
                {
                    return DataArray.Items.Select(child => child.ClassID).ToArray();
                }
            }
            set
            {
                var childs = new Rsc6FragChildData[value.Length];
                for (int i = 0; i < childs.Length; i++)
                {
                    childs[i] = new Rsc6FragChildData()
                    {
                        ClassID = value[i]
                    };
                }
                DataArray = new(childs);
            }
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            DataArray = reader.ReadArr<Rsc6FragChildData>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteArr(DataArray);
        }

        public void Read(MetaNodeReader reader)
        {
            ClassItems = reader.ReadInt32Array("DataArray");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32Array("DataArray", ClassItems);
        }
    }

    public class Rsc6FragChildData : Rsc6FileBase //sagFragTypeChildData
    {
        public override ulong BlockLength => 8;
        public override uint VFT { get; set; } = 0x0106EE74;
        public int ClassID { get; set; } //m_ClassID

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ClassID = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteInt32(ClassID);
        }

        public override string ToString()
        {
            return ClassID.ToString();
        }
    }

    public class Rsc6TargetManager : Rsc6BlockBaseMapRef, MetaNode //rage::grbTargetManager
    {
        /*
         * A manager class for a group of blend targets
         */

        public override ulong BlockLength => 36;
        public override uint VFT { get; set; } = 0x010E4920;
        public Rsc6PtrArr<Rsc6BlendShape> BlendShapes { get; set; } //m_BlendShapes
        public Rsc6AtMapArr<Rsc6TargetEntry> Targets { get; set; } //m_Targets

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlendShapes = reader.ReadPtrArr<Rsc6BlendShape>();
            Targets = reader.ReadAtMapArr<Rsc6TargetEntry>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtrArr(BlendShapes);
            writer.WriteAtMapArr(Targets);
        }

        public void Read(MetaNodeReader reader)
        {
            BlendShapes = new(reader.ReadNodeArray("BlendShapes", (_) => new Rsc6BlendShape(this)));
            Targets = new(reader.ReadNodeArray<Rsc6TargetEntry>("Targets"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("BlendShapes", BlendShapes.Items);
            writer.WriteNodeArray("Targets", Targets.Items);
        }
    }

    public class Rsc6TargetEntry : Rsc6BlockBase, MetaNode //rage::atMapEntry<rage::datPadded<unsigned short>, rage::datOwner<rage::grbTarget>>
    {
        public override ulong BlockLength => 12;
        public ushort Key { get; set; } //key, the ID of the target to blend
        public ushort Pad { get; set; } = 0xCDCD; //datPadded
        public Rsc6Ptr<Rsc6Target> Data { get; set; } //data
        public Rsc6Ptr<Rsc6TargetEntry> Next { get; set; } //next
        public Rsc6TargetEntry EntryNext { get => Next.Item; set => Next = new(value); }

        public override void Read(Rsc6DataReader reader)
        {
            Key = reader.ReadUInt16();
            Pad = reader.ReadUInt16();
            Data = reader.ReadPtr<Rsc6Target>();
            Next = reader.ReadPtr<Rsc6TargetEntry>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt16(Key);
            writer.WriteUInt16(Pad);
            writer.WritePtr(Data);
            writer.WritePtr(Next);
        }

        public void Read(MetaNodeReader reader)
        {
            Key = reader.ReadUInt16("Key");
            Data = new(reader.ReadNode<Rsc6Target>("Data"));
            Next = new(reader.ReadNode<Rsc6TargetEntry>("Next"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt16("Key", Key);
            writer.WriteNode("Data", Data.Item);
            writer.WriteNode("Next", Next.Item);
        }
    }

    public class Rsc6Target : Rsc6FileBase, MetaNode //rage::grbTarget
    {
        public override ulong BlockLength => 80;
        public override uint VFT { get; set; } = 0x010E4934;
        public string Name { get; set; } //m_Name, fixed-size string
        public int TargetIndex { get; set; } //m_TargetIndex
        public Rsc6PtrArr<Rsc6Morphable> Morphables { get; set; } //m_Morphables

        public string FixedName
        {
            get
            {
                if (!Name.Contains('\0')) return base.ToString();
                return Name[..Name.IndexOf('\0')];
            }
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadStringWithLength(64);
            TargetIndex = reader.ReadInt32();
            Morphables = reader.ReadPtrArr<Rsc6Morphable>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            var nameLength = Name.Length + 1;
            var padding = new byte[64 - nameLength];

            if (!string.IsNullOrEmpty(Name))
            {
                writer.WriteStringNullTerminated(Name);
            }

            writer.WriteBytes(padding);
            writer.WriteInt32(TargetIndex);
            writer.WritePtrArr(Morphables);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = reader.ReadString("Name");
            TargetIndex = reader.ReadInt32("TargetIndex");
            Morphables = new(reader.ReadNodeArray<Rsc6Morphable>("Morphables"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", FixedName);
            writer.WriteInt32("TargetIndex", TargetIndex);
            writer.WriteNodeArray("Morphables", Morphables.Items);
        }

        public override string ToString()
        {
            return FixedName;
        }
    }

    public class Rsc6BlendShape : Rsc6BlockBase, MetaNode //rage::grbBlendShape
    {
        /*
         * Blendshape interface, provides access to blending functionality
         */

        public override ulong BlockLength => 148;
        public string Name { get; set; } //m_Name, fixed-size string
        public int LodGroup { get; set; } //m_LodGroup
        public int ModelIndex { get; set; } //m_ModelIndex
        public Rsc6PtrArr<Rsc6Morphable> Morphs { get; set; } //m_Morphs
        public uint Manager { get; set; } //m_Manager, pointer to the parent target manager (Rsc6Ptr<Rsc6TargetManager>)

        public Rsc6TargetManager TargetManager;

        public string FixedName
        {
            get
            {
                if (!Name.Contains('\0')) return base.ToString();
                return Name[..Name.IndexOf('\0')];
            }
        }

        public Rsc6BlendShape()
        {         
        }

        public Rsc6BlendShape(Rsc6TargetManager manager)
        {
            TargetManager = manager;
        }

        public override void Read(Rsc6DataReader reader)
        {
            Name = reader.ReadStringWithLength(128);
            LodGroup = reader.ReadInt32();
            ModelIndex = reader.ReadInt32();
            Morphs = reader.ReadPtrArr<Rsc6Morphable>();
            Manager = reader.ReadUInt32();

            if (Morphs.Items != null)
            {
                for (int i = 0; i < Morphs.Items.Length; i++)
                {
                    var m = Morphs.Items[i];
                    m.BlendShape = this;
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            var nameLength = Name.Length + 1;
            var padding = new byte[128 - nameLength];

            if (!string.IsNullOrEmpty(Name))
            {
                writer.WriteStringNullTerminated(Name);
            }

            writer.WriteBytes(padding);
            writer.WriteInt32(LodGroup);
            writer.WriteInt32(ModelIndex);
            writer.WritePtrArr(Morphs);
            writer.WritePtrEmbed(TargetManager, TargetManager, 0);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = reader.ReadString("Name");
            LodGroup = reader.ReadInt32("LodGroup");
            ModelIndex = reader.ReadInt32("ModelIndex");

            var morphs = reader.ReadNodeArray<Rsc6Morphable>("Morphs");
            for (int i = 0; i < morphs.Length; i++)
            {
                var m = morphs[i];
                m.BlendShape = this;
            }
            Morphs = new(morphs);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", FixedName);
            writer.WriteInt32("LodGroup", LodGroup);
            writer.WriteInt32("ModelIndex", ModelIndex);
            writer.WriteNodeArray("Morphs", Morphs.Items);
        }

        public override string ToString()
        {
            return FixedName;
        }
    }

    public class Rsc6Morphable : Rsc6BlockBase, MetaNode //rage::grbMorphable
    {
        /*
         * Manage blend targets of an individual submesh
         */

        public override ulong BlockLength => 124;
        public string Name { get; set; } //m_Name, fixed-size string
        public int MaterialIndex { get; set; } //m_MaterialIndex
        public Rsc6PtrArr<Rsc6TargetData> Targets { get; set; } //m_Targets
        public uint Parent { get; set; } //m_Parent, pointer to the parent blend shape (Rsc6Ptr<Rsc6BlendShape>)
        public Rsc6PtrArr<Rsc6VertexBuffer> Offsets { get; set; } //m_Offsets
        public Rsc6Ptr<Rsc6VertexDeclaration> Fvf { get; set; } //m_Fvf
        public int VertexCount { get; set; } //m_VertexCount
        public int DrawBufferIdx { get; set; } //m_DrawBufferIdx
        public int UpdateBufferIdx { get; set; } //m_UpdateBufferIdx
        public bool IsResource { get; set; } //m_IsResource
        public byte[] Pad { get; set; } = new byte[3] { 0xCD, 0xCD, 0xCD }; //m_Pad
        public int BlendCount { get; set; } //BlendCount
        public uint BlendSize { get; set; } //BlendSize
        public uint Pad0 { get; set; } = 0xCDCDCDCD; //pad0
        public uint Pad1 { get; set; } = 0xCDCDCDCD; //pad1

        public Rsc6BlendShape BlendShape;

        public string FixedName
        {
            get
            {
                if (!Name.Contains('\0')) return base.ToString();
                return Name[..Name.IndexOf('\0')];
            }
        }

        public override void Read(Rsc6DataReader reader)
        {
            Name = reader.ReadStringWithLength(64);
            MaterialIndex = reader.ReadInt32();
            Targets = reader.ReadPtrArr<Rsc6TargetData>();
            Parent = reader.ReadUInt32();
            Offsets = reader.ReadPtrArr<Rsc6VertexBuffer>();
            Fvf = reader.ReadPtr<Rsc6VertexDeclaration>();
            VertexCount = reader.ReadInt32();
            DrawBufferIdx = reader.ReadInt32();
            UpdateBufferIdx = reader.ReadInt32();
            IsResource = reader.ReadBoolean();
            Pad = reader.ReadBytes(3);
            BlendCount = reader.ReadInt32();
            BlendSize = reader.ReadUInt32();
            Pad0 = reader.ReadUInt32();
            Pad1 = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            var nameLength = Name.Length + 1;
            var padding = new byte[64 - nameLength];

            if (!string.IsNullOrEmpty(Name))
            {
                writer.WriteStringNullTerminated(Name);
            }

            writer.WriteBytes(padding);
            writer.WriteInt32(MaterialIndex);
            writer.WritePtrArr(Targets);
            writer.WritePtrEmbed(BlendShape, 0, 0);
            writer.WritePtrArr(Offsets);
            writer.WritePtr(Fvf);
            writer.WriteInt32(VertexCount);
            writer.WriteInt32(DrawBufferIdx);
            writer.WriteInt32(UpdateBufferIdx);
            writer.WriteBoolean(IsResource);
            writer.WriteBytes(Pad);
            writer.WriteInt32(BlendCount);
            writer.WriteUInt32(BlendSize);
            writer.WriteUInt32(Pad0);
            writer.WriteUInt32(Pad1);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = reader.ReadString("Name");
            MaterialIndex = reader.ReadInt32("MaterialIndex");
            Targets = new(reader.ReadNodeArray<Rsc6TargetData>("Targets"));
            Offsets = new(reader.ReadNodeArray<Rsc6VertexBuffer>("Offsets"));
            Fvf = new(reader.ReadNode<Rsc6VertexDeclaration>("Fvf"));
            VertexCount = reader.ReadInt32("VertexCount");
            DrawBufferIdx = reader.ReadInt32("DrawBufferIdx");
            UpdateBufferIdx = reader.ReadInt32("UpdateBufferIdx");
            IsResource = reader.ReadBool("IsResource");
            BlendCount = reader.ReadInt32("BlendCount");
            BlendSize = reader.ReadUInt32("BlendSize");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("MaterialIndex", MaterialIndex);
            writer.WriteNodeArray("Targets", Targets.Items);
            writer.WriteNodeArray("Offsets", Offsets.Items);
            writer.WriteNode("Fvf", Fvf.Item);
            writer.WriteInt32("VertexCount", VertexCount);
            writer.WriteInt32("DrawBufferIdx", DrawBufferIdx);
            writer.WriteInt32("UpdateBufferIdx", UpdateBufferIdx);
            writer.WriteBool("IsResource", IsResource);
            writer.WriteInt32("BlendCount", BlendCount);
            writer.WriteUInt32("BlendSize", BlendSize);
        }

        public override string ToString()
        {
            return FixedName;
        }
    }

    public class Rsc6TargetData : Rsc6BlockBase, MetaNode //rage::grbTargetData
    {
        public override ulong BlockLength => 40;
        public int TotalMeshVerts { get; set; } //m_TotalMeshVerts
        public bool HasPositions { get; set; } //m_HasPositions
        public bool HasNormals { get; set; } //m_HasNormals
        public bool HasTangents { get; set; } //m_HasTangents
        public byte Pad { get; set; } = 0xCD; //m_Pad
        public Rsc6Arr<ulong> Positions { get; set; } //m_Positions
        public Rsc6Arr<int> Normals { get; set; } //m_Normals
        public Rsc6Arr<int> Tangents { get; set; } //m_Tangents
        public Rsc6Arr<int> VertexMap { get; set; } //m_VertexMap

        public Vector4[] DataPos
        {
            get
            {
                var count = Positions.Count;
                var pos = new List<Vector4>();
                for (int i = 0; i < count; i++)
                {
                    var x = (ushort)(Positions[i] & 0xFFFF);
                    var y = (ushort)((Positions[i] >> 16) & 0xFFFF);
                    var z = (ushort)((Positions[i] >> 32) & 0xFFFF);
                    var w = (ushort)((Positions[i] >> 48) & 0xFFFF);
                    pos.Add(new Vector4(x, y, z, w));
                }
                return pos.ToArray();
            }
        }

        public override void Read(Rsc6DataReader reader)
        {
            TotalMeshVerts = reader.ReadInt32();
            HasPositions = reader.ReadBoolean();
            HasNormals = reader.ReadBoolean();
            HasTangents = reader.ReadBoolean();
            Pad = reader.ReadByte();
            Positions = reader.ReadArr<ulong>();
            Normals = reader.ReadArr<int>();
            Tangents = reader.ReadArr<int>();
            VertexMap = reader.ReadArr<int>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteInt32(TotalMeshVerts);
            writer.WriteBoolean(HasPositions);
            writer.WriteBoolean(HasNormals);
            writer.WriteBoolean(HasTangents);
            writer.WriteByte(Pad);
            writer.WriteArr(Positions);
            writer.WriteArr(Normals);
            writer.WriteArr(Tangents);
            writer.WriteArr(VertexMap);
        }

        public void Read(MetaNodeReader reader)
        {
            TotalMeshVerts = reader.ReadInt32("TotalMeshVerts");
            Positions = new(reader.ReadUInt64Array("Positions"));
            Normals = new(reader.ReadInt32Array("Normals"));
            Tangents = new(reader.ReadInt32Array("Tangents"));
            VertexMap = new(reader.ReadInt32Array("VertexMap"));
            HasPositions = Positions.Items != null;
            HasNormals = Normals.Items != null;
            HasTangents = Tangents.Items != null;
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("TotalMeshVerts", TotalMeshVerts);
            writer.WriteUInt64Array("Positions", Positions.Items);
            writer.WriteInt32Array("Normals", Normals.Items);
            writer.WriteInt32Array("Tangents", Tangents.Items);
            writer.WriteInt32Array("VertexMap", VertexMap.Items);
        }
    }

    public class Rsc6FragmentCloth : Rsc6FileBase, MetaNode //rage::fragTypeCharClot
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x00F22614;
        public uint OwnedDrawable { get; set; } //m_OwnedDrawable, always 0
        public uint RefDrawablePos { get; set; } //m_ReferencedDrawable
        public uint RagDollType { get; set; } = (uint)Rpf6Crypto.VIRTUAL_BASE; //m_RagDollType
        public uint RagDoll { get; set; } //m_RagDoll, always 0
        public Rsc6PtrArr<Rsc6CharacterClothController> ClothControllers { get; set; } //m_ClothControllers, capacity always set to 16
        public bool PointersReferenced { get; set; } //m_PointersReferenced, always FALSE
        public bool DrawableCloned { get; set; } //m_DrawableCloned, always FALSE
        public ushort Pad { get; set; } //m_Pad

        public Rsc6FragmentDrawable ReferencedDrawable { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            OwnedDrawable = reader.ReadUInt32();
            RefDrawablePos = reader.ReadUInt32();
            RagDollType = reader.ReadUInt32();
            RagDoll = reader.ReadUInt32();
            ClothControllers = reader.ReadPtrArr<Rsc6CharacterClothController>();
            PointersReferenced = reader.ReadBoolean();
            DrawableCloned = reader.ReadBoolean();
            Pad = reader.ReadUInt16();

            reader.BlockPool.TryGetValue(RefDrawablePos, out var val);
            if (val != null && val is Rsc6FragmentDrawable drawable)
            {
                ReferencedDrawable = drawable;
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(OwnedDrawable);
            writer.WritePtrEmbed(ReferencedDrawable, ReferencedDrawable, 0);
            writer.WriteUInt32(RagDollType);
            writer.WriteUInt32(RagDoll);
            writer.WritePtrArr(ClothControllers);
            writer.WriteBoolean(PointersReferenced);
            writer.WriteBoolean(DrawableCloned);
            writer.WriteUInt16(Pad);
        }

        public void Read(MetaNodeReader reader)
        {
            var c = reader.ReadNodeArray<Rsc6CharacterClothController>("ClothControllers");
            ClothControllers = new(c, 16, (ushort)c.Length);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("ClothControllers", ClothControllers.Items);
        }
    }

    public class Rsc6CharacterClothController : Rsc6ClothController, MetaNode //rage::characterClothController
    {
        public override ulong BlockLength => base.BlockLength + 40;
        public uint ClothType { get; set; } = 2; //m_ClothType, always 2
        public Rsc6Ptr<Rsc6VertexBuffer> OriginalVertsOwned { get; set; } //m_OriginalVertsOwned
        public uint OriginalVertsReferenced { get; set; } //m_OriginalVertsReferenced, always 0
        public uint Skeleton { get; set; } //m_Skeleton, always 0
        public float GravityScale { get; set; } = 1.0f; //m_GravityScale, always 1.0f
        public uint Pad { get; set; } = 0xCDCDCD00; //m_Pad
        public uint BendSpringLengthChannelData { get; set; } //m_BendSpringLengthChannelData, always 0
        public uint BendSpringStrengthChannelData { get; set; } //m_BendSpringStrengthChannelData, always 0
        public uint EdgeCompressionChannelData { get; set; } //m_EdgeCompressionChannelData, always 0
        public float PinningRadiusScale { get; set; } = 1.0f; //m_PinningRadiusScale, always 1.0f

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ClothType = reader.ReadUInt32();
            OriginalVertsOwned = reader.ReadPtr<Rsc6VertexBuffer>();
            OriginalVertsReferenced = reader.ReadUInt32();
            Skeleton = reader.ReadUInt32();
            GravityScale = reader.ReadSingle();
            Pad = reader.ReadUInt32();
            BendSpringLengthChannelData = reader.ReadUInt32();
            BendSpringStrengthChannelData = reader.ReadUInt32();
            EdgeCompressionChannelData = reader.ReadUInt32();
            PinningRadiusScale = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(ClothType);
            writer.WritePtr(OriginalVertsOwned);
            writer.WriteUInt32(OriginalVertsReferenced);
            writer.WriteUInt32(Skeleton);
            writer.WriteSingle(GravityScale);
            writer.WriteUInt32(Pad);
            writer.WriteUInt32(BendSpringLengthChannelData);
            writer.WriteUInt32(BendSpringStrengthChannelData);
            writer.WriteUInt32(EdgeCompressionChannelData);
            writer.WriteSingle(PinningRadiusScale);
        }

        public new void Read(MetaNodeReader reader) //todo
        {
            base.Read(reader);
            OriginalVertsOwned = new(reader.ReadNode<Rsc6VertexBuffer>("VerticesOwned"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNode("VerticesOwned", OriginalVertsOwned.Item);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class Rsc6ClothController : Rsc6FileBase, MetaNode //rage::clothController
    {
        public override ulong BlockLength => 136;
        public override uint VFT { get; set; } = 0x00F23C08;
        public Rsc6StreamableBase[] ClientsHead { get; set; } //m_ClientsHead
        public uint[] VBPlaceData { get; set; } //m_VBPlaceData[m_Count]
        public int[] VBIndex { get; set; } //m_VertexBufferIndex[m_Count]
        public int Count { get; set; } = 4; //m_Count, always 4 (LOD_COUNT)
        public Rsc6Ptr<Rsc6VertexBuffer> VBConst { get; set; } //m_VertexBufferConst
        public Rsc6Arr<float> UVBuffer { get; set; } //m_UVBuffer
        public Rsc6Ptr<Rsc6VerletCloth> Cloth { get; set; } //m_Cloth
        public uint DrawablePos { get; set; } //m_Drawable
        public Rsc6Ptr<Rsc6ClothBridgeSim> BridgeOwned { get; set; } //m_BridgeOwned
        public uint BridgeReferenced { get; set; } //m_BridgeReferenced, always 0
        public Rsc6Str Name { get; set; } //m_Name
        public Rsc6FragmentDrawable ReferencedDrawable { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ClientsHead = new Rsc6StreamableBase[4]; //LOD_COUNT
            VBPlaceData = new uint[4]; //LOD_COUNT
            VBIndex = new int[4]; //LOD_COUNT

            for (int i = 0; i < 4; i++)
            {
                ClientsHead[i] = new Rsc6StreamableBase();
                ClientsHead[i].Read(reader);
            }

            for (int i = 0; i < 4; i++)
            {
                VBPlaceData[i] = reader.ReadUInt32();
                VBIndex[i] = reader.ReadInt32();
            }

            Count = reader.ReadInt32();
            VBConst = reader.ReadPtr<Rsc6VertexBuffer>();
            UVBuffer = reader.ReadArr<float>();
            Cloth = reader.ReadPtr<Rsc6VerletCloth>();
            DrawablePos = reader.ReadUInt32();
            BridgeOwned = reader.ReadPtr<Rsc6ClothBridgeSim>();
            BridgeReferenced = reader.ReadUInt32();
            Name = reader.ReadStr();

            reader.BlockPool.TryGetValue(DrawablePos, out var val);
            if (val != null && val is Rsc6FragmentDrawable drawable)
            {
                ReferencedDrawable = drawable;
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            if (writer.BlockList.Contains(ReferencedDrawable))
            {
                ReferencedDrawable = (Rsc6FragmentDrawable)writer.BlockList.FirstOrDefault(s => s == ReferencedDrawable);
            }

            base.Write(writer);
            for (int i = 0; i < ClientsHead.Length; i++)
            {
                ClientsHead[i].Write(writer);
            }

            for (int i = 0; i < ClientsHead.Length; i++)
            {
                writer.WriteUInt32(VBPlaceData[i]);
                writer.WriteInt32(VBIndex[i]);
            }

            writer.WriteInt32(ClientsHead?.Length ?? 0);
            writer.WritePtr(VBConst);
            writer.WriteArr(UVBuffer);
            writer.WritePtr(Cloth);
            writer.WritePtrEmbed(ReferencedDrawable, ReferencedDrawable, 0);
            writer.WritePtr(BridgeOwned);
            writer.WriteUInt32(BridgeReferenced);
            writer.WriteStr(Name);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = new(reader.ReadString("Name"));
            VBPlaceData = reader.ReadUInt32Array("VBPlaceData");
            VBIndex = reader.ReadInt32Array("VBIndices");
            VBConst = new(reader.ReadNode<Rsc6VertexBuffer>("VBConst"));
            UVBuffer = new(reader.ReadSingleArray("UVBuffer"));
            Cloth = new(reader.ReadNode<Rsc6VerletCloth>("Cloth"));
            ReferencedDrawable = reader.ReadNode<Rsc6FragmentDrawable>("ReferencedDrawable");
            BridgeOwned = new(reader.ReadNode<Rsc6ClothBridgeSim>("BridgeOwned"));
            BridgeReferenced = reader.ReadUInt32("BridgeReferenced");

            ClientsHead = new Rsc6StreamableBase[VBPlaceData?.Length ?? 0];
            Count = VBPlaceData?.Length ?? 0;
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", Name.ToString());
            writer.WriteUInt32Array("VBPlaceData", VBPlaceData);
            writer.WriteInt32Array("VBIndices", VBIndex);
            writer.WriteNode("VBConst", VBConst.Item);
            writer.WriteSingleArray("UVBuffer", UVBuffer.Items);
            writer.WriteNode("Cloth", Cloth.Item);
            writer.WriteNode("ReferencedDrawable", ReferencedDrawable);
            writer.WriteNode("BridgeOwned", BridgeOwned.Item);
            writer.WriteUInt32("BridgeReferenced", BridgeReferenced);
        }
    }

    public class Rsc6VerletCloth : Rsc6BlockBase, MetaNode //rage::phVerletCloth
    {
        public override ulong BlockLength => 400;
        public Matrix4x4 CollisionBodyLastMatrix { get; set; } = Rpf6Crypto.GetMatrix4x4NaN(); //m_CollisionBodyLastMatrix
        public Vector4 CollisionBodyLastPosition { get; set; } = Rpf6Crypto.GetVec4NaN(); //m_CollisionBodyLastLastPosition
        public Vector4 BoundingCenterAndRadius { get; set; } = Rpf6Crypto.GetVec4NaN(); //m_BoundingCenterAndRadius, center is relative to cloth vertex 0
        public uint Unknown_60h { get; set; } = 0xCDCDCDCD; //Always 0xCDCDCDCD
        public uint HasUpdated { get; set; } //m_hasUpdated
        public Rsc6Arr<uint> DynamicPinList { get; set; } //m_DynamicPinList
        public Vector4 CollisionBodyPosition { get; set; } //m_CollisionBodyPosition
        public uint BodyInstance { get; set; } //m_BodyInstance
        public float FrictionMu { get; set; } //m_FrictionMu
        public float StaticCling { get; set; } //m_StaticCling
        public float FrictionConst { get; set; } //m_FrictionConst
        public int BodyPart { get; set; } = -1; //m_BodyPart, always -1
        public int NumPinnedVerts { get; set; } //m_NumPinnedVerts
        public Rsc6ClothData ClothData { get; set; } //m_ClothData
        public int NumEdges { get; set; } //m_NumEdges
        public int NumVertices { get; set; } //m_NumVertices
        public float Damping { get; set; } //m_Damping
        public float BodyMoveDamping { get; set; } //m_BodyMoveDamping
        public float BodyMoveAccelFactor { get; set; } //m_BodyMoveAccelFactor
        public float CompressionStiffnessScale { get; set; } //m_CompressionStiffnessScale
        public float CollisionMass { get; set; } //m_collisionMass
        public float GravityFactor { get; set; } //m_GravityFactor
        public float OverRelaxationConstant { get; set; } //m_OverRelaxationConstant
        public float BendSpringStiffness { get; set; } //m_BendSpringStiffness
        public float BendSpringLengthScale { get; set; } //m_BendSpringLengthScale
        public Rsc6Arr<float> VertexRadius { get; set; } //m_VertexRadius
        public Rsc6ManagedArr<Rsc6EdgeData> EdgeData { get; set; } //m_EdgeData
        public Rsc6ManagedArr<Rsc6EdgeData> BendSprings { get; set; } //m_BendSprings
        public Rsc6Arr<int> PartsList { get; set; } //m_partsList
        public Rsc6Arr<int> PartsVertLists { get; set; } //m_partsVertLists
        public Rsc6Arr<int> CollidableEdgesParts { get; set; } //m_CollidableEdgesParts
        public Rsc6Arr<int> CollidableEdges { get; set; } //m_CollidableEdges
        public Rsc6Arr<Matrix3x4> BoundPartMatrices { get; set; } //m_boundPart_prevTcurr
        public Rsc6Arr<int> BoundPartMap { get; set; } //m_boundPartMap
        public Rsc6Arr<int> VertexColliding { get; set; } //m_VertexColliding
        public uint Unknown_14Ch { get; set; } //Always 0
        public uint CurrentBoundPart { get; set; } = 0xCDCDCDCD; //m_currentBoundPart, always 0xCDCDCDCD
        public float PrevTimeStep { get; set; } //m_PrevTimeStep
        public ushort Iterations { get; set; } //m_nIterations, the number of verlet integration steps per update
        public ushort IterationsPerCollision { get; set; } //m_nIterationsPerCollision
        public ushort IterationsPerBendCalculation { get; set; } //m_nIterationsPerBendCalculation
        public bool UseBendSprings { get; set; } //m_UseBendSprings
        public bool UsePreIntegrationCollision { get; set; } //m_UsePreIntegrationCollision
        public int UseComplexPartsVertListFilter { get; set; } //m_UseComplexPartsVertListFilter
        public int NumBendSprings { get; set; } //m_NumBendSprings
        public Rsc6Arr<int> CollisionInst { get; set; } //m_CollisionInst, count=0, capacity=BlockSize, where all int's are preset to 0xCDCDCDCD
        public uint Unknown_174h { get; set; } //Always 0
        public uint Unknown_178h { get; set; } //Always 0
        public int ClothIndex { get; set; } //m_ClothIndex, always 0
        public ushort Unknown_180h { get; set; } //Always 0
        public uint ClothPartTypeFlags { get; set; } = 0xCDCDCDCD; //m_ClothPartTypeFlags, always 0xCDCDCDCD
        public bool AllowWind { get; set; } //m_AllowWind
        public bool IncreaseWind { get; set; } //m_IncreaseWind
        public bool IsRope { get; set; } //m_IsRope
        public bool CollisionDisabled { get; set; } //m_CollisionDisabled
        public bool IsBreakable { get; set; } //m_IsBreakable
        public bool WasBroken { get; set; } //m_WasBroken
        public float RopeRadius { get; set; } //m_RopeRadius, always 0
        public uint Unknown_18Ch { get; set; } = 0xCDCDCDCD; //Always 0xCDCDCDCD

        public override void Read(Rsc6DataReader reader)
        {
            CollisionBodyLastMatrix = reader.ReadMatrix4x4();
            CollisionBodyLastPosition = reader.ReadVector4();
            BoundingCenterAndRadius = reader.ReadVector4();
            Unknown_60h = reader.ReadUInt32();
            HasUpdated = reader.ReadUInt32();
            DynamicPinList = reader.ReadArr<uint>();
            CollisionBodyPosition = reader.ReadVector4();
            BodyInstance = reader.ReadUInt32();
            FrictionMu = reader.ReadSingle();
            StaticCling = reader.ReadSingle();
            FrictionConst = reader.ReadSingle();
            BodyPart = reader.ReadInt32();
            NumPinnedVerts = reader.ReadInt32();
            ClothData = reader.ReadBlock<Rsc6ClothData>();
            NumEdges = reader.ReadInt32();
            NumVertices = reader.ReadInt32();
            Damping = reader.ReadSingle();
            BodyMoveDamping = reader.ReadSingle();
            BodyMoveAccelFactor = reader.ReadSingle();
            CompressionStiffnessScale = reader.ReadSingle();
            CollisionMass = reader.ReadSingle();
            GravityFactor = reader.ReadSingle();
            OverRelaxationConstant = reader.ReadSingle();
            BendSpringStiffness = reader.ReadSingle();
            BendSpringLengthScale = reader.ReadSingle();
            VertexRadius = reader.ReadArr<float>();
            EdgeData = reader.ReadArr<Rsc6EdgeData>();
            BendSprings = reader.ReadArr<Rsc6EdgeData>();
            PartsList = reader.ReadArr<int>();
            PartsVertLists = reader.ReadArr<int>();
            CollidableEdgesParts = reader.ReadArr<int>();
            CollidableEdges = reader.ReadArr<int>();
            BoundPartMatrices = reader.ReadArr<Matrix3x4>();
            BoundPartMap = reader.ReadArr<int>();
            VertexColliding = reader.ReadArr<int>();
            Unknown_14Ch = reader.ReadUInt32();
            CurrentBoundPart = reader.ReadUInt32();
            PrevTimeStep = reader.ReadSingle();
            Iterations = reader.ReadUInt16();
            IterationsPerCollision = reader.ReadUInt16();
            IterationsPerBendCalculation = reader.ReadUInt16();
            UseBendSprings = reader.ReadBoolean();
            UsePreIntegrationCollision = reader.ReadBoolean();
            UseComplexPartsVertListFilter = reader.ReadInt32();
            NumBendSprings = reader.ReadInt32();
            CollisionInst = reader.ReadArr<int>();
            ClothIndex = reader.ReadInt32();
            Unknown_174h = reader.ReadUInt32();
            Unknown_178h = reader.ReadUInt32();
            ClothPartTypeFlags = reader.ReadUInt32();
            Unknown_180h = reader.ReadUInt16();
            AllowWind = reader.ReadBoolean();
            IncreaseWind = reader.ReadBoolean();
            IsRope = reader.ReadBoolean();
            CollisionDisabled = reader.ReadBoolean();
            IsBreakable = reader.ReadBoolean();
            WasBroken = reader.ReadBoolean();
            RopeRadius = reader.ReadSingle();
            Unknown_18Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteMatrix4x4(CollisionBodyLastMatrix);
            writer.WriteVector4(CollisionBodyLastPosition);
            writer.WriteVector4(BoundingCenterAndRadius);
            writer.WriteUInt32(Unknown_60h);
            writer.WriteUInt32(HasUpdated);
            writer.WriteArr(DynamicPinList);
            writer.WriteVector4(CollisionBodyPosition);
            writer.WriteUInt32(BodyInstance);
            writer.WriteSingle(FrictionMu);
            writer.WriteSingle(StaticCling);
            writer.WriteSingle(FrictionConst);
            writer.WriteInt32(BodyPart);
            writer.WriteInt32(NumPinnedVerts);
            writer.WriteBlock(ClothData);
            writer.WriteInt32(NumEdges);
            writer.WriteInt32(NumVertices);
            writer.WriteSingle(Damping);
            writer.WriteSingle(BodyMoveDamping);
            writer.WriteSingle(BodyMoveAccelFactor);
            writer.WriteSingle(CompressionStiffnessScale);
            writer.WriteSingle(CollisionMass);
            writer.WriteSingle(GravityFactor);
            writer.WriteSingle(OverRelaxationConstant);
            writer.WriteSingle(BendSpringStiffness);
            writer.WriteSingle(BendSpringLengthScale);
            writer.WriteArr(VertexRadius);
            writer.WriteArr(EdgeData);
            writer.WriteArr(BendSprings);
            writer.WriteArr(PartsList);
            writer.WriteArr(PartsVertLists);
            writer.WriteArr(CollidableEdgesParts);
            writer.WriteArr(CollidableEdges);
            writer.WriteArr(BoundPartMatrices);
            writer.WriteArr(BoundPartMap);
            writer.WriteArr(VertexColliding);
            writer.WriteUInt32(Unknown_14Ch);
            writer.WriteUInt32(CurrentBoundPart);
            writer.WriteSingle(PrevTimeStep);
            writer.WriteUInt16(Iterations);
            writer.WriteUInt16(IterationsPerCollision);
            writer.WriteUInt16(IterationsPerBendCalculation);
            writer.WriteBoolean(UseBendSprings);
            writer.WriteBoolean(UsePreIntegrationCollision);
            writer.WriteInt32(UseComplexPartsVertListFilter);
            writer.WriteInt32(NumBendSprings);
            writer.WriteArr(CollisionInst);
            writer.WriteInt32(ClothIndex);
            writer.WriteUInt32(Unknown_174h);
            writer.WriteUInt32(Unknown_178h);
            writer.WriteUInt32(ClothPartTypeFlags);
            writer.WriteUInt16(Unknown_180h);
            writer.WriteBoolean(AllowWind);
            writer.WriteBoolean(IncreaseWind);
            writer.WriteBoolean(IsRope);
            writer.WriteBoolean(CollisionDisabled);
            writer.WriteBoolean(IsBreakable);
            writer.WriteBoolean(WasBroken);
            writer.WriteSingle(RopeRadius);
            writer.WriteUInt32(Unknown_18Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            CollisionBodyLastMatrix = ToXYZ(reader.ReadMatrix4x4("CollisionBodyLastMatrix"), true);
            CollisionBodyLastPosition = ToXYZ(reader.ReadVector4("CollisionBodyLastPosition"));
            BoundingCenterAndRadius = ToXYZ(reader.ReadVector4("BoundingCenter"));
            HasUpdated = reader.ReadUInt32("HasUpdated");
            NumVertices = reader.ReadInt32("NumVertices");
            DynamicPinList = new(reader.ReadUInt32Array("DynamicPinList"), false, (uint)NumVertices);
            CollisionBodyPosition = ToXYZ(reader.ReadVector4("CollisionBodyPosition"));
            BodyInstance = reader.ReadUInt32("BodyInstance");
            FrictionMu = reader.ReadSingle("FrictionMu");
            StaticCling = reader.ReadSingle("StaticCling");
            FrictionConst = reader.ReadSingle("FrictionConst");
            ClothData = reader.ReadNode<Rsc6ClothData>("ClothData");
            Damping = reader.ReadSingle("Damping");
            BodyMoveDamping = reader.ReadSingle("BodyMoveDamping");
            BodyMoveAccelFactor = reader.ReadSingle("BodyMoveAccelFactor");
            CompressionStiffnessScale = reader.ReadSingle("CompressionStiffnessScale");
            CollisionMass = reader.ReadSingle("CollisionMass");
            GravityFactor = reader.ReadSingle("GravityFactor");
            OverRelaxationConstant = reader.ReadSingle("OverRelaxationConstant");
            BendSpringStiffness = reader.ReadSingle("BendSpringStiffness");
            BendSpringLengthScale = reader.ReadSingle("BendSpringLengthScale");
            VertexRadius = new(reader.ReadSingleArray("VertexRadius"));
            EdgeData = new(reader.ReadNodeArray<Rsc6EdgeData>("EdgeData"));
            BendSprings = new(reader.ReadNodeArray<Rsc6EdgeData>("BendSprings"));
            PartsList = new(reader.ReadInt32Array("PartsList"));
            PartsVertLists = new(reader.ReadInt32Array("PartsVertLists"));
            CollidableEdgesParts = new(reader.ReadInt32Array("CollidableEdgesParts"));
            CollidableEdges = new(reader.ReadInt32Array("CollidableEdges"));
            BoundPartMatrices = new(ToXYZ(reader.ReadMatrix3x4Array("BoundPartMatrices"), true));
            BoundPartMap = new(reader.ReadInt32Array("BoundPartMap"));
            VertexColliding = new(reader.ReadInt32Array("VertexColliding"), false, (uint)NumVertices);
            PrevTimeStep = reader.ReadSingle("PrevTimeStep");
            Iterations = reader.ReadUInt16("Iterations");
            IterationsPerCollision = reader.ReadUInt16("IterationsPerCollision");
            IterationsPerBendCalculation = reader.ReadUInt16("IterationsPerBendCalculation");
            UseBendSprings = reader.ReadBool("UseBendSprings");
            UsePreIntegrationCollision = reader.ReadBool("UsePreIntegrationCollision");
            UseComplexPartsVertListFilter = reader.ReadInt32("UseComplexPartsVertListFilter");
            NumBendSprings = reader.ReadInt32("NumBendSprings");
            CollisionInst = new(reader.ReadInt32Array("CollisionInst"));
            AllowWind = reader.ReadBool("AllowWind");
            IncreaseWind = reader.ReadBool("IncreaseWind");
            IsRope = reader.ReadBool("IsRope");
            CollisionDisabled = reader.ReadBool("CollisionDisabled");
            IsBreakable = reader.ReadBool("IsBreakable");
            WasBroken = reader.ReadBool("WasBroken");
            NumPinnedVerts = VertexRadius.Items?.Length ?? 0;
            NumEdges = EdgeData.Items?.Length ?? 0;
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteMatrix4x4("CollisionBodyLastMatrix", CollisionBodyLastMatrix);
            writer.WriteVector4("CollisionBodyLastPosition", CollisionBodyLastPosition);
            writer.WriteVector4("BoundingCenter", BoundingCenterAndRadius);
            writer.WriteUInt32("HasUpdated", HasUpdated);
            writer.WriteUInt32Array("DynamicPinList", DynamicPinList.Items);
            writer.WriteVector4("CollisionBodyPosition", CollisionBodyPosition);
            writer.WriteUInt32("BodyInstance", BodyInstance);
            writer.WriteSingle("FrictionMu", FrictionMu);
            writer.WriteSingle("StaticCling", StaticCling);
            writer.WriteSingle("FrictionConst", FrictionConst);
            writer.WriteNode("ClothData", ClothData);
            writer.WriteInt32("NumVertices", NumVertices);
            writer.WriteSingle("Damping", Damping);
            writer.WriteSingle("BodyMoveDamping", BodyMoveDamping);
            writer.WriteSingle("BodyMoveAccelFactor", BodyMoveAccelFactor);
            writer.WriteSingle("CompressionStiffnessScale", CompressionStiffnessScale);
            writer.WriteSingle("CollisionMass", CollisionMass);
            writer.WriteSingle("GravityFactor", GravityFactor);
            writer.WriteSingle("OverRelaxationConstant", OverRelaxationConstant);
            writer.WriteSingle("BendSpringStiffness", BendSpringStiffness);
            writer.WriteSingle("BendSpringLengthScale", BendSpringLengthScale);
            writer.WriteSingleArray("VertexRadius", VertexRadius.Items);
            writer.WriteNodeArray("EdgeData", EdgeData.Items);
            writer.WriteNodeArray("BendSprings", BendSprings.Items);
            writer.WriteInt32Array("PartsList", PartsList.Items);
            writer.WriteInt32Array("PartsVertLists", PartsVertLists.Items);
            writer.WriteInt32Array("CollidableEdgesParts", CollidableEdgesParts.Items);
            writer.WriteInt32Array("CollidableEdges", CollidableEdges.Items);
            writer.WriteMatrix3x4Array("BoundPartMatrices", BoundPartMatrices.Items);
            writer.WriteInt32Array("BoundPartMap", BoundPartMap.Items);
            writer.WriteInt32Array("VertexColliding", VertexColliding.Items);
            writer.WriteSingle("PrevTimeStep", PrevTimeStep);
            writer.WriteUInt16("Iterations", Iterations);
            writer.WriteUInt16("IterationsPerCollision", IterationsPerCollision);
            writer.WriteUInt16("IterationsPerBendCalculation", IterationsPerBendCalculation);
            writer.WriteBool("UseBendSprings", UseBendSprings);
            writer.WriteBool("UsePreIntegrationCollision", UsePreIntegrationCollision);
            writer.WriteInt32("UseComplexPartsVertListFilter", UseComplexPartsVertListFilter);
            writer.WriteInt32("NumBendSprings", NumBendSprings);
            writer.WriteInt32Array("CollisionInst", CollisionInst.Items);
            writer.WriteBool("AllowWind", AllowWind);
            writer.WriteBool("IncreaseWind", IncreaseWind);
            writer.WriteBool("IsRope", IsRope);
            writer.WriteBool("CollisionDisabled", CollisionDisabled);
            writer.WriteBool("IsBreakable", IsBreakable);
            writer.WriteBool("WasBroken", WasBroken);
        }
    }

    public class Rsc6EdgeData : Rsc6BlockBase, MetaNode //rage::phEdgeData
    {
        public override ulong BlockLength => 16;
        public ushort VertIndex1 { get; set; } //m_vertIndices[0], the index numbers of the two cloth vertices on the edge
        public ushort VertIndex2 { get; set; } //m_vertIndices[1], the index numbers of the two cloth vertices on the edge
        public float EdgeLength { get; set; } //m_EdgeLength2, the squared rest length of the edge
        public float Weight { get; set; } //m_Weight0, fraction of the edge motion, 0.0f (no influence), 0.5f (both vertices equally) or 1.0f (only this vertex moves)
        public float CompressionWeight { get; set; } //m_CompressionWeight, scale factor for the cloth reaction to compression

        public override void Read(Rsc6DataReader reader)
        {
            VertIndex1 = reader.ReadUInt16();
            VertIndex2 = reader.ReadUInt16();
            EdgeLength = reader.ReadSingle();
            Weight = reader.ReadSingle();
            CompressionWeight = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt16(VertIndex1);
            writer.WriteUInt16(VertIndex2);
            writer.WriteSingle(EdgeLength);
            writer.WriteSingle(Weight);
            writer.WriteSingle(CompressionWeight);
        }

        public void Read(MetaNodeReader reader)
        {
            VertIndex1 = reader.ReadUInt16("VertIndex1");
            VertIndex2 = reader.ReadUInt16("VertIndex2");
            EdgeLength = reader.ReadSingle("EdgeLength");
            Weight = reader.ReadSingle("Weight");
            CompressionWeight = reader.ReadSingle("CompressionWeight");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt16("VertIndex1", VertIndex1);
            writer.WriteUInt16("VertIndex2", VertIndex2);
            writer.WriteSingle("EdgeLength", EdgeLength);
            writer.WriteSingle("Weight", Weight);
            writer.WriteSingle("CompressionWeight", CompressionWeight);
        }

        public override string ToString()
        {
            return $"VertIndex1: {VertIndex1}, VertIndex2: {VertIndex2}";
        }
    }

    public class Rsc6ClothData : Rsc6BlockBase, MetaNode //rage::phClothData
    {
        /*
         * Env cloth : not used
         * Char cloth : used for initial normals
         * Rope : used for previous positions
         */

        public override ulong BlockLength => 56;
        public Rsc6Arr<IntA> EdgeToVertexIndices { get; set; } //m_EdgeToVertexIndices
        public Rsc6Arr<Vector4> VertexInitialPositions { get; set; } //m_VertexInitialPositions
        public Rsc6Arr<Vector4> VertexInitialNormals { get; set; } //m_VertexInitialNormals, needed only for character cloth
        public Rsc6Arr<int> RopeMeshData { get; set; } //m_RopeMeshData, always NULL
        public Rsc6Arr<Vector4> VertexPositions { get; set; } //m_VertexPositions, always NULL
        public Rsc6Arr<Vector4> VertexPrevPosition { get; set; } //m_VertexPrevPositions, always NULL
        public uint BodyPolygonCache { get; set; } //m_bodyPolygonCache, rage::phClothBodyPolygonData, always NULL
        public uint Unknown_34h { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            EdgeToVertexIndices = reader.ReadArr<IntA>();
            VertexInitialPositions = reader.ReadArr<Vector4>();
            VertexInitialNormals = reader.ReadArr<Vector4>();
            RopeMeshData = reader.ReadArr<int>();
            VertexPositions = reader.ReadArr<Vector4>();
            VertexPrevPosition = reader.ReadArr<Vector4>();
            BodyPolygonCache = reader.ReadUInt32();
            Unknown_34h = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(EdgeToVertexIndices);
            writer.WriteArr(VertexInitialPositions);
            writer.WriteArr(VertexInitialNormals);
            writer.WriteArr(RopeMeshData);
            writer.WriteArr(VertexPositions);
            writer.WriteArr(VertexPrevPosition);
            writer.WriteUInt32(BodyPolygonCache);
            writer.WriteUInt32(Unknown_34h);
        }

        public void Read(MetaNodeReader reader)
        {
            EdgeToVertexIndices = new(reader.ReadStructArray<IntA>("EdgeToVertexIndices"));
            VertexInitialPositions = new(reader.ReadVector4Array("VertexInitialPositions"));
            VertexInitialNormals = new(reader.ReadVector4Array("VertexInitialNormals"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteStructArray("EdgeToVertexIndices", EdgeToVertexIndices.Items);
            writer.WriteVector4Array("VertexInitialPositions", VertexInitialPositions.Items);
            writer.WriteVector4Array("VertexInitialNormals", VertexInitialNormals.Items);
        }
    }

    public class Rsc6ClothBridgeSim : Rsc6FileBase, MetaNode //rage::clothBridgeSimGfx
    {
        public override ulong BlockLength => 104;
        public override uint VFT { get; set; } = 0x00F22198;
        public Rsc6RawArr<float> SoftPinValues { get; set; } //m_SoftPinValues
        public Rsc6RawArr<float> PinRadius { get; set; } //m_PinRadius
        public Rsc6RawArr<float> PinRamp { get; set; } //m_PinRamp
        public Rsc6RawArr<float> MinAlongNormal { get; set; } //m_MinAlongNormal
        public Rsc6RawArr<float> MaxAlongNormal { get; set; } //m_MaxAlongNormal
        public Rsc6RawArr<int> ClothDisplayMap { get; set; } //m_ClothDisplayMap
        public Rsc6RawArr<int> ClothDisplayReverseMap { get; set; } //m_ClothDisplayReverseMap
        public Rsc6VertexDeclaration FVF { get; set; } //m_Fvf
        public Rsc6VertexDeclaration FVFConst { get; set; } //m_FvfConst
        public int LOD { get; set; } //m_LOD
        public uint ModelID { get; set; } //m_ModelIdx
        public uint GeometryID { get; set; } //m_GeometryIdx
        public uint NumPinnedVerts { get; set; } //m_NumPinnedVerts
        public uint MeshVerts { get; set; } //m_MeshVerts
        public uint BoundVerts { get; set; } //m_BoundVerts
        public int RefCount { get; set; } = 2; //m_ReferenceCount, always 2
        public bool UseSoftPinning { get; set; } //m_UseSoftPinning
        public byte[] Pad { get; set; } //m_Pad

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            SoftPinValues = reader.ReadRawArrPtr<float>();
            PinRadius = reader.ReadRawArrPtr<float>();
            PinRamp = reader.ReadRawArrPtr<float>();
            MinAlongNormal = reader.ReadRawArrPtr<float>();
            MaxAlongNormal = reader.ReadRawArrPtr<float>();
            ClothDisplayMap = reader.ReadRawArrPtr<int>();
            ClothDisplayReverseMap = reader.ReadRawArrPtr<int>();
            FVF = reader.ReadBlock<Rsc6VertexDeclaration>();
            FVFConst = reader.ReadBlock<Rsc6VertexDeclaration>();
            LOD = reader.ReadInt32();
            ModelID = reader.ReadUInt32();
            GeometryID = reader.ReadUInt32();
            NumPinnedVerts = reader.ReadUInt32();
            MeshVerts = reader.ReadUInt32();
            BoundVerts = reader.ReadUInt32();
            RefCount = reader.ReadInt32();
            UseSoftPinning = reader.ReadBoolean();
            Pad = reader.ReadBytes(11);

            SoftPinValues = reader.ReadRawArrItems(SoftPinValues, MeshVerts);
            PinRadius = reader.ReadRawArrItems(PinRadius, NumPinnedVerts);
            PinRamp = reader.ReadRawArrItems(PinRamp, MeshVerts + NumPinnedVerts);
            MinAlongNormal = reader.ReadRawArrItems(MinAlongNormal, MeshVerts);
            MaxAlongNormal = reader.ReadRawArrItems(MaxAlongNormal, MeshVerts);
            ClothDisplayMap = reader.ReadRawArrItems(ClothDisplayMap, MeshVerts);
            ClothDisplayReverseMap = reader.ReadRawArrItems(ClothDisplayReverseMap, BoundVerts);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteRawArr(SoftPinValues);
            writer.WriteRawArr(PinRadius);
            writer.WriteRawArr(PinRamp);
            writer.WriteRawArr(MinAlongNormal);
            writer.WriteRawArr(MaxAlongNormal);
            writer.WriteRawArr(ClothDisplayMap);
            writer.WriteRawArr(ClothDisplayReverseMap);
            writer.WriteBlock(FVF);
            writer.WriteBlock(FVFConst);
            writer.WriteInt32(LOD);
            writer.WriteUInt32(ModelID);
            writer.WriteUInt32(GeometryID);
            writer.WriteUInt32(NumPinnedVerts);
            writer.WriteUInt32(MeshVerts);
            writer.WriteUInt32(BoundVerts);
            writer.WriteInt32(RefCount);
            writer.WriteBoolean(UseSoftPinning);
            writer.WriteBytes(Pad);
        }

        public void Read(MetaNodeReader reader)
        {
            SoftPinValues = new(reader.ReadSingleArray("SoftPinValues"));
            PinRadius = new(reader.ReadSingleArray("PinRadius"));
            PinRamp = new(reader.ReadSingleArray("PinRamp"));
            MinAlongNormal = new(reader.ReadSingleArray("MinAlongNormal"));
            MaxAlongNormal = new(reader.ReadSingleArray("MaxAlongNormal"));
            ClothDisplayMap = new(reader.ReadInt32Array("ClothDisplayMap"));
            ClothDisplayReverseMap = new(reader.ReadInt32Array("ClothDisplayReverseMap"));
            FVF = reader.ReadNode<Rsc6VertexDeclaration>("FVF");
            FVFConst = reader.ReadNode<Rsc6VertexDeclaration>("FVFConst");
            LOD = reader.ReadInt32("LOD");
            ModelID = reader.ReadUInt32("ModelID");
            GeometryID = reader.ReadUInt32("GeometryID");
            UseSoftPinning = reader.ReadBool("UseSoftPinning");

            MeshVerts = (uint)SoftPinValues.Items?.Length;
            NumPinnedVerts = (uint)PinRadius.Items?.Length;
            BoundVerts = (uint)ClothDisplayReverseMap.Items?.Length;

            Pad = new byte[11];
            Array.Fill(Pad, (byte)0xCD);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteSingleArray("SoftPinValues", SoftPinValues.Items);
            writer.WriteSingleArray("PinRadius", PinRadius.Items);
            writer.WriteSingleArray("PinRamp", PinRamp.Items);
            writer.WriteSingleArray("MinAlongNormal", MinAlongNormal.Items);
            writer.WriteSingleArray("MaxAlongNormal", MaxAlongNormal.Items);
            writer.WriteInt32Array("ClothDisplayMap", ClothDisplayMap.Items);
            writer.WriteInt32Array("ClothDisplayReverseMap", ClothDisplayReverseMap.Items);
            writer.WriteNode("FVF", FVF);
            writer.WriteNode("FVFConst", FVFConst);
            writer.WriteInt32("LOD", LOD);
            writer.WriteUInt32("ModelID", ModelID);
            writer.WriteUInt32("GeometryID", GeometryID);
            writer.WriteBool("UseSoftPinning", UseSoftPinning);
        }
    }

    public class Rsc6VariableMeshArray : Rsc6BlockBase, MetaNode //VariableMeshArray
    {
        /*
         * Used by scripts for enabling various ped components.
         * Native function: ACTOR_ENABLE_VARIABLE_MESH(Actor actor, byte component, bool enable)
         * 
         * VMB2 and VMB3 are always the same.
         * Both are also equal to VMB1 most of the time.
         * VMB1 can be different for a couple of fragments (doors, horses and also important story models like John/Jack/Uncle) for specific mesh components
         */

        public override ulong BlockLength => 93; //31*3
        public Rsc6MeshBitComponentBase VariableMeshBits1 { get; set; } //m_VariableMeshBits1 (VMB1)
        public Rsc6MeshBitComponentBase VariableMeshBits2 { get; set; } //m_VariableMeshBits2 (VMB2)
        public Rsc6MeshBitComponentBase VariableMeshBits3 { get; set; } //m_VariableMeshBits3 (VMB3)

        public string[] UsedComponents => GetUsedComponents().Keys.ToArray();
        public byte[] UsedIndices => GetUsedComponents().Values.ToArray();
        public bool IsJohnJack { get; set; } //Whether or not this is a John's or Jack's fragment

        public Rsc6VariableMeshArray()
        {
        }

        public Rsc6VariableMeshArray(bool john)
        {
            IsJohnJack = john;
        }

        public override void Read(Rsc6DataReader reader)
        {
            IsJohnJack = reader.FileEntry.Name.StartsWith("default_");
            VariableMeshBits1 = ReadBlock(reader, IsJohnJack);
            VariableMeshBits2 = ReadBlock(reader, IsJohnJack);
            VariableMeshBits3 = ReadBlock(reader, IsJohnJack);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            VariableMeshBits1?.Write(writer);
            VariableMeshBits2?.Write(writer);
            VariableMeshBits3?.Write(writer);
        }

        public void Read(MetaNodeReader reader)
        {
            IsJohnJack = reader.ReadString("@type") == "John";
            VariableMeshBits1 = ReadNode(reader, "MeshBits1", IsJohnJack);
            VariableMeshBits2 = ReadNode(reader, "MeshBits2", IsJohnJack);
            VariableMeshBits2 ??= VariableMeshBits1;
            VariableMeshBits3 = VariableMeshBits2;
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", IsJohnJack ? "John" : "Misc");
            WriteNode(writer, VariableMeshBits1, "MeshBits1", IsJohnJack);
            WriteNode(writer, VariableMeshBits2, "MeshBits2", IsJohnJack);
        }

        private static Rsc6MeshBitComponentBase ReadBlock(Rsc6DataReader reader, bool isJohn)
        {
            if (isJohn)
                return reader.ReadBlock<Rsc6MeshBitComponentJohn>();
            else
                return reader.ReadBlock<Rsc6MeshBitComponentMisc>();
        }

        private static void WriteBlock(Rsc6DataWriter writer, Rsc6MeshBitComponentBase component)
        {
            if (component is Rsc6MeshBitComponentJohn johnComponent)
                writer.WriteBlock(johnComponent);
            else if (component is Rsc6MeshBitComponentMisc miscComponent)
                writer.WriteBlock(miscComponent);
        }

        private static Rsc6MeshBitComponentBase ReadNode(MetaNodeReader reader, string node, bool isJohn)
        {
            if (isJohn)
                return reader.ReadNode<Rsc6MeshBitComponentJohn>(node);
            else
                return reader.ReadNode<Rsc6MeshBitComponentMisc>(node);
        }

        private static void WriteNode(MetaNodeWriter writer, Rsc6MeshBitComponentBase component, string node, bool isJohn)
        {
            if (isJohn)
                writer.WriteNode(node, (Rsc6MeshBitComponentJohn)component);
            else
                writer.WriteNode(node, (Rsc6MeshBitComponentMisc)component);
        }

        private Dictionary<string, byte> GetUsedComponents()
        {
            var components = new Dictionary<string, byte>();
            var type = VariableMeshBits1.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var value = (byte)field.GetValue(VariableMeshBits1);

                if (value == 255 || (value == 0 && components.ContainsValue(0))) continue;
                components[field.Name] = value;
            }

            if (components.Count == 1 && components.Values.First() == 0)
            {
                return new Dictionary<string, byte>();
            }
            return components.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value); //Order by indices
        }
    }

    public abstract class Rsc6MeshBitComponentBase : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 31;
        public FieldInfo[] GetFields() => GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public virtual byte[] GetComponentValues()
        {
            var fields = GetFields();
            return fields.Where(field => field.FieldType == typeof(byte)).Select(field => (byte)field.GetValue(this)).ToArray();
        }

        public override void Read(Rsc6DataReader reader)
        {
            foreach (var field in GetFields())
            {
                field.SetValue(this, reader.ReadByte());
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            foreach (var field in GetFields())
            {
                writer.WriteByte((byte)field.GetValue(this));
            }
        }

        public void Read(MetaNodeReader reader)
        {
            foreach (var field in GetFields())
            {
                var value = reader.ReadByte(field.Name);
                field.SetValue(this, value);
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            foreach (var field in GetFields())
            {
                writer.WriteByte(field.Name, (byte)field.GetValue(this));
            }
        }
    }

    public class Rsc6MeshBitComponentJohn : Rsc6MeshBitComponentBase
    {
        public byte BANDOLIER_RIGHT_SHOULDER_SHELLS_1;
        public byte BANDOLIER_RIGHT_SHOULDER_SHELLS_2;
        public byte BANDOLIER_RIGHT_SHOULDER_SHELLS_3;
        public byte BANDOLIER_RIGHT_SHOULDER_SHELLS_4;
        public byte BANDOLIER_RIGHT_SHOULDER_SHELLS_5;
        public byte BANDOLIER_RIGHT_SHOULDER_SHELLS_6;
        public byte BANDOLIER_RIGHT_SHOULDER_SHELLS_7;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_1;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_2;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_3;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_4;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_5;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_6;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_7;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_8;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_9;
        public byte BANDOLIER_LEFT_SHOULDER_SHELLS_10;
        public byte GUNBELT_SHELLS_1;
        public byte GUNBELT_SHELLS_2;
        public byte GUNBELT_SHELLS_3;
        public byte GUNBELT_SHELLS_4;
        public byte GUNBELT_SHELLS_5;
        public byte COMPONENT_22;
        public byte BANDOLIER_RIGHT;
        public byte BANDOLIER_LEFT;
        public byte GUN_HOLSTER;
        public byte BANDANA;
        public byte LEFT_ARM_GLOVE;
        public byte RIGHT_ARM_GLOVE;
        public byte LEFT_ARM;
        public byte RIGHT_ARM;
    }

    public class Rsc6MeshBitComponentMisc : Rsc6MeshBitComponentBase
    {
        public byte BANDANA; //Gangs, posse, misc
        public byte RIGHT_ARM_GLOVE; //Misc peds
        public byte LEFT_ARM_GLOVE; //Misc peds
        public byte RIGHT_ARM; //Misc peds
        public byte LEFT_ARM; //Misc peds
        public byte HIP_GUN_HOLTER; //Misc peds
        public byte COMPONENT_6;
        public byte MISC_ATTACHMENT_1; //Ammo belt, bandolier
        public byte COMPONENT_8;
        public byte COMPONENT_9;
        public byte COMPONENT_10;
        public byte COMPONENT_11;
        public byte MISC_ATTACHMENT_2; //Sword holder
        public byte COMPONENT_13;
        public byte MISC_ATTACHMENT_3; //Sword
        public byte COMPONENT_15;
        public byte COMPONENT_16;
        public byte RIGHT_ARM_NO_BLOOD; //Native friend, Moses
        public byte RIGHT_ARM_BLOOD; //Native friend, Moses
        public byte GUN_HOLSTER; //Misc peds
        public byte COMPONENT_20;
        public byte COMPONENT_21;
        public byte COMPONENT_22;
        public byte HEAD; //Mostly corpse, dead, undead & gore variations
        public byte BONNETS; //Mostly corpse, dead, undead & gore variations
        public byte HAIR; //Mostly corpse, dead, undead & gore variations
        public byte COMPONENT_26;
        public byte COMPONENT_27;
        public byte COMPONENT_28;
        public byte EXPLODED_HEAD_1; //Mostly corpse, dead, undead & gore variations
        public byte EXPLODED_HEAD_2; //Mostly corpse, dead, undead & gore variations
    }

    public enum Rsc6ArchetypeType : int
    {
        ARCHETYPE = 0,
        ARCHETYPE_PHYS = 1,
        ARCHETYPE_DAMP = 2
    }

    public enum Rsc6EventDirection : int
    {
        DIR_FORWARD,
        DIR_PAUSE,
        DIR_BACKWARD
    };

    public enum Rsc6MotionDampingType : int //Types of motion damping: constant, proportional to velocity or proportional to velocity squared
    {
        LINEAR_C,
        LINEAR_V,
        LINEAR_V2,
        ANGULAR_C,
        ANGULAR_V,
        ANGULAR_V2
    }

    [Flags] public enum Rsc6EventInstanceFlags : short
    {
        PLAY_FORWARD = 0,
        PLAY_BACKWARD = 1,
        PLAY_BLENDING_IN = 2,
        PLAY_BLENDING_OUT = 3
    }

    [Flags] public enum Rsc6FragTypeFlags : ushort
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

    [Flags] public enum Rsc6ObjectTypeFlags : uint
    {
        /*DEFAULT = 1, //Always set
        BROKEN = 2,
        MOVER = 4,
        BURNABLE = 128,
        GLASS_TYPE = 1024,
        COACH = 32768,
        UNKNOWN_19 = 524588, //Always set except for peds
        UNKNOWN_20 = 1048576, //Always set except for peds
        UNKNOWN_21 = 2097152,
        UNKNOWN_24 = 16777216,
        UNKNOWN_29 = 536870912, //Only used in p_gen_moneybag03x & p_gen_cavein02x
        COACH2 = 1073741824*/

        DEFAULT = 1, //Peds, all p_gen_ropesm's, p_gen_blood03x, p_blk_cityhall_clock01x
        CARS1 = 18350087, //armoredcar01x, flatcar01x
        CARS2 = 18350215, //northboxcar01x
        CARTS = 1075314695, //cart001x
        PROP_STANDARD = 1572871, //p_gen_cart01x, p_gen_barrel01x, p_gen_chair05x, p_gen_chaircomfy01x, p_gen_debrisboard02x, p_gen_coffin02x, car01x etc...
        PROP_STANDARD2 = 3670021,
        SEPERATED_PROP = 3670149, //p_gen_boxcar0101x
        STATIC_STANDARD = 1572869, //All p_gen_doorstandard's, p_gen_basin01x, p_gen_cranedock01x, p_gen_chairtheater01x, most debris_rockclusters
        STATIC_STANDARD2 = 1572868, //All p_gen_doorstandard's, p_gen_basin01x, p_gen_cranedock01x, p_gen_chairtheater01x, most debris_rockclusters
        WATERTROUGH = 20447237, //p_gen_watertrough01x
        WEAPONS = 1572865, //revolver_lemat01x
        WINDOW = 3671047,
        MISC1 = 1572999, //Most trees, p_gen_streetclock01x
        MISC2 = 1572997, //st_whitepine01x
        MISC3 = 3670023 //p_gen_milkcan02x, p_gen_trunk01x
    }

    [Flags] public enum Rsc6FragTypeGroupFlag : byte
    {
        DISAPPEARS_WHEN_DEAD = 1 << 0, //When health reaches zero, this group disappears
        MADE_OF_GLASS = 1 << 1, //This group is made out of glass and will shatter when broken
        DAMAGE_WHEN_BROKEN = 1 << 2, //When this group breaks off its parent, it will become damaged
        DOESNT_AFFECT_VEHICLES = 1 << 3, //When colliding with vehicles, the vehicle is treated as infinitely massive
        DOESNT_PUSH_VEHICLES_DOWN = 1 << 4,
        HAS_CLOTH = 1 << 5, //This group has the cloth (can't have more than one cloth per fragment)
    }

    [Flags] public enum Rsc6ExcludeFlag : byte
    {
        EXCLUDE_VEHICLE = 0,
        EXCLUDE_MOUNT,
        EXCLUDE_INVENTORY_ITEMS,
        EXCLUDE_RIDER_IF_CREATURE,
        EXCLUDE_MOVER_INSTS,
        EXCLUDE_GAME_CAMERA_EXCLUSION_LISTS_INSTS,
        HOGTIE_VICTIM_ON_SHOULDER,
        EXCLUDE_VEHICLE_RIDERS,
        EXCLUDE_VEHICLE_DRAFTS,
        EXCLUDE_DUEL_HOSTAGE
    }

    [Flags] public enum Rsc6CharacterClothFlags
    {
        IS_FORCE_SKIN = 1 << 0,
        IS_FALLING = 1 << 1,
        IS_SKYDIVING = 1 << 2,
        IS_PRONE = 1 << 3,
        IS_PRONE_FLIPPED = 1 << 4,
        IS_QUEUED_PINNING = 1 << 5,
        IS_QUEUED_POSE = 1 << 6,
        IS_END_CUTSCENE = 1 << 7
    };
}