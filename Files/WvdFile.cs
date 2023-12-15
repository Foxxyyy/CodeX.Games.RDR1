using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CodeX.Games.RDR1.Files
{
    public class WvdFile : PiecePack
    {
        public Rsc6VisualDictionary<Rsc6Drawable> DrawableDictionary;
        public List<Entity> RootEntities;
        public WvdFile Parent;
        public string Name;
        public JenkHash Hash;

        public WvdFile(Rpf6FileEntry file) : base(file)
        {
            Name = file.NameLower;
            Hash = JenkHash.GenHash(file.NameLower);
        }

        public WvdFile(string filename)
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

            DrawableDictionary = r.ReadBlock<Rsc6VisualDictionary<Rsc6Drawable>>();
            Pieces = new Dictionary<JenkHash, Piece>();

            if ((DrawableDictionary?.Drawables.Items != null) && (DrawableDictionary?.Hashes.Items != null))
            {
                var drawables = DrawableDictionary.Drawables.Items;
                var hashes = DrawableDictionary.Hashes.Items;

                for (int i = 0; i < drawables.Length; i++)
                {
                    var drawable = drawables[i];
                    var hash = (i < hashes.Length) ? hashes[i] : 0;

                    if (i == 0) Piece = drawable;
                    Pieces[hash] = drawable;
                }
            }
        }

        public override byte[] Save()
        {
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(DrawableDictionary);
            byte[] data = writer.Build(133);
            return data;
        }

        public Rsc6VisualDictionary<Rsc6Drawable> ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null)
                return null;

            var dictionary = new Rsc6VisualDictionary<Rsc6Drawable>();
            dictionary.ReadXml(node, ddsfolder);
            return dictionary;
        }

        public string WriteXml(string ddsFolder)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            Xml.OpenTag(sb, 0, "Drawable", true, "");
            Xml.StringTag(sb, 1, "Name", Name.Replace(".wvd", ""), "");
            DrawableDictionary.WriteXml(sb, 1, ddsFolder);
            Xml.CloseTag(sb, 0, "Drawable", true);
            return sb.ToString();
        }
    }

    public class XvdEntity : Entity
    {
        public WvdFile Xvd;
        public JenkHash EntityName;

        public XvdEntity()
        {
        }

        public XvdEntity(DataReader r, WvdFile xvd, JenkHash name)
        {
            Xvd = xvd;
            EntityName = name;
        }

        public override string ToString()
        {
            return $"{EntityName}";
        }
    }
}
