using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CodeX.Games.RDR1.RSC6
{
    class Rsc6Fragment : Rsc6Block
    {
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
        public Rsc6Ptr<Rsc6FragDrawable> Drawable { get; set; } //m_ExtraDrawables
        public uint Unk0 { get; set; }
        public uint Unk1 { get; set; }
        public int UnkInt { get; set; }
        public int UnkInt2 { get; set; }
        public uint DamagedDrawable { get; set; } //m_DamagedDrawable, rage::fragTypeChild
        public Rsc6RawLst<Rsc6String> GroupNames { get; set; } //m_GroupNames
        public Rsc6RawPtrArr<Rsc6FragPhysGroup> Groups { get; set; } //m_Groups, rage::fragTypeGroup
        public Rsc6RawPtrArr<Rsc6FragPhysChild> Children { get; set; } //m_Children, rage::fragTypeChild
        public uint UnkArray { get; set; } //m_EnvCloth, rage::fragTypeEnvCloth
        public ushort UnkArrayCount { get; set; } //m_Count
        public ushort UnkArraySize { get; set; } //m_Capacity
        public uint CharCloth { get; set; } //m_CharCloth, rage::fragTypeCharCloth
        public Rsc6Ptr<Rsc6FragArchetypeDamp> Archetype1 { get; set; } //rage::phArchetypeDamp
        public Rsc6Ptr<Rsc6FragArchetypeDamp> Archetype2 { get; set; } //rage::phArchetypeDamp
        public Rsc6Ptr<Rsc6Bounds> Bound { get; set; } //m_CompositeBounds
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
        public ushort Flags { get; set; } //m_Flags
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
        public Rsc6PtrToPtrArr<Rsc6AssociationInfo> AssociatedFragments { get; set; } //m_AssociatedFragments, points to an array of pointers of 'AssociationInfo'
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
            BoundingSphere = reader.ReadVector4(true);
            RootCGOffset = reader.ReadVector4(true);
            OriginalRootCGOffset = reader.ReadVector4(true);
            UnbrokenCGOffset = reader.ReadVector4(true);
            DampingLinearC = reader.ReadVector4(true);
            DampingLinearV = reader.ReadVector4(true);
            DampingLinearV2 = reader.ReadVector4(true);
            DampingAngularC = reader.ReadVector4(true);
            DampingAngularV = reader.ReadVector4(true);
            DampingAngularV2 = reader.ReadVector4(true);
            NameRef = reader.ReadStr();
            Drawable = reader.ReadPtr<Rsc6FragDrawable>();
            Unk0 = reader.ReadUInt32();
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
            Bound = reader.ReadPtr(Rsc6Bounds.Create);
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
            Flags = reader.ReadUInt16();
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
            UndamagedAngInertia = reader.ReadRawArrItems(UndamagedAngInertia, ChildCount);
            DamagedAngInertia = reader.ReadRawArrItems(DamagedAngInertia, ChildCount);
            LinkAttachments = reader.ReadRawArrItems(LinkAttachments, ChildCount);
            SelfCollisionA = reader.ReadRawArrItems(SelfCollisionA, NumSelfCollisions);
            SelfCollisionB = reader.ReadRawArrItems(SelfCollisionB, NumSelfCollisions);

            //Name = NameRef.Value;
            //BuildPiece();
        }

        public void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            if (Drawable.Item == null)
            {
                return;
            }

            Xml.SelfClosingTag(sb, indent, "BoundingSphereCenter " + FloatUtil.GetVector3XmlString(Vector3.Subtract(Drawable.Item.BoundingSphere.Center, Drawable.Item.BoundingCenter)));
            Xml.ValueTag(sb, indent, "BoundingSphereRadius", FloatUtil.ToString(Drawable.Item.BoundingSphereRadius));
            Xml.ValueTag(sb, indent, "UnknownB0", "0");
            Xml.ValueTag(sb, indent, "UnknownB8", "0");
            Xml.ValueTag(sb, indent, "UnknownBC", "0");
            Xml.ValueTag(sb, indent, "UnknownC0", "65280");
            Xml.ValueTag(sb, indent, "UnknownC4", "1");
            Xml.ValueTag(sb, indent, "UnknownCC", "0");
            Xml.ValueTag(sb, indent, "GravityFactor", "1");
            Xml.ValueTag(sb, indent, "BuoyancyFactor", "1");

            float[] matrices = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            if (Drawable.Item != null)
            {
                Xml.OpenTag(sb, indent, "Drawable", true, "");
                 //Xml.StringTag(sb, indent + 1, "Name", Name, "");
                  Xml.WriteRawArray(sb, indent + 1, "Matrix", matrices, null, 3);
                 Drawable.Item.WriteXml(sb, indent + 1, ddsfolder);
                Xml.CloseTag(sb, indent, "Drawable", true);
            }
        }

        public void BuildPiece()
        {
            var d = Drawable.Item;
            var b = Bound.Item;
            var bbmin = Vector3.Zero;
            var bbmax = Vector3.Zero;
            var loddist0 = 250.0f;
            var loddist1 = 500.0f;
            var loddist2 = 1000.0f;
            var loddist3 = 9999.0f;
            var lodlists = new List<Model>[4];

            for (int l = 0; l < 4; l++)
            {
                lodlists[l] = new List<Model>();
            }

            if (d != null)
            {
                if (d.Lods != null)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        var lodlist = lodlists[l];
                        var dlod = d.Lods[l];

                        if (dlod == null)
                            continue;

                        lodlist.AddRange(dlod.Models);
                    }
                }
                bbmin = d.BoundingBoxMin;
                bbmax = d.BoundingBoxMax;
                loddist0 = Math.Max(d.LodDistHigh, 250.0f);
                loddist1 = Math.Max(d.LodDistMed, 500.0f);
                loddist2 = Math.Max(d.LodDistLow, 1000.0f);
                loddist3 = Math.Max(d.LodDistVlow, 9999.0f);
            }

            if (b != null)
            {
                bbmin = b.BoxMin.XYZ();
                bbmax = b.BoxMax.XYZ();
            }


            if (Children.Items != null)
            {
                var sg = d?.ShaderGroup ?? new Rsc6Ptr<Rsc6ShaderGroup>();
                for (int i = 0; i < Children.Items.Length; i++)
                {
                    var child = Children[i];
                    if (child == null) continue;

                    for (int l = 0; l < 4; l++)
                    {
                        var lodlist = lodlists[l];
                        var d1 = child.Drawable.Item;

                        if (d1?.Lods != null)
                        {
                            d1.ShaderGroup = sg;
                            d1.AssignShaders();

                            var dlod = d1.Lods[l];
                            if (dlod == null) continue;

                            lodlist.AddRange(dlod.Models);
                        }
                    }
                }
            }

            bbmin = new Vector3(bbmin.Z, bbmin.X, bbmin.Y);
            bbmax = new Vector3(bbmax.Z, bbmax.X, bbmax.Y);

            /*Lods = new PieceLod[4];
            Lods[0] = new PieceLod() { LodDist = loddist0, Models = lodlists[0].ToArray() };
            Lods[1] = new PieceLod() { LodDist = loddist1, Models = lodlists[1].ToArray() };
            Lods[2] = new PieceLod() { LodDist = loddist2, Models = lodlists[2].ToArray() };
            Lods[3] = new PieceLod() { LodDist = loddist3, Models = lodlists[3].ToArray() };
            Skeleton = d.Skeleton;
            BoundingSphere = new BoundingSphere((bbmin + bbmax) * 0.5f, (bbmax - bbmin).Length() * 0.5f);
            BoundingBox = new BoundingBox(bbmin, bbmax);
            Collider = b;
            UpdateAllModels();*/
        }
    }

    public class Rsc6FragDrawable : Rsc6DrawableBase
    {
        //TODO: determine what's different here
    }

    public class Rsc6FragArchetypeDamp : Rsc6FragArchetypePhys //rage::phArchetypeDamp
    {
        public override ulong BlockLength => base.BlockLength + 100;
        public Vector4[] DampingConstants { get; set; } //m_DampingConstant

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            DampingConstants = reader.ReadVector4Arr(6, true);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public override string ToString()
        {
            return NameRef.Value;
        }
    }

    public class Rsc6FragArchetypePhys : Rsc6FragArchetype //rage::phArchetypePhys
    {
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
            AngleInertia = reader.ReadVector4(true);
            InverseAngleInertia = reader.ReadVector4(true);
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
    }

    public class Rsc6FragArchetype : Rsc6FileBase //rage::phArchetype
    {
        public override ulong BlockLength => 28;
        public int Type { get; set; } //m_Type
        public Rsc6Str NameRef { get; set; } //m_Filename
        public Rsc6Ptr<Rsc6Bounds> Bounds { get; set; } //m_Bound
        public uint TypeFlags { get; set; } //m_TypeFlags
        public int IncludeFlags { get; set; } //m_IncludeFlags
        public ushort PropertyFlags { get; set; } //m_PropertyFlags
        public ushort RefCount { get; set; } //m_RefCount

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Type = reader.ReadInt32();
            NameRef = reader.ReadStr();
            Bounds = reader.ReadPtr(Rsc6Bounds.Create);
            TypeFlags = reader.ReadUInt32();
            IncludeFlags = reader.ReadInt32();
            PropertyFlags = reader.ReadUInt16();
            RefCount = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x00E6AEE8);
        }
    }

    public class Rsc6FragPhysGroup : Rsc6BlockBase
    {
        public override ulong BlockLength => throw new NotImplementedException();

        public override void Read(Rsc6DataReader reader)
        {
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6FragPhysChild : Rsc6BlockBase
    {
        public override ulong BlockLength => 156;
        public uint VFT { get; set; }
        public byte[] Pad { get; set; } //[140] (juicy stuff)
        public Rsc6Ptr<Rsc6FragDrawable> Drawable { get; set; }
        public Rsc6Ptr<Rsc6FragDrawable> Drawable2 { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Pad = reader.ReadArray<byte>(140);
            Drawable = reader.ReadPtr<Rsc6FragDrawable>();
            Drawable2 = reader.ReadPtr<Rsc6FragDrawable>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6AssociationInfo : Rsc6Block
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
    }
}