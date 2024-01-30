using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using System.Drawing;
using System;
using System.Numerics;
using System.Text;
using CodeX.Core.Engine;
using CodeX.Games.RDR1.RPF6;
using System.Collections.Generic;
using System.Linq;

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
                sb.AppendLine($"  - TexPlacement: {item.TexPlacement}");
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
        public Rsc6Ptr<Rsc6TexPlacementValues> TexPlacement { get; set; } //m_tp, texPlacementValues, always NULL
        public Rsc6Ptr<Rsc6VertexDeclaration> Layout { get; set; } //m_VertexDeclaration, always NULL
        public Rsc6Ptr<Rsc6VertexBuffer> VertexBuffer { get; set; } //m_VertexBuffer
        public bool Zup { get; set; } //m_Zup
        public bool UseSortedBuffers { get; set; } //m_useSortedBuffers
        public ushort Pad0 { get; set; } = 0xCDCD; //m_Pad0
        public Rsc6Str Name { get; set; } //m_Type
        public uint Unknown_64h { get; set; } = 0x0B000C00; //Always 0x0B000C00
        public JenkHash NameHash { get; set; } //m_TypeHash
        public uint Unknown_6Ch { get; set; } = 0xCDCDCDCD; //m_Pad1

        public const int MAX_PATCHES_PER_FIELD = 64000;
        public const float MINIMUM_PATCH_HEIGHT = 0.2f;
        public const float SPAWN_KIDS_HEIGHT = 0.1f;
        public const float SPAWN_KID_MULTIPLIER = 4.0f;

        public void Read(Rsc6DataReader reader)
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
            Unknown_64h = reader.ReadUInt32();
            NameHash = reader.ReadUInt32();
            Unknown_6Ch = reader.ReadUInt32();

            var min = AABBMin.XYZ();
            var max = AABBMax.XYZ();
            var siz = max - min;

            float divisionsf = 64.0f;
            Vector4 extents = Extents;
            float fWidthDiff = (extents.Y - extents.X) / divisionsf;
            float fDepthDiff = (extents.W - extents.Z) / divisionsf;
            uint patchesCreated = 0;

            var texture = Rsc6GrassManager.Textures.FirstOrDefault(t => t.Name.Contains(Name.Value));
            if (texture != null)
            {
                for (float xf = extents.X; xf < extents.Y; xf += fWidthDiff)
                {
                    for (float zf = extents.Z; zf < extents.W; zf += fDepthDiff)
                    {
                        var subFieldExtents = new Vector4(xf, xf + fWidthDiff, zf, zf + fDepthDiff);
                        patchesCreated += this.Create(texture, subFieldExtents, 64, Vector4.One, new Vector2(1.0f, 1.0f), 1);
                    }
                }
            }
        }

        public void Write(Rsc6DataWriter writer)
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
            writer.WriteUInt32(Unknown_64h);
            writer.WriteUInt32(NameHash);
            writer.WriteUInt32(Unknown_6Ch);
        }

        public Vector4 GetFieldCenter()
        {
	        var center = AABBMin + AABBMax;
            return center * 0.5f;
        }

        public void SetBounds(Vector4 min, Vector4 max)
        {
            AABBMin = min;
            AABBMax = max;
            AABBScale = new Vector4(AABBMax.XYZ() - AABBMin.XYZ(), 0.0f);
            AABBOffset = new Vector4(AABBMin.XYZ(), 16000.0f);
        }

        public uint Create(Texture fieldColorMap,
                   Vector4 maxExtents,
                   float placementResolution,
                   Vector4 subImage,
                   Vector2 cardSize,
                   int widthTextureVariantCount)
        {
            bool toldEm = false;
            var rnd = new Random();
            int patchIndex = 0;
            uint spawnedKids = 0;

            float fWidth = Extents.Y - Extents.X;
            float fDepth = Extents.W - Extents.Z;
            int nCellsW = (int)(fWidth / placementResolution);
            int nCellsD = (int)(fDepth / placementResolution);
            float maxWidth = maxExtents.Y - maxExtents.X;
            float maxHeight = maxExtents.W - maxExtents.Z;
            float stepX = (fWidth * 0.5f) / nCellsW;
            float stepZ = (fDepth * 0.5f) / nCellsD;

            var tempPositions = new Vector3[MAX_PATCHES_PER_FIELD];
            var tempHeights = new float[MAX_PATCHES_PER_FIELD];
            var tempColor = new Color[MAX_PATCHES_PER_FIELD];

            for (int w = 0; w < nCellsW; w++)
            {
                for (int d = 0; d < nCellsD; d++)
                {
                    bool placeIt = true;
                    uint spawnCount = 1;

                    float fX = Extents.X + (w * placementResolution);
                    float fY = Extents.Z + (d * placementResolution);
                    float u = (fX - maxExtents.X) / maxWidth;
                    float v = (fY - maxExtents.Z) / maxHeight;
                    u = Math.Clamp(u, 0.0f, 1.0f);
                    v = Math.Clamp(v, 0.0f, 1.0f);

                    u = u * (subImage.Y - subImage.X) + subImage.X;
                    v = v * (subImage.W - subImage.Z) + subImage.Z;

                    //Get the color for this texel
                    var color = Rsc6Texture.BilinearFilterRead(fieldColorMap, u, v);
                    var height = color.W;

                    if ((height * cardSize.Y) < MINIMUM_PATCH_HEIGHT)
                    {
                        placeIt = false;
                    }
                    else if ((height * cardSize.Y) < SPAWN_KIDS_HEIGHT)
                    {
                        float scalar = 1.0f / (SPAWN_KIDS_HEIGHT - MINIMUM_PATCH_HEIGHT);
                        float kids = 1.0f - (SPAWN_KIDS_HEIGHT - MINIMUM_PATCH_HEIGHT) - ((height * cardSize.Y) - MINIMUM_PATCH_HEIGHT) * scalar;
                        kids += 0.5f;
                        kids *= SPAWN_KID_MULTIPLIER * kids;

                        uint kidCount = (uint)Math.Floor(rnd.NextDouble() * (kids + 1));
                        spawnCount += kidCount;
                        spawnedKids += kidCount;
                    }

                    if (placeIt)
                    {
                        for (uint c = 0; c < spawnCount; c++)
                        {
                            if (patchIndex < MAX_PATCHES_PER_FIELD)
                            {
                                float fX2 = fX;
                                float fY2 = fY;

                                if (c == 0)
                                {
                                    tempHeights[patchIndex] = height * cardSize.Y;
                                    fX2 += (float)(rnd.NextDouble() * 2 * stepX - stepX);
                                    fY2 += (float)(rnd.NextDouble() * 2 * stepZ - stepZ);
                                }
                                else
                                {
                                    float childHeight = (float)(rnd.NextDouble() * (height * 0.1f) + height * 0.8f);
                                    childHeight = Math.Max(6.1035156e-5f, childHeight); //float16 min value
                                    tempHeights[patchIndex] = childHeight * cardSize.Y;
                                    fX2 += (float)(rnd.NextDouble() * (2 * stepX * 0.75) - stepX * 0.75);
                                    fY2 += (float)(rnd.NextDouble() * (2 * stepZ * 0.75) - stepZ * 0.75);
                                }

                                int texNo = patchIndex;
                                float wOffset = texNo % widthTextureVariantCount / (float)widthTextureVariantCount;

                                tempColor[patchIndex] = Color.FromArgb((int)wOffset, (int)color.X, (int)color.Y, (int)color.Z);

                                if (Zup)
                                    tempPositions[patchIndex] = new Vector3(fX2, fY2, 0.0f);
                                else
                                    tempPositions[patchIndex] = new Vector3(fX2, 0.0f, fY2);
                                patchIndex++;
                            }
                            else if (!toldEm)
                            {
                                toldEm = true;
                                throw new Exception("ERROR: There is too much grass on this field!\nTurn your GridResolution back up, or carve out more of your channel map.\n");

                            }
                        }
                    }
                }
            }

            if (patchIndex != 0)
            {
                TexPlacement = new
                (
                    new Rsc6TexPlacementValues()
                    {
                        Patches = new Rsc6Arr<Vector3>(new Vector3[patchIndex]),
                        HeightScales = new Rsc6Arr<float>(new float[patchIndex]),
                        Colors = new Rsc6Arr<uint>(new uint[patchIndex])
                    }
                );

                Array.Copy(tempPositions, TexPlacement.Item.Patches.Items, patchIndex);
                Array.Copy(tempHeights, TexPlacement.Item.HeightScales.Items, patchIndex);
                Array.Copy(tempColor, TexPlacement.Item.Colors.Items, patchIndex);
            }
            return (uint)patchIndex;
        }

        public override string ToString()
        {
            return Name.Value;
        }
    }

    public class Rsc6TexPlacementValues : Rsc6Block
    {
        public ulong FilePosition { get; set; }
        public ulong BlockLength => 21;
        public bool IsPhysical => false;

        public Rsc6Arr<Vector3> Patches { get; set; } //m_Patches
        public Rsc6Arr<float> HeightScales { get; set; } //m_HeightScales
        public Rsc6Arr<uint> Colors { get; set; } //m_Color
        public byte Pad { get; set; } //pad

        public Rsc6TexPlacementValues()
        {
        }

        public void Read(Rsc6DataReader reader)
        {
            Patches = reader.ReadArr<Vector3>();
            HeightScales = reader.ReadArr<float>();
            Colors = reader.ReadArr<uint>();
            Pad = reader.ReadByte();
        }

        public void Write(Rsc6DataWriter writer)
        {
            writer.WriteArr(Patches);
            writer.WriteArr(HeightScales);
            writer.WriteArr(Colors);
            writer.WriteByte(Pad);
        }
    }

    public static class Rsc6GrassManager
    {
        public static List<Rsc6Texture> Textures;

        public static void Init(Rpf6FileManager fman)
        {
            Core.Engine.Console.Write("Rsc6GrassManager", "Initialising Grass Manager...");

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