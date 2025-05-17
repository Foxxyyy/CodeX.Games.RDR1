using System;
using System.Collections.Generic;
using CodeX.Core.Engine;
using CodeX.Core.Utilities;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6TextureDictionary : Rsc6BlockBaseMap, MetaNode
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x00A9C028;
        public uint ParentDictionary { get; set; } //Higher level dictionary (inside the resource - usually NULL)
        public uint UsageCount { get; set; } = 1; //Number of references to the object. As soon as it reaches zero, the object is released (it contains 1 inside the resource)
        public Rsc6Arr<JenkHash> Hashes { get; set; } //m_Codes
        public Rsc6PtrArr<Rsc6Texture> Textures { get; set; } //m_Entries

        public Dictionary<JenkHash, Rsc6Texture> Dict = [];
        public Dictionary<string, Rsc6Texture> DictStr = [];

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ParentDictionary = reader.ReadUInt32();
            UsageCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Textures = reader.ReadPtrArr<Rsc6Texture>();

            if (Textures.Items != null)
            {
                var imax = Math.Min(Hashes.Items.Length, Textures.Items.Length);
                for (int i = 0; i < Textures.Items.Length; i++)
                {
                    var t = Textures.Items[i];
                    var h = (i < imax) ? Hashes.Items[i] : 0;

                    Dict[h] = t;
                    var n = t?.Name;

                    if (!string.IsNullOrEmpty(n))
                    {
                        var si = n.LastIndexOf(':');
                        if ((si >= 0) && (si < (n.Length - 1)))
                        {
                            n = n[(si + 1)..];
                        }
                        DictStr[n] = t;
                    }
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            bool wvd = writer.BlockList[0] is Rsc6VisualDictionary;
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable;

            if (wvd)
                VFT = 0x01831108;
            else if (wfd)
                VFT = 0x00ECE5FC;

            base.Write(writer);
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
                list.Add(new Tuple<JenkHash, Rsc6Texture>(JenkHash.GenHash(tex.Name.Replace(".dds", "")), tex)); //Hashes don't use the extension
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

    public class Rsc6Texture : Rsc6TextureBase
    {
        public override ulong BlockLength => base.BlockLength + 52;
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
        public uint IsSRBG { get; set; } //m_IsSRBG

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
            DataRef = reader.ReadPtrOnly<Rsc6TextureData>();

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
                    Data = tex.Data;
                }
            }
            else
            {
                Data = DataRef.Item?.Data;
            }
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
            var iFormat = BitConverter.ToInt32(value, 0);
            return iFormat switch
            {
                0x31545844 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT1), //827611204
                0x33545844 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT3), //861165636
                0x35545844 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT5), //894720068
                0x444E5243 => ConvertToEngineFormat(Rsc6TextureFormat.CRND),        //1145983555
                0x32 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_A8),         //50
                0x20 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_A8R8G8B8),   //32
                _ => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_A8R8G8B8)
            };
        }

        public static byte[] ConvertTextureFormatToBytes(TextureFormat value)
        {
            int format = value switch
            {
                TextureFormat.BC1 => 0x31545844, //DXT1
                TextureFormat.BC2 => 0x33545844, //DXT3
                TextureFormat.BC3 => 0x35545844, //DXT5
                TextureFormat.A8 => 0x32,        //A8
                TextureFormat.A8R8G8B8 => 0x20,  //A8R8G8B8
                _ => throw new NotImplementedException("Unknown pixel format!")
            };
            return BitConverter.GetBytes(format);
        }

        public static TextureFormat ConvertToEngineFormat(Rsc6TextureFormat format)
        {
            return format switch
            {
                Rsc6TextureFormat.D3DFMT_DXT3 => TextureFormat.BC2,
                Rsc6TextureFormat.D3DFMT_DXT5 => TextureFormat.BC3,
                Rsc6TextureFormat.D3DFMT_A8R8G8B8 => TextureFormat.A8R8G8B8,
                Rsc6TextureFormat.D3DFMT_A8 => TextureFormat.A8,
                Rsc6TextureFormat.CRND => TextureFormat.Unknown,
                _ => TextureFormat.BC1,
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

    public class Rsc6TextureBase : Texture, IRsc6Block
    {
        public virtual ulong BlockLength => 32;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public uint VFT { get; set; }
        public uint BlockMap { get; set; }
        public uint RefCount { get; set; } = 1; //m_RefCount
        public ResourceTextureType ResourceType { get; set; } = ResourceTextureType.SEPARATED; //m_ResourceType, 0 for embedded textures or 2 for seperated
        public ushort LayerCount { get; set; } //m_LayerCount, 0 for standard/volume/depth textures or 5 for cubemaps
        public uint Unknown_10h { get; set; }
        public int TextureSize { get; set; } //m_PhysicalSize, 0 for seperated textures
        public Rsc6Str NameRef { get; set; }
        public Rsc6Ptr<Rsc6BlockMap> D3DBaseTexture { get; set; }

        public virtual void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadUInt32();
            RefCount = reader.ReadUInt32();
            ResourceType = (ResourceTextureType)reader.ReadUInt16();
            LayerCount = reader.ReadUInt16();
            Unknown_10h = reader.ReadUInt32();
            TextureSize = reader.ReadInt32();
            NameRef = reader.ReadStr();
            D3DBaseTexture = reader.ReadPtr<Rsc6BlockMap>();
            Name = NameRef.Value;
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            bool wvd = writer.BlockList[0] is Rsc6VisualDictionary;
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable;

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
                writer.WriteUInt32(0x00D253E4); //WFD texture
            else
                writer.WriteUInt32(0x00AB3704);

            writer.WriteUInt32(BlockMap);
            writer.WriteUInt32(RefCount);
            writer.WriteUInt16((TextureSize == 0) ? (ushort)ResourceTextureType.SEPARATED : (ushort)ResourceType);
            writer.WriteUInt16(LayerCount);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteInt32(TextureSize);
            writer.WriteStr(NameRef);
            writer.WritePtr(D3DBaseTexture);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            ResourceType = reader.ReadEnum<ResourceTextureType>("ResourceType");

            var dds = reader.ReadExternal(Name + ".dds");
            if (dds != null)
            {
                TextureSize = dds.Length - 128;
            }

            if (!Name.EndsWith(".dds"))
            {
                Name += ".dds";
            }
            NameRef = new Rsc6Str(Name);
        }

        public override void Write(MetaNodeWriter writer)
        {
            string[] separators = [":", "/"];
            foreach (var separator in separators)
            {
                int lastIndex = Name.LastIndexOf(separator);
                if (lastIndex != -1)
                {
                    Name = Name[(lastIndex + 1)..];
                }
            }

            if (Name.EndsWith(".dds"))
            {
                Name = Name.Replace(".dds", "");
            }

            writer.WriteEnum("ResourceType", ResourceType);
            base.Write(writer);
        }

        public override string ToString()
        {
            return "TextureBase: " + Name;
        }

        public enum ResourceTextureType : ushort
        {
            EMBEDDED = 0,
            SEPARATED = 2
        }
    }

    public class Rsc6TextureData : IRsc6Block
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

    public class Rsc6TextureCRN : Rsc6BlockBase
    {
        public override ulong BlockLength => 74;
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

        //[DllImport("crunch.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr get_dds_from_crn(byte[] pSrc_file_data, uint src_file_size, out int outBufferSize);

        public override void Read(Rsc6DataReader reader)
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

            //TODO: FIX THIS
            //do we seriously need a random dll just to do some texture format conversion?

            //var resultPtr = get_dds_from_crn(data, (uint)data.Length, out int bufferSize);
            //if (resultPtr != IntPtr.Zero)
            //{
            //    Data = new byte[bufferSize];
            //    Marshal.Copy(resultPtr, Data, 0, bufferSize);
            //}
        }

        public override void Write(Rsc6DataWriter writer)
        {

        }
    }

    public enum Rsc6TextureFormat : uint
    {
        CRND = 0,
        D3DFMT_A8 = 2,
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

}