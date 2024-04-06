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
using System.IO;
using System.Linq;
using Console = CodeX.Core.Engine.Console;

namespace CodeX.Games.RDR1.Prefabs
{
    public class RDR1Peds
    {
        public Rpf6FileManager FileManager;

        //CodeX stuff
        public List<string> PedNames;
        public string[] WasNames;
        public string[] ClipsNames;
        public Dictionary<string, RDR1PedPrefab> Prefabs = new();
        public SimpleCache<JenkHash, WasFile> WasCache = new();
        public object CacheSyncRoot = new();

        public void Init(Rpf6FileManager fman)
        {
            Console.Write("RDR1Peds", "Initialising Peds...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            FileManager = fman;
            var dfm = fman?.DataFileMgr;
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.generic, out var entries);
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wft, out var fragments);

            Console.Write("RDR1Peds", "Loading Clips names...");
            var rpf = fman.AllArchives.FirstOrDefault(e => e.Name == "animationres.rpf");
            var clipsBin = rpf.AllEntries.FirstOrDefault(e => e.Name == "clips.bin"); 

            if (clipsBin != null)
            {
                LoadClipsNames(fman, clipsBin);
            }

            Console.Write("RDR1Peds", "Building Prefabs...");
            var peds = entries
                .Where(e =>  e.Value.Name.EndsWith(".wfd") && !e.Value.Name.Contains("medlod") && !e.Value.Name.Contains("x_hilod"))
                .Select(e => e.Value.Name.Replace(".wfd", ""))
                .ToList();

            var frags = fragments
                .Where(e => e.Value.Name.EndsWith(".wft")
                        && !e.Value.Name.EndsWith("x.wft")
                        && !e.Value.Name.Contains("anim.wft")
                        && !e.Value.Name.Contains("hat")
                        && !e.Value.Name.Contains("p_gen")
                        && !e.Value.Name.Contains("rocks")
                        &&  e.Value.Name.Contains('_'))
                .Select(e => e.Value.Name.Replace(".wft", ""))
                .ToList();

            PedNames = peds.ToList();
            for (int i = 0; i < frags.Count; i++)
            {
                var f = frags[i];
                if (!PedNames.Any(name => name.Contains(f)))
                {
                    PedNames.Add(f);
                }
            }

            foreach (var name in PedNames)
            {
                Prefabs[name] = new RDR1PedPrefab(this, name);
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
            Console.Write("RDR1Peds", $"Peds initialised. Total time: {totaltime} ms");
        }


        public RDR1PedPrefab GetPrefab(string name)
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
            Console.Write("RDR1Peds", entry.Name);
            if (FileManager.LoadPiecePack(entry, null, true) is not WftFile wft) return null;
            return wft;
        }

        public WfdFile LoadWfd(Rpf6FileEntry entry)
        {
            if (entry == null) return null;
            if (FileManager?.DataFileMgr == null) return null;
            Console.Write("RDR1Peds", entry.Name);
            if (FileManager.LoadPiecePack(entry, null, true) is not WfdFile wfd) return null;
            return wfd;
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
            var was = FileManager.LoadFilePack(entry) as WasFile;

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

    public class RDR1PedPrefab : Prefab
    {
        public JenkHash NameHash;
        public RDR1Peds Peds;
        public Rpf6FileEntry WfdEntry;
        public Rpf6FileEntry WftEntry;

        public RDR1PedPrefab(RDR1Peds peds, string name)
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
        public WasFile Was;
        public string ClipName;
        public static string PreviousWas;

        public RDR1Ped(RDR1PedPrefab prefab, bool buildPiece = true) : base(false)
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
            var peds = Prefab?.Peds;
            if (peds == null) return;

            Wft = peds.LoadWft(Prefab.WftEntry);
            Wfd = peds.LoadWfd(Prefab.WfdEntry);

            var skel = Wfd?.FragDrawable?.Drawable.Item?.Skeleton ?? Wft?.Fragment?.Drawable.Item?.Skeleton;
            SetSkeleton(skel);

            var piece = Wfd?.Piece ?? Wft.Piece;
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

        public string[] GetWasAnimNames(out string defaultval)
        {
            defaultval = null;
            if (!string.IsNullOrEmpty(ClipName)) return null;

            var anims = Was?.AnimSet?.ClipDictionary.Item?.AnimDict.Item?.Animations;
            if (anims == null) return null;

            var list = new List<string>() { "" };    
            foreach (var anim in anims)
            {
                if (anim == null) continue;
                var name = anim.Hash.ToString();
                list.Add(name);
            }
            list.Sort();
            return list.ToArray();
        }

        public void LoadWas(string name)
        {
            var peds = Prefab.Peds;
            if (peds == null || string.IsNullOrEmpty(name))
            {
                if (Animator is not RDR1Animator animator) return;
                animator.Clip = null;
                animator.Anim = null;
                return;
            }

            peds.BeginWasCacheFrame();
            Was = peds.LoadStreamWas(new JenkHash(name));
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

        public void PlayAnim(string name)
        {
            if (Animator is not RDR1Animator animator) return;
            var animdict = Was?.AnimSet?.ClipDictionary.Item?.AnimDict.Item?.Dict;
            var animhash = new JenkHash(name);
            var anim = (Rsc6Animation)null;
            animdict?.TryGetValue(animhash, out anim);
            animator.Anim = anim;
            animator.Clip = null;
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
            var peds = Prefab.Peds;
            if (peds == null) return;
            if (slot == null) return;

            if (slot.Group == "Animations")
            {
                if (slot.Name == "WAS")
                {
                    options = peds.WasNames;
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
            var peds = Prefab.Peds;
            if (peds == null) return;
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