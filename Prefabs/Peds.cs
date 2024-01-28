using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Physics;
using CodeX.Core.Physics.Characters;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace CodeX.Games.RDR1.Prefabs
{
    public class Peds
    {
        public Rpf6FileManager FileManager;
        public Dictionary<JenkHash, Rpf6FileEntry> PedStreamWfts = new();
        public Dictionary<JenkHash, Dictionary<string, Rpf6FileEntry>> PedStreamFiles = new();

        //CodeX stuff
        public string[] PedNames;
        public Dictionary<string, RDR1PedPrefab> Prefabs = new();
        public SimpleCache<JenkHash, WtdFile> WtdCache = new();
        public object WtdCacheSyncRoot = new();

        public void Init(Rpf6FileManager fman)
        {
            Console.Write("RDR1Peds", "Initialising Peds...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            FileManager = fman;
            var dfm = fman?.DataFileMgr;

            Console.Write("RDR1Peds", "Building Prefabs...");
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.generic, out var entries);

            var peds = entries
                .Where(entry => entry.Value.Name.EndsWith(".wfd") && !entry.Value.Name.Contains("medlod"))
                .Select(entry => entry.Value.Name.Replace(".wfd", ""))
                .ToList();

            PedNames = peds.ToArray();
            foreach (var name in PedNames)
            {
                Prefabs[name] = new RDR1PedPrefab(this, name);
            }

            stopwatch.Stop();
            var totaltime = stopwatch.Elapsed.TotalMilliseconds;
            Console.Write("RDR1Peds", $"Peds initialised. Total time: {totaltime} ms");
        }


        public RDR1PedPrefab GetPrefab(string name)
        {
            Prefabs.TryGetValue(name, out var prefab);
            return prefab;
        }

        public WftFile LoadWft(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Peds", entry.Name);
            if (FileManager.LoadPiecePack(entry) is not WftFile wft) return null;
            return wft;
        }

        public WfdFile LoadWfd(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Peds", entry.Name);
            if (FileManager.LoadPiecePack(entry) is not WfdFile wfd) return null;
            return wfd;
        }

        public WtdFile LoadWtd(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Peds", entry.Name);
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

        public void ApplyBaseTextures(Rsc6Drawable drawable, RDR1Ped forPed)
        {
        }
    }

    public class RDR1PedPrefab : Prefab
    {
        public JenkHash NameHash;
        public Peds Peds;
        public Rpf6FileEntry WfdEntry;
        public Rpf6FileEntry WftEntry;
        public Rpf6FileEntry WtdEntry;

        public RDR1PedPrefab(Peds peds, string name)
        {
            Name = name;
            NameHash = new(name);
            Type = "Ped";
            Peds = peds;

            var dfman = Peds?.FileManager?.DataFileMgr;
            if (dfman == null) return;

            var fragmentHash = new JenkHash(GetFragFromFragDrawable(name));
            WfdEntry = dfman.TryGetStreamEntry(NameHash, Rpf6FileExt.generic);
            WftEntry = dfman.TryGetStreamEntry(fragmentHash, Rpf6FileExt.wft);
            WtdEntry = dfman.TryGetStreamEntry(NameHash, Rpf6FileExt.wtd);
        }

        public override Entity CreateInstance(string preset = null)
        {
            return new RDR1Ped(this);
        }

        private string GetFragFromFragDrawable(string wfd)
        {
            var frag = wfd.Replace(".wfd", "");
            if (frag.EndsWith("_hilod"))
            {
                frag = wfd.Replace("_hilod", "");
            }
            return frag;
        }
    }

    public class RDR1Ped : Character, PrefabInstance
    {
        public RDR1PedPrefab Prefab;
        public WfdFile Wfd;
        public WftFile Wft;
        public WtdFile Wtd;

        public string[] DrawableNames { get; set; } = new string[12];
        public Rsc6Drawable[] Drawables { get; set; } = new Rsc6Drawable[12];
        public Rsc6Texture[] TexturesD { get; set; } = new Rsc6Texture[12];
        public Rsc6Texture[] TexturesN { get; set; } = new Rsc6Texture[12];
        public Rsc6Texture[] TexturesS { get; set; } = new Rsc6Texture[12];

        public PieceLod[] Lods;

        public RDR1Ped(RDR1PedPrefab prefab) : base(false)
        {
            PhysicsType = PhysicsObjectType.Static;
            DisableMouseSelect = false;

            Prefab = prefab;
            Name = prefab.Name;

            BuildPiece();
        }

        public void BuildPiece()
        {
            var peds = Prefab?.Peds;
            if (peds == null) return;

            peds.BeginWtdCacheFrame();
            Wft = peds.LoadWft(Prefab.WftEntry);
            Wfd = peds.LoadWfd(Prefab.WfdEntry);
            Wtd = peds.LoadWtd(Prefab.WtdEntry);

            var skel = Wft?.Fragment?.Drawable.Item?.Skeleton;
            SetSkeleton(skel);

            var piece = Wfd.Piece;
            foreach (var model in piece.AllModels)
            {
                foreach (var mesh in model.Meshes)
                {
                    var quaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, FloatUtil.HalfPi);
                    mesh.MeshTransformMode = 1;
                    mesh.MeshTransform = Matrix3x4.CreateTransform(new Vector3(0f, 0f, -2f), quaternion);
                    Rpf6Crypto.ResizeBoundsForPeds(piece, false, true);
                }
            }
            SetPiece(piece);
            UpdateBounds();
        }

        public Prefab GetPrefab()
        {
            return Prefab;
        }
    }
}