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

            Log.LogLevel = LogType.Debug;
            Log.Debug(f.Global->PreRoundTimer);
        }
    }
}
