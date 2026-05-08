using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
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
    [TC(typeof(EXP))]
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
            var type = (Rsc6NavmeshDataType)r.ReadUInt32();
            r.Position -= 4;
            return Create(type);
        }

        public static Rsc6Navmesh Create(Rsc6NavmeshDataType type)
        {
            return type switch
            {
                Rsc6NavmeshDataType.NavStreamingMesh => new Rsc6NavStreamingMesh(),
                Rsc6NavmeshDataType.NavMovableMesh => new Rsc6NavMovableMesh(),
                Rsc6NavmeshDataType.NavWorldChunkHierarchy => new Rsc6NavHierarchySAGWorldChunk(),
                _ => null
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

            for (int p = 0; p < polyCount; p++)
            {
                ref var poly = ref CachedPolygons[p];
                var pic = poly.NumVertices;
                var matIndex = poly.MaterialIndex;
                _ = Rsc6NavmeshLookup.TryGetMaterial(matIndex, out var mat);
                var pcol = Rsc6NavmeshLookup.GetNavMaterialColour(mat);
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

        public static Vector3[] GetVertexPositions(Rsc6RawArr<Rsc6NavmeshVertex> vertices, Vector3 origin)
        {
            var verts = vertices.Items;
            if (verts == null) return null;

            var arr = new Vector3[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                var v = verts[i].ToVector3();
                arr[i] = origin + v;
            }
            return arr;
        }
    }

    [TC(typeof(EXP))]
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

            if (FlagContainsData == 0 || MeshChunkData == null) return;

            Extents = new BoundingBox(AABBMin.XYZ(), AABBMax.XYZ());
            CachedVertices = GetVertexPositions(MeshChunkData.Vertices, Vector3.Zero);
            CachedIndices = MeshChunkData.VertexIndices.Items;
            CachedPolygons = MeshChunkData.Polygons.Items;

            InitNavmesh();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavStreamingMesh : Rsc6Navmesh //rage::ai::navStreamingMeshDataManager::navMeshRscFile
    {
        public override ulong BlockLength => 24;
        public override uint VFT { get; set; } = 0x04A00394;
        public Rsc6ManagedArr<Rsc6NavMeshChunkDataPair> ChunkDataPairs { get; set; } //m_ChunkDataPairs
        public Rsc6ManagedArr<Rsc6NavMeshSpanDataPair> SpanDataPairs { get; set; } //m_SpanDataPairs

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ChunkDataPairs = reader.ReadArr<Rsc6NavMeshChunkDataPair>();
            SpanDataPairs = reader.ReadArr<Rsc6NavMeshSpanDataPair>();

            var pairs = ChunkDataPairs.Items;
            if (pairs == null || pairs.Length == 0) return;

            var validPairs = pairs.Where(
                p => p?.ChunkData?.Vertices.Items?.Length > 0 &&
                p.ChunkData.VertexIndices.Items?.Length > 0 &&
                p.ChunkData.Polygons.Items?.Length > 0).OrderBy(p => p.Index).ToArray();

            if (validPairs.Length == 0) return;
            var children = new List<EditablePart>();

            for (int i = 0; i < validPairs.Length; i++)
            {
                var pair = validPairs[i];
                var chunkData = pair.ChunkData;

                if (!Rsc6NavmeshLookup.TryGetChunk(pair.Index, out var chunk)) continue;

                var origin = chunk.Offset.XYZ();
                var qt = chunkData.QuadTree.Item;
                var aabb = qt?.AABB ?? new Vector4(-32.0f, -32.0f, 32.0f, 32.0f);
                var boundsMin = origin + new Vector3(aabb.X, aabb.Y, 0.0f);
                var boundsMax = origin + new Vector3(aabb.Z, aabb.W, 0.0f);

                AABBMin = new Vector4(boundsMin, 0.0f);
                AABBMax = new Vector4(boundsMax, 0.0f);
                Extents = new BoundingBox(boundsMin, boundsMax);
                CachedVertices = GetVertexPositions(chunkData.Vertices, origin);
                CachedIndices = chunkData.VertexIndices.Items;
                CachedPolygons = chunkData.Polygons.Items;
                PartMesh = null;
                PartChildren = null;

                var mesh = InitNavmesh();
                if (mesh == null) continue;

                var part = new Navmesh { PartMesh = mesh };
                part.UpdateBounds();
                children.Add(part);
            }

            PartMesh = null;
            PartChildren = children.Count > 0 ? [.. children] : null;
            UpdateBounds();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))]
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

    [TC(typeof(EXP))]
    public class Rsc6NavMeshSpanDataPair : Rsc6BlockBase //rage::ai::navStreamingMeshDataManager::navMeshSpanDataPair
    {
        public override ulong BlockLength => 16;
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

    [TC(typeof(EXP))]
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

    [TC(typeof(EXP))]
    public class Rsc6NavMeshChunkData : Rsc6FileBase //rage::ai::navMeshChunkData
    {
        public override ulong BlockLength => 44;
        public override uint VFT { get; set; } = 0x04A588C0;
        public Rsc6Ptr<Rsc6NavmeshQuadtree> QuadTree { get; set; } //m_Quadtree
        public Rsc6RawArr<Rsc6NavmeshLocalPolygonLink> LocalLinks { get; set; } //m_LocalLinks
        public Rsc6RawArr<Vector4> Normals { get; set; } //m_Normals
        public Rsc6RawArr<Rsc6NavmeshPolygon> Polygons { get; set; } //m_Polygons
        public Rsc6RawArr<Rsc6NavmeshVertex> Vertices { get; set; } //m_Vertices
        public Rsc6RawArr<ushort> LocalLinkIndices { get; set; } //m_LocalLinkIndicesForAllPolygons
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
            Normals = reader.ReadRawArrPtr<Vector4>();
            Polygons = reader.ReadRawArrPtr<Rsc6NavmeshPolygon>();
            Vertices = reader.ReadRawArrPtr<Rsc6NavmeshVertex>();
            LocalLinkIndices = reader.ReadRawArrPtr<ushort>();
            VertexIndices = reader.ReadRawArrPtr<ushort>();
            LocalLinksNum = reader.ReadUInt16();
            NormalsNum = reader.ReadUInt16();
            PolygonsNum = reader.ReadUInt16();
            VerticesNum = reader.ReadUInt16();
            NumLocalLinkIndices = reader.ReadUInt16();
            NumVertexIndices = reader.ReadUInt16();

            LocalLinks = reader.ReadRawArrItems(LocalLinks, LocalLinksNum);
            Normals = reader.ReadRawArrItems(Normals, NormalsNum);
            Polygons = reader.ReadRawArrItems(Polygons, PolygonsNum);
            Vertices = reader.ReadRawArrItems(Vertices, VerticesNum);
            LocalLinkIndices = reader.ReadRawArrItems(LocalLinkIndices, NumLocalLinkIndices);
            VertexIndices = reader.ReadRawArrItems(VertexIndices, NumVertexIndices);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavmeshQuadtree : Rsc6FileBase //rage::ai::navQuadtree
    {
        public override ulong BlockLength => 52;
        public override uint VFT { get; set; } = 0x04A58A90;
        public Vector4 AABB { get; set; } //(minX, minZ, maxX, maxZ)
        public Rsc6NavmeshQuadtreeCell Root { get; set; } //m_Root

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            AABB = reader.ReadVector4();
            Root = reader.ReadBlock<Rsc6NavmeshQuadtreeCell>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))]
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
    public class Rsc6NavHierarchySAGWorldChunk : Rsc6Navmesh, IRsc6Block //rage::ai::navHierarchyGridWorldRscChunk
    {
        public override ulong BlockLength => 28;
        public override uint VFT { get; set; } = 0x04A00714;
        public Rsc6Ptr<Rsc6NavSearchHierarchyGrid> HierarchyGrid { get; set; } //m_Hierarchy
        public Rsc6Ptr<Rsc6NavMaterialManager> MaterialManager { get; set; } //m_MaterialManager
        public uint Unknown_10h { get; set; } //?
        public int NumExtraChunkData { get; set; } //m_NumExtraChunkData
        public int NumExtraChunks { get; set; } //m_NumExtraChunks

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            HierarchyGrid = reader.ReadPtr<Rsc6NavSearchHierarchyGrid>();
            MaterialManager = reader.ReadPtr<Rsc6NavMaterialManager>();
            Unknown_10h = reader.ReadUInt32();
            NumExtraChunkData = reader.ReadInt32();
            NumExtraChunks = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavMaterialManager : Rsc6FileBase //rage::ai::navMaterialManager
    {
        public override ulong BlockLength => 16;
        public override uint VFT { get; set; } = 0x04ADD99C;
        public Rsc6PtrArr<Rsc6NavMaterial> Materials { get; set; }
        public Rsc6Ptr<Rsc6NavMaterial> Unknown_Ch { get; set; } //Default material?

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Materials = reader.ReadPtrArr<Rsc6NavMaterial>();
            Unknown_Ch = reader.ReadPtr<Rsc6NavMaterial>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavMaterial : Rsc6FileBase //rage::ai::navMaterial
    {
        public override ulong BlockLength => 16;
        public override uint VFT { get; set; } = 0x04ADD9FC;
        public uint TypeFlags { get; set; } // m_TypeFlags
        public Rsc6NavMaterialFlags Flags { get; set; } // m_Flags
        public uint LiquidDepthLevel { get; set; } // m_LiquidDepthLevel

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            TypeFlags = reader.ReadUInt32();
            Flags = (Rsc6NavMaterialFlags)reader.ReadUInt32();
            LiquidDepthLevel = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(TypeFlags);
            writer.WriteUInt32((uint)Flags);
            writer.WriteUInt32(LiquidDepthLevel);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavSearchHierarchyGrid : Rsc6BlockBaseMapRef //rage::ai::navSearchHierarchyGridWorld
    {
        public override ulong BlockLength => 60;
        public override uint VFT { get; set; } = 0x04A5A740;
        public uint Unknown_Ch { get; set; }
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public Rsc6NavSpaceHierarchy SpaceHierarchy { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_Ch = reader.ReadUInt32();
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            SpaceHierarchy = reader.ReadBlock<Rsc6NavSpaceHierarchy>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavSpaceHierarchy : Rsc6BlockBase //rage::ai::navSpaceHierarchy
    {
        public override ulong BlockLength => 20;
        public Rsc6Ptr<Rsc6BlockMap> SubSpace1 { get; set; } //m_SubSpaces[]
        public Rsc6Ptr<Rsc6NavSpaceMesh> SubSpace2 { get; set; } //m_SubSpaces[]
        public Rsc6Ptr<Rsc6BlockMap> SubSpace3 { get; set; } //m_SubSpaces[]
        public Rsc6Ptr<Rsc6BlockMap> SubSpace4 { get; set; } //m_SubSpaces[]
        public int NumLevels { get; set; } //m_NumLevels

        public override void Read(Rsc6DataReader reader)
        {
            SubSpace1 = reader.ReadPtr<Rsc6BlockMap>();
            SubSpace2 = reader.ReadPtr<Rsc6NavSpaceMesh>();
            SubSpace3 = reader.ReadPtr<Rsc6BlockMap>();
            SubSpace4 = reader.ReadPtr<Rsc6BlockMap>();
            NumLevels = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavSpaceMesh : Rsc6BlockBaseMapRef //rage::ai::navSpaceMesh
    {
        public override ulong BlockLength => 60;
        public override uint VFT { get; set; } = 0x04A5FAD0;
        public uint Unknown_Ch { get; set; }
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public Rsc6Ptr<Rsc6NavIndexedChunkData> ChunkDataContainer { get; set; } //m_ChunkDataContainer
        public uint SpanDataContainer { get; set; } //m_SpanDataContainer
        public Rsc6PtrArr<Rsc6NavMeshChunk> Chunks { get; set; } //m_Chunks
        public Rsc6ManagedArr<Rsc6NavMeshSpan> Spans { get; set; } //m_Spans
        public int NumProxyChunks { get; set; } //m_NumProxyChunks
        public int NumProxySpans { get; set; } //m_NumProxySpans
        public bool FlagStreamingEnabled { get; set; } //m_FlagStreamingEnabled
        public float HeightTolDown { get; set; } //m_HeightTolDown
        public float HeightTolUp { get; set; } //m_HeightTolUp

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_Ch = reader.ReadUInt32();
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            ChunkDataContainer = reader.ReadPtr<Rsc6NavIndexedChunkData>();
            SpanDataContainer = reader.ReadUInt32();
            Chunks = reader.ReadPtrArr<Rsc6NavMeshChunk>();
            Spans = reader.ReadArr<Rsc6NavMeshSpan>();
            NumProxyChunks = reader.ReadInt32();
            NumProxySpans = reader.ReadInt32();
            FlagStreamingEnabled = reader.ReadBoolean();
            HeightTolDown = reader.ReadSingle();
            HeightTolUp = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavMeshChunk : Rsc6FileBase //rage::ai::navMeshChunk
    {
        public override ulong BlockLength => 60;
        public override uint VFT { get; set; } = 0x04A5FA2C;
        public uint Unknown_8h { get; set; } //837592180?
        public Rsc6RawArr<ushort> ExternalSpanIndices { get; set; } //m_ExternalSpanIndices
        public Rsc6RawArr<ushort> ConnectedPolygonIndices { get; set; } //m_ConnectedPolygonIndices
        public Vector4 Offset { get; set; } //m_Offset
        public ushort ChunkDataIndex { get; set; } //m_ChunkDataIndex
        public ushort ConnectedPolygonIndicesNum { get; set; } //m_ConnectedPolygonIndicesNum
        public ushort ExternalSpanNum { get; set; } //m_ExternalSpanNum
        public ushort PolygonsNum { get; set; } //m_PolygonsNum
        public ushort Flags { get; set; } //m_Flags
        public ushort ChunkDataFileIndex { get; set; } //m_ChunkDataFileIndex

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            ExternalSpanIndices = reader.ReadRawArrPtr<ushort>();
            ConnectedPolygonIndices = reader.ReadRawArrPtr<ushort>();
            Offset = reader.ReadVector4();
            ChunkDataIndex = reader.ReadUInt16();
            ConnectedPolygonIndicesNum = reader.ReadUInt16();
            ExternalSpanNum = reader.ReadUInt16();
            PolygonsNum = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            ChunkDataFileIndex = reader.ReadUInt16();

            ExternalSpanIndices = reader.ReadRawArrItems(ExternalSpanIndices, ExternalSpanNum);
            ConnectedPolygonIndices = reader.ReadRawArrItems(ConnectedPolygonIndices, ConnectedPolygonIndicesNum);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public override string ToString()
        {
            return Offset.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavMeshSpan : Rsc6BlockBase //rage::ai::navMeshSpan
    {
        public override ulong BlockLength => 12;
        public ushort ChunkA { get; set; } //m_ChunkA
        public ushort ChunkB { get; set; } //m_ChunkB
        public ushort SpanDataIndex { get; set; } //m_SpanDataIndex
        public Rsc6NavmeshVertex Center { get; set; } //m_Center

        public override void Read(Rsc6DataReader reader)
        {
            ChunkA = reader.ReadUInt16();
            ChunkB = reader.ReadUInt16();
            SpanDataIndex = reader.ReadUInt16();
            Center = reader.ReadStruct<Rsc6NavmeshVertex>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
        }

        public override string ToString()
        {
            return $"ChunkA={ChunkA}, ChunkB={ChunkB}, Center={Center}";
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavIndexedChunkData : Rsc6BlockBase //rage::ai::navIndexedContainer<rage::ai::navMeshChunkData>
    {
        public override ulong BlockLength => 28;
        public bool StreamingEnabled { get; set; } //m_FlagStreamingEnabled
        public Rsc6RawArr<uint> Objects { get; set; } //m_ObjectArray, likely a streaming pool
        public int StreamingDataCategory { get; set; } //m_StreamingDataCategory
        public Rsc6Ptr<Rsc6NavStreamingMeshDataManager> StreamingDataManager { get; set; } //m_StreamingDataManager
        public int NumObjects { get; set; } //m_NumObjects

        public override void Read(Rsc6DataReader reader)
        {
            StreamingEnabled = reader.ReadInt32() != 0;
            Objects = reader.ReadRawArrPtr<uint>();
            StreamingDataCategory = reader.ReadInt32();
            StreamingDataManager = reader.ReadPtr<Rsc6NavStreamingMeshDataManager>();
            NumObjects = reader.ReadInt32();

            Objects = reader.ReadRawArrItems(Objects, (uint)(NumObjects * 2));
        }

        public override void Write(Rsc6DataWriter writer)
        {
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6NavStreamingMeshDataManager : Rsc6FileBase //rage::ai::navStreamingMeshDataManager
    {
        public override ulong BlockLength => 28;
        public override uint VFT { get; set; } = 0x04A5BF70;
        public ulong RscFiles { get; set; } //m_RscFiles, always 0?
        public ulong Touched { get; set; } //m_Touched, always 0?
        public Rsc6Str BaseName { get; set; } //m_BaseName ($/navres/rsc_territory_#/territory)
        public int NumFiles { get; set; } //m_NumFiles (288)

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            RscFiles = reader.ReadUInt64();
            Touched = reader.ReadUInt64();
            BaseName = reader.ReadStr();
            NumFiles = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
        }

        public override string ToString()
        {
            return $"{BaseName}, {NumFiles} file(s)";
        }
    }

    [TC(typeof(EXP))]
    public static class Rsc6NavmeshLookup
    {
        private static Rsc6NavMaterial[] NavMaterials;
        private static Rsc6NavMeshChunk[] WorldChunks;
        public static Dictionary<JenkHash, BoundingBox> NavmeshBounds = [];

        public static void Init(Rpf6FileManager fman)
        {
            if (WorldChunks != null) return;

            var dfm = fman.DataFileMgr;
            var wnmentries = dfm.StreamEntries[Rpf6FileExt.wnm];
            if (!wnmentries.TryGetValue(0xEAB7BA00, out var territoryEntry)) return; //territory

            var wnm = fman.LoadFilePack<WnmFile>(territoryEntry);
            if (wnm?.Navmesh is Rsc6NavHierarchySAGWorldChunk world)
            {
                NavMaterials = world.MaterialManager.Item?.Materials.Items;
                WorldChunks = world.HierarchyGrid.Item?.SpaceHierarchy?.SubSpace2.Item?.Chunks.Items;
            }
        }

        public static Colour GetNavMaterialColour(Rsc6NavMaterial mat)
        {
            if (mat == null) return new Colour(255, 255, 255, 255);
            if (mat.LiquidDepthLevel > 0) return new Colour(40, 90, 220, 255);

            var f = mat.Flags;
            if (mat.LiquidDepthLevel > 0 || (f & Rsc6NavMaterialFlags.Water) != 0) return new Colour(40, 90, 220, 255);
            if ((f & Rsc6NavMaterialFlags.NonNavigable) != 0) return new Colour(180, 40, 40, 255);
            if ((f & Rsc6NavMaterialFlags.IgnoreNav) != 0) return new Colour(80, 80, 80, 255);
            if ((f & Rsc6NavMaterialFlags.TrainTrack) != 0) return new Colour(120, 120, 120, 255);
            if ((f & Rsc6NavMaterialFlags.VehiclePath) != 0) return new Colour(180, 140, 60, 255);
            if ((f & Rsc6NavMaterialFlags.HorsePath) != 0) return new Colour(120, 90, 50, 255);
            if ((f & Rsc6NavMaterialFlags.PedPath) != 0) return new Colour(80, 180, 80, 255);
            if ((f & Rsc6NavMaterialFlags.Grass) != 0) return new Colour(50, 160, 50, 255);
            if ((f & Rsc6NavMaterialFlags.Sunny) != 0) return new Colour(230, 230, 230, 255);
            if ((f & Rsc6NavMaterialFlags.WideOpenArea) != 0) return new Colour(190, 210, 120, 255);
            if ((f & Rsc6NavMaterialFlags.OpenArea) != 0) return new Colour(210, 210, 150, 255);
            if ((f & Rsc6NavMaterialFlags.GenericSpawn) != 0) return new Colour(200, 160, 220, 255);
            if ((f & Rsc6NavMaterialFlags.Terrain) != 0) return new Colour(180, 180, 180, 255);
            return new Colour(255, 255, 255, 255);
        }

        public static bool TryGetMaterial(int index, out Rsc6NavMaterial mat)
        {
            mat = null;

            if (NavMaterials == null) return false;
            if ((uint)index >= (uint)NavMaterials.Length) return false;

            mat = NavMaterials[index];
            return mat != null;
        }

        public static bool TryGetChunk(int index, out Rsc6NavMeshChunk chunk)
        {
            chunk = null;

            if (WorldChunks == null) return false;
            if ((uint)index >= (uint)WorldChunks.Length) return false;

            chunk = WorldChunks[index];
            return chunk != null;
        }

        public static Dictionary<JenkHash, Rsc6Navmesh> GetAllNavmeshes(Rpf6FileManager fman)
        {
            var dict = new Dictionary<JenkHash, Rsc6Navmesh>();
            var entries = fman?.DataFileMgr?.StreamEntries[Rpf6FileExt.wbd_wcdt_wnm];

            foreach (var kvp in entries)
            {
                var entry = kvp.Value;
                if (entry == null) continue;
                if (entry.EntryParent.NameHash != 0x08F78E67) continue; //rsc_territory_w

                var wnm = fman.LoadFilePack<WnmFile>(entry);
                if (wnm == null) continue;

                dict[entry.NameHash] = wnm.Navmesh;
                NavmeshBounds[entry.NameHash] = wnm.Navmesh.BoundingBox;
            }
            return dict;
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
            var indexCount = NumVertices;

            if (indexCount < 3) return Vector3.UnitZ; //can only do this with at least 3 verts
            var v0 = GetVertex(0, nmverts, nminds);
            var v1 = GetVertex(1, nmverts, nminds);
            var v2 = GetVertex(2, nmverts, nminds);

            for (int i = 3; i < indexCount; i++) //use the average point of all the remaining vertices
            {
                v2 += GetVertex(i, nmverts, nminds);
            }

            if (indexCount > 3)
            {
                v2 *= 1.0f / (indexCount - 2);
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
        public short X { get; set; } //aiStoredFloat
        public short Y { get; set; } //aiStoredFloat
        public short Z { get; set; } //aiStoredFloat

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
            const float res = 0.05f; //rage::ai::aiStoredFloat<rage::ai::navSpaceMesh>::sm_ResolutionVec
            return new Vector3(Z * res, X * res, Y * res);
        }

        public readonly Vector3 ToVector3(in Vector3 aabbmin, in Vector3 aabbsize)
        {
            var v = ToVector3();
            return new Vector3(aabbmin.X + v.X, v.Y, aabbmin.Z + v.Z);
        }

        public void FromVector3(Vector3 v)
        {
            const float invRes = 20.0f; //rage::ai::aiStoredFloat<rage::ai::navSpaceMesh>::sm_InvResolution
            X = (short)Math.Round(v.X * invRes);
            Y = (short)Math.Round(v.Y * invRes);
            Z = (short)Math.Round(v.Z * invRes);
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

    public enum Rsc6NavmeshDataType : uint
    {
        NavMovableMesh = 0x04A5994C, //rage::ai::navMeshMovableType
        NavStreamingMesh = 0x04A00394, //rage::ai::navStreamingMeshDataManager::navMeshRscFile
        NavWorldChunkHierarchy = 0x04A00714 //rage::ai::navHierarchyGridWorldRscChunk
    }

    [Flags]
    public enum Rsc6NavMaterialFlags : uint
    {
        Terrain = 1u << 0,
        OpenArea = 1u << 1,
        WideOpenArea = 1u << 2,
        GenericSpawn = 1u << 3,
        PedPath = 1u << 4,
        HorsePath = 1u << 5,
        VehiclePath = 1u << 6,
        TrainTrack = 1u << 7,
        IgnoreNav = 1u << 8,
        Water = 1u << 9,
        NonNavigable = 1 << 10,
        Grass = 1u << 11,
        Sunny = 1u << 12,
        IgnoreAmbient = 1 << 13,
        UsedAmbient = 1 << 14,
        DropExclude = 1 << 15,
        NoCamAutoCenter = 1 << 16,
        HalfSpaceX = 1 << 17,
        HalfSpaceXNeg = 1 << 18,
        HalfSpaceY = 1 << 19,
        HalfSpaceYNeg = 1 << 20,
        PassesMatIncl0 = 1 << 21,
        PassesMatIncl1 = 1 << 22
    }
}