namespace Quantum
{
    using Photon.Deterministic;
    using System.Collections.Generic;

    public class StateData : AssetObject
    {
        public StateID StateID;
        public bool IsGuardState;
        public bool IsMovementState;
        public GuardType GuardType;
        public MovementType MovementType;
        public int FrameCount;
        public bool IsLoop;
        public int LoopFromFrame;
        public bool IsAlwaysCancelable;
        public HitboxData[] Hitboxes;
        public HurtboxData[] Hurtboxes;
        public PushboxData[] Pushboxes;
        public MovementData[] Movements;
        
        public List<HitboxData> GetHitboxData(int frame)
        {
            var hb = new List<HitboxData>();

            foreach (var data in Hitboxes)
            {
                if (frame >= data.StartEndFrame.X && frame <= data.StartEndFrame.Y)
                    hb.Add(data);
            }
            
            return hb;
        }

        public List<HurtboxData> GetHurtboxData(int frame)
        {
            var hb = new List<HurtboxData>();

            foreach (var data in Hurtboxes)
            {
                if (frame >= data.StartEndFrame.X && frame <= data.StartEndFrame.Y)
                    hb.Add(data);
            }
            
            return hb;
        }

        public PushboxData GetPushBoxData(int frame)
        {
            foreach (var data in Pushboxes)
            {
                if (frame >= data.StartEndFrame.X && frame <= data.StartEndFrame.Y)
                    return data;
            }
            
            return null;
        }

        public MovementData GetMovementData(int frame)
        {
            foreach (var data in Movements)
            {
                if (frame >= data.StartEndFrame.X && frame <= data.StartEndFrame.Y)
                {
                    return data;
                }
            }
            
            return null;
        }
    }
    
    public abstract class FrameDataBase
    {
        public FPVector2 StartEndFrame;
    }

    [System.Serializable]
    public class HitboxData : FrameDataBase
    {
        public FPVector2 RectPos;
        public FPVector2 RectWH;
        public bool IsProximity;
        public AttackProperties AttackProperties;
    }

    [System.Serializable]
    public class HurtboxData : FrameDataBase
    {
        public FPVector2 RectPos;
        public FPVector2 RectWH;

        public bool IsAirborne;
        public bool IsInvulnerable;
    }

    [System.Serializable]
    public class PushboxData : FrameDataBase
    {
        public bool UseBaseRect;
        public FPVector2 RectPos;
        public FPVector2 RectWH;
    }

    [System.Serializable]
    public class MovementData : FrameDataBase
    {
        public FPVector2 Velocity;
    }

    public enum GuardType
    {
        Standing,
        Crouching,
    }

    public enum MovementType
    {
        Grounded,
        Aerial,
    }
}
