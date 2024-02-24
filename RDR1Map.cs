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

namespace CodeX.Games.RDR1
{
    class RDR1Map : StreamingLevel<RDR1Game, RDR1MapFileCache>
    {
        public Rpf6FileManager FileManager;
        public List<LevelBVHItem> MapNodes;
        public Dictionary<JenkHash, Rsc6SectorInfo> WsiFiles;
        public HashSet<Entity> WspFiles;
        public Dictionary<JenkHash, PiecePack> WvdTiles;
        public Dictionary<JenkHash, Rsc6SectorInfo> StreamNodesPrev;
        public Dictionary<JenkHash, Rsc6SectorInfo> StreamNodes;
        private WsiFile MainStreamingEntry, TerrainStreamingEntry;

        public static Setting StartPositionSetting = Settings.Register("RDR1Map.Area", SettingType.String, "Blackwater", "Change position", false, false, GetAreas());
        public static Setting OnlyTerrainSetting = Settings.Register("RDR1Map.OnlyTerrain", SettingType.Bool, false);
        public static Setting NoPropsSetting = Settings.Register("RDR1Map.NoProps", SettingType.Bool, true);
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
                "Torquemada" => new Vector3(3456.4f, -370.5f, 80.7f),
                "Tumbleweed" => new Vector3(2950.4f, -3941.5f, 32.6f),
                "Twin Rocks" => new Vector3(2141.6f, -2430.9f, 28.0f),
                _ => new Vector3(1234.6f, 736.2f, 84.7f), //Blackwater by default
            };

            Game = game;
            StreamPosition = DefaultSpawnPoint = position;
            BoundingBox = new BoundingBox(new Vector3(-100000.0f), new Vector3(100000.0f));
            InitPhysicsSim();
        }

        protected override bool StreamingInit()
        {
            Core.Engine.Console.Write("RDR1Map", "Initialising " + Game.Name + " terrain...");
            FileManager = Game.GetFileManager() as Rpf6FileManager;

            if (FileManager.Init() == false)
            {
                throw new Exception("Failed to initialize RDR1.");
            }

            FileManager.Clear();
            FileManager.InitArchives();

            var dfm = FileManager.DataFileMgr;
            Cache = new RDR1MapFileCache(FileManager);
            StreamNodesPrev = new Dictionary<JenkHash, Rsc6SectorInfo>();
            StreamNodes = new Dictionary<JenkHash, Rsc6SectorInfo>();
            WsiFiles = new Dictionary<JenkHash, Rsc6SectorInfo>();
            MainStreamingEntry = dfm.WsiFiles.Values.First(item => item.Name == "swall.wsi"); //swAll
            TerrainStreamingEntry = dfm.WsiFiles.Values.First(item => item.Name == "swterrain.wsi"); //swTerrain

            MapNodes = new List<LevelBVHItem>();
            for (int i = 0; i < MainStreamingEntry.StreamingItems.ItemMapChilds.Count; i++)
            {
                var child = MainStreamingEntry.StreamingItems.ItemMapChilds[i];
                if (child.SectorName.StartsWith("cs") || child.SectorName.StartsWith("sw") || child.SectorName.StartsWith("dlc"))
                    continue;

                var node = new RDR1MapNode(child);
                MapNodes.Add(node);
            }

            BVH = new LevelBVH();
            BVH.Init(MapNodes);

            LoadTiles(dfm);

            Core.Engine.Console.Write("RDR1Map", FileManager.Game.Name + " terrain initialised.");
            if (!OnlyTerrainSetting.GetBool())
                Core.Engine.Console.Write("RDR1Map", "Initialising " + FileManager.Game.Name + " objects...");
            else
                Core.Engine.Console.Write("RDR1Map", FileManager.Game.Name + " map initialised.");
            return true;
        }

        public static string[] GetAreas()
        {
            return new[] { "Armadillo", "Blackwater", "Chuparosa", "Cochinay", "El Presidio", "El Matadero", "Escalera", "Fort Mercer", "Gaptooth Breach", "Las Hermanas", "MacFarlane's Ranch", "Manzanita Post", "Nosalida", "Pacific Union RR Camp", "Plainview", "Rathskeller Fork", "Ridgewood Farm", "Tesoro Azul", "Thieve's Landing", "Torquemada", "Tumbleweed", "Twin Rocks" };
        }

        public bool ShouldSkipObject(Rpf6FileEntry entry)
        {
            return entry.Name.Contains("non-terrain")
                || entry.Name.StartsWith("mp_")
                || entry.Name.Contains("low")
                || entry.Name.Contains("med")
                || entry.Name.Contains("lod");
        }

        public bool IsObjectProp(string obj)
        {
            return obj.Contains("p_gen") || (obj.StartsWith("p_") && obj.EndsWith("x"));
        }

        public void LoadSubsectors(Rpf6DataFileMgr dfm, LevelBVHItem[] mapNodes)
        {
            WsiFiles.Clear();
            for (int i = 0; i < mapNodes.Length; i++)
            {
                var mc = ((RDR1MapNode)mapNodes[i]).MapNode;
                if (mc.SectorName.StartsWith("cs") || mc.SectorName.StartsWith("sw") || mc.SectorName.StartsWith("dlc"))
                    continue;

                var wsiConverted = dfm.WsiFiles.Values.First(item => item.Name.Contains(mc.SectorName.ToLowerInvariant()));
                if (wsiConverted == null)
                    continue;

                var childEntry = dfm.WsiFiles.Values.First(item => item.Name == (mc.SectorName + ".wsi").ToLower());
                for (int f = 0; f < childEntry.StreamingItems.ItemChilds.Item?.Sectors.Count; f++)
                {
                    var sectorItem = childEntry.StreamingItems.ItemChilds.Item?.Sectors[f];
                    var sectorItemName = sectorItem.Name.Value.ToLower();

                    if (sectorItemName != childEntry.Name.Replace(".wsi", "")
                        || sectorItemName.Contains("flags")
                        || sectorItemName.Contains("props")
                        || sectorItemName.StartsWith("mp_"))
                        continue;

                    var fe = dfm.TryGetStreamEntry(JenkHash.GenHash(sectorItemName), Rpf6FileExt.wvd);
                    if (fe == null) continue;

                    WsiFiles.Add(fe.NameHash, wsiConverted.StreamingItems);
                }
            }
        }

        public void LoadTiles(Rpf6DataFileMgr dfm)
        {
            WvdTiles = new Dictionary<JenkHash, PiecePack>();
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wvd, out var files);

            foreach (var file in files)
            {
                var entry = file.Value;
                if (!entry.Name.StartsWith("tile_") || ShouldSkipObject(entry))
                    continue;

                var piece = Cache.GetPiece(file.Key.Str, file.Key);
                if (piece.Item2 != null)
                {
                    WvdTiles.Add(file.Key, piece.Item2);
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
                        var piece = Cache.GetPiece(treeFragment.Key.Str, treeFragment.Key, true);

                        if (piece.Item2 != null)
                        {
                            foreach (var p in piece.Item2.Pieces)
                            {
                                if (p.Value == null) continue;
                                var entity = new Entity
                                {
                                    Piece = p.Value,
                                    StreamingDistance = 100.0f, //mmh
                                    Position = instancePositions.Position,
                                    Name = treeFragment.Key.Str,
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

            var ents = StreamEntities.CurrentSet;
            var spos = StreamPosition;
            var nodes = StreamNodesPrev;

            StreamNodesPrev = StreamNodes;
            nodes.Clear();

            if (!OnlyTerrainSetting.GetBool())
            {
                var curNodes = BVH.GetItems(ref spos);
                LoadSubsectors(FileManager.DataFileMgr, curNodes.ToArray());

                foreach (var kvp in WsiFiles)
                {
                    var wsi = kvp.Value;
                    nodes[kvp.Key] = wsi;
                }
            }

            foreach (var tile in WvdTiles)
            {
                var v = tile.Value;
                var joinedTiles = v.Pieces.Values;
                var highLod = joinedTiles.Count == 4;
                var lowLod = joinedTiles.Count < 4;
                var tileCenter = Vector3.Zero;
                var count = 0;

                foreach (var bb in joinedTiles)
                {
                    if (bb != null)
                    {
                        tileCenter += bb.BoundingBox.Center;
                        count++;
                    }
                }

                var listed = ents.Any(i => i.Name == v.Piece.Name);
                var distance = Vector3.Distance(spos, tileCenter / count);

                if (!listed)
                {
                    if ((distance < 1300.0f) && highLod)
                        CreateMapEntity(v, ents, v.Piece.Name, 1300.0f);
                    else if (distance >= 1100.0f && lowLod)
                        CreateMapEntity(v, ents, v.Piece.Name, 9999.0f);
                }
                else
                {
                    ents.RemoveWhere(i => i.Name == tile.Key.Str);
                }
            }

            if (!OnlyTerrainSetting.GetBool())
            {
                foreach (var kvp in nodes)
                {
                    var key = kvp.Key.Str;
                    var node = kvp.Value;
                    if (node.ItemChilds.Item != null) //swAll
                    {
                        node.RootEntities ??= new List<WsiEntity>();
                        node.RootEntities.Clear();

                        //Sector childs and lights
                        for (int i = 0; i < node.ItemChilds.Item.Sectors.Count; i++)
                        {
                            var child = node.ItemChilds.Item.Sectors[i];
                            var lights = child.PlacedLightsGroup.Item;
                            var bbCenter = child.Bounds.Center;

                            var childName = child.Name.Value.ToLower();
                            if (NoPropsSetting.GetBool()
                                && (childName == node.Scope.Value
                                || childName.Contains("flags")
                                || childName.Contains("props")
                                || childName.StartsWith("mp_"))) continue;

                            if (!(child.StreamingBox.Contains(ref spos) == ContainmentType.Contains)) continue;
                            foreach (var ent in child.Entities.Items)
                            {
                                var childEntity = new WsiEntity(ent);
                                node.RootEntities.Add(childEntity);
                            }

                            if (lights != null)
                            {
                                foreach (var light in lights.Lights.Items)
                                {
                                    var lightEntity = new WsiEntity(light, lights.Name.Value);
                                    node.RootEntities.Add(lightEntity);
                                }
                            }

                            var entity = new WsiEntity(childName);
                            if (!node.RootEntities.Any(e => e.Name == entity.Name))
                            {
                                node.RootEntities.Add(entity);
                            }
                        }
                    }

                    if (node.RootEntities != null)
                    {
                        foreach (WsiEntity entity in node.RootEntities)
                        {
                            if (ents.FirstOrDefault(e => e.Name == entity.Name) == null)
                            {
                                ents.Add(entity);
                            }

                            if (entity.Lights != null)
                            {
                                var disable = (TimeOfDay > 6.75f) && (TimeOfDay < 19.25f);
                                foreach (var light in entity.Lights)
                                {
                                    light.Params.Disabled = disable;
                                }
                            }

                            entity.StreamingDistance = (entity.Position - spos).Length();
                            entity.CurrentDistance = Math.Max(entity.StreamingDistance, 0);

                            if (entity.BoundingBox.Contains(ref spos) == ContainmentType.Contains)
                            {
                                entity.CurrentDistance = 0.0f;
                            }
                        }
                    }
                }
            }

            var needsAnotherUpdate = AddExtraStreamEntities();
            foreach (var ent in ents) //Make sure all assets are loaded
            {
                var upd = ent.Piece == null;
                if ((ent.Position == Vector3.Zero || upd) && ent is WsiEntity objEntity) //Buildings, props and lights
                {
                    JenkHash name = objEntity.ModelName;
                    ent.Piece = Cache.GetPiece(name.Str, name, IsObjectProp(name.Str)).Item1;

                    if (upd && ent.Piece != null)
                    {
                        ent.EnsurePieceLightInstances();
                        ent.UpdateBounds();
                    }
                }
            }

            foreach (var ent in ents) //Update lights
            {
                if (ent is WsiEntity objEntity && objEntity.Lights != null && objEntity.ParentName != string.Empty)
                {
                    var parent = ents.FirstOrDefault(e => e.Position == objEntity.ParentPosition);
                    if (parent != null && parent.Position != Vector3.Zero && parent.Piece.Name.StartsWith("p_gen"))
                    {
                        var scale = new Vector3(500.0f);
                        ent.SetPiece(parent.Piece);
                        ent.BoundingBox = new BoundingBox(parent.BoundingBox.Minimum - scale, parent.BoundingBox.Maximum + scale);
                        ent.BoundingSphere = new BoundingSphere(ent.BoundingBox.Center, ent.BoundingBox.Size.Length() * 0.5f);
                    }
                }
            }

            NodeCountStat.SetCounter(nodes.Count);
            EntityCountStat.SetCounter(ents.Count);
            StreamNodes = nodes;
            if (needsAnotherUpdate) StreamUpdateRequest = true;

            return true;
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
                        if (!isShadow && ent.Lights != null)
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

        public void CreateMapEntity(PiecePack pack, HashSet<Entity> ents, string name, float lodDist)
        {
            foreach (var piece in pack.Pieces)
            {
                if (piece.Value == null) continue;
                var entity = new Entity
                {
                    Piece = piece.Value,
                    LodLevel = (lodDist > 2000.0f) ? 1 : 0,
                    StreamingDistance = lodDist, //mmh
                    BoundingSphere = piece.Value.BoundingSphere,
                    BoundingBox = piece.Value.BoundingBox,
                    Position = piece.Value.BoundingBox.Center,
                    Name = name,
                };
                ents.Add(entity);
            }
        }
    }

    public class RDR1MapNode : LevelBVHItem
    {
        public Rsc6SectorChild MapNode;
        public JenkHash NameHash;

        public RDR1MapNode(Rsc6SectorChild mapChild)
        {
            MapNode = mapChild;
            NameHash = new(mapChild.SectorName.ToLower());
            StreamingBox = mapChild.SectorBounds;
            BoundingBox = mapChild.SectorBounds;
        }

        public override string ToString()
        {
            return NameHash.ToString();
        }
    }

    public class RDR1MapFileCache : StreamingCache
    {
        public Rpf6FileManager FileManager;
        private readonly Dictionary<Rpf6FileExt, Dictionary<JenkHash, StreamingCacheEntry>> Cache = new();
        private readonly List<JenkHash> RemoveItems = new();

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

        public (Piece, PiecePack) GetPiece(string fileContainer, JenkHash entityHash, bool fragment = false)
        {
            if (string.IsNullOrEmpty(fileContainer)) return (null, null);
            var pack = GetPiecePack(new JenkHash(fileContainer), fragment ? Rpf6FileExt.wft : Rpf6FileExt.wvd);

            Piece piece = null;
            if (pack?.Pieces != null)
            {
                pack.Pieces.TryGetValue(entityHash, out piece);
            }
            return (piece, pack);
        }
    }
}
