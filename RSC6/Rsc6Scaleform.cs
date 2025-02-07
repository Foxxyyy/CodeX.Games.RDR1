using System;
using CodeX.Core.Numerics;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6ScaleFormContext : Rsc6BlockBaseMap //rage::swfCONTEXT
    {
        public override ulong BlockLength => 368;
        public override uint VFT { get; set; } = 0x00CF9108;
        public uint Unknown_8h { get; set; } //Always 0?
        public uint Unknown_Ch { get; set; } //Always 0?
        public uint Unknown_10h { get; set; } = 0x00CDCDCD; //Always 0x00CDCDCD
        public uint Unknown_14h { get; set; } //Always 0?
        public uint Unknown_18h { get; set; } //Always 0?
        public uint Unknown_1Ch { get; set; } //Always 0?
        public uint Unknown_20h { get; set; } //Always 0?
        public uint Unknown_24h { get; set; } //Always 0?
        public uint Unknown_28h { get; set; } //Always 0?
        public Rsc6Ptr<Rsc6ScaleFormFile> File { get; set; } //m_File
        public uint Root { get; set; } //m_Root, always 0?
        public uint Focus { get; set; } //m_Focus, always 0?
        public Rsc6AtPool<byte> InstancePool { get; set; } //m_InstancePool
        public Rsc6AtPool<byte> ClipEventPool { get; set; } //m_ClipEventPool
        public Rsc6AtPool<byte> ScriptObjectPool { get; set; } //m_ScriptObjectPool
        public Rsc6AtPool<byte> ScriptArrayPool { get; set; } //m_ScriptArrayPool
        public uint ScriptObjectKillListHead { get; set; } //m_ScriptObjectKillListHead, always 0?
        public Rsc6AtPool<byte> StringPool { get; set; } //m_StringPool
        public uint Symbols { get; set; } //m_Symbols, rage::swfSYMTAB
        public uint MGlobal { get; set; } //m__global
        public uint Stage { get; set; } //m_Stage
        public bool Updating { get; set; } //m_Updating
        public bool IsFileOwner { get; set; } //m_IsFileOwner
        public ushort Unknown_C2h { get; set; } //Always 0?
        public uint[] Unknown_C4h { get; set; } //rage::swfACTIONFUNC, array of 40 uint's, always 0?
        public int NumFunctions { get; set; } //m_numFunctions
        public Rsc6Str Name { get; set; } //m_Name
        public ushort NameLength1 { get; set; } //m_Length
        public ushort NameLength2 { get; set; } //m_Length + 1 (null-terminator)

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            File = reader.ReadPtr<Rsc6ScaleFormFile>();
            Root = reader.ReadUInt32();
            Focus = reader.ReadUInt32();
            InstancePool = reader.ReadAtPool<byte>();
            ClipEventPool = reader.ReadAtPool<byte>();
            ScriptObjectPool = reader.ReadAtPool<byte>();
            ScriptArrayPool = reader.ReadAtPool<byte>();
            ScriptObjectKillListHead = reader.ReadUInt32();
            StringPool = reader.ReadAtPool<byte>();
            Symbols = reader.ReadUInt32();
            MGlobal = reader.ReadUInt32();
            Stage = reader.ReadUInt32();
            Updating = reader.ReadBoolean();
            IsFileOwner = reader.ReadBoolean();
            Unknown_C2h = reader.ReadUInt16();
            Unknown_C4h = reader.ReadUInt32Arr(40);
            NumFunctions = reader.ReadInt32();
            Name = reader.ReadStr();
            NameLength1 = reader.ReadUInt16();
            NameLength2 = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(Unknown_1Ch);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WritePtr(File);
            writer.WriteUInt32(Root);
            writer.WriteUInt32(Focus);
            writer.WriteAtPool(InstancePool);
            writer.WriteAtPool(ClipEventPool);
            writer.WriteAtPool(ScriptObjectPool);
            writer.WriteAtPool(ScriptArrayPool);
            writer.WriteUInt32(ScriptObjectKillListHead);
            writer.WriteAtPool(StringPool);
            writer.WriteUInt32(Symbols);
            writer.WriteUInt32(MGlobal);
            writer.WriteUInt32(Stage);
            writer.WriteBoolean(Updating);
            writer.WriteBoolean(IsFileOwner);
            writer.WriteUInt16(Unknown_C2h);
            writer.WriteUInt32Array(Unknown_C4h);
            writer.WriteInt32(NumFunctions);
            writer.WriteStr(Name);
            writer.WriteUInt16(NameLength1);
            writer.WriteUInt16(NameLength2);
        }

        public override string ToString()
        {
            return Name.ToString();
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
        public Rsc6RawPtrArr<Rsc6ScaleFormObject> Directory { get; set; } //m_Directory, rage::swfOBJECT
        public byte Version { get; set; } //m_Version
        public bool UsesInput { get; set; } //m_UsesInput
        public byte Padding1 { get; set; } //pad1
        public byte Padding2 { get; set; } //pad2
        public Rsc6ScaleFormRect FrameSize { get; set; } //m_FrameSize
        public ushort FrameRate { get; set; } //m_FrameRate
        public ushort DirCount { get; set; } //m_DirCount
        public int ShapeCount { get; set; } //m_ShapeCount
        public ulong ObjectMap { get; set; } //m_ObjectMap, rage::atMap<rage::ConstString,rage::datPadded<unsigned short>>

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_14h = reader.ReadUInt32();
            Directory = reader.ReadRawPtrArrPtr<Rsc6ScaleFormObject>();
            Version = reader.ReadByte();
            UsesInput = reader.ReadBoolean();
            Padding1 = reader.ReadByte();
            Padding2 = reader.ReadByte();
            FrameSize = reader.ReadBlock<Rsc6ScaleFormRect>();
            FrameRate = reader.ReadUInt16();
            DirCount = reader.ReadUInt16();
            ShapeCount = reader.ReadInt32();
            ObjectMap = reader.ReadUInt64();
            Directory = reader.ReadRawPtrArrItem(Directory, DirCount, Create);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteRawPtrArr(Directory);
            writer.WriteByte(Version);
            writer.WriteBoolean(UsesInput);
            writer.WriteByte(Padding1);
            writer.WriteByte(Padding2);
            writer.WriteBlock(FrameSize);
            writer.WriteUInt16(FrameRate);
            writer.WriteUInt16(DirCount);
            writer.WriteInt32(ShapeCount);
            writer.WriteUInt64(ObjectMap);
        }
    }

    public class Rsc6ScaleFormCMD : Rsc6FileBase //rage::swfCMD
    {
        public override ulong BlockLength => 16;
        public override uint VFT { get; set; } = 0x00CFDA70;
        public byte Type { get; set; } //m_Type
        public byte User8 { get; set; } //m_User8
        public ushort User16 { get; set; } = 0xCDCD; //m_User16
        public Rsc6Ptr<Rsc6ScaleFormCMD> Prev { get; set; } //m_Prev
        public Rsc6Ptr<Rsc6ScaleFormCMD> Next { get; set; } //m_Next

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Type = reader.ReadByte();
            User8 = reader.ReadByte();
            User16 = reader.ReadUInt16();
            Prev = reader.ReadPtr<Rsc6ScaleFormCMD>();
            Next = reader.ReadPtr<Rsc6ScaleFormCMD>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteByte(Type);
            writer.WriteByte(User8);
            writer.WriteUInt16(User16);
            writer.WritePtr(Prev);
            writer.WritePtr(Next);
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

    public class Rsc6ScaleFormObject : Rsc6FileBase
    {
        public override ulong BlockLength => 12;
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

        public static Rsc6ScaleFormObject Create(Rsc6DataReader r)
        {
            r.Position += 8;
            var type = (Rsc6ScaleformObjectType)r.ReadByte();
            r.Position -= 9;
            return Create(type);
        }

        public static Rsc6ScaleFormObject Create(Rsc6ScaleformObjectType type)
        {
            return type switch
            {
                Rsc6ScaleformObjectType.SHAPE => new Rsc6ScaleFormShape(),
                Rsc6ScaleformObjectType.SPRITE => new Rsc6ScaleFormSprite(),
                Rsc6ScaleformObjectType.BITMAP => new Rsc6ScaleFormBitmap(),
                Rsc6ScaleformObjectType.FONT => new Rsc6ScaleformFont(),
                Rsc6ScaleformObjectType.TEXT => new Rsc6ScaleformText(),
                Rsc6ScaleformObjectType.EDITTEXT => new Rsc6ScaleformEditText(),
                _ => throw new Exception("Unknown scaleform type")
            };
        }
    }

    public class Rsc6ScaleFormSprite : Rsc6ScaleFormObject //rage::swfSPRITE
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6RawLst<Rsc6ScaleFormFrame> Frames { get; set; } //m_Frames
        public ushort FrameCount { get; set; } //m_FrameCount
        public ushort Padding { get; set; } = 0xCD; //pad

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Frames = reader.ReadRawLstPtr<Rsc6ScaleFormFrame>();
            FrameCount = reader.ReadUInt16();
            Padding = reader.ReadUInt16();

            Frames = reader.ReadRawLstItems(Frames, FrameCount);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteRawLst(Frames);
            writer.WriteUInt16(FrameCount);
            writer.WriteUInt16(Padding);
        }
    }

    public class Rsc6ScaleFormFrame : Rsc6BlockBase //rage::swfFRAME
    {
        public override ulong BlockLength => 8;
        public Colour BackgroundColor { get; set; } //m_BackgroundColor
        public Rsc6Ptr<Rsc6ScaleFormCMD> First { get; set; } //m_First

        public override void Read(Rsc6DataReader reader)
        {
            BackgroundColor = reader.ReadColour();
            First = reader.ReadPtr<Rsc6ScaleFormCMD>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteColour(BackgroundColor);
            writer.WritePtr(First);
        }
    }

    public class Rsc6ScaleFormShape : Rsc6ScaleFormObject
    {
        public override ulong BlockLength => base.BlockLength + 36;
        public uint FillStyles { get; set; } //m_FillStyles
        public uint LineStyles { get; set; } //m_LineStyles
        public uint Data { get; set; } //m_Data
        public int DataCount { get; set; } //m_DataCount
        public ushort FillStyleCount { get; set; } //m_FillStyleCount
        public ushort LineStyleCount { get; set; } //m_LineStyleCount
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            FillStyles = reader.ReadUInt32();
            LineStyles = reader.ReadUInt32();
            Data = reader.ReadUInt32();
            DataCount = reader.ReadInt32();
            FillStyleCount = reader.ReadUInt16();
            LineStyleCount = reader.ReadUInt16();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(FillStyles);
            writer.WriteUInt32(LineStyles);
            writer.WriteUInt32(Data);
            writer.WriteInt32(DataCount);
            writer.WriteUInt16(FillStyleCount);
            writer.WriteUInt16(LineStyleCount);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }
    }

    public class Rsc6ScaleformFont : Rsc6ScaleFormObject //rage::swfFONT
    {
        public override ulong BlockLength => base.BlockLength + 324;
        public uint Unknown_Ch { get; set; }
        public Rsc6RawArr<ushort> GlyphToCode { get; set; } //m_GlyphToCode
        public Rsc6RawArr<float> Advance { get; set; } //m_Advance
        public byte[] Glyphes { get; set; } //m_CodeToGlyph
        public short SheetCount { get; set; } //m_SheetCount
        public short Ascent { get; set; } //m_Ascent
        public short Descent { get; set; } //m_Descent
        public short Leading { get; set; } //m_Leading
        public short GlyphCount { get; set; } //m_GlyphCount
        public byte Flags { get; set; } //m_Flags
        public byte LangCode { get; set; } //m_LangCode
        public Rsc6Ptr<Rsc6ScaleformFontSheet>[] Sheets { get; set; }
        public string FontName { get; set; } //m_Name

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_Ch = reader.ReadUInt32();
            GlyphToCode = reader.ReadRawArrPtr<ushort>();
            Advance = reader.ReadRawArrPtr<float>();
            Glyphes = reader.ReadBytes(128);
            SheetCount = reader.ReadInt16();
            Ascent = reader.ReadInt16();
            Descent = reader.ReadInt16();
            Leading = reader.ReadInt16();
            GlyphCount = reader.ReadInt16();
            Flags = reader.ReadByte();
            LangCode = reader.ReadByte();

            Sheets = new Rsc6Ptr<Rsc6ScaleformFontSheet>[24];
            for (int i = 0; i < Sheets.Length; i++)
            {
                Sheets[i] = reader.ReadPtr<Rsc6ScaleformFontSheet>();
            }

            FontName = reader.ReadStringNullTerminated();
            GlyphToCode = reader.ReadRawArrItems(GlyphToCode, (uint)GlyphCount);
            Advance = reader.ReadRawArrItems(Advance, (uint)GlyphCount);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteRawArr(GlyphToCode);
            writer.WriteRawArr(Advance);
            writer.WriteBytes(Glyphes);
            writer.WriteInt16(SheetCount);
            writer.WriteInt16(Ascent);
            writer.WriteInt16(Descent);
            writer.WriteInt16(Leading);
            writer.WriteInt16(GlyphCount);
            writer.WriteByte(Flags);
            writer.WriteByte(LangCode);

            for (int i = 0; i < Sheets.Length; i++)
            {
                writer.WritePtr(Sheets[i]);
            }

            writer.WriteStringNullTerminated(FontName);
            writer.WriteRawArr(GlyphToCode);
            writer.WriteRawArr(Advance);
        }
    }

    public class Rsc6ScaleFormBitmap : Rsc6ScaleFormObject //rage::swfBITMAP
    {
        public override ulong BlockLength => base.BlockLength + 20;
        public Rsc6Ptr<Rsc6Texture> Texture { get; set; } //m_Texture
        public Rsc6Str TextureName { get; set; } //m_TextureName
        public ushort Width { get; set; } //m_Width
        public ushort Height { get; set; } //m_Height
        public ushort InvWidth { get; set; } //m_InvWidth
        public ushort InvHeight { get; set; } //m_InvHeight

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Texture = reader.ReadPtr<Rsc6Texture>();
            TextureName = reader.ReadStr();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            InvWidth = reader.ReadUInt16();
            InvHeight = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Texture);
            writer.WriteStr(TextureName);
            writer.WriteUInt16(Width);
            writer.WriteUInt16(Height);
            writer.WriteUInt16(InvWidth);
            writer.WriteUInt16(InvHeight);
        }
    }

    public class Rsc6ScaleformMatrix : Rsc6BlockBase //rage::swfMATRIX
    {
        public override ulong BlockLength => 24;
        public float Ax { get; set; } //ax
        public float Ay { get; set; } //ay
        public float Bx { get; set; } //bx
        public float By { get; set; } //by
        public float Cx { get; set; } //cx
        public float Cy { get; set; } //cy

        public override void Read(Rsc6DataReader reader)
        {
            Ax = reader.ReadSingle();
            Ay = reader.ReadSingle();
            Bx = reader.ReadSingle();
            By = reader.ReadSingle();
            Cx = reader.ReadSingle();
            Cy = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteSingle(Ax);
            writer.WriteSingle(Ay);
            writer.WriteSingle(Bx);
            writer.WriteSingle(By);
            writer.WriteSingle(Cx);
            writer.WriteSingle(Cy);
        }
    }

    public class Rsc6ScaleformText : Rsc6ScaleFormObject //rage::swfTEXT
    {
        public override ulong BlockLength => base.BlockLength + 36;
        public Rsc6ScaleformMatrix Matrix { get; set; } //m_Matrix
        public Rsc6RawArr<short> Data { get; set; } //m_Data
        public uint DataSize { get; set; } //m_DataSize
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Matrix = reader.ReadBlock<Rsc6ScaleformMatrix>();
            Data = reader.ReadRawArrPtr<short>();
            DataSize = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();

            Data = reader.ReadRawArrItems(Data, DataSize);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteBlock(Matrix);
            writer.WriteRawArr(Data);
            writer.WriteUInt32(DataSize);
            writer.WriteUInt32(Unknown_2Ch);
        }
    }

    public class Rsc6ScaleformEditText : Rsc6ScaleFormObject //rage::swfEDITTEXT
    {
        public override ulong BlockLength => base.BlockLength + 40;
        public Rsc6Str Name { get; set; } //m_String
        public Rsc6Str VarName { get; set; } //m_VarName
        public ushort StringSize { get; set; } //m_StringSize
        public short Leading { get; set; } //m_Leading
        public Colour Color { get; set; } //m_Color
        public ushort FontId { get; set; } //m_FontId
        public ushort FontHeight { get; set; } //m_FontHeight
        public float Width { get; set; } //m_Width
        public float Height { get; set; } //m_Height
        public float OffsetX { get; set; } //m_OffsetX
        public float OffsetY { get; set; } //m_OffsetY
        public bool Html { get; set; } //m_Html
        public bool Align { get; set; } //m_Align
        public byte Pad_0 { get; set; } = 0xCD; //pad0
        public byte Pad_1 { get; set; } = 0xCD; //pad1

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadStr();
            VarName = reader.ReadStr();
            StringSize = reader.ReadUInt16();
            Leading = reader.ReadInt16();
            Color = reader.ReadColour();
            FontId = reader.ReadUInt16();
            FontHeight = reader.ReadUInt16();
            Width = reader.ReadSingle();
            Height = reader.ReadSingle();
            OffsetX = reader.ReadSingle();
            OffsetY = reader.ReadSingle();
            Html = reader.ReadBoolean();
            Align = reader.ReadBoolean();
            Pad_0 = reader.ReadByte();
            Pad_1 = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteStr(Name);
            writer.WriteStr(VarName);
            writer.WriteUInt16(StringSize);
            writer.WriteInt16(Leading);
            writer.WriteColour(Color);
            writer.WriteUInt16(FontId);
            writer.WriteUInt16(FontHeight);
            writer.WriteSingle(Width);
            writer.WriteSingle(Height);
            writer.WriteSingle(OffsetX);
            writer.WriteSingle(OffsetY);
            writer.WriteBoolean(Html);
            writer.WriteBoolean(Align);
            writer.WriteByte(Pad_0);
            writer.WriteByte(Pad_1);
        }
    }

    public class Rsc6ScaleformFontSheet : Rsc6BlockBase //rage::swfFONT::SHEET
    {
        public override ulong BlockLength => 32;
        public Rsc6RawPtrArr<Rsc6Texture> Textures { get; set; } //m_Textures
        public Rsc6RawLst<Rsc6ScaleFormFontSheetCell> Cells { get; set; } //m_Cells
        public short Size { get; set; } //m_Size
        public short CellCount { get; set; } //m_CellCount
        public Rsc6RawArr<ushort> Indices { get; set; } //m_Indices
        public uint Unknown_Ch { get; set; } //Always 0?
        public Rsc6PtrStr TextureNames { get; set; } //m_TextureNames
        public uint TextureCount { get; set; } //m_TextureCount
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            Textures = reader.ReadRawPtrArrPtr<Rsc6Texture>();
            Cells = reader.ReadRawLstPtr<Rsc6ScaleFormFontSheetCell>();
            Size = reader.ReadInt16();
            CellCount = reader.ReadInt16();
            Indices = reader.ReadRawArrPtr<ushort>();
            TextureNames = reader.ReadPtrStr();
            TextureCount = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();

            Textures = reader.ReadRawPtrArrItem(Textures, TextureCount);
            Cells = reader.ReadRawLstItems(Cells, (uint)CellCount);
            Indices = reader.ReadRawArrItems(Indices, (uint)CellCount);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteRawPtrArr(Textures);
            writer.WriteRawLst(Cells);
            writer.WriteInt16(Size);
            writer.WriteInt16(CellCount);
            writer.WriteRawArr(Indices);
            writer.WritePtrStr(TextureNames);
            writer.WriteUInt32(TextureCount);
            writer.WriteUInt32(Unknown_1Ch);
        }
    }

    public class Rsc6ScaleFormFontSheetCell : Rsc6BlockBase //rage::swfFONT::SHEET::CELL
    {
        public override ulong BlockLength => 32;
        public float Left { get; set; } //m_Left
        public float Top { get; set; } //m_Top
        public float Width { get; set; } //m_Width
        public float Height { get; set; } //m_Height
        public float MinX { get; set; } //m_MinX
        public float MinY { get; set; } //m_MinY
        public float MaxX { get; set; } //m_MaxX
        public float MaxY { get; set; } //m_MaxY

        public override void Read(Rsc6DataReader reader)
        {
            Left = reader.ReadSingle();
            Top = reader.ReadSingle();
            Width = reader.ReadSingle();
            Height = reader.ReadSingle();
            MinX = reader.ReadSingle();
            MinY = reader.ReadSingle();
            MaxX = reader.ReadSingle();
            MaxY = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteSingle(Left);
            writer.WriteSingle(Top);
            writer.WriteSingle(Width);
            writer.WriteSingle(Height);
            writer.WriteSingle(MinX);
            writer.WriteSingle(MinY);
            writer.WriteSingle(MaxX);
            writer.WriteSingle(MaxY);
        }
    }

    public enum Rsc6ScaleformObjectType
    {
        SHAPE = 1,
        SPRITE = 2,
        BITMAP = 4,
        FONT = 5,
        TEXT = 6,
        EDITTEXT = 7
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