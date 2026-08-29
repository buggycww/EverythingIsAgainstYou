using Cainos.PixelArtTopDown_Basic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour
{
    private Transform destination;
    [SerializeField] private bool isExitPortal;
    [SerializeField] public float distance = 0.3f;

    private bool hasTeleported;
    public UnityEvent FirstTeleport;

    private void Start()
    {
        if (isExitPortal)
        {
            destination = GameObject.FindGameObjectWithTag("EntrancePortal").transform;
        }
        else
        {
            destination = GameObject.FindGameObjectWithTag("ExitPortal").transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Vector2.Distance(transform.position, collision.transform.position) > distance &&
            collision.GetComponent<PlayerController>())
        {
            if (!hasTeleported)
            {
                FirstTeleport?.Invoke();
            }

            hasTeleported = true;
            collision.transform.position = new Vector2(destination.position.x, destination.position.y);
        }
    }

    public void EnablePortal()
    {
        if (transform.GetComponent<Collider2D>().enabled == false)
        {
            transform.GetComponent<Collider2D>().enabled = true;
            transform.GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
