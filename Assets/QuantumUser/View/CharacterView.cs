namespace Quantum.Ava
{
    using Quantum;
    using UnityEngine;

    public class CharacterView : QuantumEntityViewComponent<IQuantumViewContext>
    {
        public Transform Body;

        public override void OnUpdateView()
        {
            var fighterData = PredictedFrame.Get<FighterData>(EntityRef);

            var pos = fighterData.Position.ToUnityVector2();
            var rot = fighterData.IsFacingRight ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
            Body.SetPositionAndRotation(new Vector3(pos.x, pos.y, 0), rot);
        }
    }
}
