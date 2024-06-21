using System.Linq;
using System.Collections.Generic;
using CodeX.Core.Utilities;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6StringTable : Rsc6BlockBaseMap, MetaNode //rage::txtStringTable
    {
        public override ulong BlockLength => 20;
        public override uint VFT { get; set; } = 0x00EC9BC8;
        public Rsc6Ptr<Rsc6TextHashTable> HashTable { get; set; } //m_HashTable
        public int NumIdentifiers { get; set; } //m_NumIdentifiers, always 0
        public uint Unknown_10h { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            HashTable = reader.ReadPtr<Rsc6TextHashTable>();
            NumIdentifiers = reader.ReadInt32();
            Unknown_10h = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(HashTable);
            writer.WriteInt32(NumIdentifiers);
            writer.WriteUInt32(Unknown_10h);
        }

        public void Read(MetaNodeReader reader)
        {
            HashTable = new(reader.ReadNode<Rsc6TextHashTable>("HashTable"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("HashTable", HashTable.Item);
        }
    }

    [TC(typeof(EXP))] public class Rsc6TextHashTable : Rsc6BlockBase, MetaNode //rage::txtHashTable
    {
        public override ulong BlockLength => 16;
        public int NumSlots { get; set; } //mNumSlots
        public Rsc6PtrArr<Rsc6TextHashEntry> Slots { get; set; } //mSlots
        public int NumEntries { get; set; } //mNumEntries

        public override void Read(Rsc6DataReader reader)
        {
            NumSlots = reader.ReadInt32();
            Slots = reader.ReadPtrArr<Rsc6TextHashEntry>();
            NumEntries = reader.ReadInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteInt32(NumSlots);
            writer.WritePtrArr(Slots);
            writer.WriteInt32(NumEntries);
        }

        public void Read(MetaNodeReader reader)
        {
            BuildSlots(reader.ReadNodeArray<Rsc6TextHashEntry>("Slots"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("NumSlots", NumSlots);
            writer.WriteNodeArray("Slots", Rsc6DataMap.Flatten(Slots.Items, e => e).ToArray());
        }

        public void BuildSlots(IEnumerable<Rsc6TextHashEntry> entries)
        {
            var list = entries?.ToList();
            if (list?.Count > 0)
            {
                var newlst = Rsc6DataMap.Build(list, false, false, Slots.Items);
                Slots = new(newlst.ToArray());
                NumEntries = list.Count;
            }
        }
    }

    [TC(typeof(EXP))] public class Rsc6TextHashEntry : Rsc6BlockBase, IRsc6DataMapEntry<Rsc6TextHashEntry>, MetaNode //rage::txtHashEntry
    {
        public override ulong BlockLength => 16;
        public JenkHash Hash { get; set; } //mHash, equal to mData.Hash
        public Rsc6Ptr<Rsc6TextStringData> Data { get; set; } //mData
        public Rsc6Ptr<Rsc6TextHashEntry> Next { get; set; } //mNext

        public Rsc6TextHashEntry MapNext { get => Next.Item; set => Next = new(value); }
        public uint MapKey { get => Hash; }

        public override void Read(Rsc6DataReader reader)
        {
            Hash = reader.ReadUInt32();
            Data = reader.ReadPtr<Rsc6TextStringData>();
            Next = reader.ReadPtr<Rsc6TextHashEntry>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Hash);
            writer.WritePtr(Data);
            writer.WritePtr(Next);
        }

        public void Read(MetaNodeReader reader)
        {
            Hash = reader.ReadJenkHash("@hash");
            var item = new Rsc6TextStringData
            {
                Hash = this.Hash,
                String = new(reader.ReadString("Text"))
            };
            Data = new(item);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteJenkHash("@hash", Hash);
            writer.WriteString("Text", Xml.Escape(Data.Item?.String.Value?.Trim('\x00') ?? ""));
        }

        public override string ToString()
        {
            return Hash.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6TextStringData : Rsc6BlockBase //rage::txtStringData
    {
        public override ulong BlockLength => 16;
        public JenkHash Hash { get; set; } //m_Hash
        public Rsc6Str String { get; set; } //m_StringUTF8

        public override void Read(Rsc6DataReader reader)
        {
            Hash = reader.ReadUInt32();
            String = reader.ReadStr();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Hash);
            writer.WriteStr(String);
        }
    }
}