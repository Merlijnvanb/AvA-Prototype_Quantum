namespace Quantum.Ava
{
    using Photon.Deterministic;
    using Quantum;
    using UnityEngine.Scripting;
    
    [Preserve]
    public unsafe class HitRegSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            if (!f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter1, out var fd1) ||
                !f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter2, out var fd2))
                return;

            var fd1AttackResult = GetResult(f, fd1, fd2);
            var fd2AttackResult = GetResult(f, fd2, fd1);
            ParseResult(f, fd1, fd2, fd1AttackResult);
            ParseResult(f, fd2, fd1, fd2AttackResult);
        }

        private AttackResult GetResult(Frame f, FighterData* attacker, FighterData* attackee)
        {
            var hitboxes = f.ResolveList(attacker->HitboxList);
            var hurtboxes = f.ResolveList(attackee->HurtboxList);

            var result = new AttackResult();
            
            result.IsHit = false;
            result.IsProximity = false;
            result.AttackProperties = default;
            result.HitNum = 0;

            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!HitboxCanHit(f, attacker, hitboxes[i]))
                    continue;

                for (int j = 0; j < hurtboxes.Count; j++)
                {
                    if (hurtboxes[j].IsInvulnerable)
                        continue;
                    
                    if (hitboxes[i].Overlaps(hurtboxes[j]))
                    {
                        if (hitboxes[i].IsProximity)
                        {
                            result.IsProximity = true;
                        }
                        else
                        {
                            result.IsHit = true;
                            result.AttackProperties = f.FindAsset(hitboxes[i].AttackProperties);
                            result.HitNum = hitboxes[i].HitNum;
                        }
                    }
                }
            }
            
            return result;
        }

        private void ParseResult(Frame f, FighterData* attacker, FighterData* attackee, AttackResult result)
        {
            if (result.IsHit)
            {
                var properties = f.FindAsset(result.AttackProperties);
                NotifyHitboxHit(f, attacker, properties.AttackID, result.HitNum);
                IncomingHandler.HandleIncoming(f, attackee, properties);
                f.Global->HitstopFrames = properties.HitstopFrames;
            }
            else if (result.IsProximity)
            {
                attackee->ProximityGuard = true;
            }
        }

        private bool HitboxCanHit(Frame f, FighterData* fd, Hitbox hitbox)
        {
            var registry = f.ResolveDictionary(fd->AttackRegistry);
            
            if (!f.TryFindAsset(hitbox.AttackProperties, out var attackProperties))
                return false;
            
            var attackID = attackProperties.AttackID;

            return !registry.TryGetValue(attackID, out var num) && num == hitbox.HitNum;
        }

        private void NotifyHitboxHit(Frame f, FighterData* fd, AttackID id, int hitNum)
        {
            var registry = f.ResolveDictionary(fd->AttackRegistry);
            registry.Add(id, hitNum);
        }
    }
}