using CodeX.Core.Engine;
using CodeX.Games.RDR1.Prefabs;
using CodeX.Games.RDR1.RPF6;

namespace CodeX.Games.RDR1
{
    public class RDR1Prefabs : PrefabManager
    {
        public Rpf6FileManager FileManager;
        public Peds Peds;
        public Weapons Weapons;
        public Vehicles Vehicles;

        public RDR1Prefabs(RDR1Game game)
        {
            Game = game;
        }

        public override bool Init()
        {
            FileManager = Game.GetFileManager() as Rpf6FileManager;
            if (FileManager == null) return false;
            return true;
        }

        public override string[] GetTypeList()
        {
            return new[] { "Peds", "Vehicles", "Weapons" };
        }

        public override string[] GetPrefabList(string type)
        {
            switch (type)
            {
                case "Peds":
                    EnsurePeds();
                    return Peds?.PedNames;
                case "Vehicles":
                    EnsureVehicles();
                    return Vehicles?.VehicleNames;
                /*case "Weapons":
                    EnsureWeapons();
                    return Weapons?.WeaponNames;*/
            }
            return null;
        }

        public override Prefab GetPrefab(string type, string id)
        {
            switch (type)
            {
                case "Peds":
                    return Peds?.GetPrefab(id);
                case "Vehicles":
                    return Vehicles?.GetPrefab(id);
                /*case "Weapons":
                    return Weapons?.GetPrefab(id);*/
            }
            return null;
        }

        private void EnsurePeds()
        {
            if (Peds != null) return;
            Peds = new Peds();
            Peds.Init(FileManager);
        }

        private void EnsureVehicles()
        {
            if (Vehicles != null) return;
            Vehicles = new Vehicles();
            Vehicles.Init(FileManager);
        }

        private void EnsureWeapons()
        {
            if (Weapons != null) return;
            Weapons = new Weapons();
            //Weapons.Init(FileManager);
        }
    }
}