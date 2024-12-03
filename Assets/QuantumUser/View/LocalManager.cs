using UnityEngine;

public class LocalManager : MonoBehaviour
{
    public static LocalManager Instance { get; private set; }

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
        
        Application.targetFrameRate = 60;
    }
}
