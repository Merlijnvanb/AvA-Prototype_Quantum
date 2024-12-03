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
            
            i.Left = UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow);
            i.Right = UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow);
            i.Up = UnityEngine.Input.GetKey(KeyCode.Space) || UnityEngine.Input.GetKey(KeyCode.UpArrow);
            i.Down = UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow);
            i.Light = UnityEngine.Input.GetKey(KeyCode.T) || UnityEngine.Input.GetKey(KeyCode.Keypad7);
            i.Medium = UnityEngine.Input.GetKey(KeyCode.Y) || UnityEngine.Input.GetKey(KeyCode.Keypad8);
            i.Heavy = UnityEngine.Input.GetKey(KeyCode.U) || UnityEngine.Input.GetKey(KeyCode.Keypad9);
            i.Special = UnityEngine.Input.GetKey(KeyCode.V) || UnityEngine.Input.GetKey(KeyCode.Keypad0);
            
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
    }
}