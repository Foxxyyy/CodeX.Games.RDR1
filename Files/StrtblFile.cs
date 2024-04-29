using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Collections;

namespace CodeX.Games.RDR1.Files
{
    [TC(typeof(EXP))]
    public partial class StrtblFile : DataBagPack
    {
        public Rpf6FileEntry Entry { get; set; }
        public DataSchema Schema { get; set; }
        public Rsc6StringTableLanguage[] Languages { get; set; }

        public int NumLanguages { get; set; } //numLanguages
        public uint Version { get; set; } //version, between 256 and 4096
        public int NumIdentifiers { get; set; } //m_NumIdentifiers
        public string[] Identifiers { get; set; } //m_Identifiers, this is what the game code refers to when displaying the actual string

        public StrtblFile(Rpf6FileEntry entry)
        {
            Entry = entry;
        }

        public StrtblFile(string xml) : base(null)
        {
            Bag = DataBag2.FromXml(xml);
            if (Bag?.Objects != null)
            {
                RegisterProperties();
            }
        }

        public override void Load(byte[] data)
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
                Identifiers[i] = Identifiers[i].Replace("&", "&amp;");
            }

            //Language strings
            for (int i = 0; i < Languages.Length; i++)
            {
                var language = GetLanguageFromIndex(i);
                r.Position = Languages[i].Position;
                Languages[i].Language = language;
                Languages[i].NumString = r.ReadUInt32();
                Languages[i].Hash = r.ReadUInt32();
                Languages[i].Flags = r.ReadInt16();
                Languages[i].FontLength = r.ReadInt32();
                Languages[i].StringData = new Rsc6StringTableData[Languages[i].NumString];

                for (int index = 0; index < Languages[i].NumString; index++)
                {
                    var length = r.ReadInt32();
                    var strData = r.ReadBytes(length * 2);
                    var value = Encoding.Unicode.GetString(strData);

                    value = StringRegex().Replace(value, "");
                    value = GetFixedUIString(value);
                    value = value.Replace("<0x", "(start-color)0x", StringComparison.OrdinalIgnoreCase);
                    value = value.Replace("</0x>", "(end-color)", StringComparison.OrdinalIgnoreCase);
                    value = value.Replace("&", "&amp;", StringComparison.OrdinalIgnoreCase);

                    if (value.All(c => c == ' ')) //Insignificant xml spaces
                    {
                        var spaceCount = value.Count(c => c == ' ');
                        value = string.Empty;
                        value = string.Concat(Enumerable.Repeat("(whitespace)", spaceCount));
                    }

                    Languages[i].StringData[index] = new Rsc6StringTableData()
                    {
                        Length = length,
                        ValueData = strData,
                        Value = value,
                        Scale = r.ReadVector2(),
                        OffsetX = r.ReadByte(),
                        OffsetY = r.ReadByte(),
                    };

                    //Don't include this part if we're on the last string of this language
                    if (index == Languages[i].NumString - 1) continue;
                    Languages[i].StringData[index].Character = r.ReadUInt32();
                    Languages[i].StringData[index].X = r.ReadByte();
                    Languages[i].StringData[index].Y = r.ReadByte();
                    Languages[i].StringData[index].W = r.ReadByte();
                    Languages[i].StringData[index].H = r.ReadByte();
                    Languages[i].StringData[index].Baseline = r.ReadByte();
                    Languages[i].StringData[index].AddWidth = r.ReadByte();
                }
            }
            Bag = CreateDataBag();
        }

        public override byte[] Save()
        {
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
                ident = ident.Replace("&amp;", "&");

                var identData = Encoding.UTF8.GetBytes(ident + "\0");
                identData = identData.Select(b => b == 0x3F ? (byte)0xA0 : b).ToArray();
                writer.Write(ident.Length);
                writer.Write(identData);
            }

            for (int i = 0; i < Languages.Length; i++)
            {
                if (i == 10) continue; //Spanish-spain == Spanish-mexico
                var language = Languages[i];
                positions[i] = (uint)writer.Position;
                writer.Write(language.NumString);
                writer.Write(language.Hash);
                writer.Write(language.Flags);
                writer.Write(language.FontLength);

                for (int index = 0; index < language.NumString; index++)
                {
                    var strData = language.StringData[index];
                    writer.Write(strData.Length);
                    writer.Write(strData.ValueData);
                    writer.Write(strData.Scale);
                    writer.Write(strData.OffsetX);
                    writer.Write(strData.OffsetY);

                    if (index == language.NumString - 1) continue;
                    writer.Write(strData.Character);
                    writer.Write(strData.X);
                    writer.Write(strData.Y);
                    writer.Write(strData.W);
                    writer.Write(strData.H);
                    writer.Write(strData.Baseline);
                    writer.Write(strData.AddWidth);
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

        public DataBag2 CreateDataBag()
        {
            Schema = new DataSchema();
            var bag = CreateDataBag(Languages, Identifiers);
            return bag;
        }

        public DataBag2 CreateDataBag(Rsc6StringTableLanguage[] languages, string[] identifiers)
        {
            if (languages == null || identifiers == null) return null;
            var cls = CreateSchemaClass(languages);
            if (cls == null)  return null;

            var bag = new DataBag2(cls);
            JenkIndex.Ensure("Identifiers", "RDR1");
            var idHash = JenkHash.GenHash("Identifiers");
            bag.SetObject(idHash, identifiers);

            foreach (var item in languages)
            {
                JenkIndex.Ensure(item.Language, "RDR1");
                var f = JenkHash.GenHash(item.Language);
                var fld = cls.GetField(f);
                if (fld == null) continue;

                var alen = item?.Dict.Count ?? 0;
                if (alen > 0)
                {
                    var bags = new DataBag2[alen];
                    var lkKeys = item.Dict.Keys.ToArray();
                    var lkValues = item.Dict.Values.ToArray();
                    var hashData = new byte[lkKeys.Length * 4];

                    for (int i = 0; i < lkKeys.Length; i++)
                    {
                        var temp = BitConverter.GetBytes(lkKeys[i]);
                        Array.Copy(temp, 0, hashData, i * 4, 4);
                    }

                    for (int i = 0; i < alen; i++)
                    {
                        var lkHash = lkKeys[i];
                        var lkObj = lkValues[i];
                        var lkCls = CreateLookupClass(lkHash);

                        bags[i] = new DataBag2(lkCls, hashData, i * 4);
                        bags[i].SetObject(0x6098A50E, lkHash); //"Key"
                        bags[i].SetObject(0x63FA3F2, lkObj); //"Item"
                    }
                    bag.SetObject(f, bags);
                }
            }
            return bag;
        }

        private DataSchemaClass CreateSchemaClass(Rsc6StringTableLanguage[] languages)
        {
            if (languages == null) return null;
            var cls = new DataSchemaClass()
            {
                Name = JenkHash.GenHash(Entry.Name)
            };

            var allfields = new List<DataSchemaField>();
            var idField = new DataSchemaField()
            {
                Name = JenkHash.GenHash("Identifiers"),
                DataType = DataBagValueType.Array,
                ArrayType = DataBagValueType.String,
                ArraySize = 1,
                IsAttribute = false
            };
            allfields.Add(idField);

            foreach (var item in languages)
            {
                if (item ==  null) continue;
                var fld = new DataSchemaField()
                {
                    Name = JenkHash.GenHash(item.Language),
                    DataType = DataBagValueType.Lookup,
                    IsAttribute = false
                };
                allfields.Add(fld);
            }

            cls.DataSize = 0;
            cls.Fields = allfields.ToArray();
            cls.BuildFieldsLookup(Schema);
            return cls;
        }

        private DataSchemaClass CreateLookupClass(JenkHash hash)
        {
            var cls = new DataSchemaClass()
            {
                Name = hash
            };

            var fields = new DataSchemaField[2];
            var f1 = new DataSchemaField
            {
                Name = 0x6098A50E, //"Key"
                DataType = DataBagValueType.Hash,
                IsAttribute = false
            };
            var f2 = new DataSchemaField
            {
                Name = 0x63FA3F2, //"Item"
                DataType = DataBagValueType.String,
                IsAttribute = false
            };

            fields[0] = f1;
            fields[1] = f2;

            cls.DataSize = 0;
            cls.Fields = fields;
            cls.BuildFieldsLookup(Schema);
            return cls;
        }

        private void RegisterProperties()
        {
            var objects = Bag.Objects.Values;
            var dict = new Dictionary<JenkHash, string>[objects.Count - 1];
            string[] identifiers = null;
            int index = 0;

            var tet = 0;
            foreach (var obj in objects)
            {
                if (obj is string[] arr)
                {
                    identifiers = arr;
                }
                else
                {
                    dict[index] = new Dictionary<JenkHash, string>();
                    if (obj is DataBag2[] bags)
                    {
                        foreach (var bag in bags)
                        {
                            if (bag.Objects == null) continue;
                            var values = bag.Objects.Values.ToArray();
                            var hash = (string)values[1];
                            var item = values.Length < 3 ? "" : (string)values[2];
                            if (hash.StartsWith("0x"))
                            { }
                            dict[index].Add(new JenkHash(hash), item);
                            tet++;
                        }
                    }
                    index++;
                }
            }

            if (dict.Length > 0)
            {
                NumLanguages = objects.Count - 1;
                Languages = new Rsc6StringTableLanguage[NumLanguages];
                Version = 512u;

                Identifiers = identifiers;
                NumIdentifiers = Identifiers.Length;

                for (int i = 0; i < NumLanguages; i++)
                {
                    var language = GetLanguageFromIndex(i);
                    var strLanguage = dict[i];
                    var keysLanguage = strLanguage.Keys.ToArray();
                    var valuesLanguage = strLanguage.Values.ToArray();

                    Languages[i] = new Rsc6StringTableLanguage()
                    {
                        Language = language,
                        NumString = (uint)strLanguage.Count,
                        Flags = 1,
                        FontLength = 0,
                        StringData = new Rsc6StringTableData[strLanguage.Count],
                    };

                    for (int j = 0; j < Languages[i].NumString; j++)
                    {
                        var id = keysLanguage[j];
                        var str = valuesLanguage[j];

                        if (str.Contains("(whitespace)")) //Insignificant xml spaces
                        {
                            var spaces = (str.Length - str.Replace("(whitespace)", "").Length) / "(whitespace)".Length;
                            str = string.Concat(Enumerable.Repeat(" ", spaces));
                        }

                        str = GetFixedUIString(str, true);
                        str = str.Replace("(start-color)0x", "<0x", StringComparison.OrdinalIgnoreCase);
                        str = str.Replace("(end-color)", "</0x>", StringComparison.OrdinalIgnoreCase);
                        str = str.Replace("&amp;", "&", StringComparison.OrdinalIgnoreCase);
                        str = InsertNullCharacters(str);

                        var strLength = str.Length;
                        var valueData = Encoding.Unicode.GetBytes(str);
                        Languages[i].StringData[j] = new Rsc6StringTableData()
                        {
                            Length = strLength,
                            ValueData = valueData,
                            Value = str,
                            Scale = new Vector2(1.0f, 1.0f),
                            OffsetX = 0,
                            OffsetY = 0,
                            Character = id,
                            X = 1,
                            Y = 0,
                            W = 0,
                            H = 0,
                            Baseline = 0,
                            AddWidth = 0
                        };
                    }
                }
            }
        }

        private string InsertNullCharacters(string str)
        {
            var nullChar = '\x00';
            var newStr = new StringBuilder();

            if (str == string.Empty)
            {
                return nullChar.ToString();
            }

            newStr.Append(str);
            if (str[^1] != nullChar)
            {
                newStr.Append(nullChar);
            }
            return newStr.ToString();
        }

        //When importing xml, some symbols/tags will just throw a error due to 'XmlDocument', this is used to 'fix' some specific tags
        private string GetFixedUIString(string str, bool fromXml = false)
        {
            foreach (var tag in UITags)
            {
                var tr = tag[1..^1];
                if (!str.Contains(tr, StringComparison.OrdinalIgnoreCase)) continue;

                string replacement, actual;
                if (fromXml)
                {
                    replacement = "<" + tr + ">";
                    actual = "~" + tr + "~";
                }
                else
                {
                    replacement = "~" + tr + "~";
                    actual = "<" + tr + ">";
                }
                str = str.Replace(actual, replacement, StringComparison.OrdinalIgnoreCase);
            }
            return str;
        }

        public static string GetLanguageFromIndex(int index)
        {
            string name = string.Empty;
            switch (index)
            {
                case 0: name = "English"; break;
                case 1: name = "Spanish"; break;
                case 2: name = "French"; break;
                case 3: name = "German"; break;
                case 4: name = "Italian"; break;
                case 5: name = "Japanese"; break;
                case 6: name = "Chinese-traditional"; break;
                case 7: name = "Chinese-simplified"; break;
                case 8: name = "Korean"; break;
                case 9: name = "Spanish-spain"; break;
                case 10: name = "Spanish-mexico"; break;
                case 11: name = "Portuguese"; break;
                case 12: name = "Polish"; break;
                case 13: name = "Russian"; break;
            }
            return name;
        }

        public string[] UITags = new string[]
        {
            //Inputs
            "<lb>",
            "<rb>",
            "<ls>",
            "<rs>",
            "<lt>",
            "<rt>",
            "<x>",
            "<y>",
            "<Start>",
            "<BACK>",
            "<action>",
            "<cancel>",
            "<dpadup>",
            "<dpaddown>",
            "<dpadleft>",
            "<dpadright>",
            "<lclick>",
            "<rclick>",

            //Colors
            "<red>",
            "<blue>",
            "<purple>",
            "<yellow>",
            "<orange>",
            "<green>",
            "<grey>",
            "<D>",
            "</red>",
            "</blue>",
            "</purple>",
            "</yellow>",
            "</orange>",
            "</green>",
            "</grey>",
            "</D>",

            //Blips
            "<GASP>",
            "<CROAK>",
            "<COMP_DAUGHTER>",
            "<COMP_FAMOUS_GUN>",
            "<COMP_FBI>",
            "<COMP_GRAVEROBBER>",
            "<COMP_MARSHAL>",
            "<COMP_MEX_GIRL>",
            "<COMP_MEX_HENCHMAN>",
            "<COMP_NATIVE>",
            "<COMP_OUTLAW>",
            "<COMP_RANCHER>",
            "<COMP_REBEL>",
            "<COMP_SNAKEOILMERCHANT>",
            "<RANCHHAND>",
            "<SON>",
            "<WIFE>",
            "<DUTCH >",
            "<BILL>",
            "<TYRANT>",
            "<JAVIER>",
            "<CONTACT>",
            "<CONTACT_GREEN>",
            "<CONTACT_RED>",
            "<ENEMY>",
            "<ENEMY_DOWN>",
            "<ENEMY_UP>",
            "<FRIEND>",
            "<HOME>",
            "<HORSE>",
            "<HORSE_BLUE>",
            "<MONEY>",
            "<OBJECTIVE>",
            "<OBJECTIVE_DOWN>",
            "<OBJECTIVE_UP>",
            "<PLAYER>",
            "<PLAYER_HORSE>",
            "<PICKUP>",
            "<BLACKSMITH>",
            "<COACH_DRIVER>",
            "<HEAR_NOISE>",
            "<TRAIN>",
            "<MOST_WANTED>",
            "<NIGHTWATCH>",
            "<COP_DOWN>",
            "<COP_UP>",
            "<EYEWITNESS>",
            "<SHERIFF>",
            "<TELEGRAPH>",
            "<BEER>",
            "<BULLRIDING>",
            "<CARDS>",
            "<HORSESHOE>",
            "<ARM_WRESTLING>",
            "<DUEL>",
            "<FIVE_FINGER>",
            "<COACH_TAXI_STOP>",
            "<CHECKPOINT_GENERIC>",
            "<MP_COLOR_1>",
            "<MP_COLOR_2>",
            "<MP_COLOR_3>",
            "<MP_COLOR_4>",
            "<MP_COLOR_5>",
            "<MP_COLOR_6>",
            "<MP_COLOR_7>",
            "<MP_COLOR_8>",
            "<MP_COLOR_9>",
            "<MP_COLOR_10>",
            "<MP_COLOR_11>",
            "<MP_COLOR_12>",
            "<MP_COLOR_13>",
            "<MP_COLOR_14>",
            "<MP_COLOR_15>",
            "<MP_COLOR_16>",
            "<MP_RED_BAG>",
            "<MP_BLUE_BAG>",
            "<MP_NEUTRAL_BAG>",
            "<MP_RED_BASE>",
            "<MP_BLUE_BASE>",
            "<MP_NEUTRAL_BASE>",
            "<WEAPON_CACHE>",
            "<MP_PICKUP_WEAPON>",
            "<MP_PICKUP_AMMO>",
            "<MP_PICKUP_ITEM>",
            "<POSSE_LEADER>",
            "<ACTIVE_ACTION_AREA>",
            "<SET_MAP_DESTINATION>",
            "<SCRAP>",
            "<AMBIENT_ATTACK>",
            "<ATTACK_COACH >",
            "<DEFEND_COACH>",
            "<FIRE>",
            "<RED_SKULL>",
            "<SKULL>",
            "<TRANSPORT_BROWN>",
            "<TRANSPORT_RED>",
            "<TRANSPORT_WHITE>",
            "<SHOP>",
            "<DOCTOR>",
            "<GUNSMITH>",
            "<TRAIN_TICKETS>",
            "<5>",
            "<BOMB>",
            "<CHECKER>",
            "<DYNAMITE>",
            "<FIREBOTTLE>",
            "<HEIST_END>",
            "<HEIST_START>",
            "<HOSTAGE>",
            "<MEDICINAL_PLANT>",
            "<MISC_INJURED_PERSON>",
            "<QUESTION_MARK>",
            "<RACE_CHECKPOINT>",
            "<RACE_FINISH>",
            "<RACE_OPPONENT>",
            "<STAR_RED>",
            "<TARGET>",
            "<WHITE>",
            "<WILD_HORSE>",
            "<RED_CIRCLE>",
            "<HOUSE_BUY>",
            "<HOUSE_RENT>",
            "<HERD>",
            "<HERD_MAIN>",
            "<HERD_STRAGGLER>",
            "<HERD_RETURN>",
            "<LIARSDICE>",
            "<TRAIN_RED>",
            "<TRAIN_BLUE>",
            "<FRIEND_DOWN>",
            "<FRIEND_UP>",
            "<PICKUP_DOWN>",
            "<PICKUP_UP>",
            "<POKER>",
            "<MP_COLOR_RED_TEAM>",
            "<MP_COLOR_BLUE_TEAM>",
            "<MP_COLOR_RED_TEAM_UP>",
            "<MP_COLOR_RED_TEAM_DOWN>",
            "<MP_COLOR_BLUE_TEAM_UP>",
            "<MP_COLOR_BLUE_TEAM_DOWN>",
            "<SWAG>",
            "<SWAG_DEF>",
            "<SWAG_RET>",
            "<SWAG_CAP>",
            "<SWAG_TAKE>",
            "<SWAG_Y>",
            "<SWAG_Y_DEF>",
            "<SWAG_Y_RET>",
            "<SWAG_Y_CAP>",
            "<SWAG_Y_TAKE>",
            "<SWAG_R>",
            "<SWAG_R_DEF>",
            "<SWAG_R_RET>",
            "<SWAG_R_CAP>",
            "<SWAG_R_TAKE>",
            "<SWAG_B>",
            "<SWAG_B_DEF>",
            "<SWAG_B_RET>",
            "<SWAG_B_CAP>",
            "<SWAG_B_TAKE>",
            "<CHEST>",
            "<CHEST_YELLOW>",
            "<CHEST_RED>",
            "<CHEST_BLUE>",
            "<MP_ACHIEVE>",
            "<MP_FINISHED_RACE>",
            "<MP_KO>",
            "<MPK_VIP>",
            "<MPK_SPREE>",
            "<MPK_DEFEND>",
            "<MPK_HRS>",
            "<MPK_TEAMMATE>",
            "<MPK_MULTIPLY>",
            "<MPK_KNIFE>",
            "<MPK_ANIMAL>",
            "<MPK_HEAD>",
            "<MPK_SNIPER>",
            "<MPK_SPREE_END>",
            "<MPK_NPC>",
            "<MP_WIN_GAMETYPE>",
            "<MPS_BURN>",
            "<MPS_DROWN>",
            "<MPS_FALL>",
            "<MPS_SPLODE>",
            "<MPS_CACTUS>",
            "<MP_TEAM>",
            "<MP_FFA>",
            "<MP_TDM>",
            "<MP_TCTF>",
            "<MP_DM>",
            "<MP_CTF>",
            "<MP_COOP>",
            "<TAILOR>",
            "<CANNON>",
            "<GATLING>",
            "<MOVIEHOUSE>",
            "<MP_CLASSIC_CTF>",
            "<NEWSPAPER>",
            "<MP_REZ>",
            "<MP_REZ_UP>",
            "<MP_REZ_DOWN>",
            "<XP>",
            "<MP_XP>",
            "<SPACE>",
            "<STAR_GOLD>",
            "<STAR_SILVER>",
            "<STAR_BRONZE>",
            "<MP_COOP_ADV>",
            "<MP_FFA_HC>",
            "<MP_FFA_V>",
            "<MP_FIND_FREE>",
            "<MP_QUICK>",
            "<MP_TEAM_HC>",
            "<MP_TEAM_V>",
            "<MPK_REVENGE>",
            "<MP_TEAM_C>",
            "<MP_FFA_C>",
            "<MP_SCORE>",
            "<MP_KILLS>",
            "<MP_REVIVES>",
            "<MP_INJURED>",
            "<MP_DEATH>",
            "<MP_DEFEND>",
            "<BAGCAPS>",
            "<MPK_ASSIST>",
            "<MPK_REVENGE>",
            "<MP_KILLSTREAK>",
            "<MPK_PWHIP>",
            "<VGO>",
            "<VSKIP>",
            "<VRE>",
            "<ROCKSTAR>",
            "<MP_LOCKED>",
            "<VNO>",
            "<MPS_BLED>",
            "<MP_LIVES>",
            "<ENVELOPE>",

            //Rockstar oversights
            "</WAGON>"
        };

        [GeneratedRegex("\\x00")] private static partial Regex StringRegex();
    }

    [TC(typeof(EXP))] public class Rsc6StringTableLanguage
    {
        public uint Position { get; set; } //temp
        public string Language { get; set; } //temp
        public JenkHash Hash { get; set; } //m_Hash
        public uint NumString { get; set; } //num_Strings
        public short Flags { get; set; } //m_Flags, 1 if the string should be added to the stringtable
        public int FontLength { get; set; } //fontLength
        public Rsc6StringTableData[] StringData { get; set; } //stringData

        public string[] Strings
        {
            get
            {
                if (StringData == null) return null;
                return StringData.Select(s => s.Value).ToArray();
            }
        }

        public Dictionary<JenkHash, string> Dict
        {
            get
            {
                if (StringData == null) return null;
                return StringData.ToDictionary(s => s.Character, s => s.Value);
            }
        }

        public override string ToString()
        {
            return Language;
        }
    }

    [TC(typeof(EXP))] public class Rsc6StringTableData
    {
        public int Length { get; set; } //stringLength
        public string Value { get; set; } //stringLength
        public Vector2 Scale { get; set; } //m_Scale
        public byte OffsetX { get; set; } //m_OffsetX
        public byte OffsetY { get; set; } //m_OffsetY
        public JenkHash Character { get; set; } //m_Character, used to link the string to its identifier
        public byte X { get; set; } //m_X
        public byte Y { get; set; } //m_Y
        public byte W { get; set; } //m_W
        public byte H { get; set; } //m_H
        public byte Baseline { get; set; } //m_Baseline
        public byte AddWidth { get; set; } //m_AddWidth

        public byte[] ValueData; //For writing purpose

        public override string ToString()
        {
            return Character.ToString();
        }
    }
}