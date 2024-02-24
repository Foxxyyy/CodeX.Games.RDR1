using CodeX.Core.Engine;
using CodeX.Core.Physics;
using CodeX.Core.Physics.Vehicles;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CodeX.Games.RDR1.Prefabs
{
    public class RDR1Vehicles
    {
        public Rpf6FileManager FileManager;
        public Dictionary<JenkHash, Rpf6FileEntry> VehiclesStreamWfts = new();
        public Dictionary<JenkHash, Dictionary<string, Rpf6FileEntry>> VehiclesStreamFiles = new();

        //CodeX stuff
        public string[] VehicleNames;
        public Dictionary<string, RDR1VehiclePrefab> Prefabs = new();
        public SimpleCache<JenkHash, WtdFile> WtdCache = new();
        public object WtdCacheSyncRoot = new();

        public void Init(Rpf6FileManager fman)
        {
            Console.Write("RDR1Vehicles", "Initialising Vehicles...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            FileManager = fman;
            var dfm = fman?.DataFileMgr;

            Console.Write("RDR1Vehicles", "Building Prefabs...");
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.binary, out var entries);

            var fragListEntry = entries.FirstOrDefault(entry => entry.Value.Name == "fragmentslist.bin");
            var fragList = ParseFragmentList(fragListEntry.Value);

            var vehicles = entries
                .Where(entry => entry.Value.Name.EndsWith(".vehsim") && fragList.Contains(entry.Value.Name.Replace(".vehsim", "")))
                .Select(entry => entry.Value.Name.Replace(".vehsim", ""))
                .ToList();

            VehicleNames = vehicles.ToArray();
            foreach (var name in VehicleNames)
            {
                Prefabs[name] = new RDR1VehiclePrefab(this, name);
            }

            stopwatch.Stop();
            var totaltime = stopwatch.Elapsed.TotalMilliseconds;
            Console.Write("RDR1Vehicles", $"Vehicles initialised. Total time: {totaltime} ms");
        }

        public RDR1VehiclePrefab GetPrefab(string name)
        {
            Prefabs.TryGetValue(name, out var prefab);
            return prefab;
        }

        private string[] ParseFragmentList(Rpf6FileEntry entry)
        {
            var list = new List<string>();
            var txt = FileManager.GetFileUTF8Text(entry.Path);
            string[] lines = txt.Split('\n');

            foreach (var line in lines)
            {
                if (line == string.Empty) continue;
                var start = line.IndexOf(' ') + 1;
                var end = line.IndexOf(',');
                string fragment = line[start..end];    
                list.Add(fragment);
            }
            return list.ToArray();
        }

        public WftFile LoadWft(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Vehicles", entry.Name);
            if (FileManager.LoadPiecePack(entry, null, true) is not WftFile wft) return null;
            return wft;
        }

        public WfdFile LoadWfd(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Vehicles", entry.Name);
            if (FileManager.LoadPiecePack(entry) is not WfdFile wfd) return null;
            return wfd;
        }

        public WtdFile LoadWtd(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Vehicles", entry.Name);
            if (FileManager.LoadTexturePack(entry) is not WtdFile wtd) return null;
            return wtd;
        }

        public WtdFile LoadStreamWtd(JenkHash hash)
        {
            lock (WtdCacheSyncRoot)
            {
                if (WtdCache.TryGet(hash, out var exwtd))
                {
                    return exwtd;
                }
            }

            var entry = FileManager.DataFileMgr.TryGetStreamEntry(hash, Rpf6FileExt.wtd);
            if (entry == null) return null;

            Console.Write("RDR1Peds", entry.Name);
            var wtd = FileManager.LoadTexturePack(entry) as WtdFile;

            lock (WtdCacheSyncRoot)
            {
                WtdCache.Create(hash, wtd);
            }
            return wtd;
        }

        public void BeginWtdCacheFrame()
        {
            lock (WtdCacheSyncRoot)
            {
                WtdCache.BeginFrame();
            }
        }
    }

    public class RDR1VehiclePrefab : Prefab
    {
        public JenkHash NameHash;
        public RDR1Vehicles Vehicles;
        public Rpf6FileEntry WfdEntry;
        public Rpf6FileEntry WftEntry;
        public Rpf6FileEntry WtdEntry;

        public RDR1VehiclePrefab(RDR1Vehicles vehicles, string name)
        {
            Name = name;
            NameHash = new(name);
            Type = "Ped";
            Vehicles = vehicles;

            var dfman = Vehicles?.FileManager?.DataFileMgr;
            if (dfman == null) return;

            WfdEntry = dfman.TryGetStreamEntry(new(name + "_hilod"), Rpf6FileExt.generic);
            WftEntry = dfman.TryGetStreamEntry(NameHash, Rpf6FileExt.wft);
            WtdEntry = dfman.TryGetStreamEntry(NameHash, Rpf6FileExt.wtd);
        }

        public override Entity CreateInstance(string preset = null)
        {
            return new RDR1Vehicle(this);
        }
    }

    public class RDR1Vehicle : Vehicle, PrefabInstance
    {
        public RDR1VehiclePrefab Prefab;
        public WfdFile Wfd;
        public WftFile Wft;
        public WtdFile Wtd;

        public string[] DrawableNames { get; set; } = new string[12];
        public Rsc6Drawable[] Drawables { get; set; } = new Rsc6Drawable[12];
        public Rsc6Texture[] TexturesD { get; set; } = new Rsc6Texture[12];
        public Rsc6Texture[] TexturesN { get; set; } = new Rsc6Texture[12];
        public Rsc6Texture[] TexturesS { get; set; } = new Rsc6Texture[12];

        public PieceLod[] Lods;

        public RDR1Vehicle(RDR1VehiclePrefab prefab, bool buildPiece = true)
        {
            PhysicsType = PhysicsObjectType.Static;
            DisableMouseSelect = false;

            Prefab = prefab;
            Name = prefab.Name;

            if (buildPiece)
            {
                BuildPiece();
            }
        }

        public void BuildPiece()
        {
            var peds = Prefab?.Vehicles;
            if (peds == null) return;

            peds.BeginWtdCacheFrame();
            Wft = peds.LoadWft(Prefab.WftEntry);
            Wfd = peds.LoadWfd(Prefab.WfdEntry);
            Wtd = peds.LoadWtd(Prefab.WtdEntry);

            var skel = Wft?.Fragment?.Drawable.Item?.Skeleton;
            SetSkeleton(skel);

            //A few models doesn't use a frag drawable
            if (Wfd == null || !Wft.Fragment.HasFragLOD)
                SetPiece(Wft.Piece);
            else
                SetPiece(Wfd.Piece);
            UpdateBounds();
        }

        public Prefab GetPrefab()
        {
            return Prefab;
        }

        public void GetSlotOptions(PrefabSlot slot, out object[] options, out object defaultOption)
        {
            options = null;
            defaultOption = null;
        }

        public void SetSlotOption(PrefabSlot slot, object option)
        {
            return;
        }
    }
}