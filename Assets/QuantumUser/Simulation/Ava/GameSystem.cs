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
        }
        
        public override void Update(Frame f)
        {
            if (f.Global->PreRoundTimer > 0)
                f.Global->PreRoundTimer--;
            else
                f.Global->ParseInputs = true;
            
            CheckSides(f);
            f.Events.UpdateUI(f.Global->Fighter1, f.Global->Fighter2, f.Global->Fighter1Score, f.Global->Fighter2Score);
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
