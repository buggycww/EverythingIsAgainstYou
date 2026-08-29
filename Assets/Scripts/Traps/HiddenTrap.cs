using UnityEngine;
using UnityEngine.Diagnostics;
using Cainos.PixelArtTopDown_Basic;

public class HiddenTrap : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float cooldown;
    private float timer = 0;
    private bool isOnCooldown = false;
    private bool isTrapActive = false;
    [SerializeField] private bool canTrapLoop = false;
    [SerializeField] private Collider2D trapCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        trapCollider.enabled = false;
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
            if (!isTrapActive && !isOnCooldown)
            {
                isTrapActive = true;
                animator.Play("TrapOut");
            }
        }
    }

    public void OnTrapOutAnimEvent()
    {
        trapCollider.enabled = true;
    }

    public void OnTrapDoneAnimEvent()
    {
        if (canTrapLoop)
        {
            isOnCooldown = true;
            trapCollider.enabled = false;
            isTrapActive = false;
            timer = cooldown;
        }
    }
}
