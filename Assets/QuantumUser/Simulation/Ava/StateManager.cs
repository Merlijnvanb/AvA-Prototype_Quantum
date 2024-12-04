namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class StateManager : SystemMainThreadFilter<StateManager.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public FighterData* FighterData;
        }

        public override void Update(Frame f, ref Filter filter)
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

        private bool CheckAttackState(FighterData* fd, FighterConstants fc, Input* input)
        {
            return false;
        }

        private bool CheckJumpState(FighterData* fd, FighterConstants fc, Input* input)
        {
            return false;
        }

        private bool CheckDashState(FighterData* fd, FighterConstants fc, Input* input)
        {
            return false;
        }

        private bool CheckMovementState(FighterData* fd, FighterConstants fc, Input* input)
        {
            if (input->Down)
            {
                RequestState(fd, fc, StateID.CROUCH);
                return true;
            }
            if (input->Right)
            {
                RequestState(fd, fc, StateID.FORWARD);
                return true;
            }
            if (input->Left)
            {
                RequestState(fd, fc, StateID.BACKWARD);
                return true;
            }
            return false;
        }

        private bool RequestState(FighterData* fd, FighterConstants fc, StateID stateID)
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

        private void SetCurrentState(FighterData* fd, StateID stateID, int frame = 0)
        {
            fd->CurrentState = stateID;
            fd->StateFrame = frame;
        }
    }
    
    
}
