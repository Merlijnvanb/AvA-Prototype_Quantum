namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class MovementHandler
    {
        public static void UpdateMovement(Frame f, ref FighterSystem.Filter filter)
        {
            var fd = filter.FighterData;
            var constants = f.FindAsset<FighterConstants>(fd->Constants);
            
            var deltaTime = f.DeltaTime;
            var sign = fd->IsFacingRight ? 1 : -1;

            if (fd->CurrentState == StateID.BACKWARD)
                fd->Position.X += -constants.BackWalkSpeed * deltaTime * sign;

            if (fd->CurrentState == StateID.FORWARD)
                fd->Position.X += constants.ForwardWalkSpeed * deltaTime * sign;

            CalculateVelocity(f, fd, constants);
            CalculatePushback(f, fd);

            if (fd->Velocity != FPVector2.Zero)
            {
                fd->Position.X += fd->Velocity.X * deltaTime * sign;
                fd->Position.Y += fd->Velocity.Y * deltaTime;
            }

            if (fd->Pushback != FPVector2.Zero)
            {
                fd->Position.X += fd->Pushback.X * deltaTime * sign;
                fd->Position.Y += fd->Pushback.Y * deltaTime;
            }
            
            if (fd->Position.Y < 0)
                fd->Position.Y = 0;
        }

        private static void CalculateVelocity(Frame f, FighterData* fd, FighterConstants constants)
        {
            if (fd->Position.Y <= 0)
            {
                fd->Velocity.X = 0;
                if (fd->Velocity.Y < 0) fd->Velocity.Y = 0;
            }

            if (constants.States[fd->CurrentState].IsMovementState)
            {
                var movementData = constants.States[fd->CurrentState].GetMovementData(fd->StateFrame);
                if (movementData != null)
                {
                    switch (constants.States[fd->CurrentState].MovementType)
                    {
                        case MovementType.Grounded:
                            fd->Velocity.X = movementData.Velocity.X;
                            fd->Velocity.Y = movementData.Velocity.Y;
                            break;
                        
                        case MovementType.Aerial:
                            fd->Velocity.X += movementData.Velocity.X;
                            fd->Velocity.Y += movementData.Velocity.Y;
                            break;
                    }
                }
            }

            if (fd->Position.Y > 0)
                fd->Velocity.Y -= f.Global->DownwardForce * f.DeltaTime;
        }

        private static void CalculatePushback(Frame f, FighterData* fd)
        {
            fd->PreviousPushback = fd->Pushback;
            if (fd->Pushback.X != 0) fd->Pushback.X -= f.Global->FrictionCoefficient * fd->Pushback.X;
            if (FPMath.Abs(fd->Pushback.X) < FP._0_01) fd->Pushback.X = 0;
            if (fd->Position.Y <= 0) fd->Pushback.Y = 0;
        }
    }
}
