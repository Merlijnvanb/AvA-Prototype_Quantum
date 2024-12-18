namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;
    
    [Preserve]
    public unsafe class InputManager
    {
        public static void UpdateInputs(Frame f, ref FighterSystem.Filter filter)
        {
            var fData = filter.FighterData;
            fData->InputHeadIndex = (fData->InputHeadIndex + 1) % fData->InputHistory.Length;
            
            if (!f.Global->ParseInputs || !f.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
                fData->InputHistory[fData->InputHeadIndex] = new Input();
            else
                fData->InputHistory[fData->InputHeadIndex] = *f.GetPlayerInput(playerLink->PlayerRef);
        }
    }
}