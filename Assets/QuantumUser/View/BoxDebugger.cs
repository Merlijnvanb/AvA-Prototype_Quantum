using Quantum.Collections;

namespace Quantum.Ava
{
    using UnityEngine;
    using Quantum;

    public class BoxDebugger : QuantumSceneViewComponent
    {
        [System.Serializable]
        public struct DebugData
        {
            public bool Pushboxes;
            public bool Hurtboxes;
            public bool Proximity;
            public bool Hitboxes;
        }

        [SerializeField]
        private DebugData debugData;

        private FighterData fd1;
        private FighterData fd2;
        
        void Start()
        {
            QuantumEvent.Subscribe<EventUpdateUI>(this, CheckFighters);
        }

        private void CheckFighters(EventUpdateUI data)
        {
            if (PredictedFrame.TryGet<FighterData>(data.Fighter1, out var fd1)) this.fd1 = fd1;
            if (PredictedFrame.TryGet<FighterData>(data.Fighter2, out var fd2)) this.fd2 = fd2;
        }

        private void LateUpdate()
        {
            UpdateBoxes(fd1);
            UpdateBoxes(fd2);
        }

        private void UpdateBoxes(FighterData fd)
        {
            if (debugData.Pushboxes)
            {
                DrawRect(fd.Pushbox.RectPos.ToUnityVector2(), fd.Pushbox.RectWH.ToUnityVector2(), Color.cyan);
            }

            if (debugData.Hurtboxes)
            {
                if (PredictedFrame.TryResolveList(fd.HurtboxList, out var hurtboxes))
                {
                    for (int i = 0; i < hurtboxes.Count; i++)
                    {
                        var hurtbox = hurtboxes[i];
                        
                        DrawRect(hurtbox.RectPos.ToUnityVector2(), hurtbox.RectWH.ToUnityVector2(), Color.blue);
                    }
                }
            }

            if (PredictedFrame.TryResolveList(fd.HitboxList, out var hitboxes))
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    var hitbox = hitboxes[i];

                    if (hitbox.IsProximity && debugData.Proximity)
                    {
                        DrawRect(hitbox.RectPos.ToUnityVector2(), hitbox.RectWH.ToUnityVector2(), Color.magenta);
                    }

                    if (!hitbox.IsProximity && debugData.Hitboxes)
                    {
                        DrawRect(hitbox.RectPos.ToUnityVector2(), hitbox.RectWH.ToUnityVector2(), Color.red);
                    }
                }
            }
        }

        private void DrawRect(Vector2 pos, Vector2 wh, Color color)
        {
            var min = new Vector2(pos.x - wh.x / 2, pos.y);
            var max = new Vector2(pos.x + wh.x / 2, pos.y + wh.y);
            
            Debug.DrawLine(min, new Vector3(min.x, max.y), color);
            Debug.DrawLine(new Vector3(min.x, max.y), max, color);
            Debug.DrawLine(max, new Vector3(max.x, min.y), color);
            Debug.DrawLine(min, new Vector3(max.x, min.y), color);
        }
    }
}