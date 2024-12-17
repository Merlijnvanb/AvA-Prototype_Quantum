namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class FighterSystem : SystemMainThreadFilter<FighterSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            // public Transform3D* Transform;
            public FighterData* FighterData;
        }
        
        public override void Update(Frame f, ref Filter filter)
        {
            UpdateFighters(f, ref filter);
            // UpdateView(f, ref filter);
            
            Log.LogLevel = LogType.Debug;
            Log.Debug(f.ResolveList(filter.FighterData->HurtBoxList).Count);
        }

        private void UpdateFighters(Frame f, ref Filter filter)
        {
            UpdateFacing(f, ref filter);
            
            StateManager.UpdateState(f, ref filter);
            MovementHandler.UpdateMovement(f, ref filter);
            BoxManager.UpdateBoxes(f, ref filter);
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

        // private void UpdateView(Frame f, ref Filter filter)
        // {
        //     var fighterData = filter.FighterData;
        //     
        //     
        //     filter.Transform->Teleport(f, new FPVector3(fighterData->Position.X, fighterData->Position.Y, 0));
        //     
        //     if (fighterData->IsFacingRight)
        //         filter.Transform->Rotation.Y = 0;
        //     else if (!fighterData->IsFacingRight)
        //         filter.Transform->Rotation.Y = 180;
        // }
    }
}

