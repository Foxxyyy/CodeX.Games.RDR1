using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6TextureDictionary : Rsc6FileBase, MetaNode
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

        public void Read(MetaNodeReader reader)
        {
            Build(reader.ReadNodeArray<Rsc6Texture>("Textures"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            writer.WriteNodeArray("Textures", Textures.Items);
        }

        public void Build(IEnumerable<Rsc6Texture> textures)
        {
            var list = new List<Tuple<JenkHash, Rsc6Texture>>();
            foreach (var tex in textures)
            {
                list.Add(new Tuple<JenkHash, Rsc6Texture>(JenkHash.GenHash(tex.Name.Replace(".dds", "")), tex)); //Hashes don't have the extension
            }

            var cnt = list.Count;
            var texarr = new Rsc6Texture[cnt];
            var hasharr = new JenkHash[cnt];

            for (int i = 0; i < cnt; i++)
            {
                var item = list[i];
                hasharr[i] = item.Item1;
                texarr[i] = item.Item2;
            }

            Hashes = new Rsc6Arr<JenkHash>(hasharr);
            Textures = new Rsc6PtrArr<Rsc6Texture>(texarr);
        }
    }

    public class Rsc6TextureScaleForm : Rsc6FileBase
    {
        public override ulong BlockLength => 32;
        public const ulong FILE_POSITION = 0x50000000;

        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public uint Stage { get; set; } //m_Stage
        public bool Updating { get; set; } //m_Updating
        public bool IsFileOwner { get; set; } //m_IsFileOwner
        public ushort Unknown_C2h { get; set; } //Always 0?
        public uint[] Unknown_C4h { get; set; } //rage::swfACTIONFUNC, array of 40 uint's
        public int NumFunctions { get; set; } //m_numFunctions
        public Rsc6Str Name { get; set; } //m_Name
        public ushort NameLength1 { get; set; } //m_Length
        public ushort NameLength2 { get; set; } //m_Length + 1

        public Rsc6Ptr<Rsc6BlockMap> SwfObjectPointer { get; set; }
        public Rsc6Ptr<Rsc6BlockMap> ItemArrayPointer { get; set; }
        public ushort ItemCount { get; set; }
        public List<Rsc6Ptr<Rsc6ScaleformType>> TexturesType = new List<Rsc6Ptr<Rsc6ScaleformType>>();
        public List<Rsc6Texture> Textures = new List<Rsc6Texture>();

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();

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
        public Rsc6Ptr<Rsc6TextureCRN> DataCRND { get; set; }
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

            if (Format == TextureFormat.Unknown) //CRND - .wsf
            {
                DataCRND = new Rsc6Ptr<Rsc6TextureCRN>(DataRef.Position);
                DataCRND = reader.ReadPtrItem(DataCRND);

                if (DataCRND.Item?.Data != null)
                {
                    byte[] dds = DataCRND.Item.Data;
                    var tex = DDSIO.GetTexture(dds);
                    Width = tex.Width;
                    Height = tex.Height;
                    Format = tex.Format;
                    Stride = tex.Stride;
                    MipLevels = tex.MipLevels;
                    Data =  tex.Data;
                }
            }
            else Data = DataRef.Item?.Data;
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

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
        }

        public int[] BitsPerPixel = { 4, 8, 8, 8, 8, 8, 8, 8, 8, 4, 4, 4, 8, 4, 8 }; //crn stuff

        public bool IsFormatBlockCompressed(TextureFormat fmt) //crn stuff
        {
            return ((uint)(0xC0000003F007FFFuL >> ((byte)fmt - 70)) & ((uint)(fmt - 70))) < 60;
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
                "CRND" => ConvertToEngineFormat(Rsc6TextureFormat.CRND),
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

        public static TextureFormat ConvertToEngineFormat(Rsc6TextureFormat format)
        {
            return format switch
            {
                Rsc6TextureFormat.D3DFMT_DXT3 => TextureFormat.BC2,
                Rsc6TextureFormat.D3DFMT_DXT5 => TextureFormat.BC3,
                Rsc6TextureFormat.D3DFMT_A8R8G8B8 => TextureFormat.A8R8G8B8,
                Rsc6TextureFormat.D3DFMT_L8 => TextureFormat.L8,
                Rsc6TextureFormat.CRND => TextureFormat.Unknown,
                _ => TextureFormat.BC1,
            };
        }

        public static Rsc6TextureFormat ConvertToRsc6Format(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.BC2 => Rsc6TextureFormat.D3DFMT_DXT3,
                TextureFormat.BC3 => Rsc6TextureFormat.D3DFMT_DXT5,
                TextureFormat.A8R8G8B8 => Rsc6TextureFormat.D3DFMT_A8R8G8B8,
                TextureFormat.L8 => Rsc6TextureFormat.D3DFMT_L8,
                TextureFormat.Unknown => Rsc6TextureFormat.CRND,
                _ => Rsc6TextureFormat.D3DFMT_DXT1,
            };
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

    public class Rsc6TextureCRN : Rsc6Block
    {
        public ulong BlockLength => 74;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public ushort VFT { get; set; } = 0x7848;
        public ushort HeaderSize { get; set; }
        public ushort HeaderCRC16 { get; set; }
        public uint DataSize { get; set; }
        public ushort DataCRC16 { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte Levels { get; set; }
        public byte Faces { get; set; }
        public CRNTextureFormat Format { get; set; }
        public byte[] Data { get; set; }

        [DllImport("crunch.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get_dds_from_crn(byte[] pSrc_file_data, uint src_file_size, out int outBufferSize);

        public void Read(Rsc6DataReader reader)
        {
            var pos = reader.Position;
            reader.Endianess = DataEndianess.BigEndian;
            VFT = reader.ReadUInt16();
            HeaderSize = reader.ReadUInt16();
            HeaderCRC16 = reader.ReadUInt16();
            DataSize = reader.ReadUInt32();
            DataCRC16 = reader.ReadUInt16();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Levels = reader.ReadByte();
            Faces = reader.ReadByte();
            Format = (CRNTextureFormat)reader.ReadByte();
            reader.Endianess = DataEndianess.LittleEndian;
            reader.Position = pos;

            var data = reader.ReadBytes((int)DataSize);
            var resultPtr = get_dds_from_crn(data, (uint)data.Length, out int bufferSize);

            if (resultPtr != IntPtr.Zero)
            {
                Data = new byte[bufferSize];
                Marshal.Copy(resultPtr, Data, 0, bufferSize);
            }
        }

        public void Write(Rsc6DataWriter writer)
        {
            
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

            if (!Name.EndsWith(".dds"))
            {
                Name += ".dds";
            }
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

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            if (Name.Contains('-'))
            {
                Name = Name.Replace("-", ":");
            }
            NameRef = new Rsc6Str(Name);
        }

        public override void Write(MetaNodeWriter writer)
        {
            if (Name.Contains(':'))
            {
                Name = Name.Replace(":", "-");
            }
            base.Write(writer);
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
        CRND = 0,
        D3DFMT_L8 = 2,
        D3DFMT_DXT1 = 82,
        D3DFMT_DXT3 = 83,
        D3DFMT_DXT5 = 84,
        D3DFMT_A8R8G8B8 = 134
    }

    public enum CRNTextureFormat : byte
    {
        DXT1 = 0,
        DXT3 = 1, //Not supported when writing to CRN - only DDS.
        DXT5 = 2,
        DXT5_CCxY = 3, //Luma-chroma
        DXT5_xGxR = 4, //Swizzled 2-component
        DXT5_xGBR = 5, //Swizzled 3-component
        DXT5_AGBR = 6, //Swizzled 4-component
        DXN_A2XY = 7,
        DXN_ATI2 = 8,
        DXN_ATI1 = 9,
        ETC1 = 10,
        ETC2 = 11,
        @T2A = 12,
        ET1S = 13,
        ETAS = 14
    }

    public enum SwfLanguage
    {
        /*
         * flash folder abbreviations:
         * brplu = br + pl + ru
         * efigs = en + fr + it + de + es
        */
        English, //en
        Spanish, //es
        French, //fr
        German, //de
        Italian, //it
        Japanese, //jp
        Chinese_traditional, //cht
        Chinese_simplified, //chs
        Korean, //ko
        Norwegian, //no
        Sapnish_mexican, //mx
        Portugese_brazilian, //bp
        Polish, //pl
        Russian //ru
    }
}
