using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Core.Numerics;

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

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ParentDictionary = reader.ReadUInt32();
            UsageCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Textures = reader.ReadPtrArr<Rsc6Texture>();
            CreateTexturePack(reader.FileEntry);
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

        private void CreateTexturePack(GameArchiveFileInfo e)
        {
            var texs = Textures.Items;
            var txp = new TexturePack(e)
            {
                Textures = new Dictionary<string, Texture>()
            };

            if (texs != null)
            {
                for (int i = 0; i < texs.Length; i++)
                {
                    var tex = texs[i];
                    if (tex == null) continue;
                    txp.Textures[tex.Name] = tex;
                    tex.Pack = txp;
                }
            }
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

        public int[] BitsPerPixel = [4, 8, 8, 8, 8, 8, 8, 8, 8, 4, 4, 4, 8, 4, 8]; //crn stuff

        public static bool IsFormatBlockCompressed(TextureFormat fmt) //crn stuff
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
            var iFormat = BitConverter.ToInt32(value, 0);
            return iFormat switch
            {
                0x31545844 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT1), //827611204
                0x33545844 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT3), //861165636
                0x35545844 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_DXT5), //894720068
                0x444E5243 => ConvertToEngineFormat(Rsc6TextureFormat.CRND),        //1145983555
                0x32 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_L8),         //50
                0x20 => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_A8R8G8B8),   //32
                _ => ConvertToEngineFormat(Rsc6TextureFormat.D3DFMT_A8R8G8B8) //throw new NotImplementedException("Unknown pixel format!")
            };
        }

        public static byte[] ConvertTextureFormatToBytes(TextureFormat value)
        {
            int format = value switch
            {
                TextureFormat.BC1 => 0x31545844, //DXT1
                TextureFormat.BC2 => 0x33545844, //DXT3
                TextureFormat.BC3 => 0x35545844, //DXT5
                TextureFormat.L8 => 0x32,        //L8
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

    public class Rsc6TextureBase : Texture, IRsc6Block
    {
        public virtual ulong BlockLength => 32;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public uint VFT { get; set; }
        public uint BlockMap { get; set; }
        public uint RefCount { get; set; } = 1; //m_RefCount
        public ResourceTextureType ResourceType { get; set; } //m_ResourceType, 0 for embedded textures or 2 for seperated
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
            writer.WriteUInt16((ushort)(TextureSize == 0 ? ResourceTextureType.SEPARATED : ResourceType));
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
            string[] separators = { ":", "/" };
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

    public class Rsc6ScaleFormFile : Rsc6ScaleFormSprite //rage::swfFILE
    {
        /*
         * FLASH is the default tool that is used for the UI.
         * 
         * UIComponent    : base class for all components, must be put top-level container such as UILayer and UIScene.
         * UILayer        : a generic lightweight container, all childrens are managed by this component.
         * UIScene        : a container that groups any number of components except other UIScenes.
         * UIPanel        : a generic lightweight container that groups any number of components.
         * UIScrollBar    : a scrollbar to determine the contents of the viewing area.
         * UILabel        : a display area for a short text string.
         * UIButton       : implementation of a push button.
         * UITab          : implementation of a push button.
         * UIIcon         : represents single images.
         * UIList         : a component that allows the user to select one or more objects from a list, supports scrolling.
         * UIProgressBar  : a component that communicates the progress of some work by displaying its percentage of completion.
         * UISpinner      : a single line input field that lets the user select a number or an object value from an ordered sequence.
         * UIContext      : an interface to an external tool that artists use to represent/decorate these UI components. Each component has its own context interface.
         * UIFactory      : a class to map inputs to the ones that the UI uses.
         * UIInput        : a class to manages the creation of UI elements.
         * UINavigator    : a class to manages the navigation/transitions between various UI components.
         * UIManager      : a class used for various subsystem that manages a UI system.
         * 
         * Visibility dictates if a state is shown or hidden.
         * Ideally the textures/meshes associated with the state should not be rendered at all.
         * 
         * Enabled/Disabled describes how input, events, and transitions are processed on a state.
         * Input and events are processed if this flag is true, otherwise input and events are not processed.
         * A disabled state should never be transitioned to.
         * 
         * Focused describes if a component is selected or not.
         * All states for a focused component that are on a path from that focused component to its root component should also be focused.
         * The only way for a component to receive input is if it is focused.
         * 
         * Active describes if a component is running.
         * Siblings and children of this active component may or may not be active.
         */

        public override ulong BlockLength => 64;
        public uint Unknown_14h { get; set; }
        public Rsc6RawPtrArr<Rsc6ScaleFormObjectBase> Directory { get; set; } //m_Directory, rage::swfOBJECT
        public byte Version { get; set; } //m_Version
        public bool UsesInput { get; set; } //m_UsesInput
        public byte Padding1 { get; set; } //pad1
        public byte Padding2 { get; set; } //pad2
        public Rsc6ScaleFormRect FrameSize { get; set; } //m_FrameSize
        public ushort FrameRate { get; set; } //m_FrameRate
        public ushort DirCount { get; set; } //m_DirCount
        public int ShapeCount { get; set; } //m_ShapeCount
        public ulong ObjectMap { get; set; } //m_ObjectMap, rage::atMap<rage::ConstString,rage::datPadded<unsigned short>>

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
            Unknown_14h = reader.ReadUInt32();
            Directory = reader.ReadRawPtrArrPtr<Rsc6ScaleFormObjectBase>();
            Version = reader.ReadByte();
            UsesInput = reader.ReadBoolean();
            Padding1 = reader.ReadByte();
            Padding2 = reader.ReadByte();
            FrameSize = reader.ReadBlock<Rsc6ScaleFormRect>();
            FrameRate = reader.ReadUInt16();
            DirCount = reader.ReadUInt16();
            ShapeCount = reader.ReadInt32();
            ObjectMap = reader.ReadUInt64();

            Directory = reader.ReadRawPtrArrItem(Directory, DirCount, Rsc6ScaleFormObjectBase.Create);

            reader.Position = Rpf6Crypto.VIRTUAL_BASE + 0x2C;
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

                    try
                    {
                        var tex = new Rsc6Texture();
                        tex.Read(reader);
                        Textures.Add(tex);
                    }
                    catch
                    {

                    }
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6ScaleFormSprite : Rsc6ScaleFormObject //rage::swfSPRITE
    {
        public override ulong BlockLength => base.BlockLength + 12;
        public Rsc6ScaleFormFrame Frames { get; set; } //m_Frames
        public ushort FrameCount { get; set; } //m_FrameCount
        public ushort Padding { get; set; } = 0xCD; //pad

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Frames = reader.ReadBlock<Rsc6ScaleFormFrame>();
            FrameCount = reader.ReadUInt16();
            Padding = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteBlock(Frames);
            writer.WriteUInt16(FrameCount);
            writer.WriteUInt16(Padding);
        }
    }

    public class Rsc6ScaleFormFrame : Rsc6BlockBase //rage::swfFRAME
    {
        public override ulong BlockLength => 8;
        public Colour BackgroundColor { get; set; } //m_BackgroundColor
        public uint First { get; set; } //m_First, TODO: Add rage::swfCMD

        public override void Read(Rsc6DataReader reader)
        {
            BackgroundColor = reader.ReadColour();
            First = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteColour(BackgroundColor);
            writer.WriteUInt32(First);
        }
    }

    public class Rsc6ScaleFormRect : Rsc6BlockBase //rage::swfRECT
    {
        public override ulong BlockLength => 16;
        public float MinX { get; set; } //Xmin
        public float MaxX { get; set; } //Xmax
        public float MinY { get; set; } //Ymin
        public float MaxY { get; set; } //Ymax

        public override void Read(Rsc6DataReader reader)
        {
            MinX = reader.ReadSingle();
            MaxX = reader.ReadSingle();
            MinY = reader.ReadSingle();
            MaxY = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteSingle(MinX);
            writer.WriteSingle(MaxX);
            writer.WriteSingle(MinY);
            writer.WriteSingle(MaxY);
        }
    }

    public class Rsc6ScaleFormObject : Rsc6FileBase //rage::swfOBJECT
    {
        public override ulong BlockLength => 8;
        public override uint VFT { get; set; } = 0x00CF9108;
        public byte Type { get; set; } //m_Type, object type
        public byte Pad0 { get; set; } //pad0
        public byte Pad1 { get; set; } //pad1
        public byte Pad2 { get; set; } //pad2

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Type = reader.ReadByte();
            Pad0 = reader.ReadByte();
            Pad1 = reader.ReadByte();
            Pad2 = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteByte(Type);
            writer.WriteByte(Pad0);
            writer.WriteByte(Pad1);
            writer.WriteByte(Pad2);
        }
    }

    public class Rsc6ScaleFormObjectBase : Rsc6FileBase
    {
        public override ulong BlockLength => 12; //16?
        public override uint VFT { get; set; } = 0x00CF99D0;
        public uint Unknown_4h { get; set; }
        public Rsc6ScaleformObjectType Type { get; set; } //m_Type, object type
        public byte Pad0 { get; set; } //pad0
        public byte Pad1 { get; set; } //pad1
        public byte Pad2 { get; set; } //pad2

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_4h = reader.ReadUInt32();
            Type = (Rsc6ScaleformObjectType)reader.ReadByte();
            Pad0 = reader.ReadByte();
            Pad1 = reader.ReadByte();
            Pad2 = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_4h);
            writer.WriteByte((byte)Type);
            writer.WriteByte(Pad0);
            writer.WriteByte(Pad1);
            writer.WriteByte(Pad2);
        }

        public static Rsc6ScaleFormObjectBase Create(Rsc6DataReader r)
        {
            r.Position += 8;
            var type = (Rsc6ScaleformObjectType)r.ReadByte();
            r.Position -= 9;
            return Create(type);
        }

        public static Rsc6ScaleFormObjectBase Create(Rsc6ScaleformObjectType type)
        {
            return type switch
            {
                Rsc6ScaleformObjectType.FONT => new Rsc6ScaleformFont(),
                _ => throw new Exception("Unknown scaleform type")
            };
        }
    }

    public class Rsc6ScaleformFont : Rsc6ScaleFormObjectBase
    {
        public override ulong BlockLength => base.BlockLength; //336
        public uint Unknown_Ch { get; set; }
        public uint GlyphToCode { get; set; } //m_GlyphToCode, TODO
        public uint Advance { get; set; } //m_Advance, TODO
        public byte[] Glyphs { get; set; } //m_CodeToGlyph
        public short SheetCount { get; set; } //m_SheetCount
        public short Ascent { get; set; } //m_Ascent
        public short Descent { get; set; } //m_Descent
        public short Leading { get; set; } //m_Leading
        public byte Flags { get; set; } //m_Flags
        public byte LangCode { get; set; } //m_LangCode
        public Rsc6Ptr<Rsc6ScaleformFontSheet> Sheet { get; set; }
        public ulong Unknown_A8h { get; set; }
        public ulong Unknown_B0h { get; set; }
        public ulong Unknown_B8h { get; set; }
        public ulong Unknown_C0h { get; set; }
        public ulong Unknown_C8h { get; set; }
        public ulong Unknown_D0h { get; set; }
        public ulong Unknown_D8h { get; set; }
        public ulong Unknown_E0h { get; set; }
        public ulong Unknown_E8h { get; set; }
        public ulong Unknown_F0h { get; set; }
        public ulong Unknown_F8h { get; set; }
        public uint Unknown_100h { get; set; }
        public uint Unknown_104h { get; set; } //Hash?
        public uint Unknown_108h { get; set; } //Hash?
        public uint Unknown_10Ch { get; set; } //Hash?

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_Ch = reader.ReadUInt32();
            GlyphToCode = reader.ReadUInt32();
            Advance = reader.ReadUInt32();
            Glyphs = reader.ReadBytes(128);
            SheetCount = reader.ReadInt16();
            Ascent = reader.ReadInt16();
            Descent = reader.ReadInt16();
            Leading = reader.ReadInt16();
            Flags = reader.ReadByte();
            LangCode = reader.ReadByte();
            Sheet = reader.ReadPtr<Rsc6ScaleformFontSheet>();
            Unknown_A8h = reader.ReadUInt64();
            Unknown_B0h = reader.ReadUInt64();
            Unknown_B8h = reader.ReadUInt64();
            Unknown_C0h = reader.ReadUInt64();
            Unknown_C8h = reader.ReadUInt64();
            Unknown_D0h = reader.ReadUInt64();
            Unknown_D8h = reader.ReadUInt64();
            Unknown_E0h = reader.ReadUInt64();
            Unknown_E8h = reader.ReadUInt64();
            Unknown_F0h = reader.ReadUInt64();
            Unknown_F8h = reader.ReadUInt64();
            Unknown_100h = reader.ReadUInt32();
            Unknown_104h = reader.ReadUInt32();
            Unknown_108h = reader.ReadUInt32();
            Unknown_10Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }
    }

    public class Rsc6ScaleformFontSheet : Rsc6BlockBase
    {
        public override ulong BlockLength => 32;
        public uint Cells { get; set; } //m_Cells
        public uint Indices { get; set; } //m_Indices
        public short Size { get; set; } //m_Size
        public short CellCount { get; set; } //m_CellCount
        public uint Unknown_Ch { get; set; } //Always 0?
        public Rsc6StrArr TextureNames { get; set; } //m_TextureNames
        public uint TextureCount { get; set; } //m_TextureCount

        public override void Read(Rsc6DataReader reader)
        {
            Cells = reader.ReadUInt32();
            Indices = reader.ReadUInt32();
            Size = reader.ReadInt16();
            CellCount = reader.ReadInt16();
            TextureNames = reader.ReadPtr();
            TextureCount = reader.ReadUInt32();

            TextureNames = reader.ReadItems(TextureNames, TextureCount);
        }

        public override void Write(Rsc6DataWriter writer)
        {

        }
    }

    public class Rsc6ScaleformType : Rsc6BlockBase
    {
        public override ulong BlockLength => 32;
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; }
        public Rsc6ScaleformObjectType Type { get; set; }
        public List<Rsc6Ptr<Rsc6BlockMap>> TexturesPointers { get; set; } = new();

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Type = (Rsc6ScaleformObjectType)reader.ReadByte();

            if (Type == Rsc6ScaleformObjectType.FONT)
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
            else if (Type == Rsc6ScaleformObjectType.BITMAP)
            {
                reader.Position += 0x3; //Those 3 bytes are 'pad'
                TexturesPointers.Add(reader.ReadPtr<Rsc6BlockMap>());
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
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

        [DllImport("crunch.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get_dds_from_crn(byte[] pSrc_file_data, uint src_file_size, out int outBufferSize);

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
            var resultPtr = get_dds_from_crn(data, (uint)data.Length, out int bufferSize);

            if (resultPtr != IntPtr.Zero)
            {
                Data = new byte[bufferSize];
                Marshal.Copy(resultPtr, Data, 0, bufferSize);
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {

        }
    }

    public enum Rsc6ScaleformObjectType
    {
        BITMAP = 4,
        FONT = 5
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

    public enum Rsc6UIStates
    {
        UI_ENABLED = 1, //Can be focused
        UI_INTERRUPTIBLE = 2, //Can be unfocused
        UI_ACTIVE = 4, //Can receive events, updates
        UI_FOCUSED = 8, //Can receive inputs
        UI_VISIBLE = 16, //Can receive paints, draws
        UI_ALL = 255
    };

    public enum Rsc6UITemplates
    {
        TEMPLATE_SIMPLE = 1, //PAUSEMENU
        TEMPLATE_DEFAULT = 2, //DIALOGBOX & POPUP
        TEMPLATE_MENUWITHDESCRIPTION = 3,
        TEMPLATE_PLAYERLIST = 4,
        TEMPLATE_LOBBY_PLAYERS = 5,
    };

    public enum Rsc6UIButtonIcons
    {
        NONE = 0,
        NOTREADY = 1,
        READY = 2,
        BLANK = 3,
        VOICE_ENABLED = 5,
        VOICE_MUTED = 6,
        VOICE_TALKING = 7,
        CONNECTION_STRENGTH_0 = 10,
        CONNECTION_STRENGTH_1 = 11,
        CONNECTION_STRENGTH_2 = 12,
        CONNECTION_STRENGTH_3 = 13,
        CONNECTION_STRENGTH_4 = 14,
        HASFLAG = 17,
        PARTY_LEADER = 20,
        PARTY_INVITE = 21,
        PARTY_REQUESTED = 22,
        PARTY_MEMBER = 23
    };

    public enum Rsc6UIPromptStyles
    {
        MESSAGE_BOX,
        BOTTOM_SCREEN
    };

    public enum Rsc6UIPromptTypes
    {
        NONE,
        OK,
        OK_CANCEL,
        YES_NO,
        YES_NO_CANCEL,
        CANCEL,
        CONTINUE,
        CUSTOM
    };

    public enum Rsc6UIPromptIcons
    {
        EMPTY0,
        ACCEPT,
        CANCEL,
        LEFT,
        UP,
        L_SHOULDER,
        R_SHOULDER,
        L_TRIGGER,
        R_TRIGGER,
        START,
        SELECT,
        ANALOG_L,
        ANALOG_L_UP_DOWN,
        ANALOG_L_LEFT_RIGHT,
        ANALOG_L_UP_DOWN_LEFT_RIGHT,
        ANALOG_R,
        ANALOG_R_UP_DOWN,
        ANALOG_R_LEFT_RIGHT,
        ANALOG_R_UP_DOWN_LEFT_RIGHT,
        DPAD,
        DPAD_LEFT_RIGHT,
        DPAD_UP_DOWN,
        DPAD_UP_DOWN_LEFT_RIGHT,
        LTRIGGER_RTRIGGER,
        LSHOULDER_RSHOULDER,
        EMPTY1,
        EMPTY2,
        EMPTY3,
        R3,
        L3,
        ANALOG_L_RIGHT,
        ANALOG_L_LEFT,
        ANALOG_L_UP,
        ANALOG_L_DOWN,
        ANALOG_R_RIGHT,
        ANALOG_R_LEFT,
        ANALOG_R_UP,
        ANALOG_R_DOWN,
        DPAD_RIGHT,
        DPAD_LEFT,
        DPAD_UP,
        DPAD_DOWN
    };
}