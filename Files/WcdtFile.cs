﻿using CodeX.Core.Engine;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WcdtFile : FilePack
    {
        public Rsc6ClipDictionary Clips;

        public WcdtFile(Rpf6FileEntry info) : base(info)
        {
        }

        public WcdtFile(Rsc6ClipDictionary clips) : base(null)
        {
            Clips = clips;
        }

        public override void Load(byte[] data)
        {
            var e = FileInfo as Rpf6ResourceFileEntry;
            var r = new Rsc6DataReader(e, data);
            Clips = r.ReadBlock<Rsc6ClipDictionary>();
        }

        public override byte[] Save()
        {
            if (Clips == null) return null;
            var w = new Rsc6DataWriter();
            w.WriteBlock(Clips);
            var data = w.Build(58);
            return data;
        }
    }
}