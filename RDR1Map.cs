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

namespace CodeX.Games.RDR1
{
    public class RDR1Map : StreamingLevel<RDR1Game, RDR1MapFileCache, Rpf6FileManager>
    {
        public WsiFile MainStreamingEntry; //The global #si that lists all the sectors/areas
        public WsiFile TerrainStreamingEntry; //The global terrain #si that lists all the wilderness stuff & some random props
        public RDR1MapStatesFile MapStatesFile;
        public List<WsiFile> StreamingEntries; //All the child sectors grouped together
        public List<Rpf6StoreItem> WvdTilesStore; //Terrain to render
        public List<Rpf6StoreItem> WbdBoundsStore; //Bound dictionaries to render
        public List<Rpf6StoreItem> WtbBoundsStore; //Terrain collisions to render
        public List<TreeItem> Trees; //Basic data of trees, debris, rocks & placed grass

        public Dictionary<JenkHash, RDR1ChildNode> MapNodeDict;
        public StreamingSet<RDR1ChildNode> StreamNodes;

        public static bool LoadingMap = false; //Used to detect if we're using the explorer...
        public static readonly Setting StartPositionSetting = Settings.Register("RDR1Map.Area", SettingType.String, "Blackwater", "Change position", false, false, GetAreas());
        public static readonly Setting LoadBoundsSetting = Settings.Register("RDR1Map.Collisions", SettingType.String, "No collisions", "No collisions", false, false, new string[] { "Near collisions", "Only collisions", "No collisions" });
        public static readonly Setting EnabledSetting = Settings.Register("RDR1Map.Enabled", SettingType.Bool, true, true);
        public static readonly Setting EnableTreesSetting = Settings.Register("RDR1Map.EnableTrees", SettingType.Bool, true);
        public static readonly Setting EnablePropsSetting = Settings.Register("RDR1Map.EnableProps", SettingType.Bool, true);
        public static readonly Setting EnableInstancedGrass = Settings.Register("RDR1Map.EnableInstancedGrass", SettingType.Bool, true);
        public static readonly Setting EnableLightsSetting = Settings.Register("RDR1Map.EnableLights", SettingType.Bool, false);

        public static readonly Setting TreesDistanceSetting = Settings.Register("RDR1Map.TreesDistance", SettingType.Float, 100.0f, false);
        public static readonly Setting GrassDistanceSetting = Settings.Register("RDR1Map.GrassDistance", SettingType.Float, 50.0f, false);
        
        public Statistic NodeCountStat = Statistics.Register("RDR1Map.NodeCount", StatisticType.Counter);
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
            LoadingMap = true;

            this.Game = game;
            this.DefaultSpawnPoint = position;
            this.BoundingBox = new BoundingBox(new Vector3(-100000.0f), new Vector3(100000.0f));
            this.InitPhysicsSim();

            Commands.Register("ReloadMapStates", "Reload mapstates.txt file", ReloadMapStatesCmd, game);
            Commands.Register("EnableWSI", "Enable or disable the specified WSI", EnableWsiCmd, game);
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
            this.StreamNodes = new StreamingSet<RDR1ChildNode>();
            this.StreamPosition = this.DefaultSpawnPoint;
            this.StreamingEntries = new List<WsiFile>();
            this.MapNodeDict = new Dictionary<JenkHash, RDR1ChildNode>();

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

            if (colMode != "No collisions")
            {
                this.LoadWbdBounds(dfm);
                this.WtbBoundsStore = this.FileManager.Store.TerritoryBoundsDict.Values.ToList();

                if (colMode == "Only collisions")
                {
                    this.StreamPhysicsRange = 1000.0f;
                }
            }

            if (colMode != "Only collisions")
            {
                this.LoadWvdTiles(dfm);
                if (EnableTreesSetting.GetBool())
                {
                    foreach (var wsp in dfm.WspFiles)
                    {
                        var node = new RDR1ChildNode(wsp.Value, this);
                        this.MapNodeDict[node.NameHash] = node;
                    }
                }

                foreach (var child in this.StreamingEntries)
                {
                    if (child.Name.StartsWith("cs") || child.Name.StartsWith("sw")) continue;
                    var nodes = new RDR1MapNode(child);

                    foreach (var node in nodes.ChildNodes)
                    {
                        this.MapNodeDict[node.NameHash] = node;
                    }
                }

                foreach (var tile in this.WvdTilesStore)
                {
                    var node = new RDR1ChildNode(tile);
                    this.MapNodeDict[node.NameHash] = node;
                }

                this.LoadMapStates();
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
            if (this.StreamNodes == null || !EnabledSetting.GetBool()) return false;

            this.StreamNodes.BeginIteration();
            var nodes = this.StreamNodes.CurrentSet;
            var ents = this.StreamEntities.CurrentSet;
            var spos = this.StreamPosition;

            this.StreamBVH.BeginIteration();
            this.StreamBVH.AddStreamPosition(spos);

            foreach (var node in this.StreamBVH.StreamItems.Cast<RDR1ChildNode>())
            {
                if (node.NameHash == 0 || !node.Enabled) continue;
                if (!nodes.Contains(node))
                {
                    nodes.Add(node);
                }
            }

            if (EnableInstancedGrass.GetBool())
            {
                var dist = GrassDistanceSetting.GetFloat();
                foreach (var wsg in this.FileManager.DataFileMgr.WsgFiles)
                {
                    foreach (var item in wsg.Value.GrassField.GrassItems.Items)
                    {
                        if (Vector3.Distance(item.GetAABB().Center, StreamPosition) > dist) continue;
                        foreach (var grassPos in item.GrassPositions)
                        {
                            ents.Add(new RDR1GrassEntity(item.Name.ToString(), dist, grassPos));
                        }
                    }
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
                JenkHash hash = ent switch
                {
                    RDR1GridForestEntity treeEnt => treeEnt.TreeName,
                    RDR1GrassEntity grassEnt => new JenkHash("p_" + grassEnt.Name + "x"),
                    RDR1LightEntity lightEnt => lightEnt.ModelName,
                    WsiEntity wsiEnt => string.IsNullOrEmpty(ent.Name) ? wsiEnt.ModelName : new JenkHash(ent.Name),
                    _ => 0u,
                };

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
                        ent.Batch?.StreamingUpdate(spos);

                        //If we have a tree, let's not switch between lods, low lods have no trunks...
                        if (ent is RDR1GridForestEntity ft)
                        {
                            ft.Piece.Lods[0].LodDist = 9999.0f;
                        }
                    }
                }
                else if (ent is WsiEntity || ent is RDR1LightEntity)
                {
                    var parent = (ent is WsiEntity wsiEnt) ? wsiEnt.ParentName : ((RDR1LightEntity)ent).ParentName;
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

            //Collision bounds checks
            ProcessCollisionBounds(this.WbdBoundsStore, spos, true);
            ProcessCollisionBounds(this.WtbBoundsStore, spos, false);

            this.NodeCountStat.SetCounter(nodes.Count);
            this.EntityCountStat.SetCounter(ents.Count);

            this.StreamNodes.EndIteration();
            if (needsAnotherUpdate) this.StreamUpdateRequest = true;

            return true;
        }

        private static void RecurseAddStreamEntity(Entity e, ref Vector3 spos, HashSet<Entity> ents)
        {
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
            return new[] { "Armadillo", "Blackwater", "Beecher's Hope", "Chuparosa", "Cochinay", "El Presidio", "El Matadero", "Escalera", "Fort Mercer", "Gaptooth Breach", "Las Hermanas", "MacFarlane's Ranch", "Manzanita Post", "Nosalida", "Pacific Union RR Camp", "Plainview", "Rathskeller Fork", "Ridgewood Farm", "Tesoro Azul", "Thieve's Landing", "Torquemada", "Tumbleweed", "Twin Rocks" };
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

        private void ProcessCollisionBounds(List<Rpf6StoreItem> boundsStore, Vector3 spos, bool wbd)
        {
            if (boundsStore != null)
            {
                var collsRng = new Vector3(this.StreamPhysicsRange);
                var collsBox = new BoundingBox(spos - collsRng, spos + collsRng);
                foreach (var bnd in boundsStore)
                {
                    if (bnd.Box.Contains(ref collsBox) == ContainmentType.Intersects)
                    {
                        var bounds = this.Cache.GetStaticBounds(bnd.Hash, wbd);
                        if (bounds != null)
                        {
                            foreach (var b in bounds)
                            {
                                if (b != null) this.StreamPhysics.Add(b);
                            }
                        }
                    }
                }
            }
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

            var ents = proj.View.ViewEntities ?? this.StreamEntities.ActiveSet;
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
            var fragments = this.FileManager?.DataFileMgr?.StreamEntries[Rpf6FileExt.wft];
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

            this.MapNodeDict.TryGetValue(nameHash, out var mapnode);
            if (mapnode == null && enable)
            {
                FileManager.DataFileMgr.WsiFiles.TryGetValue(nameHash, out var entry);
                if (entry == null) return;
                var nodes = new RDR1MapNode(entry);

                foreach (var node in nodes.ChildNodes)
                {
                    this.MapNodeDict[node.NameHash] = node;
                    this.StreamBVH?.Add(node);
                }
            }

            if (mapnode != null)
            {
                mapnode.Enabled = enable;
                if (!subsector)
                {
                    foreach (var node in mapnode.ParentNode?.ChildNodes)
                    {
                        node.Enabled = enable;
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

            this.StreamInvoke(() =>
            {
                this.EnableWSI(wsi, enable);
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
                this.EnableWSI(wsiName, wsiEnable);
            }
        }

        private void LoadMapStates()
        {
            this.MapStatesFile = new RDR1MapStatesFile(this);
            if (this.MapStatesFile.CurrentGroups != null)
            {
                foreach (var group in this.MapStatesFile.CurrentGroups)
                {
                    this.EnableMapStateGroup(group, group.Enabled);
                }
            }
        }

        private string ReloadMapStatesCmd()
        {
            this.StreamInvoke(() =>
            {
                this.LoadMapStates();
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

            public override void SetChildEnabled(BaseObject obj, BaseObject child, bool enabled)
            {
                if (obj is not RDR1Map map) return;
                if (child is not RDR1MapStateGroup group) return;

                map.StreamInvoke(() =>
                {
                    map.EnableMapStateGroup(group, enabled);
                });
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
        public BoundingBox StreamingBox { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public RDR1MapData[] MapData;
        public RDR1ChildNode[] ChildNodes;
        public WsiFile Scope;
        public bool Enabled;
        public JenkHash NameHash;

        public RDR1MapNode(WsiFile wsi) //Buildings, props, etc...
        {
            this.Scope = wsi;
            var childs = this.GetChilds();

            if (childs != null)
            {
                this.MapData = new RDR1MapData[childs.Length];
                for (int i = 0; i < this.MapData.Length; i++)
                {
                    var child = childs[i];
                    var parent = Rsc6SectorInfo.GetSectorParent(childs, child);
                    this.MapData[i] = new RDR1MapData(this.Scope, child, parent);
                }
            }

            if (this.MapData != null)
            {
                this.ChildNodes = new RDR1ChildNode[this.MapData.Length];
                for (int i = 0; i < this.ChildNodes.Length; i++)
                {
                    var child = this.MapData[i];
                    this.ChildNodes[i] = new RDR1ChildNode()
                    {
                        MapData = child,
                        NameHash = child.ToString(),
                        ParentNode = this,
                        BoundingBox = child.BoundingBox,
                        StreamingBox = child.StreamingBox
                    };
                }
            }

            this.NameHash = this.Scope.FileEntry.ShortNameHash;
            this.Enabled = true;
        }

        public Rsc6SectorInfo[] GetChilds() //sagScopedSectors.Sectors + Rsc6SectorInfo[]
        {
            var scopedChilds = this.Scope?.StreamingItems?.ItemChilds.Item?.Sectors.Items?.Where(s => s != null);
            var childs = this.Scope?.StreamingItems?.ChildPtrs.Items?.Where(c => c != null);

            if (scopedChilds == null && childs == null) return null;
            return (scopedChilds ?? Enumerable.Empty<Rsc6SectorInfo>())
                .Concat(childs ?? Enumerable.Empty<Rsc6SectorInfo>())
                .ToArray();
        }

        public static bool IsTerrainSector(WsiFile wsi) //If the map node is running from swTerrain
        {
            return wsi?.FileEntry?.Archive?.Name == "swterrain.rpf" == true;
        }
    }

    public class RDR1ChildNode : StreamingBVHItem
    {
        public BoundingBox StreamingBox { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public RDR1MapNode ParentNode;
        public RDR1BaseMapData MapData;
        public JenkHash NameHash;
        public bool Enabled;
        public bool IsPinned;
        public bool IsProjectNode;

        public RDR1ChildNode() //Buildings, props, etc...
        {
            Enabled = true;
        }

        public RDR1ChildNode(WspFile wsp, RDR1Map map) //Trees
        {
            MapData = new RDR1TreeMapData(wsp, map);
            NameHash = wsp.FileEntry.ShortNameHash;
            BoundingBox = MapData.BoundingBox;
            StreamingBox = MapData.StreamingBox;
            Enabled = true;
        }

        public RDR1ChildNode(Rpf6StoreItem item) //Tiles
        {
            MapData = new RDR1TerrainMapData(item);
            NameHash = item.Hash;
            BoundingBox = MapData.BoundingBox;
            StreamingBox = MapData.StreamingBox;
            Enabled = true;
        }

        public override string ToString()
        {
            return NameHash.ToString();
        }
    }

    public class RDR1MapData : RDR1BaseMapData
    {
        public Rsc6SectorInfo ParentSector;
        public Rsc6SectorInfo Sector;
        public WsiFile Wsi;

        public RDR1MapData(WsiFile wsi, Rsc6SectorInfo currentSector, Rsc6SectorInfo parentSector)
        {
            this.Wsi = wsi;
            this.FilePack = wsi;
            this.FilePack.EditorObject = this; //Allow the editor access to this
            this.Name = currentSector.SectorNameLower.ToString();
            this.Sector = currentSector;
            this.ParentSector = parentSector;
            this.BoundingBox = wsi.StreamingItems.Bounds;

            var width = this.BoundingBox.Width;
            var dist = new Vector3((width > 700.0f) ? width : 700.0f);
            this.StreamingBox = new BoundingBox(this.BoundingBox.Center - dist, this.BoundingBox.Center + dist);

            this.GetEntities();
        }

        public void GetEntities()
        {
            var name = this.Sector.Name.ToString().ToLower();
            if (name == this.Wsi.StreamingItems.Scope.ToString() ||
                name.Contains("_ffa01x") ||
                name.Contains("_base01x")) return; //Deathmatch props

            if (RDR1MapNode.IsTerrainSector(this.Wsi))
            {
                name += "_non-terrain";
            }

            //Add the sector as a entity
            this.AddSectorEntity(name);

            //Add the sector's entities
            if (RDR1Map.EnablePropsSetting.GetBool())
            {
                this.AddEntitiesFromSector(name);
            }

            //Add the sector's lights
            if (RDR1Map.EnableLightsSetting.GetBool())
            {
                this.AddLightsFromSector();
            }

            //Link each entity to its parent sector
            this.LinkEntitiesToParent();
        }

        public override void PreSaveUpdate()
        {
            base.PreSaveUpdate();
            this.UpdateEntitiesArray();
        }

        private void LinkEntitiesToParent()
        {
            for (int i = 1; i < this.Entities.Count; i++)
            {
                var entity = this.Entities[i];
                var parent = this.Entities[0]; //Assuming the first entity is the parent

                entity.LodParent = parent;
                entity.LodParent?.AddLodChild(entity);
            }
        }

        private void AddSectorEntity(string name)
        {
            var sectorEnt = new WsiEntity
            {
                LodDistMax = 750.0f,
                ModelName = new(name),
                Position = this.Sector.Bounds.Center,
                Index = Entities.Count,
                ResetPos = true,
                Level = this
            };
            this.Add(sectorEnt);
        }

        private void AddLightsFromSector()
        {
            var lights = this.Sector.PlacedLightsGroup.Item;
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

        private void AddEntitiesFromSector(string parentName)
        {
            var entities = this.Sector.Entities.Items;
            if (entities != null && entities.Length > 0)
            {
                foreach (var instance in entities)
                {
                    var e = new WsiEntity(instance)
                    {
                        Index = Entities.Count,
                        ParentName = parentName,
                        Level = this
                    };
                    this.Add(e);
                }
            }

            var drawableInstances = this.Sector.DrawableInstances.Items;
            if (drawableInstances != null && drawableInstances.Length > 0)
            {
                var ident = Matrix4x4.Identity;
                ident.M44 = 0.0f;

                foreach (var instance in drawableInstances)
                {
                    if (instance.Name.ToString().ToLower() == parentName) continue;

                    var e = new WsiEntity(instance)
                    {
                        Index = Entities.Count,
                        ParentName = parentName,
                        Level = this,
                    };

                    if (instance.Matrix.Equals(ident))
                    {
                        var bb = new BoundingBox(instance.BoundingBoxMin.XYZ(), instance.BoundingBoxMax.XYZ());
                        e.Position = bb.Center;
                        //e.ResetPos = true;
                    }
                    this.Add(e);
                }
            }
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
                var newEntities = kvp.Value.ToArray();
                if (this.ParentSector != null)
                {
                    this.ParentSector.Entities = new(newEntities);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class RDR1TreeMapData : RDR1BaseMapData
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
                    var entity = RDR1GridForestEntity.CreateFromGrid(inst, gridCell, hash, RDR1Map.TreesDistanceSetting.GetFloat());
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
                    var entity = RDR1GridForestEntity.CreateFromGrid(inst, gridCell, hash, RDR1Map.TreesDistanceSetting.GetFloat());
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
                    if (ent.ParentInParentLevel || (ent.ParentIndex < 0)) RootEntities.Add(ent);
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

        //Returns the correct hash for a tree...
        //R* use different params to generate them, with filename or path of a speedtree file, seed, size, size variance.
        private static JenkHash GetHashTree(string treeName)
        {
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

            var pack = TryGetPiecePack(packHash);
            if (pack == null)
            {
                return null;
            }
            return ExtractPieces(pack, pieceHash);
        }

        private PiecePack TryGetPiecePack(JenkHash packHash)
        {
            var pack = GetPiecePack(packHash, Rpf6FileExt.wft);
            pack ??= GetPiecePack(packHash, Rpf6FileExt.wvd);
            return pack;
        }

        private static List<Piece> ExtractPieces(PiecePack pack, JenkHash pieceHash)
        {
            var pieces = new List<Piece>();
            if (pack?.Pieces != null)
            {
                pack.Pieces.TryGetValue(pieceHash, out var piece);
                if (piece != null)
                {
                    pieces.Add(piece);
                }
                else if (pack.Pieces.Count == 4) //High terrain contains 4 pieces
                {
                    pieces.AddRange(pack.Pieces.Values);
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