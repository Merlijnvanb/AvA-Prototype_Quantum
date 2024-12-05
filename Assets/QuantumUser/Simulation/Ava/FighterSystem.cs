namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class FighterSystem : SystemMainThreadFilter<FighterSystem.Filter>
    {
        public override void Update(Frame f, ref Filter filter)
        {
            UpdateFighters(f, ref filter);
            
            Log.LogLevel = LogType.Debug;
            Log.Debug(filter.FighterData->CurrentState.ToString());
        }

        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public FighterData* FighterData;
        }

        private void UpdateFighters(Frame f, ref Filter filter)
        {
            UpdateFacing(f, ref filter);
            
            StateManager.UpdateState(f, ref filter);
            UpdateMovement(f, ref filter);
        }

        private void UpdateFacing(Frame f, ref Filter filter)
        {
            var fd = filter.FighterData;
            var constants = f.FindAsset<FighterConstants>(fd->Constants);

            if (fd->requestedSideSwitch != 0 && (fd->StateFrame > constants.States[fd->CurrentState].FrameCount ||
                                                 constants.States[fd->CurrentState].IsAlwaysCancelable))
            {
                fd->IsFacingRight = fd->requestedSideSwitch == 1;
                fd->requestedSideSwitch = 0;
            }
        }

        private void UpdateMovement(Frame f, ref Filter filter)
        {
            var deltaTime = f.DeltaTime;
            
            var fighterData = filter.FighterData;
            var constants = f.FindAsset<FighterConstants>(fighterData->Constants);

            if (fighterData->CurrentState == StateID.BACKWARD)
                fighterData->Position.X += -constants.BackWalkSpeed * deltaTime;

            if (fighterData->CurrentState == StateID.FORWARD)
                fighterData->Position.X += constants.ForwardWalkSpeed * deltaTime;
            
            filter.Transform->Teleport(f, new FPVector3(fighterData->Position.X, fighterData->Position.Y, 0));
            
            if (fighterData->IsFacingRight)
                filter.Transform->Rotation.Y = 0;
            else if (!fighterData->IsFacingRight)
                filter.Transform->Rotation.Y = 180;
        }
    }
}

