using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))]
    public class Rsc6SectorInfo : Rsc6BlockBaseMap, MetaNode //sagSectorInfo
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
        public JenkHash ScopedNameHash { get; set; } //m_iScopeNameHash, only for scoped sectors, otherwise 0
        public Rsc6Ptr<Rsc6CurveGroup>[] CurveArrays { get; set; } = new Rsc6Ptr<Rsc6CurveGroup>[24]; //m_CurveArrays, not used in 'swTerrain', 'swAll' & some random areas
        public int ParentLevelIndex { get; set; } = 24; //m_ParentLevelIndex, always 24
        public int Unknown_84h { get; set; } = -1; //Always -1
        public uint Unknown_88h { get; set; } //Always 0
        public uint CurveExtraData { get; set; } //Used only in 'swAiCurves' - (sagCurveExtraData, sagCurveStringMap & CurveNetworkGraph)
        public Vector4 MinAndBoundingRadius { get; set; } //m_MinAndBoundingRadius
        public Vector4 MaxAndInscribedRadius { get; set; } //m_MaxAndInscribedRadius
        public Vector4 BoundMin { get; set; }
        public Vector4 BoundMax { get; set; }
        public Rsc6Ptr<Rsc6PlacedLightsGroup> PlacedLightsGroup { get; set; } //m_PlacedLightsGroup
        public Rsc6ManagedArr<Rsc6PropInstanceInfo> Props { get; set; } //m_Props
        public Rsc6PtrArr<Rsc6MapAttribute> MapAttributes { get; set; }
        public uint Unknown_E4h { get; set; } = 0x00CDCDCD; //Padding
        public Rsc6ManagedArr<Rsc6SectorChild> Children { get; set; } //m_Children, 'swAll.wsi' only
        public Rsc6Ptr<Rsc6ScopedSectors> ChildGroup { get; set; } //m_ChildGroup
        public Rsc6PtrArr<Rsc6SectorInfo> ChildPtrs { get; set; } //m_ChildPtrs, used only in swTerrain, swHorseCurves, etc.
        public Rsc6ManagedArr<Rsc6DrawableInstanceBase> DrawableInstances { get; set; } //m_SectorDrawableInstances
        public Rsc6ManagedArr<Rsc6DrawableInstance> DrawableInstances2 { get; set; }
        public Rsc6ManagedArr<Rsc6Portal> Portals { get; set; } //m_Portals
        public Rsc6PtrArr<Rsc6Attribute> Attributes { get; set; } //m_Attributes
        public Rsc6StreamableBase VisualDictionary0 { get; set; } //rage::pgStreamable<rdrVisualDictionary>[0]
        public Rsc6StreamableBase VisualDictionary1 { get; set; } //rage::pgStreamable<rdrVisualDictionary>[1]
        public ulong StreamingVolumes { get; set; } //m_StreamingVolumes, always 0
        public float StreamingPriority0 { get; set; } //m_StreamingPriority[0], always 0
        public float StreamingPriority1 { get; set; }//m_StreamingPriority[1], always 0
        public Rsc6StreamableBase VLowVisualDictionary { get; set; }
        public Rsc6StreamableBase BoundDictionary { get; set; }
        public uint Unknown_16Ch { get; set; } //Always 0
        public uint Unknown_170h { get; set; } //Always 0
        public float LowLODFade { get; set; } = 1.0f; //m_VLowLODFade, always 1.0f
        public JenkHash NameHash { get; set; } //m_NameHash (SectorNameLower)
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
        public byte CurrentLOD { get; set; } = 0xFF; //m_CurrentLOD, always -1
        public bool District { get; set; } //m_District
        public byte IsTerrain { get; set; } = 0xCD; //m_IsTerrain
        public byte TotallyAllInstancesLoaded { get; set; } = 0xCD; //m_TotallyAllInstancesLoaded
        public byte HasDictFlags { get; set; } = 0xCD; //m_HasDictFlags
        public byte InstanceAge { get; set; } = 0xCD; //m_InstanceAge
        public byte BoundAge { get; set; } = 0xCD; //m_BoundAge
        public byte PropsAge { get; set; } = 0xCD; //m_PropsAge
        public byte RefCount { get; set; } //m_RefCount, always 0 except for some terrain/raiload sectors
        public byte ParentChildIndex { get; set; } //m_ParentChildIndex, always 0
        public uint Unknown_1C8h { get; set; }
        public uint Unknown_1CCh { get; set; }
        public Rsc6Ptr<Rsc6BoundInstance> BoundInstances { get; set; } //m_BoundInstances
        public uint Flags { get; set; } //m_Flags, always 0, except for hc/terrain sectors which have 0x10000000
        public bool InnerPropsInstantiated { get; set; } //m_InnerPropsInstantiated, always FALSE
        public byte InnerPropsAge { get; set; } = 0xCD; //m_InnerPropsAge, always 0xCD
        public bool RawPropsGroup { get; set; } //m_RawPropsGroup, true if the sector is a props group, otherwise false
        public byte GroupFileFlags { get; set; } //m_GroupFileFlags
        public uint NamedNodeMap { get; set; } //m_NamedNodeMap, always 0

        public BoundingBox Bounds { get; set; }
        public static Rsc6SectorInfo Root { get; set; }

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

            for (int i = 0; i < CurveArrays.Length; i++) //Start of sagCurveGroup
            {
                CurveArrays[i] = reader.ReadPtr<Rsc6CurveGroup>(); //sagCurveArray
            }

            ParentLevelIndex = reader.ReadInt32();
            Unknown_84h = reader.ReadInt32();
            Unknown_88h = reader.ReadUInt32();
            CurveExtraData = reader.ReadUInt32();
            MinAndBoundingRadius = reader.ReadVector4();
            MaxAndInscribedRadius = reader.ReadVector4(); //End of sagCurveGroup
            BoundMin = reader.ReadVector4();
            BoundMax = reader.ReadVector4();
            PlacedLightsGroup = reader.ReadPtr<Rsc6PlacedLightsGroup>();
            Props = reader.ReadArr<Rsc6PropInstanceInfo>();
            MapAttributes = reader.ReadPtrArr<Rsc6MapAttribute>();
            Unknown_E4h = reader.ReadUInt32();
            Children = reader.ReadArr<Rsc6SectorChild>();
            ChildGroup = reader.ReadPtr<Rsc6ScopedSectors>();
            ChildPtrs = reader.ReadPtrArr<Rsc6SectorInfo>();
            DrawableInstances = reader.ReadArr<Rsc6DrawableInstanceBase>();
            DrawableInstances2 = reader.ReadArr<Rsc6DrawableInstance>();
            Portals = reader.ReadArr<Rsc6Portal>();
            Attributes = reader.ReadPtrArr(Rsc6Attribute.Create);
            VisualDictionary0 = reader.ReadStruct<Rsc6StreamableBase>();
            VisualDictionary1 = reader.ReadStruct<Rsc6StreamableBase>();
            StreamingVolumes = reader.ReadUInt64();
            StreamingPriority0 = reader.ReadSingle();
            StreamingPriority1 = reader.ReadSingle();
            VLowVisualDictionary = reader.ReadStruct<Rsc6StreamableBase>();
            BoundDictionary = reader.ReadStruct<Rsc6StreamableBase>();
            Unknown_16Ch = reader.ReadUInt32();
            Unknown_170h = reader.ReadUInt32();
            LowLODFade = reader.ReadSingle();
            NameHash = reader.ReadUInt32();
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
            Unknown_1C8h = reader.ReadUInt32();
            Unknown_1CCh = reader.ReadUInt32();
            BoundInstances = reader.ReadPtr<Rsc6BoundInstance>();
            Flags = reader.ReadUInt32();
            InnerPropsInstantiated = reader.ReadBoolean();
            InnerPropsAge = reader.ReadByte();
            RawPropsGroup = reader.ReadBoolean();
            GroupFileFlags = reader.ReadByte();
            NamedNodeMap = reader.ReadUInt32();
            Bounds = new BoundingBox(BoundMin.XYZ(), BoundMax.XYZ());

            if (ChildGroup.Item != null)
            {
                Root = this;
            }

            AssignRoomIndices();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            VisualDictionary0 = new Rsc6StreamableBase(false);
            VisualDictionary1 = new Rsc6StreamableBase(false);
            VLowVisualDictionary = new Rsc6StreamableBase(false);
            BoundDictionary = new Rsc6StreamableBase(false);

            if (ChildGroup.Item != null) //root
            {
                var arr = new Rsc6SectorInfo[] { null };
                ChildPtrs = new(arr);
                Root = this;
            }
            else
            {
                var childrens = new List<Rsc6SectorInfo>();
                var childSectors = Root.ChildGroup.Item?.Sectors.Items;

                for (int i = 1; i < childSectors.Length; i++)
                {
                    var sector = childSectors[i];
                    if (Name.ToString() != sector.Scope.ToString()) continue;
                    childrens.Add(null);
                }

                if (childrens.Count > 0)
                {
                    ChildPtrs = new([.. childrens]);
                }
            }

            base.Write(writer);
            writer.WriteStr(Name);
            writer.WriteSingle(LODFade);
            writer.WriteUInt64(Unknown_10h);
            writer.WriteBoolean(Added);
            writer.WriteBoolean(PropsGroup);
            writer.WriteBoolean(MissingMedLOD);
            writer.WriteByte(Unknown_1Bh);
            writer.WriteUInt32(ScopedNameHash);

            for (int i = 0; i < CurveArrays.Length; i++)
            {
                writer.WritePtr(CurveArrays[i]);
            }

            writer.WriteInt32(ParentLevelIndex);
            writer.WriteInt32(Unknown_84h);
            writer.WriteUInt32(Unknown_88h);
            writer.WriteUInt32(CurveExtraData);
            writer.WriteVector4(MinAndBoundingRadius);
            writer.WriteVector4(MaxAndInscribedRadius);
            writer.WriteVector4(BoundMin);
            writer.WriteVector4(BoundMax);
            writer.WritePtr(PlacedLightsGroup);
            writer.WriteArr(Props);
            writer.WritePtrArr(MapAttributes, true);
            writer.WriteUInt32(Unknown_E4h);
            writer.WriteArr(Children);
            writer.WritePtr(ChildGroup);
            writer.WritePtrArr(ChildPtrs);
            writer.WriteArr(DrawableInstances);
            writer.WriteArr(DrawableInstances2);
            writer.WriteArr(Portals);
            writer.WritePtrArr(Attributes);
            writer.WriteStruct(VisualDictionary0);
            writer.WriteStruct(VisualDictionary1);
            writer.WriteUInt64(StreamingVolumes);
            writer.WriteSingle(StreamingPriority0);
            writer.WriteSingle(StreamingPriority1);
            writer.WriteStruct(VLowVisualDictionary);
            writer.WriteStruct(BoundDictionary);
            writer.WriteUInt32(Unknown_16Ch);
            writer.WriteUInt32(Unknown_170h);
            writer.WriteSingle(LowLODFade);
            writer.WriteUInt32(NameHash);
            writer.WriteArr(Occluders);
            writer.WriteUInt32(LastVisibleMarker);
            writer.WriteUInt32(ResidentStatus);
            writer.WriteUInt32(Unknown_18Ch);
            writer.WriteUInt32(Unknown_190h);
            writer.WriteUInt32(Unknown_194h);
            writer.WriteUInt32(Unknown_198h);
            writer.WritePtrStr(PropNames);
            writer.WriteArr(Locators);
            writer.WriteBoolean(AnyHighInstanceLoaded);
            writer.WriteByte(ResidentVLowCount);
            writer.WriteBoolean(HasVLowLODResource);
            writer.WriteBoolean(VLowSuperseded);
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
            writer.WriteUInt32(Unknown_1C8h);
            writer.WriteUInt32(Unknown_1CCh);
            writer.WritePtr(BoundInstances);
            writer.WriteUInt32(Flags);
            writer.WriteBoolean(InnerPropsInstantiated);
            writer.WriteByte(InnerPropsAge);
            writer.WriteBoolean(RawPropsGroup);
            writer.WriteByte(GroupFileFlags);
            writer.WriteUInt32(NamedNodeMap);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = new Rsc6Str(reader.ReadString("Name"));
            PropsGroup = reader.ReadBool("PropsGroup");
            ScopedNameHash = new JenkHash(reader.ReadString("ScopedNameHash"));

            for (int i = 0; i < CurveArrays.Length; i++)
            {
                CurveArrays[i] = new(reader.ReadNode<Rsc6CurveGroup>($"Curve{i}"));
            }

            MinAndBoundingRadius = Rpf6Crypto.ToXYZ(reader.ReadVector4("MinAndBoundingRadius"));
            MaxAndInscribedRadius = Rpf6Crypto.ToXYZ(reader.ReadVector4("MaxAndInscribedRadius"));
            BoundMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundMin"));
            BoundMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundMax"));
            PlacedLightsGroup = new(reader.ReadNode<Rsc6PlacedLightsGroup>("PlacedLightsGroup"));
            Props = new(reader.ReadNodeArray<Rsc6PropInstanceInfo>("Entities"));
            Attributes = new(reader.ReadNodeArray("Attributes", Rsc6Attribute.Create));
            BuildMapAttributes(Attributes.Items, reader.ReadNodeArray<Rsc6MapAttribute>("MapAttributes"));
            Children = new(reader.ReadNodeArray<Rsc6SectorChild>("ItemMapChilds"));
            ChildGroup = new(reader.ReadNode<Rsc6ScopedSectors>("ItemChilds"));
            ChildPtrs = new(reader.ReadNodeArray<Rsc6SectorInfo>("ChildPtrs"));
            DrawableInstances = new(reader.ReadNodeArray<Rsc6DrawableInstanceBase>("DrawableInstances"));
            Portals = new(reader.ReadNodeArray<Rsc6Portal>("Portals"));
            NameHash = new JenkHash(reader.ReadString("SectorNameLower"));
            Occluders = new(reader.ReadNodeArray<Rsc6Portal>("Occluders"));
            ResidentStatus = reader.ReadUInt32("ResidentStatus");
            Locators = new(reader.ReadNodeArray<Rsc6LocatorStatic>("Locators"));
            AnyHighInstanceLoaded = reader.ReadBool("AnyHighInstanceLoaded");
            HasVLowLODResource = reader.ReadBool("HasVLowLODResource");
            Scope = new(reader.ReadString("Scope"));
            District = reader.ReadBool("District");
            RefCount = reader.ReadByte("RefCount");
            Unknown_1C8h = reader.ReadByte("Unknown_1C8h");
            Unknown_1CCh = reader.ReadByte("Unknown_1CCh");
            Flags = reader.ReadUInt32("Flags");
            GroupFileFlags = reader.ReadByte("GroupFileFlags");
            BoundInstances = new(reader.ReadNode<Rsc6BoundInstance>("BoundInstances"));
            RawPropsGroup = ToString().Contains("props", StringComparison.CurrentCultureIgnoreCase);

            var propNames = reader.ReadStringArray("PropNames");
            var secondaryInstances = reader.ReadNodeArray<Rsc6DrawableInstance>("Unknown_104h");
            var roomMap = BuildRoomDictionary();

            //Build props name array
            if (propNames != null)
            {
                var propStr = new Rsc6Str[propNames.Length];
                for (int i = 0; i < propStr.Length; i++)
                {
                    propStr[i] = new Rsc6Str((propNames[i] == string.Empty) ? null : propNames[i]);
                }
                PropNames = new(propStr);
            }

            if (Props.Items != null)
            {
                for (int i = 0; i < Props.Count; i++)
                {
                    var prop = Props[i];
                    if (prop == null || string.IsNullOrEmpty(prop.LinkedPortalName)) continue;

                    for (int child = 0; child < ChildGroup.Item?.Sectors.Count; child++)
                    {
                        var sector = ChildGroup.Item.Sectors[child];
                        if (sector == null) continue;

                        var portals = sector.Portals.Items;
                        for (int p = 0; p < portals.Length; p++)
                        {
                            var portal = portals[p];
                            if (portal == null || portal.ToString() != prop.LinkedPortalName) continue;
                            if (portal.FrontRoom.Item != null)
                            {

                            }
                        }
                    }
                }
            }

            //Bound instances means we should have childs containing occluders, portals and/or drawable instances with linked rooms
            if (BoundInstances.Item != null)
            {
                var parentRooms = BoundInstances.Item.DrawableRoot;
                for (int child = 0; child < ChildGroup.Item?.Sectors.Count; child++)
                {
                    var sector = ChildGroup.Item.Sectors[child];
                    if (sector == null) continue;

                    //Resolve drawable instance room pointers
                    for (int i = 0; i < sector.DrawableInstances.Count; i++)
                    {
                        var inst = sector.DrawableInstances[i];
                        if (inst == null) continue;

                        inst.Index = i;
                        inst.ParentRooms = parentRooms;

                        if (!string.IsNullOrEmpty(inst.RoomName) && roomMap.TryGetValue(inst.RoomName, out var roomInfo))
                        {
                            inst.RoomIndex = roomInfo.Index;
                        }
                    }

                    //Resolve portal room pointers
                    if (sector.Portals.Items != null)
                    {
                        for (int i = 0; i < sector.Portals.Count; i++)
                        {
                            var portal = sector.Portals[i];
                            if (portal == null) continue;

                            var frontRoomName = portal.FrontRoom.Item?.ToString();
                            var backRoomName = portal.BackRoom.Item?.ToString();
                            portal.ParentRooms = parentRooms;

                            if (!string.IsNullOrEmpty(frontRoomName) && roomMap.TryGetValue(frontRoomName, out var frontRoom))
                            {
                                portal.FrontRoomIndex = frontRoom.Index;
                            }

                            if (!string.IsNullOrEmpty(backRoomName) && roomMap.TryGetValue(backRoomName, out var backRoom))
                            {
                                portal.BackRoomIndex = backRoom.Index;
                            }
                        }

                        //Rebuild each room's portal list from LinkedPortals
                        if (parentRooms.Items != null)
                        {
                            foreach (var room in parentRooms.Items)
                            {
                                if (room == null) continue;
                                if (room.RscName.ToString() != sector.ToString()) continue;

                                room.PortalOwners = [.. sector.Portals.Items];
                                if (room.LinkedPortals == null || room.LinkedPortals.Length == 0) continue;

                                var links = new HashSet<string>(room.LinkedPortals, StringComparer.Ordinal);
                                var linkedPortals = sector.Portals.Items.Where(p => p != null && links.Contains(p.ToString())).ToArray();

                                room.Portals = new(linkedPortals);
                            }
                        }
                    }
                }
            }

            //Resolve secondary drawable instances back to their primary instance nodes
            if (DrawableInstances.Items != null && secondaryInstances != null)
            {
                for (int i = 0; i < DrawableInstances.Items.Length; i++)
                {
                    var inst1 = DrawableInstances[i];
                    if (inst1 == null) continue;

                    for (int i1 = 0; i1 < secondaryInstances.Length; i1++)
                    {
                        var u = secondaryInstances[i1];
                        if (u == null) continue;
                        if (u.InstanceNodeUsed != inst1.Node) continue;
                        u.InstanceNode = inst1;
                    }
                }
            }
            DrawableInstances2 = new(secondaryInstances);
        }

        public void Write(MetaNodeWriter writer)
        {
            if (Name.Value != null) writer.WriteString("Name", Name.Value);
            writer.WriteBool("PropsGroup", PropsGroup);
            if (ScopedNameHash != 0) writer.WriteString("ScopedNameHash", ScopedNameHash.ToString());

            for (int i = 0; i < CurveArrays.Length; i++)
            {
                writer.WriteNode($"Curve{i}", CurveArrays[i].Item);
            }

            writer.WriteVector4("MinAndBoundingRadius", MinAndBoundingRadius);
            writer.WriteVector4("MaxAndInscribedRadius", MaxAndInscribedRadius);
            writer.WriteVector4("BoundMin", BoundMin);
            writer.WriteVector4("BoundMax", BoundMax);
            writer.WriteNode("PlacedLightsGroup", PlacedLightsGroup.Item);
            if (Props.Count > 0) writer.WriteNodeArray("Entities", Props.Items);
            if (Attributes.Count > 0) writer.WriteNodeArray("Attributes", Attributes.Items);

            var mapAttr = MapAttributes.Items.Where(e => e?.Target.Item != null);
            writer.WriteNodeArray("MapAttributes", mapAttr.ToArray());

            if (Children.Count > 0) writer.WriteNodeArray("ItemMapChilds", Children.Items);
            writer.WriteNode("ItemChilds", ChildGroup.Item);
            if (ChildPtrs.Count > 0 && ChildPtrs.Items[0] != null) writer.WriteNodeArray("ChildPtrs", ChildPtrs.Items);
            if (DrawableInstances.Count > 0) writer.WriteNodeArray("DrawableInstances", DrawableInstances.Items);
            if (DrawableInstances2.Count > 0) writer.WriteNodeArray("Unknown_104h", DrawableInstances2.Items);
            if (Portals.Count > 0) writer.WriteNodeArray("Portals", Portals.Items);
            writer.WriteString("SectorNameLower", NameHash.ToString());
            if (Occluders.Count > 0) writer.WriteNodeArray("Occluders", Occluders.Items);
            writer.WriteUInt32("ResidentStatus", ResidentStatus);
            if (PropNames.Count > 0) writer.WriteStringArray("PropNames", [.. PropNames.Items.Select(s => s.Value)]);
            if (Locators.Count > 0) writer.WriteNodeArray("Locators", Locators.Items);
            writer.WriteBool("AnyHighInstanceLoaded", AnyHighInstanceLoaded);
            writer.WriteBool("HasVLowLODResource", HasVLowLODResource);
            writer.WriteString("Scope", Scope.Value);
            writer.WriteBool("District", District);
            writer.WriteByte("RefCount", RefCount);
            writer.WriteUInt32("Unknown_1C8h", Unknown_1C8h);
            writer.WriteUInt32("Unknown_1CCh", Unknown_1CCh);
            writer.WriteUInt32("Flags", Flags);
            writer.WriteByte("GroupFileFlags", GroupFileFlags);
            if (BoundInstances.Item != null) writer.WriteNode("BoundInstances", BoundInstances.Item);
        }

        private void AssignRoomIndicesForSector(Rsc6BoundInstance boundInstances)
        {
            var drawables = DrawableInstances.Items;
            var portals = Portals.Items;
            var occluders = Occluders.Items;
            var parentRooms = boundInstances.DrawableRoot;
            var rooms = parentRooms.Items;

            if (rooms == null || (portals?.Length == 0 && occluders?.Length == 0 && drawables?.Length == 0)) return;

            var roomIndexMap = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < rooms.Length; i++)
            {
                var room = rooms[i];
                if (room == null) continue;

                var name = room.ToString();
                if (!string.IsNullOrEmpty(name) && !roomIndexMap.ContainsKey(name))
                {
                    roomIndexMap[name] = i;
                }
            }

            static void assignRoomIndexToDrawableArray(Rsc6DrawableInstanceBase[] instances, Rsc6ManagedArr<Rsc6Room> parentRooms, Dictionary<string, int> roomIndexMap)
            {
                if (instances == null) return;
                for (int i = 0; i < instances.Length; i++)
                {
                    var inst = instances[i];
                    if (inst == null) continue;

                    inst.ParentRooms = parentRooms;
                    var roomName = inst.Room.Item?.ToString();

                    if (!string.IsNullOrEmpty(roomName) && roomIndexMap.TryGetValue(roomName, out var roomIndex))
                    {
                        inst.RoomIndex = roomIndex;
                    }
                }
            }

            static void assignRoomIndexToPortalArray<T>(T[] portals, Rsc6ManagedArr<Rsc6Room> parentRooms, Dictionary<string, int> roomIndexMap) where T : Rsc6Portal
            {
                if (portals == null) return;
                for (int i = 0; i < portals.Length; i++)
                {
                    var portal = portals[i];
                    if (portal == null) continue;

                    portal.ParentRooms = parentRooms;
                    portal.FrontRoomIndex = -1;
                    portal.BackRoomIndex = -1;

                    var frontRoomName = portal.FrontRoom.Item?.ToString();
                    if (!string.IsNullOrEmpty(frontRoomName) && roomIndexMap.TryGetValue(frontRoomName, out var frontIndex))
                    {
                        portal.FrontRoomIndex = frontIndex;
                    }

                    var backRoomName = portal.BackRoom.Item?.ToString();
                    if (!string.IsNullOrEmpty(backRoomName) && roomIndexMap.TryGetValue(backRoomName, out var backIndex))
                    {
                        portal.BackRoomIndex = backIndex;
                    }
                }
            }

            assignRoomIndexToDrawableArray(drawables, parentRooms, roomIndexMap);
            assignRoomIndexToPortalArray(portals, parentRooms, roomIndexMap);
            assignRoomIndexToPortalArray(occluders, parentRooms, roomIndexMap);
        }

        public void AssignRoomIndices()
        {
            var sectors = ChildGroup.Item?.Sectors.Items;
            var boundInstances = BoundInstances.Item;

            if (sectors != null && boundInstances != null)
            {
                for (int i = 0; i < sectors.Length; i++)
                {
                    var sector = sectors[i];
                    if (sector == null) continue;
                    sector.AssignRoomIndicesForSector(boundInstances);
                }
            }
        }

        public void BuildMapAttributes(Rsc6Attribute[] attributes, Rsc6MapAttribute[] mapAttr)
        {
            if (attributes != null && mapAttr != null)
            {
                var newMapAttr = Rsc6DataMap.Build(mapAttr.ToList());
                if (newMapAttr == null) return;

                for (int i = 0; i < newMapAttr?.Count; i++)
                {
                    var d = newMapAttr[i];
                    if (d == null) continue;

                    foreach (var attr in attributes)
                    {
                        var hash = new JenkHash(attr.ToString());
                        if (hash == d.TargetHash)
                        {
                            d.Target = new(attr); //TODO: Make sure it links to the existing ptr and that it doesn't create a new one
                            break;
                        }
                    }
                }

                MapAttributes = new([.. newMapAttr])
                {
                    Capacity = (ushort)mapAttr.Length
                };
            }
        }

        public Dictionary<string, (Rsc6Room Room, int Index)> BuildRoomDictionary()
        {
            var rooms = BoundInstances.Item?.DrawableRoot.Items;
            if (rooms == null) return null;

            var roomMap = new Dictionary<string, (Rsc6Room Room, int Index)>(StringComparer.Ordinal);
            for (int i = 0; i < rooms.Length; i++)
            {
                var room = rooms[i];
                if (room == null) continue;

                var name = room.ToString();
                if (!string.IsNullOrEmpty(name) && !roomMap.ContainsKey(name))
                {
                    roomMap[name] = (room, i);
                }
            }
            return roomMap;
        }

        public void SetDisabled(bool disable) //sagSectorInfo::SetDisabled
        {
            Flags = (Flags & 0xFEFFFFFFu) | (disable ? 0x01000000u : 0u);
        }

        public override string ToString()
        {
            return Name.Value ?? Scope.Value;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ScopedSectors : Rsc6BlockBase, MetaNode //sagScopedSectors
    {
        public override ulong BlockLength => 28;
        public Rsc6PtrArr<Rsc6SectorInfo> Sectors { get; set; } //m_ScopedSectors
        public Rsc6ManagedArr<Rsc6ScopedSectorParent> SectorsParents { get; set; } //m_ScopedSectorsParents
        public Rsc6Arr<ushort> SectorsIndices { get; set; } //m_ScopedSectorsIndices
        public Rsc6Str ScopeName { get; set; } //m_ScopeName

        public List<string> ParentsNames { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            Sectors = reader.ReadPtrArr<Rsc6SectorInfo>();
            SectorsParents = reader.ReadArr<Rsc6ScopedSectorParent>();
            SectorsIndices = reader.ReadArr<ushort>();
            ScopeName = reader.ReadStr();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrArr(Sectors);
            writer.WriteArr(SectorsParents);
            writer.WriteArr(SectorsIndices);
            writer.WriteStr(ScopeName);
        }

        public void Read(MetaNodeReader reader)
        {
            Sectors = new(reader.ReadNodeArray<Rsc6SectorInfo>("Sectors"));
            ParentsNames = new([.. reader.ReadStringArray("SectorsParents")]);
            ScopeName = new Rsc6Str(reader.ReadString("Name"));

            if (Sectors.Items != null && Sectors.Items.Length > 1)
            {
                var parents = new List<string>();
                var indices = new List<ushort>() { 0 }; //Prepend root

                for (int i = 1; i < ParentsNames.Count; i++)
                {
                    var p = ParentsNames[i].ToLower();
                    var parentSector = Sectors.Items.FirstOrDefault(s => s.Name.ToString().Equals(p, StringComparison.CurrentCultureIgnoreCase));
                    var count = parents.Count(item => item == p);
                    parents.Add(p);
                    indices.Add((ushort)count);
                }

                SectorsIndices = new([.. indices]);
                if (ParentsNames != null)
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
                            Parent = new(Sectors.Items.FirstOrDefault(p => p.Name.Value.Equals(ParentsNames[i], StringComparison.CurrentCultureIgnoreCase)))
                        };
                    }
                    SectorsParents = new Rsc6ManagedArr<Rsc6ScopedSectorParent>(parentsPos);
                }
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Sectors", Sectors.Items);
            writer.WriteStringArray("SectorsParents", SectorsParents.Items.Select(s => s.Parent.Item?.Name.Value)
                                                                          .Where(value => value != null)
                                                                          .Prepend("root")
                                                                          .ToArray());
            writer.WriteString("Name", ScopeName.Value);
        }

        public override string ToString()
        {
            return ScopeName.Value;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ScopedSectorParent : Rsc6BlockBase
    {
        public override ulong BlockLength => 4;
        public Rsc6Ptr<Rsc6SectorInfo> Parent { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            Parent = reader.ReadPtr<Rsc6SectorInfo>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            if (Parent.Item == null)
                writer.WriteUInt32((uint)Rsc6DataReader.VIRTUAL_BASE);
            else
                writer.WritePtr(Parent);
        }

        public override string ToString()
        {
            return Parent.Item?.ToString() ?? "root";
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6DrawableInstanceBase : Rsc6FileBase, MetaNode //rage::rmpNode + sagWorldNode + rdrDrawableInstanceBase
    {
        public override ulong BlockLength => 224;
        public override uint VFT { get; set; } = 0x01913300;

        //Start of rage::rmpNode
        public float TimeLastVisible { get; set; } //m_TimeLastVisible

        public ulong Unknown_8h { get; set; } //Always 0
        public Vector4 LastKnownPositionAndFlags { get; set; } //m_LastKnownPositionAndFlags, w is a flag
        //End of rage::rmpNode

        //Start of sagWorldNode
        public int Node { get; set; } //m_Node
        public uint Unknown_24h { get; set; } //Always 0
        public int Next { get; set; } //Next, always 0
        public int Prev { get; set; } //Prev, always 0
        public uint LastFrameRendered { get; set; } //m_LastFrameRendered, always 0
        public short MatrixSetIndex { get; set; } = -1; //m_MatrixSetIndex, always -1
        public byte SPURenderable { get; set; } //m_SPURenderable, always 0
        public byte NodeType { get; set; } //m_NodeType, always 0
        public int VisibilityFlag { get; set; } //m_VisibilityFlag, always 0
        public int BucketFlag { get; set; } //m_BucketFlag, always 0
        //End of sagWorldNode

        public Matrix4x4 Matrix { get; set; } //m_Mtx
        public Vector4 BoundingBoxMin { get; set; } //m_BoundingBoxMin, if w is >= 10.0f the game skips room assignment
        public Vector4 BoundingBoxMax { get; set; } //m_BoundingBoxMax
        public JenkHash InstanceHash { get; set; } //m_InstHashCode
        public uint RoomFlags1 { get; set; } //m_RoomBits[0]
        public uint RoomFlags2 { get; set; } //m_RoomBits[1]
        public ushort Type { get; set; } //m_Type
        public byte Rooms { get; set; } //m_Rooms, 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15
        public byte Unknown_AF { get; set; } //0, 2 or 3
        public short Flags { get; set; } //m_Flags, 1, 9
        public byte AO { get; set; } //m_AO
        public byte Pad { get; set; } = 0xCD; //m_Pad
        public uint DebugVisibility { get; set; } = 0xCDCDCDCD; //m_DebugVisibility
        public Rsc6Str Name { get; set; } //m_Name
        public Rsc6Ptr<Rsc6Room> Room { get; set; } //m_Room
        public int Drawable { get; set; } //m_Drawable, always 0
        public uint Unknown_C4h { get; set; } //Always 0, alternative drawable, lod?
        public uint Unknown_C8h { get; set; } //Always 0
        public uint Unknown_CCh { get; set; } //Always 0
        public uint Unknown_D0h { get; set; } //Always 0
        public Rsc6Ptr<Rsc6DrawableInstanceBase> NextDrawable { get; set; } //m_NextDrawableForBound
        public uint Unknown_D8h { get; set; } //Always 0
        public uint Unknown_DCh { get; set; } //Always 0

        public Rsc6ManagedArr<Rsc6Room> ParentRooms { get; set; }
        public string RoomName { get; set; }
        public int RoomIndex { get; set; } = -1;
        public bool HasNextDrawable { get; set; }
        public int Index { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TimeLastVisible = reader.ReadSingle();
            Unknown_8h = reader.ReadUInt64();
            LastKnownPositionAndFlags = reader.ReadVector4();
            Node = reader.ReadInt32();
            Unknown_24h = reader.ReadUInt32();
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
            RoomFlags1 = reader.ReadUInt32();
            RoomFlags2 = reader.ReadUInt32();
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
            NextDrawable = reader.ReadPtr<Rsc6DrawableInstanceBase>();
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
            writer.WriteUInt32(Unknown_24h);
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
            writer.WriteUInt32(RoomFlags1);
            writer.WriteUInt32(RoomFlags2);
            writer.WriteUInt16(Type);
            writer.WriteByte(Rooms);
            writer.WriteByte(Unknown_AF);
            writer.WriteInt16(Flags);
            writer.WriteByte(AO);
            writer.WriteByte(Pad);
            writer.WriteUInt32(DebugVisibility);
            writer.WriteStr(Name);

            if (Room.Item != null)
                writer.WritePtr(Room);
            else if (RoomIndex >= 0)
                writer.WritePtrEmbed(ParentRooms.Items, ParentRooms.Items, (ulong)(RoomIndex * 144)); //Point directly to the room in BoundInstances.DrawableRoot
            else
                writer.WriteUInt32(0); //No room

            writer.WriteInt32(Drawable);
            writer.WriteUInt32(Unknown_C4h);
            writer.WriteUInt32(Unknown_C8h);
            writer.WriteUInt32(Unknown_CCh);
            writer.WriteUInt32(Unknown_D0h);

            if (HasNextDrawable)
                writer.WritePtrEmbed(this, this, (ulong)(Index + 1) * 224);
            else
                writer.WriteUInt32(0);

            writer.WriteUInt32(Unknown_D8h);
            writer.WriteUInt32(Unknown_DCh);
        }

        public void Read(MetaNodeReader reader)
        {
            HasNextDrawable = reader.ReadBool("HasNextDrawable");
            TimeLastVisible = reader.ReadSingle("TimeLastVisible");
            LastKnownPositionAndFlags = Rpf6Crypto.ToXYZ(reader.ReadVector4("LastKnownPositionAndFlags"));
            Node = reader.ReadInt32("Node");
            Matrix = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("Matrix"), true);
            BoundingBoxMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxMin"));
            BoundingBoxMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxMax"));
            RoomFlags1 = reader.ReadUInt32("RoomFlags1");
            RoomFlags2 = reader.ReadUInt32("RoomFlags2");
            Type = reader.ReadUInt16("Type");
            Rooms = reader.ReadByte("Rooms");
            Unknown_AF = reader.ReadByte("Unknown_AF");
            Flags = reader.ReadInt16("Flags");
            AO = reader.ReadByte("AO");
            Name = new(reader.ReadString("Name"));
            RoomIndex = reader.ReadInt32("RoomIndex", -1);
            InstanceHash = new(ToString().ToLower());
        }

        public void Write(MetaNodeWriter writer)
        {
            if (NextDrawable.Item != null) writer.WriteBool("HasNextDrawable", true);
            writer.WriteSingle("TimeLastVisible", TimeLastVisible);
            writer.WriteVector4("LastKnownPositionAndFlags", LastKnownPositionAndFlags);
            writer.WriteInt32("Node", Node);
            writer.WriteMatrix4x4("Matrix", Matrix);
            writer.WriteVector4("BoundingBoxMin", BoundingBoxMin);
            writer.WriteVector4("BoundingBoxMax", BoundingBoxMax);
            writer.WriteUInt32("RoomFlags1", RoomFlags1);
            writer.WriteUInt32("RoomFlags2", RoomFlags2);
            writer.WriteUInt16("Type", Type);
            writer.WriteByte("Rooms", Rooms);
            writer.WriteByte("Unknown_AF", Unknown_AF);
            writer.WriteInt16("Flags", Flags);
            writer.WriteByte("AO", AO);
            writer.WriteString("Name", ToString());
            writer.WriteInt32("RoomIndex", RoomIndex);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Room : Rsc6FileBase, MetaNode //rage::rmpRoom + rdrRoom
    {
        /*
         * A rmpRoom is a bounding volume that describes the world visibility and provides a simplified version of the actual polygonal volume.
         * Anything that is inside or partially inside a room is considered visible if one can see a portal connected to the room or
         * if the viewpoint is in the room itself.
         * A room maintains a list of rmpNodes that are within this room, so if the room volume can be seen, those nodes can be seen as well.
         */

        public override ulong BlockLength => 144;
        public override uint VFT { get; set; } = 0x019130E0;
        public Rsc6Str Name { get; set; } //m_ReverbName
        public Rsc6Str RscName { get; set; } //m_DebugRscName
        public uint AddMarker { get; set; } = 0xFFFFFFFF; //m_AddMarker
        public Vector4 Bounds { get; set; } //m_Bounds
        public Matrix4x4 Matrix { get; set; } //m_Matrix
        public Rsc6ManagedArr<Rsc6PolygonBlock> Polygons { get; set; } //m_Polygons
        public Rsc6Arr<ushort> StaticObjects { get; set; } //m_StaticObjects
        public Rsc6PtrArr<Rsc6Portal> Portals { get; set; } //m_Portals
        public uint Flags { get; set; } //m_Flags - 0, 2, 4, 20 or 22
        public uint Unknown_7Ch { get; set; } //Always 0
        public Rsc6Str ReverbName { get; set; } //m_ReverbName, start of rdrRoom
        public int VisibleTimestamp { get; set; } //m_VisibleTimestamp
        public uint StreamingVolume { get; set; } //m_StreamingVolume, always 0
        public uint Unknown_8Ch { get; set; } //Always 0

        public string[] LinkedPortals { get; set; }
        public List<Rsc6Portal> PortalOwners { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadStr();
            RscName = reader.ReadStr();
            AddMarker = reader.ReadUInt32();
            Bounds = reader.ReadVector4();
            Matrix = reader.ReadMatrix4x4();
            Polygons = reader.ReadArr<Rsc6PolygonBlock>();
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
            IList<object> owners = null;
            if (PortalOwners != null && Portals.Items != null)
            {
                owners = [.. Portals.Items.Select(p => PortalOwners.FirstOrDefault(o => o?.ToString() == p?.ToString())).Cast<object>()];
            }

            base.Write(writer);
            writer.WriteStr(Name);
            writer.WriteStr(RscName);
            writer.WriteUInt32(AddMarker);
            writer.WriteVector4(Bounds);
            writer.WriteMatrix4x4(Matrix);
            writer.WriteArr(Polygons);
            writer.WriteArr(StaticObjects);
            writer.WritePtrsEmbedArr(Portals, owners);
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
            StaticObjects = new(reader.ReadUInt16Array("StaticObjects"));
            LinkedPortals = reader.ReadStringArray("LinkedPortals");
            Flags = reader.ReadUInt32("Flags");
            ReverbName = new Rsc6Str(reader.ReadString("ReverbName"));
            VisibleTimestamp = reader.ReadInt32("VisibleTimestamp");
            StreamingVolume = reader.ReadUInt32("StreamingVolume");

            var polygons = reader.ReadNodeArray<Rsc6PolygonBlock>("Polygons");
            if (polygons != null)
            {
                for (int i = 0; i < polygons.Length; i++)
                {
                    var poly = polygons[i];
                    poly.Block.PolygonIndex = i;
                    poly.Block.Target = poly;
                }
                Polygons = new(polygons);
            }
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
            writer.WriteStringArray("LinkedPortals", [.. Portals.Items.Select(p => p.ToString())]);
            writer.WriteUInt32("Flags", Flags);
            writer.WriteString("ReverbName", ReverbName.Value);
            writer.WriteInt32("VisibleTimestamp", VisibleTimestamp);
            writer.WriteUInt32("StreamingVolume", StreamingVolume);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Portal : Rsc6FileBase, MetaNode //rage::rmpPortal
    {
        /*
         * A portal is a polygon that connects two (or more) rmpRooms together.
         * It's main function is to be an "edge" in the graph of all rooms,
         * and to allow for quick visibility testing (e.g you don't need to draw anything inside a room if the portal into that room isn't visible).
         */

        public override ulong BlockLength => 224;
        public override uint VFT { get; set; } = 0x019131CC;
        public Rsc6Polygon Polygons { get; set; } = new Rsc6Polygon();
        public Matrix4x4 Matrix { get; set; } //m_Matrix, rage::Matrix34
        public Vector3 Bounds { get; set; } //rage::rmpBound
        public float Radius { get; set; } //rage::spdSphere
        public Rsc6Ptr<Rsc6Room> FrontRoom { get; set; } //m_FrontRoom
        public Rsc6Ptr<Rsc6Room> BackRoom { get; set; } //m_BackRoom
        public Rsc6Str Name { get; set; } //m_DebugName
        public bool Opened { get; set; } //m_Opened
        public bool Interior { get; set; } //m_Interior
        public uint Unknown_CE { get; set; } //Always 0?
        public ushort Unknown_D2 { get; set; } //Always 0?
        public Rsc6Ptr<Rsc6PropInstanceInfo> PropInstance { get; set; } //m_PropInstance
        public uint Unknown_D8 { get; set; } //Always 0?
        public uint Unknown_DC { get; set; } = 0xCDCDCDCD; //Padding

        public Rsc6ManagedArr<Rsc6Room> ParentRooms { get; set; }
        public int FrontRoomIndex { get; set; } = -1;
        public int BackRoomIndex { get; set; } = -1;
        public static Dictionary<Rsc6Portal, Rsc6PropInstanceInfo> Linked { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Polygons.Read(reader); //rage::rmpPortal
            Matrix = reader.ReadMatrix4x4();
            Bounds = reader.ReadVector3();
            Radius = reader.ReadSingle();
            FrontRoom = reader.ReadPtr<Rsc6Room>();
            BackRoom = reader.ReadPtr<Rsc6Room>();
            Name = reader.ReadStr();
            Opened = reader.ReadBoolean();
            Interior = reader.ReadBoolean();
            Unknown_CE = reader.ReadUInt32(); //rage::rdrPortal
            Unknown_D2 = reader.ReadUInt16();
            PropInstance = reader.ReadPtr<Rsc6PropInstanceInfo>();
            Unknown_D8 = reader.ReadUInt32();
            Unknown_DC = reader.ReadUInt32();

            Linked ??= [];

            if (PropInstance.Item != null)
            {
                Linked[this] = PropInstance.Item;
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            Polygons.Write(writer);
            writer.WriteMatrix4x4(Matrix);
            writer.WriteVector3(Bounds);
            writer.WriteSingle(Radius);

            if (FrontRoom.Item != null)
                writer.WritePtrEmbed(FrontRoom.Item, FrontRoom.Item, 0);
            else if (FrontRoomIndex >= 0)
                writer.WritePtrEmbed(ParentRooms.Items, ParentRooms.Items, (ulong)(FrontRoomIndex * 144));
            else
                writer.WriteUInt32(0); //No frontroom

            if (BackRoom.Item != null)
                writer.WritePtrEmbed(BackRoom.Item, BackRoom.Item, 0);
            else if (BackRoomIndex >= 0)
                writer.WritePtrEmbed(ParentRooms.Items, ParentRooms.Items, (ulong)(BackRoomIndex * 144));
            else
                writer.WriteUInt32(0); //No backroom

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
            Polygons = reader.ReadNode<Rsc6Polygon>("Polygons");
            Matrix = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("Matrix"), true);
            Bounds = Rpf6Crypto.ToXYZ(reader.ReadVector3("Bounds"));
            Radius = reader.ReadSingle("Radius");
            Name = new(reader.ReadString("Name"));
            Opened = reader.ReadBool("Opened");
            Interior = reader.ReadBool("Interior");
            PropInstance = new(reader.ReadNode<Rsc6PropInstanceInfo>("PropInstance"));
            FrontRoomIndex = reader.ReadInt32("FrontRoomIndex", -1);
            BackRoomIndex = reader.ReadInt32("BackRoomIndex", -1);
            Polygons.Target = this;

            if (PropInstance.Item != null)
            {
                PropInstance.Item.LinkedPortal = this;
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("Polygons", Polygons);
            writer.WriteMatrix4x4("Matrix", Matrix);
            writer.WriteVector3("Bounds", Bounds);
            writer.WriteSingle("Radius", Radius);
            if (FrontRoom.Item != null) writer.WriteInt32("FrontRoomIndex", FrontRoomIndex);
            if (BackRoom.Item != null) writer.WriteInt32("BackRoomIndex", BackRoomIndex);
            writer.WriteString("Name", Name.Value);
            writer.WriteBool("Opened", Opened);
            writer.WriteBool("Interior", Interior);
            writer.WriteNode("PropInstance", PropInstance.Item);
        }

        public static bool SamePortal(Rsc6Portal a, Rsc6Portal b)
        {
            if (a == null || b == null) return false;
            if (a.ToString() != b.ToString()) return false;

            var aFront = a.FrontRoom.Item?.ToString();
            var aBack = a.BackRoom.Item?.ToString();
            var bFront = b.FrontRoom.Item?.ToString();
            var bBack = b.BackRoom.Item?.ToString();
            var frontMatches = string.IsNullOrEmpty(aFront) || string.IsNullOrEmpty(bFront) || aFront == bFront;
            var backMatches = string.IsNullOrEmpty(aBack) || string.IsNullOrEmpty(bBack) || aBack == bBack;

            return frontMatches && backMatches;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6DrawableInstance : Rsc6BlockBaseMap, MetaNode
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

        public object InstanceNode { get; set; }
        public int InstanceNodeUsed { get; set; }

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

            var instBase = InstanceNode as Rsc6DrawableInstanceBase;
            writer.WritePtrEmbed(InstanceNode, InstanceNode, (ulong)(instBase.Index * 224));
        }

        public void Read(MetaNodeReader reader)
        {
            Unknown_10h = reader.ReadUInt16("Unknown_10h");
            Unknown_12h = reader.ReadUInt16("Unknown_12h");
            Unknown_20h = reader.ReadUInt32("Unknown_20h");
            Unknown_30h = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("Unknown_30h"), true);
            Unknown_88h = reader.ReadJenkHash("Unknown_88h");
            InstanceNodeUsed = reader.ReadInt32("InstanceNodeUsed");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt16("Unknown_10h", Unknown_10h);
            writer.WriteUInt16("Unknown_12h", Unknown_12h);
            writer.WriteUInt32("Unknown_20h", Unknown_20h);
            writer.WriteMatrix4x4("Unknown_30h", Unknown_30h);
            writer.WriteJenkHash("Unknown_88h", Unknown_88h);
            if (Unknown_8Ch.Item != null) writer.WriteInt32("InstanceNodeUsed", Unknown_8Ch.Item.Node);
        }

        public override string ToString()
        {
            return $"{Unknown_8Ch.Item} : {Unknown_88h}";
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6PolygonBlock : Rsc6FileBase, MetaNode
    {
        public override ulong BlockLength => 112;
        public override uint VFT { get; set; } = 0x01909C10;
        public Rsc6Polygon Block { get; set; } = new Rsc6Polygon();

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Block.Read(reader);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            Block?.Write(writer);
        }

        public void Read(MetaNodeReader reader)
        {
            Block.Read(reader);
        }

        public void Write(MetaNodeWriter reader)
        {
            Block.Write(reader);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Polygon : MetaNode //rage::spdPolygon
    {
        /*
         * spdPolygon  - A convex, planar polygon.
         * spdPolyNode - Used to build a list of spdPolygons
         * spdPlane    - Represents an infinite plane or half-space
         */

        public int EdgeVisibleFrame { get; set; } = -1; //m_EdgeVisFrame, always -1
        public int Data { get; set; } = -1; //Always -1, for Rockstar's tools
        public int RefNumber { get; set; } = -1; //Always -1, for Rockstar's tools
        public uint Unknown_10h { get; set; }
        public int Next { get; set; } //Always 0, part of atDNode<spdPolyNode*> m_PolyNode, unused
        public int Prev { get; set; } //Always 0, part of atDNode<spdPolyNode*> m_PolyNode, unused
        public int Polygon { get; set; } //m_Polygon, offset to the current spdPolygon
        public float Dist { get; set; } //m_Dist2, always 0.0f
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD; //m_Pad1, Padding
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //m_Pad2, Padding
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //m_Pad3, Padding
        public Vector4 PlaneCoeffs { get; set; } //m_Plane, W represents the distance of the plane from the origin
        public Vector4 Center { get; set; } //m_Center
        public Rsc6Arr<Vector4> Points { get; set; } //m_Points
        public Rsc6Arr<byte> VisibleEdges { get; set; } //m_VisibleEdges, used to set visibility bits on the polygon's edges
        public bool SingleSided { get; set; } //m_SingleSided
        public uint Unknown_61h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_65h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_69h { get; set; } = 0xCDCDCDCD; //Padding
        public ushort Unknown_6Dh { get; set; } = 0xCDCD; //Padding
        public byte Unknown_6Fh { get; set; } = 0xCD; //Padding

        public int PolygonIndex { get; set; }
        public object Target { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            EdgeVisibleFrame = reader.ReadInt32();
            Data = reader.ReadInt32();
            RefNumber = reader.ReadInt32();
            Unknown_10h = reader.ReadUInt32();
            Next = reader.ReadInt32();
            Prev = reader.ReadInt32();
            Polygon = reader.ReadInt32();
            Dist = reader.ReadSingle();
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

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteInt32(EdgeVisibleFrame);
            writer.WriteInt32(Data);
            writer.WriteInt32(RefNumber);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteInt32(Next);
            writer.WriteInt32(Prev);
            writer.WritePtrEmbed(Target, Target, (ulong)PolygonIndex * 112); //Polygon
            writer.WriteSingle(Dist);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
            writer.WriteVector4(PlaneCoeffs);
            writer.WriteVector4(Center);
            writer.WriteArr(Points);
            writer.WriteArr(VisibleEdges);
            writer.WriteBoolean(SingleSided);
            writer.WriteUInt32(Unknown_61h);
            writer.WriteUInt32(Unknown_65h);
            writer.WriteUInt32(Unknown_69h);
            writer.WriteUInt16(Unknown_6Dh);
            writer.WriteByte(Unknown_6Fh);
        }

        public void Read(MetaNodeReader reader)
        {
            Unknown_10h = reader.ReadUInt32("Unknown_10h");
            Dist = reader.ReadSingle("Dist");
            PlaneCoeffs = Rpf6Crypto.ToXYZ(reader.ReadVector4("PlaneCoeffs"));
            Center = Rpf6Crypto.ToXYZ(reader.ReadVector4("Center"));
            Points = new(Rpf6Crypto.ToXYZ(reader.ReadVector4Array("Points")));
            VisibleEdges = new(reader.ReadByteArray("VisibleEdges"), false, 16);
            SingleSided = reader.ReadBool("SingleSided");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Unknown_10h", Unknown_10h);
            writer.WriteSingle("Dist", Dist);
            writer.WriteVector4("PlaneCoeffs", PlaneCoeffs);
            writer.WriteVector4("Center", Center);
            writer.WriteVector4Array("Points", Points.Items);
            writer.WriteByteArray("VisibleEdges", VisibleEdges.Items);
            writer.WriteBool("SingleSided", SingleSided);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6CurveGroup : Rsc6BlockBase, MetaNode //sagCurveArray
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

    [TC(typeof(EXP))]
    public class Rsc6Curve : Rsc6FileBase, MetaNode //sagCurve
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
                KnotsCount = Knots.Items.Length;

            if (ControlledVertices.Items != null)
                CVCount = (ushort)(ControlledVertices.Items.Length - 1);
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

    [TC(typeof(EXP))]
    public class Rsc6PropInstanceInfo : Rsc6BlockBase, MetaNode //propInstanceInfo
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
        public Rsc6Ptr<Rsc6Portal> Portal { get; set; } //m_Portal
        public uint Unknown_2Ch { get; set; } //Always 0

        public string LinkedPortalName { get; set; }
        public Rsc6Portal LinkedPortal { get; set; }
        public static Dictionary<Rsc6PropInstanceInfo, Rsc6Portal> Linked { get; set; }

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
            Portal = reader.ReadPtr<Rsc6Portal>();
            Unknown_2Ch = reader.ReadUInt32();
            LinkedPortalName = Portal.ToString();

            Linked ??= [];

            if (Portal.Item != null)
            {
                Linked[this] = Portal.Item;
            }
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
            writer.WritePtrEmbed(LinkedPortal, LinkedPortal, 0);
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
            LinkedPortalName = reader.ReadString("LinkedPortalName");
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

            if (!string.IsNullOrEmpty(LinkedPortalName))
            {
                writer.WriteString("LinkedPortalName", LinkedPortalName);
            }
        }

        public override string ToString()
        {
            return EntityName.Value;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Attribute : Rsc6FileBase, MetaNode //RDR2AttribBase
    {
        public override ulong BlockLength => 16;
        public override uint VFT { get; set; } = 0x01909530;
        public uint TargetPointer { get; set; } //m_TargetPointer
        public Rsc6AttributeTargetType TargetType { get; set; } //m_eTargetType, what kind of object the attribute points to
        public Rsc6AttributeClassType ClassID { get; set; } //m_ClassID, what kind of attribute object it is

        public string TargetedPropName;
        public Vector3 TargetedPropPosition;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TargetPointer = reader.ReadUInt32();
            TargetType = (Rsc6AttributeTargetType)reader.ReadUInt32();
            ClassID = (Rsc6AttributeClassType)reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(TargetPointer);
            writer.WriteUInt32((uint)TargetType);
            writer.WriteUInt32((uint)ClassID);
        }

        public virtual void Read(MetaNodeReader reader)
        {
            ClassID = reader.ReadEnum<Rsc6AttributeClassType>("@type");
            TargetType = reader.ReadEnum<Rsc6AttributeTargetType>("TargetType");
        }

        public virtual void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", ClassID.ToString());
            writer.WriteString("TargetType", TargetType.ToString());
        }

        public static Rsc6Attribute Create(string typeName)
        {
            if (Enum.TryParse(typeName, out Rsc6AttributeClassType type))
            {
                return Create(type);
            }
            return null;
        }

        public static Rsc6Attribute Create(Rsc6DataReader r)
        {
            r.Position += 8;
            var targetType = (Rsc6AttributeTargetType)r.ReadUInt32();
            var classID = (Rsc6AttributeClassType)r.ReadUInt32();
            r.Position -= 16;

            return classID switch
            {
                Rsc6AttributeClassType.NAME_ATTRIBUTE => new Rsc6AttributeInstancedProp(),
                Rsc6AttributeClassType.GRINGO_ATTRIBUTE => new Rsc6AttributeGringo(),
                Rsc6AttributeClassType.DOOR_LOCK_ATTRIBUTE => new Rsc6AttributeDoorLock(),
                _ => throw new NotImplementedException($"Unknown attribute class: {classID}, targetType: {targetType}")
            };
        }

        public static Rsc6Attribute Create(Rsc6AttributeClassType type)
        {
            return type switch
            {
                Rsc6AttributeClassType.NAME_ATTRIBUTE => new Rsc6AttributeInstancedProp(),
                Rsc6AttributeClassType.GRINGO_ATTRIBUTE => new Rsc6AttributeGringo(),
                Rsc6AttributeClassType.DOOR_LOCK_ATTRIBUTE => new Rsc6AttributeDoorLock(),
                _ => throw new NotImplementedException($"Unknown attribute class {type}")
            };
        }

        public static (Rsc6DrawableInstanceBase[] array, int index) GetTargetedPropMatrix(Rsc6DataWriter writer, Vector3 pos, string name)
        {
            foreach (var array in writer.BlockList.OfType<Rsc6DrawableInstanceBase[]>())
            {
                for (int i = 0; i < array.Length; i++)
                {
                    var inst = array[i];
                    if (inst == null) continue;
                    if (inst.Name.ToString() != name) continue;

                    if (Rpf6Crypto.SamePosition(inst.Matrix.Translation, pos)) //Make sure inst.EntityPosition has the correct XYZ format during import
                    {
                        return (array, i);
                    }
                }
            }
            return (null, -1);
        }

        public static (Rsc6PropInstanceInfo[] array, int index) GetTargettedPropInstance(Rsc6DataWriter writer, Vector3 pos, string name)
        {
            foreach (var array in writer.BlockList.OfType<Rsc6PropInstanceInfo[]>())
            {
                for (int i = 0; i < array.Length; i++)
                {
                    var inst = array[i];
                    if (inst == null) continue;
                    if (inst.EntityName.ToString() != name) continue;

                    if (Rpf6Crypto.SamePosition(inst.EntityPosition.XYZ(), pos)) //Make sure inst.EntityPosition has the correct XYZ format during import
                    {
                        return (array, i);
                    }
                }
            }
            return (null, -1);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6MapAttribute : Rsc6BlockBase, IRsc6DataMapEntry<Rsc6MapAttribute>, MetaNode
    {
        public override ulong BlockLength => 12;
        public JenkHash TargetHash { get; set; } //The name hash of the targeted attribute
        public Rsc6Ptr<Rsc6Attribute> Target { get; set; }
        public Rsc6Ptr<Rsc6MapAttribute> Next { get; set; }

        public Rsc6MapAttribute MapNext { get => Next.Item; set => Next = new(value); }
        public uint MapKey { get => TargetHash; }

        public override void Read(Rsc6DataReader reader)
        {
            TargetHash = reader.ReadUInt32();
            Target = reader.ReadPtr(Rsc6Attribute.Create);
            Next = reader.ReadPtr<Rsc6MapAttribute>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(TargetHash);
            writer.WritePtr(Target);
            writer.WritePtr(Next);
        }

        public void Read(MetaNodeReader reader)
        {
            TargetHash = reader.ReadJenkHash("TargetHash");
            Next = new(reader.ReadNode<Rsc6MapAttribute>("Next"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteJenkHash("TargetHash", TargetHash);
            writer.WriteNode("Next", Next.Item);
        }

        public override string ToString()
        {
            return TargetHash.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6AttributeInstancedProp : Rsc6Attribute, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 16;
        public Rsc6Ptr<Rsc6PropInstanceInfo> TargetProp { get; set; }
        public Rsc6Str Name { get; set; }
        public Vector3 Unknown_14h { get; set; } = Rpf6Crypto.DebugVector;

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
            var (array, index) = GetTargettedPropInstance(writer, TargetedPropPosition, TargetedPropName);
            if (array != null)
            {
                TargetProp = new(array[index]);
            }

            writer.WriteUInt32(0x019094C0);
            writer.WritePtr(TargetProp);
            writer.WriteUInt32((uint)TargetType);
            writer.WriteUInt32((uint)ClassID);
            writer.WriteStr(Name);
            writer.WriteVector3(Unknown_14h);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            TargetedPropName = reader.ReadString("TargetedPropName");
            TargetedPropPosition = Rpf6Crypto.ToXYZ(reader.ReadVector3("TargetedPropPosition"));
            Name = new(reader.ReadString("Name"));
            Unknown_14h = Rpf6Crypto.ToXYZ(reader.ReadVector3("Unknown_14h", Rpf6Crypto.DebugVector));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            if (TargetProp.Item != null)
            {
                writer.WriteString("TargetedPropName", TargetProp.Item?.EntityName.ToString());
                writer.WriteVector3("TargetedPropPosition", TargetProp.Item.EntityPosition.XYZ());
            }

            writer.WriteString("Name", Name.ToString());
            if (Unknown_14h != Rpf6Crypto.DebugVector) writer.WriteVector3("Unknown_14h", Unknown_14h);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6AttributeGringo : Rsc6Attribute, MetaNode //RDR2GringoAttribute
    {
        public override ulong BlockLength => base.BlockLength + 20;
        public bool IsMatrixProvider => TargetType == Rsc6AttributeTargetType.MATRIX_PROVIDER;
        public JenkHash TagID { get; set; } //m_TagID
        public uint GringoHandle { get; set; } //Always 0
        public uint Unknown_18h { get; set; } //Always 0
        public uint Unknown_1Ch { get; set; } //Always 0
        public uint Unknown_20h { get; set; } //Always 0

        //Goes somewhere else?
        public JenkHash LinkedAttributeHash { get; set; }
        public uint LinkedAttributePointer { get; set; }
        public uint Unknown_2Ch { get; set; }

        public Rsc6Ptr<Rsc6DrawableInstanceBase> TargetPropMatrix;
        public Rsc6Ptr<Rsc6PropInstanceInfo> TargetPropInstance;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TagID = reader.ReadUInt32();
            GringoHandle = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();

            if (IsMatrixProvider)
            {
                TargetPropMatrix = new Rsc6Ptr<Rsc6DrawableInstanceBase> { Position = TargetPointer };
                TargetPropMatrix = reader.ReadPtrItem(TargetPropMatrix);
            }
            else
            {
                TargetPropInstance = new Rsc6Ptr<Rsc6PropInstanceInfo> { Position = TargetPointer };
                TargetPropInstance = reader.ReadPtrItem(TargetPropInstance);
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(VFT);

            if (IsMatrixProvider)
            {
                var (array, index) = GetTargetedPropMatrix(writer, TargetedPropPosition, TargetedPropName);
                writer.WritePtrEmbed(array[index], array[index], (ulong)(224 * index));
            }
            else
            {
                var (array, index) = GetTargettedPropInstance(writer, TargetedPropPosition, TargetedPropName);
                writer.WritePtrEmbed(array[index], array[index], (ulong)(48 * index));
            }

            writer.WriteUInt32((uint)TargetType);
            writer.WriteUInt32((uint)ClassID);
            writer.WriteUInt32(TagID);
            writer.WriteUInt32(GringoHandle);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(Unknown_1Ch);
            writer.WriteUInt32(Unknown_20h);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            TagID = reader.ReadJenkHash("TagID");
            TargetedPropName = reader.ReadString("TargetedPropName");
            TargetedPropPosition = Rpf6Crypto.ToXYZ(reader.ReadVector3("TargetPropPosition"));
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteJenkHash("TagID", TagID);

            if (IsMatrixProvider)
            {
                writer.WriteString("TargetedPropName", TargetPropMatrix.Item.Name.ToString());
                writer.WriteVector3("TargetPropPosition", TargetPropMatrix.Item.Matrix.Translation);
            }
            else
            {
                writer.WriteString("TargetedPropName", TargetPropInstance.Item.EntityName.ToString());
                writer.WriteVector3("TargetPropPosition", TargetPropInstance.Item.EntityPosition.XYZ());
            }
        }

        public override string ToString()
        {
            return TagID.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6AttributeDoorLock : Rsc6Attribute, MetaNode //RDR2DoorLockAttribute
    {
        public override ulong BlockLength => base.BlockLength + 4;
        public uint LockType { get; set; } //m_eType, mode passed to the door-lock system when the prop is created

        public Rsc6Ptr<Rsc6PropInstanceInfo> TargetProp;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            LockType = reader.ReadUInt32();

            TargetProp = new Rsc6Ptr<Rsc6PropInstanceInfo> { Position = TargetPointer };
            TargetProp = reader.ReadPtrItem(TargetProp);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            var (array, index) = GetTargettedPropInstance(writer, TargetedPropPosition, TargetedPropName);
            if (array != null)
            {
                TargetProp = new(array[index]);
            }

            writer.WriteUInt32(0x019094C0);
            writer.WritePtr(TargetProp);
            writer.WriteUInt32((uint)TargetType);
            writer.WriteUInt32((uint)ClassID);
            writer.WriteUInt32(LockType);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            TargetedPropName = reader.ReadString("TargetedPropName");
            TargetedPropPosition = Rpf6Crypto.ToXYZ(reader.ReadVector3("TargetPropPosition"));
            LockType = reader.ReadUInt32("LockType");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteString("TargetedPropName", TargetProp.Item.EntityName.ToString());
            writer.WriteVector3("TargetPropPosition", TargetProp.Item.EntityPosition.XYZ());
            writer.WriteUInt32("LockType", LockType);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6SectorChild : Rsc6BlockBase, MetaNode //sagSectorChild
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
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6PlacedLightsGroup : Rsc6BlockBase, MetaNode //rdrPlacedLightsGroup
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

        public override void Read(Rsc6DataReader reader)
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

    [TC(typeof(EXP))]
    public class Rsc6PlacedLight : Rsc6BlockBase, MetaNode
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
        public Rsc6PlacedLightType Type { get; set; } //m_Type
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

        public bool IsAttachedToProp => (Flags & 0x04) != 0;

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
            Type = (Rsc6PlacedLightType)reader.ReadUInt32();
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
            writer.WriteUInt32((uint)Type);
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
            Type = reader.ReadEnum<Rsc6PlacedLightType>("Type");
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
            writer.WriteEnum("Type", Type);
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

        public bool ShouldCreateLight()
        {
            if (!Active) return false;
            if (Range <= 0.1f) return false;
            if (Intensity <= 0.0f) return false;
            if (CutsceneHash != 0) return false;
            if ((Flags & 0x300) != 0) return false; //Cutscene specific
            if (Type == Rsc6PlacedLightType.Spot) return false;
            return true;
        }

        public Light CreateLight(Vector3 pos)
        {
            var col = new Vector3((float)Color.X, (float)Color.Y, (float)Color.Z);
            var range = MathF.Max((float)Range, 0.01f);
            var intensity = MathF.Max(Intensity, 0.0f);

            if (Type == Rsc6PlacedLightType.Point)
                return Light.CreatePoint(pos, col, intensity, range, 1.0f);
            else
                return null;
        }

        public override string ToString()
        {
            return DebugName.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6PlacedLightGlow : Rsc6BlockBase, MetaNode
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

    [TC(typeof(EXP))]
    public class Rsc6LocatorStatic : Rsc6BlockBase, MetaNode //rdrLocatorStatic
    {
        public override ulong BlockLength => 48;
        public Vector4 Offset { get; set; } //Offset
        public Vector4 Eulers { get; set; } //Eulers
        public Rsc6Str Name { get; set; } //Name
        public Rsc6ManagedArr<Rsc6LocatorAttribute> Attributes { get; set; } //Attributes
        public uint Unknown_2Ch { get; set; } //Padding, always 0

        public override void Read(Rsc6DataReader reader)
        {
            Offset = reader.ReadVector4();
            Eulers = reader.ReadVector4();
            Name = reader.ReadStr();
            Attributes = reader.ReadArr<Rsc6LocatorAttribute>();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(Offset);
            writer.WriteVector4(Eulers);
            writer.WriteStr(Name);
            writer.WriteArr(Attributes);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Offset = Rpf6Crypto.ToXYZ(reader.ReadVector4("Offset"));
            Eulers = Rpf6Crypto.ToXYZ(reader.ReadVector4("Eulers"));
            Name = new(reader.ReadString("Name"));
            Attributes = new(reader.ReadNodeArray<Rsc6LocatorAttribute>("Attributes"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("Offset", Offset);
            writer.WriteVector4("Eulers", Eulers);
            writer.WriteString("Name", Name.ToString());
            writer.WriteNodeArray("Attributes", Attributes.Items);
        }

        public override string ToString()
        {
            return Name.ToString();
        }

        public class Rsc6LocatorAttribute : Rsc6BlockBase, MetaNode //rdrLocatorStatic::Attribute
        {
            public override ulong BlockLength => 20;
            public Rsc6Str Name { get; set; } //Name
            public Rsc6Str Value { get; set; } //Value
            public Rsc6ManagedArr<Rsc6LocatorAttribute> Augments { get; set; } //Augments
            public int Type { get; set; } //Type

            public override void Read(Rsc6DataReader reader)
            {
                Name = reader.ReadStr();
                Value = reader.ReadStr();
                Augments = reader.ReadArr<Rsc6LocatorAttribute>();
                Type = reader.ReadInt32();
            }

            public override void Write(Rsc6DataWriter writer)
            {
                writer.WriteStr(Name);
                writer.WriteStr(Value);
                writer.WriteArr(Augments);
                writer.WriteInt32(Type);
            }

            public void Read(MetaNodeReader reader)
            {
                Name = new(reader.ReadString("Name"));
                Value = new(reader.ReadString("Value"));
                Augments = new(reader.ReadNodeArray<Rsc6LocatorAttribute>("Augments"));
                Type = reader.ReadInt32("Type");
            }

            public void Write(MetaNodeWriter writer)
            {
                writer.WriteString("Name", Name.ToString());
                writer.WriteString("Value", Value.ToString());
                writer.WriteNodeArray("Augments", Augments.Items);
                writer.WriteInt32("Type", Type);
            }

            public override string ToString()
            {
                return $"{Name} = {Value}";
            }
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6BoundInstance : Rsc6FileBase, MetaNode //sagBoundInstance
    {
        /*
         * Represents a sector-level bound instance.
         * Holds global bounds and links to room data + spatial partition (InstanceDataMember).
         * Acts as the root container for room assignment and spatial queries.
         */

        public override ulong BlockLength => 80;
        public override uint VFT { get; set; } = 0x01909C20;
        public Rsc6Ptr<Rsc6FragArchetype> Archetype { get; set; } //m_archetype, always NULL
        public Rsc6Ptr<Rsc6FragArchetype> PhysInstance { get; set; } //m_physInstance, always NULL
        public Rsc6Ptr<Rsc6InstanceDataMember> Data { get; set; } //m_data, instance-data member record associated with this bound instance
        public Vector4 BoundMin { get; set; }
        public Vector4 BoundMax { get; set; }
        public uint Code { get; set; } //Code, always 0
        public uint Unknown_34h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_38h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_3Ch { get; set; } = 0xCDCDCDCD; //Padding
        public Rsc6ManagedArr<Rsc6Room> DrawableRoot { get; set; } //m_DrawableRoot
        public uint Unknown_48h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_4Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Archetype = reader.ReadPtr<Rsc6FragArchetype>();
            PhysInstance = reader.ReadPtr<Rsc6FragArchetype>();
            Data = reader.ReadPtr<Rsc6InstanceDataMember>();
            BoundMin = reader.ReadVector4();
            BoundMax = reader.ReadVector4();
            Code = reader.ReadUInt32();
            Unknown_34h = reader.ReadUInt32();
            Unknown_38h = reader.ReadUInt32();
            Unknown_3Ch = reader.ReadUInt32();
            DrawableRoot = reader.ReadArr<Rsc6Room>();
            Unknown_48h = reader.ReadUInt32();
            Unknown_4Ch = reader.ReadUInt32();

            var point = new Vector3(2587.1296f, -2166.7903f, 17.200884f); //i_gen_bath01x
            var room = QueryRoomInPoint(point, out var roomIndex);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Archetype);
            writer.WritePtr(PhysInstance);
            writer.WritePtr(Data);
            writer.WriteVector4(BoundMin);
            writer.WriteVector4(BoundMax);
            writer.WriteUInt32(Code);
            writer.WriteUInt32(Unknown_34h);
            writer.WriteUInt32(Unknown_38h);
            writer.WriteUInt32(Unknown_3Ch);
            writer.WriteArr(DrawableRoot);
            writer.WriteUInt32(Unknown_48h);
            writer.WriteUInt32(Unknown_4Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Data = new(reader.ReadNode<Rsc6InstanceDataMember>("Data"));
            BoundMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundMin"));
            BoundMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundMax"));
            DrawableRoot = new(reader.ReadNodeArray<Rsc6Room>("DrawableRoot"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("Data", Data.Item);
            writer.WriteVector4("BoundMin", BoundMin);
            writer.WriteVector4("BoundMax", BoundMax);
            writer.WriteNodeArray("DrawableRoot", DrawableRoot.Items);
        }

        public Rsc6Room QueryRoomInPoint(Vector3 point, out uint roomIndex) //rage::rmpRoomGroupBase::QueryRoomInPoint
        {
            roomIndex = 0xFFFFFFFF;

            var bb = new BoundingBox(BoundMin.XYZ(), BoundMax.XYZ());
            if (bb.Contains(ref point) != ContainmentType.Contains) return null;

            var root = Data.Item.InstTreeRoot.Item;
            if (root == null) return null;
            if (!root.GetLeafData(point, out roomIndex)) return null;

            var rooms = DrawableRoot.Items;
            if (roomIndex == 0xFFFFFFFE || roomIndex == 0xFFFF) return null;
            if (roomIndex >= rooms.Length) return null;

            return rooms[roomIndex];
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6InstanceDataMember : Rsc6BlockBase, MetaNode //sagInstDataMember
    {
        /*
         * Contains the spatial partitioning data (BSP tree) for instances.
         * Provides the root node used by rage::rmpRoomGroupBase::QueryRoomInPoint to resolve room indices.
         * Also stores bucket/index arrays used by BSP leaf nodes.
         */

        public override ulong BlockLength => 36;
        public int Unknown_0h { get; set; } //Always 0
        public uint InstanceCount { get; set; }
        public Rsc6Ptr<Rsc6RoomBspNode> InstTreeRoot { get; set; }
        public uint Unknown_Ch { get; set; }
        public Rsc6RawArr<uint> BucketIndices { get; set; }
        public uint IndexCount { get; set; }
        public float Unknown_18h { get; set; } = 9.0f / 256.0f; //Always 0.03515625f
        public int Instance { get; set; } = -1; //Instance, always -1
        public byte ParentType { get; set; } //ParentType, always 0
        public bool HasSplashed { get; set; } = true; //m_bHasSplashed, always TRUE
        public ushort AudEntityID { get; set; } = 0xCDCD; //m_AudEntityId, always 0xCDCD

        public int InstTreeRootIndex { get; set; } = -1;
        public Rsc6RoomBspNode[] InstNodes { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            Unknown_0h = reader.ReadInt32();
            InstanceCount = reader.ReadUInt32();
            InstTreeRoot = reader.ReadPtr<Rsc6RoomBspNode>();
            Unknown_Ch = reader.ReadUInt32();
            BucketIndices = reader.ReadRawArrPtr<uint>();
            IndexCount = reader.ReadUInt32();
            Unknown_18h = reader.ReadSingle();
            Instance = reader.ReadInt32();
            ParentType = reader.ReadByte();
            HasSplashed = reader.ReadBoolean();
            AudEntityID = reader.ReadUInt16();
            BucketIndices = reader.ReadRawArrItems(BucketIndices, IndexCount, false);

            AssignNodeIndices();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteInt32(Unknown_0h);
            writer.WriteUInt32(InstanceCount);
            writer.WritePtr(InstTreeRoot);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteRawArr(BucketIndices);
            writer.WriteUInt32(IndexCount);
            writer.WriteSingle(Unknown_18h);
            writer.WriteInt32(Instance);
            writer.WriteByte(ParentType);
            writer.WriteBoolean(HasSplashed);
            writer.WriteUInt16(AudEntityID);
        }

        public void Read(MetaNodeReader reader)
        {
            Unknown_Ch = reader.ReadUInt32("Unknown_Ch");
            BucketIndices = new(reader.ReadUInt32Array("BucketIndices"));
            InstTreeRootIndex = reader.ReadInt32("InstTreeRootIndex");
            InstNodes = reader.ReadNodeArray<Rsc6RoomBspNode>("InstNodes");

            if (InstNodes != null)
            {
                for (int i = 0; i < InstNodes.Length; i++)
                {
                    var node = InstNodes[i];
                    if (node == null) continue;

                    node.RootIndices = BucketIndices;

                    if (node.LeftIndex >= 0 && node.LeftIndex < InstNodes.Length)
                        node.LeftChild = new(InstNodes[node.LeftIndex]);

                    if (node.RightIndex >= 0 && node.RightIndex < InstNodes.Length)
                        node.RightChild = new(InstNodes[node.RightIndex]);
                }

                if (InstTreeRootIndex >= 0 && InstTreeRootIndex < InstNodes.Length)
                    InstTreeRoot = new(InstNodes[InstTreeRootIndex]);
            }

            InstanceCount = (uint)(InstNodes?.Length ?? 0);
            IndexCount = (uint)(BucketIndices.Items?.Length ?? 0);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Unknown_Ch", Unknown_Ch);
            writer.WriteUInt32Array("BucketIndices", BucketIndices.Items);
            writer.WriteInt32("InstTreeRootIndex", InstTreeRootIndex);
            writer.WriteNodeArray("InstNodes", InstNodes);
        }

        public Rsc6RoomBspNode[] GetAllInstances()
        {
            if (InstTreeRoot.Item == null) return null;

            var list = new List<Rsc6RoomBspNode>();
            var visited = new HashSet<Rsc6RoomBspNode>(ReferenceEqualityComparer<Rsc6RoomBspNode>.Instance);
            CollectInstances(InstTreeRoot.Item, list, visited);
            return [.. list];
        }

        private static void CollectInstances(Rsc6RoomBspNode instance, List<Rsc6RoomBspNode> list, HashSet<Rsc6RoomBspNode> visited)
        {
            if (instance == null || !visited.Add(instance)) return;
            list.Add(instance);

            CollectInstances(instance.LeftChild.Item, list, visited);
            CollectInstances(instance.RightChild.Item, list, visited);
        }

        private void AssignNodeIndices()
        {
            InstNodes = GetAllInstances();
            var nodeToIndex = new Dictionary<Rsc6RoomBspNode, int>(ReferenceEqualityComparer<Rsc6RoomBspNode>.Instance);

            for (int i = 0; i < InstNodes.Length; i++)
            {
                nodeToIndex[InstNodes[i]] = i;
            }

            InstTreeRootIndex = nodeToIndex[InstTreeRoot.Item];
            foreach (var node in InstNodes)
            {
                if (node.BucketSpanACount > 0)
                    node.BucketStartIndexA = (int)((node.BucketIndexSpanA.Position - BucketIndices.Position) / 4);

                if (node.BucketSpanBCount > 0)
                    node.BucketStartIndexB = (int)((node.BucketIndexSpanB.Position - BucketIndices.Position) / 4);

                node.RootIndices = BucketIndices;
                node.LeftIndex = (node.LeftChild.Item != null) ? nodeToIndex[node.LeftChild.Item] : -1;
                node.RightIndex = (node.RightChild.Item != null) ? nodeToIndex[node.RightChild.Item] : -1;
            }
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6RoomBspNode : Rsc6BlockBase, MetaNode
    {
        /*
         * Node of a BSP tree used for spatial queries.
         * Stores a split plane, child nodes and leaf bucket index spans.
         * Leaf spans resolve to room indices returned by rage::rmpRoomGroupBase::QueryRoomInPoint.
         */

        public override ulong BlockLength => 48;
        public Vector4 SplitPlane { get; set; } //Plane XYZ + distance W
        public Rsc6RawArr<uint> BucketIndexSpanA { get; set; }
        public uint BucketSpanACount { get; set; }
        public Rsc6RawArr<uint> BucketIndexSpanB { get; set; }
        public uint BucketSpanBCount { get; set; }
        public Rsc6Ptr<Rsc6RoomBspNode> LeftChild { get; set; }
        public Rsc6Ptr<Rsc6RoomBspNode> RightChild { get; set; }
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Padding

        public Rsc6RawArr<uint> RootIndices { get; set; }
        public int BucketStartIndexA { get; set; } = -1;
        public int BucketStartIndexB { get; set; } = -1;
        public int LeftIndex { get; set; } = -1;
        public int RightIndex { get; set; } = -1;

        public override void Read(Rsc6DataReader reader)
        {
            SplitPlane = reader.ReadVector4();
            BucketIndexSpanA = reader.ReadRawArrPtr<uint>();
            BucketSpanACount = reader.ReadUInt32();
            BucketIndexSpanB = reader.ReadRawArrPtr<uint>();
            BucketSpanBCount = reader.ReadUInt32();
            LeftChild = reader.ReadPtr<Rsc6RoomBspNode>();
            RightChild = reader.ReadPtr<Rsc6RoomBspNode>();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();

            BucketIndexSpanA = reader.ReadRawArrItems(BucketIndexSpanA, BucketSpanACount);
            BucketIndexSpanB = reader.ReadRawArrItems(BucketIndexSpanB, BucketSpanBCount);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(SplitPlane);

            if (BucketSpanACount > 0 && BucketStartIndexA >= 0)
                writer.WritePtrEmbed(RootIndices.Items, RootIndices.Items, (ulong)(BucketStartIndexA * 4));
            else
                writer.WriteUInt32(0);

            writer.WriteUInt32(BucketSpanACount);

            if (BucketSpanBCount > 0 && BucketStartIndexB >= 0)
                writer.WritePtrEmbed(RootIndices.Items, RootIndices.Items, (ulong)(BucketStartIndexB * 4));
            else
                writer.WriteUInt32(0);

            writer.WriteUInt32(BucketSpanBCount);
            writer.WritePtr(LeftChild);
            writer.WritePtr(RightChild);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            SplitPlane = Rpf6Crypto.ToXYZ(reader.ReadVector4("SplitPlane"));
            BucketIndexSpanA = new(reader.ReadUInt32Array("BucketIndexSpanA"));
            BucketIndexSpanB = new(reader.ReadUInt32Array("BucketIndexSpanB"));
            BucketStartIndexA = reader.ReadInt32("BucketStartIndexA", -1);
            BucketStartIndexB = reader.ReadInt32("BucketStartIndexB", -1);
            LeftIndex = reader.ReadInt32("LeftIndex", -1);
            RightIndex = reader.ReadInt32("RightIndex", -1);

            BucketSpanACount = (uint)(BucketIndexSpanA.Items?.Length ?? 0);
            BucketSpanBCount = (uint)(BucketIndexSpanB.Items?.Length ?? 0);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("SplitPlane", SplitPlane);

            if (BucketSpanACount > 0)
            {
                writer.WriteUInt32Array("BucketIndexSpanA", BucketIndexSpanA.Items);
                writer.WriteInt32("BucketStartIndexA", BucketStartIndexA);
            }

            if (BucketSpanBCount > 0)
            {
                writer.WriteUInt32Array("BucketIndexSpanB", BucketIndexSpanB.Items);
                writer.WriteInt32("BucketStartIndexB", BucketStartIndexB);
            }

            writer.WriteInt32("LeftIndex", LeftIndex);
            writer.WriteInt32("RightIndex", RightIndex);
        }

        public bool GetLeafData(Vector3 point, out uint roomIndex) //rage::spdBspTree::GetLeafData
        {
            roomIndex = 0xFFFFFFFF;
            var node = this;

            while (node != null)
            {
                var side = (point.X * node.SplitPlane.X) + (point.Y * node.SplitPlane.Y) + (point.Z * node.SplitPlane.Z) - node.SplitPlane.W;
                if (side < 0.0f)
                {
                    var left = node.LeftChild.Item;
                    if (left == null)
                    {
                        var span = node.BucketIndexSpanA.Items;
                        if (span != null && span.Length > 0)
                        {
                            roomIndex = span[0];
                            return true;
                        }
                        return false;
                    }
                    node = left;
                }
                else
                {
                    var right = node.RightChild.Item;
                    if (right == null)
                    {
                        var span = node.BucketIndexSpanB.Items;
                        if (span != null && span.Length > 0)
                        {
                            roomIndex = span[0];
                            return true;
                        }
                        return false;
                    }
                    node = right;
                }
            }
            return false;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TerrainWorldResource : Rsc6BlockBaseMap, MetaNode //sagTerrainWorldResource
    {
        public override ulong BlockLength => 144;
        public override uint VFT { get; set; } = 0x04CACFCC;
        public Rsc6ManagedArr<Rsc6TerrainVisualDictionary> Visuals { get; set; } //m_Visuals
        public Rsc6ManagedArr<Rsc6TerrainSectorDictionary> Sectors { get; set; } //m_Sectors
        public Rsc6ManagedArr<Rsc6TerrainTextureDictionary> TextureDictionaries { get; set; } //m_TextureDictionaries
        public BoundingBox4 WorldBox { get; set; } //m_WorldBox
        public Rsc6AtMapArr<Rsc6TerrainTextureEntries> TextureDictionaryLookup { get; set; } //m_TextureDictionaryLookup
        public Rsc6ManagedArr<Rsc6TerrainTextureSet> TextureSets { get; set; } //m_TextureSets, array size is 101, one for each sector in the game
        public Rsc6Ptr<Rsc6GridBucket>[] LodGridBuckets { get; set; } = new Rsc6Ptr<Rsc6GridBucket>[9]; //m_LodGridBuckets
        public short[] GridSize { get; set; } //m_gridSize[5]
        public short[] GridSubDivs { get; set; } //m_gridSubDivs[5]
        public int NumLODLevels { get; set; } //m_numLODlevels

        public string[] TextureDicts => [.. Rsc6DataMap.Flatten(TextureDictionaryLookup.Items, e => e.ToString())]; //Array size equal to lookup capacity

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Visuals = reader.ReadArr<Rsc6TerrainVisualDictionary>();
            Sectors = reader.ReadArr<Rsc6TerrainSectorDictionary>();
            TextureDictionaries = reader.ReadArr<Rsc6TerrainTextureDictionary>();
            WorldBox = reader.ReadBoundingBox4();
            TextureDictionaryLookup = reader.ReadAtMapArr<Rsc6TerrainTextureEntries>();
            TextureSets = reader.ReadArr<Rsc6TerrainTextureSet>();

            //I should really make an atRangeArray structure as this is used for a lot of stuff.
            for (int i = 0; i < LodGridBuckets.Length; i++)
            {
                LodGridBuckets[i] = reader.ReadPtr<Rsc6GridBucket>();
            }

            GridSize = reader.ReadArray<short>(5, false);
            GridSubDivs = reader.ReadArray<short>(5, false);
            NumLODLevels = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteArr(Visuals);
            writer.WriteArr(Sectors);
            writer.WriteArr(TextureDictionaries);
            writer.WriteBoundingBox4(WorldBox);
            writer.WriteAtMapArr(TextureDictionaryLookup);
            writer.WriteArr(TextureSets);

            for (int i = 0; i < LodGridBuckets.Length; i++)
            {
                writer.WritePtr(LodGridBuckets[i]);
            }

            writer.WriteStructArray(GridSize);
            writer.WriteStructArray(GridSubDivs);
            writer.WriteInt32(NumLODLevels);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("NumLODLevels", NumLODLevels);
            writer.WriteVector4("WorldBoxMin", WorldBox.Min);
            writer.WriteVector4("WorldBoxMax", WorldBox.Max);
            writer.WriteStringArray("TextureDicts", TextureDicts);
            writer.WriteInt16Array("GridSize", GridSize);
            writer.WriteInt16Array("GridSubDivs", GridSubDivs);
            writer.WriteNodeArray("Visuals", Visuals.Items);
            writer.WriteNodeArray("Sectors", Sectors.Items);
            writer.WriteNodeArray("TextureDictionaries", TextureDictionaries.Items);
            writer.WriteNodeArray("TextureSets", TextureSets.Items);

            for (int i = 0; i < LodGridBuckets.Length; i++)
            {
                writer.WriteNode(string.Format("LodGridBucket{0}", i), LodGridBuckets[i].Item);
            }
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TerrainVisualDictionary : Rsc6BlockBase, MetaNode //rage::atArray<rage::datOwner<terTerrainVisual>, 0, unsigned short>
    {
        public override ulong BlockLength => 8;
        public Rsc6PtrArr<Rsc6TerrainVisual> Elements { get; set; } //m_Elements, capacity is the total size of all pointers (count * 4)

        public override void Read(Rsc6DataReader reader)
        {
            Elements = reader.ReadPtrArr<Rsc6TerrainVisual>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrArr(Elements);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Elements", Elements.Items);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TerrainSectorDictionary : Rsc6BlockBase, MetaNode //rage::atArray<rage::datOwner<sagTerrainSector>, 0, unsigned short>
    {
        public override ulong BlockLength => 8;
        public Rsc6PtrArr<Rsc6TerrainSector> Elements { get; set; } //m_Elements

        public override void Read(Rsc6DataReader reader)
        {
            Elements = reader.ReadPtrArr<Rsc6TerrainSector>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrArr(Elements);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Elements", Elements.Items);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TerrainVisual : Rsc6BlockBase, MetaNode //terTerrainVisual
    {
        public override ulong BlockLength => 52;
        public Rsc6StreamableBase ClientsHead { get; set; } //m_ClientsHead
        public Rsc6PtrArr<Rsc6TerrainSector> Sectors { get; set; } //m_Sectors
        public int[] TextureDictionaries { get; set; } //m_TD[4]
        public int Count { get; set; } //m_Count
        public short DrawCounter { get; set; } //m_DrawCounter
        public short DrawCounterTexSets { get; set; } //m_DrawCounterTexSets
        public byte LOD { get; set; } //m_LOD
        public bool IsPagedOut { get; set; } //m_IsPagedOut
        public byte StreamPriority { get; set; } //m_StreamPriority
        public byte Unknown_33h { get; set; } //Padding, always 0

        public override void Read(Rsc6DataReader reader)
        {
            ClientsHead = reader.ReadStruct<Rsc6StreamableBase>();
            Sectors = reader.ReadPtrArr<Rsc6TerrainSector>();
            TextureDictionaries = reader.ReadInt32Arr(4);
            Count = reader.ReadInt32();
            DrawCounter = reader.ReadInt16();
            DrawCounterTexSets = reader.ReadInt16();
            LOD = reader.ReadByte();
            IsPagedOut = reader.ReadBoolean();
            StreamPriority = reader.ReadByte();
            Unknown_33h = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            ClientsHead = new Rsc6StreamableBase(false);
            writer.WriteStruct(ClientsHead);
            writer.WritePtrArr(Sectors);
            writer.WriteInt32Array(TextureDictionaries);
            writer.WriteInt32(Count);
            writer.WriteInt16(DrawCounter);
            writer.WriteInt16(DrawCounterTexSets);
            writer.WriteByte(LOD);
            writer.WriteBoolean(IsPagedOut);
            writer.WriteByte(StreamPriority);
            writer.WriteByte(Unknown_33h);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("Count", Count);
            writer.WriteInt16("DrawCounter", DrawCounter);
            writer.WriteInt16("DrawCounterTexSets", DrawCounterTexSets);
            writer.WriteByte("LOD", LOD);
            writer.WriteBool("IsPagedOut", IsPagedOut);
            writer.WriteByte("StreamPriority", StreamPriority);
            writer.WriteInt32Array("TextureDictionaries", TextureDictionaries);
            writer.WriteNodeArray("Sectors", Sectors.Items);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TerrainSector : Rsc6BlockBase, MetaNode //sagTerrainSector
    {
        public override ulong BlockLength => 80;
        public Vector4 AABBMin { get; set; } //m_MinAndBoundingRadius
        public Vector4 AABBMax { get; set; } //m_MaxAndInscribedRadius
        public JenkHash InstHashCode { get; set; } //m_InstHashCode
        public short Row { get; set; } //m_row
        public short Column { get; set; } //m_column
        public byte Lod { get; set; } //m_lod
        public byte TextureSetsRequested { get; set; } //m_TextureSetsRequested
        public ushort VisualIndex { get; set; } //m_VisualIndex
        public Rsc6Ptr<Rsc6TerrainVisual> Visual { get; set; } //m_Visual, points to the visual parent...
        public uint Unknown_30h { get; set; } //Always 0
        public Rsc6Arr<short> TextureSets { get; set; } //m_TextureSets
        public Rsc6Arr<short> TextureSetAABBMatch { get; set; } //m_TextureSetAABBMatch
        public Rsc6Arr<BoundingBox4> TextureSetAABBs { get; set; } //m_TextureSetAABBs
        public ushort SectorIndex { get; set; } //m_SectorIndex
        public bool SplitSector { get; set; } //m_SplitSector, always false
        public byte Unknown_4F { get; set; } //Padding? Always 0

        public override void Read(Rsc6DataReader reader)
        {
            AABBMin = reader.ReadVector4();
            AABBMax = reader.ReadVector4();
            InstHashCode = reader.ReadUInt32();
            Row = reader.ReadInt16();
            Column = reader.ReadInt16();
            Lod = reader.ReadByte();
            TextureSetsRequested = reader.ReadByte();
            VisualIndex = reader.ReadUInt16();
            _ = reader.ReadUInt32(); //Placeholder to avoid recursively reading sectors & getting a stack overflow (m_Visual)
            Unknown_30h = reader.ReadUInt32();
            TextureSets = reader.ReadArr<short>();
            TextureSetAABBMatch = reader.ReadArr<short>();
            TextureSetAABBs = reader.ReadArr<BoundingBox4>();
            SectorIndex = reader.ReadUInt16();
            SplitSector = reader.ReadBoolean();
            Unknown_4F = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(AABBMin);
            writer.WriteVector4(AABBMax);
            writer.WriteUInt32(InstHashCode);
            writer.WriteInt16(Row);
            writer.WriteInt16(Column);
            writer.WriteByte(Lod);
            writer.WriteByte(TextureSetsRequested);
            writer.WriteUInt16(VisualIndex);
            writer.WritePtr(Visual);
            writer.WriteUInt32(Unknown_30h);
            writer.WriteArr(TextureSets);
            writer.WriteArr(TextureSetAABBMatch);
            writer.WriteArr(TextureSetAABBs);
            writer.WriteUInt16(SectorIndex);
            writer.WriteBoolean(SplitSector);
            writer.WriteByte(Unknown_4F);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("AABBMin", AABBMin);
            writer.WriteVector4("AABBMax", AABBMax);
            writer.WriteJenkHash("InstHashCode", InstHashCode);
            writer.WriteInt16("Row", Row);
            writer.WriteInt16("Column", Column);
            writer.WriteByte("Lod", Lod);
            writer.WriteByte("TextureSetsRequested", TextureSetsRequested);
            writer.WriteUInt16("VisualIndex", VisualIndex);
            writer.WriteUInt16("SectorIndex", SectorIndex);
            writer.WriteBool("SplitSector", SplitSector);
            writer.WriteInt16Array("TextureSets", TextureSets.Items);
            writer.WriteInt16Array("TextureSetAABBMatch", TextureSetAABBMatch.Items);
            //writer.WriteStructArray("TextureSetAABBs", TextureSetAABBs.Items); //TODO: fix this, there's sadly no MetaNode.WriteBoundingBox4
        }

        public override string ToString()
        {
            return InstHashCode.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TerrainTextureDictionary : Rsc6BlockBase, MetaNode //terTextureDictionary
    {
        /*
         * Reference to a texture dictionary (#td), has to be in the "mapres" folder.
         * Only the "_dst_x_hilod" dictionaries seem to be referenced.
         */

        public override ulong BlockLength => 24;
        public Rsc6Str Name { get; set; } //m_Name
        public Rsc6StreamableBase TextureDictionary { get; set; } //m_TextureDictionary
        public uint RefCount { get; set; } //m_RefCount

        public override void Read(Rsc6DataReader reader)
        {
            Name = reader.ReadStr();
            TextureDictionary = reader.ReadStruct<Rsc6StreamableBase>();
            RefCount = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteStr(Name);
            writer.WriteStruct(TextureDictionary);
            writer.WriteUInt32(RefCount);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", Name.ToString());
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TerrainTextureEntries : Rsc6BlockBase, IRsc6DataMapEntry<Rsc6TerrainTextureEntries> //rage::atMapEntry<rage::ConstString, int>
    {
        public override ulong BlockLength => 12;
        public Rsc6Str Name { get; set; } //m_String
        public int Data { get; set; } //data
        public Rsc6Ptr<Rsc6TerrainTextureEntries> Next { get; set; } //next

        public uint MapKey { get => (uint)Data; }
        public Rsc6TerrainTextureEntries MapNext { get => Next.Item; set => Next = new(value); }

        public override void Read(Rsc6DataReader reader)
        {
            Name = reader.ReadStr();
            Data = reader.ReadInt32();
            Next = reader.ReadPtr<Rsc6TerrainTextureEntries>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteStr(Name);
            writer.WriteInt32(Data);
            writer.WritePtr(Next);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TerrainTextureSet : Rsc6BlockBase, MetaNode //terTextureSet
    {
        public override ulong BlockLength => 736;
        public byte[] ReferencingSectors { get; set; } //m_ReferencingSectors, zero bytes, seems to be a buffer to dynamically reference sectors that will use this texture asset
        public int Count { get; set; } //m_Count
        public Rsc6StreamableBase[] Dictionaries { get; set; } = new Rsc6StreamableBase[(int)Rsc6LodLevel.LOD_COUNT]; //m_Dictionaries[4]
        public int TextureSet { get; set; } //m_TextureSet, index of this terTextureSet in the array
        public float TextureSetDistMod { get; set; } //m_TextureSetDistMod
        public int CurrentLOD { get; set; } //m_CurrentLOD
        public int LODUsedBySectors { get; set; } //m_LODUsedBySectors
        public bool[] Requested { get; set; } = new bool[(int)Rsc6LodLevel.LOD_COUNT]; //m_Requested[4]
        public byte[] RefCount { get; set; } = new byte[(int)Rsc6LodLevel.LOD_COUNT]; //m_RefCount[4]
        public Rsc6Str TextureSetAssetName { get; set; } //m_TextureSetAssetName

        public override void Read(Rsc6DataReader reader)
        {
            ReferencingSectors = reader.ReadBytes(640);
            Count = reader.ReadInt32();

            for (int i = 0; i < Dictionaries.Length; i++)
            {
                Dictionaries[i] = reader.ReadStruct<Rsc6StreamableBase>();
            }

            TextureSet = reader.ReadInt32();
            TextureSetDistMod = reader.ReadSingle();
            CurrentLOD = reader.ReadInt32();
            LODUsedBySectors = reader.ReadInt32();

            for (int i = 0; i < Requested.Length; i++)
            {
                Requested[i] = reader.ReadBoolean();
            }

            for (int i = 0; i < RefCount.Length; i++)
            {
                RefCount[i] = reader.ReadByte();
            }

            TextureSetAssetName = reader.ReadStr();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteBytes(ReferencingSectors);
            writer.WriteInt32(Count);
            writer.WriteStructArray(Dictionaries);
            writer.WriteInt32(TextureSet);
            writer.WriteSingle(TextureSetDistMod);
            writer.WriteInt32(CurrentLOD);
            writer.WriteInt32(LODUsedBySectors);
            writer.WriteStructArray(Requested);
            writer.WriteStructArray(RefCount);
            writer.WriteStr(TextureSetAssetName);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("TextureSetAssetName", TextureSetAssetName.ToString());
            writer.WriteInt32("Count", Count);
            writer.WriteInt32("TextureSet", TextureSet);
            writer.WriteInt32("CurrentLOD", CurrentLOD);
            writer.WriteInt32("LODUsedBySectors", LODUsedBySectors);
            writer.WriteSingle("TextureSetDistMod", TextureSetDistMod);
        }

        public override string ToString()
        {
            return TextureSetAssetName.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6GridBucket : Rsc6BlockBase, MetaNode //gridBucket
    {
        public override ulong BlockLength => 20;
        public Rsc6Arr<short> BucketArray { get; set; } //m_BucketArray
        public short RowMin { get; set; } //m_sectorLOD_min_ROW
        public short ColumnMin { get; set; } //m_sectorLOD_min_COL
        public short RowMax { get; set; } //m_sectorLOD_max_ROW
        public short ColumnMax { get; set; } //m_sectorLOD_max_COL
        public short GridSizeRow { get; set; } //m_grid_size_r
        public short GridSizeColumn { get; set; } //m_grid_size_c

        public override void Read(Rsc6DataReader reader)
        {
            BucketArray = reader.ReadArr<short>();
            RowMin = reader.ReadInt16();
            ColumnMin = reader.ReadInt16();
            RowMax = reader.ReadInt16();
            ColumnMax = reader.ReadInt16();
            GridSizeRow = reader.ReadInt16();
            GridSizeColumn = reader.ReadInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(BucketArray);
            writer.WriteInt16(RowMin);
            writer.WriteInt16(ColumnMin);
            writer.WriteInt16(RowMax);
            writer.WriteInt16(ColumnMax);
            writer.WriteInt16(GridSizeRow);
            writer.WriteInt16(GridSizeColumn);
        }

        public void Read(MetaNodeReader reader)
        {
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt16("RowMin", RowMin);
            writer.WriteInt16("RowMax", RowMax);
            writer.WriteInt16("ColumnMin", ColumnMin);
            writer.WriteInt16("ColumnMax", ColumnMax);
            writer.WriteInt16("GridSizeRow", GridSizeRow);
            writer.WriteInt16("GridSizeColumn", GridSizeColumn);
            writer.WriteInt16Array("BucketArray", BucketArray.Items);
        }
    }

    [TC(typeof(EXP))]
    public struct Rsc6StreamableBase(bool fragment) //rage::pgStreamableBase
    {
        /*
         * A base class in RAGE Engine that seems to be used for handling streamed resources.
         * Manages assets such as models, textures and map elements that need to be loaded and unloaded dynamically (can track the residency state).
         *
         * Assets are scheduled for loading, a scheduler processes assets every frame, they're managed in priority queues.
         * They can be evicted when no longer loading using pgStreamableBase::Shutdown().
         *
         * Basic Scheduler (by default) loads assets every frame.
         * pgQueueScheduler ensures assets are loaded only once.
         */

        public uint VFT { get; set; } //Always 0, placeholder
        public int Handle { get; set; } = -1;
        public uint Unknown_8h { get; set; } = fragment ? 0u : 64u;
        public uint State { get; set; } //Always 0

        public readonly int GetVirtualSize() //rage::pgStreamableBase::GetVirtualSize
        {
            return (Handle == -1) ? -1 : (Handle & 0x7FFF) << 12;
        }

        public readonly int GetPhysicalSize() //rage::pgStreamableBase::GetPhysicalSize
        {
            return (Handle == -1) ? -1 : (Handle >> 4) & 0x7FFF000;
        }

        public readonly string GetHandleName() //rage::pgStreamableBase::GetHandleName
        {
            /*
             * uint8_t collectionIndex = (Handle >> 16) & 0xFF;
             * rage::fiCollection* collection = rage::fiCollection::sm_Collections[collectionIndex];
             * return collection->GetFullName(Handle);
             */

            if (Handle == -1)
                return "[Invalid pgStreamable]";
            else if ((Handle & 0x80000000) != 0)
                return "Memory allocation";
            else
                return string.Format("Resource with handle 0x{0:X}", Handle);
        }

        public void SetState(uint state) //rage::pgStreamableBase::SetState
        {
            State = (State & 0xCFFFFFFF) | state;
        }
    }

    public enum Rsc6PlacedLightType : uint
    {
        Point = 0, //Local point/omi/fill light
        Spot = 1 //Spot/key/camera-directional style light
    }

    public enum Rsc6AttributeClassType : uint
    {
        NAME_ATTRIBUTE = 1, //RDR2NameAttribute
        GRINGO_ATTRIBUTE = 2, //RDR2GringoAttribute
        DOOR_ATTRIBUTE = 3, //RDR2DoorAttribute, not used
        PROP_INSTANCE_ATTRIBUTE = 4, //RDR2PropInstanceAttribute, not used
        DOOR_LOCK_ATTRIBUTE = 5 //RDR2DoorLockAttribute
    }

    public enum Rsc6AttributeTargetType : uint
    {
        NONE = 0,
        PROP_INSTANCE = 1,
        MATRIX_AT_OFFSET80 = 2,
        MATRIX_PROVIDER = 3,
        MATRIX_PROVIIDER_ALT = 4
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