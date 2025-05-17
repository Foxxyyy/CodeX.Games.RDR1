using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeX.Games.RDR1.RSC6
{
    [TC(typeof(EXP))] public class Rsc6CombatCoverGrid : Rsc6BlockBaseMap //rage::ai::bhCombatCoverGrid
    {
        public override ulong BlockLength => 64;
        public override uint VFT { get; set; } = 0x04A57A40;

        public override void Read(Rsc6DataReader reader)
        {
            
        }

        public override void Write(Rsc6DataWriter writer)
        {
            
        }
    }
}