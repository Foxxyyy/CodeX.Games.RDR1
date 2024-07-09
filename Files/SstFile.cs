using System;
using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class SstFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6StringTable StringTable;

        public SstFile()
        {
        }

        public SstFile(Rpf6FileEntry e)
        {
            FileEntry = e;
        }

        public SstFile(Rsc6StringTable stringTable)
        {
            StringTable = stringTable;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rpf6Crypto.VIRTUAL_BASE
            };
            StringTable = r.ReadBlock<Rsc6StringTable>();
        }

        public override byte[] Save()
        {
            if (StringTable == null) return null;
            var w = new Rsc6DataWriter();
            w.WriteBlock(StringTable);
            var data = w.Build(1);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            StringTable = new();
            StringTable.Read(reader);
        }
        public override void Write(MetaNodeWriter writer)
        {
            StringTable?.Write(writer);
        }

        public override string ToString()
        {
            return StringTable.ToString();
        }
    }
}