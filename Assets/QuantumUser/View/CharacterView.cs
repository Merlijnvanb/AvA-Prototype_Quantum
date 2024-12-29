namespace Quantum.Ava
{
    using Quantum;
    using UnityEngine;
    using Unity.Cinemachine;

    public class CharacterView : QuantumEntityViewComponent<IQuantumViewContext>
    {
        public Transform Body;
        public CinemachineTargetGroup targetGroup;

        private bool groupAssigned = false;

        void Start()
        {
            targetGroup = FindFirstObjectByType<CinemachineTargetGroup>();
            groupAssigned = true;
        }
        
        public override void OnUpdateView()
        {
            if (!PredictedFrame.TryGet<FighterData>(EntityRef, out var fighterData))
                return;

            var pos = fighterData.Position.ToUnityVector2();
            var rot = fighterData.IsFacingRight ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
            Body.SetPositionAndRotation(new Vector3(pos.x, pos.y, 0), rot);
            
            if (!groupAssigned)
                return;
            
            if (fighterData.FighterID == 1)
                targetGroup.Targets[0].Object = Body;
            else
                targetGroup.Targets[1].Object = Body;
        }
    }
}
