using CodeX.Core.Engine;
using CodeX.Games.RDR1.RPF6;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeX.Games.RDR1
{
    public class RDR1Game : Game
    {
        public override string Name => "Red Dead Redemption";
        public override string ShortName => "RDR1";
        public override string GameFolder { get => GameFolderSetting.GetString(); set => GameFolderSetting.Set(value); }
        public override string GamePathPrefix => "RDR1\\";
        public override bool RequiresGameFolder => true;
        public override bool Enabled { get => GameEnabledSetting.GetBool(); set => GameEnabledSetting.Set(value); }
        public override bool EnableMapView => true;
        public override FileTypeIcon Icon => FileTypeIcon.Cowboy;
        public override string HashAlgorithm => "Jenkins";

        public static Setting GameFolderSetting = Settings.Register("RDR1.GameFolder", SettingType.String, "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Red Dead Redemption");
        public static Setting GameEnabledSetting = Settings.Register("RDR1.Enabled", SettingType.Bool, true);

        public override bool CheckGameFolder(string folder)
        {
            return Directory.Exists(folder) && File.Exists(folder + "\\rdr.exe");
        }

        public override bool AutoDetectGameFolder(out string source)
        {
            if (AutoDetectFolder(out Dictionary<string, string> matches))
            {
                var match = matches.First();
                source = match.Key;
                GameFolder = match.Value.Trim().TrimEnd('/', '\\', ' ');
                return true;
            }
            source = null;
            return false;
        }

        public override FileManager CreateFileManager()
        {
            return new Rpf6FileManager(this);
        }

        public override PrefabManager CreatePrefabManager()
        {
            return new RDR1Prefabs(this);
        }

        public override Level GetMapLevel()
        {
            return new RDR1Map(this);
        }

        public override Setting[] GetMapSettings()
        {
            return new[]
            {
                RDR1Map.StartPositionSetting,
                RDR1Map.EnabledSetting,
                RDR1Map.EnableInstancedGrass,
                RDR1Map.TreesDistanceSetting,
                RDR1Map.GrassDistanceSetting
            };
        }

        private bool AutoDetectFolder(out Dictionary<string, string> matches)
        {
            matches = new Dictionary<string, string>();
            if (CheckGameFolder(GameFolder))
            {
                matches.Add("Current CodeX Folder", GameFolder);
            }
            return matches.Count > 0;
        }
    }
}