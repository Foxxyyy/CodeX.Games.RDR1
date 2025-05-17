using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Numerics;
using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Text.RegularExpressions;

namespace CodeX.Games.RDR1.Files
{
    [TC(typeof(EXP))] public partial class StrtblFile : FilePack, MetaNode
    {
        public int NumLanguages { get; set; } //numLanguages
        public uint Version { get; set; } //version, between 256 and 4096
        public int NumIdentifiers { get; set; } //m_NumIdentifiers
        public string[] Identifiers { get; set; } //m_Identifiers, this is what the game code refers to when displaying the actual string
        public Rsc6StringTableLanguage[] Languages { get; set; }

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

        public override byte[] Save()
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

        public override void Read(MetaNodeReader reader)
        {
            Version = reader.ReadUInt32("Version");
            Identifiers = reader.ReadStringArray("Identifiers");
            Languages = reader.ReadNodeArray<Rsc6StringTableLanguage>("Languages");
            NumIdentifiers = Identifiers.Length;
            NumLanguages = Languages.Length;
        }

        public override void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Version", Version);
            writer.WriteStringArray("Identifiers", Identifiers);
            writer.WriteNodeArray("Languages", Languages);
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

    [TC(typeof(EXP))] public class Rsc6StringTableLanguage : MetaNode
    {
        public uint Position { get; set; } //temp
        public string Language { get; set; } //temp
        public uint NumString { get; set; } //num_Strings
        public HashValue[] Values { get; set; } //stringData
        public Rsc6StringTableData[] StringData { get; set; } //stringData

        public void Read(MetaNodeReader reader)
        {
            Language = reader.ReadString("@id");
            NumString = reader.ReadUInt32("@numStrings");
            Values = reader.ReadNodeArray<HashValue>("Dictionary");

            StringData = new Rsc6StringTableData[NumString];
            for (int i = 0; i < NumString; i++)
            {
                var hash = Values[i].Hash;
                var str = Values[i].Value;

                StringData[i] = new Rsc6StringTableData
                {
                    Length = str.Length,
                    Value = str,
                    ValueData = Encoding.Unicode.GetBytes(str),
                    Scale = new Vector2(1.0f, 1.0f),
                    OffsetX = 0,
                    OffsetY = 0,
                    FontTex = new Rsc6FontTexGlyph()
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
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@id", Language);
            writer.WriteString("@numStrings", NumString.ToString());
            writer.WriteNodeArray("Dictionary", Values);
        }

        public override string ToString()
        {
            return Language;
        }


        public class HashValue : MetaNode
        {
            public JenkHash Hash;
            public string Value;

            public void Read(MetaNodeReader reader)
            {
                Hash = reader.ReadJenkHash("@hash");
                Value = reader.ReadString("Value");

                if (Value.Contains("~whitespace~")) //A single space is treated as insignificant and interpreted as an empty string by XML parsers
                {
                    Value = Value.Replace("~whitespace~", " ");
                }

                string pattern1 = @"~(lb|rb|ls|rs|lt|rt|x|y|Start|BACK|action|cancel|dpadup|dpaddown|dpadleft|dpadright|lclick|rclick|red|blue|purple|yellow|orange|green|grey|D|GASP|CROAK|COMP_DAUGHTER|COMP_FAMOUS_GUN|COMP_FBI|COMP_GRAVEROBBER|COMP_MARSHAL|COMP_MEX_GIRL|COMP_MEX_HENCHMAN|COMP_NATIVE|COMP_OUTLAW|COMP_RANCHER|COMP_REBEL|COMP_SNAKEOILMERCHANT|RANCHHAND|SON|WIFE|DUTCH|BILL|TYRANT|JAVIER|CONTACT|CONTACT_GREEN|CONTACT_RED|ENEMY|ENEMY_DOWN|ENEMY_UP|FRIEND|HOME|HORSE|HORSE_BLUE|MONEY|OBJECTIVE|OBJECTIVE_DOWN|OBJECTIVE_UP|PLAYER|PLAYER_HORSE|PICKUP|BLACKSMITH|COACH_DRIVER|HEAR_NOISE|TRAIN|MOST_WANTED|NIGHTWATCH|COP_DOWN|COP_UP|EYEWITNESS|SHERIFF|TELEGRAPH|BEER|BULLRIDING|CARDS|HORSESHOE|ARM_WRESTLING|DUEL|FIVE_FINGER|COACH_TAXI_STOP|CHECKPOINT_GENERIC|MP_COLOR_1|MP_COLOR_2|MP_COLOR_3|MP_COLOR_4|MP_COLOR_5|MP_COLOR_6|MP_COLOR_7|MP_COLOR_8|MP_COLOR_9|MP_COLOR_10|MP_COLOR_11|MP_COLOR_12|MP_COLOR_13|MP_COLOR_14|MP_COLOR_15|MP_COLOR_16|MP_RED_BAG|MP_BLUE_BAG|MP_NEUTRAL_BAG|MP_RED_BASE|MP_BLUE_BASE|MP_NEUTRAL_BASE|WEAPON_CACHE|MP_PICKUP_WEAPON|MP_PICKUP_AMMO|MP_PICKUP_ITEM|POSSE_LEADER|ACTIVE_ACTION_AREA|SET_MAP_DESTINATION|SCRAP|AMBIENT_ATTACK|ATTACK_COACH|DEFEND_COACH|FIRE|RED_SKULL|SKULL|TRANSPORT_BROWN|TRANSPORT_RED|TRANSPORT_WHITE|SHOP|DOCTOR|GUNSMITH|TRAIN_TICKETS|5|BOMB|CHECKER|DYNAMITE|FIREBOTTLE|HEIST_END|HEIST_START|HOSTAGE|MEDICINAL_PLANT|MISC_INJURED_PERSON|QUESTION_MARK|RACE_CHECKPOINT|RACE_FINISH|RACE_OPPONENT|STAR_RED|TARGET|WHITE|WILD_HORSE|RED_CIRCLE|HOUSE_BUY|HOUSE_RENT|HERD|HERD_MAIN|HERD_STRAGGLER|HERD_RETURN|LIARSDICE|TRAIN_RED|TRAIN_BLUE|FRIEND_DOWN|FRIEND_UP|PICKUP_DOWN|PICKUP_UP|POKER|MP_COLOR_RED_TEAM|MP_COLOR_BLUE_TEAM|MP_COLOR_RED_TEAM_UP|MP_COLOR_RED_TEAM_DOWN|MP_COLOR_BLUE_TEAM_UP|MP_COLOR_BLUE_TEAM_DOWN|SWAG|SWAG_DEF|SWAG_RET|SWAG_CAP|SWAG_TAKE|SWAG_Y|SWAG_Y_DEF|SWAG_Y_RET|SWAG_Y_CAP|SWAG_Y_TAKE|SWAG_R|SWAG_R_DEF|SWAG_R_RET|SWAG_R_CAP|SWAG_R_TAKE|SWAG_B|SWAG_B_DEF|SWAG_B_RET|SWAG_B_CAP|SWAG_B_TAKE|CHEST|CHEST_YELLOW|CHEST_RED|CHEST_BLUE|MP_ACHIEVE|MP_FINISHED_RACE|MP_KO|MPK_VIP|MPK_SPREE|MPK_DEFEND|MPK_HRS|MPK_TEAMMATE|MPK_MULTIPLY|MPK_KNIFE|MPK_ANIMAL|MPK_HEAD|MPK_SNIPER|MPK_SPREE_END|MPK_NPC|MP_WIN_GAMETYPE|MPS_BURN|MPS_DROWN|MPS_FALL|MPS_SPLODE|MPS_CACTUS|MP_TEAM|MP_FFA|MP_TDM|MP_TCTF|MP_DM|MP_CTF|MP_COOP|TAILOR|CANNON|GATLING|MOVIEHOUSE|MP_CLASSIC_CTF|NEWSPAPER|MP_REZ|MP_REZ_UP|MP_REZ_DOWN|XP|MP_XP|SPACE|STAR_GOLD|STAR_SILVER|STAR_BRONZE|MP_COOP_ADV|MP_FFA_HC|MP_FFA_V|MP_FIND_FREE|MP_QUICK|MP_TEAM_HC|MP_TEAM_V|MPK_REVENGE|MP_TEAM_C|MP_FFA_C|MP_SCORE|MP_KILLS|MP_REVIVES|MP_INJURED|MP_DEATH|MP_DEFEND|BAGCAPS|MPK_ASSIST|MPK_REVENGE|MP_KILLSTREAK|MPK_PWHIP|VGO|VSKIP|VRE|ROCKSTAR|MP_LOCKED|VNO|MPS_BLED|MP_LIVES|ENVELOPE|FS1|FS2|FS3|FS4|FS5|FS6|FS7|FS8|FS9|NL|DICE1|DICE2|DICE3|DICE4|DICE5|DICE6|DICE7|BANK|BANDITS|TOWN_SAFE|CONVOY|WAGON)~";
                string pattern2 = @"~/(lb|rb|ls|rs|lt|rt|x|y|Start|BACK|action|cancel|dpadup|dpaddown|dpadleft|dpadright|lclick|rclick|red|blue|purple|yellow|orange|green|grey|D|GASP|CROAK|COMP_DAUGHTER|COMP_FAMOUS_GUN|COMP_FBI|COMP_GRAVEROBBER|COMP_MARSHAL|COMP_MEX_GIRL|COMP_MEX_HENCHMAN|COMP_NATIVE|COMP_OUTLAW|COMP_RANCHER|COMP_REBEL|COMP_SNAKEOILMERCHANT|RANCHHAND|SON|WIFE|DUTCH|BILL|TYRANT|JAVIER|CONTACT|CONTACT_GREEN|CONTACT_RED|ENEMY|ENEMY_DOWN|ENEMY_UP|FRIEND|HOME|HORSE|HORSE_BLUE|MONEY|OBJECTIVE|OBJECTIVE_DOWN|OBJECTIVE_UP|PLAYER|PLAYER_HORSE|PICKUP|BLACKSMITH|COACH_DRIVER|HEAR_NOISE|TRAIN|MOST_WANTED|NIGHTWATCH|COP_DOWN|COP_UP|EYEWITNESS|SHERIFF|TELEGRAPH|BEER|BULLRIDING|CARDS|HORSESHOE|ARM_WRESTLING|DUEL|FIVE_FINGER|COACH_TAXI_STOP|CHECKPOINT_GENERIC|MP_COLOR_1|MP_COLOR_2|MP_COLOR_3|MP_COLOR_4|MP_COLOR_5|MP_COLOR_6|MP_COLOR_7|MP_COLOR_8|MP_COLOR_9|MP_COLOR_10|MP_COLOR_11|MP_COLOR_12|MP_COLOR_13|MP_COLOR_14|MP_COLOR_15|MP_COLOR_16|MP_RED_BAG|MP_BLUE_BAG|MP_NEUTRAL_BAG|MP_RED_BASE|MP_BLUE_BASE|MP_NEUTRAL_BASE|WEAPON_CACHE|MP_PICKUP_WEAPON|MP_PICKUP_AMMO|MP_PICKUP_ITEM|POSSE_LEADER|ACTIVE_ACTION_AREA|SET_MAP_DESTINATION|SCRAP|AMBIENT_ATTACK|ATTACK_COACH|DEFEND_COACH|FIRE|RED_SKULL|SKULL|TRANSPORT_BROWN|TRANSPORT_RED|TRANSPORT_WHITE|SHOP|DOCTOR|GUNSMITH|TRAIN_TICKETS|5|BOMB|CHECKER|DYNAMITE|FIREBOTTLE|HEIST_END|HEIST_START|HOSTAGE|MEDICINAL_PLANT|MISC_INJURED_PERSON|QUESTION_MARK|RACE_CHECKPOINT|RACE_FINISH|RACE_OPPONENT|STAR_RED|TARGET|WHITE|WILD_HORSE|RED_CIRCLE|HOUSE_BUY|HOUSE_RENT|HERD|HERD_MAIN|HERD_STRAGGLER|HERD_RETURN|LIARSDICE|TRAIN_RED|TRAIN_BLUE|FRIEND_DOWN|FRIEND_UP|PICKUP_DOWN|PICKUP_UP|POKER|MP_COLOR_RED_TEAM|MP_COLOR_BLUE_TEAM|MP_COLOR_RED_TEAM_UP|MP_COLOR_RED_TEAM_DOWN|MP_COLOR_BLUE_TEAM_UP|MP_COLOR_BLUE_TEAM_DOWN|SWAG|SWAG_DEF|SWAG_RET|SWAG_CAP|SWAG_TAKE|SWAG_Y|SWAG_Y_DEF|SWAG_Y_RET|SWAG_Y_CAP|SWAG_Y_TAKE|SWAG_R|SWAG_R_DEF|SWAG_R_RET|SWAG_R_CAP|SWAG_R_TAKE|SWAG_B|SWAG_B_DEF|SWAG_B_RET|SWAG_B_CAP|SWAG_B_TAKE|CHEST|CHEST_YELLOW|CHEST_RED|CHEST_BLUE|MP_ACHIEVE|MP_FINISHED_RACE|MP_KO|MPK_VIP|MPK_SPREE|MPK_DEFEND|MPK_HRS|MPK_TEAMMATE|MPK_MULTIPLY|MPK_KNIFE|MPK_ANIMAL|MPK_HEAD|MPK_SNIPER|MPK_SPREE_END|MPK_NPC|MP_WIN_GAMETYPE|MPS_BURN|MPS_DROWN|MPS_FALL|MPS_SPLODE|MPS_CACTUS|MP_TEAM|MP_FFA|MP_TDM|MP_TCTF|MP_DM|MP_CTF|MP_COOP|TAILOR|CANNON|GATLING|MOVIEHOUSE|MP_CLASSIC_CTF|NEWSPAPER|MP_REZ|MP_REZ_UP|MP_REZ_DOWN|XP|MP_XP|SPACE|STAR_GOLD|STAR_SILVER|STAR_BRONZE|MP_COOP_ADV|MP_FFA_HC|MP_FFA_V|MP_FIND_FREE|MP_QUICK|MP_TEAM_HC|MP_TEAM_V|MPK_REVENGE|MP_TEAM_C|MP_FFA_C|MP_SCORE|MP_KILLS|MP_REVIVES|MP_INJURED|MP_DEATH|MP_DEFEND|BAGCAPS|MPK_ASSIST|MPK_REVENGE|MP_KILLSTREAK|MPK_PWHIP|VGO|VSKIP|VRE|ROCKSTAR|MP_LOCKED|VNO|MPS_BLED|MP_LIVES|ENVELOPE|FS1|FS2|FS3|FS4|FS5|FS6|FS7|FS8|FS9|NL|DICE1|DICE2|DICE3|DICE4|DICE5|DICE6|DICE7|BANK|BANDITS|TOWN_SAFE|CONVOY|WAGON)~";

                Value = Regex.Replace(Value, pattern2, match =>
                {
                    string tagName = match.Groups[1].Value;
                    return $"</{tagName}>";
                }, RegexOptions.IgnoreCase);

                Value = Regex.Replace(Value, pattern1, match =>
                {
                    string tagName = match.Groups[1].Value;
                    return $"<{tagName}>";
                }, RegexOptions.IgnoreCase);

                string hexPattern = @"~0x([0-9a-fA-F]+)~(.*?)";
                Value = Regex.Replace(Value, hexPattern, match =>
                {
                    string hex = match.Groups[1].Value;
                    string content = match.Groups[2].Value;
                    return $"<0x{hex}>{content}";
                });

                Value += "\0";
                Value = Value.Replace("&amp;", "&");
                Value = Value.Replace("&quot;", "\"");
                Value = Value.Replace("&apos;", "'");
                Value = Value.Replace("~/0x~", "</0x>");
            }

            public void Write(MetaNodeWriter writer)
            {
                string pattern1 = @"<(lb|rb|ls|rs|lt|rt|x|y|Start|BACK|action|cancel|dpadup|dpaddown|dpadleft|dpadright|lclick|rclick|red|blue|purple|yellow|orange|green|grey|D|GASP|CROAK|COMP_DAUGHTER|COMP_FAMOUS_GUN|COMP_FBI|COMP_GRAVEROBBER|COMP_MARSHAL|COMP_MEX_GIRL|COMP_MEX_HENCHMAN|COMP_NATIVE|COMP_OUTLAW|COMP_RANCHER|COMP_REBEL|COMP_SNAKEOILMERCHANT|RANCHHAND|SON|WIFE|DUTCH|BILL|TYRANT|JAVIER|CONTACT|CONTACT_GREEN|CONTACT_RED|ENEMY|ENEMY_DOWN|ENEMY_UP|FRIEND|HOME|HORSE|HORSE_BLUE|MONEY|OBJECTIVE|OBJECTIVE_DOWN|OBJECTIVE_UP|PLAYER|PLAYER_HORSE|PICKUP|BLACKSMITH|COACH_DRIVER|HEAR_NOISE|TRAIN|MOST_WANTED|NIGHTWATCH|COP_DOWN|COP_UP|EYEWITNESS|SHERIFF|TELEGRAPH|BEER|BULLRIDING|CARDS|HORSESHOE|ARM_WRESTLING|DUEL|FIVE_FINGER|COACH_TAXI_STOP|CHECKPOINT_GENERIC|MP_COLOR_1|MP_COLOR_2|MP_COLOR_3|MP_COLOR_4|MP_COLOR_5|MP_COLOR_6|MP_COLOR_7|MP_COLOR_8|MP_COLOR_9|MP_COLOR_10|MP_COLOR_11|MP_COLOR_12|MP_COLOR_13|MP_COLOR_14|MP_COLOR_15|MP_COLOR_16|MP_RED_BAG|MP_BLUE_BAG|MP_NEUTRAL_BAG|MP_RED_BASE|MP_BLUE_BASE|MP_NEUTRAL_BASE|WEAPON_CACHE|MP_PICKUP_WEAPON|MP_PICKUP_AMMO|MP_PICKUP_ITEM|POSSE_LEADER|ACTIVE_ACTION_AREA|SET_MAP_DESTINATION|SCRAP|AMBIENT_ATTACK|ATTACK_COACH|DEFEND_COACH|FIRE|RED_SKULL|SKULL|TRANSPORT_BROWN|TRANSPORT_RED|TRANSPORT_WHITE|SHOP|DOCTOR|GUNSMITH|TRAIN_TICKETS|5|BOMB|CHECKER|DYNAMITE|FIREBOTTLE|HEIST_END|HEIST_START|HOSTAGE|MEDICINAL_PLANT|MISC_INJURED_PERSON|QUESTION_MARK|RACE_CHECKPOINT|RACE_FINISH|RACE_OPPONENT|STAR_RED|TARGET|WHITE|WILD_HORSE|RED_CIRCLE|HOUSE_BUY|HOUSE_RENT|HERD|HERD_MAIN|HERD_STRAGGLER|HERD_RETURN|LIARSDICE|TRAIN_RED|TRAIN_BLUE|FRIEND_DOWN|FRIEND_UP|PICKUP_DOWN|PICKUP_UP|POKER|MP_COLOR_RED_TEAM|MP_COLOR_BLUE_TEAM|MP_COLOR_RED_TEAM_UP|MP_COLOR_RED_TEAM_DOWN|MP_COLOR_BLUE_TEAM_UP|MP_COLOR_BLUE_TEAM_DOWN|SWAG|SWAG_DEF|SWAG_RET|SWAG_CAP|SWAG_TAKE|SWAG_Y|SWAG_Y_DEF|SWAG_Y_RET|SWAG_Y_CAP|SWAG_Y_TAKE|SWAG_R|SWAG_R_DEF|SWAG_R_RET|SWAG_R_CAP|SWAG_R_TAKE|SWAG_B|SWAG_B_DEF|SWAG_B_RET|SWAG_B_CAP|SWAG_B_TAKE|CHEST|CHEST_YELLOW|CHEST_RED|CHEST_BLUE|MP_ACHIEVE|MP_FINISHED_RACE|MP_KO|MPK_VIP|MPK_SPREE|MPK_DEFEND|MPK_HRS|MPK_TEAMMATE|MPK_MULTIPLY|MPK_KNIFE|MPK_ANIMAL|MPK_HEAD|MPK_SNIPER|MPK_SPREE_END|MPK_NPC|MP_WIN_GAMETYPE|MPS_BURN|MPS_DROWN|MPS_FALL|MPS_SPLODE|MPS_CACTUS|MP_TEAM|MP_FFA|MP_TDM|MP_TCTF|MP_DM|MP_CTF|MP_COOP|TAILOR|CANNON|GATLING|MOVIEHOUSE|MP_CLASSIC_CTF|NEWSPAPER|MP_REZ|MP_REZ_UP|MP_REZ_DOWN|XP|MP_XP|SPACE|STAR_GOLD|STAR_SILVER|STAR_BRONZE|MP_COOP_ADV|MP_FFA_HC|MP_FFA_V|MP_FIND_FREE|MP_QUICK|MP_TEAM_HC|MP_TEAM_V|MPK_REVENGE|MP_TEAM_C|MP_FFA_C|MP_SCORE|MP_KILLS|MP_REVIVES|MP_INJURED|MP_DEATH|MP_DEFEND|BAGCAPS|MPK_ASSIST|MPK_REVENGE|MP_KILLSTREAK|MPK_PWHIP|VGO|VSKIP|VRE|ROCKSTAR|MP_LOCKED|VNO|MPS_BLED|MP_LIVES|ENVELOPE|FS1|FS2|FS3|FS4|FS5|FS6|FS7|FS8|FS9|NL|DICE1|DICE2|DICE3|DICE4|DICE5|DICE6|DICE7|BANK|BANDITS|TOWN_SAFE|CONVOY|WAGON)>";
                string pattern2 = @"</(lb|rb|ls|rs|lt|rt|x|y|Start|BACK|action|cancel|dpadup|dpaddown|dpadleft|dpadright|lclick|rclick|red|blue|purple|yellow|orange|green|grey|D|GASP|CROAK|COMP_DAUGHTER|COMP_FAMOUS_GUN|COMP_FBI|COMP_GRAVEROBBER|COMP_MARSHAL|COMP_MEX_GIRL|COMP_MEX_HENCHMAN|COMP_NATIVE|COMP_OUTLAW|COMP_RANCHER|COMP_REBEL|COMP_SNAKEOILMERCHANT|RANCHHAND|SON|WIFE|DUTCH|BILL|TYRANT|JAVIER|CONTACT|CONTACT_GREEN|CONTACT_RED|ENEMY|ENEMY_DOWN|ENEMY_UP|FRIEND|HOME|HORSE|HORSE_BLUE|MONEY|OBJECTIVE|OBJECTIVE_DOWN|OBJECTIVE_UP|PLAYER|PLAYER_HORSE|PICKUP|BLACKSMITH|COACH_DRIVER|HEAR_NOISE|TRAIN|MOST_WANTED|NIGHTWATCH|COP_DOWN|COP_UP|EYEWITNESS|SHERIFF|TELEGRAPH|BEER|BULLRIDING|CARDS|HORSESHOE|ARM_WRESTLING|DUEL|FIVE_FINGER|COACH_TAXI_STOP|CHECKPOINT_GENERIC|MP_COLOR_1|MP_COLOR_2|MP_COLOR_3|MP_COLOR_4|MP_COLOR_5|MP_COLOR_6|MP_COLOR_7|MP_COLOR_8|MP_COLOR_9|MP_COLOR_10|MP_COLOR_11|MP_COLOR_12|MP_COLOR_13|MP_COLOR_14|MP_COLOR_15|MP_COLOR_16|MP_RED_BAG|MP_BLUE_BAG|MP_NEUTRAL_BAG|MP_RED_BASE|MP_BLUE_BASE|MP_NEUTRAL_BASE|WEAPON_CACHE|MP_PICKUP_WEAPON|MP_PICKUP_AMMO|MP_PICKUP_ITEM|POSSE_LEADER|ACTIVE_ACTION_AREA|SET_MAP_DESTINATION|SCRAP|AMBIENT_ATTACK|ATTACK_COACH|DEFEND_COACH|FIRE|RED_SKULL|SKULL|TRANSPORT_BROWN|TRANSPORT_RED|TRANSPORT_WHITE|SHOP|DOCTOR|GUNSMITH|TRAIN_TICKETS|5|BOMB|CHECKER|DYNAMITE|FIREBOTTLE|HEIST_END|HEIST_START|HOSTAGE|MEDICINAL_PLANT|MISC_INJURED_PERSON|QUESTION_MARK|RACE_CHECKPOINT|RACE_FINISH|RACE_OPPONENT|STAR_RED|TARGET|WHITE|WILD_HORSE|RED_CIRCLE|HOUSE_BUY|HOUSE_RENT|HERD|HERD_MAIN|HERD_STRAGGLER|HERD_RETURN|LIARSDICE|TRAIN_RED|TRAIN_BLUE|FRIEND_DOWN|FRIEND_UP|PICKUP_DOWN|PICKUP_UP|POKER|MP_COLOR_RED_TEAM|MP_COLOR_BLUE_TEAM|MP_COLOR_RED_TEAM_UP|MP_COLOR_RED_TEAM_DOWN|MP_COLOR_BLUE_TEAM_UP|MP_COLOR_BLUE_TEAM_DOWN|SWAG|SWAG_DEF|SWAG_RET|SWAG_CAP|SWAG_TAKE|SWAG_Y|SWAG_Y_DEF|SWAG_Y_RET|SWAG_Y_CAP|SWAG_Y_TAKE|SWAG_R|SWAG_R_DEF|SWAG_R_RET|SWAG_R_CAP|SWAG_R_TAKE|SWAG_B|SWAG_B_DEF|SWAG_B_RET|SWAG_B_CAP|SWAG_B_TAKE|CHEST|CHEST_YELLOW|CHEST_RED|CHEST_BLUE|MP_ACHIEVE|MP_FINISHED_RACE|MP_KO|MPK_VIP|MPK_SPREE|MPK_DEFEND|MPK_HRS|MPK_TEAMMATE|MPK_MULTIPLY|MPK_KNIFE|MPK_ANIMAL|MPK_HEAD|MPK_SNIPER|MPK_SPREE_END|MPK_NPC|MP_WIN_GAMETYPE|MPS_BURN|MPS_DROWN|MPS_FALL|MPS_SPLODE|MPS_CACTUS|MP_TEAM|MP_FFA|MP_TDM|MP_TCTF|MP_DM|MP_CTF|MP_COOP|TAILOR|CANNON|GATLING|MOVIEHOUSE|MP_CLASSIC_CTF|NEWSPAPER|MP_REZ|MP_REZ_UP|MP_REZ_DOWN|XP|MP_XP|SPACE|STAR_GOLD|STAR_SILVER|STAR_BRONZE|MP_COOP_ADV|MP_FFA_HC|MP_FFA_V|MP_FIND_FREE|MP_QUICK|MP_TEAM_HC|MP_TEAM_V|MPK_REVENGE|MP_TEAM_C|MP_FFA_C|MP_SCORE|MP_KILLS|MP_REVIVES|MP_INJURED|MP_DEATH|MP_DEFEND|BAGCAPS|MPK_ASSIST|MPK_REVENGE|MP_KILLSTREAK|MPK_PWHIP|VGO|VSKIP|VRE|ROCKSTAR|MP_LOCKED|VNO|MPS_BLED|MP_LIVES|ENVELOPE|FS1|FS2|FS3|FS4|FS5|FS6|FS7|FS8|FS9|NL|DICE1|DICE2|DICE3|DICE4|DICE5|DICE6|DICE7|BANK|BANDITS|TOWN_SAFE|CONVOY|WAGON)>";

                Value = Value.Replace("</<", "</");

                string sanitizedInput = Regex.Replace(Value, pattern1, match =>
                {
                    string tagName = match.Value.Trim('<', '>');
                    return $"~{tagName}~";
                }, RegexOptions.IgnoreCase);

                sanitizedInput = Regex.Replace(sanitizedInput, pattern2, match =>
                {
                    string tagName = match.Value.Trim('<', '>');
                    return $"~{tagName}~";
                }, RegexOptions.IgnoreCase);

                string reverseHexPattern = @"<0x([0-9a-fA-F]+)>(.*?)";
                sanitizedInput = Regex.Replace(sanitizedInput, reverseHexPattern, match =>
                {
                    string hex = match.Groups[1].Value;
                    string content = match.Groups[2].Value;
                    return $"~0x{hex}~{content}";
                });

                sanitizedInput = sanitizedInput.Replace("\0", string.Empty);
                sanitizedInput = sanitizedInput.Replace("&", "&amp;");
                sanitizedInput = sanitizedInput.Replace("\"", "&quot;");
                sanitizedInput = sanitizedInput.Replace("'", "&apos;");
                sanitizedInput = sanitizedInput.Replace("</0x>", "~/0x~");

                if (string.IsNullOrWhiteSpace(sanitizedInput)) //A single space is treated as insignificant and interpreted as an empty string by XML parsers
                {
                    sanitizedInput = sanitizedInput.Replace(" ", "~whitespace~");
                }

                writer.WriteJenkHash("@hash", Hash);
                writer.WriteString("Value", sanitizedInput);
            }
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