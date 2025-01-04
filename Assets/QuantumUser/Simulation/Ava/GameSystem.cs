namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class GameSystem : SystemMainThread
    {
        public override void OnInit(Frame f)
        {
            var gameConfig = f.FindAsset<AvaGameConfig>(f.RuntimeConfig.GameConfig);
            f.Global->ParseInputs = false;
            f.Global->StageWidth = gameConfig.StageWidth;
            f.Global->MaxFighterDistance = gameConfig.MaxPlayerDistance;
            f.Global->DashAllowFrames = gameConfig.DashAllowFrames;
            f.Global->JumpAlterFrames = gameConfig.JumpAlterFrames;
            f.Global->DownwardForce = gameConfig.DownwardForce;
            f.Global->FrictionCoefficient = gameConfig.FrictionCoefficient;
            f.Global->SideSwitchThreshold = gameConfig.SideSwitchThreshold;
            f.Global->PreRoundTimer = gameConfig.PreRoundTimer;
            f.Global->RoundTimer = gameConfig.RoundTimer;
            f.Global->Fighter1Score = 0;
            f.Global->Fighter2Score = 0;
            f.Global->HitstopFrames = 0;
            f.Global->PauseSimulation = false;
            f.Global->AdvanceOneFrame = false;
        }
        
        public override void Update(Frame f)
        {
            Log.Debug(f.Global->PauseSimulation);
            
            if (f.Global->PauseSimulation)
            {
                if (!f.Global->AdvanceOneFrame)
                    return;
                
                f.Global->AdvanceOneFrame = false;
            }
            
            if (f.Global->HitstopFrames > 0)
            {
                f.Global->HitstopFrames--;
                return;
            }
            
            if (f.Global->PreRoundTimer > 0)
                f.Global->PreRoundTimer--;
            else if (f.Global->RoundTimer > 0)
            {
                f.Global->RoundTimer--;
                f.Global->ParseInputs = true;

                if (SetScore(f))
                {
                    ResetRound(f);
                }
            }
            else
            {
                TimeOut(f);
            }
            
            var filter1 = new FighterHandler.Filter()
            {
                Entity = f.Global->Fighter1,
                FighterData = f.Unsafe.GetPointer<FighterData>(f.Global->Fighter1)
            };

            var filter2 = new FighterHandler.Filter()
            {
                Entity = f.Global->Fighter2,
                FighterData = f.Unsafe.GetPointer<FighterData>(f.Global->Fighter2)
            };
            
            CheckSides(f);
            
            FighterHandler.UpdateFighter(f, ref filter1);
            FighterHandler.UpdateFighter(f, ref filter2);
            CollisionHandler.UpdateCollision(f);
            HitRegHandler.UpdateHitReg(f);
            
            f.Events.UpdateUI(f.Global->Fighter1, f.Global->Fighter2, f.Global->Fighter1Score, f.Global->Fighter2Score);
        }
        
        private bool SetScore(Frame f)
        {
            if (!f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter1, out var fighter1) ||
                !f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter2, out var fighter2))
                return false;
            
            bool p1Dead = fighter1->Health <= 0;
            bool p2Dead = fighter2->Health <= 0;

            if (p1Dead && !p2Dead)
            {
                f.Global->Fighter2Score++;
                return true;
            }
            if (p2Dead && !p1Dead)
            {
                f.Global->Fighter1Score++;
                return true;
            }
            if (p1Dead && p2Dead)
            {
                f.Global->Fighter1Score++;
                f.Global->Fighter2Score++;
                return true;
            }

            return false;
        }

        private void TimeOut(Frame f)
        {
            if (!f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter1, out var fighter1) ||
                !f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter2, out var fighter2))
                return;
            
            var p1Health = fighter1->Health;
            var p2Health = fighter2->Health;
            
            if (p1Health > p2Health) f.Global->Fighter1Score++;
            if (p2Health > p1Health) f.Global->Fighter2Score++;
            if (p1Health == p2Health)
            {
                f.Global->Fighter1Score++;
                f.Global->Fighter2Score++;
            }
            
            ResetRound(f);
        }

        private void ResetRound(Frame f)
        {
            f.Global->ParseInputs = false;
            
            var gameConfig = f.FindAsset<AvaGameConfig>(f.RuntimeConfig.GameConfig);
            f.Global->PreRoundTimer = gameConfig.PreRoundTimer;
            f.Global->RoundTimer = gameConfig.RoundTimer;
            
            if (!f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter1, out var fighter1) ||
                !f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter2, out var fighter2))
                return;
            
            ResetFighter(f, fighter1, true);
            ResetFighter(f, fighter2, false);
        }

        private void ResetFighter(Frame f, FighterData* fd, bool p1)
        {
            fd->Position = p1 ? new FPVector2(-1, 0) : new FPVector2(1, 0);
            fd->Velocity = new FPVector2(0, 0);
            fd->Pushback = new FPVector2(0, 0);
            fd->IsFacingRight = p1;
            fd->PreviousPushback = new FPVector2(0, 0);
            fd->RequestedSideSwitch = 0;
            fd->ProximityGuard = false;
            fd->Health = f.FindAsset(fd->Constants).MaxHealth;
            fd->HitStun = 0;
            fd->BlockStun = 0;
            fd->CurrentState = StateID.STAND;
            fd->StateFrame = 0;
        }

        private void CheckSides(Frame f)
        {
            if (f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter1, out var fighter1) &&
                f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter2, out var fighter2))
            {
                if (fighter1->Position.X > fighter2->Position.X + f.Global->SideSwitchThreshold)
                {
                    if (fighter1->IsFacingRight) fighter1->RequestedSideSwitch = 2;
                    if (!fighter2->IsFacingRight) fighter2->RequestedSideSwitch = 1;
                }
                else if (fighter1->Position.X < fighter2->Position.X - f.Global->SideSwitchThreshold)
                {
                    if (!fighter1->IsFacingRight) fighter1->RequestedSideSwitch = 1;
                    if (fighter2->IsFacingRight) fighter2->RequestedSideSwitch = 2;
                }
            }
        }
    }
}
