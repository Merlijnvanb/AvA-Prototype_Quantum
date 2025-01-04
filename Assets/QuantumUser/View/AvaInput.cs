using Photon.Deterministic;
using UnityEngine;

namespace Quantum.Ava
{
    public class AvaInput : MonoBehaviour
    {
        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        public void PollInput(CallbackPollInput callback)
        {
            Quantum.Input i = new Quantum.Input();
            
            i.Left = callback.PlayerSlot == 0 ? UnityEngine.Input.GetKey(KeyCode.A) : UnityEngine.Input.GetKey(KeyCode.LeftArrow);
            i.Right = callback.PlayerSlot == 0 ? UnityEngine.Input.GetKey(KeyCode.D) : UnityEngine.Input.GetKey(KeyCode.RightArrow);
            i.Up = callback.PlayerSlot == 0 ? UnityEngine.Input.GetKey(KeyCode.Space) : UnityEngine.Input.GetKey(KeyCode.UpArrow);
            i.Down = callback.PlayerSlot == 0 ? UnityEngine.Input.GetKey(KeyCode.S) : UnityEngine.Input.GetKey(KeyCode.DownArrow);
            i.Light = callback.PlayerSlot == 0 ? UnityEngine.Input.GetKey(KeyCode.T) : UnityEngine.Input.GetKey(KeyCode.Keypad7);
            i.Medium = callback.PlayerSlot == 0 ? UnityEngine.Input.GetKey(KeyCode.Y) : UnityEngine.Input.GetKey(KeyCode.Keypad8);
            i.Heavy = callback.PlayerSlot == 0 ? UnityEngine.Input.GetKey(KeyCode.U) : UnityEngine.Input.GetKey(KeyCode.Keypad9);
            i.Special = callback.PlayerSlot == 0 ? UnityEngine.Input.GetKey(KeyCode.V) : UnityEngine.Input.GetKey(KeyCode.Keypad0);
            
            callback.SetInput(CleanSOCD(i), DeterministicInputFlags.Repeatable);
        }

        private Quantum.Input CleanSOCD(Quantum.Input i)
        {
            if (i.Left && i.Right)
            {
                i.Left = false;
                i.Right = false;
            }

            if (i.Up && i.Down)
            {
                i.Up = false;
                i.Down = false;
            }

            return i;
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
                QuantumRunner.Default.Game.SendCommand(new PauseSimulation());
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.F10))
                QuantumRunner.Default.Game.SendCommand(new AdvanceOneFrame());
        }
    }
}