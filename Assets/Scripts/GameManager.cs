using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;

    [SerializeField, Header("VR")] public bool _inVR = false;
    [SerializeField] public bool _vrCapable = false;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindAnyObjectByType<GameManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Debug.Log("GameManagerInstance already exists, destroying object!");
            Destroy(this.gameObject);
        }
    } 
}
