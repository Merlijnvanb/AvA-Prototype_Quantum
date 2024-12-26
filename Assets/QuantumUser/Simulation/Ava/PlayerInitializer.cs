using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.Ava
{
    [Preserve]
    public unsafe class PlayerInitializer : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public override void OnInit(Frame f) // add logic for always having 2 fighter instances, even without players joining
        {
            for (int i = 1; i < 3; i++)
            {
                var fighterEntity = f.Create(f.FindAsset(f.RuntimeConfig.BaseFighter));

                var fData = f.Unsafe.GetPointer<FighterData>(fighterEntity);
                var fConstants = f.FindAsset(f.RuntimeConfig.BaseConstants);
                fConstants.SetupDictionaries();
                
                fData->Constants = fConstants;
                fData->FighterID = i;
                fData->Position = i == 1 ? new FPVector2(-1, 0) : new FPVector2(1, 0);
                fData->Velocity = new FPVector2(0, 0);
                fData->Pushback = new FPVector2(0, 0);
                fData->IsFacingRight = i == 1;
                fData->PreviousPushback = new FPVector2(0, 0);
                fData->RequestedSideSwitch = 0;
                fData->ProximityGuard = false;
                fData->Health = fConstants.MaxHealth;
                fData->HitStun = 0;
                fData->BlockStun = 0;
                fData->CurrentState = StateID.STAND;
                fData->StateFrame = 0;
                fData->HitboxList = f.AllocateList<Hitbox>();
                fData->HurtboxList = f.AllocateList<Hurtbox>();
                fData->Pushbox = new Pushbox();
                fData->InputHeadIndex = -1;
                fData->AttackRegistry = f.AllocateDictionary<AttackID, int>();
                
                if (i == 1)
                    f.Global->Fighter1 = fighterEntity;
                else
                    f.Global->Fighter2 = fighterEntity;
            }
        }
        
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            if (player._index > 2)
                return;
            
            var data = f.GetPlayerData(player);
            var fighterEntity = player._index == 1 ? f.Global->Fighter1 : f.Global->Fighter2;
            var playerLink = f.Unsafe.GetPointer<PlayerLink>(fighterEntity);
            var fighterData = f.Unsafe.GetPointer<FighterData>(fighterEntity);
            var constants = f.FindAsset(data.FighterConstants);

            playerLink->PlayerRef = player;
            fighterData->Constants = constants;
        }
    }
}