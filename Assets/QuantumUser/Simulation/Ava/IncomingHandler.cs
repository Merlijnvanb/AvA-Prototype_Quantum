namespace Quantum.Ava
{
    using UnityEngine.Scripting;
    using Photon.Deterministic;
    using Quantum;
    
    [Preserve]
    public unsafe class IncomingHandler
    {
        private enum HitResult
        {
            Hit,
            BlockedStanding,
            BlockedCrouching
        }
        
        public static void HandleIncoming(Frame f, FighterData* fd, AttackProperties attackProperties)
        {
            var hitResult = GetHitResult(f, fd, attackProperties);
            
            if (hitResult == HitResult.Hit)
                ApplyIncomingHit(f, fd, attackProperties);
            else
                ApplyIncomingBlocked(f, fd, attackProperties, hitResult);
        }

        private static void ApplyIncomingHit(Frame f, FighterData* fd, AttackProperties attackProperties)
        {
            if (attackProperties.Launches)
            {
                fd->Velocity = attackProperties.LaunchVelocity;
                StateManager.SetCurrentState(f, fd, StateID.LAUNCHED);
            }
            else
            {
                fd->HitStun = attackProperties.HitstunFrames;
                fd->Pushback = attackProperties.PushBackOnHit;
                StateManager.SetCurrentState(f, fd, StateID.HITSTUN);
            }

            fd->Health -= attackProperties.BaseDamage;
        }

        private static void ApplyIncomingBlocked(Frame f, FighterData* fd, AttackProperties attackProperties,
            HitResult hitResult)
        {
            fd->BlockStun = attackProperties.BlockstunFrames;
            fd->Pushback = attackProperties.PushBackOnBlock;
            
            switch (hitResult)
            {
                case HitResult.BlockedStanding:
                    StateManager.SetCurrentState(f, fd, StateID.BLOCK_STANDING);
                    break;
                
                case HitResult.BlockedCrouching:
                    StateManager.SetCurrentState(f, fd, StateID.BLOCK_CROUCHING);
                    break;
            }
        }

        private static HitResult GetHitResult(Frame f, FighterData* fd, AttackProperties attackProperties)
        {
            var constants = f.FindAsset(fd->Constants);
            var stateData = constants.States[fd->CurrentState];

            if (stateData.IsGuardState)
            {
                if (stateData.GuardType == GuardType.Standing && attackProperties.AttackHeight != AttackHeight.LOW)
                    return HitResult.BlockedStanding;

                if (stateData.GuardType == GuardType.Crouching && attackProperties.AttackHeight != AttackHeight.HIGH)
                    return HitResult.BlockedCrouching;
            }

            return HitResult.Hit;
        }
    }
}