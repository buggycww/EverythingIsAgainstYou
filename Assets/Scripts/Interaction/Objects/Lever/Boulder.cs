using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    [SerializeField] private GameObject boulderCollider;
    [SerializeField] private string boulderCollisionTag;
    private bool canKillPlayer = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var playerController = collision.GetComponent<PlayerController>();
        if (playerController != null && canKillPlayer)
        {
            SoundManager.Instance.PlaySFX("Boulder");
            playerController.Die();
            boulderCollider.SetActive(true);
        }
        else if (collision.CompareTag(boulderCollisionTag))
        {
            boulderCollider.SetActive(true);
            canKillPlayer = false;
        }
    }
}
