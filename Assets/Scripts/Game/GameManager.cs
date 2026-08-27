using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private bool canRestart;
    [SerializeField] private TextMeshProUGUI restartText;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (canRestart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
        }
    }

    public void PlayerDead()
    {
        canRestart = true;
        restartText.gameObject.SetActive(true);
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
