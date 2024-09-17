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

        public WftFile()
        {
        }

        public WftFile(Rpf6FileEntry file) : base(file)
        {
        }

        public WftFile(Rsc6Fragment fragment) : base(null)
        {
            Fragment = fragment;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rpf6Crypto.VIRTUAL_BASE
            };

            Fragment = r.ReadBlock<Rsc6Fragment>();
            Pieces = new Dictionary<JenkHash, Piece>();

            if (Fragment != null)
            {
                var d = Fragment.Drawable.Item;
                var b = Fragment.Bounds.Item;

                Piece = d.Drawable;
                Piece.Name = e.Name;
                Piece.FilePack = this;

                if (!RDR1Map.LoadingMap)
                {
                    Piece.Collider = b;
                }
                Pieces[e.ShortNameHash] = d.Drawable;
            }
        }

        public override byte[] Save()
        {
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(Fragment);
            byte[] data = writer.Build(138);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            Fragment = new();
            Fragment.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            Fragment?.Write(writer);
        }
    }
}