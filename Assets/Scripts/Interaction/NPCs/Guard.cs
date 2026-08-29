using Cainos.PixelArtTopDown_Basic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    private Vector2 targetPosition;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isRunning)
        {
            if (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.deltaTime);
                rb.MovePosition(newPos);
            }
            else
            {
                isRunning = false;
                rb.position = targetPosition;
            }
        }
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

        SoundManager.Instance.PlaySFX("GuardAttack");

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
        targetPosition = new Vector2(escapePosition.position.x, escapePosition.position.y);
    }

    public override void Die()
    {
        isDead = true;
        base.Die();
        animator.Play("Death");
        transform.GetComponent<BoxCollider2D>().enabled = false;
    }

    public override void HandleDialogueComplete()
    {
        base.HandleDialogueComplete();  
    }
}
