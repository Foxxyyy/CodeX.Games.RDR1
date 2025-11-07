using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

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
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };

            Fragment = r.ReadBlock<Rsc6Fragment>();
            Pieces = [];

            if (Fragment != null)
            {
                var d = Fragment.Drawable.Item;
                var b = Fragment.Bounds.Item;

                Piece = d;
                Piece.Name = e.Name;
                Piece.FilePack = this;
                Piece.Collider = b;

                if (Piece.Name.StartsWith("st_")) //If we have a tree, let's not switch between lods, low lods have no trunks...
                {
                    Piece.Lods[0].LodDist = 9999.0f;
                }
                Pieces[e.ShortNameHash] = d;
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