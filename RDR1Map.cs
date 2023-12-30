using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace CodeX.Games.RDR1
{
    class RDR1Map : StreamingLevel<RDR1Game, RDR1MapFileCache>
    {
        public Rpf6FileManager FileManager;
        public Dictionary<JenkHash, Rsc6SectorInfo> WsiFiles;
        public HashSet<Entity> WspFiles;
        public Dictionary<JenkHash, PiecePack> WvdTiles;
        public Dictionary<JenkHash, Rsc6SectorInfo> CurNodes;
        public Dictionary<JenkHash, Rsc6SectorInfo> StreamNodesPrev;
        public Dictionary<JenkHash, Rsc6SectorInfo> StreamNodes;
        private WsiFile MainStreamingEntry;

        public static Setting StartPositionSetting = Settings.Register("RDR1Map.Area", SettingType.String, "Blackwater", "Change position", false, false, GetAreas());
        public static Setting OnlyTerrainSetting = Settings.Register("RDR1Map.OnlyTerrain", SettingType.Bool, false);
        public static Setting UseLowestLODSetting = Settings.Register("RDR1Map.LowLOD", SettingType.Bool, false, true);
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
                _ => new Vector3(1315.5f, 735.2f, 80f), //Blackwater
            };

            Game = game;
            StreamPosition = DefaultSpawnPoint = position;
            BoundingBox = new BoundingBox(new Vector3(-100000.0f), new Vector3(100000.0f));
            InitPhysicsSim();
        }

        protected override bool StreamingInit()
        {
            FileManager = Game.GetFileManager() as Rpf6FileManager;
            if (FileManager.Init() == false)
            {
                throw new Exception("Failed to initialize RDR1");
            }
            FileManager.Clear();
            FileManager.InitArchives();
            Core.Engine.Console.Write("RDR1Map", "Initialising " + FileManager.Game.Name + " terrain...");

            var dfm = FileManager.DataFileMgr;
            Cache = new RDR1MapFileCache(FileManager);
            CurNodes = new Dictionary<JenkHash, Rsc6SectorInfo>();
            StreamNodesPrev = new Dictionary<JenkHash, Rsc6SectorInfo>();
            StreamNodes = new Dictionary<JenkHash, Rsc6SectorInfo>();
            WsiFiles = new Dictionary<JenkHash, Rsc6SectorInfo>();
            MainStreamingEntry = dfm.WsiFiles.Values.First(item => item.Name == "swall.wsi"); //Getting data from the main sagSectorInfo

            LoadTiles(dfm);
            //LoadTrees(dfm);

            Core.Engine.Console.Write("RDR1Map", FileManager.Game.Name + " terrain initialised.");
            if (!OnlyTerrainSetting.GetBool())
                Core.Engine.Console.Write("RDR1Map", "Initialising " + FileManager.Game.Name + " objects...");
            else
                Core.Engine.Console.Write("RDR1Map", FileManager.Game.Name + " map initialised.");
            return true;
        }

        private static string[] GetAreas()
        {
            return new[] { "Armadillo", "Blackwater", "Chuparosa", "Cochinay", "El Presidio", "El Matadero", "Escalera", "Fort Mercer", "Gaptooth Breach", "Las Hermanas", "MacFarlane's Ranch", "Manzanita Post", "Nosalida", "Pacific Union RR Camp", "Plainview", "Rathskeller Fork", "Ridgewood Farm", "Tesoro Azul", "Thieve's Landing", "Torquemada", "Tumbleweed", "Twin Rocks" };
        }

        private bool ShouldSkipObject(Rpf6FileEntry entry)
        {
            return entry.Name.Contains("non-terrain")
                || entry.Name.StartsWith("mp_")
                || entry.Name.Contains("low")
                || entry.Name.Contains("med")
                || entry.Name.Contains("lod");
        }

        private bool ShouldSkipLOD(GameArchiveDirectory parent)
        {
            return (UseLowestLODSetting.GetBool() ? parent.Name.StartsWith("resource_0") : parent.Name.StartsWith("resource_1"))
                || parent.Name.StartsWith("resource_2")
                || parent.Name.StartsWith("resource_3")
                || parent.Name.StartsWith("resource_4");
        }

        private bool IsObjectProp(string obj)
        {
            return obj.Contains("p_gen") || (obj.StartsWith("p_") && obj.EndsWith("x"));
        }

        private void LoadSectors(Rpf6DataFileMgr dfm)
        {
            for (int i = 0; i < MainStreamingEntry.StreamingItems.ItemMapChilds.Count; i++)
            {
                var child = MainStreamingEntry.StreamingItems.ItemMapChilds[i];
                if (child.SectorName.StartsWith("cs") || child.SectorName.StartsWith("sw") || child.SectorName.StartsWith("dlc"))
                    continue;

                var wsiConverted = dfm.WsiFiles.Values.First(item => item.Name.Contains(child.SectorName.ToLowerInvariant()));
                if (wsiConverted == null)
                    continue;

                if (child.SectorBounds.Contains(StreamPosition) == ContainmentType.Contains)
                {
                    if (WsiFiles.ContainsValue(wsiConverted.StreamingItems))
                    {
                        continue;
                    }

                    var childEntry = dfm.WsiFiles.Values.First(item => item.Name == (child.SectorName + ".wsi").ToLower());
                    for (int f = 0; f < childEntry.StreamingItems.ItemChilds.Item.Sectors.Count; f++)
                    {
                        var sectorItem = childEntry.StreamingItems.ItemChilds.Item.Sectors[f];
                        var sectorItemName = sectorItem.Name.Value.ToLower();

                        if (sectorItemName != childEntry.Name.Replace(".wsi", "") || sectorItemName.Contains("flags") || sectorItemName.Contains("props") || sectorItemName.StartsWith("mp_"))
                            continue;

                        var fe = dfm.TryGetStreamEntry(JenkHash.GenHash(sectorItemName), Rpf6FileExt.wvd);
                        if (fe == null || CurNodes.ContainsKey(fe.ShortNameHash))
                            continue;

                        wsiConverted.BoundingBox = child.SectorBounds;
                        wsiConverted.StreamingBox = child.SectorBounds;
                        WsiFiles.Add(fe.NameHash, wsiConverted.StreamingItems);
                    }
                }
                else if (WsiFiles.ContainsValue(wsiConverted.StreamingItems))
                {
                    Rpf6Crypto.RemoveDictValue(WsiFiles, wsiConverted.StreamingItems);
                }
            }
        }

        private void LoadTiles(Rpf6DataFileMgr dfm)
        {
            WvdTiles = new Dictionary<JenkHash, PiecePack>();
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wvd, out var files);

            foreach (var file in files)
            {
                var entry = file.Value;
                if (!file.Key.Str.StartsWith("tile_") || ShouldSkipObject(entry) || ShouldSkipLOD(entry.Parent))
                    continue;

                var piece = Cache.GetPiece(file.Key.Str, file.Key);
                if (piece.Item2 != null)
                {
                    WvdTiles.Add(file.Key, piece.Item2);
                }
            }
        }

        private void LoadTrees(Rpf6DataFileMgr dfm)
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
            var ents = StreamEntities.CurrentSet;
            var spos = StreamPosition;
            var nodes = StreamNodesPrev;
            StreamNodesPrev = StreamNodes;

            nodes.Clear();
            CurNodes.Clear();
            ents.Clear();
            LoadSectors(FileManager.DataFileMgr);

            foreach (var kvp in WsiFiles)
            {
                var wsi = kvp.Value;
                if (wsi.StreamingBox.Contains(ref spos) == ContainmentType.Contains)
                {
                    CurNodes[kvp.Key] = wsi;
                }

                foreach (var tile in WvdTiles)
                {
                    var v = tile.Value;
                    var joinedTiles = v.Pieces.Values;
                    var averageX = joinedTiles.Select(bb => bb.BoundingBox.Center.X).Average();
                    var averageY = joinedTiles.Select(bb => bb.BoundingBox.Center.Y).Average();
                    var averageZ = joinedTiles.Select(bb => bb.BoundingBox.Center.Z).Average();

                    if ((wsi.StreamingBox.Contains(new Vector3(averageX, averageY, averageZ)) == ContainmentType.Contains) && !ents.Any(i => i.Name == tile.Key.Str))
                    {
                        foreach (var piece in v.Pieces)
                        {
                            if (piece.Value == null) continue;
                            var entity = new Entity
                            {
                                Piece = piece.Value,
                                StreamingDistance = 1000.0f, //mmh
                                BoundingSphere = piece.Value.BoundingSphere,
                                BoundingBox = piece.Value.BoundingBox,
                                Position = piece.Value.BoundingBox.Center,
                                Name = tile.Key.Str,
                            };
                            ents.Add(entity);
                        }
                    }
                }
            }

            if (!OnlyTerrainSetting.GetBool())
            {
                foreach (var kvp in CurNodes)
                {
                    var node = kvp.Value;
                    if (node.ItemChilds.Item != null)
                    {
                        node.RootEntities ??= new List<WsiEntity>();
                        node.RootEntities.Clear();

                        for (int i = 0; i < node.ItemChilds.Item.Sectors.Count; i++)
                        {
                            var child = node.ItemChilds.Item.Sectors[i];
                            var lights = child.PlacedLightsGroup.Item;

                            foreach (var ent in child.Entities.Items) //Sectors childs's childs and props
                            {
                                var childEntity = new WsiEntity(ent);
                                node.RootEntities.Add(childEntity);
                            }

                            if (lights != null) //Lights
                            {
                                foreach (var light in lights.Lights.Items)
                                {
                                    var lightEntity = new WsiEntity(light, lights.Name.Value);
                                    node.RootEntities.Add(lightEntity);
                                }
                            }

                            var entity = new WsiEntity(child.Name.Value.ToLower());
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

            foreach (var ent in ents) //Make sure all assets are loaded
            {
                var objEntity = ent as WsiEntity;
                var upd = ent.Piece == null;

                if ((ent.Position == Vector3.Zero || upd) && objEntity != null) //Buildings, props and lights
                {
                    JenkHash name = objEntity.ModelName;
                    ent.Piece = Cache.GetPiece(name.Str, name, IsObjectProp(name.Str)).Item1;
                }

                if (upd && ent.Piece != null)
                {
                    ent.EnsurePieceLightInstances();
                    ent.UpdateBounds();
                }
            }

            foreach (var ent in ents) //Update lights
            {
                if (ent is WsiEntity objEntity && objEntity.Lights != null && objEntity.ParentName != string.Empty)
                {
                    var parent = ents.FirstOrDefault(e => e.Position == objEntity.ParentPosition);
                    if (parent != null && parent.Position != Vector3.Zero && parent.Piece.Name.StartsWith("p_gen"))
                    {
                        ent.SetPiece(parent.Piece);
                        ent.BoundingBox = parent.BoundingBox;
                        ent.BoundingSphere = parent.BoundingSphere;
                    }
                }
            }
            //ents.UnionWith(WspFiles);

            NodeCountStat.SetCounter(nodes.Count);
            EntityCountStat.SetCounter(ents.Count);
            StreamNodes = nodes;
            StreamUpdateRequest = true;
            StreamUpdate = true;

            return true;
        }

        public override void GetEntities(SceneViewProjection proj)
        {
            proj.Clear();
            var isScreen = proj.View.Type == SceneViewType.Screen;
            var isShadow = proj.View.Type == SceneViewType.ShadowMap;
            var ents = proj.View.ViewEntities ?? StreamEntities.ActiveSet;

            if (ents != null)
            {
                foreach (var ent in ents)
                {
                    if (ent == null)
                        continue;
                    if (ent.BoundingSphere.Radius < proj.MinEntitySize)
                        continue;
                    if (ent.BoundingBox.Minimum == ent.BoundingBox.Maximum)
                        continue;

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
    }

    public class RDR1MapFileCache : StreamingCache
    {
        public Rpf6FileManager FileManager;
        private Dictionary<Rpf6FileExt, Dictionary<JenkHash, StreamingCacheEntry>> Cache = new Dictionary<Rpf6FileExt, Dictionary<JenkHash, StreamingCacheEntry>>();
        
        private Dictionary<JenkHash, StreamingCacheEntry> GetCache(Rpf6FileExt ext)
        {
            if (!Cache.TryGetValue(ext, out Dictionary<JenkHash, StreamingCacheEntry> cache))
            {
                cache = new Dictionary<JenkHash, StreamingCacheEntry>();
                Cache[ext] = cache;
            }
            return cache;
        }

        private List<JenkHash> RemoveItems = new List<JenkHash>();

        public RDR1MapFileCache(Rpf6FileManager fman)
        {
            FileManager = fman;
        }

        public override void BeginFrame()
        {
            base.BeginFrame();
            foreach (var cache in Cache)
            {
                RemoveItems.Clear();
                var dict = cache.Value;

                foreach (var kvp in dict)
                {
                    var item = kvp.Value;
                    if ((CurrentFrame - item.LastUseFrame) > CacheFrameCount)
                    {
                        RemoveItems.Add(kvp.Key);
                    }
                }

                foreach (var remitem in RemoveItems)
                {
                    var dr = dict[remitem];
                    dr.Object = null;
                    dict.Remove(remitem);
                }
            }
        }

        public void RemoveFromCache(JenkHash hash)
        {
            foreach (var cache in Cache)
            {
                var dict = cache.Value;
                dict.Remove(hash);
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
                        var piecePack = FileManager.LoadPiecePack(entry);
                        FileManager.LoadDependencies(piecePack);
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
            if (string.IsNullOrEmpty(fileContainer))
                return (null, null);

            Piece piece = null;
            var hash = new JenkHash(fragment ? RemoveFileExtension(fileContainer, ".wft") : RemoveFileExtension(fileContainer, ".wvd"));
            var pack = GetPiecePack(hash, fragment ? Rpf6FileExt.wft : Rpf6FileExt.wvd);

            if (pack?.Pieces != null)
            {
                pack.Pieces.TryGetValue(entityHash, out piece);
            }
            return (piece, pack);
        }

        private string RemoveFileExtension(string fileName, string extension)
        {
            return fileName.EndsWith(extension) ? fileName.Replace(extension, "") : fileName;
        }
    }
}
