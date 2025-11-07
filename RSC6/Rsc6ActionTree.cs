using CodeX.Core.Utilities;
using System;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))]
    public class Rsc6ActionTree : Rsc6BlockBaseMap, MetaNode //rage::ActionNodeTree
    {
        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x049FFF38;
        public Rsc6Ptr<Rsc6ActionNode> RootNode { get; set; } //m_Root

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            RootNode = reader.ReadPtr(Rsc6ActionNode.Create);
        }

        public void Read(MetaNodeReader reader)
        {
            RootNode = new(reader.ReadNode("RootNode", Rsc6ActionNode.Create));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNode("RootNode", RootNode.Item);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ActionNode : Rsc6FileBase, MetaNode //rage::ActionNode
    {
        public override ulong BlockLength => 28;
        public override uint VFT { get; set; } = 0x04C9E138;
        public Rsc6Ptr<Rsc6ActionNode> ParentNode { get; set; } //m_parent
        public Rsc6PtrArr<Rsc6ActionNode> ChildNodes { get; set; } //m_childNodes
        public uint Unknown_10h { get; set; } //Always 0?
        public uint PathCRC { get; set; } //m_PathCRC, always 0?
        public ushort Unknown_18h { get; set; }
        public byte Unknown_1Ah { get; set; } //Always 0?
        public Rsc6ActionNodeType Type { get; set; } //m_Type

        public Rsc6ActionNode()
        {
        }

        public Rsc6ActionNode(Rsc6ActionNodeType type)
        {
            Type = type;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            ParentNode = reader.ReadPtr(Create);
            ChildNodes = reader.ReadPtrArr(Create);
            Unknown_10h = reader.ReadUInt32();
            PathCRC = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt16();
            Unknown_1Ah = reader.ReadByte();
            Type = (Rsc6ActionNodeType)reader.ReadByte();
        }

        public virtual void Read(MetaNodeReader reader)
        {
            Type = reader.ReadEnum<Rsc6ActionNodeType>("@type");
            ParentNode = new(reader.ReadNode<Rsc6ActionNode>("ParentNode"));
            ChildNodes = new(reader.ReadNodeArray<Rsc6ActionNode>("Childs"));
            Unknown_10h = reader.ReadUInt32("Unknown_10h");
            PathCRC = reader.ReadUInt32("PathCRC");
        }

        public virtual void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", Type.ToString());
            //writer.WriteNode("ParentNode", ParentNode.Item);
            writer.WriteNodeArray("Childs", ChildNodes.Items);
            if (Unknown_10h != 0) writer.WriteUInt32("Unknown_10h", Unknown_10h);
            if (PathCRC != 0) writer.WriteUInt32("PathCRC", PathCRC);
        }

        public static Rsc6ActionNode Create(string typeName)
        {
            if (Enum.TryParse(typeName, out Rsc6ActionNodeType type))
            {
                return Create(type);
            }
            return null;
        }

        public static Rsc6ActionNode Create(Rsc6DataReader r)
        {
            r.Position += 27;
            var type = (Rsc6ActionNodeType)r.ReadByte();
            r.Position -= 28;
            return Create(type);
        }

        public static Rsc6ActionNode Create(Rsc6ActionNodeType type)
        {
            return type switch
            {
                Rsc6ActionNodeType.ActionNodeBank => new Rsc6ActionNodeBank(),
                Rsc6ActionNodeType.ActionNodeImplementation => new Rsc6ActionNodeImplementation(),
                _ => throw new Exception("Unknown action node type")
            };
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ActionNodeBank : Rsc6ActionNode, MetaNode //rage::ActionNodeBank
    {
        public override ulong BlockLength => 52;
        public override uint VFT { get; set; } = 0x04C9E138;
        public Rsc6CName Name { get; set; } //m_Name
        public Rsc6ConditionGroup Conditions { get; set; } //m_Conditions
        public Rsc6PtrArr<Rsc6Track> Tracks { get; set; } //m_tracks

        public Rsc6ActionNodeBank() : base(Rsc6ActionNodeType.ActionNodeBank)
        {
        }

        public Rsc6ActionNodeBank(Rsc6ActionNodeType type) : base(type)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.ReadBlock<Rsc6CName>();
            Conditions = reader.ReadBlock<Rsc6ConditionGroup>();
            Tracks = reader.ReadPtrArr<Rsc6Track>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteBlock(Name);
            writer.WriteBlock(Conditions);
            writer.WritePtrArr(Tracks);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            var hash = reader.ReadJenkHash("Name");

            Conditions = reader.ReadNode<Rsc6ConditionGroup>("Conditions");
            Tracks = new(reader.ReadNodeArray<Rsc6Track>("Tracks"));

            Name = new Rsc6CName()
            {
                HashID = hash
            };
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteJenkHash("Name", Name.ToString());
            writer.WriteNode("Conditions", Conditions);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ActionNodeImplementation : Rsc6ActionNodeBank, MetaNode //rage::ActionNodeImplementation
    {
        /*
         * Same structure as rage::ActionNodeBank
         */

        public Rsc6ActionNodeImplementation() : base(Rsc6ActionNodeType.ActionNodeImplementation)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public override void Read(MetaNodeReader reader)
        {
            base.Read(reader);
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ConditionGroup : Rsc6BlockBase, MetaNode //rage::ConditionGroup
    {
        public override ulong BlockLength => 8;
        public Rsc6PtrArr<Rsc6BlockMap> Conditions { get; set; } //m_Conditions

        public override void Read(Rsc6DataReader reader)
        {
            Conditions = reader.ReadPtrArr<Rsc6BlockMap>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrArr(Conditions);
        }

        public void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public void Write(MetaNodeWriter writer)
        {
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Track : Rsc6BlockBase, MetaNode //rage::Track
    {
        public override ulong BlockLength => 12;
        public uint Unknown_0h { get; set; }
        public float TimeBegin { get; set; } //m_TimeBegin
        public float TimeEnd { get; set; } //m_TimeEnd
        public bool Master { get; set; } //m_Master

        public override void Read(Rsc6DataReader reader)
        {
            Unknown_0h = reader.ReadUInt32();
            TimeBegin = reader.ReadSingle();
            TimeEnd = reader.ReadSingle();
            Master = reader.ReadBoolean();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Unknown_0h);
            writer.WriteSingle(TimeBegin);
            writer.WriteSingle(TimeEnd);
            writer.WriteBoolean(Master);
        }

        public void Read(MetaNodeReader reader)
        {
            Unknown_0h = reader.ReadUInt32("Unknown_0h");
            TimeBegin = reader.ReadSingle("TimeBegin");
            TimeEnd = reader.ReadSingle("TimeEnd");
            Master = reader.ReadBool("Master");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Unknown_0h", Unknown_0h);
            writer.WriteSingle("TimeBegin", TimeBegin);
            writer.WriteSingle("TimeEnd", TimeEnd);
            writer.WriteBool("Master", Master);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6CName : Rsc6BlockBase //rage::CName
    {
        public override ulong BlockLength => 8;
        public JenkHash Name { get; set; } //m_name
        public JenkHash HashID { get; set; } //m_HashID

        public override void Read(Rsc6DataReader reader)
        {
            Name = reader.ReadUInt32();
            HashID = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Name);
            writer.WriteUInt32(HashID);
        }

        public override string ToString()
        {
            return HashID.ToString();
        }
    }

    public enum Rsc6ActionNodeType : byte
    {
        ActionNodeBank = 3,
        ActionNodeImplementation = 4
    }
}