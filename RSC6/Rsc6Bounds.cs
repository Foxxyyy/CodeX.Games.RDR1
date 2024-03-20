using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Numerics;
using CodeX.Core.Physics;
using CodeX.Games.RDR1.RPF6;
using System.Linq;
using System.Text.RegularExpressions;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.IO;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6BoundsDictionary : Rsc6BlockBase
    {
        public override ulong BlockLength => 24;
        public uint VFT { get; set; }
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public JenkHash ParentDictionary { get; set; }
        public uint UsageCount { get; set; }
        public Rsc6Arr<JenkHash> Hashes { get; set; }
        public Rsc6PtrArr<Rsc6Bounds> Bounds { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            ParentDictionary = reader.ReadUInt32();
            UsageCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Bounds = reader.ReadPtrArr(Rsc6Bounds.Create);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
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

    public class Rsc6Bounds : Collider, Rsc6Block
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
        public Vector4 BoxMax { get; set; } //m_BoundingBoxMax
        public Vector4 BoxMin { get; set; } //m_BoundingBoxMin
        public Vector4 BoxCenter { get; set; } //m_CentroidOffset, offset of the centroid from the local coordinate system origin
        public Vector4 CentroidOffsetWorldSpace { get; set; } //m_CentroidOffsetWorldSpace
        public Vector4 SphereCenter { get; set; } //m_CGOffset, center of gravity location in the local coordinate system
        public Vector4 VolumeDistribution { get; set; } //m_VolumeDistribution, angular inertia that this bound would have with a mass of 1kg and the bound's volume (element w).
        public Vector3 Margin { get; set; } //m_MarginV, the distance by which collision detection will be expanded beyond the bound's surface
        public uint RefCount { get; set; } //Number of physics instances (or sometimes other classes) using this bound

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
            BoxMax = reader.ReadVector4();
            BoxMin = reader.ReadVector4();
            BoxCenter = reader.ReadVector4();
            CentroidOffsetWorldSpace = reader.ReadVector4();
            SphereCenter = reader.ReadVector4();
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
            writer.WriteVector4(BoxMax);
            writer.WriteVector4(BoxMin);
            writer.WriteVector4(BoxCenter);
            writer.WriteVector4(CentroidOffsetWorldSpace);
            writer.WriteVector4(SphereCenter);
            writer.WriteVector4(VolumeDistribution);
            writer.WriteVector3(Margin);
            writer.WriteUInt32(RefCount);
        }

        public override void Read(MetaNodeReader reader)
        {
            UserData = reader.ReadUInt32Array("UserData");
            Type = (Rsc6BoundsType)reader.ReadByte("Type");
            HasCentroidOffset = reader.ReadBool("HasCentroidOffset");
            HasCGOffset = reader.ReadBool("HasCGOffset");
            WorldSpaceUpdatesEnabled = reader.ReadBool("WorldSpaceUpdatesEnabled");
            SphereRadius = reader.ReadSingle("SphereRadius");
            WorldRadius = reader.ReadSingle("WorldRadius");
            BoxMax = reader.ReadVector4("BoxMax");
            BoxMin = reader.ReadVector4("BoxMin");
            BoxCenter = reader.ReadVector4("BoxCenter");
            CentroidOffsetWorldSpace = reader.ReadVector4("CentroidOffsetWorldSpace");
            SphereCenter = reader.ReadVector4("SphereCenter");
            VolumeDistribution = reader.ReadVector4("VolumeDistribution");
            Margin = reader.ReadVector3("Margin");
            RefCount = reader.ReadUInt32("RefCount");
        }

        public override void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32Array("UserData", UserData);
            writer.WriteByte("Type", (byte)Type);
            writer.WriteBool("HasCentroidOffset", HasCentroidOffset);
            writer.WriteBool("HasCGOffset", HasCGOffset);
            writer.WriteBool("WorldSpaceUpdatesEnabled", WorldSpaceUpdatesEnabled);
            writer.WriteSingle("SphereRadius", SphereRadius);
            writer.WriteSingle("WorldRadius", WorldRadius);
            writer.WriteVector4("BoxMax", BoxMax);
            writer.WriteVector4("BoxMin", BoxMin);
            writer.WriteVector4("BoxCenter", BoxCenter);
            writer.WriteVector4("CentroidOffsetWorldSpace", CentroidOffsetWorldSpace);
            writer.WriteVector4("SphereCenter", SphereCenter);
            writer.WriteVector4("VolumeDistribution", VolumeDistribution);
            writer.WriteVector3("Margin", Margin);
            writer.WriteUInt32("RefCount", RefCount);
        }

        public static Rsc6Bounds Create(Rsc6DataReader r)
        {
            r.Position += 20;
            var type = (Rsc6BoundsType)r.ReadByte();
            r.Position -= 21;

            return type switch
            {
                Rsc6BoundsType.Sphere => new Rsc6BoundSphere(),
                Rsc6BoundsType.Capsule => new Rsc6BoundCapsule(),
                Rsc6BoundsType.Box => new Rsc6BoundBox(),
                Rsc6BoundsType.Geometry => new Rsc6BoundGeometry(),
                Rsc6BoundsType.GeometryBVH => new Rsc6BoundGeometryBVH(),
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
                Rsc6BoundsType.Composite => ColliderType.None,
                Rsc6BoundsType.Triangle => ColliderType.Triangle,
                _ => ColliderType.None,
            };
        }

        public override string ToString()
        {
            return $"{Type} : {BoxMin} : {BoxMax}";
        }
    }

    public class Rsc6BoundSphere : Rsc6Bounds
    {
        public override ulong BlockLength => base.BlockLength + 24;
        public Vector3 Radius { get; set; }
        public uint Unknown5 { get; set; }
        public Rsc6BoundMaterial Material { get; set; }

        public Rsc6BoundSphere() : base(Rsc6BoundsType.Sphere)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Radius = reader.ReadVector3();
            Unknown5 = reader.ReadUInt32();
            Material = reader.ReadStruct<Rsc6BoundMaterial>();
            _ = reader.ReadUInt32();

            PartColour = Material.Type.Colour;
            PartSize = new Vector3(SphereRadius, 0.0f, 0.0f);
            ComputeMass(ColliderType.Sphere, PartSize, 1.0f);
            ComputeBodyInertia();
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Radius = reader.ReadVector3("Radius");
            Unknown5 = reader.ReadUInt32("Unknown5");
            Material = new Rsc6BoundMaterial(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector3("Radius", Radius);
            writer.WriteUInt32("Unknown5", Unknown5);
            Material.Write(writer);
        }
    }

    public class Rsc6BoundCapsule : Rsc6Bounds
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
        public uint Unknown_5Ch { get; set; } //Padding

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

            PartColour = Material.Type.Colour;
            PartSize = new Vector3(CapsuleRadius.X, CapsuleLength.X, 0.0f);
            ComputeMass(ColliderType.Capsule, PartSize, 1.0f);
            ComputeBodyInertia();
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            CapsuleRadius = reader.ReadVector4("CapsuleRadius");
            CapsuleLength = reader.ReadVector4("CapsuleLength");
            EndPointsWorldSpace0 = reader.ReadVector4("EndPointsWorldSpace0");
            EndPointsWorldSpace1 = reader.ReadVector4("EndPointsWorldSpace1");
            Material = new Rsc6BoundMaterial(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("CapsuleRadius", CapsuleRadius);
            writer.WriteVector4("CapsuleLength", CapsuleLength);
            writer.WriteVector4("EndPointsWorldSpace0", EndPointsWorldSpace0);
            writer.WriteVector4("EndPointsWorldSpace1", EndPointsWorldSpace1);
            Material.Write(writer);
        }
    }

    public class Rsc6BoundBox : Rsc6BoundPolyhedron
    {
        //A class to represent a physics bound in the shape of a rectangular prism.
        //A phBoundBox is specified by its length, width and height.
        //The principal axes of the box are always the local axes of the bound.
        //Boxes can be created used polygons (via a phBoundPolyhedron) but using a phBoundBox allows for greater efficiency

        public override ulong BlockLength => base.BlockLength + 352;
        public Vector4 BoxSize { get; set; } //m_BoxSize
        public new Vector4[] Vertices { get; set; } //Vec3V[8] equal to (VerticesData[i] * Quantum) + CenterGeom;
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
            base.Read(reader); //phBoundPolyhedron
            BoxSize = reader.ReadVector4();
            Vertices = reader.ReadVector4Arr(8);
            Unknown_170h = reader.ReadUInt32Arr(48); //TODO: research this
            Material = reader.ReadStruct<Rsc6BoundMaterial>();
            Unknown_154h = reader.ReadUInt32();
            Unknown_158h = reader.ReadUInt32();
            Unknown_15Ch = reader.ReadUInt32();

            VertexColours = reader.ReadArray<Colour>(VerticesCount, VertexColoursPtr);
            VerticesData = reader.ReadArray<Vector3S>(VerticesCount, VerticesPtr);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            BoxSize = reader.ReadVector4("BoxSize");
            Vertices = reader.ReadVector4Array("Vertices");
            Material = new Rsc6BoundMaterial(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("BoxSize", BoxSize);
            writer.WriteVector4Array("Vertices", Vertices);
            Material.Write(writer);
        }
    }

    public class Rsc6BoundGeometry : Rsc6BoundPolyhedron //rage::phBoundGeometry
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
        public byte Unknown_2Ch { get; set; } //pad[3]
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
            PolyMatIndexList = reader.ReadRawArrItems(PolyMatIndexList, PolygonsCount);

            base.Materials = MaterialsIDs.Items;
            base.PolygonMaterialIndices = PolyMatIndexList.Items;
            base.CreateMesh();

            PartSize = BoxMax.XYZ() - BoxMin.XYZ();
            ComputeMass(ColliderType.Box, PartSize, 1.0f); //just an approximation to work with
            ComputeBasicBodyInertia(ColliderType.Box, PartSize); //just an approximation to work with
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Materials = reader.ReadNodeArray<Rsc6BoundMaterial>("Materials");
            PolyMatIndexList = new(reader.ReadByteArray("PolyMatIndexList"));
            MaterialsCount = (byte)Materials.Length;
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            if (Materials != null) writer.WriteNodeArray("Materials", Materials);
            writer.WriteByteArray("PolyMatIndexList", PolyMatIndexList.Items);
        }
    }

    public class Rsc6BoundCurvedGeometry : Rsc6BoundGeometry
    {
        //Represents a physics bound with generalized vertex locations and polygons, including curved polygons and curved edges

        public override ulong BlockLength => base.BlockLength + 32;
        public Rsc6Ptr<Rsc6BoundCurvedFace> CurvedFaces { get; set; } //m_CurvedFaces, phCurvedFace
        public Rsc6Ptr<Rsc6BoundCurvedEdge> CurvedEdges { get; set; } //m_CurvedEdges, phCurvedEdge
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
            CurvedFaces = reader.ReadPtr<Rsc6BoundCurvedFace>();
            CurvedEdges = reader.ReadPtr<Rsc6BoundCurvedEdge>();
            CurvedFaceMatIndexList = reader.ReadRawArrPtr<byte>();
            NumCurvedFaces = reader.ReadInt32();
            NumCurvedEdges = reader.ReadInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            CurvedFaceMatIndexList = reader.ReadRawArrItems(CurvedFaceMatIndexList, 6);
        }
        
        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer); //phBoundGeometry
            writer.WritePtr(CurvedFaces);
            writer.WritePtr(CurvedEdges);
            writer.WriteRawArrPtr(CurvedFaceMatIndexList);
            writer.WriteInt32(NumCurvedFaces);
            writer.WriteInt32(NumCurvedEdges);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(Unknown_1Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            CurvedFaces = new(reader.ReadNode<Rsc6BoundCurvedFace>("CurvedFaces"));
            CurvedEdges = new(reader.ReadNode<Rsc6BoundCurvedEdge>("CurvedEdges"));
            CurvedFaceMatIndexList = new(reader.ReadByteArray("CurvedFaceMatIndexList"));
            NumCurvedFaces = reader.ReadInt32("NumCurvedFaces");
            NumCurvedEdges = reader.ReadInt32("NumCurvedEdges");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            if (CurvedFaces.Item != null) writer.WriteNode("CurvedFaces", CurvedFaces.Item);
            if (CurvedEdges.Item != null) writer.WriteNode("CurvedEdges", CurvedEdges.Item);
            if (CurvedFaceMatIndexList.Items != null) writer.WriteByteArray("CurvedFaceMatIndexList", CurvedFaceMatIndexList.Items);
            writer.WriteInt32("NumCurvedFaces", NumCurvedFaces);
            writer.WriteInt32("NumCurvedEdges", NumCurvedEdges);
        }
    }

    public class Rsc6BoundCurvedFace : Rsc6Block, MetaNode
    {
        //Curved face for a curved geometry bound

        public ulong FilePosition { get; set; }
        public ulong BlockLength => 96;
        public bool IsPhysical => false;

        public Rsc6BoundPolygonTriangle Polygons { get; set; }
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
            byte[] temp = reader.ReadBytes(16);
            Polygons = new Rsc6BoundPolygonTriangle();
            Polygons.Read(new BinaryReader(new MemoryStream(temp)));

            CurvatureCenter = reader.ReadVector4();
            UnitNormal = reader.ReadVector4();
            OuterRadius = reader.ReadSingle();
            InnerRadius = reader.ReadSingle();
            MinCosine = reader.ReadSingle();
            CurvedEdgeIndices = reader.ReadArray<ushort>(4);
            CurvedEdgePolyIndices = reader.ReadArray<ushort>(4);
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
            var temp = new byte[16];
            Polygons.Write(new BinaryWriter(new MemoryStream(temp)));

            writer.WriteBytes(temp);
            writer.WriteVector4(CurvatureCenter);
            writer.WriteVector4(UnitNormal);
            writer.WriteSingle(OuterRadius);
            writer.WriteSingle(InnerRadius);
            writer.WriteSingle(MinCosine);
            writer.WriteArray(CurvedEdgeIndices);
            writer.WriteArray(CurvedEdgePolyIndices);
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
            CurvatureCenter = reader.ReadVector4("CurvatureCenter");
            UnitNormal = reader.ReadVector4("UnitNormal");
            OuterRadius = reader.ReadSingle("OuterRadius");
            InnerRadius = reader.ReadSingle("InnerRadius");
            MinCosine = reader.ReadSingle("MinCosine");
            CurvedEdgeIndices = reader.ReadUInt16Array("CurvedEdgeIndices");
            CurvedEdgePolyIndices = reader.ReadUInt16Array("CurvedEdgePolyIndices");
            FourthVertex = reader.ReadUInt16("FourthVertex");
            IsCircularFace = reader.ReadBool("IsCircularFace");
        }

        public void Write(MetaNodeWriter writer)
        {
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

    public class Rsc6BoundCurvedEdge : Rsc6Block, MetaNode
    {
        public ulong FilePosition { get; set; }
        public ulong BlockLength => 48;
        public bool IsPhysical => false;

        public Vector4 CurvatureCenter { get; set; } //m_CurvatureCenter
        public Vector4 PlaneNormal { get; set; } //m_PlaneNormal, the unit normal vector out of the plane of curvature
        public float Radius { get; set; } //m_Radius, distance between CurvatureCenter and VertexIndices[0] (radius of the curvature)
        public int[] VertexIndices { get; set; } //m_VertexIndices, the two vertex of the curved edge
        public uint UnusedInt { get; set; } = 0xCDCDCDCD; //m_UnusedInt

        public void Read(Rsc6DataReader reader)
        {
            CurvatureCenter = reader.ReadVector4();
            PlaneNormal = reader.ReadVector4();
            Radius = reader.ReadSingle();
            VertexIndices = reader.ReadArray<int>(2);
            UnusedInt = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(CurvatureCenter);
            writer.WriteVector4(PlaneNormal);
            writer.WriteSingle(Radius);
            writer.WriteInt32Array(VertexIndices);
            writer.WriteUInt32(UnusedInt);
        }

        public void Read(MetaNodeReader reader)
        {
            CurvatureCenter = reader.ReadVector4("CurvatureCenter");
            PlaneNormal = reader.ReadVector4("PlaneNormal");
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

    public class Rsc6BoundPolyhedron : Rsc6Bounds //rage::phBoundPolyhedron
    {
        public override ulong BlockLength => 224;
        public uint VerticesPad { get; set; } //m_VerticesPad
        public uint VertexColoursPtr { get; set; } //m_CompressedShrunkVertices, vertices that have been shrunk inwards by a margin
        public uint VerticesWorldSpace { get; set; } //m_VerticesWorldSpace
        public Rsc6RawArr<byte> PolygonsData { get; set; } //m_Polygons
        public Vector4 Quantum { get; set; } //m_UnQuantizeFactor
        public Vector4 CenterGeom { get; set; } //m_BoundingBoxCenter
        public uint VerticesPtr { get; set; } //m_CompressedVertices
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
        public Colour[] VertexColours { get; set; }
        public Vector3S[] VerticesData { get; set; }
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
            VertexColoursPtr = reader.ReadUInt32();
            VerticesWorldSpace = reader.ReadUInt32();
            PolygonsData = reader.ReadRawArrPtr<byte>();
            Quantum = reader.ReadVector4();
            CenterGeom = reader.ReadVector4();
            VerticesPtr = reader.ReadUInt32();
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

            VertexColours = reader.ReadArray<Colour>(VerticesCount, VertexColoursPtr);
            VerticesData = reader.ReadArray<Vector3S>(VerticesCount, VerticesPtr);
            PolygonsData = reader.ReadRawArrItems(PolygonsData, PolygonsCount * 16);
            ReadPolygons();

            if (VerticesData != null)
            {
                Vertices = new Vector3[VerticesData.Length];
                for (int i = 0; i < VerticesData.Length; i++)
                {
                    var bv = VerticesData[i];
                    bv = new Vector3S(bv.Z, bv.X, bv.Y);
                    Vertices[i] = bv.ToVector3(Quantum.XYZ()) + CenterGeom.XYZ();
                }
            }

            CreateMesh();

            PartSize = BoxMax.XYZ() - BoxMin.XYZ();
            ComputeMass(ColliderType.Box, PartSize, 1.0f); //just an approximation to work with
            ComputeBasicBodyInertia(ColliderType.Box, PartSize); //just an approximation to work with
        }

        public override void Read(MetaNodeReader reader) //TODO: finish this
        {
            base.Read(reader);
            throw new NotImplementedException("phBoundPolyhedron : 'Read' method not implemented");
        }

        public override void Write(MetaNodeWriter writer) //TODO: finish this
        {
            base.Write(writer);
            throw new NotImplementedException("phBoundPolyhedron : 'Write' method not implemented");
        }

        public void CreateMesh()
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

            int addVertex(int index, Vector3 norm, Colour matcol)
            {
                var c = verts.Count;
                verts.Add(new ShapeVertex()
                {
                    Position = new Vector4(Vertices[index], 1),
                    Normal = norm,
                    Colour = (usevertexcolours && (VertexColours != null)) ? VertexColours[index] : matcol,
                    Texcoord = Vector2.Zero,
                    Tangent = Vector3.Zero
                });
                return c;
            }

            for (int i = 0; i < PolygonsCount; i++)
            {
                var p = Polygons[i];
                var matind = p.MaterialIndex;
                var matcol = getMaterialColour(matind);
                var matcolparam = (uint)(matcol.ToRgba() << 8);

                if (p is Rsc6BoundPolygonTriangle ptri)
                {
                    var v1 = ptri.Vertex1;
                    var v2 = ptri.Vertex2;
                    var v3 = ptri.Vertex3;
                    var e1 = Vector3.Normalize(v2 - v1);
                    var e2 = Vector3.Normalize(v3 - v1);
                    var nm = Vector3.Normalize(Vector3.Cross(e1, e2));
                    var i1 = (ushort)addVertex(ptri.TriIndex1, nm, matcol);
                    var i2 = (ushort)addVertex(ptri.TriIndex2, nm, matcol);
                    var i3 = (ushort)addVertex(ptri.TriIndex3, nm, matcol);
                    indsl.Add(i1);
                    indsl.Add(i2);
                    indsl.Add(i3);
                }
                else if (p is Rsc6BoundPolygonBox pbox)
                {
                    var v1 = pbox.Vertex1;
                    var v2 = pbox.Vertex2;
                    var v3 = pbox.Vertex3;
                    var v4 = pbox.Vertex4;
                    var x = (v1 + v2) - (v3 + v4);
                    var y = (v1 + v3) - (v2 + v4);
                    var z = (v1 + v4) - (v2 + v3);
                    var t = (v1 + v2 + v3 + v4) * 0.25f;
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
                    inst.ParamW = matcolparam + 1;//signal to use box transform
                    boxes.Add(inst);
                }
                else if (p is Rsc6BoundPolygonCylinder pcyl)
                {
                    var v1 = pcyl.Vertex1;
                    var v2 = pcyl.Vertex2;
                    var t = v1;
                    var a = v2 - v1;
                    var h = a.Length();
                    var y = Vector3.Normalize(a);
                    var x = Vector3.Normalize(y.GetPerpVec());
                    var z = Vector3.Normalize(Vector3.Cross(x, y));
                    var r = pcyl.Radius;

                    inst.Matrix = new Matrix3x4(new Vector4(x, t.X), new Vector4(y, t.Y), new Vector4(z, t.Z));
                    inst.ParamX = r;
                    inst.ParamY = h;
                    inst.ParamZ = 0;
                    inst.ParamW = matcolparam + 4; //signal to use cylinder transform
                    cylinders.Add(inst);
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

        private void ReadPolygons()
        {
            if (PolygonsCount == 0) return;

            var polygonData = PolygonsData.Items;
            using var ms = new MemoryStream(polygonData);
            var br = new BinaryReader(ms);

            Polygons = new Rsc6BoundPolygon[PolygonsCount];
            for (int i = 0; i < PolygonsCount; i++)
            {
                var offset = i * 16;
                ms.Position = offset;

                var type = (Rsc6BoundPolygonType)polygonData[offset];
                var area = br.ReadSingle();
                ms.Position = offset;

                if ((area > 0.0f && area < 10000.0f) && !Rpf6Crypto.IsDefinedInEnumRange<Rsc6BoundPolygonType>((byte)type))
                {
                    type = Rsc6BoundPolygonType.Triangle;
                    PartShape = EditablePartShape.TriMesh;
                }

                var p = CreatePolygon(type);
                if (p != null)
                {
                    p.Index = i;
                    p.Read(br);
                }
                Polygons[i] = p;
            }
        }

        private Rsc6BoundPolygon CreatePolygon(Rsc6BoundPolygonType type)
        {
            Rsc6BoundPolygon p = null;
            switch (type)
            {
                case Rsc6BoundPolygonType.Box:
                    p = new Rsc6BoundPolygonBox();
                    break;
                case Rsc6BoundPolygonType.Cylinder:
                case Rsc6BoundPolygonType.Cylinder1:
                    p = new Rsc6BoundPolygonCylinder();
                    break;
                case Rsc6BoundPolygonType.Triangle:
                    p = new Rsc6BoundPolygonTriangle();
                    break;
                default:
                    break;
            }

            if (p != null)
            {
                p.Owner = this;
            }
            return p;
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

    public class Rsc6BoundGeometryBVH : Rsc6BoundGeometry //phBoundBVH
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

        public override void Read(MetaNodeReader reader) //TODO: finish this
        {
            base.Read(reader);
            throw new NotImplementedException("phBoundBVH : 'Read' method not implemented");
        }

        public override void Write(MetaNodeWriter writer) //TODO: finish this
        {
            base.Write(writer);
            throw new NotImplementedException("phBoundBVH : 'Write' method not implemented");
        }
    }

    public class Rsc6BoundComposite : Rsc6Bounds
    {
        public override ulong BlockLength => 144;
        public uint ChildrenPtr { get; set; } //m_Bounds
        public Rsc6RawArr<Matrix4x4> CurrentMatrices { get; set; } //m_CurrentMatrices
        public Rsc6RawArr<Matrix4x4> LastMatrices { get; set; } //m_LastMatrices
        public Rsc6RawArr<BoundingBox4> LocalBoxMinMaxs { get; set; } //m_LocalBoxMinMaxs
        public uint ChildrenFlags { get; set; } //m_OwnedTypeAndIncludeFlags
        public Rsc6ManagedArr<Rsc6BoundFlags> Childs { get; set; } //m_Childs, same pointer as m_OwnedTypeAndIncludeFlags but with count/capacity
        public bool ContainsBVH { get; set; } //m_ContainsBVH
        public byte Pad { get; set; } //pad, always 0
        public ushort NumActiveBounds { get; set; } //m_NumActiveBounds

        public Rsc6Bounds[] Childrens { get; set; }

        public Rsc6BoundComposite() : base(Rsc6BoundsType.Composite)
        {
        }

        public override void Read(Rsc6DataReader reader) //phBoundComposite
        {
            base.Read(reader); //phBounds
            ChildrenPtr = reader.ReadUInt32();
            CurrentMatrices = reader.ReadRawArrPtr<Matrix4x4>();
            LastMatrices = reader.ReadRawArrPtr<Matrix4x4>();
            LocalBoxMinMaxs = reader.ReadRawArrPtr<BoundingBox4>();
            ChildrenFlags = reader.ReadUInt32();
            Childs = reader.ReadArr<Rsc6BoundFlags>();
            ContainsBVH = reader.ReadBoolean();
            Pad = reader.ReadByte();
            NumActiveBounds = reader.ReadUInt16();

            var childPtrs = reader.ReadArray<uint>(NumActiveBounds, ChildrenPtr);
            CurrentMatrices = reader.ReadRawArrItems(CurrentMatrices, NumActiveBounds);
            LastMatrices = reader.ReadRawArrItems(LastMatrices, NumActiveBounds);
            LocalBoxMinMaxs = reader.ReadRawArrItems(LocalBoxMinMaxs, NumActiveBounds);

            if (childPtrs != null)
            {
                var numBounds = Math.Min(NumActiveBounds, childPtrs.Length);
                Childrens = new Rsc6Bounds[numBounds];
                for (int i = 0; i < numBounds; i++)
                {
                    Childrens[i] = reader.ReadBlock(childPtrs[i], Create);
                    if (Childrens[i] != null)
                    {
                        Childrens[i].Name = "Child" + i.ToString();
                        if ((CurrentMatrices.Items != null) && (i < CurrentMatrices.Items.Length))
                        {
                            var m = new Matrix3x4(CurrentMatrices[i]);
                            var height = Rsc6Fragment.IsSkinnedPed ? 1.0f : 0.0f;
                            m.Translation = new Vector3(m.Translation.Z, m.Translation.X, m.Translation.Y - height);
                            m.Orientation = new Quaternion(m.Orientation.Z, m.Orientation.X, m.Orientation.Y, m.Orientation.W);

                            if (Childrens[i] is Rsc6BoundCapsule)
                            {
                                var rotationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2);
                                m.Orientation *= rotationQuaternion;
                            }
                            Childrens[i].PartTransform = m;
                        }
                    }
                }
            }
            PartChildren = Childrens;
            UpdateBounds();
        }

        public override void Read(MetaNodeReader reader) //TODO: finish this
        {
            base.Read(reader);
            throw new NotImplementedException("phBoundBVH : 'Read' method not implemented");
        }

        public override void Write(MetaNodeWriter writer) //TODO: finish this
        {
            base.Write(writer);
            throw new NotImplementedException("phBoundBVH : 'Write' method not implemented");
        }
    }

    public class Rsc6BoundGeometryBVHRoot : Rsc6BlockBase
    {
        public override ulong BlockLength => 88;
        public Rsc6Arr<Rsc6BoundGeometryBVHNode> Nodes { get; set; }
        public uint Depth { get; set; } //depth of the hierarchy? but value is 0xCDCDCDCD
        public Vector4 BoundingBoxMin { get; set; }
        public Vector4 BoundingBoxMax { get; set; }
        public Vector4 BoundingBoxCenter { get; set; }
        public Vector4 BVHQuantumInverse { get; set; } // 1 / BVHQuantum
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

            //Debug.Assert(false);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))] public struct Rsc6BoundsMaterialType
    {
        public byte Index { get; set; }

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

    [TC(typeof(EXP))] public class Rsc6BoundsMaterialData //Maybe we should use proper types?
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

    [TC(typeof(EXP))] public class Rsc6BoundFlags : Rsc6BlockBase
    {
        public override ulong BlockLength => 0;

        public Rsc6BoundFlags()
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))] public abstract class Rsc6BoundPolygon //phPrimitive
    {
        //Simple wrapper around a phPolygon

        public Rsc6BoundPolygonType Type { get; set; }
        public Rsc6BoundPolyhedron Owner { get; set; } //For browsing/editing convenience

        public Rsc6BoundMaterial Material
        {
            get
            {
                if (MaterialCustom.HasValue) return MaterialCustom.Value;
                return Owner?.GetMaterial(Index) ?? new Rsc6BoundMaterial();
            }
            set
            {
                MaterialCustom = value;
            }
        }

        public Rsc6BoundMaterial? MaterialCustom; //For editing, when assigning a new material

        public int MaterialIndex
        {
            get { return Owner?.GetMaterialIndex(Index) ?? -1; }
        }

        public Vector3[] VertexPositions
        {
            get
            {
                var inds = VertexIndices;
                var va = new Vector3[inds.Length];
                if (Owner != null)
                {
                    for (int i = 0; i < inds.Length; i++)
                    {
                        va[i] = Owner.GetVertexPos(inds[i]);
                    }
                }
                return va;
            }
            set
            {
                if (value == null) return;
                var inds = VertexIndices;
                if (Owner != null)
                {
                    var imax = Math.Min(inds.Length, value.Length);
                    for (int i = 0; i < imax; i++)
                    {
                        Owner.SetVertexPos(inds[i], value[i]);
                    }
                }
            }
        }

        public int Index { get; set; } //For editing convenience, not stored
        public abstract Vector3 BoxMin { get; }
        public abstract Vector3 BoxMax { get; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 Position { get; set; }
        public abstract Quaternion Orientation { get; set; }
        public abstract int[] VertexIndices { get; set; }
        public abstract void Read(BinaryReader br);
        public abstract void Write(BinaryWriter bw);

        public override string ToString()
        {
            return Type.ToString();
        }

        public virtual string Title
        {
            get
            {
                return Type.ToString() + " " + Index.ToString();
            }
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundPolygonTriangle : Rsc6BoundPolygon //rage::phPolygon
    {
        public float Area { get; set; } //m_Area, area of this polygon (in square meters)
        public ushort TriIndex1 { get; set; } //m_VertexIndices[3], indices of the vertices that make up this polygon
        public ushort TriIndex2 { get; set; }
        public ushort TriIndex3 { get; set; }
        public ushort EdgeIndex1 { get; set; } //m_NeighboringPolygons[3], indices of the polygons that are neighbors of this polygon
        public ushort EdgeIndex2 { get; set; }
        public ushort EdgeIndex3 { get; set; }

        private Quaternion? OrientationCached;
        private Vector3? ScaleCached;

        public Vector3 Vertex1
        {
            get { return (Owner != null) ? Owner.GetVertexPos(TriIndex1) : Vector3.Zero; }
            set { Owner?.SetVertexPos(TriIndex1, value); }
        }
        public Vector3 Vertex2
        {
            get { return (Owner != null) ? Owner.GetVertexPos(TriIndex2) : Vector3.Zero; }
            set { Owner?.SetVertexPos(TriIndex2, value); }
        }
        public Vector3 Vertex3
        {
            get { return (Owner != null) ? Owner.GetVertexPos(TriIndex3) : Vector3.Zero; }
            set { Owner?.SetVertexPos(TriIndex3, value); }
        }

        public override Vector3 BoxMin => Vector3.Min(Vector3.Min(Vertex1, Vertex2), Vertex3);
        public override Vector3 BoxMax => Vector3.Max(Vector3.Max(Vertex1, Vertex2), Vertex3);

        public override Vector3 Scale
        {
            get
            {
                if (ScaleCached.HasValue) return ScaleCached.Value;
                ScaleCached = Vector3.One;
                return Vector3.One;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var cen = (v1 + v2 + v3) * (1.0f / 3.0f);
                var trans = value / Scale;
                var ori = Orientation;
                var orinv = Quaternion.Inverse(ori);
                Vertex1 = cen + ori.Multiply(trans * orinv.Multiply(v1 - cen));
                Vertex2 = cen + ori.Multiply(trans * orinv.Multiply(v2 - cen));
                Vertex3 = cen + ori.Multiply(trans * orinv.Multiply(v3 - cen));
                ScaleCached = value;
            }
        
        }

        public override Vector3 Position 
        {
            get
            {
                return (Vertex1 + Vertex2 + Vertex3) * (1.0f / 3.0f);
            }
            set
            {
                var offset = value - Position;
                Vertex1 += offset;
                Vertex2 += offset;
                Vertex3 += offset;
            }
        }

        public override Quaternion Orientation
        {
            get
            {
                if (OrientationCached.HasValue) return OrientationCached.Value;
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var dir = v2 - v1;
                var side = Vector3.Cross((v3 - v1), dir);
                var up = Vector3.Normalize(Vector3.Cross(dir, side));
                var ori = Quaternion.Inverse(QuaternionExt.LookAtRH(Vector3.Zero, side, up));
                OrientationCached = ori;
                return ori;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var cen = (v1 + v2 + v3) * (1.0f / 3.0f);
                var trans = value * Quaternion.Inverse(Orientation);
                Vertex1 = cen + trans.Multiply(v1 - cen);
                Vertex2 = cen + trans.Multiply(v2 - cen);
                Vertex3 = cen + trans.Multiply(v3 - cen);
                OrientationCached = value;
            }
        }

        public override int[] VertexIndices
        {
            get => new int[] { TriIndex1, TriIndex2, TriIndex3 };
            set
            {
                if (value?.Length >= 3)
                {
                    TriIndex1 = (ushort)value[0];
                    TriIndex2 = (ushort)value[1];
                    TriIndex3 = (ushort)value[2];
                }
            }
        }

        public new byte MaterialIndex
        {
            get { return BitConverter.GetBytes(Area)[0]; }
            set
            {
                byte[] byteArray = BitConverter.GetBytes(Area);
                byteArray[0] = value;
                Area = BitConverter.ToSingle(byteArray, 0);
            }
        }

        public Rsc6BoundPolygonTriangle()
        {
            Type = Rsc6BoundPolygonType.Triangle;
        }

        public override void Read(BinaryReader br)
        {
            Area = br.ReadSingle();
            TriIndex1 = br.ReadUInt16();
            TriIndex2 = br.ReadUInt16();
            TriIndex3 = br.ReadUInt16();
            EdgeIndex1 = br.ReadUInt16();
            EdgeIndex2 = br.ReadUInt16();
            EdgeIndex3 = br.ReadUInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Area);
            bw.Write(TriIndex1);
            bw.Write(TriIndex2);
            bw.Write(TriIndex3);
            bw.Write(EdgeIndex1);
            bw.Write(EdgeIndex2);
            bw.Write(EdgeIndex3);
        }

        public override string ToString()
        {
            return base.ToString() + ": Area: " + Area.ToString() + ", "+ TriIndex1.ToString() + ", " + TriIndex2.ToString() + ", " + TriIndex3.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundPolygonBox : Rsc6BoundPolygon
    {
        public uint BoxType { get; set; }
        public ushort BoxIndex1 { get; set; }
        public ushort BoxIndex2 { get; set; }
        public ushort BoxIndex3 { get; set; }
        public ushort BoxIndex4 { get; set; }
        public uint Unused0 { get; set; }

        private Quaternion? OrientationCached;
        private Vector3? ScaleCached;

        public Vector3 Vertex1
        {
            get { return (Owner != null) ? Owner.GetVertexPos(BoxIndex1) : Vector3.Zero; }
            set { if (Owner != null) Owner?.SetVertexPos(BoxIndex1, value); }
        }
        public Vector3 Vertex2
        {
            get { return (Owner != null) ? Owner.GetVertexPos(BoxIndex2) : Vector3.Zero; }
            set { if (Owner != null) Owner?.SetVertexPos(BoxIndex2, value); }
        }
        public Vector3 Vertex3
        {
            get { return (Owner != null) ? Owner.GetVertexPos(BoxIndex3) : Vector3.Zero; }
            set { if (Owner != null) Owner?.SetVertexPos(BoxIndex3, value); }
        }
        public Vector3 Vertex4
        {
            get { return (Owner != null) ? Owner.GetVertexPos(BoxIndex4) : Vector3.Zero; }
            set { if (Owner != null) Owner?.SetVertexPos(BoxIndex4, value); }
        }

        public override Vector3 BoxMin => Vector3.Min(Vector3.Min(Vector3.Min(Vertex1, Vertex2), Vertex3), Vertex4);
        public override Vector3 BoxMax => Vector3.Max(Vector3.Max(Vector3.Max(Vertex1, Vertex2), Vertex3), Vertex4);

        public override Vector3 Scale
        {
            get
            {
                if (ScaleCached.HasValue) return ScaleCached.Value;
                ScaleCached = Vector3.One;
                return Vector3.One;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var v4 = Vertex4;
                var cen = (v1 + v2 + v3 + v4) * 0.25f;
                var trans = value / Scale;
                var ori = Orientation;
                var orinv = Quaternion.Inverse(ori);
                Vertex1 = cen + ori.Multiply(trans * orinv.Multiply(v1 - cen));
                Vertex2 = cen + ori.Multiply(trans * orinv.Multiply(v2 - cen));
                Vertex3 = cen + ori.Multiply(trans * orinv.Multiply(v3 - cen));
                Vertex4 = cen + ori.Multiply(trans * orinv.Multiply(v4 - cen));
                ScaleCached = value;
            }

        }

        public override Vector3 Position
        {
            get
            {
                return (Vertex1 + Vertex2 + Vertex3 + Vertex4) * 0.25f;
            }
            set
            {
                var offset = value - Position;
                Vertex1 += offset;
                Vertex2 += offset;
                Vertex3 += offset;
                Vertex4 += offset;
            }
        }

        public override Quaternion Orientation
        {
            get
            {
                if (OrientationCached.HasValue) return OrientationCached.Value;
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var v4 = Vertex4;
                var dir = (v1 + v4) - (v2 + v3);
                var up = Vector3.Normalize((v3 + v4) - (v1 + v2));
                var ori = Quaternion.Inverse(QuaternionExt.LookAtRH(Vector3.Zero, dir, up));
                OrientationCached = ori;
                return ori;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var v4 = Vertex4;
                var cen = (v1 + v2 + v3 + v4) * 0.25f;
                var trans = value * Quaternion.Inverse(Orientation);
                Vertex1 = cen + trans.Multiply(v1 - cen);
                Vertex2 = cen + trans.Multiply(v2 - cen);
                Vertex3 = cen + trans.Multiply(v3 - cen);
                Vertex4 = cen + trans.Multiply(v4 - cen);
                OrientationCached = value;
            }
        }

        public override int[] VertexIndices
        {
            get => new int[] { BoxIndex1, BoxIndex2, BoxIndex3, BoxIndex4 };
            set
            {
                if (value?.Length >= 2)
                {
                    BoxIndex1 = (ushort)value[0];
                    BoxIndex2 = (ushort)value[1];
                    BoxIndex3 = (ushort)value[2];
                    BoxIndex4 = (ushort)value[3];
                }
            }
        }

        public Rsc6BoundPolygonBox()
        {
            Type = Rsc6BoundPolygonType.Box;
        }

        public override void Read(BinaryReader br)
        {
            BoxType = br.ReadUInt32();
            BoxIndex1 = br.ReadUInt16();
            BoxIndex2 = br.ReadUInt16();
            BoxIndex3 = br.ReadUInt16();
            BoxIndex4 = br.ReadUInt16();
            Unused0 = br.ReadUInt32();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(BoxType);
            bw.Write(BoxIndex1);
            bw.Write(BoxIndex2);
            bw.Write(BoxIndex3);
            bw.Write(BoxIndex4);
            bw.Write(Unused0);
        }

        public override string ToString()
        {
            return base.ToString() + ": " + BoxIndex1.ToString() + ", " + BoxIndex2.ToString() + ", " + BoxIndex3.ToString() + ", " + BoxIndex4.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6BoundPolygonCylinder : Rsc6BoundPolygon //phPrimCylinder
    {
        //A physics bound in the shape of a cylinder

        public byte PrimType { get; set; } //m_PrimType
        public byte Pad0 { get; set; } //m_Pad0
        public ushort EndIndex0 { get; set; } //m_EndIndex0
        public float Radius { get; set; } //m_Radius
        public ushort EndIndex1 { get; set; } //m_EndIndex1
        public ushort Pad1_1 { get; set; } //m_Pad1[6]
        public uint Pad1_2 { get; set; } //m_Pad1[6]

        private Quaternion? OrientationCached;
        private Vector3? ScaleCached;

        public Vector3 Vertex1
        {
            get { return (Owner != null) ? Owner.GetVertexPos(EndIndex0) : Vector3.Zero; }
            set { Owner?.SetVertexPos(EndIndex0, value); }
        }

        public Vector3 Vertex2
        {
            get { return (Owner != null) ? Owner.GetVertexPos(EndIndex1) : Vector3.Zero; }
            set { Owner?.SetVertexPos(EndIndex1, value); }
        }

        public override Vector3 BoxMin => Vector3.Min(Vertex1, Vertex2) - new Vector3(Radius); //not perfect but meh
        public override Vector3 BoxMax => Vector3.Max(Vertex1, Vertex2) + new Vector3(Radius); //not perfect but meh

        public override Vector3 Scale
        {
            get
            {
                if (ScaleCached.HasValue) return ScaleCached.Value;
                ScaleCached = Vector3.One;
                return Vector3.One;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var cen = (v1 + v2) * 0.5f;
                var trans = value / Scale;
                var ori = Orientation;
                var orinv = Quaternion.Inverse(ori);
                Vertex1 = cen + ori.Multiply(trans * orinv.Multiply(v1 - cen));
                Vertex2 = cen + ori.Multiply(trans * orinv.Multiply(v2 - cen));
                Radius = trans.X * Radius;
                ScaleCached = value;
            }

        }

        public override Vector3 Position
        {
            get
            {
                return (Vertex1 + Vertex2) * 0.5f;
            }
            set
            {
                var offset = value - Position;
                Vertex1 += offset;
                Vertex2 += offset;
            }
        }

        public override Quaternion Orientation
        {
            get
            {
                if (OrientationCached.HasValue) return OrientationCached.Value;
                var v1 = Vertex1;
                var v2 = Vertex2;
                var dir = v2 - v1;
                var up = Vector3.Normalize(dir.GetPerpVec());
                var ori = Quaternion.Inverse(QuaternionExt.LookAtRH(Vector3.Zero, dir, up));
                OrientationCached = ori;
                return ori;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var cen = (v1 + v2) * 0.5f;
                var trans = value * Quaternion.Inverse(Orientation);
                Vertex1 = cen + trans.Multiply(v1 - cen);
                Vertex2 = cen + trans.Multiply(v2 - cen);
                OrientationCached = value;
            }
        }

        public override int[] VertexIndices
        {
            get => new int[] { EndIndex0, EndIndex1 };
            set
            {
                if (value?.Length >= 3)
                {
                    EndIndex0 = (ushort)value[0];
                    EndIndex1 = (ushort)value[1];
                }
            }
        }

        public Rsc6BoundPolygonCylinder()
        {
            Type = Rsc6BoundPolygonType.Cylinder;
        }

        public override void Read(BinaryReader br)
        {
            PrimType = br.ReadByte();
            Pad0 = br.ReadByte();
            EndIndex0 = br.ReadUInt16();
            Radius = br.ReadSingle();
            EndIndex1 = br.ReadUInt16();
            Pad1_1 = br.ReadUInt16();
            Pad1_2 = br.ReadUInt32();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(PrimType);
            bw.Write(Pad0);
            bw.Write(EndIndex0);
            bw.Write(Radius);
            bw.Write(EndIndex1);
            bw.Write(Pad1_1);
            bw.Write(Pad1_2);
        }

        public override string ToString()
        {
            return base.ToString() + ": " + EndIndex0.ToString() + ", " + EndIndex1.ToString() + ", " + Radius.ToString();
        }
    }

    [TC(typeof(EXP))] public struct Rsc6BoundGeometryBVHNode
    {
        public short MinX { get; set; }
        public short MinY { get; set; }
        public short MinZ { get; set; }
        public short MaxX { get; set; }
        public short MaxY { get; set; }
        public short MaxZ { get; set; }
        public short ItemId { get; set; }
        public byte ItemCount { get; set; }
        public byte Padding1 { get; set; } //is this just ItemCount also?

        public Vector3 Min
        {
            get { return new Vector3(MinX, MinY, MinZ); }
            set { MinX = (short)value.X; MinY = (short)value.Y; MinZ = (short)value.Z; }
        }

        public Vector3 Max
        {
            get { return new Vector3(MaxX, MaxY, MaxZ); }
            set { MaxX = (short)value.X; MaxY = (short)value.Y; MaxZ = (short)value.Z; }
        }

        public override string ToString()
        {
            return ItemId.ToString() + ": " + ItemCount.ToString();
        }
    }

    [TC(typeof(EXP))] public struct Rsc6BoundGeometryBVHTree
    {
        public short MinX { get; set; }
        public short MinY { get; set; }
        public short MinZ { get; set; }
        public short MaxX { get; set; }
        public short MaxY { get; set; }
        public short MaxZ { get; set; }
        public short NodeIndex1 { get; set; } //fivem says they are ushorts
        public short NodeIndex2 { get; set; } //fivem says they are ushorts

        public Vector3 Min
        {
            get { return new Vector3(MinX, MinY, MinZ); }
            set { MinX = (short)value.X; MinY = (short)value.Y; MinZ = (short)value.Z; }
        }
        public Vector3 Max
        {
            get { return new Vector3(MaxX, MaxY, MaxZ); }
            set { MaxX = (short)value.X; MaxY = (short)value.Y; MaxZ = (short)value.Z; }
        }

        public override string ToString()
        {
            return NodeIndex1.ToString() + ", " + NodeIndex2.ToString() + "  (" + (NodeIndex2 - NodeIndex1).ToString() + " nodes)";
        }
    }

    [TC(typeof(EXP))] public struct Rsc6BoundMaterial : MetaNode
    {
        public uint Data { get; set; }

        public Rsc6BoundsMaterialType Type
        {
            readonly get => (Rsc6BoundsMaterialType)(this.Data & 0xFFu);
            set => this.Data = (this.Data & 0xFFFFFF00u) | ((byte)value & 0xFFu);
        }


        public Rsc6BoundMaterial(uint value)
        {
            this.Data = value;
        }

        public Rsc6BoundMaterial(MetaNodeReader reader, string prefix = "")
        {
            this.Read(reader);
        }

        public void Read(MetaNodeReader reader)
        {
            this.Type = new(reader.ReadByte("Type"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteByte("Index", this.Type.Index);
        }

        public override string ToString()
        {
            return this.Type.ToString();
        }
    }

    public enum Rsc6BoundPolygonType : byte //Hack
    {
        Cylinder = 205,
        Cylinder1 = 206,
        Box = 207,
        Triangle = 255
    }
}