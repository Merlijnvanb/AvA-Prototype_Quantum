using System.Collections.Generic;
using UnityEngine;

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

        [SerializeField]
        private StateDataContainer stateDataContainer;
        
        public Dictionary<StateID, StateData> States => states;
        private Dictionary<StateID, StateData> states = new Dictionary<StateID, StateData>();

        public void SetupDictionaries()
        {
            foreach (var state in stateDataContainer.States)
                states.TryAdd(state.StateID, state);
        }
    }
}
