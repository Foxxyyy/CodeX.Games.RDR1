using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WatFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6ActionTree ActionTree;
        public string Name;
        public JenkHash Hash;

        public WatFile()
        {
        }

        public WatFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
            Name = file?.NameLower;
            Hash = JenkHash.GenHash(file?.NameLower ?? "");
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };
            ActionTree = r.ReadBlock<Rsc6ActionTree>();
        }

        public override byte[] Save()
        {
            return null;
        }

        public override void Read(MetaNodeReader reader)
        {
            ActionTree = new();
            ActionTree.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            ActionTree?.Write(writer);
        }

        public override string ToString()
        {
            return ActionTree.ToString();
        }
    }
}