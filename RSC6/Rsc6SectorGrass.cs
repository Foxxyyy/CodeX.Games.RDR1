using CodeX.Core.Utilities;
using System.Numerics;
using System.Text;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6SectorGrass : Rsc6FileBase
    {
        public override ulong BlockLength => 28;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6PtrArr<Rsc6GrassField> GrassItems { get; set; } //grassField

        public Rsc6SectorGrass()
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            GrassItems = reader.ReadPtrArr<Rsc6GrassField>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(0x049FFD14);
            writer.WritePtr(BlockMap);
            writer.WritePtrArr(GrassItems);
        }

        public override string ToString()
        {
            if (GrassItems.Items == null) return string.Empty;

            var sb = new StringBuilder();
            for (int i = 0; i < GrassItems.Items.Length; i++)
            {
                var item = GrassItems.Items[i];
                sb.AppendLine($"Item {i + 1} :");
                sb.AppendLine($"  - AABBMin: {item.AABBMin}");
                sb.AppendLine($"  - AABBMax: {item.AABBMax}");
                sb.AppendLine($"  - AABBScale: {item.AABBScale}");
                sb.AppendLine($"  - AABBOffset: {item.AABBOffset}");
                sb.AppendLine($"  - TP: {item.TP}");
                sb.AppendLine($"  - VertexBuffer: {item.VertexBuffer.Item?.ToString() ?? "Unknown"}");
                sb.AppendLine($"  - Zup: {item.Zup}");
                sb.AppendLine($"  - UseSortedBuffers: {item.UseSortedBuffers}");
                sb.AppendLine($"  - Name: {item.Name}");
                sb.AppendLine($"  - NameHash: {item.NameHash}\n");
            }
            return sb.ToString();
        }
    }

    public class Rsc6GrassField : Rsc6Block //rage::grassField
    {
        public ulong BlockLength => 112;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Vector4 Extents { get; set; } //m_Extents
        public Vector4 AABBMax { get; set; } //m_aabbMax
        public Vector4 AABBMin { get; set; } //m_aabbMin
        public Vector4 AABBScale { get; set; } //m_aabbScale
        public Vector4 AABBOffset { get; set; } //m_aabbOffset
        public uint TP { get; set; } //m_tp
        public Rsc6Ptr<Rsc6VertexDeclaration> Layout { get; set; } //m_VertexDeclaration, always NULL
        public Rsc6Ptr<Rsc6VertexBuffer> VertexBuffer { get; set; } //m_VertexBuffer
        public byte Zup { get; set; } //m_Zup
        public bool UseSortedBuffers { get; set; } //m_useSortedBuffers
        public ushort Pad0 { get; set; } = 0xCDCD; //m_Pad0
        public Rsc6Str Name { get; set; } //m_Type
        public uint Unknown_64h { get; set; } //0x0B000C00 ?
        public JenkHash NameHash { get; set; } //m_TypeHash
        public uint Pad1 { get; set; } = 0xCDCDCDCD; //m_Pad1

        public void Read(Rsc6DataReader reader)
        {
            Extents = reader.ReadVector4();
            AABBMax = reader.ReadVector4();
            AABBMin = reader.ReadVector4();
            AABBScale = reader.ReadVector4();
            AABBOffset = reader.ReadVector4();
            TP = reader.ReadUInt32();
            Layout = reader.ReadPtr<Rsc6VertexDeclaration>();
            VertexBuffer = reader.ReadPtr<Rsc6VertexBuffer>();
            Zup = reader.ReadByte();
            UseSortedBuffers = reader.ReadBoolean();
            Pad0 = reader.ReadUInt16();
            Name = reader.ReadStr();
            Unknown_64h = reader.ReadUInt32();
            NameHash = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(Extents);
            writer.WriteVector4(AABBMax);
            writer.WriteVector4(AABBMin);
            writer.WriteVector4(AABBScale);
            writer.WriteVector4(AABBOffset);
            writer.WriteUInt32(TP);
            writer.WritePtr(Layout);
            writer.WritePtr(VertexBuffer);
            writer.WriteByte(Zup);
            writer.WriteByte(UseSortedBuffers ? (byte)1 : (byte)0);
            writer.WriteUInt16(Pad0);
            writer.WriteStr(Name);
            writer.WriteUInt32(Unknown_64h);
            writer.WriteUInt32(NameHash);
        }

        public override string ToString()
        {
            return Name.Value;
        }
    }
}
