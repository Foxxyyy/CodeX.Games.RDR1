using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WtbFile : PiecePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6TerrainBound TileBounds;
        public BoundingBox BoundingBox;

        public WtbFile()
        {
        }

        public WtbFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
        }

        public WtbFile(Rsc6TerrainBound bound) : base(null)
        {
            TileBounds = bound;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };

            TileBounds = r.ReadBlock<Rsc6TerrainBound>();
            Pieces = new Dictionary<JenkHash, Piece>();

            var hashes = TileBounds?.ResourceDict.Item?.Codes.Items;
            var items = TileBounds?.ResourceDict.Item?.Entries.Items;

            if ((items != null) && (hashes != null))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    var bounds = items[i].Bounds.Item;
                    if (bounds != null)
                    {
                        var center = bounds.BoundingBox.Center;
                        if (Math.Abs(center.X) <= 10f && Math.Abs(center.Y) <= 10f && Math.Abs(center.Z) <= 10f)
                        {
                            continue;
                        }

                        BoundingBox = bounds.BoundingBox;
                        var p = new Piece()
                        {
                            Name = bounds.Name,
                            Collider = bounds
                        };

                        p.UpdateBounds();
                        p.Name = e.Name;
                        p.FilePack = this;

                        Pieces.Add(hashes[i], p);
                        BoundingBox = BoundingBox.Merge(BoundingBox, p.BoundingBox); //Expand the global bounding box to encompass all pieces
                    }
                }
            }
        }

        public override byte[] Save()
        {
            if (TileBounds == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(TileBounds);
            byte[] data = writer.Build(36);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            TileBounds = new();
            TileBounds.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            TileBounds?.Write(writer);
        }

        public override string ToString()
        {
            return TileBounds.ToString();
        }
    }
}