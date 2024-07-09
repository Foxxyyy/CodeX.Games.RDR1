using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using CodeX.Games.RDR1.Files;
using CodeX.Core.UI;
using BepuPhysics.Trees;

namespace CodeX.Games.RDR1
{
    public class RDR1Map : StreamingLevel<RDR1Game, RDR1MapFileCache, Rpf6FileManager>
    {
        public WsiFile MainStreamingEntry; //The global #si that lists all the sectors/areas
        public WsiFile TerrainStreamingEntry; //The global terrain #si that lists all the wilderness stuff & some random props
        public List<WsiFile> StreamingEntries; //All the child sectors grouped together
        public List<Rpf6StoreItem> WvdTilesStore; //Basic data of tiles to render
        public List<Rpf6StoreItem> WbdBoundsStore; //Basic data of bounds dictionary to render
        public List<Rpf6StoreItem> WtbBoundsStore; //Basic data of tiles collisions to render
        public List<TreeItem> Trees; //Basic data of trees, debris, rocks & placed grass
        public HashSet<Entity> GrassBatchs; //The grass batches //TODO:

        public Dictionary<JenkHash, RDR1MapNode> MapNodeDict;
        public StreamingSet<RDR1MapNode> StreamNodes;

        public static bool LoadingMap = false; //Used to detect if we're using the explorer...
        public static readonly Setting StartPositionSetting = Settings.Register("RDR1Map.Area", SettingType.String, "Blackwater", "Change position", false, false, GetAreas());
        public static readonly Setting LoadBoundsSetting = Settings.Register("RDR1Map.Collisions", SettingType.String, "No collisions", "No collisions", false, false, new string[] { "Near collisions", "Only collisions", "No collisions" });
        public static readonly Setting EnabledSetting = Settings.Register("RDR1Map.Enabled", SettingType.Bool, true, true);
        public static readonly Setting EnableTreesSetting = Settings.Register("RDR1Map.EnableTrees", SettingType.Bool, true);
        public static readonly Setting EnablePropsSetting = Settings.Register("RDR1Map.EnableProps", SettingType.Bool, true);
        public static readonly Setting EnableLightsSetting = Settings.Register("RDR1Map.EnableLights", SettingType.Bool, false);
        
        public Statistic NodeCountStat = Statistics.Register("RDR1Map.NodeCount", StatisticType.Counter);
        public Statistic EntityCountStat = Statistics.Register("RDR1Map.EntityCount", StatisticType.Counter);

        public RDR1Map(RDR1Game game) : base(game, "RDR1 Map Level")
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
                _ => new Vector3(1234.6f, 736.2f, 84.7f) //Blackwater by default
            };

            Rsc6DataWriter.UseProjectExplorer = true;
            LoadingMap = true;

            this.Game = game;
            this.DefaultSpawnPoint = position;
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

            var colMode = LoadBoundsSetting.GetString();
            var dfm = this.FileManager.DataFileMgr;

            this.Cache = new RDR1MapFileCache(this.FileManager);
            this.StreamNodes = new StreamingSet<RDR1MapNode>();
            this.StreamPosition = this.DefaultSpawnPoint;
            this.StreamingEntries = new List<WsiFile>();
            this.MapNodeDict = new Dictionary<JenkHash, RDR1MapNode>();

            this.MainStreamingEntry = dfm.WsiFiles[new JenkHash("swall")]; //swAll
            this.TerrainStreamingEntry = dfm.WsiFiles[new JenkHash("swterrain")]; //swTerrain

            foreach (var child in this.MainStreamingEntry.StreamingItems.ItemMapChilds.Items)
            {
                var entry = dfm.WsiFiles[new JenkHash(child.Name.ToLower())];
                this.StreamingEntries.Add(entry);
            }

            foreach (var child in this.TerrainStreamingEntry.StreamingItems.ItemMapChilds.Items)
            {
                var entry = dfm.WsiFiles[new JenkHash(child.Name.ToLower())];
                this.StreamingEntries.Add(entry);
            }

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
                if (EnableTreesSetting.GetBool())
                {
                    foreach (var wsp in dfm.WspFiles)
                    {
                        var node = new RDR1MapNode(wsp.Value);
                        this.MapNodeDict[node.NameHash] = node;
                    }
                }

                foreach (var child in this.StreamingEntries)
                {
                    if (child.Name.StartsWith("cs") || child.Name.StartsWith("sw") || child.Name.StartsWith("dlc")) continue;
                    var node = new RDR1MapNode(child);
                    this.MapNodeDict[node.NameHash] = node;
                }

                foreach (var tile in this.WvdTilesStore)
                {
                    var node = new RDR1MapNode(tile);
                    this.MapNodeDict[node.NameHash] = node;
                }
            }

            this.StreamBVH = new StreamingBVH();
            foreach (var kvp in this.MapNodeDict)
            {
                var mapnode = kvp.Value;
                if (mapnode.StreamingBox.Minimum != mapnode.StreamingBox.Maximum)
                {
                    this.StreamBVH.Add(mapnode);
                }
            }

            Core.Engine.Console.Write("RDR1Map", FileManager.Game.Name + " map initialised.");
            return true;
        }

        protected override bool StreamingUpdate()
        {
            if (this.StreamNodes == null) return false;
            if (EnabledSetting.GetBool() == false) return false;

            this.StreamNodes.BeginIteration();

            var nodes = this.StreamNodes.CurrentSet;
            var ents = this.StreamEntities.CurrentSet;
            var spos = this.StreamPosition;

            this.StreamBVH.BeginIteration();
            this.StreamBVH.AddStreamPosition(spos);

            
            foreach (var node in this.StreamBVH.StreamItems.Cast<RDR1MapNode>())
            {
                var n = node;
                if (n.NameHash == 0) continue;
                if (n.Enabled == false) continue;

                while (!nodes.Contains(n))
                {
                    nodes.Add(n);
                }
            }

            if (this.GrassBatchs != null) //Grass batches
            {
                foreach (var batch in this.GrassBatchs)
                {
                    RecurseAddStreamEntity((WsiEntity)batch, ref spos, ents);
                }
            }

            foreach (var node in nodes) //Find current entities for max lod level
            {
                var mapdata = node.MapData;
                if (mapdata?.RootEntities != null)
                {
                    foreach (var e in mapdata.RootEntities)
                    {
                        RecurseAddStreamEntity(e, ref spos, ents);
                    }
                }
            }

            var needsAnotherUpdate = AddExtraStreamEntities();
            foreach (var ent in ents.ToList()) //Make sure all current entities assets are loaded
            {
                JenkHash hash = 0u;
                if (ent is RDR1GrassBatch grassEnt)
                {
                    hash = new JenkHash("p_" + grassEnt.Name + "x");
                }
                else if (ent is RDR1GridForestEntity treeEnt)
                {
                    hash = treeEnt.TreeName;
                }
                else if (ent is RDR1LightEntity lightEnt)
                {
                    hash = lightEnt.ModelName;
                }
                else if (ent is WsiEntity wsiEnt)
                {
                    var modelName = wsiEnt.ModelName;
                    if (ent.Name != null)
                        hash = new JenkHash(ent.Name);
                    else
                        hash = modelName;
                }

                var pieces = this.Cache.GetPieces(hash, hash);
                if (pieces != null)
                {
                    if (ent.LodDistMax == 800.0f)
                    {
                        foreach (var piece in pieces)
                        {
                            if (piece == null) continue;
                            var newEnt = new WsiEntity()
                            {
                                ModelName = new(piece.Name),
                                LodLevel = 0,
                                LodDistMax = 800.0f,
                                ResetPos = true,
                                Index = Entities.Count,
                                Level = this
                            };
                            newEnt.SetPiece(piece);
                            ents.Add(newEnt);
                        }
                        ents.Remove(ent);
                    }
                    else
                    {
                        ent.SetPiece(pieces[0]);
                        ent.Batch?.Update(ref spos);

                        //If we have a tree, let's not switch between lods, low lods have no trunks...
                        if (ent is RDR1GridForestEntity ft)
                        {
                            ft.Piece.Lods[0].LodDist = 9999.0f;
                        }
                    }
                }
                else if (ent is WsiEntity || ent is RDR1LightEntity)
                {
                    string parent = null;
                    if (ent is WsiEntity wsiEnt)
                        parent = wsiEnt.ParentName;
                    else if (ent is RDR1LightEntity lightEnt)
                        parent = lightEnt.ParentName;

                    if (string.IsNullOrEmpty(parent)) continue;
                    pieces = this.Cache.GetPieces(new(parent), hash);
                    if (pieces != null)
                    {
                        ent.SetPiece(pieces[0]);
                    }
                }
            }

            var scale = new Vector3(10.0f);
            foreach (var ent in ents) //Update lights
            {
                if (ent is RDR1LightEntity light && light.Lights != null && light.ParentName != string.Empty)
                {
                    var parent = ents.FirstOrDefault(e => e.Position == light.ParentPosition);
                    if (parent != null && parent.Position != Vector3.Zero)
                    {
                        light.SetPiece(parent.Piece);
                        light.BoundingBox = new BoundingBox(parent.BoundingBox.Minimum - scale, parent.BoundingBox.Maximum + scale);
                        light.BoundingSphere = new BoundingSphere(light.BoundingBox.Center, light.BoundingBox.Size.Length() * 0.5f);
                    }
                }
            }

            if (this.WbdBoundsStore != null)
            {
                var collsRng = new Vector3(this.StreamCollisionsRange);
                var collsBox = new BoundingBox(spos - collsRng, spos + collsRng);
                foreach (var bnd in this.WbdBoundsStore)
                {
                    if (bnd.Box.Contains(ref collsBox) == ContainmentType.Intersects)
                    {
                        var bounds = this.Cache.GetStaticBounds(bnd.Hash, true);
                        if (bounds != null)
                        {
                            foreach (var b in bounds)
                            {
                                if (b != null) this.StreamCollisions.Add(b);
                            }
                        }
                    }
                }
            }

            if (this.WtbBoundsStore != null)
            {
                var collsRng = new Vector3(this.StreamCollisionsRange);
                var collsBox = new BoundingBox(spos - collsRng, spos + collsRng);
                foreach (var bnd in this.WtbBoundsStore)
                {
                    if (bnd.Box.Contains(ref collsBox) == ContainmentType.Intersects)
                    {
                        var bounds = this.Cache.GetStaticBounds(bnd.Hash, false);
                        if (bounds != null)
                        {
                            foreach (var b in bounds)
                            {
                                if (b != null) this.StreamCollisions.Add(b);
                            }
                        }
                    }
                }
            }

            this.NodeCountStat.SetCounter(nodes.Count);
            this.EntityCountStat.SetCounter(ents.Count);

            this.StreamNodes.EndIteration();
            if (needsAnotherUpdate) this.StreamUpdateRequest = true;

            return true;
        }

        private static void RecurseAddStreamEntity(Entity e, ref Vector3 spos, HashSet<Entity> ents)
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
                ents.Add(e);
            }
        }

        public static string[] GetAreas()
        {
            return new[] { "Armadillo", "Blackwater", "Chuparosa", "Cochinay", "El Presidio", "El Matadero", "Escalera", "Fort Mercer", "Gaptooth Breach", "Las Hermanas", "MacFarlane's Ranch", "Manzanita Post", "Nosalida", "Pacific Union RR Camp", "Plainview", "Rathskeller Fork", "Ridgewood Farm", "Tesoro Azul", "Thieve's Landing", "Torquemada", "Tumbleweed", "Twin Rocks" };
        }

        public static bool ShouldSkipVisualDict(Rpf6FileEntry entry) //Avoid loading useless #vd's for map viewer
        {
            return entry.Name.Contains("non-terrain")
                || entry.Name.StartsWith("mp_")
                || entry.Name.Contains("low")
                || entry.Name.Contains("med")
                || entry.Parent.Name != "resource_0"
                || entry.Name.Contains("lod");
        }

        public static bool ShouldSkipCollisions(Rpf6FileEntry entry) //Avoid loading useless #bd's for map viewer
        {
            return entry.Name.Contains("prop")
                || entry.Name.Contains("dlc")
                || entry.Name.Contains("_int_")
                || entry.Name.Contains("mp_")
                || entry.Parent.Parent.Name.Contains("cs_")
                || entry.Parent.Parent.Name.Contains("sw_")
                || entry.Name.Contains("flags");
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
                if (!path.EndsWith(".wvd") || ShouldSkipVisualDict(entry)) continue;

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

        public override void GetAssets(uint typeHash, List<Asset> assets)
        {
            var fragments = FileManager?.DataFileMgr?.StreamEntries[Rpf6FileExt.wft];
            if (fragments == null) return;

            foreach (var kv in fragments)
            {
                assets.Add(new RDR1MapAsset(this, kv.Key));
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

    public class RDR1MapNode : StreamingBVHItem
    {
        public BoundingBox StreamingBox { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public RDR1MapNode ParentNode;
        public RDR1BaseMapData MapData;
        public JenkHash NameHash;
        public bool Enabled;
        public bool IsPinned;
        public bool IsProjectNode;

        public RDR1MapNode(WsiFile wsi) //Buildings, props, etc...
        {
            var swTerrain = wsi.FileEntry.Archive.Name == "swterrain.rpf";
            MapData = new RDR1MapData(wsi, swTerrain);
            NameHash = wsi.FileEntry.ShortNameHash;
            BoundingBox = MapData.BoundingBox;
            StreamingBox = MapData.StreamingBox;
            Enabled = true;
        }

        public RDR1MapNode(WspFile wsp) //Trees
        {
            MapData = new RDR1MapData(wsp);
            NameHash = wsp.FileEntry.ShortNameHash;
            BoundingBox = MapData.BoundingBox;
            StreamingBox = MapData.StreamingBox;
            Enabled = true;
        }

        public RDR1MapNode(Rpf6StoreItem item) //Tiles
        {
            MapData = new RDR1TerrainMapData(item);
            NameHash = item.Hash;
            BoundingBox = MapData.BoundingBox;
            StreamingBox = MapData.StreamingBox;
            Enabled = true;
        }

        public void LoadMapData(WsiFile mapChild)
        {
            if (MapData != null)
            {
                UnloadMapData();
            }
            MapData ??= new RDR1MapData(mapChild);
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

            if (IsPinned == false)
            {
                MapData = null;
            }
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

    public class RDR1MapData : RDR1BaseMapData
    {
        private readonly Rsc6SectorInfo[] Childs; //The sub-sectors containing the props and buildings

        public RDR1MapData(WsiFile wsi, bool swTerrain = false)
        {
            var dist = new Vector3(700.0f);
            FilePack = wsi;
            FilePack.EditorObject = this; //Allow the editor access to this
            Name = wsi.Name;
            BoundingBox = wsi.StreamingItems.Bounds;
            StreamingBox = new BoundingBox(BoundingBox.Center - dist, BoundingBox.Center + dist);

            var scopedChilds = wsi?.StreamingItems?.ItemChilds.Item?.Sectors.Items?.Where(i => i != null);
            var childs = wsi?.StreamingItems?.ChildPtrs.Items?.Where(i => i != null);
            if (scopedChilds == null && childs == null)
            {
                return;
            }

            var allChilds = new List<Rsc6SectorInfo>();
            if (scopedChilds != null)
            {
                allChilds.AddRange(scopedChilds);
            }
            if (childs != null)
            {
                allChilds.AddRange(childs);
            }
            Childs = allChilds.ToArray();

            foreach (var sector in Childs)
            {
                var name = sector.Name.ToString().ToLower();
                if (name == wsi.StreamingItems.Scope.ToString()) continue;
                if (name.Contains("_ffa01x") || name.Contains("_base01x") || name.Contains("_flags01x")) continue; //Deathmatch props

                if (swTerrain)
                {
                    name += "_non-terrain";
                }

                //Add the sector as a entity
                var sectorEnt = new WsiEntity()
                {
                    LodDistMax = 750.0f,
                    ModelName = new(name),
                    Position = sector.Bounds.Center,
                    Index = Entities.Count,
                    ResetPos = true,
                    Level = this
                };
                this.Add(sectorEnt);

                if (RDR1Map.EnablePropsSetting.GetBool())
                {
                    var entities = sector.Entities.Items;
                    if (entities != null && entities.Length > 0)
                    {
                        foreach (var instance in entities)
                        {
                            var e = new WsiEntity(instance)
                            {
                                Index = Entities.Count,
                                ParentName = name,
                                Level = this
                            };
                            this.Add(e);
                        }
                    }

                    var drawableInstances = sector.DrawableInstances.Items;
                    if (drawableInstances != null && drawableInstances.Length > 0)
                    {
                        foreach (var instance in drawableInstances)
                        {
                            if (instance.Name.ToString().ToLower() == name)
                            {
                                continue;
                            }

                            var ident = Matrix4x4.Identity;
                            ident.M44 = 0.0f;
                            var global = instance.Matrix.Equals(ident);
                            var bb = new BoundingBox(instance.BoundingBoxMin.XYZ(), instance.BoundingBoxMax.XYZ());

                            var e = new WsiEntity(instance)
                            {
                                Index = Entities.Count,
                                ParentName = name,
                                Level = this,
                                ResetPos = global,
                                Position = global ? bb.Center : new Vector3()
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
                            var e = new RDR1LightEntity(light, lights.Name.ToString())
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

        public RDR1MapData(WspFile wsp)
        {
            var dist = new Vector3(700.0f);
            FilePack = wsp;
            FilePack.EditorObject = this; //Allow the editor access to this
            Name = wsp.Name;
            BoundingBox = wsp.Grid.BoundingBox;
            StreamingBox = new BoundingBox(BoundingBox.Center - dist, BoundingBox.Center + dist);

            var grid = wsp.Grid;
            foreach (var gridCell in grid?.GridCells.Items)
            {
                var pos = gridCell.CombinedInstanceListPos;
                var matrices = gridCell.CombinedInstanceListMatrix;

                //Trees
                for (int i = 0; i < pos.Items.Length; i++)
                {
                    var inst = pos.Items[i];
                    var name = grid.TreeNames.Items[inst.TreeIndex].ToString();
                    if (name == null) continue;

                    name = name.Replace(".spt", "x"); //SpeedTree to fragment name
                    var hash = new JenkHash(name);
                    var entity = RDR1GridForestEntity.CreateFromGrid(inst, hash);
                    this.Add(entity);
                }

                //Debris and foliages around buildings and roads
                for (int i = 0; i < matrices.Items.Length; i++)
                {
                    var inst = matrices.Items[i];
                    var name = grid.TreeNames.Items[inst.TreeTypeID].ToString();
                    if (name == null) continue;

                    name = name.Replace(".spt", "x"); //SpeedTree to fragment name
                    var hash = new JenkHash(name);
                    var entity = RDR1GridForestEntity.CreateFromGrid(inst, hash);
                    this.Add(entity);
                }
            }
        }
        

        public override void PreSaveUpdate()
        {
            base.PreSaveUpdate();
            UpdateEntitiesArray();
        }

        private void UpdateEntitiesArray()
        {
            var entities = new List<WsiEntity>();
            foreach (var ent in Entities)
            {
                if (ent is not WsiEntity wsiEnt) continue;
                entities.Add(wsiEnt);
            }

            //Dictionary to hold the new entities for each parent
            var parentEntitiesDict = new Dictionary<string, List<Rsc6PropInstanceInfo>>();
            foreach (var ent in entities)
            {
                if (ent.ParentName == null) continue;

                if (!parentEntitiesDict.TryGetValue(ent.ParentName, out var parentEntities))
                {
                    parentEntities = new List<Rsc6PropInstanceInfo>();
                    parentEntitiesDict[ent.ParentName] = parentEntities;
                }

                parentEntities.Add(new Rsc6PropInstanceInfo()
                {
                    EntityName = new(ent.Name),
                    RotationX = ent.RotationX,
                    RotationY = ent.RotationY,
                    RotationZ = ent.RotationZ,
                    Flags = ent.Flags,
                    AO = ent.AO,
                    ModMode = ent.ModMode,
                    NetworkingFlags = ent.NetworkingFlags,
                    RotationType = ent.RotationType,
                    EntityPosition = new Vector4(ent.Position, 0.0f)
                });
            }

            //Update each parent's entities
            foreach (var kvp in parentEntitiesDict)
            {
                var parentName = kvp.Key;
                var newEntities = kvp.Value.ToArray();
                var parent = Childs.FirstOrDefault(e => e.Name.ToString() == parentName);

                if (parent != null)
                {
                    parent.Entities = new(newEntities);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class RDR1TerrainMapData : RDR1BaseMapData
    {
        public RDR1TerrainMapData(Rpf6StoreItem item)
        {
            var dist = new Vector3(750.0f);
            var e = new WsiEntity()
            {
                ModelName = item.Hash,
                Position = item.Box.Center,
                LodLevel = 0,
                LodDistMax = 800.0f,
                Index = Entities.Count,
                Level = this
            };
            this.Add(e);

            e.LodParent = e;
            e.LodParent?.AddLodChild(e);
            BoundingBox = item.Box;
            StreamingBox = new BoundingBox(BoundingBox.Center - dist, BoundingBox.Center + dist);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public abstract class RDR1BaseMapData : Level
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public class RDR1MapAsset : Asset
    {
        public RDR1Map Map;
        public JenkHash NameHash;

        public RDR1MapAsset(RDR1Map map, JenkHash asset)
        {
            Map = map;
            Level = map;
            NameHash = asset;
            Name = JenkIndex.TryGetString(NameHash);

            if (string.IsNullOrEmpty(Name) && (NameHash != 0))
            {
                Name = "0x" + NameHash.Hex;
            }
            DelayLoad = true;
        }

        public override void DelayLoadPiece()
        {
            Map.Cache.RequestLoadAsset(this);
            Map.StreamUpdateRequest = true;
        }

        public override void LoadPiece()
        {
            var pieces = Map.Cache.GetPieces(NameHash, NameHash);
            if (pieces != null)
            {
                Piece = pieces[0];
            }
        }
    }

    public class RDR1MapFileCache : StreamingCache
    {
        public Rpf6FileManager FileManager;
        private readonly Dictionary<Rpf6FileExt, StreamingCacheDict<JenkHash>> Cache = new();
        private readonly StreamingCacheDict<JenkHash> BoundsCache;

        public RDR1MapFileCache(Rpf6FileManager fman)
        {
            FileManager = fman;
            BoundsCache = new StreamingCacheDict<JenkHash>(this);
        }

        private Dictionary<JenkHash, StreamingCacheEntry> GetCache(Rpf6FileExt ext)
        {
            if (!Cache.TryGetValue(ext, out var cache))
            {
                cache = new StreamingCacheDict<JenkHash>(this);
                Cache[ext] = cache;
            }
            return cache;
        }

        public override void Invalidate(string gamepath)
        {
            if (string.IsNullOrEmpty(gamepath)) return;

            Rpf6FileManager.GetRpf6FileHashExt(gamepath, out var hash, out var ext);
            Cache.TryGetValue(ext, out var cache);
            cache?.Remove(hash);

            if (ext == Rpf6FileExt.wbd_wcdt)
            {
                BoundsCache.Remove(hash);
            }
        }

        public override void SetPinned(string gamepath, bool pinned)
        {
            if (string.IsNullOrEmpty(gamepath)) return;

            Rpf6FileManager.GetRpf6FileHashExt(gamepath, out var hash, out var ext);
            Cache.TryGetValue(ext, out var cache);
            cache?.SetPinned(hash, pinned);

            if (ext == Rpf6FileExt.wbd_wcdt)
            {
                BoundsCache.SetPinned(hash, pinned);
            }
        }

        public override void BeginFrame()
        {
            base.BeginFrame();
            foreach (var cache in Cache)
            {
                cache.Value.RemoveOldItems();
            }
            BoundsCache.RemoveOldItems();
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

        public List<Piece> GetPieces(JenkHash packHash, JenkHash pieceHash)
        {
            if (packHash == new JenkHash())
            {
                return null;
            }

            var pack = TryGetPiecePack(packHash, out bool isFragment);
            if (pack == null)
            {
                return null;
            }
            return ExtractPieces(pack, pieceHash);
        }

        private PiecePack TryGetPiecePack(JenkHash packHash, out bool isFragment)
        {
            var pack = GetPiecePack(packHash, Rpf6FileExt.wft);
            isFragment = true;

            if (pack == null)
            {
                pack = GetPiecePack(packHash, Rpf6FileExt.wvd);
                isFragment = false;
            }
            return pack;
        }

        private List<Piece> ExtractPieces(PiecePack pack, JenkHash pieceHash)
        {
            var pieces = new List<Piece>();
            if (pack?.Pieces != null)
            {
                pack.Pieces.TryGetValue(pieceHash, out var piece);
                if (piece == null)
                {
                    if (pack.Pieces.Count == 4) //High terrain contains 4 pieces
                        pieces.AddRange(pack.Pieces.Values);
                    else
                        return null;
                }
                else
                {
                    pieces.Add(piece);
                }
            }
            return pieces.Count > 0 ? pieces : null;
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