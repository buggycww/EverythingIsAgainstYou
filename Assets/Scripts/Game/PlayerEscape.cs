using System.Collections;
using UnityEngine;

public class PlayerEscape : MonoBehaviour
{
    [SerializeField] private GameObject EndScreen;

    public void OnPlayerEscaped()
    {
        EndScreen.SetActive(true);
        StartCoroutine(LoadMainMenu());
    }

    private IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(3f);
        GameManager.instance.MainMenu();
    }
}
