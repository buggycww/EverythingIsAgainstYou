using UnityEngine;
using UnityEngine.Diagnostics;
using Cainos.PixelArtTopDown_Basic;

public class HiddenSpikes : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float cooldown;
    [SerializeField] private float timer = 0;
    [SerializeField] private bool isOnCooldown = false;
    [SerializeField] private bool isSpikesActive = false;
    [SerializeField] private Collider2D spikesCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        spikesCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                timer = 0;
                isOnCooldown = false;
                transform.GetComponent<Collider2D>().enabled = false;
                transform.GetComponent<Collider2D>().enabled = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var playerController = collision.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (!playerController.isDead && !isSpikesActive && !isOnCooldown)
            {
                Debug.Log("SpikeActive");
                isSpikesActive = true;
                animator.Play("SpikesOut");
            }
        }
    }

    public void OnSpikesOutAnimEvent()
    {
        spikesCollider.enabled = true;
    }

    public void OnSpikesInAnimEvent()
    {
        isOnCooldown = true;
        spikesCollider.enabled = false;
        isSpikesActive = false;
        timer = cooldown;
    }
}
