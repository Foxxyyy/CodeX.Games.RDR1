using System;
using CodeX.Core.Engine;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class SstFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6StringTable StringTable;

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
            var e = (Rpf6ResourceFileEntry)FileEntry;
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

        public override string ToString()
        {
            return StringTable.ToString();
        }
    }
}