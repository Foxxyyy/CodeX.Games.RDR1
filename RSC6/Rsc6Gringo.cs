using CodeX.Core.Utilities;
using System;
using System.Linq;
using System.Numerics;
using static BepuPhysics.Collidables.CompoundBuilder;
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
        public Rsc6PtrArr<Rsc6GringoBase> Gringos { get; set; }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Unknown_8h = reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            Hashes = reader.ReadArr<JenkHash>();
            Gringos = reader.ReadPtrArr(Rsc6GringoBase.Create);
        }

        public void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public void Write(MetaNodeWriter writer)
        {
            writer.WriteNodeArray("Gringos", Gringos.Items);
        }
    }

    [TC(typeof(EXP))] public class Rsc6GringoBase : Rsc6FileBase, MetaNode //ggoComponentBase
    {
        public override ulong BlockLength => 16;
        public override uint VFT { get; set; } = 0x01979634;
        public Rsc6Str QueryName { get; set; } //mp_QueryName
        public uint HashCode { get; set; } //m_HashCode
        public Rsc6Ptr<Rsc6GringoBase> ParentComponent { get; set; } //mp_ParentComponent

        public Rsc6ComponentType Type { get; set; }

        public Rsc6GringoBase()
        {
        }

        public Rsc6GringoBase(Rsc6ComponentType type)
        {
            Type = type;
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            QueryName = reader.ReadStr();
            HashCode = reader.ReadUInt32();
            ParentComponent = reader.ReadPtr<Rsc6GringoBase>();
        }

        public virtual void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public virtual void Write(MetaNodeWriter writer)
        {
            writer.WriteString("@type", Type.ToString());
            if (QueryName.Value != null) writer.WriteString("QueryName", QueryName.ToString());
            writer.WriteString("HashCode", "0x" + HashCode.ToString("X8"));
        }

        public static Rsc6GringoBase Create(Rsc6DataReader r)
        {
            var type = (Rsc6ComponentType)r.ReadUInt32();
            if (!Enum.IsDefined(typeof(Rsc6ComponentType), type))
            {
            }
            r.Position -= 4;
            return Create(type);
        }

        public static Rsc6GringoBase Create(Rsc6ComponentType type)
        {
            return type switch
            {
                Rsc6ComponentType.ComponentItemGringo => new Rsc6Gringo(),
                Rsc6ComponentType.ComponentUseContext => new Rsc6GringoUseContext(),
                Rsc6ComponentType.ComponentItemAttributes => new Rsc6GringoItemAttributes(),
                //_ => throw new Exception("Unknown gringo component")
                _ => new Rsc6GringoBase()
            };
        }
    }

    [TC(typeof(EXP))] public class Rsc6Gringo : Rsc6GringoBase //ggoItemGringo
    {
        public override ulong BlockLength => base.BlockLength + 60;
        public short InstanceIndex { get; set; } //m_iInstanceIndex
        public short PaddingToFoxPs3 { get; set; } = -1; //m_iPaddingToFoxPs3
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public Rsc6Str ScriptName { get; set; } //mp_ScriptName
        public Rsc6Str GringoName { get; set; } //mp_GringoName
        public Rsc6PtrArr<Rsc6GringoBase> Childs { get; set; } //ggoChildComponentList<ggoComponentBase>
        public Rsc6PtrArr<Rsc6BlockMap> InstancedItems { get; set; } //m_InstancedItems, ggoInstancedTypeDataBase
        public uint HashedName { get; set; } //m_iHashedName
        public uint MessageMask { get; set; } //m_iMessageMask
        public float ActivationRadius { get; set; } //m_ActivationRadius
        public int InstanceSlotCount { get; set; } //m_iInstanceSlotCount
        public bool Critical { get; set; } //m_bCritical
        public bool LargeScript { get; set; } //m_bLargeScript
        public bool MaintainState { get; set; } //m_bMaintainState
        public byte Unknown_4Bh { get; set; } //m_bMaintainState

        public Rsc6Gringo() : base(Rsc6ComponentType.ComponentItemGringo)
        {
        }

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
            Childs = reader.ReadPtrArr(Create);
            InstancedItems = reader.ReadPtrArr<Rsc6BlockMap>();
            HashedName = reader.ReadUInt32();
            MessageMask = reader.ReadUInt32();
            ActivationRadius = reader.ReadSingle();
            InstanceSlotCount = reader.ReadInt32();
            Critical = reader.ReadBoolean();
            LargeScript = reader.ReadBoolean();
            MaintainState = reader.ReadBoolean();
            Unknown_4Bh = reader.ReadByte();
        }

        public override void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(MetaNodeWriter writer)
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
            writer.WriteUInt32("MessageMask", MessageMask);
            writer.WriteSingle("ActivationRadius", ActivationRadius);
            writer.WriteInt32("InstanceSlotCount", InstanceSlotCount);
            writer.WriteBool("Critical", Critical);
            writer.WriteBool("LargeScript", LargeScript);
            writer.WriteBool("MaintainState", MaintainState);
        }

        public override string ToString()
        {
            return ScriptName.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6GringoUseContext : Rsc6GringoBase //ggoComponentUseContext
    {
        public override ulong BlockLength => base.BlockLength + 112;
        public Rsc6PtrArr<Rsc6BlockMap> Attributes { get; set; } //m_Attributes, atArray<ggoItemPureAttribList>
        public short InstanceIndex { get; set; } //m_iInstanceIndex
        public short PaddingToFoxPs3 { get; set; } //m_iPaddingToFoxPs3
        public float Facing { get; set; } //m_fFacing
        public Vector4 LocalPosition { get; set; } //m_LocalPosition
        public float Radius { get; set; } //m_Radius
        public int ParentTransformRemap { get; set; } //m_iParentTransformRemap
        public Rsc6Str ParentTransformRemappedBone { get; set; } //m_ParentTransformRemap_BoneName, placeholder
        public uint Unknown_3Ch { get; set; } //Always 0
        public Rsc6Str RaceType { get; set; } //m_RaceTypeAsString, placeholder
        public uint Unknown_44h { get; set; } //Always 0
        public int UsePriorityTweak { get; set; } //m_iUsePriorityTweak
        public Rsc6Str UnusableWeather { get; set; } //m_UnusableWeatherAsString, placeholder
        public uint Unknown_50h { get; set; } //Always 0
        public Rsc6PtrArr<Rsc6GringoBase> ChildComponents { get; set; } //m_ChildComponents
        public Rsc6Ptr<Rsc6BlockMap> Fulfillment { get; set; } //mp_Fulfillment, motMotiveFulfillment
        public int UseButton { get; set; } //m_iUseButton
        public Rsc6Str UserTag { get; set; } //m_UserTag
        public ushort UnusableWeatherType { get; set; } //m_UnusableWeatherType
        public bool SuspendMover { get; set; } //m_bSuspendMover
        public bool FixUserMover { get; set; } //m_bFixUserMover
        public bool PlayerUsable { get; set; } //m_bPlayerUsable
        public bool PositionParentActorRelative { get; set; } //m_bPositionParentActorRelative
        public bool ActorBecomesObstacle { get; set; } //m_bActorBecomesObstacle
        public bool IsMeleeAttack { get; set; } //m_bIsMeleeAttack
        public bool GringoHandlesMovement { get; set; } //m_bGringoHandlesMovement
        public bool IsCombatFriendly { get; set; } //m_bIsCombatFriendly
        public bool IsJumpGringo { get; set; } //m_bIsJumpGringo
        public bool RequiresPhysicsCheck { get; set; } //m_bRequiresPhysicsCheck
        public bool RequiresGroundCheck { get; set; } //m_bRequiresGroundCheck
        public bool RequiresLOSCheck { get; set; } //m_bRequiresLOSCheck
        public bool RequiresNavProbeCheck { get; set; } //m_bRequiresNavProbeCheck
        public bool StartUnavailable { get; set; } //m_bStartUnavailable
        public bool BlockInjuryReactions { get; set; } //m_bBlockInjuryReactions
        public bool AllowAiShoot { get; set; } //m_bAllowAiShoot
        public bool AutoPlayForPlayer { get; set; } //m_bAutoPlayForPlayer
        public bool AlwaysApproach { get; set; } //m_bAlwaysApproach
        public bool WaitForStill { get; set; } //m_bWaitForStill
        public bool SlowDownWhenApproaching { get; set; } //m_bSlowDownWhenApproaching
        public bool AllowNavigateTo { get; set; } //m_bAllowNavigateTo
        public byte Unknown_7Fh { get; set; } = 0xCF; //Padding

        public Rsc6GringoUseContext() : base(Rsc6ComponentType.ComponentUseContext)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Attributes = reader.ReadPtrArr<Rsc6BlockMap>();
            InstanceIndex = reader.ReadInt16();
            PaddingToFoxPs3 = reader.ReadInt16();
            Facing = reader.ReadSingle();
            LocalPosition = reader.ReadVector4();
            Radius = reader.ReadSingle();
            ParentTransformRemap = reader.ReadInt32();
            ParentTransformRemappedBone = reader.ReadStr();
            Unknown_3Ch = reader.ReadUInt32();
            RaceType = reader.ReadStr();
            Unknown_44h = reader.ReadUInt32();
            UsePriorityTweak = reader.ReadInt32();
            UnusableWeather = reader.ReadStr();
            Unknown_50h = reader.ReadUInt32();
            ChildComponents = reader.ReadPtrArr<Rsc6GringoBase>();
            Fulfillment = reader.ReadPtr<Rsc6BlockMap>();
            UseButton = reader.ReadInt32();
            UserTag = reader.ReadStr();
            UnusableWeatherType = reader.ReadUInt16();
            SuspendMover = reader.ReadBoolean();
            FixUserMover = reader.ReadBoolean();
            PlayerUsable = reader.ReadBoolean();
            PositionParentActorRelative = reader.ReadBoolean();
            ActorBecomesObstacle = reader.ReadBoolean();
            IsMeleeAttack = reader.ReadBoolean();
            GringoHandlesMovement = reader.ReadBoolean();
            IsCombatFriendly = reader.ReadBoolean();
            IsJumpGringo = reader.ReadBoolean();
            RequiresPhysicsCheck = reader.ReadBoolean();
            RequiresGroundCheck = reader.ReadBoolean();
            RequiresLOSCheck = reader.ReadBoolean();
            RequiresNavProbeCheck = reader.ReadBoolean();
            StartUnavailable = reader.ReadBoolean();
            BlockInjuryReactions = reader.ReadBoolean();
            AllowAiShoot = reader.ReadBoolean();
            AutoPlayForPlayer = reader.ReadBoolean();
            AlwaysApproach = reader.ReadBoolean();
            WaitForStill = reader.ReadBoolean();
            SlowDownWhenApproaching = reader.ReadBoolean();
            AllowNavigateTo = reader.ReadBoolean();
            Unknown_7Fh = reader.ReadByte();
        }

        public override void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(MetaNodeWriter writer)
        {
            var remap = ParentTransformRemappedBone.Value;
            var racetype = RaceType.Value;
            var weather = UnusableWeather.Value;
            var usertag = UserTag.Value;

            base.Write(writer);
            //writer.WriteNodeArray("Attributes", Attributes.Items);
            writer.WriteInt16("InstanceIndex", InstanceIndex);
            writer.WriteInt16("PaddingToFoxPs3", PaddingToFoxPs3);
            writer.WriteSingle("Facing", Facing);
            writer.WriteVector4("LocalPosition", LocalPosition);
            writer.WriteSingle("Radius", Radius);
            writer.WriteInt32("ParentTransformRemap", ParentTransformRemap);
            if (!string.IsNullOrEmpty(remap)) writer.WriteString("ParentTransformRemappedBone", remap);
            writer.WriteUInt32("Unknown_3Ch", Unknown_3Ch);
            if (!string.IsNullOrEmpty(racetype)) writer.WriteString("RaceType", racetype);
            writer.WriteUInt32("Unknown_44h", Unknown_44h);
            writer.WriteInt32("UsePriorityTweak", UsePriorityTweak);
            if (!string.IsNullOrEmpty(weather)) writer.WriteString("UnusableWeather", weather);
            writer.WriteUInt32("Unknown_50h", Unknown_50h);
            writer.WriteNodeArray("ChildComponents", ChildComponents.Items);
            //writer.WriteNode("Fulfillment", Fulfillment.Item);
            writer.WriteInt32("UseButton", UseButton);
            if (!string.IsNullOrEmpty(usertag)) writer.WriteString("UserTag", usertag);
            writer.WriteUInt16("UnusableWeatherType", UnusableWeatherType);
            writer.WriteBool("SuspendMover", SuspendMover);
            writer.WriteBool("FixUserMover", FixUserMover);
            writer.WriteBool("PlayerUsable", PlayerUsable);
            writer.WriteBool("PositionParentActorRelative", PositionParentActorRelative);
            writer.WriteBool("ActorBecomesObstacle", ActorBecomesObstacle);
            writer.WriteBool("IsMeleeAttack", IsMeleeAttack);
            writer.WriteBool("GringoHandlesMovement", GringoHandlesMovement);
            writer.WriteBool("IsCombatFriendly", IsCombatFriendly);
            writer.WriteBool("IsJumpGringo", IsJumpGringo);
            writer.WriteBool("RequiresPhysicsCheck", RequiresPhysicsCheck);
            writer.WriteBool("RequiresGroundCheck", RequiresGroundCheck);
            writer.WriteBool("RequiresLOSCheck", RequiresLOSCheck);
            writer.WriteBool("RequiresNavProbeCheck", RequiresNavProbeCheck);
            writer.WriteBool("StartUnavailable", StartUnavailable);
            writer.WriteBool("BlockInjuryReactions", BlockInjuryReactions);
            writer.WriteBool("AllowAiShoot", AllowAiShoot);
            writer.WriteBool("AutoPlayForPlayer", AutoPlayForPlayer);
            writer.WriteBool("AlwaysApproach", AlwaysApproach);
            writer.WriteBool("WaitForStill", WaitForStill);
            writer.WriteBool("SlowDownWhenApproaching", SlowDownWhenApproaching);
            writer.WriteBool("AllowNavigateTo", AllowNavigateTo);
            writer.WriteByte("Unknown_7Fh", Unknown_7Fh);
        }

        public override string ToString()
        {
            return UserTag.ToString();
        }
    }

    [TC(typeof(EXP))] public class Rsc6GringoItemAttributes : Rsc6GringoBase //ggoItemPureAttribList
    {
        public override ulong BlockLength => base.BlockLength + 112;
        public Rsc6PtrArr<Rsc6GringoItemAttribBase> Attributes { get; set; } //m_Attributes, atArray<ggoItemPureAttribBaseRef>

        public Rsc6GringoItemAttributes() : base(Rsc6ComponentType.ComponentItemAttributes)
        {
        }

        public override void Read(Rsc6DataReader reader)
        {
            base.Read(reader);
            Attributes = reader.ReadPtrArr<Rsc6GringoItemAttribBase>();
        }

        public override void Read(MetaNodeReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(MetaNodeWriter writer)
        {
            base.Write(writer);
            if (Attributes.Items != null)
            {
                writer.WriteUInt32Array("Attributes", Attributes.Items.Select(attr => attr.HashCode).ToArray());
            }
        }
    }

    public class Rsc6GringoItemAttribBase : Rsc6BlockBase //ggoItemPureAttribBaseRef
    {
        public override ulong BlockLength => 4;
        public uint HashCode { get; set; } //m_HashCode

        public override void Read(Rsc6DataReader reader)
        {
            HashCode = reader.ReadUInt32();
        }

        public override void Write(Rsc6DataWriter writer)
        {
            writer.WriteUInt32(HashCode);
        }
    }

    public enum Rsc6ComponentType : uint
    {
        ComponentBase = 0x0,
        ComponentItemGringo = 0x01979634, //ggoItemGringo
        ComponentAnimation = 95797610,
        ComponentUseContext = 0xE03269C1, //ggoComponentUseContext
        ComponentItemAttributes = 0xB16C14A8, //ggoItemPureAttribList
    }
}
