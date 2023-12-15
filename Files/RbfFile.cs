using System;
using System.Collections.Generic;
using System.Text;
using CodeX.Core.Engine;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using CodeX.Core.Utilities;
using System.IO;
using System.Numerics;

namespace CodeX.Games.RDR1.Files
{
    [TC(typeof(EXP))]
    public class RbfFile : DataBagPack
    {
        private const int RBF_IDENT = 0x30464252;
        public RbfStructure Current { get; set; }
        public Stack<RbfStructure> Stack { get; set; }
        public List<RbfEntryDescription> Descriptors { get; set; }
        public Dictionary<string, int> OutDescriptors { get; private set; } = new Dictionary<string, int>();
        public DataSchema Schema { get; set; }

        public override void Load(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                Load(ms);
            }
        }

        public RbfStructure Load(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                return Load(fileStream);
            }
        }

        public RbfStructure Load(Stream stream)
        {
            Stack = new Stack<RbfStructure>();
            Descriptors = new List<RbfEntryDescription>();

            var reader = new DataReader(stream);
            var ident = reader.ReadInt32();

            if (ident != RBF_IDENT)
                throw new Exception("The file identifier does not match.");

            while (reader.Position < reader.Length)
            {
                var descriptorIndex = reader.ReadByte();
                if (descriptorIndex == 0xFF) // close tag
                {
                    var b = reader.ReadByte();
                    if (b != 0xFF)
                        throw new Exception("Expected 0xFF but was " + b.ToString("X2"));

                    if (Stack.Count > 0)
                    {
                        Current = Stack.Pop();
                    }
                    else
                    {
                        if (reader.Position != reader.Length)
                            throw new Exception("Expected end of stream but was not.");

                        Bag = CreateDataBag();
                        return Current;
                    }
                }
                else if (descriptorIndex == 0xFD) // bytes
                {
                    var b = reader.ReadByte();
                    if (b != 0xFF)
                        throw new Exception("Expected 0xFF but was " + b.ToString("X2"));

                    var dataLength = reader.ReadInt32();
                    if (dataLength == 0)
                        continue;

                    var data = reader.ReadBytes(dataLength);
                    var datatype = "string";
                    var contentattr = Current.FindAttribute("content") as RbfString;
                    if (contentattr != null)
                    {
                        datatype = contentattr.Value;
                    }

                    switch (datatype)
                    {
                        default:
                        case "char_array": //just a byte array
                            var bytesValue = new RbfBytes();
                            bytesValue.Value = data;
                            Current.Children.Add(bytesValue);
                            Current.Content = bytesValue;
                            break;
                        case "short_array": //convert byte array to short array
                            var scnt = dataLength / 2;
                            var shrvalue = new RbfBytesShortArray();
                            shrvalue.Value = BufferUtil.ReadArray<short>(data, 0, scnt);
                            Current.Children.Add(shrvalue);
                            Current.Content = shrvalue;
                            break;
                        case "string": //byte array is a string
                            var strvalue = new RbfBytesString();
                            strvalue.Value = Encoding.ASCII.GetString(data, 0, data.Length);
                            Current.Children.Add(strvalue);
                            Current.Content = strvalue;
                            break;
                    }
                }
                else
                {
                    var dataType = (RbfDataType)reader.ReadByte();
                    if (descriptorIndex == Descriptors.Count) // new descriptor + data
                    {
                        var nameLength = reader.ReadInt16();
                        var nameBytes = reader.ReadBytes(nameLength);
                        var name = Encoding.ASCII.GetString(nameBytes);

                        var descriptor = new RbfEntryDescription();
                        descriptor.Name = name;
                        descriptor.Type = dataType;
                        Descriptors.Add(descriptor);

                        ParseElement(reader, Descriptors.Count - 1, dataType);
                    }
                    else // existing descriptor + data
                    {
                        if (dataType != Descriptors[descriptorIndex].Type)
                        {
                            //throw new Exception("Data type does not match. Expected "
                            //    + descriptors[descriptorIndex].Type.ToString() + " but found "
                            //    + dataType.ToString() + ". Descriptor: " + descriptors[descriptorIndex].Name);
                        }
                        ParseElement(reader, descriptorIndex, dataType);
                    }
                }
            }
            throw new Exception("Unexpected end of stream.");
        }

        private void ParseElement(DataReader reader, int descriptorIndex, RbfDataType dataType)
        {
            var descriptor = Descriptors[descriptorIndex];
            switch (dataType)
            {
                case RbfDataType.None: // open element...
                    {
                        var structureValue = new RbfStructure();
                        structureValue.Name = descriptor.Name;

                        if (Current != null)
                        {
                            Current.AddChild(structureValue);
                            Stack.Push(Current);
                        }
                        Current = structureValue;

                        var x1 = reader.ReadInt16();
                        var x2 = reader.ReadInt16();
                        Current.PendingAttributes = reader.ReadInt16();
                        break;
                    }
                case RbfDataType.UInt32:
                    {
                        var intValue = new RbfUint32();
                        intValue.Name = descriptor.Name;
                        intValue.Value = reader.ReadUInt32();
                        Current.AddChild(intValue);
                        break;
                    }
                case RbfDataType.BoolTrue:
                    {
                        var booleanValue = new RbfBoolean();
                        booleanValue.Name = descriptor.Name;
                        booleanValue.Value = true;
                        Current.AddChild(booleanValue);
                        break;
                    }
                case RbfDataType.BoolFalse:
                    {
                        var booleanValue = new RbfBoolean();
                        booleanValue.Name = descriptor.Name;
                        booleanValue.Value = false;
                        Current.AddChild(booleanValue);
                        break;
                    }
                case RbfDataType.Float:
                    {
                        var floatValue = new RbfFloat();
                        floatValue.Name = descriptor.Name;
                        floatValue.Value = reader.ReadSingle();
                        Current.AddChild(floatValue);
                        break;
                    }
                case RbfDataType.Float3:
                    {
                        var floatVectorValue = new RbfFloat3();
                        floatVectorValue.Name = descriptor.Name;
                        floatVectorValue.X = reader.ReadSingle();
                        floatVectorValue.Y = reader.ReadSingle();
                        floatVectorValue.Z = reader.ReadSingle();
                        Current.AddChild(floatVectorValue);
                        break;
                    }
                case RbfDataType.String:
                    {
                        var valueLength = reader.ReadInt16();
                        var valueBytes = reader.ReadBytes(valueLength);
                        var value = Encoding.ASCII.GetString(valueBytes);
                        var stringValue = new RbfString();
                        stringValue.Name = descriptor.Name;
                        stringValue.Value = value;
                        Current.AddChild(stringValue);
                        break;
                    }
                default:
                    throw new Exception("Unsupported data type.");
            }
        }

        public byte GetDescriptorIndex(IRbfType t, out bool isNew)
        {
            var key = t.Name;// $"{t.Name}_{t.DataType}";
            isNew = false;

            if (!OutDescriptors.TryGetValue(key, out var idx))
            {
                idx = OutDescriptors.Count;
                OutDescriptors.Add(key, idx);
                isNew = true;
            }
            return (byte)idx;
        }

        private DataBag2 CreateDataBag()
        {
            Schema = new DataSchema();
            var bag = CreateDataBag(Current);
            return bag;
        }

        private DataBag2 CreateDataBag(RbfStructure structure)
        {
            if (structure == null)
                return null;

            var cls = CreateSchemaClass(structure);
            if (cls == null)
                return null;

            var bag = new DataBag2(cls);
            var allitems = new List<IRbfType>();

            allitems.AddRange(structure.Attributes);
            allitems.AddRange(structure.Children);

            foreach (var item in allitems)
            {
                JenkIndex.Ensure(item.Name, "RDR1");
                JenkHash f = JenkHash.GenHash(item.Name);

                var fld = cls.GetField(f);
                if (fld == null)
                    continue;

                var cstruc = item as RbfStructure;
                switch (fld.DataType)
                {
                    case DataBagValueType.Array:
                        var alen = cstruc?.Children.Count ?? 0;
                        if (alen > 0)
                        {
                            switch (fld.ArrayType)
                            {
                                case DataBagValueType.UInt8: //byte array
                                    bag.SetObject(f, (cstruc.Content as RbfBytes)?.Value);
                                    break;
                                case DataBagValueType.Int16: //short array
                                    bag.SetObject(f, (cstruc.Content as RbfBytesShortArray)?.Value);
                                    break;
                                case DataBagValueType.Object:
                                    var arr = new DataBag2[alen];
                                    for (int i = 0; i < alen; i++)
                                    {
                                        arr[i] = CreateDataBag(cstruc.Children[i] as RbfStructure);
                                    }
                                    bag.SetObject(f, arr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;

                    case DataBagValueType.String:
                        if (cstruc?.Content is RbfBytesString cstr)
                            bag.SetString(f, cstr.Value);
                        else if (item is RbfString istr)
                            bag.SetString(f, istr.Value);
                        break;
                    case DataBagValueType.UInt32:
                        if (item is RbfUint32 cuint)
                            bag.SetUInt32(f, cuint.Value);
                        break;
                    case DataBagValueType.Float:
                        if (item is RbfFloat cf)
                            bag.SetFloat(f, cf.Value);
                        break;
                    case DataBagValueType.Float3:
                        if (item is RbfFloat3 cf3)
                            bag.SetFloat3(f, new Vector3(cf3.X, cf3.Y, cf3.Z));
                        break;

                    case DataBagValueType.None:
                    case DataBagValueType.Object:
                        if (cstruc != null)
                        {
                            if ((cstruc.Children.Count > 0) || (cstruc.Attributes.Count > 0))
                            {
                                bag.SetObject(f, CreateDataBag(cstruc));
                            }
                        }
                        break;

                    case DataBagValueType.Boolean:
                        if (item is RbfBoolean cb)
                            bag.SetBoolean(f, cb.Value);
                        break;
                    default:
                        break;
                }
            }
            return bag;
        }

        private DataSchemaClass CreateSchemaClass(RbfStructure structure)
        {
            if (structure == null) return null;

            var cls = new DataSchemaClass()
            {
                Name = JenkHash.GenHash(structure.Name)
            };

            var allitems = new List<IRbfType>();
            allitems.AddRange(structure.Attributes);
            allitems.AddRange(structure.Children);

            var allfields = new List<DataSchemaField>();
            uint slen = 0;
            uint icnt = 0;

            foreach (var item in allitems)
            {
                var et = GetEngineType(item.DataType);
                var fl = DataBagValueTypes.GetLength(et);

                var fld = new DataSchemaField();
                fld.Name = JenkHash.GenHash(item.Name);
                fld.DataType = et;
                fld.IsAttribute = (icnt < structure.Attributes.Count);
                fld.Offset = (int)slen;
                allfields.Add(fld);
                slen += fl;
                icnt++;

                if (item.DataType == RbfDataType.None)
                {
                    var cstruc = item as RbfStructure;
                    var isarray = false;
                    if ((cstruc != null) && (cstruc.Children.Count > 0))
                    {
                        var ccname = cstruc.Children[0].Name;
                        isarray = (ccname != null) && (cstruc.Content == null);
                        for (int i = 1; i < cstruc.Children.Count; i++)
                        {
                            if (ccname != cstruc.Children[i].Name)
                            {
                                isarray = false;
                                break;
                            }
                        }
                        var isitemarray = (ccname == "Item") || (ccname == "item");
                        isarray = isarray || isitemarray;
                        if (cstruc.Children.Count == 1)
                        {
                            isarray = isitemarray;
                        }

                        if (isarray && !isitemarray)
                        {
                            fld.TypeName = JenkHash.GenHash(ccname);
                        }

                    }
                    if (isarray)
                    {
                        fld.DataType = DataBagValueType.Array;
                        fld.ArrayType = DataBagValueType.Object;
                    }
                    else
                    {
                        if (cstruc?.Content != null)
                        {
                            if (cstruc.Children.Count != 1)
                            { }
                            if (cstruc.Content is RbfBytesString)
                            {
                                fld.DataType = DataBagValueType.String;
                                fld.ArrayType = DataBagValueType.UInt8;
                            }
                            else if (cstruc.Content is RbfBytesShortArray)
                            {
                                fld.DataType = DataBagValueType.Array;
                                fld.ArrayType = DataBagValueType.Int16;
                            }
                            else
                            {
                                fld.DataType = DataBagValueType.Array;
                                fld.ArrayType = DataBagValueType.UInt8;
                            }
                        }
                        else if (cstruc != null)
                        {
                            fld.DataType = DataBagValueType.Object;
                        }
                    }
                }
            }
            cls.DataSize = slen;
            cls.Fields = allfields.ToArray();
            cls.BuildFieldsLookup(Schema);
            return cls;
        }

        public override byte[] Save()
        {
            var ms = new MemoryStream();
            Save(ms);

            var buf = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buf, 0, buf.Length);

            return buf;
        }

        public void Save(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                Save(fileStream);
            }
        }

        public void Save(Stream stream)
        {
            OutDescriptors = new Dictionary<string, int>();

            var writer = new DataWriter(stream);
            writer.Write(RBF_IDENT);

            Current.Save(this, writer);
        }

        public void WriteRecordId(IRbfType type, DataWriter writer)
        {
            writer.Write(GetDescriptorIndex(type, out var isNew));
            writer.Write((byte)type.DataType);

            if (isNew)
            {
                writer.Write((ushort)type.Name.Length);
                writer.Write(Encoding.ASCII.GetBytes(type.Name));
            }
        }

        public DataBagValueType GetEngineType(RbfDataType t)
        {
            switch (t)
            {
                default:
                case RbfDataType.None: return DataBagValueType.None;
                case RbfDataType.UInt32: return DataBagValueType.UInt32;
                case RbfDataType.BoolTrue: return DataBagValueType.Boolean;
                case RbfDataType.BoolFalse: return DataBagValueType.Boolean;
                case RbfDataType.Float: return DataBagValueType.Float;
                case RbfDataType.Float3: return DataBagValueType.Float3;
                case RbfDataType.String: return DataBagValueType.String;
            }
        }

    }

    public enum RbfDataType : byte
    {
        None = 0,
        UInt32 = 0x10,
        BoolTrue = 0x20,
        BoolFalse = 0x30,
        Float = 0x40,
        Float3 = 0x50,
        String = 0x60,
    }

    [TC(typeof(EXP))]
    public class RbfEntryDescription
    {
        public string Name { get; set; }
        public RbfDataType Type { get; set; }
        public override string ToString() { return Name + ": " + Type.ToString(); }
    }

    [TC(typeof(EXP))]
    public interface IRbfType
    {
        string Name { get; set; }
        RbfDataType DataType { get; }
        void Save(RbfFile file, DataWriter writer);
    }

    [TC(typeof(EXP))]
    public class RbfBytes : IRbfType
    {
        public string Name { get; set; }
        public byte[] Value { get; set; }
        public RbfDataType DataType => RbfDataType.None;

        public void Save(RbfFile root, DataWriter writer)
        {
            writer.Write((byte)0xFD);
            writer.Write((byte)0xFF);
            writer.Write(Value.Length);
            writer.Write(Value);
        }

        public override string ToString()
        {
            return "byte[" + Value.Length.ToString() + "]";
        }
    }

    [TC(typeof(EXP))]
    public class RbfBytesString : IRbfType
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public RbfDataType DataType => RbfDataType.None;

        public void Save(RbfFile root, DataWriter writer)
        {
            writer.Write((byte)0xFD);
            writer.Write((byte)0xFF);
            writer.Write(Value.Length + 1);
            writer.Write(Value, true);
        }

        public override string ToString()
        {
            return Value;
        }
    }

    [TC(typeof(EXP))]
    public class RbfBytesShortArray : IRbfType
    {
        public string Name { get; set; }
        public short[] Value { get; set; }
        public RbfDataType DataType => RbfDataType.None;

        public void Save(RbfFile root, DataWriter writer)
        {
            writer.Write((byte)0xFD);
            writer.Write((byte)0xFF);
            writer.Write(Value.Length * 2);
            writer.Write(BufferUtil.GetByteArray(Value));
        }

        public override string ToString()
        {
            return "short[" + Value.Length.ToString() + "]";
        }
    }

    [TC(typeof(EXP))]
    public class RbfUint32 : IRbfType
    {
        public string Name { get; set; }
        public uint Value { get; set; }
        public RbfDataType DataType => RbfDataType.UInt32;

        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
            writer.Write(Value);
        }

        public override string ToString()
        {
            return Name + ": " + Value.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class RbfBoolean : IRbfType
    {
        public string Name { get; set; }
        public bool Value { get; set; }
        public RbfDataType DataType => ((Value) ? RbfDataType.BoolTrue : RbfDataType.BoolFalse);

        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
        }
        public override string ToString()
        {
            return Name + ": " + Value.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class RbfFloat : IRbfType
    {
        public string Name { get; set; }
        public float Value { get; set; }
        public RbfDataType DataType => RbfDataType.Float;

        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
            writer.Write(Value);
        }

        public override string ToString()
        {
            return Name + ": " + Value.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class RbfFloat3 : IRbfType
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public RbfDataType DataType => RbfDataType.Float3;

        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }

        public override string ToString()
        {
            return string.Format("{0}: X:{1}, Y:{2}, Z:{3}", Name, X, Y, Z);
        }
    }

    [TC(typeof(EXP))]
    public class RbfString : IRbfType
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public RbfDataType DataType => RbfDataType.String;

        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
            writer.Write((short)Value.Length);
            writer.Write(Encoding.ASCII.GetBytes(Value));
        }

        public override string ToString()
        {
            return Name + ": " + Value.ToString();
        }
    }

    [TC(typeof(EXP))]
    public class RbfStructure : IRbfType
    {
        public string Name { get; set; }
        public List<IRbfType> Children { get; set; } = new List<IRbfType>();
        public List<IRbfType> Attributes { get; set; } = new List<IRbfType>();
        public object Content { get; set; } //single content child node
        internal int PendingAttributes { get; set; }
        public RbfDataType DataType => RbfDataType.None;

        public IRbfType FindChild(string name)
        {
            foreach (var child in Children)
            {
                if (child == null) continue;
                if (child.Name == name) return child;
            }
            return null;
        }

        public IRbfType FindAttribute(string name)
        {
            foreach (var attr in Attributes)
            {
                if (attr == null)
                    continue;
                if (attr.Name == name)
                    return attr;
            }
            return null;
        }

        public void Save(RbfFile root, DataWriter writer)
        {
            root.WriteRecordId(this, writer);
            writer.Write(new byte[4]); // 00

            // count of non-primitive fields in this (... attributes??)
            writer.Write((short)Attributes.Count);   //writer.Write((short)Children.TakeWhile(a => !(a is RbfBytes || a is RbfStructure)).Count());
            foreach (var attr in Attributes)
            {
                attr.Save(root, writer);
            }

            foreach (var child in Children)
            {
                child.Save(root, writer);
            }
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
        }

        internal void AddChild(IRbfType value)
        {
            if (PendingAttributes > 0)
            {
                PendingAttributes--;
                Attributes.Add(value);
            }
            else
            {
                Children.Add(value);
            }
        }

        public override string ToString()
        {
            if (Content != null)
            {
                return Name + ": " + Content.ToString();
            }
            return Name + ": {" + Children.Count.ToString() + "}";
        }
    }
}
