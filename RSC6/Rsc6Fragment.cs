using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CodeX.Games.RDR1.RSC6
{
    class Rsc6Fragment : Rsc6Block
    {
        public ulong FilePosition { get; set; }
        public ulong BlockLength => 204;
        public bool IsPhysical => false;
        public uint VFT { get; set; }
        public Rsc6Ptr<Rsc6BlockMap> BlockMap { get; set; }
        public Rsc6Ptr<Rsc6TextureDictionary> Textures { get; set; }
        public float MaxInertiaNormalized { get; set; }
        public float MaxInertia { get; set; }
        public Vector4 CenterRadius { get; set; }
        public Vector4 CenterOfMass { get; set; }
        public Vector4 CenterOfMass2 { get; set; }
        public Vector4 UnbrokenCGOffset { get; set; }
        public Vector4 DampingLinearC { get; set; }
        public Vector4 DampingLinearV { get; set; }
        public Vector4 DampingLinearV2 { get; set; }
        public Vector4 DampingAngularC { get; set; }
        public Vector4 DampingAngularV { get; set; }
        public Vector4 DampingAngularV2 { get; set; }
        public Rsc6Str NameRef { get; set; }
        public Rsc6Ptr<Rsc6FragDrawable> Drawable { get; set; }
        public uint Unk0 { get; set; }
        public uint Unk1 { get; set; }
        public int UnkInt { get; set; }
        public int UnkInt2 { get; set; }
        public uint ChildPtr { get; set; } //pointer to a child?
        public uint GroupNamesPtr { get; set; } //string array
        public uint GroupsPtr { get; set; } //fragTypeGroup array
        public uint ChildrenPtr { get; set; } //fragTypeChild array
        public uint UnkArray { get; set; }
        public ushort UnkArrayCount { get; set; }
        public ushort UnkArraySize { get; set; }
        public uint UnkPtr { get; set; }
        public Rsc6Ptr<Rsc6FragPhysArchetype> Archetype { get; set; }
        public Rsc6Ptr<Rsc6FragPhysArchetype> Archetype2 { get; set; }
        public Rsc6Ptr<Rsc6Bounds> Bound { get; set; }
        public uint ChildInertiaPtr { get; set; } //vector3 array
        public uint ChildInertiaDamagedPtr { get; set; } //vector3 array
        public uint MatricesPtr { get; set; } //matrix3x4 array
        public uint SelfCollisionIndices1Ptr { get; set; } //byte array
        public uint SelfCollisionIndices2Ptr { get; set; } //byte array
        public uint ModelIndex { get; set; } //?
        public uint CollisionEventsPtr { get; set; } //evtSet array
        public uint EstimatedCacheSize { get; set; }
        public uint EstimatedArticulatedCacheSize { get; set; }
        public byte SelfCollisionCount { get; set; }
        public byte SelfCollisionCountAllocated { get; set; }
        public byte GroupCount { get; set; }
        public byte ChildCount { get; set; }
        public byte FragTypeGroupCount { get; set; }
        public byte DamageRegions { get; set; }
        public byte ChildCount2 { get; set; }
        public byte Flags { get; set; }
        public byte EntityClass { get; set; }
        public byte BecomeRope { get; set; }
        public byte ArtAssetId { get; set; }
        public byte AttachBottomEnd { get; set; }
        public int UnkInt3 { get; set; }
        public float MinMoveForce { get; set; }

        public string[] GroupNames { get; set; }
        public Rsc6FragTypeGroup[] Groups { get; set; }
        public Rsc6FragTypeChild[] Children { get; set; }
        public Vector4[] ChildInertia { get; set; }
        public Vector4[] ChildInertiaDamaged { get; set; }
        public Matrix3x4[] Matrices { get; set; }
        public byte[] SelfCollisionIndices1 { get; set; }
        public byte[] SelfCollisionIndices2 { get; set; }

        public void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            BlockMap = reader.ReadPtr<Rsc6BlockMap>();

            //Loading textures before the rest or they won't be assigned
            ulong pos = reader.Position;
            reader.Position += 0x1E8;
            Textures = reader.ReadPtr<Rsc6TextureDictionary>();
            Files.WfdFile.TextureDictionary = Textures;
            reader.Position = pos;

            MaxInertiaNormalized = reader.ReadSingle();
            MaxInertia = reader.ReadSingle();
            CenterRadius = reader.ReadVector4(true);
            CenterOfMass = reader.ReadVector4(true);
            CenterOfMass2 = reader.ReadVector4(true);
            UnbrokenCGOffset = reader.ReadVector4(true);
            DampingLinearC = reader.ReadVector4(true);
            DampingLinearV = reader.ReadVector4(true);
            DampingLinearV2 = reader.ReadVector4(true);
            DampingAngularC = reader.ReadVector4(true);
            DampingAngularV = reader.ReadVector4(true);
            DampingAngularV2 = reader.ReadVector4(true);
            NameRef = reader.ReadStr();
            Drawable = reader.ReadPtr<Rsc6FragDrawable>();
            Unk0 = reader.ReadUInt32();
            Unk1 = reader.ReadUInt32();
            UnkInt = reader.ReadInt32();
            UnkInt2 = reader.ReadInt32();
            ChildPtr = reader.ReadUInt32(); //pointer to a child?
            GroupNamesPtr = reader.ReadUInt32(); //string array
            GroupsPtr = reader.ReadUInt32(); //fragTypeGroup array
            ChildrenPtr = reader.ReadUInt32(); //fragTypeChild array
            UnkArray = reader.ReadUInt32();
            UnkArrayCount = reader.ReadUInt16();
            UnkArraySize = reader.ReadUInt16();
            UnkPtr = reader.ReadUInt32();
            Archetype = reader.ReadPtr<Rsc6FragPhysArchetype>();
            Archetype2 = reader.ReadPtr<Rsc6FragPhysArchetype>();
            Bound = reader.ReadPtr(Rsc6Bounds.Create);
            ChildInertiaPtr = reader.ReadUInt32(); //vector3 array
            ChildInertiaDamagedPtr = reader.ReadUInt32(); //vector3 array
            MatricesPtr = reader.ReadUInt32(); //matrix3x4 array
            SelfCollisionIndices1Ptr = reader.ReadUInt32(); //byte array
            SelfCollisionIndices2Ptr = reader.ReadUInt32(); //byte array
            ModelIndex = reader.ReadUInt32(); //?
            CollisionEventsPtr = reader.ReadUInt32();//evtSet array
            var _fC0 = reader.ReadUInt32();
            var _fC4 = reader.ReadUInt32();
            var _fC8 = reader.ReadUInt32(); //4 separate bytes?      
            var frame = reader.ReadUInt32(); //crFrame ptr
            var f1DC = reader.ReadUInt32(); //obj2 ptr
            var f1E0 = reader.ReadUInt32(); //obj2 ptr
            var f1E4 = reader.ReadUInt32(); //unk ptr
            EstimatedCacheSize = reader.ReadUInt32();
            EstimatedArticulatedCacheSize = reader.ReadUInt32();
            SelfCollisionCount = reader.ReadByte();
            SelfCollisionCountAllocated = reader.ReadByte();
            GroupCount = reader.ReadByte();
            ChildCount = reader.ReadByte();
            FragTypeGroupCount = reader.ReadByte();
            DamageRegions = reader.ReadByte();
            ChildCount2 = reader.ReadByte();
            Flags = reader.ReadByte();
            EntityClass = reader.ReadByte();
            BecomeRope = reader.ReadByte();
            ArtAssetId = reader.ReadByte();
            AttachBottomEnd = reader.ReadByte();
            UnkInt3 = reader.ReadInt32();
            MinMoveForce = reader.ReadSingle();
            var m_UnbrokenElasticity = reader.ReadSingle();
            var m_BuoyancyFactor = reader.ReadSingle();
            var m_GlassAttachmentBone = reader.ReadByte();
            var m_NumGlassPaneModelInfos = reader.ReadByte();

            if (GroupNamesPtr != 0)
            {
                var groupNamesPtrs = reader.ReadArray<uint>(GroupCount, GroupNamesPtr);
                GroupNames = new string[GroupCount];
                for (int i = 0; i < GroupCount; i++)
                {
                    if (groupNamesPtrs[i] != 0)
                    {
                        reader.Position = groupNamesPtrs[i];
                        GroupNames[i] = reader.ReadString();
                    }
                }
            }

            if (GroupsPtr != 0)
            {
                var groupsPtrs = reader.ReadArray<uint>(GroupCount, GroupsPtr);
                Groups = new Rsc6FragTypeGroup[GroupCount];
                for (int i = 0; i < GroupCount; i++)
                {
                    if (groupsPtrs[i] != 0)
                    {
                        reader.Position = groupsPtrs[i];
                        Groups[i] = reader.ReadBlock<Rsc6FragTypeGroup>();
                    }
                }
            }

            if (ChildrenPtr != 0)
            {
                var childrenPtrs = reader.ReadArray<uint>(ChildCount, ChildrenPtr);
                Children = new Rsc6FragTypeChild[ChildCount];
                for (int i = 0; i < ChildCount; i++)
                {
                    if (childrenPtrs[i] != 0)
                    {
                        reader.Position = childrenPtrs[i];
                        Children[i] = reader.ReadBlock<Rsc6FragTypeChild>();
                    }
                }
            }

            if (ChildInertiaPtr != 0)
            {
                ChildInertia = reader.ReadArray<Vector4>(ChildCount, ChildInertiaPtr);
            }

            if (ChildInertiaDamagedPtr != 0)
            {
                ChildInertiaDamaged = reader.ReadArray<Vector4>(ChildCount, ChildInertiaDamagedPtr);
            }

            if (MatricesPtr != 0)
            {
                Matrices = reader.ReadArray<Matrix3x4>(ChildCount, MatricesPtr);
            }

            if (SelfCollisionIndices1Ptr != 0)
            {
                SelfCollisionIndices1 = reader.ReadArray<byte>(SelfCollisionCount, SelfCollisionIndices1Ptr);
            }

            if (SelfCollisionIndices2Ptr != 0)
            {
                SelfCollisionIndices2 = reader.ReadArray<byte>(SelfCollisionCount, SelfCollisionIndices2Ptr);
            }

            //Name = NameRef.Value;
            //BuildPiece();
        }

        public void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            if (Drawable.Item == null)
            {
                return;
            }

            Xml.SelfClosingTag(sb, indent, "BoundingSphereCenter " + FloatUtil.GetVector3XmlString(Vector3.Subtract(Drawable.Item.BoundingSphere.Center, Drawable.Item.BoundingCenter)));
            Xml.ValueTag(sb, indent, "BoundingSphereRadius", FloatUtil.ToString(Drawable.Item.BoundingSphereRadius));
            Xml.ValueTag(sb, indent, "UnknownB0", "0");
            Xml.ValueTag(sb, indent, "UnknownB8", "0");
            Xml.ValueTag(sb, indent, "UnknownBC", "0");
            Xml.ValueTag(sb, indent, "UnknownC0", "65280");
            Xml.ValueTag(sb, indent, "UnknownC4", "1");
            Xml.ValueTag(sb, indent, "UnknownCC", "0");
            Xml.ValueTag(sb, indent, "GravityFactor", "1");
            Xml.ValueTag(sb, indent, "BuoyancyFactor", "1");

            float[] matrices = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            if (Drawable.Item != null)
            {
                Xml.OpenTag(sb, indent, "Drawable", true, "");
                 //Xml.StringTag(sb, indent + 1, "Name", Name, "");
                  Xml.WriteRawArray(sb, indent + 1, "Matrix", matrices, null, 3);
                 Drawable.Item.WriteXml(sb, indent + 1, ddsfolder);
                Xml.CloseTag(sb, indent, "Drawable", true);
            }
        }

        public void BuildPiece()
        {
            var d = Drawable.Item;
            var b = Bound.Item;
            var bbmin = Vector3.Zero;
            var bbmax = Vector3.Zero;
            var loddist0 = 250.0f;
            var loddist1 = 500.0f;
            var loddist2 = 1000.0f;
            var loddist3 = 9999.0f;
            var lodlists = new List<Model>[4];

            for (int l = 0; l < 4; l++)
            {
                lodlists[l] = new List<Model>();
            }

            if (d != null)
            {
                if (d.Lods != null)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        var lodlist = lodlists[l];
                        var dlod = d.Lods[l];

                        if (dlod == null)
                            continue;

                        lodlist.AddRange(dlod.Models);
                    }
                }
                bbmin = d.BoundingBoxMin;
                bbmax = d.BoundingBoxMax;
                loddist0 = Math.Max(d.LodDistHigh, 250.0f);
                loddist1 = Math.Max(d.LodDistMed, 500.0f);
                loddist2 = Math.Max(d.LodDistLow, 1000.0f);
                loddist3 = Math.Max(d.LodDistVlow, 9999.0f);
            }

            if (b != null)
            {
                bbmin = b.BoxMin.XYZ();
                bbmax = b.BoxMax.XYZ();
            }


            if (Children != null)
            {
                var sg = d?.ShaderGroup ?? new Rsc6Ptr<Rsc6ShaderGroup>();
                for (int i = 0; i < Children.Length; i++)
                {
                    var child = Children[i];
                    if (child == null) continue;

                    for (int l = 0; l < 4; l++)
                    {
                        var lodlist = lodlists[l];
                        var d1 = child.Drawable.Item;

                        if (d1?.Lods != null)
                        {
                            d1.ShaderGroup = sg;
                            d1.AssignShaders();

                            var dlod = d1.Lods[l];
                            if (dlod == null) continue;

                            lodlist.AddRange(dlod.Models);
                        }
                    }
                }
            }

            bbmin = new Vector3(bbmin.Z, bbmin.X, bbmin.Y);
            bbmax = new Vector3(bbmax.Z, bbmax.X, bbmax.Y);

            /*Lods = new PieceLod[4];
            Lods[0] = new PieceLod() { LodDist = loddist0, Models = lodlists[0].ToArray() };
            Lods[1] = new PieceLod() { LodDist = loddist1, Models = lodlists[1].ToArray() };
            Lods[2] = new PieceLod() { LodDist = loddist2, Models = lodlists[2].ToArray() };
            Lods[3] = new PieceLod() { LodDist = loddist3, Models = lodlists[3].ToArray() };
            Skeleton = d.Skeleton;
            BoundingSphere = new BoundingSphere((bbmin + bbmax) * 0.5f, (bbmax - bbmin).Length() * 0.5f);
            BoundingBox = new BoundingBox(bbmin, bbmax);
            Collider = b;
            UpdateAllModels();*/
        }
    }

    public class Rsc6FragDrawable : Rsc6DrawableBase
    {
        //TODO: determine what's different here
    }

    public class Rsc6FragPhysArchetype : Rsc6BlockBase
    {
        public override ulong BlockLength => throw new NotImplementedException();

        public override void Read(Rsc6DataReader reader)
        {
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6FragTypeGroup : Rsc6BlockBase
    {
        public override ulong BlockLength => throw new NotImplementedException();

        public override void Read(Rsc6DataReader reader)
        {
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Rsc6FragTypeChild : Rsc6BlockBase
    {
        public override ulong BlockLength => 156;
        public uint VFT { get; set; }
        public byte[] Pad { get; set; } //[140] (juicy stuff)
        public Rsc6Ptr<Rsc6FragDrawable> Drawable { get; set; }
        public Rsc6Ptr<Rsc6FragDrawable> Drawable2 { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            VFT = reader.ReadUInt32();
            Pad = reader.ReadArray<byte>(140);
            Drawable = reader.ReadPtr<Rsc6FragDrawable>();
            Drawable2 = reader.ReadPtr<Rsc6FragDrawable>();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
