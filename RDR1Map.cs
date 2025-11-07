using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;
using System.Numerics;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1
{
    public class RDR1Map : StreamingLevel<RDR1Game, RDR1MapFileCache, Rpf6FileManager>
    {
        public Dictionary<JenkHash, WsiFile> WsiFiles;
        public Dictionary<JenkHash, RDR1MapNode> MapNodes;
        public Dictionary<JenkHash, RDR1MapSector> MapSectors;
        public Rsc6TerrainWorldResource TerrainWorld;
        public Dictionary<Vector3I, RDR1TerrainTile> TerrainTiles;
        public RDR1Grid SmicGrid;
        public RDR1Grid TreeGrid;
        public RDR1Grid GrassGrid;
        public RDR1Grid BoundsGrid;
        public RDR1MapStatesFile MapStatesFile;

        public static string[] GetAreas()
        {
            return ["Armadillo", "Blackwater", "Beecher's Hope", "Chuparosa", "Cochinay", "El Presidio", "El Matadero", "Escalera", "Fort Mercer", "Gaptooth Breach", "Las Hermanas", "MacFarlane's Ranch", "Manzanita Post", "Nosalida", "Pacific Union RR Camp", "Plainview", "Rathskeller Fork", "Ridgewood Farm", "Tesoro Azul", "Thieve's Landing", "Torquemada", "Tumbleweed", "Twin Rocks"];
        }

        public static readonly Setting StartPositionSetting = Settings.Register("RDR1Map.Area", SettingType.String, "Blackwater", "Change position", false, false, GetAreas());
        public static readonly Setting EnabledSetting = Settings.Register("RDR1Map.Enabled", SettingType.Bool, true, true);
        public static readonly Setting EnableInstancedGrass = Settings.Register("RDR1Map.EnableInstancedGrass", SettingType.Bool, true);
        public static readonly Setting TreesDistanceSetting = Settings.Register("RDR1Map.TreesDistance", SettingType.Float, 200.0f, false);
        public static readonly Setting GrassDistanceSetting = Settings.Register("RDR1Map.GrassDistance", SettingType.Float, 50.0f, false);
        public Statistic EntityCountStat = Statistics.Register("RDR1Map.EntityCount", StatisticType.Counter);

        public override BaseField[] GetFields()
        {
            return BaseFields;
        }

        private static readonly BaseField[] BaseFields = new[]
        {
            new MapStatesField()
        };

        public RDR1Map(RDR1Game game) : base(game, "RDR1 Map Level")
        {
            SetOrientation(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * -0.5f)); //Make the sun go from east to west instead of north to south (+Y is east!)

            var position = StartPositionSetting.GetString() switch
            {
                "Armadillo" => new Vector3(2617.4f, -2180.6f, 20.2f),
                "Beecher's Hope" => new Vector3(1375.9f, -117.5f, 121.1f),
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
                "Thieve's Landing" => new Vector3(2333.3f, 73.3f, 76.4f),
                "Torquemada" => new Vector3(3456.4f, 370.5f, 80.7f),
                "Tumbleweed" => new Vector3(2950.4f, -3941.5f, 32.6f),
                "Twin Rocks" => new Vector3(2141.6f, -2430.9f, 28.0f),
                _ => new Vector3(1234.6f, 736.2f, 84.7f) //Blackwater by default
            };

            Rsc6DataWriter.UseProjectExplorer = true;

            Game = game;
            DefaultSpawnPoint = position;
            BoundingBox = new BoundingBox(new Vector3(-100000.0f), new Vector3(100000.0f));
            InitRenderData();
            InitPhysicsSim();

            Commands.Register("ReloadMapStates", "Reload mapstates.txt file", ReloadMapStatesCmd, game);
            Commands.Register("EnableWSI", "Enable or disable the specified WSI", EnableWsiCmd, game);
        }

        protected override bool StreamingInit()
        {
            Core.Engine.Console.Write("RDR1Map", "Initialising " + Game.Name + "...");
            FileManager = Game.GetFileManager() as Rpf6FileManager;

            if (FileManager == null)
            {
                throw new Exception("Failed to initialize RDR1.");
            }

            if (EnabledSetting.GetBool() == false)
            {
                Cache = new RDR1MapFileCache(this, FileManager);
                return true;
            }

            var dfm = FileManager.DataFileMgr;
            Cache = new RDR1MapFileCache(this, FileManager);
            StreamPosition = DefaultSpawnPoint;
            MapSectors = [];
            MapNodes = [];

            Core.Engine.Console.Write("RDR1Map", "Loading sector info...");
            WsiFiles = [];
            var wsientries = dfm.StreamEntries[Rpf6FileExt.wsi];

            foreach (var kvp in wsientries)
            {
                var fe = kvp.Value;
                var wsi = FileManager.LoadFilePack<WsiFile>(fe);
                if (wsi == null) continue;
                JenkIndex.Ensure(wsi.Name, "RDR1");
                WsiFiles[fe.ShortNameHash] = wsi;

                var node = new RDR1MapNode(this, wsi);
                MapNodes[fe.ShortNameHash] = node;

                if (node.Sectors != null)
                {
                    foreach (var sector in node.Sectors)
                    {
                        MapSectors[sector.NameHash] = sector;
                    }
                }
            }

            Core.Engine.Console.Write("RDR1Map", "Loading terrain tiles...");
            InitTerrainTiles();

            Core.Engine.Console.Write("RDR1Map", "Loading grids...");
            SmicGrid = new RDR1Grid(FileManager, 0x100, "game\\tune_d11generic.rpf\\tune\\smicgrid", "rdr2", "XXXXYYYY.txt");
            TreeGrid = new RDR1Grid(FileManager, 0x100, "game\\treeres.rpf\\treeres", "rdr2", "XXXXYYYY_spd.wsp"); //TODO: also tree_instancefile_YYYYXXXX.txt ?
            GrassGrid = new RDR1Grid(FileManager, 0x80, "game\\grassres.rpf\\grassres", "rdr2", "grass_instancefile_XXXXYYYY.wsg");
            BoundsGrid = new RDR1Grid(FileManager, 0x40, "game\\terrainboundres.rpf\\terrainboundres\\territory_swall_noid", "rdr2", "XXXXYYYY_bnd.wtb");

            Core.Engine.Console.Write("RDR1Map", "Loading map states...");
            LoadMapStates();

            Core.Engine.Console.Write("RDR1Map", FileManager.Game.Name + " map initialised.");
            return true;
        }

        protected override bool StreamingUpdate()
        {
            if (!EnabledSetting.GetBool()) return false;

            var ents = StreamEntities.CurrentSet;
            var spos = StreamPosition;
            var needsAnotherUpdate = false;

            AddWsiEntities(ref spos, ents);
            AddTerrainBoundsTiles(ref spos);
            AddTerrainTiles(ref spos, ents);
            AddTreeTiles(ref spos, ents);
            AddGrassTiles(ref spos, ents);

            /*
            var scale = new Vector3(10.0f);
            foreach (var ent in ents) //Update lights
            {
                if (ent is WsiLightEntity light && light.Lights != null && light.ParentName != string.Empty)
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
            */

            EntityCountStat.SetCounter(ents.Count);
            if (needsAnotherUpdate) StreamUpdateRequest = true;
            return true;
        }

        private void AddWsiEntities(ref Vector3 spos, HashSet<Entity> ents)
        {
            if (MapNodes == null) return;
            foreach (var kvp in MapNodes)
            {
                var node = kvp.Value;
                if (node == null) continue;

                var roots = node.RootSectors;
                if (roots == null) continue;

                foreach (var root in roots)
                {
                    RecurseAddWsiEntities(ref spos, ents, root, root);
                }
            }

            var crng = new Vector3(StreamPhysicsRange);
            var collsbox = new BoundingBox(spos - crng, spos + crng);
            foreach (var kvp in MapNodes)
            {
                var node = kvp.Value;
                if (node == null) continue;

                var sectors = node.Sectors;
                if (sectors == null) continue;

                foreach (var sector in sectors)
                {
                    if (sector.IsScopedSector == false) continue;
                    if (sector.ChildSectors != null) continue;
                    if (sector.BoundingBox.Intersects(collsbox) == false) continue;

                    var bndents = Cache.GetSectorBounds(sector.NameHash);
                    if (bndents != null)
                    {
                        foreach (var bndent in bndents)
                        {
                            ents.Add(bndent);
                        }
                    }
                    else
                    { }
                }
            }
        }

        private void RecurseAddWsiEntities(ref Vector3 spos, HashSet<Entity> ents, RDR1MapSector sector, RDR1MapSector root)
        {
            if (sector == null) return;
            if (sector.Enabled == false) return;
            var split = sector.StreamingBox.Contains(ref spos) == ContainmentType.Contains;
            if (split && (sector.ChildSectors != null))
            {
                foreach (var child in sector.ChildSectors)
                {
                    RecurseAddWsiEntities(ref spos, ents, child, root);
                }
            }
            else if (split && (sector.ScopedEntity != null))
            {
                var ent = sector.ScopedEntity;
                var pack = Cache.GetPiecePack(ent.ModelName, Rpf6FileExt.wvd);
                var rootpack = Cache.GetPiecePack(root.NameHash, Rpf6FileExt.wvd) as WvdFile;
                var piece = pack?.Piece as Rsc6Drawable;
                if (pack?.Pieces?.Count > 1)
                {
                    pack.Pieces.TryGetValue(ent.ModelName, out var ppiece);
                    piece = ppiece as Rsc6Drawable;
                }
                if (piece == null) return;
                var roottxd = rootpack?.VisualDictionary?.TextureDictionary.Item;
                if (roottxd != null)
                {
                    piece.ApplyTextures(roottxd);
                }
                ent.SetPiece(piece);
                ents.Add(ent);
            }
            else
            {
                var vlow = sector.ScopedEntityVlow;
                var med = sector.ScopedEntityMed;
                var medpack = (med != null) ? Cache.GetPiecePack(med.ModelName, Rpf6FileExt.wvd) : null;
                var vlowpack = (vlow != null) ? Cache.GetPiecePack(vlow.ModelName, Rpf6FileExt.wvd) : null;
                if ((med != null) && (medpack?.Piece != null))
                {
                    var piece = medpack?.Piece as Rsc6Drawable;
                    if (piece == null) return;
                    var rootpack = Cache.GetPiecePack(root.NameHash, Rpf6FileExt.wvd) as WvdFile;
                    var roottxd = rootpack?.VisualDictionary?.TextureDictionary.Item;
                    if (roottxd != null)
                    {
                        piece.ApplyTextures(roottxd);
                    }
                    med.SetPiece(piece);
                    ents.Add(med);
                }
                else if (vlow != null)
                {
                    var piece = vlowpack?.Piece;
                    if (piece == null) return;
                    vlow.SetPiece(piece);
                    ents.Add(vlow);
                }
                else
                { }
            }

            if (split)
            {
                var smicmap = FileManager.DataFileMgr.SmicMapping;
                smicmap.TryGetValue(sector.Name, out var sectorsmic);

                var rootpack = Cache.GetPiecePack(root.NameHash, Rpf6FileExt.wvd) as WvdFile;
                var roottxd = rootpack?.VisualDictionary?.TextureDictionary.Item;

                foreach (var ent in sector.Entities)
                {
                    var wsient = ent as WsiEntity;
                    if (wsient == null) continue;

                    var dist = (wsient.Position - spos).Length();
                    if (dist > wsient.LodDistMax) continue;
                    wsient.StreamingDistance = dist;

                    Piece piece = null;
                    var modelName = wsient.ModelName;
                    var pi = wsient.PropInstance;
                    var di = wsient.DrawableInstance;
                    if (pi != null)
                    {
                        var pack = Cache.GetPiecePack(modelName, Rpf6FileExt.wft);
                        piece = pack?.Piece;
                        if (piece == null)
                        { }
                    }
                    else if (di != null)
                    {
                        var pname = wsient.ParentName?.ToLowerInvariant();
                        var pack = Cache.GetPiecePack(pname, Rpf6FileExt.wvd);
                        if (pack == null)
                        {
                            pack = Cache.GetPiecePack(pname + "_non-terrain", Rpf6FileExt.wvd);
                            if (pack != null)
                            { }
                        }
                        if (pack?.Pieces != null)
                        {
                            pack.Pieces.TryGetValue(modelName, out piece);
                            if (piece == null)
                            { }
                        }
                        else
                        { }
                    }

                    if (piece == null) continue;

                    var dwbl = piece as Rsc6Drawable;
                    if (dwbl != null)
                    {
                        if (roottxd != null)
                        {
                            dwbl.ApplyTextures(roottxd);
                        }
                        if (sectorsmic != null)
                        {
                            foreach (var secsmic in sectorsmic)
                            {
                                var wtd = Cache.GetTexturePack(secsmic?.ToLowerInvariant()) as WtdFile;
                                var txd = wtd?.TextureDictionary;
                                if (txd != null)
                                {
                                    dwbl.ApplyTextures(txd);
                                }
                            }
                        }
                    }

                    wsient.SetPiece(piece);
                    ents.Add(wsient);

                    ents.Add(ent);
                }

                //if (sector.LightsEntity != null)
                //{
                //    ents.Add(sector.LightsEntity);
                //}
            }
        }

        private void InitTerrainTiles()
        {
            var dfm = FileManager.DataFileMgr;
            var wtlentries = dfm.StreamEntries[Rpf6FileExt.wtl];
            wtlentries.TryGetValue("lod", out var lodwtlentry);
            var lodwtl = FileManager.LoadFilePack<WtlFile>(lodwtlentry);
            TerrainWorld = lodwtl?.TerrainLod;
            TerrainTiles = new Dictionary<Vector3I, RDR1TerrainTile>();
            var wvdbasepath = lodwtlentry.Parent.PathLower;
            var wvdentries = dfm.StreamEntries[Rpf6FileExt.wvd];
            var wvddivs = new int[] { 2, 4, 1, 1, 8 };
            var texdictnames = new List<string>();
            var texsetnames = new List<string>();
            var texdicts = TerrainWorld.TextureDictionaries.Items;//probably just load all these
            var texsets = TerrainWorld.TextureSets.Items;
            for (int l = 0; l < TerrainWorld.NumLODLevels; l++)
            {
                var gridsize = (int)TerrainWorld.GridSize[l];//128,128,512,1024,1024
                var griddivs = (int)TerrainWorld.GridSubDivs[l];//2,4,2,1,1
                var cellsize = (l == 0) ? (gridsize / 2) : gridsize;
                var bucket = TerrainWorld.LodGridBuckets[l].Item;
                var sectors = TerrainWorld.Sectors.Items[l].Elements;
                var visuals = TerrainWorld.Visuals.Items[l].Elements;
                var barr = bucket.BucketArray.Items;
                var rmin = (int)bucket.RowMin;
                var cmin = (int)bucket.ColumnMin;
                var rcnt = (int)bucket.GridSizeRow;
                var ccnt = (int)bucket.GridSizeColumn;
                for (var r = 0; r < rcnt; r++)
                {
                    for (var c = 0; c < ccnt; c++)
                    {
                        var i = r * ccnt + c;
                        var b = barr[i];
                        if (b < 0) continue;
                        var sector = sectors[b];
                        var visual = visuals[sector.VisualIndex];
                        var x = rmin + r;
                        var y = cmin + c;
                        var coord = new Vector3I(x * cellsize, y * cellsize, l);
                        var ent = new RDR1TerrainTile();
                        ent.Sector = sector;
                        ent.Visual = visual;
                        ent.GridCoord = coord;
                        ent.WvdPieceHash = sector.InstHashCode;
                        ent.CellSize = cellsize;

                        var wvdd = wvddivs[l];
                        var wvdx = ((x + 1024) / wvdd) * wvdd - 1024 + 100;
                        var wvdy = ((y + 1024) / wvdd) * wvdd - 1024;
                        if (l == 4) { wvdx = 100; wvdy = -1; }//lod4 only has one wvd
                        var tilename = $"tile_{wvdx}_{wvdy}";
                        var wvdname = $"resource_{l}\\{tilename}.wvd";
                        var wvdpath = $"{wvdbasepath}\\{wvdname}";
                        FileManager.EntryDict.TryGetValue(wvdpath, out var wvdentry);
                        ent.Name = tilename;
                        ent.WvdName = wvdname;
                        ent.WvdEntry = wvdentry as Rpf6FileEntry;

                        texdictnames.Clear();
                        texsetnames.Clear();
                        for (int t = 0; t < sector.TextureSets.Count; t++)
                        {
                            var ti = sector.TextureSets.Items[t];
                            if (ti < 0) continue;
                            var texset = texsets[ti];
                            var tname = texset.TextureSetAssetName.Value;
                            if (string.IsNullOrEmpty(tname)) continue;
                            texsetnames.Add(tname);
                        }
                        for (int t = 0; t < visual.Count; t++)
                        {
                            var ti = visual.TextureDictionaries[t];
                            if (ti < 0) continue;
                            var texdict = texdicts[ti];
                            var tname = texdict.Name.Value;
                            if (string.IsNullOrEmpty(tname)) continue;
                            texdictnames.Add(tname);
                        }
                        ent.TexDictNames = (texdictnames.Count > 0) ? texdictnames.ToArray() : null;
                        ent.TexSetNames = (texsetnames.Count > 0) ? texsetnames.ToArray() : null;
                        ent.UpdateBounds();

                        TerrainTiles[coord] = ent;
                    }
                }
            }
        }

        private void AddTerrainTiles(ref Vector3 spos, HashSet<Entity> ents)
        {
            var lod = 3;//start terrain at lod3, since lod4 currently can't be drawn
            var gridsize = (int)TerrainWorld.GridSize[lod];//128,128,512,1024,1024
            var griddivs = (int)TerrainWorld.GridSubDivs[lod];//2,4,2,1,1
            var cellsize = (lod == 0) ? (gridsize / 2) : gridsize;
            var bucket = TerrainWorld.LodGridBuckets[lod].Item;
            var rmin = (int)bucket.RowMin;
            var cmin = (int)bucket.ColumnMin;
            var rcnt = (int)bucket.GridSizeRow;
            var ccnt = (int)bucket.GridSizeColumn;
            for (var r = 0; r < rcnt; r++)
            {
                for (var c = 0; c < ccnt; c++)
                {
                    var x = rmin + r;
                    var y = cmin + c;
                    var coord = new Vector3I(x * cellsize, y * cellsize, lod);
                    RecurseAddTerrainTile(coord, ref spos, ents);
                }
            }
        }

        private bool RecurseAddTerrainTile(in Vector3I coord, ref Vector3 spos, HashSet<Entity> ents)
        {
            TerrainTiles.TryGetValue(coord, out var tile);
            if (tile == null) return false;

            var splitdist = 3.0f;
            var center = tile.BoundingSphere.Center;
            var radius = tile.BoundingSphere.Radius;

            var lod = coord.Z;
            var dist = (center - spos).Length();
            if ((dist < (radius * splitdist)) && (lod > 0))
            {
                var clod = lod - 1;
                var gridsize = (int)TerrainWorld.GridSize[clod];//128,128,512,1024,1024
                var griddivs = (int)TerrainWorld.GridSubDivs[clod];//2,4,2,1,1
                var cellsize = (clod == 0) ? (gridsize / 2) : gridsize;
                var anyok = false;
                for (var cx = 0; cx < griddivs; cx++)
                {
                    for (var cy = 0; cy < griddivs; cy++)
                    {
                        var ox = cx * cellsize;
                        var oy = cy * cellsize;
                        var ccoord = new Vector3I(coord.X + ox, coord.Y + oy, clod);
                        var ok = RecurseAddTerrainTile(ccoord, ref spos, ents);
                        anyok = anyok || ok;
                    }
                }
                if (anyok) return true;
            }

            tile.Piece = Cache.GetTerrainTileWvdPiece(tile.WvdEntry, tile.WvdPieceHash);
            if (tile.Piece == null) return false;
            var dwbl = tile.Piece as Rsc6Drawable;

            foreach (var restxd in FileManager.DataFileMgr.ResidentTxds)
            {
                dwbl?.ApplyTextures(restxd);
            }

            if (tile.TexDictNames != null)
            {
                foreach (var texdict in tile.TexDictNames)
                {
                    var wtd = Cache.GetTexturePack(texdict) as WtdFile;
                    if (wtd == null) continue;
                    dwbl?.ApplyTextures(wtd.TextureDictionary);
                }
            }
            if (tile.TexSetNames != null)
            {
                foreach (var texset in tile.TexSetNames)
                {
                    var wtdname = texset + "_hilod";
                    var wtd = Cache.GetTexturePack(wtdname) as WtdFile;
                    if (wtd == null) continue;
                    dwbl?.ApplyTextures(wtd.TextureDictionary);
                }
            }

            ents.Add(tile);

            return true;
        }

        private void AddTerrainBoundsTiles(ref Vector3 spos)
        {
            var range = StreamPhysicsRange;
            BoundsGrid.UpdateVisibleCells(spos, range);
            var sbox = BoundsGrid.GetVisibleBox(spos, range);

            foreach (var cell in BoundsGrid.VisibleCells)
            {
                var tile = Cache.GetTerrainBoundsTile(cell.Item2, BoundsGrid, cell.Item1);
                if (tile == null) continue;

                foreach (var ent in tile.Entities)
                {
                    StreamPhysics.Add(ent);
                }
            }
        }

        private void AddTreeTiles(ref Vector3 spos, HashSet<Entity> ents)
        {
            var range = TreesDistanceSetting.GetFloat();
            TreeGrid.UpdateVisibleCells(spos, range);
            var sbox = TreeGrid.GetVisibleBox(spos, range);

            foreach (var cell in TreeGrid.VisibleCells)
            {
                var tile = Cache.GetTreeGridTile(cell.Item2, TreeGrid, cell.Item1);
                if (tile == null) continue;
                if (tile.Batches == null) continue;

                foreach (var batch in tile.Batches)
                {
                    if (batch == null) continue;
                    if (batch.Enabled == false) continue;
                    var bbox = batch.BoundingBox;
                    if (bbox.Intersects(sbox) == false) continue;
                    var pack = Cache.GetPiecePack(batch.ModelHash, Rpf6FileExt.wft);
                    if (pack == null) continue;
                    if (pack.Pieces.Count > 1) { }
                    var piece = pack.Piece;
                    if (piece == null) continue;
                    batch.SetPiece(piece);
                    batch.Batch?.StreamingUpdate(spos);
                    ents.Add(batch);
                }
            }
        }

        private void AddGrassTiles(ref Vector3 spos, HashSet<Entity> ents)
        {
            if (EnableInstancedGrass.GetBool() == false) return;
            var range = GrassDistanceSetting.GetFloat();
            GrassGrid.UpdateVisibleCells(spos, range);
            var sbox = GrassGrid.GetVisibleBox(spos, range);

            foreach (var cell in GrassGrid.VisibleCells)
            {
                var tile = Cache.GetGrassGridTile(cell.Item2, GrassGrid, cell.Item1);
                if (tile == null) continue;
                if (tile.Batches == null) continue;

                foreach (var batch in tile.Batches)
                {
                    if (batch == null) continue;
                    if (batch.Enabled == false) continue;
                    var bbox = batch.BoundingBox;
                    if (bbox.Intersects(sbox) == false) continue;
                    var pack = Cache.GetPiecePack(batch.ModelHash, Rpf6FileExt.wft);
                    if (pack == null)
                    {
                        if (batch.Name.StartsWith("p_sno")) //fallback since p_sno_grass02x doesn't seem to exist
                        {
                            var newname = "p_cho" + batch.Name.Substring(5);
                            pack = Cache.GetPiecePack(newname, Rpf6FileExt.wft);
                            if (pack == null)
                            { }
                        }
                    }
                    if (pack == null) continue;
                    if (pack.Pieces.Count > 1) { }
                    var piece = pack.Piece;
                    if (piece == null) continue;
                    batch.SetPiece(piece);
                    batch.Batch?.StreamingUpdate(spos);
                    ents.Add(batch);
                }
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

        public void EnableWSI(JenkHash nameHash, bool enable) //Try to replicate the game's ENABLE_WORLD_SECTOR/DISABLE_WORLD_SECTOR
        {
            bool subsector = false;
            var name = nameHash.ToString();

            if (name.Contains('/')) //Sub-sector detected, instead try to replicate ENABLE_CHILD_SECTOR/DISABLE_CHILD_SECTOR
            {
                nameHash = new(name[(name.LastIndexOf('/') + 1)..]);
                subsector = true;
            }

            MapSectors.TryGetValue(nameHash, out var sector);
            if (sector == null && enable)
            {
                WsiFiles.TryGetValue(nameHash, out var wsi);
                if (wsi == null) return;
                var node = new RDR1MapNode(this, wsi);
                var nodeSectors = node.Sectors;
                if (nodeSectors != null)
                {
                    foreach (var nodeSector in nodeSectors)
                    {
                        MapSectors[nodeSector.NameHash] = nodeSector;
                        //StreamBVH?.Add(nodeSector);
                    }
                }
            }

            if (sector != null)
            {
                sector.Enabled = enable;
                if (!subsector)
                {
                    var nodeSectors = sector.MapNode?.Sectors;
                    if (nodeSectors != null)
                    {
                        foreach (var nodeSector in nodeSectors)
                        {
                            nodeSector.Enabled = enable;
                        }
                    }
                }
            }
        }

        private string EnableWsiCmd(string[] args)
        {
            var wsi = ((args != null) && (args.Length > 0)) ? args[0].ToLowerInvariant() : "";
            var arg = ((args != null) && (args.Length > 1)) ? args[1].ToLowerInvariant() : "";
            var enable = (arg != "false") && (arg != "disable") && (arg != "0");

            if (string.IsNullOrEmpty(wsi))
            {
                return "No WSI specified! Command usage: EnableWSI <wsiname> <false/disable/0>";
            }

            StreamInvoke(() =>
            {
                EnableWSI(wsi, enable);
            });
            return $"{wsi} {(enable ? "Enabled" : "Disabled")}";
        }

        public void EnableMapStateGroup(RDR1MapStateGroup group, bool enable)
        {
            group.Enabled = enable;
            foreach (var wsi in group.WSIs)
            {
                if (string.IsNullOrEmpty(wsi.Item1)) continue;
                var wsiName = wsi.Item1.ToLowerInvariant();
                bool wsiEnable;
                if (!enable)
                {
                    if (wsi.Item2 == "disabled")
                        wsiEnable = group.IsStateActive(wsi.Item2, wsi.Item3);
                    else
                        wsiEnable = false;
                }
                else
                {
                    wsiEnable = (group.Undead && wsi.Item2 != "disabled") || group.IsStateActive(wsi.Item2, wsi.Item3);
                }
                EnableWSI(wsiName, wsiEnable);
            }
        }

        private void LoadMapStates()
        {
            MapStatesFile = new RDR1MapStatesFile(this);
            if (MapStatesFile.CurrentGroups != null)
            {
                foreach (var group in MapStatesFile.CurrentGroups)
                {
                    EnableMapStateGroup(group, group.Enabled);
                }
            }
        }

        private string ReloadMapStatesCmd()
        {
            StreamInvoke(() =>
            {
                LoadMapStates();
                Commands.TryExecute("EditorUI.ReloadWorldSettings");
            });
            return "Reloading Map States...";
        }

        private class MapStatesField : BaseField
        {
            public MapStatesField() : base(BaseFieldType.ObjectArray, "Map States")
            {
            }

            public override void GetValue(BaseObject obj, out object value)
            {
                var map = obj as RDR1Map;
                value = map?.MapStatesFile?.CurrentGroups;
            }

            public override void SetValue(BaseObject obj, object value)
            {
            }
        }
    }

    public class RDR1MapStateGroup : BaseObject
    {
        public RDR1Map Map;
        public bool MP;
        public bool Undead;
        public Dictionary<string, string> States;
        public List<(string, string, string)> WSIs = new(); //name,statename,stateval
        private Dictionary<string, string[]> StateOptions;
        private BaseField[] Fields;

        public RDR1MapStateGroup(RDR1Map map)
        {
            this.Map = map;
        }

        public override void SetEnabled(bool enabled)
        {
            Map.StreamInvoke(() =>
            {
                Map.EnableMapStateGroup(this, enabled);
            });
        }

        public bool IsStateActive(string name, string val)
        {
            if (!this.Enabled || string.IsNullOrEmpty(name)) return true;
            if (this.Undead) return false;
            if (!this.States.TryGetValue(name, out var cstate)) return true;
            return cstate == val;
        }

        public void BuildStateOptions()
        {
            if (this.States == null)
            {
                this.StateOptions = null;
                return;
            }

            var opts = new Dictionary<string, List<string>>();
            var inputs = new List<(string, string)>();

            if (this.States != null)
            {
                foreach (var kvp in this.States)
                {
                    inputs.Add((kvp.Key, kvp.Value));
                }
            }

            foreach (var ipl in this.WSIs)
            {
                inputs.Add((ipl.Item2, ipl.Item3));
            }

            foreach (var i in inputs)
            {
                var state = i.Item1;
                var val = i.Item2;
                if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(val)) continue;

                if (opts.TryGetValue(state, out var list) == false)
                {
                    list = new List<string>() { "" }; //Start with an empty entry, just so that's an option
                    opts[state] = list;
                }

                if (list.Contains(val) == false)
                {
                    list.Add(val);
                }
            }

            if (opts.Count == 0)
            {
                this.StateOptions = null;
                return;
            }

            this.StateOptions = new Dictionary<string, string[]>();
            foreach (var kvp in opts)
            {
                this.StateOptions[kvp.Key] = kvp.Value.ToArray();
            }
        }

        public override BaseField[] GetFields()
        {
            if ((this.Fields == null) && (this.StateOptions != null))
            {
                var fields = new BaseField[this.StateOptions.Count];
                var i = 0;
                foreach (var kvp in this.StateOptions)
                {
                    fields[i] = new StateField(kvp.Key, kvp.Value);
                    i++;
                }
                this.Fields = fields;
            }
            return this.Fields;
        }

        private class StateField : BaseField
        {
            private readonly string[] Options;

            public StateField(string name, string[] options) : base(BaseFieldType.String, name)
            {
                this.Options = options;
            }

            public override void GetOptions(BaseObject obj, out object[] options)
            {
                options = this.Options;
            }

            public override void GetValue(BaseObject obj, out object value)
            {
                value = null;
                if (obj is not RDR1MapStateGroup group) return;
                if (group.States == null) return;
                group.States.TryGetValue(Name, out var str);
                value = str;
            }

            public override void SetValue(BaseObject obj, object value)
            {
                if (obj is not RDR1MapStateGroup group) return;
                group.Map.StreamInvoke(() =>
                {
                    group.States ??= new Dictionary<string, string>();
                    group.States[Name] = value as string;
                    group.Map.EnableMapStateGroup(group, group.Enabled); //Need to refresh this group
                });
            }
        }
    }

    public class RDR1MapStatesFile
    {
        public List<RDR1MapStateGroup> AllGroups = new();
        public RDR1MapStateGroup[] CurrentGroups;

        public RDR1MapStatesFile(RDR1Map map)
        {
            var file = StartupUtil.GetFilePath("CodeX.Games.RDR1.mapstates.txt");
            if (System.IO.File.Exists(file) == false) return;

            var lines = System.IO.File.ReadAllLines(file);
            if (lines == null) return;

            var delims = new[] { ' ', '\t' };
            var splitopt = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            var section = Section.None;
            var group = (RDR1MapStateGroup)null;

            foreach (var line in lines)
            {
                var comment = line.IndexOf("//");
                var tline = (comment >= 0) ? line[..comment] : line;
                var parts = tline.Split(delims, splitopt);

                if (parts == null || parts.Length == 0) continue;
                if (parts[0].StartsWith("group:"))
                {
                    section = Section.None;
                    group = new RDR1MapStateGroup(map)
                    {
                        Name = parts[0][6..],
                        Enabled = true
                    };

                    AllGroups.Add(group);
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var subparts = parts[i].Split(':', splitopt);
                        if (subparts == null) continue;
                        if (subparts.Length < 2) continue;

                        var fname = subparts[0];
                        var fval = subparts[1];
                        var fval2 = (subparts.Length > 2) ? subparts[2] : null;

                        switch (fname)
                        {
                            case "mp": group.MP = fval == "true"; break;
                            case "enabled": group.Enabled = fval == "true"; break;
                        }
                    }
                }
                else if (parts[0] == "wsis:")
                {
                    section = Section.WSIs;
                }
                else if (parts[0] == "states:")
                {
                    section = Section.States;
                }
                else if (group != null)
                {
                    var name = parts[0]?.ToLowerInvariant();
                    if (section == Section.WSIs)
                    {
                        var stateparts = (parts.Length > 1) ? parts[1].Split(':') : null;
                        var stateplen = (stateparts != null) ? stateparts.Length : 0;
                        var statename = (stateplen > 0) ? stateparts[0] : null;
                        var stateval = (stateplen > 1) ? stateparts[1] : null;

                        group.WSIs.Add((name, statename, stateval));
                        if (parts.Length > 1 && stateplen == 1)
                        {
                            group.Undead = true;
                        }

                        JenkIndex.Ensure(name, "RDR1");
                    }
                    else if (section == Section.States)
                    {
                        group.States ??= new();
                        var val = (parts.Length > 1) ? parts[1] : "";
                        group.States[name] = val;
                    }
                }
            }

            var currGroups = new List<RDR1MapStateGroup>();
            foreach (var g in AllGroups)
            {
                g.BuildStateOptions();
                currGroups.Add(g);
            }
            CurrentGroups = currGroups.ToArray();
        }

        private enum Section
        {
            None,
            WSIs,
            States
        }
    }

    public class RDR1MapNode
    {
        public RDR1Map Map;
        public WsiFile Wsi { get; set; }
        public Rsc6SectorInfo SectorInfo { get; set; }
        public RDR1MapSector[] Sectors { get; set; }
        public RDR1MapSector[] RootSectors { get; set; }
        public JenkHash NameHash { get; set; }

        public RDR1MapNode(RDR1Map map, WsiFile wsi) //Buildings, props, etc...
        {
            Map = map;
            Wsi = wsi;
            NameHash = wsi.FileEntry.ShortNameHash;

            SectorInfo = Wsi?.SectorInfo;
            if (SectorInfo == null) return;
            var childGroup = SectorInfo.ChildGroup.Item;
            var childSectors = SectorInfo.ChildPtrs.Items;

            var sectors = new List<RDR1MapSector>();
            var roots = new List<RDR1MapSector>();
            if (childGroup != null)
            {
                var childLists = new Dictionary<Rsc6SectorInfo, List<RDR1MapSector>>();
                var scopedSectors = childGroup.Sectors.Items;
                var scopedParents = childGroup.SectorsParents.Items;
                var imax = Math.Min(scopedSectors?.Length ?? 0, scopedParents?.Length ?? 0);
                for (int i = 0; i < imax; i++)
                {
                    var sector = scopedSectors[i];
                    var parent = scopedParents[i]?.Parent.Item;
                    if (sector == null) continue;
                    var mapSector = new RDR1MapSector(this, wsi, sector, parent, true);
                    sectors.Add(mapSector);
                    if (parent == SectorInfo) roots.Add(mapSector);
                    childLists.TryGetValue(parent, out var pchildren);
                    if (pchildren == null)
                    {
                        pchildren = new List<RDR1MapSector>();
                        childLists[parent] = pchildren;
                    }
                    pchildren.Add(mapSector);
                }
                foreach (var sector in sectors)
                {
                    var s = sector?.Sector;
                    if (s == null) continue;
                    childLists.TryGetValue(s, out var childList);
                    if (childList != null)
                    {
                        sector.ChildSectors = childList.ToArray();
                    }
                }
            }
            if (childSectors != null)
            {
                foreach (var childSector in childSectors)
                {
                    if (childSector == null) continue;
                    var mapSector = new RDR1MapSector(this, wsi, childSector, SectorInfo, false);
                    sectors.Add(mapSector);
                    roots.Add(mapSector);
                }
            }
            if (sectors.Count == 0) return;

            Sectors = sectors.ToArray();
            RootSectors = roots.ToArray();
        }
    }

    public class RDR1MapSector : Level
    {
        public RDR1MapNode MapNode { get; set; }
        public JenkHash NameHash { get; set; }
        public Rsc6SectorInfo ParentSector { get; set; }
        public Rsc6SectorInfo Sector { get; set; }
        public WsiFile Wsi { get; set; }
        public bool IsScopedSector { get; set; }
        public RDR1MapSector[] ChildSectors { get; set; }
        public WsiEntity ScopedEntity { get; set; }
        public WsiEntity ScopedEntityMed { get; set; }
        public WsiEntity ScopedEntityVlow { get; set; }
        public Entity LightsEntity { get; set; }

        public RDR1MapSector(RDR1MapNode mapNode, WsiFile wsi, Rsc6SectorInfo currentSector, Rsc6SectorInfo parentSector, bool scoped)
        {
            MapNode = mapNode;
            Wsi = wsi;
            FilePack = wsi;
            FilePack.EditorObject = this; //Allow the editor access to this
            NameHash = currentSector.NameHash;
            Name = NameHash.ToString();
            Sector = currentSector;
            ParentSector = parentSector;
            BoundingBox = currentSector.Bounds;
            IsScopedSector = scoped;

            var width = BoundingBox.Width;
            var dist = new Vector3(BoundingBox.Size.Length());//(width > 700.0f) ? width : 700.0f);
            StreamingBox = new BoundingBox(BoundingBox.Center - dist, BoundingBox.Center + dist);

            if (scoped)
            {
                var name = currentSector.Name.Value?.ToLowerInvariant();
                var dfm = mapNode.Map.FileManager.DataFileMgr;
                var wvdentry = dfm.TryGetStreamEntry(NameHash, Rpf6FileExt.wvd);
                if (wvdentry != null)
                {
                    ScopedEntity = new WsiEntity()
                    {
                        ModelName = wvdentry.ShortNameHash,
                        LodDistMax = 9999
                    };
                }
                else
                { }
                var wvdmedentry = dfm.TryGetStreamEntry(name + "_med", Rpf6FileExt.wvd);
                if (wvdmedentry != null)
                {
                    ScopedEntityMed = new WsiEntity()
                    {
                        ModelName = wvdmedentry.ShortNameHash,
                        LodDistMax = 9999
                    };
                }
                else
                { }
                var wvdvlowentry = dfm.TryGetStreamEntry(name + "_vlow", Rpf6FileExt.wvd);
                if (wvdvlowentry != null)
                {
                    ScopedEntityVlow = new WsiEntity()
                    {
                        ModelName = wvdvlowentry.ShortNameHash,
                        LodDistMax = 9999
                    };
                }
                else
                { }
            }

            GetEntities();
        }

        public void GetEntities()
        {
            if (Wsi == null) return;
            if (Wsi.SectorInfo == null) return;
            var name = Sector.Name.Value?.ToLowerInvariant() ?? "";
            if (name == Wsi.SectorInfo.Scope.Value ||
                name.Contains("_ffa01x") ||
                name.Contains("_base01x")) return; //Deathmatch props

            if (Wsi.FileEntry?.Archive?.Name == "swterrain.rpf")//IsTerrainSector())
            {
                name += "_non-terrain";
            }

            AddEntitiesFromSector(name);//Add the sector's entities
            AddLightsFromSector();//Add the sector's lights
        }

        public override void PreSaveUpdate()
        {
            base.PreSaveUpdate();
            UpdateEntitiesArray();
        }

        private void AddLightsFromSector()
        {
            var lights = Sector.PlacedLightsGroup.Item;
            if (lights != null)
            {
                ////// TODO!!! these lights are all messed up
                //var lightList = new List<Light>();
                //foreach (var light in lights.Lights.Items)
                //{
                //    ////var e = new WsiLightEntity(light, Entities.Count, lights.Name.ToString());
                //    ////Add(e);
                //    var pos = light.Position.XYZ();
                //    var dir = new Vector3((float)light.Direction.Z, (float)light.Direction.X, (float)light.Direction.Y);
                //    var ty = dir.GetPerpVec();
                //    var tx = Vector3.Normalize(Vector3.Cross(dir, ty));
                //    var col = new Vector3((float)light.Color.X / 5.0f, (float)light.Color.Y / 5.0f, (float)light.Color.Z / 5.0f);
                //    var intensity = light.Intensity;
                //    var range = light.Range;
                //    var innerAngle = FloatUtil.DegToRad((float)light.InnerConeOuterCone.X);
                //    var outerAngle = FloatUtil.DegToRad((float)light.InnerConeOuterCone.X);
                //    var l = Light.CreateSpot(pos, dir, tx, ty, col, intensity, range, 5.0f, innerAngle, outerAngle);
                //    l.Params.CastShadows = false;//TODO
                //    lightList.Add(l);
                //}
                //if (lightList.Count > 0)
                //{
                //    LightsEntity = new Entity();
                //    LightsEntity.Name = lights.Name.Value;
                //    var p = new Piece() { Lights = lightList.ToArray(), Name = LightsEntity.Name };
                //    foreach (var light in lightList)
                //    {
                //        light.Piece = p;
                //        light.Entity = LightsEntity;
                //    }
                //    p.UpdateBounds();
                //    LightsEntity.SetPiece(p);
                //    LightsEntity.UpdateBounds();
                //}
            }
        }

        private void AddEntitiesFromSector(string parentName)
        {
            var entities = Sector.Props.Items;
            if (entities != null && entities.Length > 0)
            {
                foreach (var instance in entities)
                {
                    var e = new WsiEntity(instance, Entities.Count, parentName);
                    Add(e);
                }
            }

            var drawableInstances = Sector.DrawableInstances.Items;
            if (drawableInstances != null && drawableInstances.Length > 0)
            {
                var ident = Matrix4x4.Identity;
                ident.M44 = 0.0f;

                foreach (var instance in drawableInstances)
                {
                    var e = new WsiEntity(instance, Entities.Count, parentName);
                    Add(e);
                }
            }
        }

        private void UpdateEntitiesArray()
        {
            var entities = new List<WsiEntity>();
            foreach (var ent in Entities)
            {
                if (ent is WsiLightEntity) continue;//TODO: handle lights and DrawableInstances
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
                    RotationX = ent.PropInstance.RotationX,//TODO: use current entity quaternion
                    RotationY = ent.PropInstance.RotationY,
                    RotationZ = ent.PropInstance.RotationZ,
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
                var newEntities = kvp.Value.ToArray();
                if (ParentSector != null)
                {
                    ParentSector.Props = new(newEntities);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [TC(typeof(EXP))]
    public class WsiEntity : Entity
    {
        public bool ResetPos = false;
        public JenkHash ModelName { get; set; }

        public Rsc6PropInstanceInfo PropInstance { get; set; }
        public Rsc6DrawableInstanceBase DrawableInstance { get; set; }

        public string ParentName { get; set; }
        public byte Flags { get; set; }
        public byte AO { get; set; }
        public byte ModMode { get; set; }
        public byte NetworkingFlags { get; set; }
        public byte RotationType { get; set; }

        public override string Name => ModelName.ToString();
        public RDR1MapSector MapSector => Level as RDR1MapSector;
        public WsiFile Wsi => MapSector?.Wsi;

        public WsiEntity()
        {
        }

        public WsiEntity(Rsc6PropInstanceInfo entity, int index, string parentName) //Fragments, props
        {
            Index = index;
            ParentName = parentName;
            PropInstance = entity;

            var name = entity.EntityName.Value?.ToLowerInvariant();
            if (name.Contains('/'))
            {
                name = name[(name.LastIndexOf("/") + 1)..];
            }

            var yaw = (float)entity.RotationZ; //green
            var pitch = (float)entity.RotationX; //red
            var roll = (float)entity.RotationY; //blue

            var rotation = Quaternion.Identity;
            switch (entity.RotationType)
            {
                default:
                case 0: //[-π/2, π/2]
                    var halfPi = MathF.PI / 2.0f;
                    var minusHalfPi = -MathF.PI / 2.0f;
                    if (roll < minusHalfPi || roll > halfPi || pitch < minusHalfPi || pitch > halfPi)
                        rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI - roll);
                    else
                        rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, roll);
                    break;

                case 2:
                    rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI - roll);
                    break;
            }

            Position = entity.EntityPosition.XYZ();
            Orientation = rotation;
            ModelName = JenkHash.GenHash(name);
            LodDistMax = 100.0f;
            Flags = entity.Flags;
            AO = entity.AO;
            ModMode = entity.ModMode;
            NetworkingFlags = entity.NetworkingFlags;
            RotationType = entity.RotationType;
        }

        public WsiEntity(Rsc6DrawableInstanceBase entity, int index, string parentName) //Fragments, props
        {
            Index = index;
            ParentName = parentName;
            DrawableInstance = entity;

            var name = entity.Name.Value?.ToLowerInvariant();
            entity.Matrix.Decompose(out var scale, out var rot, out var translation);
            rot = new Quaternion(rot.Z, rot.X, rot.Y, rot.W);

            Position = translation;
            Orientation = rot;
            OrientationInv = Quaternion.Inverse(rot);
            Scale = scale;
            ModelName = JenkHash.GenHash(name);
            LodDistMax = 500.0f;

            //var min = entity.BoundingBoxMin.XYZ();
            //var max = entity.BoundingBoxMax.XYZ();
            //var size = (max - min).Length();
            //LodDistMax = Math.Min(size * 20.0f, 500.0f);
        }

        public override void UpdateBounds()
        {
            var pos = Position;
            if (ResetPos)
            {
                Position = Vector3.Zero;
            }
            base.UpdateBounds();
            Position = pos;
        }

        public override void SetPiece(Piece p)
        {
            base.SetPiece(p);

            if (DrawableInstance != null)
            {
                var min = DrawableInstance.BoundingBoxMin.XYZ();
                var max = DrawableInstance.BoundingBoxMax.XYZ();
                var size = (max - min).Length();
                LodDistMax = Math.Min(size * 20.0f, 500.0f);
            }
        }
    }

    [TC(typeof(EXP))]
    public class WsiLightEntity : WsiEntity
    {
        //TODO: delete this and add lights directly to the sector scoped entity

        public Vector3 ParentPosition;

        public WsiLightEntity(Rsc6PlacedLight light, int index, string parent)
        {
            Index = index;
            Position = light.Position.XYZ();
            ParentPosition = light.ParentPosition.XYZ();
            ParentName = parent;
            ModelName = JenkHash.GenHash(light.DebugName.Value.ToLowerInvariant());
            LodDistMax = 500.0f;
            ResetPos = true;

            var pos = light.Position.XYZ();
            var dir = new Vector3((float)light.Direction.Z, (float)light.Direction.X, (float)light.Direction.Y);
            var ty = dir.GetPerpVec();
            var tx = Vector3.Normalize(Vector3.Cross(dir, ty));
            var col = new Vector3((float)light.Color.X / 5.0f, (float)light.Color.Y / 5.0f, (float)light.Color.Z / 5.0f);
            var intensity = light.Intensity;
            var range = light.Range;
            var innerAngle = FloatUtil.DegToRad((float)light.InnerConeOuterCone.X);
            var outerAngle = FloatUtil.DegToRad((float)light.InnerConeOuterCone.X);
            var l = Light.CreateSpot(pos, dir, tx, ty, col, intensity, range, 5.0f, innerAngle, outerAngle);
            Lights = new Light[] { l };
        }
    }

    public class RDR1Grid
    {
        public Vector2 WorldOffset = new Vector2(-6144, -6144);//grid (0,0) corner in world space.
        public int CellSize;
        public string Path;
        public string LevelName;
        public string EntryFormat;
        public string ValidInstDir;
        public string ValidInstMasterObjectFile;
        public int ValidInstCount;
        public int ValidInstVersion;
        public Dictionary<Vector2I, Rpf6FileEntry> CellEntries = new();//NOTE:(Y,X)!
        public List<(Vector2I, Rpf6FileEntry)> VisibleCells = new();//NOTE:(Y,X)!

        public RDR1Grid(Rpf6FileManager fileMan, int cellSize, string path, string levelName, string entryFormat)
        {
            CellSize = cellSize;
            Path = path;
            LevelName = levelName;
            EntryFormat = entryFormat;

            var validinsttxtpath = $"{path}\\validinstance_{levelName}.txt";
            var validinsttxt = fileMan.GetFileUTF8Text(validinsttxtpath);
            var validinstlines = validinsttxt?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (validinstlines == null) return;

            var hexnum = System.Globalization.NumberStyles.HexNumber;
            var invcul = System.Globalization.CultureInfo.InvariantCulture;
            foreach (var line in validinstlines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                var colind = line.IndexOf(':');
                if (colind > 0)
                {
                    var fname = line.Substring(0, colind);
                    var fval = line.Substring(colind).Trim();
                    switch (fname)
                    {
                        case "Dir": ValidInstDir = fval; break;
                        case "MasterObjectFile": ValidInstMasterObjectFile = fval; break;
                        case "Count": int.TryParse(fval, out ValidInstCount); break;
                        case "Version": int.TryParse(fval, out ValidInstVersion); break;
                        default: break;
                    }
                }
                else
                {
                    var xstr = line.Substring(0, 4).ToLowerInvariant();
                    var ystr = line.Substring(4).Trim().ToLowerInvariant();
                    int.TryParse(xstr, hexnum, invcul, out var x);
                    int.TryParse(ystr, hexnum, invcul, out var y);
                    var coord = new Vector2I(x, y);
                    var entryName = entryFormat.Replace("XXXX", xstr).Replace("YYYY", ystr);
                    var entryPath = $"{path}\\{entryName}";
                    var entry = fileMan.GetEntry(entryPath) as Rpf6FileEntry;
                    if (entry == null) continue;
                    CellEntries[coord] = entry;
                }
            }
            if ((ValidInstCount != 0) && (ValidInstCount != CellEntries.Count))
            { }

            //in validinstance file:
            //Dir: $\treeRes
            //MasterObjectFile: $\treeRes\masterObject_RDR2.txt
            //Count: n
            //YYYY XXXX  (or YYYYXXXX)

            //smicgrid: YYYYXXXX.txt
            //treeres: tree_instancefile_YYYYXXXX.txt, YYYYXXXX_spd.wsp
            //grassres: grass_instancefile_YYYYXXXX.wsg
            //terrainboundres: YYYYXXXX_bnd.wtb

            //For grids:
            //+X is SOUTH (using "converted" coords)
            //+Y is EAST
            //tree grid:
            //cell size 256
            //YYYYXXXX_spd.wsp (XXXX, YYYY are 16bit hex)
            //world Y = -6144 + YYYY
            //world X = -6144 + XXXX
            //YYYY = world Y + 6144  (as hex digits, round as necessary for grid cell size)
            //XXXX = world X + 6144
        }

        public void UpdateVisibleCells(Vector3 spos, float range)
        {
            VisibleCells.Clear();

            var rngvec = new Vector2(range);
            var worldmin = (spos.XY() - rngvec);
            var worldmax = (spos.XY() + rngvec);
            var cellmin = ((worldmin - WorldOffset) / CellSize);
            var cellmax = ((worldmax - WorldOffset) / CellSize);
            var gridmin = new Vector2I(cellmin.Floor());
            var gridmax = new Vector2I(cellmax.Ceiling());

            for (int x = gridmin.X; x <= gridmax.X; x++)
            {
                for (int y = gridmin.Y; y <= gridmax.Y; y++)
                {
                    var coord = new Vector2I(y, x) * CellSize;
                    CellEntries.TryGetValue(coord, out var cellEntry);
                    if (cellEntry == null) continue;
                    VisibleCells.Add((coord, cellEntry));
                }
            }
        }

        public BoundingBox GetVisibleBox(Vector3 spos, float range)
        {
            var srng = new Vector3(range);
            var sbox = new BoundingBox(spos - srng, spos + srng);
            return sbox;
        }
    }

    public class RDR1TerrainTile : Entity
    {
        public Rsc6TerrainSector Sector { get; set; }
        public Rsc6TerrainVisual Visual { get; set; }
        public string[] TexDictNames { get; set; }
        public string[] TexSetNames { get; set; }
        public string WvdName { get; set; }
        public Rpf6FileEntry WvdEntry { get; set; }
        public JenkHash WvdPieceHash { get; set; }
        public int CellSize { get; set; }
        public Vector3I GridCoord { get; set; }

        public override void UpdateBounds()
        {
            //meshes have position baked in, so use custom bounds
            //since this entity's position is (0,0,0) it needs a custom box
            //base.UpdateBounds();
            UpdateWorldTransform();
            if (Sector != null)
            {
                BoundingBox = new BoundingBox(Sector.AABBMin.XYZ(), Sector.AABBMax.XYZ());
                BoundingSphere = new BoundingSphere(BoundingBox.Center, Sector.AABBMin.W);
            }
        }
    }

    public class RDR1TerrainBoundsTile : Level
    {
        public WtbFile Wtb { get; set; }
        public RDR1Grid Grid { get; set; }
        public Vector2I Coord { get; set; }
        public Vector3 TileCenter { get; set; }
        public Dictionary<JenkHash, Piece> BoundPieces = new();

        public RDR1TerrainBoundsTile(WtbFile wtb, RDR1Grid grid, Vector2I coord)
        {
            Wtb = wtb;
            Grid = grid;
            Coord = coord;
            FilePack = wtb;
            //FilePack.EditorObject = this;
            Name = (wtb.FileInfo as Rpf6FileEntry).ShortNameLower;

            var wtbbnds = wtb.TileBounds;
            if (wtbbnds == null) return;
            var wtbrd = wtbbnds.ResourceDict.Item;
            if (wtbrd == null) return;
            var hashes = wtbrd.Codes.Items;
            if (hashes == null) return;
            var entries = wtbrd.Entries.Items;
            if (entries == null) return;

            for (int i = 0; i < entries.Length; i++)
            {
                var hash = hashes[i];
                var entry = entries[i];
                if (entry == null) continue;
                var bounds = entry.Bounds.Item;
                if (bounds == null) continue;

                //create a piece for this bounds
                var piece = new Piece();
                piece.Name = bounds.Name ?? (Name + "_0x" + hash.Hex);
                piece.FilePack = wtb;
                piece.Collider = bounds;
                piece.UpdateBounds();
                BoundPieces[hash] = piece;

                var instdict = entry.InstanceDict.Item;
                if (instdict == null) continue;
                var ihashes = instdict.Codes.Items;
                if (ihashes == null) continue;
                var ientries = instdict.Entries.Items;
                if (ientries == null) continue;
                for (int j = 0; j < ientries.Length; j++)
                {
                    var ihash = ihashes[j];
                    var ientry = ientries[j];
                    if (ientry == null) continue;
                    var iinst = ientry.Instance.Item;
                    if (iinst == null) continue;
                    var mat = iinst.Matrix;

                    var pos = mat.Translation;
                    if (pos != Vector3.Zero)
                    { }
                    var r1 = mat.Row3();
                    var r2 = mat.Row1();
                    var r3 = mat.Row2();
                    mat.Row1(new Vector4(r1.Z, r1.X, r1.Y, r1.W));
                    mat.Row2(new Vector4(r2.Z, r2.X, r2.Y, r2.W));
                    mat.Row3(new Vector4(r3.Z, r3.X, r3.Y, r3.W));
                    mat.Row4(new Vector4(0, 0, 0, 1));
                    mat.Decompose(out var sca, out var ori, out var pos2);

                    //create an entity for this instance
                    var ent = new Entity();
                    ent.Name = piece.Name + "_0x" + ihash.Hex;
                    ent.FilePack = wtb;
                    ent.SetTransform(pos, ori, sca, false);
                    ent.SetPiece(piece);

                    Add(ent);
                }
            }
        }

        public override void PreSaveUpdate()
        {
            base.PreSaveUpdate();
            //TODO: editing
        }
    }

    public class RDR1TreeGridTile : Level
    {
        public WspFile Wsp { get; set; }
        public RDR1Grid Grid { get; set; }
        public Vector2I Coord { get; set; }
        public Vector3 TileCenter { get; set; }
        public RDR1TreeGridBatch[] Batches { get; set; }

        public RDR1TreeGridTile(WspFile wsp, RDR1Map map, RDR1Grid grid, Vector2I coord)
        {
            Wsp = wsp;
            Grid = grid;
            Level = map;
            Coord = coord;
            FilePack = wsp;
            //FilePack.EditorObject = this;
            Name = (wsp.FileInfo as Rpf6FileEntry).ShortNameLower;

            var wspgrid = wsp.Grid;
            if (wspgrid == null) return;

            var treenames = wspgrid.TreeNames.Items;
            if (treenames == null) return;
            var names = new string[treenames.Length];
            var hashes = new JenkHash[treenames.Length];
            for (int i = 0; i < treenames.Length; i++)
            {
                var name = treenames[i].Value;
                var dotind = name.LastIndexOf('.');
                if (dotind > 0) name = name.Substring(0, dotind);
                names[i] = name;
                hashes[i] = new JenkHash(name.ToLowerInvariant() + "x");
            }

            var currentcell = (Rsc6TreeForestGridCell)null;
            var currentbatches = new Dictionary<int, RDR1TreeGridBatch>();
            var batchlist = new List<RDR1TreeGridBatch>();
            RDR1TreeGridBatch getBatch(int id)
            {
                if (currentbatches.TryGetValue(id, out var batch)) return batch;
                if ((id < 0) || (id > names.Length)) return null;
                batch = new RDR1TreeGridBatch();
                batch.Tile = this;
                batch.Cell = currentcell;
                batch.Name = names[id];
                batch.ModelHash = hashes[id];
                batchlist.Add(batch);
                currentbatches[id] = batch;
                return batch;
            }

            var cells = wspgrid.GridCells.Items;
            if (cells == null) return;
            foreach (var cell in cells)
            {
                if (cell == null) continue;
                currentbatches.Clear();
                currentcell = cell;

                var positions = cell.CombinedInstanceListPos.Items;
                if (positions != null)
                {
                    foreach (var position in positions)
                    {
                        if (position == null) continue;
                        var batch = getBatch(position.TreeIndex);
                        if (batch == null) continue;
                        var inst = new EntityBatchInstance1();
                        inst.Matrix = Matrix3x4.CreateTranslation(position.Position);
                        batch.Instances.Add(inst);
                    }
                }
                var matrices = cell.CombinedInstanceListMatrix.Items;
                if (matrices != null)
                {
                    foreach (var matrix in matrices)
                    {
                        if (matrix == null) continue;
                        var batch = getBatch(matrix.TreeTypeID);
                        if (batch == null) continue;
                        var inst = new EntityBatchInstance1();
                        var mat = matrix.Transform;
                        var pos = mat.Translation;
                        var c1 = mat.Row3();
                        var c2 = mat.Row1();
                        var c3 = mat.Row2();
                        inst.Matrix.Row1 = new Vector4(c1.Z, c1.X, c1.Y, pos.X);
                        inst.Matrix.Row2 = new Vector4(c2.Z, c2.X, c2.Y, pos.Y);
                        inst.Matrix.Row3 = new Vector4(c3.Z, c3.X, c3.Y, pos.Z);
                        batch.Instances.Add(inst);
                    }
                }
            }

            foreach (var gridbatch in batchlist)
            {
                var ok = gridbatch.BuildFromInstances();
                if (ok == false) continue;
                Add(gridbatch);
            }

            Batches = batchlist.ToArray();
        }

        public override void PreSaveUpdate()
        {
            base.PreSaveUpdate();
            //TODO: move editing code from RDR1TreeMapData
        }
    }

    public class RDR1TreeGridBatch : Entity
    {
        public RDR1TreeGridTile Tile { get; set; }
        public Rsc6TreeForestGridCell Cell { get; set; }
        public JenkHash ModelHash { get; set; }
        public BoundingBox PointsBox { get; set; }
        public BoundingBox PieceBox { get; set; }
        public List<EntityBatchInstance1> Instances = new();

        public bool BuildFromInstances()
        {
            var insts = Instances.ToArray();
            if (insts.Length == 0)
            {
                Enabled = false;//empty batch - don't bother with it
                return false;
            }

            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            for (int i = 0; i < insts.Length; i++)
            {
                var trans = insts[i].Matrix.Translation;
                min = Vector3.Min(min, trans);
                max = Vector3.Max(max, trans);
            }
            var bmin = new Vector4(min, 0);
            var bsiz = new Vector4(Vector3.Max(max - min, Vector3.One), 0);
            var sparams = new Vector4(1, 1, 0, 0);
            var entbatch = new EntityBatch(this, insts.Length, 1, bmin, bsiz, sparams);
            BufferUtil.WriteArray(entbatch.Data, 0, insts);
            var gridscale = 0.2;//inverse of min cell size
            var maxgs = (int)Math.Min(Math.Max(Math.Max(bsiz.X, bsiz.Y) * gridscale, 2), 32);
            entbatch.InitGrid(maxgs);
            SetBatch(entbatch);

            PointsBox = new BoundingBox(min, max);
            UpdateBounds();

            return true;
        }

        public override void UpdateBounds()
        {
            //base.UpdateBounds();
            var pieceRad = new Vector3(PieceBox.Size.Length());
            var min = PointsBox.Minimum - pieceRad;
            var max = PointsBox.Maximum + pieceRad;
            BoundingBox = new BoundingBox(min, max);
            BoundingSphere.Center = BoundingBox.Center;
            BoundingSphere.Radius = BoundingBox.Size.Length() * 0.5f;
        }

        public override void SetPiece(Piece p)
        {
            var changed = (p != Piece);
            if (p != null) PieceBox = p.BoundingBox;
            base.SetPiece(p);
            if ((p == null) || (changed == false)) return;

            var d = p as Rsc6Drawable;
            if (d == null) return;
            var hdist = 9999f;//d.LodDistHigh;
            var mdist = d.LodDistMed;
            Batch.InitLods(hdist, mdist);
            Batch.Lods[0].RenderEntity = new RDR1BatchLodEntity(this, Batch.Lods[0], p, 0);
            //Batch.Lods[1].RenderEntity = new RDR1BatchLodEntity(this, Batch.Lods[1], p, 1);

            ////lod1 of RDR1 trees is missing the trunk for some reason.
            ////TODO: use a proper billboarding/impostor system for these
        }

        public override void ApplyTransforms()
        {
            //base.ApplyTransforms();
            //TODO: handle moving instances and updating bounds
        }
    }

    public class RDR1GrassGridTile : Level
    {
        public WsgFile Wsg { get; set; }
        public RDR1Grid Grid { get; set; }
        public Vector2I Coord { get; set; }
        public Vector3 TileCenter { get; set; }
        public RDR1GrassGridBatch[] Batches { get; set; }

        public RDR1GrassGridTile(WsgFile wsg, RDR1Map map, RDR1Grid grid, Vector2I coord)
        {
            Wsg = wsg;
            Grid = grid;
            Level = map;
            Coord = coord;
            FilePack = wsg;
            //FilePack.EditorObject = this;
            Name = (wsg.FileInfo as Rpf6FileEntry).ShortNameLower;

            var gfield = wsg.GrassField;
            if (gfield == null) return;
            var gitems = gfield.GrassItems.Items;
            if (gitems == null) return;

            var batchlist = new List<RDR1GrassGridBatch>();
            for (int i = 0; i < gitems.Length; i++)
            {
                var gitem = gitems[i];
                if (gitem == null) continue;

                var gname = gitem.Name.Value;
                var ghash = gitem.NameHash;
                var mname = "p_" + gname + "x";

                var batch = new RDR1GrassGridBatch();
                batch.Tile = this;
                batch.GrassField = gitem;
                batch.Name = mname;
                batch.ModelHash = new JenkHash(mname);

                var ok = batch.BuildFromGrassField();
                if (ok == false) continue;

                batchlist.Add(batch);
                Add(batch);
            }

            Batches = batchlist.ToArray();
        }
    }

    public class RDR1GrassGridBatch : Entity
    {
        public RDR1GrassGridTile Tile { get; set; }
        public Rsc6GrassField GrassField { get; set; }
        public JenkHash ModelHash { get; set; }
        public BoundingBox PointsBox { get; set; }
        public BoundingBox PieceBox { get; set; }
        //public List<EntityBatchInstance2> Instances = new();

        public bool BuildFromGrassField()
        {
            var vbuf = GrassField?.VertexBuffer.Item;
            if (vbuf == null) return false;
            var vfmt = vbuf.Layout.Item;
            if (vfmt == null) return false;
            var vdata = vbuf.VertexData.Items;
            if (vdata == null) return false;
            var cnt = (int)vbuf.VertexCount;
            if (cnt == 0) return false;
            if (vdata.Length != cnt * 4) return false;

            var min = GrassField.AABBMin.XYZ();
            var max = GrassField.AABBMax.XYZ();
            var rng = GrassField.AABBScale.XYZ();
            if (rng != (max - min))
            { }
            if (rng == Vector3.Zero)
            { }
            var bmin = new Vector4(min, 0);
            var bsiz = new Vector4(rng, 1);
            var sparams = new Vector4(1, 1, 0, 0);
            var batch = new EntityBatch(this, cnt, 2, bmin, bsiz, sparams);

            var insts = new EntityBatchInstance2[cnt];
            for (var i = 0; i < cnt; i++)
            {
                ref var inst = ref insts[i];
                var cv = BufferUtil.ReadColour(vdata, i * 4);
                var n = Vector2.Zero;
                inst.PositionX = (ushort)(cv.R * 256);
                inst.PositionY = (ushort)(cv.B * 256);
                inst.PositionZ = (ushort)(cv.G * 256);
                inst.NormalX = (byte)((n.X * 0.5f + 0.5f) * 0xFF);
                inst.NormalY = (byte)((n.Y * 0.5f + 0.5f) * 0xFF);
                inst.ColourR = 0x7F;
                inst.ColourG = 0x7F;
                inst.ColourB = 0x7F;
            }
            BufferUtil.WriteArray(batch.Data, 0, insts);
            var gridscale = 0.5;//inverse of min cell size
            var maxgs = (int)Math.Min(Math.Max(Math.Max(bsiz.X, bsiz.Y) * gridscale, 2), 32);
            batch.InitGrid(maxgs);
            SetBatch(batch);

            PointsBox = new BoundingBox(min, max);
            UpdateBounds();

            return true;
        }

        public override void UpdateBounds()
        {
            //base.UpdateBounds();
            var pieceRad = new Vector3(PieceBox.Size.Length());
            var min = PointsBox.Minimum - pieceRad;
            var max = PointsBox.Maximum + pieceRad;
            BoundingBox = new BoundingBox(min, max);
            BoundingSphere.Center = BoundingBox.Center;
            BoundingSphere.Radius = BoundingBox.Size.Length() * 0.5f;
        }

        public override void SetPiece(Piece p)
        {
            var changed = (p != Piece);
            if (p != null) PieceBox = p.BoundingBox;
            base.SetPiece(p);
            if ((p == null) || (changed == false)) return;

            var d = p as Rsc6Drawable;
            if (d == null) return;
            var hdist = d.LodDistHigh;
            var mdist = d.LodDistMed;
            Batch.InitLods(hdist, mdist);
            Batch.Lods[0].RenderEntity = new RDR1BatchLodEntity(this, Batch.Lods[0], p, 0);

            //RDR1 grass only has one lod..
            //TODO: use a billboarding impostor system for this
        }

        public override void ApplyTransforms()
        {
            //base.ApplyTransforms();
            //TODO: handle moving instances and updating bounds
        }
    }

    public class RDR1BatchLodEntity : Entity
    {
        public Entity ParentBatch;

        public RDR1BatchLodEntity(Entity batch, EntityBatchLod lod, Piece p, int pieceLod)
        {
            ParentBatch = batch;
            Level = batch.Level;
            Piece = p;
            PieceLodOverride = pieceLod;
            CurrentDistance = lod.LodDist;//this isn't great...
            BoundingBox = batch.BoundingBox;
            BoundingSphere = batch.BoundingSphere;
            Position = batch.Position;// BoundingBox.Center;
            Orientation = Quaternion.Identity;
            Scale = Vector3.One;
            ParentIndex = -1;
            LodLevel = 0;
            LodDistMax = 9999;
            SetBatchLod(lod);
            UpdateWorldTransform();
        }

        public override void UpdateBounds()
        {
            //base.UpdateBounds();
        }

        public override void ApplyTransforms()
        {
            //base.ApplyTransforms();
            ParentBatch.ApplyTransforms();//need to bounce this because we're actually rendering/editing this object
        }
    }

    /*
    TODO: use this old code to help rebuild editing code in RDR1TreeGridTile/RDR1TreeGridBatch

    public class RDR1TreeMapData : Level
    {
        private readonly RDR1Map Map;
        public WspFile Wsp;

        public RDR1TreeMapData(WspFile wsp, RDR1Map map)
        {
            var dist = new Vector3(700.0f);
            Map = map;
            FilePack = wsp;
            FilePack.EditorObject = this; //Allow the editor access to this
            Name = wsp.Name;
            Wsp = wsp;
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
                    if (name.EndsWith(".s"))
                    {
                        name = name.Replace(".s", ".spt"); //There's an issue from R* with a tree that has a name that's too long...
                    }

                    name = name.Replace(".spt", "x"); //SpeedTree to fragment name
                    var hash = new JenkHash(name);
                    var entity = new RDR1GridForestEntity(inst, gridCell, hash, RDR1Map.TreesDistanceSetting.GetFloat());
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
                    var entity = new RDR1GridForestEntity(inst, gridCell, hash, RDR1Map.TreesDistanceSetting.GetFloat());
                    this.Add(entity);
                }
            }
        }

        public override BaseCreateType[] GetCreateTypes()
        {
            return new[] { BaseCreateType.Entity };
        }

        public override BaseObject CreateObject(in BaseCreateArgs args)
        {
            if (args.Type.Name == "Entity")
            {
                var ent = new RDR1GridForestEntity(args, RDR1Map.TreesDistanceSetting.GetFloat())
                {
                    Level = this
                };
                InsertObject(ent);
                return ent;
            }
            return null;
        }

        public override void InsertObject(BaseObject obj)
        {
            Map.StreamInvoke(() => //Make sure this runs on stream thread to synchronise Entities use
            {
                if (obj is RDR1GridForestEntity ent)
                {
                    var id = ((ent.Index >= 0) && (ent.Index <= Entities.Count)) ? ent.Index : Entities.Count;
                    Entities.Insert(id, ent);
                    //if (ent.ParentInParentLevel || (ent.ParentIndex < 0)) RootEntities.Add(ent);
                    UpdateEntityIndexes();
                    ent.LodParent?.AddLodChild(ent);
                }
            });
        }

        public override void PreSaveUpdate()
        {
            base.PreSaveUpdate();
            UpdateEntitiesArray();
        }

        private void UpdateEntityIndexes()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var e = Entities[i];
                if (e == null) continue;
                e.Index = i;
            }
        }

        private void UpdateEntitiesArray()
        {
            var grid = Wsp.Grid;
            var entities = new List<RDR1GridForestEntity>();

            foreach (var ent in Entities)
            {
                if (ent is not RDR1GridForestEntity wspEnt) continue;
                entities.Add(wspEnt);
            }

            //Fist step, update existing trees
            foreach (var ent in entities)
            {
                if (ent.Position == ent.OriginalPosition) continue;
                foreach (var gridCell in grid.GridCells.Items)
                {
                    if (ent.GridCell != gridCell) continue;
                    var instances = gridCell.CombinedInstanceListPos.Items;
                    UpdatePackedInstanceList(ent, instances);
                    break;
                }
            }

            //Second step, update added trees
            var newEntities = entities.Where(e => e.Created == true).ToList();
            if (newEntities.Any())
            {
                var newInstances = new List<Rsc6PackedInstancePos>();
                var newIndices = new List<short>();
                var newCell = new Rsc6TreeForestGridCell();
                var newEntNames = newEntities.Select(e => e.Name.Replace(e.Name.Last().ToString(), ".spt")).Distinct().ToList();
                var treeNamesSet = new HashSet<string>(grid.TreeNames.Items.Select(tn => tn.ToString()));

                //An undefined tree type was added, rebuild the grid data
                if (newEntNames.Any(name => !treeNamesSet.Contains(name)))
                {
                    var treeValues = Enumerable.Repeat(uint.MaxValue, (int)(grid.Trees.Count + newEntNames.Count)).ToArray();
                    var treeNames = grid.TreeNames.Items.ToList();
                    treeNames.AddRange(newEntNames.Select(s => new Rsc6Str(s)).ToArray());

                    var treeHashes = grid.TreeHashes.Items.ToList();
                    foreach (var n in newEntNames)
                    {
                        treeHashes.Add(GetHashTree(n));
                    }

                    grid.Trees = new(treeValues);
                    grid.TreeHashes = new(treeHashes.ToArray());
                    grid.TreeNames = new(treeNames.ToArray());
                }

                newEntities.Sort((x, y) => string.Compare(x.Name, y.Name));
                newIndices.Add(0);

                short lastIndex = -1;
                for (int i = 0; i < newEntities.Count; i++)
                {
                    var ent = newEntities[i];
                    if (ent.Position == ent.OriginalPosition || ent.GridCell != null) continue;

                    var treeIndex = (short)Array.FindIndex(grid.TreeNames.Items, n => n.ToString() == ent.Name.Replace(ent.Name.Last().ToString(), ".spt"));
                    var maxIndex = (short)(newEntities.FindLastIndex(e => e.Name == ent.Name) + 1);
                    var inst = new Rsc6PackedInstancePos(ent.Position);
                    UpdatePackedInstance(inst, ent.Position);

                    newInstances.Add(inst);
                    if (maxIndex > lastIndex)
                    {
                        newIndices.Add(treeIndex);
                        newIndices.Add(maxIndex);
                        lastIndex = maxIndex;
                    }
                }

                var sphere = CalculateCellSphere(newEntities);
                newCell.BoundSphere = sphere;
                newCell.CombinedInstanceListPos = new(newInstances.ToArray());
                newCell.IndexList = new(newIndices.ToArray()); //Atleast 3 indices for a single tree [startIndex, treeType, endIndex]

                //Find and use a empty cell if there's one
                var cellFound = false;
                var cells = grid.GridCells.Items.ToList();
                for (int i = 0; i < cells.Count; i++)
                {
                    if (cells[i].BoundSphere != new BoundingSphere()) continue;
                    cells.RemoveAt(i);
                    cells.Insert(i, newCell);
                    cellFound = true;
                    break;
                }

                //If all the cells are used, we'll increase the number of cell and fill the array with empty cells
                if (!cellFound)
                {
                    var total = grid.CellsHeight * grid.CellsWidth;
                    var newTotal = (grid.CellsHeight + 1) * (grid.CellsWidth + 1);
                    var gap = newTotal - total;

                    grid.CellsHeight += 1;
                    grid.CellsWidth += 1;
                    cells.Add(newCell);

                    for (int i = 0; i < gap - 1; i++)
                    {
                        var cell = new Rsc6TreeForestGridCell();
                        cells.Add(cell);
                    }
                }
                grid.GridCells = new(cells.ToArray());
            }
        }

        private void UpdatePackedInstanceList(RDR1GridForestEntity ent, Rsc6PackedInstancePos[] instances)
        {
            if (instances == null) return;
            for (int i = 0; i < instances.Length; i++)
            {
                var inst = instances[i];
                if (inst.Position == ent.Position || inst.Position != ent.OriginalPosition) continue;
                UpdatePackedInstance(inst, ent.Position);
                instances[i] = inst;
                break;
            }
        }

        private void UpdatePackedInstance(Rsc6PackedInstancePos inst, Vector3 pos)
        {
            inst.Position = pos;
            var bb = Wsp.Grid.BoundingBox;
            var scaledPos = pos - bb.Minimum;
            var normalizedPos = Vector3.Divide(scaledPos, bb.Size);
            var newPos = Vector3.Multiply(normalizedPos, 65535.0f);

            inst.Z = (ushort)Math.Clamp((int)Math.Round(newPos.X), 0, 65535);
            inst.X = (ushort)Math.Clamp((int)Math.Round(newPos.Y), 0, 65535);
            inst.Y = (ushort)Math.Clamp((int)Math.Round(newPos.Z), 0, 65535);
        }

        private static void UpdateInstanceMatrices(RDR1GridForestEntity ent, Rsc6InstanceMatrix[] matrices)
        {
            if (matrices == null) return;
            for (int i = 0; i < matrices.Length; i++)
            {
                var inst = matrices[i];
                if (inst.Transform.Translation == ent.Position || inst.Transform.Translation != ent.OriginalPosition) continue;
                var pos = new Vector3(ent.Position.Y, ent.Position.Z, ent.Position.X);
                var ori = new Quaternion(ent.Orientation.Y, ent.Orientation.Z, ent.Orientation.X, ent.Orientation.W);
                inst.Transform = Matrix4x4.CreateTranslation(pos);
                inst.Transform = Matrix4x4.CreateRotationX(ori.X);
                inst.Transform = Matrix4x4.CreateRotationY(ori.Y);
                inst.Transform = Matrix4x4.CreateRotationZ(ori.Z);
                break;
            }
        }

        public BoundingSphere CalculateCellSphere(List<RDR1GridForestEntity> ents)
        {
            var emin = new Vector3(float.MaxValue);
            var emax = new Vector3(float.MinValue);

            foreach (var ent in ents)
            {
                if (ent is RDR1GridForestEntity ge)
                {
                    var bmin = ge.BoundingBox.Minimum;
                    var bmax = ge.BoundingBox.Maximum;
                    emin = Vector3.Min(emin, bmin);
                    emax = Vector3.Max(emax, bmax);
                }
            }

            if (emin == new Vector3(float.MaxValue)) emin = Vector3.Zero;
            if (emax == new Vector3(float.MinValue)) emax = Vector3.Zero;

            var bb = new BoundingBox(emin, emax);
            var center = bb.Center;
            return new BoundingSphere(center, bb.Width);
        }

        private static JenkHash GetHashTree(string treeName)
        {
            //Returns the correct hash for a tree...
            //R* use different params to generate them, with filename or path of a speedtree file, seed, size, size variance.
            return treeName switch
            {
                "st_cornstalks01.spt" => 0xB8A3DB31,
                "st_liveoak01.spt" => 0x60AD9B35,
                "st_mesquite01.spt" => 0x1B6184D7,
                "st_scruboak01.spt" => 0xCA2D914E,
                "st_silvermaple01.spt" => 0xC0E46185,
                "st_silvermaplefall01.spt" => 0xDA32F6CE,
                "st_silvermaple02.spt" => 0x97553B61,
                "st_liveoak02.spt" => 0x6A96467E,
                "st_fanpalm01.spt" => 0x76074199,
                "st_sugarpine01.spt" => 0xF42F32E2,
                "st_easternredcedarsnow01.spt" => 0xBF2300A8,
                "st_longleafpine01.spt" => 0x602DE683,
                "st_chollocactus01.spt" => 0x80574FA5,
                "st_creosotebush01.spt" => 0x1CBB5242,
                "st_joshuatree01.spt" => 0xE660F06C,
                "st_sagebrush01.spt" => 0xA7F39B37,
                "st_joshuatree02.spt" => 0x863F9426,
                "st_joshuatree03.spt" => 0x391ABB79,
                "st_joshuatree04.spt" => 0x6CAB0040,
                "st_joshuatree05.spt" => 0x2F4E0093,
                "st_beavertailcactus01.spt" => 0x1725062E,
                "st_desertbroom01.spt" => 0x3E9D7F48,
                "st_giantsaguarocactus01.spt" => 0xF4490729,
                "st_pricklypearcactus01.spt" => 0xA9CC30B7,
                "st_saguarocactus01.spt" => 0x5E60C97B,
                "st_saguarocactus02.spt" => 0xC8E50CCE,
                "st_saguarocactus03.spt" => 0x30A897E9,
                "st_snakeweed01.spt" => 0x3C8C07A0,
                "st_agave01.spt" => 0x9F3D3B74,
                "st_aloevera01.spt" => 0xB85CC136,
                "st_catclaw01.spt" => 0xE02D85F8,
                "st_desertironwood01.spt" => 0x06322E4D,
                "st_rata01.spt" => 0xE5649386,
                "st_rata02.spt" => 0x2C69F02D,
                "st_russianolivefall01.spt" => 0xFD3E3877,
                "st_snakeweedflower01.spt" => 0xEE238496,
                "st_bigberrymanzanita01.spt" => 0x512D274C,
                "st_columnarenglishoak01.spt" => 0x3C2219A3,
                "st_columnarenglishoakwinter01.spt" => 0x0D63C017,
                "st_douglasfirdead01.spt" => 0x690ACF60,
                "st_easternredcedar01.spt" => 0x48AC619A,
                "st_greypoplar01.spt" => 0x93024E3E,
                "st_sugarpinedead01.spt" => 0x05455926,
                "st_utahjuniper01.spt" => 0x1C218FB9,
                "st_whitepine01.spt" => 0x032099CF,
                "st_hangingtreefrontier01.spt" => 0xE628516D,
                "st_americanboxwood01.spt" => 0x4E84FA81,
                "st_bottlebrushtree01.spt" => 0xF4B6B5F4,
                "st_palmetto01.spt" => 0x169B6E51,
                "st_riverbirch01.spt" => 0x761147C3,
                "st_whitebirch01.spt" => 0xEC4F25FC,
                _ => 0,
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
    public class RDR1GridForestEntity : Entity
    {
        public Rsc6TreeForestGridCell GridCell;
        public JenkHash TreeName;
        public Vector3 OriginalPosition; //Used so if we're moving a tree in the map viewer, we can find it back from a #sp using its original position.
        public bool Created = false; //Used to make sure we'll update new trees

        public override string Name => TreeName.ToString();
        public RDR1TreeMapData Wsp => Level as RDR1TreeMapData;

        public RDR1GridForestEntity(BaseCreateArgs args, float dist) //Create
        {
            TreeName = args.AssetName;
            LodDistMax = dist;
            Position = args.Transform.Position;
            Orientation = args.Transform.Orientation;
            Scale = args.Transform.Scale;
            OrientationInv = Orientation.IsIdentity ? Quaternion.Identity : Quaternion.Inverse(Orientation);
            Created = true;
        }

        public RDR1GridForestEntity(Rsc6PackedInstancePos inst, Rsc6TreeForestGridCell gridCell, JenkHash name, float dist) //Trees
        {
            TreeName = name;
            GridCell = gridCell;
            LodDistMax = dist;
            Position = inst.Position;
            OriginalPosition = Position;
        }

        public RDR1GridForestEntity(Rsc6InstanceMatrix inst, Rsc6TreeForestGridCell gridCell, JenkHash name, float dist) //Debris and foliages around buildings and roads
        {
            inst.Transform.Decompose(out var scale, out var rot, out var translation);
            TreeName = name;
            GridCell = gridCell;
            LodDistMax = dist;
            Position = translation;
            OriginalPosition = Position;
            Orientation = new Quaternion(rot.Z, rot.X, rot.Y, rot.W);
            OrientationInv = Quaternion.Inverse(Orientation);
            Scale = scale;
        }

        public override void SetPiece(Piece p)
        {
            var changed = p != Piece;
            Piece = p;

            if ((p != null) && changed)
            {
                UpdateBounds();
            }
        }
    }

    */

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
            Piece = Map.Cache.GetPiece(NameHash);
        }
    }

    public class RDR1MapFileCache : StreamingCache
    {
        public RDR1Map Map;
        public Rpf6FileManager FileManager;
        private readonly Dictionary<Rpf6FileExt, StreamingCacheDict<JenkHash>> Cache = new();
        private StreamingCacheDict<Rpf6FileEntry> TerrainTileWvdCache;
        private StreamingCacheDict<Rpf6FileEntry> TerrainTileWtbCache;
        private StreamingCacheDict<Rpf6FileEntry> TreeGridTileCache;
        private StreamingCacheDict<Rpf6FileEntry> GrassGridTileCache;
        private StreamingCacheDict<JenkHash> SectorBoundsCache;

        public RDR1MapFileCache(RDR1Map map, Rpf6FileManager fman)
        {
            Map = map;
            FileManager = fman;
            TerrainTileWvdCache = new StreamingCacheDict<Rpf6FileEntry>(this);
            TerrainTileWtbCache = new StreamingCacheDict<Rpf6FileEntry>(this);
            TreeGridTileCache = new StreamingCacheDict<Rpf6FileEntry>(this);
            GrassGridTileCache = new StreamingCacheDict<Rpf6FileEntry>(this);
            SectorBoundsCache = new StreamingCacheDict<JenkHash>(this);
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

        public override void BeginFrame()
        {
            base.BeginFrame();
            foreach (var cache in Cache)
            {
                cache.Value.RemoveOldItems();
            }
            TerrainTileWvdCache.RemoveOldItems();
            TerrainTileWtbCache.RemoveOldItems();
            TreeGridTileCache.RemoveOldItems();
            GrassGridTileCache.RemoveOldItems();
            SectorBoundsCache.RemoveOldItems();
        }

        public TexturePack GetTexturePack(JenkHash hash)
        {
            var ext = Rpf6FileExt.wtd_wtx;
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
                        cacheItem.Object = FileManager.LoadTexturePack(entry);
                    }
                    catch { }
                }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            cache[hash] = cacheItem;
            return cacheItem.Object as TexturePack;
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
#if !DEBUG
                    try
                    {
#endif
                    var pack = FileManager.LoadPiecePack(entry);//, null, true);
                    cacheItem.Object = pack;

                    if (pack != null)
                    {
                        LoadDependencies(pack);
                    }
#if !DEBUG
                    }
                    catch { }
#endif
                }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            cache[hash] = cacheItem;
            return cacheItem.Object as PiecePack;
        }

        public WvdFile GetTerrainTileWvd(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            var cache = TerrainTileWvdCache;
            if (!cache.TryGetValue(entry, out var cacheItem))
            {
                cacheItem = new StreamingCacheEntry();
                Core.Engine.Console.Write("RDR1Map", entry.Name);
                try
                {
                    var pack = FileManager.LoadPiecePack(entry);
                    cacheItem.Object = pack;
                }
                catch { }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            cache[entry] = cacheItem;
            return cacheItem.Object as WvdFile;
        }

        public Piece GetTerrainTileWvdPiece(Rpf6FileEntry entry, JenkHash hash)
        {
            var wvd = GetTerrainTileWvd(entry);
            if (wvd == null) return null;
            if (wvd.Pieces == null) return null;
            wvd.Pieces.TryGetValue(hash, out var piece);
            return piece;
        }

        public RDR1TerrainBoundsTile GetTerrainBoundsTile(Rpf6FileEntry entry, RDR1Grid grid, Vector2I coord)
        {
            if (entry == null) return null;
            var cache = TerrainTileWtbCache;
            if (!cache.TryGetValue(entry, out var cacheItem))
            {
                cacheItem = new StreamingCacheEntry();
                Core.Engine.Console.Write("RDR1Map", entry.Name);
                try
                {
                    var pack = FileManager.LoadFilePack<WtbFile>(entry);
                    var tile = new RDR1TerrainBoundsTile(pack, grid, coord);
                    cacheItem.Object = tile;
                }
                catch { }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            cache[entry] = cacheItem;
            return cacheItem.Object as RDR1TerrainBoundsTile;
        }

        public RDR1TreeGridTile GetTreeGridTile(Rpf6FileEntry entry, RDR1Grid grid, Vector2I coord)
        {
            if (entry == null) return null;
            var cache = TreeGridTileCache;
            if (!cache.TryGetValue(entry, out var cacheItem))
            {
                cacheItem = new StreamingCacheEntry();
                Core.Engine.Console.Write("RDR1Map", entry.Name);
                try
                {
                    var pack = FileManager.LoadFilePack<WspFile>(entry);
                    var tile = new RDR1TreeGridTile(pack, Map, grid, coord);
                    cacheItem.Object = tile;
                }
                catch { }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            cache[entry] = cacheItem;
            return cacheItem.Object as RDR1TreeGridTile;
        }

        public RDR1GrassGridTile GetGrassGridTile(Rpf6FileEntry entry, RDR1Grid grid, Vector2I coord)
        {
            if (entry == null) return null;
            var cache = GrassGridTileCache;
            if (!cache.TryGetValue(entry, out var cacheItem))
            {
                cacheItem = new StreamingCacheEntry();
                Core.Engine.Console.Write("RDR1Map", entry.Name);
                try
                {
                    var pack = FileManager.LoadFilePack<WsgFile>(entry);
                    var tile = new RDR1GrassGridTile(pack, Map, grid, coord);
                    cacheItem.Object = tile;
                }
                catch { }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            cache[entry] = cacheItem;
            return cacheItem.Object as RDR1GrassGridTile;
        }

        public Entity[] GetSectorBounds(JenkHash hash)
        {
            if (!SectorBoundsCache.TryGetValue(hash, out var cacheItem))
            {
                cacheItem = new StreamingCacheEntry();
                var pack = GetPiecePack(hash, Rpf6FileExt.wbd_wcdt);
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
                    cacheItem.Object = ents.ToArray();
                }
            }
            cacheItem.LastUseFrame = CurrentFrame;
            SectorBoundsCache[hash] = cacheItem;
            return cacheItem.Object as Entity[];
        }

        private void LoadDependencies(PiecePack pack)
        {
            if (pack?.Pieces == null) return;
            var entry = pack?.FileInfo as Rpf6FileEntry;
            if (entry == null) return;
            var name = entry.ShortNameLower;
            var folder = entry.Parent?.NameLower;
            if (string.IsNullOrEmpty(name)) return;
            var dfm = FileManager.DataFileMgr;
            foreach (var restxd in dfm.ResidentTxds)
            {
                foreach (var kvp in pack.Pieces)
                {
                    var dwbl = kvp.Value as Rsc6Drawable;
                    dwbl?.ApplyTextures(restxd);
                }
            }
            var smicmap = dfm?.SmicMapping;
            if (smicmap == null) return;

            smicmap.TryGetValue(name, out var smicnames);
            if (smicnames != null)
            {
                foreach (var smicname in smicnames)
                {
                    var hash = new JenkHash(smicname);
                    var wtd = GetTexturePack(hash) as WtdFile;
                    var txd = wtd?.TextureDictionary;
                    if (txd != null)
                    {
                        foreach (var kvp in pack.Pieces)
                        {
                            var dwbl = kvp.Value as Rsc6Drawable;
                            dwbl?.ApplyTextures(txd);
                        }
                    }
                }
            }
        }

        public Piece GetPiece(JenkHash modelHash)
        {
            var pack = GetPiecePack(modelHash, Rpf6FileExt.wft);
            if (pack == null)
            {
                pack = GetPiecePack(modelHash, Rpf6FileExt.wvd);
            }
            return pack?.Piece;
        }

        public override void Invalidate(string gamepath)
        {
            if (string.IsNullOrEmpty(gamepath)) return;

            Rpf6FileManager.GetRpf6FileHashExt(gamepath, out var hash, out var ext);
            Cache.TryGetValue(ext, out var cache);
            cache?.Remove(hash);

            if (ext == Rpf6FileExt.wbd_wcdt)
            {
                SectorBoundsCache.Remove(hash);
            }

            //TODO:
            //TerrainTileWvdCache.Remove(entry);
            //TerrainTileWtbCache.Remove(entry);
            //TreeGridTileCache.Remove(entry);
            //GrassGridTileCache.Remove(entry);
        }

        public override void SetPinned(string gamepath, bool pinned)
        {
            if (string.IsNullOrEmpty(gamepath)) return;

            Rpf6FileManager.GetRpf6FileHashExt(gamepath, out var hash, out var ext);
            Cache.TryGetValue(ext, out var cache);
            cache?.SetPinned(hash, pinned);

            if (ext == Rpf6FileExt.wbd_wcdt)
            {
                SectorBoundsCache.SetPinned(hash, pinned);
            }

            //TODO:
            //TerrainTileWvdCache.SetPinned(entry, pinned);
            //TerrainTileWtbCache.SetPinned(entry, pinned);
            //TreeGridTileCache.SetPinned(entry, pinned);
            //GrassGridTileCache.SetPinned(entry, pinned);
        }
    }
}