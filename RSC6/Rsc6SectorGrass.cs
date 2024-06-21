using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RPF6;

namespace CodeX.Games.RDR1.RSC6
{
    public class Rsc6SectorGrass : Rsc6BlockBaseMap, MetaNode
    {
        public override ulong BlockLength => 28;
        public override uint VFT { get; set; } = 0x049FFD14;
        public Rsc6PtrArr<Rsc6GrassField> GrassItems { get; set; } //grassField
        public uint Unknown_10h { get; set; } //Always 0
        public uint Unknown_14h { get; set; } //Always 0
        public uint Unknown_18h { get; set; } = 65536; //Always 65536

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            GrassItems = reader.ReadPtrArr<Rsc6GrassField>();
            Unknown_10h = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            base.Write(writer);
            writer.WritePtrArr(GrassItems);
            writer.WriteUInt32(Unknown_10h);
            writer.WriteUInt32(Unknown_14h);
            writer.WriteUInt32(Unknown_18h);
        }

        public void Read(MetaNodeReader reader)
        {
            var items = reader.ReadNodeArray<Rsc6GrassField>("GrassItems");
            if (items != null)
            {
                GrassItems = new Rsc6PtrArr<Rsc6GrassField>(items);
            }
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("GrassItems", GrassItems.Items);
        }
    }

    public class Rsc6GrassField : Rsc6BlockBase, MetaNode //rage::grassField
    {
        public override ulong BlockLength => 112;
        public Vector4 Extents { get; set; } //m_Extents
        public Vector4 AABBMax { get; set; } //m_aabbMax
        public Vector4 AABBMin { get; set; } //m_aabbMin
        public Vector4 AABBScale { get; set; } //m_aabbScale
        public Vector4 AABBOffset { get; set; } //m_aabbOffset
        public Rsc6Ptr<Rsc6TexPlacementValues> TexPlacement { get; set; } //m_tp, texPlacementValues, always NULL
        public Rsc6Ptr<Rsc6VertexDeclaration> Layout { get; set; } //m_VertexDeclaration, always NULL
        public Rsc6Ptr<Rsc6VertexBuffer> VertexBuffer { get; set; } //m_VertexBuffer
        public bool Zup { get; set; } //m_Zup
        public bool UseSortedBuffers { get; set; } //m_useSortedBuffers
        public ushort Pad0 { get; set; } = 0xCDCD; //m_Pad0
        public Rsc6Str Name { get; set; } //m_Type
        public ushort NameLength1 { get; set; } //Name length
        public ushort NameLength2 { get; set; } //Name length + 1 (null terminator)
        public JenkHash NameHash { get; set; } //m_TypeHash
        public uint Unknown_6Ch { get; set; } = 0xCDCDCDCD; //m_Pad1

        public List<EntityBatchInstance3> Batchs { get; set; }
        public byte[] BatchData { get; set; }

        public const int MAX_PATCHES_PER_FIELD = 64000;
        public const float MINIMUM_PATCH_HEIGHT = 0.2f;
        public const float SPAWN_KIDS_HEIGHT = 0.1f;
        public const float SPAWN_KID_MULTIPLIER = 4.0f;

        public override void Read(Rsc6DataReader reader)
        {
            Extents = reader.ReadVector4();
            AABBMax = reader.ReadVector4();
            AABBMin = reader.ReadVector4();
            AABBScale = reader.ReadVector4();
            AABBOffset = reader.ReadVector4();
            TexPlacement = reader.ReadPtr<Rsc6TexPlacementValues>();
            Layout = reader.ReadPtr<Rsc6VertexDeclaration>();
            VertexBuffer = reader.ReadPtr<Rsc6VertexBuffer>();
            Zup = reader.ReadBoolean();
            UseSortedBuffers = reader.ReadBoolean();
            Pad0 = reader.ReadUInt16();
            Name = reader.ReadStr();
            NameLength1 = reader.ReadUInt16();
            NameLength2 = reader.ReadUInt16();
            NameHash = reader.ReadUInt32();
            Unknown_6Ch = reader.ReadUInt32();

            if (VertexBuffer.Item != null) //Shouldn't be the case
            {
                Batchs = new List<EntityBatchInstance3>();
                var data = VertexBuffer.Item?.VertexData.Items;
                var br = new BinaryReader(new MemoryStream(data));

                for (int i = 0; i < data.Length; i += 4)
                {
                    var d = FloatUtil.Dec3NToVector4(br.ReadUInt32());
                    d = new Vector4(d.Z, d.X, d.Y, d.Z);
                    var loc = (d * AABBScale) + AABBOffset;

                    /*var x = br.ReadByte();
                    var y = br.ReadByte();
                    var z = br.ReadByte();
                    var scale = br.ReadByte();
                    var pos = new Vector3(z, x, y);

                    var float16min = new Vector3(6.1035156e-5f, 6.1035156e-5f, 6.1035156e-5f);
                    var invScale = new Vector3
                    {
                        X = Math.Abs(AABBScale.X) < float.Epsilon ? 0 : 1.0f / AABBScale.X,
                        Y = Math.Abs(AABBScale.Y) < float.Epsilon ? 0 : 1.0f / AABBScale.Y,
                        Z = Math.Abs(AABBScale.Z) < float.Epsilon ? 0 : 1.0f / AABBScale.Z
                    };

                    pos = (pos - AABBOffset.XYZ()) * invScale;
                    pos = Vector3.Max(pos, float16min);
                    var center = new Vector4(pos.X, pos.Y, pos.Z, scale * 0.9f);*/

                    var batch = new EntityBatchInstance3
                    {
                        Position = loc
                    };
                    Batchs.Add(batch);
                }

                BatchData = new byte[Batchs.Count * 64];
                for (int i = 0; i < Batchs.Count; i++)
                {
                    var batch = Batchs[i];
                    var temp = new byte[64];
                    Buffer.BlockCopy(Rpf6Crypto.Vector4ToByteArray(batch.Position), 0, temp, 0, 16);
                    Buffer.BlockCopy(Rpf6Crypto.Vector4ToByteArray(batch.Extents), 0, temp, 16, 16);
                    Buffer.BlockCopy(Rpf6Crypto.Vector4ToByteArray(batch.Colour), 0, temp, 32, 16);
                    Buffer.BlockCopy(Rpf6Crypto.Vector4ToByteArray(batch.Params), 0, temp, 48, 16);
                    Buffer.BlockCopy(temp, 0, BatchData, i * 64, temp.Length);
                }
            }
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteVector4(Extents);
            writer.WriteVector4(AABBMax);
            writer.WriteVector4(AABBMin);
            writer.WriteVector4(AABBScale);
            writer.WriteVector4(AABBOffset);
            writer.WritePtr(TexPlacement);
            writer.WritePtr(Layout);
            writer.WritePtr(VertexBuffer);
            writer.WriteByte(Zup ? (byte)1 : (byte)0);
            writer.WriteByte(UseSortedBuffers ? (byte)1 : (byte)0);
            writer.WriteUInt16(Pad0);
            writer.WriteStr(Name);
            writer.WriteUInt16(NameLength1);
            writer.WriteUInt16(NameLength2);
            writer.WriteUInt32(NameHash);
            writer.WriteUInt32(Unknown_6Ch);
        }

        public void Read(MetaNodeReader reader)
        {
            Name = new(reader.ReadString("Name"));
            AABBMax = Rpf6Crypto.ToXYZ(reader.ReadVector4("AABBMax"));
            AABBMin = Rpf6Crypto.ToXYZ(reader.ReadVector4("AABBMin"));
            AABBScale = Rpf6Crypto.ToXYZ(reader.ReadVector4("AABBScale"));
            AABBOffset = Rpf6Crypto.ToXYZ(reader.ReadVector4("AABBOffset"));
            TexPlacement = new(reader.ReadNode<Rsc6TexPlacementValues>("TexPlacement"));
            Layout = new(reader.ReadNode<Rsc6VertexDeclaration>("Layout"));
            Zup = reader.ReadBool("Zup");
            UseSortedBuffers = reader.ReadBool("UseSortedBuffers");

            var vbuf = new Rsc6VertexBuffer();
            var mesh = new Mesh();
            mesh.Read(reader);

            var fvf = 0u;
            var elems = mesh.VertexLayout?.Elements;

            for (int i = 0; i < elems.Length; i++)
            {
                var channel = Rsc6VertexComponentTypes.GetChannelFromName(elems[i]);
                Rsc6VertexComponentTypes.UpdateFVF(channel, ref fvf);
            }

            var layout = new Rsc6VertexDeclaration()
            {
                FVF = fvf,
                FVFSize = (byte)mesh.VertexStride,
                ChannelCount = (byte)mesh.VertexLayout.ElementCount,
                Types = Rsc6VertexDeclarationTypes.GRASS_BATCH
            };

            vbuf.VertexCount = (ushort)mesh.VertexCount;
            vbuf.VertexStride = (uint)mesh.VertexStride;
            vbuf.VertexData = new(mesh.VertexData);
            vbuf.Layout = new(layout);

            VertexBuffer = new(vbuf);
            NameHash = new(Name.Value);
            NameLength1 = (ushort)Name.Value.Length;
            NameLength2 = (ushort)(NameLength1 + 1);
            Extents = Rpf6Crypto.GetVec4NaN();
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteString("Name", Name.Value);
            writer.WriteVector4("AABBMax", AABBMax);
            writer.WriteVector4("AABBMin", AABBMin);
            writer.WriteVector4("AABBScale", AABBScale);
            writer.WriteVector4("AABBOffset", AABBOffset);
            writer.WriteNode("TexPlacement", TexPlacement.Item);
            writer.WriteNode("Layout", Layout.Item);
            writer.WriteBool("Zup", Zup);
            writer.WriteBool("UseSortedBuffers", UseSortedBuffers);

            if (VertexBuffer.Item != null)
            {
                var mesh = new Mesh
                {
                    VertexData = VertexBuffer.Item.VertexData.Items,
                    VertexCount = VertexBuffer.Item.VertexCount,
                    VertexStride = (int)VertexBuffer.Item.VertexStride,
                    VertexLayout = VertexBuffer.Item.Layout.Item?.VertexLayout
                };
                mesh.Write(writer);
            }
        }

        public Vector4 GetFieldCenter()
        {
	        var center = AABBMin + AABBMax;
            return center * 0.5f;
        }

        public BoundingBox GetAABB()
        {
            return new BoundingBox(AABBMin.XYZ(), AABBMax.XYZ());
        }

        public Vector4 GetAABBSize()
        {
            return new Vector4(GetAABB().Size, 0.0f);
        }

        public void SetBounds(Vector4 min, Vector4 max)
        {
            AABBMin = min;
            AABBMax = max;
            AABBScale = new Vector4(AABBMax.XYZ() - AABBMin.XYZ(), 0.0f);
            AABBOffset = new Vector4(AABBMin.XYZ(), 16000.0f);
        }

        public override string ToString()
        {
            return Name.Value;
        }
    }

    public class Rsc6TexPlacementValues : Rsc6BlockBase, MetaNode
    {
        public override ulong BlockLength => 21;
        public Rsc6Arr<Vector3> Patches { get; set; } //m_Patches
        public Rsc6Arr<float> HeightScales { get; set; } //m_HeightScales
        public Rsc6Arr<uint> Colors { get; set; } //m_Color
        public byte Pad { get; set; } //pad

        public override void Read(Rsc6DataReader reader)
        {
            Patches = reader.ReadArr<Vector3>();
            HeightScales = reader.ReadArr<float>();
            Colors = reader.ReadArr<uint>();
            Pad = reader.ReadByte();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(Patches);
            writer.WriteArr(HeightScales);
            writer.WriteArr(Colors);
            writer.WriteByte(Pad);
        }

        public void Read(MetaNodeReader reader)
        {
            Patches = new(reader.ReadVector3Array("Patches"));
            HeightScales = new(reader.ReadSingleArray("HeightScales"));
            Colors = new(reader.ReadUInt32Array("Colors"));
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteVector3Array("Patches", Patches.Items);
            writer.WriteSingleArray("HeightScales", HeightScales.Items);
            writer.WriteUInt32Array("Colors", Colors.Items);
        }
    }

    public static class Rsc6GrassManager
    {
        public static List<Rsc6Texture> Textures;

        public static void Init(Rpf6FileManager fman)
        {
            Core.Engine.Console.Write("Rsc6GrassManager", "Initialising grass manager...");

            var textures = new List<Rsc6Texture>();
            var rpf = fman.AllArchives.FirstOrDefault(e => e.Name == "grassres.rpf");
            var entries = rpf.AllEntries.Where(e => e.Name.EndsWith(".wtd")).ToList();

            if (entries != null)
            {
                foreach (var entry in entries.Cast<Rpf6FileEntry>())
                {
                    var pack = fman.LoadTexturePack(entry);
                    foreach (var texture in pack.Textures)
                    {
                        textures.Add((Rsc6Texture)texture.Value);
                    }
                }
            }
            Textures = textures;
        }
    }
}