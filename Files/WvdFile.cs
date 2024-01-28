using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WvdFile : PiecePack
    {
        public Rsc6VisualDictionary<Rsc6Drawable> VisualDictionary;
        public List<Entity> RootEntities;
        public WvdFile Parent;

        public WvdFile(Rpf6FileEntry file) : base(file)
        {
            
        }

        public WvdFile(Rsc6VisualDictionary<Rsc6Drawable> visualDict) : base(null)
        {
            VisualDictionary = visualDict;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + 0x50000000
            };

            VisualDictionary = r.ReadBlock<Rsc6VisualDictionary<Rsc6Drawable>>();
            Pieces = new Dictionary<JenkHash, Piece>();

            if ((VisualDictionary?.Drawables.Items != null) && (VisualDictionary?.Hashes.Items != null))
            {
                var drawables = VisualDictionary.Drawables.Items;
                var hashes = VisualDictionary.Hashes.Items;

                for (int i = 0; i < drawables.Length; i++)
                {
                    var drawable = drawables[i];
                    var hash = (i < hashes.Length) ? hashes[i] : 0;

                    if (i == 0) Piece = drawable;
                    Pieces[hash] = drawable;
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
    }
}