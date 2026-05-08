using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Prefabs;
using CodeX.Games.RDR1.RPF6;
using System;
using System.Collections.Generic;

namespace CodeX.Games.RDR1
{
    public class RDR1Prefabs : PrefabManager
    {
        public Rpf6FileManager FileManager;
        public RDR1Peds Peds;
        public RDR1Animals Animals;
        public RDR1Vehicles Vehicles;

        public RDR1Prefabs(RDR1Game game)
        {
            Game = game;
        }

        public override bool Init()
        {
            FileManager = Game.GetFileManager() as Rpf6FileManager;
            if (FileManager == null) return false;
            return true;
        }

        public override string[] GetTypeList()
        {
            return ["Peds", "Vehicles", "Animals"];
        }

        public override string[] GetPrefabList(string type)
        {
            switch (type)
            {
                case "Peds":
                    EnsurePeds();
                    return Peds?.PedNames;
                case "Vehicles":
                    EnsureVehicles();
                    return Vehicles?.VehicleNames;
                case "Animals":
                    EnsureAnimals();
                    return Animals?.AnimalNames;
            }
            return null;
        }

        public override Prefab GetPrefab(string type, string id)
        {
            return type switch
            {
                "Peds" => Peds?.GetPrefab(id),
                "Vehicles" => Vehicles?.GetPrefab(id),
                "Animals" => Animals?.GetPrefab(id),
                _ => null,
            };
        }

        private void EnsurePeds()
        {
            if (Peds != null) return;
            Peds = new RDR1Peds();
            Peds.Init(FileManager);
        }

        private void EnsureVehicles()
        {
            if (Vehicles != null) return;
            Vehicles = new RDR1Vehicles();
            Vehicles.Init(FileManager);
        }

        private void EnsureAnimals()
        {
            if (Animals != null) return;
            Animals = new RDR1Animals();
            Animals.Init(FileManager);
        }
    }

    public class RDR1AnimEntry
    {
        public string Name;
        public List<string> AnimSets = [];

        public string DefaultAnimSet => (AnimSets.Count > 0) ? AnimSets[0] : null;

        public override string ToString()
        {
            return $"{Name}, {string.Join(", ", AnimSets)}";
        }
    }

    public class RDR1AnimalAnimSet : RDR1AnimEntry
    {
    }

    public class RDR1AnimSetGroup
    {
        public string Name;
        public List<RDR1AnimEntry> Entries = [];
    }

    public class RDR1AnimListFile
    {
        public List<RDR1AnimSetGroup> AllGroups = [];
        public RDR1AnimSetGroup[] CurrentGroups;

        public RDR1AnimListFile()
        {
            var file = StartupUtil.GetFilePath("CodeX.Games.RDR1.animlist.txt");
            if (System.IO.File.Exists(file) == false) return;

            var lines = System.IO.File.ReadAllLines(file);
            if (lines == null) return;

            var delims = new[] { ' ', '\t' };
            var splitopt = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

            var section = Section.None;
            var group = (RDR1AnimSetGroup)null;

            foreach (var line in lines)
            {
                var comment = line.IndexOf("//");
                var tline = (comment >= 0) ? line[..comment] : line;
                var parts = tline.Split(delims, splitopt);

                if (parts == null || parts.Length == 0) continue;
                if (parts[0].StartsWith("group:"))
                {
                    section = Section.None;
                    group = new RDR1AnimSetGroup() { Name = parts[0][6..].ToLowerInvariant() };
                    AllGroups.Add(group);
                }
                else if (parts[0] == "entries:")
                {
                    section = Section.Entries;
                }
                else if (group != null && section == Section.Entries)
                {
                    RDR1AnimEntry entry = null;
                    entry = group.Name switch
                    {
                        "animals" => new RDR1AnimalAnimSet(),
                        _ => new RDR1AnimEntry(),
                    };

                    entry.Name = parts[0].ToLowerInvariant();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var subparts = parts[i].Split(':', splitopt);
                        if (subparts == null || subparts.Length < 2) continue;

                        var fname = subparts[0].ToLowerInvariant();
                        var fval = subparts[1].ToLowerInvariant();

                        switch (fname)
                        {
                            case "animset":
                                foreach (var anim in fval.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                                {
                                    entry.AnimSets.Add(anim.ToLowerInvariant());
                                }
                                break;
                        }
                    }

                    if (entry.AnimSets.Count == 0)
                    {
                        entry.AnimSets.Add(entry.Name);
                    }
                    group.Entries.Add(entry);
                }
            }
            CurrentGroups = [.. AllGroups];
        }

        public string[] GetAnimSets(string animalName)
        {
            if (string.IsNullOrEmpty(animalName)) return null;
            animalName = animalName.ToLowerInvariant();

            foreach (var group in CurrentGroups ?? [])
            {
                foreach (var entry in group.Entries)
                {
                    if (entry.Name == animalName)
                        return [.. entry.AnimSets];
                }
            }
            return null;
        }

        private enum Section
        {
            None,
            Entries
        }
    }
}