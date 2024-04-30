using System.IO;
using CodeX.Core.Utilities;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeX.Games.RDR1.Files
{
    [TC(typeof(EXP))] public class FonttexFile : MetaNode
    {
        public float Version { get; set; } //version, 1.02f
        public int CharHeight { get; set; } //m_CharHeight, delta y for a linefeed
        public int CharSpacing { get; set; } //m_CharSpacing, spacing between adjacent characters (very small)
        public int SpaceWidth { get; set; } //m_SpaceWidth, width of a space (not counting m_CharSpacing)
        public int MonospaceWidth { get; set; } //m_MonospaceWidth, width of characters in monospace mode (not counting m_CharSpacing)
        public int MaxAscent { get; set; } //m_MaxAscent, maxheight above the baseline
        public int MaxDescent { get; set; } //m_MaxDescent, maxheight below the baseline
        public int ShadowLeft { get; set; } //m_ShadowLeft, extra space left of each glyph for shadow
        public int ShadowRight { get; set; } //m_ShadowRight, extra space right of each glyph for shadow
        public int ShadowTop { get; set; } //m_ShadowTop, extra room above each glyph for shadow
        public int ShadowBottom { get; set; } //m_ShadowBottom, extra room below each glyph for shadow
        public int NumGlyphs { get; set; } //m_NumGlyphs
        public Rsc6FontTexGlyph[] Glyphs { get; set; } //m_Glyphs
        public int NumTextures { get; set; } //numTextures
        public int[] GlyphsPerTexture { get; set; } //m_GlyphsPerTexture

        public FonttexFile()
        {

        }

        public void Load(byte[] data)
        {
            using var ms = new MemoryStream(data);
            var r = new DataReader(ms);

            Version = r.ReadSingle();
            CharHeight = r.ReadInt32();
            CharSpacing = r.ReadInt32();
            SpaceWidth = r.ReadInt32();
            MonospaceWidth = r.ReadInt32();
            MaxAscent = r.ReadInt32();
            MaxDescent = r.ReadInt32();
            ShadowLeft = r.ReadInt32();
            ShadowRight = r.ReadInt32();
            ShadowTop = r.ReadInt32();
            ShadowBottom = r.ReadInt32();

            NumGlyphs = r.ReadInt32();
            Glyphs = new Rsc6FontTexGlyph[NumGlyphs];
            for (int i = 0; i < NumGlyphs; i++)
            {
                Glyphs[i] = new Rsc6FontTexGlyph()
                {
                    Character = r.ReadUInt32(),
                    X = r.ReadByte(),
                    Y = r.ReadByte(),
                    W = r.ReadByte(),
                    H = r.ReadByte(),
                    Baseline = r.ReadByte(),
                    AddWidth = r.ReadByte()
                };
            }

            NumTextures = r.ReadInt32();
            GlyphsPerTexture = new int[NumTextures];
            for (int i = 0; i < NumTextures; i++)
            {
                GlyphsPerTexture[i] = r.ReadInt32();
            }
        }

        public byte[] Save()
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
            writer.Write(Version);
            writer.Write(CharHeight);
            writer.Write(CharSpacing);
            writer.Write(SpaceWidth);
            writer.Write(MonospaceWidth);
            writer.Write(MaxAscent);
            writer.Write(MaxDescent);
            writer.Write(ShadowLeft);
            writer.Write(ShadowRight);
            writer.Write(ShadowTop);
            writer.Write(ShadowBottom);
            writer.Write(NumGlyphs);

            foreach (var glyph in Glyphs)
            {
                glyph.Write(writer);
            }

            writer.Write(NumTextures);
            writer.WriteArray(GlyphsPerTexture, (wtr, v) => wtr.Write(v));
        }

        public void Read(MetaNodeReader reader)
        {
            CharHeight = reader.ReadInt32("CharHeight");
            CharSpacing = reader.ReadInt32("CharSpacing");
            SpaceWidth = reader.ReadInt32("SpaceWidth");
            MonospaceWidth = reader.ReadInt32("MonospaceWidth");
            MaxAscent = reader.ReadInt32("MaxAscent");
            MaxDescent = reader.ReadInt32("MaxDescent");
            ShadowLeft = reader.ReadInt32("ShadowLeft");
            ShadowRight = reader.ReadInt32("ShadowRight");
            ShadowTop = reader.ReadInt32("ShadowTop");
            ShadowBottom = reader.ReadInt32("ShadowBottom");
            Glyphs = reader.ReadNodeArray<Rsc6FontTexGlyph>("Glyphs");
            GlyphsPerTexture = reader.ReadInt32Array("GlyphsPerTexture");

            Version = 1.02f;
            NumGlyphs = Glyphs?.Length ?? 0;
            NumTextures = GlyphsPerTexture?.Length ?? 0;
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteInt32("CharHeight", CharHeight);
            writer.WriteInt32("CharSpacing", CharSpacing);
            writer.WriteInt32("SpaceWidth", SpaceWidth);
            writer.WriteInt32("MonospaceWidth", MonospaceWidth);
            writer.WriteInt32("MaxAscent", MaxAscent);
            writer.WriteInt32("MaxDescent", MaxDescent);
            writer.WriteInt32("ShadowLeft", ShadowLeft);
            writer.WriteInt32("ShadowRight", ShadowRight);
            writer.WriteInt32("ShadowTop", ShadowTop);
            writer.WriteInt32("ShadowBottom", ShadowBottom);
            writer.WriteNodeArray("Glyphs", Glyphs);
            writer.WriteInt32Array("GlyphsPerTexture", GlyphsPerTexture);
        }
    }
}