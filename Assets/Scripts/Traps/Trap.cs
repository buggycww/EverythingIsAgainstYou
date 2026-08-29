using UnityEngine;
using UnityEngine.Events;
using Cainos.PixelArtTopDown_Basic;

public class Trap : MonoBehaviour
{
    public UnityEvent OnPlayerKilled;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var playerController = other.GetComponent<PlayerController>();
        if (playerController)
        {
            if (!playerController.isDead)
            {
                OnPlayerKilled?.Invoke();
                playerController.Die();
            }
        }
    }
}
