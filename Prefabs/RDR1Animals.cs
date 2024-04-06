using CodeX.Core.Engine;
using CodeX.Core.Physics.Characters;
using CodeX.Core.Physics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace CodeX.Games.RDR1.Prefabs
{
    public class RDR1Animals
    {
        public Rpf6FileManager FileManager;

        //CodeX stuff
        public Dictionary<string, RDR1AnimalPrefab> Prefabs = new();
        public object CacheSyncRoot = new();
        public string[] AnimalNames;

        public void Init(Rpf6FileManager fman)
        {
            Console.Write("RDR1Animals", "Initialising Animals...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            FileManager = fman;
            var dfm = fman?.DataFileMgr;
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wft, out var fragments);

            Console.Write("RDR1Animals", "Building Prefabs...");

            var animals = fragments
                .Where(e => e.Value.Name.EndsWith(".wft")
                        && !e.Value.Name.EndsWith("x.wft")
                        && !e.Value.Name.Contains("anim.wft")
                        && !e.Value.Name.Contains("hat")
                        && !e.Value.Name.Contains("p_gen")
                        && !e.Value.Name.Contains("rocks")
                        && !e.Value.Name.Contains('_'))
                .Select(e => e.Value.Name.Replace(".wft", ""))
                .ToList();

            AnimalNames = animals.ToArray();
            foreach (var name in AnimalNames)
            {
                Prefabs[name] = new RDR1AnimalPrefab(this, name);
            }

            stopwatch.Stop();
            var totaltime = stopwatch.Elapsed.TotalMilliseconds;
            Console.Write("RDR1Animals", $"Animals initialised. Total time: {totaltime} ms");
        }

        public RDR1AnimalPrefab GetPrefab(string name)
        {
            Prefabs.TryGetValue(name, out var prefab);
            return prefab;
        }

        public WftFile LoadWft(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Animals", entry.Name);
            if (FileManager.LoadPiecePack(entry, null, true) is not WftFile wft) return null;
            return wft;
        }
    }

    public class RDR1AnimalPrefab : Prefab
    {
        public JenkHash NameHash;
        public RDR1Animals Animals;
        public Rpf6FileEntry WftEntry;

        public RDR1AnimalPrefab(RDR1Animals animals, string name)
        {
            Name = name;
            NameHash = new(name);
            Type = "Animals";
            Animals = animals;

            var dfman = Animals?.FileManager?.DataFileMgr;
            if (dfman == null) return;

            var hash = new JenkHash(name);
            WftEntry = dfman.TryGetStreamEntry(hash, Rpf6FileExt.wft);
        }

        public override Entity CreateInstance(string preset = null)
        {
            return new RDR1Animal(this);
        }
    }

    public class RDR1Animal : Character, PrefabInstance
    {
        public RDR1AnimalPrefab Prefab;
        public WftFile Wft;
        public string ClipName;
        public static string PreviousWas;

        public RDR1Animal(RDR1AnimalPrefab prefab, bool buildPiece = true) : base(false)
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
            var peds = Prefab?.Animals;
            if (peds == null) return;

            Wft = peds.LoadWft(Prefab.WftEntry);

            var skel = Wft?.Fragment?.Drawable.Item?.Skeleton;
            SetSkeleton(skel);

            var piece = Wft.Piece;
            piece.ImmediateLoad = true;

            SetPiece(piece);
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
            
        }
    }
}