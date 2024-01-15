using CodeX.Core.Engine;
using CodeX.Games.RDR1.RPF6;
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
        public override bool GameFolderOk => Directory.Exists(GameFolder);
        public override bool RequiresGameFolder => true;
        public override bool Enabled { get => GameEnabledSetting.GetBool(); set => GameEnabledSetting.Set(value); }
        public override bool EnableMapView => true;
        public override FileTypeIcon Icon => FileTypeIcon.Cowboy;
        public override string HashAlgorithm => "Jenkins";

        public static Setting GameFolderSetting = Settings.Register("RDR1.GameFolder", SettingType.String, @"C:\XboxGames\RDR Nintendo Switch\Dump");
        public static Setting GameEnabledSetting = Settings.Register("RDR1.Enabled", SettingType.Bool, true);

        public override bool CheckGameFolder(string folder)
        {
            return Directory.Exists(folder);
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

        public override Level GetMapLevel()
        {
            return new RDR1Map(this);
        }

        public override Setting[] GetMapSettings()
        {
            return new[] { RDR1Map.StartPositionSetting, RDR1Map.OnlyTerrainSetting, RDR1Map.UseLowestLODSetting };
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
