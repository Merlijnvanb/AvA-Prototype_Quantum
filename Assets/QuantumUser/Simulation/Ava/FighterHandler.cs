namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class FighterHandler
    {
        public struct Filter
        {
            public EntityRef Entity;
            // public Transform3D* Transform;
            public FighterData* FighterData;
        }
        
        public static void UpdateFighter(Frame f, ref Filter filter)
        {
            UpdateFacing(f, ref filter);
            IncrementState(f, ref filter);
            
            InputManager.UpdateInputs(f, ref filter);
            StateManager.UpdateState(f, ref filter);
            MovementHandler.UpdateMovement(f, ref filter);
            BoxManager.UpdateBoxes(f, ref filter);
        }

        private static void UpdateFacing(Frame f, ref Filter filter)
        {
            var fd = filter.FighterData;
            var constants = f.FindAsset<FighterConstants>(fd->Constants);

            if (fd->RequestedSideSwitch != 0 && (fd->StateFrame > constants.States[fd->CurrentState].FrameCount ||
                                                 constants.States[fd->CurrentState].IsAlwaysCancelable))
            {
                fd->IsFacingRight = fd->RequestedSideSwitch == 1;
                fd->RequestedSideSwitch = 0;
            }
        }

        private static void IncrementState(Frame f, ref Filter filter)
        {
            var fd = filter.FighterData;

            if (fd->HitStun != 0)
            {
                fd->HitStun--;
                return;
            }

            if (fd->BlockStun != 0)
            {
                fd->BlockStun--;
                return;
            }
            
            fd->StateFrame++;
        }
    }
}

