using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.Ava
{
    [Preserve]
    public unsafe class PlayerInitializer : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public override void OnInit(Frame f)
        {
            for (int i = 0; i < 2; i++)
            {
                
            }
        }
        
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var data = f.GetPlayerData(player);
            var fighterEntity = f.Create(f.FindAsset<EntityPrototype>(data.PlayerAvatar));

            var fighterData = f.Unsafe.GetPointer<FighterData>(fighterEntity);
            var constants = f.FindAsset<FighterConstants>(data.FighterConstants);
            
            constants.SetupDictionaries();
            fighterData->Constants = constants;

            if (player._index == 1)
            {
                fighterData->Position = new FPVector2(-1, 0);
                fighterData->IsFacingRight = true;
                f.Global->Fighter1 = fighterEntity;
            }
            else if (player._index == 2)
            {
                fighterData->Position = new FPVector2(1, 0);
                fighterData->IsFacingRight = false;
                f.Global->Fighter2 = fighterEntity;
            }
            
            fighterData->Velocity = new FPVector2(0, 0);
            fighterData->Pushback = new FPVector2(0, 0);
            fighterData->IsGrounded = true;
            fighterData->requestedSideSwitch = 0;
            fighterData->Health = constants.MaxHealth;
            fighterData->HitStun = 0;
            fighterData->BlockStun = 0;
            fighterData->CurrentState = StateID.STAND;
            fighterData->StateFrame = 0;
            fighterData->HitBoxList = new QListPtr<HitBox>();
            fighterData->HurtBoxList = new QListPtr<HurtBox>();
            fighterData->PushBox = new PushBox();

            f.Add(fighterEntity, new PlayerLink { PlayerRef = player });
        }
    }
}