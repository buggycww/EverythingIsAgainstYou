using Cainos.PixelArtTopDown_Basic;
using UnityEngine;
using UnityEngine.Events;

public class Gate : MonoBehaviour
{
    [SerializeField] private Collider2D gateCollider;
    [SerializeField] private Sprite openedSprite;
    private bool isLocked = true;

    public UnityEvent OnUnlocked;

    private void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLocked)
        {
            return;
        }

        if (collision.GetComponent<PlayerController>() != null)
        {
            OnUnlocked?.Invoke();
            Open();
        }
    }

    public void Open()
    {
        SoundManager.Instance.PlaySFX("DoorOpen");
        isLocked = false;
        transform.GetComponent<SpriteRenderer>().sprite = openedSprite;
        gateCollider.enabled = false;
    }
}
