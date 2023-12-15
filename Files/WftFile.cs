using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Physics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CodeX.Games.RDR1.Files
{
    class WftFile : PiecePack
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
                var b = Fragment.Bound.Item;

                d.Collider = b;
                d.UpdateBounds();

                if (d.BoundingSphere.Center == Vector3.Zero)
                {
                    d.BoundingSphere = new BoundingSphere(d.BoundingBox.Center, d.BoundingBox.Size.Length() * 0.5f);
                }

                Piece = d;
                Piece.Name = e.Name;
                Piece.Collider = b;
                Pieces.Add(e.ShortNameHash, d);
            }
        }

        public override byte[] Save()
        {
            return null;
        }

        public virtual string WriteXml(string ddsfolder)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            Xml.OpenTag(sb, 0, "Fragment", true, "");
            Xml.StringTag(sb, 1, "Name", Name.Replace(".wft", ""), "");
            Fragment.WriteXml(sb, 1, ddsfolder);
            Xml.CloseTag(sb, 0, "Fragment", true);
            return sb.ToString();
        }
    }
}
