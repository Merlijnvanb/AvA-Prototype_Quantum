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
            if (CheckJumpState(fData, fConstants, f.Global->JumpAlterFrames)) return;
            if (CheckDashState(fData, fConstants, f.Global->DashAllowFrames)) return;
            if (CheckMovementState(fData, fConstants)) return;

            RequestState(fData, fConstants, StateID.STAND);
        }

        private static bool CheckAttackState(FighterData* fd, FighterConstants fc)
        {
            return false;
        }

        private static bool CheckJumpState(FighterData* fd, FighterConstants fc, int jumpAlterFrames)
        {
            var currentInput = fd->InputHistory[fd->InputHeadIndex];
            
            if (currentInput.Up)
            {
                RequestState(fd, fc, StateID.JUMP_NEUTRAL);
                if (fd->CurrentState == StateID.JUMP_NEUTRAL && fd->StateFrame < jumpAlterFrames)
                {
                    SetCurrentState(fd, InputUtils.CheckJumpType(fd, currentInput), fd->StateFrame);
                }
            }
            
            return false;
        }

        private static bool CheckDashState(FighterData* fd, FighterConstants fc, int dashAllowFrames)
        {
            if (InputUtils.CheckDash(fd, dashAllowFrames, Direction.Forward))
            {
                Log.Debug("Forward Dash Requested");
                RequestState(fd, fc, StateID.DASH_FORWARD);
                return true;
            }
            if (InputUtils.CheckDash(fd, dashAllowFrames, Direction.Backward))
            {
                Log.Debug("Backward Dash Requested");
                RequestState(fd, fc, StateID.DASH_BACKWARD);
                return true;
            }
            
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
            if (InputUtils.IsDirection(fd, currentInput, Direction.Forward))
            {
                RequestState(fd, fc, StateID.FORWARD);
                return true;
            }
            if (InputUtils.IsDirection(fd, currentInput, Direction.Backward))
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
