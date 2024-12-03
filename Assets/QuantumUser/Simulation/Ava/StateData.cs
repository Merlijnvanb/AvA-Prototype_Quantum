namespace Quantum
{
    using Photon.Deterministic;

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
        public StatusData[] Statuses;
        public HitboxData[] Hitboxes;
        public HurtboxData[] Hurtboxes;
        public PushboxData[] Pushboxes;
        public MovementData[] Movements;
        public bool IsAlwaysCancelable;
    }
    
    public abstract class FrameDataBase
    {
        public FPVector2 StartEndFrame;
    }

    [System.Serializable]
    public class StatusData : FrameDataBase
    {
        public bool IsAirborne;
        public bool IsInvulnerable;
    }

    [System.Serializable]
    public class HitboxData : FrameDataBase
    {
        public FPVector2 RectPos;
        public FPVector2 RectWH;
        public AttackID AttackID;
        public int HitNum;
        public bool Proximity;
    }

    [System.Serializable]
    public class HurtboxData : FrameDataBase
    {
        public bool UseBaseRect;
        public FPVector2 RectPos;
        public FPVector2 RectWH;
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
