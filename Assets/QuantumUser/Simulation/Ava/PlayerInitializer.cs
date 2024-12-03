using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.Ava
{
    [Preserve]
    public unsafe class PlayerInitializer : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var data = f.GetPlayerData(player);
            var entityPrototypeAsset = f.FindAsset<EntityPrototype>(data.PlayerAvatar);

            var fighterEntity = f.Create(entityPrototypeAsset);
            
            var fighterData = f.Unsafe.GetPointer<FighterData>(fighterEntity);
            var constants = f.FindAsset<FighterConstants>(data.FighterConstants);
            constants.SetupDictionaries();
            fighterData->Constants = constants;
            
            Transform3D* fighterTransform = f.Unsafe.GetPointer<Transform3D>(fighterEntity);

            if (player._index == 1)
            {
                fighterTransform->Position = new FPVector3(-1, 0, 0);
                f.Global->Fighter1 = fighterEntity;
            }
            else if (player._index == 2)
            {
                fighterTransform->Position = new FPVector3(1, 0, 0);
                f.Global->Fighter2 = fighterEntity;
            }

            f.Add(fighterEntity, new PlayerLink { PlayerRef = player });
        }
    }
}