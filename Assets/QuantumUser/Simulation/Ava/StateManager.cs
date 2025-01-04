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
            
            //Log.Debug(fData->ProximityGuard);

            if (CheckAttackState(f, fData, fConstants) ||
                CheckJumpState(f, fData, fConstants, f.Global->JumpAlterFrames) ||
                CheckDashState(f, fData, fConstants, f.Global->DashAllowFrames) ||
                CheckMovementState(f, fData, fConstants))
            {
                fData->ProximityGuard = false;
                return;
            }

            RequestState(f, fData, fConstants, StateID.STAND);
            fData->ProximityGuard = false;
        }

        private static bool CheckAttackState(Frame f, FighterData* fd, FighterConstants fc)
        {
            var currentInput = fd->InputHistory[fd->InputHeadIndex];
            
            if (currentInput.Medium.WasPressed)
            {
                if (currentInput.Down)
                    RequestState(f, fd, fc, StateID.CROUCH_MEDIUM);
                else if (InputUtils.IsDirection(fd, currentInput, Direction.Forward))
                    RequestState(f, fd, fc, StateID.FORWARD_MEDIUM);
                else
                    RequestState(f, fd, fc, StateID.STAND_MEDIUM);

                return true;
            }

            if (currentInput.Light.WasPressed)
            {
                if (currentInput.Down)
                    RequestState(f, fd, fc, StateID.CROUCH_LIGHT);
                else
                    RequestState(f, fd, fc, StateID.STAND_LIGHT);

                return true;
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
                    SetCurrentState(f, fd, InputUtils.CheckJumpType(fd, currentInput), fd->StateFrame);
                return true;
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
                if (InputUtils.IsDirection(fd, currentInput, Direction.Backward))
                {
                    if (fd->ProximityGuard)
                        RequestState(f, fd, fc, StateID.PROXIMITY_CROUCHING);
                    else 
                        RequestState(f, fd, fc, StateID.CROUCH_BACK);
                    return true;
                }
                
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
                if (fd->ProximityGuard)
                    RequestState(f, fd, fc, StateID.PROXIMITY_STANDING);
                else
                    RequestState(f, fd, fc, StateID.BACKWARD);
                return true;
            }
            
            return false;
        }

        private static bool RequestState(Frame f, FighterData* fd, FighterConstants fc, StateID stateID)
        {
            if (fd->StateFrame > fc.States[fd->CurrentState].FrameCount)
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

        public static void SetCurrentState(Frame f, FighterData* fd, StateID stateID, int frame = 1)
        {
            fd->CurrentState = stateID;
            fd->StateFrame = frame;

            if (f.TryResolveDictionary(fd->AttackRegistry, out var registry))
                registry.Clear();
        }
    }
}
