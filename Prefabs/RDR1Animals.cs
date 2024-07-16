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
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Prefabs
{
    public class RDR1Animals
    {
        public string[] AnimalNames;
        public string[] WasNames;
        public string[] ClipsNames;
        public object CacheSyncRoot = new();
        public Rpf6FileManager FileManager;
        public Dictionary<string, RDR1AnimalPrefab> Prefabs = new();
        public SimpleCache<JenkHash, WasFile> WasCache = new();

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

            var wasEntries = FileManager.DataFileMgr.StreamEntries[Rpf6FileExt.was];
            if (wasEntries != null)
            {
                var wasList = new List<string>() { "" };
                foreach (var was in wasEntries)
                {
                    if (was.Value == null) continue;
                    if (!was.Value.Name.EndsWith(".was")) continue;
                    wasList.Add(was.Value.ShortName);
                }
                wasList.Sort();
                WasNames = wasList.ToArray();
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

        public void LoadClipsNames(Rpf6FileManager fman, GameArchiveEntry entry)
        {
            var txt = fman.GetFileUTF8Text(entry.Path);
            var clips = txt.Split('\0');
            ClipsNames = clips.Where(e => e != string.Empty).ToArray();
        }

        public WftFile LoadWft(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Animals", entry.Name);
            if (FileManager.LoadPiecePack(entry, null, true) is not WftFile wft) return null;
            return wft;
        }

        public WasFile LoadStreamWas(JenkHash hash)
        {
            lock (CacheSyncRoot)
            {
                if (WasCache.TryGet(hash, out var exWas))
                {
                    return exWas;
                }
            }

            var entry = FileManager.DataFileMgr.TryGetStreamEntry(hash, Rpf6FileExt.was);
            if (entry == null) return null;

            Console.Write("RDR1Peds", entry.Name);
            var was = FileManager.LoadFilePack<WasFile>(entry);

            lock (CacheSyncRoot)
            {
                WasCache.Create(hash, was);
            }
            return was;
        }

        public void BeginWasCacheFrame()
        {
            lock (CacheSyncRoot)
            {
                WasCache.BeginFrame();
            }
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

            var slots = new List<PrefabSlot>();
            var wasSlot = new PrefabSlot("WAS", PrefabSlotType.String, "Animations");
            var clipSlot = new PrefabSlot("Clip", PrefabSlotType.String, "Animations") { DependentOnSlot = wasSlot };
            var animSlot = new PrefabSlot("Anim", PrefabSlotType.String, "Animations") { DependentOnSlot = clipSlot };
            slots.Add(wasSlot);
            slots.Add(clipSlot);
            slots.Add(animSlot);
            Slots = slots.ToArray();
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
        public WasFile Was;
        public string ClipName;
        public static string PreviousWas;

        public RDR1Animal(RDR1AnimalPrefab prefab, bool buildPiece = true) : base(false)
        {
            PhysicsType = PhysicsObjectType.Static;
            DisableMouseSelect = false;

            Prefab = prefab;
            Name = prefab.Name;

            Animator = new RDR1Animator
            {
                Target = this
            };

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

            var skel = Wft?.Fragment?.Drawable.Item?.Drawable.Skeleton;
            SetSkeleton(skel);

            var piece = Wft.Piece;
            piece.ImmediateLoad = true;

            SetPiece(piece);
            UpdateBounds();
            LoadWas(PreviousWas ?? "stand_ambient");
        }

        public string[] GetWasClipNames(out string defaultval)
        {
            defaultval = null;
            var dict = Was?.AnimSet?.ClipDictionary.Item?.Dict;
            if (dict == null) return null;

            var list = new List<string>() { "" };
            foreach (var kvp in dict)
            {
                if (kvp.Value == null) continue;
                var name = kvp.Value.Name;
                list.Add(Path.GetFileName(name));

                if (name == "stand_ambient")
                {
                    defaultval = "stand_ambient";
                }
                else
                {
                    var firstClip = dict.Keys.First();
                    defaultval = dict[firstClip].ClipName.Value;
                }
            }

            ClipName = defaultval;
            list.Sort();
            return list.ToArray();
        }

        public void LoadWas(string name)
        {
            var animals = Prefab.Animals;
            if (animals == null || string.IsNullOrEmpty(name))
            {
                if (Animator is not RDR1Animator animator) return;
                animator.Clip = null;
                animator.Anim = null;
                return;
            }

            animals.BeginWasCacheFrame();
            Was = animals.LoadStreamWas(new JenkHash(name));
            PreviousWas = name;

            var clipdict = Was?.AnimSet?.ClipDictionary.Item?.Dict;
            var firstClip = clipdict.Keys.First();
            var clipName = clipdict[firstClip].ClipName.Value;

            PlayClip(clipName);
        }

        public void PlayClip(string name)
        {
            ClipName = name;
            if (Animator is not RDR1Animator animator) return;

            var clip = (Rsc6Clip)null;
            var clipDict = Was?.AnimSet?.ClipDictionary.Item?.Dict;

            if (clipDict != null)
            {
                foreach (var entry in clipDict)
                {
                    var entryName = entry.Value.Name;
                    if (Path.GetFileName(entryName) == Path.GetFileName(ClipName))
                    {
                        clip = entry.Value;
                        break;
                    }
                }
            }
            animator.Clip = clip;
            animator.Anim = null;
        }

        public Prefab GetPrefab()
        {
            return Prefab;
        }

        public void GetSlotOptions(PrefabSlot slot, out object[] options, out object defaultOption)
        {
            options = null;
            defaultOption = null;

            if (Prefab == null) return;
            var animals = Prefab.Animals;
            if (animals == null) return;
            if (slot == null) return;

            if (slot.Group == "Animations")
            {
                if (slot.Name == "WAS")
                {
                    options = animals.WasNames;
                    defaultOption = PreviousWas ?? "stand_ambient";
                }
                else if (slot.Name == "Clip")
                {
                    options = GetWasClipNames(out var defaultstr);
                    defaultOption = Path.GetFileName(defaultstr);
                }
                /*else if (slot.Name == "Anim")
                {
                    options = GetWasAnimNames(out var defaultstr);
                    defaultOption = defaultstr;
                }*/
            }
        }

        public void SetSlotOption(PrefabSlot slot, object option)
        {
            if (Prefab == null) return;
            var animals = Prefab.Animals;
            if (animals == null) return;
            if (slot == null) return;

            if (slot.Group == "Animations")
            {
                if (slot.Name == "WAS")
                {
                    LoadWas(option as string);
                }
                else if (slot.Name == "Clip")
                {
                    PlayClip(option as string);
                }
                /*else if (slot.Name == "Anim")
                {
                    PlayAnim(option as string);
                }*/
            }
        }
    }
}