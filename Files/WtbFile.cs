using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using SharpDX.Direct2D1.Effects;
using System.Collections.Generic;
using System.Numerics;

namespace CodeX.Games.RDR1.Files
{
    public class WtbFile : PiecePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6TerrainBound TileBounds;
        public BoundingBox BoundingBox;

        public WtbFile(Rpf6FileEntry file) : base(file)
        {
            FileEntry = file;
        }

        public override void Load(byte[] data)
        {
            var e = (Rpf6ResourceFileEntry)FileEntry;
            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + 0x50000000
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
                        if (center.X >= -10f && center.X <= 10f &&
                            center.Y >= -10f && center.Y <= 10f &&
                            center.Z >= -10f && center.Z <= 10f)
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
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return TileBounds.ToString();
        }
    }
}
