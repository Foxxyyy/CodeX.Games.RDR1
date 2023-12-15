using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6TextureDictionary : Rsc6FileBase
    {
        public override ulong BlockLength => 32;
        public Rsc6Ptr<Rsc6BlockMap> BlockMapPointer { get; set; }
        public uint ParentDictionary { get; set; } //Higher level dictionary (inside the resource - usually NULL)
        public uint UsageCount { get; set; } = 1; //Number of references to the object. As soon as it reaches zero, the object is released (it contains 1 inside the resource)
        public Rsc6Arr<JenkHash> Hashes { get; set; } //m_Codes
        public Rsc6PtrArr<Rsc6Texture> Textures { get; set; } //m_Entries

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMapPointer = reader.ReadPtr<Rsc6BlockMap>();
            ParentDictionary = reader.ReadUInt32();
            UsageCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Textures = reader.ReadPtrArr<Rsc6Texture>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            bool wvd = writer.BlockList[0] is Rsc6VisualDictionary<Rsc6Drawable>;
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32((uint)(wvd ? 0x01831108 : wfd ? 0x00ECE5FC : 0x00A9C028));
            writer.WritePtr(BlockMapPointer);
            writer.WriteUInt32(ParentDictionary);
            writer.WriteUInt32(UsageCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Textures);
        }

        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            if (Textures.Items != null)
            {
                Xml.OpenTag(sb, indent, "TextureDictionary");
                foreach (var tex in Textures.Items)
                {
                    Xml.OpenTag(sb, indent + 1, "Item");
                    tex.WriteXml(sb, indent + 2, ddsfolder);
                    Xml.CloseTag(sb, indent + 1, "Item");
                }
                Xml.CloseTag(sb, indent, "TextureDictionary");
            }
        }
    }

    public class Rsc6TextureScaleForm : Rsc6FileBase
    {
        public override ulong BlockLength => 32;
        public const ulong FILE_POSITION = 0x50000000;
        public Rsc6Ptr<Rsc6BlockMap> BlockMapPointer { get; set; }
        public Rsc6Ptr<Rsc6BlockMap> SwfObjectPointer { get; set; }
        public Rsc6Ptr<Rsc6BlockMap> ItemArrayPointer { get; set; }
        public ushort ItemCount { get; set; }
        public List<Rsc6Ptr<Rsc6ScaleformType>> TexturesType = new List<Rsc6Ptr<Rsc6ScaleformType>>();
        public List<Rsc6Texture> Textures = new List<Rsc6Texture>();

        public override void Read(Rsc6DataReader reader)
        {
            reader.Position = FILE_POSITION + 0x2C;
            SwfObjectPointer = reader.ReadPtr<Rsc6BlockMap>();

            reader.Position = SwfObjectPointer.Position + 0x18;
            ItemArrayPointer = reader.ReadPtr<Rsc6BlockMap>();

            reader.Position = SwfObjectPointer.Position + 0x32;
            ItemCount = reader.ReadUInt16();

            reader.Position = ItemArrayPointer.Position;
            for (int i = 0; i < ItemCount; i++)
            {
                var t = reader.ReadPtr<Rsc6ScaleformType>();
                if (t.Item != null && t.Item.TexturesPointers.Count > 0)
                    TexturesType.Add(t);
            }

            for (int i = 0; i < TexturesType.Count; i++)
            {
                if (TexturesType[i].Item.TexturesPointers.Count <= 0)
                    continue;

                for (int c = 0; c < TexturesType[i].Item.TexturesPointers.Count; c++)
                {
                    reader.Position = TexturesType[i].Item.TexturesPointers[c].Position;

                    var tex = new Rsc6Texture();
                    tex.Read(reader);
                    Textures.Add(tex);
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtr(SwfObjectPointer);
            writer.WritePtr(ItemArrayPointer);
            writer.WriteUInt16(ItemCount);

            foreach (var type in TexturesType)
            {
                writer.WritePtr(type);
            }
        }

        public long MemoryUsage
        {
            get
            {
                long val = 0;
                if (Textures != null)
                {
                    foreach (var tex in Textures)
                    {
                        if (tex != null)
                        {
                            val += tex.MemoryUsage;
                        }
                    }
                }
                return val;
            }
        }
    }

    public class Rsc6ScaleformType : Rsc6BlockBase
    {
        public override ulong BlockLength => 32;
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; }
        public TextureType Type { get; set; }
        public List<Rsc6Ptr<Rsc6BlockMap>> TexturesPointers = new List<Rsc6Ptr<Rsc6BlockMap>>();

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Type = (TextureType)reader.ReadByte();

            if (Type == TextureType.FONT)
            {
                ulong offset = reader.Position + 0x9B;
                for (int i = 0; i < 3; i++)
                {
                    reader.Position = offset + (ulong)(i * 8);
                    var dwObjectOffset = reader.ReadPtr<Rsc6BlockMap>();

                    if (dwObjectOffset.Position <= 0)
                        continue;

                    reader.Position = dwObjectOffset.Position;
                    var grcTextureStructureOffset = reader.ReadPtr<Rsc6BlockMap>();

                    if (grcTextureStructureOffset.Position <= 0)
                        continue;

                    reader.Position = dwObjectOffset.Position + 0x14;
                    ushort fontCount = reader.ReadUInt16();

                    for (int c = 0; c < fontCount; c++)
                    {
                        reader.Position = grcTextureStructureOffset.Position + (ulong)(c * 4);
                        TexturesPointers.Add(reader.ReadPtr<Rsc6BlockMap>());
                    }
                }
            }
            else if (Type == TextureType.BITMAP)
            {
                reader.Position += 0x3; //Those 3 bytes are 'pad'
                TexturesPointers.Add(reader.ReadPtr<Rsc6BlockMap>());
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }

        public enum TextureType
        {
            BITMAP = 4,
            FONT = 5
        }
    }

    public class Rsc6Texture : Rsc6TextureBase
    {
        public override ulong BlockLength => 84;
        public byte TextureType { get; set; } //m_ImageType
        public float ColorExpR { get; set; } = 1.0f; //m_ColorExprR
        public float ColorExpG { get; set; } = 1.0f; //m_ColorExprG
        public float ColorExpB { get; set; } = 1.0f; //m_ColorExprB
        public float ColorOfsR { get; set; } = 0.0f; //m_ColorOfsR
        public float ColorOfsG { get; set; } = 0.0f; //m_ColorOfsG
        public float ColorOfsB { get; set; } = 0.0f; //m_ColorOfsB
        public uint PrevTextureOffset { get; set; } = 0xCDCDCDCD;
        public uint NextTextureOffset { get; set; } = 0xCDCDCDCD;
        public Rsc6Ptr<Rsc6TextureData> DataRef { get; set; }
        public uint IsSRBG { get; set; } = 1; //m_IsSRBG

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader); //grcTextureD11
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Format = GetTextureFormat(reader.ReadBytes(4));
            Stride = reader.ReadUInt16();
            TextureType = reader.ReadByte();
            MipLevels = reader.ReadByte();
            ColorExpR = reader.ReadSingle();
            ColorExpG = reader.ReadSingle();
            ColorExpB = reader.ReadSingle();
            ColorOfsR = reader.ReadSingle();
            ColorOfsG = reader.ReadSingle();
            ColorOfsB = reader.ReadSingle();
            PrevTextureOffset = reader.ReadUInt32();
            NextTextureOffset = reader.ReadUInt32();
            DataRef = reader.ReadPtrPtr<Rsc6TextureData>();
            DataRef = reader.ReadPtrItem(DataRef, (_) => new Rsc6TextureData((ulong)CalcDataSize()));
            IsSRBG = reader.ReadUInt32();

            Data = DataRef.Item?.Data;
            Sampler = TextureSampler.AnisotropicWrap;
        }

        public override void Write(Rsc6DataWriter writer)
        {
            DataRef = new Rsc6Ptr<Rsc6TextureData>(new Rsc6TextureData(Data));
            base.Write(writer);
            writer.WriteUInt16(Width);
            writer.WriteUInt16(Height);
            writer.WriteBytes(ConvertTextureFormatToBytes(Format));
            writer.WriteUInt16(Stride);
            writer.WriteByte(TextureType);
            writer.WriteByte(MipLevels);
            writer.WriteSingle(ColorExpR);
            writer.WriteSingle(ColorExpG);
            writer.WriteSingle(ColorExpB);
            writer.WriteSingle(ColorOfsR);
            writer.WriteSingle(ColorOfsG);
            writer.WriteSingle(ColorOfsB);
            writer.WriteUInt32(PrevTextureOffset);
            writer.WriteUInt32(NextTextureOffset);
            writer.WritePtr(DataRef);
            writer.WriteUInt32(IsSRBG);
        }

        public void ReadXml(XmlNode node, string ddsfolder)
        {
            base.ReadXml(node);
            Width = (ushort)Xml.GetChildUIntAttribute(node, "Width", "value");
            Height = (ushort)Xml.GetChildUIntAttribute(node, "Height", "value");
            MipLevels = (byte)Xml.GetChildUIntAttribute(node, "MipLevels", "value");
            Format = Xml.GetChildEnumInnerText<TextureFormat>(node, "Format");

            var filename = Xml.GetChildInnerText(node, "FileName");
            if ((!string.IsNullOrEmpty(filename)) && (!string.IsNullOrEmpty(ddsfolder)))
            {
                var filepath = Path.Combine(ddsfolder, filename);
                if (File.Exists(filepath))
                {
                    try
                    {
                        var dds = File.ReadAllBytes(filepath);
                        var tex = DDSIO.GetTexture(dds);

                        if (tex != null)
                        {
                            Data = tex.Data;
                            Width = tex.Width;
                            Height = tex.Height;
                            Depth = tex.Depth;
                            MipLevels = tex.MipLevels;
                            Format = tex.Format;
                            Stride = tex.Stride;
                        }
                    }
                    catch
                    {
                        throw new Exception("Texture file format not supported:\n" + filepath);
                    }
                }
                else
                {
                    throw new Exception("Texture file not found:\n" + filepath);
                }
            }
        }

        public long MemoryUsage
        {
            get
            {
                long val = 0;
                if (Data != null)
                {
                    val += Data.LongLength;
                }
                return val;
            }
        }

        public int CalcDataSize()
        {
            int fullLength = 0;
            int length = Stride * Height;
            for (int i = 0; i < MipLevels; i++)
            {
                fullLength += length;
                length /= 4;
            }
            return fullLength;
        }

        public static TextureFormat GetTextureFormat(byte[] value)
        {
            return Encoding.ASCII.GetString(value) switch
            {
                "DXT1" => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT1),
                "DXT3" => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT3),
                "DXT5" => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT5),
                "2\0\0\0" => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_L8),
                _ => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_A8R8G8B8) //Likely
            };
        }

        public static byte[] ConvertTextureFormatToBytes(TextureFormat value)
        {
            string format = value switch
            {
                TextureFormat.BC1 => "DXT1",
                TextureFormat.BC2 => "DXT3",
                TextureFormat.L8 => "2\0\0\0",
                TextureFormat.BC3 => "DXT5",
                _ => "DXT5", //Mmh
            };
            return Encoding.ASCII.GetBytes(format);
        }

        public static TextureFormat ConvertToEngineFormat(Rsc6TextureFormat f)
        {
            switch (f)
            {
                default:
                case Rsc6TextureFormat.D3DFMT_DXT1: return TextureFormat.BC1;
                case Rsc6TextureFormat.D3DFMT_DXT3: return TextureFormat.BC2;
                case Rsc6TextureFormat.D3DFMT_DXT5: return TextureFormat.BC3;
                case Rsc6TextureFormat.D3DFMT_A8R8G8B8: return TextureFormat.A8R8G8B8;
                case Rsc6TextureFormat.D3DFMT_L8: return TextureFormat.L8;
            }
        }

        public static Rsc6TextureFormat ConvertToRsc6Format(TextureFormat f)
        {
            switch (f)
            {
                default:
                case TextureFormat.BC1: return Rsc6TextureFormat.D3DFMT_DXT1;
                case TextureFormat.BC2: return Rsc6TextureFormat.D3DFMT_DXT3;
                case TextureFormat.BC3: return Rsc6TextureFormat.D3DFMT_DXT5;
                case TextureFormat.A8R8G8B8: return Rsc6TextureFormat.D3DFMT_A8R8G8B8;
                case TextureFormat.L8: return Rsc6TextureFormat.D3DFMT_L8;
            }
        }

        public static Rsc6Texture Create(Texture t)
        {
            var texture = new Rsc6Texture
            {
                Name = t.Name,
                Sampler = t.Sampler,
                Format = t.Format,
                Width = t.Width,
                Height = t.Height,
                Depth = t.Depth,
                Stride = t.Stride,
                MipLevels = t.MipLevels,
                Data = t.Data,
            };
            texture.TextureSize = texture.CalcDataSize();
            return texture;
        }

        public override string ToString()
        {
            return "Texture: " + Width.ToString() + "x" + Height.ToString() + ": " + Name;
        }
    }

    public class Rsc6TextureBase : Texture, Rsc6Block
    {
        public virtual ulong BlockLength => 32;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; }
        public uint RefCount { get; set; } = 1; //m_RefCount
        public ushort ResourceType { get; set; } //m_ResourceType, 0 for embedded textures or 2 for externals
        public ushort LayerCount { get; set; } //m_LayerCount, mostly 0 or sometimes 5 for interior textures
        public uint Unknown_10h { get; set; }
        public int TextureSize { get; set; } //m_PhysicalSize
        public Rsc6Str NameRef { get; set; }
        public Rsc6Ptr<Rsc6BlockMap> D3DBaseTexture { get; set; }

        public virtual void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            RefCount = reader.ReadUInt32();
            ResourceType = reader.ReadUInt16();
            LayerCount = reader.ReadUInt16();
            Unknown_10h = reader.ReadUInt32();
            TextureSize = reader.ReadInt32();
            NameRef = reader.ReadStr();
            D3DBaseTexture = reader.ReadPtr<Rsc6BlockMap>();
            Name = NameRef.Value;
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            bool wvd = writer.BlockList[0] is Rsc6VisualDictionary<Rsc6Drawable>;
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;

            if (!Name.EndsWith(".dds")) Name += ".dds";
            NameRef = new Rsc6Str(Name);

            if (TextureSize == 0)
                writer.WriteUInt32(0x018489E8); //External texture (for WVD)
            else if(wvd) 
                writer.WriteUInt32(0x01848890); //WVD texture
            else if (wfd)
                writer.WriteUInt32(0x00EE53E4); //WFD texture
            else
                writer.WriteUInt32(0x00AB3704);

            writer.WriteUInt32(Unknown_4h);
            writer.WriteUInt32(RefCount);
            writer.WriteUInt16((ushort)(TextureSize == 0 ? 2 : ResourceType));
            writer.WriteUInt16(LayerCount);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteInt32(TextureSize);
            writer.WriteStr(NameRef);
            writer.WritePtr(D3DBaseTexture);
        }

        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name") + ".dds";
            if (Name.Contains('-')) Name = Name.Replace("-", ":");
            NameRef = new Rsc6Str(Name);
        }

        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            string d3dFormat = Format switch
            {
                TextureFormat.BC1 => "D3DFMT_DXT1",
                TextureFormat.BC2 => "D3DFMT_DXT3",
                TextureFormat.BC3 => "D3DFMT_DXT5",
                TextureFormat.L8 => "D3DFMT_L8",
                _ => "D3DFMT_A8R8G8B8",
            };

            string newTextureName = Name;
            if (newTextureName.Contains(':'))
            {
                newTextureName = newTextureName.Replace(":", "-");
            }

            Xml.StringTag(sb, indent, "Name", Xml.Escape(newTextureName.Replace(".dds", "")));
            Xml.ValueTag(sb, indent, "Unk32", "128");
            Xml.StringTag(sb, indent, "Usage", "DEFAULT");
            Xml.StringTag(sb, indent, "UsageFlags", "UNK24");
            Xml.ValueTag(sb, indent, "ExtraFlags", "0");
            Xml.ValueTag(sb, indent, "Width", Width.ToString());
            Xml.ValueTag(sb, indent, "Height", Height.ToString());
            Xml.ValueTag(sb, indent, "MipLevels", MipLevels.ToString());
            Xml.StringTag(sb, indent, "Format", d3dFormat);
            Xml.StringTag(sb, indent, "FileName", Xml.Escape(newTextureName ?? "null.dds"));

            try
            {
                if (!string.IsNullOrEmpty(ddsfolder))
                {
                    if (!Directory.Exists(ddsfolder))
                    {
                        Directory.CreateDirectory(ddsfolder);
                    }
                    var filepath = Path.Combine(ddsfolder, newTextureName ?? "null.dds");
                    var dds = DDSIO.GetDDSFile(this);
                    File.WriteAllBytes(filepath, dds);
                }
            }
            catch { }
        }

        public override string ToString()
        {
            return "TextureBase: " + Name;
        }
    }

    public class Rsc6TextureData : Rsc6Block
    {
        public ulong BlockLength { get; set; }
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public byte[] Data { get; set; }

        public Rsc6TextureData()
        {

        }

        public Rsc6TextureData(ulong length)
        {
            BlockLength = length;
        }

        public Rsc6TextureData(byte[] data)
        {
            BlockLength = (uint)(data?.Length ?? 0);
            Data = data;
        }

        public void Read(Rsc6DataReader reader)
        {
            Data = reader.ReadBytes((int)BlockLength);
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteBytes(Data);
        }
    }

    public enum Rsc6TextureFormat : uint
    {
        D3DFMT_L8 = 2,
        D3DFMT_DXT1 = 82,
        D3DFMT_DXT3 = 83,
        D3DFMT_DXT5 = 84,
        D3DFMT_A8R8G8B8 = 134
    }
}
