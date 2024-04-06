using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace CodeX.Games.RDR1
{
    public class RDR1Map : StreamingLevel<RDR1Game, RDR1MapFileCache>
    {
        public Rpf6FileManager FileManager;
        public WsiFile MainStreamingEntry, TerrainStreamingEntry;
        public HashSet<Entity> WspFiles;
        public HashSet<Entity> GrassBatchs;
        public List<LevelBVHItem> MapNodes;
        public List<Rpf6StoreItem> WvdTilesStore;
        public List<Rpf6StoreItem> WbdBoundsStore;
        public List<Rpf6StoreItem> WtbBoundsStore;
        public Dictionary<JenkHash, Rsc6SectorInfo> WsiFiles;
        public Dictionary<JenkHash, PiecePack> WvdTiles;
        public Dictionary<JenkHash, RDR1MapNode> MapNodeDict;
        public Dictionary<JenkHash, RDR1MapNode> StreamNodesPrev;
        public Dictionary<JenkHash, RDR1MapNode> StreamNodes;

        public static bool LoadingMap = false; //Used to detect whether or not we use the explorer
        public static readonly Setting StartPositionSetting = Settings.Register("RDR1Map.Area", SettingType.String, "Blackwater", "Change position", false, false, GetAreas());
        public static readonly Setting LoadBoundsSetting = Settings.Register("RDR1Map.Collisions", SettingType.String, "Near collisions", "No collisions", false, false, new string[] { "Near collisions", "Only collisions", "No collisions" });
        public static readonly Setting EnabledSetting = Settings.Register("RDR1Map.Enabled", SettingType.Bool, true, true);
        public static readonly Setting EnablePropsSetting = Settings.Register("RDR1Map.EnableProps", SettingType.Bool, true);
        public static readonly Setting EnableLightsSetting = Settings.Register("RDR1Map.EnableLights", SettingType.Bool, true);
        
        public Statistic NodeCountStat = Statistics.Register("RDR1Map.NodeCount", StatisticType.Counter);
        public Statistic EntityCountStat = Statistics.Register("RDR1Map.EntityCount", StatisticType.Counter);

        public RDR1Map(RDR1Game game) : base("RDR1 Map Level")
        {
            var position = StartPositionSetting.GetString() switch
            {
                "Armadillo" => new Vector3(2617.4f, -2180.6f, 20.2f),
                "Chuparosa" => new Vector3(4243.8f, -2690.5f, 42.8f),
                "Cochinay" => new Vector3(998.1f, -888.7f, 205.3f),
                "El Presidio" => new Vector3(3304.5f, -690.9f, 68.2f),
                "El Matadero" => new Vector3(3912.3f, -453.2f, 23.9f),
                "Escalera" => new Vector3(4422.8f, -4339.2f, 35.5f),
                "Fort Mercer" => new Vector3(3493.1f, -2699.5f, 70.7f),
                "Gaptooth Breach" => new Vector3(3256.36f, -4453.0f, 13.8f),
                "Las Hermanas" => new Vector3(4240.1f, -1699.9f, 12.6f),
                "MacFarlane's Ranch" => new Vector3(2404.3f, -844.2f, 95.1f),
                "Manzanita Post" => new Vector3(1639.6f, -435.1f, 155.6f),
                "Nosalida" => new Vector3(3964.4f, -4673.6f, 7.0f),
                "Pacific Union RR Camp" => new Vector3(2105.8f, -256.0f, 88.5f),
                "Plainview" => new Vector3(3732.4f, -3130.1f, 50.3f),
                "Rathskeller Fork" => new Vector3(2123.8f, -3661.3f, 48.4f),
                "Ridgewood Farm" => new Vector3(2721.0f, -3274.5f, 20.2f),
                "Tesoro Azul" => new Vector3(4547.6f, -3261.8f, 41.6f),
                "Thieve's Landing" => new Vector3(2230.7f, -134.1f, 76.4f),
                "Torquemada" => new Vector3(3456.4f, 370.5f, 80.7f),
                "Tumbleweed" => new Vector3(2950.4f, -3941.5f, 32.6f),
                "Twin Rocks" => new Vector3(2141.6f, -2430.9f, 28.0f),
                _ => new Vector3(1234.6f, 736.2f, 84.7f), //Blackwater by default
            };

            LoadingMap = true;
            this.Game = game;
            this.StreamPosition = this.DefaultSpawnPoint = position;
            this.BoundingBox = new BoundingBox(new Vector3(-100000.0f), new Vector3(100000.0f));
            this.InitPhysicsSim();
        }

        protected override bool StreamingInit()
        {
            Core.Engine.Console.Write("RDR1Map", "Initialising " + this.Game.Name + "...");
            this.FileManager = Game.GetFileManager() as Rpf6FileManager;

            if (this.FileManager == null)
            {
                throw new Exception("Failed to initialize RDR1.");
            }

            if (EnabledSetting.GetBool() == false)
            {
                Cache = new RDR1MapFileCache(FileManager);
                return true;
            }

            this.FileManager.Clear();
            this.FileManager.InitArchives();

            var colMode = LoadBoundsSetting.GetString();
            var dfm = this.FileManager.DataFileMgr;
            this.Cache = new RDR1MapFileCache(this.FileManager);
            this.StreamNodesPrev = new Dictionary<JenkHash, RDR1MapNode>();
            this.StreamNodes = new Dictionary<JenkHash, RDR1MapNode>();
            this.WsiFiles = new Dictionary<JenkHash, Rsc6SectorInfo>();
            this.MapNodes = new List<LevelBVHItem>();
            this.MapNodeDict = new Dictionary<JenkHash, RDR1MapNode>();
            this.MainStreamingEntry = dfm.WsiFiles.Values.First(item => item.Name == "swall.wsi"); //swAll
            this.TerrainStreamingEntry = dfm.WsiFiles.Values.First(item => item.Name == "swterrain.wsi"); //swTerrain

            if (colMode != "Only collisions")
            {
                this.LoadWvdTiles(dfm);
            }

            if (colMode != "No collisions")
            {
                this.LoadWbdBounds(dfm);
                this.WtbBoundsStore = this.FileManager.Store.TerritoryBoundsDict.Values.ToList();

                if (colMode == "Only collisions")
                {
                    StreamCollisionsRange = 1000.0f;
                }
            }

            /*
            this.LoadTrees(dfm);
            */

            /*var grassRpf = this.FileManager.AllArchives.FirstOrDefault(e => e.Name == "grassres.rpf");
            var grassEntries = grassRpf.AllEntries.Where(e => e.Name.EndsWith(".wsg")).ToList();
            this.GrassBatchs = new HashSet<Entity>();

            for (int i = 0; i < grassEntries.Count; i++)
            {
                var ent = (Rpf6FileEntry)grassEntries[i];
                var pack = (WsgFile)this.FileManager.LoadFilePack(ent);
                var fields = pack?.GrassField?.GrassItems.Items;

                if (fields == null) continue;
                for (int j = 0; j < fields.Length; j++)
                {
                    var field = fields[j];
                    var batch = new RDR1GrassBatch(field, this);

                    if (batch != null)
                    {
                        GrassBatchs.Add(batch);
                    }
                }
            }*/

            if (colMode != "Only collisions")
            {
                for (int i = 0; i < this.MainStreamingEntry.StreamingItems.ItemMapChilds.Count; i++)
                {
                    var child = this.MainStreamingEntry.StreamingItems.ItemMapChilds[i];
                    if (child.SectorName.StartsWith("cs") || child.SectorName.StartsWith("sw") || child.SectorName.StartsWith("dlc")) continue;

                    var node = new RDR1MapNode(child, this);
                    this.MapNodes.Add(node);
                    this.MapNodeDict[node.NameHash] = node;
                }

                for (int i = 0; i < WvdTilesStore.Count; i++)
                {
                    var tile = WvdTilesStore[i];
                    var node = new RDR1MapNode(tile);
                    this.MapNodes.Add(node);
                    this.MapNodeDict[node.NameHash] = node;
                }
            }

            this.BVH = new LevelBVH();
            this.BVH.Init(this.MapNodes);

            Core.Engine.Console.Write("RDR1Map", FileManager.Game.Name + " map initialised.");

            return true;
        }

        public static string[] GetAreas()
        {
            return new[] { "Armadillo", "Blackwater", "Chuparosa", "Cochinay", "El Presidio", "El Matadero", "Escalera", "Fort Mercer", "Gaptooth Breach", "Las Hermanas", "MacFarlane's Ranch", "Manzanita Post", "Nosalida", "Pacific Union RR Camp", "Plainview", "Rathskeller Fork", "Ridgewood Farm", "Tesoro Azul", "Thieve's Landing", "Torquemada", "Tumbleweed", "Twin Rocks" };
        }

        public static bool ShouldSkipObject(Rpf6FileEntry entry)
        {
            return entry.Name.Contains("non-terrain")
                || entry.Name.StartsWith("mp_")
                || entry.Name.Contains("low")
                || entry.Name.Contains("med")
                || (entry.Parent.Name != "resource_0" && entry.Parent.Name != "resource_2")
                || entry.Name.Contains("lod");
        }

        public static bool ShouldSkipCollisions(Rpf6FileEntry entry)
        {
            return entry.Name.Contains("prop")
                || entry.Name.Contains("dlc")
                || entry.Name.Contains("_int_")
                || entry.Name.Contains("mp_")
                || entry.Parent.Parent.Name.Contains("cs_")
                || entry.Parent.Parent.Name.Contains("sw_")
                || entry.Name.Contains("flags");
        }

        public static bool IsObjectProp(string obj)
        {
            return obj.Contains("p_gen") || (obj.StartsWith("p_") && obj.EndsWith("x"));
        }

        public void LoadWvdTiles(Rpf6DataFileMgr dfm)
        {
            this.WvdTilesStore = new List<Rpf6StoreItem>();
            var dict = this.FileManager.Store.TilesDict; //Filter this to only the current stream entries
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wvd, out var files);

            foreach (var file in files)
            {
                var entry = file.Value;
                if (entry == null) continue;
                var path = entry.PathLower;
                if (!path.EndsWith(".wvd") || ShouldSkipObject(entry)) continue;

                if (dict.TryGetValue(path, out var tilesItem))
                {
                    this.WvdTilesStore.Add(tilesItem);
                }
            }
        }

        public void LoadWbdBounds(Rpf6DataFileMgr dfm)
        {
            this.WbdBoundsStore = new List<Rpf6StoreItem>();
            var dict = this.FileManager.Store.BoundsDict; //Filter this to only the current stream entries
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wbd_wcdt, out var files);

            foreach (var file in files)
            {
                var entry = file.Value;
                if (entry == null) continue;
                var path = entry.PathLower;
                if (!path.EndsWith(".wbd") || ShouldSkipCollisions(entry)) continue;

                if (dict.TryGetValue(path, out var boundsItem))
                {
                    this.WbdBoundsStore.Add(boundsItem);
                }
            }
        }

        public void LoadTrees(Rpf6DataFileMgr dfm)
        {
            WspFiles = new HashSet<Entity>();
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wft, out var files);

            var treeFragment = new KeyValuePair<JenkHash, Rpf6FileEntry>();
            foreach (var file in files)
            {
                if (file.Value.Name != "st_joshuatree01x.wft") continue;
                treeFragment = file;
                break;
            }

            var wsps = dfm.WspFiles.Values.ToList();
            for (int i = 0; i < dfm.WspFiles.Count; i++)
            {
                var wsp = wsps[i].Trees;
                for (int i1 = 0; i1 < wsp.GridCells.Items.Length; i1++)
                {
                    for (int i2 = 0; i2 < wsp.GridCells.Items[i1].CombinedInstanceListPos.Count; i2++)
                    {
                        var instancePositions = wsp.GridCells.Items[i1].CombinedInstanceListPos[i2];
                        if (Vector3.Distance(instancePositions.Position, StreamPosition) > 300.0f) continue;
                        var pieces = Cache.GetPieces(treeFragment.Key, true);

                        if (pieces != null)
                        {
                            foreach (var p in pieces)
                            {
                                if (p == null) continue;
                                var entity = new WsiEntity(treeFragment.Key.Str)
                                {
                                    Piece = p,
                                    ResetPos = true,
                                    LodDistMax = 50.0f,
                                    Position = instancePositions.Position,
                                };
                                entity.UpdateBounds();
                                WspFiles.Add(entity);
                            }
                        }
                    }
                }
            }
        }

        protected override bool StreamingUpdate()
        {
            if (StreamNodes == null) return false;
            if (EnabledSetting.GetBool() == false) return false;

            var ents = StreamEntities.CurrentSet;
            var spos = StreamPosition;
            var nodes = StreamNodesPrev;

            StreamNodesPrev = StreamNodes;
            nodes.Clear();

            var curNodes = BVH.GetItems(ref spos);
            foreach (var node in curNodes.Cast<RDR1MapNode>())
            {
                var n = node;
                if (n.NameHash == 0) continue;

                while (!nodes.ContainsKey(n.NameHash))
                {
                    nodes[n.NameHash] = n;
                    if (n.ParentNameHash == 0) break;
                    if (!MapNodeDict.TryGetValue(n.ParentNameHash, out n)) break;
                    if (n == null) break;
                }
            }

            //ents.UnionWith(WspFiles);
            foreach (var batch in GrassBatchs)
            {
                RecurseAddStreamEntity((WsiEntity)batch, ref spos, ents);
            }

            foreach (var kvp in nodes) //Find current entities for max lod level
            {
                var node = kvp.Value;
                var mapdata = node.MapData;
                if (node?.MapData?.RootEntities != null)
                {
                    foreach (var e in mapdata.RootEntities.Cast<WsiEntity>())
                    {
                        RecurseAddStreamEntity(e, ref spos, ents);
                    }
                }
            }

            var needsAnotherUpdate = AddExtraStreamEntities();
            var newEnts = new HashSet<WsiEntity>();

            //Make sure all assets are loaded
            foreach (var ent in ents.ToList()) //Creates a copy to avoid modifying collection while iterating
            {
                var hash = new JenkHash();
                if (ent is RDR1GrassBatch grassEnt)
                {
                    hash = new JenkHash("p_" + grassEnt.Name + "x");
                }
                else if (ent is WsiEntity wsiEnt)
                {
                    var t = wsiEnt.ModelName;
                    if (ent.Name != null)
                    {
                        hash = new JenkHash(ent.Name);
                    }
                    else
                    {
                        if (t.Str.Contains("0x"))
                            hash = t;
                        else
                            hash = new JenkHash(t.Str);
                    }
                }

                var pieces = Cache.GetPieces(hash, IsObjectProp(hash.Str));
                if (pieces.Count > 0)
                {
                    if (!pieces[0].FilePack.FileInfo.Path.Contains("resource_0"))
                    {
                        ent.SetPiece(pieces[0]);
                        ent.Batch?.Update(ref spos);
                    }
                    else if (pieces.Count == 4)
                    {
                        foreach (var piece in pieces)
                        {
                            if (piece == null) continue;
                            var newEnt = new WsiEntity(piece.Name)
                            {
                                LodLevel = 0,
                                LodDistMax = 750.0f,
                                ResetPos = true,
                                Index = Entities.Count,
                                Level = this
                            };
                            newEnt.SetPiece(piece);
                            newEnt.Batch?.Update(ref spos);
                            ents.Add(newEnt);
                        }
                        ents.Remove(ent);
                    }
                }
            }

            foreach (var ent in ents) //Update lights
            {
                if ((ent is WsiEntity wEnt) && (wEnt.Lights != null) && (wEnt.ParentName != string.Empty))
                {
                    var parent = ents.FirstOrDefault(e => e.Position == wEnt.ParentPosition);
                    if (parent != null && parent.Position != Vector3.Zero && parent.Piece.Name.StartsWith("p_gen"))
                    {
                        var scale = new Vector3(500.0f);
                        wEnt.SetPiece(parent.Piece);
                        wEnt.BoundingBox = new BoundingBox(parent.BoundingBox.Minimum - scale, parent.BoundingBox.Maximum + scale);
                        wEnt.BoundingSphere = new BoundingSphere(ent.BoundingBox.Center, ent.BoundingBox.Size.Length() * 0.5f);
                    }
                }
            }

            if (WbdBoundsStore != null)
            {
                var collsRng = new Vector3(StreamCollisionsRange);
                var collsBox = new BoundingBox(spos - collsRng, spos + collsRng);
                foreach (var bnd in WbdBoundsStore)
                {
                    if (bnd.Box.Contains(ref collsBox) == ContainmentType.Intersects)
                    {
                        var bounds = Cache.GetStaticBounds(bnd.Hash, true);
                        if (bounds != null)
                        {
                            foreach (var b in bounds)
                            {
                                if (b != null) StreamCollisions.Add(b);
                            }
                        }
                    }
                }
            }
            if (WtbBoundsStore != null)
            {
                var collsRng = new Vector3(StreamCollisionsRange);
                var collsBox = new BoundingBox(spos - collsRng, spos + collsRng);
                foreach (var bnd in WtbBoundsStore)
                {
                    if (bnd.Box.Contains(ref collsBox) == ContainmentType.Intersects)
                    {
                        var bounds = Cache.GetStaticBounds(bnd.Hash, false);
                        if (bounds != null)
                        {
                            foreach (var b in bounds)
                            {
                                if (b != null) StreamCollisions.Add(b);
                            }
                        }
                    }
                }
            }

            NodeCountStat.SetCounter(nodes.Count);
            EntityCountStat.SetCounter(ents.Count);

            StreamNodes = nodes;
            if (needsAnotherUpdate) StreamUpdateRequest = true;

            return true;
        }

        private void RecurseAddStreamEntity(WsiEntity e, ref Vector3 spos, HashSet<Entity> ents)
        {
            if (e.Batch != null)
            {
                var d = (e.Position - spos).Length();
                if (d > e.LodDistMax) return;
                e.StreamingDistance = (e.Batch.Lods == null) ? 1000.0f : d;

                if (e.Batch.Lods == null)
                {
                    if (e.BoundingBox.Contains(ref spos) == ContainmentType.Contains)
                    {
                        e.StreamingDistance = 0.0f;
                    }
                }
                ents.Add(e);
                return;
            }

            e.StreamingDistance = (e.Position - spos).Length();
            if (e.BoundingBox.Contains(ref spos) == ContainmentType.Contains)
            {
                e.StreamingDistance = 0.0f;
            }

            if ((e.StreamingDistance < e.LodDistMax) && ((e.LodChildren == null) || (e.StreamingDistance >= e.LodDistMin)))
            {
                if (e.LodLevel == 1 && e.StreamingDistance <= 600.0f) return;
                ents.Add(e);
            }
        }

        public override void GetEntities(SceneViewProjection proj)
        {
            base.GetEntities(proj);

            var isScreen = proj.View.Type == SceneViewType.Screen;
            var isShadow = proj.View.Type == SceneViewType.ShadowMap;

            var ents = proj.View.ViewEntities ?? StreamEntities.ActiveSet;
            if (ents != null)
            {
                foreach (var ent in ents)
                {
                    if (ent == null) continue;
                    if (ent.Piece == null)
                    {
                        ents.Remove(ent);
                        continue;
                    }

                    if (ent.BoundingSphere.Radius < proj.MinEntitySize) continue;
                    if (ent.BoundingBox.Minimum == ent.BoundingBox.Maximum) continue;

                    ent.CurrentDistance = ent.StreamingDistance;
                    if (proj.Frustum.ContainsAABB(ref ent.BoundingBox))
                    {
                        if (ent.Batch?.Lods != null)
                        {
                            //Add separate entities for the lod batches
                            for (int i = 0; i < ent.Batch.Lods.Length; i++)
                            {
                                var l = ent.Batch.Lods[i];
                                if (l?.RenderEntity != null)
                                {
                                    l.RenderEntity.CurrentDistance = ent.CurrentDistance;
                                    proj.Entities.Add(l.RenderEntity);
                                }
                            }
                        }
                        else if (!isShadow && ent.Lights != null)
                            proj.Lights.Add(ent);
                        else
                            proj.Entities.Add(ent);
                    }
                }
            }

            proj.SortEntities();
            if (isScreen)
            {
                UpdateStreamingPosition(proj.Params.Position);
            }
        }
    }

    public class RDR1GrassBatch : WsiEntity
    {
        public float CellSize;
        public sbyte GridSizeX;
        public sbyte GridSizeY;
        public string GrassFieldName;

        public RDR1GrassBatch(Rsc6GrassField field, Level level)
        {
            if (field == null) return;
            SetFieldBatch(field);

            Level = level;
            GrassFieldName = field.Name.Value;

            var b = new EntityBatch(this)
            {
                InstanceCount = field.Batchs.Count,
                InstanceLayout = 3,
                InstanceScaleParams = field.AABBScale,
                InstanceBatchMin = field.AABBMin,
                InstanceBatchSize = field.GetAABBSize(),
                Data = field.BatchData
            };

            var bbs = field.GetAABBSize();
            var csx = bbs.X / 32;
            var csy = bbs.Y / 32;

            var x = (sbyte)(bbs.X > 127 ? 127 : Math.Abs((sbyte)bbs.X));
            var y = (sbyte)(bbs.Y > 127 ? 127 : Math.Abs((sbyte)bbs.Y));

            if (csx >= csy)
            {
                CellSize = csx;
                GridSizeX = x;
                GridSizeY = y;
            }
            else
            {
                CellSize = csy;
                GridSizeY = x;
                GridSizeX = y;
            }

            b.InitGrid(GridSizeX, GridSizeY, CellSize);
            SetBatch(b);
        }

        protected Entity CreateBatchLodEntity(EntityBatchLod lod, Piece p, int pieceLod)
        {
            var e = new Entity
            {
                Level = Level,
                Piece = p,
                Name = GrassFieldName,
                PieceLodOverride = pieceLod,
                CurrentDistance = lod.LodDist,
                BoundingBox = BoundingBox,
                BoundingSphere = BoundingSphere,
                Position = BoundingBox.Center,
                Orientation = Quaternion.Identity,
                Scale = Vector3.One,
                ParentIndex = -1
            };
            e.SetBatchLod(lod);
            e.UpdateWorldTransform();
            return e;
        }

        public override void SetPiece(Piece p)
        {
            var changed = p != Piece;
            base.SetPiece(p);

            if (changed)
            {
                if ((Batch?.Grid != null) && (p is Rsc6Drawable d))
                {
                    var hdist = d.LodDistHigh;
                    Batch.InitLods(hdist);
                    Batch.Lods[0].RenderEntity = CreateBatchLodEntity(Batch.Lods[0], p, 0);
                }
            }
        }
    }

    public class RDR1MapNode : LevelBVHItem
    {
        public RDR1MapNode ParentNode;
        public RDR1MapData MapData;
        public JenkHash NameHash;
        public JenkHash ParentNameHash;

        public RDR1MapNode(Rsc6SectorChild mapChild, RDR1Map map)
        {
            MapData = new RDR1MapData(mapChild, map);
            NameHash = new(mapChild.SectorName.ToLower());
            StreamingBox = mapChild.SectorBounds;
            BoundingBox = mapChild.SectorBounds;
        }

        public RDR1MapNode(Rpf6StoreItem item)
        {
            var highLod = item.Path.Contains("resource_0");
            var dist = highLod ? new Vector3(700.0f) : new Vector3(9999.0f);

            MapData = new RDR1MapData(item);
            NameHash = item.Hash;
            StreamingBox = new BoundingBox(item.Box.Center - dist, item.Box.Center + dist);
            BoundingBox = item.Box;
        }

        public void UnloadMapData()
        {
            if (MapData != null)
            {
                foreach (var e in MapData.RootEntities)
                {
                    e.LodParent?.RemoveLodChild(e);
                }
                MapData.LodParent = null;
            }

            MapData = null;
            ParentNode = null;
        }

        public void SetParentNode(RDR1MapNode node)
        {
            if (ParentNode != node)
            {
                ParentNode = node;
                MapData?.SetParent(node?.MapData);
            }
        }

        public override string ToString()
        {
            return NameHash.ToString();
        }
    }

    public class RDR1MapData : Level
    {
        public RDR1MapData(Rsc6SectorChild mapChild, RDR1Map map)
        {
            var dfm = map.FileManager.DataFileMgr;
            var mapNode = dfm.WsiFiles.FirstOrDefault(e => e.Key.Equals(new JenkHash(mapChild.SectorName.ToLower()))).Value;

            var childs = mapNode?.StreamingItems?.ItemChilds.Item;
            if (childs == null) return;

            foreach (var sector in childs.Sectors.Items)
            {
                if (sector.Name.Value == childs.Name.Value) continue;

                //Add the sector as a entity
                var sectorEnt = new WsiEntity(sector.Name.Value.ToLower())
                {
                    LodDistMax = 750.0f,
                    Position = sector.Bounds.Center,
                    ResetPos = true,
                    Index = Entities.Count,
                    Level = this
                };
                this.Add(sectorEnt);

                if (RDR1Map.EnablePropsSetting.GetBool())
                {
                    var entities = sector.Entities.Items;
                    if (entities != null && entities.Length > 0)
                    {
                        foreach (var entity in entities)
                        {
                            var e = new WsiEntity(entity)
                            {
                                Index = Entities.Count,
                                Level = this
                            };
                            this.Add(e);
                        }
                    }
                }

                if (RDR1Map.EnableLightsSetting.GetBool())
                {
                    var lights = sector.PlacedLightsGroup.Item;
                    if (lights != null)
                    {
                        foreach (var light in lights.Lights.Items)
                        {
                            var e = new WsiEntity(light, lights.Name.Value)
                            {
                                Index = Entities.Count,
                                Level = this
                            };
                            this.Add(e);
                        }
                    }
                }

                //Link each entity to its parent sector
                for (int i = 1; i < Entities.Count; i++)
                {
                    var entity = Entities[i];
                    var parent = Entities[0];

                    entity.LodParent = parent;
                    entity.LodParent?.AddLodChild(entity);
                }
            }
        }

        public RDR1MapData(Rpf6StoreItem item)
        {
            var highLod = item.Path.Contains("resource_0");
            var e = new WsiEntity(item)
            {
                LodLevel = highLod ? 0 : 1,
                LodDistMax = highLod ? 750.0f : 9999.0f,
                ResetPos = true,
                Index = Entities.Count,
                Level = this
            };
            this.Add(e);

            e.LodParent = e;
            e.LodParent?.AddLodChild(e);
        }
    }

    public class RDR1MapFileCache : StreamingCache
    {
        public Rpf6FileManager FileManager;
        private Dictionary<Rpf6FileExt, Dictionary<JenkHash, StreamingCacheEntry>> Cache = new();
        private Dictionary<JenkHash, StreamingCacheEntry> BoundsCache = new();
        private List<JenkHash> RemoveItems = new();

        public RDR1MapFileCache(Rpf6FileManager fman)
        {
            FileManager = fman;
        }

        private Dictionary<JenkHash, StreamingCacheEntry> GetCache(Rpf6FileExt ext)
        {
            if (!Cache.TryGetValue(ext, out Dictionary<JenkHash, StreamingCacheEntry> cache))
            {
                cache = new Dictionary<JenkHash, StreamingCacheEntry>();
                Cache[ext] = cache;
            }
            return cache;
        }

        public override void BeginFrame()
        {
            base.BeginFrame();
            foreach (var cache in Cache)
            {
                RemoveOldItems(cache.Value, RemoveItems);
            }
            RemoveOldItems(BoundsCache, RemoveItems);
        }

        public PiecePack GetPiecePack(JenkHash hash, Rpf6FileExt ext)
        {
            var cache = GetCache(ext);
            if (!cache.TryGetValue(hash, out var cacheItem))
            {
                cacheItem = new StreamingCacheEntry();
                var entry = FileManager.DataFileMgr.TryGetStreamEntry(hash, ext);
                if (entry != null)
                {
                    Core.Engine.Console.Write("RDR1Map", entry.Name);
                    try
                    {
                        var piecePack = FileManager.LoadPiecePack(entry, null, true);
                        cacheItem.Object = piecePack;
                    }
                    catch { }
                }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            cache[hash] = cacheItem;
            return cacheItem.Object as PiecePack;
        }

        public List<Piece> GetPieces(JenkHash hash, bool fragment = false)
        {
            if (string.IsNullOrEmpty(hash.Str)) return null;
            var pack = GetPiecePack(new JenkHash(hash.Str), fragment ? Rpf6FileExt.wft : Rpf6FileExt.wvd);

            var pieces = new List<Piece>();
            if (pack?.Pieces != null)
            {
                pack.Pieces.TryGetValue(hash, out var piece);
                if (piece == null)
                {
                    if (pack.Pieces.Count == 4)
                        pieces.AddRange(pack.Pieces.Values);
                    else
                        pieces.Add(pack.Pieces.FirstOrDefault().Value); //Low lods contain only one piece
                }
                else
                {
                    pieces.Add(piece);
                }
            }
            return pieces;
        }

        public List<Entity> GetStaticBounds(JenkHash hash, bool wbd)
        {
            if (!BoundsCache.TryGetValue(hash, out var cacheItem))
            {
                cacheItem = new StreamingCacheEntry();
                var pack = GetPiecePack(hash, wbd ? Rpf6FileExt.wbd_wcdt : Rpf6FileExt.wtb);
                var pieces = pack?.Pieces;

                if (pieces != null)
                {
                    var ents = new List<Entity>();
                    foreach (var piece in pieces)
                    {
                        if (piece.Value == null) continue;
                        var ent = new Entity(piece.Value);
                        ents.Add(ent);
                    }
                    cacheItem.Object = ents;
                }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            BoundsCache[hash] = cacheItem;
            return cacheItem.Object as List<Entity>;
        }
    }
}