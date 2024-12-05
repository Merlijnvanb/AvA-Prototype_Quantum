namespace Quantum.Ava
{
    using Photon.Deterministic;
    using Collections;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class BoxManager
    {
        public static void UpdateBoxes(Frame f, ref FighterSystem.Filter filter)
        {
            var fd = filter.FighterData;
            var constants = f.FindAsset<FighterConstants>(fd->Constants);

            var hitBoxes = f.ResolveList(fd->HitBoxList);
            var hurtBoxes = f.ResolveList(fd->HurtBoxList);
            
            hitBoxes.Clear();
            hurtBoxes.Clear();

            foreach (var hitboxData in constants.States[fd->CurrentState].GetHitboxData(fd->StateFrame))
            {
                var hitBox = new HitBox
                {
                    RectPos = ConvertRectPos(hitboxData.RectPos, fd),
                    RectWH = hitboxData.RectWH,
                    IsProximity = hitboxData.IsProximity,
                    AttackProperties = hitboxData.IsProximity ?
                        null : hitboxData.AttackProperties
                };
                
                hitBoxes.Add(hitBox);
            }

            foreach (var hurtBoxData in constants.States[fd->CurrentState].GetHurtboxData(fd->StateFrame))
            {
                var hurtBox = new HurtBox
                {
                    RectPos = ConvertRectPos(hurtBoxData.RectPos, fd),
                    RectWH = hurtBoxData.RectWH,
                    IsAirborne = hurtBoxData.IsAirborne,
                    IsInvulnerable = hurtBoxData.IsInvulnerable,
                };
                
                hurtBoxes.Add(hurtBox);
            }

            var pushboxData = constants.States[fd->CurrentState].GetPushBoxData(fd->StateFrame);
            var pushbox = new PushBox
            {
                RectPos = ConvertRectPos(pushboxData.RectPos, fd),
                RectWH = pushboxData.RectWH,
            };
            
            fd->PushBox = pushbox;
        }

        private static FPVector2 ConvertRectPos(FPVector2 rectPos, FighterData* fd)
        {
            var sign = fd->IsFacingRight ? -1 : 1;
            return new FPVector2(fd->Position.X + rectPos.X * sign, fd->Position.Y + rectPos.Y);
        }
    }
}
