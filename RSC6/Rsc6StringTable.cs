using CodeX.Core.Utilities;
using System.Collections.Generic;
using System.Linq;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))]
    public class Rsc6StringTable : Rsc6BlockBaseMap //rage::txtStringTable
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
    }

    [TC(typeof(EXP))]
    public class Rsc6TextHashTable : Rsc6BlockBase //rage::txtHashTable
    {
        public override ulong BlockLength => 16;
        public int NumSlots { get; set; } //mNumSlots, number of max slots in the table, always 101
        public Rsc6PtrArr<Rsc6TextHashEntry> Slots { get; set; } //mSlots
        public int NumEntries { get; set; } //mNumEntries, total number of entries used by all slots

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

        public void BuildSlots(IEnumerable<Rsc6TextHashEntry> entries)
        {
            var list = entries?.ToList();
            if (list?.Count > 0)
            {
                var newlst = Rsc6DataMap.Build(list, 101, false, false, Slots.Items);
                Slots = new([.. newlst]);
                NumSlots = Slots.Count;
                NumEntries = list.Count;
            }
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TextHashEntry : Rsc6BlockBase, IRsc6DataMapEntry<Rsc6TextHashEntry> //rage::txtHashEntry
    {
        public override ulong BlockLength => 12;
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

        public override string ToString()
        {
            return Hash.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6TextStringData : Rsc6BlockBase //rage::txtStringData
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