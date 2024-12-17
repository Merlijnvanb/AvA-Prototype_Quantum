using Photon.Deterministic;
using UnityEngine;

namespace Quantum.Ava
{
    public class AvaGameConfig : AssetObject

    {
        [Header("Ava Game Config")] 
        public FP StageWidth = 10;
        public FP MaxPlayerDistance = 5;

        public int PreRoundTimer = 3 * 60;
        public int RoundTimer = 50 * 60;
        
        public int DashAllowFrames = 10;
        public int JumpAlterFrames = 4;

        public FP DownwardForce = 13;
        public FP FrictionCoefficient = FP._0_33;

        public FP SideSwitchThreshold = FP._0_10;

    }
}
