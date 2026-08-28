using UnityEngine;
using UnityEngine.Events;
using Cainos.PixelArtTopDown_Basic;

public class Spikes : MonoBehaviour
{
    public UnityEvent OnPlayerKilled;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Spikes trigger");
        var playerController = other.GetComponent<PlayerController>();
        if (playerController)
        {
            Debug.Log("KillPlayer");
            playerController.Die();
        }
    }
}
