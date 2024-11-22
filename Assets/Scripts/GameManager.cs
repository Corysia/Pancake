using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [ShowOnly, Header("VR")] public bool inVR;
    [ShowOnly] public bool vrCapable;

    public static GameManager instance
    {
        get
        {
            if (_instance == null) _instance = FindAnyObjectByType<GameManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("GameManagerInstance already exists, destroying object!");
            Destroy(gameObject);
        }
    }
}