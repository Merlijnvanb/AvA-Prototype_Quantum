namespace Quantum
{
    using Photon.Deterministic;
    
    public class FighterConstants : AssetObject
    {
        public int MaxHealth = 1000;

        public FP ForwardWalkSpeed = 3;
        public FP BackWalkSpeed = 2;

        public int DashAllowFrames = 10;
        public int JumpAlterFrames = 4;

        public FP DownwardForce = 13;
        public FP FrictionCoefficient = FP._0_75;

        public FP SideSwitchThreshold = FP._0_10;

        public FPVector2 BaseHurtBoxRectPos;
        public FPVector2 BaseHurtBoxRectWH;
        
        public FPVector2 BasePushBoxRectPos;
        public FPVector2 BasePushBoxRectWH;
        
        //public 

        public void GetState(Frame f)
        {
            Log.LogLevel = LogType.Debug;
            Log.Debug("Retreived state.");
        }
    }
}
