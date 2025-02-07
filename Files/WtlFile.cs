using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WtlFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6TerrainWorldResource TerrainLod;

        public WtlFile()
        {
        }

        public WtlFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
        }

        public WtlFile(Rsc6TerrainWorldResource terrain) : base(null)
        {
            TerrainLod = terrain;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };
            TerrainLod = r.ReadBlock<Rsc6TerrainWorldResource>();
        }

        public override byte[] Save()
        {
            return null;
        }

        public override void Read(MetaNodeReader reader)
        {
        }

        public override void Write(MetaNodeWriter writer)
        {
            TerrainLod?.Write(writer);
        }

        public override string ToString()
        {
            return TerrainLod.ToString();
        }
    }
}