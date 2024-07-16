using CodeX.Core.Utilities;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6GringoDictionary : Rsc6BlockBaseMap, MetaNode
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x0091BC40;
        public uint Unknown_8h { get; set; }
        public uint Unknown_Ch { get; set; }
        public Rsc6Arr<JenkHash> Hashes { get; set; }
        public Rsc6PtrArr<Rsc6Gringo> Gringos { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Gringos = reader.ReadPtrArr<Rsc6Gringo>();
        }

        public void Read(MetaNodeReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Gringos", Gringos.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6Gringo : Rsc6GringoBase, MetaNode //ggoItemGringo
    {
        public override ulong BlockLength => base.BlockLength + 60;
        public short InstanceIndex { get; set; } //m_iInstanceIndex
        public short PaddingToFoxPs3 { get; set; } //m_iPaddingToFoxPs3
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public Rsc6Str ScriptName { get; set; } //mp_ScriptName
        public Rsc6Str GringoName { get; set; } //mp_GringoName
        public Rsc6PtrArr<Rsc6GringoBase> Childs { get; set; } //ggoChildComponentList
        public Rsc6PtrArr<Rsc6BlockMap> InstancedItems { get; set; } //m_InstancedItems ggoInstancedTypeDataBase
        public uint HashedName { get; set; } //m_iHashedName
        public Rsc6MsgMaskType MessageMask { get; set; } //m_iMessageMask
        public float ActivationRadius { get; set; } //m_ActivationRadius
        public int InstanceSlotCount { get; set; } //m_iInstanceSlotCount
        public bool Critical { get; set; } //m_bCritical
        public bool LargeScript { get; set; } //m_bLargeScript
        public bool MaintainState { get; set; } //m_bMaintainState
        public byte Unknown_4Bh { get; set; } //m_bMaintainState

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            InstanceIndex = reader.ReadInt16();
            PaddingToFoxPs3 = reader.ReadInt16();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            ScriptName = reader.ReadStr();
            GringoName = reader.ReadStr();
            Childs = reader.ReadPtrArr<Rsc6GringoBase>();
            InstancedItems = reader.ReadPtrArr<Rsc6BlockMap>();
            HashedName = reader.ReadUInt32();
            MessageMask = (Rsc6MsgMaskType)reader.ReadUInt32();
            ActivationRadius = reader.ReadSingle();
            InstanceSlotCount = reader.ReadInt32();
            Critical = reader.ReadBoolean();
            LargeScript = reader.ReadBoolean();
            MaintainState = reader.ReadBoolean();
            Unknown_4Bh = reader.ReadByte();
        }

        public new void Read(MetaNodeReader reader)
        {
            throw new System.NotImplementedException();
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteInt16("InstanceIndex", InstanceIndex);
            writer.WriteInt16("PaddingToFoxPs3", PaddingToFoxPs3);
            writer.WriteUInt32("Unknown_14h", Unknown_14h);
            writer.WriteUInt32("Unknown_18h", Unknown_18h);
            writer.WriteUInt32("Unknown_1Ch", Unknown_1Ch);
            if (ScriptName.Value != null) writer.WriteString("ScriptName", ScriptName.ToString());
            if (GringoName.Value != null) writer.WriteString("GringoName", GringoName.ToString());
            writer.WriteNodeArray("InstancedItems", Childs.Items);
            writer.WriteUInt32("HashedName", HashedName);
            writer.WriteEnum("MessageMask", MessageMask);
            writer.WriteSingle("ActivationRadius", ActivationRadius);
            writer.WriteInt32("InstanceSlotCount", InstanceSlotCount);
            writer.WriteBool("Critical", Critical);
            writer.WriteBool("LargeScript", LargeScript);
            writer.WriteBool("MaintainState", MaintainState);
        }
    }

    [TC(typeof(EXP))] public class Rsc6GringoBase : Rsc6FileBase, MetaNode //ggoComponentBase
    {
        public override ulong BlockLength => 16;
        public override uint VFT { get; set; } = 0x01979634;
        public Rsc6Str QueryName { get; set; } //mp_QueryName
        public uint HashCode { get; set; } //m_HashCode
        public Rsc6Ptr<Rsc6Gringo> ParentComponent { get; set; } //mp_ParentComponent

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            QueryName = reader.ReadStr();
            HashCode = reader.ReadUInt32();
            ParentComponent = reader.ReadPtr<Rsc6Gringo>();
        }

        public void Read(MetaNodeReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void Write(MetaNodeWriter writer)
        {
            if (QueryName.Value != null) writer.WriteString("QueryName", QueryName.ToString());
            writer.WriteUInt32("HashCode", HashCode);
        }
    }

    public enum Rsc6MsgMaskType : uint
    {
        ComponentBase = 0,
        ComponentEnvWeap = 58,
        ComponentHotspot = 576,
        ComponentAnimation = 1039,
        ItemGringo = 2048,
        ComponentProp = 10351
    }
}
