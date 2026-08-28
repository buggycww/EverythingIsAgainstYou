using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }
    public Vector3 RespawnPosition;
    [SerializeField] private Transform player;

    private void Awake()
    {
        // If there's no Instance, this is the first one. Keep it.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // SURVIVES scene reloads!
            RespawnPosition = player.position;
        }
        // CRUCIAL FIX: If an Instance already exists, destroy this NEW copy.
        else
        {
            Destroy(gameObject);
        }
    }
}