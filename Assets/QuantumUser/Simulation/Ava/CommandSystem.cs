namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class CommandSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            for (int i = 0; i < f.PlayerCount; i++)
            {
                var pause = f.GetPlayerCommand(i) as PauseSimulation;
                pause?.Execute(f);
                
                var advanceOnce = f.GetPlayerCommand(i) as AdvanceOneFrame;
                advanceOnce?.Execute(f);
            }
        }
    }
}
