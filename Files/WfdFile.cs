using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CodeX.Games.RDR1.Files
{
    public class WfdFile : PiecePack
    {
        public Rsc6FragDrawable<Rsc6Drawable> Drawable;
        public static Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary;
        public string Name;
        public JenkHash Hash;

        public WfdFile(Rpf6FileEntry file) : base(file)
        {
            Name = file.NameLower;
            Hash = JenkHash.GenHash(file.NameLower);
        }

        public WfdFile(string filename)
        {
            Name = filename;
            Hash = JenkHash.GenHash(filename);
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + 0x50000000
            };

            Drawable = r.ReadBlock<Rsc6FragDrawable<Rsc6Drawable>>();
            Pieces = new Dictionary<JenkHash, Piece>();

            if (Drawable != null)
            {
                var d = Drawable.Drawables.Item;

                Rpf6Crypto.ResizeBoundsForPeds(d, false);

                Pieces.Add(e.ShortNameHash, d);
                Piece = d;
            }
        }

        public override byte[] Save()
        {
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(Drawable);
            byte[] data = writer.Build(1);
            return data;
        }

        public Rsc6FragDrawable<Rsc6Drawable> ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null)
                return null;

            var dictionary = new Rsc6FragDrawable<Rsc6Drawable>();
            dictionary.ReadXml(node, ddsfolder);
            return dictionary;
        }

        public string WriteXml(string ddsFolder)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            Xml.OpenTag(sb, 0, "Drawable", true, "");
            Xml.StringTag(sb, 1, "Name", Name.Replace(".wfd", ""), "");
            Drawable.WriteXml(sb, 1, ddsFolder);
            Xml.CloseTag(sb, 0, "Drawable", true);
            return sb.ToString();
        }
    }
}
