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
using System.Numerics;
using Console = CodeX.Core.Engine.Console;

namespace CodeX.Games.RDR1.Prefabs
{
    public class RDR1Peds
    {
        public string[] WasNames;
        public string[] ClipsNames;
        public string[] PedNames;
        public object CacheSyncRoot = new();
        public Rpf6FileManager FileManager;
        public Dictionary<string, RDR1PedPrefab> Prefabs = [];
        public SimpleCache<JenkHash, WasFile> WasCache = new();

        public void Init(Rpf6FileManager fman)
        {
            Console.Write("RDR1Peds", "Initialising Peds...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            FileManager = fman;
            var dfm = fman?.DataFileMgr;
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.generic, out var entries);
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.wft, out var fragments);
            dfm.StreamEntries.TryGetValue(Rpf6FileExt.was, out var anims);

            Console.Write("RDR1Peds", "Loading Clips names...");
            var rpf = fman.AllArchives.FirstOrDefault(e => e.Name == "animationres.rpf");
            var clipsBin = rpf.AllEntries.FirstOrDefault(e => e.Name == "clips.bin");

            if (clipsBin != null)
            {
                LoadClipsNames(fman, clipsBin);
            }

            Console.Write("RDR1Peds", "Building Prefabs...");
            var peds = entries
                .Where(e => e.Value.Name.EndsWith(".wfd") && !e.Value.Name.Contains("medlod") && !e.Value.Name.Contains("x_hilod"))
                .Select(e => e.Value.Name.Replace(".wfd", ""))
                .ToList();

            var frags = fragments
                .Where(e => e.Value.Name.EndsWith(".wft")
                        && !e.Value.Name.EndsWith("x.wft")
                        && !e.Value.Name.Contains("anim.wft")
                        && !e.Value.Name.Contains("hat")
                        && !e.Value.Name.Contains("p_gen")
                        && !e.Value.Name.Contains("rocks")
                        && e.Value.Name.Contains('_'))
                .Select(e => e.Value.Name.Replace(".wft", ""))
                .ToList();

            PedNames = [.. peds];
            var list = PedNames.ToList();
            for (int i = 0; i < frags.Count; i++)
            {
                var f = frags[i];
                if (!list.Any(name => name.Contains(f)))
                {
                    list.Add(f);
                }
            }
            PedNames = [.. list];

            foreach (var name in PedNames)
            {
                Prefabs[name] = new RDR1PedPrefab(this, name);
            }

            if (anims != null)
            {
                WasNames = [.. anims.Where(kv => kv.Value != null).Select(kv => kv.Key.ToString()).Prepend("").Order()];
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
            Slots = [.. slots];
        }

        public override Entity CreateInstance(bool preview, string preset)
        {
            return new RDR1Ped(this);
        }

        private static string GetFragFromFragDrawable(string wfd)
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

            //Turn peds to the sun and the camera
            SetOrientation(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, FloatUtil.HalfPi), false);

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
            piece.Skeleton = skel;
            piece.ImmediateLoad = true;
            piece.UpdateAllModels();
            piece.UpdateBounds();

            SetPiece(piece);
            LoadWas(PreviousWas ?? "gped_idl0");
        }

        public string[] GetWasClipNames(out string defaultval)
        {
            defaultval = null;
            var dict = Was?.AnimSet?.ClipDictionary.Item?.Dict;
            if (dict == null) return null;

            var list = new List<string>() { "" };
            var list1 = new List<string>() { "" };
            foreach (var kvp in dict)
            {
                if (kvp.Value == null) continue;
                var name = kvp.Value.Name;
                list.Add(Path.GetFileName(name));
                list1.Add(name);

                if (name == "gped_idl0")
                {
                    defaultval = "gped_idl0";
                }
                else
                {
                    var firstClip = dict.Keys.First();
                    defaultval = dict[firstClip].ClipName.Value;
                }
            }

            ClipName = defaultval;
            list.Sort();
            list1.Sort();

            /*if (Was.FileInfo.NameLower == "gent.was")
            {
                var sb = new StringBuilder();
                sb.Append("= { ");
                int count = 0;

                foreach (var item in list1)
                {
                    var ttt = item.ToLowerInvariant();
                    if (string.IsNullOrEmpty(ttt) || !ttt.StartsWith("human"))
                    {
                        continue;
                    }

                    var name = Path.GetFileName(ttt);
                    //if (!name.StartsWith("gent_wag_") && !name.StartsWith("gped_wag_"))
                        //continue;

                    if (name.StartsWith("gent_col_") || name.StartsWith("gped_col_"))
                        continue;
                    if (name.StartsWith("gent_crc_") || name.StartsWith("gped_crc_"))
                        continue;
                    if (name.StartsWith("gent_hrs_") || name.StartsWith("gped_hrs_"))
                        continue;
                    if (name.StartsWith("gent_hnd_") || name.StartsWith("gped_hnd_"))
                        continue;
                    if (name.StartsWith("gent_ldg_") || name.StartsWith("gped_ldg_"))
                        continue;
                    if (name.StartsWith("gent_lvl_") || name.StartsWith("gped_lvl_"))
                        continue;
                    if (name.StartsWith("gent_mne_") || name.StartsWith("gped_mne_"))
                        continue;
                    if (name.StartsWith("gent_mel_") || name.StartsWith("gped_mel_"))
                        continue;
                    if (name.StartsWith("gent_nor_") || name.StartsWith("gped_nor_"))
                        continue;
                    if (name.StartsWith("gent_rft_") || name.StartsWith("gped_rft_"))
                        continue;
                    if (name.StartsWith("gent_stg_") || name.StartsWith("gped_stg_"))
                        continue;
                    if (name.StartsWith("gent_wag_") || name.StartsWith("gped_wag_"))
                        continue;

                    var animName = name.Substring(name.IndexOf('_') + 1);
                    sb.Append($", \"{animName}\"");
                    count++;
                }

                sb.Append(" };");
                Debug.WriteLine(sb.ToString());
                Debug.WriteLine(count.ToString() + " items");
            }*/

            return [.. list];
        }

        public string[] GetWasAnimNames(out string defaultval)
        {
            defaultval = null;

            var anims = Was?.AnimSet?.ClipDictionary.Item?.AnimDict.Item?.Animations;
            var animTypes = Was?.AnimSet?.ClipDictionary.Item?.AnimDict.Item?.AnimTypes;
            if (anims == null) return null;

            var list = new List<string>();
            for (int i = 0; i < anims.Length; i++)
            {
                var anim = anims[i];
                if (anim == null) continue;

                var type = animTypes[i];
                if (type != "human") continue;

                var name = anim.RefactoredName.ToString();
                list.Add(name);
            }
            list.Sort();
            return [.. list];
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
            var key = clipdict.Keys.First();

            if (name == "gped_idl0")
            {
                key = 284372659U;
            }

            var clipName = clipdict[key].ClipName.Value;
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

        public void PlayAnim(string animName)
        {
            if (Animator is not RDR1Animator animator) return;
            var animdict = Was?.AnimSet?.ClipDictionary.Item?.AnimDict.Item?.Dict;
            var anim = (Rsc6Animation)null;

            foreach (var kv in animdict)
            {
                if (kv.Value.RefactoredName != animName) continue;
                anim = kv.Value;
            }
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
                    defaultOption = PreviousWas ?? "gped_idl0";
                }
                else if (slot.Name == "Clip")
                {
                    options = GetWasClipNames(out var defaultstr);
                    defaultOption = Path.GetFileName(defaultstr);
                }
                else if (slot.Name == "Anim")
                {
                    options = GetWasAnimNames(out var defaultstr);
                    defaultOption = defaultstr;
                }
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
                else if (slot.Name == "Anim")
                {
                    PlayAnim(option as string);
                }
            }
        }

        public string GetPresetString()
        {
            return null;
        }
    }
}