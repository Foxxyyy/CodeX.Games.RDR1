using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WbdFile : PiecePack
    {
        public Rsc6BoundsDictionary BoundsDictionary;
        public BoundingBox BoundingBox;

        public WbdFile()
        {
        }

        public WbdFile(Rpf6FileEntry file) : base(file)
        {
        }

        public WbdFile(Rsc6BoundsDictionary boundsDictionary) : base(null)
        {
            BoundsDictionary = boundsDictionary;
        }

        public override void Load(byte[] data)
        {
            var e = FileInfo as Rpf6ResourceFileEntry;
            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };

            BoundsDictionary = r.ReadBlock<Rsc6BoundsDictionary>();
            Pieces = new Dictionary<JenkHash, Piece>();

            var items = BoundsDictionary?.Bounds.Items;
            var hashes = BoundsDictionary?.Hashes.Items;

            if ((items != null) && (hashes != null))
            {
                BoundingBox = items[0].BoundingBox;
                for (int i = 0; i < items.Length; i++)
                {
                    var b = items[i];
                    var h = hashes[i];
                    var p = new Piece()
                    {
                        Name = b.Name,
                        Collider = b
                    };

                    p.UpdateBounds();
                    Pieces.Add(h, p);

                    BoundingBox = BoundingBox.Merge(BoundingBox, p.BoundingBox); //Expand the global bounding box to encompass all pieces
                }
            }
        }

        public override byte[] Save()
        {
            if (BoundsDictionary == null) return null;
            var w = new Rsc6DataWriter();
            w.WriteBlock(BoundsDictionary);
            var data = w.Build(31);
            return data;
        }
    }
}