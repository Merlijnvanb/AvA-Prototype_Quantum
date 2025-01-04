namespace Quantum
{
    using UnityEngine.Scripting;
    using Photon.Deterministic;
    
    [Preserve]
    public unsafe class PauseSimulation : DeterministicCommand
    {
        public override void Serialize(BitStream stream)
        {}

        public void Execute(Frame f)
        {
            f.Global->PauseSimulation = !f.Global->PauseSimulation;
        }
    }
    
    [Preserve]
    public unsafe class AdvanceOneFrame : DeterministicCommand
    {
        public override void Serialize(BitStream stream)
        {}

        public void Execute(Frame f)
        {
            if (f.Global->PauseSimulation)
                f.Global->AdvanceOneFrame = true;
        }
    }
}
