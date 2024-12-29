namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class BoxManager
    {
        public static void UpdateBoxes(Frame f, ref FighterSystem.Filter filter)
        {
            var fd = filter.FighterData;
            var constants = f.FindAsset<FighterConstants>(fd->Constants);

            var hitboxes = f.ResolveList(fd->HitboxList);
            var hurtboxes = f.ResolveList(fd->HurtboxList);
            
            hitboxes.Clear();
            hurtboxes.Clear();
            

            foreach (var hitboxData in constants.States[fd->CurrentState].GetHitboxData(fd->StateFrame))
            {
                var hitBox = new Hitbox
                {
                    RectPos = ConvertRectPos(hitboxData.RectPos, fd),
                    RectWH = hitboxData.RectWH,
                    IsProximity = hitboxData.IsProximity,
                    AttackProperties = hitboxData.AttackProperties
                };
                
                hitboxes.Add(hitBox);
            }
            

            foreach (var hurtboxData in constants.States[fd->CurrentState].GetHurtboxData(fd->StateFrame))
            {
                var hurtPos = hurtboxData.UseBaseRect ? constants.BaseHurtBoxRectPos : hurtboxData.RectPos;
                var hurtWH = hurtboxData.UseBaseRect ? constants.BaseHurtBoxRectWH : hurtboxData.RectWH;
                
                var hurtBox = new Hurtbox
                {
                    RectPos = ConvertRectPos(hurtPos, fd),
                    RectWH = hurtWH,
                    IsAirborne = hurtboxData.IsAirborne,
                    IsInvulnerable = hurtboxData.IsInvulnerable,
                };
                
                hurtboxes.Add(hurtBox);
            }
            

            var pushboxData = constants.States[fd->CurrentState].GetPushboxData(fd->StateFrame);
            
            var pushPos = pushboxData.UseBaseRect ? constants.BasePushBoxRectPos : pushboxData.RectPos;
            var pushWH = pushboxData.UseBaseRect ? constants.BasePushBoxRectWH : pushboxData.RectWH;
            
            var pushbox = new Pushbox
            {
                RectPos = ConvertRectPos(pushPos, fd),
                RectWH = pushWH,
            };
            
            fd->Pushbox = pushbox;
        }

        private static FPVector2 ConvertRectPos(FPVector2 rectPos, FighterData* fd)
        {
            var sign = fd->IsFacingRight ? 1 : -1;
            return new FPVector2(fd->Position.X + rectPos.X * sign, fd->Position.Y + rectPos.Y);
        }
    }
}
