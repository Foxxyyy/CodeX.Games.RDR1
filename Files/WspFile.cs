using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WspFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6TreeForestGrid Grid { get; set; }
        public string Name;
        public JenkHash Hash;

        public WspFile()
        {
        }

        public WspFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
            Name = file?.NameLower;
            Hash = JenkHash.GenHash(file?.NameLower ?? "");
        }

        public WspFile(Rsc6TreeForestGrid grid) : base(null)
        {
            Grid = grid;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };
            Grid = r.ReadBlock<Rsc6TreeForestGrid>();
        }

        public override byte[] Save()
        {
            if (Grid == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(Grid);
            byte[] data = writer.Build(116);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
        }

        public override void Write(MetaNodeWriter writer)
        {
        }

        public override string ToString()
        {
            return Grid.ToString();
        }
    }
}