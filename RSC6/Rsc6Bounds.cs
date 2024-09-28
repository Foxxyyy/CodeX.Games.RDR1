using System;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeX.Core.Engine;
using CodeX.Core.Physics;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6TerrainBound : Rsc6BlockBaseMap, MetaNode //terrainBoundTile
    {
        public override ulong BlockLength => 40; //terrainBoundTile + terrainTileScanData
        public override uint VFT { get; set; } = 0x04A007B8;
        public Rsc6Ptr<Rsc6TerrainDictBoundResource> ResourceDict { get; set; } //m_ResourceDict
        public uint PointMaterials_AND { get; set; } //m_PointMaterials_AND
        public uint Unknown_10h { get; set; } //Always 0
        public uint PointMaterials_OR { get; set; } //m_PointMaterials_OR
        public float MinElevation { get; set; } //m_MinElevation
        public float ElevationRange { get; set; } //m_ElevationRange
        public Rsc6ManagedArr<Rsc6ScanData> ScanData { get; set; } //m_ScanData

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ResourceDict = reader.ReadPtr<Rsc6TerrainDictBoundResource>();
            PointMaterials_AND = reader.ReadUInt32();
            Unknown_10h = reader.ReadUInt32();
            PointMaterials_OR = reader.ReadUInt32();
            MinElevation = reader.ReadSingle();
            ElevationRange = reader.ReadSingle();
            ScanData = reader.ReadArr<Rsc6ScanData>();
        } //14802140_bnd

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(ResourceDict);
            writer.WriteUInt32(PointMaterials_AND);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteUInt32(PointMaterials_OR);
            writer.WriteSingle(MinElevation);
            writer.WriteSingle(ElevationRange);
            writer.WriteArr(ScanData);
        }

        public void Read(MetaNodeReader reader)
        {
            ResourceDict = new(reader.ReadNode<Rsc6TerrainDictBoundResource>("ResourceDict"));
            PointMaterials_AND = reader.ReadUInt32("PointMaterials_AND");
            PointMaterials_OR = reader.ReadUInt32("PointMaterials_OR");
            MinElevation = reader.ReadSingle("MinElevation");
            ElevationRange = reader.ReadSingle("ElevationRange");
            ScanData = new(reader.ReadNodeArray<Rsc6ScanData>("ScanData"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("PointMaterials_AND", PointMaterials_AND);
            writer.WriteUInt32("PointMaterials_OR", PointMaterials_OR);
            writer.WriteSingle("MinElevation", MinElevation);
            writer.WriteSingle("ElevationRange", ElevationRange);
            writer.WriteNode("ResourceDict", ResourceDict.Item);
            writer.WriteNodeArray("ScanData", ScanData.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6TerrainDictBoundResource : Rsc6BlockBaseMap, MetaNode //pgDictionary<terrainBoundResource>
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x04A007D0;
        public uint Unknown_8h { get; set; } //Always 0?
        public uint RefCount { get; set; } //m_RefCount
        public Rsc6Arr<JenkHash> Codes { get; set; } //m_Codes
        public Rsc6PtrArr<Rsc6TerrainBoundResource> Entries { get; set; } //m_Entries

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            RefCount = reader.ReadUInt32();
            Codes = reader.ReadArr<JenkHash>();
            Entries = reader.ReadPtrArr<Rsc6TerrainBoundResource>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(RefCount);
            writer.WriteArr(Codes);
            writer.WritePtrArr(Entries);
        }

        public void Read(MetaNodeReader reader)
        {
            RefCount = reader.ReadUInt32("RefCount");
            Codes = new(reader.ReadJenkHashArray("Codes"));
            Entries = new(reader.ReadNodeArray<Rsc6TerrainBoundResource>("Entries"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("RefCount", RefCount);
            writer.WriteJenkHashArray("Codes", Codes.Items);
            writer.WriteNodeArray("Entries", Entries.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6TerrainBoundResource : Rsc6BlockBase, MetaNode //terrainBoundResource
    {
        public override ulong BlockLength => 12;
        public Rsc6Ptr<Rsc6Bounds> Bounds { get; set; } //m_Bound, same as m_Archetype.Bounds
        public Rsc6Ptr<Rsc6FragArchetype> Archetype { get; set; } //m_Archetype
        public Rsc6Ptr<Rsc6TerrainDictBoundInstance> InstanceDict { get; set; } //m_InstanceDict

        public override void Read(Rsc6DataReader reader)
        {
            Bounds = reader.ReadPtr(Rsc6Bounds.Create);
            Archetype = reader.ReadPtr<Rsc6FragArchetype>();
            InstanceDict = reader.ReadPtr<Rsc6TerrainDictBoundInstance>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtr(Bounds);
            writer.WritePtr(Archetype);
            writer.WritePtr(InstanceDict);
        }

        public void Read(MetaNodeReader reader)
        {
            InstanceDict = new(reader.ReadNode<Rsc6TerrainDictBoundInstance>("InstanceDict"));
            if (InstanceDict.Item?.Entries.Items != null)
            {
                var entry = InstanceDict.Item?.Entries.Items[0];
                if (entry != null && entry.Instance.Item?.Archetype != null)
                {
                    var arch = entry.Instance.Item.Archetype;
                    Archetype = arch;
                    Bounds = arch.Item.Bounds;
                }
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("InstanceDict", InstanceDict.Item);
        }
    }

    [TC(typeof(EXP))] public class Rsc6TerrainDictBoundInstance : Rsc6BlockBaseMap, MetaNode //pgDictionary<terrainBoundInstance>
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x04A007D0;
        public uint Unknown_8h { get; set; } //Always 0?
        public uint RefCount { get; set; } //m_RefCount
        public Rsc6Arr<JenkHash> Codes { get; set; } //m_Codes
        public Rsc6PtrArr<Rsc6TerrainBoundInstance> Entries { get; set; } //m_Entries

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            RefCount = reader.ReadUInt32();
            Codes = reader.ReadArr<JenkHash>();
            Entries = reader.ReadPtrArr<Rsc6TerrainBoundInstance>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(RefCount);
            writer.WriteArr(Codes);
            writer.WritePtrArr(Entries);
        }

        public void Read(MetaNodeReader reader)
        {
            RefCount = reader.ReadUInt32("RefCount");
            Codes = new(reader.ReadJenkHashArray("Codes"));
            Entries = new(reader.ReadNodeArray<Rsc6TerrainBoundInstance>("Entries"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("RefCount", RefCount);
            writer.WriteJenkHashArray("Codes", Codes.Items);
            writer.WriteNodeArray("Entries", Entries.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6TerrainBoundInstance : Rsc6BlockBase, MetaNode //terrainBoundInstance
    {
        public override ulong BlockLength => 4;
        public Rsc6Ptr<Rsc6PhysicsInstance> Instance { get; set; } //m_phInst

        public override void Read(Rsc6DataReader reader)
        {
            Instance = reader.ReadPtr<Rsc6PhysicsInstance>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtr(Instance);
        }

        public void Read(MetaNodeReader reader)
        {
            Instance = new(reader.ReadNode<Rsc6PhysicsInstance>("Instance"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("Instance", Instance.Item);
        }
    }

    [TC(typeof(EXP))] public class Rsc6ScanData : IRsc6Block, MetaNode //terrainTileScanData
    {
        public ulong BlockLength => 8;
        public bool IsPhysical => false;
        public ulong FilePosition { get; set; }
        public uint MaterialId { get; set; } //m_MaterialId
        public uint PackedElevation { get; set; } //m_PackedElevationSlopeBump

        public void Read(Rsc6DataReader reader)
        {
            MaterialId = reader.ReadUInt32();
            PackedElevation = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(MaterialId);
            writer.WriteUInt32(PackedElevation);
        }

        public void Read(MetaNodeReader reader)
        {
            MaterialId = reader.ReadUInt32("MaterialId");
            PackedElevation = reader.ReadUInt32("PackedElevation");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("MaterialId", MaterialId);
            writer.WriteUInt32("PackedElevation", PackedElevation);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundsDictionary : Rsc6BlockBaseMap, MetaNode
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x01830BC0;
        public JenkHash ParentDictionary { get; set; }
        public uint UsageCount { get; set; } = 1;
        public Rsc6Arr<JenkHash> Hashes { get; set; }
        public Rsc6PtrArr<Rsc6Bounds> Bounds { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ParentDictionary = reader.ReadUInt32();
            UsageCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Bounds = reader.ReadPtrArr(Rsc6Bounds.Create);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(ParentDictionary);
            writer.WriteUInt32(UsageCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Bounds);
        }

        public void Read(MetaNodeReader reader)
        {
            ParentDictionary = reader.ReadJenkHash("ParentDictionary");
            Hashes = new(reader.ReadJenkHashArray("Hashes"));
            Bounds = new(reader.ReadNodeArray("Bounds", Rsc6Bounds.Create));
        }

        public void Write(MetaNodeWriter writer)
        {
            if (ParentDictionary != 0) writer.WriteJenkHash("ParentDictionary", ParentDictionary);
            writer.WriteJenkHashArray("Hashes", Hashes.Items);
            writer.WriteNodeArray("Bounds", Bounds.Items);
        }
    }

    public enum Rsc6BoundsType : byte
    {
        Sphere = 0, //phBoundSphere
        Capsule = 1, //phBoundCapsule
        TaperedCapsule = 2, //phBoundTaperedCapsule
        Box = 3, //phBoundBox
        Geometry = 4, //phBoundGeometry
        CurvedGeometry = 5, //phBoundCurvedGeometry
        Octree = 6,
        Quadtree = 7,
        Grid = 8, //phBoundGrid
        Ribbon = 9, //phBoundRibbon
        GeometryBVH = 10, //phBoundBVH
        Surface = 11, //phBoundSurface
        Composite = 12, //phBoundComposite
        Triangle = 13
    }

    [TC(typeof(EXP))] public class Rsc6Bounds : Collider, IRsc6Block
    {
        public virtual ulong BlockLength => 144;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public uint[] UserData { get; set; } //m_UserData[4]
        public uint VFT { get; set; }
        public Rsc6BoundsType Type { get; set; } //m_Type
        public bool HasCentroidOffset { get; set; } //m_HasCentroidOffset
        public bool HasCGOffset { get; set; } //m_HasCGOffset
        public bool WorldSpaceUpdatesEnabled { get; set; } //m_WorldSpaceUpdatesEnabled
        public float SphereRadius { get; set; } //m_RadiusAroundCentroid, upper bound on the distance from center to any point in this bound.
        public float WorldRadius { get; set; } //m_RadiusAroundLocalOrigin
        public Vector3 BoxMax { get; set; } //m_BoundingBoxMax
        public float Unknown_2Ch { get; set; } = Rpf6Crypto.NaN(); //0x0100807F
        public Vector3 BoxMin { get; set; } //m_BoundingBoxMin
        public float Unknown_3Ch { get; set; } = Rpf6Crypto.NaN(); //0x0100807F
        public Vector3 BoxCenter { get; set; } //m_CentroidOffset, offset of the centroid from the local coordinate system origin
        public float Unknown_4Ch { get; set; } = Rpf6Crypto.NaN(); //0x0100807F
        public Vector3 CentroidOffsetWorldSpace { get; set; } //m_CentroidOffsetWorldSpace
        public float Unknown_5Ch { get; set; } = Rpf6Crypto.NaN(); //0x0100807F
        public Vector3 SphereCenter { get; set; } //m_CGOffset, center of gravity location in the local coordinate system
        public float Unknown_6Ch { get; set; } = Rpf6Crypto.NaN(); //0x0100807F
        public Vector4 VolumeDistribution { get; set; } //m_VolumeDistribution, angular inertia that this bound would have with a mass of 1kg and the bound's volume (element w).
        public Vector3 Margin { get; set; } //m_MarginV, the distance by which collision detection will be expanded beyond the bound's surface
        public uint RefCount { get; set; } = 1; //Number of physics instances (or sometimes other classes) using this bound, mostly 1, can also be 2

        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity; //When it's the child of a bound composite
        public Matrix4x4 TransformInv { get; set; } = Matrix4x4.Identity;

        public Rsc6Bounds()
        {
        }

        public Rsc6Bounds(Rsc6BoundsType type)
        {
            Type = type;
            InitCollider(GetEngineType(type));
        }

        public virtual void Read(Rsc6DataReader reader) //phBounds
        {
            UserData = reader.ReadUInt32Arr(4);
            VFT = reader.ReadUInt32();
            Type = (Rsc6BoundsType)reader.ReadByte();
            HasCentroidOffset = reader.ReadBoolean();
            HasCGOffset = reader.ReadBoolean();
            WorldSpaceUpdatesEnabled = reader.ReadBoolean();
            SphereRadius = reader.ReadSingle();
            WorldRadius = reader.ReadSingle();
            BoxMax = reader.ReadVector3();
            Unknown_2Ch = reader.ReadSingle();
            BoxMin = reader.ReadVector3();
            Unknown_3Ch = reader.ReadSingle();
            BoxCenter = reader.ReadVector3();
            Unknown_4Ch = reader.ReadSingle();
            CentroidOffsetWorldSpace = reader.ReadVector3();
            Unknown_5Ch = reader.ReadSingle();
            SphereCenter = reader.ReadVector3();
            Unknown_6Ch = reader.ReadSingle();
            VolumeDistribution = reader.ReadVector4();
            Margin = reader.ReadVector3();
            RefCount = reader.ReadUInt32();
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32Array(UserData);
            writer.WriteUInt32(VFT);
            writer.WriteByte((byte)Type);
            writer.WriteBoolean(HasCentroidOffset);
            writer.WriteBoolean(HasCGOffset);
            writer.WriteBoolean(WorldSpaceUpdatesEnabled);
            writer.WriteSingle(SphereRadius);
            writer.WriteSingle(WorldRadius);
            writer.WriteVector3(BoxMax);
            writer.WriteSingle(Unknown_2Ch);
            writer.WriteVector3(BoxMin);
            writer.WriteSingle(Unknown_3Ch);
            writer.WriteVector3(BoxCenter);
            writer.WriteSingle(Unknown_4Ch);
            writer.WriteVector3(CentroidOffsetWorldSpace);
            writer.WriteSingle(Unknown_5Ch);
            writer.WriteVector3(SphereCenter);
            writer.WriteSingle(Unknown_6Ch);
            writer.WriteVector4(VolumeDistribution);
            writer.WriteVector3(Margin);
            writer.WriteUInt32(RefCount);
        }

        public override void Read(MetaNodeReader reader)
        {
            Type = reader.ReadEnum("@type", Rsc6BoundsType.GeometryBVH);
            UserData = reader.ReadUInt32Array("UserData");
            HasCentroidOffset = reader.ReadBool("HasCentroidOffset");
            HasCGOffset = reader.ReadBool("HasCGOffset");
            WorldSpaceUpdatesEnabled = reader.ReadBool("WorldSpaceUpdatesEnabled");
            SphereRadius = reader.ReadSingle("SphereRadius");
            WorldRadius = reader.ReadSingle("WorldRadius");
            BoxMax = Rpf6Crypto.ToXYZ(reader.ReadVector3("BoxMax"));
            BoxMin = Rpf6Crypto.ToXYZ(reader.ReadVector3("BoxMin"));
            BoxCenter = Rpf6Crypto.ToXYZ(reader.ReadVector3("BoxCenter"));
            CentroidOffsetWorldSpace = Rpf6Crypto.ToXYZ(reader.ReadVector3("CentroidOffsetWorldSpace"));
            SphereCenter = Rpf6Crypto.ToXYZ(reader.ReadVector3("SphereCenter"));
            VolumeDistribution = Rpf6Crypto.ToXYZ(reader.ReadVector4("VolumeDistribution"));
            Margin = Rpf6Crypto.ToXYZ(reader.ReadVector3("Margin"));
            RefCount = reader.ReadUInt32("NumPhysicsIntance");
        }

        public override void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", Type.ToString());
            writer.WriteUInt32Array("UserData", UserData);
            writer.WriteBool("HasCentroidOffset", HasCentroidOffset);
            writer.WriteBool("HasCGOffset", HasCGOffset);
            writer.WriteBool("WorldSpaceUpdatesEnabled", WorldSpaceUpdatesEnabled);
            writer.WriteSingle("SphereRadius", SphereRadius);
            writer.WriteSingle("WorldRadius", WorldRadius);
            writer.WriteVector3("BoxMax", BoxMax);
            writer.WriteVector3("BoxMin", BoxMin);
            writer.WriteVector3("BoxCenter", BoxCenter);
            writer.WriteVector3("CentroidOffsetWorldSpace", CentroidOffsetWorldSpace);
            writer.WriteVector3("SphereCenter", SphereCenter);
            writer.WriteVector4("VolumeDistribution", VolumeDistribution);
            writer.WriteVector3("Margin", Margin);
            writer.WriteUInt32("NumPhysicsIntance", RefCount);
        }

        public static Rsc6Bounds Create(string typeName)
        {
            if (Enum.TryParse(typeName, out Rsc6BoundsType type))
            {
                return Create(type);
            }
            return null;
        }

        public static Rsc6Bounds Create(Rsc6DataReader r)
        {
            r.Position += 20;
            var type = (Rsc6BoundsType)r.ReadByte();
            r.Position -= 21;
            return Create(type);
        }

        public static Rsc6Bounds Create(Rsc6BoundsType type)
        {
            return type switch
            {
                Rsc6BoundsType.Sphere => new Rsc6BoundSphere(),
                Rsc6BoundsType.Capsule => new Rsc6BoundCapsule(),
                Rsc6BoundsType.Box => new Rsc6BoundBox(),
                Rsc6BoundsType.Geometry => new Rsc6BoundGeometry(),
                Rsc6BoundsType.GeometryBVH => new Rsc6BoundGeometryBVH(),
                Rsc6BoundsType.Surface => new Rsc6BoundSurface(),
                Rsc6BoundsType.Composite => new Rsc6BoundComposite(),
                Rsc6BoundsType.CurvedGeometry => new Rsc6BoundCurvedGeometry(),
                _ => throw new Exception("Unknown bounds type"),
            };
        }

        public static ColliderType GetEngineType(Rsc6BoundsType t)
        {
            return t switch
            {
                Rsc6BoundsType.Sphere => ColliderType.Sphere,
                Rsc6BoundsType.Capsule => ColliderType.Capsule,
                Rsc6BoundsType.TaperedCapsule => ColliderType.Capsule2,
                Rsc6BoundsType.Box => ColliderType.Box,
                Rsc6BoundsType.Geometry => ColliderType.Mesh,
                Rsc6BoundsType.GeometryBVH => ColliderType.Mesh,
                Rsc6BoundsType.Surface => ColliderType.Mesh,
                Rsc6BoundsType.Composite => ColliderType.None,
                Rsc6BoundsType.CurvedGeometry => ColliderType.Mesh,
                Rsc6BoundsType.Triangle => ColliderType.Triangle,
                _ => ColliderType.None,
            };
        }

        public override string ToString()
        {
            return $"{Type} : {BoxMin} : {BoxMax}";
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundSphere : Rsc6Bounds
    {
        public override ulong BlockLength => base.BlockLength + 24;
        public Vector4 Radius { get; set; }
        public Rsc6BoundMaterial Material { get; set; }
        public uint Unknown_A4h { get; set; }

        public Rsc6BoundSphere() : base(Rsc6BoundsType.Sphere)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Radius = reader.ReadVector4();
            Material = reader.ReadStruct<Rsc6BoundMaterial>();
            Unknown_A4h = reader.ReadUInt32();
            InitSpherePart();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4(Radius);
            Material.Write(writer);
            writer.WriteUInt32(Unknown_A4h);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Radius = Rpf6Crypto.ToXYZ(reader.ReadVector4("Radius"));
            Material = reader.ReadStruct<Rsc6BoundMaterial>("Material");
            InitSpherePart();
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("Radius", Radius);
            writer.WriteStruct("Material", Material);
        }

        private void InitSpherePart()
        {
            PartColour = Material.Type.Colour;
            PartSize = new Vector3(SphereRadius, 0.0f, 0.0f);
            ComputeMass(ColliderType.Sphere, PartSize, 1.0f);
            ComputeBodyInertia();
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundCapsule : Rsc6Bounds
    {
        public override ulong BlockLength => 240; //144 + 96
        public Vector4 CapsuleRadius { get; set; } //m_CapsuleRadius
        public Vector4 CapsuleLength { get; set; } //m_CapsuleLength
        public Vector4 EndPointsWorldSpace0 { get; set; } //m_EndPointsWorldSpace[0]
        public Vector4 EndPointsWorldSpace1 { get; set; } //m_EndPointsWorldSpace[1]
        public Vector4 Axis { get; set; } //m_Axis
        public Rsc6BoundMaterial Material { get; set; } //m_MaterialId
        public uint Unknown_54h { get; set; } //Padding
        public uint Unknown_58h { get; set; } //Padding
        public new uint Unknown_5Ch { get; set; } //Padding

        public Rsc6BoundCapsule() : base(Rsc6BoundsType.Capsule)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBound
            CapsuleRadius = reader.ReadVector4();
            CapsuleLength = reader.ReadVector4();
            EndPointsWorldSpace0 = reader.ReadVector4();
            EndPointsWorldSpace1 = reader.ReadVector4();
            Axis = reader.ReadVector4();
            Material = reader.ReadStruct<Rsc6BoundMaterial>();
            Unknown_54h = reader.ReadUInt32();
            Unknown_58h = reader.ReadUInt32();
            Unknown_5Ch = reader.ReadUInt32();
            InitCapsulePart();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4(CapsuleRadius);
            writer.WriteVector4(CapsuleLength);
            writer.WriteVector4(EndPointsWorldSpace0);
            writer.WriteVector4(EndPointsWorldSpace1);
            Material.Write(writer);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            CapsuleRadius = Rpf6Crypto.ToXYZ(reader.ReadVector4("CapsuleRadius"));
            CapsuleLength = Rpf6Crypto.ToXYZ(reader.ReadVector4("CapsuleLength"));
            EndPointsWorldSpace0 = Rpf6Crypto.ToXYZ(reader.ReadVector4("EndPointsWorldSpace0"));
            EndPointsWorldSpace1 = Rpf6Crypto.ToXYZ(reader.ReadVector4("EndPointsWorldSpace1"));
            Material = reader.ReadStruct<Rsc6BoundMaterial>("Material");
            InitCapsulePart();
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("CapsuleRadius", CapsuleRadius);
            writer.WriteVector4("CapsuleLength", CapsuleLength);
            writer.WriteVector4("EndPointsWorldSpace0", EndPointsWorldSpace0);
            writer.WriteVector4("EndPointsWorldSpace1", EndPointsWorldSpace1);
            writer.WriteStruct("Material", Material);
        }

        private void InitCapsulePart()
        {
            PartColour = Material.Type.Colour;
            PartSize = new Vector3(CapsuleRadius.X, CapsuleLength.X, CapsuleRadius.X); //CapsuleLength, CapsuleRadius
            ComputeMass(ColliderType.Capsule, PartSize, 1.0f);
            ComputeBodyInertia();
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundBox : Rsc6BoundPolyhedron
    {
        //A class to represent a physics bound in the shape of a rectangular prism.
        //A phBoundBox is specified by its length, width and height.
        //The principal axes of the box are always the local axes of the bound.
        //Boxes can be created used polygons (via a phBoundPolyhedron) but using a phBoundBox allows for greater efficiency

        public override ulong BlockLength => base.BlockLength + 352;
        public Vector4 BoxSize { get; set; } //m_BoxSize
        public Vector4[] PlainVertices { get; set; } //Vec3V[8] equal to (VerticesData[i] * Quantum) + CenterGeom;
        public uint[] Unknown_170h { get; set; } //(0xCDCDCDCD + 0x00000000 + 0x0000 0xFFFFFFFF + 0xFFFF) x 12 -> 16 bytes * 12
        public Rsc6BoundMaterial Material { get; set; }
        public uint Unknown_154h { get; set; } //Padding
        public uint Unknown_158h { get; set; } //Padding
        public uint Unknown_15Ch { get; set; } //Padding

        public Rsc6BoundBox(Rsc6BoundsType type = Rsc6BoundsType.Box) : base(type)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBoundPolyhedron Rsc6FragArchetype
            BoxSize = reader.ReadVector4();
            PlainVertices = reader.ReadVector4Arr(8);
            Unknown_170h = reader.ReadUInt32Arr(48); //TODO: research this (3452816845, 0, uint.MaxValue, uint.MaxValue) x 12
            Material = reader.ReadStruct<Rsc6BoundMaterial>();
            Unknown_154h = reader.ReadUInt32();
            Unknown_158h = reader.ReadUInt32();
            Unknown_15Ch = reader.ReadUInt32();
            VertexColours = reader.ReadRawArrItems(VertexColours, VerticesCount);
            VerticesData = reader.ReadRawArrItems(VerticesData, VerticesCount);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4(BoxSize);
            writer.WriteVector4Array(PlainVertices);
            writer.WriteUInt32Array(Unknown_170h);
            Material.Write(writer);
            writer.WriteUInt32(Unknown_154h);
            writer.WriteUInt32(Unknown_158h);
            writer.WriteUInt32(Unknown_15Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            BoxSize = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoxSize"));
            Unknown_170h = reader.ReadUInt32Array("Unknown_170h");
            Material = reader.ReadStruct<Rsc6BoundMaterial>("Material");

            var vertices = new List<Vector4>();
            for (int i = 0; i < Vertices.Length; i++)
            {
                vertices.Add(new Vector4(Vertices[i], Rpf6Crypto.NaN()));
            }
            PlainVertices = vertices.ToArray();
            base.InitPolyhedronPart();
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("BoxSize", BoxSize);
            writer.WriteUInt32Array("Unknown_170h", Unknown_170h);
            writer.WriteStruct("Material", Material);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundGeometry : Rsc6BoundPolyhedron //rage::phBoundGeometry
    {
        /*
         * Represents a physics bound with generalized vertex locations and edge/polygon topology.
         * These are represented internally by a 'polygon soup', that is, an arbitrary set of polygons without any particular topology to them.
         */

        public override ulong BlockLength => base.BlockLength + 16;
        public Rsc6RawArr<Rsc6BoundMaterial> MaterialsIDs { get; set; } //m_MaterialIds, phMaterialMgr
        public uint Unknown_22Ch { get; set; } //pad1[4]
        public Rsc6RawArr<byte> PolyMatIndexList { get; set; } //m_PolyMatIndexList, list of index numbers into this bound's list of material ids, one for each polygon
        public byte MaterialsCount { get; set; } //m_NumMaterials, number of materials that exist on this bound
        public byte Unknown_2Bh { get; set; } //pad[3]
        public new byte Unknown_2Ch { get; set; } //pad[3]
        public byte Unknown_2Dh { get; set; } //pad[3]

        public Rsc6BoundGeometry(Rsc6BoundsType type = Rsc6BoundsType.Geometry) : base(type)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBoundPolyhedron
            MaterialsIDs = reader.ReadRawArrPtr<Rsc6BoundMaterial>();
            Unknown_22Ch = reader.ReadUInt32();
            PolyMatIndexList = reader.ReadRawArrPtr<byte>();
            MaterialsCount = reader.ReadByte();
            Unknown_2Bh = reader.ReadByte();
            Unknown_2Ch = reader.ReadByte();
            Unknown_2Dh = reader.ReadByte();

            MaterialsIDs = reader.ReadRawArrItems(MaterialsIDs, MaterialsCount);
            PolyMatIndexList = reader.ReadRawArrItems(PolyMatIndexList, Math.Max(1, PolygonsCount));

            base.Materials = MaterialsIDs.Items;
            base.PolygonMaterialIndices = PolyMatIndexList.Items;
            base.CreateMesh();
            base.InitPolyhedronPart();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            BuildMaterials();

            writer.WriteRawArr(MaterialsIDs);
            writer.WriteUInt32(Unknown_22Ch);
            writer.WriteRawArr(PolyMatIndexList);
            writer.WriteByte(MaterialsCount);
            writer.WriteByte(Unknown_2Bh);
            writer.WriteByte(Unknown_2Ch);
            writer.WriteByte(Unknown_2Dh);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Materials = reader.ReadStructArray<Rsc6BoundMaterial>("Materials");
            PolyMatIndexList = new(reader.ReadByteArray("PolyMatIndexList"));
            MaterialsCount = (byte)Materials.Length;
            base.InitPolyhedronPart();
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteStructArray("Materials", Materials);
            writer.WriteByteArray("PolyMatIndexList", PolyMatIndexList.Items);
        }

        public void BuildMaterials() //Update Materials and PolyMatIndexList arrays, using custom materials from polys and existing materials
        {
            var matlist = new List<Rsc6BoundMaterial>();
            matlist.AddRange(Materials);

            MaterialsCount = (byte)matlist.Count;
            Materials = matlist.ToArray();
            MaterialsIDs = new Rsc6RawArr<Rsc6BoundMaterial>() { Items = matlist.ToArray() };
            PolyMatIndexList = new Rsc6RawArr<byte>() { Items = PolygonMaterialIndices };
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundCurvedGeometry : Rsc6BoundGeometry //rage::phBoundCurvedGeometry
    {
        //Represents a physics bound with generalized vertex locations and polygons, including curved polygons and curved edges

        public override ulong BlockLength => base.BlockLength + 32;
        public Rsc6RawLst<Rsc6BoundCurvedFace> CurvedFaces { get; set; } //m_CurvedFaces, phCurvedFace
        public Rsc6RawLst<Rsc6BoundCurvedEdge> CurvedEdges { get; set; } //m_CurvedEdges, phCurvedEdge
        public Rsc6RawArr<byte> CurvedFaceMatIndexList { get; set; } //m_CurvedFaceMatIndexLists, list of index numbers into this bound's list of material ids, one for each polygon
        public int NumCurvedFaces { get; set; } //m_NumCurvedFaces
        public int NumCurvedEdges { get; set; } //m_NumCurvedEdges
        public uint Unknown_14h { get; set; } //Padding
        public uint Unknown_18h { get; set; } //Padding
        public uint Unknown_1Ch { get; set; } //Padding

        public Rsc6BoundCurvedGeometry(Rsc6BoundsType type = Rsc6BoundsType.CurvedGeometry) : base(type)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBoundGeometry
            CurvedFaces = reader.ReadRawLstPtr<Rsc6BoundCurvedFace>();
            CurvedEdges = reader.ReadRawLstPtr<Rsc6BoundCurvedEdge>();
            CurvedFaceMatIndexList = reader.ReadRawArrPtr<byte>();
            NumCurvedFaces = reader.ReadInt32();
            NumCurvedEdges = reader.ReadInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();

            CurvedFaces = reader.ReadRawLstItems(CurvedFaces, (uint)NumCurvedFaces);
            CurvedEdges = reader.ReadRawLstItems(CurvedEdges, (uint)NumCurvedEdges);
            CurvedFaceMatIndexList = reader.ReadRawArrItems(CurvedFaceMatIndexList, 6);
        }
        
        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer); //phBoundGeometry
            writer.WriteRawLst(CurvedFaces);
            writer.WriteRawLst(CurvedEdges);
            writer.WriteRawArr(CurvedFaceMatIndexList);
            writer.WriteInt32(NumCurvedFaces);
            writer.WriteInt32(NumCurvedEdges);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(Unknown_1Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            CurvedFaces = new(reader.ReadNodeArray<Rsc6BoundCurvedFace>("CurvedFaces"));
            CurvedEdges = new(reader.ReadNodeArray<Rsc6BoundCurvedEdge>("CurvedEdges"));
            CurvedFaceMatIndexList = new(reader.ReadByteArray("CurvedFaceMatIndexList"));
            NumCurvedFaces = CurvedFaces.Items?.Length ?? 0;
            NumCurvedEdges = CurvedEdges.Items?.Length ?? 0;
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNodeArray("CurvedFaces", CurvedFaces.Items);
            writer.WriteNodeArray("CurvedEdges", CurvedEdges.Items);
            writer.WriteByteArray("CurvedFaceMatIndexList", CurvedFaceMatIndexList.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundCurvedFace : IRsc6Block, MetaNode //phCurvedFace
    {
        //Curved face for a curved geometry bound

        public ulong FilePosition { get; set; }
        public ulong BlockLength => 96;
        public bool IsPhysical => false;
        public Rsc6BoundPolyTriangle Polygons { get; set; }
        public Vector4 CurvatureCenter { get; set; } //m_CurvatureCenter, the center of curvature of the face
        public Vector4 UnitNormal { get; set; } //m_UnitNormal, the unit-length normal vector
        public float OuterRadius { get; set; } //m_OuterRadius, the radius of curvature of the face
        public float InnerRadius { get; set; } //m_InnerRadius, the distance from the center of curvature about which to rotate the local center of curvature
        public float MinCosine { get; set; } //m_MinCosine, the cosine between the local normal and the midpoint normal at the edge of the curved face
        public ushort[] CurvedEdgeIndices { get; set; } //m_CurvedEdgeIndices, the curved polyhedron bound's index numbers for the curved edges in this face
        public ushort[] CurvedEdgePolyIndices { get; set; } //m_CurvedEdgePolyIndices, this face's index numbers for the curved edges in this face
        public int NumCurvedEdges { get; set; } //m_NumCurvedEdges, the number of curved edges in the curved face (it can also have straight edges)
        public ushort FourthVertex { get; set; } //m_FourthVertex, index number of the last vertex if this is a curved quad
        public bool IsCircularFace { get; set; } //m_IsCircularFace, tells whether this curved face is circular, like the side of a wheel -> can be ring-shaped, like the contact surface of a tire
        public byte Unknown_53h { get; set; } //Padding
        public uint Unknown_54h { get; set; } //Padding
        public uint Unknown_58h { get; set; } //Padding
        public uint Unknown_5Ch { get; set; } //Padding

        public void Read(Rsc6DataReader reader)
        {
            Polygons = reader.ReadStruct<Rsc6BoundPolyTriangle>();
            CurvatureCenter = reader.ReadVector4();
            UnitNormal = reader.ReadVector4();
            OuterRadius = reader.ReadSingle();
            InnerRadius = reader.ReadSingle();
            MinCosine = reader.ReadSingle();
            CurvedEdgeIndices = reader.ReadUInt16Arr(4);
            CurvedEdgePolyIndices = reader.ReadUInt16Arr(4);
            NumCurvedEdges = reader.ReadInt32();
            FourthVertex = reader.ReadUInt16();
            IsCircularFace = reader.ReadBoolean();
            Unknown_53h = reader.ReadByte();
            Unknown_54h = reader.ReadUInt32();
            Unknown_58h = reader.ReadUInt32();
            Unknown_5Ch = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteStruct(Polygons);
            writer.WriteVector4(CurvatureCenter);
            writer.WriteVector4(UnitNormal);
            writer.WriteSingle(OuterRadius);
            writer.WriteSingle(InnerRadius);
            writer.WriteSingle(MinCosine);
            writer.WriteUInt16Array(CurvedEdgeIndices);
            writer.WriteUInt16Array(CurvedEdgePolyIndices);
            writer.WriteInt32(NumCurvedEdges);
            writer.WriteUInt16(FourthVertex);
            writer.WriteBoolean(IsCircularFace);
            writer.WriteByte(Unknown_53h);
            writer.WriteUInt32(Unknown_54h);
            writer.WriteUInt32(Unknown_58h);
            writer.WriteUInt32(Unknown_5Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Polygons = reader.ReadStruct<Rsc6BoundPolyTriangle>("Polygons");
            CurvatureCenter = Rpf6Crypto.ToXYZ(reader.ReadVector4("CurvatureCenter"));
            UnitNormal = Rpf6Crypto.ToXYZ(reader.ReadVector4("UnitNormal"));
            OuterRadius = reader.ReadSingle("OuterRadius");
            InnerRadius = reader.ReadSingle("InnerRadius");
            MinCosine = reader.ReadSingle("MinCosine");
            CurvedEdgeIndices = reader.ReadUInt16Array("CurvedEdgeIndices");
            CurvedEdgePolyIndices = reader.ReadUInt16Array("CurvedEdgePolyIndices");
            NumCurvedEdges = reader.ReadInt32("NumCurvedEdges");
            FourthVertex = reader.ReadUInt16("FourthVertex");
            IsCircularFace = reader.ReadBool("IsCircularFace");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteStruct("Polygons", Polygons);
            writer.WriteVector4("CurvatureCenter", CurvatureCenter);
            writer.WriteVector4("UnitNormal", UnitNormal);
            writer.WriteSingle("OuterRadius", OuterRadius);
            writer.WriteSingle("InnerRadius", InnerRadius);
            writer.WriteSingle("MinCosine", MinCosine);
            writer.WriteUInt16Array("CurvedEdgeIndices", CurvedEdgeIndices);
            writer.WriteUInt16Array("CurvedEdgePolyIndices", CurvedEdgePolyIndices);
            writer.WriteInt32("NumCurvedEdges", NumCurvedEdges);
            writer.WriteUInt16("FourthVertex", FourthVertex);
            writer.WriteBool("IsCircularFace", IsCircularFace);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundCurvedEdge : IRsc6Block, MetaNode //phCurvedEdge
    {
        public ulong FilePosition { get; set; }
        public ulong BlockLength => 48;
        public bool IsPhysical => false;

        public Vector4 CurvatureCenter { get; set; } //m_CurvatureCenter
        public Vector4 PlaneNormal { get; set; } //m_PlaneNormal, the unit normal vector out of the plane of curvature
        public float Radius { get; set; } //m_Radius, distance between CurvatureCenter and VertexIndices[0] (radius of the curvature)
        public int[] VertexIndices { get; set; } //m_VertexIndices, the two vertex of the curved edge
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //m_UnusedInt

        public void Read(Rsc6DataReader reader)
        {
            CurvatureCenter = reader.ReadVector4();
            PlaneNormal = reader.ReadVector4();
            Radius = reader.ReadSingle();
            VertexIndices = reader.ReadInt32Arr(2);
            Unknown_2Ch = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(CurvatureCenter);
            writer.WriteVector4(PlaneNormal);
            writer.WriteSingle(Radius);
            writer.WriteInt32Array(VertexIndices);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            CurvatureCenter = Rpf6Crypto.ToXYZ(reader.ReadVector4("CurvatureCenter"));
            PlaneNormal = Rpf6Crypto.ToXYZ(reader.ReadVector4("PlaneNormal"));
            Radius = reader.ReadSingle("Radius");
            VertexIndices = reader.ReadInt32Array("VertexIndices");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("CurvatureCenter", CurvatureCenter);
            writer.WriteVector4("PlaneNormal", PlaneNormal);
            writer.WriteSingle("Radius", Radius);
            writer.WriteInt32Array("VertexIndices", VertexIndices);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundPolyhedron : Rsc6Bounds //rage::phBoundPolyhedron
    {
        public override ulong BlockLength => base.BlockLength + 80;
        public uint VerticesPad { get; set; } //m_VerticesPad, always 0
        public Rsc6RawArr<Colour> VertexColours { get; set; }
        public uint VerticesWorldSpace { get; set; } //m_VerticesWorldSpace
        public Rsc6RawArr<byte> PolygonsData { get; set; } //m_Polygons
        public Vector3 Quantum { get; set; } //m_UnQuantizeFactor
        public float QuantumW { get; set; } = Rpf6Crypto.NaN(); //0x0100807F
        public Vector3 CenterGeom { get; set; } //m_BoundingBoxCenter
        public float CenterGeomW { get; set; } = Rpf6Crypto.NaN(); //0x0100807F
        public Rsc6RawArr<Vector3S> VerticesData { get; set; } //m_CompressedVertices
        public uint SmallPolygonsWorldSpace { get; set; } //m_SmallPolygonsWorldSpace
        public bool UseActiveComponents { get; set; } //m_UseActiveComponents, true if this bound supports subsets of itself being active at any time
        public bool IsFlat { get; set; } //m_IsFlat, flag to tell if this bound is flat, only used for debug drawing
        public short NumConvexHullVertices { get; set; } //m_NumConvexHullVertices, the number of vertices on the convex hull of this bound
        public short NumActivePolygons { get; set; } //m_NumActivePolygons
        public short NumActiveVertices { get; set; } //m_NumActiveVertices
        public uint ActivePolygonIndices { get; set; } //m_ActivePolygonIndices
        public uint ActiveVertexIndices { get; set; } //m_ActiveVertexIndices
        public uint VerticesCount { get; set; } //m_NumVertices
        public uint PolygonsCount { get; set; } //m_NumPolygons

        public Vector3[] Vertices { get; set; }
        public Rsc6BoundPolygon[] Polygons { get; set; }
        public Rsc6BoundMaterial[] Materials { get; set; }
        public byte[] PolygonMaterialIndices { get; set; }

        public Rsc6BoundPolyhedron(Rsc6BoundsType type)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBound
            VerticesPad = reader.ReadUInt32();
            VertexColours = reader.ReadRawArrPtr<Colour>();
            VerticesWorldSpace = reader.ReadUInt32();
            PolygonsData = reader.ReadRawArrPtr<byte>();
            Quantum = reader.ReadVector3();
            QuantumW = reader.ReadSingle();
            CenterGeom = reader.ReadVector3();
            CenterGeomW = reader.ReadSingle();
            VerticesData = reader.ReadRawArrPtr<Vector3S>();
            SmallPolygonsWorldSpace = reader.ReadUInt32();
            UseActiveComponents = reader.ReadBoolean();
            IsFlat = reader.ReadBoolean();
            NumConvexHullVertices = reader.ReadInt16();
            NumActivePolygons = reader.ReadInt16();
            NumActiveVertices = reader.ReadInt16();
            ActivePolygonIndices = reader.ReadUInt32();
            ActiveVertexIndices = reader.ReadUInt32();
            VerticesCount = reader.ReadUInt32();
            PolygonsCount = reader.ReadUInt32();

            VertexColours = reader.ReadRawArrItems(VertexColours, VerticesCount);
            VerticesData = reader.ReadRawArrItems(VerticesData, VerticesCount);
            PolygonsData = reader.ReadRawArrItems(PolygonsData, Math.Max(1, PolygonsCount * 16));

            if (VerticesData.Items != null)
            {
                Vertices = new Vector3[VerticesData.Items.Length];
                for (int i = 0; i < Vertices.Length; i++)
                {
                    var bv = VerticesData[i];
                    bv = new Vector3S(bv.Z, bv.X, bv.Y);
                    Vertices[i] = bv.Vector * Quantum;
                }
            }

            this.CreateMesh();
            this.InitPolyhedronPart();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            UpdateStuffBeforeSaving();
            base.Write(writer);
            writer.WriteUInt32(VerticesPad);
            writer.WriteRawArr(VertexColours);
            writer.WriteUInt32(VerticesWorldSpace);
            writer.WriteRawArr(PolygonsData);
            writer.WriteVector3(Quantum);
            writer.WriteSingle(QuantumW);
            writer.WriteVector3(CenterGeom);
            writer.WriteSingle(CenterGeomW);
            writer.WriteRawArr(VerticesData);
            writer.WriteUInt32(SmallPolygonsWorldSpace);
            writer.WriteBoolean(UseActiveComponents);
            writer.WriteBoolean(IsFlat);
            writer.WriteInt16(NumConvexHullVertices);
            writer.WriteInt16(NumActivePolygons);
            writer.WriteInt16(NumActiveVertices);
            writer.WriteUInt32(ActivePolygonIndices);
            writer.WriteUInt32(ActiveVertexIndices);
            writer.WriteUInt32(VerticesCount);
            writer.WriteUInt32(PolygonsCount);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            VertexColours = new(reader.ReadColourArray("VertexColours"));
            VerticesWorldSpace = reader.ReadUInt32("VerticesWorldSpace");
            SetPolygonsString(reader.ReadString("Polygons"));
            Vertices = SetVerticesString(reader.ReadString("Vertices"));
            SmallPolygonsWorldSpace = reader.ReadUInt32("SmallPolygonsWorldSpace");
            UseActiveComponents = reader.ReadBool("UseActiveComponents");
            IsFlat = reader.ReadBool("IsFlat");
            NumConvexHullVertices = reader.ReadInt16("NumConvexHullVertices");
            NumActivePolygons = reader.ReadInt16("NumActivePolygons");
            NumActiveVertices = reader.ReadInt16("NumActiveVertices");
            ActivePolygonIndices = reader.ReadUInt32("ActivePolygonIndices");
            ActiveVertexIndices = reader.ReadUInt32("ActiveVertexIndices");
        }

        public override void Write(MetaNodeWriter writer)
        {
            EnsurePolygons();
            base.Write(writer);
            writer.WriteColourArray("VertexColours", VertexColours.Items);
            writer.WriteUInt32("VerticesWorldSpace", VerticesWorldSpace);
            writer.WriteString("Polygons", GetPolygonsString());
            writer.WriteString("Vertices", GetVerticesString(Vertices));
            writer.WriteUInt32("SmallPolygonsWorldSpace", SmallPolygonsWorldSpace);
            writer.WriteBool("UseActiveComponents", UseActiveComponents);
            writer.WriteBool("IsFlat", IsFlat);
            writer.WriteInt16("NumConvexHullVertices", NumConvexHullVertices);
            writer.WriteInt16("NumActivePolygons", NumActivePolygons);
            writer.WriteInt16("NumActiveVertices", NumActiveVertices);
            writer.WriteUInt32("ActivePolygonIndices", ActivePolygonIndices);
            writer.WriteUInt32("ActiveVertexIndices", ActiveVertexIndices);
        }

        public void UpdateStuffBeforeSaving()
        {
            CalculateQuantum();
            UpdateEdgeIndices();
            UpdateTriangleAreas();
            UpdateVerticesData();
            UpdatePolygonsData();
        }

        protected void InitPolyhedronPart()
        {
            PartSize = BoxMax - BoxMin;
            ComputeMass(ColliderType.Box, PartSize, 1.0f);
            ComputeBodyInertia();
        }

        public void EnsurePolygons()
        {
            if ((Polygons != null) && (Polygons.Length == PolygonsCount)) return;
            CreatePolygons();
        }

        private void CreatePolygons()
        {
            if (PolygonsCount == 0)
            {
                Polygons = null;
                return;
            }

            var polygonData = PolygonsData.Items;
            Polygons = new Rsc6BoundPolygon[PolygonsCount];

            for (int i = 0; i < PolygonsCount; i++)
            {
                var offset = i * 16;
                var b0 = polygonData[offset] & 0xFB; //Clear high type bit
                var type = (Rsc6BoundPolygonType)(b0 & 0x7);
                var area = BufferUtil.ReadSingle(polygonData, offset);

                if (!Rpf6Crypto.IsDefinedInEnumRange<Rsc6BoundPolygonType>((byte)type) && area > 0.0f && area < 50000.0f)
                {
                    type = Rsc6BoundPolygonType.Triangle;
                    PartShape = EditablePartShape.TriMesh;
                }

                var p = new Rsc6BoundPolygon(type, this, i);
                p.Read(polygonData, offset);
                p.MaterialIndex = GetMaterialIndex(i);
                Polygons[i] = p;
            }
        }

        protected void CreateMesh()
        {
            bool usevertexcolours = false;
            var verts = new List<ShapeVertex>();
            var indsl = new List<ushort>();
            var spheres = new List<ModelBatchInstance>();
            var capsules = new List<ModelBatchInstance>();
            var boxes = new List<ModelBatchInstance>();
            var cylinders = new List<ModelBatchInstance>();
            var inst = new ModelBatchInstance();
            var children = new List<EditablePart>();

            Colour getMaterialColour(int matind)
            {
                var matcol = Colour.White;
                if ((Materials != null) && (matind < Materials.Length))
                {
                    matcol = Materials[matind].Type.Colour;
                }
                return matcol;
            }

            void addVertex(int index, Vector3 norm, Colour matcol)
            {
                var c = verts.Count;
                verts.Add(new ShapeVertex()
                {
                    Position = new Vector4(Vertices[index] + CenterGeom, 1),
                    Normal = norm,
                    Colour = (usevertexcolours && (VertexColours.Items != null)) ? VertexColours.Items[index] : matcol,
                    Texcoord = Vector2.Zero,
                    Tangent = Vector3.Zero
                });
                indsl.Add((ushort)c);
            }

            void createChildMeshAndReset()
            {
                var mesh = Shape.Create("BoundPolyhedron", verts.ToArray(), indsl.ToArray());
                verts.Clear();
                indsl.Clear();

                var part = new Collider(ColliderType.Mesh)
                {
                    PartMesh = mesh
                };

                part.UpdateBounds();
                children.Add(part);
            }

            var polygonData = PolygonsData.Items;
            for (int i = 0; i < PolygonsCount; i++)
            {
                var offset = i * 16;
                var b0 = polygonData[offset] & 0xFB; //Clear high type bit
                var type = (Rsc6BoundPolygonType)(b0 & 0x7);
                var area = BufferUtil.ReadSingle(polygonData, offset);

                if ((!Rpf6Crypto.IsDefinedInEnumRange<Rsc6BoundPolygonType>((byte)type) && area > 0.0f && area < 50000.0f) || Type == Rsc6BoundsType.Box)
                {
                    type = Rsc6BoundPolygonType.Triangle;
                    PartShape = EditablePartShape.TriMesh;
                }

                var matind = GetMaterialIndex(i);
                var matcol = getMaterialColour(matind);
                var matcolparam = (uint)(matcol.ToRgba() << 8);

                if (type == Rsc6BoundPolygonType.Triangle)
                {
                    if (verts.Count >= 65000)
                    {
                        createChildMeshAndReset(); //Don't overflow the index buffer!
                    }
                    var ptri = BufferUtil.ReadStruct<Rsc6BoundPolyTriangle>(polygonData, offset);
                    var v1 = Vertices[ptri.TriIndex1];
                    var v2 = Vertices[ptri.TriIndex2];
                    var v3 = Vertices[ptri.TriIndex3];
                    var e1 = Vector3.Normalize(v2 - v1);
                    var e2 = Vector3.Normalize(v3 - v1);
                    var nm = Vector3.Normalize(Vector3.Cross(e1, e2));
                    addVertex(ptri.TriIndex1, nm, matcol);
                    addVertex(ptri.TriIndex2, nm, matcol);
                    addVertex(ptri.TriIndex3, nm, matcol);
                }
                else if (type == Rsc6BoundPolygonType.Box)
                {
                    var pbox = BufferUtil.ReadStruct<Rsc6BoundPolyBox>(polygonData, offset);
                    var v1 = Vertices[pbox.BoxIndex1];
                    var v2 = Vertices[pbox.BoxIndex2];
                    var v3 = Vertices[pbox.BoxIndex3];
                    var v4 = Vertices[pbox.BoxIndex4];
                    var x = (v1 + v2) - (v3 + v4);
                    var y = (v1 + v3) - (v2 + v4);
                    var z = (v1 + v4) - (v2 + v3);
                    var t = (v1 + v2 + v3 + v4) * 0.25f + CenterGeom;
                    var dx = Vector3.Normalize(x);
                    var dy = Vector3.Normalize(y);
                    var dz = Vector3.Normalize(z);
                    var lx = x.Length();
                    var ly = y.Length();
                    var lz = z.Length();
                    inst.Matrix = new Matrix3x4(new Vector4(dx, t.X), new Vector4(dy, t.Y), new Vector4(dz, t.Z));
                    inst.ParamX = lx * 0.5f;
                    inst.ParamY = ly * 0.5f;
                    inst.ParamZ = lz * 0.5f;
                    inst.ParamW = matcolparam + 1; //Signal to use box transform
                    boxes.Add(inst);
                }
                else if (type == Rsc6BoundPolygonType.Sphere)
                {
                    var psph = BufferUtil.ReadStruct<Rsc6BoundPolySphere>(polygonData, offset);
                    var v1 = Vertices[psph.CenterIndex] + CenterGeom;
                    inst.Matrix = Matrix3x4.CreateTranslation(v1);
                    inst.ParamX = psph.SphereRadius;
                    inst.ParamY = psph.SphereRadius;
                    inst.ParamZ = psph.SphereRadius;
                    inst.ParamW = matcolparam + 2; //Signal to use sphere transform
                    spheres.Add(inst);
                }
                else if (type == Rsc6BoundPolygonType.Capsule)
                {
                    var pcap = BufferUtil.ReadStruct<Rsc6BoundPolyCapsule>(polygonData, offset);
                    var v1 = Vertices[pcap.EndIndex0];
                    var v2 = Vertices[pcap.EndIndex1];
                    var t = v1 + CenterGeom;
                    var a = v2 - v1;
                    var h = a.Length();
                    var y = Vector3.Normalize(a);
                    var x = Vector3.Normalize(y.GetPerpVec());
                    var z = Vector3.Normalize(Vector3.Cross(x, y));
                    var r = pcap.CapsuleRadius;
                    inst.Matrix = new Matrix3x4(new Vector4(x, t.X), new Vector4(y, t.Y), new Vector4(z, t.Z));
                    inst.ParamX = r;
                    inst.ParamY = h;
                    inst.ParamZ = 0;
                    inst.ParamW = matcolparam + 3; //Signal to use capsule transform
                    capsules.Add(inst);
                }
                else
                {
                    throw new NotImplementedException("Rsc6Bounds: Unknown primitive type!");
                }
            }

            if (verts.Count > 0 && indsl.Count > 0)
            {
                PartMesh = Shape.Create("BoundPolyhedron", verts.ToArray(), indsl.ToArray());
            }
            else ColliderType = ColliderType.None;

            void addInstChild(ColliderType shape, List<ModelBatchInstance> instances)
            {
                var size = Vector3.One;
                switch (shape)
                {
                    case ColliderType.Capsule:
                    case ColliderType.Cylinder:
                        size = new Vector3(1, 0, 1);
                        break;
                }
                var part = new Collider(shape)
                {
                    PartSize = size,
                    PartInstances = instances.ToArray()
                };
                part.UpdateBounds();
                children.Add(part);
            }

            if (spheres.Count > 0) addInstChild(ColliderType.Sphere, spheres);
            if (capsules.Count > 0) addInstChild(ColliderType.Capsule, capsules);
            if (boxes.Count > 0) addInstChild(ColliderType.Box, boxes);
            if (cylinders.Count > 0) addInstChild(ColliderType.Cylinder, cylinders);

            if (children.Count > 0)
            {
                PartChildren = children.ToArray();
            }
            UpdateBounds();
        }

        public void CalculateQuantum()
        {
            var min = BoxMin;
            var max = BoxMax;
            var c = (min + max) * 0.5f;
            var maxsiz = (max - min) * 0.5f;
            var q = maxsiz / 32767.0f;
            Quantum = q;
            CenterGeom = c;
        }

        public void UpdateEdgeIndices() //Update all triangle edge indices, based on shared vertex indices
        {
            EnsurePolygons();
            if (Polygons == null) return;

            for (int i = 0; i < Polygons.Length; i++)
            {
                var poly = Polygons[i];
                if (poly != null)
                {
                    poly.Index = i;
                }
            }

            var edgedict = new Dictionary<Rsc6BoundEdgeRef, Rsc6BoundEdge>();
            foreach (var poly in Polygons)
            {
                if (poly.Type == Rsc6BoundPolygonType.Triangle)
                {
                    ref var btri = ref poly.Triangle;
                    var e1 = new Rsc6BoundEdgeRef(btri.TriIndex1, btri.TriIndex2);
                    var e2 = new Rsc6BoundEdgeRef(btri.TriIndex2, btri.TriIndex3);
                    var e3 = new Rsc6BoundEdgeRef(btri.TriIndex3, btri.TriIndex1);

                    if (edgedict.TryGetValue(e1, out Rsc6BoundEdge edge1))
                    {
                        if (edge1.Triangle2 != null)
                        {
                            poly.SetEdgeIndex(1, (short)edge1.Triangle1.Index);
                        }
                        else
                        {
                            edge1.Triangle2 = poly;
                            edge1.EdgeID2 = 1;
                        }
                    }
                    else
                    {
                        edgedict[e1] = new Rsc6BoundEdge(poly, 1);
                    }

                    if (edgedict.TryGetValue(e2, out Rsc6BoundEdge edge2))
                    {
                        if (edge2.Triangle2 != null)
                        {
                            poly.SetEdgeIndex(2, (short)edge2.Triangle1.Index);
                        }
                        else
                        {
                            edge2.Triangle2 = poly;
                            edge2.EdgeID2 = 2;
                        }
                    }
                    else
                    {
                        edgedict[e2] = new Rsc6BoundEdge(poly, 2);
                    }

                    if (edgedict.TryGetValue(e3, out Rsc6BoundEdge edge3))
                    {
                        if (edge3.Triangle2 != null)
                        {
                            poly.SetEdgeIndex(3, (short)edge3.Triangle1.Index);
                        }
                        else
                        {
                            edge3.Triangle2 = poly;
                            edge3.EdgeID2 = 3;
                        }
                    }
                    else
                    {
                        edgedict[e3] = new Rsc6BoundEdge(poly, 3);
                    }

                }
            }

            foreach (var kvp in edgedict)
            {
                var edge = kvp.Value;
                if (edge.Triangle1 == null) continue;

                if (edge.Triangle2 == null)
                {
                    edge.Triangle1.SetEdgeIndex(edge.EdgeID1, -1);
                }
                else
                {
                    edge.Triangle1.SetEdgeIndex(edge.EdgeID1, (short)edge.Triangle2.Index);
                    edge.Triangle2.SetEdgeIndex(edge.EdgeID2, (short)edge.Triangle1.Index);
                }
            }


            foreach (var poly in Polygons)
            {
                if (poly.Type == Rsc6BoundPolygonType.Triangle)
                {
                    ref var btri = ref poly.Triangle;
                }
            }
        }

        public void UpdateTriangleAreas() //Update all triangle areas, based on vertex positions
        {
            EnsurePolygons();
            if (Polygons == null) return;

            foreach (var poly in Polygons)
            {
                if (poly.Type == Rsc6BoundPolygonType.Triangle)
                {
                    ref var btri = ref poly.Triangle;
                    var v1 = poly.GetVertex(btri.TriIndex1);
                    var v2 = poly.GetVertex(btri.TriIndex2);
                    var v3 = poly.GetVertex(btri.TriIndex3);
                    var area = TriangleMath.Area(ref v1, ref v2, ref v3);

                    if (float.IsNormal(area) == false)
                    {
                        area = float.Epsilon;
                    }
                    btri.Area = area;
                }
            }
        }

        public void UpdateVerticesData()
        {
            if ((Vertices != null) && (Vertices.Length > 0))
            {
                var verts = new Vector3S[Vertices.Length];
                for (int i = 0; i < Vertices.Length; i++)
                {
                    var vq = Vertices[i] / Quantum;
                    verts[i] = new Vector3S(vq);
                }
                VerticesData = new Rsc6RawArr<Vector3S>() { Items = verts };
            }
            else
            {
                VerticesData = new Rsc6RawArr<Vector3S>();
            }
            VerticesCount = (uint)(Vertices != null ? Vertices.Length : 0);
        }

        public void UpdatePolygonsData()
        {
            if ((Polygons != null) && (Polygons.Length > 0))
            {
                var polymats = new byte[Polygons.Length];
                var polydata = new byte[Polygons.Length * 16];

                for (int i = 0; i < Polygons.Length; i++)
                {
                    var p = Polygons[i];
                    var o = i * 16;
                    p.Write(polydata, o);

                    if (p.Type == Rsc6BoundPolygonType.Triangle)
                    {
                        var b = polydata[o];
                        polydata[o] = (byte)(b & 0xF8); //Add the poly types back in!
                    }
                    polymats[i] = (byte)p.MaterialIndex;
                }
                PolygonsData = new Rsc6RawArr<byte>() { Items = polydata };
                PolygonMaterialIndices = polymats;
            }
            else
            {
                var arr = new byte[1] { 0xCD };
                PolygonsData = new Rsc6RawArr<byte>(arr);
                PolygonMaterialIndices = arr;
                return;
            }
            PolygonsCount = (uint)(Polygons != null ? Polygons.Length : 0);
        }

        public string GetPolygonsString()
        {
            if (Polygons == null) return "";
            var sb = new StringBuilder();
            foreach (var poly in Polygons)
            {
                sb.AppendLine(poly.MetaString);
            }
            return sb.ToString();
        }

        public void SetPolygonsString(string polysstr)
        {
            if (string.IsNullOrEmpty(polysstr)) return;
            var polystrs = polysstr.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var polys = new Rsc6BoundPolygon[polystrs.Length];

            for (int i = 0; i < polystrs.Length; i++)
            {
                polys[i] = new Rsc6BoundPolygon(polystrs[i], this, i);
            }

            Polygons = polys;
            UpdatePolygonsData();
        }

        public string GetVerticesString(Vector3[] verts)
        {
            var sb = new StringBuilder();
            foreach (var vert in verts)
            {
                sb.AppendLine(FloatUtil.GetVector3String(vert));
            }
            return sb.ToString();
        }

        public Vector3[] SetVerticesString(string vertsstr)
        {
            if (string.IsNullOrEmpty(vertsstr)) return null;
            var list = new List<Vector3>();
            var strarr = vertsstr.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var str in strarr)
            {
                var v = FloatUtil.ParseVector3String(str);
                list.Add(Rpf6Crypto.ToXYZ(v));
            }

            if (list.Count == 0) return null;
            return list.ToArray();
        }

        public Vector3 GetVertexPos(int index)
        {
            var p = ((index >= 0) && (index < Vertices.Length)) ? Vertices[index] : Vector3.Zero;
            return Vector3.Transform(p, Transform);
        }

        public void SetVertexPos(int index, Vector3 v)
        {
            if ((index >= 0) && (index < Vertices.Length))
            {
                var t = Vector3.Transform(v, TransformInv);
                Vertices[index] = t;
            }
        }

        public int GetMaterialIndex(int polyIndex)
        {
            var matind = 0;
            var inds = PolygonMaterialIndices;
            if ((inds != null) && (polyIndex < inds.Length))
            {
                matind = inds[polyIndex];
            }
            return matind;
        }

        public Rsc6BoundMaterial GetMaterialByIndex(int matIndex)
        {
            var mats = Materials;
            if ((mats != null) && (matIndex < mats.Length))
            {
                return mats[matIndex];
            }
            return new Rsc6BoundMaterial();
        }

        public Rsc6BoundMaterial GetMaterial(int polyIndex)
        {
            var matind = GetMaterialIndex(polyIndex);
            return GetMaterialByIndex(matind);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundGeometryBVH : Rsc6BoundGeometry //phBoundBVH
    {
        public override ulong BlockLength => base.BlockLength + 16;
        public Rsc6Ptr<Rsc6BoundGeometryBVHRoot> BVH { get; set; } //m_BVH
        public uint Unknown_F4h { get; set; } //Always 0
        public uint Unknown_F8h { get; set; } //Always 0
        public uint Unknown_FCh { get; set; } //Always 0

        public Rsc6BoundGeometryBVH() : base(Rsc6BoundsType.GeometryBVH)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BVH = reader.ReadPtr<Rsc6BoundGeometryBVHRoot>();
            Unknown_F4h = reader.ReadUInt32();
            Unknown_F8h = reader.ReadUInt32();
            Unknown_FCh = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(BVH);
            writer.WriteUInt32(Unknown_F4h);
            writer.WriteUInt32(Unknown_F8h);
            writer.WriteUInt32(Unknown_FCh);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            BVH = new(reader.ReadNode<Rsc6BoundGeometryBVHRoot>("BVH"));
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNode("BVH", BVH.Item);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundSurface : Rsc6Bounds, MetaNode //rage::phBoundSurface
    {
        public override ulong BlockLength => base.BlockLength + 14384;
        public Vector2[] VelocityGrid { get; set; } //m_VelocityGrid
        public float[] OffsetGrid { get; set; } //m_OffsetGrid
        public float PeakWaveHeight { get; set; } //m_PeakWaveHeight
        public float Spacing { get; set; } //m_Spacing
        public float MinElevation { get; set; } //m_MinElevation
        public float MaxElevation { get; set; } //m_MaxElevation
        public short CellX { get; set; } //m_CellX
        public short CellY { get; set; } //m_CellY
        public float MinX { get; set; } = 0xCDCDCDCD; //m_MinX
        public float MaxX { get; set; } = 0xCDCDCDCD; //m_MaxX
        public float MinZ { get; set; } = 0xCDCDCDCD; //m_MinZ
        public float MaxZ { get; set; } = 0xCDCDCDCD; //m_MaxZ
        public uint Unknown_3024h { get; set; } //Always 0, probably padding
        public uint Unknown_3028h { get; set; } //Always 0, probably padding
        public uint Unknown_302Ch { get; set; } //Always 0, probably padding
        public uint[] SurfaceGrid { get; set; } //m_SurfaceGrid

        public const int GridLength = 32;
        public const int GridPointCount = 1024; //GridLength²

        public Rsc6BoundSurface() : base(Rsc6BoundsType.Surface)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            VelocityGrid = reader.ReadVector2Arr(GridPointCount);
            OffsetGrid = reader.ReadSingleArr(GridPointCount);
            PeakWaveHeight = reader.ReadSingle();
            Spacing = reader.ReadSingle();
            MinElevation = reader.ReadSingle();
            MaxElevation = reader.ReadSingle();
            CellX = reader.ReadInt16();
            CellY = reader.ReadInt16();
            MinX = reader.ReadSingle();
            MaxX = reader.ReadSingle();
            MinZ = reader.ReadSingle();
            MaxZ = reader.ReadSingle();
            Unknown_3024h = reader.ReadUInt32();
            Unknown_3028h = reader.ReadUInt32();
            Unknown_302Ch = reader.ReadUInt32();
            SurfaceGrid = reader.ReadUInt32Arr(GridPointCount / 2);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteVector2Array(VelocityGrid);
            writer.WriteSingleArray(OffsetGrid);
            writer.WriteSingle(PeakWaveHeight);
            writer.WriteSingle(Spacing);
            writer.WriteSingle(MinElevation);
            writer.WriteSingle(MaxElevation);
            writer.WriteInt16(CellX);
            writer.WriteInt16(CellY);
            writer.WriteSingle(MinX);
            writer.WriteSingle(MaxX);
            writer.WriteSingle(MinZ);
            writer.WriteSingle(MaxZ);
            writer.WriteUInt32(Unknown_3024h);
            writer.WriteUInt32(Unknown_3028h);
            writer.WriteUInt32(Unknown_302Ch);
            writer.WriteUInt32Array(SurfaceGrid);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            VelocityGrid = reader.ReadVector2Array("VelocityGrid");
            OffsetGrid = reader.ReadSingleArray("OffsetGrid");
            PeakWaveHeight = reader.ReadSingle("PeakWaveHeight");
            Spacing = reader.ReadSingle("Spacing");
            MinElevation = reader.ReadSingle("MinElevation");
            MaxElevation = reader.ReadSingle("MaxElevation");
            CellX = reader.ReadInt16("CellX");
            CellY = reader.ReadInt16("CellY");
            MinX = reader.ReadSingle("MinX");
            MaxX = reader.ReadSingle("MaxX");
            MinZ = reader.ReadSingle("MinZ");
            MaxZ = reader.ReadSingle("MaxZ");
            SurfaceGrid = reader.ReadUInt32Array("SurfaceGrid");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector2Array("VelocityGrid", VelocityGrid);
            writer.WriteSingleArray("OffsetGrid", OffsetGrid);
            writer.WriteSingle("PeakWaveHeight", PeakWaveHeight);
            writer.WriteSingle("Spacing", Spacing);
            writer.WriteSingle("MinElevation", MinElevation);
            writer.WriteSingle("MaxElevation", MaxElevation);
            writer.WriteInt16("CellX", CellX);
            writer.WriteInt16("CellY", CellY);
            writer.WriteSingle("MinX", MinX);
            writer.WriteSingle("MaxX", MaxX);
            writer.WriteSingle("MinZ", MinZ);
            writer.WriteSingle("MaxZ", MaxZ);
            writer.WriteUInt32Array("SurfaceGrid", SurfaceGrid);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundComposite : Rsc6Bounds //rage::phBoundComposite
    {
        //Represents a physics bound that is an aggregate of multiple other physics bounds

        public override ulong BlockLength => base.BlockLength + 32;
        public Rsc6RawPtrArr<Rsc6Bounds> Childrens { get; set; } //m_Bounds, sub-bounds that compose this composite bound
        public Rsc6RawArr<Matrix4x4> CurrentMatrices { get; set; } //m_CurrentMatrices, current matrices for each sub-bound
        public Rsc6RawArr<Matrix4x4> LastMatrices { get; set; } //m_LastMatrices, previous (last update) matrices for each sub-bound, same as m_CurrentMatrices
        public Rsc6RawArr<BoundingBox4> LocalBoxMinMaxs { get; set; } //m_LocalBoxMinMaxs, the bounding boxes, in part local space, of all of the sub-bounds
        public Rsc6RawArr<Rsc6BoundCompositeChildrenFlags> TypeFlags { get; set; } //m_OwnedTypeAndIncludeFlags, optional per-component type flags
        public Rsc6RawArr<Rsc6BoundCompositeChildrenFlags> IncludeFlags { get; set; } //Points to m_OwnedTypeAndIncludeFlags
        public ushort MaxNumBounds { get; set; } //m_MaxNumBounds
        public ushort NumBounds { get; set; } //m_NumBounds
        public bool ContainsBVH { get; set; } //m_ContainsBVH
        public byte Pad { get; set; } //pad, always 0
        public ushort NumActiveBounds { get; set; } //m_NumActiveBounds

        public Rsc6BoundComposite() : base(Rsc6BoundsType.Composite)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBounds
            Childrens = reader.ReadRawPtrArrPtr<Rsc6Bounds>();
            CurrentMatrices = reader.ReadRawArrPtr<Matrix4x4>();
            LastMatrices = reader.ReadRawArrPtr<Matrix4x4>();
            LocalBoxMinMaxs = reader.ReadRawArrPtr<BoundingBox4>();
            TypeFlags = reader.ReadRawArrPtr<Rsc6BoundCompositeChildrenFlags>();
            IncludeFlags = reader.ReadRawArrPtr<Rsc6BoundCompositeChildrenFlags>();
            MaxNumBounds = reader.ReadUInt16();
            NumBounds = reader.ReadUInt16();
            ContainsBVH = reader.ReadBoolean();
            Pad = reader.ReadByte();
            NumActiveBounds = reader.ReadUInt16();

            Childrens = reader.ReadRawPtrArrItem(Childrens, MaxNumBounds, Create);
            CurrentMatrices = reader.ReadRawArrItems(CurrentMatrices, MaxNumBounds);
            LastMatrices = reader.ReadRawArrItems(LastMatrices, MaxNumBounds);
            LocalBoxMinMaxs = reader.ReadRawArrItems(LocalBoxMinMaxs, NumBounds);
            TypeFlags = reader.ReadRawArrItems(TypeFlags, MaxNumBounds);
            IncludeFlags = reader.ReadRawArrItems(IncludeFlags, MaxNumBounds);

            if (Childrens.Items != null)
            {
                for (int i = 0; i < Childrens.Items.Length; i++)
                {
                    var c = Childrens.Items[i];
                    var flag = TypeFlags.Items?[i];

                    if (c == null) continue;
                    c.Name = "Child" + i.ToString();

                    if ((CurrentMatrices.Items != null) && (i < CurrentMatrices.Items.Length))
                    {
                        var height = Rsc6Fragment.SkinnedHeightPos; //Approximative fix for displaying bounds
                        if (flag?.TypeFlags == Rsc6ObjectTypeFlags.STATIC_STANDARD || flag?.TypeFlags == Rsc6ObjectTypeFlags.STATIC_STANDARD2)
                        {
                            height = 0.0f;
                        }

                        var m = new Matrix3x4(CurrentMatrices[i]);
                        m.Translation = new Vector3(m.Translation.Y, m.Translation.Z, m.Translation.X - height);
                        m.Orientation = new Quaternion(m.Orientation.Z, m.Orientation.X, m.Orientation.Y, m.Orientation.W);

                        if (c is Rsc6BoundCapsule)
                        {
                            var rotationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2);
                            m.Orientation *= rotationQuaternion;
                        }
                        c.PartTransform = m;
                    }

                }
            }
            PartChildren = Childrens.Items;
            UpdateBounds();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteRawPtrArr(Childrens);
            writer.WriteRawArr(CurrentMatrices);
            writer.WriteRawArr(LastMatrices);
            writer.WriteRawArr(LocalBoxMinMaxs);
            writer.WriteRawArr(TypeFlags);
            writer.WriteRawArr(IncludeFlags);
            writer.WriteUInt16(MaxNumBounds);
            writer.WriteUInt16(NumBounds);
            writer.WriteBoolean(ContainsBVH);
            writer.WriteByte(Pad);
            writer.WriteUInt16(NumActiveBounds);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Childrens = new(reader.ReadNodeArray("Childrens", Create));
            CurrentMatrices = new(Rpf6Crypto.ToZXY(reader.ReadMatrix4x4Array("Matrices")));
            TypeFlags = new(reader.ReadNodeArray<Rsc6BoundCompositeChildrenFlags>("OwnedTypeAndIncludeFlags"));
            IncludeFlags = TypeFlags;
            ContainsBVH = reader.ReadBool("ContainsBVH");
            NumActiveBounds = reader.ReadUInt16("NumActiveBounds");

            if (Childrens.Items != null)
            {
                MaxNumBounds = (ushort)Childrens.Items.Length;
                NumBounds = (ushort)Childrens.Items.Length;

                var localBbs = new BoundingBox4[NumBounds];
                for (int i = 0; i < NumBounds; i++)
                {
                    var child = Childrens.Items[i];
                    var bb = new BoundingBox(child.BoxMin, child.BoxMax);
                    localBbs[i] = new BoundingBox4(bb);
                }
                LocalBoxMinMaxs = new(localBbs);
            }
            LastMatrices = new(CurrentMatrices.Items);
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            if (Childrens.Items != null && Childrens.Items[0] != null) writer.WriteNodeArray("Childrens", Childrens.Items);
            writer.WriteMatrix4x4Array("Matrices", CurrentMatrices.Items);
            writer.WriteNodeArray("OwnedTypeAndIncludeFlags", TypeFlags.Items);
            writer.WriteBool("ContainsBVH", ContainsBVH);
            writer.WriteUInt16("NumActiveBounds", NumActiveBounds);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundGeometryBVHRoot : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 104;
        public Rsc6Arr<Rsc6BoundGeometryBVHNode> Nodes { get; set; }
        public uint Depth { get; set; } = 0xCDCDCDCD; //Depth of the hierarchy? but value is 0xCDCDCDCD
        public Vector4 BoundingBoxMin { get; set; }
        public Vector4 BoundingBoxMax { get; set; }
        public Vector4 BoundingBoxCenter { get; set; }
        public Vector4 BVHQuantumInverse { get; set; } //1/BVHQuantum
        public Vector4 BVHQuantum { get; set; }
        public Rsc6Arr<Rsc6BoundGeometryBVHTree> Trees { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            Nodes = reader.ReadArr<Rsc6BoundGeometryBVHNode>(true);
            Depth = reader.ReadUInt32();
            BoundingBoxMin = reader.ReadVector4();
            BoundingBoxMax = reader.ReadVector4();
            BoundingBoxCenter = reader.ReadVector4();
            BVHQuantumInverse = reader.ReadVector4();
            BVHQuantum = reader.ReadVector4();
            Trees = reader.ReadArr<Rsc6BoundGeometryBVHTree>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(Nodes, true);
            writer.WriteUInt32(Depth);
            writer.WriteVector4(BoundingBoxMin);
            writer.WriteVector4(BoundingBoxMax);
            writer.WriteVector4(BoundingBoxCenter);
            writer.WriteVector4(BVHQuantumInverse);
            writer.WriteVector4(BVHQuantum);
            writer.WriteArr(Trees);
        }

        public void Read(MetaNodeReader reader)
        {
            Nodes = new(reader.ReadStructArray<Rsc6BoundGeometryBVHNode>("Nodes"));
            Depth = reader.ReadUInt32("Depth");
            BoundingBoxMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxMin"));
            BoundingBoxMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxMax"));
            BoundingBoxCenter = Rpf6Crypto.ToXYZ(reader.ReadVector4("BoundingBoxCenter"));
            BVHQuantum = Rpf6Crypto.ToXYZ(reader.ReadVector4("BVHQuantum"));
            Trees = new(reader.ReadStructArray<Rsc6BoundGeometryBVHTree>("Trees"));
            BVHQuantumInverse = new Vector4(1 / BVHQuantum.X, 1 / BVHQuantum.Y, 1 / BVHQuantum.Z, 0.0f);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteStructArray("Nodes", Nodes.Items);
            if (Depth != 0xCDCDCDCD) writer.WriteUInt32("Depth", Depth);
            writer.WriteVector4("BoundingBoxMin", BoundingBoxMin);
            writer.WriteVector4("BoundingBoxMax", BoundingBoxMax);
            writer.WriteVector4("BoundingBoxCenter", BoundingBoxCenter);
            writer.WriteVector4("BVHQuantum", BVHQuantum);
            writer.WriteStructArray("Trees", Trees.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundsMaterialData
    {
        public string Elasticity { get; set; } = "0.400f";
        public string Friction { get; set; } = "0.699f";
        public string FXMaterial { get; set; } = string.Empty;
        public string Fatal { get; set; } = "0";
        public string WallJump { get; set; } = "1";
        public string Conveyor { get; set; } = "0";
        public string ConveyorSpeed { get; set; } = "0";
        public string Injure { get; set; } = "0";
        public string InjureRate { get; set; } = "0";
        public string Water { get; set; } = "0";
        public string AffectPlayer { get; set; } = "1";
        public string AffectNPC { get; set; } = "1";
        public string StopsBullets { get; set; } = "1";
        public string IsStickable { get; set; } = "0";
        public string Ramp { get; set; } = "0";
        public string Drag { get; set; } = "0";
        public string Width { get; set; } = "1";
        public string Height { get; set; } = "0";
        public string Depth { get; set; } = "0";
        public string PtxThreshold0 { get; set; } = "0.25";
        public string PtxThreshold1 { get; set; } = "0.5";

        public Colour Colour { get; set; } = Colour.Red;
        public string MaterialName { get; set; } = "Unknown";

        public override string ToString()
        {
            return MaterialName;
        }
    }

    [TC(typeof(EXP))] public static class Rsc6BoundsMaterialTypes
    {
        public static List<Rsc6BoundsMaterialData> Materials;

        public static void Init(Rpf6FileManager fman)
        {
            if (fman.AllArchives.Count == 0)
            {
                return;
            }

            Core.Engine.Console.Write("Rsc6BoundsMaterialTypes", "Initialising bounds materials...");
            var list = new List<Rsc6BoundsMaterialData>();
            var rpf = fman.AllArchives.FirstOrDefault(e => e.Name == "tune_switch.rpf");
            var rootMatList = rpf.AllEntries.FirstOrDefault(e => e.Name == "materials.list" && e.Parent.Parent.Name == "tune"); //There's two materials.list with same parents...

            if (rootMatList != null)
            {
                string matTxt = fman.GetFileUTF8Text(rootMatList.Path);
                var matList = ParseMaterialList(matTxt, fman, rpf);
                var matFiles = GetMaterialFilesFromList(matList, rpf);

                foreach (var file in matFiles)
                {
                    string txt = fman.GetFileUTF8Text(file.Path);
                    AddMaterialsDat(txt, file.Name, list);
                }
            }
            Materials = list;
        }

        private static List<string> ParseMaterialList(string txt, Rpf6FileManager fman, GameArchive rpf)
        {
            var list = new List<string>();
            var lines = txt.Split('\n');
            lines = lines.Select(s => s.ToLower()).ToArray();

            foreach (var line in lines)
            {
                var str = line.Replace("\r", "");
                if (!str.StartsWith("#"))
                    list.Add(str);
                else
                {
                    var matToLoad = str[(str.IndexOf(" ") + 1)..];
                    var matList = rpf.AllEntries.FirstOrDefault(e => e.Name == matToLoad);
                    string matTxt = fman.GetFileUTF8Text(matList.Path);
                    list.AddRange(ParseMaterialList(matTxt, fman, rpf)); //Recurse and add seperated materials
                }
            }
            return list.Distinct().ToList(); //Remove duplicated materials
        }

        private static List<Rpf6FileEntry> GetMaterialFilesFromList(List<string> list, GameArchive rpf)
        {
            var files = new List<Rpf6FileEntry>();
            foreach (var item in list)
            {
                var file = rpf.AllEntries.FirstOrDefault(e => e.Name == item + ".mtl");
                if (file != null)
                {
                    files.Add((Rpf6FileEntry)file);
                }
            }
            return files;
        }

        private static void AddMaterialsDat(string txt, string filename, List<Rsc6BoundsMaterialData> list)
        {
            if (txt == null) return;
            string[] lines = txt.Split('\n');

            var m = new Rsc6BoundsMaterialData()
            {
                MaterialName = filename
            };

            //Materials all use different parameters
            foreach (var line in lines)
            {
                var l = Regex.Replace(line, @"[\r\t""]", "").Trim();
                if (l == string.Empty || !l.Contains(':') || l == "{" || l == "}") continue;

                var delimiterIndex = l.IndexOf(':');
                var name = l[..delimiterIndex];
                var value = l.Substring(delimiterIndex + 1, l.Length - delimiterIndex - 1);

                switch (name)
                {
                    case "elasticity": m.Elasticity = value; break;
                    case "friction": m.Friction = value; break;
                    case "fxMaterial": m.FXMaterial = value; break;
                    case "Fatal": m.Fatal = value; break;
                    case "WallJump": m.WallJump = value; break;
                    case "Conveyor": m.Conveyor = value; break;
                    case "ConveyorSpeed": m.ConveyorSpeed = value; break;
                    case "Injure": m.Injure = value; break;
                    case "InjureRate": m.InjureRate = value; break;
                    case "Water": m.Water = value; break;
                    case "AffectPlayer": m.AffectPlayer = value; break;
                    case "AffectNPC": m.AffectNPC = value; break;
                    case "StopsBullets": m.StopsBullets = value; break;
                    case "IsStickable": m.IsStickable = value; break;
                    case "Ramp": m.Ramp = value; break;
                    case "Drag": m.Drag = value; break;
                    case "Width": m.Width = value; break;
                    case "Height": m.Height = value; break;
                    case "Depth": m.Depth = value; break;
                    case "PtxThreshold0": m.PtxThreshold0 = value; break;
                    case "PtxThreshold1": m.PtxThreshold1 = value; break;
                    case "displayColor": m.Colour = Rpf6Crypto.ParseRGBString(value); break;
                }
            }
            list.Add(m);
        }

        public static Rsc6BoundsMaterialData GetMaterial(Rsc6BoundsMaterialType type)
        {
            if (Materials == null) return null;
            if (type.Index >= Materials.Count) return null;
            return Materials[type.Index];
        }

        public static Rsc6BoundsMaterialData GetMaterial(byte index)
        {
            if (Materials == null) return null;
            if (index >= Materials.Count) return null;
            return Materials[index];
        }

        public static string GetMaterialName(Rsc6BoundsMaterialType type)
        {
            var m = GetMaterial(type);
            if (m == null) return string.Empty;
            return m.MaterialName;
        }

        public static Colour GetMaterialColour(Rsc6BoundsMaterialType type)
        {
            var m = GetMaterial(type);
            if (m == null) return new Colour(0xFFCCCCCC);
            return m.Colour;
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundPolygon //phPrimitive, convenience class for doing operations on polygons
    {
        //Simple wrapper around a phPolygon

        public int Index { get; set; }
        public int MaterialIndex { get; set; }
        public Rsc6BoundPolygonType Type { get; private set; }
        public Rsc6BoundPolyhedron Owner { get; private set; }

        public Rsc6BoundPolyBox Box;
        public Rsc6BoundPolySphere Sphere;
        public Rsc6BoundPolyCapsule Capsule;
        public Rsc6BoundPolyTriangle Triangle;

        public Rsc6BoundMaterial Material
        {
            get
            {
                return Owner?.GetMaterialByIndex(Index) ?? new Rsc6BoundMaterial();
            }
        }

        public string MetaString
        {
            get
            {
                return Type switch
                {
                    Rsc6BoundPolygonType.Box => $"Box {MaterialIndex} {Box.BoxIndex1} {Box.BoxIndex2} {Box.BoxIndex3} {Box.BoxIndex4}",
                    Rsc6BoundPolygonType.Sphere => $"Sph {MaterialIndex} {Sphere.CenterIndex} {FloatUtil.ToString(Sphere.SphereRadius)}",
                    Rsc6BoundPolygonType.Capsule => $"Cap {MaterialIndex} {Capsule.EndIndex0} {Capsule.EndIndex1} {FloatUtil.ToString(Capsule.CapsuleRadius)}",
                    Rsc6BoundPolygonType.Triangle => $"Tri {MaterialIndex} {Triangle.TriIndex1} {Triangle.TriIndex2} {Triangle.TriIndex3}",
                    _ => null,
                };
            }
            set
            {
                var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length < 2) return;

                var type = parts[0];
                if (!int.TryParse(parts[1], out var matInd)) return;
                MaterialIndex = matInd;

                switch (type)
                {
                    case "Box":
                        Type = Rsc6BoundPolygonType.Box;
                        Box.BoxType = 3452816847;
                        Box.Unused0 = 4294967295;
                        if (parts.Length >= 6)
                        {
                            ushort.TryParse(parts[2], out Box.BoxIndex1);
                            ushort.TryParse(parts[3], out Box.BoxIndex2);
                            ushort.TryParse(parts[4], out Box.BoxIndex3);
                            ushort.TryParse(parts[5], out Box.BoxIndex4);
                        }
                        break;
                    case "Sph":
                        Type = Rsc6BoundPolygonType.Sphere;
                        if (parts.Length >= 4)
                        {
                            ushort.TryParse(parts[2], out Sphere.CenterIndex);
                            FloatUtil.TryParse(parts[3], out Sphere.SphereRadius);
                        }
                        break;
                    case "Cap":
                        Type = Rsc6BoundPolygonType.Capsule;
                        Capsule.CapsuleType = 206;
                        Capsule.Pad0 = 205;
                        Capsule.Unused0 = 65535;
                        Capsule.Unused1 = 4294967295;
                        if (parts.Length >= 5)
                        {
                            ushort.TryParse(parts[2], out Capsule.EndIndex0);
                            ushort.TryParse(parts[3], out Capsule.EndIndex1);
                            FloatUtil.TryParse(parts[4], out Capsule.CapsuleRadius);
                        }
                        break;
                    case "Tri":
                        Type = Rsc6BoundPolygonType.Triangle;
                        if (parts.Length >= 5)
                        {
                            ushort.TryParse(parts[2], out Triangle.TriIndex1);
                            ushort.TryParse(parts[3], out Triangle.TriIndex2);
                            ushort.TryParse(parts[4], out Triangle.TriIndex3);
                        }
                        break;
                }
            }
        }

        public Rsc6BoundPolygon(Rsc6BoundPolygonType type, Rsc6BoundPolyhedron owner, int index)
        {
            Type = type;
            Owner = owner;
            Index = index;
        }

        public Rsc6BoundPolygon(string str, Rsc6BoundPolyhedron owner, int index)
        {
            Owner = owner;
            Index = index;
            MetaString = str;
        }

        public void Read(byte[] data, int offset)
        {
            switch (Type)
            {
                case Rsc6BoundPolygonType.Box: Box = BufferUtil.ReadStruct<Rsc6BoundPolyBox>(data, offset); break;
                case Rsc6BoundPolygonType.Triangle: Triangle = BufferUtil.ReadStruct<Rsc6BoundPolyTriangle>(data, offset); break;
                case Rsc6BoundPolygonType.Sphere: Sphere = BufferUtil.ReadStruct<Rsc6BoundPolySphere>(data, offset); break;
                case Rsc6BoundPolygonType.Capsule: Capsule = BufferUtil.ReadStruct<Rsc6BoundPolyCapsule>(data, offset); break;
                default: break;
            }
        }

        public void Write(byte[] data, int offset)
        {
            switch (Type)
            {
                case Rsc6BoundPolygonType.Box: BufferUtil.WriteStruct(data, offset, ref Box); break;
                case Rsc6BoundPolygonType.Triangle: BufferUtil.WriteStruct(data, offset, ref Triangle); break;
                case Rsc6BoundPolygonType.Sphere: BufferUtil.WriteStruct(data, offset, ref Sphere); break;
                case Rsc6BoundPolygonType.Capsule: BufferUtil.WriteStruct(data, offset, ref Capsule); break;
                default: break;
            }
        }

        public Vector3 GetVertex(int i)
        {
            return (Owner != null) ? Owner.GetVertexPos(i) : Vector3.Zero;
        }

        public void SetEdgeIndex(int edgeid, short polyindex)
        {
            if (Type != Rsc6BoundPolygonType.Triangle) return;
            switch (edgeid)
            {
                case 1:
                    Triangle.EdgeIndex1 = polyindex;
                    break;
                case 2:
                    Triangle.EdgeIndex2 = polyindex;
                    break;
                case 3:
                    Triangle.EdgeIndex3 = polyindex;
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    [TC(typeof(EXP))] public struct Rsc6BoundEdgeRef //Convenience struct for updating edge indices
    {
        public int Vertex1 { get; set; }
        public int Vertex2 { get; set; }

        public Rsc6BoundEdgeRef(int i1, int i2)
        {
            Vertex1 = Math.Min(i1, i2);
            Vertex2 = Math.Max(i1, i2);
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundEdge //Convenience class for updating edge indices
    {
        public Rsc6BoundPolygon Triangle1 { get; set; }
        public Rsc6BoundPolygon Triangle2 { get; set; }
        public int EdgeID1 { get; set; }
        public int EdgeID2 { get; set; }

        public Rsc6BoundEdge(Rsc6BoundPolygon t1, int e1)
        {
            Triangle1 = t1;
            EdgeID1 = e1;
        }
    }

    public class Rsc6BVHBuilder
    {
        public static int MaxNodeItemCount = 4; //Item threshold: 1 for composites, 4 for geometries
        public static int MaxTreeNodeCount = 127; //Max number of nodes found in any tree

        public static Rsc6BVHBuilderNode[] Unbuild(Rsc6BoundGeometryBVHRoot bvh)
        {
            if ((bvh?.Trees.Items == null) || (bvh?.Nodes.Items == null)) return null;
            var nodes = new List<Rsc6BVHBuilderNode>();

            foreach (var tree in bvh.Trees.Items)
            {
                var bnode = new Rsc6BVHBuilderNode();
                bnode.Unbuild(bvh, tree.NodeIndex1, tree.NodeIndex2);
                nodes.Add(bnode);
            }
            return nodes.ToArray();
        }
    }

    public class Rsc6BVHBuilderNode
    {
        public List<Rsc6BVHBuilderNode> Children;
        public List<Rsc6BVHBuilderItem> Items;
        public Vector3 Min;
        public Vector3 Max;
        public int Index;

        public int TotalNodes
        {
            get
            {
                int c = 1;
                if (Children != null)
                {
                    foreach (var child in Children)
                    {
                        c += child.TotalNodes;
                    }
                }
                return c;
            }
        }

        public int TotalItems
        {
            get
            {
                int c = Items?.Count ?? 0;
                if (Children != null)
                {
                    foreach (var child in Children)
                    {
                        c += child.TotalItems;
                    }
                }
                return c;
            }
        }

        public void UpdateMinMax()
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            if (Items != null)
            {
                foreach (var item in Items)
                {
                    min = Vector3.Min(min, item.Min);
                    max = Vector3.Max(max, item.Max);
                }
            }
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.UpdateMinMax();
                    min = Vector3.Min(min, child.Min);
                    max = Vector3.Max(max, child.Max);
                }
            }
            Min = min;
            Max = max;
        }

        public void GatherNodes(List<Rsc6BVHBuilderNode> nodes)
        {
            Index = nodes.Count;
            nodes.Add(this);

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.GatherNodes(nodes);
                }
            }
        }

        public void GatherTrees(List<Rsc6BVHBuilderNode> trees)
        {
            if ((TotalNodes > Rsc6BVHBuilder.MaxTreeNodeCount) && ((Children?.Count ?? 0) > 0))
            {
                foreach (var child in Children)
                {
                    child.GatherTrees(trees);
                }
            }
            else
            {
                trees.Add(this);
            }
        }

        public void Unbuild(Rsc6BoundGeometryBVHRoot bvh, int nodeIndex1, int nodeIndex2)
        {
            var q = bvh.BVHQuantum.XYZ();
            var c = bvh.BoundingBoxCenter.XYZ();
            var nodeitems = bvh.Nodes.Items;
            int nodeind = nodeIndex1;
            int lastind = nodeIndex2;

            while (nodeind < lastind)
            {
                var node = nodeitems[nodeind];
                if (node.ItemCount <= 0) //intermediate node with child nodes
                {
                    Children = new List<Rsc6BVHBuilderNode>();
                    var cind1 = nodeind + 1;
                    var lcind = nodeind + node.ItemId; //(child node count)

                    while (cind1 < lcind)
                    {
                        var cnode = nodeitems[cind1];
                        var ccount = (cnode.ItemCount <= 0) ? cnode.ItemId : 1;
                        var cind2 = cind1 + ccount;
                        var chi = new Rsc6BVHBuilderNode();
                        chi.Unbuild(bvh, cind1, cind2);
                        Children.Add(chi);
                        cind1 = cind2;
                    }
                    nodeind += node.ItemId;
                }
                else //leaf node, with polygons
                {
                    Items = new List<Rsc6BVHBuilderItem>();
                    for (int i = 0; i < node.ItemCount; i++)
                    {
                        var item = new Rsc6BVHBuilderItem();
                        item.Index = node.ItemId + i;
                        Items.Add(item);
                    }
                    nodeind++;
                }
                Min = node.Min * q + c;
                Max = node.Max * q + c;
            }
        }

        public override string ToString()
        {
            var fstr = (Children != null) ? (TotalNodes.ToString() + ", 0 - ") : (Items != null) ? ("i, " + TotalItems.ToString() + " - ") : "error!";
            var cstr = (Children != null) ? (Children.Count.ToString() + " children") : "";
            var istr = (Items != null) ? (Items.Count.ToString() + " items") : "";
            if (string.IsNullOrEmpty(cstr)) return fstr + istr;
            if (string.IsNullOrEmpty(istr)) return fstr + cstr;
            return cstr + ", " + istr;
        }
    }

    public class Rsc6BVHBuilderItem
    {
        public Vector3 Min;
        public Vector3 Max;
        public int Index;
        public Rsc6Bounds Bounds;
        public Rsc6BoundPolygon Polygon;
    }

    [TC(typeof(EXP))] public struct Rsc6BoundsMaterialType
    {
        public byte Index;

        public readonly Rsc6BoundsMaterialData MaterialData
        {
            get
            {
                return Rsc6BoundsMaterialTypes.GetMaterial(this);
            }
        }

        public readonly Colour Colour
        {
            get
            {
                var mat = MaterialData;
                if (mat != null)
                {
                    return mat.Colour;
                }
                return Colour.Red;
            }
        }

        public Rsc6BoundsMaterialType(byte index)
        {
            this.Index = index;
        }

        public override readonly string ToString()
        {
            return Rsc6BoundsMaterialTypes.GetMaterialName(this);
        }

        public static implicit operator byte(Rsc6BoundsMaterialType matType)
        {
            return matType.Index; //implicit conversion
        }

        public static implicit operator Rsc6BoundsMaterialType(byte b)
        {
            return new Rsc6BoundsMaterialType() { Index = b };
        }
    }

    [TC(typeof(EXP))] public struct Rsc6BoundGeometryBVHNode
    {
        public short MinX;
        public short MinY;
        public short MinZ;
        public short MaxX;
        public short MaxY;
        public short MaxZ;
        public short ItemId;
        public byte ItemCount;
        public byte Padding1;

        public Vector3 Min
        {
            get { return new Vector3(MinX, MinY, MinZ); }
            set
            {
                MinX = (short)FloatUtil.Clamp(MathF.Floor(value.X), -32767, 32767);
                MinY = (short)FloatUtil.Clamp(MathF.Floor(value.Y), -32767, 32767);
                MinZ = (short)FloatUtil.Clamp(MathF.Floor(value.Z), -32767, 32767);
            }
        }

        public Vector3 Max
        {
            get { return new Vector3(MaxX, MaxY, MaxZ); }
            set
            {
                MaxX = (short)FloatUtil.Clamp(MathF.Ceiling(value.X), -32767, 32767);
                MaxY = (short)FloatUtil.Clamp(MathF.Ceiling(value.Y), -32767, 32767);
                MaxZ = (short)FloatUtil.Clamp(MathF.Ceiling(value.Z), -32767, 32767);
            }
        }

        public override string ToString()
        {
            return ItemId.ToString() + ": " + ItemCount.ToString();
        }
    }

    [TC(typeof(EXP))] public struct Rsc6BoundGeometryBVHTree
    {
        public short MinX;
        public short MinY;
        public short MinZ;
        public short MaxX;
        public short MaxY;
        public short MaxZ;
        public short NodeIndex1;
        public short NodeIndex2;

        public Vector3 Min
        {
            get { return new Vector3(MinX, MinY, MinZ); }
            set
            {
                MinX = (short)FloatUtil.Clamp(MathF.Floor(value.X), -32767, 32767);
                MinY = (short)FloatUtil.Clamp(MathF.Floor(value.Y), -32767, 32767);
                MinZ = (short)FloatUtil.Clamp(MathF.Floor(value.Z), -32767, 32767);
            }
        }

        public Vector3 Max
        {
            get { return new Vector3(MaxX, MaxY, MaxZ); }
            set
            {
                MaxX = (short)FloatUtil.Clamp(MathF.Ceiling(value.X), -32767, 32767);
                MaxY = (short)FloatUtil.Clamp(MathF.Ceiling(value.Y), -32767, 32767);
                MaxZ = (short)FloatUtil.Clamp(MathF.Ceiling(value.Z), -32767, 32767);
            }
        }

        public override string ToString()
        {
            return NodeIndex1.ToString() + ", " + NodeIndex2.ToString() + "  (" + (NodeIndex2 - NodeIndex1).ToString() + " nodes)";
        }
    }

    [TC(typeof(EXP))] public struct Rsc6BoundMaterial
    {
        public uint Data;

        public Rsc6BoundsMaterialType Type
        {
            get => (Rsc6BoundsMaterialType)(this.Data & 0xFFu);
            set => this.Data = (this.Data & 0xFFFFFF00u) | ((uint)value & 0xFFu);
        }

        public Rsc6BoundMaterial(uint value)
        {
            this.Data = value;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(this.Data);
        }

        public override string ToString()
        {
            return this.Type.ToString();
        }
    }

    public struct Rsc6BoundCompositeChildrenFlags : MetaNode
    {
        public Rsc6ObjectTypeFlags TypeFlags; //type(s) for this object
        public Rsc6ObjectTypeFlags IncludeFlags; //what type(s) this object can collide with

        public void Read(MetaNodeReader reader)
        {
            TypeFlags = reader.ReadEnum<Rsc6ObjectTypeFlags>("TypeFlags");
            IncludeFlags = reader.ReadEnum<Rsc6ObjectTypeFlags>("IncludeFlags");
        }

        public readonly void Write(MetaNodeWriter writer)
        {
            writer.WriteEnum("TypeFlags", TypeFlags);
            writer.WriteEnum("IncludeFlags", IncludeFlags);
        }

        public override string ToString()
        {
            return TypeFlags.ToString() + ", " + IncludeFlags.ToString();
        }
    }

    public struct Rsc6BoundPolyTriangle
    {
        public float Area;
        public ushort TriIndex1;
        public ushort TriIndex2;
        public ushort TriIndex3;
        public short EdgeIndex1;
        public short EdgeIndex2;
        public short EdgeIndex3;
    }

    public struct Rsc6BoundPolySphere
    {
        public byte SphereType;
        public byte Pad0;
        public ushort CenterIndex;
        public float SphereRadius;
        public uint Unused0;
        public uint Unused1;
    }

    public struct Rsc6BoundPolyCapsule
    {
        public byte CapsuleType;
        public byte Pad0;
        public ushort EndIndex0;
        public float CapsuleRadius;
        public ushort EndIndex1;
        public ushort Unused0;
        public uint Unused1;
    }

    public struct Rsc6BoundPolyBox
    {
        public uint BoxType;
        public ushort BoxIndex1;
        public ushort BoxIndex2;
        public ushort BoxIndex3;
        public ushort BoxIndex4;
        public uint Unused0;
    }

    public struct Rsc6BoundPolyCylinder
    {
        public byte PrimType;
        public byte Pad0;
        public ushort EndIndex0;
        public float Radius;
        public ushort EndIndex1;
        public ushort Pad1_1;
        public uint Pad1_2;
    }

    [Flags] public enum Rsc6BoundCompositeFlags : uint
    {
        NONE = 0u,
        UNKNOWN = 1u,
        MAP_WEAPON = 1u << 1,
        MAP_DYNAMIC = 1u << 2,
        MAP_ANIMAL = 1u << 3,
        MAP_COVER = 1u << 4,
        MAP_VEHICLE = 1u << 5,
        VEHICLE_NOT_BVH = 1u << 6,
        VEHICLE_BVH = 1u << 7,
        VEHICLE_BOX = 1u << 8,
        PED = 1u << 9,
        RAGDOLL = 1u << 10,
        ANIMAL = 1u << 11,
        ANIMAL_RAGDOLL = 1u << 12,
        OBJECT = 1u << 13,
        OBJECT_ENV_CLOTH = 1u << 14,
        PLANT = 1u << 15,
        PROJECTILE = 1u << 16,
        EXPLOSION = 1u << 17,
        PICKUP = 1u << 18,
        FOLIAGE = 1u << 19,
        FORKLIFT_FORKS = 1u << 20,
        TEST_WEAPON = 1u << 21,
        TEST_CAMERA = 1u << 22,
        TEST_AI = 1u << 23,
        TEST_SCRIPT = 1u << 24,
        TEST_VEHICLE_WHEEL = 1u << 25,
        GLASS = 1u << 26,
        MAP_RIVER = 1u << 27,
        SMOKE = 1u << 28,
        UNSMASHED = 1u << 29,
        MAP_STAIRS = 1u << 30,
        MAP_DEEP_SURFACE = 1u << 31
    }

    public enum Rsc6BoundPolygonType : byte
    {
        Triangle = 255,
        Sphere = 1,
        Capsule = 2,
        Box = 3
    }
}