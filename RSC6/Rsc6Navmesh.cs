using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6Navmesh : Navmesh, IRsc6Block
    {
        public virtual ulong BlockLength => 8;
        public virtual bool IsPhysical => false;
        public virtual ulong FilePosition { get; set; }
        public virtual uint VFT { get; set; } = 0x04A00394;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }

        public Vector4 AABBMin;
        public Vector4 AABBMax;
        public BoundingBox Extents;
        public Vector3[] CachedVertices;
        public ushort[] CachedIndices;
        public Rsc6NavmeshPolygon[] CachedPolygons;

        public virtual void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(VFT);
            writer.WritePtr(BlockMap);
        }

        public static Rsc6Navmesh Create(Rsc6DataReader r)
        {
            var type = (Rsc6NavmeshType)r.ReadUInt32();
            r.Position -= 4;
            return Create(type);
        }

        public static Rsc6Navmesh Create(Rsc6NavmeshType type)
        {
            return type switch
            {
                Rsc6NavmeshType.NavStreamingMesh => new Rsc6NavStreamingMesh(),
                Rsc6NavmeshType.NavMovableMesh => new Rsc6NavMovableMesh(),
                _ => throw new Exception("Unknown navmesh type")
            };
        }

        public Mesh InitNavmesh() //PartMesh = Mesh, or PartChildren with PartMesh
        {
            var vertexCount = CachedVertices?.Length ?? 0;
            var indexCount = CachedIndices?.Length ?? 0;
            var polyCount = CachedPolygons?.Length ?? 0;
            if ((vertexCount == 0) || (indexCount == 0) || (polyCount == 0)) return null;

            var tribuilder = new ShapeBuilder("Navmesh", false);
            var linbuilder = new ShapeBuilder("Navmesh", false, MeshTopology.LineList);

            var rand = new Random(unchecked((int)DateTime.Now.Ticks));
            for (int p = 0; p < polyCount; p++)
            {
                ref var poly = ref CachedPolygons[p];
                var pic = poly.NumLocalLinks;
                var pcol = new Colour(rand.Next(64, 256), rand.Next(64, 256), rand.Next(64, 256), 255);

                var pnorm = poly.ComputeNormal(this);
                if (pic < 1) continue;

                var p0 = poly.GetVertex(0, CachedVertices, CachedIndices);
                if (pic < 2) continue; //Only one point? TODO: handle this?

                if (pic == 2) //Only 2 points - it's a line
                {
                    var p1 = poly.GetVertex(1, CachedVertices, CachedIndices);
                    linbuilder.EnsureSpaceForPolygon(2);
                    linbuilder.AddVertex(p0, pnorm, pcol);
                    linbuilder.AddVertex(p1, pnorm, pcol);
                }
                else //At least one triangle
                {
                    var tricount = pic - 2; //Turn polys into triangles
                    tribuilder.EnsureSpaceForPolygon(tricount * 3);
                    tribuilder.AddPolygon(tricount);

                    for (int t = 0; t < tricount; t++)
                    {
                        var p1 = poly.GetVertex(t + 1, CachedVertices, CachedIndices);
                        var p2 = poly.GetVertex(t + 2, CachedVertices, CachedIndices);
                        tribuilder.AddVertex(p0, pnorm, pcol);
                        tribuilder.AddVertex(p1, pnorm, pcol);
                        tribuilder.AddVertex(p2, pnorm, pcol);
                    }
                }
            }

            tribuilder.EndBuild();
            linbuilder.EndBuild();

            var meshes = tribuilder.Shapes;
            meshes.AddRange(linbuilder.Shapes);
            if (meshes.Count == 0) return null;

            PartMesh = meshes[0];
            if (meshes.Count > 1)
            {
                var children = new List<EditablePart>();
                for (int i = 1; i < meshes.Count; i++)
                {
                    var part = new Navmesh
                    {
                        PartMesh = meshes[i]
                    };
                    part.UpdateBounds();
                    children.Add(part);
                }
                PartChildren = [.. children];
            }

            UpdateBounds();
            return PartMesh;
        }

        public Vector3[] GetVertexPositions(Rsc6RawArr<Rsc6NavmeshVertex> vertices)
        {
            var verts = vertices.Items;
            if (verts == null) return null;

            var aabbmin = AABBMin.XYZ();
            var aabbsize = Extents.Size;
            var arr = new Vector3[verts.Length];

            for (int i = 0; i < verts.Length; i++)
            {
                arr[i] = verts[i].ToVector3(aabbmin, aabbsize);
            }
            return arr;
        }
    }

    public class Rsc6NavMovableMesh : Rsc6Navmesh //rage::ai::navMeshMovableType
    {
        public override ulong BlockLength => 24;
        public override uint VFT { get; set; } = 0x04A5994C;
        public uint FlagContainsData { get; set; } //m_FlagContainsData
        public uint Unknown_Ch { get; set; } //Always 0?
        public Rsc6NavMeshChunkData MeshChunkData { get; set; } //m_MeshChunkData

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            FlagContainsData = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            AABBMax = reader.ReadVector4(); //m_BoundBoxMax
            AABBMin = reader.ReadVector4(); //m_BoundBoxMin
            MeshChunkData = reader.ReadBlock<Rsc6NavMeshChunkData>();

            Extents = new BoundingBox(AABBMin.XYZ(), AABBMax.XYZ());
            CachedVertices = GetVertexPositions(MeshChunkData.Vertices);
            CachedIndices = MeshChunkData.VertexIndices.Items;
            CachedPolygons = MeshChunkData.Polygons.Items;

            InitNavmesh();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    public class Rsc6NavStreamingMesh : Rsc6Navmesh //rage::ai::navStreamingMeshDataManager::navMeshRscFile
    {
        public override ulong BlockLength => 24;
        public override uint VFT { get; set; } = 0x04A00394;
        public Rsc6ManagedArr<Rsc6NavMeshChunkDataPair> ChunkDataPairs { get; set; } //m_ChunkDataPairs
        public Rsc6ManagedArr<Rsc6NavMeshSpanDataPair> SpanDataPairs { get; set; } //m_SpanDataPairs

        public Vector3 ComputedSize { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ChunkDataPairs = reader.ReadArr<Rsc6NavMeshChunkDataPair>();
            SpanDataPairs = reader.ReadArr<Rsc6NavMeshSpanDataPair>();

            var meshes = new List<Mesh>();
            if (ChunkDataPairs.Items != null)
            {
                for (int i = 0; i < ChunkDataPairs.Items.Length; i++)
                {
                    var pair = ChunkDataPairs.Items[i];
                    var chunk = pair.ChunkData;
                    var quadtree = chunk.QuadTree.Item;
                    if (quadtree == null) continue;

                    var aabb = quadtree.AABB; //minX, minZ, maxX, maxZ
                    if (aabb == Vector4.Zero) continue;

                    //XZ extents from AABB
                    float minX = aabb.X;
                    float maxX = aabb.Z;
                    float minZ = aabb.Y;
                    float maxZ = aabb.W;

                    float sizeX = maxX - minX;
                    float sizeZ = maxZ - minZ;

                    var verts = chunk.Vertices.Items;
                    if (verts == null || verts.Length == 0) continue;

                    //The quadtree AABB gives us XZ range, so we derive Y from vertices.
                    //Start with a temporary AABBMin/Size that matches the quadtree’s XZ span.
                    var tempAabbMin = new Vector3(minX, 0.0f, minZ);
                    var tempAabbSize = new Vector3(sizeX, 1.0f, sizeZ);

                    Debug.WriteLine($"Chunk {i}:");
                    Debug.WriteLine($"  AABB (XZ): min=({minX:F2}, {minZ:F2}) max=({maxX:F2}, {maxZ:F2}) size=({sizeX:F2}, {sizeZ:F2})");
                    Debug.WriteLine($"  Raw ushort ranges: X [{verts.Min(v => v.X)}, {verts.Max(v => v.X)}], " +
                                      $"Y [{verts.Min(v => v.Y)}, {verts.Max(v => v.Y)}], Z [{verts.Min(v => v.Z)}, {verts.Max(v => v.Z)}]");

                    // Decode a few sample vertices to inspect scaling
                    for (int s = 0; s < Math.Min(5, verts.Length); s++)
                    {
                        var p = verts[s].ToVector3(tempAabbMin, tempAabbSize);
                        Debug.WriteLine($"    v{s}: {p}");
                    }

                    float minY = float.MaxValue;
                    float maxY = float.MinValue;

                    foreach (var vert in verts)
                    {
                        var pos = vert.ToVector3(tempAabbMin, tempAabbSize);
                        if (pos.Y < minY) minY = pos.Y;
                        if (pos.Y > maxY) maxY = pos.Y;
                    }

                    //Store final values
                    var aabbMin = new Vector3(minX, minY, minZ);
                    var aabbMax = new Vector3(maxX, maxY, maxZ);
                    AABBMin = new Vector4(aabbMin, 0.0f);
                    AABBMax = new Vector4(aabbMax, 0.0f);
                    Extents = new BoundingBox(aabbMin, aabbMax);
                    ComputedSize = aabbMax - aabbMin;

                    //Use values in downstream
                    CachedVertices = GetVertexPositions(chunk.Vertices);
                    CachedIndices = chunk.VertexIndices.Items;
                    CachedPolygons = chunk.Polygons.Items;

                    var mesh = InitNavmesh();
                    if (mesh != null)
                    {
                        meshes.Add(mesh);
                    }
                }
            }

            if (meshes.Count > 0)
            {
                var childrens = new List<EditablePart>();
                foreach (var mesh in meshes)
                {
                    var part = new Navmesh { PartMesh = mesh };
                    part.UpdateBounds();
                    childrens.Add(part);
                }

                PartChildren = [.. childrens];
                UpdateBounds();
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            throw new NotImplementedException();
        }
    }

    public class Rsc6NavMeshChunkDataPair : Rsc6BlockBase //rage::ai::navStreamingMeshDataManager::navMeshChunkDataPair
    {
        public override ulong BlockLength => 48;
        public int Index { get; set; } //index
        public Rsc6NavMeshChunkData ChunkData { get; set; } //data

        public override void Read(Rsc6DataReader reader)
        {
            Index = reader.ReadInt32();
            ChunkData = reader.ReadBlock<Rsc6NavMeshChunkData>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6NavMeshSpanDataPair : Rsc6BlockBase //rage::ai::navStreamingMeshDataManager::navMeshSpanDataPair
    {
        public override ulong BlockLength => 48;
        public int Index { get; set; } //index
        public Rsc6NavMeshSpanData ChunkData { get; set; } //data

        public override void Read(Rsc6DataReader reader)
        {
            Index = reader.ReadInt32();
            ChunkData = reader.ReadBlock<Rsc6NavMeshSpanData>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6NavMeshSpanData : Rsc6FileBase //rage::ai::navMeshSpanData
    {
        public override ulong BlockLength => 12;
        public override uint VFT { get; set; } = 0x04A589AC;
        public Rsc6RawArr<Rsc6NavmeshExternalPolygonLink> Links { get; set; } //m_Links
        public uint NumLinks { get; set; } //m_NumLinks

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Links = reader.ReadRawArrPtr<Rsc6NavmeshExternalPolygonLink>();
            NumLinks = reader.ReadUInt32();
            Links = reader.ReadRawArrItems(Links, NumLinks);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6NavMeshChunkData : Rsc6FileBase //rage::ai::navMeshChunkData
    {
        public override ulong BlockLength => 44;
        public override uint VFT { get; set; } = 0x04A588C0;
        public Rsc6Ptr<Rsc6NavmeshQuadtree> QuadTree { get; set; } //m_Quadtree
        public Rsc6RawArr<Rsc6NavmeshLocalPolygonLink> LocalLinks { get; set; } //m_LocalLinks
        public Rsc6PtrUnmanaged<Vector3> Normals { get; set; } //m_Normals
        public Rsc6RawArr<Rsc6NavmeshPolygon> Polygons { get; set; } //m_Polygons
        public Rsc6RawArr<Rsc6NavmeshVertex> Vertices { get; set; } //m_Vertices
        public Rsc6Ptr<Rsc6BlockMap> LocalLinkIndices { get; set; } //m_LocalLinkIndicesForAllPolygons
        public Rsc6RawArr<ushort> VertexIndices { get; set; } //m_VertexIndicesForAllPolygons
        public ushort LocalLinksNum { get; set; } //m_LocalLinksNum
        public ushort NormalsNum { get; set; } //m_NormalsNum
        public ushort PolygonsNum { get; set; } //m_PolygonsNum
        public ushort VerticesNum { get; set; } //m_VerticesNum
        public ushort NumLocalLinkIndices { get; set; } //m_NumLocalLinkIndicesForAllPolygons
        public ushort NumVertexIndices { get; set; } //m_NumVertexIndicesForAllPolygons

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            QuadTree = reader.ReadPtr<Rsc6NavmeshQuadtree>();
            LocalLinks = reader.ReadRawArrPtr<Rsc6NavmeshLocalPolygonLink>();
            Normals = reader.ReadPtrUnmanaged<Vector3>();
            Polygons = reader.ReadRawArrPtr<Rsc6NavmeshPolygon>();
            Vertices = reader.ReadRawArrPtr<Rsc6NavmeshVertex>();
            LocalLinkIndices = reader.ReadPtr<Rsc6BlockMap>();
            VertexIndices = reader.ReadRawArrPtr<ushort>();
            LocalLinksNum = reader.ReadUInt16();
            NormalsNum = reader.ReadUInt16();
            PolygonsNum = reader.ReadUInt16();
            VerticesNum = reader.ReadUInt16();
            NumLocalLinkIndices = reader.ReadUInt16();
            NumVertexIndices = reader.ReadUInt16();

            LocalLinks = reader.ReadRawArrItems(LocalLinks, LocalLinksNum);
            Polygons = reader.ReadRawArrItems(Polygons, PolygonsNum);
            Vertices = reader.ReadRawArrItems(Vertices, VerticesNum);
            VertexIndices = reader.ReadRawArrItems(VertexIndices, NumVertexIndices);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6NavmeshQuadtree : Rsc6FileBase //rage::ai::navQuadtree
    {
        public override ulong BlockLength => 52;
        public override uint VFT { get; set; } = 0x04A58A90;
        public Vector4 AABB { get; set; } //(minX, minZ, maxX, maxZ)
        public Rsc6NavmeshQuadtreeCell Root { get; set; } //m_Root

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            AABB = reader.ReadVector4(false);
            Root = reader.ReadBlock<Rsc6NavmeshQuadtreeCell>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6NavmeshQuadtreeCell : Rsc6BlockBase //rage::ai::navQuadtreeCell
    {
        public override ulong BlockLength => 32;
        public Rsc6Ptr<Rsc6NavmeshQuadtreeCell> Children1 { get; set; } //m_Children[0]
        public Rsc6Ptr<Rsc6NavmeshQuadtreeCell> Children2 { get; set; } //m_Children[1]
        public Rsc6Ptr<Rsc6NavmeshQuadtreeCell> Children3 { get; set; } //m_Children[2]
        public Rsc6Ptr<Rsc6NavmeshQuadtreeCell> Children4 { get; set; } //m_Children[3]
        public uint Objects { get; set; } //m_Objects, always 0?
        public uint NumObjects { get; set; } //m_NumObjects, always 0?
        public float SplitX { get; set; } //m_SplitX
        public float SplitZ { get; set; } //m_SplitZ

        public override void Read(Rsc6DataReader reader)
        {
            Children1 = reader.ReadPtr<Rsc6NavmeshQuadtreeCell>();
            Children2 = reader.ReadPtr<Rsc6NavmeshQuadtreeCell>();
            Children3 = reader.ReadPtr<Rsc6NavmeshQuadtreeCell>();
            Children4 = reader.ReadPtr<Rsc6NavmeshQuadtreeCell>();
            Objects = reader.ReadUInt32();
            NumObjects = reader.ReadUInt32();
            SplitX = reader.ReadSingle();
            SplitZ = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))]
    public struct Rsc6NavmeshPolygon //rage::ai::navMeshPolygon
    {
        public Rsc6NavmeshVertex LocalCenter { get; set; } //m_LocalCenter
        public byte MaterialIndex { get; set; } //m_MaterialIndex
        public bool HasExternalConnections { get; set; } //m_HasExternalConnections
        public float Area { get; set; } //m_Area
        public float PlaneD { get; set; } //m_PlaneD
        public ushort LocalLinkIndices { get; set; } //m_LocalLinkIndices
        public ushort VertexIndices { get; set; } //m_VertexIndices
        public byte NormalIndex { get; set; } //m_NormalIndex
        public byte NumLocalLinks { get; set; } //m_NumLocalLinks
        public byte NumVertices { get; set; } //m_NumVertices
        public byte Unknown_17h { get; set; } //Always 0?

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 GetVertex(int polyVertexIndex, Vector3[] nmverts, ushort[] nminds)
        {
            if (polyVertexIndex < 0) return default;
            var i = VertexIndices + polyVertexIndex;
            if (i >= nminds.Length) return default;
            var ind = nminds[i];
            if (ind >= nmverts.Length) return default;
            return nmverts[ind];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 ComputeNormal(Rsc6Navmesh nm)
        {
            var nminds = nm.CachedIndices;
            var nmverts = nm.CachedVertices;
            var IndexCount = 1;

            if (IndexCount < 3) return Vector3.UnitZ; //can only do this with at least 3 verts
            var v0 = GetVertex(0, nmverts, nminds);
            var v1 = GetVertex(1, nmverts, nminds);
            var v2 = GetVertex(2, nmverts, nminds);

            for (int i = 3; i < IndexCount; i++) //use the average point of all the remaining vertices
            {
                v2 += GetVertex(i, nmverts, nminds);
            }

            if (IndexCount > 3)
            {
                v2 *= 1.0f / (IndexCount - 2);
            }

            if ((v0 == v1) || (v0 == v2) || (v1 == v2)) return Vector3.UnitZ; //degenerate..
            var d1 = Vector3.Normalize(v1 - v0);
            var d2 = Vector3.Normalize(v2 - v0);
            return Vector3.Normalize(Vector3.Cross(d1, d2));
        }
    }

    [TC(typeof(EXP))]
    public struct Rsc6NavmeshExternalPolygonLink //rage::ai::navMeshExternalPolygonLink
    {
        public byte ActorRadiusDist1 { get; set; } //m_ActorRadiusDist1
        public byte ActorRadiusDist2 { get; set; } //m_ActorRadiusDist2
        public ushort StoredDistance { get; set; } //m_Distance
        public ushort PolygonInChunkA { get; set; } //m_PolygonInChunkA
        public ushort PolygonInChunkB { get; set; } //m_PolygonInChunkB
        public ushort VertexLeft { get; set; } //m_VertexLeft
        public ushort VertexRight { get; set; } //m_VertexRight
        public ushort Span { get; set; } //m_Span

        public float Distance
        {
            readonly get => ToFloat();
            set
            {
                FromFloat(value);
            }
        }

        public readonly float ToFloat()
        {
            return StoredDistance / ushort.MaxValue;
        }

        public void FromFloat(float f)
        {
            StoredDistance = (ushort)Math.Round(f * ushort.MaxValue);
        }

        public override readonly string ToString()
        {
            return Distance.ToString();
        }
    }

    [TC(typeof(EXP))]
    public struct Rsc6NavmeshVertex //rage::ai::aiStoredVector3
    {
        public ushort X { get; set; } //aiStoredFloat
        public ushort Y { get; set; } //aiStoredFloat
        public ushort Z { get; set; } //aiStoredFloat

        public Vector3 Position
        {
            readonly get => ToVector3();
            set
            {
                FromVector3(value);
            }
        }

        public readonly Vector3 ToVector3()
        {
            const float xz_scale = ushort.MaxValue;
            const float y_scale = 512.0f;
            return new Vector3(Z / xz_scale, X / xz_scale, Y / y_scale);
        }

        public readonly Vector3 ToVector3(in Vector3 aabbmin, in Vector3 aabbsize)
        {
            return aabbmin + (aabbsize * ToVector3());
        }

        public void FromVector3(Vector3 v)
        {
            const float usmax = ushort.MaxValue;
            X = (ushort)Math.Round(v.X * usmax);
            Y = (ushort)Math.Round(v.Y * usmax);
            Z = (ushort)Math.Round(v.Z * usmax);
        }

        public override readonly string ToString()
        {
            return Position.ToString();
        }
    }

    [TC(typeof(EXP))]
    public struct Rsc6NavmeshLocalPolygonLink //rage::ai::navMeshLocalPolygonLink
    {
        public byte ActorRadiusDist1 { get; set; } //m_ActorRadiusDist1
        public byte ActorRadiusDist2 { get; set; } //m_ActorRadiusDist2
        public ushort PackedA { get; set; } //m_PackedA
        public ushort PackedB { get; set; } //m_PackedB
        public ushort PackedC { get; set; } //m_PackedC
        public ushort PackedD { get; set; } //m_PackedD
    }

    public enum Rsc6NavmeshType : uint
    {
        NavMovableMesh = 0x04A5994C, //rage::ai::navMeshMovableType (vehicles)
        NavStreamingMesh = 0x04A00394 //rage::ai::navStreamingMeshDataManager::navMeshRscFile (terrain)
    }
}