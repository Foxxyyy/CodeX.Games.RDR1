using CodeX.Core.Engine;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WsfFile : TexturePack
    {
        public Rsc6ScaleFormFile TexturesScaleForm;

        public WsfFile(Rpf6FileEntry file) : base(file)
        {
        }

        public override void Load(byte[] data) //Was made for the Xbox 360 version, I have not changed anything since then
        {
            var e = FileInfo as Rpf6ResourceFileEntry;
            var r = new Rsc6DataReader(e, data);

            TexturesScaleForm = r.ReadBlock<Rsc6ScaleFormFile>();
            Textures = new Dictionary<string, Texture>();

            if (TexturesScaleForm?.Textures != null)
            {
                foreach (var tex in TexturesScaleForm.Textures)
                {
                    Textures[tex.Name] = tex;
                }
            }
        }

        public override byte[] Save()
        {
            var w = new Rsc6DataWriter();
            w.WriteBlock(TexturesScaleForm);
            byte[] data = null;
            return data;
        }

        public override void BuildFromTextureList(List<Texture> textures)
        {
            base.BuildFromTextureList(textures);
        }
    }
}