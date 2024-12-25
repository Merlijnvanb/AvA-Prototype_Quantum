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
            
            if (CheckAttackState(f, fData, fConstants)) return;
            if (CheckJumpState(f, fData, fConstants, f.Global->JumpAlterFrames)) return;
            if (CheckDashState(f, fData, fConstants, f.Global->DashAllowFrames)) return;
            if (CheckMovementState(f, fData, fConstants)) return;

            RequestState(f, fData, fConstants, StateID.STAND);
        }

        private static bool CheckAttackState(Frame f, FighterData* fd, FighterConstants fc)
        {
            var currentInput = fd->InputHistory[fd->InputHeadIndex];

            if (currentInput.Light.WasPressed)
            {
                RequestState(f, fd, fc, StateID.STAND_LIGHT);
            }
            
            return false;
        }

        private static bool CheckJumpState(Frame f, FighterData* fd, FighterConstants fc, int jumpAlterFrames)
        {
            var currentInput = fd->InputHistory[fd->InputHeadIndex];
            
            if (currentInput.Up)
            {
                RequestState(f, fd, fc, StateID.JUMP_NEUTRAL);
                if (fd->CurrentState == StateID.JUMP_NEUTRAL && fd->StateFrame < jumpAlterFrames)
                {
                    SetCurrentState(f, fd, InputUtils.CheckJumpType(fd, currentInput), fd->StateFrame);
                }
            }
            
            return false;
        }

        private static bool CheckDashState(Frame f, FighterData* fd, FighterConstants fc, int dashAllowFrames)
        {
            if (InputUtils.CheckDash(fd, dashAllowFrames, Direction.Forward))
            {
                Log.Debug("Forward Dash Requested");
                RequestState(f, fd, fc, StateID.DASH_FORWARD);
                return true;
            }
            if (InputUtils.CheckDash(fd, dashAllowFrames, Direction.Backward))
            {
                Log.Debug("Backward Dash Requested");
                RequestState(f, fd, fc, StateID.DASH_BACKWARD);
                return true;
            }
            
            return false;
        }

        private static bool CheckMovementState(Frame f, FighterData* fd, FighterConstants fc)
        {
            var currentInput = fd->InputHistory[fd->InputHeadIndex];
            
            if (currentInput.Down)
            {
                RequestState(f, fd, fc, StateID.CROUCH);
                return true;
            }
            if (InputUtils.IsDirection(fd, currentInput, Direction.Forward))
            {
                RequestState(f, fd, fc, StateID.FORWARD);
                return true;
            }
            if (InputUtils.IsDirection(fd, currentInput, Direction.Backward))
            {
                RequestState(f, fd, fc, StateID.BACKWARD);
                return true;
            }
            return false;
        }

        private static bool RequestState(Frame f, FighterData* fd, FighterConstants fc, StateID stateID)
        {
            if (fd->StateFrame >= fc.States[fd->CurrentState].FrameCount)
            {
                SetCurrentState(f, fd, stateID);
                return true;
            }

            if (fd->CurrentState == stateID)
                return false;
            
            if (fc.States[fd->CurrentState].IsAlwaysCancelable)
            {
                SetCurrentState(f, fd, stateID);
                return true;
            }
            
            return false;
        }

        public static void SetCurrentState(Frame f, FighterData* fd, StateID stateID, int frame = 0)
        {
            fd->CurrentState = stateID;
            fd->StateFrame = frame;

            var registry = f.ResolveDictionary(fd->AttackRegistry);
            registry.Clear();
        }
    }
}
