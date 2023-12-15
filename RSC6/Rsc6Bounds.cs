using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Numerics;
using BepuUtilities;
using CodeX.Core.Physics;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.IO;
using SharpDX.Direct2D1;
using BepuPhysics.Collidables;

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
        public float SphereRadius { get; set; } //m_RadiusAroundCentroid
        public float WorldRadius { get; set; } //m_RadiusAroundLocalOrigin
        public Vector4 BoxMax { get; set; } //m_BoundingBoxMax
        public Vector4 BoxMin { get; set; } //m_BoundingBoxMin
        public Vector4 BoxCenter { get; set; } //m_CentroidOffset
        public Vector4 CentroidOffsetWorldSpace { get; set; } //m_CentroidOffsetWorldSpace
        public Vector4 SphereCenter { get; set; } //m_CGOffset
        public Vector4 VolumeDistribution { get; set; } //m_VolumeDistribution
        public Vector3 Margin { get; set; } //m_MarginV
        public uint RefCount { get; set; }

        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity; //when it's the child of a bound composite
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
            BoxMax = reader.ReadVector4(true);
            BoxMin = reader.ReadVector4(true);
            BoxCenter = reader.ReadVector4(true);
            CentroidOffsetWorldSpace = reader.ReadVector4(true);
            SphereCenter = reader.ReadVector4(true);
            VolumeDistribution = reader.ReadVector4(true);
            Margin = reader.ReadVector3();
            RefCount = reader.ReadUInt32();

            Name = "Bounds";
            ColliderType = GetEngineType(Type);
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }

        public static Rsc6Bounds Create(Rsc6DataReader r)
        {
            r.Position += 20;
            var type = (Rsc6BoundsType)r.ReadByte();
            r.Position -= 21;

            switch (type)
            {
                case Rsc6BoundsType.Sphere: return new Rsc6BoundSphere();
                case Rsc6BoundsType.Capsule: return new Rsc6BoundCapsule();
                case Rsc6BoundsType.Box: return new Rsc6BoundBox();
                case Rsc6BoundsType.Geometry: return new Rsc6BoundGeometry();
                case Rsc6BoundsType.GeometryBVH: return new Rsc6BoundGeometryBVH();
                case Rsc6BoundsType.Composite: return new Rsc6BoundComposite();
                case (Rsc6BoundsType)5: break; //in fragments
                default: throw new Exception("Unknown bounds type");
            }
            return new Rsc6Bounds();
        }

        public static ColliderType GetEngineType(Rsc6BoundsType t)
        {
            switch (t)
            {
                case Rsc6BoundsType.Sphere: return ColliderType.Sphere;
                case Rsc6BoundsType.Capsule: return ColliderType.Capsule;
                case Rsc6BoundsType.TaperedCapsule: return ColliderType.Capsule2;
                case Rsc6BoundsType.Box: return ColliderType.Box;
                case Rsc6BoundsType.Geometry: return ColliderType.Mesh;
                case Rsc6BoundsType.GeometryBVH: return ColliderType.Mesh;
                case Rsc6BoundsType.Composite: return ColliderType.None;
                case Rsc6BoundsType.Triangle: return ColliderType.Triangle;
                default: return ColliderType.None;
            }
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

            PartColour = Colour.White;
            PartSize = new Vector3(Radius.X, 0.0f, 0.0f);
            ComputeMass(ColliderType.Sphere, PartSize, 1.0f);
            ComputeBodyInertia();
        }
    }

    public class Rsc6BoundCapsule : Rsc6Bounds
    {
        public override ulong BlockLength => 236;
        public Vector4 Radius { get; set; } //m_CapsuleRadius
        public Vector4 Height { get; set; } //m_CapsuleLength
        public Vector4 Unknown5 { get; set; } //m_EndPointsWorldSpace[0]
        public Vector4 Unknown6 { get; set; } //m_EndPointsWorldSpace[1]
        public Vector4 Unknown7 { get; set; } //m_Axis
        public Rsc6BoundMaterial Material { get; set; } //m_MaterialId

        public Rsc6BoundCapsule() : base(Rsc6BoundsType.Capsule)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBound
            Radius = reader.ReadVector4(true);
            Height = reader.ReadVector4(true);
            Unknown5 = reader.ReadVector4(true);
            Unknown6 = reader.ReadVector4(true);
            Unknown7 = reader.ReadVector4(true);
            Material = reader.ReadStruct<Rsc6BoundMaterial>();

            PartColour = Colour.White;
            PartSize = new Vector3(Radius.X, Height.X, 0.0f);
            ComputeMass(ColliderType.Capsule, PartSize, 1.0f);
            ComputeBodyInertia();
        }
    }

    public class Rsc6BoundBox : Rsc6BoundPolyhedron
    {
        public override ulong BlockLength => base.BlockLength + 348;
        public Vector4 BoxSize { get; set; } //m_BoxSize
        public Vector4[] Unknown_F0h { get; set; } //Vec3V[8]
        public uint[] Unknown_170h { get; set; } //0xCDCDCDCD + 0x00000000 + 0x0000 0xFFFFFFFF + 0xFFFF -> 16 bytes * 12 -> unitialized data?
        public Rsc6BoundMaterial Material { get; set; }

        public Rsc6BoundBox(Rsc6BoundsType type = Rsc6BoundsType.Box) : base(type)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //phBoundPolyhedron
            BoxSize = reader.ReadVector4(true);
            Unknown_F0h = reader.ReadVector4Arr(8, true);
            Unknown_170h = reader.ReadUInt32Arr(48);
            Material = reader.ReadStruct<Rsc6BoundMaterial>();

            PartColour = Colour.White;
            PartSize = BoxMax.XYZ() - BoxMin.XYZ();
            ComputeMass(ColliderType.Box, PartSize, 1.0f);
            ComputeBodyInertia();
        }
    }

    public class Rsc6BoundGeometry : Rsc6BoundPolyhedron
    {
        public override ulong BlockLength => base.BlockLength + 12;
        public uint MaterialsPtr { get; set; } //m_MaterialIds
        public uint Unknown_22Ch { get; set; } //Always 0?
        public uint VertexMaterial { get; set; } //m_PolyMatIndexList
        public byte MaterialsCount { get; set; } //m_NumMaterials
        public byte Pad1 { get; set; }  //Always 0
        public byte Pad2 { get; set; } //Always 0
        public byte Pad3 { get; set; }  //Always 0

        public Vector3[] Vertices { get; set; }
        public Colour[] VertexColours { get; set; }
        public Vector3S[] VerticesData { get; set; }
        public Rsc6BoundPolygon[] Polygons { get; set; }
        public Rsc6BoundMaterial[] Materials { get; set; }

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

            VertexColours = reader.ReadArray<Colour>(VerticesCount, VertexColoursPtr);
            VerticesData = reader.ReadArray<Vector3S>(VerticesCount, VerticesPtr);
            PolygonsData = reader.ReadRawArrItems(PolygonsData, PolygonsCount * 16);
            Materials = reader.ReadArray<Rsc6BoundMaterial>(MaterialsCount, MaterialsPtr);

            if (VerticesData != null)
            {
                Vertices = new Vector3[VerticesData.Length];
                for (int i = 0; i < VerticesData.Length; i++)
                {
                    var bv = VerticesData[i];
                    bv = new Vector3S(bv.Z, bv.X, bv.Y);
                    Vertices[i] = (bv.Vector * Quantum.XYZ()) + CenterGeom.XYZ();
                }
            }

            ReadPolygons();
            CreateMesh();

            PartSize = BoxMax.XYZ() - BoxMin.XYZ();
            ComputeMass(ColliderType.Box, PartSize, 1.0f); //just an approximation to work with
            ComputeBasicBodyInertia(ColliderType.Box, PartSize); //just an approximation to work with
        }

        private void CreateMesh()
        {
            var children = new List<EditablePart>();
            var spheres = new List<ModelBatchInstance>();
            var capsules = new List<ModelBatchInstance>();
            var boxes = new List<ModelBatchInstance>();
            var cylinders = new List<ModelBatchInstance>();

            bool usevertexcolours = false;
            var verts = new List<ShapeVertex>();
            var indsl = new List<ushort>();

            int addVertex(int index, Vector3 norm)
            {
                var c = verts.Count;
                verts.Add(new ShapeVertex()
                {
                    Position = new Vector4(Vertices[index], 1),
                    Normal = norm,
                    Colour = (usevertexcolours && (VertexColours != null)) ? VertexColours[index] : Colour.White,
                    Texcoord = Vector2.Zero,
                    Tangent = Vector3.Zero
                });
                return c;
            }

            for (int i = 0; i < PolygonsCount; i++)
            {
                var p = Polygons[i];
                if (p is Rsc6BoundPolygonTriangle ptri)
                {
                    ptri.VertIndex1 = (ptri.VertIndex1 >= Vertices.Length) ? 0 : ptri.VertIndex1;
                    ptri.VertIndex2 = (ptri.VertIndex2 >= Vertices.Length) ? 0 : ptri.VertIndex2;
                    ptri.VertIndex3 = (ptri.VertIndex3 >= Vertices.Length) ? 0 : ptri.VertIndex3;
                    var v1 = ptri.Vertex1;
                    var v2 = ptri.Vertex2;
                    var v3 = ptri.Vertex3;
                    var e1 = Vector3.Normalize(v2 - v1);
                    var e2 = Vector3.Normalize(v3 - v1);
                    var nm = Vector3.Normalize(Vector3.Cross(e1, e2)); //ouch
                    var i1 = (ushort)addVertex(ptri.VertIndex1, nm);
                    var i2 = (ushort)addVertex(ptri.VertIndex2, nm);
                    var i3 = (ushort)addVertex(ptri.VertIndex3, nm);
                    indsl.Add(i1);
                    indsl.Add(i2);
                    indsl.Add(i3);
                }
            }

            if ((verts.Count > 0) && (indsl.Count > 0))
                PartMesh = Shape.Create("BoundGeometry", verts.ToArray(), indsl.ToArray());
            else
                ColliderType = ColliderType.None;

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
                var part = new Collider(shape) { PartSize = size };
                part.PartInstances = instances.ToArray();
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
            BoundingBox = PartMesh.BoundingBox;
        }

        private void ReadPolygons()
        {
            if (PolygonsCount == 0) return;

            var polygonData = PolygonsData.Items;
            using (var ms = new MemoryStream(polygonData))
            {
                var br = new BinaryReader(ms);
                Polygons = new Rsc6BoundPolygon[PolygonsCount];
                for (int i = 0; i < PolygonsCount; i++)
                {
                    var offset = i * 16;
                    ms.Position = offset;
                    byte b0 = polygonData[offset];
                    polygonData[offset] = (byte)(b0 & 0xF8); //mask it off
                    var type = (Rsc6BoundPolygonType)(b0 & 7);
                    var p = CreatePolygon(type);

                    if (p != null)
                    {
                        p.Index = i;
                        p.Read(br);
                    }
                    Polygons[i] = p;
                }
            }
        }

        private Rsc6BoundPolygon CreatePolygon(Rsc6BoundPolygonType type)
        {
            Rsc6BoundPolygon p = null;
            switch (type)
            {
                default:
                case Rsc6BoundPolygonType.Triangle:
                    p = new Rsc6BoundPolygonTriangle();
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

        public Colour GetVertexColour(int index)
        {
            var vc = VertexColours;
            return ((vc != null) && (index >= 0) && (index < vc.Length)) ? vc[index] : new Colour();
        }

        public void SetVertexColour(int index, Colour c)
        {
            var vc = VertexColours;
            if ((vc != null) && (index >= 0) && (index < vc.Length))
            {
                vc[index] = c;
            }
        }
    }

    public class Rsc6BoundPolyhedron : Rsc6Bounds
    {
        public override ulong BlockLength => 224;
        public uint VerticesPad { get; set; } //m_VerticesPad
        public uint VertexColoursPtr { get; set; } //m_CompressedShrunkVertices
        public uint VerticesWorldSpace { get; set; } //m_VerticesWorldSpace
        public Rsc6RawArr<byte> PolygonsData { get; set; } //m_Polygons
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
            Quantum = reader.ReadVector4(true);
            CenterGeom = reader.ReadVector4(true);
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
    }

    public class Rsc6BoundComposite : Rsc6Bounds
    {
        public override ulong BlockLength => 144;
        public uint ChildrenPtr { get; set; } //m_Bounds
        public uint ChildrenTransforms1Ptr { get; set; } //m_CurrentMatrices
        public uint ChildrenTransforms2Ptr { get; set; } //m_LastMatrices
        public uint ChildrenBoundingBoxesPtr { get; set; } //m_LocalBoxMinMaxs
        public uint ChildrenFlags { get; set; } //m_OwnedTypeAndIncludeFlags
        public Rsc6CustomArr<Rsc6BoundFlags> ChildrenFlags2 { get; set; } //m_Childs, same pointer as m_OwnedTypeAndIncludeFlags but with count/capacity
        public bool ContainsBVH { get; set; } //m_ContainsBVH
        public byte Pad { get; set; } //pad, always 0
        public ushort NumActiveBounds { get; set; } //m_NumActiveBounds

        public Rsc6Bounds[] Children { get; set; }
        public Matrix[] ChildrenTransforms1 { get; set; }
        public Matrix[] ChildrenTransforms2 { get; set; }
        public BoundingBox4[] ChildrenBoundingBoxes { get; set; }

        public Rsc6BoundComposite() : base(Rsc6BoundsType.Composite)
        {
        }

        public override void Read(Rsc6DataReader reader) //phBoundComposite
        {
            base.Read(reader); //phBounds
            ChildrenPtr = reader.ReadUInt32();
            ChildrenTransforms1Ptr = reader.ReadUInt32();
            ChildrenTransforms2Ptr = reader.ReadUInt32();
            ChildrenBoundingBoxesPtr = reader.ReadUInt32();
            ChildrenFlags = reader.ReadUInt32();
            ChildrenFlags2 = reader.ReadArr<Rsc6BoundFlags>();
            ContainsBVH = reader.ReadBoolean();
            Pad = reader.ReadByte();
            NumActiveBounds = reader.ReadUInt16();

            var childptrs = reader.ReadArray<uint>(ChildrenFlags2.Count, ChildrenPtr);
            ChildrenTransforms1 = reader.ReadArray<Matrix>(ChildrenFlags2.Count, ChildrenTransforms1Ptr);
            ChildrenTransforms2 = reader.ReadArray<Matrix>(ChildrenFlags2.Count, ChildrenTransforms2Ptr);
            ChildrenBoundingBoxes = reader.ReadArray<BoundingBox4>(ChildrenFlags2.Count, ChildrenBoundingBoxesPtr);

            if (childptrs != null)
            {
                var cc = Math.Min(ChildrenFlags2.Count, childptrs.Length);
                Children = new Rsc6Bounds[cc];
                for (int i = 0; i < cc; i++)
                {
                    Children[i] = reader.ReadBlock(childptrs[i], Create);
                    if (Children[i] != null)
                    {
                        Children[i].Name = "Child" + i.ToString();
                    }
                }
            }
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

    [TC(typeof(EXP))]
    public abstract class Rsc6BoundPolygon
    {
        public Rsc6BoundPolygonType Type { get; set; }
        public Rsc6BoundGeometry Owner { get; set; } //for browsing/editing convenience
        public Rsc6BoundMaterial? MaterialCustom; //for editing, when assigning a new material.
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
        public int Index { get; set; } //for editing convenience, not stored
        public abstract Vector3 BoxMin { get; }
        public abstract Vector3 BoxMax { get; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 Position { get; set; }
        public abstract Quaternion Orientation { get; set; }
        public abstract int[] VertexIndices { get; set; }
        public abstract void Read(BinaryReader br);
        public abstract void Write(BinaryWriter bw);

        public virtual string Title
        {
            get
            {
                return Type.ToString() + " " + Index.ToString();
            }
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6BoundPolygonTriangle : Rsc6BoundPolygon
    {
        public float Area { get; set; } //m_Area
        public ushort TriIndex1 { get; set; } //m_VertexIndices[3]
        public ushort TriIndex2 { get; set; }
        public ushort TriIndex3 { get; set; }
        public ushort EdgeIndex1 { get; set; } //m_NeighboringPolygons[3]
        public ushort EdgeIndex2 { get; set; }
        public ushort EdgeIndex3 { get; set; }

        public int VertIndex1 { get { return (TriIndex1 & 0x7FFF); } set { TriIndex1 = (ushort)((value & 0x7FFF) + (VertFlag1 ? 0x8000 : 0)); } }
        public int VertIndex2 { get { return (TriIndex2 & 0x7FFF); } set { TriIndex2 = (ushort)((value & 0x7FFF) + (VertFlag2 ? 0x8000 : 0)); } }
        public int VertIndex3 { get { return (TriIndex3 & 0x7FFF); } set { TriIndex3 = (ushort)((value & 0x7FFF) + (VertFlag3 ? 0x8000 : 0)); } }
        public bool VertFlag1 { get { return (TriIndex1 & 0x8000) > 0; } set { TriIndex1 = (ushort)(VertIndex1 + (value ? 0x8000 : 0)); } }
        public bool VertFlag2 { get { return (TriIndex2 & 0x8000) > 0; } set { TriIndex2 = (ushort)(VertIndex2 + (value ? 0x8000 : 0)); } }
        public bool VertFlag3 { get { return (TriIndex3 & 0x8000) > 0; } set { TriIndex3 = (ushort)(VertIndex3 + (value ? 0x8000 : 0)); } }

        public Vector3 Vertex1
        {
            get { return (Owner != null) ? Owner.GetVertexPos(VertIndex1) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(VertIndex1, value); }
        }
        public Vector3 Vertex2
        {
            get { return (Owner != null) ? Owner.GetVertexPos(VertIndex2) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(VertIndex2, value); }
        }
        public Vector3 Vertex3
        {
            get { return (Owner != null) ? Owner.GetVertexPos(VertIndex3) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(VertIndex3, value); }
        }

        public override Vector3 BoxMin
        {
            get
            {
                return Vector3.Min(Vector3.Min(Vertex1, Vertex2), Vertex3);
            }
        }
        public override Vector3 BoxMax
        {
            get
            {
                return Vector3.Max(Vector3.Max(Vertex1, Vertex2), Vertex3);
            }
        }
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

        private Quaternion? OrientationCached;
        private Vector3? ScaleCached;

        public override int[] VertexIndices
        {
            get
            {
                return new[] { VertIndex1, VertIndex2, VertIndex3 };
            }
            set
            {
                if (value?.Length >= 3)
                {
                    VertIndex1 = value[0];
                    VertIndex2 = value[1];
                    VertIndex3 = value[2];
                }
            }
        }

        public void SetEdgeIndex(int edgeid, ushort polyindex)
        {
            switch (edgeid)
            {
                case 1:
                    EdgeIndex1 = polyindex;
                    break;
                case 2:
                    EdgeIndex2 = polyindex;
                    break;
                case 3:
                    EdgeIndex3 = polyindex;
                    break;
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
            
        }

        public override string ToString()
        {
            return base.ToString() + ": " + TriIndex1.ToString() + ", " + TriIndex2.ToString() + ", " + TriIndex3.ToString();
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
            BoundingBoxMin = reader.ReadVector4(true);
            BoundingBoxMax = reader.ReadVector4(true);
            BoundingBoxCenter = reader.ReadVector4(true);
            BVHQuantumInverse = reader.ReadVector4(true);
            BVHQuantum = reader.ReadVector4(true);
            Trees = reader.ReadArr<Rsc6BoundGeometryBVHTree>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public struct Rsc6BoundMaterial
    {
        public uint MaterialID { get; set; }
        public ulong Flags { get; set; }

        public override string ToString()
        {
            return MaterialID.ToString() + ": " + Flags.ToString();
        }
    }

    public enum Rsc6BoundPolygonType : byte
    {
        Triangle = 0,
        Sphere = 1,
        Capsule = 2,
        Box = 3,
        Cylinder = 4,
    }
}
