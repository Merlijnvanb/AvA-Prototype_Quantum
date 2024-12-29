using Photon.Client.StructWrapping;
using UnityEngine;
using Quantum;
using Unity.VisualScripting;

public class LocalManager : MonoBehaviour
{
    public static LocalManager Instance { get; private set; }

    [SerializeField] private bool unlockFPS = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        Application.targetFrameRate = unlockFPS ? 0 : 60;
    }
}
