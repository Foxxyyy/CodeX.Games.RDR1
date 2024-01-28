using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeX.Games.RDR1.Files
{
    public class WtdFile : TexturePack
    {
        public Rsc6TextureDictionary TextureDictionary;
        public Dictionary<uint, Rsc6Texture> Dict { get; set; }

        public WtdFile()
        {
            TextureDictionary = new Rsc6TextureDictionary();
            Textures = new Dictionary<string, Texture>();
        }

        public WtdFile(Rpf6FileEntry file) : base(file)
        {

        }

        public WtdFile(List<Rsc6Texture> textures)
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

            TextureDictionary = r.ReadBlock<Rsc6TextureDictionary>();
            Textures = new Dictionary<string, Texture>();

            if (TextureDictionary?.Textures.Items != null)
            {
                foreach (var tex in TextureDictionary.Textures.Items)
                {
                    Textures[tex.Name] = tex;
                    tex.Pack = this;
                }
            }
            BuildDict();
        }

        public override byte[] Save()
        {
            if (TextureDictionary == null) return null;
            var writer = new Rsc6DataWriter();
            writer.WriteBlock(TextureDictionary);
            byte[] data = writer.Build(10);
            return data;
        }

        public Rsc6Texture Lookup(uint hash)
        {
            Rsc6Texture tex = null;
            Dict?.TryGetValue(hash, out tex);
            return tex;
        }

        public void BuildDict()
        {
            var dict = new Dictionary<uint, Rsc6Texture>();
            if ((TextureDictionary?.Textures != null) && (TextureDictionary?.Hashes != null))
            {
                for (int i = 0; (i < TextureDictionary.Textures.Count) && (i < TextureDictionary.Hashes.Count); i++)
                {
                    var tex = TextureDictionary.Textures[i];
                    var hash = TextureDictionary.Hashes[i];
                    dict[hash] = tex;
                }
            }
            Dict = dict;
        }

        public override void BuildFromTextureList(List<Texture> textures)
        {
            TextureDictionary ??= new Rsc6TextureDictionary();
            var newtexs = new List<Rsc6Texture>();
            var hashes = new List<JenkHash>();

            var sortedNames = (TextureDictionary.Textures.Count > 0)
                ? TextureDictionary.Textures.Items.Select(texture => texture.Name).ToList()
                : textures.Select(texture => texture.Name).ToList();

            //Create a sorted list of textures based on what's already in TextureDictionary
            var sortedTextures = new List<Texture>();
            foreach (var sortedName in sortedNames)
            {
                var matchingTexture = textures.FirstOrDefault(texture => texture.Name == sortedName);
                if (matchingTexture != null)
                {
                    sortedTextures.Add(matchingTexture);
                }
            }

            //Finally, include textures that are not in TextureDictionary
            foreach (var texture in textures)
            {
                if (!sortedNames.Contains(texture.Name) && !sortedTextures.Contains(texture))
                {
                    sortedTextures.Add(texture);
                }
            }
            textures = sortedTextures;

            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i] is not Rsc6Texture rtex)
                {
                    rtex = Rsc6Texture.Create(textures[i]);
                }

                newtexs.Add(rtex);
                hashes.Add(JenkHash.GenHash(Path.GetFileNameWithoutExtension(textures[i].Name.ToLowerInvariant())));
            }

            TextureDictionary.Textures = new Rsc6PtrArr<Rsc6Texture>(newtexs.ToArray());
            TextureDictionary.Hashes = new Rsc6Arr<JenkHash>(hashes.ToArray());

            base.BuildFromTextureList(textures);
            BuildDict();
        }
    }
}
