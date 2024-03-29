﻿using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Text;

namespace CodeX.Games.RDR1.Files
{
    public class WsiFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6SectorInfo StreamingItems;
        public JenkHash Hash;
        public string Name;

        public WsiFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
            Name = file?.NameLower;
            Hash = JenkHash.GenHash(file?.NameLower ?? "");
        }

        public WsiFile(Rsc6SectorInfo si) : base(null)
        {
            StreamingItems = si;
        }

        public override void Load(byte[] data)
        {
            var e = (Rpf6ResourceFileEntry)FileEntry;
            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + 0x50000000
            };
            StreamingItems = r.ReadBlock<Rsc6SectorInfo>();
        }

        public override byte[] Save()
        {
            if (StreamingItems == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(StreamingItems);
            byte[] data = writer.Build(134);
            return data;
        }

        public override string ToString()
        {
            if (StreamingItems == null || StreamingItems.ItemChilds.Item == null || StreamingItems.ItemChilds.Item.Sectors.Items == null)
            {
                return "Invalid data";
            }

            var items = StreamingItems.ItemChilds.Item.Sectors.Items;
            var sb = new StringBuilder();

            int num = 0;
            for (int i = 0; i < items.Length; i++)
            {
                num += items[i].Entities.Items.Length;
            }

            sb.AppendLine($"Number of objects referenced : {num}");
            sb.AppendLine("Sector Name : " + ((StreamingItems.Name.Value == null) ? items[0].Name : StreamingItems.Name.Value));
            sb.AppendLine($"AABB Min : {StreamingItems.BoundMin}");
            sb.AppendLine($"AABB Max : {StreamingItems.BoundMax}\n");

            for (int i = 0; i < items.Length; i++)
            {
                sb.AppendLine($"Child {i + 1}:");
                sb.AppendLine(items[i].ToString());
            }

            if (sb.Length > 0)
            {
                return sb.ToString();
            }
            return "Invalid data";
        }
    }
}
