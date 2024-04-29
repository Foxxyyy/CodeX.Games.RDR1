using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WfdFile : PiecePack
    {
        public Rsc6FragDrawable FragDrawable;
        public static Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary;

        public WfdFile(Rpf6FileEntry file) : base(file)
        {
            
        }

        public WfdFile(Rsc6FragDrawable drawable) : base(null)
        {
            FragDrawable = drawable;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rpf6Crypto.VIRTUAL_BASE
            };

            FragDrawable = r.ReadBlock<Rsc6FragDrawable>();
            Pieces = new Dictionary<JenkHash, Piece>();

            if (FragDrawable != null)
            {
                var d = FragDrawable.Drawable.Item;
                Piece = d;
                Piece.FilePack = this;
                Pieces.Add(e.ShortNameHash, d);
            }
        }

        public override byte[] Save()
        {
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(FragDrawable);
            byte[] data = writer.Build(1);
            return data;
        }
    }
}