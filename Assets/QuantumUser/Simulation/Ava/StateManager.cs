namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class StateManager
    {
        public static void UpdateState(Frame f, ref FighterSystem.Filter filter)
        {
            Input* input = default;
            if (f.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
            {
                input = f.GetPlayerInput(playerLink->PlayerRef);

                if (!f.Global->ParseInputs)
                {
                    input->Up = false;
                    input->Down = false;
                    input->Left = false;
                    input->Right = false;
                    input->Light = false;
                    input->Medium = false;
                    input->Heavy = false;
                    input->Special = false;
                }
            }
            var fData = filter.FighterData;
            var fConstants = f.FindAsset(fData->Constants);
            //fConstants.SetupDictionaries();
            
            if (CheckAttackState(fData, fConstants, input)) return;
            if (CheckJumpState(fData, fConstants, input)) return;
            if (CheckDashState(fData, fConstants, input)) return;
            if (CheckMovementState(fData, fConstants, input)) return;

            RequestState(fData, fConstants, StateID.STAND);
        }

        private static bool CheckAttackState(FighterData* fd, FighterConstants fc, Input* input)
        {
            return false;
        }

        private static bool CheckJumpState(FighterData* fd, FighterConstants fc, Input* input)
        {
            return false;
        }

        private static bool CheckDashState(FighterData* fd, FighterConstants fc, Input* input)
        {
            return false;
        }

        private static bool CheckMovementState(FighterData* fd, FighterConstants fc, Input* input)
        {
            if (input->Down)
            {
                RequestState(fd, fc, StateID.CROUCH);
                return true;
            }
            if (InputUtils.IsForward(input, fd))
            {
                RequestState(fd, fc, StateID.FORWARD);
                return true;
            }
            if (InputUtils.IsBackward(input, fd))
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
