using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Shaders;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;
using System.Diagnostics;
using System.Xml.Linq;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6VisualDictionary : Rsc6BlockBaseMap, MetaNode //rdrVisualDictionary
    {
        public override ulong BlockLength => 60;
        public override uint VFT { get; set; } = 0x01908FF8;
        public uint Unknown_8h { get; set; } = 0x01876DE0; //0x01876DE0 (buildings & props) or 0x04A744AC (tiles)
        public uint Unknown_Ch { get; set; } //Always 0
        public uint ParentDictionary { get; set; } //Always 0
        public uint UsageCount { get; set; } = 1; //Always 1
        public Rsc6Arr<JenkHash> Hashes { get; set; }
        public Rsc6PtrArr<Rsc6Drawable> Drawables { get; set; } //m_Drawables
        public Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary { get; set; } //m_Textures
        public Rsc6Ptr<Rsc6TextureDictionary> DerivedTextures { get; set; } //m_DerivedTextures, unused
        public uint Unknown_30h { get; set; } //Always 0
        public Rsc6LodLevel LODLevel { get; set; }
        public uint Unknown_38h { get; set; } //Always 0

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            ParentDictionary = reader.ReadUInt32();
            UsageCount = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Drawables = reader.ReadPtrArr<Rsc6Drawable>();
            TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>();
            DerivedTextures = reader.ReadPtr<Rsc6TextureDictionary>();
            Unknown_30h = reader.ReadUInt32();
            LODLevel = (Rsc6LodLevel)reader.ReadUInt32();
            Unknown_38h = reader.ReadUInt32();

            TexturePack texpack = null;
            var txdict = TextureDictionary.Item?.DictStr;
            if (txdict != null)
            {
                texpack = new TexturePack(reader.FileEntry)
                {
                    Textures = []
                };

                foreach (var kvp in txdict)
                {
                    texpack.Textures[kvp.Key] = kvp.Value;
                }
            }

            if (Drawables.Items != null)
            {
                var imax = Math.Min(Hashes.Items?.Length ?? 0, Drawables.Items.Length);
                for (int i = 0; i < Drawables.Items.Length; i++)
                {
                    var d = Drawables.Items[i];
                    var h = (i < imax) ? Hashes.Items[i] : 0;
                    d.NameHash = h;
                    d.Name = h.ToString();
                    d.TexturePack = texpack;
                    d.ApplyTextures(TextureDictionary.Item);
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            //When the file contains at least one drawable, this should be true
            //Some #vd files are only used as textures dictionaries where there's no drawable inside...
            if (TextureDictionary.Item == null)
            {
                var textures = new List<Rsc6Texture>();
                var hashes = new List<JenkHash>();
                foreach (var item in Drawables.Items)
                {
                    foreach (var tex in item.ShaderGroup.Item.TextureDictionary.Item.Textures.Items)
                    {
                        textures.Add(tex);
                        var name = tex.NameRef.Value.Replace(".dds", ""); //Hashes don't store the .dds extension
                        hashes.Add(JenkHash.GenHash(name));
                    }
                }

                var dict = new Rsc6TextureDictionary
                {
                    Textures = new Rsc6PtrArr<Rsc6Texture>(textures.ToArray()),
                    Hashes = new Rsc6Arr<JenkHash>(hashes.ToArray())
                };
                TextureDictionary = new Rsc6Ptr<Rsc6TextureDictionary>(dict);
            }

            base.Write(writer);
            writer.WriteUInt32(Unknown_8h);
            writer.WriteUInt32(Unknown_Ch);
            writer.WriteUInt32(ParentDictionary);
            writer.WriteUInt32(UsageCount);
            writer.WriteArr(Hashes);
            writer.WritePtrArr(Drawables);
            writer.WritePtr(TextureDictionary);
            writer.WritePtr(DerivedTextures);
            writer.WriteUInt32(Unknown_30h);
            writer.WriteUInt32((uint)LODLevel);
            writer.WriteUInt32(Unknown_38h);
        }

        public void Read(MetaNodeReader reader)
        {
            LODLevel = reader.ReadEnum("LODLevel", Rsc6LodLevel.HIGH);
            Drawables = new(reader.ReadNodeArray<Rsc6Drawable>("Drawables"));
            TextureDictionary = new(reader.ReadNode<Rsc6TextureDictionary>("TextureDictionary"));

            if (Drawables.Items != null)
            {
                Hashes = new(Drawables.Items.Select(d => new JenkHash(d.Name)).ToArray());
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            writer.WriteEnum("LODLevel", LODLevel);
            writer.WriteNodeArray("Drawables", Drawables.Items);
            writer.WriteNode("TextureDictionary", TextureDictionary.Item);
        }
    }

    [TC(typeof(EXP))] public class Rsc6FragDrawable : Rsc6BlockBaseMap, MetaNode
    {
        //WFD file root object

        public override ulong BlockLength => 16;
        public override uint VFT { get; set; } = 0x00DDC0A0;
        public Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary { get; set; }
        public Rsc6Ptr<Rsc6Drawable> Drawable { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Drawable = reader.ReadPtr<Rsc6Drawable>();
            TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>();
            Drawable.Item?.ApplyTextures(TextureDictionary.Item);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtr(Drawable);
            writer.WritePtr(TextureDictionary);
        }

        public void Read(MetaNodeReader reader)
        {
            TextureDictionary = new(reader.ReadNode<Rsc6TextureDictionary>("TextureDictionary"));
            Drawable = new(reader.ReadNode<Rsc6Drawable>("Drawable"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            writer.WriteNode("TextureDictionary", TextureDictionary.Item);
            writer.WriteNode("Drawable", Drawable.Item);
        }
    }

    [TC(typeof(EXP))] public class Rsc6DrawableLod : PieceLod, IRsc6Block
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

        public override void Read(MetaNodeReader reader)
        {
            var models = reader.ReadNodeArray<Rsc6DrawableModel>("Models");
            if (models != null)
            {
                ModelsData = new(models);
                Models = ModelsData.Items;
            } 
        }
        public override void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Models", Models);
        }
    }

    [TC(typeof(EXP))] public class Rsc6DrawableGeometry : Mesh, IRsc6Block //rage::grmGeometry + rage::grmGeometryQB
    {
        /*
         * grmGeometryQB represents a "packet" of vertex data, which is the data sent down to the hardware for rendering.
         */

        public ulong BlockLength => 80;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public uint VFT { get; set; } = 0x018572A0;
        public uint Unknown_4h { get; set; }
        public uint Unknown_8h { get; set; }
        public Rsc6Ptr<Rsc6VertexBuffer> VertexBuffer { get; set; } //m_VB[4] - rage::grcVertexBuffer
        public Rsc6Ptr<Rsc6VertexBuffer> VertexBuffer2 { get; set; }
        public Rsc6Ptr<Rsc6VertexBuffer> VertexBuffer3 { get; set; }
        public Rsc6Ptr<Rsc6VertexBuffer> VertexBuffer4 { get; set; }
        public Rsc6Ptr<Rsc6IndexBuffer> IndexBuffer { get; set; } //m_IB[4] - rage::grcIndexBuffer
        public Rsc6Ptr<Rsc6IndexBuffer> IndexBuffer2 { get; set; }
        public Rsc6Ptr<Rsc6IndexBuffer> IndexBuffer3 { get; set; }
        public Rsc6Ptr<Rsc6IndexBuffer> IndexBuffer4 { get; set; }
        public uint IndicesCount { get; set; } //m_IndexCount
        public uint TrianglesCount { get; set; } //m_PrimCount
        public byte PrimitiveType { get; set; } = 3; //m_PrimType, rendering primitive type
        public bool DoubleBuffered { get; set; } //m_DoubleBuffered
        public Rsc6RawArr<ushort> BoneIds { get; set; } //m_MtxPalette, matrix palette for this geometry
        public ushort BoneIdsCount { get; set; } //m_MtxCount, the number of matrices in the matrix paletter
        public Rsc6RawArr<byte> VertexDataRef { get; set; } //m_VtxDeclOffset, always 0xCDCDCDCD
        public uint OffsetBuffer { get; set; } //m_OffsetBuffer, PS3 only I think
        public uint IndexOffset { get; set; } //m_IndexOffset, PS3 only I think
        public uint Unknown_3Ch { get; set; }

        public Rsc6ShaderFX ShaderRef { get; set; } //Written by parent DrawableBase, using ShaderID
        public ushort ShaderID { get; set; } //Read-written by parent model
        public BoundingBox4 AABB { get; set; } //Read-written by parent model

        public Rsc6DrawableGeometry()
        {
        }

        public void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt32();
            VertexBuffer = reader.ReadPtr<Rsc6VertexBuffer>();
            VertexBuffer2 = reader.ReadPtr<Rsc6VertexBuffer>();
            VertexBuffer3 = reader.ReadPtr<Rsc6VertexBuffer>();
            VertexBuffer4 = reader.ReadPtr<Rsc6VertexBuffer>();
            IndexBuffer = reader.ReadPtr<Rsc6IndexBuffer>();
            IndexBuffer2 = reader.ReadPtr<Rsc6IndexBuffer>();
            IndexBuffer3 = reader.ReadPtr<Rsc6IndexBuffer>();
            IndexBuffer4 = reader.ReadPtr<Rsc6IndexBuffer>();
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

            BoneIds = reader.ReadRawArrItems(BoneIds, BoneIdsCount);

            if (VertexBuffer.Item != null) //hack to fix stupid "locked" things
            {
                VertexLayout = VertexBuffer.Item?.Layout.Item?.VertexLayout;
                VertexData = VertexBuffer.Item.LockedData.Items ?? VertexBuffer.Item.VertexData.Items;

                if (VertexCount == 0)
                {
                    VertexCount = VertexBuffer.Item.VertexCount;
                }
            }

            //Swap RDR axis
            byte[] numArray = VertexData;
            var elems = VertexLayout.Elements;
            var elemcount = elems.Length;

            for (int index = 0; index < numArray.Length; index += VertexStride)
            {
                for (int i = 0; i < elemcount; i++)
                {
                    var elem = elems[i];
                    int elemoffset = elem.Offset;

                    switch (elem.Format)
                    {
                        case VertexElementFormat.Float3:
                            var v3 = BufferUtil.ReadVector3(numArray, index + elemoffset);
                            Rpf6Crypto.WriteVector3AtIndex(v3, numArray, index + elemoffset);
                            break;
                        case VertexElementFormat.Float4:
                            var v4 = BufferUtil.ReadVector4(numArray, index + elemoffset);
                            Rpf6Crypto.WriteVector4AtIndex(v4, numArray, index + elemoffset);
                            break;
                        case VertexElementFormat.Colour:
                            var color = BufferUtil.ReadColour(numArray, index + elemoffset);
                            var newColor = new Colour(color.B, color.G, color.R, color.A);
                            BufferUtil.WriteColour(numArray, index + elemoffset, ref newColor);
                            break;
                        case VertexElementFormat.Dec3N:
                            var packed = BufferUtil.ReadUint(numArray, index + elemoffset);
                            var pv = Rpf6Crypto.Dec3NToVector4(packed); //Convert Dec3N to Vector4
                            var np = Rpf6Crypto.Vector4ToDec3N(new Vector4(pv.Z, pv.X, pv.Y, pv.W)); //Convert Vector4 back to Dec3N
                            BufferUtil.WriteUint(numArray, index + elemoffset, np);
                            break;
                        default:
                            break;
                    }
                }
            }

            //Triangles or strips
            if (PrimitiveType == 3)
                Indices = IndexBuffer.Item?.Indices.Items;
            else
                Indices = ConvertStripToTriangles(IndexBuffer.Item?.Indices.Items).ToArray();

            VertexData = numArray;
        }

        public void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable;
            bool wft = writer.BlockList[0] is Rsc6Fragment;

            if (wfd)
                writer.WriteUInt32(0x00D3397C);
            else if (wft)
                writer.WriteUInt32(0x00EF397C);
            else
                writer.WriteUInt32(VFT);

            writer.WriteUInt32(Unknown_4h);
            writer.WriteUInt32(Unknown_8h);
            writer.WritePtr(VertexBuffer);
            writer.WritePtr(VertexBuffer2);
            writer.WritePtr(VertexBuffer3);
            writer.WritePtr(VertexBuffer4);
            writer.WritePtr(IndexBuffer);
            writer.WritePtr(IndexBuffer2);
            writer.WritePtr(IndexBuffer3);
            writer.WritePtr(IndexBuffer4);
            writer.WriteUInt32(IndicesCount);
            writer.WriteUInt32(TrianglesCount);
            writer.WriteUInt16((ushort)VertexCount);
            writer.WriteByte(PrimitiveType);
            writer.WriteBoolean(DoubleBuffered);
            writer.WriteRawArr(BoneIds);
            writer.WriteUInt16((ushort)VertexStride);
            writer.WriteUInt16(BoneIdsCount);
            writer.WriteUInt32(0xCDCDCDCD); //VertexDataRef
            writer.WriteUInt32(OffsetBuffer);
            writer.WriteUInt32(IndexOffset);
            writer.WriteUInt32(Unknown_3Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            var min = Rpf6Crypto.ToXYZ(reader.ReadVector3("BoundingBoxMin"));
            var max = Rpf6Crypto.ToXYZ(reader.ReadVector3("BoundingBoxMax"));
            BoundingBox = new BoundingBox(min, max);
            AABB = new BoundingBox4(BoundingBox);
            ShaderID = reader.ReadUInt16("ShaderID");
            BoneIds = new(reader.ReadUInt16Array("BoneIDs"));
            BoneIdsCount = (ushort)(BoneIds.Items?.Length ?? 0);
            Unknown_3Ch = reader.ReadUInt32("Unknown_3Ch");

            var vbuf = new Rsc6VertexBuffer();
            base.Read(reader);

            var elems = VertexLayout.Elements;
            var elemcount = elems.Length;
            for (int index = 0; index < VertexData.Length; index += VertexStride)
            {
                for (int i = 0; i < elemcount; i++)
                {
                    var elem = elems[i];
                    int elemoffset = elem.Offset;

                    switch (elem.Format)
                    {
                        case VertexElementFormat.Float3:
                            var newVert = BufferUtil.ReadVector3(VertexData, index + elemoffset);
                            Rpf6Crypto.WriteVector3AtIndex(newVert, VertexData, index + elemoffset, false); //Convert Vector3 to Dec3N with RDR axis
                            break;
                        case VertexElementFormat.Dec3N:
                            var packed = BufferUtil.ReadUint(VertexData, index + elemoffset);
                            var pv = Rpf6Crypto.Dec3NToVector4(packed); //Convert Dec3N to Vector4
                            var np = Rpf6Crypto.Vector4ToDec3N(new Vector4(pv.Y, pv.Z, pv.X, pv.W)); //Convert Vector4 back to Dec3N with RDR axis
                            BufferUtil.WriteUint(VertexData, index + elemoffset, np);
                            break;
                        case VertexElementFormat.Colour:
                            var color = BufferUtil.ReadColour(VertexData, index + elemoffset);
                            color = new Colour(color.B, color.G, color.R, color.A);
                            BufferUtil.WriteColour(VertexData, index + elemoffset, ref color);
                            break;
                        default:
                            break;
                    }
                }
            }

            uint fvf = 0;
            for (int i = 0; i < elems.Length; i++)
            {
                var channel = Rsc6VertexComponentTypes.GetChannelFromName(elems[i]);
                Rsc6VertexComponentTypes.UpdateFVF(channel, ref fvf);
            }

            var layout = new Rsc6VertexDeclaration()
            {
                FVF = fvf,
                FVFSize = (byte)VertexStride,
                ChannelCount = (byte)VertexLayout.ElementCount,
                Types = Rsc6VertexDeclarationTypes.DEFAULT
            };

            var ibuf = new Rsc6IndexBuffer
            {
                IndicesCount = (uint)(Indices?.Length ?? 0),
                Indices = new(Indices)
            };

            vbuf.VertexCount = (ushort)VertexCount;
            vbuf.VertexStride = (uint)VertexStride;
            vbuf.VertexData = new(VertexData);
            vbuf.Layout = new(layout);

            VertexBuffer = new(vbuf);
            IndexBuffer = new(ibuf);

            IndicesCount = (IndexBuffer.Item != null) ? IndexBuffer.Item.IndicesCount : 0;
            TrianglesCount = IndicesCount / 3;
        }

        public override void Write(MetaNodeWriter writer)
        {
            if (AABB.Min.XYZ() != default) writer.WriteVector3("BoundingBoxMin", AABB.Min.XYZ());
            if (AABB.Max.XYZ() != default) writer.WriteVector3("BoundingBoxMax", AABB.Max.XYZ());
            writer.WriteUInt16("ShaderID", ShaderID);
            writer.WriteUInt32("Unknown_3Ch", Unknown_3Ch);
            if (BoneIds.Items != null) writer.WriteUInt16Array("BoneIDs", BoneIds.Items);
            base.Write(writer);
        }

        public static List<ushort> ConvertStripToTriangles(ushort[] stripIndices)
        {
            var triangleIndices = new List<ushort>();
            for (int i = 2; i < stripIndices.Length; i++)
            {
                if (i % 2 == 0)
                {
                    //Even indices: Add vertices in clockwise order
                    triangleIndices.Add(stripIndices[i - 2]);
                    triangleIndices.Add(stripIndices[i - 1]);
                    triangleIndices.Add(stripIndices[i]);
                }
                else
                {
                    //Odd indices: Add vertices in counterclockwise order
                    triangleIndices.Add(stripIndices[i - 1]);
                    triangleIndices.Add(stripIndices[i - 2]);
                    triangleIndices.Add(stripIndices[i]);
                }
            }
            return triangleIndices;
        }

        public int GetBufferIndex()
        {
            return ((DoubleBuffered ? 1 : 0) & (int)Rsc6VertexBufferType.USE_SECONDARY_BUFFER_INDICES) >> 2;
        }

        public int GetDoubleBuffered()
        {
            return (DoubleBuffered ? 1 : 0) & ~(int)Rsc6VertexBufferType.USE_SECONDARY_BUFFER_INDICES;
        }

        public void SetShader(Rsc6ShaderFX shader, Model model)
        {
            ShaderRef = shader;
            Name = shader?.Name.ToString();

            if (shader != null)
            {
                var bucket = shader.RenderBucket;
                var hash = shader.Name.Hash;

                switch (hash)
                {
                    #region specific shaders
                    case 0x60F5992A: //rdr2_clouds_animsoft
                    case 0x693DE1A1: //rdr2_clouds_altitude
                    case 0x5218CB1D: //rdr2_clouds_fast
                    case 0xE3136EFD: //rdr2_clouds_anim
                    case 0xF6C04CCD: //rdr2_clouds_soft
                    case 0xFE72D6A5: //rdr2_clouds_fog
                        SetupSkyShader(shader);
                        break;
                    case 0xC242DAA7: //rdr2_terrain_blend
                    case 0xF98973D1: //rdr2_terrain
                        SetupBlendTerrainShader(shader);
                        break;
                    case 0x3103407E: //rdr2_cliffwall_ao_low_lod
                    case 0x249BB297: //rdr2_cliffwall_ao
                    case 0x227C5611: //rdr2_cliffwall_alpha
                        SetupClifwallTerrainShader(shader);
                        break;
                    case 0xB34AF114: //rdr2_layer_2_nospec_ambocc_decal
                    case 0x5A170205: //rdr2_layer_2_nospec_ambocc
                        SetDiffuse2Shader(shader);
                        break;
                    case 0x24982D70: //rdr2_layer_3_nospec_normal_ambocc
                        SetDiffuse3Shader(shader);
                        break;
                    case 0x173D5F9D: //rdr2_grass
                        SetupGrassShader(shader);
                        break;
                    case 0xC714B86E: //rdr2_alpha_foliage
                    case 0x592D7DC2: //rdr2_alpha_foliage_no_fade
                        SetupTreesShader(shader);
                        break;
                    case 0xA1100B4E: //rdr2_river_water
                    case 0x372E2B02: //rdr2_river_water_joint
                        SetupWaterShader(shader, model);
                        return;//don't mess with buckets or other params below
                    #endregion
                    #region default shaders
                    case 0x707EF967: //rdr2_flattenterrain_blend //(PNCCTTXX) TODO: terrain lods have "bands" - needs 2 layers and tinting?
                    case 0xb71272ea: //rdr2_flattenterrain
                    case 0x387e0fde: //rdr2_low_lod_nodirt
                    case 0xa042c1ce: //rdr2_diffuse
                    case 0x2e1239a8: //rdr2_low_lod
                    case 0x2e9c4c9e: //rdr2_bump_ambocc
                    case 0x32a4918e: //rdr2_alpha
                    case 0xaa95cd3f: //rdr2_poster
                    case 0x24c91669: //rdr2_low_lod_decal
                    case 0x6c25115d: //rdr2_window_glow
                    case 0xed7cd8d7: //rdr2_low_lod_nodirt_singlesided
                    case 0x2fe0f698: //rdr2_bump_spec_ambocc_shared
                    case 0x171c9e47: //rdr2_glass_glow
                    case 0x949ec19c: //rdr2_alpha_bspec_ao_shared
                    case 0x6b8805b0: //rdr2_pond_water
                    case 0x25a07a25: //rdr2_door_glow
                    case 0xd70c66e0: //rdr2_low_lod_singlesided
                    case 0x0018e2b6: //rdr2_glass_notint_shared
                    case 0x72a21ffe: //rdr2_glass_nodistortion_bump_spec_ao
                    case 0xc47e1378: //rdr2_alpha_bspec_ao_cloth
                    case 0x7668b157: //rdr2_glass_nodistortion_bump_spec_ao_shared
                    case 0x18c56b10: //rdr2_treerock_prototype
                    case 0x31ef2dcb: //rdr2_traintrack_low_lod
                    case 0xb5fdee16: //rdr2_traintrack
                    case 0x0c1b762b: //rdr2_debris
                    case 0xf551d60f: //rdr2_mirror
                    case 0x23dfa9fb: //rdr2_bump_spec_ambocc_reflection_shared
                    case 0x34454dee: //rdr2_bump_spec_ambocc_smooth_shared
                    case 0xf8a043a1: //rdr2_bump_spec_ao_cloth
                    case 0x4bc61b93: //rdr2_glass_notint
                    case 0x2ee5e6bb: //rdr2_bump_spec_ao_dirt_cloth
                    case 0x66e8f6a0: //rdr2_alpha_blend
                    case 0xe3915961: //rdr2_cliffwall
                    case 0x27f82c88: //rdr2_cati
                        SetupDefaultShader(shader);
                        break;
                    #endregion
                    default:
                        SetupDefaultShader(shader);
                        break;
                }

                switch (bucket)
                {
                    default: throw new Exception("Unknown RenderBucket");
                    case 0: ShaderBucket = ShaderBucket.Solid; break;  //Opaque
                    case 1: ShaderBucket = ShaderBucket.Solid; break; //Double-sided opaque
                    case 2: ShaderBucket = ShaderBucket.Alpha1; break; //Hair
                    case 3: ShaderBucket = ShaderBucket.Alpha1; break; //AlphaMask
                    case 4: ShaderBucket = ShaderBucket.Alpha2; break; //Water
                    case 5: ShaderBucket = ShaderBucket.Alpha2; break; //Transparent
                    case 6: ShaderBucket = ShaderBucket.Alpha2; break; //DistortionGlass
                    case 8: ShaderBucket = ShaderBucket.Alpha1; ShaderInputs.SetUInt32(0x0188ECE8, 1u); break; //Alpha
                }

                switch (hash)
                {
                    case 0x32A4918E: //rdr2_alpha
                    case 0x173D5F9D: //rdr2_grass
                    case 0x171C9E47: //rdr2_glass_glow
                    case 0x47A5EF34: //rdr2_graffiti
                    case 0xC714B86E: //rdr2_alpha_foliage
                    case 0x592D7DC2: //rdr2_alpha_foliage_no_fade
                    case 0xBBCB0BF8: //rdr2_alpha_bspec_ao_shareduv
                    case 0x0039EC69: //rdr2_alpha_bspec_ao_shareduv_character
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
                        ShaderInputs.SetFloat4(0x5C3AB6E9, new Vector4(1, 0, 0, 0)); //DecalMasks           
                        ShaderInputs.SetFloat(0x4D52C5FF, 1.0f); //AlphaScale
                        break;
                    case 0xB71272EA: //rdr2_flattenterrain
                        ShaderInputs.SetUInt32(0xE0D5A584, (uint)DefaultShader.NormalMapConfig.DirectWY); //"NormalMapConfig"
                        break;
                    case 0x4C03B90B: //rdr2_shadowonly
                        ShaderInputs.SetUInt32(0x0188ECE8, 1U);  //DecalMode - Hack to remove useless meshes
                        break;
                    case 0x227C5611: //rdr2_cliffwall_alpha
                        ShaderInputs.SetFloat(0x7CB163F5, 1.5f); //BumpScales
                        break;
                }
            }
        }

        private void SetupDefaultShader(Rsc6ShaderFX s) //diffuse + bump + ambocc
        {
            SetDefaultShader();
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0xE0D5A584, 30); //NormalMapConfig

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
                            case 0x2B5170FD: //texturesampler
                                Textures[0] = tex;
                                break;
                            case 0x46B7C64F: //bumpsampler
                            case 0x8AC11CB0: //normalsampler
                                Textures[1] = tex;
                                break;
                        }
                    }
                }
                else
                {
                    switch (parm.Hash)
                    {
                        case 0xF6712B81: //bumpiness
                            ShaderInputs.SetFloat(0xDF918855, parm.Vector.X * 0.5f); //BumpScale
                            break;
                        case 0xBBEED254: //fresnelterm         //~0.3-1, low for metals, ~0.96 for nonmetals
                            sfresnel = parm.Vector.X;
                            break;
                        case 0x484A5EBD: //specularcolorfactor   //0-1, final multiplier?
                            sintensitymult = parm.Vector.X;
                            break;
                        case 0x166E0FD1: //specularfactor    //10-150+?, higher is shinier
                            sfalloffmult = parm.Vector.X;
                            break;
                        case 0xE1322212: //mainuvmodulate
                            ShaderInputs.SetUInt32(0x01C01210, 1); //MeshUVMode
                            ShaderInputs.SetFloat4(0x9DBE8E24, parm.Vector); //MeshUVScaleOffset
                            break;
                    }
                }
            }
            ShaderInputs.SetFloat(0x57C22E45, FloatUtil.Saturate(sfalloffmult / 100.0f)); //MeshParamsMult
            ShaderInputs.SetFloat(0xDA9702A9, FloatUtil.Saturate(sintensitymult * (1.0f - ((sfresnel - 0.1f) / 0.896f)))); //MeshMetallicity
        }

        private void SetupBlendTerrainShader(Rsc6ShaderFX s)
        {
            SetCoreShader<BlendShader>(ShaderBucket.Solid);
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0x9B920BD, 25); //BlendMode
            ShaderInputs.SetFloat4(0x7CB163F5, Vector4.One);//"BumpScales"

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;

            if (s.Name == 0xC242DAA7) //rdr2_terrain_blend
            {
                bool rails = parms[0]?.Texture?.Name.StartsWith("rr") ?? false;
                ShaderInputs.SetFloat(0x7CB163F5, rails ? 1.5f : 2.5f); //BumpScales
            }

            Textures = new Texture[15];
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
                        case 0xFF4494B8: //terrainnormalsampler1
                            Textures[6] = prm.Texture;
                            break;
                        case 0x2B0FEC4E: //terrainnormalsampler2
                            Textures[7] = prm.Texture;
                            break;
                        case 0x1CAB4F85: //terrainnormalsampler3
                            Textures[8] = prm.Texture;
                            break;
                        case 0xC668A301: //terrainnormalsampler4
                            Textures[9] = prm.Texture;
                            break;
                        case 0xB82F068E: //terrainnormalsampler5
                            Textures[10] = prm.Texture;
                            break;
                        case 0xE4685EFC: //terrainnormalsampler6
                            Textures[11] = prm.Texture;
                            break;
                        case 0x0ED966D5: //terrainblendmap1
                            Textures[12] = prm.Texture;
                            break;
                        case 0xA0918A47: //terrainblendmap2
                            Textures[13] = prm.Texture;
                            break;
                        case 0x2B5170FD: //texturesampler - 'rdr2_terrain_blend' only
                            Textures[14] = prm.Texture;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (prm.Hash)
                    {
                        case 0x66C79BD6: //megatilerepetitions, how many times, across the 0-1 of the UV channel map, do the tiles repeat
                            ShaderInputs.SetFloat4(0x401BDDBB, prm.Vector); //"UVLookupIndex"
                            break;
                        case 0x4385A0D2: //megatileoffset - offset of the UV for the tile when at (0,0) in the channel map
                            ShaderInputs.SetFloat4(0xAD966CCC, prm.Vector); //"UVScaleOffset"      float4
                            break;
                        case 0x9FBAB08B: //blendmapscale1
                            ShaderInputs.SetFloat4(0xA83AA336, prm.Vector); //LODColourLevels    float4
                            break;
                        case 0xAC181AA0: //blendmapoffset1
                            ShaderInputs.SetFloat4(0x8D01D9A3, prm.Vector); //LODColourBlends    float4
                            break;
                        case 0x62503593: //blendmapscale2
                            ShaderInputs.SetFloat4(0xB0379AA1, prm.Vector); //HBBScales          float4
                            break;
                        case 0xBDDEBE2D: //blendmapoffset2
                            ShaderInputs.SetFloat4(0xFF6E0669, prm.Vector); //HBBOffsets         float4
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
            Textures = new Texture[3];

            for (int k = 0; k < parms.Length; k++)
            {
                var prm = parms[k];
                if (prm.DataType == 0)
                {
                    switch (prm.Hash)
                    {
                        case 0x2B5170FD: //texturesampler
                            Textures[0] = prm.Texture;
                            break;
                        case 0x46B7C64F: //bumpsampler
                            Textures[1] = prm.Texture;
                            break;
                        case 0x0ED966D5: //terrainblendmap1
                            Textures[2] = prm.Texture;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (prm.Hash)
                    {
                        case 0xF6712B81: //bumpiness
                            ShaderInputs.SetFloat4(0x7CB163F5, prm.Vector * 0.5f); //BumpScales
                            break;
                        case 0xE55CF27C: //blendmapscalecliffflatten
                        case 0x606B83EE: //blendmapscalecliff
                            ShaderInputs.SetFloat4(0xA83AA336, prm.Vector); //LODColourLevels    float4
                            break;
                        case 0x92165D5E: //blendmapoffsetcliffflatten
                        case 0x99276EAE: //blendmapoffsetcliff
                            ShaderInputs.SetFloat4(0x8D01D9A3, prm.Vector); //LODColourBlends    float4
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
            Textures = new Texture[3];

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
                            case 0xA3348DA6: //texturesampler3
                                Textures[1] = tex;
                                break;
                            case 0x46B7C64F: //bumpsampler
                                Textures[2] = tex;
                                break;
                        }
                    }
                }
                else
                {
                    switch (parm.Hash)
                    {
                        case 0xF6712B81: //bumpiness
                            ShaderInputs.SetFloat4(0x7CB163F5, parm.Vector * 0.5f); //BumpScales
                            break;
                    }
                }
            }
        }

        private void SetDiffuse3Shader(Rsc6ShaderFX s) //diffuse + diffuse2 + diffuse3 + bump + ambocc
        {
            SetCoreShader<BlendShader>(ShaderBucket.Solid);
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0x9B920BD, 23); //BlendMode

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;
            Textures = new Texture[4];

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
                            case 0x46B7C64F: //bumpsampler
                                Textures[3] = tex;
                                break;
                        }
                    }
                }
                else
                {
                    switch (parm.Hash)
                    {
                        case 0xF6712B81: //bumpiness
                            ShaderInputs.SetFloat4(0x7CB163F5, parm.Vector * 0.5f); //BumpScales
                            break;
                    }
                }
            }
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

        private void SetupGrassShader(Rsc6ShaderFX s)
        {
            SetDefaultShader();
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0xE0D5A584, 30); //NormalMapConfig
            ShaderInputs.SetUInt32(0x65DD2E63, 1); //MeshWindMode, grass wind mode
            ShaderInputs.SetFloat(0x8B342EF3, 1); //MeshWindAmount
            ShaderInputs.SetFloat(0xB52CC88F, BoundingBox.Size.Z); //MeshWindHeight
            ShaderState = ShaderState.Alpha;

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;
            Textures = new Texture[5];
            Textures[3] = Noise.Perm;

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
                                Textures[1] = tex;
                                break;
                        }
                    }
                }
                else
                {
                    switch (parm.Hash)
                    {
                        case 0xF6712B81: //bumpiness
                            ShaderInputs.SetFloat(0xDF918855, parm.Vector.X * 0.5f); //BumpScale
                            break;
                    }
                }
            }
        }

        private void SetupTreesShader(Rsc6ShaderFX s)
        {
            SetDefaultShader();
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0xE0D5A584, 30); //NormalMapConfig
            ShaderInputs.SetUInt32(0x65DD2E63, 5); //MeshWindMode, grass wind mode
            ShaderInputs.SetFloat(0x8B342EF3, 0.5f); //MeshWindAmount
            ShaderInputs.SetFloat(0xB52CC88F, MathF.Max(MathF.Max(BoundingBox.Maximum.Z, BoundingBox.Size.Z) * 3, 5)); //MeshWindHeight

            if (s == null) return;
            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;
            Textures = new Texture[5];
            Textures[3] = Noise.Perm;

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
                                Textures[1] = tex;
                                break;
                        }
                    }
                }
                else
                {
                    switch (parm.Hash)
                    {
                        case 0xF6712B81: //bumpiness
                            ShaderInputs.SetFloat(0xDF918855, parm.Vector.X * 0.5f); //BumpScale
                            break;
                    }
                }
            }
        }

        private void SetupWaterShader(Rsc6ShaderFX s, Model model)
        {
            var rippleSign = 1.0f;//use this to flip the direction of ripples up/down in case of handedness swap (currently won't do anything as ripples are just symmetrical sine waves! but for choppy waves this is important)
            SetCoreShader<RefractShader>(ShaderBucket.Translucency);
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0x047FEB72, 1); //NormalsMode - animated water
            ShaderInputs.SetUInt32(0x5598A4F3, 2); //RefractMode - SSR water
            ShaderInputs.SetFloat(0x3B55F632, 0.95f); //RefractiveIndex - for water this should be 0.75, but that's too much for screen space refraction
            ShaderInputs.SetFloat4(0x9A7F494F, new Vector4(0.1f, 0.02f, 0.01f, 0)); //RefractAbsorption
            ShaderInputs.SetFloat(0xCC9749CA, 5.0f); //RefractEdgeBlend
            ShaderInputs.SetUInt32(0xC7AFBF36, 1); //WaterFlowUVSource - texcoord1
            ShaderInputs.SetUInt32(0xD7CA292B, 1);//NormalsSource - always up (0,0,1)
            ShaderInputs.SetFloat(0x16557DF5, 0.01f * rippleSign);//WaterRippleHeight (base value)
            ShaderInputs.SetFloat4(0xF6B32D0C, new Vector4(0, 1, 0, 0));//WaterFlowTextureMaskX
            ShaderInputs.SetFloat4(0xE06E0082, new Vector4(0, 0, 0, -1));//WaterFlowTextureMaskY

            model.RenderInShadowView = false;

            var parms = s.ParametersList.Item?.Parameters;
            if (parms == null) return;

            Textures = new Texture[3];
            //Textures[2] = new Rsc6TextureBase() { Name = "waterfoam" }; //placeholder to signal load the actual texture here

            var rippleSize = new Vector3(1.0f, 1.0f, 0.25f * rippleSign);
            var rippleSpeed = 1.0f;
            var rippleUVscale = 0.0f;
            var flowUVscaleoff = new Vector4(1, 1, 0, 0);

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
                            case 0x991ECEBE: //fogsampler
                                Textures[0] = parm.Texture;
                                break;
                            case 0x485F22B0: //flowsampler
                                Textures[1] = parm.Texture;
                                break;
                            case 0x8F4A2632: //riverfoamsampler
                                Textures[2] = parm.Texture;
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    switch (parm.Hash)
                    {
                        case 0xd3cd3e65: //"ripplescale"
                        case 0x5BA61836: //"ripplescalej"
                            rippleUVscale = parm.Vector.X * 4;
                            break;
                        case 0xb9470b30: //"ripplebumpiness"
                            rippleSize.Z = parm.Vector.X * 0.5f * rippleSign;
                            break;
                        case 0x45e94323: //"ripplespeed"
                        case 0x59E18655: //"ripplespeedj"
                            rippleSpeed = parm.Vector.X * 0.1f;
                            break;
                        case 0x37BBB4F2: //"waterloduvscalesflow"
                            flowUVscaleoff.X = parm.Vector.X;
                            flowUVscaleoff.Y = parm.Vector.Y;
                            break;
                        case 0x9E125C7A: //"waterloduvoffsetsflow"
                            flowUVscaleoff.Z = parm.Vector.X;
                            flowUVscaleoff.W = parm.Vector.Y;
                            break;
                        case 0xDB2BCA5C: //"rippleamplitude"
                        case 0x417E6C4A: //"rippleamplitudej"
                            break;
                        case 0xAE16E2BD: //"rippleflowamplitude"
                        case 0x7FDBA5E7: //"rippleflowamplitudej"
                            //rippleSpeed = parm.Vector.X;// * 0.5f;
                            break;
                        default:
                            break;
                    }
                }
            }
            ShaderInputs.SetFloat3(0x22963AF5, rippleSize); //WaterRippleSize
            ShaderInputs.SetFloat(0xA52368EA, rippleSpeed); //WaterRippleSpeed
            ShaderInputs.SetFloat(0xDE5D864F, rippleUVscale); //WaterRippleUVScale, nonzero=texcoord0 mult
            ShaderInputs.SetFloat4(0xFF459F05, flowUVscaleoff); //WaterFlowUVScaleOffset
        }

        public override string ToString()
        {
            return VertexCount.ToString() + " verts, " + (ShaderRef?.ToString() ?? "NULL SHADER)");
        }
    }

    [TC(typeof(EXP))] public class Rsc6DrawableModel : Model, IRsc6Block //rage::grmModel
    {
        /*
         * Base class for all new model code rendered by RAGE.
         * It maintains information about shaders, matrices (i.e. for skinning) and other "generic" model information
         */

        public ulong BlockLength => 28;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public ulong VFT { get; set; } = 0x01854414;
        public Rsc6PtrArr<Rsc6DrawableGeometry> Geometries { get; set; } //m_Geometries
        public Rsc6RawArr<BoundingBox4> BoundsData { get; set; } //m_AABBs, one for each geometry + one for the whole model (unless there's only one model)
        public Rsc6RawArr<ushort> ShaderMapping { get; set; } //m_ShaderIndex
        public byte MatrixCount { get; set; } //m_MatrixCount, bone count
        public byte Flags { get; set; } //m_Flags
        public byte Type { get; set; } = 0xCD; //m_Type, always 0xCD?
        public byte MatrixIndex { get; set; } //m_MatrixIndex
        public byte Stride { get; set; } //m_Stride, always 0?
        public byte SkinFlag { get; set; } //m_SkinFlag, determine whether to render with the skinned draw path or not
        public ushort GeometriesCount { get; set; } //m_Count

        public BoundingBox BoundingBox { get; set; } //Created from first GeometryBounds item

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
            GeometriesCount = reader.ReadUInt16();

            var geocount = Geometries.Count;
            ShaderMapping = reader.ReadRawArrItems(ShaderMapping, geocount);
            BoundsData = reader.ReadRawArrItems(BoundsData, geocount > 1 ? geocount + 1u : geocount);

            var geoms = Geometries.Items;
            if (geoms != null)
            {
                var smap = ShaderMapping.Items;
                var bdat = BoundsData.Items;
                for (int i = 0; i < geoms.Length; i++)
                {
                    var geom = geoms[i];
                    if (geom != null)
                    {
                        geom.ShaderID = ((smap != null) && (i < smap.Length)) ? smap[i] : (ushort)0;
                        geom.AABB = (bdat != null) ? ((bdat.Length > 1) && ((i + 1) < bdat.Length)) ? bdat[i + 1] : bdat[0] : new BoundingBox4();
                        geom.BoundingBox = new BoundingBox(geom.AABB.Min.XYZ(), geom.AABB.Max.XYZ());
                        geom.BoundingSphere = new BoundingSphere(geom.BoundingBox.Center, geom.BoundingBox.Size.Length() * 0.5f);
                    }
                }

                if ((bdat != null) && (bdat.Length > 0))
                {
                    ref var bb = ref bdat[0];
                    BoundingBox = new BoundingBox(bb.Min.XYZ(), bb.Max.XYZ());
                }
            }

            Meshes = Geometries.Items;
            RenderInMainView = true;
            RenderInShadowView = true;
            RenderInEnvmapView = true;
        }

        public void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable;
            bool wft = writer.BlockList[0] is Rsc6Fragment;

            if (wfd)
                writer.WriteUInt32(0x00D30B04);
            else if (wft)
                writer.WriteUInt32(0x00EF0B04);
            else
                writer.WriteUInt32((uint)VFT);

            writer.WritePtrArr(Geometries);
            writer.WriteRawArr(BoundsData);
            writer.WriteRawArr(ShaderMapping);
            writer.WriteByte(MatrixCount);
            writer.WriteByte(Flags);
            writer.WriteByte(Type);
            writer.WriteByte(MatrixIndex);
            writer.WriteByte(Stride);
            writer.WriteByte(SkinFlag);
            writer.WriteUInt16(GeometriesCount);
        }

        public override void Read(MetaNodeReader reader)
        {
            Type = reader.ReadByte("Mask", 0xCD);
            SkinFlag = reader.ReadByte("HasSkin");
            MatrixIndex = reader.ReadByte("BoneIndex");
            MatrixCount = reader.ReadByte("BoneCount");

            var min = Rpf6Crypto.ToXYZ(reader.ReadVector3("BoundingBoxMin"));
            var max = Rpf6Crypto.ToXYZ(reader.ReadVector3("BoundingBoxMax"));
            BoundingBox = new BoundingBox(min, max);
            Geometries = new(reader.ReadNodeArray<Rsc6DrawableGeometry>("Geometries"));

            var geoms = Geometries.Items;
            var gcnt = geoms?.Length ?? 0;
            var smaps = (gcnt > 0) ? new ushort[gcnt] : null;
            var boundsCount = (uint)(gcnt > 1 ? gcnt + 1 : gcnt);
            var bndoff = (gcnt > 1) ? 1 : 0;

            var gbnds = new BoundingBox4[boundsCount];
            gbnds[0] = new BoundingBox4(BoundingBox);

            for (int i = 0; i < gcnt; i++)
            {
                smaps[i] = geoms[i].ShaderID;
                gbnds[i + bndoff] = geoms[i].AABB;
            }

            Flags = (SkinFlag == 1) ? (byte)Rsc6ModelFlags.MODEL_RELATIVE : (byte)0;
            BoundsData = new(gbnds);
            ShaderMapping = new(smaps);
            GeometriesCount = (ushort)gcnt;
        }

        public override void Write(MetaNodeWriter writer)
        {
            if (Type != 0xCD) writer.WriteByte("Mask", Type);
            if (SkinFlag == 1) writer.WriteByte("HasSkin", SkinFlag);
            if (MatrixIndex != 0) writer.WriteByte("BoneIndex", MatrixIndex);
            writer.WriteByte("BoneCount", MatrixCount);
            writer.WriteVector3("BoundingBoxMin", BoundingBox.Minimum);
            writer.WriteVector3("BoundingBoxMax", BoundingBox.Maximum);
            writer.WriteNodeArray("Geometries", Geometries.Items);
        }

        public enum Rsc6ModelFlags : byte
        {
            MODEL_RELATIVE = 0x01,
            RESOURCED = 0x02
        };

        public override string ToString()
        {
            var geocount = Geometries.Items?.Length ?? 0;
            return "(" + geocount.ToString() + " geometr" + (geocount != 1 ? "ies)" : "y)");
        }
    }

    [TC(typeof(EXP))] public class Rsc6VertexBuffer : Rsc6BlockBase, MetaNode //rage::grcVertexBuffer
    {
        public override ulong BlockLength => 64;
        public uint VFT { get; set; } = 0x01858684;
        public ushort VertexCount { get; set; } //m_VertCount
        public byte Locked { get; set; } //m_Locked, always 0?
        public byte Flags { get; set; } //m_Flags, mostly 0, sometimes 1 with 'p_gen_ropesm'
        public Rsc6RawArr<byte> LockedData { get; set; } //m_pLockedData, pointer to buffer obtained by grcVertexBufferD11::Lock, in file, same as m_pVertexData
        public uint VertexStride { get; set; } //m_Stride
        public Rsc6RawArr<byte> VertexData { get; set; } //m_pVertexData
        public uint LockThreadID { get; set; } //m_dwLockThreadId, always 0?
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
            VertexData = reader.ReadRawArrPtr<byte>();
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

            LockedData = reader.ReadRawArrItems(LockedData, (uint)(VertexCount * Layout.Item.FVFSize));
            VertexData = reader.ReadRawArrItems(VertexData, (uint)(VertexCount * Layout.Item.FVFSize));
        }

        public override void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable;
            bool wft = writer.BlockList[0] is Rsc6Fragment;
            bool wsg = writer.BlockList[0] is Rsc6SectorGrass;

            if (wfd)
                writer.WriteUInt32(0x00D34D6C);
            else if (wft)
                writer.WriteUInt32(0x00EF4D6C);
            else if (wsg)
                writer.WriteUInt32(0x04A3E26C);
            else
                writer.WriteUInt32(VFT);

            writer.WriteUInt16(VertexCount);
            writer.WriteByte(Locked);
            writer.WriteByte(Flags);
            writer.WriteRawArr(LockedData); //Should be NULL
            writer.WriteUInt32(VertexStride);
            writer.WriteRawArr(VertexData);
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

        public void Read(MetaNodeReader reader)
        {
            Locked = reader.ReadByte("Locked");
            Flags = reader.ReadByte("Flags");
            LockThreadID = reader.ReadUInt32("LockThreadID");
            Layout = new(reader.ReadNode<Rsc6VertexDeclaration>("Layout"));
        }

        public void Write(MetaNodeWriter writer)
        {
            if (Locked != 0) writer.WriteByte("Locked", Locked);
            if (Flags != 0) writer.WriteByte("Flags", Flags);
            if (LockThreadID != 0) writer.WriteUInt32("LockThreadID", LockThreadID);
            writer.WriteNode("Layout", Layout.Item);
        }

        public bool IsReadWrite()
        {
            return (Flags & ((byte)Rsc6VertexBufferFlags.READ_WRITE | (byte)Rsc6VertexBufferFlags.DYNAMIC)) != 0;
        }

        public bool IsDynamic()
        {
            return (Flags & (byte)Rsc6VertexBufferFlags.DYNAMIC) != 0;
        }

        public bool IsPreallocatedMemory()
        {
            return (Flags & (byte)Rsc6VertexBufferFlags.PREALLOCATED_MEMORY) != 0;
        }

        public override string ToString()
        {
            var cstr = "Count: " + VertexCount.ToString();
            if (Layout.Item == null) return "!NULL LAYOUT! - " + cstr;
            return "Type: " + Layout.Item.FVF.ToString() + ", " + cstr;
        }
    }

    [TC(typeof(EXP))] public class Rsc6IndexBuffer : Rsc6BlockBase //rage::grcIndexBuffer
    {
        public override ulong BlockLength => 48;
        public uint VFT { get; set; } = 0x01858D60;
        public uint IndicesCount { get; set; } //m_IndexCount
        public uint Unknown_Ch { get; set; } //Always 0?
        public Rsc6RawArr<ushort> Indices { get; set; } //m_IndexData
        public uint Unknown_10h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_14h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_18h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_1Ch { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_20h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_24h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_28h { get; set; } = 0xCDCDCDCD; //Padding
        public uint Unknown_2Ch { get; set; } = 0xCDCDCDCD; //Padding

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            IndicesCount = reader.ReadUInt32();
            Indices = reader.ReadRawArrPtr<ushort>();
            Unknown_Ch = reader.ReadUInt32();
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
            Unknown_1Ch = reader.ReadUInt32();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
            Indices = reader.ReadRawArrItems(Indices, IndicesCount);
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(VFT);
            writer.WriteUInt32(IndicesCount);
            writer.WriteRawArr(Indices);
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
    }

    [TC(typeof(EXP))] public class Rsc6Drawable : Piece, IRsc6Block //rmcDrawable (grmShaderGroup + crSkeletonData + rmcLodGroup)
    {
        /*
         * An rmcDrawable contains up to four levels of detail; each level of detail
         * consists of zero or more models. Each model within the LOD can be bound to
         * a different bone, allowing complex objects to render with a single draw call.
         * It also contains a shader group, which is an array of all shaders used by all
         * models within the drawable.
         */

        public virtual ulong BlockLength => 120;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public uint VFT { get; set; } = 0x01908F7C;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6Ptr<Rsc6ShaderGroup> ShaderGroup { get; set; } //rage::grmShaderGroup
        public Rsc6Ptr<Rsc6SkeletonData> SkeletonRef { get; set; } //rage::crSkeletonData
        public Vector4 BoundingCenter { get; set; } //m_CullSphere
        public Vector4 BoundingBoxMin { get; set; } //m_BoxMin
        public Vector4 BoundingBoxMax { get; set; } //m_BoxMax
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
        public uint PpuOnly { get; set; } //m_PpuOnly, 0 or 1 if PPU is set as default processor
        public JenkHash NameHash { get; set; }

        public virtual void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            ShaderGroup = reader.ReadPtr<Rsc6ShaderGroup>();
            SkeletonRef = reader.ReadPtr<Rsc6SkeletonData>();
            BoundingCenter = reader.ReadVector4();
            BoundingBoxMin = reader.ReadVector4();
            BoundingBoxMax = reader.ReadVector4();
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

            Name = Path.GetFileNameWithoutExtension(reader.FileEntry.Name);
            Lods =
            [
                LodHigh.Item,
                LodMed.Item,
                LodLow.Item,
                LodVlow.Item
            ];

            if (LodHigh.Item != null) LodHigh.Item.LodDist = LodDistHigh;
            if (LodMed.Item != null) LodMed.Item.LodDist = LodDistMed;
            if (LodLow.Item != null) LodLow.Item.LodDist = LodDistLow;
            if (LodVlow.Item != null) LodVlow.Item.LodDist = LodDistVlow;

            UpdateAllModels();
            SetSkeleton(SkeletonRef.Item);
            AssignShaders();
            CreateTexturePack(reader.FileEntry);

            UpdateBounds();
            BoundingSphere = new BoundingSphere(BoundingBox.Center, BoundingSphereRadius);

            //Approximative fix for displaying bounds
            if (BoundingCenter.Z > 0.05f && !Name.EndsWith('x'))
                Rsc6Fragment.SkinnedHeightPos = (BoundingCenter.Z > 0.6f) ? 1.0f : ((BoundingCenter.Z < 0.4f) ? 0.0f : BoundingCenter.Z + 0.1f);
            else
                Rsc6Fragment.SkinnedHeightPos = 0.0f;
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable;
            bool wft = writer.BlockList[0] is Rsc6Fragment;

            if (wfd)
                writer.WriteUInt32(0x00E63DF0u);
            else if (wft)
                writer.WriteUInt32(0x00F230C0);
            else
                writer.WriteUInt32(VFT);

            writer.WritePtr(BlockMap);
            writer.WritePtr(ShaderGroup);
            writer.WritePtr(SkeletonRef);
            writer.WriteVector4(BoundingCenter);
            writer.WriteVector4(BoundingBoxMin);
            writer.WriteVector4(BoundingBoxMax);
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

        public override void Read(MetaNodeReader reader)
        {
            var nan = Rpf6Crypto.FNaN;
            Name = reader.ReadString("Name");
            BoundingCenter = new Vector4(Rpf6Crypto.ToXYZ(reader.ReadVector3("BoundingSphereCenter")), nan);
            BoundingSphereRadius = reader.ReadSingle("BoundingSphereRadius");
            BoundingBoxMin = new Vector4(Rpf6Crypto.ToXYZ(reader.ReadVector3("BoundingBoxMin")), nan);
            BoundingBoxMax = new Vector4(Rpf6Crypto.ToXYZ(reader.ReadVector3("BoundingBoxMax")), nan);
            LodDistHigh = reader.ReadSingle("LodDistHigh");
            LodDistMed = reader.ReadSingle("LodDistMed");
            LodDistLow = reader.ReadSingle("LodDistLow");
            LodDistVlow = reader.ReadSingle("LodDistVlow");
            DrawBucketMaskHigh = reader.ReadUInt32("FlagsHigh");
            DrawBucketMaskMed = reader.ReadUInt32("FlagsMed");
            DrawBucketMaskLow = reader.ReadUInt32("FlagsLow");
            DrawBucketMaskVlow = reader.ReadUInt32("FlagsVlow");
            ShaderGroup = new(reader.ReadNode<Rsc6ShaderGroup>("ShaderGroup"));
            SkeletonRef = new(reader.ReadNode<Rsc6SkeletonData>("Skeleton"));
            LodHigh = new(reader.ReadNode<Rsc6DrawableLod>("LodHigh"));
            LodMed = new(reader.ReadNode<Rsc6DrawableLod>("LodMed"));
            LodLow = new(reader.ReadNode<Rsc6DrawableLod>("LodLow"));
            LodVlow = new(reader.ReadNode<Rsc6DrawableLod>("LodVlow"));
            PpuOnly = reader.ReadUInt32("PpuOnly");

            Skeleton = SkeletonRef.Item;
            Lods = new[]
            {
                LodHigh.Item,
                LodMed.Item,
                LodLow.Item,
                LodVlow.Item
            };

            UpdateAllModels();
            SetSkeleton(SkeletonRef.Item);
            AssignShaders();
        }

        public override void Write(MetaNodeWriter writer)
        {
            if (Name != null) writer.WriteString("Name", Name);
            if (BoundingCenter != default) writer.WriteVector3("BoundingSphereCenter", BoundingCenter.XYZ());
            if (BoundingSphereRadius != default) writer.WriteSingle("BoundingSphereRadius", BoundingSphereRadius);
            if (BoundingBoxMin != default) writer.WriteVector3("BoundingBoxMin", BoundingBoxMin.XYZ());
            if (BoundingBoxMax != default) writer.WriteVector3("BoundingBoxMax", BoundingBoxMax.XYZ());
            if (LodDistHigh != 0) writer.WriteSingle("LodDistHigh", LodDistHigh);
            if (LodDistMed != 0) writer.WriteSingle("LodDistMed", LodDistMed);
            if (LodDistLow != 0) writer.WriteSingle("LodDistLow", LodDistLow);
            if (LodDistVlow != 0) writer.WriteSingle("LodDistVlow", LodDistVlow);
            if (DrawBucketMaskHigh != 0) writer.WriteUInt32("FlagsHigh", DrawBucketMaskHigh);
            if (DrawBucketMaskMed != 0) writer.WriteUInt32("FlagsMed", DrawBucketMaskMed);
            if (DrawBucketMaskLow != 0) writer.WriteUInt32("FlagsLow", DrawBucketMaskLow);
            if (DrawBucketMaskVlow != 0) writer.WriteUInt32("FlagsVlow", DrawBucketMaskVlow);
            writer.WriteNode("ShaderGroup", ShaderGroup.Item);
            writer.WriteNode("Skeleton", SkeletonRef.Item);
            writer.WriteNode("LodHigh", LodHigh.Item);
            writer.WriteNode("LodMed", LodMed.Item);
            writer.WriteNode("LodLow", LodLow.Item);
            writer.WriteNode("LodVlow", LodVlow.Item);
            writer.WriteUInt32("PpuOnly", PpuOnly);
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
                            if (model.Meshes[j] is Rsc6DrawableGeometry mesh)
                            {
                                var shader = (mesh.ShaderID < shaders.Length) ? shaders[mesh.ShaderID] : null;
                                mesh.SetShader(shader, model);
                            }
                        }
                    }
                }
            }
        }

        public void SetSkeleton(Rsc6SkeletonData skel)
        {
            Skeleton = skel;
            if (AllModels == null) return;

            var bones = skel?.Bones;
            if (bones == null) return;

            var origbones = (skel != SkeletonRef.Item) ? SkeletonRef.Item.BoneData.Items : null;
            foreach (var model in AllModels.Cast<Rsc6DrawableModel>())
            {
                if (model == null) continue;
                if (model.Meshes == null) continue;

                var boneidx = model.MatrixIndex;
                if ((model.SkinFlag == 0) && (boneidx < bones.Length))
                {
                    if (model.Meshes != null)
                    {
                        foreach (var mesh in model.Meshes)
                        {
                            mesh.BoneIndex = boneidx;
                            if ((boneidx < 0) && (bones.Length > 1))
                            {
                                mesh.Enabled = false;
                            }
                        }
                    }
                }
                else if (model.SkinFlag == 1)
                {
                    foreach (var mesh in model.Meshes)
                    {
                        if (mesh is not Rsc6DrawableGeometry geom) continue;
                        var boneids = geom.BoneIds.Items;
                        if (boneids != null)
                        {
                            var boneinds = new int[boneids.Length];
                            for (int i = 0; i < boneinds.Length; i++)
                            {
                                if (origbones != null) //Make sure to preseve original bone ordering!
                                {
                                    var origbone = origbones[boneids[i]];
                                    if ((origbone != null) && skel.BonesMap.TryGetValue(origbone.BoneId, out var newbone))
                                    {
                                        boneinds[i] = newbone.Index;
                                    }
                                    else
                                    {
                                        boneinds[i] = boneids[i];
                                    }
                                }
                                else
                                {
                                    boneinds[i] = boneids[i];
                                }
                            }
                            geom.Rig = new SkeletonRig(skel, true, boneinds);
                        }
                        else
                        {
                            geom.Rig = new SkeletonRig(skel, true);
                        }
                        geom.RigMode = MeshRigMode.MeshRig;
                        geom.IsSkin = true;
                    }
                }
            }
        }

        public void ApplyTextures(Rsc6TextureDictionary txd)
        {
            //if(txd!=null)return;
            var dict = txd?.DictStr;
            var dict2 = txd?.Dict;
            if (dict == null) return;
            if (dict2 == null) return;
            if (AllModels == null) return;
            for (int m = 0; m < AllModels.Length; m++)
            {
                var model = AllModels[m];
                if (model == null) continue;
                if (model.Meshes == null) continue;
                for (int n = 0; n < model.Meshes.Length; n++)
                {
                    var mesh = model.Meshes[n];
                    if (mesh == null) continue;
                    if (mesh.Textures == null) continue;
                    for (int i = 0; i < mesh.Textures.Length; i++)
                    {
                        var texture = mesh.Textures[i];
                        if (texture == null) continue;
                        //if (texture.Data != null) continue;//it's already loaded - need to replace or not? TODO
                        var texnamel = texture.Name?.ToLowerInvariant() ?? "";
                        if (dict.TryGetValue(texnamel, out var packtex))
                        {
                            mesh.Textures[i] = packtex;
                        }
                        else if (dict2.TryGetValue(texnamel, out packtex))
                        {
                            mesh.Textures[i] = packtex;
                        }
                        else
                        { }
                    }
                }
            }
        }


        private void CreateTexturePack(GameArchiveFileInfo e)
        {
            var txd = ShaderGroup.Item?.TextureDictionary.Item;
            if (txd == null) return;

            var texs = txd.Textures.Items;
            var txp = new TexturePack(e)
            {
                Textures = []
            };

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

        public void BuildMasks() //TODO: make sure this works
        {
            var hmask = (LodDistHigh > 0) ? BuildMask(LodHigh.Item?.ModelsData.Items) : (byte)0;
            var mmask = (LodDistMed > 0) ? BuildMask(LodMed.Item?.ModelsData.Items) : (byte)0;
            var lmask = (LodDistLow > 0) ? BuildMask(LodLow.Item?.ModelsData.Items) : (byte)0;
            var vmask = (LodDistVlow > 0) ? BuildMask(LodVlow.Item?.ModelsData.Items) : (byte)0;

            DrawBucketMaskHigh = hmask;
            DrawBucketMaskMed = mmask;
            DrawBucketMaskLow = lmask;
            DrawBucketMaskVlow = vmask;
        }

        private byte BuildMask(Rsc6DrawableModel[] models) //TODO: make sure this works
        {
            byte mask = 0;
            if (models != null)
            {
                foreach (var model in models)
                {
                    mask = (byte)(mask | model.Stride);
                }
            }
            return mask;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6Skeleton : IRsc6Block, MetaNode //rage::crSkeleton
    {
        /*
         * Instance of a particular skeleton.
         * Holds the current pose information of a particular character in a game.
         * Sets the local matrices of all the crBones, and then computes the current object matrices for each bone.
         */

        public ulong BlockLength => 40;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public uint Flags { get; set; } //m_Flags, always 0
        public Rsc6Ptr<Rsc6SkeletonData> Skeleton { get; set; } //m_SkeletonData, same as Fragment.Drawable.SkeletonRef
        public Rsc6RawArr<Matrix4x4> ParentMtx { get; set; } //m_ParentMtx, always NULL
        public Rsc6RawLst<Rsc6Bone> Bones { get; set; } //m_Bones
        public int NumBones { get; set; } //m_NumBones
        public ushort UpdateBufferIdx { get; set; } //m_UpdateBufferIdx
        public ushort RenderBufferIdx { get; set; } //m_RenderBufferIdx
        public Rsc6ManagedArr<Rsc6MatrixBuffer> GlobalMtxBuffers { get; set; } //m_GlobalMtxBuffers
        public uint Unknown_20h { get; set; } //Always 0
        public uint Unknown_24h { get; set; } //Always 0

        public void Read(Rsc6DataReader reader)
        {
            Flags = reader.ReadUInt32();
            Skeleton = reader.ReadPtr<Rsc6SkeletonData>();
            ParentMtx = reader.ReadRawArrPtr<Matrix4x4>();
            Bones = reader.ReadRawLstPtr<Rsc6Bone>();
            NumBones = reader.ReadInt32();
            UpdateBufferIdx = reader.ReadUInt16();
            RenderBufferIdx = reader.ReadUInt16();
            GlobalMtxBuffers = reader.ReadArr<Rsc6MatrixBuffer>();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();

            ParentMtx = reader.ReadRawArrItems(ParentMtx, (uint)NumBones);
            Bones = reader.ReadRawLstItems(Bones, (uint)NumBones);
        }

        public void Write(Rsc6DataWriter writer)
        {
            var skel = writer.BlockList.OfType<Rsc6SkeletonData>().FirstOrDefault();
            NumBones = skel?.BoneCount ?? 0;

            writer.WriteUInt32(Flags);
            writer.WritePtrEmbed(skel, skel, 0);
            writer.WriteRawArr(ParentMtx);
            writer.WriteRawLst(Bones);
            writer.WriteInt32(NumBones);
            writer.WriteUInt16(UpdateBufferIdx);
            writer.WriteUInt16(RenderBufferIdx);
            writer.WriteArr(GlobalMtxBuffers);
            writer.WriteUInt32(Unknown_20h);
            writer.WriteUInt32(Unknown_24h);
        }

        public void Read(MetaNodeReader reader)
        {
            ParentMtx = new(Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4Array("ParentMtx")));
            Bones = new(reader.ReadNodeArray<Rsc6Bone>("Bones"));
            UpdateBufferIdx = reader.ReadUInt16("UpdateBufferIdx");
            RenderBufferIdx = reader.ReadUInt16("RenderBufferIdx");
            GlobalMtxBuffers = new(reader.ReadNodeArray<Rsc6MatrixBuffer>("GlobalMtxBuffers"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt16("UpdateBufferIdx", UpdateBufferIdx);
            writer.WriteUInt16("RenderBufferIdx", RenderBufferIdx);
            writer.WriteMatrix4x4Array("ParentMtx", ParentMtx.Items);
            writer.WriteNodeArray("Bones", Bones.Items);
            writer.WriteNodeArray("GlobalMtxBuffers", GlobalMtxBuffers.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6Bone : IRsc6Block, MetaNode //rage::crBone
    {
        public ulong BlockLength => 80;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Matrix4x4 LocalMtx { get; set; } //m_LocalMtx
        public Rsc6Ptr<Rsc6Skeleton> Skeleton { get; set; } //m_Skeleton
        public int Index { get; set; } //m_Index
        public uint Dofs { get; set; } //m_Dofs, bone degree-of-freedom flags
        public uint Padding { get; set; } //m_Padding

        public Rsc6Skeleton SkeletonRef { get; set; } //For writing purposes

        public void Read(Rsc6DataReader reader)
        {
            LocalMtx = reader.ReadMatrix4x4();
            Skeleton = reader.ReadPtr<Rsc6Skeleton>();
            Index = reader.ReadInt32();
            Dofs = reader.ReadUInt32();
            Padding = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteMatrix4x4(LocalMtx);
            writer.WritePtrEmbed(SkeletonRef, SkeletonRef, 0);
            writer.WriteInt32(Index);
            writer.WriteUInt32(Dofs);
            writer.WriteUInt32(Padding);
        }

        public void Read(MetaNodeReader reader)
        {
            LocalMtx = Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4("LocalMtx"));
            Dofs = reader.ReadUInt32("Dofs");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteMatrix4x4("LocalMtx", LocalMtx);
            writer.WriteUInt32("Dofs", Dofs);
        }
    }

    [TC(typeof(EXP))] public class Rsc6SkeletonData : Skeleton, IRsc6Block //rage::crSkeletonData
    {
        /*
         * Holds data that applies to all crSkeleton's of a particular type.
         * Most of its actual data is in the crBoneData it owns, and in the structure they describe.
         */

        public ulong BlockLength => 68;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Rsc6RawLst<Rsc6BoneData> BoneData { get; set; } //m_Bones, pointer to bone data, can't be NULL
        public Rsc6RawArr<int> ParentIndices { get; set; } //m_ParentIndices, pointer to parent indices table, NULL if none calculated
        public Rsc6RawArr<Matrix4x4> JointScaleOrients { get; set; } //m_CumulativeJointScaleOrients, mostly NULL
        public Rsc6RawArr<Matrix4x4> InverseJointScaleOrients { get; set; } //m_CumulativeInverseJointScaleOrients, inverse cumulative joint scale orient matrices
        public Rsc6RawArr<Matrix4x4> DefaultTransforms { get; set; } //m_DefaultTransforms, default bone transform
        public Rsc6RawArr<Matrix4x4> CumulativeDefaultTransforms { get; set; } //m_CumulativeDefaultTransforms, default bone transform * parent bone transform
        public ushort BoneCount { get; set; } // m_NumBones, number of bones in skeleton
        public ushort NumTranslationDofs { get; set; } //m_NumTranslationDofs
        public ushort NumRotationDofs { get; set; } //m_NumRotationDofs
        public ushort NumScaleDofs { get; set; } //m_NumScaleDofs
        public uint Flags { get; set; } = 10; //m_Flags, range from 9 to 15?
        public Rsc6ManagedArr<Rsc6SkeletonBoneTag> BoneIDs { get; set; } //m_BoneIdTable, rage::crSkeletonData
        public uint RefCount { get; set; } = 1; //m_RefCount
        public uint Signature { get; set; } //m_Signature, skeleton signature (a hash value that identifies the skeleton's structure, the order of the branches of child bones matter)
        public Rsc6Str JointDataFileName { get; set; } //m_JointDataFileName, always NULL?
        public uint JointData { get; set; } = 13814012; //m_JointData, 13814012/16435452/15649020/10595580
        public uint Unknown6 { get; set; } //Padding
        public uint Unknown7 { get; set; } //Padding

        public Dictionary<Rsc6BoneIdEnum, Rsc6BoneData> BonesMap { get; set; } //For convienience finding bones by tag

        public void Read(Rsc6DataReader reader)
        {
            BoneData = reader.ReadRawLstPtr<Rsc6BoneData>();
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
                var b = (Rsc6BoneData)Bones[i];
                b.ParentIndex = (ParentIndices.Items != null) ? ParentIndices.Items[i] : 0;
                b.JointScaleOrients = (JointScaleOrients.Items != null) ? JointScaleOrients.Items[i] : Matrix4x4.Identity;
                b.InverseJointScaleOrients = (InverseJointScaleOrients.Items != null) ? InverseJointScaleOrients.Items[i] : Matrix4x4.Identity;
                b.DefaultTransforms = (DefaultTransforms.Items != null) ? DefaultTransforms.Items[i] : Matrix4x4.Identity;
                Bones[i] = b;
            }

            for (uint i = 0; i < BoneCount; i++)
            {
                var bone = (Rsc6BoneData)Bones[i];
                var ns = bone.NextSibling;
                var fc = bone.FirstChild;
                var pr = bone.ParentRef;

                if (reader.BlockPool.TryGetValue(ns.Position, out var nsi)) ns.Item = nsi as Rsc6BoneData;
                if (reader.BlockPool.TryGetValue(fc.Position, out var fci)) fc.Item = fci as Rsc6BoneData;
                if (reader.BlockPool.TryGetValue(pr.Position, out var pri)) pr.Item = pri as Rsc6BoneData;

                bone.NextSibling = ns;
                bone.FirstChild = fc;
                bone.ParentRef = pr;
                bone.Parent = pr.Item;
            }

            var bonesSorted = Bones.ToList();
            bonesSorted.Sort((a, b) => a.Index.CompareTo(b.Index));

            for (int i = 0; i < bonesSorted.Count; i++)
            {
                var bone = bonesSorted[i];
                bone.UpdateAnimTransform();
                bone.AbsTransform = bone.AnimTransform;
                bone.BindTransformInv = Matrix4x4Ext.Invert(bone.AnimTransform);
                bone.BindTransformInv.M44 = 1.0f;
                bone.UpdateSkinTransform();
            }

            BuildBoneTags(false);
            UpdateBoneTransforms();
            BuildBonesDictionary();
        }

        public void Write(Rsc6DataWriter writer)
        {
            var bd = new Rsc6SkeletonBoneData(BoneData.Items);
            writer.WriteBlock(bd);
            writer.WritePtrEmbed(bd, bd, 0);
            writer.WriteRawArr(ParentIndices);
            writer.WriteRawArr(JointScaleOrients);
            writer.WriteRawArr(InverseJointScaleOrients);
            writer.WriteRawArr(DefaultTransforms);
            writer.WriteRawArr(CumulativeDefaultTransforms);
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

        public override void Read(MetaNodeReader reader)
        {
            Flags = reader.ReadUInt32("Flags");
            Signature = reader.ReadUInt32("Signature");
            JointData = reader.ReadUInt32("JointData");
            BoneData = new(reader.ReadNodeArray<Rsc6BoneData>("Bones"));
            BoneIDs = new(reader.ReadNodeArray<Rsc6SkeletonBoneTag>("BoneIDs"));

            Bones = BoneData.Items;
            BoneCount = (ushort)(BoneData.Items?.Length ?? 0);

            for (int i = 0; i < BoneCount; i++)
            {
                var bone = (Rsc6BoneData)Bones[i];
                NumTranslationDofs += bone.NumTransChannels;
                NumRotationDofs += bone.NumRotChannels;
                NumScaleDofs += bone.NumScaleChannels;
            }

            BuildIndices();
            AssignBoneParents();
            BuildTransformations();
            BuildBoneTags(true);

            if (Signature <= 0)
            {
                CalculateSignature();
            }
        }

        public override void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("Flags", Flags);
            writer.WriteUInt32("Signature", Signature);
            writer.WriteUInt32("JointData", JointData);
            writer.WriteNodeArray("Bones", BoneData.Items);
            writer.WriteNodeArray("BoneIDs", BoneIDs.Items);
        }

        public void CalculateSignature()
        {
            //Calculate signature
            uint signature = 0;
            for (int i = 0; i < BoneCount; i++)
            {
                var bone = (Rsc6BoneData)Bones[i];
                var idAndDofs = ((uint)bone.BoneId << 32) | bone.Dofs;
                signature = UpdateCrc32(signature, BitConverter.GetBytes(idAndDofs));
            }
            Signature = signature;
        }

        private static uint UpdateCrc32(uint crc, byte[] data)
        {
            var crc32 = new Crc32();
            crc32.Update(data);
            return (uint)crc32.Value ^ crc; //XOR the CRC32 value with the existing crc
        }

        public bool HasBoneIDs()
        {
            for (int i = 0; i < BoneCount; i++)
            {
                var bone = (Rsc6BoneData)Bones[i];
                if ((ushort)bone.BoneId != i)
                {
                    return true;
                }
            }
            return false;
        }

        public void BuildBoneTags(bool fromXml)
        {
            BonesMap = [];
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
                    BonesMap[bone.BoneId] = bone;
                    tags.Add(tag);
                }
            }

            if (!fromXml) return;
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

            if (BoneIDs.Items == null)
            {
                tags = tags.OrderBy(tag => tag.BoneIndex).ToList();
                BoneIDs = new([.. tags]);
            }
        }

        public void BuildTransformations()
        {
            var transforms = new List<Matrix4x4>();
            var cumulativeTransforms = new List<Matrix4x4>();

            if (Bones != null)
            {
                foreach (var bone in Bones.Cast<Rsc6BoneData>())
                {
                    var localTransform = Matrix4x4Ext.Transformation(bone.Scale, bone.Rotation, bone.Position);
                    transforms.Add(localTransform);

                    var cumulativeTransform = localTransform;
                    var pbone = bone.Parent;

                    while (pbone != null)
                    {
                        var parentTransform = Matrix4x4Ext.Transformation(pbone.Scale, pbone.Rotation, pbone.Position);
                        cumulativeTransform *= parentTransform;
                        pbone = pbone.Parent;
                    }

                    cumulativeTransforms.Add(cumulativeTransform);
                    //bone.GlobalOffset = new Vector4(cumulativeTransform.Translation, 0.0f);
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
            ParentIndices = new((parents.Count > 0) ? [.. parents] : null);
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
                        bone.ParentRef = new Rsc6Ptr<Rsc6BoneData>(bones[pind]);
                    }

                    if (sibling >= 0)
                    {
                        bone.NextSibling = new Rsc6Ptr<Rsc6BoneData>(bones[sibling]);
                    }

                    if (child >= 0)
                    {
                        bone.FirstChild = new Rsc6Ptr<Rsc6BoneData>(bones[child]);
                    }
                }
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

    [TC(typeof(EXP))] public class Rsc6BoneData : Bone, IRsc6Block //rage::crBoneData
    {
        /*
         * The crBoneData holds data that is general to all instances of a particular matrix that belongs
         * to skeletons of the type of described by a particular crSkeletonData.
         * 
         * Several crBoneData are owned by a crSkeletonData, the connections between them determine the structure of the skeleton.
         * 
         * RDR1 Dofs :
         *     525310       root           (no min/max but pi x 2)            3 translations and 3 rotations (6DoF)
         *     524303                      (no min/max)                       0 translations and 3 rotations (3DoF)
         *     524414                      (min/max constraints)              0 translations and 3 rotations (3DoF)
         *     525198       head/neck      (no min/max)                       3 translations and 3 rotations (6DoF)
         */

        public ulong BlockLength => 224;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public Rsc6Str NameStr { get; set; } //m_Name, bone name
        public uint Dofs { get; set; } //m_Dofs, bone data degree-of-freedom flags
        public Rsc6Ptr<Rsc6BoneData> NextSibling { get; set; } //m_Next, pointer to the bone's next sibling, or NULL if no more siblings
        public Rsc6Ptr<Rsc6BoneData> FirstChild { get; set; } //m_Child, pointer to the bone's first child, or NULL if no children
        public Rsc6Ptr<Rsc6BoneData> ParentRef { get; set; } //m_Parent, pointer to the bone's parent, or NULL if no parent
        public Rsc6BoneIdEnum BoneId { get; set; } //m_BoneId, the bone id of this bone (or if bone ids not used, returns the bone's index)
        public ushort MirrorIndex { get; set; } //m_MirrorIndex, index of the bone that is a mirror to this bone (used to mirror animations/frames)
        public byte NumTransChannels { get; set; } //m_NumTransChannels, related to TranslationMin and TranslationMax
        public byte NumRotChannels { get; set; } //m_NumRotChannels, related to RotationMin and RotationMax
        public byte NumScaleChannels { get; set; } //m_NumScaleChannels, related to OrigScale
        public ushort Unknown_1Dh { get; set; } //Pad
        public byte Unknown_1Fh { get; set; } //Pad
        public Vector4 DefaultTranslation { get; set; } //m_DefaultTranslation, default translation vector of bone, this is the offset between this bone and it's parent
        public Vector4 DefaultRotation { get; set; } //m_DefaultRotation, default rotation on the bone
        public Quaternion DefaultRotationQuat { get; set; } //m_DefaultRotationQuat, default rotation on the bone as a quaternion
        public Vector4 DefaultScale { get; set; } //m_DefaultScale, default scale vector (not properly implemented in RAGE)
        public Vector4 GlobalOffset { get; set; } //m_GlobalOffset, depending on Dofs, Parent->DefaultTranslation or DefaultTranslation transformed to the model space
        public Vector4 JointOrient { get; set; } //m_JointOrient
        public Vector4 ScaleOrient { get; set; } //m_ScaleOrient
        public Vector4 TranslationMin { get; set; } //m_TransMin
        public Vector4 TranslationMax { get; set; } //m_TransMax
        public Vector4 RotationMin { get; set; } //m_RotMin
        public Vector4 RotationMax { get; set; } //m_RotMax
        public uint JointData { get; set; } //m_JointData, always 0
        public JenkHash NameHash { get; set; } //m_NameHash, bone name hashed
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
            NextSibling = new Rsc6Ptr<Rsc6BoneData>() { Position = reader.ReadUInt32() };
            FirstChild = new Rsc6Ptr<Rsc6BoneData>() { Position = reader.ReadUInt32() };
            ParentRef = new Rsc6Ptr<Rsc6BoneData>() { Position = reader.ReadUInt32() };
            Index = reader.ReadUInt16();
            BoneId = (Rsc6BoneIdEnum)reader.ReadUInt16();
            MirrorIndex = reader.ReadUInt16();
            NumTransChannels = reader.ReadByte();
            NumRotChannels = reader.ReadByte();
            NumScaleChannels = reader.ReadByte();
            Unknown_1Dh = reader.ReadUInt16();
            Unknown_1Fh = reader.ReadByte();
            DefaultTranslation = reader.ReadVector4();
            DefaultRotation = reader.ReadVector4();
            DefaultRotationQuat = reader.ReadVector4().ToQuaternion();
            DefaultScale = reader.ReadVector4();
            GlobalOffset = reader.ReadVector4();
            JointOrient = reader.ReadVector4();
            ScaleOrient = reader.ReadVector4();
            TranslationMin = reader.ReadVector4();
            TranslationMax = reader.ReadVector4();
            RotationMin = reader.ReadVector4();
            RotationMax = reader.ReadVector4();
            JointData = reader.ReadUInt32();
            NameHash = reader.ReadUInt32();
            Unknown_D8h = reader.ReadUInt32();
            Unknown_DCh = reader.ReadUInt32();

            Name = NameStr.Value;
            Position = DefaultTranslation.XYZ();
            Rotation = DefaultRotationQuat;
            Scale = Vector3.One;

            AnimRotation = Rotation;
            AnimTranslation = Position;
            AnimScale = Scale;
        }

        public void Write(Rsc6DataWriter writer)
        {
            Rsc6BoneData parent = null, child = null, sibling = null;
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
            writer.WriteUInt16((ushort)BoneId);
            writer.WriteUInt16(MirrorIndex);
            writer.WriteByte(NumTransChannels);
            writer.WriteByte(NumRotChannels);
            writer.WriteByte(NumScaleChannels);
            writer.WriteUInt16(Unknown_1Dh);
            writer.WriteByte(Unknown_1Fh);
            writer.WriteVector4(DefaultTranslation);
            writer.WriteVector4(DefaultRotation);
            writer.WriteVector4(DefaultRotationQuat.ToVector4());
            writer.WriteVector4(DefaultScale);
            writer.WriteVector4(GlobalOffset);
            writer.WriteVector4(JointOrient);
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

        public override void Read(MetaNodeReader reader) //6 5 7 32 30 4
        {
            NameStr = new(reader.ReadString("Name"));
            Index = reader.ReadInt32("Index");
            BoneId = Enum.TryParse(NameStr.ToString(), true, out Rsc6BoneIdEnum bId) ? bId : Rsc6BoneIdEnum.ROOT;
            MirrorIndex = reader.ReadUInt16("MirrorIndex");
            DefaultTranslation = Rpf6Crypto.ToXYZ(reader.ReadVector4("DefaultTranslation"));
            DefaultRotation = Rpf6Crypto.ToXYZ(reader.ReadVector4("DefaultRotation"));
            DefaultRotationQuat = Rpf6Crypto.ToXYZ(reader.ReadVector4("DefaultRotationQuat")).ToQuaternion();
            DefaultScale = Rpf6Crypto.ToXYZ(reader.ReadVector4("DefaultScale"));
            GlobalOffset = Rpf6Crypto.ToXYZ(reader.ReadVector4("GlobalOffset"));
            RotationMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("RotationMin"));
            RotationMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("RotationMax"));
            JointData = reader.ReadUInt32("JointData");
            SiblingIndex = reader.ReadInt32("SiblingIndex");
            ChildIndex = reader.ReadInt32("ChildIndex");
            ParentIndex = reader.ReadInt32("ParentIndex");

            Name = NameStr.ToString();
            NameHash = new JenkHash(Name);
            Position = DefaultTranslation.XYZ();
            Rotation = DefaultRotationQuat;

            CalculateDofsAndLimits();

            Scale = Vector3.One;
            AnimRotation = Rotation;
            AnimTranslation = Position;
        }

        public override void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", Name);
            writer.WriteInt32("Index", Index);
            writer.WriteUInt16("BoneId", (ushort)BoneId);
            writer.WriteUInt16("MirrorIndex", MirrorIndex);
            writer.WriteVector4("DefaultTranslation", DefaultTranslation);
            writer.WriteVector4("DefaultRotation", DefaultRotation);
            writer.WriteVector4("DefaultRotationQuat", DefaultRotationQuat.ToVector4());
            writer.WriteVector4("DefaultScale", new Vector4(Scale, 1.0f));
            writer.WriteVector4("GlobalOffset", GlobalOffset);
            writer.WriteVector4("RotationMin", RotationMin);
            writer.WriteVector4("RotationMax", RotationMax);
            writer.WriteUInt32("JointData", JointData);
            writer.WriteInt32("SiblingIndex", (NextSibling.Item == null) ? -1 : NextSibling.Item.Index);
            writer.WriteInt32("ChildIndex", (FirstChild.Item == null) ? -1 : FirstChild.Item.Index);
            writer.WriteInt32("ParentIndex", (Parent == null) ? -1 : Parent.Index);
        }

        public bool HasDofs(uint dofMask) //Check if any of the degrees of freedom in the mask exist on this bone
        {
            return (Dofs & dofMask) != 0;
        }

        public uint GetSignatureNonChiral() //Skeleton signature, insensitive to chirality, it cares only about the dofs and ids present in the skeleton (not their order)
        {
            return (uint)((ushort)BoneId << 32) |
                   (((Dofs & ((uint)Rsc6DoFs.TRANSLATE_X | (uint)Rsc6DoFs.TRANSLATE_Y | (uint)Rsc6DoFs.TRANSLATE_Z)) != 0 ? 0x1U : 0x0U) |
                   ((Dofs & ((uint)Rsc6DoFs.ROTATE_X | (uint)Rsc6DoFs.ROTATE_Y | (uint)Rsc6DoFs.ROTATE_Z)) != 0 ? 0x2U : 0x0U) |
                   ((Dofs & ((uint)Rsc6DoFs.SCALE_X | (uint)Rsc6DoFs.SCALE_Y | (uint)Rsc6DoFs.SCALE_Z)) != 0 ? 0x4U : 0x0U));
        }

        public void CalculateDofsAndLimits()
        {
            NumScaleChannels = 0;
            switch (Name)
            {
                case "root":
                    Dofs = 525310;
                    NumTransChannels = 3;
                    NumRotChannels = 3;
                    break;
                case "thumb_01_l":
                case "thumb_02_l":
                case "thumb_03_l":
                case "finger_11_l":
                case "finger_12_l":
                case "finger_13_l":
                case "finger_21_l":
                case "finger_22_l":
                case "finger_23_l":
                case "finger_31_l":
                case "finger_32_l":
                case "finger_33_l":
                case "finger_41_l":
                case "finger_42_l":
                case "finger_43_l":
                case "thumb_01_r":
                case "thumb_02_r":
                case "thumb_03_r":
                case "finger_11_r":
                case "finger_12_r":
                case "finger_13_r":
                case "finger_21_r":
                case "finger_22_r":
                case "finger_23_r":
                case "finger_31_r":
                case "finger_32_r":
                case "finger_33_r":
                case "finger_41_r":
                case "finger_42_r":
                case "finger_43_r":
                case "breast_top_l":
                case "breast_l":
                case "breast_top_r":
                case "breast_r":
                case "toe_l":
                case "toe_r":
                    Dofs = 524303;
                    NumTransChannels = 0;
                    NumRotChannels = 3;
                    break;
                case "upperEyeLid_l":
                case "lipCorner_l":
                case "lipCorner_r":
                case "head_Attachment":
                case "neck_Attachment":
                case "wrist_l_Attachment":
                case "elbow_l_Attachment":
                case "arm_l_Attachment":
                case "clavicle_l_Attachment":
                case "wrist_r_Attachment":
                case "elbow_r_Attachment":
                case "arm_r_Attachment":
                case "clavicle_r_Attachment":
                case "spine03_Attachment":
                case "spine02_Attachment":
                case "spine01_Attachment":
                case "spine00_Attachment":
                case "ankle_l_Attachment":
                case "knee_l_Attachment":
                case "hip_l_Attachment":
                case "ankle_r_Attachment":
                case "knee_r_Attachment":
                case "hip_r_Attachment":
                case "pelvis_Attachment":
                case "root_Attachment":
                    Dofs = 525198;
                    NumTransChannels = 2;
                    NumRotChannels = 3;
                    break;
                case "spine00":
                case "spine01":
                case "spine02":
                case "spine03":
                case "neck":
                case "head":
                case "clavicle_l":
                case "arm_l":
                case "armroll_l":
                case "elbow_l":
                case "wristroll_l":
                case "wrist_l":
                case "clavicle_r":
                case "arm_r":
                case "armroll_r":
                case "elbow_r":
                case "wristroll_r":
                case "wrist_r":
                case "pelvis":
                case "hip_l":
                case "hiproll_l":
                case "knee_l":
                case "ankle_l":
                case "ball_l":
                case "hip_r":
                case "hiproll_r":
                case "knee_r":
                case "ankle_r":
                case "ball_r":
                default:
                    Dofs = 524414;
                    NumTransChannels = 0;
                    NumRotChannels = 3;
                    break;
            }
        }
    }

    [TC(typeof(EXP))] public class Rsc6SkeletonBoneTag : Rsc6BlockBase, MetaNode //rage::crSkeletonData::BoneIdData
    {
        public override ulong BlockLength => 4;
        public Rsc6BoneIdEnum BoneTag { get; set; } //m_Id
        public ushort BoneIndex { get; set; } //m_Index

        public override void Read(Rsc6DataReader reader)
        {
            BoneTag = (Rsc6BoneIdEnum)reader.ReadUInt16();
            BoneIndex = reader.ReadUInt16();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt16((ushort)BoneTag);
            writer.WriteUInt16(BoneIndex);
        }

        public override string ToString()
        {
            return $"{BoneTag} : {BoneIndex}";
        }

        public void Read(MetaNodeReader reader)
        {
            BoneTag = reader.ReadEnum<Rsc6BoneIdEnum>("BoneTag");
            BoneIndex = reader.ReadUInt16("BoneIndex");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteEnum("BoneTag", BoneTag);
            writer.WriteUInt16("BoneIndex", BoneIndex);
        }
    }

    [TC(typeof(EXP))] public class Rsc6SkeletonBoneData : Rsc6BlockBase
    {
        public override ulong BlockLength => BonesCount * 224;
        public uint BonesCount { get; set; }
        public Rsc6BoneData[] Bones { get; set; }

        public Rsc6SkeletonBoneData()
        {
        }

        public Rsc6SkeletonBoneData(Rsc6BoneData[] bones)
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

    [TC(typeof(EXP))] public class Rsc6MatrixBuffer : IRsc6Block, MetaNode
    {
        public ulong BlockLength => 8;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;
        public Rsc6Arr<Matrix4x4> MtxBuffers { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            MtxBuffers = reader.ReadArr<Matrix4x4>();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(MtxBuffers);
        }

        public void Read(MetaNodeReader reader)
        {
            MtxBuffers = new(Rpf6Crypto.ToXYZ(reader.ReadMatrix4x4Array("MtxBuffers")));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteMatrix4x4Array("MtxBuffers", MtxBuffers.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6ShaderGroup : Rsc6BlockBase, MetaNode //rage::grmShaderGroup
    {
        public override ulong BlockLength => 32;
        public uint VFT { get; set; } = 0x00EEB23C;
        public Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary { get; set; } //m_TextureDictionary, always NULL
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
            bool alt = (writer.BlockList[0] is Rsc6FragDrawable) || (writer.BlockList[0] is Rsc6Fragment);
            writer.WriteUInt32(alt ? VFT : 0x0184A26C);
            writer.WriteUInt32(0); //Unused
            writer.WritePtrArr(Shaders);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
            writer.WriteUInt32(Unknown_1Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            TextureDictionary = new(reader.ReadNode<Rsc6TextureDictionary>("TextureDictionary"));
            Shaders = new(reader.ReadNodeArray<Rsc6ShaderFX>("Shaders"));

            foreach (var shader in Shaders.Items)
            {
                foreach (var param in shader.ParametersList.Item?.Parameters)
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
            }

            var texs = TextureDictionary.Item?.Textures.Items;
            if (texs != null)
            {
                var dictionary = new WtdFile(TextureDictionary.Item?.Textures.Items?.ToList());
                if ((Shaders.Items != null) && (TextureDictionary.Item != null))
                {
                    foreach (var shader in Shaders.Items)
                    {
                        var sparams = shader?.ParametersList.Item?.Parameters;
                        if (sparams != null)
                        {
                            foreach (var sparam in sparams)
                            {
                                if (sparam.Texture != null && dictionary != null)
                                {
                                    var tex2 = dictionary.Lookup(JenkHash.GenHash(sparam.Texture.NameRef.Value));
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
        }

        public void Write(MetaNodeWriter writer)
        {
            var dict = new List<Texture>();
            foreach (var tex in Shaders.Items)
            {
                foreach (var param in tex?.ParametersList.Item?.Parameters)
                {
                    if (param.DataType != 0
                        || param.Texture == null
                        || dict.FirstOrDefault(tex => tex.Name == param.Texture.Name) != null
                        || param.Texture?.Height == 0)
                        continue;
                    dict.Add(param.Texture);
                }
            }

            var texDict = new Rsc6TextureDictionary()
            {
                Textures = new(dict.Select(tex => (Rsc6Texture)tex).ToArray())
            };

            writer.WriteNode("TextureDictionary", texDict);
            writer.WriteNodeArray("Shaders", Shaders.Items.Where(item => item.Name.Str != "").ToArray());
        }
    }

    [TC(typeof(EXP))] public class Rsc6ShaderParameter : MetaNode
    {
        public byte RegisterCount { get; set; }
        public byte RegisterIndex { get; set; }
        public Rsc6ShaderParamType DataType { get; set; } //0: texture, 1: vector4
        public byte Unknown_3h { get; set; }
        public uint DataPointer { get; set; }
        public JenkHash Hash { get; set; }

        public Rsc6TextureBase Texture { get; set; }
        public Vector4 Vector { get; set; }
        public Vector4[] Array { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            DataType = (Rsc6ShaderParamType)reader.ReadByte();
            RegisterIndex = reader.ReadByte();
            RegisterCount = reader.ReadByte();
            Unknown_3h = reader.ReadByte();
            DataPointer = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer, Rsc6ShaderParametersBlock pramBlock, uint pos)
        {
            writer.WriteByte((byte)DataType);
            writer.WriteByte(RegisterIndex);
            writer.WriteByte(RegisterCount);
            writer.WriteByte(Unknown_3h);

            Rsc6TextureBase texture = null;
            if (Texture != null && Texture.ResourceType == Rsc6TextureBase.ResourceTextureType.EMBEDDED)
            {
                object block = writer.BlockList[0];
                if (block is Rsc6FragDrawable wfd)
                    texture = wfd.TextureDictionary.Item.Textures.Items.FirstOrDefault(e => e.Name == Texture.Name);
                else if (block is Rsc6Fragment wft)
                    texture = wft.Textures.Item.Textures.Items.FirstOrDefault(e => e.Name == Texture.Name);
                else
                    texture = ((Rsc6VisualDictionary)block).TextureDictionary.Item.Textures.Items.FirstOrDefault(e => e.Name == Texture.Name);
            }

            switch (DataType)
            {
                case Rsc6ShaderParamType.Texture:
                    if (texture == null)
                        writer.WritePtr(new Rsc6Ptr<Rsc6TextureBase>(Texture)); //External texture
                    else
                        writer.WritePtrEmbed(texture, texture, 0); //Embedded texture
                    break;
                case Rsc6ShaderParamType.CBuffer:
                    writer.WritePtrEmbed(pramBlock, pramBlock, pos);
                    break;
                default:
                    writer.WritePtrEmbed(pramBlock, pramBlock, pos);
                    break;
            }
        }

        public void Read(MetaNodeReader reader)
        {
            DataType = reader.ReadEnum("@type", Rsc6ShaderParamType.CBuffer);
            Hash = new JenkHash(reader.ReadString("@name"));

            switch (DataType)
            {
                case Rsc6ShaderParamType.Texture:
                    var tex = reader.ReadString("@texture");
                    if (tex == null) break;
                    if (tex.Contains('-')) tex = tex.Replace("-", ":");
                    Texture = new Rsc6TextureBase()
                    {
                        Name = tex + ".dds",
                        NameRef = new Rsc6Str(tex + ".dds")
                    };
                    break;
                case Rsc6ShaderParamType.CBuffer:
                    var length = reader.ReadUInt16("@length") / 16;
                    switch (length)
                    {
                        case 0:
                            break;
                        case 1:
                            Vector = new(reader.ReadSingle("@x"), reader.ReadSingle("@y"), reader.ReadSingle("@z"), reader.ReadSingle("@w"));
                            break;
                        default:
                            Array = reader.ReadVector4Array("Array");
                            break;
                    }
                    DataType = (Rsc6ShaderParamType)length;
                    break;
                default:
                    break;
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            string type = (DataType == Rsc6ShaderParamType.Texture) ? "Texture" : "CBuffer";
            writer.WriteString("@type", type);
            writer.WriteString("@name", Hash.ToString());

            switch (DataType)
            {
                case Rsc6ShaderParamType.Texture:
                    if (Texture != null)
                        writer.WriteString("@texture", Texture.Name.Replace(".dds", ""));
                    break;
                default:
                    writer.WriteUInt16("@length", (ushort)((int)DataType * 16));
                    switch ((int)DataType)
                    {
                        case 0:
                            break;
                        case 1:
                            writer.WriteSingle("@x", Vector.X);
                            writer.WriteSingle("@y", Vector.Y);
                            writer.WriteSingle("@z", Vector.Z);
                            writer.WriteSingle("@w", Vector.W);
                            break;
                        default:
                            writer.WriteVector4Array("Array", Array);
                            break;
                    }
                    break;
            }
        }

        public override string ToString()
        {
            var n = Hash.ToString() + ": ";
            if (DataType == 0) return n + Texture?.ToString();
            if (Array != null) return n + "Count: " + Array.Length.ToString();
            return n + Vector.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6ShaderParametersBlock : Rsc6BlockBase, MetaNode
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
                    offset += 16 * (byte)x.DataType;
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
                    size += (ushort)(16 * (byte)x.DataType);
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

        public Rsc6ShaderParametersBlock(Rsc6ShaderFX owner)
        {
            Owner = owner;
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
                    case Rsc6ShaderParamType.Texture:
                        var block = reader.ReadBlock<Rsc6TextureBase>(p.DataPointer);
                        if (block is Rsc6Texture tex)
                        { }
                        p.Texture = block;
                        ///////////p.Hash = new JenkHash(block?.Name);
                        //var tex = WfdFile.TextureDictionary.Item;
                        //if (tex == null)
                        //{
                        //    if (block != null && block.Name != null && block.Name.EndsWith(".dds"))
                        //    {
                        //        p.Texture = new Rsc6TextureBase() //Placeholder
                        //        {
                        //            MipLevels = 1,
                        //            Name = block.Name
                        //        };
                        //        p.Hash = JenkHash.GenHash(block.Name);
                        //    }
                        //    break;
                        //}
                        //Rsc6Str name = block.NameRef;
                        //if (name.Value == null) break;
                        //for (int t = 0; t < tex.Textures.Count; t++)
                        //{
                        //    if (!tex.Textures[t].NameRef.Value.Contains(name.Value.ToLower()))
                        //    {
                        //        if (t == tex.Textures.Count - 1)
                        //        {
                        //            p.Texture = new Rsc6TextureBase()
                        //            {
                        //                Format = TextureFormat.A8R8G8B8,
                        //                MipLevels = 1,
                        //                Name = name.Value
                        //            };
                        //            p.Hash = JenkHash.GenHash(name.Value);
                        //        }
                        //    }
                        //    else if (p.Texture == null)
                        //    {
                        //        p.Texture = tex.Textures[t];
                        //        p.Hash = JenkHash.GenHash(name.Value);
                        //        break;
                        //    }
                        //}
                        break;
                    case Rsc6ShaderParamType.CBuffer:
                        reader.Position = p.DataPointer;
                        p.Vector = reader.ReadVector4(false);
                        break;
                    default:
                        reader.Position = p.DataPointer;
                        p.Array = reader.ReadVector4Arr((int)p.DataType, false);
                        break;
                }
            }

            ushort size = (ushort)((paras?.Length ?? 0) * 8);
            foreach (var x in paras)
            {
                size += (ushort)(16 * (byte)x.DataType);
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
            var headerBlockSize = (uint)Parameters.Length * 8;
            if (headerBlockSize % 16 != 0)
            {
                headerBlockSize += (ushort)(16 - (ushort)(headerBlockSize % 16));
            }

            var parameters = Parameters;
            var positions = new uint[parameters.Length];
            var position = headerBlockSize;
            var size = 0u;

            for (int i = 0; i < positions.Length; i++)
            {
                var pram = parameters[i];
                if (pram.DataType == Rsc6ShaderParamType.Texture) continue;
                positions[i] = position + size;
                size += (uint)pram.DataType * 16;
            }

            //Write parameters
            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                var paramPos = positions[i];
                param.Write(writer, this, paramPos);
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
                if (param.DataType == (Rsc6ShaderParamType)1)
                    writer.WriteVector4(param.Vector);
                else if (param.DataType > (Rsc6ShaderParamType)1)
                {
                    foreach (var v in param.Array)
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

        public void Read(MetaNodeReader reader)
        {
            Parameters = reader.ReadNodeArray<Rsc6ShaderParameter>("Items");
            Hashes = Parameters.Select(param => param.Hash).ToArray();
            Count = Parameters.Length;

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
                    offset += (byte)param.DataType;
                }
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Items", Parameters);
        }
    }

    [TC(typeof(EXP))] public class Rsc6ShaderFX : Rsc6BlockBase, MetaNode //rage::grmShader + rage::grcInstanceData
    {
        public override ulong BlockLength => 32;
        public Rsc6Ptr<Rsc6ShaderParametersBlock> ParametersList { get; set; } //Data
        public JenkHash Name { get; set; } //BasisHashCode
        public byte ParameterCount { get; set; } //Count
        public byte RenderBucket { get; set; } //DrawBucket
        public byte PhysMtl { get; set; } //PhysMtl, always 0
        public byte Flags { get; set; } //Flags, always 0 (can be 0x01 or 0x80 in other games)
        public ushort ParameterSize { get; set; } //SpuSize
        public ushort ParameterDataSize { get; set; } //TotalSize
        public JenkHash FileName { get; set; } //MaterialHashCode, valid at resource time if we inherited from material, in RDR1 always 0
        public int RenderBucketMask { get; set; } //DrawBucketMask, always 0
        public ushort LastFrame { get; set; } //LastFrame, mostly 0 or sometimes 8 with a few WVD's
        public byte TextureDmaListSize { get; set; } //TextureDmaListSize, always 0
        public byte TextureParametersCount { get; set; } //TextureCount
        public uint SortKey { get; set; } //SortKey

        public override void Read(Rsc6DataReader reader)
        {
            ParametersList = reader.ReadPtrOnly<Rsc6ShaderParametersBlock>();
            Name = reader.ReadUInt32();
            ParameterCount = reader.ReadByte();
            RenderBucket = reader.ReadByte();
            PhysMtl = reader.ReadByte();
            Flags = reader.ReadByte();
            ParameterSize = reader.ReadUInt16();
            ParameterDataSize = reader.ReadUInt16();
            FileName = reader.ReadUInt32();
            RenderBucketMask = reader.ReadInt32();
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
            writer.WriteUInt32(0); //Unused
            writer.WriteInt32(RenderBucketMask);
            writer.WriteUInt16(LastFrame);
            writer.WriteByte(TextureDmaListSize);
            writer.WriteByte(TextureParametersCount);
            writer.WriteUInt32(SortKey);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = new JenkHash(reader.ReadString("Name"));
            RenderBucket = reader.ReadByte("DrawBucket");
            SortKey = reader.ReadUInt32("SortKey");
            ParametersList = new(reader.ReadNode("Parameters", (_) => new Rsc6ShaderParametersBlock(this)));
            ParameterSize = ParametersList.Item?.ParametersSize ?? 0;
            ParameterDataSize = ParametersList.Item?.ParametersDataSize ?? 0;
            TextureParametersCount = ParametersList.Item?.TextureParamsCount ?? 0;
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", Name.ToString());
            writer.WriteByte("DrawBucket", RenderBucket);
            writer.WriteUInt32("SortKey", SortKey);
            writer.WriteNode("Parameters", ParametersList.Item);
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

    [TC(typeof(EXP))] public class Rsc6VertexDeclaration : Rsc6BlockBase, MetaNode //rage::grcFvf
    {
        /*
         * FVF - Flexible Vertex Format
         * This class uses the concepts of channels and data size/type.
         * A channel represents actual data sent, such as positions or normals.
         * A data size/type represents how that data is stored in a vertex buffer
         */

        public override ulong BlockLength => 16;
        public uint FVF { get; set; } //m_Fvf, fvf channels currently used, (16601, 16473, 16857, etc)
        public byte FVFSize { get; set; } //m_FvfSize, total size of the fvf
        public byte Flags { get; set; } //m_Flags, various flags to use (i.e. transformed positions, etc)
        public byte DynamicOrder { get; set; } //m_DynamicOrder, if fvf is in dynamic order or standard order
        public byte ChannelCount { get; set; } //m_ChannelCount, number of 1's in 'Flags'
        public Rsc6VertexDeclarationTypes Types { get; set; } //m_FvfChannelSizes, 16 fields 4 bits each

        public VertexLayout VertexLayout { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            FVF = reader.ReadUInt32();
            FVFSize = reader.ReadByte();
            Flags = reader.ReadByte();
            DynamicOrder = reader.ReadByte();
            ChannelCount = reader.ReadByte();
            Types = (Rsc6VertexDeclarationTypes)reader.ReadUInt64();

            ulong t = (ulong)Types;
            ulong types = 0;
            ulong semantics = 0;
            int n = 0;

            for (int i = 0; i < 16; i++)
            {
                if (((FVF >> i) & 1) != 0)
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
            writer.WriteUInt32(FVF);
            writer.WriteByte(FVFSize);
            writer.WriteByte(Flags);
            writer.WriteByte(DynamicOrder);
            writer.WriteByte(ChannelCount);
            writer.WriteUInt64((ulong)Types);
        }

        public void Read(MetaNodeReader reader)
        {
            FVF = reader.ReadUInt32("FVF");
            FVFSize = reader.ReadByte("FVFSize");
            Flags = reader.ReadByte("Flags");
            DynamicOrder = reader.ReadByte("DynamicOrder");
            ChannelCount = reader.ReadByte("ChannelCount");
            Types = (Rsc6VertexDeclarationTypes)reader.ReadUInt64("Types");
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("FVF", FVF);
            writer.WriteByte("FVFSize", FVFSize);
            writer.WriteByte("Flags", Flags);
            writer.WriteByte("DynamicOrder", DynamicOrder);
            writer.WriteByte("ChannelCount", ChannelCount);
            writer.WriteUInt64("Types", (ulong)Types);
        }

        public bool IsPreTransform() //Query to see if the position channel contains pre-transformed data
        {
            return (Flags & 0x1) == 0x1;
        }

        public bool IsChannelActive(Rsc6VertexElementBits channel) //Determine if a channel is active/enabled
        {
            uint msk = 1U << (int)channel;
            return (FVF & msk) == msk;
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
                if (((FVF >> k) & 0x1) == 1)
                {
                    var componentType = GetComponentType(k);
                    offset += Rsc6VertexComponentTypes.GetSizeInBytes(componentType);
                }
            }
            return offset;
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
                case Rsc6VertexComponentType.UShort2N: return VertexElementFormat.Short2N;
                default: return VertexElementFormat.None;
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
            return FVFSize.ToString() + ": " + ChannelCount.ToString() + ": " + FVF.ToString() + ": " + Types.ToString();
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

        public static void UpdateFVF(Rsc6VertexElementSemantic channel, ref uint fvf)
        {
            uint channelMask = (uint)1 << (int)channel;
            bool channelSet = (fvf & channelMask) != 0;

            if (!channelSet)
            {
                fvf |= channelMask;
            }
        }

        public static Rsc6VertexElementSemantic GetChannelFromName(VertexElement element)
        {
            var name = element.SemanticName;
            var index = element.SemanticIndex;

            if (name == "COLOR" || name == "TEXCOORD" || name == "TANGENT" || name == "BINORMAL")
            {
                name += index.ToString();
            }

            return name switch
            {
                "BLENDWEIGHTS" => Rsc6VertexElementSemantic.BlendWeights,
                "BLENDINDICES" => Rsc6VertexElementSemantic.BlendIndices,
                "NORMAL" => Rsc6VertexElementSemantic.Normal,
                "COLOR0" => Rsc6VertexElementSemantic.Colour0,
                "COLOR1" => Rsc6VertexElementSemantic.Colour1,
                "TEXCOORD0" => Rsc6VertexElementSemantic.TexCoord0,
                "TEXCOORD1" => Rsc6VertexElementSemantic.TexCoord1,
                "TEXCOORD2" => Rsc6VertexElementSemantic.TexCoord2,
                "TEXCOORD3" => Rsc6VertexElementSemantic.TexCoord3,
                "TEXCOORD4" => Rsc6VertexElementSemantic.TexCoord4,
                "TEXCOORD5" => Rsc6VertexElementSemantic.TexCoord5,
                "TEXCOORD6" => Rsc6VertexElementSemantic.TexCoord6,
                "TEXCOORD7" => Rsc6VertexElementSemantic.TexCoord7,
                "TANGENT0" => Rsc6VertexElementSemantic.Tangent0,
                "TANGENT1" => Rsc6VertexElementSemantic.Tangent1,
                "BINORMAL0" => Rsc6VertexElementSemantic.Binormal0,
                "BINORMAL1" => Rsc6VertexElementSemantic.Binormal1,
                _ => Rsc6VertexElementSemantic.Position,
            };
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

    public enum Rsc6ShaderParamType : byte
    {
        Texture = 0,
        CBuffer = 1,
    }

    public enum Rsc6VertexBufferType : byte
    {
        DONT_TRIPPLE_BUFFER = 0,
        TRIPLE_VERTEX_BUFFER = 1 << 0,
        TRIPLE_INDEX_BUFFER = 1 << 1,
        USE_SECONDARY_BUFFER_INDICES = 1 << 2
    }

    public enum Rsc6VertexDeclarationTypes : ulong
    {
        DEFAULT = 0xAA1111111199A996,
        TERRAIN_TILE = 0xAAEEEEEEEE99A996,
        GRASS_BATCH = 0x0000000000080000,
        CLOTH = 0x0700000077097977
    }

    public enum Rsc6LodLevel : uint
    {
        HIGH = 0,
        MEDIUM = 1,
        LOW = 2,
        VLOW = 3,
        LOD_COUNT = 4
    }

    [Flags] public enum Rsc6VertexBufferFlags : byte
    {
        DYNAMIC = 1 << 0,
        PREALLOCATED_MEMORY = 1 << 1,
        READ_WRITE = 1 << 2
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
        ROTATION = ROTATE_X | ROTATE_Y | ROTATE_Z,
        TRANSLATION = TRANSLATE_X | TRANSLATE_Y | TRANSLATE_Z,
        SCALE = SCALE_X | SCALE_Y | SCALE_Z,
    };

    public enum Rsc6BoneIdEnum : ushort
    {
        ROOT = 0,
        SPINE00 = 13194,
        SPINE01 = 13195,
        SPINE02 = 13196,
        SPINE03 = 13197,
        NECK = 12832,
        HEAD = 53384,
        FACIAL_ROOT = 39051,
        FOREHEAD_C = 9695,
        EYEBROW_A_L = 8878,
        EYEBROW_A_R = 8852,
        JAW_C = 57105,
        TONGUE_A = 44579,
        TONGUE_B = 44580,
        LOWERLIP_L = 7601,
        LOWERLIP_R = 7703,
        CHIN_C = 26230,
        NOSTRIL_L = 2015,
        NOSTRIL_R = 1893,
        UPPERLIP_R = 27318,
        UPPERLIP_L = 27440,
        UPPERCHEEK_L = 28732,
        LOWERCHEEK_L = 25373,
        EAR_L = 53989,
        EYEBROW_B_L = 8622,
        EYEBROW_B_R = 8596,
        UPPERCHEEK_R = 28674,
        LOWERCHEEK_R = 25315,
        EAR_R = 53995,
        UPPEREYELID_R = 55953,
        EYEPIVOT_R = 3929,
        LOWEREYELID_R = 15424,
        EYEPIVOT_L = 3859,
        LOWEREYELID_L = 15450,
        UPPEREYELID_L = 55979,
        LIPCORNER_L = 29172,
        LIPCORNER_R = 29178,
        HEAD_ATTACHMENT = 33509,
        NECK_ATTACHMENT = 40551,
        CLAVICLE_L = 16240,
        ARM_L = 55698,
        ARMROLL_L = 59203,
        ELBOW_L = 55089,
        WRISTROLL_L = 682,
        WRIST_L = 36956,
        THUMB_01_L = 62597,
        THUMB_02_L = 62341,
        THUMB_03_L = 61061,
        FINGER_11_L = 36451,
        FINGER_12_L = 36707,
        FINGER_13_L = 37987,
        FINGER_21_L = 40547,
        FINGER_22_L = 40803,
        FINGER_23_L = 42083,
        FINGER_31_L = 28259,
        FINGER_32_L = 28515,
        FINGER_33_L = 29795,
        FINGER_41_L = 32355,
        FINGER_42_L = 32611,
        FINGER_43_L = 33891,
        WRIST_L_ATTACHMENT = 24642,
        ELBOW_L_ATTACHMENT = 62455,
        ARM_L_ATTACHMENT = 4722,
        CLAVICLE_L_ATTACHMENT = 38572,
        CLAVICLE_R = 16182,
        ARM_R = 55704,
        ARMROLL_R = 59177,
        ELBOW_R = 54967,
        WRISTROLL_R = 688,
        WRIST_R = 36866,
        THUMB_01_R = 62667,
        THUMB_02_R = 62411,
        THUMB_03_R = 61131,
        FINGER_11_R = 36553,
        FINGER_12_R = 36809,
        FINGER_13_R = 38089,
        FINGER_21_R = 40649,
        FINGER_22_R = 40905,
        FINGER_23_R = 42185,
        FINGER_31_R = 28361,
        FINGER_32_R = 28617,
        FINGER_33_R = 29897,
        FINGER_41_R = 32457,
        FINGER_42_R = 32713,
        FINGER_43_R = 33993,
        WRIST_R_ATTACHMENT = 29877,
        ELBOW_R_ATTACHMENT = 11772,
        ARM_R_ATTACHMENT = 63762,
        CLAVICLE_R_ATTACHMENT = 13618,
        BREAST_TOP_L = 32064,
        BREAST_L = 7260,
        BREAST_TOP_R = 32070,
        BREAST_R = 7234,
        SPINE03_ATTACHMENT = 35461,
        SPINE02_ATTACHMENT = 12068,
        SPINE01_ATTACHMENT = 47269,
        SPINE00_ATTACHMENT = 53173,
        PELVIS = 50708,
        HIP_L = 22185,
        HIPROLL_L = 18440,
        KNEE_L = 56997,
        ANKLE_L = 1301,
        BALL_L = 29528,
        TOE_L = 48373,
        ANKLE_L_ATTACHMENT = 47349,
        KNEE_L_ATTACHMENT = 5254,
        HIP_L_ATTACHMENT = 47554,
        HIP_R = 22191,
        HIPROLL_R = 18382,
        KNEE_R = 57003,
        ANKLE_R = 1179,
        BALL_R = 29534,
        TOE_R = 48379,
        ANKLE_R_ATTACHMENT = 14601,
        KNEE_R_ATTACHMENT = 17062,
        HIP_R_ATTACHMENT = 41427,
        PELVIS_ATTACHMENT = 42977,
        ROOT_ATTACHMENT = 57638,
        AXLE_R_00_JOINT = 63384,
        WHEEL_07_JOINT = 32475,
        WHEEL_06_JOINT = 42053,
        WHEEL_05_JOINT = 12756,
        WHEEL_04_JOINT = 48626,
        AXLE_F_00_JOINT = 47000,
        WHEEL_03_JOINT = 19329,
        WHEEL_02_JOINT = 28907,
        WHEEL_01_JOINT = 64777,
        WHEEL_00_JOINT = 35480,
        JMP_BCK_TUNA0 = 22256,
        JMP_BCK_TUNA1 = 22257,
        HITCHFRONT = 4815,
        HITCHFRONT_ATTACH = 657,
        HITCHBACK = 30483,
        HITCHBACK_ATTACH = 58489,
        CHASSISMAIN_JOINT = 6493,
        BODY_JOINT = 25813,
        LF_DOOR_JOINT = 46191,
        RT_DOOR_JOINT = 1189,
        AXLE_FR_JOINT = 53623,
        AXLE_RR_JOINT = 52151,
        ARM_FR_LF_JOINT = 37399,
        ARM_FR_RT_JOINT = 31834,
        ARM_RR_LF_JOINT = 15036,
        ARM_RR_RT_JOINT = 9471,
        HUB_FR_LF_JOINT = 19040,
        WHEEL_FR_LF_JOINT = 771,
        HUB_FR_RT_JOINT = 26173,
        WHEEL_FR_RT_JOINT = 57909,
        HUB_RR_LF_JOINT = 61844,
        WHEEL_RR_LF_JOINT = 43798,
        HUB_RR_RT_JOINT = 3810,
        WHEEL_RR_RT_JOINT = 35769,
        CRANK_JOINT = 31965,
        STEER_JOINT = 58567,
        BREAK_JOINT01 = 20949,
        BREAK_JOINT02 = 20950,
        CATTLEDOOR_JOINT01 = 46031,
        CATTLEDOOR_JOINT02 = 46032,
        BODY = 31437,
        LEVER = 8403,
        SLIDING = 15344,
        TRIGGER = 26541,
        HAMMER = 3727,
        LATCH = 56632,
        BREAKJOINT = 63591,
        BREAKJOINT3 = 36654,
        BREAKJOINT2 = 36653,
        BREAKJOINT1 = 36652,
        BREAKJOINT01 = 1342,
        BREAKJOINT02 = 1343,
        BREAKJOINT03 = 1344,
        BREAKJOINT04 = 1345,
        BREAKJOINT05 = 1346,
        BREAKJOINT06 = 1347,
        BREAKABLEFRAME02 = 25260,
        BREAKABLEFRAME07 = 25265,
        BREAKABLEFRAME03 = 25261,
        BREAKABLEFRAME01 = 25259,
        BREAKABLEFRAME04 = 25262,
        BREAKABLEFRAME05 = 25263,
        BREAKABLEFRAME06 = 25264,
        BREAKJOINT07 = 1348,
        BREAKJOINT08 = 1349,
        BREAKJOINT09 = 1350,
        BREAKJOINT10 = 1325,
        BREAKJOINT11 = 1326,
        BREAKJOINT12 = 1327,
        BREAKJOINT13 = 1328,
        BREAKJOINT14 = 1329,
        BREAKJOINT15 = 1330,
        BREAKJOINT16 = 1331,
        BREAKJOINT17 = 1332,
        BREAKJOINT18 = 1333,
        BREAKABLEFRAME08 = 25266,
        BREAKABLEFRAME27 = 25297,
        BREAKABLEFRAME10 = 25274,
        BREAKABLEFRAME13 = 25277,
        BREAKABLEFRAME14 = 25278,
        BREAKABLEFRAME11 = 25275,
        BREAKABLEFRAME15 = 25279,
        BREAKABLEFRAME12 = 25276,
        BREAKABLEFRAME16 = 25280,
        BREAKABLEFRAME09 = 25267,
        BREAKABLEFRAME28 = 25298,
        BREAKABLEFRAME17 = 25281,
        BREAKABLEFRAME24 = 25294,
        BREAKABLEFRAME19 = 25283,
        BREAKABLEFRAME21 = 25291,
        BREAKABLEFRAME22 = 25292,
        BREAKABLEFRAME23 = 25293,
        BREAKABLEFRAME26 = 25296,
        BREAKABLEFRAME25 = 25295,
        BREAKABLEFRAME20 = 25290,
        BOX_BOUND011 = 62888,
        BREAK_JOINT03 = 20951,
        BREAK_JOINT04 = 20952,
        BREAK_JOINT05 = 20953,
        BREAK_JOINT06 = 20954,
        BREAK_JOINT07 = 20955,
        BREAK_JOINT08 = 20956,
        JOINT01 = 20725,
        JOINT_BOTTOM = 38303,
        JOINT_TOP = 43086,
        JOINT1 = 4780,
        JOINT_MIN_A = 19759,
        JOINT_HR_A = 29229,
        JOINT_MIN_B = 19760,
        JOINT_HR_B = 29230,
        JOINT4 = 4783,
        JOINT2 = 4781,
        JOINT3 = 4782,
        JOINT_BLADE = 27806,
        LID_JOINT = 19138,
        CAPSULE_BOUND01 = 21356,
        HOR = 20146,
        VER01 = 10700,
        VER02 = 10701,
        HANDPIECE_PROPTRANS = 35269,
        HANDPIECEBRACKET = 47556,
        LATCH_JOINT = 17400,
        HANDLE_JOINT = 15796,
        BOUND = 45547,
        BOX_BOUND01 = 42362,
        JOINT01_LIGHT = 39153,
        JOINT02_LIGHT = 9856,
        JOINT03_LIGHT = 45726,
        JOINT04_LIGHT = 16429,
        JOINT05_LIGHT = 52299,
        JOINT06_LIGHT = 23002,
        JOINT07_LIGHT = 58872,
        JOINT08_LIGHT = 29575,
        JOINT2_BODY = 896,
        JOINT3_GLASS = 58096,
        JOINT02 = 20726,
        JOINT03 = 20727,
        JOINT_BODY = 60629,
        JOINT_LID_PROPTRANS = 4053,
        JOINTROOT1 = 2186,
        JOINTROOT2 = 2187,
        JOINTROOT3 = 2188,
        JOINTROOT4 = 2189,
        JOINTROOT5 = 2190,
        GLASSBREAK01 = 34175,
        GLASSBREAK02 = 34176,
        GLASSBREAK04 = 34178,
        GLASSBREAK06 = 34180,
        GLASSBREAK07 = 34181,
        GLASSBREAK08 = 34182,
        JOINT_CRANK = 34993,
        JOINT_RECIEVER_PROPTRANS = 41327,
        MESH01 = 57650,
        MESH02 = 57651,
        TOPJOINT = 31129,
        JOINT00 = 20724,
        CANDEL_01 = 5487,
        CANDEL_02 = 5488,
        CANDEL_03 = 5489,
        HANDLE_JOINT_PROPTRANS = 61075,
        POT_FRAGMENT = 11833,
        LID_FRAGMENT = 38032,
        HANDLE_FRAGMENT = 56695,
        DOOR_JOINT = 39483,
        BODY_FRAGMENT = 61385,
        RT_JOINT = 5410,
        MAP_JOINT_PROPTRANS = 32633,
        BOOK_JOINT01 = 54315,
        BOOK_JOINT02 = 54316,
        BOOK_JOINT03 = 54317,
        SWIVEL_JOINT = 8787,
        LID_PROPTRANS = 44124,
        GLOBEJOINT = 37449,
        CENTER_JOINT = 14367,
        RIGHT_JOINT = 445,
        RIGHT_JOINT_EXTREME = 17224,
        LEFT_JOINT = 3343,
        LEFT_JOINT_EXTREME = 63515,
        BREAKJOINT4 = 36655,
        BREAKJOINT5 = 36656,
        BREAKJOINT6 = 36657,
        BREAKJOINT7 = 36658,
        JOINT07_LIGHTMAIN = 27645,
        JOINT5 = 4784,
        JOINT6 = 4785,
        JOINT7 = 4786,
        ROTATION_JOINT = 23610,
        BOUNDS_SHOOTER = 54665,
        MAIN_JOINT = 20070,
        LEVER03_JOINT = 32439,
        LEVER02_JOINT = 3142,
        LEVER01_JOINT = 39012,
        WHEEL04_JOINT = 26082,
        WHEEL03_JOINT = 55379,
        WHEEL02_JOINT = 58384,
        WHEEL01_JOINT = 22514,
        ARM_JOINT = 12984,
        ARMWHEEL01_JOINT = 29950,
        ARMWHEEL02_JOINT = 653,
        CHAIN01_JOINT = 13093,
        CHAIN02_JOINT = 48963,
        CHAIN03_JOINT = 19778,
        CHAIN04_JOINT = 2952,
        DOOR_FRAGMENT = 35338,
        JOINT_COVER = 41994,
        JOINT_DOOR = 52734,
        JOINT_HANDLE = 17463,
        BREAK_JOINT09 = 20957,
        BREAK_JOINT10 = 20932,
        BREAK_JOINT11 = 20933,
        BREAK_JOINT12 = 20934,
        BREAK_JOINT13 = 20935,
        BREAK_JOINT14 = 20936,
        BREAK_JOINT15 = 20937,
        BREAK_JOINT16 = 20938,
        BREAK_JOINT17 = 20939,
        BREAK_JOINT18 = 20940,
        BREAK_JOINT19 = 20941,
        BREAK_JOINT20 = 20916,
        BREAK_JOINT21 = 20917,
        BREAK_JOINT22 = 20918,
        BREAK_JOINT23 = 20919,
        BREAK_JOINT24 = 20920,
        BREAK_JOINT25 = 20921,
        BREAK_JOINT26 = 20922,
        BREAK_JOINT27 = 20923,
        BREAK_JOINT28 = 20924,
        BREAK_JOINT29 = 20925,
        BREAK_JOINT30 = 20900,
        JOINT_FIRECIRCLE = 62813,
        JOINT_UPRIGHTS = 40666,
        JOINT_KETTLE = 45176,
        JOINT_ROOT = 58345,
        SIDEJOINT = 60367,
        NULL = 17073,
        WHEEL_JOINT = 29134,
        PEDAL_JOINT = 10691,
        BREAKJOINT00 = 1341,
        BREAKJOINT010 = 8097,
        BREAKJOINT011 = 8098,
        BREAKJOINT012 = 8099,
        BREAKJOINT013 = 8100,
        SHOCK_FR_LF_JOINT = 63703,
        SHOCK_FR_RT_JOINT = 4613,
        YOKE_JOINT = 11057,
        HITCH_JOINT = 51644,
        HANDLEBRAKE_JOINT = 31691,
        CRATE01_JOINT = 63155,
        TRUNK01_JOINT = 31017,
        STEP_JOINT = 49529,
        CHASSISSUPPORT_LF_JOINT = 2317,
        CHASSISSUPPORT_RT_JOINT = 23457,
        CHASSISBRAKE_JOINT = 62315,
        SHOCK_RR_LF_JOINT = 50419,
        SHOCK_RR_RT_JOINT = 56496,
        GOURD_JOINT03 = 26080,
        GOURD_JOINT02 = 26079,
        GOURD_JOINT01 = 26078,
        RL_DOOR_JOINT = 19124,
        RR_DOOR_JOINT = 12997,
        JMP_BCK_TUNA3 = 22259,
        JMP_BCK_TUNA2 = 22258,
        BREAKJOINT014 = 8101,
        BREAKJOINT015 = 8102,
        BREAKJOINT016 = 8103,
        BREAKJOINT017 = 8104,
        HINGE = 21282,
        CYLINDER = 35988,
        SPINE04 = 13198,
        SPINE05 = 13199,
        SPINE06 = 13200,
        SPINE07 = 13201,
        SPINE08 = 13202,
        SPINE09 = 13203,
        SPINE10 = 13114,
        SPINE11 = 13115,
        SPINE12 = 13116,
        JAW = 20439,
        TONGUE01 = 43811,
        TONGUE02 = 43812,
        TONGUE03 = 43813,
        PELVIS01 = 35322,
        PELVIS02 = 35323,
        PELVIS03 = 35324,
        PELVIS04 = 35325,
        PELVIS05 = 35326,
        PELVIS06 = 35327,
        PELVIS07 = 35328,
        PELVIS08 = 35329,
        PELVIS09 = 35330,
        PELVIS10 = 35209,
        PELVIS11 = 35210,
        PELVIS12 = 35211,
        PELVIS13 = 35212,
        PELVIS14 = 35213,
        PELVIS15 = 35214,
        WHEELSFRONT_L_JOINT = 20334,
        WHEELFRONT_L_02_JOINT = 12661,
        WHEELFRONT_L_00_JOINT = 58784,
        WHEELSFRONT_R_JOINT = 679,
        WHEELFRONT_R_03_JOINT = 40436,
        WHEELFRONT_R_01_JOINT = 7571,
        WHEELSREAR_L_JOINT = 62324,
        WHEELREAR_L_04_JOINT = 6304,
        WHEELREAR_L_06_JOINT = 12877,
        ARM00_L_JOINT = 6479,
        ARM00ROTATE_L_JOINT = 61284,
        ARM01PISTON_L_JOINT = 52683,
        WHEELREAR_L_08_JOINT = 58325,
        ARM02_L_JOINT = 4943,
        ARM02ROTATE_L_JOINT = 30052,
        WHEELSREAR_R_JOINT = 3826,
        WHEELREAR_R_05_JOINT = 55463,
        WHEELREAR_R_07_JOINT = 35744,
        ARM01_R_JOINT = 25814,
        ARM01ROTATE_R_JOINT = 22743,
        ARM01PISTON_R_JOINT = 20365,
        WHEELREAR_R_09_JOINT = 42317,
        ARM03_R_JOINT = 24278,
        ARM03ROTATE_R_JOINT = 26327,
        LEVER01 = 40268,
        LEVER02 = 40269,
        NECK01 = 63953,
        NECK02 = 63954,
        NECK03 = 63955,
        SHOULDER_L = 28488,
        FTH_01_01_L = 26346,
        FTH_08_01_L = 2507,
        FTH_10_01_L = 49739,
        FTH_11_01_L = 55643,
        SHOULDER_R = 28366,
        FTH_01_01_R = 26416,
        FTH_08_01_R = 2577,
        FTH_10_01_R = 49809,
        FTH_11_01_R = 55713,
        TOE_11_L = 41187,
        TOE_12_L = 40931,
        TOE_13_L = 43747,
        TOE_31_L = 49379,
        TOE_32_L = 49123,
        TOE_33_L = 51939,
        TOE_11_R = 41225,
        TOE_12_R = 40969,
        TOE_13_R = 43785,
        TOE_31_R = 49417,
        TOE_32_R = 49161,
        TOE_33_R = 51977,
        TAIL = 36481,
        TAIL_01 = 17563,
        TAIL_02 = 17564,
        RIFLE = 23004,
        PISTOL = 15266,
        THROWER = 7686,
        ROPE = 31978,
        MELEE = 6199,
        RIGHTMIDJOINT = 22107,
        RIGHTTOPJOINT = 42814,
        LEFTMIDJOINT = 58344,
        LEFTFRONTJOINT = 23242,
        RIGHTFRONTJOINT = 62951,
        JOINT_CHAIN = 11110,
        CAPSULE_BOUND041 = 25209,
        CAPSULE_BOUND02 = 21357,
        CAPSULE_BOUND03 = 21358,
        CAPSULE_BOUND04 = 21359,
        CAPSULE_BOUND05 = 21360,
        CAPSULE_BOUND06 = 21361,
        CAPSULE_BOUND07 = 21362,
        CAPSULE_BOUND08 = 21363,
        CAPSULE_BOUND09 = 21364,
        CAPSULE_BOUND010 = 25160,
        CAPSULE_BOUND011 = 25161,
        CAPSULE_BOUND012 = 25162,
        CAPSULE_BOUND013 = 25163,
        CAPSULE_BOUND014 = 25164,
        JOINT_CANDLE01 = 65032,
        JOINT_CANDLE02 = 65033,
        JOINT_01 = 19210,
        JOINT_02 = 19211,
        JOINT_03 = 19212,
        LEFT_POLE = 1514,
        RT_POLE = 26884,
        TOP_SLAT = 58414,
        MID_SLAT = 19740,
        BOT_SLAT = 16383,
        YAW = 24279,
        PITCH = 25709,
        BARRELS = 45875,
        CRANK = 53091,
        MAGDRUM = 36938,
        BREAK_JOINT31 = 20901,
        BREAK_JOINT32 = 20902,
        JOINT_02_PROPTRANS = 36595,
        JOINT_03_PROPTRANS = 34012,
        LATHE_JOINT = 60019,
        LATHE_JOINT2 = 34497,
        LATHE_JOINT3 = 34498,
        ROOT_JOINT = 8703,
        JOINT_PLANK01 = 53406,
        JOINT_PLANK02 = 53407,
        JOINT_PLANK03 = 53408,
        JOINT_PLANK04 = 53409,
        JOINT_PLANK05 = 53410,
        JOINT_PLANK06 = 53411,
        JOINT_PLANK07 = 53412,
        JOINT_PLANK08 = 53413,
        JOINT_PLANK09 = 53414,
        JOINTCENTER = 61220,
        JOINTRIGHT = 22091,
        JOINTLEFT = 58609,
        PAGEJOINT = 17540,
        PCYLINDER1 = 5114,
        GRAB = 52614,
        OFFSETA = 52465,
        GATE01_WHEEL = 2927,
        OFFSETB = 52466,
        GATE01_WHEEL1 = 3564,
        JOINT8 = 4787,
        JOINT9 = 4788,
        ARROWJOINT = 14422,
        FAN_JOINT = 41273,
        JOINT04 = 20728,
        TOMATO_JOINT01 = 55421,
        TOMATO_JOINT02 = 55422,
        TOMATO_JOINT03 = 55423,
        TOMATO_JOINT04 = 55424,
        TOMATO_JOINT05 = 55425,
        TOMATO_JOINT06 = 55426,
        TOMATO_JOINT07 = 55427,
        TOMATO_JOINT08 = 55428,
        TOMATO_JOINT09 = 55429,
        TOMATO_JOINT010 = 26190,
        TOMATO_JOINT011 = 26191,
        TOMATO_JOINT012 = 26192,
        TOMATO_JOINT013 = 26193,
        STATICJOINT = 52926,
        CANJOINT = 12908,
        WADJOINT = 18843,
        COREJOINT = 4477,
        BOTTLEBOTTOMJOINT = 13592,
        CAN01JOINT = 10996,
        BOTTLETOP01JOINT = 13781,
        BOTTLETOP02JOINT = 19685,
        CAN02JOINT = 16900,
        STONE_JOINT_PROPTRANS = 65489,
        SIDEB_1 = 63508,
        SIDEB_2 = 63509,
        LOWERCORNERA_R = 12615,
        LOWERCORNERB_R = 12359,
        LOWERCORNER_NULL01 = 44702,
        LOWERCORNERA_L = 12705,
        LOWERCORNERB_L = 12449,
        LOWERCORNER_NULL = 40093,
        SIDEA_1 = 63252,
        SIDEA_2 = 63253,
        UPPERCORNERA_R = 24344,
        UPPERCORNERN_R = 27160,
        UPPERCORNER_NULL01 = 9682,
        UPPERCORNERA_L = 24402,
        UPPERCORNERB_L = 24146,
        UPPERCORNER_NULL = 4318,
        LF_JOINT_PROPTRANS = 39461,
        MIDJOINT = 57622,
        BOTTOMJOINT = 50548,
        LEFTBOTTOMJOINT = 4524,
        RIGHTBOTTOMJOINT = 47621,
        LBOTJOINT = 3050,
        MBOTJOINT = 2794,
        RBOTJOINT = 2538,
        RMIDJOINT = 5255,
        MTOPJOINT = 45209,
        LMIDJOINT = 5767,
        LTOPJOINT = 44441,
        RTOPJOINT = 43929,
        JOINTCANDLE1 = 52679,
        JOINTCANDLE2 = 52680,
        JOINTCANDLE3 = 52681,
        JOINTCANDLE4 = 52682,
        GLASS_JOINT = 61348,
        CANDLEEBREAKJOINT = 39500,
        JOINT_LEG01 = 62255,
        JOINT_LEG02 = 62256,
        JOINT_LEG03 = 62257,
        BASKET_JOINT = 4498,
        HANDLE = 5618,
        _NULL1 = 46573,
        TAIL_R = 58351,
        TAUNG00 = 24038,
        TAUNG01 = 24039,
        TAUNG02 = 24040,
        TAUNG03 = 24041,
        JAW_ATTACHMENT = 52905,
        FINGER_L = 12364,
        FINGER_R = 12434,
        TAIL_00 = 17562,
        TAIL_03 = 17565,
        UPLIP1 = 15870,
        LOWLIP1 = 61390,
        NAIL_L = 22921,
        NAIL_R = 22927,
        THIGHT_L = 33338,
        THIGHT_R = 33344,
        TAIL_L_01 = 55945,
        TAIL_L_02 = 55946,
        BREAK_JOINT33 = 20903,
        BREAK_JOINT34 = 20904,
        BREAK_JOINT35 = 20905,
        BREAK_JOINT36 = 20906,
        BREAK_JOINT37 = 20907,
        BREAK_JOINT38 = 20908,
        BREAK_JOINT39 = 20909,
        BREAK_JOINT40 = 20884,
        BRANCH = 55972,
        SKULL = 35412,
        AMMO = 26963,
        ROOT_JOINT1 = 8043,
        ROOT_JOINT2 = 8044,
        ROOT_JOINT3 = 8045,
        ROOT_JOINT4 = 8046,
        LID = 21060,
        BASE_TRANS01 = 11528,
        BASE_TRANS02 = 11529,
        BASE_TRANS03 = 11530,
        BREACH = 3861,
        ROPE01 = 12637,
        ROPE02 = 12638,
        ROPE03 = 12639,
        ROPE04_PROPTRANS = 19593,
        FRONT_WHEEL = 16059,
        REAR_WHEEL = 44070,
        JOINTLBOT = 59617,
        JOINTLMID = 64849,
        JOINTLTOP = 63197,
        JOINTR = 4813,
        JOINTMBOT = 63713,
        JOINTMMID = 3778,
        JOINTMTOP = 2126,
        JOINTRBOT = 51425,
        JOINTRTOP = 55005,
        BREAKPOINT_BOT = 40214,
        BREAKPOINT_TOP = 44818,
        JOINT_LID = 12595,
        BREAK1 = 3966,
        BASE = 28073,
        RIGHTTOP = 35281,
        RIGHTBOTTOM = 16075,
        JOINT1DAMAGE = 6415,
        CANDLEJOINT01 = 45904,
        CANDLEJOINT02 = 45905,
        HEAD_JOINT = 52094,
        ARM_RT_JOINT = 55362,
        ARM_LF_JOINT = 36174,
        PADDLE_JOINT = 27329,
        JOINT_TOOL01 = 39267,
        JOINT_TOOL02 = 39268,
        JOINT_TOOL03 = 39269,
        JOINT_TOOL04 = 39270,
        JOINT_TOOL05 = 39271,
        HIP_L_JOINTROTTRANS = 43362,
        HIP_R_JOINTROTTRANS = 40802,
        BIGWHEEL_JOINT = 13183,
        BEAM_JOINT = 1540,
        SHAFT_JOINT = 5326,
        DUALWHEEL_JOINT = 36774,
        MOTORWHEEL_JOINT = 58174,
        FEATHER_L = 28242,
        FTH_01_02_L = 26602,
        FTH_01_03_L = 25834,
        FTH_02_01_L = 32250,
        FTH_02_02_L = 32506,
        FTH_02_03_L = 31738,
        FTH_03_01_L = 20219,
        FTH_03_02_L = 20475,
        FTH_03_03_L = 19707,
        FTH_04_01_L = 26123,
        FTH_04_02_L = 26379,
        FTH_04_03_L = 25611,
        FTH_05_01_L = 32027,
        FTH_05_02_L = 32283,
        FTH_05_03_L = 31515,
        FTH_06_01_L = 37931,
        FTH_06_02_L = 38187,
        FTH_06_03_L = 37419,
        FTH_07_01_L = 61770,
        FTH_07_02_L = 62026,
        FTH_07_03_L = 61258,
        FTH_09_01_L = 8411,
        FTH_12_01_L = 61547,
        FEATHER_R = 28280,
        FTH_01_02_R = 26672,
        FTH_01_03_R = 25904,
        FTH_02_01_R = 32320,
        FTH_02_02_R = 32576,
        FTH_02_03_R = 31808,
        FTH_03_01_R = 20289,
        FTH_03_02_R = 20545,
        FTH_03_03_R = 19777,
        FTH_04_02_R = 26449,
        FTH_04_03_R = 25681,
        FTH_05_01_R = 32097,
        FTH_05_02_R = 32353,
        FTH_05_03_R = 31585,
        FTH_06_01_R = 38001,
        FTH_06_02_R = 38257,
        FTH_06_03_R = 37489,
        FTH_07_01_R = 61840,
        FTH_07_02_R = 62096,
        FTH_07_03_R = 61328,
        FTH_09_01_R = 8481,
        FTH_12_01_R = 61617,
        TOE_21_L = 45283,
        TOE_22_L = 45027,
        TOE_23_L = 47843,
        TOE_21_R = 45321,
        TOE_22_R = 45065,
        TOE_23_R = 47881,
        TAIL_M = 58346,
        EYELID_R = 41158,
        EYELID_L = 41088,
        EYE_R = 18637,
        EYE_L = 18631,
        NOSE_L = 2088,
        BITEMUSCLE_L = 62170,
        BITEMUSCLE_R = 62240,
        UPLIP1_L = 35153,
        UPLIP1_R = 35223,
        LOWLIP1_L = 58011,
        LOWLIP1_R = 57889,
        HAIR01 = 53421,
        HAIR02 = 53422,
        HAIR03 = 53423,
        HAIR04 = 53424,
        HAIR05 = 53425,
        HAIR06 = 53426,
        CHEST_01_L = 54686,
        CHEST_02_L = 54942,
        CHEST_01_R = 54756,
        CHEST_02_R = 55012,
        TAIL_M_01 = 3066,
        TAIL_M_02 = 3067,
        TAIL_M_03 = 3068,
        TAIL_L_03 = 55947,
        TAIL_R_01 = 47753,
        TAIL_R_02 = 47754,
        TAIL_R_03 = 47755,
        BELLY_01_L = 13957,
        BELLY_02_L = 14213,
        BELLY_01_R = 13995,
        BELLY_02_R = 14251,
        PAN01 = 56245,
        PAN02 = 56246,
        TAIL_04 = 17566,
        TAIL_05 = 17567,
        TAUNG04 = 24042,
        FTH_01_L = 5499,
        FTH_02_L = 5755,
        FTH_01_R = 5601,
        FTH_02_R = 5857,
        HAIR01_02 = 954,
        HAIR02_01_L = 13695,
        HAIR02_02_L = 13951,
        HAIR02_04_L = 13439,
        HAIR02_01_R = 13605,
        HAIR02_02_R = 13861,
        HAIR02_04_R = 13349,
        HAIR03_01_L = 1664,
        HAIR03_02_L = 1920,
        HAIR03_03_L = 1152,
        HAIR03_04_L = 1408,
        HAIR03_01_R = 1574,
        HAIR03_02_R = 1830,
        HAIR03_03_R = 1062,
        HAIR04_01_L = 7568,
        HAIR04_02_L = 7824,
        HAIR04_03_L = 7056,
        HAIR04_04_L = 7312,
        HAIR04_01_R = 7478,
        HAIR04_02_R = 7734,
        HAIR04_03_R = 6966,
        HAIR04_04_R = 7222,
        HAIR05_01_L = 13472,
        HAIR05_02_L = 13728,
        HAIR05_03_L = 12960,
        HAIR05_04_L = 13216,
        HAIR05_01_R = 13382,
        HAIR05_02_R = 13638,
        HAIR05_03_R = 12870,
        HAIR05_04_R = 13126,
        HAIR06_01_L = 19376,
        HAIR06_02_L = 19632,
        HAIR06_03_L = 18864,
        HAIR06_04_L = 19120,
        HAIR06_01_R = 19286,
        HAIR06_02_R = 19542,
        HAIR06_03_R = 18774,
        HAIR06_04_R = 19030,
        STIRRUP01_R = 19641,
        STIRRUP02_R = 19897,
        STIRRUP01_L = 19667,
        STIRRUP02_L = 19923,
        STRING02_01_R = 11208,
        STRING02_02_R = 11464,
        STRING02_03_R = 12744,
        STRING02_01_L = 11234,
        STRING02_02_L = 11490,
        STRING02_03_L = 12770,
        STRING03_01_R = 64790,
        STRING03_02_R = 65046,
        STRING03_03_R = 1159,
        STRING03_01_L = 64816,
        STRING03_02_L = 65072,
        STRING03_03_L = 1185,
        NOSE_R = 2094,
        HAIR03_04_R = 1318,
        GENITLE_01 = 19226,
        TESTRT_01 = 20581,
        TESTRT_02_JOINTROTTRANS = 61049,
        TESTLT_01 = 18367,
        TESTLT_02_JOINTROTTRANS = 28731,
        BOTTOM_FRAGMENT = 23595,
        TOP_PART_ARTICULATION = 20102,
    }
}