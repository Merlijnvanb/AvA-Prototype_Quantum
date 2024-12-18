namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class InputUtils
    {
        public static Input[] GetLastInputs(int count, FighterData* fd)
        {
            Input[] lastInputs = new Input[count];
            for (int i = 0; i < count; i++)
            {
                int index = (fd->InputHeadIndex - i + fd->InputHistory.Length) % fd->InputHistory.Length;
                lastInputs[i] = fd->InputHistory[index];
            }
            return lastInputs;
        }
        
        // public static bool CheckForwardDash(Input* input, FighterData* fighterData)
        // {
        //     
        // }
        
        public static bool IsForward(Input input, FighterData* fd)
        {
            return (input.Right && fd->IsFacingRight) || (input.Left && !fd->IsFacingRight);
        }

        public static bool IsBackward(Input input, FighterData* fd)
        {
            return (input.Left && fd->IsFacingRight) || (input.Right && !fd->IsFacingRight);
        }
    }
}
