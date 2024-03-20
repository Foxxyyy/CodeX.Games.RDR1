using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Physics;
using CodeX.Core.Physics.Characters;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Console = CodeX.Core.Engine.Console;

namespace CodeX.Games.RDR1.Prefabs
{
    public class RDR1Peds
    {
        public Rpf6FileManager FileManager;
        public Dictionary<JenkHash, Rpf6FileEntry> PedStreamWfts = new();
        public Dictionary<JenkHash, Dictionary<string, Rpf6FileEntry>> PedStreamFiles = new();

        //CodeX stuff
        public string[] PedNames;
        public string[] WasNames;
        public string[] ClipsNames;
        public Dictionary<string, RDR1PedPrefab> Prefabs = new();
        public SimpleCache<JenkHash, WtdFile> WtdCache = new();
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

            Console.Write("RDR1Peds", "Loading Clips names...");
            var rpf = fman.AllArchives.FirstOrDefault(e => e.Name == "animationres.rpf");
            var clipsBin = rpf.AllEntries.FirstOrDefault(e => e.Name == "clips.bin"); 

            if (clipsBin != null)
            {
                LoadClipsNames(fman, clipsBin);
            }

            Console.Write("RDR1Peds", "Building Prefabs...");
            var peds = entries
                .Where(entry => entry.Value.Name.EndsWith(".wfd") && !entry.Value.Name.Contains("medlod"))
                .Select(entry => entry.Value.Name.Replace(".wfd", ""))
                .ToList();

            PedNames = peds.ToArray();
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
            lock (CacheSyncRoot)
            {
                if (WtdCache.TryGet(hash, out var exwtd))
                {
                    return exwtd;
                }
            }

            var entry = FileManager.DataFileMgr.TryGetStreamEntry(hash, Rpf6FileExt.wtd_wtx);
            if (entry == null) return null;

            Console.Write("RDR1Peds", entry.Name);
            var wtd = FileManager.LoadTexturePack(entry) as WtdFile;

            lock (CacheSyncRoot)
            {
                WtdCache.Create(hash, wtd);
            }
            return wtd;
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

        public void BeginWtdCacheFrame()
        {
            lock (CacheSyncRoot)
            {
                WtdCache.BeginFrame();
            }
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
        public Rpf6FileEntry WtdEntry;

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
            WtdEntry = dfman.TryGetStreamEntry(NameHash, Rpf6FileExt.wtd_wtx);

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
        public WtdFile Wtd;
        public WasFile Was;
        public string ClipName;

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

            peds.BeginWtdCacheFrame();
            Wft = peds.LoadWft(Prefab.WftEntry);
            Wfd = peds.LoadWfd(Prefab.WfdEntry);
            Wtd = peds.LoadWtd(Prefab.WtdEntry);

            var skel = Wfd?.FragDrawable?.Drawable.Item?.Skeleton ?? Wft?.Fragment?.Drawable.Item?.Skeleton;
            SetSkeleton(skel);

            var piece = Wfd?.Piece ?? Wft.Piece;
            foreach (var model in piece.AllModels)
            {
                foreach (var mesh in model.Meshes)
                {
                    var quaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, FloatUtil.HalfPi);
                    mesh.MeshTransformMode = 1;
                    mesh.MeshTransform = Matrix3x4.CreateTransform(new Vector3(0f, 0f, 0f), quaternion);
                    Rpf6Crypto.ResizeBoundsForPeds(piece);
                }
            }

            SetPiece(piece);
            UpdateBounds();
            LoadWas("angrymob"); //stand_ambient
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
                list.Add(name);

                if (name == "angrymob") //stand_ambient
                {
                    defaultval = "angrymob";
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

            var clipdict = Was?.AnimSet?.ClipDictionary.Item?.Dict;
            var firstClip = clipdict.Keys.First();
            var clipName = clipdict[firstClip].ClipName.Value;

            PlayClip(clipName);
        }

        public void PlayClip(string name)
        {
            var actualName = name[(name.LastIndexOf('\\') + 1)..];
            actualName = actualName[(actualName.LastIndexOf('/') + 1)..];

            ClipName = actualName;
            if (Animator is not RDR1Animator animator) return;

            var clip = (Rsc6Clip)null;
            var clipDict = Was?.AnimSet?.ClipDictionary.Item?.Dict;

            if (clipDict != null)
            {
                foreach (var entry in clipDict)
                {
                    if (entry.Value.Name.EndsWith(actualName))
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
                    defaultOption = "angrymob"; //stand_ambient
                }
                else if (slot.Name == "Clip")
                {
                    options = GetWasClipNames(out var defaultstr);
                    defaultOption = defaultstr;
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