using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.Files
{
    public class AudioDatFile : FilePack
    {
        public byte[] RawFileData { get; set; }
        public DatFileType RelType { get; set; }
        public uint DataLength { get; set; }
        public byte[] DataBlock { get; set; }
        public uint DataUnkVal { get; set; }
        public int NameTableLength { get; set; }
        public int NameTableCount { get; set; }
        public byte[] NameTableBytes { get; set; }
        public int IndexCount { get; set; }
        public uint IndexStringFlags { get; set; }
        public int HashTableCount { get; set; }
        public int PackTableCount { get; set; }

        public DatIndexString[] IndexStrings { get; set; }
        public uint[] HashTableOffsets { get; set; }
        public JenkHash[] HashTable { get; set; }
        public uint[] PackTableOffsets { get; set; }
        public JenkHash[] PackTable { get; set; }

        public override void Load(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            var sb = new StringBuilder();

            RawFileData = data;
            RelType = (DatFileType)br.ReadUInt32();
            DataLength = br.ReadUInt32();
            DataBlock = br.ReadBytes((int)DataLength);

            //Name table
            NameTableLength = br.ReadInt32();
            NameTableCount = br.ReadInt32();
            if (NameTableCount > 0)
            {
                NameTableBytes = br.ReadBytes(NameTableLength);
            }

            //Index hashes
            IndexCount = br.ReadInt32();
            if (IndexCount > 0)
            {
                var indexstrs = new DatIndexString[IndexCount];
                IndexStringFlags = br.ReadUInt32();

                for (uint i = 0; i < IndexCount; i++)
                {
                    var sl = br.ReadByte();
                    sb.Clear();

                    for (int j = 0; j < sl; j++)
                    {
                        char c = (char)br.ReadByte();
                        if (c != 0) sb.Append(c);
                    }

                    var ristr = new DatIndexString
                    {
                        Name = sb.ToString(),
                        Offset = br.ReadUInt32(),
                        Length = br.ReadUInt32()
                    };
                    indexstrs[i] = ristr;
                }
                IndexStrings = indexstrs;
            }

            //Hash table
            HashTableCount = br.ReadInt32();
            if (HashTableCount != 0)
            {
                var htoffsets = new uint[HashTableCount];
                var hthashes = new JenkHash[HashTableCount];

                for (uint i = 0; i < HashTableCount; i++)
                {
                    htoffsets[i] = br.ReadUInt32();
                    var pos = ms.Position;
                    ms.Position = htoffsets[i];
                    hthashes[i] = new JenkHash(br.ReadUInt32());
                    ms.Position = pos;
                }
                HashTableOffsets = htoffsets;
                HashTable = hthashes;
            }

            //Pack table
            PackTableCount = br.ReadInt32();
            if (PackTableCount != 0)
            {
                var ptoffsets = new uint[PackTableCount];
                var pthashes = new JenkHash[PackTableCount];

                for (uint i = 0; i < PackTableCount; i++)
                {
                    ptoffsets[i] = br.ReadUInt32();

                    var pos = ms.Position;
                    ms.Position = ptoffsets[i];
                    pthashes[i] = new JenkHash(br.ReadUInt32());
                    ms.Position = pos;
                }
                PackTableOffsets = ptoffsets;
                PackTable = pthashes;
            }
            ParseDataBlock();
        }

        public override byte[] Save()
        {
            throw new NotImplementedException();
        }

        public override void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(MetaNodeWriter writer)
        {
            throw new NotImplementedException();
        }

        private void ParseDataBlock()
        {
            var ms = new MemoryStream(DataBlock);
            var br = new BinaryReader(ms);

            DataUnkVal = br.ReadUInt32();

            var reldatas = new List<AudioData>();
            if (IndexStrings != null)
            {
                foreach (var indexstr in IndexStrings)
                {
                    reldatas.Add(ReadRelData(br, indexstr));
                }
            }
        }

        private AudioData ReadRelData(BinaryReader br, DatIndexString s)
        {
            return ReadRelData(br, s.Name, JenkHash.GenHash(s.Name.ToLowerInvariant()), s.Offset, s.Length);
        }

        private AudioData ReadRelData(BinaryReader br, string name, JenkHash hash, uint offset, uint length)
        {
            br.BaseStream.Position = offset;
            byte[] data = br.ReadBytes((int)length);


            var d = new AudioData(this)
            {
                Name = name,
                NameHash = hash,
                DataOffset = offset,
                DataLength = length,
                Data = data
            };

            using var dbr = new BinaryReader(new MemoryStream(data));
            d.ReadType(dbr);
            return d;
        }
    }

    [TC(typeof(EXP))]
    public class AudioData
    {
        public JenkHash NameHash { get; set; }
        public string Name { get; set; }
        public uint DataOffset { get; set; }
        public uint DataLength { get; set; }
        public byte[] Data { get; set; }
        public byte TypeID { get; set; }

        public AudioDatFile Rel { get; set; }

        public AudioData(AudioDatFile rel)
        {
            Rel = rel;
        }

        public AudioData(AudioData d)
        {
            NameHash = d.NameHash;
            Name = d.Name;
            DataOffset = d.DataOffset;
            DataLength = d.DataLength;
            Data = d.Data;
            TypeID = d.TypeID;
            Rel = d.Rel;
        }

        public void ReadType(BinaryReader br)
        {
            TypeID = br.ReadByte();
        }

        public virtual uint[] GetHashTableOffsets()
        {
            return null;
        }

        public virtual uint[] GetPackTableOffsets()
        {
            return null;
        }

        public string GetNameString()
        {
            return (string.IsNullOrEmpty(Name)) ? NameHash.ToString() : Name;
        }

        public string GetBaseString()
        {
            return DataOffset.ToString() + ", " + DataLength.ToString() + ": " + GetNameString();
        }

        public override string ToString()
        {
            return GetBaseString() + ": " + TypeID.ToString();
        }

        public static bool Bit(uint f, int b)
        {
            return (f & (1u << b)) != 0;
        }
        public static bool BadF(float f)
        {
            return (f < -15000) || (f > 15000);
        }
    }

    [TC(typeof(EXP))]
    public struct DatIndexHash
    {
        public JenkHash Name { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }

        public override string ToString()
        {
            return Name.ToString() + ", " + Offset.ToString() + ", " + Length.ToString();
        }
    }

    [TC(typeof(EXP))]
    public struct DatIndexString
    {
        public string Name { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }

        public override string ToString()
        {
            return Name + ", " + Offset.ToString() + ", " + Length.ToString();
        }
    }

    public enum DatFileType : uint
    {
        Dat45 = 45 //game.dat is little endian, game.dat45 is big-endian
    }
}