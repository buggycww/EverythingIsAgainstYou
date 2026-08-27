using Cainos.PixelArtTopDown_Basic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Guard : NPC
{
    [SerializeField] private float knockbackForce = 100f;

    private Animator animator;
    [SerializeField] private int mockPlayerDialogueIndex;
    [SerializeField] private int screamDialogueIndex;

    [SerializeField] private Transform escapePosition;
    private bool isRunning;
    private Rigidbody2D rb;
    private Coroutine moveCoroutine;
    [SerializeField] private float speed = 10f;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnInteract(GameObject interactor)
    {
        if (!isShowingText)
            return;

        base.OnInteract(interactor);

        animator.Play("Attack2");

        var playerController = interactor.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Stop();
        }

        StartCoroutine(KillPlayerAfterDelay(playerController, 0.3f, interactor));
    }

    private IEnumerator KillPlayerAfterDelay(PlayerController controller, float delay, GameObject interactor)
    {
        yield return new WaitForSeconds(delay);

        Vector2 direction = (interactor.transform.position - transform.position).normalized;
        Rigidbody2D playerRb = interactor.GetComponent<Rigidbody2D>();

        Vector2 force = new Vector2(0.5f * knockbackForce, -1 * knockbackForce);
        playerRb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(delay);
        if (controller != null)
        {
            controller.enabled = true;
        }

        controller.Die();
        DialogueSystem.StartDialogue(dialogues[mockPlayerDialogueIndex]);
    }

    public override void Run()
    {
        if (isRunning)
            return;

        isRunning = true;
        DialogueSystem.StartDialogue(dialogues[screamDialogueIndex]);
        animator.Play("Walk");
        moveCoroutine = StartCoroutine(MoveToPosition(new Vector2(escapePosition.position.x, escapePosition.position.y)));
    }

    private IEnumerator MoveToPosition(Vector2 target)
    {
        float stopDistance = 0.1f;

        while (Vector2.Distance(transform.position, target) > stopDistance)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.deltaTime);
            rb.MovePosition(newPos);
            yield return null;
        }

        rb.position = target;
        animator.Play("Idle");
    }

    public override void Die()
    {
        isDead = true;
        base.Die();
        StopCoroutine(moveCoroutine);
        animator.Play("Death");
        transform.GetComponent<BoxCollider2D>().enabled = false;
    }

    public override void HandleDialogueComplete()
    {
        base.HandleDialogueComplete();  
    }
}
