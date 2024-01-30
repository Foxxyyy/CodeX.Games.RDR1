using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WedtFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6ExpressionDictionary Expressions;
        public string Name;
        public JenkHash Hash;

        public WedtFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
            Name = file?.NameLower;
            Hash = JenkHash.GenHash(file?.NameLower ?? "");
        }

        public override void Load(byte[] data)
        {
            var e = (Rpf6ResourceFileEntry)FileEntry;
            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + 0x50000000
            };
            Expressions = r.ReadBlock<Rsc6ExpressionDictionary>();
        }

        public override byte[] Save()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return Expressions.ToString();
        }
    }
}