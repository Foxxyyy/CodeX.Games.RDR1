using CodeX.Core.Engine;
using CodeX.Games.RDR1.Prefabs;
using CodeX.Games.RDR1.RPF6;

namespace CodeX.Games.RDR1
{
    public class RDR1Prefabs : PrefabManager
    {
        public Rpf6FileManager FileManager;
        public RDR1Peds Peds;
        public RDR1Animals Animals;
        public RDR1Vehicles Vehicles;

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
            return new[] { "Peds", "Vehicles", "Animals" };
        }

        public override string[] GetPrefabList(string type)
        {
            switch (type)
            {
                case "Peds":
                    EnsurePeds();
                    return Peds?.PedNames.ToArray();
                case "Vehicles":
                    EnsureVehicles();
                    return Vehicles?.VehicleNames;
                case "Animals":
                    EnsureAnimals();
                    return Animals?.AnimalNames;
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
                case "Animals":
                    return Animals?.GetPrefab(id);
            }
            return null;
        }

        private void EnsurePeds()
        {
            if (Peds != null) return;
            Peds = new RDR1Peds();
            Peds.Init(FileManager);
        }

        private void EnsureVehicles()
        {
            if (Vehicles != null) return;
            Vehicles = new RDR1Vehicles();
            Vehicles.Init(FileManager);
        }

        private void EnsureAnimals()
        {
            if (Animals != null) return;
            Animals = new RDR1Animals();
            Animals.Init(FileManager);
        }
    }
}