using CodeX.Core.Engine;
using CodeX.Games.RDR1.RPF6;

namespace CodeX.Games.RDR1.Prefabs
{
    public class RDR1Weapons
    {
        public string[] WeaponNames;

        public void Init(Rpf6FileManager fman)
        {

        }

        public RDR1WeaponPrefab GetPrefab(string name)
        {
            return null;
        }
    }

    public class RDR1WeaponPrefab : Prefab
    {
        public override Entity CreateInstance(string preset = null)
        {
            return null;
        }
    }
}