namespace Quantum.Ava
{
    using UnityEngine;
    using Quantum;
    using TMPro;

    public class UIView : QuantumSceneViewComponent
    {
        [Header("Debug UI")]
        public TextMeshProUGUI P1CurrentState;
        public TextMeshProUGUI P1BlockstunText;
        public TextMeshProUGUI P1HitstunText;

        public TextMeshProUGUI P2CurrentState;
        public TextMeshProUGUI P2BlockstunText;
        public TextMeshProUGUI P2HitstunText;

        [Header("Health UI")]
        public TextMeshProUGUI P1HealthText;
        public TextMeshProUGUI P2HealthText;

        [Header("Score UI")]
        public TextMeshProUGUI P1Score;
        public TextMeshProUGUI P2Score;

        void Start()
        {
            QuantumEvent.Subscribe<EventUpdateUI>(this, UpdateUI);
        }

        private void UpdateUI(EventUpdateUI data)
        {
            P1Score.SetText("" + data.F1Score);
            P2Score.SetText("" + data.F2Score);

            if (!PredictedFrame.TryGet<FighterData>(data.Fighter1, out var fd1) ||
                !PredictedFrame.TryGet<FighterData>(data.Fighter2, out var fd2))
                return;
            
            P1CurrentState.SetText("P1 Current State: " + fd1.CurrentState);
            P1BlockstunText.SetText("P1 Blockstun: " + fd1.BlockStun);
            P1HitstunText.SetText("P1 Hitstun: " + fd1.HitStun);

            P2CurrentState.SetText("P2 Current State: " + fd2.CurrentState);
            P2BlockstunText.SetText("P2 Blockstun: " + fd2.BlockStun);
            P2HitstunText.SetText("P2 Hitstun: " + fd2.HitStun);

            P1HealthText.SetText("P1 Health: " + fd1.Health);
            P2HealthText.SetText("P2 Health: " + fd2.Health);
        }
    }
}
