namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public enum AttackHeight
    {
        LOW,
        MID,
        HIGH,
    }
    
    public class AttackProperties : AssetObject
    {
        [Header("Damage")]
        public int BaseDamage;

        [Header("FrameData")]
        public int HitstunFrames;
        public int BlockstunFrames;

        [Header("Pushback")]
        public FPVector2 PushBackOnHit;
        public FPVector2 PushBackOnBlock;

        [Header("Attack Height")]
        public AttackHeight AttackHeight;

        [Header("Launch")]
        public bool Launches;
        public FPVector2 LaunchVelocity;
    }
}
