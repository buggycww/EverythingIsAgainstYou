using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string sortingLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            RespawnManager.Instance.RespawnPosition = transform.position;
            RespawnManager.Instance.sortingLayer = sortingLayer;
        }
    }
}
