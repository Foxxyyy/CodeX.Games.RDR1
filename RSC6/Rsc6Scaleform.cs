using CodeX.Core.Numerics;
using System;
using System.Collections.Generic;

namespace CodeX.Games.RDR1.RSC6
{
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

            reader.Position = Rsc6DataReader.VIRTUAL_BASE + 0x2C;
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

    public enum Rsc6ScaleformObjectType
    {
        BITMAP = 4,
        FONT = 5
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