using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Diagnostics;
using System.Text;

namespace CodeX.Games.RDR1.Files
{
    public class WsiFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6SectorInfo StreamingItems;
        public JenkHash Hash;
        public string Name;

        public WsiFile()
        {
        }

        public WsiFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
            Name = file?.NameLower;
            Hash = JenkHash.GenHash(file?.NameLower ?? "");
        }

        public WsiFile(Rsc6SectorInfo si) : base(null)
        {
            StreamingItems = si;
        }

        public override void Load(byte[] data)
        {
            FileEntry ??= (Rpf6FileEntry)FileInfo;
            var e = (Rpf6ResourceFileEntry)FileEntry;
            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rpf6Crypto.VIRTUAL_BASE
            };
            StreamingItems = r.ReadBlock<Rsc6SectorInfo>();
        }

        public override byte[] Save()
        {
            if (StreamingItems == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(StreamingItems);
            byte[] data = writer.Build(134);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            StreamingItems = new();
            StreamingItems.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            StreamingItems?.Write(writer);
        }
    }
}
