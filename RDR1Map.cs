using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CodeX.Games.RDR1
{
    class RDR1Map : StreamingLevel<RDR1Game, RDR1MapFileCache>
    {
        public Rpf6FileManager FileManager;
        public Dictionary<JenkHash, Rsc6SectorInfo> WsiFiles;
        public Dictionary<JenkHash, PiecePack> WvdTiles;
        public Dictionary<JenkHash, Rsc6SectorInfo> CurNodes;
        public Dictionary<JenkHash, Rsc6SectorInfo> StreamNodesPrev;
        public Dictionary<JenkHash, Rsc6SectorInfo> StreamNodes;
        private WsiFile MainStreamingEntry;

        public static Setting StartPositionSetting = Settings.Register("RDR1Map.StartPosition", SettingType.Vector3, new Vector3(3000f, -2150f, 500f)); //Around Armadillo
        public static Setting EnableBuildingsAndProps = Settings.Register("RDR1Map.EnableBuildingsAndProps", SettingType.Bool, false);
        public static Setting UseLowestLOD = Settings.Register("RDR1Map.UseLowestLOD", SettingType.Bool, true, true);
        public Statistic NodeCountStat = Statistics.Register("RDR1Map.NodeCount", StatisticType.Counter);
        public Statistic EntityCountStat = Statistics.Register("RDR1Map.EntityCount", StatisticType.Counter);

        public RDR1Map(RDR1Game game) : base("RDR1 Map Level")
        {
            Game = game;
            StreamPosition = DefaultSpawnPoint = StartPositionSetting.GetVector3();
            BoundingBox = new BoundingBox(new Vector3(-100000.0f), new Vector3(100000.0f));
        }

        private void LoadSectors(Rpf6DataFileMgr dfm)
        {
            WsiFiles = new Dictionary<JenkHash, Rsc6SectorInfo>();
            if (MainStreamingEntry == null)
            {
                MainStreamingEntry = dfm.WsiFiles.Values.First(item => item.Name == "swall.wsi"); //Getting data from the main map parent xsi
            }

            for (int i = 0; i < MainStreamingEntry.StreamingItems.ItemMapChilds.Count; i++)
            {
                var child = MainStreamingEntry.StreamingItems.ItemMapChilds[i];
                if (child.SectorName.StartsWith("cs") || child.SectorName.StartsWith("sw"))
                    continue;

                var xsiConverted = dfm.WsiFiles.Values.First(item => item.Name.Contains(child.SectorName.ToLowerInvariant()));
                if (xsiConverted == null)
                    continue;

                var sectorBounds = child.SectorBounds.GetCorners();
                if (sectorBounds[0].X - 1000f <= StreamPosition.X && StreamPosition.X <= sectorBounds[6].X + 1000f && sectorBounds[0].Y + 1000f >= StreamPosition.Y && StreamPosition.Y >= sectorBounds[6].Y - 1000f)
                {
                    var childEntry = dfm.WsiFiles.Values.First(item => item.Name == (child.SectorName + ".wsi").ToLower());
                    for (int f = 0; f < childEntry.StreamingItems.ItemChilds.Item.Sectors.Count; f++)
                    {
                        var sectorItem = childEntry.StreamingItems.ItemChilds.Item.Sectors[f];
                        var sectorItemName = sectorItem.Name.Value.ToLower();

                        if (sectorItemName.Contains("flags") || sectorItemName.Contains("props") || sectorItemName.StartsWith("mp_") || sectorItemName != childEntry.Name.Replace(".wsi", ""))
                            continue;

                        var fe = dfm.TryGetStreamEntry(JenkHash.GenHash(sectorItemName), Rpf6FileExt.wvd);
                        if (fe == null)
                            continue;
                        if (CurNodes.ContainsKey(fe.ShortNameHash))
                            continue;

                        xsiConverted.BoundingBox = child.SectorBounds;
                        xsiConverted.StreamingBox = child.SectorBounds;
                        WsiFiles.Add(fe.NameHash, xsiConverted.StreamingItems);
                    }
                }
            }
        }

        private bool ShouldSkipEntry(Rpf6FileEntry entry)
        {
            return entry.Name.Contains("non-terrain")
                || entry.Name.StartsWith("mp_")
                || entry.Name.Contains("low")
                || entry.Name.Contains("med")
                || entry.Name.Contains("lod");
        }

        private bool ShouldSkipParent(GameArchiveDirectory parent)
        {
            return parent.Name.StartsWith("resource_2")
                || parent.Name.StartsWith("resource_3")
                || parent.Name.StartsWith("resource_4");
        }

        private void LoadTiles(Rpf6DataFileMgr dfm)
        {
            WvdTiles = new Dictionary<JenkHash, PiecePack>();
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wvd, out var files);

            foreach (var file in files)
            {
                var entry = file.Value;

                if (ShouldSkipEntry(entry))
                    continue;

                if (ShouldSkipParent(entry.Parent))
                    continue;

                if (UseLowestLOD.GetBool())
                {
                    if (entry.Parent.Name.StartsWith("resource_0"))
                        continue;
                }
                else if (entry.Parent.Name.StartsWith("resource_1"))
                    continue;

                if (!file.Key.Str.StartsWith("tile_"))
                    continue;

                var piece = Cache.GetPiece(file.Key.Str, file.Key);
                if (piece.Item2 != null)
                {
                    WvdTiles.Add(file.Key, piece.Item2);
                }
            }
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
            Core.Engine.Console.Write("RDR1Map", "Initialising " + FileManager.Game.Name + " map...");

            var dfm = FileManager.DataFileMgr;
            Cache = new RDR1MapFileCache(FileManager);
            CurNodes = new Dictionary<JenkHash, Rsc6SectorInfo>();
            StreamNodesPrev = new Dictionary<JenkHash, Rsc6SectorInfo>();
            StreamNodes = new Dictionary<JenkHash, Rsc6SectorInfo>();

            LoadSectors(dfm);
            LoadTiles(dfm);

            Core.Engine.Console.Write("RDR1Map", FileManager.Game.Name + " map initialised.");
            return true;
        }

        protected override bool StreamingUpdate()
        {
            LoadSectors(FileManager.DataFileMgr);
            var ents = StreamEntities.CurrentSet;
            var spos = StreamPosition;
            var nodes = StreamNodesPrev;
            StreamNodesPrev = StreamNodes;

            nodes.Clear();
            CurNodes.Clear();

            foreach (var kvp in WsiFiles)
            {
                var wvd = kvp.Value;
                if (wvd.StreamingBox.Contains(ref spos) == ContainmentType.Contains)
                {
                    CurNodes[kvp.Key] = wvd;
                }

                foreach (var tile in WvdTiles)
                {
                    var v = tile.Value;
                    if ((wvd.StreamingBox.Contains(v.Pieces.Values.First().BoundingBox.Center) == ContainmentType.Contains) && !ents.Any(i => i.Name == tile.Key.Str))
                    {
                        foreach (var piece in v.Pieces)
                        {
                            var entity = new Entity
                            {
                                Piece = piece.Value,
                                StreamingDistance = 1000f, //mmh
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

            foreach (var kvp in CurNodes)
            {
                var node = kvp.Value;
                while (node != null)
                {
                    if (nodes.ContainsKey(node.ScopedNameHash))
                        break;
                    nodes[node.ScopedNameHash] = node;
                }
            }

            if (EnableBuildingsAndProps.GetBool())
            {
                foreach (var kvp in CurNodes)
                {
                    var node = kvp.Value;
                    if (node.ItemChilds.Item != null)
                    {
                        for (int i = 0; i < node.ItemChilds.Item.Sectors.Count; i++)
                        {
                            var sector = node.ItemChilds.Item.Sectors[i];
                            var fe = FileManager.DataFileMgr.TryGetStreamEntry(JenkHash.GenHash(sector.Name.Value.ToLower()), Rpf6FileExt.wvd);

                            if (fe == null)
                                continue;

                            if (node.RootEntities == null)
                                node.RootEntities = new List<WsiEntity>();

                            foreach (var se in sector.Entities.Items)
                            {
                                var sectorEntity = new WsiEntity(se);
                                node.RootEntities.Add(sectorEntity);
                            }
                            var entity = new WsiEntity
                            {
                                ModelName = JenkHash.GenHash(sector.Name.Value.ToLower())
                            };
                            if (!node.RootEntities.Any(e => e.Name == entity.Name))
                                node.RootEntities.Add(entity);
                        }
                    }
                    if (node.RootEntities != null)
                    {
                        foreach (WsiEntity entity in node.RootEntities)
                        {
                            if (!ents.Any(e => e.Name == entity.Name))
                                ents.Add(entity);
                            RecurseAddStreamEntity(entity, ref spos, ents);
                        }
                    }
                }
            }

            var needsAnotherUpdate = AddExtraStreamEntities();
            foreach (var ent in ents) //Make sure all assets are loaded
            {
                var upd = ent.Piece == null;
                if (ent.Position == Vector3.Zero || upd) //Buildings and props
                {
                    JenkHash name = ((WsiEntity)ent).ModelName;
                    bool prop = name.Str.Contains("p_gen") || (name.Str.StartsWith("p_") && name.Str.EndsWith("x"));
                    ent.Piece = Cache.GetPiece(name.Str, name, prop).Item1;
                }

                if (upd && (ent.Piece != null))
                {
                    ent.EnsurePieceLightInstances();
                    ent.UpdateBounds();
                }
            }

            NodeCountStat.SetCounter(nodes.Count);
            EntityCountStat.SetCounter(ents.Count);

            StreamNodes = nodes;
            if (needsAnotherUpdate)
                StreamUpdate = true;
            return true;
        }

        private void RecurseAddStreamEntity(WsiEntity e, ref Vector3 spos, HashSet<Entity> ents)
        {
            if (e.Lights != null)
            {
                var disable = (TimeOfDay > 6.75f) && (TimeOfDay < 19.25f);
                foreach (var light in e.Lights)
                {
                    light.Params.Disabled = disable;
                }
            }

            e.StreamingDistance = (e.Position - spos).Length();
            e.CurrentDistance = Math.Max(e.StreamingDistance, 0);

            if (e.BoundingBox.Contains(ref spos) == ContainmentType.Contains)
            {
                e.CurrentDistance = 0.0f;
            }

            if (e.StreamingDistance < e.LodDistMax)
            {
                if ((e.LodChildren != null) && (e.StreamingDistance < e.LodDistMin))
                {
                    foreach (WsiEntity c in e.LodChildren.Cast<WsiEntity>())
                    {
                        RecurseAddStreamEntity(c, ref spos, ents);
                    }
                }
                else
                {
                    ents.Add(e);
                }
            }
            else if ((e.LodParent != null) && (e.LodParent.StreamingDistance < e.LodParent.LodDistMin))
            {
                ents.Add(e);
            }
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
                        proj.Entities.Add(ent);
                        if (!isShadow && ent.Lights != null)
                            proj.Lights.Add(ent);
                    }
                }
            }

            proj.SortEntities();
            if (isScreen)
                UpdateStreamingPosition(proj.Params.Position);
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

        public PiecePack GetPiecePack(JenkHash hash, Rpf6FileExt ext, out bool loaddeps)
        {
            var cache = GetCache(ext);
            loaddeps = false;

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
                cache[hash] = cacheItem;
                loaddeps = true;
            }
            cacheItem.LastUseFrame = CurrentFrame;
            return cacheItem.Object as PiecePack;
        }

        public (Piece, PiecePack) GetPiece(string fileContainer, JenkHash entityHash, bool fragment = false)
        {
            if (string.IsNullOrEmpty(fileContainer))
                return (null, null);

            if (fragment)
            {
                JenkHash hashFragment = new JenkHash(fileContainer.EndsWith(".wft") ? fileContainer.Replace(".wft", "") : fileContainer);
                Piece pieceFragment = null;
                PiecePack packFragment = GetPiecePack(hashFragment, Rpf6FileExt.wft, out var useless);
                packFragment?.Pieces?.TryGetValue(entityHash, out pieceFragment);
                return (pieceFragment, packFragment);
            }
            JenkHash hash = new JenkHash(fileContainer.EndsWith(".wvd") ? fileContainer.Replace(".wvd", "") : fileContainer);
            Piece piece = null;
            PiecePack pack = GetPiecePack(hash, Rpf6FileExt.wvd, out var loaddeps);
            pack?.Pieces?.TryGetValue(entityHash, out piece);
            return (piece, pack);
        }
    }
}
