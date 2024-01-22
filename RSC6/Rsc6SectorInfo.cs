using CodeX.Core.Utilities;
using System;
using CodeX.Core.Engine;
using System.Collections.Generic;
using System.Numerics;
using CodeX.Core.Numerics;
using System.Text;
using System.Linq;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6SectorInfo : Rsc6FileBase, MetaNode
    {
        public override ulong BlockLength => 480;
        public List<WsiEntity> RootEntities { get; set; } = new List<WsiEntity>();

        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6Str Name { get; set; } //Only for scoped sectors, otherwise NULL
        public float LODFade { get; set; } //m_LODFade, always 0.0f
        public ulong Unknown_10h { get; set; } //Always 0
        public bool Added { get; set; } //m_Added, always FALSE
        public bool PropsGroup { get; set; } //m_PropsGroup
        public bool MissingMedLOD { get; set; } //m_MissingMedLOD, always FALSE
        public byte Unknown_1Bh { get; set; } = 0xFF; //0xFF padding
        public JenkHash ScopedNameHash { get; set; } //m_iScopeNameHash, only for scoped sectors, otherwise NULL
        public Rsc6Ptr<Rsc6CurveGroup>[] Curves { get; set; } = new Rsc6Ptr<Rsc6CurveGroup>[24]; //m_CurveArrays, not used in 'swTerrain', 'swAll' & some random areas
        public int ParentLevelIndex { get; set; } //m_ParentLevelIndex
        public bool DoNotManage { get; set; } //m_DoNotManage
        public bool Managed { get; set; } //m_Managed
        public bool Dynamic { get; set; } //m_Dynamic
        public bool HasBoneData { get; set; } //m_HasBoneData
        public uint Unknown_88h { get; set; } //Always 0
        public uint ExtraCurveData { get; set; } //Used only in 'swAiCurves' - (sagCurveExtraData, sagCurveStringMap & CurveNetworkGraph)
        public Vector4 MinAndBoundingRadius { get; set; } //m_MinAndBoundingRadius
        public Vector4 MaxAndInscribedRadius { get; set; } //m_MaxAndInscribedRadius
        public Vector4 BoundMin { get; set; }
        public Vector4 BoundMax { get; set; }
        public Rsc6Ptr<Rsc6PlacedLightsGroup> PlacedLightsGroup { get; set; } //m_PlacedLightsGroup
        public Rsc6ManagedArr<Rsc6PropInstanceInfo> Entities { get; set; } //m_Props
        public Rsc6PtrArr<Rsc6BlockMap> Unknown_DCh { get; set; }
        public Rsc6ManagedArr<Rsc6SectorChild> ItemMapChilds { get; set; } //m_Children, 'swAll.wsi' only
        public uint Unknown_E4h { get; set; } = 0x00CDCDCD; //Always 0xCDCDCD00
        public Rsc6Ptr<Rsc6ScopedSectors> ItemChilds { get; set; } //m_ChildGroup
        public Rsc6PtrArr<Rsc6SectorInfo> ChildPtrs { get; set; } //m_ChildPtrs
        public Rsc6ManagedArr<Rsc6DrawableInstanceBase> DrawableInstances { get; set; } //m_SectorDrawableInstances
        public Rsc6ManagedArr<Rsc6BlockMap> Portals { get; set; } //m_Portals
        public Rsc6ManagedArr<Rsc6BlockMap> Attributes { get; set; } //m_Attributes
        public Rsc6StreamableBase VisualDictionary { get; set; } = new Rsc6StreamableBase();
        public Rsc6StreamableBase MedVisualDictionary { get; set; } = new Rsc6StreamableBase();
        public Rsc6StreamableBase LowVisualDictionary { get; set; } = new Rsc6StreamableBase();
        public Rsc6StreamableBase VLowVisualDictionary { get; set; } = new Rsc6StreamableBase();
        public Rsc6StreamableBase Unknown_154h { get; set; } = new Rsc6StreamableBase();
        public Rsc6StreamableBase BoundDictionary { get; set; } = new Rsc6StreamableBase();
        public float LowLODFade { get; set; } = 1.0f;
        public JenkHash SectorNameLower { get; set; }
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
        public byte ResidentVLowCount { get; set; } //m_ResidentVLowCount
        public bool HasVLowLODResource { get; set; } //m_HasVLowLodResource
        public bool VLowSuperseded { get; set; } //m_VLowSuperseded
        public Rsc6Str Scope { get; set; } //m_Scope
        public ushort Unknown_1B4h { get; set; } //Always 0
        public ushort NumGroupedChildren { get; set; } //m_iNumGroupedChildren
        public ushort LODReferences { get; set; } //m_LODReferences
        public ushort Unknown_1B8h { get; set; } //Always 0
        public ushort Unknown_1BAh { get; set; } //Always 0
        public byte CurrentLOD { get; set; } //m_CurrentLOD
        public byte District { get; set; } //m_District
        public bool IsTerrain { get; set; } //m_IsTerrain
        public bool TotallyAllInstancesLoaded { get; set; } //m_TotallyAllInstancesLoaded
        public bool HasDictFlags { get; set; } //m_HasDictFlags
        public byte InstanceAge { get; set; } //m_InstanceAge
        public byte BoundAge { get; set; } //m_BoundAge
        public byte PropsAge { get; set; } //m_PropsAge
        public byte RefCount { get; set; } //m_RefCount
        public byte ParentChildIndex { get; set; } //m_ParentChildIndex
        public uint Flags { get; set; } //m_Flags
        public bool InnerPropsInstanciated { get; set; } //m_InnerPropsInstantiated
        public byte InnerPropsAge { get; set; } //m_InnerPropsAge
        public byte InnerPropsGroup { get; set; } //m_RawPropsGroup
        public byte GroupFileFlags { get; set; } //m_GroupFileFlags
        public ulong BoundInstances { get; set; } //m_BoundInstances
        public byte Unknown_1D8h { get; set; }
        public byte Unknown_1D9h { get; set; } = 0xCD; //Padding
        public uint Unknown_1DAh { get; set; }
        public ushort Unknown_1DEh { get; set; } //Always 0

        public BoundingBox Bounds { get; set; }
        public BoundingBox StreamingBox { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
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
            DoNotManage = reader.ReadBoolean();
            Managed = reader.ReadBoolean(); 
            Dynamic = reader.ReadBoolean();
            HasBoneData = reader.ReadBoolean();
            Unknown_88h = reader.ReadUInt32();
            ExtraCurveData = reader.ReadUInt32();
            MinAndBoundingRadius = reader.ReadVector4();
            MaxAndInscribedRadius = reader.ReadVector4(); //End of sagCurveGroup
            BoundMin = reader.ReadVector4();
            BoundMax = reader.ReadVector4();
            PlacedLightsGroup = reader.ReadPtr<Rsc6PlacedLightsGroup>();
            Entities = reader.ReadArr<Rsc6PropInstanceInfo>();
            Unknown_DCh = reader.ReadPtrArr<Rsc6BlockMap>();
            Unknown_E4h = reader.ReadUInt32();
            ItemMapChilds = reader.ReadArr<Rsc6SectorChild>();
            ItemChilds = reader.ReadPtr<Rsc6ScopedSectors>();
            ChildPtrs = reader.ReadPtrArr<Rsc6SectorInfo>();
            DrawableInstances = reader.ReadArr<Rsc6DrawableInstanceBase>();
            Portals = reader.ReadArr<Rsc6BlockMap>();
            Attributes = reader.ReadArr<Rsc6BlockMap>(); //Array of UInt32
            VisualDictionary.Read(reader);
            MedVisualDictionary.Read(reader);
            LowVisualDictionary.Read(reader);
            VLowVisualDictionary.Read(reader);
            Unknown_154h.Read(reader);
            BoundDictionary.Read(reader);
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
            District = reader.ReadByte();
            IsTerrain = reader.ReadBoolean();
            TotallyAllInstancesLoaded = reader.ReadBoolean();
            HasDictFlags = reader.ReadBoolean();
            InstanceAge = reader.ReadByte();
            BoundAge = reader.ReadByte();
            PropsAge = reader.ReadByte();
            RefCount = reader.ReadByte();
            ParentChildIndex = reader.ReadByte();
            Flags = reader.ReadUInt32();
            InnerPropsInstanciated = reader.ReadBoolean();
            InnerPropsAge = reader.ReadByte();
            InnerPropsGroup = reader.ReadByte();
            GroupFileFlags = reader.ReadByte();
            BoundInstances = reader.ReadUInt64();
            Unknown_1D8h = reader.ReadByte();
            Unknown_1D9h = reader.ReadByte();
            Unknown_1DAh = reader.ReadUInt32();
            Unknown_1DEh = reader.ReadUInt16();

            var scale = new Vector3(500.0f);
            Bounds = new BoundingBox(BoundMin.XYZ(), BoundMax.XYZ());
            StreamingBox = new BoundingBox(BoundMin.XYZ() - scale, BoundMax.XYZ() + scale);

            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                RootEntities.Add(new WsiEntity(entity));
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x01909C38);
            writer.WritePtr(BlockMap);
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
            writer.WriteByte(DoNotManage ? (byte)255 : (byte)0);
            writer.WriteByte(Managed ? (byte)255 : (byte)0);
            writer.WriteByte(Dynamic ? (byte)255 : (byte)0);
            writer.WriteByte(HasBoneData ? (byte)255 : (byte)0);
            writer.WriteUInt32(Unknown_88h);
            writer.WriteUInt32(ExtraCurveData);
            writer.WriteVector4(MinAndBoundingRadius);
            writer.WriteVector4(MaxAndInscribedRadius);
            writer.WriteVector4(BoundMin);
            writer.WriteVector4(BoundMax);
            writer.WritePtr(PlacedLightsGroup);
            writer.WriteArr(Entities);
            writer.WritePtrArr(Unknown_DCh);
            writer.WriteUInt32(Unknown_E4h);
            writer.WriteArr(ItemMapChilds);
            writer.WritePtr(ItemChilds);
            writer.WritePtrArr(ChildPtrs);
            writer.WriteArr(DrawableInstances);
            writer.WriteArr(Portals);
            writer.WriteArr(Attributes);
            VisualDictionary.Write(writer, true);
            MedVisualDictionary.Write(writer);
            LowVisualDictionary.Write(writer, false, true);
            VLowVisualDictionary.Write(writer, true);
            Unknown_154h.Write(writer);
            BoundDictionary.Write(writer, false, true);
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
            writer.WriteByte(District);
            writer.WriteByte(IsTerrain ? (byte)205 : (byte)0);
            writer.WriteByte(TotallyAllInstancesLoaded ? (byte)205 : (byte)0);
            writer.WriteByte(HasDictFlags ? (byte)205 : (byte)0);
            writer.WriteByte(InstanceAge);
            writer.WriteByte(BoundAge);
            writer.WriteByte(PropsAge);
            writer.WriteByte(RefCount);
            writer.WriteByte(ParentChildIndex);
            writer.WriteUInt32(Flags);
            writer.WriteByte(InnerPropsInstanciated ? (byte)1 : (byte)0);
            writer.WriteByte(InnerPropsAge);
            writer.WriteByte(InnerPropsGroup);
            writer.WriteByte(GroupFileFlags);
            writer.WriteUInt64(BoundInstances);
            writer.WriteByte(Unknown_1D8h);
            writer.WriteByte(Unknown_1D9h);
            writer.WriteUInt32(Unknown_1DAh);
            writer.WriteUInt16(Unknown_1DEh);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = new Rsc6Str(reader.ReadString("Name"));
            LODFade = reader.ReadSingle("LODFade");
            Added = reader.ReadBool("Added");
            PropsGroup = reader.ReadBool("PropsGroup");
            MissingMedLOD = reader.ReadBool("MissingMedLOD");
            ScopedNameHash = new JenkHash(reader.ReadString("ScopedNameHash"));

            for (int i = 0; i < Curves.Length; i++)
            {
                Curves[i] = new(reader.ReadNode<Rsc6CurveGroup>($"Curve{i}"));
            }

            ParentLevelIndex = reader.ReadInt32("ParentLevelIndex");
            DoNotManage = reader.ReadBool("DoNotManage");
            Managed = reader.ReadBool("Managed");
            Dynamic = reader.ReadBool("Dynamic");
            HasBoneData = reader.ReadBool("HasBoneData");
            ExtraCurveData = reader.ReadUInt32("ExtraCurveData");
            MinAndBoundingRadius = reader.ReadVector4("MinAndBoundingRadius");
            MaxAndInscribedRadius = reader.ReadVector4("MaxAndInscribedRadius");
            BoundMin = reader.ReadVector4("BoundMin");
            BoundMax = reader.ReadVector4("BoundMax");
            PlacedLightsGroup = new(reader.ReadNode<Rsc6PlacedLightsGroup>("PlacedLightsGroup"));
            Entities = new(reader.ReadNodeArray<Rsc6PropInstanceInfo>("Entities"));
            ItemMapChilds = new(reader.ReadNodeArray<Rsc6SectorChild>("ItemMapChilds"));
            ItemChilds = new(reader.ReadNode<Rsc6ScopedSectors>("ItemChilds"));
            ChildPtrs = new(reader.ReadUInt32("ChildPtrs"));
            DrawableInstances = new(reader.ReadNodeArray<Rsc6DrawableInstanceBase>("DrawableInstances"));
            //Portals.Items = reader.ReadNodeArray("Portals");
            LowLODFade = reader.ReadSingle("LowLODFade");
            SectorNameLower = new JenkHash(reader.ReadString("SectorNameLower"));
            Occluders = new(reader.ReadNodeArray<Rsc6Portal>("Occluders"));
            LastVisibleMarker = reader.ReadUInt32("LastVisibleMarker");
            ResidentStatus = reader.ReadUInt32("ResidentStatus");
            PropNames = new((reader.ReadStringArray("PropNames") ?? Array.Empty<string>()).Select(s => new Rsc6Str(s)).ToArray());
            Locators = new(reader.ReadNodeArray<Rsc6LocatorStatic>("Locators"));
            AnyHighInstanceLoaded = reader.ReadBool("AnyHighInstanceLoaded");
            ResidentVLowCount = reader.ReadByte("ResidentVLowCount");
            HasVLowLODResource = reader.ReadBool("HasVLowLODResource");
            VLowSuperseded = reader.ReadBool("VLowSuperseded");
            Scope = new(reader.ReadString("Scope"));
            NumGroupedChildren = reader.ReadUInt16("NumGroupedChildren");
            LODReferences = reader.ReadUInt16("LODReferences");
            CurrentLOD = reader.ReadByte("CurrentLOD");
            District = reader.ReadByte("District");
            IsTerrain = reader.ReadBool("IsTerrain");
            TotallyAllInstancesLoaded = reader.ReadBool("TotallyAllInstancesLoaded");
            HasDictFlags = reader.ReadBool("HasDictFlags");
            InstanceAge = reader.ReadByte("InstanceAge");
            BoundAge = reader.ReadByte("BoundAge");
            PropsAge = reader.ReadByte("PropsAge");
            RefCount = reader.ReadByte("RefCount");
            ParentChildIndex = reader.ReadByte("ParentChildIndex");
            Flags = reader.ReadUInt32("Flags");
            InnerPropsInstanciated = reader.ReadBool("InnerPropsInstanciated");
            InnerPropsAge = reader.ReadByte("InnerPropsAge");
            InnerPropsGroup = reader.ReadByte("InnerPropsGroup");
            GroupFileFlags = reader.ReadByte("GroupFileFlags");
            //BoundInstances = reader.ReadUInt64("BoundInstances");
            Unknown_1DAh = reader.ReadUInt32("Unknown_1DAh");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            if (Name.Value != null) writer.WriteString("Name", Name.Value);
            writer.WriteSingle("LODFade", LODFade);
            writer.WriteBool("Added", Added);
            writer.WriteBool("PropsGroup", PropsGroup);
            writer.WriteBool("MissingMedLOD", MissingMedLOD);
            if (ScopedNameHash != 0) writer.WriteString("ScopedNameHash", ScopedNameHash.ToString());

            for (int i = 0; i < Curves.Length; i++)
            {
                writer.WriteNode($"Curve{i}", Curves[i].Item);
            }

            writer.WriteInt32("ParentLevelIndex", ParentLevelIndex);
            writer.WriteBool("DoNotManage", DoNotManage);
            writer.WriteBool("Managed", Managed);
            writer.WriteBool("Dynamic", Dynamic);
            writer.WriteBool("HasBoneData", HasBoneData);
            writer.WriteUInt32("ExtraCurveData", ExtraCurveData);
            writer.WriteVector4("MinAndBoundingRadius", MinAndBoundingRadius);
            writer.WriteVector4("MaxAndInscribedRadius", MaxAndInscribedRadius);
            writer.WriteVector4("BoundMin", BoundMin);
            writer.WriteVector4("BoundMax", BoundMax);
            writer.WriteNode("PlacedLightsGroup", PlacedLightsGroup.Item);
            if (Entities.Items != null) writer.WriteNodeArray("Entities", Entities.Items);
            if (ItemMapChilds.Items != null) writer.WriteNodeArray("ItemMapChilds", ItemMapChilds.Items);
            if (ItemChilds.Item != null) writer.WriteNode("ItemChilds", ItemChilds.Item);
            if (ChildPtrs.Pointers != null) writer.WriteInt32("ChildPtrs", ChildPtrs.Pointers.Length);
            if (DrawableInstances.Items != null) writer.WriteNodeArray("DrawableInstances", DrawableInstances.Items);
            //if (Portals.Items != null) writer.WriteNodeArray("Portals", Portals.Items);
            writer.WriteSingle("LowLODFade", LowLODFade);
            if (SectorNameLower != 0) writer.WriteString("SectorNameLower", SectorNameLower.ToString());
            if (Occluders.Items != null) writer.WriteNodeArray("Occluders", Occluders.Items);
            writer.WriteUInt32("LastVisibleMarker", LastVisibleMarker);
            writer.WriteUInt32("ResidentStatus", ResidentStatus);
            if (PropNames.Items != null) writer.WriteStringArray("PropNames", PropNames.Items.Select(s => s.Value).ToArray());
            if (Locators.Items != null) writer.WriteNodeArray("Locators", Locators.Items);
            writer.WriteBool("AnyHighInstanceLoaded", AnyHighInstanceLoaded);
            writer.WriteByte("ResidentVLowCount", ResidentVLowCount);
            writer.WriteBool("HasVLowLODResource", HasVLowLODResource);
            writer.WriteBool("VLowSuperseded", VLowSuperseded);
            writer.WriteString("Scope", Scope.Value);
            writer.WriteUInt16("NumGroupedChildren", NumGroupedChildren);
            writer.WriteUInt16("LODReferences", LODReferences);
            writer.WriteByte("CurrentLOD", CurrentLOD);
            writer.WriteByte("District", District);
            writer.WriteBool("IsTerrain", IsTerrain);
            writer.WriteBool("TotallyAllInstancesLoaded", TotallyAllInstancesLoaded);
            writer.WriteBool("HasDictFlags", HasDictFlags);
            writer.WriteByte("InstanceAge", InstanceAge);
            writer.WriteByte("BoundAge", BoundAge);
            writer.WriteByte("PropsAge", PropsAge);
            writer.WriteByte("RefCount", RefCount);
            writer.WriteByte("ParentChildIndex", ParentChildIndex);
            writer.WriteUInt32("Flags", Flags);
            writer.WriteBool("InnerPropsInstanciated", InnerPropsInstanciated);
            writer.WriteByte("InnerPropsAge", InnerPropsAge);
            writer.WriteByte("InnerPropsGroup", InnerPropsGroup);
            writer.WriteByte("GroupFileFlags", GroupFileFlags);
            //writer.WriteUInt64("BoundInstances", BoundInstances);
            writer.WriteUInt32("Unknown_1DAh", Unknown_1DAh);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\tChild Name : {Name}");
            sb.AppendLine($"\tChild AABB Min : {BoundMin.XYZ()}");
            sb.AppendLine($"\tChild AABB Max : {BoundMax.XYZ()}");
            sb.AppendLine(string.Format("\tChild Objects : {0}{1}", RootEntities.Count, (RootEntities.Count > 0) ? "\n" : ""));

            for (int i = 0; i < RootEntities.Count; i++)
            {
                sb.AppendLine($"\tObject {i + 1}:");
                sb.AppendLine(RootEntities[i].ToString());
            }
            return sb.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ScopedSectors : Rsc6Block, MetaNode
    {
        public ulong BlockLength => 28;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6PtrArr<Rsc6SectorInfo> Sectors { get; set; }
        public Rsc6Arr<uint> SectorsParents { get; set; }
        public Rsc6RawArr<ushort> SectorsIndices { get; set; }
        public ushort IndicesCount { get; set; }
        public ushort IndicesCapacity { get; set; }
        public Rsc6Str Name { get; set; }

        public List<Rsc6SectorInfo> Parents { get; set; }
        public List<string> ParentsNames { get; set; }

        public void Read(Rsc6DataReader reader) //sagScopedSectors
        {
            Sectors = reader.ReadPtrArr<Rsc6SectorInfo>(); //m_ScopedSectors
            SectorsParents = reader.ReadArr<uint>(); //m_ScopedSectorsParents
            SectorsIndices = reader.ReadRawArrPtr<ushort>(); //m_ScopedSectorsIndices
            IndicesCount = reader.ReadUInt16();
            IndicesCapacity = reader.ReadUInt16();
            Name = reader.ReadStr(); //m_ScopeName

            SectorsIndices = reader.ReadRawArrItems(SectorsIndices, IndicesCount);
            Parents = new List<Rsc6SectorInfo>();

            for (int i = 0; i < SectorsParents.Count; i++)
            {
                var items = Sectors.Items;
                var parents = SectorsParents.Items;

                if (items[i].FilePosition <= 0 || parents[i] <= 0) continue;
                for (int i1 = 0; i1 < items.Length; i1++)
                {
                    if (items[i1].FilePosition != parents[i]) continue;
                    Parents.Add(items[i1]);
                }
            }
        }

        public void Write(Rsc6DataWriter writer)
        {
            IndicesCount = (ushort)SectorsIndices.Items.Length;
            IndicesCapacity = (ushort)SectorsIndices.Items.Length;

            var parentsPos = new uint[ParentsNames.Count];
            for (int i = 0; i < ParentsNames.Count; i++)
            {
                if (i == 0)
                    parentsPos[i] = 0U;
                else
                    parentsPos[i] = (uint)Sectors.Items.FirstOrDefault(p => p.Name.Value == ParentsNames[i]).FilePosition;
            }
            //SectorsParents = new Rsc6Arr<uint>(parentsPos);

            SectorsParents = new Rsc6Arr<uint>(new uint[] { 1342177280, 1342193664, 1342193664, 1342193664 });

            writer.WritePtrArr(Sectors);
            writer.WriteArr(SectorsParents);
            writer.WriteRawArrPtr(SectorsIndices);
            writer.WriteUInt16(IndicesCount);
            writer.WriteUInt16(IndicesCapacity);
            writer.WriteStr(Name);
        }

        public void Read(MetaNodeReader reader)
        {
            Sectors = new(reader.ReadNodeArray<Rsc6SectorInfo>("Sectors"));
            ParentsNames = new(reader.ReadStringArray("SectorsParents").ToList());
            SectorsIndices = new(reader.ReadUInt16Array("SectorsIndices"));
            Name = new Rsc6Str(reader.ReadString("Name"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Sectors", Sectors.Items);
            writer.WriteStringArray("SectorsParents", Parents.Select(s => s.Name.Value).Where(value => value != null).Prepend("root").ToArray());
            writer.WriteUInt16Array("SectorsIndices", SectorsIndices.Items);
            writer.WriteString("Name", Name.Value);
        }
        
        public override string ToString()
        {
            return Name.Value;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6DrawableInstanceBase : Rsc6FileBase, MetaNode
    {
        public override ulong BlockLength => 224;
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
        public uint DebugVisibility { get; set; } //m_DebugVisibility
        public Rsc6Str Name { get; set; } //m_Name
        public Rsc6Ptr<Rsc6Room> Room { get; set; } //m_Room
        public int Drawable { get; set; } //m_Drawable, always 0
        public uint Unknown_C4h { get; set; } //Always 0
        public uint Unknown_C8h { get; set; } //Always 0
        public uint Unknown_CCh { get; set; } //Always 0
        public uint Unknown_D0h { get; set; } //Always 0
        public uint NextDrawableOffsetForBound { get; set; } //m_NextDrawableForBound
        public uint Unknown_D8h { get; set; } //Always 0
        public uint Unknown_DCh { get; set; } //Always 0
        public BoundingBox Bounds { get; set; }

        public override void Read(Rsc6DataReader reader) //rdrDrawableInstanceBase
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
            Room = reader.ReadPtr<Rsc6Room>(); //pointer
            Drawable = reader.ReadInt32();
            Unknown_C4h = reader.ReadUInt32();
            Unknown_C8h = reader.ReadUInt32();
            Unknown_CCh = reader.ReadUInt32();
            Unknown_D0h = reader.ReadUInt32();
            NextDrawableOffsetForBound = reader.ReadUInt32();
            Unknown_D8h = reader.ReadUInt32();
            Unknown_DCh = reader.ReadUInt32();
            Bounds = new BoundingBox(BoundingBoxMin.XYZ(), BoundingBoxMax.XYZ());
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x01913300);
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
            writer.WriteUInt32(NextDrawableOffsetForBound);
            writer.WriteUInt32(Unknown_D8h);
            writer.WriteUInt32(Unknown_DCh);
        }

        public void Read(MetaNodeReader reader)
        {
            TimeLastVisible = reader.ReadSingle("TimeLastVisible");
            LastKnownPositionAndFlags = reader.ReadVector4("LastKnownPositionAndFlags");
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
            Matrix = reader.ReadMatrix4x4("Matrix");
            BoundingBoxMin = reader.ReadVector4("BoundingBoxMin");
            BoundingBoxMax = reader.ReadVector4("BoundingBoxMax");
            InstanceHash = reader.ReadUInt32("InstanceHash");
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
            NextDrawableOffsetForBound = reader.ReadUInt32("NextDrawableOffsetForBound");
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
            writer.WriteUInt32("InstanceHash", InstanceHash);
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
            writer.WriteUInt32("NextDrawableOffsetForBound", NextDrawableOffsetForBound);
        }

        public override string ToString()
        {
            return Name.Value;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Room : Rsc6FileBase, MetaNode
    {
        public override ulong BlockLength => 144;

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
        public Rsc6Str ReverbName { get; set; } //m_ReverbName
        public int VisibleTimestamp { get; set; } //m_VisibleTimestamp
        public uint StreamingVolume { get; set; } //m_StreamingVolume, always 0
        public uint Unknown_8Ch { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //rage::rmpRoom
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
            writer.WriteUInt32(0x019130E0);
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
            Bounds = reader.ReadVector4("Bounds");
            Matrix = reader.ReadMatrix4x4("Matrix");
            Polygons = new(reader.ReadNodeArray<Rsc6Polygon>("Polygons"));
            StaticObjects = new(reader.ReadUInt16Array("StaticObjects") ?? Array.Empty<ushort>());
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

    [TC(typeof(EXP))]
    public class Rsc6Portal : Rsc6FileBase, MetaNode
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
        }

        public void Read(MetaNodeReader reader)
        {
            Polygons = new Rsc6Polygon();
            Polygons.Read(reader);
            Matrix = reader.ReadMatrix4x4("Matrix");
            Bounds = reader.ReadVector3("Bounds");
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

    [TC(typeof(EXP))]
    public class Rsc6Polygon : Rsc6FileBase, MetaNode
    {
        public override ulong BlockLength => 112;
        public int EdgeVisFrame { get; set; }
        public int Data { get; set; }
        public int RefNumber { get; set; }
        public int PolyNode { get; set; } //Start of rage::spdPolyNode (m_Node)
        public int AtDNode { get; set; }
        public int Next { get; set; }
        public int Prev { get; set; } //Offset of the current spdPolyNode
        public int Polygon { get; set; } //m_Dist2 calculated but not stored
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Padding, end of rage::spdPolyNode (m_Node)
        public Vector4 PlaneCoeffs { get; set; } //rage::spdPlane (m_Plane)
        public Vector4 Center { get; set; }
        public Rsc6Arr<Vector4> Points { get; set; }
        public Rsc6Arr<byte> VisibleEdges { get; set; } //TODO: Make sure Rsc6Arr.Capacity is correct
        public bool SingleSided { get; set; }
        public uint Unknown_61h { get; set; } //Padding
        public uint Unknown_65h { get; set; } //Padding
        public uint Unknown_69h { get; set; } //Padding
        public ushort Unknown_6Dh { get; set; } //Padding
        public byte Unknown_6Fh { get; set; } //Padding

        public override void Read(Rsc6DataReader reader) //spdPolygon
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
            writer.WriteUInt32(0x01909C10);
            writer.WriteInt32(EdgeVisFrame);
            writer.WriteInt32(Data);
            writer.WriteInt32(RefNumber);
            writer.WriteInt32(PolyNode);
            writer.WriteInt32(AtDNode);
            writer.WriteInt32(Next);
            writer.WriteInt32(Prev);
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
            PlaneCoeffs = reader.ReadVector4("PlaneCoeffs");
            Center = reader.ReadVector4("Center");
            Points = new(reader.ReadVector4Array("Points"));
            VisibleEdges = new(reader.ReadByteArray("VisibleEdges"));
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

    [TC(typeof(EXP))]
    public class Rsc6CurveGroup : Rsc6Block, MetaNode
    {
        /*
         * There's a block of 24 pointers in each sagSectorInfo that refer to this sagCurveGroup structure
         * Most of the time only 1 or 2 curves are used per sector
         * 'swTerrain', 'swAll' & some other random sectors like 'theCeliaWastes' doesn't have any
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

        public ulong BlockLength => 48;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Vector4 BoundingBoxMin { get; set; } //m_AABBMin
        public Vector4 BoundingBoxMax { get; set; } //m_AABBMax
        public Rsc6Arr<uint> Curves { get; set; } //m_CurveArray
        public int Next { get; set; } //m_Next
        public int ParentLevelIndex { get; set; } //m_ParentLevelIndex

        public void Read(Rsc6DataReader reader) //sagCurveGroup + sagCurveArray
        {
            BoundingBoxMin = reader.ReadVector4();
            BoundingBoxMax = reader.ReadVector4();
            Curves = reader.ReadArr<uint>();
            Next = reader.ReadInt32();
            ParentLevelIndex = reader.ReadInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(BoundingBoxMin);
            writer.WriteVector4(BoundingBoxMax);
            writer.WriteArr(Curves);
            writer.WriteInt32(Next);
            writer.WriteInt32(ParentLevelIndex);
        }

        public void Read(MetaNodeReader reader)
        {
            BoundingBoxMin = reader.ReadVector4("BoundingBoxMin");
            BoundingBoxMax = reader.ReadVector4("BoundingBoxMax");
            Curves = new(reader.ReadUInt32Array("Curves"));
            Next = reader.ReadInt32("Next");
            ParentLevelIndex = reader.ReadInt32("ParentLevelIndex");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("BoundingBoxMin", BoundingBoxMin);
            writer.WriteVector4("BoundingBoxMax", BoundingBoxMax);
            writer.WriteUInt32Array("Curves", Curves.Items);
            writer.WriteInt32("Next", Next);
            writer.WriteInt32("ParentLevelIndex", ParentLevelIndex);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6PropInstanceInfo : Rsc6Block, MetaNode
    {
        public ulong BlockLength => 48;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Rsc6Str EntityName { get; set; } //m_TypeName
        public uint Light { get; set; } //m_Light, always 0
        public Half RotationX { get; set; } //m_Rx
        public Half RotationY { get; set; } //m_Ry
        public Half RotationZ { get; set; } //m_Rz
        public byte Flags { get; set; } //m_Flags
        public byte AO { get; set; } //m_AO
        public Vector4 EntityPosition { get; set; } //m_Offset
        public uint Unknown_20h { get; set; } //Always 0
        public uint Unknown_24h { get; set; } //Always 0
        public uint PortalOffset { get; set; }
        public uint Unknown_2Ch { get; set; } //Always 0

        public void Read(Rsc6DataReader reader) //propInstanceInfo
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
            Unknown_24h = reader.ReadUInt32();
            PortalOffset = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
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
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(PortalOffset);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            EntityName = new(reader.ReadString("EntityName"));
            RotationX = reader.ReadStruct<Half>("RotationX");
            RotationY = reader.ReadStruct<Half>("RotationY");
            RotationZ = reader.ReadStruct<Half>("RotationZ");
            Flags = reader.ReadByte("Flags");
            AO = reader.ReadByte("AO");
            EntityPosition = reader.ReadVector4("EntityPosition");
            PortalOffset = reader.ReadUInt32("PortalOffset");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("EntityName", EntityName.Value);
            writer.WriteStruct("RotationX", RotationX);
            writer.WriteStruct("RotationY", RotationY);
            writer.WriteStruct("RotationZ", RotationZ);
            writer.WriteByte("Flags", Flags);
            writer.WriteByte("AO", AO);
            writer.WriteVector4("EntityPosition", EntityPosition);
            writer.WriteUInt32("PortalOffset", PortalOffset);
        }

        public override string ToString()
        {
            return EntityName.Value;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6SectorChild : Rsc6Block, MetaNode
    {
        public ulong BlockLength => 112;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public BoundingBox SectorBounds { get; set; }
        public Vector4 SectorBoundsMin { get; set; } //m_BoundingBoxMin
        public Vector4 SectorBoundsMax { get; set; } //m_BoundingBoxMax
        public uint Unknown_20h { get; set; } //Always 0
        public string SectorName { get; set; } //m_Name
        public Rsc6Str SectorName2 { get; set; } //m_Scope
        public uint Unknown_68h { get; set; } //m_String, always 0
        public uint IsImportantLandmark { get; set; } //m_IsImportantLandmark, always 0 except for 'fillMoreTunnel'

        public void Read(Rsc6DataReader reader) //sagSectorChild
        {
            SectorBoundsMin = reader.ReadVector4();
            SectorBoundsMax = reader.ReadVector4();
            Unknown_20h = reader.ReadUInt32();
            SectorName = reader.ReadString();

            while (reader.Position < FilePosition + 0x64)
            {
                reader.ReadByte(); //0xCD padding
            }

            SectorName2 = reader.ReadStr();
            Unknown_68h = reader.ReadUInt32();
            IsImportantLandmark = reader.ReadUInt32();

            var scale = new Vector3(1000.0f);
            SectorBounds = new BoundingBox(SectorBoundsMin.XYZ() - scale, SectorBoundsMax.XYZ() + scale);
        }

        public void Write(Rsc6DataWriter writer)
        {
            ulong pos = writer.Position;
            writer.WriteVector4(SectorBoundsMin);
            writer.WriteVector4(SectorBoundsMax);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteStringNullTerminated(SectorName);

            while (writer.Position < pos + 0x64)
            {
                writer.WriteByte(0xCD); //Padding
            }

            writer.WriteStr(SectorName2);
            writer.WriteUInt32(Unknown_68h);
            writer.WriteUInt32(IsImportantLandmark);
        }

        public void Read(MetaNodeReader reader)
        {
            SectorBoundsMin = reader.ReadVector4("SectorBoundsMin");
            SectorBoundsMax = reader.ReadVector4("SectorBoundsMax");
            SectorName = reader.ReadString("SectorName");
            SectorName2 = new Rsc6Str(reader.ReadString("SectorName2"));
            IsImportantLandmark = reader.ReadUInt32("IsImportantLandmark");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("SectorBoundsMin", SectorBoundsMin);
            writer.WriteVector4("SectorBoundsMax", SectorBoundsMax);
            writer.WriteString("SectorName", SectorName);
            writer.WriteString("SectorName2", SectorName2.Value);
            writer.WriteUInt32("IsImportantLandmark", IsImportantLandmark);
        }

        public override string ToString()
        {
            return SectorName;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6PlacedLightsGroup : Rsc6Block, MetaNode
    {
        public ulong BlockLength => 64;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Vector4 BoundsMin { get; set; } //m_AABBMin
        public Vector4 BoundsMax { get; set; } //m_AABBMax
        public Rsc6Str Name { get; set; } //m_Name
        public Rsc6ManagedArr<Rsc6PlacedLight> Lights { get; set; } //m_Lights
        public uint Unknown_2Ch { get; set; } //Always 0
        public uint Unknown_30h { get; set; } //Always 0
        public uint Unknown_34h { get; set; } //Always 0
        public uint Unknown_38h { get; set; } //Always 0
        public uint Unknown_3Ch { get; set; } //Always 0

        public void Read(Rsc6DataReader reader) //rdrPlacedLightsGroup
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

        public void Write(Rsc6DataWriter writer)
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
            BoundsMin = reader.ReadVector4("BoundsMin");
            BoundsMax = reader.ReadVector4("BoundsMax");
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

    [TC(typeof(EXP))]
    public class Rsc6PlacedLight : Rsc6Block, MetaNode
    {
        public ulong BlockLength => 160;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

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

        public void Read(Rsc6DataReader reader) //rdrPlacedLight
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

        public void Write(Rsc6DataWriter writer)
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
            Position = reader.ReadVector4("Position");
            ParentPosition = reader.ReadVector4("ParentPosition");
            Direction = reader.ReadStruct<Half4>("Direction");
            Color = reader.ReadStruct<Half4>("Color");
            EnvInfluence = reader.ReadStruct<Half4>("EnvInfluence");
            FillInfluence = reader.ReadStruct<Half4>("FillInfluence");
            FlickerStrength = reader.ReadStruct<Half4>("FlickerStrength");
            FlickerSpeed = reader.ReadStruct<Half4>("FlickerSpeed");
            Attenuation = reader.ReadStruct<Half4>("Attenuation");
            InnerConeOuterCone = reader.ReadStruct<Half2>("InnerConeOuterCone");
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
            writer.WriteStruct("Direction", Direction);
            writer.WriteStruct("Color", Color);
            writer.WriteStruct("EnvInfluence", EnvInfluence);
            writer.WriteStruct("FillInfluence", FillInfluence);
            writer.WriteStruct("FlickerStrength", FlickerStrength);
            writer.WriteStruct("FlickerSpeed", FlickerSpeed);
            writer.WriteStruct("Attenuation", Attenuation);
            writer.WriteStruct("InnerConeOuterCone", InnerConeOuterCone);
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

    [TC(typeof(EXP))]
    public class Rsc6PlacedLightGlow : Rsc6Block, MetaNode
    {
        public ulong BlockLength => 64;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

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

        public void Read(Rsc6DataReader reader) //rdrPlacedLightGlow
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

        public void Write(Rsc6DataWriter writer)
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
            GlowScale = reader.ReadStruct<Half2>("GlowScale");
            GlowIntensity = reader.ReadStruct<Half>("GlowIntensity");
            FogIntensity = reader.ReadStruct<Half>("FogIntensity");
            GlowOpacity = reader.ReadStruct<Half>("GlowOpacity");
            FogConeAngle = reader.ReadStruct<Half>("FogConeAngle");
            FogConeStart = reader.ReadStruct<Half>("FogConeStart");
            FogConeEnd = reader.ReadStruct<Half>("FogConeEnd");
            InnerRange = reader.ReadStruct<Half>("InnerRange");
            NoiseSpeed = reader.ReadStruct<Half>("NoiseSpeed");
            NoiseScale = reader.ReadStruct<Half>("NoiseScale");
            NoiseFade = reader.ReadStruct<Half>("NoiseFade");
            NoiseAzimuth = reader.ReadStruct<Half>("NoiseAzimuth");
            NoiseElevation = reader.ReadStruct<Half>("NoiseElevation");
            GlowEnable = reader.ReadBool("GlowEnable");
            FogEnable = reader.ReadBool("FogEnable");
            NoiseEnable = reader.ReadBool("NoiseEnable");
            FogConeOffset = reader.ReadStruct<Half>("FogConeOffset");
            NoiseType = reader.ReadInt32("NoiseType");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteColour("GlowColor", GlowColor);
            writer.WriteColour("StarColor", StarColor);
            writer.WriteColour("FogColor", FogColor);
            writer.WriteInt32("StarTexture", StarTexture);
            writer.WriteStruct("GlowScale", GlowScale);
            writer.WriteStruct("GlowIntensity", GlowIntensity);
            writer.WriteStruct("FogIntensity", FogIntensity);
            writer.WriteStruct("GlowOpacity", GlowOpacity);
            writer.WriteStruct("FogConeAngle", FogConeAngle);
            writer.WriteStruct("FogConeStart", FogConeStart);
            writer.WriteStruct("FogConeEnd", FogConeEnd);
            writer.WriteStruct("InnerRange", InnerRange);
            writer.WriteStruct("NoiseSpeed", NoiseSpeed);
            writer.WriteStruct("NoiseScale", NoiseScale);
            writer.WriteStruct("NoiseFade", NoiseFade);
            writer.WriteStruct("NoiseAzimuth", NoiseAzimuth);
            writer.WriteStruct("NoiseElevation", NoiseElevation);
            writer.WriteBool("GlowEnable", GlowEnable);
            writer.WriteBool("FogEnable", FogEnable);
            writer.WriteBool("NoiseEnable", NoiseEnable);
            writer.WriteStruct("FogConeOffset", FogConeOffset);
            writer.WriteInt32("NoiseType", NoiseType);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6LocatorStatic : Rsc6Block, MetaNode
    {
        public ulong BlockLength => 48;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Vector4 Offset { get; set; }
        public Vector4 Eulers { get; set; }
        public Rsc6Str Name { get; set; }
        public uint Unknown_24h { get; set; } //Padding
        public uint Unknown_28h { get; set; } //Padding
        public uint Unknown_2Ch { get; set; } //Padding


        public void Read(Rsc6DataReader reader) //rdrLocatorStatic
        {
            Offset = reader.ReadVector4();
            Eulers = reader.ReadVector4();
            Name = reader.ReadStr();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(Offset);
            writer.WriteVector4(Eulers);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Offset = reader.ReadVector4("Offset");
            Eulers = reader.ReadVector4("Eulers");
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

    [TC(typeof(EXP))]
    public class Rsc6StreamableBase
    {
        public uint Item { get; set; } = 64;
        public ushort ItemCount { get; set; }
        public ushort ItemCapacity { get; set; }
        public ulong Unknown_8h { get; set; } = 0xFFFFFFFF00000000;

        public void Read(Rsc6DataReader reader) //pgStreamableBase?
        {
            Item = reader.ReadUInt32();
            ItemCount = reader.ReadUInt16();
            ItemCapacity = reader.ReadUInt16();
            Unknown_8h = reader.ReadUInt64();
        }

        public void Write(Rsc6DataWriter writer, bool unused = false, bool nobase = false)
        {
            writer.WriteUInt32(unused ? 0U :Item);
            writer.WriteUInt16(ItemCount);
            writer.WriteUInt16(ItemCapacity);
            writer.WriteUInt64(nobase ? 0UL : Unknown_8h);
        }
    }

    public class WsiEntity : Entity
    {
        public new string Name => ModelName.ToString();
        public JenkHash ModelName;
        public string ParentName; //Used for lights
        public Vector3 ParentPosition; //Used for lights

        public WsiEntity(string modelName)
        {
            ModelName = JenkHash.GenHash(modelName);
        }

        public WsiEntity(Rsc6PropInstanceInfo entity) //Fragments, props
        {
            Position = entity.EntityPosition.XYZ();
            Orientation = Quaternion.CreateFromYawPitchRoll((float)entity.RotationZ, (float)entity.RotationX, (float)entity.RotationY);
            OrientationInv = Quaternion.Inverse(Orientation);
            ModelName = JenkHash.GenHash(entity.EntityName.Value.ToLowerInvariant());
            LodDistMax = 100.0f;
        }

        public WsiEntity(Rsc6PlacedLight light, string parent) //Lights
        {
            Position = light.Position.XYZ();
            ParentPosition = light.ParentPosition.XYZ();
            ParentName = parent;
            ModelName = JenkHash.GenHash(light.DebugName.Value.ToLowerInvariant());
            LodDistMax = 100.0f;

            Lights = new Light[]
            {
                Light.CreatePoint(light.Position.XYZ(),
                    new Vector3((float)light.Color.X / 5.0f, (float)light.Color.Y / 5.0f, (float)light.Color.Z / 5.0f),
                    0.05f,
                    5.0f,
                    1.0f)
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("\t\tObject Name : " + ModelName);
            sb.AppendLine("\t\tObject Position : " + Position);
            return sb.ToString();
        }
    }
}