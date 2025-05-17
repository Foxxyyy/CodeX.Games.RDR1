using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;
using System.Linq;

namespace CodeX.Games.RDR1.Files
{
    public class WsiFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6SectorInfo SectorInfo;
        public JenkHash Hash;
        public string Name;

        public WsiFile()
        {
        }

        public WsiFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
            Name = file?.NameLower;
            Hash = JenkHash.GenHash(file?.NameLower ?? "");
        }

        public WsiFile(Rsc6SectorInfo si) : base(null)
        {
            SectorInfo = si;
        }

        public override void Load(byte[] data)
        {
            FileEntry ??= (Rpf6FileEntry)FileInfo;
            var e = (Rpf6ResourceFileEntry)FileEntry;
            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };
            SectorInfo = r.ReadBlock<Rsc6SectorInfo>();
        }

        public override byte[] Save()
        {
            if (SectorInfo == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(SectorInfo);
            byte[] data = writer.Build(134);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            SectorInfo = new();
            SectorInfo.Read(reader);



            /*

            WHY IS THIS HERE?

            //Set ChildPtrs count for each subsector, only if sagSectorInfo isn't for horse/terrain curves
            var si = SectorInfo;
            if (si?.ChildPtrs.Items == null && si?.ChildGroup.Item != null)
            {
                ushort gsChildCount = 0;
                var secs = si.ChildGroup.Item.Sectors;
                var parents = si.ChildGroup.Item.SectorsParents;

                for (int i = 0; i < secs.Count; i++)
                {
                    ushort childCount = 0;
                    var sec = secs[i];

                    if (sec.Name.ToString() == sec.Scope.ToString())
                    {
                        gsChildCount++;
                    }

                    for (int p = 0; p < parents.Count; p++)
                    {
                        var par = parents[p];
                        if (par.Parent.Item?.Name.ToString() != sec.Name.ToString()) continue;
                        childCount++;
                    }

                    var childs = new Rsc6SectorInfo[childCount];
                    sec.ChildPtrs = new(childs, childCount, childCount);
                }

                var globalChilds = new Rsc6SectorInfo[gsChildCount];
                si.ChildPtrs = new(globalChilds, gsChildCount, gsChildCount);
            }

            */

        }

        public override void Write(MetaNodeWriter writer)
        {
            SectorInfo?.Write(writer);
        }
    }
}
