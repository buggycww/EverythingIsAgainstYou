using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private bool canRestart;
    [SerializeField] private TextMeshProUGUI restartText;
    [SerializeField] private Transform player;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    public void PlayerDead()
    {
        canRestart = true;
        restartText.gameObject.SetActive(true);
    }

    private void Restart()
    {
        RespawnManager.Instance.RespawnPosition = player.transform.position;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
