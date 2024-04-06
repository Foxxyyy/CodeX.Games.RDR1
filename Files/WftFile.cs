using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WftFile : PiecePack
    {
        public Rsc6Fragment Fragment;
        public JenkHash Hash;
        public string Name;

        public WftFile(Rpf6FileEntry file) : base(file)
        {
            Name = file.NameLower;
            Hash = JenkHash.GenHash(file.NameLower);
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + 0x50000000
            };

            Fragment = r.ReadBlock<Rsc6Fragment>();
            Pieces = new Dictionary<JenkHash, Piece>();

            if (Fragment != null)
            {
                var d = Fragment.Drawable.Item;
                var b = Fragment.Bounds.Item;

                Piece = d;
                Piece.Name = e.Name;
                Piece.FilePack = this;

                if (!RDR1Map.LoadingMap)
                {
                    Piece.Collider = b;
                }
                Pieces.Add(e.ShortNameHash, d);
            }
        }

        public override byte[] Save()
        {
            return null;
        }
    }
}