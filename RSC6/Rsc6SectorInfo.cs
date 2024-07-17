using System;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using CodeX.Core.Utilities;
using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Games.RDR1.RPF6;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Xml.Linq;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6SectorInfo : Rsc6BlockBaseMap, MetaNode //sagSectorInfo
    {
        public override ulong BlockLength => 480;
        public override uint VFT { get; set; } = 0x01909C38;
        public Rsc6Str Name { get; set; } //Only for scoped sectors, otherwise NULL
        public float LODFade { get; set; } //m_LODFade, always 0.0f
        public ulong Unknown_10h { get; set; } //Always 0
        public bool Added { get; set; } //m_Added, always FALSE
        public bool PropsGroup { get; set; } //m_PropsGroup
        public bool MissingMedLOD { get; set; } //m_MissingMedLOD, always FALSE
        public byte Unknown_1Bh { get; set; } = 0xFF; //0xFF padding
        public JenkHash ScopedNameHash { get; set; } //m_iScopeNameHash, only for scoped sectors, otherwise NULL
        public Rsc6Ptr<Rsc6CurveGroup>[] Curves { get; set; } = new Rsc6Ptr<Rsc6CurveGroup>[24]; //m_CurveArrays, not used in 'swTerrain', 'swAll' & some random areas
        public int ParentLevelIndex { get; set; } = 24; //m_ParentLevelIndex, always 24
        public int Unknown_84h { get; set; } = -1; //Always -1
        public uint Unknown_88h { get; set; } //Always 0
        public uint ExtraCurveData { get; set; } //Used only in 'swAiCurves' - (sagCurveExtraData, sagCurveStringMap & CurveNetworkGraph)
        public Vector4 MinAndBoundingRadius { get; set; } //m_MinAndBoundingRadius
        public Vector4 MaxAndInscribedRadius { get; set; } //m_MaxAndInscribedRadius
        public Vector4 BoundMin { get; set; }
        public Vector4 BoundMax { get; set; }
        public Rsc6Ptr<Rsc6PlacedLightsGroup> PlacedLightsGroup { get; set; } //m_PlacedLightsGroup
        public Rsc6ManagedArr<Rsc6PropInstanceInfo> Entities { get; set; } //m_Props
        public Rsc6PtrArr<Rsc6MapAttribute> DoorsAttributes { get; set; }
        public uint Unknown_E4h { get; set; } = 0x00CDCDCD; //Padding
        public Rsc6ManagedArr<Rsc6SectorChild> ItemMapChilds { get; set; } //m_Children, 'swAll.wsi' only
        public Rsc6Ptr<Rsc6ScopedSectors> ItemChilds { get; set; } //m_ChildGroup
        public Rsc6PtrArr<Rsc6SectorInfo> ChildPtrs { get; set; } //m_ChildPtrs, used only in swTerrain, swHorseCurves, etc.
        public Rsc6ManagedArr<Rsc6DrawableInstanceBase> DrawableInstances { get; set; } //m_SectorDrawableInstances
        public Rsc6ManagedArr<Rsc6DrawableInstance> Unknown_104h { get; set; }
        public Rsc6ManagedArr<Rsc6Portal> Portals { get; set; } //m_Portals
        public Rsc6PtrArr<Rsc6Attribute> Attributes { get; set; } //m_Attributes
        public uint Unknown_11Ch { get; set; } //Always 0
        public Rsc6StreamableBase VisualDictionary { get; set; } = new Rsc6StreamableBase();
        public Rsc6StreamableBase MedVisualDictionary { get; set; } = new Rsc6StreamableBase();
        public uint Unknown_140h { get; set; } //Always 0
        public uint Unknown_144h { get; set; } //Always 0
        public uint Unknown_148h { get; set; } //Always 0
        public uint Unknown_14Ch { get; set; } //Always 0
        public Rsc6StreamableBase VLowVisualDictionary { get; set; } = new Rsc6StreamableBase();
        public Rsc6StreamableBase BoundDictionary { get; set; } = new Rsc6StreamableBase();
        public uint Unknown_170h { get; set; } //Always 0
        public float LowLODFade { get; set; } = 1.0f; //m_VLowLODFade, always 1.0f
        public JenkHash SectorNameLower { get; set; } //m_NameHash
        public Rsc6ManagedArr<Rsc6Portal> Occluders { get; set; } //m_Occluders
        public uint LastVisibleMarker { get; set; } //m_LastVisibleMarker, always 0
        public uint ResidentStatus { get; set; } = 85; //m_ResidentStatus - 0 or 85
        public uint Unknown_18Ch { get; set; } //m_StreamedObjects? - always 0
        public uint Unknown_190h { get; set; } //Always 0
        public uint Unknown_194h { get; set; } //m_ParentWithTD? - always 0
        public uint Unknown_198h { get; set; } //Always 0
        public Rsc6PtrStr PropNames { get; set; }
        public Rsc6ManagedArr<Rsc6LocatorStatic> Locators { get; set; } //m_Locators
        public bool AnyHighInstanceLoaded { get; set; } //m_AnyHighLODInstancesLoaded
        public byte ResidentVLowCount { get; set; } //m_ResidentVLowCount, always 0
        public bool HasVLowLODResource { get; set; } //m_HasVLowLodResource
        public bool VLowSuperseded { get; set; } //m_VLowSuperseded, always FALSE
        public Rsc6Str Scope { get; set; } //m_Scope
        public ushort Unknown_1B4h { get; set; } //Always 0
        public ushort NumGroupedChildren { get; set; } //m_iNumGroupedChildren, always 0
        public ushort LODReferences { get; set; } //m_LODReferences, always 0
        public ushort Unknown_1B8h { get; set; } //Always 0
        public ushort Unknown_1BAh { get; set; } //Always 0
        public byte CurrentLOD { get; set; } = 0xFF; //m_CurrentLOD
        public bool District { get; set; } //m_District
        public byte IsTerrain { get; set; } = 0xCD; //m_IsTerrain
        public byte TotallyAllInstancesLoaded { get; set; } = 0xCD; //m_TotallyAllInstancesLoaded
        public byte HasDictFlags { get; set; } = 0xCD; //m_HasDictFlags
        public byte InstanceAge { get; set; } = 0xCD; //m_InstanceAge
        public byte BoundAge { get; set; } = 0xCD; //m_BoundAge
        public byte PropsAge { get; set; } = 0xCD; //m_PropsAge
        public byte RefCount { get; set; } //m_RefCount
        public byte ParentChildIndex { get; set; } //m_ParentChildIndex, always 0
        public uint Flags { get; set; } //m_Flags
        public bool InnerPropsInstanciated { get; set; } = true; //m_InnerPropsInstantiated, always TRUE
        public byte InnerPropsAge { get; set; } //m_InnerPropsAge, always 0
        public byte RawPropsGroup { get; set; } //m_RawPropsGroup, always 0
        public byte GroupFileFlags { get; set; } //m_GroupFileFlags, always 0
        public Rsc6ManagedArr<Rsc6BoundInstance> BoundInstances { get; set; } //m_BoundInstances
        public ulong NamedNodeMap { get; set; } //m_NamedNodeMap, rage::atMap<int,rage::datRef<RDR2NameAttribute>>
        public BoundingBox Bounds { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadStr();
            LODFade = reader.ReadSingle();
            Unknown_10h = reader.ReadUInt64();
            Added = reader.ReadBoolean();
            PropsGroup = reader.ReadBoolean();
            MissingMedLOD = reader.ReadBoolean(); 
            Unknown_1Bh = reader.ReadByte(); 
            ScopedNameHash = reader.ReadUInt32();

            for (int i = 0; i < Curves.Length; i++) //Start of sagCurveGroup
            {
                Curves[i] = reader.ReadPtr<Rsc6CurveGroup>(); //sagCurveArray
            }

            ParentLevelIndex = reader.ReadInt32();
            Unknown_84h = reader.ReadInt32();
            Unknown_88h = reader.ReadUInt32();
            ExtraCurveData = reader.ReadUInt32();
            MinAndBoundingRadius = reader.ReadVector4();
            MaxAndInscribedRadius = reader.ReadVector4(); //End of sagCurveGroup
            BoundMin = reader.ReadVector4();
            BoundMax = reader.ReadVector4();
            PlacedLightsGroup = reader.ReadPtr<Rsc6PlacedLightsGroup>();
            Entities = reader.ReadArr<Rsc6PropInstanceInfo>();
            DoorsAttributes = reader.ReadPtrArr<Rsc6MapAttribute>();
            Unknown_E4h = reader.ReadUInt32();
            ItemMapChilds = reader.ReadArr<Rsc6SectorChild>();
            ItemChilds = reader.ReadPtr<Rsc6ScopedSectors>();
            ChildPtrs = reader.ReadPtrArr<Rsc6SectorInfo>();
            DrawableInstances = reader.ReadArr<Rsc6DrawableInstanceBase>();
            Unknown_104h = reader.ReadArr<Rsc6DrawableInstance>();
            Portals = reader.ReadArr<Rsc6Portal>();
            Attributes = reader.ReadPtrArr(Rsc6Attribute.Create);
            Unknown_11Ch = reader.ReadUInt32();
            VisualDictionary.Read(reader);
            MedVisualDictionary.Read(reader);
            Unknown_140h = reader.ReadUInt32();
            Unknown_144h = reader.ReadUInt32();
            Unknown_148h = reader.ReadUInt32();
            Unknown_14Ch = reader.ReadUInt32();
            VLowVisualDictionary.Read(reader);
            BoundDictionary.Read(reader);
            Unknown_170h = reader.ReadUInt32();
            LowLODFade = reader.ReadSingle();
            SectorNameLower = reader.ReadUInt32();
            Occluders = reader.ReadArr<Rsc6Portal>();
            LastVisibleMarker = reader.ReadUInt32();
            ResidentStatus = reader.ReadUInt32();
            Unknown_18Ch = reader.ReadUInt32();
            Unknown_190h = reader.ReadUInt32();
            Unknown_194h = reader.ReadUInt32();
            Unknown_198h = reader.ReadUInt32();
            PropNames = reader.ReadPtrStr();
            Locators = reader.ReadArr<Rsc6LocatorStatic>();
            AnyHighInstanceLoaded = reader.ReadBoolean();
            ResidentVLowCount = reader.ReadByte();
            HasVLowLODResource = reader.ReadBoolean();
            VLowSuperseded = reader.ReadBoolean();
            Scope = reader.ReadStr();
            Unknown_1B4h = reader.ReadUInt16();
            NumGroupedChildren = reader.ReadUInt16();
            LODReferences = reader.ReadUInt16();
            Unknown_1B8h = reader.ReadUInt16();
            Unknown_1BAh = reader.ReadUInt16();
            CurrentLOD = reader.ReadByte();
            District = reader.ReadBoolean();
            IsTerrain = reader.ReadByte();
            TotallyAllInstancesLoaded = reader.ReadByte();
            HasDictFlags = reader.ReadByte();
            InstanceAge = reader.ReadByte();
            BoundAge = reader.ReadByte();
            PropsAge = reader.ReadByte();
            RefCount = reader.ReadByte();
            ParentChildIndex = reader.ReadByte();
            Flags = reader.ReadUInt32();
            InnerPropsInstanciated = reader.ReadBoolean();
            InnerPropsAge = reader.ReadByte();
            RawPropsGroup = reader.ReadByte();
            GroupFileFlags = reader.ReadByte();
            BoundInstances = reader.ReadArr<Rsc6BoundInstance>();
            NamedNodeMap = reader.ReadUInt64();
            Bounds = new BoundingBox(BoundMin.XYZ(), BoundMax.XYZ());

            /*Debug.WriteLine("--------------------------------------");
            Debug.WriteLine("Name: " + Name.ToString());
            Debug.WriteLine("Unknown_1B8h: " + Unknown_1B8h.ToString());
            Debug.WriteLine("Unknown_1BAh: " + Unknown_1BAh.ToString());
            Debug.WriteLine("CurrentLOD: " + CurrentLOD.ToString());
            Debug.WriteLine("District: " + District.ToString());
            Debug.WriteLine("IsTerrain: " + IsTerrain.ToString());
            Debug.WriteLine("TotallyAllInstancesLoaded: " + TotallyAllInstancesLoaded.ToString());
            Debug.WriteLine("HasDictFlags: " + HasDictFlags.ToString());
            Debug.WriteLine("InstanceAge: " + InstanceAge.ToString());
            Debug.WriteLine("BoundAge: " + BoundAge.ToString());
            Debug.WriteLine("PropsAge: " + PropsAge.ToString());
            Debug.WriteLine("RefCount: " + RefCount.ToString());
            Debug.WriteLine("ParentChildIndex: " + ParentChildIndex.ToString());
            Debug.WriteLine("Flags: " + Flags.ToString());
            Debug.WriteLine("InnerPropsInstanciated: " + InnerPropsInstanciated.ToString());
            Debug.WriteLine("InnerPropsAge: " + InnerPropsAge.ToString());
            Debug.WriteLine("RawPropsGroup: " + RawPropsGroup.ToString());
            Debug.WriteLine("GroupFileFlags: " + GroupFileFlags.ToString());
            Debug.WriteLine("BoundInstances: " + BoundInstances.ToString());
            Debug.WriteLine("NamedNodeMap: " + NamedNodeMap.ToString());*/
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteStr(Name);
            writer.WriteSingle(LODFade);
            writer.WriteUInt64(Unknown_10h);
            writer.WriteByte(Added ? (byte)1 : (byte)0);
            writer.WriteByte(PropsGroup ? (byte)1 : (byte)0);
            writer.WriteByte(MissingMedLOD ? (byte)1 : (byte)0);
            writer.WriteByte(Unknown_1Bh);
            writer.WriteUInt32(ScopedNameHash);

            for (int i = 0; i < Curves.Length; i++)
            {
                writer.WritePtr(Curves[i]);
            }

            writer.WriteInt32(ParentLevelIndex);
            writer.WriteInt32(Unknown_84h);
            writer.WriteUInt32(Unknown_88h);
            writer.WriteUInt32(ExtraCurveData);
            writer.WriteVector4(MinAndBoundingRadius);
            writer.WriteVector4(MaxAndInscribedRadius);
            writer.WriteVector4(BoundMin);
            writer.WriteVector4(BoundMax);
            writer.WritePtr(PlacedLightsGroup);
            writer.WriteArr(Entities);
            writer.WritePtrArr(DoorsAttributes, true);
            writer.WriteUInt32(Unknown_E4h);
            writer.WriteArr(ItemMapChilds);
            writer.WritePtr(ItemChilds);
            writer.WritePtrArr(ChildPtrs);
            writer.WriteArr(DrawableInstances);
            writer.WriteArr(Unknown_104h);
            writer.WriteArr(Portals);
            writer.WritePtrArr(Attributes);
            writer.WriteUInt32(Unknown_11Ch);
            VisualDictionary.Write(writer);
            MedVisualDictionary.Write(writer);
            writer.WriteUInt32(Unknown_140h);
            writer.WriteUInt32(Unknown_144h);
            writer.WriteUInt32(Unknown_148h);
            writer.WriteUInt32(Unknown_14Ch);
            VLowVisualDictionary.Write(writer);
            BoundDictionary.Write(writer);
            writer.WriteUInt32(Unknown_170h);
            writer.WriteSingle(LowLODFade);
            writer.WriteUInt32(SectorNameLower);
            writer.WriteArr(Occluders);
            writer.WriteUInt32(LastVisibleMarker);
            writer.WriteUInt32(ResidentStatus);
            writer.WriteUInt32(Unknown_18Ch);
            writer.WriteUInt32(Unknown_190h);
            writer.WriteUInt32(Unknown_194h);
            writer.WriteUInt32(Unknown_198h);
            writer.WritePtrStr(PropNames);
            writer.WriteArr(Locators);
            writer.WriteByte(AnyHighInstanceLoaded ? (byte)1 : (byte)0);
            writer.WriteByte(ResidentVLowCount);
            writer.WriteByte(HasVLowLODResource ? (byte)1 : (byte)0);
            writer.WriteByte(VLowSuperseded ? (byte)1 : (byte)0);
            writer.WriteStr(Scope);
            writer.WriteUInt16(Unknown_1B4h);
            writer.WriteUInt16(NumGroupedChildren);
            writer.WriteUInt16(LODReferences);
            writer.WriteUInt16(Unknown_1B8h);
            writer.WriteUInt16(Unknown_1BAh);
            writer.WriteByte(CurrentLOD);
            writer.WriteBoolean(District);
            writer.WriteByte(IsTerrain);
            writer.WriteByte(TotallyAllInstancesLoaded);
            writer.WriteByte(HasDictFlags);
            writer.WriteByte(InstanceAge);
            writer.WriteByte(BoundAge);
            writer.WriteByte(PropsAge);
            writer.WriteByte(RefCount);
            writer.WriteByte(ParentChildIndex);
            writer.WriteUInt32(Flags);
            writer.WriteByte(InnerPropsInstanciated ? (byte)1 : (byte)0);
            writer.WriteByte(InnerPropsAge);
            writer.WriteByte(RawPropsGroup);
            writer.WriteByte(GroupFileFlags);
            writer.WriteArr(BoundInstances);
            writer.WriteUInt64(NamedNodeMap);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = new Rsc6Str(reader.ReadString("Name"));
            PropsGroup = reader.ReadBool("PropsGroup");
            ScopedNameHash = new JenkHash(reader.ReadString("ScopedNameHash"));

            for (int i = 0; i < Curves.Length; i++)
            {
                Curves[i] = new(reader.ReadNode<Rsc6CurveGroup>($"Curve{i}"));
            }

            MinAndBoundingRadius = Rpf6Crypto.ToXYZ(reader.ReadVector4("MinAndBoundingRadius"));
            MaxAndInscribedRadius = Rpf6Crypto.ToXYZ(reader.ReadVector4("MaxAndInscribedRadius"));
            BoundMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundMin"));
            BoundMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundMax"));
            PlacedLightsGroup = new(reader.ReadNode<Rsc6PlacedLightsGroup>("PlacedLightsGroup"));
            Entities = new(reader.ReadNodeArray<Rsc6PropInstanceInfo>("Entities"));
            //Attributes = new(reader.ReadNodeArray("Attributes", Rsc6Attribute.Create));

            var dAttr = reader.ReadNodeArray<Rsc6MapAttribute>("DoorsAttributes");
            if (dAttr != null)
            {
                //DoorsAttributes = new(dAttr, (ushort)dAttr.Where(a => a != null).ToArray().Length, (ushort)dAttr.Length);
            }

            ItemMapChilds = new(reader.ReadNodeArray<Rsc6SectorChild>("ItemMapChilds"));
            ItemChilds = new(reader.ReadNode<Rsc6ScopedSectors>("ItemChilds"));
            ChildPtrs = new(reader.ReadNodeArray<Rsc6SectorInfo>("ChildPtrs"));
            //DrawableInstances = new(reader.ReadNodeArray<Rsc6DrawableInstanceBase>("DrawableInstances"));
            Unknown_104h = new(reader.ReadNodeArray<Rsc6DrawableInstance>("Unknown_104h"));
            Portals = new(reader.ReadNodeArray<Rsc6Portal>("Portals"));
            SectorNameLower = new JenkHash(reader.ReadString("SectorNameLower"));
            Occluders = new(reader.ReadNodeArray<Rsc6Portal>("Occluders"));
            ResidentStatus = reader.ReadUInt32("ResidentStatus");

            var propNames = reader.ReadStringArray("PropNames");
            if (propNames != null)
            {
                var propStr = new Rsc6Str[propNames.Length];
                for (int i = 0; i < propStr.Length; i++)
                {
                    propStr[i] = new Rsc6Str((propNames[i] == string.Empty) ? null : propNames[i]);
                }
                PropNames = new(propStr);
            }

            Locators = new(reader.ReadNodeArray<Rsc6LocatorStatic>("Locators"));
            AnyHighInstanceLoaded = reader.ReadBool("AnyHighInstanceLoaded");
            HasVLowLODResource = reader.ReadBool("HasVLowLODResource");
            Scope = new(reader.ReadString("Scope"));
            District = reader.ReadBool("District");
            RefCount = reader.ReadByte("RefCount");
            Flags = reader.ReadUInt32("Flags");
            BoundInstances = new(reader.ReadNodeArray<Rsc6BoundInstance>("BoundInstances"));
            NamedNodeMap = reader.ReadUInt64("NamedNodeMap");

            if (Attributes.Items != null && DoorsAttributes.Items != null)
            {
                for (int i = 0; i < DoorsAttributes.Count; i++)
                {
                    var d = DoorsAttributes[i];
                    if (d == null) continue;

                    var target = d.Target.Item;
                    if (target == null || target.ToString() == string.Empty) continue; 

                    foreach (var attr in Attributes.Items)
                    {
                        if (attr.ToString() == target.ToString())
                        {
                            d.AttrTarget = attr;
                            break;
                        }
                    }
                }
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            if (Name.Value != null) writer.WriteString("Name", Name.Value);
            writer.WriteBool("PropsGroup", PropsGroup);
            if (ScopedNameHash != 0) writer.WriteString("ScopedNameHash", ScopedNameHash.ToString());

            for (int i = 0; i < Curves.Length; i++)
            {
                writer.WriteNode($"Curve{i}", Curves[i].Item);
            }

            writer.WriteVector4("MinAndBoundingRadius", MinAndBoundingRadius);
            writer.WriteVector4("MaxAndInscribedRadius", MaxAndInscribedRadius);
            writer.WriteVector4("BoundMin", BoundMin);
            writer.WriteVector4("BoundMax", BoundMax);
            writer.WriteNode("PlacedLightsGroup", PlacedLightsGroup.Item);
            if (Entities.Count > 0) writer.WriteNodeArray("Entities", Entities.Items);
            if (Attributes.Capacity > 0) writer.WriteNodeArray("DoorsAttributes", DoorsAttributes.Items);
            if (ItemMapChilds.Count > 0) writer.WriteNodeArray("ItemMapChilds", ItemMapChilds.Items);
            writer.WriteNode("ItemChilds", ItemChilds.Item);
            if (ChildPtrs.Count > 0) writer.WriteNodeArray("ChildPtrs", ChildPtrs.Items);
            if (DrawableInstances.Count > 0) writer.WriteNodeArray("DrawableInstances", DrawableInstances.Items);
            if (Unknown_104h.Count > 0) writer.WriteNodeArray("Unknown_104h", Unknown_104h.Items);
            if (Portals.Count > 0) writer.WriteNodeArray("Portals", Portals.Items);
            if (Attributes.Count > 0) writer.WriteNodeArray("Attributes", Attributes.Items);
            writer.WriteString("SectorNameLower", SectorNameLower.ToString());
            if (Occluders.Count > 0) writer.WriteNodeArray("Occluders", Occluders.Items);
            writer.WriteUInt32("ResidentStatus", ResidentStatus);
            if (PropNames.Count > 0) writer.WriteStringArray("PropNames", PropNames.Items.Select(s => s.Value).ToArray());
            if (Locators.Count > 0) writer.WriteNodeArray("Locators", Locators.Items);
            writer.WriteBool("AnyHighInstanceLoaded", AnyHighInstanceLoaded);
            writer.WriteBool("HasVLowLODResource", HasVLowLODResource);
            writer.WriteString("Scope", Scope.Value);
            writer.WriteBool("District", District);
            writer.WriteByte("RefCount", RefCount);
            writer.WriteUInt32("Flags", Flags);
            if (BoundInstances.Count > 0) writer.WriteNodeArray("BoundInstances", BoundInstances.Items);
            if (NamedNodeMap != 0) writer.WriteUInt64("NamedNodeMap", NamedNodeMap);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6ScopedSectors : Rsc6BlockBase, MetaNode //sagScopedSectors
    {
        public override ulong BlockLength => 28;
        public Rsc6PtrArr<Rsc6SectorInfo> Sectors { get; set; } //m_ScopedSectors
        public Rsc6ManagedArr<Rsc6ScopedSectorParent> SectorsParents { get; set; } //m_ScopedSectorsParents
        public Rsc6RawArr<ushort> SectorsIndices { get; set; } //m_ScopedSectorsIndices
        public ushort IndicesCount { get; set; }
        public ushort IndicesCapacity { get; set; }
        public Rsc6Str Name { get; set; } //m_ScopeName

        public List<string> ParentsNames; //For writing purposes

        public override void Read(Rsc6DataReader reader)
        {
            Sectors = reader.ReadPtrArr<Rsc6SectorInfo>();
            SectorsParents = reader.ReadArr<Rsc6ScopedSectorParent>();
            SectorsIndices = reader.ReadRawArrPtr<ushort>();
            IndicesCount = reader.ReadUInt16();
            IndicesCapacity = reader.ReadUInt16();
            Name = reader.ReadStr();
            SectorsIndices = reader.ReadRawArrItems(SectorsIndices, IndicesCount);

            for (int i = 0; i < SectorsParents.Count; i++)
            {
                var items = Sectors.Items;
                var parents = SectorsParents.Items;

                if (items[i].FilePosition <= 0 || parents[i].ParentPointer <= 0) continue;
                for (int i1 = 0; i1 < items.Length; i1++)
                {
                    if (items[i1].FilePosition != parents[i].ParentPointer) continue;
                    parents[i].Parent = items[i1];
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            if (ParentsNames != null) //When reading xml, project explorer skips this part
            {
                var parentsPos = new Rsc6ScopedSectorParent[ParentsNames.Count];
                for (int i = 0; i < ParentsNames.Count; i++)
                {
                    if (i == 0)
                    {
                        parentsPos[i] = new Rsc6ScopedSectorParent();
                        continue;
                    }

                    parentsPos[i] = new Rsc6ScopedSectorParent()
                    {
                        Parent = Sectors.Items.FirstOrDefault(p => p.Name.Value.ToLower() == ParentsNames[i].ToLower())
                    };
                }
                SectorsParents = new Rsc6ManagedArr<Rsc6ScopedSectorParent>(parentsPos);
            }

            writer.WritePtrArr(Sectors);
            writer.WriteArr(SectorsParents);
            writer.WriteRawArr(SectorsIndices);
            writer.WriteUInt16(IndicesCount);
            writer.WriteUInt16(IndicesCapacity);
            writer.WriteStr(Name);
        }

        public void Read(MetaNodeReader reader)
        {
            Sectors = new(reader.ReadNodeArray<Rsc6SectorInfo>("Sectors"));
            ParentsNames = new(reader.ReadStringArray("SectorsParents").ToList());
            Name = new Rsc6Str(reader.ReadString("Name"));

            if (Sectors.Items != null && Sectors.Items.Length > 1)
            {
                var parents = new List<string>();
                var indices = new List<ushort>() { 0 }; //Prepend root

                for (int i = 1; i < ParentsNames.Count; i++)
                {
                    var p = ParentsNames[i].ToLower();
                    var parentSector = Sectors.Items.FirstOrDefault(s => s.Name.ToString().ToLower() == p);
                    var count = parents.Count(item => item == p);
                    parents.Add(p);
                    indices.Add((ushort)count);
                }

                SectorsIndices = new Rsc6RawArr<ushort>(indices.ToArray());
                IndicesCount = (ushort)SectorsIndices.Items.Length;
                IndicesCapacity = (ushort)SectorsIndices.Items.Length;
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Sectors", Sectors.Items);
            writer.WriteStringArray("SectorsParents", SectorsParents.Items.Select(s => s.Parent?.Name.Value)
                                                                          .Where(value => value != null)
                                                                          .Prepend("root")
                                                                          .ToArray());
            writer.WriteString("Name", Name.Value);
        }
        
        public override string ToString()
        {
            return Name.Value;
        }
    }

    [TC(typeof(EXP))] public class Rsc6ScopedSectorParent : Rsc6BlockBase
    {
        public override ulong BlockLength => 4;
        public uint ParentPointer { get; set; }
        public Rsc6SectorInfo Parent { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            ParentPointer = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            if (Parent == null)
                writer.WriteUInt32((uint)Rpf6Crypto.VIRTUAL_BASE);
            else
                writer.WritePtrEmbed(Parent, Parent, 0);
        }

        public override string ToString()
        {
            return Parent?.ToString() ?? "root";
        }
    }

    [TC(typeof(EXP))] public class Rsc6DrawableInstanceBase : Rsc6FileBase, MetaNode //rdrDrawableInstanceBase
    {
        public override ulong BlockLength => 224;
        public override uint VFT { get; set; } = 0x01913300;
        public float TimeLastVisible { get; set; } //m_TimeLastVisible
        public ulong Unknown_8h { get; set; } //Always 0
        public Vector4 LastKnownPositionAndFlags { get; set; } //m_LastKnownPositionAndFlags
        public int Node { get; set; } //m_Node
        public int AtDNode { get; set; } //atDNode
        public int Next { get; set; } //Next
        public int Prev { get; set; } //Prev
        public uint LastFrameRendered { get; set; } //m_LastFrameRendered
        public short MatrixSetIndex { get; set; } //m_MatrixSetIndex
        public byte SPURenderable { get; set; } //m_SPURenderable
        public byte NodeType { get; set; } //m_NodeType
        public int VisibilityFlag { get; set; } //m_VisibilityFlag
        public int BucketFlag { get; set; } //m_BucketFlag
        public Matrix4x4 Matrix { get; set; } //m_Mtx
        public Vector4 BoundingBoxMin { get; set; } //m_BoundingBoxMin
        public Vector4 BoundingBoxMax { get; set; } //m_BoundingBoxMax
        public JenkHash InstanceHash { get; set; } //m_InstHashCode
        public int RoomBits { get; set; } //m_RoomBits
        public uint Elements { get; set; } //m_Elements, rage::datSerializeWrite::Add<rage::atFixedBitSet<32, unsigned int>>
        public ushort Type { get; set; } //m_Type
        public byte Rooms { get; set; } //m_Rooms
        public byte Unknown_AF { get; set; } //0, 2 or 3
        public short Flags { get; set; } //m_Flags
        public byte AO { get; set; } //m_AO
        public byte Pad { get; set; } //m_Pad
        public uint DebugVisibility { get; set; } = 0xCDCDCDCD; //m_DebugVisibility
        public Rsc6Str Name { get; set; } //m_Name
        public Rsc6Ptr<Rsc6Room> Room { get; set; } //m_Room
        public int Drawable { get; set; } //m_Drawable, always 0
        public uint Unknown_C4h { get; set; } //Always 0
        public uint Unknown_C8h { get; set; } //Always 0
        public uint Unknown_CCh { get; set; } //Always 0
        public uint Unknown_D0h { get; set; } //Always 0
        public uint NextDrawableOffset { get; set; } //m_NextDrawableForBound
        public uint Unknown_D8h { get; set; } //Always 0
        public uint Unknown_DCh { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TimeLastVisible = reader.ReadSingle();
            Unknown_8h = reader.ReadUInt64();
            LastKnownPositionAndFlags = reader.ReadVector4();
            Node = reader.ReadInt32();
            AtDNode = reader.ReadInt32();
            Next = reader.ReadInt32();
            Prev = reader.ReadInt32();
            LastFrameRendered = reader.ReadUInt32();
            MatrixSetIndex = reader.ReadInt16();
            SPURenderable = reader.ReadByte();
            NodeType = reader.ReadByte();
            VisibilityFlag = reader.ReadInt32();
            BucketFlag = reader.ReadInt32();
            Matrix = reader.ReadMatrix4x4();
            BoundingBoxMin = reader.ReadVector4();
            BoundingBoxMax = reader.ReadVector4();
            InstanceHash = reader.ReadUInt32();
            RoomBits = reader.ReadInt32();
            Elements = reader.ReadUInt32(); //rage::atFixedBitSet
            Type = reader.ReadUInt16();
            Rooms = reader.ReadByte();
            Unknown_AF = reader.ReadByte();
            Flags = reader.ReadInt16();
            AO = reader.ReadByte();
            Pad = reader.ReadByte();
            DebugVisibility = reader.ReadUInt32();
            Name = reader.ReadStr();
            Room = reader.ReadPtr<Rsc6Room>();
            Drawable = reader.ReadInt32();
            Unknown_C4h = reader.ReadUInt32();
            Unknown_C8h = reader.ReadUInt32();
            Unknown_CCh = reader.ReadUInt32();
            Unknown_D0h = reader.ReadUInt32();
            NextDrawableOffset = reader.ReadUInt32();
            Unknown_D8h = reader.ReadUInt32();
            Unknown_DCh = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle(TimeLastVisible);
            writer.WriteUInt64(Unknown_8h);
            writer.WriteVector4(LastKnownPositionAndFlags);
            writer.WriteInt32(Node);
            writer.WriteInt32(AtDNode);
            writer.WriteInt32(Next);
            writer.WriteInt32(Prev);
            writer.WriteUInt32(LastFrameRendered);
            writer.WriteInt16(MatrixSetIndex);
            writer.WriteByte(SPURenderable);
            writer.WriteByte(NodeType);
            writer.WriteInt32(VisibilityFlag);
            writer.WriteInt32(BucketFlag);
            writer.WriteMatrix4x4(Matrix);
            writer.WriteVector4(BoundingBoxMin);
            writer.WriteVector4(BoundingBoxMax);
            writer.WriteUInt32(InstanceHash);
            writer.WriteInt32(RoomBits);
            writer.WriteUInt32(Elements);
            writer.WriteUInt16(Type);
            writer.WriteByte(Rooms);
            writer.WriteByte(Unknown_AF);
            writer.WriteInt16(Flags);
            writer.WriteByte(AO);
            writer.WriteByte(Pad);
            writer.WriteUInt32(DebugVisibility);
            writer.WriteStr(Name);
            writer.WritePtr(Room);
            writer.WriteInt32(Drawable);
            writer.WriteUInt32(Unknown_C4h);
            writer.WriteUInt32(Unknown_C8h);
            writer.WriteUInt32(Unknown_CCh);
            writer.WriteUInt32(Unknown_D0h);
            writer.WriteUInt32(NextDrawableOffset);
            writer.WriteUInt32(Unknown_D8h);
            writer.WriteUInt32(Unknown_DCh);
        }

        public void Read(MetaNodeReader reader)
        {
            TimeLastVisible = reader.ReadSingle("TimeLastVisible");
            LastKnownPositionAndFlags = Rpf6Crypto.ToXYZ(reader.ReadVector4("LastKnownPositionAndFlags"));
            Node = reader.ReadInt32("Node");
            AtDNode = reader.ReadInt32("AtDNode");
            Next = reader.ReadInt32("Next");
            Prev = reader.ReadInt32("Prev");
            LastFrameRendered = reader.ReadUInt32("LastFrameRendered");
            MatrixSetIndex = reader.ReadInt16("MatrixSetIndex");
            SPURenderable = reader.ReadByte("SPURenderable");
            NodeType = reader.ReadByte("NodeType");
            VisibilityFlag = reader.ReadInt32("VisibilityFlag");
            BucketFlag = reader.ReadInt32("BucketFlag");
            Matrix = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("Matrix"), true);
            BoundingBoxMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxMin"));
            BoundingBoxMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxMax"));
            RoomBits = reader.ReadInt32("RoomBits");
            Elements = reader.ReadUInt32("Elements");
            Type = reader.ReadUInt16("Type");
            Rooms = reader.ReadByte("Rooms");
            Unknown_AF = reader.ReadByte("Unknown_AF");
            Flags = reader.ReadInt16("Flags");
            AO = reader.ReadByte("AO");
            Pad = reader.ReadByte("Pad");
            DebugVisibility = reader.ReadUInt32("DebugVisibility");
            Name = new Rsc6Str(reader.ReadString("Name"));
            Room = new(reader.ReadNode<Rsc6Room>("Room"));
            Drawable = reader.ReadInt32("Drawable");
            InstanceHash = new(Name.Value);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteSingle("TimeLastVisible", TimeLastVisible);
            writer.WriteVector4("LastKnownPositionAndFlags", LastKnownPositionAndFlags);
            writer.WriteInt32("Node", Node);
            writer.WriteInt32("AtDNode", AtDNode);
            writer.WriteInt32("Next", Next);
            writer.WriteInt32("Prev", Prev);
            writer.WriteUInt32("LastFrameRendered", LastFrameRendered);
            writer.WriteInt16("MatrixSetIndex", MatrixSetIndex);
            writer.WriteByte("SPURenderable", SPURenderable);
            writer.WriteByte("NodeType", NodeType);
            writer.WriteInt32("VisibilityFlag", VisibilityFlag);
            writer.WriteInt32("BucketFlag", BucketFlag);
            writer.WriteMatrix4x4("Matrix", Matrix);
            writer.WriteVector4("BoundingBoxMin", BoundingBoxMin);
            writer.WriteVector4("BoundingBoxMax", BoundingBoxMax);
            writer.WriteInt32("RoomBits", RoomBits);
            writer.WriteUInt32("Elements", Elements);
            writer.WriteUInt16("Type", Type);
            writer.WriteByte("Rooms", Rooms);
            writer.WriteByte("Unknown_AF", Unknown_AF);
            writer.WriteInt16("Flags", Flags);
            writer.WriteByte("AO", AO);
            writer.WriteByte("Pad", Pad);
            writer.WriteUInt32("DebugVisibility", DebugVisibility);
            writer.WriteString("Name", Name.Value);
            writer.WriteNode("Room", Room.Item);
            writer.WriteInt32("Drawable", Drawable);
        }

        public override string ToString()
        {
            return Name.Value;
        }
    }

    [TC(typeof(EXP))] public class Rsc6Room : Rsc6FileBase, MetaNode //rage::rmpRoom + rdrRoom
    {
        public override ulong BlockLength => 144;
        public override uint VFT { get; set; } = 0x019130E0;
        public Rsc6Str Name { get; set; } //m_ReverbName
        public Rsc6Str RscName { get; set; } //m_DebugRscName
        public uint AddMarker { get; set; } = 0xFFFFFFFF; //m_AddMarker
        public Vector4 Bounds { get; set; } //m_Bounds
        public Matrix4x4 Matrix { get; set; } //m_Matrix
        public Rsc6ManagedArr<Rsc6Polygon> Polygons { get; set; } //m_Polygons
        public Rsc6Arr<ushort> StaticObjects { get; set; } //m_StaticObjects
        public Rsc6PtrArr<Rsc6Portal> Portals { get; set; } //m_Portals
        public uint Flags { get; set; } //m_Flags - 0, 2, 4, 20 or 22
        public uint Unknown_7Ch { get; set; } //Always 0
        public Rsc6Str ReverbName { get; set; } //m_ReverbName, start of rdrRoom
        public int VisibleTimestamp { get; set; } //m_VisibleTimestamp
        public uint StreamingVolume { get; set; } //m_StreamingVolume, always 0
        public uint Unknown_8Ch { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadStr();
            RscName = reader.ReadStr();
            AddMarker = reader.ReadUInt32();
            Bounds = reader.ReadVector4();
            Matrix = reader.ReadMatrix4x4();
            Polygons = reader.ReadArr<Rsc6Polygon>();
            StaticObjects = reader.ReadArr<ushort>();
            Portals = reader.ReadPtrArr<Rsc6Portal>();
            Flags = reader.ReadUInt32();
            Unknown_7Ch = reader.ReadUInt32();
            ReverbName = reader.ReadStr(); //rage::rdrRoom
            VisibleTimestamp = reader.ReadInt32();
            StreamingVolume = reader.ReadUInt32(); //rdrStreamingVolume
            Unknown_8Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteStr(Name);
            writer.WriteStr(RscName);
            writer.WriteUInt32(AddMarker);
            writer.WriteVector4(Bounds);
            writer.WriteMatrix4x4(Matrix);
            writer.WriteArr(Polygons);
            writer.WriteArr(StaticObjects);
            writer.WritePtrArr(Portals);
            writer.WriteUInt32(Flags);
            writer.WriteUInt32(Unknown_7Ch);
            writer.WriteStr(ReverbName);
            writer.WriteInt32(VisibleTimestamp);
            writer.WriteUInt32(StreamingVolume);
            writer.WriteUInt32(Unknown_8Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = new Rsc6Str(reader.ReadString("Name"));
            RscName = new Rsc6Str(reader.ReadString("RscName"));
            AddMarker = reader.ReadUInt32("AddMarker");
            Bounds = Rpf6Crypto.ToXYZ(reader.ReadVector4("Bounds"));
            Matrix = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("Matrix"), true);
            Polygons = new(reader.ReadNodeArray<Rsc6Polygon>("Polygons"));
            StaticObjects = new(reader.ReadUInt16Array("StaticObjects"));
            Portals = new(reader.ReadNodeArray<Rsc6Portal>("Portals"));
            Flags = reader.ReadUInt32("Flags");
            ReverbName = new Rsc6Str(reader.ReadString("ReverbName"));
            VisibleTimestamp = reader.ReadInt32("VisibleTimestamp");
            StreamingVolume = reader.ReadUInt32("StreamingVolume");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", Name.Value);
            writer.WriteString("RscName", RscName.Value);
            writer.WriteUInt32("AddMarker", AddMarker);
            writer.WriteVector4("Bounds", Bounds);
            writer.WriteMatrix4x4("Matrix", Matrix);
            writer.WriteNodeArray("Polygons", Polygons.Items);
            writer.WriteUInt16Array("StaticObjects", StaticObjects.Items);
            writer.WriteNodeArray("Portals", Portals.Items);
            writer.WriteUInt32("Flags", Flags);
            writer.WriteString("ReverbName", ReverbName.Value);
            writer.WriteInt32("VisibleTimestamp", VisibleTimestamp);
            writer.WriteUInt32("StreamingVolume", StreamingVolume);
        }

        public override string ToString()
        {
            return Name.Value;
        }
    }

    [TC(typeof(EXP))] public class Rsc6Portal : Rsc6BlockBase, MetaNode //rage::rmpPortal
    {
        public override ulong BlockLength => 224;
        public Rsc6Polygon Polygons { get; set; } = new Rsc6Polygon();
        public Matrix4x4 Matrix { get; set; } //m_Matrix, rage::Matrix34
        public Vector3 Bounds { get; set; } //rage::rmpBound
        public float Radius { get; set; } //rage::spdSphere
        public int FrontRoom { get; set; } //m_FrontRoom
        public int BackRoom { get; set; } //m_BackRoom
        public Rsc6Str Name { get; set; } //m_DebugName
        public bool Opened { get; set; } //m_Opened
        public bool Interior { get; set; } //m_Interior
        public uint Unknown_CE { get; set; } //Always 0?
        public ushort Unknown_D2 { get; set; } //Always 0?
        public Rsc6Ptr<Rsc6PropInstanceInfo> PropInstance { get; set; } //m_PropInstance
        public uint Unknown_D8 { get; set; } //Always 0?
        public uint Unknown_DC { get; set; } //Padding

        public override void Read(Rsc6DataReader reader)
        {
            Polygons.Read(reader); //rage::rmpPortal
            Matrix = reader.ReadMatrix4x4();
            Bounds = reader.ReadVector3();
            Radius = reader.ReadSingle();
            FrontRoom = reader.ReadInt32();
            BackRoom = reader.ReadInt32();
            Name = reader.ReadStr();
            Opened = reader.ReadBoolean();
            Interior = reader.ReadBoolean();
            Unknown_CE = reader.ReadUInt32(); //rage::rdrPortal
            Unknown_D2 = reader.ReadUInt16();
            PropInstance = reader.ReadPtr<Rsc6PropInstanceInfo>();
            Unknown_D8 = reader.ReadUInt32();
            Unknown_DC = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            Polygons.Write(writer);
            writer.WriteMatrix4x4(Matrix);
            writer.WriteVector3(Bounds);
            writer.WriteSingle(Radius);
            writer.WriteInt32(FrontRoom);
            writer.WriteInt32(BackRoom);
            writer.WriteStr(Name);
            writer.WriteBoolean(Opened);
            writer.WriteBoolean(Interior);
            writer.WriteUInt32(Unknown_CE);
            writer.WriteUInt16(Unknown_D2);
            writer.WritePtr(PropInstance);
            writer.WriteUInt32(Unknown_D8);
            writer.WriteUInt32(Unknown_DC);
        }

        public void Read(MetaNodeReader reader)
        {
            Polygons = new Rsc6Polygon();
            Polygons.Read(reader);
            Matrix = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("Matrix"), true);
            Bounds = Rpf6Crypto.ToXYZ(reader.ReadVector3("Bounds"));
            Radius = reader.ReadSingle("Radius");
            FrontRoom = reader.ReadInt32("FrontRoom");
            BackRoom = reader.ReadInt32("BackRoom");
            Name = new Rsc6Str(reader.ReadString("Name"));
            Opened = reader.ReadBool("Opened");
            Interior = reader.ReadBool("Interior");
            PropInstance = new(reader.ReadNode<Rsc6PropInstanceInfo>("PropInstance"));
        }

        public void Write(MetaNodeWriter writer)
        {
            Polygons.Write(writer);
            writer.WriteMatrix4x4("Matrix", Matrix);
            writer.WriteVector3("Bounds", Bounds);
            writer.WriteSingle("Radius", Radius);
            writer.WriteInt32("FrontRoom", FrontRoom);
            writer.WriteInt32("BackRoom", BackRoom);
            writer.WriteString("Name", Name.Value);
            writer.WriteBool("Opened", Opened);
            writer.WriteBool("Interior", Interior);
            writer.WriteNode("PropInstance", PropInstance.Item);
        }

        public override string ToString()
        {
            return Name.Value;
        }
    }

    [TC(typeof(EXP))] public class Rsc6DrawableInstance : Rsc6BlockBaseMap, MetaNode
    {
        public override ulong BlockLength => 144;
        public override uint VFT { get; set; } = 0x0198EFC0;
        public uint Unknown_8h { get; set; } //Always 0
        public uint Unknown_Ch { get; set; } //Always 0
        public ushort Unknown_10h { get; set; }
        public ushort Unknown_12h { get; set; }
        public int Unknown_14h { get; set; } = -1; //Always -1
        public uint Unknown_18h { get; set; } //Always 0
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD; //Always 0xCDCDCDCD
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; } //Pointer to VFT (start of the struct)
        public short Unknown_28h { get; set; } = -1; //Always -1
        public ushort Unknown_2Ah { get; set; } //Always 0
        public uint Unknown_2Ch { get; set; } //Pointer to Unknown_70h
        public Matrix4x4 Unknown_30h { get; set; }
        public uint Unknown_70h { get; set; } = 0x018E3C34; //26098740 or 11199752
        public uint Unknown_74h { get; set; } //Pointer to VFT (start of the struct)
        public uint Unknown_78h { get; set; } //Pointer to Unknown_20h
        public ushort Unknown_7Ch { get; set; } = 2; //Always 2
        public short Unknown_7Eh { get; set; } = -1; //Always -1
        public uint Unknown_80h { get; set; } //Always 0
        public uint Unknown_84h { get; set; } //Always 0
        public JenkHash Unknown_88h { get; set; }
        public Rsc6Ptr<Rsc6DrawableInstanceBase> Unknown_8Ch { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            Unknown_10h = reader.ReadUInt16();
            Unknown_12h = reader.ReadUInt16();
            Unknown_14h = reader.ReadInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadInt16();
            Unknown_2Ah = reader.ReadUInt16();
            Unknown_2Ch = reader.ReadUInt32();
            Unknown_30h = reader.ReadMatrix4x4();
            Unknown_70h = reader.ReadUInt32();
            Unknown_74h = reader.ReadUInt32();
            Unknown_78h = reader.ReadUInt32();
            Unknown_7Ch = reader.ReadUInt16();
            Unknown_7Eh = reader.ReadInt16();
            Unknown_80h = reader.ReadUInt32();
            Unknown_84h = reader.ReadUInt32();
            Unknown_88h = reader.ReadUInt32();
            Unknown_8Ch = reader.ReadPtr<Rsc6DrawableInstanceBase>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteUInt16(Unknown_10h);
            writer.WriteUInt16(Unknown_12h);
            writer.WriteInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(Unknown_1Ch);
            writer.WriteUInt32(Unknown_20h);
            writer.WritePtrEmbed(this, this, 0); //Unknown_24h
            writer.WriteInt16(Unknown_28h);
            writer.WriteUInt16(Unknown_2Ah);
            writer.WritePtrEmbed(this, this, 0x70); //Unknown_2Ch
            writer.WriteMatrix4x4(Unknown_30h);
            writer.WriteUInt32(Unknown_70h);
            writer.WritePtrEmbed(this, this, 0); //Unknown_74h
            writer.WritePtrEmbed(this, this, 0x20); //Unknown_78h
            writer.WriteUInt16(Unknown_7Ch);
            writer.WriteInt16(Unknown_7Eh);
            writer.WriteUInt32(Unknown_80h);
            writer.WriteUInt32(Unknown_84h);
            writer.WriteUInt32(Unknown_88h);
            writer.WritePtr(Unknown_8Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Unknown_10h = reader.ReadUInt16("Unknown_10h");
            Unknown_12h = reader.ReadUInt16("Unknown_12h");
            Unknown_20h = reader.ReadUInt32("Unknown_20h");
            Unknown_30h = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("Unknown_30h"), true);
            Unknown_88h = reader.ReadJenkHash("Unknown_88h");
            Unknown_8Ch = new(reader.ReadNode<Rsc6DrawableInstanceBase>("Unknown_8Ch"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt16("Unknown_10h", Unknown_10h);
            writer.WriteUInt16("Unknown_12h", Unknown_12h);
            writer.WriteUInt32("Unknown_20h", Unknown_20h);
            writer.WriteMatrix4x4("Unknown_30h", Unknown_30h);
            writer.WriteJenkHash("Unknown_88h", Unknown_88h);
            writer.WriteNode("Unknown_8Ch", Unknown_8Ch.Item);
        }
    }

    [TC(typeof(EXP))] public class Rsc6Polygon : Rsc6FileBase, MetaNode //rage::spdPolygon
    {
        public override ulong BlockLength => 112;
        public override uint VFT { get; set; } = 0x01909C10;
        public int EdgeVisFrame { get; set; } //Always -1
        public int Data { get; set; } = -1; //Always -1
        public int RefNumber { get; set; } = -1; //Always -1
        public int PolyNode { get; set; } //m_Node, start of rage::spdPolyNode
        public int AtDNode { get; set; } //Always 0
        public int Next { get; set; } //Always 0
        public int Prev { get; set; } //Offset of the current spdPolyNode
        public int Polygon { get; set; } //Always 0
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Padding
        public Vector4 PlaneCoeffs { get; set; } //m_Plane, start of rage::spdPlane
        public Vector4 Center { get; set; } //m_Center
        public Rsc6Arr<Vector4> Points { get; set; } //m_Points
        public Rsc6Arr<byte> VisibleEdges { get; set; } //m_VisibleEdges, capacity constantly set to 16
        public bool SingleSided { get; set; } //m_SingleSided
        public uint Unknown_61h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_65h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_69h { get; set; } = 0xCDCDCDCD; //Padding
        public ushort Unknown_6Dh { get; set; } = 0xCDCD; //Padding
        public byte Unknown_6Fh { get; set; } = 0xCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            EdgeVisFrame = reader.ReadInt32();
            Data = reader.ReadInt32();
            RefNumber = reader.ReadInt32();
            PolyNode = reader.ReadInt32();
            AtDNode = reader.ReadInt32();
            Next = reader.ReadInt32();
            Prev = reader.ReadInt32();
            Polygon = reader.ReadInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
            PlaneCoeffs = reader.ReadVector4();
            Center = reader.ReadVector4();
            Points = reader.ReadArr<Vector4>();
            VisibleEdges = reader.ReadArr<byte>();
            SingleSided = reader.ReadBoolean();
            Unknown_61h = reader.ReadUInt32();
            Unknown_65h = reader.ReadUInt32();
            Unknown_69h = reader.ReadUInt32();
            Unknown_6Dh = reader.ReadUInt16();
            Unknown_6Fh = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteInt32(EdgeVisFrame);
            writer.WriteInt32(Data);
            writer.WriteInt32(RefNumber);
            writer.WriteInt32(PolyNode);
            writer.WriteInt32(AtDNode);
            writer.WriteInt32(Next);
            writer.WritePtrEmbed(this, this, 0); //Prev
            writer.WriteInt32(Polygon);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
            writer.WriteVector4(PlaneCoeffs);
            writer.WriteVector4(Center);
            writer.WriteArr(Points);
            writer.WriteArr(VisibleEdges);
            writer.WriteByte(SingleSided ? (byte)1 : (byte)0);
            writer.WriteUInt32(Unknown_61h);
            writer.WriteUInt32(Unknown_65h);
            writer.WriteUInt32(Unknown_69h);
            writer.WriteUInt16(Unknown_6Dh);
            writer.WriteByte(Unknown_6Fh);
        }

        public void Read(MetaNodeReader reader)
        {
            EdgeVisFrame = reader.ReadInt32("EdgeVisFrame");
            Data = reader.ReadInt32("Data");
            RefNumber = reader.ReadInt32("RefNumber");
            PolyNode = reader.ReadInt32("PolyNode");
            AtDNode = reader.ReadInt32("AtDNode");
            Next = reader.ReadInt32("Next");
            Prev = reader.ReadInt32("Prev");
            Polygon = reader.ReadInt32("Polygon");
            PlaneCoeffs = Rpf6Crypto.ToXYZ(reader.ReadVector4("PlaneCoeffs"));
            Center = Rpf6Crypto.ToXYZ(reader.ReadVector4("Center"));
            Points = new(Rpf6Crypto.ToXYZ(reader.ReadVector4Array("Points")));
            VisibleEdges = new(reader.ReadByteArray("VisibleEdges"), false, 16);
            SingleSided = reader.ReadBool("SingleSided");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("EdgeVisFrame", EdgeVisFrame);
            writer.WriteInt32("Data", Data);
            writer.WriteInt32("RefNumber", RefNumber);
            writer.WriteInt32("PolyNode", PolyNode);
            writer.WriteInt32("AtDNode", AtDNode);
            writer.WriteInt32("Next", Next);
            writer.WriteInt32("Prev", Prev);
            writer.WriteInt32("Polygon", Polygon);
            writer.WriteVector4("PlaneCoeffs", PlaneCoeffs);
            writer.WriteVector4("Center", Center);
            writer.WriteVector4Array("Points", Points.Items);
            writer.WriteByteArray("VisibleEdges", VisibleEdges.Items);
            writer.WriteBool("SingleSided", SingleSided);
        }
    }

    [TC(typeof(EXP))] public class Rsc6CurveGroup : Rsc6BlockBase, MetaNode //sagCurveArray
    {
        /*
         * There's a block of 24 pointers in each sagSectorInfo that refer to this sagCurveGroup structure
         * Most of the time only 1 or 2 curves are used per sector
         * 'swTerrain', 'swAll' & some other random sectors like 'theCeliaWastes' doesn't have any curves
         * 
         * Curves 12, 13, 14, 21, 22, 23 & 24 are never used
         * Curves 8, 9 & 10 are only used in 'swWater'
         * Curves 5, 6, 18 & 19 are only used in 'swAiCurves'
         * Curve 3 is used in 'blackwater' & some 'points of interest locations'
         * Curve 4 is used in 'swAiCurves' & 'swRailroad'
         * Curve 11 is used in 'cuevaSeco' & 'plataGrande'
         * Curve 20 is used for horse curves resources like 'o17p18_hc'
         * Others are mostly used in hub sectors like towns
         */

        public override ulong BlockLength => 48; //sagCurveGroup + sagCurveArray
        public Vector4 BoundingBoxMin { get; set; } //m_AABBMin
        public Vector4 BoundingBoxMax { get; set; } //m_AABBMax
        public Rsc6ManagedArr<Rsc6Curve> Curves { get; set; } //m_CurveArray
        public int Next { get; set; } //m_Next
        public int ParentLevelIndex { get; set; } //m_ParentLevelIndex

        public override void Read(Rsc6DataReader reader)
        {
            BoundingBoxMin = reader.ReadVector4();
            BoundingBoxMax = reader.ReadVector4();
            Curves = reader.ReadArr<Rsc6Curve>();
            Next = reader.ReadInt32();
            ParentLevelIndex = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(BoundingBoxMin);
            writer.WriteVector4(BoundingBoxMax);
            writer.WriteArr(Curves);
            writer.WriteInt32(Next);
            writer.WriteInt32(ParentLevelIndex);
        }

        public void Read(MetaNodeReader reader)
        {
            BoundingBoxMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxMin"));
            BoundingBoxMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxMax"));
            Curves = new(reader.ReadNodeArray<Rsc6Curve>("Curves"));
            Next = reader.ReadInt32("Next");
            ParentLevelIndex = reader.ReadInt32("ParentLevelIndex");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("BoundingBoxMin", BoundingBoxMin);
            writer.WriteVector4("BoundingBoxMax", BoundingBoxMax);
            writer.WriteNodeArray("Curves", Curves.Items);
            writer.WriteInt32("Next", Next);
            writer.WriteInt32("ParentLevelIndex", ParentLevelIndex);
        }
    }

    [TC(typeof(EXP))] public class Rsc6Curve : Rsc6FileBase, MetaNode //sagCurve
    {
        public override ulong BlockLength => 80; //rage::mayaCurve + sagCurve
        public override uint VFT { get; set; } = 0x0190DAB8;
        public Rsc6RawArr<Vector4> ControlledVertices { get; set; } //m_pCVs
        public uint Unknown_8h { get; set; } = 0x01877240; //25653824 or 10755280, probably VFTs
        public Rsc6RawArr<float> Knots { get; set; } //m_pKnots, maya-style knot vectors (joint points)
        public int KnotsCount { get; set; } //m_Length
        public float TRange { get; set; } //m_TRange, the real t-range of the curve
        public float TMax { get; set; } //m_TMax, the maximum allowed real t value, less than or equal to TRange
        public ushort CVCount { get; set; } //m_N, number of controlled vertices - 1
        public Rsc6MayaCurveForm Form { get; set; } //m_Form
        public byte Degree { get; set; } //m_Degree, polynomial degree of each curve span
        public Vector4 AABBMin { get; set; } //m_MinAndBoundingRadius
        public Vector4 AABBMax { get; set; } //m_MaxAndInscribedRadius
        public Rsc6Str Name { get; set; } //m_Name
        public bool Dynamic { get; set; } //m_Dynamic
        public bool Active { get; set; } //m_Active
        public bool ScriptCreated { get; set; } //m_ScriptCreated
        public byte ScriptDefinedWeight { get; set; } //m_ScriptDefinedWeight
        public short BoneIndex { get; set; } //m_BoneIndex
        public short ParentLevelIndex { get; set; } //m_ParentLevelIndex
        public uint CRC { get; set; } //m_Crc

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ControlledVertices = reader.ReadRawArrPtr<Vector4>();
            Unknown_8h = reader.ReadUInt32();
            Knots = reader.ReadRawArrPtr<float>(); //rage::mayaKnotVector
            KnotsCount = reader.ReadInt32();
            TRange = reader.ReadSingle();
            TMax = reader.ReadSingle();
            CVCount = reader.ReadUInt16();
            Form = (Rsc6MayaCurveForm)reader.ReadByte();
            Degree = reader.ReadByte();
            AABBMin = reader.ReadVector4();
            AABBMax = reader.ReadVector4();
            Name = reader.ReadStr();
            Dynamic = reader.ReadBoolean();
            Active = reader.ReadBoolean();
            ScriptCreated = reader.ReadBoolean();
            ScriptDefinedWeight = reader.ReadByte();
            BoneIndex = reader.ReadInt16();
            ParentLevelIndex = reader.ReadInt16();
            CRC = reader.ReadUInt32();
            ControlledVertices = reader.ReadRawArrItems(ControlledVertices, (uint)CVCount + 1);
            Knots = reader.ReadRawArrItems(Knots, (uint)KnotsCount);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteRawArr(ControlledVertices);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteRawArr(Knots);
            writer.WriteInt32(KnotsCount);
            writer.WriteSingle(TRange);
            writer.WriteSingle(TMax);
            writer.WriteUInt16(CVCount);
            writer.WriteByte((byte)Form);
            writer.WriteByte(Degree);
            writer.WriteVector4(AABBMin);
            writer.WriteVector4(AABBMax);
            writer.WriteStr(Name);
            writer.WriteBoolean(Dynamic);
            writer.WriteBoolean(Active);
            writer.WriteBoolean(ScriptCreated);
            writer.WriteByte(ScriptDefinedWeight);
            writer.WriteInt16(BoneIndex);
            writer.WriteInt16(ParentLevelIndex);
            writer.WriteUInt32(CRC);
        }

        public void Read(MetaNodeReader reader)
        {
            ControlledVertices = new(Rpf6Crypto.ToXYZ(reader.ReadVector4Array("ControlledVertices")));
            Knots = new(reader.ReadSingleArray("Knots"));
            TRange = reader.ReadSingle("TRange");
            TMax = reader.ReadSingle("TMax");
            Form = reader.ReadEnum<Rsc6MayaCurveForm>("Form");
            Degree = reader.ReadByte("Degree");
            AABBMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("AABBMin"));
            AABBMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("AABBMax"));
            Name = new(reader.ReadString("Name"));
            Dynamic = reader.ReadBool("Dynamic");
            Active = reader.ReadBool("Active");
            ScriptCreated = reader.ReadBool("ScriptCreated");
            ScriptDefinedWeight = reader.ReadByte("ScriptDefinedWeight");
            BoneIndex = reader.ReadInt16("BoneIndex");
            ParentLevelIndex = reader.ReadInt16("ParentLevelIndex");
            CRC = reader.ReadUInt32("CRC");

            if (Knots.Items != null)
            {
                KnotsCount = Knots.Items.Length;
            }
            if (ControlledVertices.Items != null)
            {
                CVCount = (ushort)(ControlledVertices.Items.Length - 1);
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4Array("ControlledVertices", ControlledVertices.Items);
            writer.WriteSingleArray("Knots", Knots.Items);
            writer.WriteSingle("TRange", TRange);
            writer.WriteSingle("TMax", TMax);
            writer.WriteEnum("Form", Form);
            writer.WriteByte("Degree", Degree);
            writer.WriteVector4("AABBMin", AABBMin);
            writer.WriteVector4("AABBMax", AABBMax);
            writer.WriteString("Name", Name.ToString());
            writer.WriteBool("Dynamic", Dynamic);
            writer.WriteBool("Active", Active);
            writer.WriteBool("ScriptCreated", ScriptCreated);
            writer.WriteByte("ScriptDefinedWeight", ScriptDefinedWeight);
            writer.WriteInt16("BoneIndex", BoneIndex);
            writer.WriteInt16("ParentLevelIndex", ParentLevelIndex);
            writer.WriteUInt32("CRC", CRC);
        }

        public override string ToString()
        {
            return Name.Value ?? "";
        }
    }

    [TC(typeof(EXP))] public class Rsc6PropInstanceInfo : Rsc6BlockBase, MetaNode //propInstanceInfo
    {
        public override ulong BlockLength => 48;
        public Rsc6Str EntityName { get; set; } //m_TypeName
        public uint Light { get; set; } //m_Light, always 0
        public Half RotationX { get; set; } //m_Rx
        public Half RotationY { get; set; } //m_Ry
        public Half RotationZ { get; set; } //m_Rz
        public byte Flags { get; set; } //m_Flags (0, 65, 66 (mostly), 67, 128, 193, 194, etc)
        public byte AO { get; set; } //m_AO (64, 84, 57, 22, 18, 41, 27, 33, etc)
        public Vector4 EntityPosition { get; set; } //m_Offset
        public uint Unknown_20h { get; set; } //Always 0
        public byte ModMode { get; set; } //m_ModMode
        public byte NetworkingFlags { get; set; } //m_NetworkingFlags
        public byte RotationType { get; set; } //m_RotationType
        public byte ClassTypeAndExtraFlags { get; set; } //m_ClassTypeAndExtraFlags
        public uint PortalOffset { get; set; } //m_Portal
        public uint Unknown_2Ch { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader) //propInstanceInfo
        {
            EntityName = reader.ReadStr();
            Light = reader.ReadUInt32();
            RotationX = reader.ReadHalf();
            RotationY = reader.ReadHalf();
            RotationZ = reader.ReadHalf();
            Flags = reader.ReadByte();
            AO = reader.ReadByte();
            EntityPosition = reader.ReadVector4();
            Unknown_20h = reader.ReadUInt32();
            ModMode = reader.ReadByte();
            NetworkingFlags = reader.ReadByte();
            RotationType = reader.ReadByte();
            ClassTypeAndExtraFlags = reader.ReadByte();
            PortalOffset = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteStr(EntityName);
            writer.WriteUInt32(Light);
            writer.WriteHalf(RotationX);
            writer.WriteHalf(RotationY);
            writer.WriteHalf(RotationZ);
            writer.WriteByte(Flags);
            writer.WriteByte(AO);
            writer.WriteVector4(EntityPosition);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteByte(ModMode);
            writer.WriteByte(NetworkingFlags);
            writer.WriteByte(RotationType);
            writer.WriteByte(ClassTypeAndExtraFlags);
            writer.WriteUInt32(PortalOffset);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            EntityName = new(reader.ReadString("EntityName"));
            RotationX = (Half)reader.ReadSingle("RotationX");
            RotationY = (Half)reader.ReadSingle("RotationY");
            RotationZ = (Half)reader.ReadSingle("RotationZ");
            Flags = reader.ReadByte("Flags");
            AO = reader.ReadByte("AO");
            ModMode = reader.ReadByte("ModMode");
            NetworkingFlags = reader.ReadByte("NetworkingFlags");
            RotationType = reader.ReadByte("RotationType");
            EntityPosition = Rpf6Crypto.ToXYZ(reader.ReadVector4("EntityPosition"));
            PortalOffset = reader.ReadUInt32("PortalOffset");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("EntityName", EntityName.Value);
            writer.WriteSingle("RotationX", (float)RotationX);
            writer.WriteSingle("RotationY", (float)RotationY);
            writer.WriteSingle("RotationZ", (float)RotationZ);
            writer.WriteByte("Flags", Flags);
            writer.WriteByte("AO", AO);
            writer.WriteByte("ModMode", ModMode);
            writer.WriteByte("NetworkingFlags", NetworkingFlags);
            writer.WriteByte("RotationType", RotationType);
            writer.WriteVector4("EntityPosition", EntityPosition);
            writer.WriteUInt32("PortalOffset", PortalOffset);
        }

        public override string ToString()
        {
            return EntityName.Value;
        }
    }

    [TC(typeof(EXP))] public class Rsc6Attribute : Rsc6FileBase, MetaNode //RDR2AttribBase
    {
        public override ulong BlockLength => 16;
        public override uint VFT { get; set; } //TODO: finish writing
        public uint TargetPointer { get; set; } //m_TargetPointer
        public uint TargetType { get; set; } //m_eTargetType, mostly 1, sometimes 3 when ClassID is equal to 2 or 5
        public uint ClassID { get; set; } //m_ClassID, 1, 2 or 5

        public string TargetedProp; //For writing purposes
        public Rsc6AttributeType Type; //For writing purposes

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TargetPointer = reader.ReadUInt32();
            TargetType = reader.ReadUInt32();
            ClassID = reader.ReadUInt32();
            
            if (TargetType == 1 && ClassID == 1)
                Type = Rsc6AttributeType.PROPS_DOORS_1;
            else if (TargetType == 3 && ClassID == 2)
                Type = Rsc6AttributeType.PROPS_GRINGOS;
            else
                Type = Rsc6AttributeType.PROPS_DOORS_2;
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(TargetPointer);
            writer.WriteUInt32(TargetType);
            writer.WriteUInt32(ClassID);
        }

        public void Read(MetaNodeReader reader)
        {
            Type = reader.ReadEnum("@type", Rsc6AttributeType.PROPS_DOORS_1);
            switch (Type)
            {
                case Rsc6AttributeType.PROPS_DOORS_1:
                    TargetType = 1;
                    ClassID = 1;
                    break;
                case Rsc6AttributeType.PROPS_GRINGOS:
                    TargetType = 3;
                    ClassID = 2;
                    break;
                case Rsc6AttributeType.PROPS_DOORS_2:
                    TargetType = 1;
                    ClassID = 2;
                    break;
            }

            if (Type == Rsc6AttributeType.PROPS_DOORS_2)
            {
                ClassID = reader.ReadUInt32("ClassID");
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", Type.ToString());
            if (Type == Rsc6AttributeType.PROPS_DOORS_2)
            {
                writer.WriteUInt32("ClassID", ClassID);
            }
        }

        public static Rsc6Attribute Create(string typeName)
        {
            if (Enum.TryParse(typeName, out Rsc6AttributeType type))
            {
                return Create(type);
            }
            return null;
        }

        public static Rsc6Attribute Create(Rsc6DataReader r)
        {
            r.Position += 8;
            var targetType = r.ReadUInt32();
            var classID = r.ReadUInt32();
            r.Position -= 16;

            switch (classID)
            {
                case 1: return new Rsc6AttributeInstancedProp(); //Prop & doors
                case 2:
                case 5:
                    {
                        if (targetType == 3) return new Rsc6AttributeDrawableInstanceProp(); //Prop & gringos
                        else return new Rsc6AttributeInstancedProp2();
                    }
                default: throw new NotImplementedException("Unknown RDRAttribute type");
            }
        }

        public static Rsc6Attribute Create(Rsc6AttributeType type)
        {
            return type switch
            {
                Rsc6AttributeType.PROPS_DOORS_1 => new Rsc6AttributeInstancedProp(),
                Rsc6AttributeType.PROPS_GRINGOS => new Rsc6AttributeDrawableInstanceProp(),
                Rsc6AttributeType.PROPS_DOORS_2 => new Rsc6AttributeInstancedProp2(),
                _ => throw new NotImplementedException("Unknown RDRAttribute type"),
            };
        }
    }

    [TC(typeof(EXP))] public class Rsc6MapAttribute : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 12;
        public JenkHash TargetHash { get; set; } //The name of the attribute
        public Rsc6Ptr<Rsc6Attribute> Target { get; set; }
        public Rsc6Ptr<Rsc6MapAttribute> Next { get; set; } //Next attribute...

        public Rsc6Attribute AttrTarget; //For writing purposes

        public override void Read(Rsc6DataReader reader)
        {
            TargetHash = reader.ReadUInt32();
            Target = reader.ReadPtr(Rsc6Attribute.Create);
            Next = reader.ReadPtr<Rsc6MapAttribute>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(TargetHash);
            writer.WritePtrEmbed(AttrTarget, AttrTarget, 0);
            writer.WritePtr(Next);
        }

        public void Read(MetaNodeReader reader)
        {
            Target = new(reader.ReadNode("Target", Rsc6Attribute.Create));
            Next = new(reader.ReadNode<Rsc6MapAttribute>("Next"));
            if (Target.Item != null)
            {
                TargetHash = new(Target.Item.ToString());
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("Target", Target.Item);
            writer.WriteNode("Next", Next.Item);
        }
    }

    [TC(typeof(EXP))] public class Rsc6AttributeInstancedProp : Rsc6Attribute, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 16;
        public Rsc6Ptr<Rsc6PropInstanceInfo> TargetProp { get; set; }
        public Rsc6Str Name { get; set; }
        public Vector3 Unknown_14h { get; set; } = new Vector3(-431602080f); //Usually NULL

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadStr();
            Unknown_14h = reader.ReadVector3();

            TargetProp = new Rsc6Ptr<Rsc6PropInstanceInfo> { Position = TargetPointer };
            TargetProp = reader.ReadPtrItem(TargetProp);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            Rsc6PropInstanceInfo target = null;
            var enumerable = writer.BlockList.FirstOrDefault(s => s is Rsc6PropInstanceInfo[]);

            if (enumerable != null && enumerable is Rsc6PropInstanceInfo[] instances)
            {
                for (int i = 0; i < instances.Length; i++)
                {
                    var inst = instances[i];
                    if (inst == null) continue;
                    if (inst.EntityName.ToString() != TargetedProp) continue;
                    target = inst;
                    break;
                }
            }

            writer.WriteUInt32(0x019094C0);
            writer.WritePtrEmbed(target, target, 0);
            writer.WriteUInt32(TargetType);
            writer.WriteUInt32(ClassID);
            writer.WriteStr(Name);
            writer.WriteVector3(Unknown_14h);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            TargetedProp = reader.ReadString("TargetProp");
            Name = new(reader.ReadString("Name"));
            Unknown_14h = Rpf6Crypto.ToXYZ(reader.ReadVector3("Unknown_14h"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteString("TargetProp", TargetProp.Item?.EntityName.ToString());
            writer.WriteString("Name", Name.ToString());

            if (Unknown_14h != new Vector3(-431602080f))
            {
                writer.WriteVector3("Unknown_14h", Unknown_14h);
            }
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6AttributeInstancedProp2 : Rsc6Attribute, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 16;
        public Rsc6Ptr<Rsc6PropInstanceInfo> TargetProp { get; set; }
        public uint Unknown_10h { get; set; }
        public Vector3 Unknown_14h { get; set; } = new Vector3(-431602080f); //Usually NULL

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadVector3();

            TargetProp = new Rsc6Ptr<Rsc6PropInstanceInfo> { Position = TargetPointer };
            TargetProp = reader.ReadPtrItem(TargetProp);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x01909530);
            writer.WritePtr(TargetProp);
            writer.WriteUInt32(TargetType);
            writer.WriteUInt32(ClassID);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteVector3(Unknown_14h);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            TargetedProp = reader.ReadString("TargetProp");
            Unknown_10h = reader.ReadUInt32("Unknown_10h");
            Unknown_14h = Rpf6Crypto.ToXYZ(reader.ReadVector3("Unknown_14h"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteString("TargetProp", TargetProp.Item?.EntityName.ToString());
            writer.WriteUInt32("Unknown_10h", Unknown_10h);

            if (Unknown_14h != new Vector3(-431602080f))
            {
                writer.WriteVector3("Unknown_14h", Unknown_14h);
            }
        }

        public override string ToString()
        {
            return TargetProp.Item?.EntityName.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6AttributeDrawableInstanceProp : Rsc6Attribute, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 32;
        public Rsc6Ptr<Rsc6DrawableInstanceBase> TargetProp { get; set; }
        public JenkHash Name { get; set; }
        public Vector3 Unknown_14h { get; set; } = new Vector3(-431602080f); //NULL or equal to Vector3.Zero
        public uint Unknown_20h { get; set; } //Always 0
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD; //Seems to be a hash? Only used when 'Unknown_28h' and 'Unknown_2Ch' aren't NULL
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //Actually a pointer...
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Actually a pointer...

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadUInt32();
            Unknown_14h = reader.ReadVector3();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();

            TargetProp = new Rsc6Ptr<Rsc6DrawableInstanceBase> { Position = TargetPointer };
            TargetProp = reader.ReadPtrItem(TargetProp);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x01909530);
            writer.WritePtr(TargetProp);
            writer.WriteUInt32(TargetType);
            writer.WriteUInt32(ClassID);
            writer.WriteUInt32(Name);
            writer.WriteVector3(Unknown_14h);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            TargetedProp = reader.ReadString("TargetProp");
            Name = reader.ReadUInt32("Name");
            Unknown_14h = Rpf6Crypto.ToXYZ(reader.ReadVector3("Unknown_14h"));
            Unknown_24h = reader.ReadUInt32("Unknown_24h");
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteString("TargetProp", TargetProp.Item?.Name.ToString());
            writer.WriteUInt32("Name", Name);

            if (Unknown_14h != new Vector3(-431602080f))
            {
                writer.WriteVector3("Unknown_14h", Unknown_14h);
            }
            if (Unknown_24h != 0xCDCDCDCD)
            {
                writer.WriteUInt32("Unknown_24h", Unknown_24h);
            }
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6SectorChild : Rsc6BlockBase, MetaNode //sagSectorChild
    {
        public override ulong BlockLength => 112;
        public BoundingBox SectorBounds { get; set; }
        public Vector4 BoundsMin { get; set; } //m_BoundingBoxMin
        public Vector4 BoundsMax { get; set; } //m_BoundingBoxMax
        public uint Unknown_20h { get; set; } //Always 0
        public string Name { get; set; } //m_Name
        public Rsc6Str Scope { get; set; } //m_Scope
        public uint Unknown_68h { get; set; } //m_String, always 0
        public uint IsImportantLandmark { get; set; } //m_IsImportantLandmark, always 0 except for 'fillMoreTunnel'

        public override void Read(Rsc6DataReader reader)
        {
            BoundsMin = reader.ReadVector4();
            BoundsMax = reader.ReadVector4();
            Unknown_20h = reader.ReadUInt32();
            Name = reader.ReadString();

            while (reader.Position < FilePosition + 0x64)
            {
                reader.ReadByte(); //Padding
            }

            Scope = reader.ReadStr();
            Unknown_68h = reader.ReadUInt32();
            IsImportantLandmark = reader.ReadUInt32();

            //Scaling bounds for map viewer
            var scale = new Vector3(1000.0f);
            SectorBounds = new BoundingBox(BoundsMin.XYZ() - scale, BoundsMax.XYZ() + scale);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            ulong pos = writer.Position;
            writer.WriteVector4(BoundsMin);
            writer.WriteVector4(BoundsMax);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteStringNullTerminated(Name);

            while (writer.Position < pos + 0x64)
            {
                writer.WriteByte(0xCD); //Padding
            }

            writer.WriteStr(Scope);
            writer.WriteUInt32(Unknown_68h);
            writer.WriteUInt32(IsImportantLandmark);
        }

        public void Read(MetaNodeReader reader)
        {
            BoundsMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundsMin"));
            BoundsMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundsMax"));
            Name = reader.ReadString("Name");
            Scope = new Rsc6Str(reader.ReadString("Scope"));
            IsImportantLandmark = reader.ReadUInt32("IsImportantLandmark");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("BoundsMin", BoundsMin);
            writer.WriteVector4("BoundsMax", BoundsMax);
            writer.WriteString("Name", Name);
            writer.WriteString("Scope", Scope.Value);
            writer.WriteUInt32("IsImportantLandmark", IsImportantLandmark);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [TC(typeof(EXP))] public class Rsc6PlacedLightsGroup : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 64;
        public Vector4 BoundsMin { get; set; } //m_AABBMin
        public Vector4 BoundsMax { get; set; } //m_AABBMax
        public Rsc6Str Name { get; set; } //m_Name
        public Rsc6ManagedArr<Rsc6PlacedLight> Lights { get; set; } //m_Lights
        public uint Unknown_2Ch { get; set; } //Always 0
        public uint Unknown_30h { get; set; } //Always 0
        public uint Unknown_34h { get; set; } //Always 0
        public uint Unknown_38h { get; set; } //Always 0
        public uint Unknown_3Ch { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader) //rdrPlacedLightsGroup
        {
            BoundsMin = reader.ReadVector4();
            BoundsMax = reader.ReadVector4();
            Name = reader.ReadStr();
            Lights = reader.ReadArr<Rsc6PlacedLight>();
            Unknown_2Ch = reader.ReadUInt32();
            Unknown_30h = reader.ReadUInt32();
            Unknown_34h = reader.ReadUInt32();
            Unknown_38h = reader.ReadUInt32();
            Unknown_3Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(BoundsMin);
            writer.WriteVector4(BoundsMax);
            writer.WriteStr(Name);
            writer.WriteArr(Lights);
            writer.WriteUInt32(Unknown_2Ch);
            writer.WriteUInt32(Unknown_30h);
            writer.WriteUInt32(Unknown_34h);
            writer.WriteUInt32(Unknown_38h);
            writer.WriteUInt32(Unknown_3Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            BoundsMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundsMin"));
            BoundsMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundsMax"));
            Name = new Rsc6Str(reader.ReadString("Name"));
            Lights = new(reader.ReadNodeArray<Rsc6PlacedLight>("Lights"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("BoundsMin", BoundsMin);
            writer.WriteVector4("BoundsMax", BoundsMax);
            writer.WriteString("Name", Name.Value);
            writer.WriteNodeArray("Lights", Lights.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6PlacedLight : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 160;
        public Vector4 Position { get; set; } //m_Position
        public Vector4 ParentPosition { get; set; }  //m_ParentPosition
        public Half4 Direction { get; set; } //m_Direction
        public Half4 Color { get; set; } //m_Color
        public Half4 EnvInfluence { get; set; } //m_EnvInfluence
        public Half4 FillInfluence { get; set; } //m_FillInfluence
        public Half4 FlickerStrength { get; set; } //m_FlickerStrength
        public Half4 FlickerSpeed { get; set; } //m_FlickerSpeed
        public Half4 Attenuation { get; set; } //m_Attenuation
        public Half2 InnerConeOuterCone { get; set; } //m_InnerConeOuterCone
        public uint Type { get; set; } //m_Type
        public uint Flags { get; set; } //m_Flags
        public float Range { get; set; } //m_Range
        public float Intensity { get; set; } //m_Intensity
        public ulong Unknown_6Ch { get; set; }
        public ulong Unknown_74h { get; set; }
        public JenkHash CutsceneHash { get; set; } //m_cutsceneHash
        public int GlowData { get; set; } //m_GlowData
        public Rsc6Ptr<Rsc6PlacedLightGlow> LightGlow { get; set; } //m_Glow
        public Rsc6Str DebugName { get; set; } //m_DebugName
        public float Bias { get; set; } //bias
        public float Start { get; set; } //Start
        public float End { get; set; } //end
        public int Resolution { get; set; } //resolution
        public bool Active { get; set; } //active
        public bool DrawRoomOnly { get; set; } //drawRoomOnly
        public bool DrawPropsOnly { get; set; } //drawPropsOnly
        public byte Transparency { get; set; } //transparency

        public override void Read(Rsc6DataReader reader) //rdrPlacedLight
        {
            Position = reader.ReadVector4();
            ParentPosition = reader.ReadVector4();
            Direction = reader.ReadHalf4();
            Color = reader.ReadHalf4();
            EnvInfluence = reader.ReadHalf4();
            FillInfluence = reader.ReadHalf4();
            FlickerStrength = reader.ReadHalf4();
            FlickerSpeed = reader.ReadHalf4();
            Attenuation = reader.ReadHalf4();
            InnerConeOuterCone = reader.ReadHalf2();
            Type = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
            Range = reader.ReadSingle();
            Intensity = reader.ReadSingle();
            Unknown_6Ch = reader.ReadUInt64();
            Unknown_74h = reader.ReadUInt64();
            CutsceneHash = reader.ReadUInt32();
            GlowData = reader.ReadInt32();
            LightGlow = reader.ReadPtr<Rsc6PlacedLightGlow>();
            DebugName = reader.ReadStr();
            Bias = reader.ReadSingle();
            Start = reader.ReadSingle();
            End = reader.ReadSingle();
            Resolution = reader.ReadInt32();
            Active = reader.ReadBoolean();
            DrawRoomOnly = reader.ReadBoolean();
            DrawPropsOnly = reader.ReadBoolean();
            Transparency = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(Position);
            writer.WriteVector4(ParentPosition);
            writer.WriteHalf4(Direction);
            writer.WriteHalf4(Color);
            writer.WriteHalf4(EnvInfluence);
            writer.WriteHalf4(FillInfluence);
            writer.WriteHalf4(FlickerStrength);
            writer.WriteHalf4(FlickerSpeed);
            writer.WriteHalf4(Attenuation);
            writer.WriteHalf2(InnerConeOuterCone);
            writer.WriteUInt32(Type);
            writer.WriteUInt32(Flags);
            writer.WriteSingle(Range);
            writer.WriteSingle(Intensity);
            writer.WriteUInt64(Unknown_6Ch);
            writer.WriteUInt64(Unknown_74h);
            writer.WriteUInt32(CutsceneHash);
            writer.WriteInt32(GlowData);
            writer.WritePtr(LightGlow);
            writer.WriteStr(DebugName);
            writer.WriteSingle(Bias);
            writer.WriteSingle(Start);
            writer.WriteSingle(End);
            writer.WriteInt32(Resolution);
            writer.WriteBoolean(Active);
            writer.WriteBoolean(DrawRoomOnly);
            writer.WriteBoolean(DrawPropsOnly);
            writer.WriteByte(Transparency);
        }

        public void Read(MetaNodeReader reader)
        {
            Position = Rpf6Crypto.ToXYZ(reader.ReadVector4("Position"));
            ParentPosition = Rpf6Crypto.ToXYZ(reader.ReadVector4("ParentPosition"));
            Direction = reader.ReadVector4("Direction");

            var color = reader.ReadColour("Color");
            Color = new Half4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

            EnvInfluence = reader.ReadVector4("EnvInfluence");
            FillInfluence = reader.ReadVector4("FillInfluence");
            FlickerStrength = reader.ReadVector4("FlickerStrength");
            FlickerSpeed = reader.ReadVector4("FlickerSpeed");
            Attenuation = reader.ReadVector4("Attenuation");
            InnerConeOuterCone = reader.ReadVector2("InnerConeOuterCone");
            Type = reader.ReadUInt32("Type");
            Flags = reader.ReadUInt32("Flags");
            Range = reader.ReadSingle("Range");
            Intensity = reader.ReadSingle("Intensity");
            CutsceneHash = new JenkHash(reader.ReadString("CutsceneHash"));
            GlowData = reader.ReadInt32("GlowData");
            LightGlow = new(reader.ReadNode<Rsc6PlacedLightGlow>("LightGlow"));
            DebugName = new Rsc6Str(reader.ReadString("DebugName"));
            Bias = reader.ReadSingle("Bias");
            Start = reader.ReadSingle("Start");
            End = reader.ReadSingle("End");
            Resolution = reader.ReadInt32("Resolution");
            Active = reader.ReadBool("Active");
            DrawRoomOnly = reader.ReadBool("DrawRoomOnly");
            DrawPropsOnly = reader.ReadBool("DrawPropsOnly");
            Transparency = reader.ReadByte("Transparency");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("Position", Position);
            writer.WriteVector4("ParentPosition", ParentPosition);
            writer.WriteVector4("Direction", Rpf6Crypto.Half4ToVector4(Direction));
            writer.WriteColour("Color", new Colour(Color));
            writer.WriteVector4("EnvInfluence", Rpf6Crypto.Half4ToVector4(EnvInfluence));
            writer.WriteVector4("FillInfluence", Rpf6Crypto.Half4ToVector4(FillInfluence));
            writer.WriteVector4("FlickerStrength", Rpf6Crypto.Half4ToVector4(FlickerStrength));
            writer.WriteVector4("FlickerSpeed", Rpf6Crypto.Half4ToVector4(FlickerSpeed));
            writer.WriteVector4("Attenuation", Rpf6Crypto.Half4ToVector4(Attenuation));
            writer.WriteVector2("InnerConeOuterCone", Rpf6Crypto.Half2ToVector2(InnerConeOuterCone));
            writer.WriteUInt32("Type", Type);
            writer.WriteUInt32("Flags", Flags);
            writer.WriteSingle("Range", Range);
            writer.WriteSingle("Intensity", Intensity);
            if (CutsceneHash != 0) writer.WriteString("CutsceneHash", CutsceneHash.ToString());
            writer.WriteInt32("GlowData", GlowData);
            if (LightGlow.Item != null) writer.WriteNode("LightGlow", LightGlow.Item);
            writer.WriteString("DebugName", DebugName.Value);
            writer.WriteSingle("Bias", Bias);
            writer.WriteSingle("Start", Start);
            writer.WriteSingle("End", End);
            writer.WriteInt32("Resolution", Resolution);
            writer.WriteBool("Active", Active);
            writer.WriteBool("DrawRoomOnly", DrawRoomOnly);
            writer.WriteBool("DrawPropsOnly", DrawPropsOnly);
            writer.WriteByte("Transparency", Transparency);
        }

        public override string ToString()
        {
            return DebugName.Value;
        }
    }

    [TC(typeof(EXP))] public class Rsc6PlacedLightGlow : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 64;
        public Colour GlowColor { get; set; } //m_GlowColor
        public Colour StarColor { get; set; } //m_StarColor
        public Colour FogColor { get; set; } //m_FogColor
        public int StarTexture { get; set; } //m_StarTexture
        public Half2 GlowScale { get; set; } //m_GlowScale
        public Half GlowIntensity { get; set; } //m_GlowIntensity
        public Half FogIntensity { get; set; } //m_FogIntensity
        public Half GlowOpacity { get; set; } //m_GlowOpacity
        public Half FogConeAngle { get; set; } //m_FogConeAngle
        public Half FogConeStart { get; set; } //m_FogConeStart
        public Half FogConeEnd { get; set; } //m_FogConeEnd
        public Half InnerRange { get; set; } //m_InnerRange
        public Half NoiseSpeed { get; set; } //m_NoiseSpeed
        public Half NoiseScale { get; set; } //m_NoiseScale
        public Half NoiseFade { get; set; } //m_NoiseFade
        public Half NoiseAzimuth { get; set; } //m_NoiseAzimuth
        public Half NoiseElevation { get; set; } //m_NoiseElevation
        public bool GlowEnable { get; set; } //m_GlowEnable
        public bool FogEnable { get; set; } //m_FogEnable
        public bool NoiseEnable { get; set; } //m_NoiseEnable
        public Half FogConeOffset { get; set; } //m_FogConeOffset
        public int NoiseType { get; set; } //m_NoiseType

        public override void Read(Rsc6DataReader reader) //rdrPlacedLightGlow
        {
            GlowColor = reader.ReadColour();
            StarColor = reader.ReadColour();
            FogColor = reader.ReadColour();
            StarTexture = reader.ReadInt32();
            GlowScale = reader.ReadHalf2();
            GlowIntensity = reader.ReadHalf();
            FogIntensity = reader.ReadHalf();
            GlowOpacity = reader.ReadHalf();
            FogConeAngle = reader.ReadHalf();
            FogConeStart = reader.ReadHalf();
            FogConeEnd = reader.ReadHalf();
            InnerRange = reader.ReadHalf();
            NoiseSpeed = reader.ReadHalf();
            NoiseScale = reader.ReadHalf();
            NoiseFade = reader.ReadHalf();
            NoiseAzimuth = reader.ReadHalf();
            NoiseElevation = reader.ReadHalf();
            GlowEnable = reader.ReadBoolean();
            FogEnable = reader.ReadBoolean();
            NoiseEnable = reader.ReadBoolean();
            FogConeOffset = reader.ReadHalf();
            NoiseType = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteColour(GlowColor);
            writer.WriteColour(StarColor);
            writer.WriteColour(FogColor);
            writer.WriteInt32(StarTexture);
            writer.WriteHalf2(GlowScale);
            writer.WriteHalf(GlowIntensity);
            writer.WriteHalf(FogIntensity);
            writer.WriteHalf(GlowOpacity);
            writer.WriteHalf(FogConeAngle);
            writer.WriteHalf(FogConeStart);
            writer.WriteHalf(FogConeEnd);
            writer.WriteHalf(InnerRange);
            writer.WriteHalf(NoiseSpeed);
            writer.WriteHalf(NoiseScale);
            writer.WriteHalf(NoiseFade);
            writer.WriteHalf(NoiseAzimuth);
            writer.WriteHalf(NoiseElevation);
            writer.WriteBoolean(GlowEnable);
            writer.WriteBoolean(FogEnable);
            writer.WriteBoolean(NoiseEnable);
            writer.WriteHalf(FogConeOffset);
            writer.WriteInt32(NoiseType);
        }

        public void Read(MetaNodeReader reader)
        {
            GlowColor = reader.ReadColour("GlowColor");
            StarColor = reader.ReadColour("StarColor");
            FogColor = reader.ReadColour("FogColor");
            StarTexture = reader.ReadInt32("StarTexture");
            GlowScale = reader.ReadVector2("GlowScale");
            GlowIntensity = (Half)reader.ReadSingle("GlowIntensity");
            FogIntensity = (Half)reader.ReadSingle("FogIntensity");
            GlowOpacity = (Half)reader.ReadSingle("GlowOpacity");
            FogConeAngle = (Half)reader.ReadSingle("FogConeAngle");
            FogConeStart = (Half)reader.ReadSingle("FogConeStart");
            FogConeEnd = (Half)reader.ReadSingle("FogConeEnd");
            InnerRange = (Half)reader.ReadSingle("InnerRange");
            NoiseSpeed = (Half)reader.ReadSingle("NoiseSpeed");
            NoiseScale = (Half)reader.ReadSingle("NoiseScale");
            NoiseFade = (Half)reader.ReadSingle("NoiseFade");
            NoiseAzimuth = (Half)reader.ReadSingle("NoiseAzimuth");
            NoiseElevation = (Half)reader.ReadSingle("NoiseElevation");
            GlowEnable = reader.ReadBool("GlowEnable");
            FogEnable = reader.ReadBool("FogEnable");
            NoiseEnable = reader.ReadBool("NoiseEnable");
            FogConeOffset = (Half)reader.ReadSingle("FogConeOffset");
            NoiseType = reader.ReadInt32("NoiseType");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteColour("GlowColor", GlowColor);
            writer.WriteColour("StarColor", StarColor);
            writer.WriteColour("FogColor", FogColor);
            writer.WriteInt32("StarTexture", StarTexture);
            writer.WriteVector2("GlowScale", Rpf6Crypto.Half2ToVector2(GlowScale));
            writer.WriteSingle("GlowIntensity", (float)GlowIntensity);
            writer.WriteSingle("FogIntensity", (float)FogIntensity);
            writer.WriteSingle("GlowOpacity", (float)GlowOpacity);
            writer.WriteSingle("FogConeAngle", (float)FogConeAngle);
            writer.WriteSingle("FogConeStart", (float)FogConeStart);
            writer.WriteSingle("FogConeEnd", (float)FogConeEnd);
            writer.WriteSingle("InnerRange", (float)InnerRange);
            writer.WriteSingle("NoiseSpeed", (float)NoiseSpeed);
            writer.WriteSingle("NoiseScale", (float)NoiseScale);
            writer.WriteSingle("NoiseFade", (float)NoiseFade);
            writer.WriteSingle("NoiseAzimuth", (float)NoiseAzimuth);
            writer.WriteSingle("NoiseElevation", (float)NoiseElevation);
            writer.WriteBool("GlowEnable", GlowEnable);
            writer.WriteBool("FogEnable", FogEnable);
            writer.WriteBool("NoiseEnable", NoiseEnable);
            writer.WriteSingle("FogConeOffset", (float)FogConeOffset);
            writer.WriteInt32("NoiseType", NoiseType);
        }
    }

    [TC(typeof(EXP))] public class Rsc6LocatorStatic : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 48;
        public Vector4 Offset { get; set; }
        public Vector4 Eulers { get; set; }
        public Rsc6Str Name { get; set; }
        public uint Unknown_24h { get; set; } //Padding
        public uint Unknown_28h { get; set; } //Padding
        public uint Unknown_2Ch { get; set; } //Padding


        public override void Read(Rsc6DataReader reader) //rdrLocatorStatic
        {
            Offset = reader.ReadVector4();
            Eulers = reader.ReadVector4();
            Name = reader.ReadStr();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(Offset);
            writer.WriteVector4(Eulers);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Offset = Rpf6Crypto.ToXYZ(reader.ReadVector4("Offset"));
            Eulers = Rpf6Crypto.ToXYZ(reader.ReadVector4("Eulers"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("Offset", Offset);
            writer.WriteVector4("Eulers", Eulers);
        }

        public override string ToString()
        {
            return Name.Value;
        }
    }

    [TC(typeof(EXP))] public class Rsc6StreamableBase : Rsc6BlockBase //pgStreamableBase
    {
        public override ulong BlockLength => 16;
        public uint Unknown_0h { get; set; } = uint.MaxValue; //Always 0xFFFFFFFF (for #si), 0 for fragments
        public uint Unknown_4h { get; set; } = 64; //Always 64 (for #si), 0xFFFFFFFF for fragments
        public uint Unknown_8h { get; set; } //Always 0
        public uint Unknown_Ch { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            Unknown_0h = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Unknown_0h);
            writer.WriteUInt32(Unknown_4h);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(Unknown_Ch);
        }
    }

    [TC(typeof(EXP))] public class WsiEntity : Entity
    {
        public bool ResetPos = false;
        public JenkHash ModelName;

        public string ParentName;
        public Half RotationX;
        public Half RotationY;
        public Half RotationZ;
        public byte Flags;
        public byte AO;
        public byte ModMode;
        public byte NetworkingFlags;
        public byte RotationType;

        public override string Name => ModelName.ToString();
        public RDR1MapData Wsi => Level as RDR1MapData;

        public WsiEntity()
        {
        }

        public WsiEntity(Rsc6PropInstanceInfo entity) //Fragments, props
        {
            var name = entity.EntityName.ToString().ToLowerInvariant();
            if (name.Contains('/'))
            {
                name = name[(name.LastIndexOf("/") + 1)..];
            }

            var yaw = (float)entity.RotationZ;
            var pitch = (float)entity.RotationX;
            var roll = (float)entity.RotationY;

            if (entity.RotationType == 2)
            {
                roll = -roll;
            }
            else if (entity.RotationType == 0 && pitch > 1.0f && yaw < 1.0f)
            {
                roll = -roll;
            }

            Position = entity.EntityPosition.XYZ();
            Orientation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            OrientationInv = Quaternion.Inverse(Orientation);
            ModelName = JenkHash.GenHash(name);
            LodDistMax = 100.0f;
            RotationX = entity.RotationX;
            RotationY = entity.RotationY;
            RotationZ = entity.RotationZ;
            Flags = entity.Flags;
            AO = entity.AO;
            ModMode = entity.ModMode;
            NetworkingFlags = entity.NetworkingFlags;
            RotationType = entity.RotationType;
        }

        public WsiEntity(Rsc6DrawableInstanceBase entity) //Fragments, props
        {
            var name = entity.Name.ToString().ToLowerInvariant();
            entity.Matrix.Decompose(out var scale, out var rot, out var translation);
            rot = new Quaternion(rot.Z, rot.X, rot.Y, rot.W);

            Position = translation;
            Orientation = rot;
            OrientationInv = Quaternion.Inverse(rot);
            Scale = scale;
            ModelName = JenkHash.GenHash(name);
            LodDistMax = 500.0f;
        }

        public override void SetPiece(Piece p)
        {
            var changed = p != Piece;
            Piece = p;

            if ((p != null) && changed)
            {
                if (Batch == null) //Batch bounds are set directly
                {
                    var pos = Position;
                    if (ResetPos)
                    {
                        Position = Vector3.Zero;
                    }
                    UpdateBounds();
                    Position = pos;
                }
                EnsurePieceLightInstances();
            }
        }

        public void SetFieldBatch(Rsc6GrassField field)
        {
            var aabb = field.GetAABB();
            ModelName = new(field.Name.Value);
            LodDistMax = 100.0f;
            Position = aabb.Center;
            BoundingBox = aabb;
            BoundingSphere = new BoundingSphere(aabb.Center, aabb.Size.Length() * 0.5f);
            ResetPos = true;
        }
    }

    [TC(typeof(EXP))] public class RDR1LightEntity : Entity
    {
        public bool ResetPos = false;
        public string ParentName;
        public Vector3 ParentPosition;
        public JenkHash ModelName;

        public override string Name => ModelName.ToString();
        public RDR1MapData Wsi => Level as RDR1MapData;

        public RDR1LightEntity(Rsc6PlacedLight light, string parent)
        {
            Position = light.Position.XYZ();
            ParentPosition = light.ParentPosition.XYZ();
            ParentName = parent;
            ModelName = JenkHash.GenHash(light.DebugName.Value.ToLowerInvariant());
            LodDistMax = 500.0f;
            ResetPos = true;

            var pos = light.Position.XYZ();
            var dir = new Vector3((float)light.Direction.Z, (float)light.Direction.X, (float)light.Direction.Y);
            var ty = dir.GetPerpVec();
            var tx = Vector3.Normalize(Vector3.Cross(dir, ty));
            var col = new Vector3((float)light.Color.X / 5.0f, (float)light.Color.Y / 5.0f, (float)light.Color.Z / 5.0f);
            var intensity = light.Intensity;
            var range = light.Range;
            var innerAngle = FloatUtil.DegToRad((float)light.InnerConeOuterCone.X);
            var outerAngle = FloatUtil.DegToRad((float)light.InnerConeOuterCone.X);
            var l = Light.CreateSpot(pos, dir, tx, ty, col, intensity, range, 5.0f, innerAngle, outerAngle);
            //var l = Light.CreatePoint(pos, col, intensity, range, 5.0f);
            Lights = new Light[] { l };
        }

        public override void SetPiece(Piece p)
        {
            var changed = p != Piece;
            Piece = p;

            if ((p != null) && changed)
            {
                var pos = Position;
                if (ResetPos)
                {
                    Position = Vector3.Zero;
                }
                UpdateBounds();
                Position = pos;
                EnsurePieceLightInstances();
            }
        }
    }

    [TC(typeof(EXP))] public class RDR1GridForestEntity : Entity
    {
        public Rsc6TreeForestGridCell GridCell;
        public JenkHash TreeName;
        public Vector3 OriginalPosition; //Used so if we're moving a tree in the map viewer, we can find it back from a #sp using its original position.
        public bool Created = false; //Used to make sure we'll update new trees
        
        public override string Name => TreeName.ToString();
        public RDR1TreeMapData Wsp => Level as RDR1TreeMapData;

        public RDR1GridForestEntity(BaseCreateArgs args, float dist) //Create
        {
            TreeName = args.Asset;
            LodDistMax = dist;
            Position = args.Position;
            Orientation = args.Rotation;
            OrientationInv = Orientation.IsIdentity ? Quaternion.Identity : Quaternion.Inverse(Orientation);
            Scale = args.Scale;
            Created = true;
        }

        public RDR1GridForestEntity(Rsc6PackedInstancePos inst, Rsc6TreeForestGridCell gridCell, JenkHash name, float dist) //Trees
        {
            TreeName = name;
            GridCell = gridCell;
            LodDistMax = dist;
            Position = inst.Position;
            OriginalPosition = Position;
        }

        public RDR1GridForestEntity(Rsc6InstanceMatrix inst, Rsc6TreeForestGridCell gridCell, JenkHash name, float dist) //Debris and foliages around buildings and roads
        {
            inst.Transform.Decompose(out var scale, out var rot, out var translation);
            TreeName = name;
            GridCell = gridCell;
            LodDistMax = dist;
            Position = translation;
            OriginalPosition = Position;
            Orientation = new Quaternion(rot.Z, rot.X, rot.Y, rot.W);
            OrientationInv = Quaternion.Inverse(Orientation);
            Scale = scale;
        }

        public static RDR1GridForestEntity CreateFromGrid(object instance, Rsc6TreeForestGridCell gridCell, JenkHash name, float dist)
        {
            if (instance is Rsc6PackedInstancePos instPos)
                return new RDR1GridForestEntity(instPos, gridCell, name, dist);
            else if (instance is Rsc6InstanceMatrix instMatrix)
                return new RDR1GridForestEntity(instMatrix, gridCell, name, dist);
            else
                return null;
        }

        public override void SetPiece(Piece p)
        {
            var changed = p != Piece;
            Piece = p;

            if ((p != null) && changed)
            {
                UpdateBounds();
            }
        }
    }

    [TC(typeof(EXP))] public class RDR1GrassEntity : Entity
    {
        public JenkHash GrassName;
        public override string Name => GrassName.ToString();

        public RDR1GrassEntity(string name, float dist, Vector3 pos)
        {
            GrassName = new(name);
            Position = pos;
            LodDistMax = dist;
        }

        public override void SetPiece(Piece p)
        {
            var changed = p != Piece;
            Piece = p;

            if ((p != null) && changed)
            {
                UpdateBounds();
            }
        }
    }

    public enum Rsc6AttributeType : uint
    {
        PROPS_DOORS_1 = 1,
        PROPS_GRINGOS = 3,
        PROPS_DOORS_2 = 5
    }

    public enum Rsc6MayaCurveForm : short
    {
        FORM_UNKNOWN = -1,
	    FORM_OPEN = 0, //Open curve
	    FORM_CLOSED, //Closed curve
        FORM_PERIODIC, //When you choose "Close curve" in Maya. The curve will repeat smoothly.
	    FORM_NUMFORMS
    }
}