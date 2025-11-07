using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WvdFile : PiecePack
    {
        public Rsc6VisualDictionary VisualDictionary;
        public List<Entity> RootEntities;
        public WvdFile Parent;
        public BoundingBox BoundingBox;

        public WvdFile()
        {
        }

        public WvdFile(Rpf6FileEntry file) : base(file)
        {
        }

        public WvdFile(Rsc6VisualDictionary visualDict) : base(null)
        {
            VisualDictionary = visualDict;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };

            VisualDictionary = r.ReadBlock<Rsc6VisualDictionary>();
            Pieces = [];

            if ((VisualDictionary?.Drawables.Items != null) && (VisualDictionary?.Hashes.Items != null))
            {
                var drawables = VisualDictionary.Drawables.Items;
                var hashes = VisualDictionary.Hashes.Items;
                Piece = drawables[0];
                BoundingBox = drawables[0].BoundingBox;

                for (int i = 0; i < drawables.Length; i++)
                {
                    var drawable = drawables[i];
                    if (drawable != null)
                    {
                        drawable.FilePack = this;
                    }

                    var hash = (i < hashes.Length) ? hashes[i] : 0;
                    Pieces[hash] = drawable;
                    BoundingBox = BoundingBox.Merge(BoundingBox, drawable.BoundingBox); //Expand the global bounding box to encompass all pieces
                }
            }
        }

        public override byte[] Save()
        {
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(VisualDictionary);
            byte[] data = writer.Build(133);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            VisualDictionary = new();
            VisualDictionary.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            VisualDictionary?.Write(writer);
        }
    }
}