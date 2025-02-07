using CodeX.Core.Engine;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.Files
{
    public class WsfFile(Rpf6FileEntry file) : TexturePack(file)
    {
        public Rsc6ScaleFormContext TexturesScaleForm;

        public override void Load(byte[] data)
        {
            var e = FileInfo as Rpf6ResourceFileEntry;
            var r = new Rsc6DataReader(e, data);

            TexturesScaleForm = r.ReadBlock<Rsc6ScaleFormContext>();
            Textures = [];

            var swfFile = TexturesScaleForm?.File.Item;
            if (swfFile != null)
            {
                foreach (var obj in swfFile.Directory.Items)
                {
                    if (obj is Rsc6ScaleformFont objFont)
                    {
                        foreach (var sheet in objFont.Sheets)
                        {
                            if (sheet.Item == null) continue;
                            foreach (var tex in sheet.Item.Textures.Items)
                            {
                                if (tex == null) continue;
                                Textures[tex.Name] = tex;
                            }
                        }
                    }
                    else if (obj is Rsc6ScaleFormBitmap objBitmap)
                    {
                        var bmp = objBitmap.Texture.Item;
                        Textures[bmp.Name] = bmp;
                    }
                }
            }
        }

        public override byte[] Save()
        {
            if (TexturesScaleForm == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(TexturesScaleForm);
            byte[] data = writer.Build(33);
            return data;
        }

        public override void BuildFromTextureList(List<Texture> textures)
        {
            base.BuildFromTextureList(textures);
        }
    }
}