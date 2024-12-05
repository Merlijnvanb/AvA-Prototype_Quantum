namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class InputUtils
    {
        public static bool IsForward(Input* input, FighterData* fd)
        {
            return (input->Right && fd->IsFacingRight) || (input->Left && !fd->IsFacingRight);
        }

        public static bool IsBackward(Input* input, FighterData* fd)
        {
            return (input->Left && fd->IsFacingRight) || (input->Right && !fd->IsFacingRight);
        }
    }
}
