using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WfdFile : PiecePack
    {
        public Rsc6FragDrawable<Rsc6Drawable> Drawable;
        public static Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary;

        public WfdFile(Rpf6FileEntry file) : base(file)
        {
            
        }

        public WfdFile(Rsc6FragDrawable<Rsc6Drawable> drawable) : base(null)
        {
            Drawable = drawable;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + 0x50000000
            };

            Drawable = r.ReadBlock<Rsc6FragDrawable<Rsc6Drawable>>();
            Pieces = new Dictionary<JenkHash, Piece>();

            if (Drawable != null)
            {
                var d = Drawable.Drawables.Item;

                Rpf6Crypto.ResizeBoundsForPeds(d, false);

                Pieces.Add(e.ShortNameHash, d);
                Piece = d;
            }
        }

        public override byte[] Save()
        {
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(Drawable);
            byte[] data = writer.Build(1);
            return data;
        }
    }
}