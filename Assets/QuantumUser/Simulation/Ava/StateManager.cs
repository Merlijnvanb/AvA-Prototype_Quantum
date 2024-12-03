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

            if (CheckAttackState(input)) return;
            if (CheckJumpState(input)) return;
            if (CheckDashState(input)) return;
            if (CheckMovementState(input)) return;
        }

        private bool CheckAttackState(Input* input)
        {
            return false;
        }

        private bool CheckJumpState(Input* input)
        {
            return false;
        }

        private bool CheckDashState(Input* input)
        {
            return false;
        }

        private bool CheckMovementState(Input* input)
        {
            return false;
        }

        private bool RequestState(FighterData* fd, StateID stateID)
        {
            if (fd->StateFrame >= fd->StateDuration)
            {
                SetCurrentState(fd, stateID);
                return true;
            }

            if (fd->CurrentState == stateID)
                return false;
            
            // if isAlwaysCancelable request state and return true
            
            return false;
        }

        private void SetCurrentState(FighterData* fd, StateID stateID, int frame = 0)
        {
            fd->CurrentState = stateID;
            fd->StateFrame = frame;
        }
    }
    
    
}
