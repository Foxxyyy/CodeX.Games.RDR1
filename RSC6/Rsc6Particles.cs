using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6ParticleEffects : Rsc6BlockBaseMap, MetaNode //rage::ptxFxList
    {
        public override ulong BlockLength => 40;
        public override uint VFT { get; set; } = 0x0096A764;
        public Rsc6Ptr<Rsc6TextureDictionary> TexturesDict { get; set; } //m_Textures
        public Rsc6Ptr<Rsc6BlockMap> TextureListPusher { get; set; } //m_TextureListPusher, NULL
        public Rsc6Ptr<Rsc6Drawable> Models { get; set; } //m_Models, NULL
        public Rsc6Ptr<Rsc6PtxRuleDictionary> PtxRules { get; set; } //m_PtxRules
        public Rsc6Ptr<Rsc6BlockMap> TextureListPopper { get; set; } //m_TextureListPopper, NULL
        public Rsc6Ptr<Rsc6PtxEmitRuleDictionary> EmitRules { get; set; } //m_EmitRules
        public Rsc6Ptr<Rsc6PtxEffectRuleDictionary> EffectRules { get; set; } //m_EffectRules (rage::ptxEffectRule)
        public int UseCount { get; set; } //m_UseCount

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TexturesDict = reader.ReadPtr<Rsc6TextureDictionary>();
            TextureListPusher = reader.ReadPtr<Rsc6BlockMap>();
            Models = reader.ReadPtr<Rsc6Drawable>();
            PtxRules = reader.ReadPtr<Rsc6PtxRuleDictionary>();
            TextureListPopper = reader.ReadPtr<Rsc6BlockMap>();
            EmitRules = reader.ReadPtr<Rsc6PtxEmitRuleDictionary>();
            EffectRules = reader.ReadPtr<Rsc6PtxEffectRuleDictionary>();
            UseCount = reader.ReadInt32();

            Models.Item?.ApplyTextures(TexturesDict.Item);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(TexturesDict);
            writer.WritePtr(TextureListPusher);
            writer.WritePtr(Models);
            writer.WritePtr(PtxRules);
            writer.WritePtr(TextureListPopper);
            writer.WritePtr(EmitRules);
            writer.WritePtr(EffectRules);
            writer.WriteInt32(UseCount);
        }

        public void Read(MetaNodeReader reader)
        {
            TexturesDict = new(reader.ReadNode<Rsc6TextureDictionary>("TextureDictionary"));
            PtxRules = new(reader.ReadNode<Rsc6PtxRuleDictionary>("ParticleRules"));
            EmitRules = new(reader.ReadNode<Rsc6PtxEmitRuleDictionary>("EmitterRules"));
            EffectRules = new(reader.ReadNode<Rsc6PtxEffectRuleDictionary>("EffectRules"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("TextureDictionary", TexturesDict.Item);
            writer.WriteNode("ParticleRules", PtxRules.Item);
            writer.WriteNode("EmitterRules", EmitRules.Item);
            writer.WriteNode("EffectRules", EffectRules.Item);
        }
    }

    public class Rsc6PtxRuleDictionary : Rsc6BlockBaseMap, MetaNode //rage::pgDictionary<rage::ptxRule>
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x0096A778;
        public int Unknown_8h { get; set; } //Always 0
        public int RefCount { get; set; } = 1; //m_RefCount
        public Rsc6Arr<JenkHash> Hashes { get; set; } //m_Codes
        public Rsc6PtrArr<Rsc6PtxRule> Entries { get; set; } //m_Entries

        public Dictionary<JenkHash, Rsc6PtxRule> Dict { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadInt32();
            RefCount = reader.ReadInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Entries = reader.ReadPtrArr<Rsc6PtxRule>();

            var hashes = Hashes.Items;
            var entries = Entries.Items;

            if (hashes != null && entries != null)
            {
                Dict = new Dictionary<JenkHash, Rsc6PtxRule>();
                for (int i = 0; i < hashes.Length; i++)
                {
                    Dict[hashes[i]] = entries[i];
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteInt32(Unknown_8h);
            writer.WriteInt32(RefCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Entries);
        }

        public void Read(MetaNodeReader reader)
        {
            Hashes = new(reader.ReadJenkHashArray("Hashes"));
            Entries = new(reader.ReadNodeArray<Rsc6PtxRule>("Entries"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteJenkHashArray("Hashes", Hashes.Items);
            writer.WriteNodeArray("Entries", Entries.Items);
        }
    }

    public class Rsc6PtxRule : Rsc6FileBase, MetaNode //rage::ptxRule
    {
        public override ulong BlockLength => 320;
        public override uint VFT { get; set; } = 0x009754A0;
        public uint RefCount { get; set; } //references
        public uint Unknown_8h { get; set; } //Always 0?
        public float FileVersion { get; set; } //m_FileVersion
        public Vector4 ColorScalar { get; set; } //m_ColorScalar, always NaN
        public Rsc6PtxTriggerEvent SpawnEffectA { get; set; } //m_SpawnEffectA
        public Rsc6PtxTriggerEvent SpawnEffectB { get; set; } //m_SpawnEffectB
        public Rsc6PtxRenderState RenderState { get; set; } //m_RenderState
        public float PhysicalRange { get; set; } //m_PhysicalRange
        public float StopVel { get; set; } //m_StopVel
        public uint Flags1 { get; set; } //m_Flags (atFixedBitSet)
        public uint Flags2 { get; set; } //m_Flags (atFixedBitSet)
        public Rsc6Str Name { get; set; } //m_Name
        public string String { get; set; }
        public short Unknown_133h { get; set; } = 254; //Prev string-padding
        public byte PercentPhysical { get; set; } //m_PercentPhysical
        public byte PercentKill { get; set; } //m_PercentKill
        public uint LastEvoList { get; set; } //m_LastEvoList
        public uint Unknown_138h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_13Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            RefCount = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt32();
            FileVersion = reader.ReadSingle();
            ColorScalar = reader.ReadVector4();
            SpawnEffectA = reader.ReadBlock<Rsc6PtxTriggerEvent>();
            SpawnEffectB = reader.ReadBlock<Rsc6PtxTriggerEvent>();
            RenderState = reader.ReadBlock<Rsc6PtxRenderState>();
            PhysicalRange = reader.ReadSingle();
            StopVel = reader.ReadSingle();
            Flags1 = reader.ReadUInt32();
            Flags2 = reader.ReadUInt32();
            Name = reader.ReadStr();
            String = reader.ReadStringNullTerminated();
            Unknown_133h = reader.ReadInt16();
            PercentPhysical = reader.ReadByte();
            PercentKill = reader.ReadByte();
            LastEvoList = reader.ReadUInt32();
            Unknown_138h = reader.ReadUInt32();
            Unknown_13Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(RefCount);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteSingle(FileVersion);
            writer.WriteVector4(ColorScalar);
            writer.WriteBlock(SpawnEffectA);
            writer.WriteBlock(SpawnEffectB);
            writer.WriteBlock(RenderState);
            writer.WriteSingle(PhysicalRange);
            writer.WriteSingle(StopVel);
            writer.WriteUInt32(Flags1);
            writer.WriteUInt32(Flags2);
            writer.WriteStr(Name);
            writer.WriteStringNullTerminated(String);
            writer.WriteInt16(Unknown_133h);
            writer.WriteByte(PercentPhysical);
            writer.WriteByte(PercentKill);
            writer.WriteUInt32(LastEvoList);
            writer.WriteUInt32(Unknown_138h);
            writer.WriteUInt32(Unknown_13Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            FileVersion = reader.ReadSingle("FileVersion");
            PhysicalRange = reader.ReadSingle("PhysicalRange");
            StopVel = reader.ReadSingle("StopVel");
            Flags1 = reader.ReadUInt32("Flags1");
            Flags2 = reader.ReadUInt32("Flags2");
            Name = new(reader.ReadString("Name"));
            String = reader.ReadString("String");
            PercentPhysical = reader.ReadByte("PercentPhysical");
            PercentKill = reader.ReadByte("PercentKill");
            LastEvoList = reader.ReadUInt32("LastEvoList");
            SpawnEffectA = reader.ReadNode<Rsc6PtxTriggerEvent>("SpawnEffectA");
            SpawnEffectB = reader.ReadNode<Rsc6PtxTriggerEvent>("SpawnEffectB");
            RenderState = reader.ReadNode<Rsc6PtxRenderState>("RenderState");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteSingle("FileVersion", FileVersion);
            writer.WriteSingle("PhysicalRange", PhysicalRange);
            writer.WriteSingle("StopVel", StopVel);
            writer.WriteUInt32("Flags1", Flags1);
            writer.WriteUInt32("Flags2", Flags2);
            writer.WriteString("Name", Name.ToString());
            writer.WriteString("String", String);
            writer.WriteByte("PercentPhysical", PercentPhysical);
            writer.WriteByte("PercentKill", PercentKill);
            writer.WriteUInt32("LastEvoList", LastEvoList);
            writer.WriteNode("SpawnEffectA", SpawnEffectA);
            writer.WriteNode("SpawnEffectA", SpawnEffectB);
            writer.WriteNode("RenderState", RenderState);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class Rsc6PtxEmitRuleDictionary : Rsc6BlockBaseMap, MetaNode //rage::pgDictionary<rage::ptxEmitRule>
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x0096A778;
        public int Unknown_8h { get; set; } //Always 0
        public int RefCount { get; set; } = 1; //m_RefCount
        public Rsc6Arr<JenkHash> Hashes { get; set; } //m_Codes
        public Rsc6PtrArr<Rsc6PtxEmitRule> Entries { get; set; } //m_Entries

        public Dictionary<JenkHash, Rsc6PtxEmitRule> Dict { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadInt32();
            RefCount = reader.ReadInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Entries = reader.ReadPtrArr<Rsc6PtxEmitRule>();

            var hashes = Hashes.Items;
            var entries = Entries.Items;

            if (hashes != null && entries != null)
            {
                Dict = new Dictionary<JenkHash, Rsc6PtxEmitRule>();
                for (int i = 0; i < hashes.Length; i++)
                {
                    Dict[hashes[i]] = entries[i];
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteInt32(Unknown_8h);
            writer.WriteInt32(RefCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Entries);
        }

        public void Read(MetaNodeReader reader)
        {
            Hashes = new(reader.ReadJenkHashArray("Hashes"));
            Entries = new(reader.ReadNodeArray<Rsc6PtxEmitRule>("Entries"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteJenkHashArray("Hashes", Hashes.Items);
            writer.WriteNodeArray("Entries", Entries.Items);
        }
    }

    public class Rsc6PtxEmitRule : Rsc6BlockBaseMapRef, MetaNode //rage::ptxEmitRule
    {
        public override ulong BlockLength => 28;
        public override uint VFT { get; set; } = 0x009754A0;
        public Rsc6PtrArr<Rsc6PtxKeyFrameProperty> PropertyList { get; set; } //m_PropList
        public float Duration { get; set; } //m_Duration
        public uint ClassType { get; set; } //m_ClassType

        public string Name; //For editing purposes

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            PropertyList = reader.ReadPtrArr<Rsc6PtxKeyFrameProperty>();
            Duration = reader.ReadSingle();
            ClassType = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtrArr(PropertyList);
            writer.WriteSingle(Duration);
            writer.WriteUInt32(ClassType);
        }

        public void Read(MetaNodeReader reader)
        {
            Duration = reader.ReadSingle("Duration");
            ClassType = reader.ReadUInt32("ClassType");
            PropertyList = new(reader.ReadNodeArray<Rsc6PtxKeyFrameProperty>("Properties"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteSingle("Duration", Duration);
            writer.WriteUInt32("ClassType", ClassType);
            writer.WriteNodeArray("Properties", PropertyList.Items);
        }

        public override string ToString()
        {
            var count = (PropertyList.Items != null) ? PropertyList.Count : 0;
            return $"{count} {(count > 1 ? "properties" : "property")}, Duration: {Duration}";
        }
    }

    public class Rsc6PtxEffectRuleDictionary : Rsc6BlockBaseMap, MetaNode //rage::pgDictionary<rage::ptxEffectRule>
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x0096A7A0;
        public int Unknown_8h { get; set; } //Always 0
        public int RefCount { get; set; } = 1; //m_RefCount
        public Rsc6Arr<JenkHash> Hashes { get; set; } //m_Codes
        public Rsc6PtrArr<Rsc6PtxEffectRuleStd> Entries { get; set; } //m_Entries

        public Dictionary<JenkHash, Rsc6PtxEffectRuleStd> Dict { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadInt32();
            RefCount = reader.ReadInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Entries = reader.ReadPtrArr<Rsc6PtxEffectRuleStd>();

            var hashes = Hashes.Items;
            var entries = Entries.Items;

            if (hashes != null && entries != null)
            {
                Dict = new Dictionary<JenkHash, Rsc6PtxEffectRuleStd>();
                for (int i = 0; i < hashes.Length; i++)
                {
                    Dict[hashes[i]] = entries[i];
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteInt32(Unknown_8h);
            writer.WriteInt32(RefCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Entries);
        }

        public void Read(MetaNodeReader reader)
        {
            Hashes = new(reader.ReadJenkHashArray("Hashes"));
            Entries = new(reader.ReadNodeArray<Rsc6PtxEffectRuleStd>("Entries"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteJenkHashArray("Hashes", Hashes.Items);
            writer.WriteNodeArray("Entries", Entries.Items);
        }
    }

    public class Rsc6PtxEffectRuleStd : Rsc6PtxEffectRule, MetaNode //rage::ptxEffectRuleStd
    {
        public override ulong BlockLength => base.BlockLength + 128;
        public Rsc6PtxTimeLine TimeLine { get; set; } //m_TimeLine, the effect rule's timeline
        public float FadeDistance { get; set; } //m_FadeDistance
        public float CullRadius { get; set; } //m_CullRadius
        public float CullDistance { get; set; } //m_CullDistance
        public float LodNearDistance { get; set; } //m_LodNearDistance
        public float LodFarDistance { get; set; } //m_LodFarDistance
        public float FileVersion { get; set; } //m_FileVersion, the effect rule's file version
        public float DurationMin { get; set; } //m_DurationMin
        public float DurationMax { get; set; } //m_DurationMax
        public float TimeScalarMin { get; set; } //m_TimeScalarMin
        public float TimeScalarMax { get; set; } //m_TimeScalarMax
        public uint Unknown_26Ch { get; set; } //Always 0
        public uint Unknown_270h { get; set; } //Always 0
        public bool UseCullSphere { get; set; } //m_UseCullSphere
        public bool CullNoUpdate { get; set; } //m_CullNoUpdate
        public bool CullNoEmit { get; set; } //m_CullNoEmit
        public bool CullNoDraw { get; set; } //m_CullNoDraw
        public bool SortEvents { get; set; } //m_SortEvents
        public bool Quality { get; set; } //m_Quality
        public ushort Pad { get; set; } = 0xCDCD; //m_PadB
        public uint Unknown_27Ch { get; set; } //Always 0
        public Vector4 Unknown_280h { get; set; } = new Vector4(0.0f, 0.0f, 0.0f, Rpf6Crypto.FNaN); //0, 0.2, 0, 0
        public Vector4 Unknown_290h { get; set; } = new Vector4(0.0f, 0.0f, 0.0f, Rpf6Crypto.FNaN); //0, 2, 0, 0

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TimeLine = reader.ReadBlock<Rsc6PtxTimeLine>();
            FadeDistance = reader.ReadSingle();
            CullRadius = reader.ReadSingle();
            CullDistance = reader.ReadSingle();
            LodNearDistance = reader.ReadSingle();
            LodFarDistance = reader.ReadSingle();
            FileVersion = reader.ReadSingle();
            DurationMin = reader.ReadSingle();
            DurationMax = reader.ReadSingle();
            TimeScalarMin = reader.ReadSingle();
            TimeScalarMax = reader.ReadSingle();
            Unknown_26Ch = reader.ReadUInt32();
            Unknown_270h = reader.ReadUInt32();
            UseCullSphere = reader.ReadBoolean();
            CullNoUpdate = reader.ReadBoolean();
            CullNoEmit = reader.ReadBoolean();
            CullNoDraw = reader.ReadBoolean();
            SortEvents = reader.ReadBoolean();
            Quality = reader.ReadBoolean();
            Pad = reader.ReadUInt16();
            Unknown_27Ch = reader.ReadUInt32();
            Unknown_280h = reader.ReadVector4();
            Unknown_290h = reader.ReadVector4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class Rsc6PtxEffectRule : Rsc6BlockBaseMap, MetaNode //rage::ptxEffectRule
    {
        public override ulong BlockLength => 544;
        public override uint VFT { get; set; } = 0x009722F4;
        public Rsc6Str Name { get; set; } //m_Name
        public uint ClassType { get; set; } //m_ClassType, 1
        public Rsc6PtxKeyFrameProperty KFPColorTint { get; set; } //m_KFPColorTint
        public Rsc6PtxKeyFrameProperty KFPColorTintMax { get; set; } //m_KFPColorTintMax
        public Rsc6PtxKeyFrameProperty KFPZoom { get; set; } //m_KFPZoom
        public Rsc6PtxKeyFrameProperty KFPRotation { get; set; } //m_KFPRotation
        public Rsc6PtxKeyFrameProperty KFPDataSphere { get; set; } //m_KFPDataSphere
        public Rsc6PtxKeyFrameProperty KFPDataCapsuleA { get; set; } //m_KFPDataCapsuleA
        public uint FxList { get; set; } //m_FxList, NULL
        public uint PtxEvoList { get; set; } //m_PtxEvoList, NULL (rage::ptxEvolutionList)
        public float Zoom { get; set; } //m_FZoom, 100.0f
        public byte ZoomCullDist { get; set; } //m_ZoomCullDist, 0
        public bool UseRandomColor { get; set; } //m_UseRandomColor, FALSE
        public bool UseDefaultFunctors { get; set; } //m_UseDefaultFunctors, FALSE
        public bool InterfaceLoop { get; set; } //m_InterfaceLoop, FALSE
        public bool InterfaceAnimate { get; set; } //m_InterfaceAnimate, FALSE
        public bool HasDataSphere { get; set; } //m_HasDataSphere, FALSE
        public byte DataObjectType { get; set; } //m_DataObjectType, 0
        public byte GameFlags { get; set; } //m_GameFlags, 0
        public uint NumActiveInstances { get; set; } //m_NumActiveInstances, 0
        public Rsc6PtrArr<Rsc6PtxKeyFrameProperty> PropertyList { get; set; } //m_PropList, capacity set to 16x
        public uint Unknown_210h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_214h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_218h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_21Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadStr();
            ClassType = reader.ReadUInt32();
            KFPColorTint = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            KFPColorTintMax = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            KFPZoom = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            KFPRotation = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            KFPDataSphere = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            KFPDataCapsuleA = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            FxList = reader.ReadUInt32();
            PtxEvoList = reader.ReadUInt32();
            Zoom = reader.ReadSingle();
            ZoomCullDist = reader.ReadByte();
            UseRandomColor = reader.ReadBoolean();
            UseDefaultFunctors = reader.ReadBoolean();
            InterfaceLoop = reader.ReadBoolean();
            InterfaceAnimate = reader.ReadBoolean();
            HasDataSphere = reader.ReadBoolean();
            DataObjectType = reader.ReadByte();
            GameFlags = reader.ReadByte();
            NumActiveInstances = reader.ReadUInt32();
            PropertyList = reader.ReadPtrArr<Rsc6PtxKeyFrameProperty>();
            Unknown_210h = reader.ReadUInt32();
            Unknown_214h = reader.ReadUInt32();
            Unknown_218h = reader.ReadUInt32();
            Unknown_21Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteStr(Name);
            writer.WriteUInt32(ClassType);
            writer.WriteBlock(KFPColorTint);
            writer.WriteBlock(KFPColorTintMax);
            writer.WriteBlock(KFPZoom);
            writer.WriteBlock(KFPRotation);
            writer.WriteBlock(KFPDataSphere);
            writer.WriteBlock(KFPDataCapsuleA);
            writer.WriteUInt32(FxList);
            writer.WriteUInt32(PtxEvoList);
            writer.WriteSingle(Zoom);
            writer.WriteByte(ZoomCullDist);
            writer.WriteBoolean(UseRandomColor);
            writer.WriteBoolean(UseDefaultFunctors);
            writer.WriteBoolean(InterfaceLoop);
            writer.WriteBoolean(InterfaceAnimate);
            writer.WriteBoolean(HasDataSphere);
            writer.WriteByte(DataObjectType);
            writer.WriteByte(GameFlags);
            writer.WriteUInt32(NumActiveInstances);
            writer.WritePtrArr(PropertyList);
            writer.WriteUInt32(Unknown_210h);
            writer.WriteUInt32(Unknown_214h);
            writer.WriteUInt32(Unknown_218h);
            writer.WriteUInt32(Unknown_21Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = new(reader.ReadString("Name"));
            ClassType = reader.ReadUInt32("ClassType", 1);
            FxList = reader.ReadUInt32("FxList");
            PtxEvoList = reader.ReadUInt32("PtxEvoList");
            Zoom = reader.ReadSingle("Zoom");
            ZoomCullDist = reader.ReadByte("ZoomCullDist");
            UseRandomColor = reader.ReadBool("UseRandomColor");
            UseDefaultFunctors = reader.ReadBool("UseDefaultFunctors");
            InterfaceLoop = reader.ReadBool("InterfaceLoop");
            InterfaceAnimate = reader.ReadBool("InterfaceAnimate");
            HasDataSphere = reader.ReadBool("HasDataSphere");
            DataObjectType = reader.ReadByte("DataObjectType");
            GameFlags = reader.ReadByte("GameFlags");
            NumActiveInstances = reader.ReadUInt32("NumActiveInstances");
            KFPColorTint = reader.ReadNode<Rsc6PtxKeyFrameProperty>("KFPColorTint");
            KFPColorTintMax = reader.ReadNode<Rsc6PtxKeyFrameProperty>("KFPColorTintMax");
            KFPZoom = reader.ReadNode<Rsc6PtxKeyFrameProperty>("KFPZoom");
            KFPRotation = reader.ReadNode<Rsc6PtxKeyFrameProperty>("KFPRotation");
            KFPDataSphere = reader.ReadNode<Rsc6PtxKeyFrameProperty>("KFPDataSphere");
            KFPDataCapsuleA = reader.ReadNode<Rsc6PtxKeyFrameProperty>("KFPDataCapsuleA");

            var propList = reader.ReadNodeArray<Rsc6PtxKeyFrameProperty>("PropertyList");
            if (propList != null)
            {
                PropertyList = new(propList, 16, (ushort)propList.Length);
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", Name.ToString());
            writer.WriteUInt32("ClassType", ClassType);
            writer.WriteUInt32("FxList", FxList);
            writer.WriteUInt32("PtxEvoList", PtxEvoList);
            writer.WriteSingle("Zoom", Zoom);
            writer.WriteByte("ZoomCullDist", ZoomCullDist);
            writer.WriteBool("UseRandomColor", UseRandomColor);
            writer.WriteBool("UseDefaultFunctors", UseDefaultFunctors);
            writer.WriteBool("InterfaceLoop", InterfaceLoop);
            writer.WriteBool("InterfaceAnimate", InterfaceAnimate);
            writer.WriteBool("HasDataSphere", HasDataSphere);
            writer.WriteByte("DataObjectType", DataObjectType);
            writer.WriteByte("GameFlags", GameFlags);
            writer.WriteUInt32("NumActiveInstances", NumActiveInstances);
            writer.WriteNode("KFPColorTint", KFPColorTint);
            writer.WriteNode("KFPColorTintMax", KFPColorTintMax);
            writer.WriteNode("KFPZoom", KFPZoom);
            writer.WriteNode("KFPRotation", KFPRotation);
            writer.WriteNode("KFPDataSphere", KFPDataSphere);
            writer.WriteNode("KFPDataCapsuleA", KFPDataCapsuleA);
            writer.WriteNodeArray("PropertyList", PropertyList.Items);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class Rsc6PtxKeyFrameProperty : Rsc6BlockBaseMap, MetaNode //rage::ptxKeyframeProp
    {
        public override ulong BlockLength => 80;
        public override uint VFT { get; set; } = 0x00971394;
        public JenkHash PropertyID { get; set; } //m_PropHashID, the id of the property that is keyframed (hash of the class and variable name)
        public bool InvertBias { get; set; } //m_InvertBias, whether any bias linking on this property should be inverted
        public byte RandIndex { get; set; } //m_Rnd, an index into the random table that this property uses
        public ushort Unknown_Eh { get; set; } = 0xCDCD; //m_pad
        public Rsc6PtxKeyFrame KeyFrame { get; set; } //m_Keyframe, the keyframe data for this property

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            PropertyID = reader.ReadUInt32();
            InvertBias = reader.ReadBoolean();
            RandIndex = reader.ReadByte();
            Unknown_Eh = reader.ReadUInt16();
            KeyFrame = reader.ReadBlock<Rsc6PtxKeyFrame>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(PropertyID);
            writer.WriteBoolean(InvertBias);
            writer.WriteByte(RandIndex);
            writer.WriteUInt16(Unknown_Eh);
            writer.WriteBlock(KeyFrame);
        }

        public void Read(MetaNodeReader reader)
        {
            PropertyID = reader.ReadJenkHash("PropertyID");
            InvertBias = reader.ReadBool("InvertBias");
            RandIndex = reader.ReadByte("RandIndex");
            KeyFrame = reader.ReadNode<Rsc6PtxKeyFrame>("KeyFrame");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteJenkHash("PropertyID", PropertyID);
            writer.WriteBool("InvertBias", InvertBias);
            writer.WriteByte("RandIndex", RandIndex);
            writer.WriteNode("KeyFrame", KeyFrame);
        }

        public override string ToString()
        {
            return KeyFrame?.ToString();
        }
    }

    public class Rsc6PtxKeyFrame : Rsc6BlockBase, MetaNode //rage::rmPtfxKeyframe
    {
        public override ulong BlockLength => 64;
        public Rsc6PtxKeyFrameData InitialData { get; set; } //m_InitialData
        public Rsc6ManagedArr<Rsc6PtxKeyFrameData> KeyFrameData { get; set; } //m_KeyframeData
        public Rsc6Ptr<Rsc6BlockMap> KeyFrameWidget { get; set; } //m_KeyframeWidget, always NULL
        public Rsc6Ptr<Rsc6BlockMap> Info { get; set; } //m_Info, always NULL

        public override void Read(Rsc6DataReader reader)
        {
            InitialData = reader.ReadBlock<Rsc6PtxKeyFrameData>();
            KeyFrameData = reader.ReadArr<Rsc6PtxKeyFrameData>();
            KeyFrameWidget = reader.ReadPtr<Rsc6BlockMap>();
            Info = reader.ReadPtr<Rsc6BlockMap>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteBlock(InitialData);
            writer.WriteArr(KeyFrameData);
            writer.WritePtr(KeyFrameWidget);
            writer.WritePtr(Info);
        }

        public void Read(MetaNodeReader reader)
        {
            InitialData = reader.ReadNode<Rsc6PtxKeyFrameData>("InitialData");
            KeyFrameData = new(reader.ReadNodeArray<Rsc6PtxKeyFrameData>("KeyFrameData"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("InitialData", InitialData);
            if (KeyFrameData.Count > 0) writer.WriteNodeArray("KeyFrameData", KeyFrameData.Items);
        }

        public override string ToString()
        {
            return InitialData?.ToString();
        }
    }

    public class Rsc6PtxKeyFrameData : Rsc6BlockBase, MetaNode //rage::rmPtfxKeyframeDataN<4>
    {
        public override ulong BlockLength => 48;
        public Vector4 StartTime { get; set; } //m_StartTimeV, the time of the keyframe entry (ScalarVFromF32)
        public Vector4 Value { get; set; } //m_vValue, the value of the keyframe entry
        public Vector4 Delta { get; set; } //m_vDelta, the pre calculated delta value to minimise keyframe interpolation costs

        public override void Read(Rsc6DataReader reader)
        {
            StartTime = reader.ReadVector4();
            Value = reader.ReadVector4();
            Delta = reader.ReadVector4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(StartTime);
            writer.WriteVector4(Value);
            writer.WriteVector4(Delta);
        }

        public void Read(MetaNodeReader reader)
        {
            StartTime = new Vector4(reader.ReadSingle("KFTime"));
            Value = Rpf6Crypto.ToXYZ(reader.ReadVector4("KFValue"));
            Delta = Rpf6Crypto.ToXYZ(reader.ReadVector4("KFDelta"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteSingle("KFTime", StartTime.X);
            writer.WriteVector4("KFValue", Value);
            writer.WriteVector4("KFDelta", Delta);
        }

        public override string ToString()
        {
            return $"Value: {Value}";
        }
    }

    public class Rsc6PtxTriggerEvent : Rsc6BlockBase, MetaNode //rage::ptxTriggerEvent
    {
        public override ulong BlockLength => 112;
        public Rsc6PtxEffectOverridables OverrideMins { get; set; } //m_OverrideMins, min spawned effect scalar settings
        public Rsc6PtxEffectOverridables OverrideMaxes { get; set; } //m_OverrideMaxes, max spawned effect scalar settings
        public Rsc6Ptr<Rsc6PtxEffectRule> EffectRule { get; set; } //m_pEffectRule, the effect rule that gets spawned
        public uint Type { get; set; } //m_Type
        public float Time { get; set; } //m_Time
        public bool Toggle { get; set; } //m_Toggle1
        public byte Pad0 { get; set; } = 0xCD; //m_Pad[0]
        public byte Pad1 { get; set; } = 0xCD; //m_Pad[1]
        public byte Pad2 { get; set; } = 0xCD; //m_Pad[2]

        private string SpawnedEffectRuleName; //For editing purposes

        public override void Read(Rsc6DataReader reader)
        {
            OverrideMins = reader.ReadBlock<Rsc6PtxEffectOverridables>();
            OverrideMaxes = reader.ReadBlock<Rsc6PtxEffectOverridables>();
            EffectRule = reader.ReadPtr<Rsc6PtxEffectRule>();
            Type = reader.ReadUInt32();
            Time = reader.ReadSingle();
            Toggle = reader.ReadBoolean();
            Pad0 = reader.ReadByte();
            Pad1 = reader.ReadByte();
            Pad2 = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            var rule = writer.BlockList.OfType<Rsc6PtxEffectRule>()?.Where(e => e.ToString().ToLower() == SpawnedEffectRuleName)?.FirstOrDefault();
            writer.WriteBlock(OverrideMins);
            writer.WriteBlock(OverrideMaxes);
            writer.WritePtrEmbed(rule, rule, 0);
            writer.WriteUInt32(Type);
            writer.WriteSingle(Time);
            writer.WriteBoolean(Toggle);
            writer.WriteByte(Pad0);
            writer.WriteByte(Pad1);
            writer.WriteByte(Pad2);
        }

        public void Read(MetaNodeReader reader)
        {
            SpawnedEffectRuleName = reader.ReadString("SpawnedEffectRuleName");
            Type = reader.ReadUInt32("Type");
            Time = reader.ReadSingle("Time");
            Toggle = reader.ReadBool("Toggle");
            OverrideMins = reader.ReadNode<Rsc6PtxEffectOverridables>("OverrideMins");
            OverrideMaxes = reader.ReadNode<Rsc6PtxEffectOverridables>("OverrideMaxes");
        }

        public void Write(MetaNodeWriter writer)
        {
            if (EffectRule.Item != null) writer.WriteString("SpawnedEffectRuleName", EffectRule.Item.ToString());
            writer.WriteUInt32("Type", Type);
            writer.WriteSingle("Time", Time);
            writer.WriteBool("Toggle", Toggle);
            writer.WriteNode("OverrideMins", OverrideMins);
            writer.WriteNode("OverrideMaxes", OverrideMaxes);
        }
    }

    public class Rsc6PtxEffectOverridables : Rsc6BlockBase, MetaNode //rage::ptxEffectOverridables
    {
        public override ulong BlockLength => 48;
        public Vector4 SizeScale { get; set; } //m_SizeScale
        public float DurationScalar { get; set; } //m_Duration
        public float PlaybackRate { get; set; } //m_PlaybackRate
        public Colour ColorTint { get; set; } //m_ColorTint
        public float ZoomScalar { get; set; } //m_Zoom
        public Rsc6PtxEffectScalarFlags Flags { get; set; } //m_WhichFields
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            SizeScale = reader.ReadVector4();
            DurationScalar = reader.ReadSingle();
            PlaybackRate = reader.ReadSingle();
            ColorTint = reader.ReadColour();
            ZoomScalar = reader.ReadSingle();
            Flags = (Rsc6PtxEffectScalarFlags)reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(SizeScale);
            writer.WriteSingle(DurationScalar);
            writer.WriteSingle(PlaybackRate);
            writer.WriteColour(ColorTint);
            writer.WriteSingle(ZoomScalar);
            writer.WriteUInt32((uint)Flags);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            SizeScale = reader.ReadVector4("SizeScale");
            DurationScalar = reader.ReadSingle("DurationScalar");
            PlaybackRate = reader.ReadSingle("PlaybackRate");
            ColorTint = reader.ReadColour("ColorTint");
            ZoomScalar = reader.ReadSingle("ZoomScalar");
            Flags = reader.ReadEnum("Flags", Rsc6PtxEffectScalarFlags.ALL_ACTIVE);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("SizeScale", SizeScale);
            writer.WriteSingle("DurationScalar", DurationScalar);
            writer.WriteSingle("PlaybackRate", PlaybackRate);
            writer.WriteColour("ColorTint", ColorTint);
            writer.WriteSingle("ZoomScalar", ZoomScalar);
            writer.WriteEnum("Flags", Flags);
        }
    }

    public class Rsc6PtxRenderState : Rsc6BlockBase, MetaNode //rage::ptxRenderState
    {
        public override ulong BlockLength => 20;
        public Rsc6PtxCullModeTypes CullMode { get; set; } //m_CullMode
        public Rsc6PtxBlendSetTypes BlendSet { get; set; } //m_BlendSet
        public int DepthBias { get; set; } //m_DepthBias
        public int LightingMode { get; set; } //m_LightingMode
        public bool DepthWrite { get; set; } //m_DepthWrite
        public bool DepthTest { get; set; } //m_DepthTest
        public bool AlphaBlend { get; set; } //m_AlphaBlend
        public byte Pad { get; set; } //m_Pad

        public override void Read(Rsc6DataReader reader)
        {
            CullMode = (Rsc6PtxCullModeTypes)reader.ReadInt32();
            BlendSet = (Rsc6PtxBlendSetTypes)reader.ReadInt32();
            DepthBias = reader.ReadInt32();
            LightingMode = reader.ReadInt32();
            DepthWrite = reader.ReadBoolean();
            DepthTest = reader.ReadBoolean();
            AlphaBlend = reader.ReadBoolean();
            Pad = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteInt32((int)CullMode);
            writer.WriteInt32((int)BlendSet);
            writer.WriteInt32(DepthBias);
            writer.WriteInt32(LightingMode);
            writer.WriteBoolean(DepthTest);
            writer.WriteBoolean(AlphaBlend);
            writer.WriteByte(Pad);
        }

        public void Read(MetaNodeReader reader)
        {
            CullMode = reader.ReadEnum("CullMode", Rsc6PtxCullModeTypes.BACK_CULLING);
            BlendSet = reader.ReadEnum("BlendSet", Rsc6PtxBlendSetTypes.NORMAL);
            DepthBias = reader.ReadInt32("DepthBias");
            LightingMode = reader.ReadInt32("LightingMode");
            DepthWrite = reader.ReadBool("DepthWrite");
            DepthTest = reader.ReadBool("DepthTest");
            AlphaBlend = reader.ReadBool("AlphaBlend");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteEnum("CullMode", CullMode);
            writer.WriteEnum("BlendSet", BlendSet);
            writer.WriteInt32("DepthBias", DepthBias);
            writer.WriteInt32("LightingMode", LightingMode);
            writer.WriteBool("DepthTest", DepthTest);
            writer.WriteBool("AlphaBlend", AlphaBlend);
        }
    }

    public class Rsc6PtxTimeLine : Rsc6FileBase, MetaNode //rage::ptxTimeLine
    {
        public override ulong BlockLength => 36;
        public override uint VFT { get; set; } = 0x009722B8;
        public float Duration { get; set; } //m_Duration
        public float PreUpdate { get; set; } //m_PreUpdate
        public int NumLoops { get; set; } //m_NumLoops
        public Rsc6PtrArr<Rsc6PtxEvent> Events { get; set; } //m_Events, capacity set to 32?
        public Rsc6Ptr<Rsc6PtxEffectRuleStd> EffectRule { get; set; } //m_EffectRule
        public uint SelectedIndex { get; set; } = 0xCDCDCDCD; //m_SelectedIndex, always 0xCDCDCDCD
        public Rsc6Ptr<Rsc6PtxEffectRuleStd> ParentRule { get; set; } //m_ParentRule, same as EffectRule

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Duration = reader.ReadSingle();
            PreUpdate = reader.ReadSingle();
            NumLoops = reader.ReadInt32();
            Events = reader.ReadPtrArr(Rsc6PtxEvent.Create);
            EffectRule = reader.ReadPtr<Rsc6PtxEffectRuleStd>();
            SelectedIndex = reader.ReadUInt32();
            ParentRule = reader.ReadPtr<Rsc6PtxEffectRuleStd>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
        }
    }

    public class Rsc6PtxEventEmitter : Rsc6PtxEvent, MetaNode //rage::ptxEventEmitter
    {
        public override ulong BlockLength => base.BlockLength + 48;
        public float DurationScalarMin { get; set; } //m_DurationScalarMin
        public float DurationScalarMax { get; set; } //m_DurationScalarMax
        public float TimeScalarMin { get; set; } //m_TimeScalarMin
        public float TimeScalarMax { get; set; } //m_TimeScalarMax
        public float ZoomMin { get; set; } //m_ZoomMin
        public float ZoomMax { get; set; } //m_ZoomMax
        public Colour ColorTintMin { get; set; } //m_ColorTintMin
        public Colour ColorTintMax { get; set; } //m_ColorTintMaxs
        public Rsc6Str EmitRuleName { get; set; } //m_EmitRuleName
        public Rsc6Str PtxRuleName { get; set; } //m_PtxRuleName
        public Rsc6Ptr<Rsc6PtxEmitRule> EmitRule { get; set; } //m_EmitRule
        public Rsc6Ptr<Rsc6PtxRule> PtxRule { get; set; } //m_PtxRule

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            DurationScalarMin = reader.ReadSingle();
            DurationScalarMax = reader.ReadSingle();
            TimeScalarMin = reader.ReadSingle();
            TimeScalarMax = reader.ReadSingle();
            ZoomMin = reader.ReadSingle();
            ZoomMax = reader.ReadSingle();
            ColorTintMin = reader.ReadColour();
            ColorTintMax = reader.ReadColour();
            EmitRuleName = reader.ReadStr();
            PtxRuleName = reader.ReadStr();
            EmitRule = reader.ReadPtr<Rsc6PtxEmitRule>();
            PtxRule = reader.ReadPtr<Rsc6PtxRule>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            Rsc6PtxEmitRule emitRule = null;
            Rsc6PtxRule ptxRule = null;
            var emitName = EmitRuleName.ToString();
            var ptxName = PtxRuleName.ToString();

            if (!string.IsNullOrEmpty(emitName))
            {
                emitRule = writer.BlockList.OfType<Rsc6PtxEmitRule>()?.Where(e => e.Name.ToLower() == emitName)?.FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(ptxName))
            {
                ptxRule = writer.BlockList.OfType<Rsc6PtxRule>()?.Where(e => e.ToString().ToLower() == ptxName)?.FirstOrDefault();
            }

            base.Write(writer);
            writer.WriteSingle(DurationScalarMin);
            writer.WriteSingle(DurationScalarMax);
            writer.WriteSingle(TimeScalarMin);
            writer.WriteSingle(TimeScalarMax);
            writer.WriteSingle(ZoomMin);
            writer.WriteSingle(ZoomMax);
            writer.WriteColour(ColorTintMin);
            writer.WriteColour(ColorTintMax);
            writer.WriteStr(EmitRuleName);
            writer.WriteStr(EmitRuleName);
            writer.WritePtrEmbed(emitRule, emitRule, 0);
            writer.WritePtrEmbed(ptxRule, ptxRule, 0);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            DurationScalarMin = reader.ReadSingle("DurationScalarMin");
            DurationScalarMax = reader.ReadSingle("DurationScalarMax");
            TimeScalarMin = reader.ReadSingle("TimeScalarMin");
            TimeScalarMax = reader.ReadSingle("TimeScalarMax");
            ZoomMin = reader.ReadSingle("ZoomMin");
            ZoomMax = reader.ReadSingle("ZoomMax");
            ColorTintMin = reader.ReadColour("ColorTintMin");
            ColorTintMax = reader.ReadColour("ColorTintMax");
            EmitRuleName = new(reader.ReadString("EmitRuleName"));
            PtxRuleName = new(reader.ReadString("PtxRuleName"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle("DurationScalarMin", DurationScalarMin);
            writer.WriteSingle("DurationScalarMax", DurationScalarMax);
            writer.WriteSingle("TimeScalarMin", TimeScalarMin);
            writer.WriteSingle("TimeScalarMax", TimeScalarMax);
            writer.WriteSingle("ZoomMin", ZoomMin);
            writer.WriteSingle("ZoomMax", ZoomMax);
            writer.WriteColour("ColorTintMin", ColorTintMin);
            writer.WriteColour("ColorTintMax", ColorTintMax);
            writer.WriteString("EmitRuleName", EmitRuleName.ToString());
            writer.WriteString("PtxRuleName", EmitRuleName.ToString());
        }
    }

    public class Rsc6PtxEventEffect : Rsc6PtxEvent, MetaNode //rage::ptxEventEffect
    {
        public override ulong BlockLength => base.BlockLength + 140;
        public Rsc6Str EffectName { get; set; } //m_EffectName
        public Rsc6Ptr<Rsc6PtxEffectRule> Effect { get; set; } //m_Effect
        public int EmitterDomainType { get; set; } //m_EmitterDomainType
        public Vector4 RotationMin { get; set; } //m_RotationMin
        public Rsc6PtxEffectOverridables OverrideMins { get; set; } //m_OverrideMins
        public Rsc6PtxEffectOverridables OverrideMaxes { get; set; } //m_OverrideMaxes
        public Rsc6Ptr<Rsc6PtxDomain> EmitterDomain { get; set; } //m_EmitterDomain
        public uint Unknown_A4h { get; set; } = 0xCDCDCDCD; //Padding
        public bool ShowEmitterDomain { get; set; } //m_ShowEmitterDomain
        public byte Unknown_A9h { get; set; } = 0xCD; //Padding
        public ushort Unknown_AAh { get; set; } = 0xCDCD; //Padding
        public uint Unknown_ACh { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            EffectName = reader.ReadStr();
            Effect = reader.ReadPtr<Rsc6PtxEffectRule>();
            EmitterDomainType = reader.ReadInt32();
            RotationMin = reader.ReadVector4();
            OverrideMins = reader.ReadBlock<Rsc6PtxEffectOverridables>();
            OverrideMaxes = reader.ReadBlock<Rsc6PtxEffectOverridables>();
            EmitterDomain = reader.ReadPtr<Rsc6PtxDomain>();
            Unknown_A4h = reader.ReadUInt32();
            ShowEmitterDomain = reader.ReadBoolean();
            Unknown_A9h = reader.ReadByte();
            Unknown_AAh = reader.ReadUInt16();
            Unknown_ACh = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
        }
    }

    public class Rsc6PtxEvent : Rsc6BlockBaseMap, MetaNode //rage::ptxEvent
    {
        public override ulong BlockLength => 36;
        public override uint VFT { get; set; } = 0x00976EC4;
        public float TriggerTime { get; set; } //m_TriggerTime
        public Rsc6Ptr<Rsc6PtxEvolutionList> EvolutionList1 { get; set; } //m_pEvolutionList1
        public Rsc6Ptr<Rsc6PtxEvolutionList> EvolutionList2 { get; set; } //m_pEvolutionList2
        public float DistToCamera { get; set; } //m_Dist2ToCamera, space to store the distance from the event to the camera
        public Rsc6PtxEventTypes Type { get; set; } //m_Type, the event type (emitter or effect)
        public int TriggerCap { get; set; } = -1; //m_TriggerCap, always -1?
        public int Index { get; set; } //m_Index, the index of the event

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TriggerTime = reader.ReadSingle();
            EvolutionList1 = reader.ReadPtr<Rsc6PtxEvolutionList>();
            EvolutionList2 = reader.ReadPtr<Rsc6PtxEvolutionList>();
            DistToCamera = reader.ReadSingle();
            Type = (Rsc6PtxEventTypes)reader.ReadInt32();
            TriggerCap = reader.ReadInt32();
            Index = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle(TriggerTime);
            writer.WritePtr(EvolutionList1);
            writer.WritePtr(EvolutionList2);
            writer.WriteSingle(DistToCamera);
            writer.WriteInt32((int)Type);
            writer.WriteInt32(TriggerCap);
            writer.WriteInt32(Index);
        }

        public void Read(MetaNodeReader reader)
        {
            Type = reader.ReadEnum("@type", Rsc6PtxEventTypes.EMITTER);
            TriggerTime = reader.ReadSingle("TriggerTime");
            EvolutionList1 = new(reader.ReadNode<Rsc6PtxEvolutionList>("EvolutionList1"));
            EvolutionList1 = new(reader.ReadNode<Rsc6PtxEvolutionList>("EvolutionList2"));
            DistToCamera = reader.ReadSingle("DistToCamera");
            TriggerCap = reader.ReadInt32("TriggerCap");
            Index = reader.ReadInt32("Index");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteEnum("@type", Type);
            writer.WriteSingle("TriggerTime", TriggerTime);
            writer.WriteNode("EvolutionList1", EvolutionList1.Item);
            writer.WriteNode("EvolutionList2", EvolutionList2.Item);
            writer.WriteSingle("DistToCamera", DistToCamera);
            writer.WriteInt32("TriggerCap", TriggerCap);
            writer.WriteInt32("Index", Index);
        }

        public static Rsc6PtxEvent Create(Rsc6DataReader r)
        {
            r.Position += 24;
            var type = (Rsc6PtxEventTypes)r.ReadInt32();
            r.Position -= 28;
            return Create(type);
        }

        public static Rsc6PtxEvent Create(Rsc6PtxEventTypes type)
        {
            return type switch
            {
                Rsc6PtxEventTypes.EMITTER => new Rsc6PtxEventEmitter(),
                Rsc6PtxEventTypes.EFFECT => new Rsc6PtxEventEffect(),
                _ => throw new Exception("Unknown particle event type")
            };
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    public class Rsc6PtxDomain : Rsc6FileBase, MetaNode //rage::ptxDomainObj
    {
        public override ulong BlockLength => 512;
        public override uint VFT { get; set; } = 0x00971644;
        public float Scale { get; set; } //m_Scale
        public Rsc6PtxDomainTypes DomainType { get; set; } //m_DomainType
        public int DomainFunction { get; set; } //m_DomainFunction
        public Matrix4x4 Matrix { get; set; } //m_Matrix
        public Matrix4x4 Unknown_50h { get; set; } //Always 0, rage::VMath::AoS::Vec3V[4]
        public Rsc6PtxKeyFrameProperty PositionKFP { get; set; } //m_PositionKFP, keyframeable position of the domain
        public Rsc6PtxKeyFrameProperty DirectionKFP { get; set; } //m_DirectionKFP, keyframeable direction of the domain
        public Rsc6PtxKeyFrameProperty SizeKFP { get; set; } //m_SizeKFP, keyframeable size of the domain
        public Rsc6PtxKeyFrameProperty InnerSizeKFP { get; set; } //m_InnerSizeKFP, keyframeable inner size of the domain
        public Rsc6PtrArr<Rsc6PtxKeyFrameProperty> PropertyList { get; set; } //m_PropList, capacity set to 16
        public float KeyTime { get; set; } //m_KeyTime
        public float FileVersion { get; set; } //m_FileVersion, this domain's file version
        public Rsc6ManagedArr<Rsc6PtxEvolutionListStruct> EvoList { get; set; } //m_EvoList, always NULL
        public uint RelMatrix { get; set; } //m_RelMatrix
        public uint Unknown_1ECh { get; set; } //Always 0
        public uint Unknown_1F0h { get; set; } //Always 0
        public uint Unknown_1F4h { get; set; } //Always 0
        public uint Unknown_1F8h { get; set; } //Always 0
        public bool WorldSpace { get; set; } //m_WorldSpace
        public bool PointRelative { get; set; } //m_PointRelative
        public ushort Unknown_1FEh { get; set; } = 0xCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Scale = reader.ReadSingle();
            DomainType = (Rsc6PtxDomainTypes)reader.ReadInt32();
            DomainFunction = reader.ReadInt32();
            Matrix = reader.ReadMatrix4x4();
            Unknown_50h = reader.ReadMatrix4x4();
            PositionKFP = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            DirectionKFP = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            SizeKFP = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            InnerSizeKFP = reader.ReadBlock<Rsc6PtxKeyFrameProperty>();
            PropertyList = reader.ReadPtrArr<Rsc6PtxKeyFrameProperty>();
            KeyTime = reader.ReadSingle();
            FileVersion = reader.ReadSingle();
            EvoList = reader.ReadArr<Rsc6PtxEvolutionListStruct>();
            RelMatrix = reader.ReadUInt32();
            Unknown_1ECh = reader.ReadUInt32();
            Unknown_1F0h = reader.ReadUInt32();
            Unknown_1F4h = reader.ReadUInt32();
            Unknown_1F8h = reader.ReadUInt32();
            WorldSpace = reader.ReadBoolean();
            PointRelative = reader.ReadBoolean();
            Unknown_1FEh = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
        }
    }

    public class Rsc6PtxEvolutionList : Rsc6BlockBase, MetaNode //rage::ptxEvolutionList
    {
        public override ulong BlockLength => 28;
        public Rsc6ManagedArr<Rsc6PtxEvolutionListStruct> Evolutions { get; set; } //m_Evolutions
        public Rsc6ManagedArr<Rsc6PtxEvolutionPropBase> PropertyList { get; set; } //m_PropList
        public bool Sorted { get; set; } = true; //Sorted
        public byte Pad0 { get; set; } = 0xCD; //Pad[0]
        public byte Pad1 { get; set; } = 0xCD; //Pad[1]
        public byte Pad2 { get; set; } = 0xCD; //Pad[2]
        public Rsc6ManagedArr<Rsc6PtxEvolutionPropBaseMap> Data { get; set; } //Data, atBinaryMap<uint, Rsc6PtxEvolutionPropBase>

        public override void Read(Rsc6DataReader reader)
        {
            Evolutions = reader.ReadArr<Rsc6PtxEvolutionListStruct>();
            PropertyList = reader.ReadArr<Rsc6PtxEvolutionPropBase>();
            Sorted = reader.ReadBoolean();
            Pad0 = reader.ReadByte();
            Pad1 = reader.ReadByte();
            Pad2 = reader.ReadByte();
            Data = reader.ReadArr<Rsc6PtxEvolutionPropBaseMap>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
        }
    }

    public class Rsc6PtxEvolutionListStruct : Rsc6BlockBase, MetaNode //rage::ptxEvolutionList::evolutionStruct
    {
        public override ulong BlockLength => 16;
        public Rsc6Str EvoName { get; set; } //m_EvoName, name of the evolution
        public float TestEvoTime { get; set; } //m_TestEvoTime
        public JenkHash EvoNameHash { get; set; } //m_nameHash, hash of the evolution name
        public bool OverrideProc { get; set; } = true; //m_OverrideProc, whether the evolution value is overriden (from the editor)
        public byte Pad0 { get; set; } = 0xCD; //Pad[0]
        public byte Pad1 { get; set; } = 0xCD; //Pad[1]
        public byte Pad2 { get; set; } = 0xCD; //Pad[2]

        public override void Read(Rsc6DataReader reader)
        {
            EvoName = reader.ReadStr();
            TestEvoTime = reader.ReadSingle();
            EvoNameHash = reader.ReadUInt32();
            OverrideProc = reader.ReadBoolean();
            Pad0 = reader.ReadByte();
            Pad1 = reader.ReadByte();
            Pad2 = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
        }

        public override string ToString()
        {
            return EvoName.ToString();
        }
    }

    public class Rsc6PtxEvolutionPropBaseMap : Rsc6BlockBase, MetaNode //rage::ptxEvoPropBase
    {
        public override ulong BlockLength => 8;
        public JenkHash Key { get; set; } //key
        public Rsc6Ptr<Rsc6PtxEvolutionPropBase> Value { get; set; } //data

        public override void Read(Rsc6DataReader reader)
        {
            Key = reader.ReadUInt32();
            Value = reader.ReadPtr<Rsc6PtxEvolutionPropBase>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Key);
            writer.WritePtr(Value);
        }

        public void Read(MetaNodeReader reader)
        {
            Key = reader.ReadUInt32("Key");
            Value = new(reader.ReadUInt32("Value"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Key", Key);
            writer.WriteNode("Value", Value.Item);
        }
    }

    public class Rsc6PtxEvolutionPropBase : Rsc6BlockBase, MetaNode //rage::ptxEvoPropBase
    {
        public override ulong BlockLength => 16;
        public Rsc6ManagedArr<Rsc6PtxEvolutionPropChain> PropertyChain { get; set; } //m_PropChain, array of this property's evolved keyframes
        public JenkHash PropertyHash { get; set; } //m_PropHashID, the id of the keyframed property
        public Rsc6PtxEvoBlendModes EvoBlendMode { get; set; } //m_EvoBlendMode, how the base and evolved keyframes are blended to produce the final keyframe
        public byte Pad0 { get; set; } = 0xCD; //Pad[0]
        public byte Pad1 { get; set; } = 0xCD; //Pad[1]
        public byte Pad2 { get; set; } = 0xCD; //Pad[2]

        public override void Read(Rsc6DataReader reader)
        {
            PropertyChain = reader.ReadArr<Rsc6PtxEvolutionPropChain>();
            PropertyHash = reader.ReadUInt32();
            EvoBlendMode = (Rsc6PtxEvoBlendModes)reader.ReadByte();
            Pad0 = reader.ReadByte();
            Pad1 = reader.ReadByte();
            Pad2 = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
        }
    }

    public class Rsc6PtxEvolutionPropChain : Rsc6BlockBase, MetaNode //rage::ptxEvoPropChain
    {
        public override ulong BlockLength => 80;
        public Rsc6PtxKeyFrame Keyframe { get; set; } //m_Keyframe
        public int EvoIndex { get; set; } //m_EvoIndex
        public uint Unknown_44h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_48h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_4Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            Keyframe = reader.ReadBlock<Rsc6PtxKeyFrame>();
            EvoIndex = reader.ReadInt32();
            Unknown_44h = reader.ReadUInt32();
            Unknown_48h = reader.ReadUInt32();
            Unknown_4Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
        }

        public override string ToString()
        {
            return Keyframe.ToString();
        }
    }

    public enum Rsc6PtxEventTypes : int
    {
        EMITTER,
        EFFECT
    };

    public enum Rsc6PtxDomainTypes : int
    {
        DOMAIN__CREATION,
        DOMAIN_TARGET,
        DOMAIN_ATTRACTOR
    };

    public enum Rsc6PtxEvoBlendModes : byte
    {
        MODE_AVERAGE,
        MODE_ADD,
        MODE_MAX,
        MODE_FULL_AVERAGE
    };

    public enum Rsc6PtxCullModeTypes : int
    {
        NO_CULLING, //No culling
        FRONT_CULLING, //Cull front-facing polygons
        BACK_CULLING //Cull back-facing polygons
    };

    public enum Rsc6PtxBlendSetTypes : int
    {
        NORMAL, //Standard dest * (1 - srcAlpha) + src * srcAlpha blend
        ADD, //Dest + src
        SUBSTRACT, //Src - dest
        LIGHTMAP, //Dest * srcAlpha + src * (1 - srcAlpha), same as 'NORMAL' but with inverted alpha
        MATTE, //Dest + src * destAlpha, for additive HUD cutout
        OVERWRITE, //Src only (same as disabling alpha blending)
        DEST, //Dest * (1 - destAlpha) + src * destAlpha, for blend HUD cutout
        ALPHA_ADD, //Dest + src * srcAlpha
        REVERSE_SUBSTRACT, //Dest - src
        MIN, //Min(src, dest)
        MAX, //Max(src, dest)
        ALPHA_SUBSTRACT, //Dest - src * srcAlpha
        MULTIPLY_SRC_DEST, //Multiply dest * src
        COMPOSITE_ALPHA, //Dest * (1 - srcAlpha) + src
        COMPOSITE_ALPHA_SUBTRACT //Dest * (1 - srcAlpha) - src
    };

    [Flags]
    public enum Rsc6PtxEffectScalarFlags : uint
    {
        NONE_ACTIVE = 0 << 0,
        ACTIVE_DURATION = 1 << 0,
        ACTIVE_PLAYBACK_RATE = 1 << 1,
        ACTIVE_COLOUR_TINT = 1 << 2,
        ACTIVE_ZOOM = 1 << 3,
        ACTIVE_SIZE_SCALAR = 1 << 4,
        ALL_ACTIVE = 1 << 5
    };
}