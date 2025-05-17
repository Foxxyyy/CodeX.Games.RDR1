using System;
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
        public JenkHash Hash;
        public string Name;
        public bool HasHumanAnim;
        public bool HasAnimalAnim;

        public WasFile()
        {
        }

        public WasFile(Rpf6FileEntry e)
        {
            FileEntry = e;
            FileInfo = e;
            Name = e?.NameLower;
            Hash = JenkHash.GenHash(e?.NameLower ?? "");
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };
            AnimSet = r.ReadBlock<Rsc6AnimationSet>();

            foreach (var type in AnimSet?.ClipDictionary.Item?.AnimDict.Item?.AnimTypes)
            {
                if (type == "human")
                    HasHumanAnim = true;
                else if (type == "animal")
                    HasAnimalAnim = true;
            }
        }

        public override byte[] Save()
        {
            throw new NotImplementedException();
        }

        public override void Read(MetaNodeReader reader)
        {
            AnimSet = new();
            AnimSet.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            AnimSet?.Write(writer);
        }

        public override string ToString()
        {
            return AnimSet.ToString();
        }
    }
}