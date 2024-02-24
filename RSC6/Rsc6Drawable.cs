using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Shaders;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RPF6;
using ICSharpCode.SharpZipLib.Checksum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6VisualDictionary<T> : Rsc6FileBase, MetaNode where T : Rsc6Drawable, new()
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
            WfdFile.TextureDictionary = TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>(); //ReadAhead<rage::datOwner<rage::pgDictionary<rage::grcTexture>>>
            DerivedTextures = reader.ReadPtr<Rsc6TextureDictionary>(); //ReadAhead<rage::datOwner<rage::pgDictionary<rage::grcTexture>>>
            Unknown_30h = reader.ReadUInt32();
            LODLevel = reader.ReadUInt32();
            Unknown_38h = reader.ReadUInt32();
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
            var fileName = Drawables.Items[0]?.Name ?? "";
            var textures = new List<Rsc6Texture>();
            var hashes = new List<JenkHash>();

            foreach (var item in Drawables.Items)
            {
                foreach (var tex in item.ShaderGroup.Item.TextureDictionary.Item.Textures.Items)
                {
                    textures.Add(tex);
                    var name = tex.NameRef.Value.Replace(".dds", ""); //Hashes don't use the extension
                    hashes.Add(JenkHash.GenHash(name));
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

            if (fileName.Contains("_med"))
                writer.WriteUInt32(1);
            else if (fileName.Contains("_vlow"))
                writer.WriteUInt32(2);
            else
                writer.WriteUInt32(LODLevel);

            writer.WriteUInt32(Unknown_38h);
        }

        public void Read(MetaNodeReader reader)
        {
            Drawables = new(reader.ReadNodeArray<Rsc6Drawable>("Drawables"));
            Hashes = new(Drawables.Items.Select(d => new JenkHash(d.Name)).ToArray());
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            writer.WriteNodeArray("Drawables", Drawables.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6FragDrawable<T> : Rsc6FileBase, MetaNode where T : Rsc6Drawable, new()
    {
        public override ulong BlockLength => 16;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6Ptr<Rsc6TextureDictionary> TextureDictionary { get; set; } //ReadAhead<rage::datOwner<rage::pgDictionary<rage::grcTexture>>>
        public Rsc6Ptr<Rsc6Drawable> Drawable { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();

            //Loading textures before everything else so we can assign them properly
            reader.Position += 4;
            TextureDictionary = reader.ReadPtr<Rsc6TextureDictionary>();
            WfdFile.TextureDictionary = TextureDictionary;
            reader.Position -= 8;
            Drawable = reader.ReadPtr<Rsc6Drawable>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            var textures = new List<Rsc6Texture>();
            var hashes = new List<JenkHash>();

            foreach (var tex in Drawable.Item?.ShaderGroup.Item?.TextureDictionary.Item?.Textures.Items)
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

            writer.WriteUInt32(0x00F9C0A0);
            writer.WritePtr(BlockMap);
            writer.WritePtr(Drawable);
            writer.WritePtr(TextureDictionary);
        }

        public void Read(MetaNodeReader reader)
        {
            var drawable = new Rsc6Drawable();
            drawable.Read(reader);
            Drawable = new Rsc6Ptr<Rsc6Drawable>(drawable);
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteUInt32("@version", 0);
            Drawable.Item?.Write(writer);
        }
    }

    [TC(typeof(EXP))] public class Rsc6DrawableLod : PieceLod, Rsc6Block
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

    [TC(typeof(EXP))] public class Rsc6DrawableGeometry : Mesh, Rsc6Block //rage::grmGeometry + rage::grmGeometryQB
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
            /*if (AreTexcoordsNormalizable() && highLOD)
            {
                Rpf6Crypto.NormalizeUVs(uvX, uvY, uvOffset, ref numArray);
                Rpf6Crypto.NormalizeUVs(uvX1, uvY1, uvOffset1, ref numArray);
            }*/

            //Triangles or strips
            if (PrimitiveType == 3)
                Indices = IndexBuffer.Item?.Indices.Items;
            else
                Indices = ConvertStripToTriangles(IndexBuffer.Item?.Indices.Items).ToArray();

            VertexData = numArray;
        }

        public void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x00EF397C : VFT);
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
            writer.WriteRawArrPtr(BoneIds);
            writer.WriteUInt16((ushort)VertexStride);
            writer.WriteUInt16(BoneIdsCount);
            writer.WriteUInt32(0xCDCDCDCD); //VertexDataRef
            writer.WriteUInt32(OffsetBuffer);
            writer.WriteUInt32(IndexOffset);
            writer.WriteUInt32(Unknown_3Ch);
        }

        public override void Read(MetaNodeReader reader)
        {
            var min = Rpf6Crypto.ToYZX(reader.ReadVector3("BoundingBoxMin"));
            var max = Rpf6Crypto.ToYZX(reader.ReadVector3("BoundingBoxMax"));
            BoundingBox = new BoundingBox(min, max);
            AABB = new BoundingBox4(BoundingBox);
            ShaderID = reader.ReadUInt16("ShaderID");
            BoneIds = new(reader.ReadUInt16Array("BoneIDs"));
            BoneIdsCount = (ushort)(BoneIds.Items?.Length ?? 0);

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
                            Rpf6Crypto.WriteVector3AtIndex(newVert, VertexData, index + elemoffset, false);
                            break;
                        case VertexElementFormat.Dec3N:
                            var packed = BufferUtil.ReadUint(VertexData, index + elemoffset);
                            var packedVec = FloatUtil.Dec3NToVector3(packed);
                            packed = Rpf6Crypto.GetDec3N(new Vector3(packedVec.X, packedVec.Y, packedVec.Z), false);
                            BufferUtil.WriteUint(VertexData, index + elemoffset, packed);
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
                Types = Rsc6VertexDeclarationTypes.RDR1_1
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
            if (BoneIds.Items != null) writer.WriteUInt16Array("BoneIDs", BoneIds.Items);
            base.Write(writer);
        }

        public List<ushort> ConvertStripToTriangles(ushort[] stripIndices)
        {
            List<ushort> triangleIndices = new List<ushort>();
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

        public bool AreTexcoordsNormalizable()
        {
            return VertexBuffer.Item.Layout.Item.FVF == 49369;
        }

        public int GetBufferIndex()
        {
            return ((DoubleBuffered ? 1 : 0) & (int)Rsc6VertexBufferType.USE_SECONDARY_BUFFER_INDICES) >> 2;
        }

        public int GetDoubleBuffered()
        {
            return (DoubleBuffered ? 1 : 0) & ~(int)Rsc6VertexBufferType.USE_SECONDARY_BUFFER_INDICES;
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

                switch (bucket)
                {
                    default: throw new Exception("Unknown RenderBucket");
                    case 0: ShaderBucket = ShaderBucket.Solid; break;  //Opaque
                    case 1: ShaderBucket = ShaderBucket.Solid; break; //Double-sided opaque
                    case 2: ShaderBucket = ShaderBucket.Alpha1; break; //Hair
                    case 3: ShaderBucket = ShaderBucket.Alpha1; break; //AlphaMask
                    case 4: ShaderBucket = ShaderBucket.Translucency; break; //Water
                    case 5: ShaderBucket = ShaderBucket.Alpha1; break; //Transparent
                    case 6: ShaderBucket = ShaderBucket.Alpha2; break; //DistortionGlass
                    case 8: ShaderBucket = ShaderBucket.AlphaF; ShaderInputs.SetUInt32(0x0188ECE8, 1u); break; //Alpha
                }

                switch (hash)
                {
                    case 0x32A4918E: //rdr2_alpha
                    case 0x173D5F9D: //rdr2_grass
                    case 0x171C9E47: //rdr2_glass_glow
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
                        ShaderInputs.SetFloat4(0x5C3AB6E9, new Vector4(1, 0, 0, 0)); //"DecalMasks"
                        ShaderInputs.SetUInt32(0x0188ECE8, 1u);  //"DecalMode"
                        ShaderInputs.SetFloat(0x4D52C5FF, 1.0f); //AlphaScale
                        break;
                    case 0xB71272EA: //rdr2_flattenterrain
                        ShaderInputs.SetFloat(0xDF918855, 0.0f); //BumpScale
                        Textures[1] = null;
                        break;
                    case 0x4C03B90B: //rdr2_shadowonly
                        ShaderInputs.SetUInt32(0x0188ECE8, 1U);  //DecalMode - Hack to remove useless meshes
                        break;
                }
            }
        }

        private void SetupDefaultShader(Rsc6ShaderFX s) //diffuse + bump + ambocc
        {
            SetDefaultShader();
            ShaderInputs = Shader.CreateShaderInputs();
            ShaderInputs.SetUInt32(0xE0D5A584, 30); //NormalMapConfig
            ShaderInputs.SetUInt32(0x249983FD, 5); //ParamsMapConfig

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
                            case 0x608799C6: //specsampler
                                Textures[2] = tex;
                                break;
                            case 0x3E19076B: //detailmapsampler
                                break;
                        }
                    }
                }
                else if (parm.Vector != null)
                {
                    switch (parm.Hash)
                    {
                        case 0xF6712B81: //bumpiness
                            ShaderInputs.SetFloat(0xDF918855, parm.Vector.Vector.X); //BumpScale
                            break;
                        case 0xBBEED254: //fresnelterm         //~0.3-1, low for metals, ~0.96 for nonmetals
                            sfresnel = parm.Vector.Vector.X;
                            break;
                        case 0x484A5EBD: //specularcolorfactor   //0-1, final multiplier?
                            sintensitymult = parm.Vector.Vector.X;
                            break;
                        case 0x166E0FD1: //specularfactor    //10-150+?, higher is shinier
                            sfalloffmult = parm.Vector.Vector.X;
                            break;
                        case 0xE1322212: //mainuvmodulate
                            ShaderInputs.SetUInt32(0x01C01210, 1); //MeshUVMode
                            ShaderInputs.SetFloat4(0x9DBE8E24, parm.Vector.Vector); //MeshUVScaleOffset
                            break;
                    }
                }
            }
            ShaderInputs.SetFloat(0x57C22E45, FloatUtil.Saturate(sfalloffmult / 100.0f)); //"MeshParamsMult"
            ShaderInputs.SetFloat(0xDA9702A9, FloatUtil.Saturate(sintensitymult * (1.0f - ((sfresnel - 0.1f) / 0.896f)))); //"MeshMetallicity"
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
                        default:
                            break;
                    }
                }
                else
                {
                    switch (prm.Hash)
                    {
                        case 0xF6712B81: //bumpiness
                            ShaderInputs.SetFloat4(0x7CB163F5, prm.Vector.Vector); //BumpScales
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
                            ShaderInputs.SetFloat4(0x7CB163F5, prm.Vector.Vector); //BumpScales
                            break;
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
                            case 0xA3348DA6: //texturesampler3
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

    [TC(typeof(EXP))] public class Rsc6DrawableModel : Model, Rsc6Block //rage::grmModel
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
        public Rsc6RawArr<ushort> ShaderMapping { get; set; } //m_ShaderIndex, requires that for dynamic arrays, the pointer comes before its count in the structure
        public byte MatrixCount { get; set; } //m_MatrixCount, bone count
        public byte Flags { get; set; } //m_Flags
        public byte Type { get; set; } = 0xCD; //m_Type, always 0xCD?
        public byte MatrixIndex { get; set; } //m_MatrixIndex
        public byte Stride { get; set; } //m_Stride, always 0?
        public byte SkinFlag { get; set; } //m_SkinFlag, determine whether to render with the skinned draw path or not
        public ushort GeometriesCount { get; set; } //m_Count

        public BoundingBox BoundingBox { get; set; } //Created from first GeometryBounds item

        public bool IsModelRelative() //Returns true if model is skinned
        {
            return (Flags & (byte)Rsc6ModelFlags.MODEL_RELATIVE) != 0;
        }

        public bool IsResourced() //Returns true if model is resourced
        {
            return (Flags & (byte)Rsc6ModelFlags.MODEL_RELATIVE) != 0;
        }

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

            for (int i = 0; i < BoundsData.Items.Length; i++)
            {
                BoundsData.Items[i] = Rpf6Crypto.ToZXY(BoundsData.Items[i]);
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
                        geom.BoundingBox = new BoundingBox(geom.AABB.Min.XYZ(), geom.AABB.Max.XYZ());
                        geom.BoundingSphere = new BoundingSphere(geom.BoundingBox.Center, geom.BoundingBox.Size.Length() * 0.5f);
                    }
                }

                if ((boundsData != null) && (boundsData.Length > 0))
                {
                    ref var bb = ref boundsData[0];
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
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x00EF0B04 : (uint)VFT);
            writer.WritePtrArr(Geometries);
            writer.WriteRawArrPtr(BoundsData);
            writer.WriteRawArrPtr(ShaderMapping);
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
            Flags = reader.ReadByte("Flags");
            SkinFlag = reader.ReadByte("HasSkin");
            MatrixIndex = reader.ReadByte("BoneIndex");
            MatrixCount = reader.ReadByte("BoneCount");

            var min = Rpf6Crypto.ToYZX(reader.ReadVector3("BoundingBoxMin"));
            var max = Rpf6Crypto.ToYZX(reader.ReadVector3("BoundingBoxMax"));
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

            BoundsData = new(gbnds);
            ShaderMapping = new(smaps);
            GeometriesCount = (ushort)gcnt;
        }

        public override void Write(MetaNodeWriter writer)
        {
            if (Type != 0xCD) writer.WriteByte("Mask", Type);
            if (Flags != 0) writer.WriteByte("Flags", Flags);
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
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x00D34D6C : VFT);
            writer.WriteUInt16(VertexCount);
            writer.WriteByte(Locked);
            writer.WriteByte(Flags);
            writer.WriteRawArrPtr(LockedData); //Should be NULL
            writer.WriteUInt32(VertexStride);
            writer.WriteRawArrPtr(VertexData);
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
            return (Flags & ((byte)Rsc6VertexBufferFlags.ReadWrite | (byte)Rsc6VertexBufferFlags.Dynamic)) != 0;
        }

        public bool IsDynamic()
        {
            return (Flags & (byte)Rsc6VertexBufferFlags.Dynamic) != 0;
        }

        public bool IsPreallocatedMemory()
        {
            return (Flags & (byte)Rsc6VertexBufferFlags.PreallocatedMemory) != 0;
        }

        public override string ToString()
        {
            var cstr = "Count: " + VertexCount.ToString();
            if (Layout.Item == null) return "!NULL LAYOUT! - " + cstr;
            return "Type: " + Layout.Item.FVF.ToString() + ", " + cstr;
        }
    }

    [TC(typeof(EXP))] public class Rsc6IndexBuffer : Rsc6BlockBase
    {
        public override ulong BlockLength => 48;
        public uint VFT { get; set; } = 0x01858D60;
        public uint IndicesCount { get; set; }
        public uint Unknown_Ch { get; set; } //Always 0?
        public Rsc6RawArr<ushort> Indices { get; set; }
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
            Indices = reader.ReadRawArrItems(Indices, IndicesCount);
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
    }

    [TC(typeof(EXP))] public class Rsc6Drawable : Piece, Rsc6Block //rmcDrawable (grmShaderGroup + crSkeletonData + rmcLodGroup)
    {
        /*
         * An rmcDrawable contains up to four levels of detail; each level of detail
         * consists of zero or more models. Each model within the LOD can be bound to
         * a different bone, allowing complex objects to render with a single draw call.
         * It also contains a shader group, which is an array of all shaders used by all
         * models within the drawable.
         * 
         * The only reason it derives from Base is so that the vptr is at a known location for resources.
         */

        public ulong BlockLength => 120;
        public ulong FilePosition { get; set; }
        public bool IsPhysical => false;

        public ulong VFT { get; set; } = 0x01908F7C;
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6Ptr<Rsc6ShaderGroup> ShaderGroup { get; set; } //rage::grmShaderGroup
        public Rsc6Ptr<Rsc6SkeletonData> SkeletonRef { get; set; } //rage::crSkeletonData
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
        public uint PpuOnly { get; set; } = 0; //m_PpuOnly, 0 or 1 if PPU is set as default processor

        public virtual void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();
            ShaderGroup = reader.ReadPtr<Rsc6ShaderGroup>();
            SkeletonRef = reader.ReadPtr<Rsc6SkeletonData>();
            BoundingCenter = reader.ReadVector3();
            Unknown_1Ch = reader.ReadSingle();
            BoundingBoxMin = reader.ReadVector3();
            Unknown_2Ch = reader.ReadSingle();
            BoundingBoxMax = reader.ReadVector3();
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

            Name = Path.GetFileNameWithoutExtension(reader.FileEntry.Name);
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
            SetSkeleton(SkeletonRef.Item);
            AssignShaders();
            CreateTexturePack(reader.FileEntry);

            UpdateBounds();
            BoundingSphere = new BoundingSphere(BoundingBox.Center, BoundingSphereRadius);
        }

        public virtual void Write(Rsc6DataWriter writer)
        {
            bool wfd = writer.BlockList[0] is Rsc6FragDrawable<Rsc6Drawable>;
            writer.WriteUInt32(wfd ? 0x01023DF0 : (uint)VFT);
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

        public override void Read(MetaNodeReader reader)
        {
            Name = reader.ReadString("Name");
            BoundingCenter = Rpf6Crypto.ToYZX(reader.ReadVector3("BoundingSphereCenter"));
            BoundingSphereRadius = reader.ReadSingle("BoundingSphereRadius");
            BoundingBoxMin = Rpf6Crypto.ToYZX(reader.ReadVector3("BoundingBoxMin"));
            BoundingBoxMax = Rpf6Crypto.ToYZX(reader.ReadVector3("BoundingBoxMax"));
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
            if (BoundingCenter != default) writer.WriteVector3("BoundingSphereCenter", BoundingCenter);
            if (BoundingSphereRadius != default) writer.WriteSingle("BoundingSphereRadius", BoundingSphereRadius);
            if (BoundingBoxMin != default) writer.WriteVector3("BoundingBoxMin", BoundingBoxMin);
            if (BoundingBoxMax != default) writer.WriteVector3("BoundingBoxMax", BoundingBoxMax);
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

        public bool IsSkinned()
        {
            foreach (var model in AllModels.Cast<Rsc6DrawableModel>())
            {
                if (model.SkinFlag == 1) return true;
            }
            return false;
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
                                mesh.SetShader(shader);
                            }
                        }
                    }
                }
            }

        }

        public void SetSkeleton(Rsc6SkeletonData skel)
        {
            Skeleton = skel;
            if (AllModels != null)
            {
                var bones = skel?.Bones;
                foreach (var model in AllModels.Cast<Rsc6DrawableModel>())
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
            var txd = WfdFile.TextureDictionary.Item; //TODO: only include embedded textures
            if (txd == null) return;

            var texs = txd.Textures.Items;
            var txp = new TexturePack(e)
            {
                Textures = new Dictionary<string, Texture>()
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

    [TC(typeof(EXP))] public class Rsc6SkeletonData : Skeleton, Rsc6Block //rage::crSkeletonData
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
        public Rsc6RawArr<Matrix4x4> DefaultTransforms { get; set; } //m_DefaultTransforms, default transform matrices
        public Rsc6RawArr<Matrix4x4> CumulativeDefaultTransforms { get; set; } //m_CumulativeDefaultTransforms
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

                if (reader.BlockPool.TryGetValue(ns.Position, out var nsi))
                    ns.Item = nsi as Rsc6BoneData;
                if (reader.BlockPool.TryGetValue(fc.Position, out var fci))
                    fc.Item = fci as Rsc6BoneData;
                if (reader.BlockPool.TryGetValue(pr.Position, out var pri))
                    pr.Item = pri as Rsc6BoneData;

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

        public override void Read(MetaNodeReader reader)
        {
            Flags = reader.ReadUInt32("Flags");
            Signature = reader.ReadUInt32("Signature");
            JointData = reader.ReadUInt32("JointData");
            BoneData = new(reader.ReadNodeArray<Rsc6BoneData>("Bones"));

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
            BuildBoneTags();

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

            //Calculate signature (non-chiral)
            uint signatureNonChiral = 0;
            if (HasBoneIDs())
            {
                foreach (var bone in BoneIDs.Items)
                {
                    var idAndDofs = ((Rsc6BoneData)Bones[bone.BoneIndex]).GetSignatureNonChiral();
                    signatureNonChiral = UpdateCrc32(signatureNonChiral, BitConverter.GetBytes(idAndDofs));
                }
            }
            else
            {
                for (int i = 0; i < BoneCount; i++)
                {
                    var bone = (Rsc6BoneData)Bones[i];
                    var idAndDofs = bone.GetSignatureNonChiral();
                    signatureNonChiral = UpdateCrc32(signatureNonChiral, BitConverter.GetBytes(idAndDofs));
                }
            }

            //Calculate signature (comprehensive)
            uint signatureComprehensive = 0;
            for (int i = 0; i < BoneCount; i++)
            {
                var bone = (Rsc6BoneData)Bones[i];
                var idAndDofs = ((uint)bone.BoneId << 32) | bone.Dofs;
                signatureComprehensive = UpdateCrc32(signatureComprehensive, BitConverter.GetBytes(idAndDofs));
                signatureComprehensive = UpdateCrc32(signatureComprehensive, Rpf6Crypto.Vector4ToByteArray(bone.DefaultTranslation));
                signatureComprehensive = UpdateCrc32(signatureComprehensive, Rpf6Crypto.Vector4ToByteArray(bone.DefaultRotation));
                signatureComprehensive = UpdateCrc32(signatureComprehensive, Rpf6Crypto.Vector4ToByteArray(bone.DefaultScale));
            }
            Signature = signature;
        }

        private uint UpdateCrc32(uint crc, byte[] data)
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
                if (bone.BoneId != i)
                {
                    return true;
                }
            }
            return false;
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
                foreach (var bone in Bones.Cast<Rsc6BoneData>())
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

                        bone.GlobalOffset = new Vector4(cumulativeTransform.Translation, 0.0f);
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

    [TC(typeof(EXP))] public class Rsc6BoneData : Bone, Rsc6Block //rage::crBoneData
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
        public ushort BoneId { get; set; } //m_BoneId, the bone id of this bone (or if bone ids not used, returns the bone's index)
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
            BoneId = reader.ReadUInt16();
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

            if (Dofs == 525198)
            {
                Debug.WriteLine(Name);
            }
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
            writer.WriteUInt16(BoneId);
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

        public override void Read(MetaNodeReader reader)
        {
            NameStr = new(reader.ReadString("Name"));
            Index = reader.ReadInt32("Index");
            BoneId = reader.ReadUInt16("BoneId");
            MirrorIndex = reader.ReadUInt16("MirrorIndex");
            DefaultTranslation = Rpf6Crypto.ToYZX(reader.ReadVector4("DefaultTranslation"));
            DefaultRotation = Rpf6Crypto.ToYZX(reader.ReadVector4("DefaultRotation"));
            DefaultRotationQuat = Rpf6Crypto.ToYZX(reader.ReadVector4("DefaultRotationQuat")).ToQuaternion();
            DefaultScale = Rpf6Crypto.ToYZX(reader.ReadVector4("DefaultScale"));
            GlobalOffset = Rpf6Crypto.ToYZX(reader.ReadVector4("GlobalOffset"));
            RotationMin = Rpf6Crypto.ToYZX(reader.ReadVector4("RotationMin"));
            RotationMax = Rpf6Crypto.ToYZX(reader.ReadVector4("RotationMax"));
            JointData = reader.ReadUInt32("JointData");
            SiblingIndex = reader.ReadInt32("SiblingIndex");
            ChildIndex = reader.ReadInt32("ChildIndex");
            ParentIndex = reader.ReadInt32("ParentIndex");

            Name = NameStr.Value;
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
            writer.WriteUInt16("BoneId", BoneId);
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
            return (uint)(BoneId << 32) |
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

    [TC(typeof(EXP))] public class Rsc6SkeletonBoneTag : Rsc6BlockBase //rage::crSkeletonData::BoneIdData
    {
        public override ulong BlockLength => 4;
        public ushort BoneTag { get; set; } //m_Id
        public ushort BoneIndex { get; set; } //m_Index

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

    [TC(typeof(EXP))] public class Rsc6ShaderGroup : Rsc6BlockBase, MetaNode //rage::grmShaderGroup
    {
        public override ulong BlockLength => 32;
        public uint VFT { get; set; } = 0x0184A26C;
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

            //Tests
            if (Unknown_10h != 0 || Unknown_14h != 0 || Unknown_18h != 0 || Unknown_1Ch != 0)
            {
                throw new Exception("Rsc6ShaderGroup: Unknown property");
            }
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

            var dictionary = new WtdFile(TextureDictionary.Item.Textures.Items.ToList());
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
        public Rsc6Vector4 Vector { get; set; }
        public Rsc6Vector4 Array { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            DataType = (Rsc6ShaderParamType)reader.ReadByte();
            RegisterIndex = reader.ReadByte();
            RegisterCount = reader.ReadByte();
            Unknown_3h = reader.ReadByte();
            DataPointer = reader.ReadUInt32();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteByte((byte)DataType);
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
                case Rsc6ShaderParamType.Texture:
                    if (texture == null)
                        writer.WritePtr(new Rsc6Ptr<Rsc6TextureBase>(Texture)); //External texture
                    else
                        writer.WritePtrEmbed(texture, texture, 0); //Embedded texture
                    break;
                case Rsc6ShaderParamType.CBuffer:
                    writer.WritePtr(new Rsc6Ptr<Rsc6Vector4>(Vector));
                    break;
                default:
                    writer.WritePtr(new Rsc6Ptr<Rsc6Vector4>(Array));
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
                    var cbuffer = new Rsc6Vector4() { Count = length };
                    switch (length)
                    {
                        case 0:
                            break;
                        case 1:
                            cbuffer.Vector = new(reader.ReadSingle("@x"), reader.ReadSingle("@y"), reader.ReadSingle("@z"), reader.ReadSingle("@w"));
                            Vector = new(cbuffer.Vector);
                            break;
                        default:
                            cbuffer.Array = reader.ReadVector4Array("Array");
                            Array = new(cbuffer.Array);
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
                            writer.WriteSingle("@x", Vector.Vector.X);
                            writer.WriteSingle("@y", Vector.Vector.Y);
                            writer.WriteSingle("@z", Vector.Vector.Z);
                            writer.WriteSingle("@w", Vector.Vector.W);
                            break;
                        default:
                            writer.WriteVector4Array("Array", Array.Array);
                            break;
                    }
                    break;
            }
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
                    case Rsc6ShaderParamType.CBuffer:
                        p.Vector = reader.ReadBlock(p.DataPointer, (_) => new Rsc6Vector4((int)p.DataType));
                        break;
                    default:
                        p.Array = reader.ReadBlock(p.DataPointer, (_) => new Rsc6Vector4((int)p.DataType));
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
            //Update DataPointer for each param
            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                if (param.Texture == null) continue;

                if (param.DataType == 0)
                    param.DataPointer = (uint)param.Texture.FilePosition;
                else if (param.DataType == Rsc6ShaderParamType.CBuffer)
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
                if (param.DataType == (Rsc6ShaderParamType)1)
                    writer.WriteVector4(param.Vector.Vector);
                else if (param.DataType > (Rsc6ShaderParamType)1)
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
                Vector = reader.ReadVector4(false);
            }
            else
            {
                Array = reader.ReadVector4Arr(Count, false);
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

    [TC(typeof(EXP))] public class Rsc6ShaderFX : Rsc6BlockBase, MetaNode //rage::grmShader + rage::grcInstanceData
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
        public uint RenderBucketMask { get; set; } //DrawBucketMask, always 0
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
                case Rsc6VertexComponentType.UShort2N: return VertexElementFormat.UShort2N;
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

    public enum Rsc6VertexBufferFlags : byte
    {
        Dynamic = 1 << 0,
        PreallocatedMemory = 1 << 1,
        ReadWrite = 1 << 2
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
        //HAS_ROTATE_LIMITS = 8, //Is rotation limited?
        TRANSLATE_X = 16, //Can translate in x-axis
        TRANSLATE_Y = 32, //Can translate in y-axis
        TRANSLATE_Z = 64, //Can translate in z-axis
        //HAS_TRANSLATE_LIMITS = 128, //Is translation limited?
        SCALE_X = 256, //Can scale in x-axis
        SCALE_Y = 512, //Can scale in y-axis
        SCALE_Z = 1024, //Can scale in z-axis
        /*HAS_SCALE_LIMITS = 2048, //Is scale limited?
        HAS_CHILD = 4096, //Children?
        IS_SKINNED = 8192, //Bone is skinned to
        ROTATION = ROTATE_X | ROTATE_Y | ROTATE_Z,
        TRANSLATION = TRANSLATE_X | TRANSLATE_Y | TRANSLATE_Z,
        SCALE = SCALE_X | SCALE_Y | SCALE_Z,*/
    };
}