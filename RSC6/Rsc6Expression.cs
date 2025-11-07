using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;
using System;
using System.Collections.Generic;
using System.Numerics;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))]
    public class Rsc6ExpressionDictionary : Rsc6BlockBaseMapRef, MetaNode //pgDictionary<crExpressions>
    {
        /*
         * Dictionary of expressions
         */

        public override ulong BlockLength => 32;
        public override uint VFT { get; set; } = 0x00D0E590;
        public new uint RefCount { get; set; } = 1; //m_RefCount
        public Rsc6Arr<JenkHash> Hashes { get; set; }
        public Rsc6PtrArr<Rsc6Expressions> Expressions { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            RefCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Expressions = reader.ReadPtrArr<Rsc6Expressions>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(RefCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Expressions);
        }

        public void Read(MetaNodeReader reader)
        {
            Hashes = new(reader.ReadJenkHashArray("Hashes"));
            Expressions = new(reader.ReadNodeArray<Rsc6Expressions>("Dictionary"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteJenkHashArray("Hashes", Hashes.Items);
            writer.WriteNodeArray("Dictionary", Expressions.Items);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Expressions : Rsc6FileBase, MetaNode //rage::crExpressions
    {
        /*
         * Contains a series of expressions
         */

        public override ulong BlockLength => 36;
        public override uint VFT { get; set; } = 0x00D4E824;
        public uint RefCount { get; set; } //m_RefCount
        public Rsc6PtrArr<Rsc6Expression> Expressions { get; set; } //m_Expressions
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } = 1; //Always 1
        public uint Unknown_18h { get; set; } //Always 0
        public uint ExpressionFilter { get; set; } //m_ExpressionFilter
        public uint MaxPackedSize { get; set; } //m_MaxPackedSize, max length of any item

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            RefCount = reader.ReadUInt32();
            Expressions = reader.ReadPtrArr<Rsc6Expression>();
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            ExpressionFilter = reader.ReadUInt32();
            MaxPackedSize = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(RefCount);
            writer.WritePtrArr(Expressions);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(ExpressionFilter);
            writer.WriteUInt32(MaxPackedSize);
        }

        public void Read(MetaNodeReader reader)
        {
            ExpressionFilter = reader.ReadUInt32("ExpressionFilter");
            MaxPackedSize = reader.ReadUInt32("MaxPackedSize");
            Expressions = new(reader.ReadNodeArray<Rsc6Expression>("Expressions"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("ExpressionFilter", ExpressionFilter);
            writer.WriteUInt32("MaxPackedSize", MaxPackedSize);
            writer.WriteNodeArray("Expressions", Expressions.Items);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Expression : Rsc6BlockBase, MetaNode //rage::crExpression
    {
        /*
         * An individual expression, describes the calculation or modification of a degree of freedom.
         */

        public override ulong BlockLength => 32;
        public ushort Enabled { get; set; } //m_Enabled
        public ushort StackDepth { get; set; } //m_StackDepth
        public Rsc6Ptr<Rsc6ExpressionOp> ExpressionOp { get; set; } //m_ExpressionOp
        public uint Signature { get; set; } //m_Signature
        public uint PackedSize { get; set; } //m_PackedSize
        public Rsc6ManagedArr<Rsc6ExpressionIODof> InputOutputDofs { get; set; } //m_InputOutputDofs
        public ushort NumAcceleratedIndices { get; set; } //m_NumAcceleratedIndices
        public uint Unknown_1Ah { get; set; } //Always 0, padding
        public ushort Unknown_1Eh { get; set; } //Always 0, padding

        public override void Read(Rsc6DataReader reader)
        {
            Enabled = reader.ReadUInt16();
            StackDepth = reader.ReadUInt16();
            ExpressionOp = reader.ReadPtr(Rsc6ExpressionOp.Create);
            Signature = reader.ReadUInt32();
            PackedSize = reader.ReadUInt32();
            InputOutputDofs = reader.ReadArr<Rsc6ExpressionIODof>();
            NumAcceleratedIndices = reader.ReadUInt16();
            Unknown_1Ah = reader.ReadUInt32();
            Unknown_1Eh = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt16(Enabled);
            writer.WriteUInt16(StackDepth);
            writer.WritePtr(ExpressionOp);
            writer.WriteUInt32(Signature);
            writer.WriteUInt32(PackedSize);
            writer.WriteArr(InputOutputDofs);
            writer.WriteUInt16(NumAcceleratedIndices);
            writer.WriteUInt32(Unknown_1Ah);
            writer.WriteUInt16(Unknown_1Eh);
        }

        public void Read(MetaNodeReader reader)
        {
            Enabled = (ushort)(reader.ReadBool("Enabled") ? 1 : 0);
            StackDepth = reader.ReadUInt16("StackDepth");
            Signature = reader.ReadUInt32("Signature");
            PackedSize = reader.ReadUInt32("PackedSize");
            ExpressionOp = new(reader.ReadNode("ExpressionOp", Rsc6ExpressionOp.Create));
            InputOutputDofs = new(reader.ReadNodeArray<Rsc6ExpressionIODof>("InputOutputDofs"));
            NumAcceleratedIndices = (ushort)(InputOutputDofs.Items?.Length ?? 0);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteBool("Enabled", Enabled == 1);
            writer.WriteUInt16("StackDepth", StackDepth);
            writer.WriteUInt32("Signature", Signature);
            writer.WriteUInt32("PackedSize", PackedSize);
            writer.WriteNode("ExpressionOp", ExpressionOp.Item);
            writer.WriteNodeArray("InputOutputDofs", InputOutputDofs.Items);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionIODof : Rsc6BlockBase, MetaNode //rage::crExpression::InputOutputDof
    {
        public override ulong BlockLength => 12;
        public Rsc6BoneIdEnum OutputTrackID { get; set; } //m_OutputTrackId
        public Rsc6Arr<uint> InputTrackIDs { get; set; } //m_InputTrackIds

        public Rsc6BoneIdEnum[] ITrackIDs
        {
            get
            {
                var list = new List<Rsc6BoneIdEnum>();
                foreach (var id in InputTrackIDs.Items)
                {
                    list.Add((Rsc6BoneIdEnum)id);
                }
                return list.ToArray();
            }
            set
            {
                var list = new List<uint>();
                foreach (var id in value)
                {
                    list.Add((uint)id);
                }
                InputTrackIDs = new(list.ToArray());
            }
        }

        public override void Read(Rsc6DataReader reader)
        {
            OutputTrackID = (Rsc6BoneIdEnum)reader.ReadUInt32();
            InputTrackIDs = reader.ReadArr<uint>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32((uint)OutputTrackID);
            writer.WriteArr(InputTrackIDs);
        }

        public void Read(MetaNodeReader reader)
        {
            OutputTrackID = reader.ReadEnum<Rsc6BoneIdEnum>("OutputTrackID");
            ITrackIDs = reader.ReadEnumArray<Rsc6BoneIdEnum>("InputTrackIDs");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteEnum("OutputTrackID", OutputTrackID);
            writer.WriteEnumArray("InputTrackIDs", ITrackIDs);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOp : Rsc6FileBase, MetaNode //rage::crExpressionOp (expression operations)
    {
        public override ulong BlockLength => 8;
        public override uint VFT { get; set; }
        public Rsc6OperationTypes OpType { get; set; } //m_OpType
        public ushort OpSubType { get; set; } //m_OpSubType

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            OpType = (Rsc6OperationTypes)reader.ReadUInt16();
            OpSubType = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt16((ushort)OpType);
            writer.WriteUInt16(OpSubType);
        }

        public void Read(MetaNodeReader reader)
        {
            OpType = reader.ReadEnum<Rsc6OperationTypes>("@type");
            OpSubType = reader.ReadUInt16("OpSubType");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", OpType.ToString());
            writer.WriteUInt16("SubType", OpSubType);
        }

        public static Rsc6ExpressionOp Create(string typeName)
        {
            if (Enum.TryParse(typeName, out Rsc6OperationTypes type))
            {
                return Create(type);
            }
            return null;
        }

        public static Rsc6ExpressionOp Create(Rsc6DataReader r)
        {
            r.Position += 4;
            var type = (Rsc6OperationTypes)r.ReadUInt16();
            r.Position -= 6;
            return Create(type);
        }

        public static Rsc6ExpressionOp Create(Rsc6OperationTypes type)
        {
            return type switch
            {
                Rsc6OperationTypes.CONSTANT => new Rsc6ExpressionOpConstant(),
                Rsc6OperationTypes.CONSTANT_FLOAT => new Rsc6ExpressionOpFloat(),
                Rsc6OperationTypes.GET => new Rsc6ExpressionOpGetSet(),
                Rsc6OperationTypes.SET => new Rsc6ExpressionOpSet(),
                Rsc6OperationTypes.COMPONENT_GET => new Rsc6ExpressionOpGetSet(),
                Rsc6OperationTypes.FAST_BLEND_VECTOR => new Rsc6ExpressionOpFastBlendVector(),
                Rsc6OperationTypes.NULLARY => new Rsc6ExpressionOpNullary(),
                Rsc6OperationTypes.UNARY => new Rsc6ExpressionOpUnary(),
                Rsc6OperationTypes.BINARY => new Rsc6ExpressionOpBinary(),
                Rsc6OperationTypes.TERNARY => new Rsc6ExpressionOpTernary(),
                Rsc6OperationTypes.NARY => new Rsc6ExpressionOpNary(),
                Rsc6OperationTypes.SPECIAL_CURVE => new Rsc6ExpressionOpSpecialCurve(),
                _ => null,
            };
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpConstant : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 24;
        public byte[] Padding { get; set; } //m_Padding[8]
        public Vector4 Value { get; set; } //m_Values[4]

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Padding = reader.ReadBytes(8);
            Value = reader.ReadVector4();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteBytes(Padding);
            writer.WriteVector4(Value);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = Rpf6Crypto.ToXYZ(reader.ReadVector4("Value"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteVector4("Value", Value);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpFloat : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 4;
        public float Value { get; set; } //m_Float

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Value = reader.ReadSingle();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle(Value);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Value = reader.ReadSingle("Value");
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteSingle("Value", Value);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpGetSet : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 4;
        public byte Track { get; set; } //m_Track
        public byte Type { get; set; } //m_Type
        public Rsc6BoneIdEnum ID { get; set; } //m_Id

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Track = reader.ReadByte();
            Type = reader.ReadByte();
            ID = (Rsc6BoneIdEnum)reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteByte(Track);
            writer.WriteByte(Type);
            writer.WriteUInt16((ushort)ID);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Track = reader.ReadByte("Track");
            Type = reader.ReadByte("Type");
            ID = reader.ReadEnum<Rsc6BoneIdEnum>("ID");
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteByte("Track", Track);
            writer.WriteByte("Type", Type);
            writer.WriteEnum("ID", ID);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpSet : Rsc6ExpressionOpGetSet, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 4;
        public Rsc6Ptr<Rsc6ExpressionOp> Source { get; set; } //m_Sources

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Source = reader.ReadPtr(Create);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Source);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Source = new(reader.ReadNode("Source", Create));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNode("Source", Source.Item);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpFastBlendVector : Rsc6ExpressionOp, MetaNode //rage::crExpressionOpFastBlendVector
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6ManagedArr<Rsc6ExpressionOpFastBlendVectorBlock> SourceWeights { get; set; } //m_SourceWeights

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            SourceWeights = reader.ReadArr<Rsc6ExpressionOpFastBlendVectorBlock>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteArr(SourceWeights);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            SourceWeights = new(reader.ReadNodeArray<Rsc6ExpressionOpFastBlendVectorBlock>("SourceWeights"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNodeArray("SourceWeights", SourceWeights.Items);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpFastBlendVectorBlock : Rsc6BlockBase, MetaNode //rage::crExpressionOpFastBlendVector::SourceWeight
    {
        public override ulong BlockLength => 64;
        public Vector4 Weight { get; set; } //m_Weight
        public Vector4 Add { get; set; } //m_Add
        public Vector4 Mult { get; set; } //m_Mult
        public int AcceleratorID { get; set; } //m_AcceleratorIdx
        public int Component { get; set; } //m_Component
        public ushort ID { get; set; } //m_Id
        public byte Track { get; set; } //m_Track
        public byte Type { get; set; } //m_Type
        public uint Unknown_3Ch { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            Weight = reader.ReadVector4();
            Add = reader.ReadVector4();
            Mult = reader.ReadVector4();
            AcceleratorID = reader.ReadInt32();
            Component = reader.ReadInt32();
            ID = reader.ReadUInt16();
            Track = reader.ReadByte();
            Type = reader.ReadByte();
            Unknown_3Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(Weight);
            writer.WriteVector4(Add);
            writer.WriteVector4(Mult);
            writer.WriteInt32(AcceleratorID);
            writer.WriteInt32(Component);
            writer.WriteUInt16(ID);
            writer.WriteByte(Track);
            writer.WriteByte(Type);
            writer.WriteUInt32(Unknown_3Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Weight = Rpf6Crypto.ToXYZ(reader.ReadVector4("Weight"));
            Add = Rpf6Crypto.ToXYZ(reader.ReadVector4("Add"));
            Mult = Rpf6Crypto.ToXYZ(reader.ReadVector4("Mult"));
            AcceleratorID = reader.ReadInt32("AcceleratorID");
            Component = reader.ReadInt32("Component");
            ID = reader.ReadUInt16("ID");
            Track = reader.ReadByte("Track");
            Type = reader.ReadByte("Type");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector4("Weight", Weight);
            writer.WriteVector4("Add", Add);
            writer.WriteVector4("Mult", Mult);
            writer.WriteInt32("AcceleratorID", AcceleratorID);
            writer.WriteInt32("Component", Component);
            writer.WriteUInt16("ID", ID);
            writer.WriteByte("Track", Track);
            writer.WriteByte("Type", Type);
        }

        public override string ToString()
        {
            return Weight.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpNullary : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public uint Padding1 { get; set; } = 0xCDCDCDCD;
        public uint Padding2 { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Padding1 = reader.ReadUInt32();
            Padding2 = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32(Padding1);
            writer.WriteUInt32(Padding2);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
        }

        public enum Rsc6NullaryOperationTypes : ushort
        {
            NONE,
            ZERO,
            ONE,
            PI,
            TIME,
            RANDOM,
            VECTOR_ZERO,
            VECTOR_ONE,
            DELTA_TIME,
            QUATERNION_IDENTITY
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpUnary : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6Ptr<Rsc6ExpressionOp> Source { get; set; } //m_Source
        public uint Padding { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Source = reader.ReadPtr(Create);
            Padding = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Source);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Source = new(reader.ReadNode("Source", Create));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNode("Source", Source.Item);
        }

        public enum Rsc6UnaryOperationTypes : ushort
        {
            NONE,
            LOGICAL_NOT,
            NEGATE,
            RECIPROCAL,
            SQUARE,
            SQUARE_ROOT,
            ABSOLUTE,
            FLOOR,
            CEIL,
            LOG,
            LN,
            EXP,
            CLAMP01,
            COS,
            SIN,
            TAN,
            ARC_COS,
            ARC_SIN,
            ARC_TAN,
            COS_H,
            SIN_H,
            TAN_H,
            DEGREES_TO_RADIANS,
            RADIANS_TO_DEGREES,
            FROM_EULER,
            TO_EULER,
            SPLAT,
            VECTOR_CLAMP01,
            QUATERNION_INVERSE
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpBinary : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6Ptr<Rsc6ExpressionOp> Source1 { get; set; } //m_Sources[0]
        public Rsc6Ptr<Rsc6ExpressionOp> Source2 { get; set; } //m_Sources[1]

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Source1 = reader.ReadPtr(Create);
            Source2 = reader.ReadPtr(Create);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Source1);
            writer.WritePtr(Source2);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Source1 = new(reader.ReadNode("Source1", Create));
            Source2 = new(reader.ReadNode("Source2", Create));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNode("Source1", Source1.Item);
            writer.WriteNode("Source2", Source2.Item);
        }

        public enum Rsc6BinaryOperationTypes : ushort
        {
            NONE,
            EQUAL_TO,
            NOT_EQUAL_TO,
            GREATER_THAN,
            LESS_THAN,
            GREATER_THAN_EQUAL_TO,
            LESS_THAN_EQUAL_TO,
            ADD,
            SUBSTRACT,
            MULTIPLY,
            DIVIDE,
            MODULUS,
            EXPONENT,
            MAX,
            MIN,
            LOGICAL_AND,
            LOGICAL_OR,
            LOGICAL_XOR,
            QUATERNION_MULTIPLY,
            VECTOR_ADD,
            VECTOR_MULTIPLY,
            VECTOR_TRANSFORM,
            VECTOR_SUBSTRACT,
            QUATERNION_SCALE
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpTernary : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 12;
        public Rsc6Ptr<Rsc6ExpressionOp> Source1 { get; set; } //m_Sources[0]
        public Rsc6Ptr<Rsc6ExpressionOp> Source2 { get; set; } //m_Sources[1]
        public Rsc6Ptr<Rsc6ExpressionOp> Source3 { get; set; } //m_Sources[2]

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Source1 = reader.ReadPtr(Create);
            Source2 = reader.ReadPtr(Create);
            Source3 = reader.ReadPtr(Create);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Source1);
            writer.WritePtr(Source2);
            writer.WritePtr(Source3);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Source1 = new(reader.ReadNode("Source1", Create));
            Source2 = new(reader.ReadNode("Source2", Create));
            Source3 = new(reader.ReadNode("Source3", Create));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNode("Source1", Source1.Item);
            writer.WriteNode("Source2", Source2.Item);
            writer.WriteNode("Source3", Source3.Item);
        }

        public enum Rsc6TernaryOperationTypes : ushort
        {
            NONE,
            MULTIPLY_ADD,
            CLAMP,
            LERP,
            TO_QUATERNION,
            TO_VECTOR,
            CONDITIONAL,
            VECTOR_MULTIPLY_ADD,
            VECTOR_LERP_VECTOR,
            QUATERNION_LERP
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpNary : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 8;
        public Rsc6PtrArr<Rsc6ExpressionOp> Sources { get; set; } //m_Sources

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Sources = reader.ReadPtrArr(Create);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtrArr(Sources);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Sources = new(reader.ReadNodeArray("Sources", Create));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNodeArray("Sources", Sources.Items);
        }

        public enum Rsc6NaryOperationTypes : ushort
        {
            NONE,
            COMMA,
            SUM,
            LIST,
            LOGICAL_AND,
            LOGICAL_OR
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6ExpressionOpSpecialCurve : Rsc6ExpressionOp, MetaNode
    {
        public override ulong BlockLength => base.BlockLength + 12;
        public Rsc6Ptr<Rsc6ExpressionOp> Source { get; set; } //m_Source
        public Rsc6Arr<Vector2> Keys { get; set; } //m_Keys (m_In + m_Out)

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Source = reader.ReadPtr(Create);
            Keys = reader.ReadArr<Vector2>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Source);
            writer.WriteArr(Keys);
        }

        public new void Read(MetaNodeReader reader)
        {
            base.Read(reader);
            Source = new(reader.ReadNode("Source", Create));
            Keys = new(reader.ReadVector2Array("Keys"));
        }

        public new void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            writer.WriteNode("Source", Source.Item);
            writer.WriteVector2Array("Keys", Keys.Items);
        }
    }

    public enum Rsc6OperationTypes : ushort
    {
        NONE, //0
        CONSTANT, //1
        CONSTANT_FLOAT, //2
        GET, //3
        SET, //4
        VALID, //5
        COMPONENT_GET, //6
        COMPONENT_SET, //7
        OBJECT_SPACE_GET, //8
        OBJECT_SPACE_SET, //9
        OBJECT_SPACE_CONVERT_TO, //10
        OBJECT_SPACE_CONVERT_FROM, //11
        NULLARY, //12
        UNARY, //13
        BINARY, //14
        TERNARY, //15
        NARY, //16
        SPECIAL_BLEND, //17
        DEPRECATED, //18
        SPECIAL_CURVE, //19
        SPECIAL_LOOK_AT, //20
        FAST_BLEND_VECTOR, //21
        VARIABLE_GET, //22
        VARIABLE_SET, //23
        SPECIAL_LINEAR //24
    };
}