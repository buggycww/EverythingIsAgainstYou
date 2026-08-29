using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HintSystem : MonoBehaviour
{
    public static HintSystem Instance { get; private set; }

    [Header("Hints")]
    [SerializeField] private Transform[] checkpointLocations;
    [SerializeField] private string[] hints;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private GameObject button;

    [Header("Scene Management")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

    public void SetHint(Vector3 position)
    {
        float distance = float.MaxValue;
        int index = 0;

        for (int i = 0; i < checkpointLocations.Length; i++)
        {
            if (Vector3.Distance(position, checkpointLocations[i].position) < distance)
            {
                distance = Vector3.Distance(position, checkpointLocations[i].position);
                index = i;
            }
        }

        hintText.text = hints[index];
        if (index != RespawnManager.Instance.currentIndex)
        {
            RespawnManager.Instance.respawnCounter = 0;
            button.SetActive(false);
        }
        RespawnManager.Instance.currentIndex = index;
    }

    public void CheckHintDisplay(int count)
    {
        if (count > 5)
        {
            button.SetActive(true);
        }
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
