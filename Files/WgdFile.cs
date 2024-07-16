using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WgdFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6GringoDictionary GringoDict;
        public string Name;
        public JenkHash Hash;

        public WgdFile()
        {
        }

        public WgdFile(Rpf6FileEntry file) : base(file)
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
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rpf6Crypto.VIRTUAL_BASE
            };
            GringoDict = r.ReadBlock<Rsc6GringoDictionary>();
        }

        public override byte[] Save()
        {
            return null;
        }

        public override void Read(MetaNodeReader reader)
        {
            GringoDict = new();
            GringoDict.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            GringoDict?.Write(writer);
        }

        public override string ToString()
        {
            return GringoDict.ToString();
        }
    }
}