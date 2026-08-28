using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

public class WindClearTrigger : MonoBehaviour
{
    [SerializeField] private WickedWind wickedWind;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            wickedWind.PlayerClearedWind();
        }
    }
}
