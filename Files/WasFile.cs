﻿using System;
using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WasFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6AnimationSet AnimSet;
        public string Name;
        public JenkHash Hash;

        public WasFile(Rpf6FileEntry e)
        {
            FileEntry = e;
            if (FileEntry != null)
            {
                Name = FileEntry.Name;
                Hash = FileEntry.ShortNameHash;
            }
        }

        public override void Load(byte[] data)
        {
            var e = (Rpf6ResourceFileEntry)FileEntry;
            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rpf6Crypto.VIRTUAL_BASE
            };
            AnimSet = r.ReadBlock<Rsc6AnimationSet>();
        }

        public override byte[] Save()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return AnimSet.ToString();
        }
    }
}