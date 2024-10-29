using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using System.Collections.Generic;
using CodeX.Games.RDR1.RSC6;

namespace CodeX.Games.RDR1.Files
{
    public class WpflFile : TexturePack
    {
        public Rsc6ParticleEffects ParticleEffects;
        public Dictionary<uint, Rsc6Texture> Dict { get; set; }

        public WpflFile(Rpf6FileEntry file) : base(file)
        {

        }

        public WpflFile()
        {
            ParticleEffects = new Rsc6ParticleEffects();
            Textures = new Dictionary<string, Texture>();
        }

        public WpflFile(Rsc6ParticleEffects td) : base(null)
        {
            ParticleEffects = td;
        }

        public WpflFile(List<Rsc6Texture> textures)
        {
            var list = new List<Texture>();
            foreach (var texture in textures)
            {
                texture.TextureSize = texture.CalcDataSize();
                if (texture is Texture tex)
                {
                    list.Add(tex);
                }
            }
            BuildFromTextureList(list);
        }

        public override void Load(byte[] data)
        {
            var e = FileInfo as Rpf6ResourceFileEntry;
            var r = new Rsc6DataReader(e, data);

            ParticleEffects = r.ReadBlock<Rsc6ParticleEffects>();
            Textures = new Dictionary<string, Texture>();

            var textures = ParticleEffects?.TexturesDict.Item?.Textures.Items;
            if (textures != null)
            {
                foreach (var tex in textures)
                {
                    Textures[tex.Name] = tex;
                    tex.Pack = this;
                }
            }
            BuildDict();
        }

        public override byte[] Save()
        {
            if (ParticleEffects == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(ParticleEffects);
            byte[] data = writer.Build(10);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            ParticleEffects = new();
            ParticleEffects.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            ParticleEffects?.Write(writer);
        }

        public void BuildDict()
        {
            var dict = new Dictionary<uint, Rsc6Texture>();
            var hashes = ParticleEffects?.TexturesDict.Item?.Hashes.Items;
            var textures = ParticleEffects?.TexturesDict.Item?.Textures.Items;

            if (textures != null && hashes != null)
            {
                for (int i = 0; (i < textures.Length) && (i < hashes.Length); i++)
                {
                    var tex = textures[i];
                    var hash = hashes[i];
                    dict[hash] = tex;
                }
            }
            Dict = dict;
        }
    }
}