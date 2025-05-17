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

        public WedtFile()
        {
        }

        public WedtFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
        }

        public WedtFile(Rsc6ExpressionDictionary expressions) : base(null)
        {
            Expressions = expressions;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };
            Expressions = r.ReadBlock<Rsc6ExpressionDictionary>();
        }

        public override byte[] Save()
        {
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(Expressions);
            byte[] data = writer.Build(11);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            Expressions = new();
            Expressions.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            Expressions?.Write(writer);
        }
    }
}