using Cainos.PixelArtTopDown_Basic;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlowerMonster : BaseEnemy
{
    [Header("Config")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackKnockback = 200f;
    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private string emergeAnimationName = "Emerge";

    [Header("Sprite Flipping")]
    [SerializeField] private bool flipSprite = true;
    [SerializeField] private float flipThreshold = 0.1f;

    [Header("NPC Targeting")]
    [SerializeField] private float npcDetectionRange = 8f;
    [SerializeField] private LayerMask npcLayerMask;
    [SerializeField] private bool prioritizeNPCs = true;

    private SpriteRenderer spriteRenderer;
    private bool isEmerged = false;
    private bool isFacingRight = true;
    private bool hasAttacked = false;

    private Transform currentTarget;
    private List<NPC> nearbyNPCs = new List<NPC>();
    private float npcSearchTimer = 0f;
    private const float NPC_SEARCH_COOLDOWN = 0.5f;

    [SerializeField] private int yummyDialogueIndex;

    public override void Awake()
    {
        base.Awake();
        attackAnimationName = "Attack";
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            isFacingRight = !spriteRenderer.flipX;

        if (npcLayerMask == 0)
            npcLayerMask = LayerMask.GetMask("NPC");
    }

    protected override void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        if (isEmerged && currentState != EnemyState.Dead && !hasAttacked)
        {
            UpdateTarget();
            UpdateSpriteDirection(currentTarget ?? player);
        }
    }

    #region Interactable
    public override bool IsInteractable() => !isEmerged;

    public override void OnInteract(GameObject interactor)
    {
        if (!isShowingText) return;
        base.OnInteract(interactor);
        Emerge();
    }
    #endregion

    #region Emerge
    private void Emerge()
    {
        isEmerged = false;
        hasAttacked = false;

        UpdateTarget();
        Transform initialTarget = currentTarget ?? player;
        if (initialTarget != null && flipSprite)
        {
            Vector2 dir = (initialTarget.position - transform.position).normalized;
            SetFacingDirection(dir.x > 0);
        }

        if (animator != null)
            animator.Play(emergeAnimationName);

        HideInteractionPrompt();
    }

    public void OnEmergeAnimEvent()
    {
        isEmerged = true;
        UpdateTarget();

        if (currentTarget != null && Vector2.Distance(transform.position, currentTarget.position) <= chaseRange)
            TransitionTo(EnemyState.Chasing);
        else
            TransitionTo(EnemyState.Idle);
    }
    #endregion

    #region Targeting
    private void UpdateTarget()
    {
        if (currentTarget != null && currentTarget != player)//currentTarget.GetComponent<NPC>() != null)
        {
            return;
        }

        npcSearchTimer -= Time.deltaTime;
        if (npcSearchTimer <= 0)
        {
            SearchForNPCs();
            npcSearchTimer = NPC_SEARCH_COOLDOWN;
        }

        Transform newTarget = null;

        if (prioritizeNPCs)
        {
            NPC nearest = GetNearestNPC();
            if (nearest != null)
            {
                nearest.Run();
                newTarget = nearest.transform;
            }
        }

        if (newTarget == null && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= chaseRange) newTarget = player;
        }

        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            if (currentTarget != null && currentState == EnemyState.Idle && isEmerged && !hasAttacked)
                TransitionTo(EnemyState.Chasing);
            else if (currentTarget == null && currentState == EnemyState.Chasing)
                TransitionTo(EnemyState.Idle);
        }
    }

    private void SearchForNPCs()
    {
        nearbyNPCs.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, npcDetectionRange, npcLayerMask);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<NPC>() != null && !hit.CompareTag("Player"))
                nearbyNPCs.Add(hit.GetComponent<NPC>());
        }
    }

    private NPC GetNearestNPC()
    {
        nearbyNPCs.RemoveAll(t => t == null);
        NPC nearest = null;
        float minDist = float.MaxValue;
        foreach (var npc in nearbyNPCs)
        {
            float d = Vector2.Distance(transform.position, npc.transform.position);
            if (d <= chaseRange && d < minDist && !npc.isDead)
            {
                minDist = d;
                nearest = npc;
            }
        }
        return nearest;
    }
    #endregion

    #region Overrides
    protected override void UpdateChasing()
    {
        if (hasAttacked) { TransitionTo(EnemyState.Idle); return; }
        if (!isEmerged) { TransitionTo(EnemyState.Idle); return; }

        Transform target = currentTarget ?? player;
        if (target == null) { TransitionTo(EnemyState.Idle); return; }

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= attackRange && canAttack)
        {
            TransitionTo(EnemyState.Attacking);
            return;
        }

        if (dist > chaseRange)
        {
            UpdateTarget();
            if (currentTarget == null) TransitionTo(EnemyState.Idle);
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        if (animator != null) animator.SetBool("IsMoving", true);
    }

    public override void OnDamageFrameAnimEvent()
    {
        SoundManager.Instance.PlaySFX("FlowerAttack");

        if (currentTarget != null)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist <= attackRange + 0.5f)
                DamagePlayer();
        }
        else if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRange + 0.5f)
                DamagePlayer();
        }
    }

    protected override void DamagePlayer()
    {
        if (hasAttacked) return;

        base.DamagePlayer();

        hasAttacked = true;

        if (currentTarget != null)
        {
            NPC npc = currentTarget.GetComponent<NPC>();
            if (npc != null)
            {
                npc.Die();
                Debug.Log($"FlowerMonster killed NPC {currentTarget.name}");
            }
            else
            {
                DamageTargetPlayer();
            }
        }
        else if (player != null)
        {
            DamageTargetPlayer();
        }

        if (currentTarget != null)
        {
            Rigidbody2D targetRb = currentTarget.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 dir = (currentTarget.position - transform.position).normalized;
                targetRb.AddForce(dir * attackKnockback, ForceMode2D.Impulse);
            }
        }

        if (attackEffectPrefab != null)
            Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);

        StopCompletely();
    }

    private void DamageTargetPlayer()
    {
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
                StartCoroutine(KillPlayerAfterDelay(0.3f));
            }
        }
    }

    private IEnumerator KillPlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.Die();
        }

        DialogueSystem.StartDialogue(dialogues[yummyDialogueIndex]);
    }

    protected override void OnEnterState(EnemyState state, EnemyState previousState)
    {
        base.OnEnterState(state, previousState);

        switch (state)
        {
            case EnemyState.Idle:
                if (isEmerged && animator != null)
                {
                    animator.SetBool("IsMoving", false);
                    animator.Play("Idle");
                }
                break;

            case EnemyState.Chasing:
                if (isEmerged && animator != null)
                    animator.SetBool("IsMoving", true);
                break;

            case EnemyState.Attacking:
                Transform attackTarget = currentTarget ?? player;
                if (attackTarget != null && flipSprite)
                {
                    Vector2 dir = (attackTarget.position - transform.position).normalized;
                    SetFacingDirection(dir.x > 0);
                }
                break;
        }
    }
    #endregion

    #region Stop & Reset
    private void StopCompletely()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        TransitionTo(EnemyState.Idle);
        canAttack = false;
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.Play("Idle");
        }
        ResetFlowerMonster();
    }

    public void ResetFlowerMonster()
    {
        hasAttacked = false;
        isEmerged = false;
        currentTarget = null;
        nearbyNPCs.Clear();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.Play("Hide");
        canAttack = true;
        currentState = EnemyState.Idle;
    }
    #endregion

    #region Sprite Flipping
    private void UpdateSpriteDirection(Transform target)
    {
        if (!flipSprite || spriteRenderer == null || target == null) return;

        Vector2 dir = (target.position - transform.position).normalized;
        float dot = Vector2.Dot(dir, Vector2.right);

        if (Mathf.Abs(dot) > flipThreshold)
        {
            bool faceRight = dot > 0;
            if (faceRight && !isFacingRight)
                FlipToRight();
            else if (!faceRight && isFacingRight)
                FlipToLeft();
        }
    }

    private void FlipToLeft() { isFacingRight = false; spriteRenderer.flipX = true; }
    private void FlipToRight() { isFacingRight = true; spriteRenderer.flipX = false; }
    private void SetFacingDirection(bool faceRight)
    {
        if (!flipSprite || spriteRenderer == null) return;
        if (faceRight) FlipToRight();
        else FlipToLeft();
    }
    #endregion

    #region Editor
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, npcDetectionRange);
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
        }
    }
    #endregion
}