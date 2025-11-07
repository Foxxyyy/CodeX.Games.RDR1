using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WnmFile : PiecePack
    {
        public Rsc6Navmesh Navmesh;

        public WnmFile()
        {
        }

        public WnmFile(Rpf6FileEntry file) : base(file)
        {
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };

            Navmesh = r.ReadBlock(reader => Rsc6Navmesh.Create(r));
            Pieces = [];

            if (Navmesh != null)
            {
                Piece = new Piece
                {
                    Name = e.Name,
                    FilePack = this,
                    Navmesh = Navmesh
                };
                Piece.UpdateBounds();
                Pieces.Add(e.ShortNameHash, Piece);
            }
        }

        public override byte[] Save()
        {
            return null;
        }

        public override void Read(MetaNodeReader reader)
        {
            //Navmesh = new();
            //Navmesh.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            //Navmesh?.Write(writer);
        }
    }
}