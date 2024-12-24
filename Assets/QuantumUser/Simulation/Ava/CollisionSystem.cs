namespace Quantum.Ava
{
    using Photon.Deterministic;
    using Quantum;
    using UnityEngine.Scripting;
    
    [Preserve]
    public unsafe class CollisionSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            var fighters = new FighterData*[2];
            
            if (f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter1, out var fighter1)) fighters[0] = fighter1;
            if (f.Unsafe.TryGetPointer<FighterData>(f.Global->Fighter2, out var fighter2)) fighters[1] = fighter2;
            
            AgainstWall(f, fighters);
            AgainstFighter(f, fighters);
            FixWallClip(f, fighters);
            AgainstDistance(f, fighters);
        }

        private void AgainstWall(Frame f, FighterData*[] fighters)
        {
            var leftWall = f.Global->StageWidth * -1 / 2;
            var rightWall = f.Global->StageWidth / 2;

            for (int i = 0; i < 2; i++)
            {
                if (fighters[i] == null)
                    continue;

                if (fighters[i]->Pushbox.XMin < leftWall)
                {
                    ApplyPosition(f, fighters[i], new FPVector2(leftWall - fighters[i]->Pushbox.XMin, 0));
                }
                else if (fighters[i]->Pushbox.XMax > rightWall)
                {
                    ApplyPosition(f, fighters[i], new FPVector2(rightWall - fighters[i]->Pushbox.XMax, 0));
                }
            }
        }

        private void AgainstFighter(Frame f, FighterData*[] fighters)
        {
            if (fighters[0] == null || fighters[1] == null)
                return;
            
            var box1 = fighters[0]->Pushbox;
            var box2 = fighters[1]->Pushbox;

            if (box1.Overlaps(box2))
            {
                if (fighters[0]->Position.X < fighters[1]->Position.X)
                {
                    ApplyPosition(f, fighters[0], new FPVector2((box1.XMax - box2.XMin) * -1 / 2, 0));
                    ApplyPosition(f, fighters[1], new FPVector2((box1.XMax - box2.XMin) * 1 / 2, 0));
                }
                else if (fighters[0]->Position.X > fighters[1]->Position.X)
                {
                    ApplyPosition(f, fighters[0], new FPVector2((box2.XMax - box1.XMin) * 1 / 2, 0));
                    ApplyPosition(f, fighters[1], new FPVector2((box2.XMax - box1.XMin) * -1 / 2, 0));
                }
            }
        }

        private void FixWallClip(Frame f, FighterData*[] fighters)
        {
            var leftWall = f.Global->StageWidth * -1 / 2;
            var rightWall = f.Global->StageWidth / 2;

            for (int i = 0; i < 2; i++)
            {
                if (fighters[i] == null)
                    continue;

                if (fighters[i]->Pushbox.XMin < leftWall)
                {
                    var adjustment = leftWall - fighters[i]->Pushbox.XMin;
                    ApplyPosition(f, fighters[0], new FPVector2(adjustment, 0));
                    ApplyPosition(f, fighters[1], new FPVector2(adjustment, 0));
                }
                if (fighters[i]->Pushbox.XMax > rightWall)
                {
                    var adjustment = rightWall - fighters[i]->Pushbox.XMax;
                    ApplyPosition(f, fighters[0], new FPVector2(adjustment, 0));
                    ApplyPosition(f, fighters[1], new FPVector2(adjustment, 0));
                }
            }
        }

        private void AgainstDistance(Frame f, FighterData*[] fighters)
        {
            if (fighters[0] == null || fighters[1] == null)
                return;
            
            var box1 = fighters[0]->Pushbox;
            var box2 = fighters[1]->Pushbox;
            var maxDist = f.Global->MaxFighterDistance;

            if (box2.XMax - box1.XMin > maxDist)
            {
                ApplyPosition(f, fighters[0], new FPVector2((box2.XMax - box1.XMin - maxDist) / 2, 0));
                ApplyPosition(f, fighters[1], new FPVector2((box2.XMax - box1.XMin - maxDist) * -1 / 2, 0));
            }
            else if (box1.XMax - box2.XMin > maxDist)
            {
                ApplyPosition(f, fighters[0], new FPVector2((box1.XMax - box2.XMin - maxDist) * -1 / 2, 0));
                ApplyPosition(f, fighters[1], new FPVector2((box1.XMax - box2.XMin - maxDist) / 2, 0));
            }
        }

        private void ApplyPosition(Frame f, FighterData* fd, FPVector2 pos)
        {
            fd->Position.X += pos.X;
            fd->Position.Y += pos.Y;

            var hitboxes = f.ResolveList(fd->HitboxList);
            var hurtboxes = f.ResolveList(fd->HurtboxList);

            for (int i = 0; i < hitboxes.Count; i++)
            {
                var hitbox = hitboxes[i];
                
                hitbox.RectPos.X += pos.X;
                hitbox.RectPos.Y += pos.Y;
            }

            for (int i = 0; i < hurtboxes.Count; i++)
            {
                var hurtbox = hurtboxes[i];
                
                hurtbox.RectPos.X += pos.X;
                hurtbox.RectPos.Y += pos.Y;
            }

            fd->Pushbox.RectPos.X += pos.X;
            fd->Pushbox.RectPos.Y += pos.Y;
        }
    }
}