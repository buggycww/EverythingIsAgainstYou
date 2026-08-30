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

    [Header("HintSystem")]
    public int currentIndex;
    public int respawnCounter;

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerController>().transform;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SoundManager.Instance.PlayMusic("Game");
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (player != null)
        {
            SetRespawnPoint(player.position);
            Instance.sortingLayer = player.GetComponent<SpriteRenderer>().sortingLayerName;
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

        player = FindAnyObjectByType<PlayerController>().transform;
        incrementCounter();
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
        HintSystem.Instance.SetHint(position);
    }

    public void incrementCounter()
    {
        respawnCounter++;

        HintSystem.Instance.CheckHintDisplay(respawnCounter);
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