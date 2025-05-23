﻿using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using System.Linq;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6AnimationSet : Rsc6BlockBaseMap, MetaNode //animAnimSet
    {
        public override ulong BlockLength => 88;
        public override uint VFT { get; set; }
        public Rsc6PtrArr<Rsc6ClipDictionaryEntry> Entries { get; set; } //m_ASTtoClipMap
        public uint Unknown_10h { get; set; } = 0x00CDCDCD;
        public Rsc6Ptr<Rsc6ClipDictionary> ClipDictionary { get; set; } //m_ClipDict
        public uint Unknown_18h { get; set; } = 0xCDCDCD00;
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD;
        public uint Unknown_20h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD;
        public uint Unknown_30h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_34h { get; set; } = 0xCDCDCDCD;
        public string AnimSetName { get; set; } //m_AnimSetName

        public Rsc6Clip[] Clips => Rsc6DataMap.Flatten(Entries.Items, e => e.Entry.Item).ToArray();
        public Dictionary<uint, Rsc6Clip> Dict { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Entries = reader.ReadPtrArr<Rsc6ClipDictionaryEntry>();
            Unknown_10h = reader.ReadUInt32();
            ClipDictionary = reader.ReadPtr<Rsc6ClipDictionary>();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
            Unknown_30h = reader.ReadUInt32();
            Unknown_34h = reader.ReadUInt32();
            AnimSetName = reader.ReadString();
            CreateDict();
        }

        public void Read(MetaNodeReader reader)
        {

        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            writer.WriteNodeArray("Clips", Clips);
            ClipDictionary.Item?.Write(writer);
        }

        public void CreateDict()
        {
            Dict = Rsc6DataMap.GetDictionary(Entries.Items, e => e.Entry.Item);
        }
    }

    public class Rsc6ClipDictionary : Rsc6BlockBaseMapRef, MetaNode //rage::crClipDictionary
    {
        /*
         * Simple clip container class
         */

        public override ulong BlockLength => 32;
        public override uint VFT { get; set; }
        public Rsc6Ptr<Rsc6AnimDictionary> AnimDict { get; set; } //m_AnimDictionary
        public bool AnimDictionaryOwner { get; set; } = true; //m_AnimDictionaryOwner
        public bool BaseNameKeys { get; set; } //m_BaseNameKeys
        public ushort Padding { get; set; } //m_Padding
        public Rsc6AtMapArr<Rsc6ClipDictionaryEntry> Entries { get; set; } //m_Clips
        public Rsc6Clip[] Clips => Rsc6DataMap.Flatten(Entries.Items, e => e.Entry.Item).ToArray();
        public Dictionary<uint, Rsc6Clip> Dict { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            AnimDict = reader.ReadPtr<Rsc6AnimDictionary>();
            AnimDictionaryOwner = reader.ReadBoolean();
            BaseNameKeys = reader.ReadBoolean();
            Padding = reader.ReadUInt16();
            Entries = reader.ReadAtMapArr<Rsc6ClipDictionaryEntry>();

            CreateDict();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(AnimDict);
            writer.WriteBoolean(AnimDictionaryOwner);
            writer.WriteBoolean(BaseNameKeys);
            writer.WriteUInt16(Padding);
            writer.WriteAtMapArr(Entries);
        }

        public void Read(MetaNodeReader reader)
        {
            
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            writer.WriteNodeArray("Clips", Clips);
            AnimDict.Item?.Write(writer);
        }

        public void CreateDict()
        {
            Dict = Rsc6DataMap.GetDictionary(Entries.Items, e => e.Entry.Item);
        }
    }

    public class Rsc6ClipDictionaryEntry : Rsc6BlockBase, IRsc6DataMapEntry<Rsc6ClipDictionaryEntry> //atMapEntry<uint, rage::crClip>
    {
        public override ulong BlockLength => 12;
        public JenkHash ClipHash { get; set; }
        public Rsc6Ptr<Rsc6Clip> Entry { get; set; }
        public Rsc6Ptr<Rsc6ClipDictionaryEntry> Next { get; set; }

        public uint MapKey { get => ClipHash; }
        public Rsc6ClipDictionaryEntry MapNext { get => Next.Item; set => Next = new(value); }

        public override void Read(Rsc6DataReader reader)
        {
            ClipHash = (JenkHash)reader.ReadUInt32();
            Entry = reader.ReadPtr(Rsc6Clip.Create);
            Next = reader.ReadPtr<Rsc6ClipDictionaryEntry>();

            if (Entry.Item != null)
            {
                Entry.Item.NameHash = ClipHash;
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(ClipHash);
            writer.WritePtr(Entry);
            writer.WritePtr(Next);
        }

        public override string ToString()
        {
            return $"{ClipHash.Hex} : {Entry.Item}";
        }
    }

    public class Rsc6Clip : AnimationClip, IRsc6Block //rage::crClip
    {
        /*
         * Represents a motion clip.
         * Clips are a higher level wrapper for "animated" data.
         * They provide additional features, such as metadata (eg properties, tags, events) and internally hide their storage implementation details.
         */

        public virtual ulong BlockLength => 28;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public uint VFT { get; set; } = 0x011EF2E8;
        public Rsc6ClipType ClipType { get; set; } //m_Type
        public Rsc6Str ClipName { get; set; } //m_Name
        public short ClipNameLength1 { get; set; } //Clip name length
        public short ClipNameLength2 { get; set; } //Clip name length + 1
        public Rsc6Str Comment { get; set; } //m_Comment
        public short CommentLength1 { get; set; } //Comment length
        public short CommentLength2 { get; set; } //Comment length + 1
        public bool IsLooped { get; set; } //m_IsLooped
        public byte Unknown_19h { get; set; } //Padding
        public byte Unknown_1Ah { get; set; } //Padding
        public byte Unknown_1Bh { get; set; } //Padding
        public Rsc6Ptr<Rsc6ClipTags> Tags { get; set; } //m_Tags
        public Rsc6Ptr<Rsc6ClipPropertyMap> Properties { get; set; } //m_Properties
        public uint RefCount { get; set; } = 1; //m_RefCount

        public JenkHash NameHash; //Used by the parent clip dictionary

        public virtual void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            ClipType = (Rsc6ClipType)reader.ReadUInt32();
            ClipName = reader.ReadStr();
            ClipNameLength1 = reader.ReadInt16();
            ClipNameLength2 = reader.ReadInt16();
            Comment = reader.ReadStr();
            CommentLength1 = reader.ReadInt16();
            CommentLength2 = reader.ReadInt16();
            IsLooped = reader.ReadBoolean();
            Unknown_19h = reader.ReadByte();
            Unknown_1Ah = reader.ReadByte();
            Unknown_1Bh = reader.ReadByte();
            Tags = reader.ReadPtr<Rsc6ClipTags>();
            Properties = reader.ReadPtr<Rsc6ClipPropertyMap>();
            RefCount = reader.ReadUInt32();

            Name = ClipName.Value ?? "";
            JenkIndex.Ensure(Path.GetFileNameWithoutExtension(Name), "RDR1");
            JenkIndex.Ensure(Path.GetFileName(Path.GetDirectoryName(Name)), "RDR1");
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            ClipNameLength1 = (short)(ClipName.Value?.Length ?? 0);
            ClipNameLength2 = (short)(ClipNameLength1 + 1);

            CommentLength1 = (short)(Comment.Value?.Length ?? 0);
            CommentLength2 = (short)(CommentLength1 + 1);

            writer.WriteUInt32(VFT);
            writer.WriteUInt32((uint)ClipType);
            writer.WriteStr(ClipName);
            writer.WriteInt16(ClipNameLength1);
            writer.WriteInt16(ClipNameLength2);
            writer.WriteStr(Comment);
            writer.WriteInt16(CommentLength1);
            writer.WriteInt16(CommentLength2);
            writer.WriteBoolean(IsLooped);
            writer.WriteByte(Unknown_19h);
            writer.WriteByte(Unknown_1Ah);
            writer.WriteByte(Unknown_1Bh);
        }

        public override void Read(MetaNodeReader reader)
        {
            
        }

        public override void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", ClipType.ToString());
            writer.WriteString("Name", Name);
            writer.WriteNodeArray("Tags", Tags.Item?.Tags.Items);
            writer.WriteNodeArray("Properties", Properties.Item?.Properties);
        }

        public static Rsc6Clip Create(string typeName)
        {
            if (Enum.TryParse(typeName, out Rsc6ClipType type))
            {
                return Create(type);
            }
            return null;
        }

        public static Rsc6Clip Create(Rsc6DataReader reader)
        {
            reader.Position += 4;
            var type = (Rsc6ClipType)reader.ReadByte();
            reader.Position -= 5;
            return Create(type);
        }

        public static Rsc6Clip Create(Rsc6ClipType type)
        {
            return type switch
            {
                Rsc6ClipType.SINGLE => new Rsc6ClipSingle(),
                Rsc6ClipType.MULTI => new Rsc6ClipMulti(),
                _ => null,
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Rsc6ClipSingle : Rsc6Clip //rage::crClipAnimation
    {
        public override ulong BlockLength => base.BlockLength + 16;
        public Rsc6Ptr<Rsc6Animation> AnimationRef { get; set; } //m_Animation
        public JenkHash AnimationHash { get; set; }//Used by XML

        public Rsc6ClipSingle()
        {
            ClipType = Rsc6ClipType.SINGLE;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            AnimationRef = reader.ReadPtr<Rsc6Animation>();
            StartTime = reader.ReadSingle(); //m_StartTime
            EndTime = reader.ReadSingle(); //m_EndTime
            Speed = reader.ReadSingle(); //m_Rate

            Animation = AnimationRef.Item;
            AnimationHash = AnimationRef.Item?.Hash ?? 0;
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(AnimationRef);
            writer.WriteSingle(StartTime);
            writer.WriteSingle(EndTime);
            writer.WriteSingle(Speed);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            AnimationHash = new(reader.ReadString("Animation"));
            StartTime = reader.ReadSingle("StartTime");
            EndTime = reader.ReadSingle("EndTime");
            Speed = reader.ReadSingle("Speed");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteString("Animation", AnimationHash.ToString());
            writer.WriteSingle("StartTime", StartTime);
            writer.WriteSingle("EndTime", EndTime);
            writer.WriteSingle("Speed", Speed);
        }
    }

    public class Rsc6ClipMulti : Rsc6Clip
    {
        public override ulong BlockLength => base.BlockLength;

        public Rsc6ClipMulti()
        {
            ClipType = Rsc6ClipType.MULTI;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            throw new NotImplementedException("Rsc6ClipMulti: Not implemented!");
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
        }
    }

    public class Rsc6ClipTags : Rsc6BlockBase //rage::crTags
    {
        /*
         * Manages a time line of tags  
         * Performs containment, sorting, searching and editing operations.
         */

        public override ulong BlockLength => 16;
        public Rsc6Str Name { get; set; } //m_Name
        public short NameLength1 { get; set; } //Name length
        public short NameLength2 { get; set; } //Name length + 1
        public Rsc6PtrArr<Rsc6ClipTag> Tags { get; set; } // m_Tags

        public override void Read(Rsc6DataReader reader)
        {
            Name = reader.ReadStr();
            NameLength1 = reader.ReadInt16();
            NameLength2 = reader.ReadInt16();
            Tags = reader.ReadPtrArr<Rsc6ClipTag>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            NameLength1 = (short)(Name.Value?.Length ?? 0);
            NameLength2 = (short)(NameLength1 + 1);

            writer.WriteStr(Name);
            writer.WriteInt16(NameLength1);
            writer.WriteInt16(NameLength2);
            writer.WritePtrArr(Tags);
        }
    }

    public class Rsc6ClipTag : Rsc6BlockBase, MetaNode //rage::crTag
    {
        /*
         * Tags are light weight phase based wrapper for properties, enabling clips (and parameterized motion)
         * to be marked up with temporal based metadata.
        */

        public override ulong BlockLength => 32;
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; }
        public float Start { get; set; } //m_Start, tag start phase [0..1]
        public float Mid { get; set; } //m_Mid, tag mid phase [0..1]
        public float End { get; set; } //m_End, tag end phase [0..1]
        public ushort Flags { get; set; } //m_Flags
        public ushort Unknown_16h { get; set; }
        public ushort Type { get; set; } //m_Type, always 2
        public byte TrackNumber { get; set; } //m_TrackNumber, always 0
        public byte Unknown_1Bh { get; set; } //Always 0
        public uint Unknown_1Ch { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            Unknown_0h = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Start = reader.ReadSingle();
            Mid = reader.ReadSingle();
            End = reader.ReadSingle();
            Flags = reader.ReadUInt16();
            Unknown_16h = reader.ReadUInt16();
            Type = reader.ReadUInt16();
            TrackNumber = reader.ReadByte();
            Unknown_1Bh = reader.ReadByte();
            Unknown_1Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Unknown_0h);
            writer.WriteUInt32(Unknown_4h);
            writer.WriteSingle(Start);
            writer.WriteSingle(Mid);
            writer.WriteSingle(End);
            writer.WriteUInt16(Flags);
            writer.WriteUInt16(Unknown_16h);
            writer.WriteUInt16(Type);
            writer.WriteByte(TrackNumber);
            writer.WriteByte(Unknown_1Bh);
            writer.WriteUInt32(Unknown_1Ch);
        }

        public void Read(MetaNodeReader reader)
        {

        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Unknown_0h", Unknown_0h);
            writer.WriteUInt32("Unknown_4h", Unknown_4h);
            writer.WriteSingle("StartPhase", Start);
            writer.WriteSingle("MidPhase", Mid);
            writer.WriteSingle("EndPhase", End);
        }

        public bool HasDuration() => Start != End; //Does tag have duration (or is it instant)

        public float GetDuration() //Get tag duration in phase [0..1]
        {
            if (Start <= End)
                return End - Start;
            else
                return End - Start + 1.0f;
        }

        public float GetMid() //Get tag mid phase [0..1]
        {
            if (Start < End)
                return (Start + End) * 0.5f;
            else if (End > Start)
            {
                float mid = (Start + End + 1.0f) * 0.5f;
                if (mid > 1.0f)
                    mid -= MathF.Floor(mid);
                return mid;
            }
            return Start;
        }

        public override string ToString()
        {
            return $"Start: {Start}, Mid: {Mid}, End: {End}";
        }
    }

    public class Rsc6ClipPropertyMap : Rsc6BlockBase //rage::crProperties
    {
        /*
         * Manages a collection of properties (timeless metadata)
         * Performs containment, efficient retrieval and editing operations, plus serialization.
         */

        public override ulong BlockLength => 12;
        public Rsc6AtMapArr<Rsc6ClipPropertyMapItem> Items { get; set; } //m_Properties
        public Rsc6ClipProperty[] Properties => Rsc6DataMap.Flatten(Items.Items, e => e?.Property.Item).ToArray();

        public override void Read(Rsc6DataReader reader)
        {
            Items = reader.ReadAtMapArr<Rsc6ClipPropertyMapItem>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteAtMapArr(Items);
        }
    }

    public class Rsc6ClipPropertyMapItem : Rsc6BlockBase, IRsc6DataMapEntry<Rsc6ClipPropertyMapItem> //rage::crProperties::PropertyPair
    {
        public override ulong BlockLength => 16;
        public JenkHash Hash { get; set; }
        public Rsc6Str PropertyName { get; set; } //m_Name
        public short PropertyNameLength1 { get; set; } //PropertyName size
        public short PropertyNameLength2 { get; set; } //PropertyName size + 1
        public Rsc6Ptr<Rsc6ClipProperty> Property { get; set; }
        public Rsc6Ptr<Rsc6ClipPropertyMapItem> Next { get; set; }
        public uint MapKey { get => Hash; set => Hash = value; }
        public Rsc6ClipPropertyMapItem MapNext { get => Next.Item; set => Next = new(value); }

        public override void Read(Rsc6DataReader reader)
        {
            Hash = reader.ReadUInt32();
            PropertyName = reader.ReadStr();
            PropertyNameLength1 = reader.ReadInt16();
            PropertyNameLength2 = reader.ReadInt16();
            Property = reader.ReadPtr(reader => Rsc6ClipProperty.Create(reader, PropertyName.Value));
            Next = reader.ReadPtr<Rsc6ClipPropertyMapItem>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            
        }

        public override string ToString()
        {
            return $"{Hash} : {Property.Item}";
        }
    }

    public class Rsc6ClipProperty : Rsc6FileBase, MetaNode //rage::crProperty
    {
        public override ulong BlockLength => 8;
        public override uint VFT { get; set; }
        public Rsc6ClipPropertyAttributeType PropertyType1 { get; set; } //0, 1 or 2 - If it is 0, then PropertyType2 is used
        public Rsc6ClipPropertyAttributeType PropertyType2 { get; set; } //0 if PropertyType1 is greater than 0
        public ushort Unknown_6h { get; set; } //Always 0?
        public string PropertyName { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            PropertyType1 = (Rsc6ClipPropertyAttributeType)reader.ReadByte();
            PropertyType2 = (Rsc6ClipPropertyAttributeType)reader.ReadByte();
            Unknown_6h = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            
        }

        public virtual void Read(MetaNodeReader reader)
        {
            
        }

        public virtual void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", (PropertyType1 == 0) ? PropertyType2.ToString() : PropertyType1.ToString());
            writer.WriteString("Name", PropertyName);
        }

        public static Rsc6ClipProperty Create(Rsc6DataReader reader, string propertyName)
        {
            reader.Position += 4;
            var type1 = (Rsc6ClipPropertyAttributeType)reader.ReadByte();
            var type2 = (Rsc6ClipPropertyAttributeType)reader.ReadByte();
            reader.Position -= 6;
            return Create((type1 == 0) ? type2 : type1, propertyName);
        }

        public static Rsc6ClipProperty Create(Rsc6ClipPropertyAttributeType type, string propertyName)
        {
            return type switch
            {
                Rsc6ClipPropertyAttributeType.FLOAT => new Rsc6ClipPropertyAttributeFloat(propertyName),
                Rsc6ClipPropertyAttributeType.INT => new Rsc6ClipPropertyAttributeInt(propertyName),
                Rsc6ClipPropertyAttributeType.BOOL => new Rsc6ClipPropertyAttributeBool(propertyName),
                Rsc6ClipPropertyAttributeType.STRING => new Rsc6ClipPropertyAttributeString(propertyName),
                Rsc6ClipPropertyAttributeType.VECTOR3 => new Rsc6ClipPropertyAttributeVector3(propertyName),
                Rsc6ClipPropertyAttributeType.VECTOR4 => new Rsc6ClipPropertyAttributeVector4(propertyName),
                //Rsc6ClipPropertyAttributeType.HashString => new Rsc6ClipPropertyAttributeHashString(),
                _ => new Rsc6ClipProperty(),
            };
        }

        public override string ToString()
        {
            return (PropertyType1 == 0) ? PropertyType2.ToString() : PropertyType1.ToString();
        }
    }

    public class Rsc6ClipPropertyAttributeFloat : Rsc6ClipProperty //rage::crPropertyFloat
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public float Value { get; set; } //m_Float
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public Rsc6ClipPropertyAttributeFloat(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Value = reader.ReadSingle();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle(Value);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = reader.ReadSingle("Value");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle("Value", Value);
        }
    }

    public class Rsc6ClipPropertyAttributeInt : Rsc6ClipProperty //rage::crPropertyInt
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public int Value { get; set; } //m_Int
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public Rsc6ClipPropertyAttributeInt(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Value = reader.ReadInt32();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteInt32(Value);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = reader.ReadInt32("Value");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteInt32("Value", Value);
        }
    }

    public class Rsc6ClipPropertyAttributeBool : Rsc6ClipProperty
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public uint Value { get; set; }
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public Rsc6ClipPropertyAttributeBool(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Value = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Value);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = reader.ReadUInt32("Value");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32("Value", Value);
        }
    }

    public class Rsc6ClipPropertyAttributeString : Rsc6ClipProperty //rage::crPropertyString
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6Str Value { get; set; } //m_String
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public Rsc6ClipPropertyAttributeString(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Value = reader.ReadStr();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteStr(Value);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = new(reader.ReadString("Value"));
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteString("Value", Value.Value);
        }
    }

    public class Rsc6ClipPropertyAttributeVector3 : Rsc6ClipProperty //rage::crPropertyVector3
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6PtrUnmanaged<Vector4> Value { get; set; } //m_Vector3, with a W component -_-
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public Rsc6ClipPropertyAttributeVector3(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Value = reader.ReadPtrUnmanaged<Vector4>();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtrUnmanaged(Value);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = new(reader.ReadVector4("Value"));
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("Value", Value.Item);
        }
    }

    public class Rsc6ClipPropertyAttributeVector4 : Rsc6ClipProperty //rage::crPropertyVector4
    {
        public override ulong BlockLength => base.BlockLength + 24;
        public uint Unknown_8h { get; set; }
        public uint Unknown_Ch { get; set; }
        public Vector4 Value { get; set; } //m_Vector4

        public Rsc6ClipPropertyAttributeVector4(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            Value = reader.ReadVector4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteVector4(Value);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = reader.ReadVector4("Value");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("Value", Value);
        }
    }

    public class Rsc6AnimDictionary : Rsc6BlockBaseMapRef, MetaNode //rage::crAnimDictionary
    {
        public override ulong BlockLength => 24;
        public override uint VFT { get; set; } = 0x01322FA4;
        public Rsc6PtrArr<Rsc6AnimDictionaryEntry> AnimEntries { get; set; } //m_Animations
        public ushort Unknown_14h { get; set; } = 0xCDCD; //Pad
        public byte Unknown_16h { get; set; } = 0xCD; //Pad
        public byte BaseNameKeys { get; set; } //m_BaseNameKeys, always 1?

        public Rsc6Animation[] Animations { get; set; }
        public Dictionary<uint, Rsc6Animation> Dict;
        public string[] AnimTypes;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            AnimEntries = reader.ReadPtrArr<Rsc6AnimDictionaryEntry>(); //atMap<rage::crAnimation>
            Unknown_14h = reader.ReadUInt16();
            Unknown_16h = reader.ReadByte();
            BaseNameKeys = reader.ReadByte();

            var list = new List<Rsc6Animation>();
            foreach (var entry in AnimEntries.Items)
            {
                if (entry == null) continue;
                if (entry.Anim.Item == null) continue;
                list.Add(entry.Anim.Item);
            }

            Animations = list.ToArray();
            CreateDict();

            AnimTypes = new string[Animations.Length];
            for (int i = 0; i < Animations.Length; i++)
            {
                var anim = Animations[i];
                var str = "animationCompress";
                var name = anim.Name;

                if (name.Contains("animOut") || !name.Contains(str))
                {
                    AnimTypes[i] = "animTest";
                    continue;
                }

                var start = name.IndexOf(str) + str.Length + 1;
                int nextSlashIndex = name.IndexOf('\\', start);

                if (nextSlashIndex != -1)
                    AnimTypes[i] = name[start..nextSlashIndex];
                else
                    AnimTypes[i] = name[start..];
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public void Read(MetaNodeReader reader)
        {
            
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Animations", Animations);
        }

        public void CreateDict()
        {
            var dict = new Dictionary<uint, Rsc6Animation>();
            foreach (var entry in AnimEntries.Items)
            {
                if (entry == null) continue;
                var anim = entry.Anim.Item;
                if (anim == null) continue;
                dict[JenkHash.GenHash(anim.Name)] = anim;
            }
            Dict = dict;
        }
    }

    public class Rsc6AnimDictionaryEntry : Rsc6BlockBase
    {
        public override ulong BlockLength => 12;

        public uint MapKey
        {
            get => Hash;
            set => Hash = value;
        }

        public Rsc6AnimDictionaryEntry MapNext
        {
            get => Next.Item;
            set => Next = new(value);
        }

        public JenkHash Hash; //Name hash of the anim
        public Rsc6Ptr<Rsc6Animation> Anim;
        public Rsc6Ptr<Rsc6AnimDictionaryEntry> Next;

        public override void Read(Rsc6DataReader reader)
        {
            Hash = reader.ReadUInt32();
            Anim = reader.ReadPtr<Rsc6Animation>();
            Next = reader.ReadPtr<Rsc6AnimDictionaryEntry>();

            if (Anim.Item != null)
            {
                Anim.Item.Hash = Hash;
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            
        }

        public override string ToString()
        {
            return $"Animation: {Anim.Item?.Name}, Duration: {Anim.Item?.Duration}";
        }
    }

    public class Rsc6Animation : Animation, IRsc6Block //rage::crAnimation
    {
        /*
         * Animations represent the change in a series of values (tracks) over a period of time (duration)
         * The crAnimation class hides the internal storage of these tracks, the channels and compression used
         * The animation also constructs a parallel structure of blocks and chunks, to help organize the memory layout of the animation data in a more temporal fashion
         */

        public ulong BlockLength => 48;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public uint VFT { get; set; }
        public Rsc6AnimationFlags Flags { get; set; } //m_Flags, RAGE flags
        public ushort ProjectFlags { get; set; } //m_ProjectFlags, project specific flags
        public ushort FramesPerChunk { get; set; } = 127; //m_FramesPerChunk, frames per chunk (excluding any terminating frames)
        public uint Signature { get; set; } //m_Signature, animation signature (a hash of the animation structure)
        public Rsc6PtrArr<Rsc6AnimBlock> Blocks { get; set; } //m_Blocks, rage::crAnimBlock
        public Rsc6Str NameRef { get; set; } //m_Name
        public Rsc6ManagedArr<Rsc6AnimTrack> Tracks { get; set; } //m_Tracks, animation tracks, structurally organized view of the compressed animation data
        public uint MaxBlockSize { get; set; } //m_MaxBlockSize, maximum block size, if animation data is packed
        public ulong RefCount { get; set; } //m_RefCount

        public List<Rsc6AnimBoneId> BoneIds { get; set; }
        public JenkHash Hash { get; set; } //Used by XML
        public string RefactoredName => Name[(Name.LastIndexOf('\\') + 1)..];

        public void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Flags = (Rsc6AnimationFlags)reader.ReadUInt16();
            ProjectFlags = reader.ReadUInt16();
            FrameCount = reader.ReadUInt16();
            FramesPerChunk = reader.ReadUInt16();
            Duration = reader.ReadSingle(); //Inherited from Animation, duration of the animation in seconds
            Signature = reader.ReadUInt32();
            Blocks = reader.ReadPtrArr<Rsc6AnimBlock>();
            NameRef = reader.ReadStr();
            Tracks = reader.ReadArr<Rsc6AnimTrack>();
            MaxBlockSize = reader.ReadUInt32();
            RefCount = reader.ReadUInt32();

            Name = NameRef.Value;
            BoneIds = [];

            var block = Blocks[0];
            for (int i1 = 0; i1 < block.Chunks.Count; i1++)
            {
                var chunk = block.Chunks[i1];
                if (chunk == null) continue;
                BoneIds.Add(chunk.BoneId);
            }
        }

        public void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable;
            writer.WriteUInt32(wfd ? (uint)0x00FA2AF0 : 0x011C2AF0);
        }

        public override void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt16("Flags", (ushort)Flags);
            writer.WriteUInt16("ProjectFlags", ProjectFlags);
            writer.WriteUInt16("FrameCount", (ushort)FrameCount);
            writer.WriteUInt16("FramesPerChunk", FramesPerChunk);
            writer.WriteSingle("Duration", Duration);
            writer.WriteUInt32("Signature", Signature);
            writer.WriteString("Name", NameRef.Value ?? "Unknown");
            if (Blocks.Items != null) writer.WriteNodeArray("Blocks", Blocks.Items);
            writer.WriteUInt64("MaxBlockSize", MaxBlockSize);
        }

        public override Vector4 Evaluate(in AnimationFramePosition frame, int track)
        {
            var s = frame.Frame0 / FramesPerChunk;
            int f0 = frame.Frame0 % FramesPerChunk;
            int f1 = f0 + 1;
            var seq = Blocks.Items[s % Blocks.Items.Length];
            var aseq = seq.Chunks[track];
            var v0 = aseq.EvaluateVector(f0);
            var v1 = aseq.EvaluateVector(f1);
            var v = (v0 * frame.Alpha0) + (v1 * frame.Alpha1);
            return v;
        }
    }

    public class Rsc6AnimBlock : Rsc6BlockBase, MetaNode //rage::crAnimBlock
    {
        public override ulong BlockLength => 32;
        public uint Offset { get; set; } //Pointer to the current anim block
        public Rsc6PtrArr<Rsc6AnimChunk> Chunks { get; set; } //m_Chunks, rage::crAnimChunk
        public uint BlockSize { get; set; } //m_BlockSize
        public ushort NumFrames { get; set; } //m_NumFrames
        public ushort Flags { get; set; } //m_Flags
        public uint CompactBlockSize { get; set; } //m_CompactBlockSize
        public uint CompactSlopSize { get; set; } //m_CompactSlopSize
        public uint Unknown_1Ch { get; set; } //Always 0?

        public override void Read(Rsc6DataReader reader)
        {
            Offset = reader.ReadUInt32();
            Chunks = reader.ReadPtrArr<Rsc6AnimChunk>();
            BlockSize = reader.ReadUInt32();
            NumFrames = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            CompactBlockSize = reader.ReadUInt32();
            CompactSlopSize = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Offset);
            writer.WritePtrArr(Chunks);
            writer.WriteUInt32(BlockSize);
            writer.WriteUInt16(NumFrames);
            writer.WriteUInt16(Flags);
            writer.WriteUInt32(CompactBlockSize);
            writer.WriteUInt32(CompactSlopSize);
            writer.WriteUInt32(Unknown_1Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("BlockSize", BlockSize);
            writer.WriteUInt16("NumFrames", NumFrames);
            writer.WriteUInt16("Flags", Flags);
            writer.WriteUInt32("CompactBlockSize", CompactBlockSize);
            writer.WriteUInt32("CompactSlopSize", CompactSlopSize);
            if (Chunks.Items != null) writer.WriteNodeArray("Chunks", Chunks.Items);
        }
    }

    public class Rsc6AnimTrack : Rsc6BlockBase //rage::crAnimTrack
    {
        /*
         * Internal storage class of crAnimation
         * Tracks represent change in single value, which may be of type float/vector3/quaternion etc over the entire duration of an animation
         * Internally tracks hold their values in a series of one of more chunks, which then in turn compress their data within one or more channels
         * Chunking and compressing of data is deliberately hidden from the end user.
         */

        public override ulong BlockLength => 16;
        public Rsc6AnimBoneId BoneId { get; set; }
        public ushort FramesPerChunk { get; set; } //m_FramesPerChunk, number of internal frames per chunk
        public ushort Flags { get; set; } //m_Flags
        public Rsc6ManagedArr<Rsc6AnimChunk> Chunks { get; set; } //m_Chunks, rage::crAnimChunk

        public override void Read(Rsc6DataReader reader)
        {
            BoneId = reader.ReadStruct<Rsc6AnimBoneId>();
            FramesPerChunk = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            Chunks = reader.ReadArr<Rsc6AnimChunk>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteStruct(BoneId);
            writer.WriteUInt16(FramesPerChunk);
            writer.WriteUInt16(Flags);
            writer.WriteArr(Chunks);
        }

        public uint CalcNumChunks(uint numFrames)
        {
	        if(numFrames > 1)
	        {
		        return ((numFrames - 2) / FramesPerChunk) + 1;
	        }
	        return 1;
        }

        public override string ToString()
        {
            return $"BoneId: {BoneId}, FramesPerChunk: {FramesPerChunk}";
        }
    }

    public class Rsc6AnimChunk : Rsc6BlockBase, MetaNode //rage::crAnimChunk
    {
        /*
         * Animation chunks represent the change in a value over a short period of time
         * They may store compound types (ie vectors, quaternions) or basic types (ie floats, integers etc)
         * The period of time may be that of the entire animation, or some short subsection of it
         * Internally they pack this changing value using one or more channels, which can use a variety of different compression techniques
         */

        public override ulong BlockLength => 16;
        public Rsc6AnimBoneId BoneId { get; set; }
        public Rsc6AnimChannel[] Channels { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            BoneId = reader.ReadStruct<Rsc6AnimBoneId>();
            var channels = new List<Rsc6AnimChannel>();
            for (int i = 0; i < 4; i++) //atFixedArray, m_Channels
            {
                var channel = reader.ReadPtr<Rsc6AnimChannel>();
                if (channel.Item != null)
                {
                    channels.Add(channel.Item);
                }
            }
            Channels = [.. channels];
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Read(MetaNodeReader reader)
        {
            
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteEnum("TrackId", BoneId.TrackId);
            writer.WriteByte("FormatId", BoneId.TypeId);
            writer.WriteString("BoneId", BoneId.ID.ToString());
            writer.WriteNodeArray("Channels", Channels);
        }

        public Vector4 EvaluateVector(int frame)
        {
            var v = Vector4.Zero;
            var c = 0;

            for (int i = 0; i < Channels.Length; i++)
            {
                var pack = BoneId.GetPack();
                var channel = Channels[i];
                if (channel == null) continue;

                switch (channel.ChannelType)
                {
                    case Rsc6AnimChannelType.STATIC_FLOAT:
                    case Rsc6AnimChannelType.RAW_FLOAT:
                    case Rsc6AnimChannelType.QUANTIZE_FLOAT:
                    case Rsc6AnimChannelType.INDIRECT_QUANTIZE_FLOAT:
                    case Rsc6AnimChannelType.LINEAR_FLOAT:
                        v[c] = channel.EvaluateFloat(frame);
                        c++;
                        break;
                    case Rsc6AnimChannelType.STATIC_VECTOR3:
                        channel.EvaluateVector(frame, c, ref v);
                        c += 3;
                        break;
                    case Rsc6AnimChannelType.STATIC_QUATERNION:
                        channel.EvaluateVector(frame, c, ref v);
                        c += 4;
                        break;
                    case Rsc6AnimChannelType.SMALLEST_THREE_QUATERNION:
                        channel.EvaluateVector(frame, c, ref v);
                        c++;
                        break;
                }
            }
            return v;
        }

        public override string ToString()
        {
            return $"{BoneId.ID} : {Channels.Length} channels";
        }
    }

    public class Rsc6AnimChannel : Rsc6ChannelAttribute, IRsc6Block //rage::crAnimChannel
    {
        /*
         * Animation channels represent the change in value over time of a single type.
         * They may store compound types (ie vectors, quaternions) or basic types (ie floats, integers etc)
         * There are many different types of channel, for storing all the different value types, and using different compression techniques
         */

        public Rsc6ChannelAttribute Attribute;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Attribute = Create(ChannelType);
            Attribute.Read(reader);
            Attribute.ChannelType = ChannelType;
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            Attribute.Write(writer);
        }

        public override void Read(MetaNodeReader reader)
        {

        }

        public override void Write(MetaNodeWriter writer)
        {
            Attribute.Write(writer);
        }

        public override float EvaluateFloat(int frame)
        {
            return Attribute.EvaluateFloat(frame);
        }

        public override void EvaluateVector(int frame, int c, ref Vector4 v)
        {
            Attribute.EvaluateVector(frame, c, ref v);
        }

        public static Rsc6ChannelAttribute Create(Rsc6AnimChannelType type)
        {
            return type switch
            {
                Rsc6AnimChannelType.STATIC_QUATERNION => new Rsc6ChannelStaticQuaternion(),
                Rsc6AnimChannelType.STATIC_VECTOR3 => new Rsc6ChannelStaticVector3(),
                Rsc6AnimChannelType.STATIC_FLOAT => new Rsc6ChannelStaticFloat(),
                Rsc6AnimChannelType.RAW_FLOAT => new Rsc6ChannelRawFloat(),
                Rsc6AnimChannelType.QUANTIZE_FLOAT => new Rsc6ChannelQuantizeFloat(),
                Rsc6AnimChannelType.SMALLEST_THREE_QUATERNION => new Rsc6ChannelSmallestThreeQuaternion(),
                _ => throw new NotImplementedException($"Rsc6ChannelAttribute: Unknown type: {type}")
            };
        }

        public override string ToString()
        {
            return $"{Attribute.ChannelType}";
        }
    }

    public abstract class Rsc6ChannelAttribute : IRsc6Block, MetaNode
    {
        public virtual ulong FilePosition { get; set; }
        public virtual ulong BlockLength => 8;
        public virtual bool IsPhysical => false;

        public byte Flags;
        public byte Type;
        public byte Unknown_2h;
        public ushort Unknown_3h; //Padding
        public Rsc6AnimChannelType ChannelType;
        public byte CompressCost;
        public byte DecompressCost;

        public virtual void Read(Rsc6DataReader reader)
        {
            Flags = reader.ReadByte();
            Type = reader.ReadByte();
            Unknown_2h = reader.ReadByte();
            Unknown_3h = reader.ReadUInt16();
            ChannelType = (Rsc6AnimChannelType)reader.ReadByte();
            CompressCost = reader.ReadByte();
            DecompressCost = reader.ReadByte();
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            writer.WriteByte(Flags);
            writer.WriteByte(Type);
            writer.WriteByte(Unknown_2h);
            writer.WriteUInt16(Unknown_3h);
            writer.WriteByte((byte)ChannelType);
            writer.WriteByte(CompressCost);
            writer.WriteByte(DecompressCost);
        }

        public virtual void Read(MetaNodeReader reader)
        {
        }

        public virtual void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", ChannelType.ToString());
            writer.WriteByte("Flags", Flags);
            writer.WriteString("Type", Type.ToString());
            writer.WriteByte("CompressCost", CompressCost);
            writer.WriteByte("DecompressCost", DecompressCost);
        }

        public virtual float EvaluateFloat(int frame)
        {
            return 0;
        }

        public virtual void EvaluateVector(int frame, int c, ref Vector4 v)
        {

        }
    }

    public class Rsc6ChannelStaticVector3 : Rsc6ChannelAttribute //rage::crAnimChannelStaticVector3
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6PtrUnmanaged<Vector3> Value { get; set; }
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            Value = reader.ReadPtrUnmanaged<Vector3>();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrUnmanaged(Value);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = new(reader.ReadVector3("Value"));
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector3("Value", Value.Item);
        }

        public override void EvaluateVector(int frame, int c, ref Vector4 v)
        {
            v.X = Value.Item.Y;
            v.Y = Value.Item.Z;
            v.Z = Value.Item.X;
        }
    }

    public class Rsc6ChannelStaticFloat : Rsc6ChannelAttribute //rage::crAnimChannelStaticFloat
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public float Value { get; set; } //m_Float
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            Value = reader.ReadSingle();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteSingle(Value);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {

        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle("Value", Value);
        }

        public override float EvaluateFloat(int frame)
        {
            return Value;
        }
    }

    public class Rsc6ChannelStaticQuaternion : Rsc6ChannelAttribute //rage::crAnimChannelStaticQuaternion
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6PtrUnmanaged<Quaternion> Value { get; set; }
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            Value = reader.ReadPtrUnmanaged<Quaternion>();
            Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrUnmanaged(Value);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {

        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("Value", Value.Item.ToVector4());
        }

        public override void EvaluateVector(int frame, int c, ref Vector4 v)
        {
            if (c != 0) return;
            v = Value.Item.ToVector4();
            v = new Vector4(v.Y, v.Z, v.X, v.W);
        }
    }

    public class Rsc6ChannelRawFloat : Rsc6ChannelAttribute //rage::crAnimChannelRawFloat
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6Arr<float> Values { get; set; } //m_Floats

        public override void Read(Rsc6DataReader reader)
        {
            Values = reader.ReadArr<float>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(Values);
        }

        public override void Read(MetaNodeReader reader)
        {

        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteSingleArray("Value", Values.Items);
        }

        public override float EvaluateFloat(int frame)
        {
            if (Values.Items == null) return 0;
            return Values[frame % Values.Items.Length];
        }
    }

    public class Rsc6ChannelQuantizeFloat : Rsc6ChannelAttribute //rage::crAnimChannelQuantizeFloat
    {
        public override ulong BlockLength => base.BlockLength + 24;
        public Rsc6PackedArr QuantizedValues { get; set; } //m_QuantizedValues
        public float Scale { get; set; } //m_Scale
        public float Offset { get; set; } //m_Offset
        public uint Unknown_Ch { get; set; } = 0xCDCDCDCD;

        public float[] Values;

        public override void Read(Rsc6DataReader reader)
        {
            QuantizedValues = reader.ReadPackedArr(); //atPackedArray
            Scale = reader.ReadSingle();
            Offset = reader.ReadSingle();
            Unknown_Ch = reader.ReadUInt32();

            var count = QuantizedValues.ElementMax;
            Values = new float[count];

            for (uint i = 0; i < count; i++)
            {
                Values[i] = (QuantizedValues.GetElement(i) * Scale) + Offset;
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePackedArr(QuantizedValues);
            writer.WriteSingle(Scale);
            writer.WriteSingle(Offset);
            writer.WriteUInt32(Unknown_Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Values = reader.ReadSingleArray("Values");
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteSingleArray("Values", Values);
        }

        public override float EvaluateFloat(int frame)
        {
            if (Values == null) return 0;
            return Values[frame % Values.Length];
        }
    }

    public class Rsc6ChannelSmallestThreeQuaternion : Rsc6ChannelAttribute //rage::crAnimChannelSmallestThreeQuaternion
    {
        public override ulong BlockLength => base.BlockLength + 28;
        public QuaternionQuantizedFloats[] QuantizedFloats { get; set; } //m_QuantizedFloats
        public Rsc6FormatReconstructOrder QuantizedOrder { get; set; } //m_QuantizedOrder

        public override void Read(Rsc6DataReader reader)
        {
            QuantizedFloats = new QuaternionQuantizedFloats[3];
            for (int i = 0; i < QuantizedFloats.Length; i++) //atRangeArray
            {
                QuantizedFloats[i] = new QuaternionQuantizedFloats();
                QuantizedFloats[i].Read(reader);
            }
            QuantizedOrder = (Rsc6FormatReconstructOrder)reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            for (int i = 0; i < QuantizedFloats.Length; i++)
            {
                QuantizedFloats[i].Write(writer);
            }
            writer.WriteUInt32((uint)QuantizedOrder);
        }

        public override void EvaluateVector(int frame, int c, ref Vector4 vector)
        {
            var q0 = QuantizedFloats[0].ScaleAndValues.Item;
            var q1 = QuantizedFloats[1].ScaleAndValues.Item;
            var q2 = QuantizedFloats[2].ScaleAndValues.Item;

            var qv0 = (q0 == null) ? 0 : q0.QuantizedValues.GetElement((uint)c);
            var qv1 = (q1 == null) ? 0 : q1.QuantizedValues.GetElement((uint)c);
            var qv2 = (q2 == null) ? 0 : q2.QuantizedValues.GetElement((uint)c);

            var x = qv0 * ((q0 == null) ? 1.0f : q0.Scale) + QuantizedFloats[0].Offset;
            var y = qv1 * ((q1 == null) ? 1.0f : q1.Scale) + QuantizedFloats[1].Offset;
            var z = qv2 * ((q2 == null) ? 1.0f : q2.Scale) + QuantizedFloats[2].Offset;
            var w = (float)Math.Sqrt(Math.Max(1.0f - (x * x + y * y + z * z), 0.0));

            vector = QuantizedOrder switch
            {
                Rsc6FormatReconstructOrder.FORMAT_RECONSTRUCT_X => new Vector4(w, x, y, z), //w, z, x, y
                Rsc6FormatReconstructOrder.FORMAT_RECONSTRUCT_Y => new Vector4(x, w, y, z), //z, w, x, y
                Rsc6FormatReconstructOrder.FORMAT_RECONSTRUCT_Z => new Vector4(x, y, w, z), //z, x, w, y
                Rsc6FormatReconstructOrder.FORMAT_RECONSTRUCT_W => new Vector4(x, y, z, w), //z, x, y, w
                _ => vector,
            };
        }

        public class QuaternionQuantizedFloats //rage::crAnimChannelSmallestThreeQuaternion::QuantizedFloats
        {
            public float Offset { get; set; } //m_Offset
            public Rsc6Ptr<QuaternionQuantizedScaleFloats> ScaleAndValues { get; set; } //m_ScaleAndValues

            public void Read(Rsc6DataReader reader)
            {
                Offset = reader.ReadSingle();
                ScaleAndValues = reader.ReadPtr<QuaternionQuantizedScaleFloats>();
            }

            public void Write(Rsc6DataWriter writer)
            {
                writer.WriteSingle(Offset);
                writer.WritePtr(ScaleAndValues);
            }

            public override string ToString()
            {
                return $"Offset: {Offset}, ScaleAndValues: {ScaleAndValues}";
            }
        }

        public class QuaternionQuantizedScaleFloats : IRsc6Block //rage::crAnimChannelSmallestThreeQuaternion::QuantizedFloats::ScaleAndValues
        {
            public ulong FilePosition { get; set; }
            public ulong BlockLength => 16;
            public bool IsPhysical => false;
            public float Scale { get; set; } //m_Scale
            public Rsc6PackedArr QuantizedValues { get; set; } //m_QuantizedValues

            public void Read(Rsc6DataReader reader)
            {
                Scale = reader.ReadSingle();
                QuantizedValues = reader.ReadPackedArr(); //atPackedArray
            }

            public void Write(Rsc6DataWriter writer)
            {
                writer.WriteSingle(Scale);
                writer.WritePackedArr(QuantizedValues);
            }

            public override string ToString()
            {
                return $"Scale: {Scale}";
            }
        }
    }

    public struct Rsc6AnimBoneId
    {
        public Rsc6TrackID TrackId; //m_Track
        public byte TypeId; //m_Format, 0=Vector3, 1=Quaternion, 2=Float
        public Rsc6BoneIdEnum ID; //m_Id

        public uint Packed
        {
            readonly get => (ushort)ID + ((uint)TypeId << 16) + ((uint)TrackId << 24);
            set
            {
                ID = (Rsc6BoneIdEnum)(value & 0xFFFF);
                TypeId = (byte)((value >> 16) & 0xFF);
                TrackId = (Rsc6TrackID)((value >> 24) & 0xFF);
            }
        }

        public Rsc6AnimBoneId(Rsc6BoneIdEnum boneId, byte typeId, Rsc6TrackID trackId)
        {
            ID = boneId;
            TypeId = typeId;
            TrackId = trackId;
        }

        public Rsc6AnimBoneId(uint packed)
        {
            Packed = packed;
        }

        public readonly Rsc6TrackPackFormat GetPack() //Returns information about storage packing used
        {
            return (Rsc6TrackPackFormat)(TypeId & (byte)Rsc6TrackPackFormat.FORMAT_PACK_MASK);
        }

        public override readonly string ToString()
        {
            return $"{ID} : {TypeId} : {TrackId}";
        }
    }

    public enum Rsc6ClipType : byte
    {
        NONE,
        SINGLE,
        MULTI,
        EXPRESSION
    };

    public enum Rsc6ClipPropertyAttributeType : uint
    {
        NONE = 0,
        FLOAT = 1,
        INT = 2,
        BOOL = 3,
        STRING = 4, //atString
        BIT_SET = 5, //atBitSet
        VECTOR3 = 6, //Vec3V
        VECTOR4 = 7, //Vec4V
        QUATERNION = 8, //QuatV
        MATRIX3X4 = 9, //Mat34V
        SITUATION = 10, //TransformV
        DATA = 11, //atArray<u8>
        HASH_STRING = 12 //atHashString
    }

    public enum Rsc6AnimChannelType : byte
    {
        NONE = 0,
        RAW_FLOAT = 1,
        VECTOR3 = 2,
        QUATERNION = 3,
        STATIC_FLOAT = 4,
        CURVE_FLOAT = 5,
        QUANTIZE_FLOAT = 6,
        RAW_INT = 7,
        RAW_BOOL = 8,
        STATIC_QUATERNION = 9,
        DELAT_FLOAT = 10,
        STATIC_INT = 11,
        RLE_INT = 12,
        STATIC_VECTOR3 = 13,
        SMALLEST_THREE_QUATERNION = 14,
        VARIABLE_QUANTIZE_FLOAT = 15,
        INDIRECT_QUANTIZE_FLOAT = 16,
        LINEAR_FLOAT = 17,
        QUADRATIC_BSPLINE = 18,
        CUBIC_BSPLINE = 19,
        STATIC_SMALLEST_THREE_QUATERNION = 20
    }

    public enum Rsc6TrackType : byte
    {
        VECTOR3 = 0,
        QUATERNION = 1,
        FLOAT = 2
    }

    public enum Rsc6TrackPackFormat : byte
    {
        FORMAT_PACK_RAW = 0,
        FORMAT_PACK_XYZW = 4,
        FORMAT_PACK_QUATERNION_XYZ_RECONSTRUCT = 8,
        FORMAT_PACK_MASK = 12
    }

    public enum Rsc6FormatReconstructOrder :uint
    {
        FORMAT_RECONSTRUCT_X = 0,
        FORMAT_RECONSTRUCT_Y = 1,
        FORMAT_RECONSTRUCT_Z = 2,
        FORMAT_RECONSTRUCT_W = 3,
        FORMAT_RECONSTRUCT_NONE
    }

    public enum Rsc6AnimationFlags
    {
        LOOPED = 1 << 0,
        RAW = 1 << 3,
        MOVER_TRACKS = 1 << 4,
        PACKED = 1 << 8,
        COMPACT = 1 << 10,
        NON_SERIALIZABLE_MASK = PACKED | COMPACT
    }

    public enum Rsc6TrackID : byte
    {
        BONE_TRANSLATION = 0,
        BONE_ROTATION,
        BONE_SCALE,
        BONE_CONSTRAINT,
        VISIBILITY,
        MOVER_TRANSLATION,
        MOVER_ROTATION,
        CAMERA_TRANSLATION,
        CAMERA_ROTATION,
        CAMERA_SCALE,
        CAMERA_FOCAL_LENGTH,
        CAMERA_HORIZONTAL_FILM_APERTURE,
        CAMERA_APERTURE,
        CAMERA_FOCAL_POINT,
        CAMERA_F_STOP,
        CAMERA_FOCUS_DISTANCE,
        SHADER_FRAME_INDEX,
        SHADER_SLIDE_U,
        SHADER_SLIDE_V,
        SHADER_ROTATE_UV,
        MOVER_SCALE,
        BLEND_SHAPE,
        VISEMES,
        ANIMATED_NORMAL_MAPS,
        FACIAL_CONTROL,
        FACIAL_TRANSLATION,
        FACIAL_ROTATION,
        CAMERA_FIELD_OF_VIEW,
        CAMERA_DEPTH_OF_FIELD,
        COLOR,
        LIGHT_INTENSITY,
        LIGHT_FALL_OFF,
        LIGHT_CONE_ANGLE,
        GENERIC_CONTROL,
        GENERIC_TRANSLATION,
        GENERIC_ROTATION,
        CAMERA_DEPTH_OF_FIELD_STRENGTH,
        FACIAL_SCALE,
        GENERIC_SCALE,
        CAMERA_SHALLOW_DEPTH_OF_FIELD,
        CAMERA_MOTION_BLUR,
        PARTICLE_DATA,
        LIGHT_DIRECTION,
        CAMERA_DEPTH_OF_FIELD_NEAR_OUT_OF_FOCUS_PLANE,
        CAMERA_DEPTH_OF_FIELD_NEAR_IN_FOCUS_PLANE,
        CAMERA_DEPTH_OF_FIELD_FAR_OUT_OF_FOCUS_PLANE,
        CAMERA_DEPTH_OF_FIELD_FAR_IN_FOCUS_PLANE,
        LIGHT_EXP_FALL_OFF,
        CAMERA_SIMPLE_DEPTH_OF_FIELD,
        CAMERA_COC,
        FACIAL_TINTING,
        CAMERA_FOCUS,
        CAMERA_NIGHT_COC,
        CAMERA_LIMIT
    }
}