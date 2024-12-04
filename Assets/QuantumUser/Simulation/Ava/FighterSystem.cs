namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class FighterSystem : SystemMainThreadFilter<FighterSystem.Filter>
    {
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
            
            UpdateFighters(f, ref filter, input);
            
            Log.LogLevel = LogType.Debug;
            Log.Debug(filter.FighterData->CurrentState.ToString());
        }

        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public FighterData* FighterData;
        }

        private void UpdateFighters(Frame f, ref Filter filter, Input* input)
        {
            Log.LogLevel = LogType.Debug;
            
            
            UpdateMovement(f, ref filter, input);
        }

        private void UpdateState()
        {
            
        }

        private void UpdateMovement(Frame f, ref Filter filter, Input* input)
        {
            var deltaTime = f.DeltaTime;
            
            var fighterData = filter.FighterData;
            var constants = f.FindAsset(fighterData->Constants);

            FPVector3 position = filter.Transform->Position;

            if (fighterData->CurrentState == StateID.BACKWARD)
                position.X += -constants.BackWalkSpeed * deltaTime;

            if (fighterData->CurrentState == StateID.FORWARD)
                position.X += constants.ForwardWalkSpeed * deltaTime;
            
            filter.Transform->Teleport(f, position);
        }
    }
}

