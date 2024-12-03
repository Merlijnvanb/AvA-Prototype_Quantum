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

    }
}
