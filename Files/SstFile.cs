using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeX.Games.RDR1.Files
{
    public class SstFile : FilePack
    {
        public Rpf6FileEntry FileEntry;
        public Rsc6StringTable StringTable;

        public SstFile()
        {
        }

        public SstFile(GameArchiveFileInfo file)
        {
            FileInfo = file;
        }

        public SstFile(Rpf6FileEntry e)
        {
            FileEntry = e;
        }

        public SstFile(Rsc6StringTable stringTable)
        {
            StringTable = stringTable;
        }

        public override void Load(byte[] data)
        {
            if (FileInfo is not Rpf6ResourceFileEntry e)
                return;

            var r = new Rsc6DataReader(e, data)
            {
                Position = (ulong)e.FlagInfos.RSC85_ObjectStart + Rsc6DataReader.VIRTUAL_BASE
            };
            StringTable = r.ReadBlock<Rsc6StringTable>();
        }

        public override byte[] Save()
        {
            if (StringTable == null) return null;
            var w = new Rsc6DataWriter();
            w.WriteBlock(StringTable);
            var data = w.Build(1);
            return data;
        }

        public override void Read(MetaNodeReader reader)
        {
            throw new Exception("No XML support for .wst files");
        }

        public override void Write(MetaNodeWriter writer)
        {
            throw new Exception("No XML support for .wst files");
        }

        public string ToReadableText()
        {
            if (StringTable?.HashTable.Item?.Slots.Items == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var entries = Rsc6DataMap.Flatten(StringTable.HashTable.Item.Slots.Items, e => e).Where(e => e?.Data.Item != null).ToList();
            
            foreach (var entry in entries)
            {
                var hash = entry.Hash;
                var key = JenkIndex.TryGetString(hash);
                var value = entry.Data.Item.String.Value ?? "";
                sb.AppendLine($"\"{key}\": \"{value}\"");
            }
            return sb.ToString();
        }

        public void FromReadableText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            var entries = new List<Rsc6TextHashEntry>();

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (!line.StartsWith('"')) continue;

                var keyEnd = line.IndexOf('"', 1);
                if (keyEnd < 0) continue;

                var colonIdx = line.IndexOf(':', keyEnd);
                if (colonIdx < 0) continue;

                var valueStart = line.IndexOf('"', colonIdx);
                var valueEnd = line.LastIndexOf('"');
                if (valueStart < 0 || valueEnd <= valueStart) continue;

                var key = line[1..keyEnd];
                var value = line.Substring(valueStart + 1, valueEnd - valueStart - 1);

                var hash = JenkHash.GenHash(key.ToLowerInvariant());
                var strData = new Rsc6TextStringData
                {
                    Hash = hash,
                    String = new(value + "\0")
                };

                var entry = new Rsc6TextHashEntry
                {
                    Hash = hash,
                    Data = new(strData)
                };
                entries.Add(entry);
            }

            var hashTable = new Rsc6TextHashTable();
            hashTable.BuildSlots(entries);

            StringTable = new Rsc6StringTable
            {
                HashTable = new(hashTable),
                NumIdentifiers = 0,
                Unknown_10h = 0
            };
        }

        public override string ToString()
        {
            return StringTable.ToString();
        }
    }
}