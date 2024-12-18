namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class StateManager
    {
        public static void UpdateState(Frame f, ref FighterSystem.Filter filter)
        {
            var fData = filter.FighterData;
            var fConstants = f.FindAsset(fData->Constants);
            
            if (CheckAttackState(fData, fConstants)) return;
            if (CheckJumpState(fData, fConstants)) return;
            if (CheckDashState(fData, fConstants)) return;
            if (CheckMovementState(fData, fConstants)) return;

            RequestState(fData, fConstants, StateID.STAND);
        }

        private static bool CheckAttackState(FighterData* fd, FighterConstants fc)
        {
            return false;
        }

        private static bool CheckJumpState(FighterData* fd, FighterConstants fc)
        {
            return false;
        }

        private static bool CheckDashState(FighterData* fd, FighterConstants fc)
        {
            return false;
        }

        private static bool CheckMovementState(FighterData* fd, FighterConstants fc)
        {
            var currentInput = fd->InputHistory[fd->InputHeadIndex];
            
            if (currentInput.Down)
            {
                RequestState(fd, fc, StateID.CROUCH);
                return true;
            }
            if (InputUtils.IsForward(currentInput, fd))
            {
                RequestState(fd, fc, StateID.FORWARD);
                return true;
            }
            if (InputUtils.IsBackward(currentInput, fd))
            {
                RequestState(fd, fc, StateID.BACKWARD);
                return true;
            }
            return false;
        }

        private static bool RequestState(FighterData* fd, FighterConstants fc, StateID stateID)
        {
            if (fd->StateFrame >= fc.States[fd->CurrentState].FrameCount)
            {
                SetCurrentState(fd, stateID);
                return true;
            }

            if (fd->CurrentState == stateID)
                return false;
            
            if (fc.States[fd->CurrentState].IsAlwaysCancelable)
            {
                SetCurrentState(fd, stateID);
                return true;
            }
            
            return false;
        }

        private static void SetCurrentState(FighterData* fd, StateID stateID, int frame = 0)
        {
            fd->CurrentState = stateID;
            fd->StateFrame = frame;
        }
    }
}
