using Cainos.PixelArtTopDown_Basic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Player Respawn Settings")]
    public Vector3 RespawnPosition;
    public string sortingLayer;
    [SerializeField] private Transform player;

    [Header("Scene Management")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (player != null)
            {
                RespawnPosition = player.position;
                Instance.sortingLayer = player.GetComponent<SpriteRenderer>().sortingLayerName;
            }
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == mainMenuSceneIndex)
        {
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        // Clear the static instance
        if (Instance == this)
        {
            Instance = null;
        }

        Destroy(gameObject);
    }

    public void SetRespawnPoint(Vector3 position)
    {
        RespawnPosition = position;
        Debug.Log($"Respawn point set to: {position}");
    }


    private void OnDestroy()
    {
        // Clean up when destroyed
        if (Instance == this)
        {
            Instance = null;
        }
    }
}