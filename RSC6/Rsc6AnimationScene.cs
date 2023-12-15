using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6AnimationScene : Rsc6FileBase
    {
        public override ulong BlockLength => 12;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6CustomArr<Rsc6ClipDictionary> ClipDictionary { get; set; } //m_ClipDict
        public uint Unknown_10h { get; set; } = 0x00CDCDCD;
        public Rsc6Ptr<Rsc6Clip> ClipMap { get; set; } //m_ASTtoClipMap
        public uint Unknown_18h { get; set; } = 0xCDCDCD00;
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD;
        public uint Unknown_20h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD;
        public uint Unknown_30h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_34h { get; set; } = 0xCDCDCDCD;
        public string Name { get; set; } //m_AnimSetName

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            ClipDictionary = reader.ReadArr<Rsc6ClipDictionary>();
            Unknown_10h = reader.ReadUInt32();
            ClipMap = reader.ReadPtr<Rsc6Clip>();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
            Unknown_30h = reader.ReadUInt32();
            Unknown_34h = reader.ReadUInt32();
            Name = reader.ReadString();
        }
    }
}
