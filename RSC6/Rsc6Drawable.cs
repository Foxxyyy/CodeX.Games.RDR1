using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Shaders;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))]
    public class Rsc6VisualDictionary<T> : Rsc6FileBase where T : Rsc6DrawableBase, new()
    {
        public override ulong BlockLength => 60;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public uint Unknown_8h { get; set; } = 0x01876DE0; //0x01876DE0 (buildings & props) or 0x04A744AC (tiles)
        public uint Unknown_Ch { get; set; } //Always 0
        public uint ParentDictionary { get; set; } //Always 0
        public uint UsageCount { get; set; } = 1; //Always 1
        public Rsc6Arr<JenkHash> Hashes { get; set; }
        public Rsc6PtrArr<Rsc6Drawable> Drawables { get; set; } //m_Drawables
        public Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary { get; set; } //m_Textures
        public Rsc6Ptr<Rsc6TextureDictionary> DerivedTextures { get; set; } //m_DerivedTextures, unused
        public uint Unknown_30h { get; set; } //Always 0
        public uint LODLevel { get; set; } //0 for everything, 1 for '_med' or 3 for '_vlow'
        public uint Unknown_38h { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();

            //Loading textures before drawables so we can bind them properly
            reader.Position += 32;
            TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>(); //ReadAhead<rage::datOwner<rage::pgDictionary<rage::grcTexture>>>
            DerivedTextures = reader.ReadPtr<Rsc6TextureDictionary>(); //ReadAhead<rage::datOwner<rage::pgDictionary<rage::grcTexture>>>
            Unknown_30h = reader.ReadUInt32();
            LODLevel = reader.ReadUInt32();
            Unknown_38h = reader.ReadUInt32();
            WfdFile.TextureDictionary = TextureDictionary;
            reader.Position -= 52;

            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            ParentDictionary = reader.ReadUInt32();
            UsageCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Drawables = reader.ReadPtrArr<Rsc6Drawable>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            var textures = new List<Rsc6Texture>();
            var hashes = new List<JenkHash>();

            foreach (var item in Drawables.Items)
            {
                foreach (var tex in item.ShaderGroup.Item.TextureDictionary.Item.Textures.Items)
                {
                    textures.Add(tex);
                    hashes.Add(JenkHash.GenHash(tex.NameRef.Value.Replace(".dds", ""))); //Hashes don't have the extension
                }
            }

            var dict = new Rsc6TextureDictionary
            {
                Textures = new Rsc6PtrArr<Rsc6Texture>(textures.ToArray()),
                Hashes = new Rsc6Arr<JenkHash>(hashes.ToArray())
            };
            TextureDictionary = new Rsc6Ptr<Rsc6TextureDictionary>(dict);

            writer.WriteUInt32(0x01908FF8);
            writer.WritePtr(BlockMap);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteUInt32(ParentDictionary);
            writer.WriteUInt32(UsageCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Drawables);
            writer.WritePtr(TextureDictionary);
            writer.WritePtr(DerivedTextures);
            writer.WriteUInt32(Unknown_30h);
            writer.WriteUInt32(LODLevel);
            writer.WriteUInt32(Unknown_38h);
        }

        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var drawables = new List<Rsc6Drawable>();
            var hashes = new List<JenkHash>();

            var drawable = new Rsc6Drawable();
            drawable.ReadXml(node, ddsfolder);
            drawables.Add(drawable);
            hashes.Add(JenkHash.GenHash(drawable.Name));

            Hashes = new Rsc6Arr<JenkHash>(hashes.ToArray());
            Drawables = new Rsc6PtrArr<Rsc6Drawable>(drawables.ToArray());
        }

        public void WriteXml(StringBuilder sb, int indent, string ddsFolder)
        {
            if (Drawables.Items != null)
            {
                for (int i = 0; i < Drawables.Items.Length; i++)
                {
                    var d = Drawables.Items[i];
                    d.WriteXml(sb, indent + 1, ddsFolder);
                }
            }
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6FragDrawable<T> : Rsc6FileBase where T : Rsc6DrawableBase, new()
    {
        public override ulong BlockLength => 16;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary { get; set; } //ReadAhead<rage::datOwner<rage::pgDictionary<rage::grcTexture>>>
        public Rsc6Ptr<Rsc6Drawable> Drawables { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();

            //Loading textures before everything else so we can assign them properly
            reader.Position += 4;
            TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>();
            WfdFile.TextureDictionary = TextureDictionary;
            reader.Position -= 8;
            Drawables = reader.ReadPtr<Rsc6Drawable>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            var textures = new List<Rsc6Texture>();
            var hashes = new List<JenkHash>();

            foreach (var tex in Drawables.Item?.ShaderGroup.Item?.TextureDictionary.Item?.Textures.Items)
            {
                textures.Add(tex);
                hashes.Add(JenkHash.GenHash(tex.NameRef.Value.Replace(".dds", ""))); //Hashes don't have the extension
            }
            

            var dict = new Rsc6TextureDictionary
            {
                Textures = new Rsc6PtrArr<Rsc6Texture>(textures.ToArray()),
                Hashes = new Rsc6Arr<JenkHash>(hashes.ToArray())
            };
            TextureDictionary = new Rsc6Ptr<Rsc6TextureDictionary>(dict);

            writer.WriteUInt32(0x00DDC0A0);
            writer.WritePtr(BlockMap);
            writer.WritePtr(Drawables);
            writer.WritePtr(TextureDictionary);
        }

        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var drawable = new Rsc6Drawable();
            drawable.ReadXml(node, ddsfolder);
            Drawables = new Rsc6Ptr<Rsc6Drawable>(drawable);
        }

        public void WriteXml(StringBuilder sb, int indent, string ddsFolder)
        {
            Drawables.Item?.WriteXml(sb, indent + 1, ddsFolder);
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6Drawable : Rsc6DrawableBase //rage::rmcDrawable
    {
        public override ulong BlockLength => 208;

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Name = reader.FileEntry.Name;
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
        }

        public new void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            base.ReadXml(node, ddsfolder);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6DrawableLod : PieceLod, Rsc6Block
    {
        public ulong BlockLength => 8;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6PtrArr<Rsc6DrawableModel> ModelsData { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            ModelsData = reader.ReadPtrArr<Rsc6DrawableModel>();
            Models = ModelsData.Items;
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WritePtrArr(ModelsData);
        }

        public void ReadXml(XmlNode node)
        {
            if (node != null)
            {
                var iNodes = node.SelectNodes("Item");
                if (iNodes?.Count > 0)
                {
                    var listDrawable = new List<Rsc6DrawableModel>();
                    foreach (XmlNode iNode in iNodes)
                    {
                        var drawable = new Rsc6DrawableModel();
                        drawable.ReadXml(iNode);
                        listDrawable.Add(drawable);
                    }
                    ModelsData = new Rsc6PtrArr<Rsc6DrawableModel>(listDrawable.ToArray());
                    Models = ModelsData.Items;
                }
            }
        }

        public void WriteXml(StringBuilder sb, int indent, Vector3 center)
        {
            foreach (var m in ModelsData.Items)
            {
                Xml.OpenTag(sb, indent, "Item");
                m.WriteXml(sb, indent + 1, center);
                Xml.CloseTag(sb, indent, "Item");
            }
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6DrawableGeometry : Mesh, Rsc6Block
    {
        public ulong BlockLength => 80;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public uint VFT { get; set; } = 0x018572A0;
        public uint Unknown_4h { get; set; }
        public uint Unknown_8h { get; set; }
        public Rsc6Ptr<Rsc6VertexBuffer> VertexBuffer { get; set; } //m_VB[4] - rage::grcVertexBuffer
        public uint VertexBuffer2 { get; set; }
        public uint VertexBuffer3 { get; set; }
        public uint VertexBuffer4 { get; set; }
        public Rsc6Ptr<Rsc6IndexBuffer> IndexBuffer { get; set; } //m_IB[4] - rage::grcIndexBuffer
        public uint IndexBuffer2 { get; set; }
        public uint IndexBuffer3 { get; set; }
        public uint IndexBuffer4 { get; set; }
        public uint IndicesCount { get; set; } //m_IndexCount
        public uint TrianglesCount { get; set; } //m_PrimCount
        public byte PrimitiveType { get; set; } = 3; //m_PrimType, indices per primitive (triangle)
        public bool DoubleBuffered { get; set; } //m_DoubleBuffered
        public Rsc6RawArr<ushort> BoneIds { get; set; } //m_MtxPalette, data is embedded at the end of this struct (m_boneMapping)
        public ushort BoneIdsCount { get; set; } //m_MtxCount
        public Rsc6RawArr<byte> VertexDataRef { get; set; } //m_VtxDeclOffset, always 0xCDCDCDCD
        public uint OffsetBuffer { get; set; } //m_OffsetBuffer
        public uint IndexOffset { get; set; } //m_IndexOffset
        public uint Unknown_3Ch { get; set; }

        public Rsc6ShaderFX ShaderRef { get; set; } //Written by parent DrawableBase, using ShaderID
        public ushort ShaderID { get; set; } //Read-written by parent model
        public BoundingBox4 AABB { get; set; } //Read-written by parent model

        public Rsc6DrawableGeometry()
        {
        }

        public void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32(); //rage::grmGeometry
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt32();
            VertexBuffer = reader.ReadPtr<Rsc6VertexBuffer>();
            VertexBuffer2 = reader.ReadUInt32();
            VertexBuffer3 = reader.ReadUInt32();
            VertexBuffer4 = reader.ReadUInt32();
            IndexBuffer = reader.ReadPtr<Rsc6IndexBuffer>();
            IndexBuffer2 = reader.ReadUInt32();
            IndexBuffer3 = reader.ReadUInt32();
            IndexBuffer4 = reader.ReadUInt32();
            IndicesCount = reader.ReadUInt32();
            TrianglesCount = reader.ReadUInt32();
            VertexCount = reader.ReadUInt16();
            PrimitiveType = reader.ReadByte();
            DoubleBuffered = reader.ReadBoolean();
            BoneIds = reader.ReadRawArrPtr<ushort>();
            VertexStride = reader.ReadUInt16();
            BoneIdsCount = reader.ReadUInt16();
            VertexDataRef = reader.ReadRawArrPtr<byte>();
            OffsetBuffer = reader.ReadUInt32();
            IndexOffset = reader.ReadUInt32();
            Unknown_3Ch = reader.ReadUInt32();

            if (VertexBuffer.Item != null) //hack to fix stupid "locked" things
            {
                VertexLayout = VertexBuffer.Item?.Layout.Item?.VertexLayout;
                VertexData = VertexBuffer.Item.LockedData.Items ?? VertexBuffer.Item.Data2.Items;

                if (VertexCount == 0)
                {
                    VertexCount = VertexBuffer.Item.VertexCount;
                }
            }

            //Swap RDR axis
            byte[] numArray = VertexData;
            var elems = VertexLayout.Elements;
            var elemcount = elems.Length;
            var uvX = new List<float>();
            var uvX1 = new List<float>();
            var uvY = new List<float>();
            var uvY1 = new List<float>();
            var uvOffset = new List<int>();
            var uvOffset1 = new List<int>();
            var highLOD = reader.FileEntry.EntryParent.Name == "resource_0";
            var terrainMesh = reader.FileEntry.Name.StartsWith("tile");

            for (int index = 0; index < numArray.Length; index += VertexStride) //set render.debugoutput 1
            {
                for (int i = 0; i < elemcount; i++)
                {
                    var elem = elems[i];
                    int elemoffset = elem.Offset;

                    switch (elem.Format)
                    {
                        case VertexElementFormat.Float3: //XYZ to ZXY
                            var newVert = BufferUtil.ReadVector3(numArray, index + elemoffset);
                            Rpf6Crypto.WriteVector3AtIndex(newVert, numArray, index + elemoffset);
                            break;
                        case VertexElementFormat.Dec3N:
                            var packed = BufferUtil.ReadUint(numArray, index + elemoffset);
                            var packedVec = FloatUtil.Dec3NToVector3(packed); //XYZ to ZXY
                            packed = Rpf6Crypto.GetDec3N(packedVec);
                            BufferUtil.WriteUint(numArray, index + elemoffset, packed);
                            break;
                        case VertexElementFormat.Half2: //Scale terrain UVs
                            if (!terrainMesh) continue;
                            var half2 = BufferUtil.ReadStruct<Half2>(numArray, index + elemoffset);
                            half2 = Rpf6Crypto.RescaleHalf2(half2, 2.0f);
                            BufferUtil.WriteStruct(numArray, index + elemoffset, ref half2);
                            break;
                        case VertexElementFormat.UShort2N: //Scale UVs
                            var tUv = Rpf6Crypto.ReadRescaleUShort2N(numArray, index + elemoffset, highLOD);
                            if (tUv[0] is float.NaN || tUv[1] is float.NaN) continue; //Nothing to do
                            if (elem.SemanticIndex == 0)
                            {
                                uvX.Add(tUv[0]);
                                uvY.Add(tUv[1]);
                                uvOffset.Add(index + elemoffset);
                            }
                            else if (elem.SemanticIndex == 1)
                            {
                                uvX1.Add(tUv[0]);
                                uvY1.Add(tUv[1]);
                                uvOffset1.Add(index + elemoffset);
                            }             
                            break;
                        default:
                            break;
                    }
                }
            }

            //rdr2_cliffwall_ao
            if (AreTexcoordsNormalizable() && highLOD)
            {
                Rpf6Crypto.NormalizeUVs(uvX, uvY, uvOffset, ref numArray);
                Rpf6Crypto.NormalizeUVs(uvX1, uvY1, uvOffset1, ref numArray);      
            }

            VertexData = numArray;
            Indices = IndexBuffer.Item?.Indices.Items;
            BoneIds = new Rsc6RawArr<ushort>();
        }

        public void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x00D3397C : VFT);
            writer.WriteUInt32(Unknown_4h);
            writer.WriteUInt32(Unknown_8h);
            writer.WritePtr(VertexBuffer);
            writer.WriteUInt32(VertexBuffer2);
            writer.WriteUInt32(VertexBuffer3);
            writer.WriteUInt32(VertexBuffer4);
            writer.WritePtr(IndexBuffer);
            writer.WriteUInt32(IndexBuffer2);
            writer.WriteUInt32(IndexBuffer3);
            writer.WriteUInt32(IndexBuffer4);
            writer.WriteUInt32(IndicesCount);
            writer.WriteUInt32(TrianglesCount);
            writer.WriteUInt16((ushort)VertexCount);
            writer.WriteByte(PrimitiveType);
            writer.WriteBoolean(DoubleBuffered);
            writer.WriteRawArrPtr(BoneIds);
            writer.WriteUInt16((ushort)VertexStride);
            writer.WriteUInt16(BoneIdsCount);
            writer.WriteUInt32(0xCDCDCDCD);
            writer.WriteUInt32(OffsetBuffer);
            writer.WriteUInt32(IndexOffset);
            writer.WriteUInt32(Unknown_3Ch);
        }

        public void ReadXml(XmlNode node)
        {
            ShaderID = (ushort)Xml.GetChildUIntAttribute(node, "ShaderIndex", "value");
            BoneIdsCount = (ushort)Xml.GetChildUIntAttribute(node, "BoneIdsCount", "value");
            Unknown_3Ch = Xml.GetChildUIntAttribute(node, "Unknown_3Ch", "value");

            var aabb = new BoundingBox4()
            {
                Min = Rpf6Crypto.GetXmlVector4(node, "BoundingBoxMin"),
                Max = Rpf6Crypto.GetXmlVector4(node, "BoundingBoxMax")
            };
            AABB = aabb;

            var bnode = node.SelectSingleNode("BoneIDs");
            if (bnode != null)
            {
                var astr = bnode.InnerText;
                var arr = astr.Split(',');
                var blist = new List<ushort>();

                foreach (var bstr in arr)
                {
                    var tstr = bstr?.Trim();
                    if (string.IsNullOrEmpty(tstr))
                        continue;

                    if (ushort.TryParse(tstr, out ushort u))
                    {
                        blist.Add(u);
                    }
                }
                BoneIds = (blist.Count > 0) ? new Rsc6RawArr<ushort>(blist.ToArray()) : new Rsc6RawArr<ushort>(null);
            }

            var vnode = node.SelectSingleNode("VertexBuffer");
            if (vnode != null)
            {
                var vertexBuffer = new Rsc6VertexBuffer();
                vertexBuffer.ReadXml(vnode);
                VertexBuffer = new Rsc6Ptr<Rsc6VertexBuffer>(vertexBuffer);
                VertexData = VertexBuffer.Item.LockedData.Items ?? VertexBuffer.Item.Data2.Items;
            }

            var inode = node.SelectSingleNode("IndexBuffer");
            if (inode != null)
            {
                var indexBuffer = new Rsc6IndexBuffer();
                indexBuffer.ReadXml(inode);
                IndexBuffer = new Rsc6Ptr<Rsc6IndexBuffer>(indexBuffer);
            }

            //Update data
            VertexCount = (ushort)(VertexData != null ? VertexBuffer.Item.VertexCount : 0);
            VertexStride = (ushort)(VertexBuffer.Item != null ? VertexBuffer.Item.VertexStride : 0);
            IndicesCount = (IndexBuffer.Item != null ? IndexBuffer.Item.IndicesCount : 0);
            TrianglesCount = IndicesCount / 3;
        }

        public void WriteXml(StringBuilder sb, int indent, Vector3 center)
        {
            var aabbMin = AABB.Min;
            var aabbMax = AABB.Max;

            Xml.ValueTag(sb, indent, "ShaderIndex", ShaderID.ToString());
            Xml.ValueTag(sb, indent, "BoneIdsCount", BoneIdsCount.ToString());
            Xml.ValueTag(sb, indent, "Unknown_3Ch", Unknown_3Ch.ToString());
            Xml.SelfClosingTag(sb, indent, "BoundingBoxMin " + FloatUtil.GetVector4XmlString(aabbMin));
            Xml.SelfClosingTag(sb, indent, "BoundingBoxMax " + FloatUtil.GetVector4XmlString(aabbMax));

            if (VertexLayout != null)
            {
                Xml.OpenTag(sb, indent, "VertexBuffer");
                Xml.ValueTag(sb, indent, "Flags", "0");
                Xml.OpenTag(sb, indent, string.Format("Layout type=\"{0}\"", VertexBuffer.Item.Layout.Item.Types.ToString()).Replace("RDR1_1", "GTAV1").Replace("RDR1_2", "GTAV1"));
                var elems = VertexLayout.Elements;
                var elemcount = elems.Length;

                for (int i = 0; i < elemcount; i++)
                {
                    var elem = elems[i];
                    var name = TranslateToSollumz(elem.SemanticName);

                    if (string.IsNullOrEmpty(name))
                        continue;
                    if (name == "Binormal") //Skipping some elements
                        continue;

                    if (name == "Colour" || name == "TexCoord")
                        Xml.SelfClosingTag(sb, indent + 1, name + elem.SemanticIndex);
                    else if ((name == "Tangent" || name == "BlendWeights" || name == "BlendIndices") && elem.SemanticIndex > 0)
                        Xml.SelfClosingTag(sb, indent + 1, name + elem.SemanticIndex);
                    else
                        Xml.SelfClosingTag(sb, indent + 1, name);
                }

                Xml.CloseTag(sb, indent, "Layout");
                Xml.OpenTag(sb, indent, "Data");

                var elemoffset = 0;
                for (int v = 0; v < VertexCount; v++)
                {
                    Xml.Indent(sb, indent + 1);
                    var formatedOutput = GenerateVertexData(v, center, ref elemoffset);
                    sb.Append(formatedOutput);
                    sb.AppendLine();
                }
                Xml.CloseTag(sb, indent, "Data");
                Xml.CloseTag(sb, indent, "VertexBuffer");
            }

            if (IndexBuffer.Item != null)
            {
                Xml.OpenTag(sb, indent, "IndexBuffer");
                Xml.WriteRawArray(sb, indent, "Data", Indices);
                Xml.CloseTag(sb, indent, "IndexBuffer");
            }
        }

        public bool AreTexcoordsNormalizable()
        {
            return VertexBuffer.Item.Layout.Item.Flags == 49369;
        }

        public string GenerateVertexData(int v, Vector3 center, ref int elemoffset)
        {
            if (VertexLayout == null)
                return "";

            var elems = VertexLayout.Elements;
            var elemcount = elems.Length;
            var sb = new StringBuilder();

            var vertexPositions = new List<Vector3>();
            var vertexColors = new List<Colour>();
            var vertexNormals = new List<Vector4>();
            var vertexTangents = new List<Vector4>();
            var texCoords = new List<Vector2>();
            var blendWeights = new List<Colour>();
            var blendIndices = new List<Colour>();

            for (int i = 0; i < elemcount; i++)
            {
                var elem = elems[i];
                var index = elem.SemanticIndex;
                var elemsize = VertexElementFormats.GetSizeInBytes(elem.Format);
                var name = TranslateToSollumz(elem.SemanticName);

                if (name == "Binormal")
                {
                    elemoffset += elemsize;
                    continue;
                }

                switch (elem.Format)
                {
                    case VertexElementFormat.UShort2N:
                        var xUnk4 = BitConverter.ToUInt16(VertexData, elemoffset);
                        var yUnk4 = BitConverter.ToUInt16(VertexData, elemoffset + 2);
                        texCoords.Add(new Vector2(xUnk4 * (float)3.05185094e-005, yUnk4 * (float)3.05185094e-005));
                        break;
                    case VertexElementFormat.Half2:
                        var h2 = BufferUtil.ReadStruct<Half2>(VertexData, elemoffset);
                        texCoords.Add(new Vector2((float)h2.X, (float)h2.Y));
                        break;
                    case VertexElementFormat.Float2:
                        var xy = BufferUtil.ReadVector2(VertexData, elemoffset);
                        texCoords.Add(xy);
                        break;

                    case VertexElementFormat.Float3:
                        var pos = BufferUtil.ReadVector3(VertexData, elemoffset);
                        //pos = Vector3.Subtract(pos, center);
                        vertexPositions.Add(pos);
                        break;

                    case VertexElementFormat.Colour:
                        var color = BufferUtil.ReadColour(VertexData, elemoffset);
                        switch (elem.SemanticName)
                        {
                            case "BLENDINDICES": blendIndices.Add(color); break;
                            case "BLENDWEIGHTS": blendWeights.Add(color); break;
                            case "COLOR": vertexColors.Add(color); break;
                            default: continue;
                        }
                        break;

                    case VertexElementFormat.UByte4:
                        var bi = BufferUtil.ReadColour(VertexData, elemoffset);
                        blendIndices.Add(bi);
                        break;

                    case VertexElementFormat.Dec3N:
                        uint value = BufferUtil.ReadUint(VertexData, elemoffset);
                        uint ux = (value >> 0) & 0x3FF;
                        uint uy = (value >> 10) & 0x3FF;
                        uint uz = (value >> 20) & 0x3FF;
                        uint uw = (value >> 30);

                        bool posX = (ux & 0x200) > 0;
                        bool posY = (uy & 0x200) > 0;
                        bool posZ = (uz & 0x200) > 0;
                        bool posW = (uw & 0x200) > 0;

                        var valueX = ((posX ? ~ux : ux) & 0x1FF) / (posX ? -511.0f : 511.0f);
                        var valueY = ((posY ? ~uy : uy) & 0x1FF) / (posY ? -511.0f : 511.0f);
                        var valueZ = ((posZ ? ~uz : uz) & 0x1FF) / (posZ ? -511.0f : 511.0f);
                        var valueW = ((posW ? ~uw : uw) & 0x1FF) / (posW ? -511.0f : 511.0f);

                        switch (elem.SemanticName)
                        {
                            case "NORMAL": vertexNormals.Add(new Vector4(valueX, valueY, valueZ, valueW)); break;
                            case "TANGENT": vertexTangents.Add(new Vector4(valueX, valueY, valueZ, valueW)); break;
                            default: continue;
                        }
                        break;

                    default:
                        break;
                }
                elemoffset += elemsize;

                switch (elem.SemanticName)
                {
                    case "POSITION": sb.Append(string.Format("{0} {1} {2}   ", vertexPositions[index].X, vertexPositions[index].Y, vertexPositions[index].Z).Replace(",", ".")); break;
                    case "BLENDWEIGHTS": var bw = blendWeights[index].ToArray(); sb.Append(string.Format("{0} {1} {2} {3}   ", bw[0], bw[1], bw[2], bw[3])); break;
                    case "BLENDINDICES": var bi = blendIndices[index].ToArray(); sb.Append(string.Format("{0} {1} {2} {3}   ", bi[0], bi[1], bi[2], bi[3])); break;
                    case "NORMAL": sb.Append(string.Format("{0} {1} {2}   ", vertexNormals[index].X, vertexNormals[index].Y, vertexNormals[index].Z).Replace(",", ".")); break;
                    case "COLOR": var color = vertexColors[index].ToArray(); sb.Append(string.Format("{0} {1} {2} {3}   ", color[0], color[1], color[2], color[3])); break;
                    case "TEXCOORD": sb.Append(string.Format("{0} {1}   ", texCoords[index].X, texCoords[index].Y).Replace(",", ".")); break;
                    case "TANGENT": sb.Append(string.Format("{0} {1} {2} {3}   ", vertexTangents[index].X, vertexTangents[index].Y, vertexTangents[index].Z, vertexTangents[index].W).Replace(",", ".")); break;
                    default: break;
                }
            }
            return sb.ToString();
        }

        public string TranslateToSollumz(string name)
        {
            switch (name)
            {
                case "POSITION": return "Position";
                case "BLENDWEIGHTS": return "BlendWeights";
                case "BLENDINDICES": return "BlendIndices";
                case "NORMAL": return "Normal";
                case "COLOR": return "Colour";
                case "TEXCOORD": return "TexCoord";
                case "TANGENT": return "Tangent";
                case "BINORMAL": return "Binormal";
                default: return "";
            }
        }

        public void SetShader(Rsc6ShaderFX shader)
        {
            ShaderRef = shader;
            if (shader != null)
            {
                var bucket = shader.RenderBucket;
                var hash = shader.Name.Hash;

                switch (hash)
                {
                    case 0x60F5992A: //rdr2_clouds_animsoft
                    case 0x693DE1A1: //rdr2_clouds_altitude
                    case 0x5218CB1D: //rdr2_clouds_fast
                    case 0xE3136EFD: //rdr2_clouds_anim
                    case 0xF6C04CCD: //rdr2_clouds_soft
                    case 0xFE72D6A5: //rdr2_clouds_fog
                        SetupSkyShader(shader);
                        break;
                    case 0x707EF967: //rdr2_flattenterrain_blend
                    case 0xC242DAA7: //rdr2_terrain_blend
                    case 0xF98973D1: //rdr2_terrain
                        SetupBlendTerrainShader(shader);
                        break;
                    case 0x3103407E: //rdr2_cliffwall_ao_low_lod
                    case 0x249BB297: //rdr2_cliffwall_ao
                        SetupClifwallTerrainShader(shader);
                        break;
                    case 0xB34AF114: //rdr2_layer_2_nospec_ambocc_decal
                    case 0x5A170205: //rdr2_layer_2_nospec_ambocc
                        SetDiffuse2Shader(shader); 
                        break;
                    case 0x24982D70: //rdr2_layer_3_nospec_normal_ambocc
                        SetDiffuse3Shader(shader);
                        break;
                    default:
                        SetupDefaultShader(shader);
                        break;
                }

                switch (hash)
                {
                    case 0x32A4918E: //rdr2_alpha
                    case 0x173D5F9D: //rdr2_grass
                    case 0x171C9E47: //rdr2_glass_glow
                    case 0xC714B86E: //rdr2_alpha_foliage
                    case 0x592D7DC2: //rdr2_alpha_foliage_no_fade
                    case 0xBBCB0BF8: //rdr2_alpha_bspec_ao_shareduv
                    case 0xC4724CCA: //rdr2_alpha_bspec_ao_shareduv_character_nooff
                    case 0x14577406: //rdr2_alpha_bspec_ao_shareduv_character_hair
                    case 0xC14300BB: //rdr2_alpha_bspec_ao_shareduv_character_cutscene
                    case 0x2FE0F698: //rdr2_bump_spec_ambocc_shared
                        ShaderInputs.SetFloat(0x4D52C5FF, 1.0f); //AlphaScale
                        break;
                    case 0x949EC19C: //rdr2_alpha_bspec_ao_shared
                        ShaderInputs.SetFloat(0xDF918855, 1.0f); //BumpScale
                        ShaderInputs.SetFloat(0x4D52C5FF, 1.0f); //AlphaScale
                        break;
                    case 0x7668B157: //rdr2_glass_nodistortion_bump_spec_ao_shared
                    case 0x72A21FFE: //rdr2_glass_nodistortion_bump_spec_ao
                        ShaderInputs.SetFloat4(0x5C3AB6E9, new Vector4(1, 0, 0, 0)); //"DecalMasks"
                        ShaderInputs.SetUInt32(0x0188ECE8, 1u);  //"DecalMode"
                        ShaderInputs.SetFloat(0x4D52C5FF, 1.0f); //AlphaScale
                        break;
                }

                switch (bucket)
                {
                    default: throw new Exception("Unknown RenderBucket");
                    case 0: ShaderBucket = ShaderBucket.Solid; break;  //Opaque
                    case 1: ShaderBucket = ShaderBucket.SolidB; break; //Double-sided opaque
                    case 2: ShaderBucket = ShaderBucket.Alpha1; break; //Hair
                    case 3: ShaderBucket = ShaderBucket.Alpha1; break; //AlphaMask
                    case 4: ShaderBucket = ShaderBucket.Translucency; break; //Water
                    case 5: ShaderBucket = ShaderBucket.Alpha1; break; //Transparent
                    case 6: ShaderBucket = ShaderBucket.Alpha2; break; //DistortionGlass
                    case 8: ShaderBucket = ShaderBucket.AlphaF; ShaderInputs.SetUInt32(0x0188ECE8, 1u); break; //Alpha
                }
            }
        }

        private void SetupDefaultShader(Rsc6ShaderFX s) //diffuse + bump + ambocc
        {
            SetDefaultShader();
            ShaderInputs = Shader.CreateShaderInputs();

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;
            Textures = new Texture[2];

            var sfresnel = 0.96f;
            var sintensitymult = 0.2f;
            var sfalloffmult = 35.0f;

            for (int p = 0; p < parms.Length; p++)
            {
                var parm = parms[p];
                if (parm.DataType == 0)
                {
                    var tex = parm.Texture;
                    if (tex != null)
                    {
                        switch (parm.Hash)
                        {
                            case 0xF1FE2B71: //diffusesampler
                            case 0x2B5170FD: //texturesampler
                                Textures[0] = tex;
                                break;
                            case 0x46B7C64F: //bumpsampler
                            case 0x8AC11CB0: //normalsampler
                                //Textures[1] = tex;
                                break;
                        }
                    }
                }
                else if (parm.Vector != null)
                {
                    switch (parm.Hash)
                    {
                        case 0xf6712b81: //bumpiness
                            //ShaderInputs.SetFloat(0xDF918855, parm.Vector.Vector.X); //"BumpScale"
                            break;
                        case 0xBBEED254: //fresnelterm         //~0.3-1, low for metals, ~0.96 for nonmetals
                            sfresnel = parm.Vector.Vector.X;
                            break;
                        case 0x484A5EBD: //specularcolorfactor eg 1.8
                            sintensitymult = parm.Vector.Vector.X;
                            break;
                        case 0x166E0FD1: //specularfactor eg 30
                            sfalloffmult = parm.Vector.Vector.X;
                            break;
                    }
                }
            }
            ShaderInputs.SetFloat(0xDA9702A9, FloatUtil.Saturate(sintensitymult * (1.0f - ((sfresnel - 0.1f) / 0.896f)))); //"MeshMetallicity"
            ShaderInputs.SetFloat(0x57C22E45, FloatUtil.Saturate(sfalloffmult / 100.0f)); //"MeshParamsMult"
            ShaderInputs.SetFloat(0x92176B1A, 0.3f); //MeshSmoothness
        }

        private void SetupBlendTerrainShader(Rsc6ShaderFX s)
        {
            SetCoreShader<BlendShader>(ShaderBucket.Solid);
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0x9B920BD, 25); //BlendMode

            if (s == null)
                return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null)
                return;

            Textures = new Texture[14];
            for (int k = 0; k < parms.Length; k++)
            {
                var prm = parms[k];
                if (prm.DataType == 0)
                {
                    switch (prm.Hash)
                    {
                        case 0xB5C6B283: //terraindiffusesampler1
                            Textures[0] = prm.Texture;
                            break;
                        case 0x13376D63: //terraindiffusesampler2
                            Textures[1] = prm.Texture;
                            break;
                        case 0x3412AF91: //terraindiffusesampler3
                            Textures[2] = prm.Texture;
                            break;
                        case 0x3D734252: //terraindiffusesampler4
                            Textures[3] = prm.Texture;
                            break;
                        case 0x4FB1E6CF: //terraindiffusesampler5
                            Textures[4] = prm.Texture;
                            break;
                        case 0xEAE71D3B: //terraindiffusesampler6
                            Textures[5] = prm.Texture;
                            break;
                        case 0x0ED966D5: //terrainblendmap1
                            Textures[12] = prm.Texture;
                            break;
                        case 0xA0918A47: //terrainblendmap2
                            Textures[13] = prm.Texture;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (prm.Hash)
                    {
                        case 0xf6712b81://"bumpiness"
                            //ShaderInputs.SetFloat4(0x7CB163F5, new Vector4(prm.Vector.Vector.X)); //"BumpScales"
                            break;
                        case 0x66C79BD6: //megatilerepetitions, how many times, across the 0-1 of the UV channel map, do the tiles repeat
                            ShaderInputs.SetFloat4(0x401BDDBB, prm.Vector.Vector); //"UVLookupIndex"
                            break;
                        case 0x4385A0D2: //megatileoffset - offset of the UV for the tile when at (0,0) in the channel map
                            ShaderInputs.SetFloat4(0xAD966CCC, prm.Vector.Vector); //"UVScaleOffset"      float4
                            break;
                        case 0x9FBAB08B: //blendmapscale1
                            ShaderInputs.SetFloat4(0xA83AA336, prm.Vector.Vector); //LODColourLevels    float4
                            break;
                        case 0xAC181AA0: //blendmapoffset1
                            ShaderInputs.SetFloat4(0x8D01D9A3, prm.Vector.Vector); //LODColourBlends    float4
                            break;
                        case 0x62503593: //blendmapscale2
                            ShaderInputs.SetFloat4(0xB0379AA1, prm.Vector.Vector); //HBBScales          float4
                            break;
                        case 0xBDDEBE2D: //blendmapoffset2
                            ShaderInputs.SetFloat4(0xFF6E0669, prm.Vector.Vector); //HBBOffsets         float4
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void SetupClifwallTerrainShader(Rsc6ShaderFX s)
        {
            SetCoreShader<BlendShader>(ShaderBucket.Solid);
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0x9B920BD, 24); //BlendMode

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;
            Textures = new Texture[13];

            for (int k = 0; k < parms.Length; k++)
            {
                var prm = parms[k];
                if (prm.DataType == 0)
                {
                    switch (prm.Hash)
                    {
                        case 0x2B5170FD: //"texturesampler"
                            Textures[0] = prm.Texture;
                            break;
                        case 0x0ED966D5: //terrainblendmap1
                            Textures[12] = prm.Texture;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (prm.Hash)
                    {
                        case 0xE55CF27C: //blendmapscalecliffflatten
                        case 0x606B83EE: //blendmapscalecliff
                            ShaderInputs.SetFloat4(0xA83AA336, prm.Vector.Vector); //LODColourLevels    float4
                            break;
                        case 0x92165D5E: //blendmapoffsetcliffflatten
                        case 0x99276EAE: //blendmapoffsetcliff
                            ShaderInputs.SetFloat4(0x8D01D9A3, prm.Vector.Vector); //LODColourBlends    float4
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void SetDiffuse2Shader(Rsc6ShaderFX s) //diffuse + diffuse2 + bump + ambocc
        {
            SetCoreShader<BlendShader>(ShaderBucket.Solid);
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0x9B920BD, 22); //BlendMode

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;
            Textures = new Texture[2];

            var sfresnel = 0.96f;
            var sintensitymult = 0.2f;
            var sfalloffmult = 35.0f;

            for (int p = 0; p < parms.Length; p++)
            {
                var parm = parms[p];
                if (parm.DataType == 0)
                {
                    var tex = parm.Texture;
                    if (tex != null)
                    {
                        switch (parm.Hash)
                        {
                            case 0xF1FE2B71: //diffusesampler
                            case 0x2b5170fd: //texturesampler
                            case 0x3e19076b: //detailmapsampler
                            case 0x605fcc60: //distancemapsampler
                                Textures[0] = tex;
                                break;
                            case 0x05645204: //texturesampler2
                                Textures[1] = tex;
                                break;
                        }
                    }
                }
                else
                {
                    switch (parm.Hash)
                    {
                        case 0xBBEED254: //fresnelterm         //~0.3-1, low for metals, ~0.96 for nonmetals
                            sfresnel = parm.Vector.Vector.X;
                            break;
                        case 0x484A5EBD: //specularcolorfactor   //0-1, final multiplier?
                            sintensitymult = parm.Vector.Vector.X;
                            break;
                        case 0x166E0FD1: //specularfactor    //10-150+?, higher is shinier
                            sfalloffmult = parm.Vector.Vector.X;
                            break;
                    }
                }
            }
            ShaderInputs.SetFloat(0xDA9702A9, FloatUtil.Saturate(sintensitymult * (1.0f - ((sfresnel - 0.1f) / 0.896f)))); //"MeshMetallicity"
            ShaderInputs.SetFloat(0x57C22E45, FloatUtil.Saturate(sfalloffmult / 100.0f)); //"MeshParamsMult"
            ShaderInputs.SetFloat(0x92176B1A, FloatUtil.Saturate(0.3f)); //"MeshSmoothness
        }

        private void SetDiffuse3Shader(Rsc6ShaderFX s) //diffuse + diffuse2 + diffuse3 + bump + ambocc
        {
            SetCoreShader<BlendShader>(ShaderBucket.Solid);
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0x9B920BD, 23); //BlendMode

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;
            Textures = new Texture[3];

            var sfresnel = 0.96f;
            var sintensitymult = 0.2f;
            var sfalloffmult = 35.0f;

            for (int p = 0; p < parms.Length; p++)
            {
                var parm = parms[p];
                if (parm.DataType == 0)
                {
                    var tex = parm.Texture;
                    if (tex != null)
                    {
                        switch (parm.Hash)
                        {
                            case 0xF1FE2B71: //diffusesampler
                            case 0x2b5170fd: //texturesampler
                            case 0x3e19076b: //detailmapsampler
                            case 0x605fcc60: //distancemapsampler
                                Textures[0] = tex;
                                break;
                            case 0x05645204: //texturesampler2
                                Textures[1] = tex;
                                break;
                            case 0xA3348DA6: //texturesampler3
                                Textures[2] = tex;
                                break;
                        }
                    }
                }
                else
                {
                    switch (parm.Hash)
                    {
                        case 0xBBEED254: //fresnelterm         //~0.3-1, low for metals, ~0.96 for nonmetals
                            sfresnel = parm.Vector.Vector.X;
                            break;
                        case 0x484A5EBD: //specularcolorfactor   //0-1, final multiplier?
                            sintensitymult = parm.Vector.Vector.X;
                            break;
                        case 0x166E0FD1: //specularfactor    //10-150+?, higher is shinier
                            sfalloffmult = parm.Vector.Vector.X;
                            break;
                    }
                }
            }
            ShaderInputs.SetFloat(0xDA9702A9, FloatUtil.Saturate(sintensitymult * (1.0f - ((sfresnel - 0.1f) / 0.896f)))); //MeshMetallicity
            ShaderInputs.SetFloat(0x57C22E45, FloatUtil.Saturate(sfalloffmult / 100.0f)); //MeshParamsMult
            ShaderInputs.SetFloat(0x92176B1A, FloatUtil.Saturate(0.3f)); //"MeshSmoothness
        }

        private void SetupSkyShader(Rsc6ShaderFX s)
        {
            SetCoreShader<CloudShader>(ShaderBucket.CloudAndFog);
            ShaderInputs = Shader.CreateShaderInputs();

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;
            Textures = new Texture[6];

            for (int p = 0; p < parms.Length; p++)
            {
                var parm = parms[p];
                if (parm.DataType == 0)
                {
                    var tex = parm.Texture;
                    if (tex != null)
                    {
                        switch (parm.Hash)
                        {
                            case 0xE43044D6: //densitysampler
                                Textures[0] = tex;
                                break;
                            case 0x8AC11CB0: //normalsampler
                                Textures[1] = tex;
                                break;
                            case 0x874FD28B: //detaildensitysampler
                                Textures[2] = tex;
                                break;
                            case 0xAD1518E5: //detailnormalsampler
                                Textures[3] = tex;
                                break;
                            case 0x9A35E36C: //detaildensity2sampler
                                Textures[4] = tex;
                                break;
                            case 0x77755B8F: //detailnormal2sampler
                                Textures[5] = tex;
                                break;
                        }
                    }
                }
                else
                {

                    switch (parm.Hash)
                    {
                        case 0xB33E5862: //gsuncolor
                            break;
                        case 0xC4B23E96: //guvoffset
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return VertexCount.ToString() + " verts, " + (ShaderRef?.ToString() ?? "NULL SHADER)");
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6DrawableModel : Model, Rsc6Block //rage::grmModel
    {
        public ulong BlockLength => 28;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public ulong VFT { get; set; } = 0x01854414;
        public Rsc6PtrArr<Rsc6DrawableGeometry> Geometries { get; set; } //m_Geometries
        public Rsc6RawArr<BoundingBox4> BoundsData { get; set; } //m_AABBs
        public Rsc6RawArr<ushort> ShaderMapping { get; set; } //m_ShaderIndex
        public byte MatrixCount { get; set; } //m_MatrixCount, bone count
        public byte Flags { get; set; } //m_Flags
        public byte Type { get; set; } = 0xCD; //m_Type
        public byte MatrixIndex { get; set; } //m_MatrixIndex
        public byte Stride { get; set; } //m_Stride
        public byte SkinFlag { get; set; } //m_SkinFlag
        public ushort GeometriesCount3 { get; set; } //m_shaderMappingCount?

        public void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Geometries = reader.ReadPtrArr<Rsc6DrawableGeometry>();
            BoundsData = reader.ReadRawArrPtr<BoundingBox4>();
            ShaderMapping = reader.ReadRawArrPtr<ushort>();
            MatrixCount = reader.ReadByte();
            Flags = reader.ReadByte();
            Type = reader.ReadByte();
            MatrixIndex = reader.ReadByte();
            Stride = reader.ReadByte();
            SkinFlag = reader.ReadByte();
            GeometriesCount3 = reader.ReadUInt16();

            var geocount = Geometries.Count;
            ShaderMapping = reader.ReadRawArrItems(ShaderMapping, geocount, true);
            BoundsData = reader.ReadRawArrItems(BoundsData, geocount > 1 ? geocount + 1u : geocount);

            for (int i = 0; i < BoundsData.Items.Length; i++)
            {
                float zMax = BoundsData.Items[i].Max.Z;
                float zMin = BoundsData.Items[i].Min.Z;
                float yMax = BoundsData.Items[i].Max.Y;
                float yMin = BoundsData.Items[i].Max.Y;
                float xMax = BoundsData.Items[i].Max.X;
                float xMin = BoundsData.Items[i].Min.X;
                BoundsData.Items[i].Min = new Vector4(zMin, xMin, yMin, BoundsData.Items[i].Min.W);
                BoundsData.Items[i].Max = new Vector4(zMax, xMax, yMax, BoundsData.Items[i].Max.W);
            }

            var geoms = Geometries.Items;
            if (geoms != null)
            {
                var shaderMapping = ShaderMapping.Items;
                var boundsData = BoundsData.Items;
                for (int i = 0; i < geoms.Length; i++)
                {
                    var geom = geoms[i];
                    if (geom != null)
                    {
                        geom.ShaderID = ((shaderMapping != null) && (i < shaderMapping.Length)) ? shaderMapping[i] : (ushort)0;
                        geom.AABB = (boundsData != null) ? ((boundsData.Length > 1) && ((i + 1) < boundsData.Length)) ? boundsData[i + 1] : boundsData[0] : new BoundingBox4();
                        geom.BoundingBox = geom.AABB.ToBoundingBox();
                    }
                }
            }
            Meshes = Geometries.Items;
            RenderInMainView = true;
            RenderInShadowView = true;
            RenderInEnvmapView = true;
        }

        public void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x00D30B04 : (uint)VFT);
            writer.WritePtrArr(Geometries);
            writer.WriteRawArrPtr(BoundsData);
            writer.WriteRawArrPtr(ShaderMapping);
            writer.WriteByte(MatrixCount);
            writer.WriteByte(Flags);
            writer.WriteByte(Type);
            writer.WriteByte(MatrixIndex);
            writer.WriteByte(Stride);
            writer.WriteByte(SkinFlag);
            writer.WriteUInt16(GeometriesCount3);
        }

        public void ReadXml(XmlNode node)
        {
            Type = (byte)Xml.GetChildUIntAttribute(node, "RenderMask", "value");
            Flags = (byte)Xml.GetChildUIntAttribute(node, "Flags", "value");
            SkinFlag = (byte)Xml.GetChildUIntAttribute(node, "HasSkin", "value");
            MatrixIndex = (byte)Xml.GetChildUIntAttribute(node, "BoneIndex", "value");
            MatrixCount = (byte)Xml.GetChildUIntAttribute(node, "MatrixCount", "value");

            var geoms = new List<Rsc6DrawableGeometry>();
            var aabbs = new List<BoundingBox4>();
            var shids = new List<ushort>();
            var min = new Vector4(float.MaxValue);
            var max = new Vector4(float.MinValue);

            var geoNodes = node.SelectSingleNode("Geometries");
            if (geoNodes != null)
            {
                var itemNodes = geoNodes.SelectNodes("Item");
                if (itemNodes != null)
                {
                    if (itemNodes?.Count > 0)
                    {
                        foreach (XmlNode gNode in itemNodes)
                        {
                            var geometry = new Rsc6DrawableGeometry();
                            geometry.ReadXml(gNode);
                            geoms.Add(geometry);
                        }
                    }
                }
            }

            if (geoms != null)
            {
                Geometries = new Rsc6PtrArr<Rsc6DrawableGeometry>(geoms.ToArray());
                foreach (var geom in geoms)
                {
                    aabbs.Add(geom.AABB);
                    shids.Add(geom.ShaderID);
                    min = Vector4.Min(min, geom.AABB.Min);
                    max = Vector4.Max(max, geom.AABB.Max);
                }
                GeometriesCount3 = (ushort)geoms.Count;
            }
            if (aabbs.Count > 1)
            {
                var outeraabb = new BoundingBox4() { Min = min, Max = max };
                aabbs.Insert(0, outeraabb);
            }

            BoundsData = (aabbs.Count > 0) ? new Rsc6RawArr<BoundingBox4>(aabbs.ToArray()) : new Rsc6RawArr<BoundingBox4>(null);
            ShaderMapping = (shids.Count > 0) ? new Rsc6RawArr<ushort>(shids.ToArray()) : new Rsc6RawArr<ushort>(null);
        }

        public void WriteXml(StringBuilder sb, int indent, Vector3 center)
        {
            Xml.ValueTag(sb, indent, "RenderMask", Type.ToString());
            Xml.ValueTag(sb, indent, "Flags", Flags.ToString());
            Xml.ValueTag(sb, indent, "HasSkin", SkinFlag.ToString());
            Xml.ValueTag(sb, indent, "BoneIndex", MatrixIndex.ToString());
            Xml.ValueTag(sb, indent, "MatrixCount", MatrixCount.ToString());

            if (Geometries.Items != null)
            {
                Xml.OpenTag(sb, indent, "Geometries");
                foreach (var m in Geometries.Items)
                {
                    Xml.OpenTag(sb, indent + 1, "Item");
                    m.WriteXml(sb, indent + 2, center);
                    Xml.CloseTag(sb, indent + 1, "Item");
                }
                Xml.CloseTag(sb, indent, "Geometries");
            }
        }

        public long MemoryUsage
        {
            get
            {
                long val = 0;
                var geoms = Geometries.Items;
                if (geoms != null)
                {
                    foreach (var geom in geoms)
                    {
                        if (geom == null) continue;
                        if (geom.VertexData != null)
                        {
                            val += geom.VertexData.Length;
                        }
                        var ibi = geom.IndexBuffer.Item;
                        if (ibi != null)
                        {
                            val += ibi.IndicesCount * 4;
                        }
                        var vbi = geom.VertexBuffer.Item;
                        if (vbi != null)
                        {
                            if ((vbi.LockedData.Items != null) && (vbi.LockedData.Items != geom.VertexData))
                            {
                                val += vbi.LockedData.Items.Length;
                            }
                            if ((vbi.Data2.Items != null) && (vbi.Data2.Items != geom.VertexData))
                            {
                                val += vbi.Data2.Items.Length;
                            }
                        }
                    }
                }
                if (BoundsData.Items != null)
                {
                    val += BoundsData.Items.Length * 32;
                }
                return val;
            }
        }

        public override string ToString()
        {
            var geocount = Geometries.Items?.Length ?? 0;
            return "(" + geocount.ToString() + " geometr" + (geocount != 1 ? "ies)" : "y)");
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6VertexBuffer : Rsc6BlockBase //rage::grcVertexBuffer
    {
        public override ulong BlockLength => 64;
        public uint VFT { get; set; } = 0x01858684;
        public ushort VertexCount { get; set; } //m_VertCount
        public byte Locked { get; set; } //m_Locked
        public byte Flags { get; set; } //m_Flags
        public Rsc6RawArr<byte> LockedData { get; set; } //m_pLockedData, pointer to buffer obtained by grcVertexBufferD11::Lock, in file, same as m_pVertexData
        public uint VertexStride { get; set; } //m_Stride
        public Rsc6RawArr<byte> Data2 { get; set; } //m_pVertexData
        public uint LockThreadID { get; set; } //m_dwLockThreadId
        public Rsc6Ptr<Rsc6VertexDeclaration> Layout { get; set; } //m_Fvf
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD;
        public uint Unknown_20h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD;
        public uint Unknown_30h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_34h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_38h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_3Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            VertexCount = reader.ReadUInt16();
            Locked = reader.ReadByte();
            Flags = reader.ReadByte();
            LockedData = reader.ReadRawArrPtr<byte>();
            VertexStride = reader.ReadUInt32();
            Data2 = reader.ReadRawArrPtr<byte>();
            LockThreadID = reader.ReadUInt32();
            Layout = reader.ReadPtr<Rsc6VertexDeclaration>();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
            Unknown_30h = reader.ReadUInt32();
            Unknown_34h = reader.ReadUInt32();
            Unknown_38h = reader.ReadUInt32();
            Unknown_3Ch = reader.ReadUInt32();

            LockedData = reader.ReadRawArrItems(LockedData, (uint)(VertexCount * Layout.Item.Stride));
            Data2 = reader.ReadRawArrItems(Data2, (uint)(VertexCount * Layout.Item.Stride));
        }

        public override void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x00D34D6C : VFT);
            writer.WriteUInt16(VertexCount);
            writer.WriteByte(Locked);
            writer.WriteByte(Flags);
            writer.WriteUInt32(0);
            writer.WriteUInt32(VertexStride);
            writer.WriteRawArrPtr(Data2);
            writer.WriteUInt32(LockThreadID);
            writer.WritePtr(Layout);
            writer.WriteUInt32(Unknown_1Ch);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
            writer.WriteUInt32(Unknown_30h);
            writer.WriteUInt32(Unknown_34h);
            writer.WriteUInt32(Unknown_38h);
            writer.WriteUInt32(Unknown_3Ch);
        }

        public void ReadXml(XmlNode node)
        {
            Flags = (byte)Xml.GetChildUIntAttribute(node, "Flags", "value");

            var inode = node.SelectSingleNode("Layout");
            if (inode != null)
            {
                var layout = new Rsc6VertexDeclaration();
                layout.ReadXml(inode);
                Layout = new Rsc6Ptr<Rsc6VertexDeclaration>(layout);
                VertexStride = Layout.Item.Stride;
            }

            var dnode = node.SelectSingleNode("Data");
            dnode ??= node.SelectSingleNode("Data2");

            if (dnode != null)
            {
                if (Layout.Item != null)
                {
                    var flags = Layout.Item.Flags;
                    var stride = Layout.Item.Stride;
                    var vstrs = new List<string[]>();
                    var coldelim = new[] { ' ', '\t' };
                    var rowdelim = new[] { '\n' };
                    var rows = node?.InnerText?.Trim()?.Split(rowdelim, StringSplitOptions.RemoveEmptyEntries);

                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            var rowt = row.Trim();
                            if (string.IsNullOrEmpty(rowt)) continue;
                            var cols = row.Split(coldelim, StringSplitOptions.RemoveEmptyEntries);
                            vstrs.Add(cols);
                        }
                    }

                    if (vstrs.Count > 0)
                    {
                        AllocateData(vstrs.Count);
                        for (int v = 0; v < vstrs.Count; v++)
                        {
                            var vstr = vstrs[v];
                            var sind = 0;

                            for (int k = 0; k < 16; k++)
                            {
                                if (((flags >> k) & 0x1) == 1)
                                {
                                    SetString(v, k, vstr, ref sind);
                                }
                            }
                        }
                    }
                }
                Data2 = LockedData;
            }
        }

        public void AllocateData(int vertexCount)
        {
            if (Layout.Item != null)
            {
                var stride = Layout.Item.Stride;
                var byteCount = vertexCount * stride;
                LockedData = new Rsc6RawArr<byte>(new byte[byteCount]);
                VertexCount = (ushort)vertexCount;
            }
        }

        public void SetString(int v, int c, string[] strs, ref int sind)
        {
            if ((Layout.Item != null) && (LockedData.Items != null) && (strs != null))
            {
                var ind = sind;
                float f(int i) => FloatUtil.Parse(strs[ind + i].Trim());
                byte b(int i)
                {
                    if (byte.TryParse(strs[ind + i].Trim(), out byte x))
                        return x;
                    else
                        return 0;
                }

                var ct = Layout.Item.GetComponentType(c);
                var cc = Rsc6VertexComponentTypes.GetComponentCount(ct);

                switch (ct)
                {
                    case Rsc6VertexComponentType.Float: SetFloat(v, c, f(0)); break;
                    case Rsc6VertexComponentType.Float2: SetVector2(v, c, new Vector2(f(0), f(1))); break;
                    case Rsc6VertexComponentType.Float3: SetVector3(v, c, new Vector3(f(1), f(2), f(0))); break;
                    case Rsc6VertexComponentType.Float4: SetVector4(v, c, new Vector4(f(0), f(1), f(2), f(3))); break;
                    case Rsc6VertexComponentType.Dec3N: SetDec3N(v, c, new Vector3(f(0), f(1), f(2))); break;
                    case Rsc6VertexComponentType.Half2: SetHalf2(v, c, new Half2(f(0), f(1))); break;
                    case Rsc6VertexComponentType.Half4: SetHalf4(v, c, new Half4(f(0), f(1), f(2), f(3))); break;
                    case Rsc6VertexComponentType.Colour: SetColour(v, c, new Colour(b(0), b(1), b(2), b(3))); break;
                    case Rsc6VertexComponentType.UByte4: SetUByte4(v, c, new Colour(b(0), b(1), b(2), b(3))); break;
                    default:
                        break;
                }
                sind += cc;
            }
        }

        public void SetFloat(int v, int c, float val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;

                if (e <= LockedData.Items.Length)
                {
                    var b = BitConverter.GetBytes(val);
                    Buffer.BlockCopy(b, 0, LockedData.Items, o, 4);
                }
            }
        }

        public void SetVector2(int v, int c, Vector2 val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 8;

                if (e <= LockedData.Items.Length)
                {
                    var x = BitConverter.GetBytes(val.X);
                    var y = BitConverter.GetBytes(val.Y);
                    Buffer.BlockCopy(x, 0, LockedData.Items, o + 0, 4);
                    Buffer.BlockCopy(y, 0, LockedData.Items, o + 4, 4);
                }
            }
        }

        public void SetVector3(int v, int c, Vector3 val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 12;

                if (e <= LockedData.Items.Length)
                {
                    var x = BitConverter.GetBytes(val.X);
                    var y = BitConverter.GetBytes(val.Y);
                    var z = BitConverter.GetBytes(val.Z);
                    Buffer.BlockCopy(x, 0, LockedData.Items, o + 0, 4);
                    Buffer.BlockCopy(y, 0, LockedData.Items, o + 4, 4);
                    Buffer.BlockCopy(z, 0, LockedData.Items, o + 8, 4);
                }
            }
        }

        public void SetVector4(int v, int c, Vector4 val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 16;

                if (e <= LockedData.Items.Length)
                {
                    var x = BitConverter.GetBytes(val.X);
                    var y = BitConverter.GetBytes(val.Y);
                    var z = BitConverter.GetBytes(val.Z);
                    var w = BitConverter.GetBytes(val.W);
                    Buffer.BlockCopy(x, 0, LockedData.Items, o + 0, 4);
                    Buffer.BlockCopy(y, 0, LockedData.Items, o + 4, 4);
                    Buffer.BlockCopy(z, 0, LockedData.Items, o + 8, 4);
                    Buffer.BlockCopy(w, 0, LockedData.Items, o + 12, 4);
                }
            }
        }

        public void SetDec3N(int v, int c, Vector3 val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;

                if (e <= LockedData.Items.Length)
                {
                    var u = Rpf6Crypto.PackFixedPoint(val.X, 10, 0) | Rpf6Crypto.PackFixedPoint(val.Y, 10, 10) | Rpf6Crypto.PackFixedPoint(val.Z, 10, 20);
                    var b = BitConverter.GetBytes(u);
                    Buffer.BlockCopy(b, 0, LockedData.Items, o, 4);
                }
            }
        }

        public void SetHalf2(int v, int c, Half2 val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;

                if (e <= LockedData.Items.Length)
                {
                    var hx = BitConverter.ToUInt16(BitConverter.GetBytes(val.X)); //a simple cast doesn't work for half->ushort
                    var hy = BitConverter.ToUInt16(BitConverter.GetBytes(val.Y));
                    var x = BitConverter.GetBytes(hx);
                    var y = BitConverter.GetBytes(hy);
                    Buffer.BlockCopy(x, 0, LockedData.Items, o + 0, 2);
                    Buffer.BlockCopy(y, 0, LockedData.Items, o + 2, 2);
                }
            }
        }

        public void SetHalf4(int v, int c, Half4 val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 8;

                if (e <= LockedData.Items.Length)
                {
                    var x = BitConverter.GetBytes((ushort)val.X);
                    var y = BitConverter.GetBytes((ushort)val.Y);
                    var z = BitConverter.GetBytes((ushort)val.Z);
                    var w = BitConverter.GetBytes((ushort)val.W);
                    Buffer.BlockCopy(x, 0, LockedData.Items, o + 0, 2);
                    Buffer.BlockCopy(y, 0, LockedData.Items, o + 2, 2);
                    Buffer.BlockCopy(z, 0, LockedData.Items, o + 4, 2);
                    Buffer.BlockCopy(w, 0, LockedData.Items, o + 6, 2);
                }
            }
        }

        public void SetColour(int v, int c, Colour val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;

                if (e <= LockedData.Items.Length)
                {
                    var u = val.ToRgba();
                    var b = BitConverter.GetBytes(u);
                    Buffer.BlockCopy(b, 0, LockedData.Items, o, 4);
                }
            }
        }

        public void SetUByte4(int v, int c, Colour val)
        {
            if ((Layout.Item != null) && (LockedData.Items != null))
            {
                var s = Layout.Item.Stride;
                var co = Layout.Item.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;

                if (e <= LockedData.Items.Length)
                {
                    var u = val.ToRgba();
                    var b = BitConverter.GetBytes(u);
                    Buffer.BlockCopy(b, 0, LockedData.Items, o, 4);
                }
            }
        }

        public override string ToString()
        {
            var cstr = "Count: " + VertexCount.ToString();
            if (Layout.Item == null) return "!NULL LAYOUT! - " + cstr;
            return "Type: " + Layout.Item.Flags.ToString() + ", " + cstr;
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6IndexBuffer : Rsc6BlockBase
    {
        public override ulong BlockLength => 48;
        public uint VFT { get; set; } = 0x01858D60;
        public uint IndicesCount { get; set; }
        public uint Unknown_Ch { get; set; }
        public Rsc6RawArr<ushort> Indices { get; set; }
        public uint Unknown_10h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_14h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_18h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD;
        public uint Unknown_20h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD;
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD;

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            IndicesCount = reader.ReadUInt32();
            Indices = reader.ReadRawArrPtr<ushort>();
            Unknown_Ch = reader.ReadUInt32();
            Indices = reader.ReadRawArrItems(Indices, IndicesCount, true);
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(VFT);
            writer.WriteUInt32(IndicesCount);
            writer.WriteRawArrPtr(Indices);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(Unknown_1Ch);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteUInt32(Unknown_24h);
            writer.WriteUInt32(Unknown_28h);
            writer.WriteUInt32(Unknown_2Ch);
        }

        public void ReadXml(XmlNode node)
        {
            var inode = node.SelectSingleNode("Data");
            if (inode != null)
            {
                var ushorts = Xml.GetRawUshortArray(node);
                Indices = new Rsc6RawArr<ushort>(ushorts);
                IndicesCount = (uint)(ushorts?.Length ?? 0);
            }
        }
    }

    [TC(typeof(EXP))]
    public class Rsc6DrawableBase : Piece, Rsc6Block
    {
        public virtual ulong BlockLength => 120;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public ulong VFT { get; set; } = 0x01908F7C;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6Ptr<Rsc6ShaderGroup> ShaderGroup { get; set; } //rage::grmShaderGroup
        public Rsc6Ptr<Rsc6Skeleton> SkeletonRef { get; set; } //rage::crSkeletonData
        public Vector3 BoundingCenter { get; set; } //m_CullSphere
        public float Unknown_1Ch { get; set; } = float.NaN;
        public Vector3 BoundingBoxMin { get; set; } //m_BoxMin
        public float Unknown_2Ch { get; set; } = float.NaN;
        public Vector3 BoundingBoxMax { get; set; } //m_BoxMax
        public float Unknown_3Ch { get; set; } = float.NaN;
        public Rsc6Ptr<Rsc6DrawableLod> LodHigh { get; set; } //m_Lod[0]
        public Rsc6Ptr<Rsc6DrawableLod> LodMed { get; set; } //m_Lod[1]
        public Rsc6Ptr<Rsc6DrawableLod> LodLow { get; set; } //m_Lod[2]
        public Rsc6Ptr<Rsc6DrawableLod> LodVlow { get; set; } //m_Lod[3]
        public float LodDistHigh { get; set; } //m_LodThresh[0]
        public float LodDistMed { get; set; } //m_LodThresh[1]
        public float LodDistLow { get; set; } //m_LodThresh[2]
        public float LodDistVlow { get; set; } //m_LodThresh[3]
        public uint DrawBucketMaskHigh { get; set; } //m_BucketMask[0]
        public uint DrawBucketMaskMed { get; set; } //m_BucketMask[1]
        public uint DrawBucketMaskLow { get; set; } //m_BucketMask[2]
        public uint DrawBucketMaskVlow { get; set; } //m_BucketMask[3]
        public float BoundingSphereRadius { get; set; } //m_CullRadius
        public uint PpuOnly { get; set; } //m_PpuOnly

        public virtual void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            ShaderGroup = reader.ReadPtr<Rsc6ShaderGroup>();
            SkeletonRef = reader.ReadPtr<Rsc6Skeleton>();
            BoundingCenter = reader.ReadVector3(true);
            Unknown_1Ch = reader.ReadSingle();
            BoundingBoxMin = reader.ReadVector3(true);
            Unknown_2Ch = reader.ReadSingle();
            BoundingBoxMax = reader.ReadVector3(true);
            Unknown_3Ch = reader.ReadSingle();
            LodHigh = reader.ReadPtr<Rsc6DrawableLod>();
            LodMed = reader.ReadPtr<Rsc6DrawableLod>();
            LodLow = reader.ReadPtr<Rsc6DrawableLod>();
            LodVlow = reader.ReadPtr<Rsc6DrawableLod>();
            LodDistHigh = reader.ReadSingle();
            LodDistMed = reader.ReadSingle();
            LodDistLow = reader.ReadSingle();
            LodDistVlow = reader.ReadSingle();
            DrawBucketMaskHigh = reader.ReadUInt32();
            DrawBucketMaskMed = reader.ReadUInt32();
            DrawBucketMaskLow = reader.ReadUInt32();
            DrawBucketMaskVlow = reader.ReadUInt32();
            BoundingSphereRadius = reader.ReadSingle();
            PpuOnly = reader.ReadUInt32();

            Lods = new[]
            {
                LodHigh.Item,
                LodMed.Item,
                LodLow.Item,
                LodVlow.Item
            };

            if (LodHigh.Item != null) LodHigh.Item.LodDist = LodDistHigh;
            if (LodMed.Item != null) LodMed.Item.LodDist = LodDistMed;
            if (LodLow.Item != null) LodLow.Item.LodDist = LodDistLow;
            if (LodVlow.Item != null) LodVlow.Item.LodDist = LodDistVlow;

            UpdateAllModels();
            AssignShaders();
            SetSkeleton(SkeletonRef.Item);
            CreateTexturePack(reader.FileEntry);

            BoundingBox = new BoundingBox(BoundingBoxMin, BoundingBoxMax);
            BoundingSphere = new BoundingSphere(BoundingBox.Center, BoundingSphereRadius);
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x00E63DF0 : (uint)VFT);
            writer.WritePtr(BlockMap);
            writer.WritePtr(ShaderGroup);
            writer.WritePtr(SkeletonRef);
            writer.WriteVector3(BoundingCenter);
            writer.WriteSingle(Unknown_1Ch);
            writer.WriteVector3(BoundingBoxMin);
            writer.WriteSingle(Unknown_2Ch);
            writer.WriteVector3(BoundingBoxMax);
            writer.WriteSingle(Unknown_3Ch);
            writer.WritePtr(LodHigh);
            writer.WritePtr(LodMed);
            writer.WritePtr(LodLow);
            writer.WritePtr(LodVlow);
            writer.WriteSingle(LodDistHigh);
            writer.WriteSingle(LodDistMed);
            writer.WriteSingle(LodDistLow);
            writer.WriteSingle(LodDistVlow);
            writer.WriteUInt32(DrawBucketMaskHigh);
            writer.WriteUInt32(DrawBucketMaskMed);
            writer.WriteUInt32(DrawBucketMaskLow);
            writer.WriteUInt32(DrawBucketMaskVlow);
            writer.WriteSingle(BoundingSphereRadius);
            writer.WriteUInt32(PpuOnly);
        }

        public void ReadXml(XmlNode node, string ddsfolder)
        {
            BoundingCenter = Rpf6Crypto.GetXmlVector3(node, "BoundingSphereCenter");
            BoundingBoxMin = Rpf6Crypto.GetXmlVector3(node, "BoundingBoxMin");
            BoundingBoxMax = Rpf6Crypto.GetXmlVector3(node, "BoundingBoxMax");
            BoundingSphereRadius = Xml.GetChildFloatAttribute(node, "BoundingSphereRadius", "value");
            LodDistHigh = Xml.GetChildFloatAttribute(node, "LodDistHigh", "value");
            LodDistMed = Xml.GetChildFloatAttribute(node, "LodDistMed", "value");
            LodDistLow = Xml.GetChildFloatAttribute(node, "LodDistLow", "value");
            LodDistVlow = Xml.GetChildFloatAttribute(node, "LodDistVlow", "value");
            DrawBucketMaskHigh = Xml.GetChildUIntAttribute(node, "FlagsHigh", "value");
            DrawBucketMaskMed = Xml.GetChildUIntAttribute(node, "FlagsMed", "value");
            DrawBucketMaskLow = Xml.GetChildUIntAttribute(node, "FlagsLow", "value");
            DrawBucketMaskVlow = Xml.GetChildUIntAttribute(node, "FlagsVlow", "value");

            BoundingBoxMin = new Vector3(BoundingBoxMin.X, BoundingBoxMin.Y + 1.0f, BoundingBoxMin.Z);
            BoundingBoxMax = new Vector3(BoundingBoxMax.X, BoundingBoxMax.Y + 1.0f, BoundingBoxMax.Z);
            BoundingCenter = new Vector3(BoundingCenter.X, BoundingCenter.Y + 1.0f, BoundingCenter.Z);

            var sgnode = node.SelectSingleNode("ShaderGroup");
            if (sgnode != null)
            {
                var group = new Rsc6ShaderGroup();
                group.ReadXml(sgnode, ddsfolder);
                ShaderGroup = new Rsc6Ptr<Rsc6ShaderGroup>(group);
            }

            var sknode = node.SelectSingleNode("Skeleton");
            if (sknode != null)
            {
                var skeleton = new Rsc6Skeleton();
                skeleton.ReadXml(sknode);
                SkeletonRef = new Rsc6Ptr<Rsc6Skeleton>(skeleton);
            }

            var highNode = node.SelectSingleNode("DrawableModelsHigh");
            if (highNode != null)
            {
                var hlod = new Rsc6DrawableLod();
                hlod.ReadXml(highNode);
                LodHigh = new Rsc6Ptr<Rsc6DrawableLod>(hlod);
            }

            Lods = new[]
            {
                LodHigh.Item,
                LodMed.Item,
                LodLow.Item,
                LodVlow.Item
            };

            UpdateAllModels();
            AssignShaders();
            SetSkeleton(SkeletonRef.Item);
        }

        public virtual void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            var bbmin = new Vector3(BoundingBoxMin.X, BoundingBoxMin.Y, BoundingBoxMin.Z - 1.0f);
            var bbmax = new Vector3(BoundingBoxMax.X, BoundingBoxMax.Y, BoundingBoxMax.Z - 1.0f);
            var center = new Vector3(BoundingCenter.X, BoundingCenter.Y, BoundingCenter.Z - 1.0f);

            Xml.SelfClosingTag(sb, indent, "BoundingSphereCenter " + FloatUtil.GetVector3XmlString(center));
            Xml.ValueTag(sb, indent, "BoundingSphereRadius", FloatUtil.ToString(BoundingSphereRadius));
            Xml.SelfClosingTag(sb, indent, "BoundingBoxMin " + FloatUtil.GetVector3XmlString(bbmin));
            Xml.SelfClosingTag(sb, indent, "BoundingBoxMax " + FloatUtil.GetVector3XmlString(bbmax));
            Xml.ValueTag(sb, indent, "LodDistHigh", FloatUtil.ToString(LodDistHigh));
            Xml.ValueTag(sb, indent, "LodDistMed", FloatUtil.ToString(LodDistMed));
            Xml.ValueTag(sb, indent, "LodDistLow", FloatUtil.ToString(LodDistLow));
            Xml.ValueTag(sb, indent, "LodDistVlow", FloatUtil.ToString(LodDistVlow));
            Xml.ValueTag(sb, indent, "FlagsHigh", DrawBucketMaskHigh.ToString());
            Xml.ValueTag(sb, indent, "FlagsMed", DrawBucketMaskMed.ToString());
            Xml.ValueTag(sb, indent, "FlagsLow", DrawBucketMaskLow.ToString());
            Xml.ValueTag(sb, indent, "FlagsVlow", DrawBucketMaskVlow.ToString());
            Xml.ValueTag(sb, indent, "PpuOnly", PpuOnly.ToString());

            if (ShaderGroup.Item != null)
            {
                Xml.OpenTag(sb, indent, "ShaderGroup");
                ShaderGroup.Item.WriteXml(sb, indent + 1, ddsfolder);
                Xml.CloseTag(sb, indent, "ShaderGroup");
            }

            if (SkeletonRef.Item != null)
            {
                Xml.OpenTag(sb, indent, "Skeleton");
                SkeletonRef.Item.WriteXml(sb, indent + 1);
                Xml.CloseTag(sb, indent, "Skeleton");
            }

            if (LodHigh.Item != null)
            {
                Xml.OpenTag(sb, indent, "DrawableModelsHigh");
                LodHigh.Item.WriteXml(sb, indent + 1, BoundingCenter);
                Xml.CloseTag(sb, indent, "DrawableModelsHigh");
            }

            if (LodMed.Item != null)
            {
                Xml.OpenTag(sb, indent, "DrawableModelsMedium");
                LodMed.Item.WriteXml(sb, indent + 1, BoundingCenter);
                Xml.CloseTag(sb, indent, "DrawableModelsMedium");
            }

            if (LodLow.Item != null)
            {
                Xml.OpenTag(sb, indent, "DrawableModelsLow");
                LodLow.Item.WriteXml(sb, indent + 1, BoundingCenter);
                Xml.CloseTag(sb, indent, "DrawableModelsLow");
            }

            if (LodVlow.Item != null)
            {
                Xml.OpenTag(sb, indent, "DrawableModelsVeryLow");
                LodVlow.Item.WriteXml(sb, indent + 1, BoundingCenter);
                Xml.CloseTag(sb, indent, "DrawableModelsVeryLow");
            }
        }

        public void AssignShaders()
        {
            //Assign embedded textures to mesh for rendering
            if ((ShaderGroup.Item?.Shaders.Items != null) && (AllModels != null))
            {
                var shaders = ShaderGroup.Item?.Shaders.Items;
                for (int i = 0; i < AllModels.Length; i++)
                {
                    var model = AllModels[i];
                    if (model.Meshes != null)
                    {
                        for (int j = 0; j < model.Meshes.Length; j++)
                        {
                            var mesh = model.Meshes[j] as Rsc6DrawableGeometry;
                            if (mesh != null)
                            {
                                var shader = (mesh.ShaderID < shaders.Length) ? shaders[mesh.ShaderID] : null;
                                mesh.SetShader(shader);
                            }
                        }
                    }
                }
            }

        }

        public void SetSkeleton(Rsc6Skeleton skel)
        {
            Skeleton = skel;
            if (AllModels != null)
            {
                var bones = skel?.Bones;
                foreach (Rsc6DrawableModel model in AllModels)
                {
                    var boneidx = model.MatrixIndex;
                    if ((model.SkinFlag == 0) && (bones != null) && (boneidx < bones.Length))
                    {
                        if (model.Meshes != null)
                        {
                            foreach (var mesh in model.Meshes)
                            {
                                mesh.BoneIndex = boneidx;
                            }
                        }
                    }
                }
            }
        }

        private void CreateTexturePack(GameArchiveEntry e)
        {
            var txd = WfdFile.TextureDictionary.Item;
            if (txd == null) return;

            var txp = new TexturePack(e) { Textures = new Dictionary<string, Texture>() };
            var texs = txd.Textures.Items;
            if (texs != null)
            {
                for (int i = 0; i < texs.Length; i++)
                {
                    var tex = texs[i];
                    if (tex == null) continue;
                    txp.Textures[tex.Name] = tex;
                    tex.Pack = txp;
                }
            }
            TexturePack = txp;
        }
    }

    [TC(typeof(EXP))] public class Rsc6Skeleton : Skeleton, Rsc6Block
    {
        public ulong BlockLength => 68;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Rsc6RawLst<Rsc6Bone> BoneData { get; set; } //m_Bones, rage::crBoneData
        public Rsc6RawArr<int> ParentIndices { get; set; } //m_ParentIndices
        public Rsc6RawArr<Matrix4x4> JointScaleOrients { get; set; } //m_CumulativeJointScaleOrients, mostly NULL
        public Rsc6RawArr<Matrix4x4> InverseJointScaleOrients { get; set; } //m_CumulativeInverseJointScaleOrients, mostly NULL
        public Rsc6RawArr<Matrix4x4> DefaultTransforms { get; set; } //m_DefaultTransforms
        public Rsc6RawArr<Matrix4x4> CumulativeDefaultTransforms { get; set; } //m_CumulativeDefaultTransforms
        public ushort BoneCount { get; set; } // m_NumBones
        public ushort NumTranslationDofs { get; set; } //m_NumTranslationDofs
        public ushort NumRotationDofs { get; set; } //m_NumRotationDofs
        public ushort NumScaleDofs { get; set; } //m_NumScaleDofs
        public uint Flags { get; set; } = 10; //m_Flags, seems to be mostly 10, sometimes 9 for .wft or 14
        public Rsc6CustomArr<Rsc6SkeletonBoneTag> BoneIDs { get; set; } //m_BoneIdTable, rage::crSkeletonData
        public uint RefCount { get; set; } = 1; //m_RefCount
        public uint Signature { get; set; } = 2135087653; //m_Signature
        public Rsc6Str JointDataFileName { get; set; } //m_JointDataFileName, always NULL?
        public uint JointData { get; set; } = 15649020; //m_JointData, rage::crJointDataFile, no idea? 15649020 is the most common value with 13814012 and 16435452
        public uint Unknown6 { get; set; } //Always 0
        public uint Unknown7 { get; set; } //Always 0

        public void Read(Rsc6DataReader reader)
        {
            BoneData = reader.ReadRawLstPtr<Rsc6Bone>();
            ParentIndices = reader.ReadRawArrPtr<int>();
            JointScaleOrients = reader.ReadRawArrPtr<Matrix4x4>();
            InverseJointScaleOrients = reader.ReadRawArrPtr<Matrix4x4>();
            DefaultTransforms = reader.ReadRawArrPtr<Matrix4x4>();
            CumulativeDefaultTransforms = reader.ReadRawArrPtr<Matrix4x4>();
            BoneCount = reader.ReadUInt16();
            NumTranslationDofs = reader.ReadUInt16();
            NumRotationDofs = reader.ReadUInt16();
            NumScaleDofs = reader.ReadUInt16();
            Flags = reader.ReadUInt32();
            BoneIDs = reader.ReadArr<Rsc6SkeletonBoneTag>();
            RefCount = reader.ReadUInt32();
            Signature = reader.ReadUInt32();
            JointDataFileName = reader.ReadStr();
            JointData = reader.ReadUInt32();
            Unknown6 = reader.ReadUInt32();
            Unknown7 = reader.ReadUInt32();

            BoneData = reader.ReadRawLstItems(BoneData, BoneCount);
            ParentIndices = reader.ReadRawArrItems(ParentIndices, BoneCount);
            JointScaleOrients = reader.ReadRawArrItems(JointScaleOrients, BoneCount);
            InverseJointScaleOrients = reader.ReadRawArrItems(InverseJointScaleOrients, BoneCount);
            DefaultTransforms = reader.ReadRawArrItems(DefaultTransforms, BoneCount);
            CumulativeDefaultTransforms = reader.ReadRawArrItems(CumulativeDefaultTransforms, BoneCount);
            Bones = BoneData.Items;

            for (uint i = 0; i < BoneCount; i++)
            {
                var b = (Rsc6Bone)Bones[i];
                b.ParentIndex = (ParentIndices.Items != null) ? ParentIndices.Items[i] : 0;
                b.JointScaleOrients = (JointScaleOrients.Items != null) ? JointScaleOrients.Items[i] : Matrix4x4.Identity;
                b.InverseJointScaleOrients = (InverseJointScaleOrients.Items != null) ? InverseJointScaleOrients.Items[i] : Matrix4x4.Identity;
                b.DefaultTransforms = (DefaultTransforms.Items != null) ? DefaultTransforms.Items[i] : Matrix4x4.Identity;
                Bones[i] = b;
            }

            for (uint i = 0; i < BoneCount; i++)
            {
                var bone = (Rsc6Bone)Bones[i];
                var ns = bone.NextSibling;
                var fc = bone.FirstChild;
                var pr = bone.ParentRef;

                if (reader.BlockPool.TryGetValue(ns.Position, out var nsi))
                    ns.Item = nsi as Rsc6Bone;
                if (reader.BlockPool.TryGetValue(fc.Position, out var fci))
                    fc.Item = fci as Rsc6Bone;
                if (reader.BlockPool.TryGetValue(pr.Position, out var pri))
                    pr.Item = pri as Rsc6Bone;

                bone.NextSibling = ns;
                bone.FirstChild = fc;
                bone.ParentRef = pr;
                bone.Parent = pr.Item;
            }

            var bonesSorted = Bones.ToList();
            for (int i = 0; i < bonesSorted.Count; i++)
            {
                var bone = bonesSorted[i];
                bone.UpdateAnimTransform();
                bone.AbsTransform = bone.AnimTransform;
                bone.BindTransformInv = Matrix4x4.Identity;
                bone.BindTransformInv.M44 = 1.0f;
                bone.UpdateSkinTransform();
            }

            UpdateBoneTransforms();
            BuildBonesDictionary();
            BuildBoneTags();

            ///////////////////// Tests /////////////////
            if ((Flags != 9 && Flags != 10 && Flags != 14) || Unknown6 != 0 || Unknown7 != 0)
            {
                throw new Exception($"Unknown values, flags : {Flags}");
            }
        }

        public void Write(Rsc6DataWriter writer)
        {
            var bd = new Rsc6SkeletonBoneData(BoneData.Items);
            writer.WriteBlock(bd);
            writer.WritePtrEmbed(bd, bd, 0);
            writer.WriteRawArrPtr(ParentIndices);
            writer.WriteRawArrPtr(JointScaleOrients);
            writer.WriteRawArrPtr(InverseJointScaleOrients);
            writer.WriteRawArrPtr(DefaultTransforms);
            writer.WriteRawArrPtr(CumulativeDefaultTransforms);
            writer.WriteUInt16(BoneCount);
            writer.WriteUInt16(NumTranslationDofs);
            writer.WriteUInt16(NumRotationDofs);
            writer.WriteUInt16(NumScaleDofs);
            writer.WriteUInt32(Flags);
            writer.WriteArr(BoneIDs);
            writer.WriteUInt32(RefCount);
            writer.WriteUInt32(Signature);
            writer.WriteStr(JointDataFileName);
            writer.WriteUInt32(JointData);
            writer.WriteUInt32(Unknown6);
            writer.WriteUInt32(Unknown7);
        }

        public void ReadXml(XmlNode node)
        {
            NumTranslationDofs = (ushort)Xml.GetChildIntAttribute(node, "NumTranslationDofs", "value");
            NumRotationDofs = (ushort)Xml.GetChildIntAttribute(node, "NumRotationDofs", "value");
            NumScaleDofs = (ushort)Xml.GetChildIntAttribute(node, "NumScaleDofs", "value");
            Flags = Xml.GetChildUIntAttribute(node, "Flags", "value");
            Signature = Xml.GetChildUIntAttribute(node, "Signature", "value");
            JointData = Xml.GetChildUIntAttribute(node, "JointData", "value");

            var snode = node.SelectSingleNode("Bones");
            if (snode != null)
            {
                var iNodes = snode.SelectNodes("Item");
                if (iNodes != null)
                {
                    var bones = new List<Rsc6Bone>();
                    foreach (XmlNode inode in iNodes)
                    {
                        var bone = new Rsc6Bone();
                        bone.ReadXml(inode);
                        bones.Add(bone);
                    }

                    BoneData = new Rsc6RawLst<Rsc6Bone>(bones.ToArray());
                    Bones = BoneData.Items;
                    BoneCount = (ushort)(BoneData.Items?.Length ?? 0);
                }
            }

            BuildIndices();
            AssignBoneParents();
            BuildTransformations();
            BuildBoneTags();
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            Xml.ValueTag(sb, indent, "NumTranslationDofs", NumTranslationDofs.ToString());
            Xml.ValueTag(sb, indent, "NumRotationDofs", NumRotationDofs.ToString());
            Xml.ValueTag(sb, indent, "NumScaleDofs", NumScaleDofs.ToString());
            Xml.ValueTag(sb, indent, "Flags", Flags.ToString());
            Xml.ValueTag(sb, indent, "Signature", Signature.ToString());
            Xml.ValueTag(sb, indent, "JointData", JointData.ToString());

            if (Bones != null)
            {
                Xml.OpenTag(sb, indent, "Bones");
                foreach (Rsc6Bone bone in Bones.Cast<Rsc6Bone>())
                {
                    Xml.OpenTag(sb, indent + 1, "Item");
                    bone.WriteXml(sb, indent + 2);
                    Xml.CloseTag(sb, indent + 1, "Item");
                }
                Xml.CloseTag(sb, indent, "Bones");
            }
        }

        public void BuildBoneTags()
        {
            var tags = new List<Rsc6SkeletonBoneTag>();
            var bones = BoneData.Items;

            if (bones != null)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    var bone = bones[i];
                    var tag = new Rsc6SkeletonBoneTag
                    {
                        BoneTag = bone.BoneId,
                        BoneIndex = (ushort)i
                    };
                    tags.Add(tag);
                }
            }

            var skip = tags.Count < 1;
            if (tags.Count == 1)
            {
                var t0 = tags[0];
                skip = t0.BoneTag == 0;
            }

            if (skip)
            {
                BoneIDs = new();
                return;
            }

            tags = tags.OrderBy(tag => tag.BoneIndex).ToList();
            BoneIDs = new(tags.ToArray());
        }

        public void BuildTransformations()
        {
            var transforms = new List<Matrix4x4>();
            var cumulativeTransforms = new List<Matrix4x4>();
            if (Bones != null)
            {
                foreach (var bone in Bones.Cast<Rsc6Bone>())
                {
                    var pos = bone.Position;
                    var ori = bone.Rotation;
                    var sca = bone.Scale;

                    var localTransform = Matrix4x4Ext.Transformation(sca, ori, pos);
                    var cumulativeTransform = localTransform;

                    var pbone = bone.Parent;
                    while (pbone != null)
                    {
                        var parentPos = pbone.Position;
                        var parentOri = pbone.Rotation;
                        var parentSca = pbone.Scale;
                        var parentTransform = Matrix4x4Ext.Transformation(parentSca, parentOri, parentPos);
                        cumulativeTransform = parentTransform * cumulativeTransform;

                        bone.AbsolutePosition = new Vector4(cumulativeTransform.Translation, 0.0f);
                        pos = pbone.Rotation.Multiply(pos) + pbone.Position;
                        ori = pbone.Rotation * ori;
                        pbone = pbone.Parent;
                    }
                    transforms.Add(localTransform);
                    cumulativeTransforms.Add(cumulativeTransform);
                }
            }
            DefaultTransforms = (transforms.Count > 0) ? new(transforms.ToArray()) : new(null);
            CumulativeDefaultTransforms = (cumulativeTransforms.Count > 0) ? new(cumulativeTransforms.ToArray()) : new(null);
        }

        public void BuildIndices()
        {
            var parents = new List<int>();
            var bones = BoneData.Items;

            if (bones != null)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    var bone = bones[i];
                    var pind = bone.ParentIndex;
                    parents.Add(pind);
                }
            }
            ParentIndices = new((parents.Count > 0) ? parents.ToArray() : null);
        }

        public void AssignBoneParents()
        {
            var bones = BoneData.Items;
            var pinds = ParentIndices.Items;

            if ((bones != null) && (pinds != null))
            {
                var maxcnt = Math.Min(bones.Length, pinds.Length);
                for (int i = 0; i < maxcnt; i++)
                {
                    var bone = bones[i];
                    var pind = pinds[i];
                    var sibling = bone.SiblingIndex;
                    var child = bone.ChildIndex;

                    if ((pind >= 0) && (pind < bones.Length))
                    {
                        bone.Parent = bones[pind];
                        bone.ParentRef = new Rsc6Ptr<Rsc6Bone>(bones[pind]);
                    }

                    if (sibling >= 0)
                    {
                        bone.NextSibling = new Rsc6Ptr<Rsc6Bone>(bones[sibling]);
                    }

                    if (child >= 0)
                    {
                        bone.FirstChild = new Rsc6Ptr<Rsc6Bone>(bones[child]);
                    }
                }

                /*for (uint i = 0; i < BoneCount; i++)
                {
                    var bone = bones[i];
                    var ns = bone.NextSibling;
                    var fc = bone.FirstChild;
                    var pr = bone.ParentRef;

                    if (reader.BlockPool.TryGetValue(ns.Position, out var nsi))
                        ns.Item = nsi as Rsc6Bone;
                    if (reader.BlockPool.TryGetValue(fc.Position, out var fci))
                        fc.Item = fci as Rsc6Bone;
                    if (reader.BlockPool.TryGetValue(pr.Position, out var pri))
                        pr.Item = pri as Rsc6Bone;

                    bone.NextSibling = ns;
                    bone.FirstChild = fc;
                    bone.ParentRef = pr;
                    bone.Parent = pr.Item;
                }*/
            }
        }

        public void BuildBindMatrices()
        {
            var mats = new List<Matrix4x4>();
            var matsinv = new List<Matrix4x4>();
            var bones = BoneData.Items;

            if (bones != null)
            {
                foreach (var bone in bones)
                {
                    var pos = bone.Position;
                    var ori = bone.Rotation;
                    var sca = bone.Scale;
                    var m = Matrix4x4Ext.Transformation(sca, ori, pos);

                    var pbone = bone.Parent;
                    while (pbone != null)
                    {
                        pos = pbone.Rotation.Multiply(pos) + pbone.Position;
                        ori = pbone.Rotation * ori;
                        pbone = pbone.Parent;
                    }
                    var m2 = Matrix4x4Ext.Transformation(sca, ori, pos);
                    var mi = Matrix4x4Ext.Invert(m2);
                    mi.Column4(Vector4.Zero);

                    mats.Add(m);
                    matsinv.Add(mi);
                }
            }
            JointScaleOrients = new((mats.Count > 0) ? mats.ToArray() : null);
            InverseJointScaleOrients = new((matsinv.Count > 0) ? matsinv.ToArray() : null);
        }
    }

    [TC(typeof(EXP))] public class Rsc6Bone : Bone, Rsc6Block //rage::crBoneData
    {
        public ulong BlockLength => 224;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Rsc6Str NameStr { get; set; } //m_Name
        public uint Dofs { get; set; } //m_Dofs
        public Rsc6Ptr<Rsc6Bone> NextSibling { get; set; } //m_Next
        public Rsc6Ptr<Rsc6Bone> FirstChild { get; set; } //m_Child
        public Rsc6Ptr<Rsc6Bone> ParentRef { get; set; } //m_Parent
        public ushort BoneId { get; set; } //m_BoneId
        public ushort MirrorIndex { get; set; } //m_MirrorIndex
        public byte NumTransChannels { get; set; } //m_NumTransChannels, related to TranslationMin and TranslationMax
        public byte NumRotChannels { get; set; } //m_NumRotChannels, related to RotationMin and RotationMax
        public byte NumScaleChannels { get; set; } //m_NumScaleChannels, related to OrigScale
        public ushort Unknown_1Dh { get; set; } //Pad
        public byte Unknown_1Fh { get; set; } //Pad
        public Vector4 OrigPosition { get; set; } //m_DefaultTranslation
        public Vector4 OrigRotationEuler { get; set; } //m_DefaultRotation
        public Vector4 OrigRotation { get; set; } //m_DefaultRotationQuat
        public Vector4 OrigScale { get; set; } //m_DefaultScale
        public Vector4 AbsolutePosition { get; set; } //m_GlobalOffset, depending on Dofs, ParentRef->m_vOffset or m_vOffset transformed to the model space
        public Vector4 AbsoluteRotationEuler { get; set; } //m_JointOrient
        public Vector4 ScaleOrient { get; set; } //m_ScaleOrient
        public Vector4 TranslationMin { get; set; } //m_TransMin
        public Vector4 TranslationMax { get; set; } //m_TransMax
        public Vector4 RotationMin { get; set; } //m_RotMin
        public Vector4 RotationMax { get; set; } //m_RotMax
        public uint JointData { get; set; } //m_JointData, always 0
        public JenkHash NameHash { get; set; } //m_NameHash
        public uint Unknown_D8h { get; set; } //Always 0
        public uint Unknown_DCh { get; set; } //Always 0

        public int SiblingIndex { get; set; }
        public int ChildIndex { get; set; }
        public int ParentIndex { get; set; }
        public Matrix4x4 JointScaleOrients { get; set; }
        public Matrix4x4 InverseJointScaleOrients { get; set; }
        public Matrix4x4 DefaultTransforms { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            NameStr = reader.ReadStr();
            Dofs = reader.ReadUInt32();
            NextSibling = new Rsc6Ptr<Rsc6Bone>() { Position = reader.ReadUInt32() };
            FirstChild = new Rsc6Ptr<Rsc6Bone>() { Position = reader.ReadUInt32() };
            ParentRef = new Rsc6Ptr<Rsc6Bone>() { Position = reader.ReadUInt32() };
            Index = reader.ReadUInt16();
            BoneId = reader.ReadUInt16();
            MirrorIndex = reader.ReadUInt16();
            NumTransChannels = reader.ReadByte();
            NumRotChannels = reader.ReadByte();
            NumScaleChannels = reader.ReadByte();
            Unknown_1Dh = reader.ReadUInt16();
            Unknown_1Fh = reader.ReadByte();
            OrigPosition = reader.ReadVector4(true);
            OrigRotationEuler = reader.ReadVector4(true);
            OrigRotation = reader.ReadVector4(true);
            OrigScale = reader.ReadVector4(true);
            AbsolutePosition = reader.ReadVector4(true);
            AbsoluteRotationEuler = reader.ReadVector4(true);
            ScaleOrient = reader.ReadVector4(true);
            TranslationMin = reader.ReadVector4(true);
            TranslationMax = reader.ReadVector4(true);
            RotationMin = reader.ReadVector4(true);
            RotationMax = reader.ReadVector4(true);
            JointData = reader.ReadUInt32();
            NameHash = reader.ReadUInt32();
            Unknown_D8h = reader.ReadUInt32();
            Unknown_DCh = reader.ReadUInt32();

            Name = NameStr.Value;
            Position = OrigPosition.XYZ();
            Rotation = OrigRotation.ToQuaternion();
            Scale = Vector3.One;

            AnimRotation = Rotation;
            AnimTranslation = Position;
            AnimScale = Scale;
        }
        
        public void Write(Rsc6DataWriter writer)
        {
            Rsc6Bone parent = null, child = null, sibling = null;
            var bdata = writer.BlockList.OfType<Rsc6SkeletonBoneData>().FirstOrDefault();

            if (bdata != null)
            {
                if (NextSibling.Item != null)
                    sibling = bdata.Bones.FirstOrDefault(b => string.Equals(b.Name, NextSibling.Item.Name));
                if (FirstChild.Item != null)
                    child = bdata.Bones.FirstOrDefault(b => string.Equals(b.Name, FirstChild.Item.Name));
                if (ParentRef.Item != null)
                    parent = bdata.Bones.FirstOrDefault(b => string.Equals(b.Name, ParentRef.Item.Name));
            }

            writer.WriteStr(NameStr);
            writer.WriteUInt32(Dofs);
            writer.WritePtrEmbed(sibling, sibling, (ulong)(224 * sibling?.Index ?? 0));
            writer.WritePtrEmbed(child, child, (ulong)(224 * child?.Index ?? 0));
            writer.WritePtrEmbed(parent, parent, (ulong)(224 * parent?.Index ?? 0));
            writer.WriteUInt16((ushort)Index);
            writer.WriteUInt16(BoneId);
            writer.WriteUInt16(MirrorIndex);
            writer.WriteByte(NumTransChannels);
            writer.WriteByte(NumRotChannels);
            writer.WriteByte(NumScaleChannels);
            writer.WriteUInt16(Unknown_1Dh);
            writer.WriteByte(Unknown_1Fh);
            writer.WriteVector4(OrigPosition);
            writer.WriteVector4(OrigRotationEuler);
            writer.WriteVector4(OrigRotation);
            writer.WriteVector4(OrigScale);
            writer.WriteVector4(AbsolutePosition);
            writer.WriteVector4(AbsoluteRotationEuler);
            writer.WriteVector4(ScaleOrient);
            writer.WriteVector4(TranslationMin);
            writer.WriteVector4(TranslationMax);
            writer.WriteVector4(RotationMin);
            writer.WriteVector4(RotationMax);
            writer.WriteUInt32(JointData);
            writer.WriteUInt32(NameHash);
            writer.WriteUInt32(Unknown_D8h);
            writer.WriteUInt32(Unknown_DCh);
        }

        public void ReadXml(XmlNode node)
        {
            NameStr = new(Xml.GetChildInnerText(node, "Name"));
            Dofs = Xml.GetChildUIntAttribute(node, "Dofs", "value");
            Index = Xml.GetChildIntAttribute(node, "Index", "value");
            BoneId = (ushort)Xml.GetChildIntAttribute(node, "BoneId", "value");
            MirrorIndex = (ushort)Xml.GetChildIntAttribute(node, "MirrorIndex", "value");
            NumTransChannels = (byte)Xml.GetChildIntAttribute(node, "NumTransChannels", "value");
            NumRotChannels = (byte)Xml.GetChildIntAttribute(node, "NumRotChannels", "value");
            NumScaleChannels = (byte)Xml.GetChildIntAttribute(node, "NumScaleChannels", "value");
            OrigPosition = Rpf6Crypto.GetXmlVector4(node, "OrigPosition");
            OrigRotationEuler = Rpf6Crypto.GetXmlVector4(node, "RotationEuler");
            OrigRotation = Rpf6Crypto.GetXmlVector4(node, "Rotation");
            OrigScale = Rpf6Crypto.GetXmlVector4(node, "Scale");
            AbsolutePosition = Rpf6Crypto.GetXmlVector4(node, "AbsolutePosition");
            AbsoluteRotationEuler = Rpf6Crypto.GetXmlVector4(node, "AbsoluteRotationEuler");
            ScaleOrient = Rpf6Crypto.GetXmlVector4(node, "ScaleOrient");
            TranslationMin = Rpf6Crypto.GetXmlVector4(node, "TranslationMin");
            TranslationMax = Rpf6Crypto.GetXmlVector4(node, "TranslationMax");
            RotationMin = Rpf6Crypto.GetXmlVector4(node, "RotationMin");
            RotationMax = Rpf6Crypto.GetXmlVector4(node, "RotationMax");
            JointData = Xml.GetChildUIntAttribute(node, "JointData");
            SiblingIndex = Xml.GetChildIntAttribute(node, "SiblingIndex", "value");
            ChildIndex = Xml.GetChildIntAttribute(node, "ChildIndex", "value");
            ParentIndex = Xml.GetChildIntAttribute(node, "ParentIndex", "value");

            Name = NameStr.Value;
            NameHash = new JenkHash(Name);
            Position = OrigPosition.XYZ();
            Rotation = OrigRotation.ToQuaternion();

            Scale = Vector3.One;
            AnimRotation = Rotation;
            AnimTranslation = Position;
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            Xml.StringTag(sb, indent, "Name", Name);
            Xml.ValueTag(sb, indent, "Dofs", Dofs.ToString());
            Xml.ValueTag(sb, indent, "Index", Index.ToString());
            Xml.ValueTag(sb, indent, "BoneId", BoneId.ToString());
            Xml.ValueTag(sb, indent, "MirrorIndex", MirrorIndex.ToString());
            Xml.ValueTag(sb, indent, "NumTransChannels", NumTransChannels.ToString());
            Xml.ValueTag(sb, indent, "NumRotChannels", NumRotChannels.ToString());
            Xml.ValueTag(sb, indent, "NumScaleChannels", NumScaleChannels.ToString());
            Xml.SelfClosingTag(sb, indent, "OrigPosition " + FloatUtil.GetVector4XmlString(OrigPosition));
            Xml.SelfClosingTag(sb, indent, "RotationEuler " + FloatUtil.GetVector4XmlString(OrigRotationEuler));
            Xml.SelfClosingTag(sb, indent, "Rotation " + FloatUtil.GetVector4XmlString(OrigRotation));
            Xml.SelfClosingTag(sb, indent, "Scale " + FloatUtil.GetVector4XmlString(OrigScale));
            Xml.SelfClosingTag(sb, indent, "AbsolutePosition " + FloatUtil.GetVector4XmlString(AbsolutePosition));
            Xml.SelfClosingTag(sb, indent, "AbsoluteRotationEuler " + FloatUtil.GetVector4XmlString(AbsoluteRotationEuler));
            Xml.SelfClosingTag(sb, indent, "ScaleOrient " + FloatUtil.GetVector4XmlString(ScaleOrient));
            Xml.SelfClosingTag(sb, indent, "TranslationMin " + FloatUtil.GetVector4XmlString(TranslationMin));
            Xml.SelfClosingTag(sb, indent, "TranslationMax " + FloatUtil.GetVector4XmlString(TranslationMax));
            Xml.SelfClosingTag(sb, indent, "RotationMin " + FloatUtil.GetVector4XmlString(RotationMin));
            Xml.SelfClosingTag(sb, indent, "RotationMax " + FloatUtil.GetVector4XmlString(RotationMax));
            Xml.ValueTag(sb, indent, "JointData", JointData.ToString());
            Xml.ValueTag(sb, indent, "SiblingIndex", (NextSibling.Item == null) ? "-1" : NextSibling.Item.Index.ToString());
            Xml.ValueTag(sb, indent, "ChildIndex", FirstChild.Item == null ? "-1" : FirstChild.Item.Index.ToString());
            Xml.ValueTag(sb, indent, "ParentIndex", Parent == null ? "-1" : Parent.Index.ToString());
        }

        public ulong GetSignatureNonChiral()
        {
            return ((ulong)BoneId << 32) |
                   ((Dofs & ((uint)Rsc6DoFs.TRANSLATE_X | (uint)Rsc6DoFs.TRANSLATE_Y | (uint)Rsc6DoFs.TRANSLATE_Z)) != 0 ? 0x1UL : 0x0UL) |
                   ((Dofs & ((uint)Rsc6DoFs.ROTATE_X | (uint)Rsc6DoFs.ROTATE_Y | (uint)Rsc6DoFs.ROTATE_Z)) != 0 ? 0x2UL : 0x0UL) |
                   ((Dofs & ((uint)Rsc6DoFs.SCALE_X | (uint)Rsc6DoFs.SCALE_Y | (uint)Rsc6DoFs.SCALE_Z)) != 0 ? 0x4UL : 0x0UL);
        }
    }

    [TC(typeof(EXP))] public class Rsc6SkeletonBoneTag : Rsc6BlockBase //rage::crSkeletonData::BoneIdData
    {
        public override ulong BlockLength => 4;
        public ushort BoneTag { get; set; }
        public ushort BoneIndex { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            BoneTag = reader.ReadUInt16();
            BoneIndex = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt16(BoneTag);
            writer.WriteUInt16(BoneIndex);
        }

        public override string ToString()
        {
            return $"{BoneTag} : {BoneIndex}";
        }
    }

    [TC(typeof(EXP))] public class Rsc6SkeletonBoneData : Rsc6BlockBase
    {
        public override ulong BlockLength => BonesCount * 224;
        public uint BonesCount { get; set; }
        public Rsc6Bone[] Bones { get; set; }

        public Rsc6SkeletonBoneData()
        {
        }

        public Rsc6SkeletonBoneData(Rsc6Bone[] bones)
        {
            Bones = bones;
            BonesCount = (uint)(bones?.Length ?? 0);
        }

        public override void Read(Rsc6DataReader reader)
        {
            //Only use this for writing BoneData
        }

        public override void Write(Rsc6DataWriter writer)
        {
            if (Bones != null)
            {
                foreach (var bone in Bones)
                {
                    bone.Write(writer);
                }
            }
        }
    }

    [TC(typeof(EXP))] public class Rsc6ShaderGroup : Rsc6BlockBase
    {
        public override ulong BlockLength => 32;
        public uint VFT { get; set; } = 0x0184A26C;
        public Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary { get; set; } //m_TextureDictionary
        public Rsc6PtrArr<Rsc6ShaderFX> Shaders { get; set; } //m_Shaders
        public uint Unknown_10h { get; set; } //m_ShaderGroupVars[]
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; } //m_ShaderGroupHashes[]
        public uint Unknown_1Ch { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>();
            Shaders = reader.ReadPtrArr<Rsc6ShaderFX>();
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x00EEB23C : VFT);
            writer.WriteUInt32(0); //Unused
            writer.WritePtrArr(Shaders);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(Unknown_1Ch);
        }

        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var tnode = node.SelectSingleNode("TextureDictionary");
            WtdFile wtd = null;
            if (tnode != null)
            {
                wtd = new WtdFile();
                wtd.ReadXml(tnode, ddsfolder);
                TextureDictionary = new Rsc6Ptr<Rsc6TextureDictionary>(wtd.TextureDictionary);
            }

            var snode = node.SelectSingleNode("Shaders");
            if (snode != null)
            {
                var iNodes = snode.SelectNodes("Item");
                if (iNodes != null)
                {
                    var shaders = new List<Rsc6ShaderFX>();
                    foreach (XmlNode inode in iNodes)
                    {
                        var shader = new Rsc6ShaderFX();
                        shader.ReadXml(inode);

                        foreach (var param in shader?.ParametersList.Item?.Parameters)
                        {
                            if (param.DataType == 0 && param.Texture != null)
                            {
                                var embeddedTex = TextureDictionary.Item?.Textures.Items?.Where(t => t.Name == param.Texture.Name).FirstOrDefault();
                                if (embeddedTex != null)
                                {
                                    param.Texture = embeddedTex;
                                }
                            }
                        }
                        shaders.Add(shader);
                    }
                    Shaders = new Rsc6PtrArr<Rsc6ShaderFX>(shaders.ToArray());
                }
            }

            if ((Shaders.Items != null) && (TextureDictionary.Item != null))
            {
                foreach (var shader in Shaders.Items)
                {
                    var sparams = shader?.ParametersList.Item?.Parameters;
                    if (sparams != null)
                    {
                        foreach (var sparam in sparams)
                        {
                            if (sparam.Texture != null && wtd != null)
                            {
                                var tex2 = wtd.Lookup(JenkHash.GenHash(sparam.Texture.NameRef.Value));
                                if (tex2 != null)
                                {
                                    sparam.Texture = tex2; //Swap the parameter out for the embedded texture
                                }
                            }
                        }
                    }
                }
            }
        }

        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            var shaders = Shaders.Items;
            if (shaders != null)
            {
                var writtenTextures = new List<string>();
                Xml.OpenTag(sb, indent, "TextureDictionary");
                foreach (var tex in shaders)
                {
                    foreach (var param in tex?.ParametersList.Item?.Parameters)
                    {
                        if (param.DataType != 0 || writtenTextures.Contains(param.Texture?.Name) || param.Texture?.Height == 0) continue;

                        Xml.OpenTag(sb, indent + 1, "Item");
                        param.Texture?.WriteXml(sb, indent + 2, ddsfolder);
                        writtenTextures.Add(param.Texture?.Name);
                        Xml.CloseTag(sb, indent + 1, "Item");
                    }
                }
                Xml.CloseTag(sb, indent, "TextureDictionary");
            }

            Xml.OpenTag(sb, indent, "Shaders");
            if (shaders != null)
            {
                indent += 2;
                foreach (var v in shaders)
                {
                    if (v.Name.Str == string.Empty || v.ParameterCount == 0) continue;

                    Xml.OpenTag(sb, indent - 1, "Item");
                    Xml.StringTag(sb, indent, "Name", v.Name.ToString());
                    Xml.StringTag(sb, indent, "FileName", v.Name.ToString() + ".sps");
                    Xml.ValueTag(sb, indent, "RenderBucket", v.RenderBucket.ToString());
                    Xml.OpenTag(sb, indent, "Parameters");

                    foreach (var p in v.GetParams())
                    {
                        p.WriteXml(sb, indent + 1);
                    }
                    Xml.CloseTag(sb, indent, "Parameters");
                    Xml.CloseTag(sb, indent - 1, "Item");
                }
            }
            Xml.CloseTag(sb, indent, "Shaders");
        }

        private Rsc6ShaderFX ConvertToRDR1(Rsc6ShaderFX shader)
        {
            var s = shader;
            byte textureCount;
            ushort parameterSize;
            ushort parameterDataSize;
            JenkHash[] hashes;
            Rsc6ShaderParameter[] parametersGTAV = s.GetParams();
            Rsc6ShaderParameter[] parametersRDR;

            if (shader.Name.Str == "ped")
            {
                s.Name = JenkHash.GenHash("rdr2_bump_spec_ambocc_shareduv_character_skin");
                s.FileName = new JenkHash(); //FileName seems to always be 0
                parametersRDR = new Rsc6ShaderParameter[27];

                foreach (var p in parametersGTAV)
                {
                    switch (p.Hash.Str)
                    {
                        case "diffusesampler":
                            parametersRDR[0] = p;
                            parametersRDR[0].Hash = new JenkHash("texturesampler");
                            parametersRDR[0].Texture = p.Texture;
                            break;
                        case "specsampler":
                            parametersRDR[1] = p;
                            parametersRDR[1].Hash = new JenkHash("amboccsampler");
                            parametersRDR[1].Texture = p.Texture;
                            break;
                        case "bumpsampler":
                            parametersRDR[2] = p;
                            parametersRDR[2].Hash = new JenkHash("bumpsampler");
                            parametersRDR[2].Texture = p.Texture;
                            break;
                        case "bumpiness":
                            parametersRDR[5] = p;
                            parametersRDR[5].Hash = new JenkHash("bumpiness");
                            parametersRDR[5].Vector = p.Vector;
                            break;
                    }
                }

                if (parametersRDR[1] == null)
                {
                    //amboccsampler
                    parametersRDR[1] = new Rsc6ShaderParameter
                    {
                        Hash = new JenkHash("amboccsampler"),
                        DataType = 0
                    };
                }
                if (parametersRDR[2] == null)
                {
                    //bumpsampler
                    parametersRDR[2] = new Rsc6ShaderParameter
                    {
                        Hash = new JenkHash("bumpsampler"),
                        DataType = 0
                    };
                }

                //detailmapsampler
                parametersRDR[3] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("detailmapsampler"),
                    DataType = 0
                };

                //Colors
                var colorsVectors = new Vector4[]
                {
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0.5f, 0f, 1f),
                    new Vector4(1f, 1f, 0f, 1f),
                    new Vector4(0f, 1f, 0f, 1f),
                    new Vector4(0f, 1f, 1f, 1f),
                    new Vector4(0f, 0f, 1f, 1f),
                    new Vector4(0.5f, 0f, 1f, 1f),
                    new Vector4(1f, 0f, 1f, 1f),
                    new Vector4(1f, 1f, 1f, 1f),
                };
                var colorsParams = new Rsc6Vector4(colorsVectors);
                parametersRDR[4] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("colors"),
                    DataType = 9,
                    Array = colorsParams
                };

                //rimglowscale
                var rimglowscaleParams = new Rsc6Vector4(new Vector4(1f, 0f, 0f, 0f));
                parametersRDR[6] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("rimglowscale"),
                    DataType = 1,
                    Vector = rimglowscaleParams
                };

                //rimamount
                var rimamountParams = new Rsc6Vector4(new Vector4(0.03f, 0f, 0f, 0f));
                parametersRDR[7] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("rimamount"),
                    DataType = 1,
                    Vector = rimamountParams
                };

                //rimpower
                var rimpowerParams = new Rsc6Vector4(new Vector4(2.7f, 0f, 0f, 0f));
                parametersRDR[8] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("rimpower"),
                    DataType = 1,
                    Vector = rimpowerParams
                };

                //specularfresnelmax
                var specularfresnelmaxParams = new Rsc6Vector4(new Vector4(4f, 0f, 0f, 0f));
                parametersRDR[9] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("specularfresnelmax"),
                    DataType = 1,
                    Vector = specularfresnelmaxParams
                };

                //specularfresnelmin
                var specularfresnelminParams = new Rsc6Vector4(new Vector4(0.15f, 0f, 0f, 0f));
                parametersRDR[10] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("specularfresnelmin"),
                    DataType = 1,
                    Vector = specularfresnelminParams
                };

                //specularfresnelexp
                var specularfresnelexpParams = new Rsc6Vector4(new Vector4(2.163f, 0f, 0f, 0f));
                parametersRDR[11] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("specularfresnelexp"),
                    DataType = 1,
                    Vector = specularfresnelexpParams
                };

                //specdiffuseamount
                var specdiffuseamountParams = new Rsc6Vector4(new Vector4(0f, 0f, 0f, 0f));
                parametersRDR[12] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("specdiffuseamount"),
                    DataType = 1,
                    Vector = specdiffuseamountParams
                };

                //specularcolorfactor2
                var specularcolorfactor2Params = new Rsc6Vector4(new Vector4(0.5f, 0f, 0f, 0f));
                parametersRDR[13] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("specularcolorfactor2"),
                    DataType = 1,
                    Vector = specularcolorfactor2Params
                };

                //specularfactor2
                var specularfactor2Params = new Rsc6Vector4(new Vector4(22.9f, 0f, 0f, 0f));
                parametersRDR[14] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("specularfactor2"),
                    DataType = 1,
                    Vector = specularfactor2Params
                };

                //specularcolorfactor
                var specularcolorfactorParams = new Rsc6Vector4(new Vector4(1f, 0f, 0f, 0f));
                parametersRDR[15] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("specularcolorfactor"),
                    DataType = 1,
                    Vector = specularcolorfactorParams
                };

                //specularfactor
                var specularfactorParams = new Rsc6Vector4(new Vector4(20f, 0f, 0f, 0f));
                parametersRDR[16] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("specularfactor"),
                    DataType = 1,
                    Vector = specularfactorParams
                };

                //detailmapscale
                var detailmapscaleParams = new Rsc6Vector4(new Vector4(0.5f, 0f, 0f, 0f));
                parametersRDR[17] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("detailmapscale"),
                    DataType = 1,
                    Vector = detailmapscaleParams
                };

                //detailmapscalev
                var detailmapscalevParams = new Rsc6Vector4(new Vector4(39f, 0f, 0f, 0f));
                parametersRDR[18] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("detailmapscalev"),
                    DataType = 1,
                    Vector = detailmapscalevParams
                };

                //detailmapscalev
                var detailmapscaleuParams = new Rsc6Vector4(new Vector4(54f, 0f, 0f, 0f));
                parametersRDR[19] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("detailmapscaleu"),
                    DataType = 1,
                    Vector = detailmapscaleuParams
                };

                //0xA7B06B29
                var A7B06B29Params = new Rsc6Vector4(new Vector4[] { Vector4.Zero, Vector4.Zero, Vector4.Zero });
                parametersRDR[20] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash(694923431),
                    DataType = 3,
                    Array = A7B06B29Params
                };

                //blursize
                var blursizeParams = new Rsc6Vector4(new Vector4(1f, 0f, 0f, 0f));
                parametersRDR[21] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("blursize"),
                    DataType = 1,
                    Vector = blursizeParams
                };

                //epidermiswieght
                var epidermiswieghtParams = new Rsc6Vector4(new Vector4(0.2f, 0f, 0f, 0f));
                parametersRDR[22] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("epidermiswieght"),
                    DataType = 1,
                    Vector = epidermiswieghtParams
                };

                //epidermiscolor
                var epidermiscolorParams = new Rsc6Vector4(new Vector4(1f, 1f, 1f, 0f));
                parametersRDR[23] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("epidermiscolor"),
                    DataType = 1,
                    Vector = epidermiscolorParams
                };

                //dermiswieght
                var dermiswieghtParams = new Rsc6Vector4(new Vector4(0.2f, 1f, 1f, 0f));
                parametersRDR[24] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("dermiswieght"),
                    DataType = 1,
                    Vector = dermiswieghtParams
                };

                //dermiscolor
                var dermiscolorParams = new Rsc6Vector4(new Vector4(0.8705f, 0.7725f, 0.7647f, 0f));
                parametersRDR[25] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("dermiscolor"),
                    DataType = 1,
                    Vector = dermiscolorParams
                };

                //PhysicsMaterial
                parametersRDR[26] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("physicsmaterial"),
                    DataType = 1,
                    Vector = new Rsc6Vector4(Vector4.Zero)
                };

                hashes = new JenkHash[parametersRDR.Length];
                for (int i = 0; i < hashes.Length; i++)
                {
                    hashes[i] = parametersRDR[i].Hash;
                }

                textureCount = 4;
                parameterSize = 752;
                parameterDataSize = 896;
            }
            else
            {
                s.Name = JenkHash.GenHash("rdr2_low_lod_nodirt"); //Easier shader to handle (single sampler, simple layout)
                s.FileName = new JenkHash(); //FileName seems to always be 0
                parametersRDR = new Rsc6ShaderParameter[3];

                foreach (var p in parametersGTAV)
                {
                    switch (p.Hash.Str)
                    {
                        case "diffusesampler":
                            parametersRDR[0] = p;
                            parametersRDR[0].Hash = new JenkHash("texturesampler");
                            break;
                    }
                }

                //Colors
                var colorsVectors = new Vector4[]
                {
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0.5f, 0f, 1f),
                    new Vector4(1f, 1f, 0f, 1f),
                    new Vector4(0f, 1f, 0f, 1f),
                    new Vector4(0f, 1f, 1f, 1f),
                    new Vector4(0f, 0f, 1f, 1f),
                    new Vector4(0.5f, 0f, 1f, 1f),
                    new Vector4(1f, 0f, 1f, 1f),
                    new Vector4(1f, 1f, 1f, 1f),
                };
                var colorsParams = new Rsc6Vector4(colorsVectors);
                parametersRDR[1] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("colors"),
                    DataType = 9,
                    Array = colorsParams
                };

                //PhysicsMaterial
                parametersRDR[2] = new Rsc6ShaderParameter
                {
                    Hash = new JenkHash("physicsmaterial"),
                    DataType = 1,
                    Vector = new Rsc6Vector4(Vector4.Zero)
                };

                hashes = new JenkHash[parametersRDR.Length];
                for (int i = 0; i < hashes.Length; i++)
                {
                    hashes[i] = parametersRDR[i].Hash;
                }
                textureCount = 1;
                parameterSize = 192;
                parameterDataSize = 240;
            }
            s.SetParams(parametersRDR, hashes);
            s.ParametersList.Item.Count = parametersRDR.Length;
            s.ParameterCount = (byte)parametersRDR.Length;
            s.TextureParametersCount = textureCount;
            s.ParameterSize = parameterSize;
            s.ParameterDataSize = parameterDataSize;
            return s;
        }
    }

    [TC(typeof(EXP))] public class Rsc6ShaderParameter
    {
        public byte RegisterCount { get; set; }
        public byte RegisterIndex { get; set; }
        public byte DataType { get; set; } //0: texture, 1: vector4
        public byte Unknown_3h { get; set; }
        public uint DataPointer { get; set; }
        public JenkHash Hash { get; set; }
        public Rsc6TextureBase Texture { get; set; }
        public Rsc6Vector4 Vector { get; set; }
        public Rsc6Vector4 Array { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            DataType = reader.ReadByte();
            RegisterIndex = reader.ReadByte();
            RegisterCount = reader.ReadByte();
            Unknown_3h = reader.ReadByte();
            DataPointer = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteByte(DataType);
            writer.WriteByte(RegisterIndex);
            writer.WriteByte(RegisterCount);
            writer.WriteByte(Unknown_3h);

            Rsc6TextureBase texture = null;
            if (Texture != null)
            {
                object block = writer.BlockList[0];
                if (block is Rsc6FragDrawable<Rsc6Drawable> wfd)
                    texture = wfd.TextureDictionary.Item.Textures.Items.FirstOrDefault(e => e.Name == Texture.Name);
                else
                    texture = ((Rsc6VisualDictionary<Rsc6Drawable>)block).TextureDictionary.Item.Textures.Items.FirstOrDefault(e => e.Name == Texture.Name);
            }

            switch (DataType)
            {
                case 0:
                    writer.WritePtrEmbed(texture, texture, 0);
                    break;
                case 1:
                    writer.WritePtr(new Rsc6Ptr<Rsc6Vector4>(Vector));
                    break;
                default:
                    writer.WritePtr(new Rsc6Ptr<Rsc6Vector4>(Array));
                    break;
            }
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            var otstr = "Item name=\"" + Hash.ToString() + "\" type=\"" + ((DataType > 1) ? "Array" : ((Rsc6ShaderParamType)DataType).ToString()) + "\"";
            switch (DataType)
            {
                case (byte)Rsc6ShaderParamType.Texture:
                    var name = Texture?.Name ?? "";
                    if (name != "")
                    {
                        if (name.EndsWith(".dds"))
                            name = name.Replace(".dds", "");
                        if (name.Contains(':'))
                            name = name.Replace(':', '-');
                        Xml.OpenTag(sb, indent, otstr);
                        Xml.StringTag(sb, indent + 1, "Name", Xml.Escape(name.ToLower()));
                        Xml.CloseTag(sb, indent, "Item");
                    }
                    break;

                case (byte)Rsc6ShaderParamType.Vector:
                    Xml.SelfClosingTag(sb, indent, otstr + " " + FloatUtil.GetVector4XmlString(Vector.Vector));
                    break;

                default:
                    Xml.OpenTag(sb, indent, otstr);
                    foreach (var vec in Array.Array)
                    {
                        Xml.SelfClosingTag(sb, indent + 1, "Value " + FloatUtil.GetVector4XmlString(vec));
                    }
                    Xml.CloseTag(sb, indent, "Item");
                    break;
            }
        }

        public enum Rsc6ShaderParamType : byte
        {
            Texture = 0,
            Vector = 1
        }

        public override string ToString()
        {
            var n = Hash.ToString() + ": ";
            if (Texture != null)
                return n + Texture.ToString();
            if (Array.Array != null)
                return n + "Count: " + Array.Array.ToString();
            if (DataType != 0)
                return n + Array.Vector.ToString();
            return n + DataType.ToString() + ": " + DataPointer.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6ShaderParametersBlock : Rsc6BlockBase
    {
        public override ulong BlockLength
        {
            get
            {
                return ParametersDataSize;
            }
        }

        public long BaseSize
        {
            get
            {
                long offset = 44;
                foreach (var x in Parameters)
                {
                    offset += 8;
                    offset += 16 * x.DataType;
                }

                offset += Parameters.Length * 4;
                return offset;
            }
        }

        public ushort ParametersSize
        {
            get
            {
                ushort size = (ushort)((Parameters?.Length ?? 0) * 8);
                foreach (var x in Parameters)
                {
                    size += (ushort)(16 * x.DataType);
                }

                if ((size % 16) != 0)
                {
                    size += (ushort)(16 - (ushort)(size % 16));
                }
                return size;
            }
        }

        public ushort ParametersDataSize
        {
            get
            {
                var size = (ushort)(32 + ParametersSize + (4 * Parameters?.Length ?? 0));
                if ((size % 16) != 0)
                {
                    size += (ushort)(16 - (ushort)(size % 16));
                }
                return size;
            }
        }

        public byte TextureParamsCount
        {
            get
            {
                byte c = 0;
                foreach (var x in Parameters)
                {
                    if (x.DataType == 0) c++;
                }
                return c;
            }
        }

        public Rsc6ShaderFX Owner { get; set; }
        public Rsc6ShaderParameter[] Parameters { get; set; }
        public JenkHash[] Hashes { get; set; }
        public int Count { get; set; }

        public Rsc6ShaderParametersBlock()
        {

        }

        public Rsc6ShaderParametersBlock(Rsc6ShaderFX owner, int count)
        {
            Owner = owner;
            Count = count;
        }

        public override void Read(Rsc6DataReader reader)
        {
            ulong offset = reader.Position;
            var paras = new Rsc6ShaderParameter[Count];

            for (int i = 0; i < Count; i++)
            {
                var p = new Rsc6ShaderParameter();
                p.Read(reader);
                paras[i] = p;
            }

            for (int i = 0; i < Count; i++)
            {
                var p = paras[i];
                if (p.DataPointer <= 0) continue;

                switch (p.DataType)
                {
                    case 0:
                        var block = reader.ReadBlock<Rsc6TextureBase>(p.DataPointer);
                        var tex = WfdFile.TextureDictionary.Item;
                        if (tex == null)
                        {
                            if (block != null && block.Name != null && block.Name.EndsWith(".dds"))
                            {
                                p.Texture = new Rsc6TextureBase() //Placeholder
                                {
                                    MipLevels = 1,
                                    Name = block.Name
                                };
                                p.Hash = JenkHash.GenHash(block.Name);
                            }
                            break;
                        }

                        Rsc6Str name = block.NameRef;
                        if (name.Value == null) break;

                        for (int t = 0; t < tex.Textures.Count; t++)
                        {
                            if (!tex.Textures[t].NameRef.Value.Contains(name.Value.ToLower()))
                            {
                                if (t == tex.Textures.Count - 1)
                                {
                                    p.Texture = new Rsc6TextureBase()
                                    {
                                        Format = TextureFormat.A8R8G8B8,
                                        MipLevels = 1,
                                        Name = name.Value
                                    };
                                    p.Hash = JenkHash.GenHash(name.Value);
                                }
                            }
                            else if (p.Texture == null)
                            {
                                p.Texture = tex.Textures[t];
                                p.Hash = JenkHash.GenHash(name.Value);
                                break;
                            }
                        }
                        break;
                    case 1:
                        p.Vector = reader.ReadBlock(p.DataPointer, (_) => new Rsc6Vector4(p.DataType));
                        break;
                    default:
                        p.Array = reader.ReadBlock(p.DataPointer, (_) => new Rsc6Vector4(p.DataType));
                        break;
                }
            }

            ushort size = (ushort)((paras?.Length ?? 0) * 8);
            foreach (var x in paras)
            {
                size += (ushort)(16 * x.DataType);
            }

            if ((size % 16) != 0)
            {
                size += (ushort)(16 - (ushort)(size % 16));
            }

            reader.Position = offset + size;
            var hashes = new JenkHash[Count];
            for (int i = 0; i < Count; i++)
            {
                hashes[i] = reader.ReadUInt32();
                paras[i].Hash = hashes[i];
            }
            Parameters = paras;
            Hashes = hashes;
        }

        public override void Write(Rsc6DataWriter writer)
        {
            //Update DataPointer for each param
            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                if (param.Texture == null) continue;

                if (param.DataType == 0)
                    param.DataPointer = (uint)param.Texture.FilePosition;
                else if (param.DataType == 1)
                    param.DataPointer = (uint)param.Vector.FilePosition;
                else
                    param.DataPointer = (uint)param.Array.FilePosition;
            }

            //Write parameters
            foreach (var param in Parameters)
            {
                param.Write(writer);
            }

            //Write padding
            if (writer.Position % 16 != 0)
            {
                writer.WriteBytes(new byte[16 - (writer.Position % 16)]);
            }

            //Write vector data
            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                if (param.DataType == 1)
                    writer.WriteVector4(param.Vector.Vector);
                else if (param.DataType > 1)
                {
                    foreach (var v in param.Array.Array)
                    {
                        writer.WriteVector4(v);
                    }
                }
            }

            //Write hashes
            foreach (var hash in Hashes)
            {
                writer.WriteUInt32((uint)hash);
            }
        }

        public void ReadXml(XmlNode node)
        {
            var plist = new List<Rsc6ShaderParameter>();
            var hlist = new List<JenkHash>();
            var pnodes = node.SelectNodes("Item");

            foreach (XmlNode pnode in pnodes)
            {
                var p = new Rsc6ShaderParameter
                {
                    Hash = JenkHash.GenHash(Xml.GetStringAttribute(pnode, "name")?.ToLowerInvariant())
                };
                var type = Xml.GetStringAttribute(pnode, "type");

                if (type == "Texture")
                {
                    p.DataType = 0;
                    if (pnode.SelectSingleNode("Name") != null)
                    {
                        var tex = new Rsc6TextureBase();
                        tex.ReadXml(pnode);
                        p.Texture = tex;
                    }
                }
                else if (type == "Vector")
                {
                    float fx = Xml.GetFloatAttribute(pnode, "x");
                    float fy = Xml.GetFloatAttribute(pnode, "y");
                    float fz = Xml.GetFloatAttribute(pnode, "z");
                    float fw = Xml.GetFloatAttribute(pnode, "w");
                    p.Vector = new Rsc6Vector4()
                    {
                        Vector = new Vector4(fx, fz, fy, fw),
                        Count = 1
                    };
                    p.DataType = 1;
                }
                else if (type == "Array")
                {
                    var vecs = new List<Vector4>();
                    if (Xml.GetStringAttribute(pnode, "name")?.ToLowerInvariant() == "colors")
                    {
                        var colors = CreateColorsParameters();
                        foreach (var c in colors)
                        {
                            vecs.Add(c);
                        }
                    }
                    else
                    {
                        var inodes = pnode.SelectNodes("Value");
                        foreach (XmlNode inode in inodes)
                        {
                            float fx = Xml.GetFloatAttribute(inode, "x");
                            float fy = Xml.GetFloatAttribute(inode, "y");
                            float fz = Xml.GetFloatAttribute(inode, "z");
                            float fw = Xml.GetFloatAttribute(inode, "w");
                            vecs.Add(new Vector4(fx, fz, fy, fw));
                        }
                    }
                    p.Array = new Rsc6Vector4()
                    {
                        Array = vecs.ToArray(),
                        Count = vecs.Count
                    };
                    p.DataType = (byte)vecs.Count;
                }
                plist.Add(p);
                hlist.Add(p.Hash);
            }

            Parameters = plist.ToArray();
            Hashes = hlist.ToArray();
            Count = plist.Count;

            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                if (param.DataType == 0)
                {
                    param.RegisterIndex = (byte)(i + 2);
                }
            }

            var offset = 160;
            for (int i = Parameters.Length - 1; i >= 0; i--)
            {
                var param = Parameters[i];
                if (param.DataType != 0)
                {
                    param.RegisterIndex = (byte)offset;
                    offset += param.DataType;
                }
            }
        }

        private Vector4[] CreateColorsParameters()
        {
            return new Vector4[9]
            {
                new Vector4(1f, 0f, 0f , 1f),
                new Vector4(1f, 0.5f, 0f , 1f),
                new Vector4(1f, 1f, 0f , 1f),
                new Vector4(0f, 1f, 0f , 1f),
                new Vector4(0f, 1f, 1f , 1f),
                new Vector4(0f, 0f, 1f , 1f),
                new Vector4(0.5f, 0f, 1f , 1f),
                new Vector4(1f, 0f, 1f , 1f),
                new Vector4(1f, 1f, 1f , 1f),
            };
        }
    }

    [TC(typeof(EXP))] public class Rsc6Vector4 : Rsc6BlockBase
    {
        public override ulong BlockLength => (ulong)(16 * Count);
        public int Count { get; set; }
        public Vector4 Vector { get; set; }
        public Vector4[] Array { get; set; }

        public Rsc6Vector4()
        {

        }

        public Rsc6Vector4(int count)
        {
            Count = count;
        }

        public Rsc6Vector4(Vector4 vector)
        {
            Vector = vector;
            Count = 1;
        }

        public Rsc6Vector4(Vector4[] vectors)
        {
            Array = vectors;
            Count = vectors.Length;
        }

        public override void Read(Rsc6DataReader reader)
        {
            if (Count == 1)
            {
                Vector = reader.ReadVector4();
            }
            else
            {
                Array = reader.ReadVector4Arr(Count);
            }
        }
        
        public override void Write(Rsc6DataWriter writer)
        {
            if (Array != null)
            {
                foreach (var v in Array)
                {
                    writer.WriteVector4(v);
                }
            }
            else
            {
                writer.WriteVector4(Vector);
            }
        }
    }

    [TC(typeof(EXP))] public class Rsc6ShaderFX : Rsc6BlockBase //rage::grmShader + rage::grcInstanceData
    {
        public override ulong BlockLength => 32;
        public Rsc6Ptr<Rsc6ShaderParametersBlock> ParametersList { get; set; } //Data
        public JenkHash Name { get; set; } //BasisHashCode
        public byte ParameterCount { get; set; } //Count
        public byte RenderBucket { get; set; } = 1; //DrawBucket
        public byte PhysMtl { get; set; } //PhysMtl
        public byte Flags { get; set; } //Flags
        public ushort ParameterSize { get; set; } //SpuSize
        public ushort ParameterDataSize { get; set; } //TotalSize
        public JenkHash FileName { get; set; } //MaterialHashCode
        public uint RenderBucketMask { get; set; } //Always 0
        public ushort LastFrame { get; set; } //LastFrame
        public byte TextureDmaListSize { get; set; } //TextureDmaListSize
        public byte TextureParametersCount { get; set; } //TextureCount
        public uint SortKey { get; set; } //SortKey

        public override void Read(Rsc6DataReader reader)
        {
            ParametersList = reader.ReadPtrPtr<Rsc6ShaderParametersBlock>();
            Name = reader.ReadUInt32();
            ParameterCount = reader.ReadByte();
            RenderBucket = reader.ReadByte();
            PhysMtl = reader.ReadByte();
            Flags = reader.ReadByte();
            ParameterSize = reader.ReadUInt16();
            ParameterDataSize = reader.ReadUInt16();
            FileName = reader.ReadUInt32();
            RenderBucketMask = reader.ReadUInt32();
            LastFrame = reader.ReadUInt16();
            TextureDmaListSize = reader.ReadByte();
            TextureParametersCount = reader.ReadByte();
            SortKey = reader.ReadUInt32();
            ParametersList = reader.ReadPtrItem(ParametersList, (_) => new Rsc6ShaderParametersBlock(this, ParameterCount));

            if (FileName == 0)
            {
                FileName = new JenkHash(Name.Str + ".fxc");
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            ParameterCount = (byte)(ParametersList.Item != null ? ParametersList.Item.Count : 0);
            writer.WritePtr(ParametersList);
            writer.WriteUInt32(Name);
            writer.WriteByte(ParameterCount);
            writer.WriteByte(RenderBucket);
            writer.WriteByte(PhysMtl);
            writer.WriteByte(Flags);
            writer.WriteUInt16(ParameterSize);
            writer.WriteUInt16(ParameterDataSize);
            writer.WriteUInt32(0);
            writer.WriteUInt32(RenderBucketMask);
            writer.WriteUInt16(LastFrame);
            writer.WriteByte(TextureDmaListSize);
            writer.WriteByte(TextureParametersCount);
            writer.WriteUInt32(SortKey);
        }

        public void ReadXml(XmlNode node)
        {
            Name = JenkHash.GenHash(Xml.GetChildInnerText(node, "Name"));
            FileName = JenkHash.GenHash(Xml.GetChildInnerText(node, "FileName"));
            RenderBucket = (byte)Xml.GetChildUIntAttribute(node, "RenderBucket", "value");

            var pnode = node.SelectSingleNode("Parameters");
            if (pnode != null)
            {
                var parametersList = new Rsc6ShaderParametersBlock();
                parametersList.Owner = this;
                parametersList.ReadXml(pnode);

                ParametersList = new Rsc6Ptr<Rsc6ShaderParametersBlock>(parametersList);
                ParameterCount = (byte)parametersList.Count;
                ParameterSize = parametersList.ParametersSize;
                ParameterDataSize = parametersList.ParametersDataSize;
                TextureParametersCount = parametersList.TextureParamsCount;
            }
        }

        public Rsc6ShaderParameter[] GetParams()
        {
            var sp = ParametersList.Item;
            return sp?.Parameters;
        }

        public Rsc6ShaderParameter[] SetParams(Rsc6ShaderParameter[] param, JenkHash[] hashes = null)
        {
            var sp = ParametersList.Item;
            sp.Parameters = param;
            if (hashes != null)
            {
                sp.Hashes = hashes;
            }
            return sp?.Parameters;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6VertexDeclaration : Rsc6BlockBase
    {
        public override ulong BlockLength => 16;
        public uint Flags { get; set; } //m_Fvf, fvf channels currently used, mostly 16601, 16473 or 16857
        public ushort Stride { get; set; } //m_FvfSize + m_Flags, total size of the fvf + various flags for transformed positions, etc
        public byte DynamicOrder { get; set; } //m_DynamicOrder, padded dynamic order instead of standard order
        public byte Count { get; set; } //m_ChannelCount, number of '1's in 'Flags'
        public Rsc6VertexDeclarationTypes Types { get; set; } //m_FvfChannelSizes, 16 fields 4 bits each
        public VertexLayout VertexLayout { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            Flags = reader.ReadUInt32();
            Stride = reader.ReadUInt16();
            DynamicOrder = reader.ReadByte();
            Count = reader.ReadByte();
            Types = (Rsc6VertexDeclarationTypes)reader.ReadUInt64();

            ulong t = (ulong)Types;
            ulong types = 0;
            ulong semantics = 0;
            int n = 0;

            for (int i = 0; i < 16; i++)
            {
                if (((Flags >> i) & 1) != 0)
                {
                    var i4 = i * 4;
                    var n4 = n * 4;
                    var ef = GetEngineElementFormat((Rsc6VertexComponentType)((t >> i4) & 0xF));
                    var si = GetEngineSemanticIndex((Rsc6VertexElementSemantic)i);
                    types += (((ulong)ef) << n4);
                    semantics += (((ulong)si) << n4);
                    n++;
                }
            }
            VertexLayout = new VertexLayout(types, semantics);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(Flags);
            writer.WriteUInt16(Stride);
            writer.WriteByte(DynamicOrder);
            writer.WriteByte(Count);
            writer.WriteUInt64((ulong)Types);
        }

        public void ReadXml(XmlNode node)
        {
            if (node == null) return;

            Types = Xml.GetEnumValue<Rsc6VertexDeclarationTypes>(Xml.GetStringAttribute(node, "type").Replace("GTAV1", "RDR1_1"));
            uint f = 0;

            foreach (XmlNode cnode in node.ChildNodes)
            {
                if (cnode is XmlElement celem)
                {
                    var componentSematic = Xml.GetEnumValue<Rsc6VertexElementSemantic>(celem.Name);
                    var idx = (int)componentSematic;
                    f |= (1u << idx);
                }
            }
            Flags = f;
            UpdateCountAndStride();
        }

        public ulong GetDeclarationId()
        {
            ulong res = 0;
            for (int i = 0; i < 16; i++)
            {
                if (((Flags >> i) & 1) == 1)
                {
                    res += ((ulong)Types & (0xFu << (i * 4)));
                }
            }
            return res;
        }

        private bool IsPreTransform() //Query to see if the position channel contains pre-transformed data
        {
            return (Flags & 0x1) == 0x1;
        }

        private bool IsChannelActive(Rsc6VertexElementBits channel) //Determine if a channel is active
        {
            uint msk = 1U << (int)channel;
            return (Flags & msk) == msk;
        }

        public Rsc6VertexComponentType GetComponentType(int index)
        {
            //index is the flags bit index
            return (Rsc6VertexComponentType)(((ulong)Types >> (index * 4)) & 0x0000000F);
        }

        public int GetComponentOffset(int index)
        {
            //index is the flags bit index
            var offset = 0;
            for (int k = 0; k < index; k++)
            {
                if (((Flags >> k) & 0x1) == 1)
                {
                    var componentType = GetComponentType(k);
                    offset += Rsc6VertexComponentTypes.GetSizeInBytes(componentType);
                }
            }
            return offset;
        }

        public void UpdateCountAndStride()
        {
            var cnt = 0;
            var str = 0;
            for (int k = 0; k < 16; k++)
            {
                if (((Flags >> k) & 0x1) == 1)
                {
                    var componentType = GetComponentType(k);
                    str += Rsc6VertexComponentTypes.GetSizeInBytes(componentType);
                    cnt++;
                }
            }
            Count = (byte)cnt;
            Stride = (ushort)str;
        }

        public VertexLayout GetEngineLayout()
        {
            ulong formats = 0;
            ulong semantics = 0;
            int eidx = 0;
            var types = (ulong)Types;
            for (int i = 0; i < 16; i++)
            {
                if (((Flags >> i) & 1) == 1)
                {
                    var t = (Rsc6VertexComponentType)((types >> (i * 4)) & 0xF);
                    var efmt = GetEngineElementFormat(t);
                    var esem = GetEngineElementSemantic(i);
                    var o4 = eidx * 4;
                    formats += (((ulong)efmt) << o4);
                    semantics += (((ulong)esem) << o4);
                    eidx++;
                }
            }
            var vl = new VertexLayout(formats, semantics);
            return vl;
        }

        private static VertexElementFormat GetEngineElementFormat(Rsc6VertexComponentType t)
        {
            switch (t)
            {
                case Rsc6VertexComponentType.Half2: return VertexElementFormat.Half2;
                case Rsc6VertexComponentType.Float: return VertexElementFormat.Float;
                case Rsc6VertexComponentType.Half4: return VertexElementFormat.Half4;
                case Rsc6VertexComponentType.FloatUnk: return VertexElementFormat.Float;
                case Rsc6VertexComponentType.Float2: return VertexElementFormat.Float2;
                case Rsc6VertexComponentType.Float3: return VertexElementFormat.Float3;
                case Rsc6VertexComponentType.Float4: return VertexElementFormat.Float4;
                case Rsc6VertexComponentType.UByte4: return VertexElementFormat.UByte4;
                case Rsc6VertexComponentType.Colour: return VertexElementFormat.Colour;
                case Rsc6VertexComponentType.Dec3N: return VertexElementFormat.Dec3N;
                case Rsc6VertexComponentType.UShort2N: return VertexElementFormat.UShort2N;
                default: return VertexElementFormat.None;
            }
        }

        private static byte GetEngineElementSemantic(int i)
        {
            switch (i)
            {
                default:
                case 0: return 0; //"POSITION"
                case 1: return 1; //"BLENDWEIGHTS"
                case 2: return 2; //"BLENDINDICES"
                case 3: return 3; //"NORMAL"
                case 4:
                case 5: return 4; //"COLOR"
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13: return 5; //"TEXCOORD"
                case 14: return 6; //"TANGENT"
                case 15: return 7; //"BINORMAL"
            }
        }

        public byte GetEngineSemanticIndex(Rsc6VertexElementSemantic s)
        {
            switch (s)
            {
                default:
                case Rsc6VertexElementSemantic.Position: return 0;
                case Rsc6VertexElementSemantic.BlendWeights: return 1;
                case Rsc6VertexElementSemantic.BlendIndices: return 2;
                case Rsc6VertexElementSemantic.Normal: return 3;
                case Rsc6VertexElementSemantic.Colour0: return 4;
                case Rsc6VertexElementSemantic.Colour1: return 4;
                case Rsc6VertexElementSemantic.TexCoord0: return 5;
                case Rsc6VertexElementSemantic.TexCoord1: return 5;
                case Rsc6VertexElementSemantic.TexCoord2: return 5;
                case Rsc6VertexElementSemantic.TexCoord3: return 5;
                case Rsc6VertexElementSemantic.TexCoord4: return 5;
                case Rsc6VertexElementSemantic.TexCoord5: return 5;
                case Rsc6VertexElementSemantic.TexCoord6: return 5;
                case Rsc6VertexElementSemantic.TexCoord7: return 5;
                case Rsc6VertexElementSemantic.Tangent0: return 6;
                case Rsc6VertexElementSemantic.Tangent1: return 6;
                case Rsc6VertexElementSemantic.Binormal0: return 7;
                case Rsc6VertexElementSemantic.Binormal1: return 7;
            }
        }

        public override string ToString()
        {
            return Stride.ToString() + ": " + Count.ToString() + ": " + Flags.ToString() + ": " + Types.ToString();
        }
    }

    [TC(typeof(EXP))] public static class Rsc6VertexComponentTypes
    {
        public static int GetSizeInBytes(Rsc6VertexComponentType type)
        {
            switch (type)
            {
                case Rsc6VertexComponentType.Nothing: return 2; //Half
                case Rsc6VertexComponentType.Half2: return 4; //Half2
                case Rsc6VertexComponentType.Float: return 6; //Half3
                case Rsc6VertexComponentType.Half4: return 8; //Half4
                case Rsc6VertexComponentType.FloatUnk: return 4; //Float
                case Rsc6VertexComponentType.Float2: return 8; //Float2
                case Rsc6VertexComponentType.Float3: return 12; //Float3
                case Rsc6VertexComponentType.Float4: return 16; //Float4
                case Rsc6VertexComponentType.UByte4: return 4; //UByte4
                case Rsc6VertexComponentType.Colour: return 4; //Color
                case Rsc6VertexComponentType.Dec3N: return 4; //PackedNormal
                case Rsc6VertexComponentType.Unk1: return 2; //Short_UNorm
                case Rsc6VertexComponentType.Unk2: return 4; //Short2_Unorm
                case Rsc6VertexComponentType.Unk3: return 2; //Byte2_UNorm
                case Rsc6VertexComponentType.UShort2N: return 4; //Short2
                case Rsc6VertexComponentType.Unk5: return 8; //Short4
                default: return 0;
            }
        }

        public static int GetComponentCount(Rsc6VertexComponentType f)
        {
            switch (f)
            {
                case Rsc6VertexComponentType.Nothing: return 0;
                case Rsc6VertexComponentType.Float: return 1;
                case Rsc6VertexComponentType.Float2: return 2;
                case Rsc6VertexComponentType.Float3: return 3;
                case Rsc6VertexComponentType.Float4: return 4;
                case Rsc6VertexComponentType.Colour: return 4;
                case Rsc6VertexComponentType.UByte4: return 4;
                case Rsc6VertexComponentType.Half2: return 2;
                case Rsc6VertexComponentType.Half4: return 4;
                case Rsc6VertexComponentType.Dec3N: return 3;
                case Rsc6VertexComponentType.UShort2N: return 2;
                default: return 0;
            }
        }
    }

    public enum Rsc6VertexElementSemantic : byte //grcFvfChannels, list of fvf channels available
    {
        Position = 0,
        BlendWeights = 1,
        BlendIndices = 2, //Binding
        Normal = 3,
        Colour0 = 4, //Normal
        Colour1 = 5, //Diffuse
        TexCoord0 = 6,
        TexCoord1 = 7,
        TexCoord2 = 8,
        TexCoord3 = 9,
        TexCoord4 = 10,
        TexCoord5 = 11,
        TexCoord6 = 12,
        TexCoord7 = 13,
        Tangent0 = 14,
        Tangent1 = 15,
        Binormal0 = 16,
        Binormal1 = 17,
    }

    public enum Rsc6VertexElementBits : uint //grcFvfChannelBits
    {
        PositionMask = 0x1,
        BlendWeightsMask = 0x2,
        BlendIndicesMask = 0x4, //Binding
        NormalMask = 0x8,
        Colour0Mask = 0x10, //Normal
        Colour1Mask = 0x20, //Diffuse
        TexCoord0Mask = 0x40,
        TexCoord1Mask = 0x80,
        TexCoord2Mask = 0x100,
        TexCoord3Mask = 0x200,
        TexCoord4Mask = 0x400,
        TexCoord5Mask = 0x800,
        TexCoord6Mask = 0x1000,
        TexCoord7Mask = 0x2000,
        Tangent0Mask = 0x4000,
        Tangent1Mask = 0x8000,
        Binormal0Mask = 0x10000,
        Binormal1Mask = 0x20000,
        PositionHalf4Mask = 0x40000000,
        PositionFloat4Mask = 0x20000000,
    }

    public enum Rsc6VertexComponentType : byte
    {
        Nothing = 0,
        Half2 = 1,
        Float = 2,
        Half4 = 3,
        FloatUnk = 4,
        Float2 = 5,
        Float3 = 6,
        Float4 = 7,
        UByte4 = 8,
        Colour = 9,
        Dec3N = 10,
        Unk1 = 11,
        Unk2 = 12,
        Unk3 = 13,
        UShort2N = 14,
        Unk5 = 15,
    }

    public enum Rsc6VertexDeclarationTypes : ulong
    {
        RDR1_1 = 0xAA1111111199A996, //12254594826059229590 - Used by most drawables
        RDR1_2 = 0xAAEEEEEEEE99A996 //12317044740877560214 - Used by terrain tiles
    }

    public enum Rsc6LightmapTypes : uint
    {
        Lightmap,
        LightmapColorHDR,
        LightmapExpHDR
    }

    [Flags] public enum Rsc6DoFs : uint //Degrees of freedom ({rotate,translate,scale} X {x,y,z}) and limits
    {
        ROTATE_X = 1, //Can rotate on x-axis
        ROTATE_Y = 2, // Can rotate on y-axis
        ROTATE_Z = 4, //Can rotate on z-axis
        HAS_ROTATE_LIMITS = 8, //Is rotation limited?
        TRANSLATE_X = 16, //Can translate in x-axis
        TRANSLATE_Y = 32, //Can translate in y-axis
        TRANSLATE_Z = 64, //Can translate in z-axis
        HAS_TRANSLATE_LIMITS = 128, //Is translation limited?
        SCALE_X = 256, //Can scale in x-axis
        SCALE_Y = 512, //Can scale in y-axis
        SCALE_Z = 1024, //Can scale in z-axis
        HAS_SCALE_LIMITS = 2048, //Is scale limited?
        HAS_CHILD = 4096, //Children?
        IS_SKINNED = 8192, //Bone is skinned to
        ROTATION = ROTATE_X | ROTATE_Y | ROTATE_Z, //7
        TRANSLATION = TRANSLATE_X | TRANSLATE_Y | TRANSLATE_Z, //112
        SCALE = SCALE_X | SCALE_Y | SCALE_Z, //1792
    };
}
