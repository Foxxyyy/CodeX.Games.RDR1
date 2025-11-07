using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.Files
{
    [TC(typeof(EXP))] public partial class StrtblFile
    {
        public int NumLanguages { get; set; } //numLanguages
        public uint Version { get; set; } //version, between 256 and 4096
        public int NumIdentifiers { get; set; } //m_NumIdentifiers
        public string[] Identifiers { get; set; } //m_Identifiers, this is what the game code refers to when displaying the actual string
        public Rsc6StringTableLanguage[] Languages { get; set; }

        public void Load(byte[] data)
        {
            using var ms = new MemoryStream(data);
            var r = new DataReader(ms);

            //Language count & blocks positions
            NumLanguages = r.ReadInt32();
            Languages = new Rsc6StringTableLanguage[NumLanguages];

            for (int i = 0; i < NumLanguages; i++)
            {
                Languages[i] = new Rsc6StringTableLanguage
                {
                    Position = r.ReadUInt32()
                };
            }

            //Identifiers
            Version = r.ReadUInt32();
            NumIdentifiers = r.ReadInt32();
            Identifiers = new string[NumIdentifiers];

            for (int i = 0; i < NumIdentifiers; i++)
            {
                var length = r.ReadUInt32();
                Identifiers[i] = r.ReadString(true, Encoding.ASCII);
            }

            //Language strings
            for (int i = 0; i < Languages.Length; i++)
            {
                var language = GetLanguageFromIndex(i);
                r.Position = Languages[i].Position;
                if (r.Position == r.Length) continue;

                Languages[i].Language = language;
                Languages[i].NumString = r.ReadUInt32();
                Languages[i].StringData = new Rsc6StringTableData[Languages[i].NumString];
                Languages[i].Values = new Rsc6StringTableLanguage.HashValue[Languages[i].NumString];

                for (int index = 0; index < Languages[i].NumString; index++)
                {
                    Languages[i].StringData[index] = new Rsc6StringTableData
                    {
                        FontTex = new Rsc6FontTexGlyph()
                        {
                            Character = r.ReadUInt32(),
                            X = r.ReadByte(),
                            Y = r.ReadByte(),
                            W = r.ReadByte(),
                            H = r.ReadByte(),
                            Baseline = r.ReadByte(),
                            AddWidth = r.ReadByte()
                        }
                    };

                    var length = r.ReadInt32();
                    var strData = r.ReadBytes(length * 2);
                    var value = Encoding.Unicode.GetString(strData);

                    Languages[i].StringData[index].Length = length;
                    Languages[i].StringData[index].ValueData = strData;
                    Languages[i].StringData[index].Value = value;
                    Languages[i].StringData[index].Scale = r.ReadVector2();
                    Languages[i].StringData[index].OffsetX = r.ReadByte();
                    Languages[i].StringData[index].OffsetY = r.ReadByte();

                    Languages[i].Values[index] = new Rsc6StringTableLanguage.HashValue()
                    {
                        Hash = Languages[i].StringData[index].FontTex.Character,
                        Value = value
                    };
                }
            }
        }

        public byte[] Save()
        {
            if (NumLanguages == 0) return null;

            var ms = new MemoryStream();
            this.Save(ms);

            var buf = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buf, 0, buf.Length);

            return buf;
        }

        public void Save(Stream stream)
        {
            var writer = new DataWriter(stream);
            var positions = new uint[NumLanguages];

            writer.Write(NumLanguages);
            for (int i = 0; i < NumLanguages; i++)
            {
                writer.Write(0); //Will be updated later
            }

            writer.Write(Version);
            writer.Write(NumIdentifiers);

            for (int i = 0; i < NumIdentifiers; i++)
            {
                var ident = Identifiers[i];
                var identData = Encoding.UTF8.GetBytes(ident + "\0");

                identData = [.. identData.Select(b => b == 0x3F ? (byte)0xA0 : b)];
                writer.Write(ident.Length);
                writer.Write(identData);
            }

            for (int i = 0; i < Languages.Length; i++)
            {
                if (i == 10) continue; //Spanish-spain == Spanish-mexico
                var language = Languages[i];
                positions[i] = (uint)writer.Position;

                if (language.NumString == 0) continue;
                writer.Write(language.NumString);

                for (int index = 0; index < language.NumString; index++)
                {
                    var strData = language.StringData[index];
                    writer.Write(strData.FontTex.Character);
                    writer.Write(strData.FontTex.X);
                    writer.Write(strData.FontTex.Y);
                    writer.Write(strData.FontTex.W);
                    writer.Write(strData.FontTex.H);
                    writer.Write(strData.FontTex.Baseline);
                    writer.Write(strData.FontTex.AddWidth);
                    writer.Write(strData.Length);
                    writer.Write(strData.ValueData);
                    writer.Write(strData.Scale);
                    writer.Write(strData.OffsetX);
                    writer.Write(strData.OffsetY);
                }
            }

            //Now, update the blocks position
            writer.Position = 0x4;
            for (int i = 0; i < NumLanguages; i++)
            {
                if (i == 10) //Spanish-spain == Spanish-mexico
                {
                    writer.Write(positions[i - 1]);
                    continue;
                }
                writer.Write(positions[i]);
            }
        }

        public string ToReadableText()
        {
            if (Languages == null || Languages.Length == 0 || Identifiers == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            for (int langIndex = 0; langIndex < Languages.Length; langIndex++)
            {
                var lang = Languages[langIndex];
                if (lang == null) continue;

                sb.AppendLine($"# Language: {lang.Language}");
                sb.AppendLine();

                var dict = new Dictionary<JenkHash, string>();
                if (lang.Values != null)
                {
                    foreach (var v in lang.Values)
                    {
                        dict[v.Hash] = v.Value?.TrimEnd('\0') ?? string.Empty;
                    }
                }

                for (int i = 0; i < NumIdentifiers; i++)
                {
                    var key = Identifiers[i];
                    var hash = JenkHash.GenHash(key.ToLowerInvariant());
                    var value = dict.TryGetValue(hash, out var v) ? v : string.Empty;
                    sb.AppendLine($"\"{key}\": \"{value}\"");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void FromReadableText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            //Parse languages and entries
            var sections = new List<(string Language, List<(string Key, string Value)> Entries)>();
            string lang = null;
            var entries = new List<(string, string)>();

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("# Language:", StringComparison.OrdinalIgnoreCase))
                {
                    if (lang != null)
                    {
                        sections.Add((lang, new List<(string, string)>(entries)));
                    }

                    lang = line[11..].Trim();
                    entries.Clear();
                    continue;
                }

                //"key": "value"
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
                entries.Add((key, value));
            }

            if (lang != null && entries.Count > 0)
            {
                sections.Add((lang, entries));
            }
            this.BuildStringTable(sections);
        }

        private void BuildStringTable(List<(string Language, List<(string Key, string Value)> Entries)> sections)
        {
            if (sections == null || sections.Count == 0) return;

            //Collect all unique identifiers from every language
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (_, entries) in sections)
            {
                foreach (var (key, _) in entries)
                {
                    ids.Add(key);
                }
            }

            Identifiers = [.. ids];
            NumIdentifiers = Identifiers.Length;

            //Build languages
            Languages = new Rsc6StringTableLanguage[sections.Count];
            NumLanguages = Languages.Length;

            for (int li = 0; li < sections.Count; li++)
            {
                var (langName, entries) = sections[li];
                var lang = new Rsc6StringTableLanguage
                {
                    Language = langName,
                    NumString = (uint)entries.Count,
                    Values = new Rsc6StringTableLanguage.HashValue[entries.Count],
                    StringData = new Rsc6StringTableData[entries.Count]
                };

                //Create a lookup for the entries of this language
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (key, value) in entries)
                {
                    dict[key] = value;
                }

                for (int i = 0; i < Identifiers.Length; i++)
                {
                    var key = Identifiers[i];
                    var value = dict.TryGetValue(key, out var v) ? v : string.Empty;
                    var fullValue = value + "\0";
                    var hash = JenkHash.GenHash(key.ToLowerInvariant());

                    lang.Values[i] = new Rsc6StringTableLanguage.HashValue
                    {
                        Hash = hash,
                        Value = value
                    };

                    lang.StringData[i] = new Rsc6StringTableData
                    {
                        Length = fullValue.Length,
                        Value = fullValue,
                        ValueData = Encoding.Unicode.GetBytes(fullValue),
                        Scale = new Vector2(1.0f, 1.0f),
                        OffsetX = 0,
                        OffsetY = 0,
                        FontTex = new Rsc6FontTexGlyph
                        {
                            Character = hash,
                            X = 1,
                            Y = 0,
                            W = 0,
                            H = 0,
                            Baseline = 0,
                            AddWidth = 0
                        }
                    };
                }
                Languages[li] = lang;
            }
            Version = 256;
        }

        public static string GetLanguageFromIndex(int index)
        {
            /*
             * Flash languages abbreviations:
             * brplu = br + pl + ru
             * efigs = en + fr + it + de + es
            */

            string name = string.Empty;
            switch (index)
            {
                case 0: name = "English"; break; //en
                case 1: name = "Spanish"; break; //es
                case 2: name = "French"; break; //fr
                case 3: name = "German"; break; //de
                case 4: name = "Italian"; break; //it
                case 5: name = "Japanese"; break; //jp
                case 6: name = "Chinese-traditional"; break; //cht
                case 7: name = "Chinese-simplified"; break; //chs
                case 8: name = "Korean"; break; //ko
                case 9: name = "Spanish-spain"; break;
                case 10: name = "Spanish-mexico"; break; //mx
                case 11: name = "Portuguese"; break; //bp
                case 12: name = "Polish"; break; //pl
                case 13: name = "Russian"; break; //ru
            }
            return name;
        }
    }

    [TC(typeof(EXP))] public class Rsc6StringTableLanguage
    {
        public uint Position { get; set; } //temp
        public string Language { get; set; } //temp
        public uint NumString { get; set; } //num_Strings
        public HashValue[] Values { get; set; } //stringData
        public Rsc6StringTableData[] StringData { get; set; } //stringData

        public override string ToString()
        {
            return Language;
        }

        public class HashValue
        {
            public JenkHash Hash;
            public string Value;  
        }
    }

    [TC(typeof(EXP))] public class Rsc6StringTableData
    {
        public int Length { get; set; } //stringLength
        public string Value { get; set; } //m_String, text that is displayed by the game
        public Vector2 Scale { get; set; } //m_Scale, adjusts the text size
        public byte OffsetX { get; set; } //m_OffsetX, adjust the X location of the text
        public byte OffsetY { get; set; } //m_OffsetY, adjust the Y location of the text
        public Rsc6FontTexGlyph FontTex { get; set; }

        public byte[] ValueData; //For writing purpose

        public override string ToString()
        {
            return Value;
        }
    }

    [TC(typeof(EXP))] public class Rsc6FontTexGlyph : MetaNode
    {
        public JenkHash Character { get; set; } //m_Character, used to link strings to identifiers, or serves as glyphs indices for fonts
        public byte X { get; set; } //m_X, horizontal position
        public byte Y { get; set; } //m_Y, vertical position
        public byte W { get; set; } //m_W, width of the glyph
        public byte H { get; set; } //m_H, height of the glyph
        public byte Baseline { get; set; } //m_Baseline, for determining the vertical positioning of glyph within text
        public byte AddWidth { get; set; } //m_AddWidth, additional width

        public Vector4 Position
        {
            get => new(X, Y, W, H);
            set
            {
                X = (byte)value.X;
                Y = (byte)value.Y;
                W = (byte)value.Z;
                H = (byte)value.W;
            }
        }

        public void Write(DataWriter writer)
        {
            writer.Write(Character);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(W);
            writer.Write(H);
            writer.Write(Baseline);
            writer.Write(AddWidth);
        }

        public void Read(MetaNodeReader reader)
        {
            Character = reader.ReadUInt32("Character");
            Position = reader.ReadVector4("Position");
            Baseline = reader.ReadByte("Baseline");
            AddWidth = reader.ReadByte("AddWidth");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Character", Character);
            writer.WriteVector4("Position", Position);
            writer.WriteByte("Baseline", Baseline);
            writer.WriteByte("AddWidth", AddWidth);
        }

        public override string ToString()
        {
            return $"ID: {Character.Dec}, Position: {Position}";
        }
    }
}