using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WsgFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6SectorGrass GrassField;
        public string Name;
        public JenkHash Hash;

        public WsgFile()
        {
        }

        public WsgFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
            Name = file?.NameLower;
            Hash = JenkHash.GenHash(file?.NameLower ?? "");
        }

        public WsgFile(Rsc6SectorGrass field) : base(null)
        {
            GrassField = field;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };
            GrassField = r.ReadBlock<Rsc6SectorGrass>();
        }

        public override byte[] Save()
        {
            if (GrassField == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(GrassField);
            byte[] data = writer.Build(18);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            GrassField = new();
            GrassField.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            GrassField?.Write(writer);
        }

        public override string ToString()
        {
            return GrassField.ToString();
        }
    }
}