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
using System.Diagnostics;

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
            Polygons = reader.ReadArray<Rsc6BoundBoxPolygon>(PolygonsCount, PolygonsPtr);

            CreateMesh();

            PartColour = Material.Type.Colour;
            PartSize = BoxSize.XYZ();
            ComputeMass(ColliderType.Box, PartSize, 1.0f);
            ComputeBodyInertia();
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

        private void CreateMesh()
        {
            bool usevertexcolours = false;
            var verts = new List<ShapeVertex>();
            var indsl = new List<ushort>();

            int addVertex(int index, Vector3 norm)
            {
                var c = verts.Count;
                verts.Add(new ShapeVertex()
                {
                    Position = new Vector4(Vertices[index].XYZ(), 1),
                    Normal = norm,
                    Colour = (usevertexcolours && (VertexColours != null)) ? VertexColours[index] : Material.Type.Colour,
                    Texcoord = Vector2.Zero,
                    Tangent = Vector3.Zero
                });
                return c;
            }

            for (int i = 0; i < PolygonsCount; i++)
            {
                var p = Polygons[i];
                var i1 = (ushort)addVertex(p.TriIndex1, Vector3.One);
                var i2 = (ushort)addVertex(p.TriIndex2, Vector3.One);
                var i3 = (ushort)addVertex(p.TriIndex3, Vector3.One);
                indsl.Add(i1);
                indsl.Add(i2);
                indsl.Add(i3);
            }

            PartMesh = Shape.Create("BoundGeometry", verts.ToArray(), indsl.ToArray());
            UpdateBounds();
        }
    }

    public class Rsc6BoundGeometry : Rsc6BoundPolyhedron
    {
        //Will perform collision-detection and test-probes on geometric bounds. It’s suited for small objects

        public override ulong BlockLength => base.BlockLength + 16;
        public uint MaterialsPtr { get; set; } //m_MaterialIds
        public uint Unknown_22Ch { get; set; } //Always 0?
        public uint VertexMaterial { get; set; } //m_PolyMatIndexList
        public byte MaterialsCount { get; set; } //m_NumMaterials
        public byte Pad1 { get; set; }  //Always 0
        public byte Pad2 { get; set; } //Always 0
        public byte Pad3 { get; set; }  //Always 0

        public Rsc6BoundGeometry(Rsc6BoundsType type = Rsc6BoundsType.Geometry) : base(type)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBoundPolyhedron
            MaterialsPtr = reader.ReadUInt32();
            Unknown_22Ch = reader.ReadUInt32();
            VertexMaterial = reader.ReadUInt32();
            MaterialsCount = reader.ReadByte();
            Pad1 = reader.ReadByte();
            Pad2 = reader.ReadByte();
            Pad3 = reader.ReadByte();
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Materials = reader.ReadNodeArray<Rsc6BoundMaterial>("Materials");
            VertexMaterial = reader.ReadUInt32("VertexMaterial");
            MaterialsCount = (byte)Materials.Length;
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            if (Materials != null) writer.WriteNodeArray("Materials", Materials);
            writer.WriteUInt32("VertexMaterial", VertexMaterial);
        }

        private void CreateMesh()
        {
            bool usevertexcolours = false;
            var verts = new List<ShapeVertex>();
            var indsl = new List<ushort>();

            int addVertex(int index, Vector3 norm, uint matind)
            {
                var matcol = Colour.White;
                if ((Materials != null) && (matind < Materials.Length))
                {
                    matcol = Materials[matind].Type.Colour;
                }

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
                var i1 = (ushort)addVertex(p.TriIndex1, Vector3.One, matind);
                var i2 = (ushort)addVertex(p.TriIndex2, Vector3.One, matind);
                var i3 = (ushort)addVertex(p.TriIndex3, Vector3.One, matind);
                indsl.Add(i1);
                indsl.Add(i2);
                indsl.Add(i3);
            }

            PartMesh = Shape.Create("BoundGeometry", verts.ToArray(), indsl.ToArray());
            UpdateBounds();
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

        public Rsc6BoundBoxPolygon Polygons { get; set; }
        public Vector4 CurvatureCenter { get; set; } //m_CurvatureCenter
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
            Polygons = reader.ReadStruct<Rsc6BoundBoxPolygon>();
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
            writer.WriteStruct(Polygons);
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
            Polygons = reader.ReadStruct<Rsc6BoundBoxPolygon>("Polygons");
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
        public uint VertexColoursPtr { get; set; } //m_CompressedShrunkVertices
        public uint VerticesWorldSpace { get; set; } //m_VerticesWorldSpace
        public uint PolygonsPtr { get; set; } //m_Polygons
        public Vector4 Quantum { get; set; } //m_UnQuantizeFactor
        public Vector4 CenterGeom { get; set; } //m_BoundingBoxCenter
        public uint VerticesPtr { get; set; } //m_CompressedVertices
        public uint SmallPolygonsWorldSpace { get; set; } //m_SmallPolygonsWorldSpace
        public bool UseActiveComponents { get; set; } //m_UseActiveComponents
        public bool IsFlat { get; set; } //m_IsFlat
        public short NumConvexHullVertices { get; set; } //m_NumConvexHullVertices
        public short NumActivePolygons { get; set; } //m_NumActivePolygons
        public short NumActiveVertices { get; set; } //m_NumActiveVertices
        public uint ActivePolygonIndices { get; set; } //m_ActivePolygonIndices
        public uint ActiveVertexIndices { get; set; } //m_ActiveVertexIndices
        public uint VerticesCount { get; set; } //m_NumVertices
        public uint PolygonsCount { get; set; } //m_NumPolygons

        public Vector3[] Vertices { get; set; }
        public Colour[] VertexColours { get; set; }
        public Vector3S[] VerticesData { get; set; }
        public Rsc6BoundBoxPolygon[] Polygons { get; set; }
        public Rsc6BoundMaterial[] Materials { get; set; }

        public Rsc6BoundPolyhedron(Rsc6BoundsType type)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBound
            VerticesPad = reader.ReadUInt32();
            VertexColoursPtr = reader.ReadUInt32();
            VerticesWorldSpace = reader.ReadUInt32();
            PolygonsPtr = reader.ReadUInt32();
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
            Polygons = reader.ReadArray<Rsc6BoundBoxPolygon>(PolygonsCount, PolygonsPtr);

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

        private void CreateMesh()
        {
            bool usevertexcolours = false;
            var verts = new List<ShapeVertex>();
            var indsl = new List<ushort>();

            int addVertex(int index, Vector3 norm)
            {
                var matcol = Colour.White;
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
                var i1 = (ushort)addVertex(p.TriIndex1, Vector3.One);
                var i2 = (ushort)addVertex(p.TriIndex2, Vector3.One);
                var i3 = (ushort)addVertex(p.TriIndex3, Vector3.One);
                indsl.Add(i1);
                indsl.Add(i2);
                indsl.Add(i3);
            }

            PartMesh = Shape.Create("BoundPolyhedron", verts.ToArray(), indsl.ToArray());
            UpdateBounds();
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

            Debug.Assert(false);
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

        public Rsc6Bounds[] Children { get; set; }

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
                Children = new Rsc6Bounds[numBounds];
                for (int i = 0; i < numBounds; i++)
                {
                    Children[i] = reader.ReadBlock(childPtrs[i], Create);
                    if (Children[i] != null)
                    {
                        Children[i].Name = "Child" + i.ToString();
                        if ((CurrentMatrices.Items != null) && (i < CurrentMatrices.Items.Length))
                        {
                            var m = new Matrix3x4(CurrentMatrices[i]);
                            m.Translation = new Vector3(m.Translation.Z, m.Translation.X, m.Translation.Y);
                            m.Orientation = new Quaternion(m.Orientation.Z, m.Orientation.X, m.Orientation.Y, m.Orientation.W);

                            if (Children[i] is Rsc6BoundCapsule)
                            {
                                var rotationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2);
                                m.Orientation *= rotationQuaternion;
                            }
                            Children[i].PartTransform = m;
                        }
                    }
                }
            }

            PartChildren = Children;
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

            Debug.Assert(false);
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

    public static class Rsc6BoundsMaterialTypes
    {
        public static List<Rsc6BoundsMaterialData> Materials;

        public static void Init(Rpf6FileManager fman)
        {
            Core.Engine.Console.Write("Rsc6BoundsMaterialTypes", "Initialising Bounds Material Types...");

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

    public class Rsc6BoundFlags : Rsc6BlockBase
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

    [TC(typeof(EXP))] public struct Rsc6BoundBoxPolygon //phPolygon
    {
        //Polygon class used by physics bounds, vertex and neighboring polygon index numbers, and methods for
        //polygon-polygon intersections and polygon-segment intersections

        public float Area { get; set; } //m_Area
        public ushort TriIndex1 { get; set; } //m_VertexIndices[3]
        public ushort TriIndex2 { get; set; }
        public ushort TriIndex3 { get; set; }
        public ushort EdgeIndex1 { get; set; } //m_NeighboringPolygons[3]
        public ushort EdgeIndex2 { get; set; }
        public ushort EdgeIndex3 { get; set; }

        public byte MaterialIndex { get { return (byte)(BitConverter.DoubleToInt64Bits(Area) & 0xFF); } }

        public override string ToString()
        {
            return "Material: " + MaterialIndex.ToString() + ", Area: " + Area.ToString();
        }
    }

    public struct Rsc6BoundGeometryBVHNode
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

    public struct Rsc6BoundGeometryBVHTree
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

    public struct Rsc6BoundMaterial : MetaNode
    {
        public uint Data { get; set; }

        public Rsc6BoundsMaterialType Type
        {
            readonly get => (Rsc6BoundsMaterialType)(this.Data & 0xFFu);
            set => this.Data = (this.Data & 0xFFFFFF00u) | ((byte)value & 0xFFu);
        }

        public byte ProceduralId
        {
            readonly get => (byte)((this.Data >> 8) & 0xFFu);
            set => this.Data = (this.Data & 0xFFFF00FFu) | ((value & 0xFFu) << 8);
        }

        public byte RoomId
        {
            readonly get => (byte)((this.Data >> 16) & 0x1Fu);
            set => this.Data = (this.Data & 0xFFE0FFFFu) | ((value & 0x1Fu) << 16);
        }

        public byte PedDensity
        {
            readonly get => (byte)((this.Data >> 21) & 0x7u);
            set => this.Data = (this.Data & 0xFF1FFFFFu) | ((value & 0x7u) << 21);
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
            this.ProceduralId = reader.ReadByte("ProcID");
            this.RoomId = reader.ReadByte("RoomID");
            this.PedDensity = reader.ReadByte("PedDens");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteByte("Index", this.Type.Index);
            if (this.ProceduralId != 0) writer.WriteByte("ProcID", this.ProceduralId);
            if (this.RoomId != 0) writer.WriteByte("RoomID", this.RoomId);
            if (this.PedDensity != 0) writer.WriteByte("PedDens", this.PedDensity);
        }

        public override string ToString()
        {
            return this.Type.ToString() + ", " + this.ProceduralId.ToString() + ", " + this.RoomId.ToString() + ", " + this.PedDensity.ToString();
        }
    }
}