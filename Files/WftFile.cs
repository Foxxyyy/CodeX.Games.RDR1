using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;
using System.Text;

namespace CodeX.Games.RDR1.Files
{
    public class WftFile : PiecePack
    {
        public Rsc6Fragment Fragment;
        public JenkHash Hash;
        public string Name;

        public WftFile(Rpf6FileEntry file) : base(file)
        {
            Name = file.NameLower;
            Hash = JenkHash.GenHash(file.NameLower);
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + 0x50000000
            };

            Fragment = r.ReadBlock<Rsc6Fragment>();
            Pieces = new Dictionary<JenkHash, Piece>();

            if (Fragment != null)
            {
                var d = Fragment.Drawable.Item;
                var b = Fragment.Bounds.Item;
                var ped = Fragment.Archetype1.Item?.TypeFlags == Rsc6ObjectTypeFlags.OBJ_SKINNED;

                if (d.IsSkinned() && (Fragment.HasFragLOD || ped))
                {
                    Rpf6Crypto.ResizeBoundsForPeds(d, true);
                }

                Piece = d;
                Piece.Name = e.Name;
                Piece.Collider = b;
                Pieces.Add(e.ShortNameHash, d);
            }
        }

        public override byte[] Save()
        {
            return null;
        }

        //Temporary method
        public string WriteXml(string ddsFolder)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            Xml.OpenTag(sb, 0, "Drawable", true, "");
            Xml.StringTag(sb, 1, "Name", Name.Replace(".wft", ""), "");
            Fragment.Drawable.Item.WriteXml(sb, 1, ddsFolder);
            Xml.CloseTag(sb, 0, "Drawable", true);
            return sb.ToString();
        }
    }
}