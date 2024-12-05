namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class MovementHandler
    {
        public static void UpdateMovement(Frame f, ref FighterSystem.Filter filter)
        {
            var fighterData = filter.FighterData;
            var constants = f.FindAsset<FighterConstants>(fighterData->Constants);
            
            var deltaTime = f.DeltaTime;
            var sign = fighterData->IsFacingRight ? 1 : -1;

            if (fighterData->CurrentState == StateID.BACKWARD)
                fighterData->Position.X += -constants.BackWalkSpeed * deltaTime * sign;

            if (fighterData->CurrentState == StateID.FORWARD)
                fighterData->Position.X += constants.ForwardWalkSpeed * deltaTime * sign;
            
            filter.Transform->Teleport(f, new FPVector3(fighterData->Position.X, fighterData->Position.Y, 0));
            
            if (fighterData->IsFacingRight)
                filter.Transform->Rotation.Y = 0;
            else if (!fighterData->IsFacingRight)
                filter.Transform->Rotation.Y = 180;
        }
    }
}
