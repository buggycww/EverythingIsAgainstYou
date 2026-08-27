using UnityEngine;
using System.Collections;

public abstract class BaseEnemy : BaseInteractable
{
    [Header("Enemy Settings")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float chaseRange = 5f;
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float attackCooldown = 2f;

    [Header("References")]
    [SerializeField] protected Transform player;
    [SerializeField] protected string attackAnimationName = "Attack";

    protected Animator animator;
    protected Rigidbody2D rb;
    protected bool canAttack = true;

    protected enum EnemyState { Idle, Chasing, Attacking, Cooldown, Dead }
    protected EnemyState currentState = EnemyState.Idle;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    protected virtual void Start()
    {
        TransitionTo(EnemyState.Idle);
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead) return;

        switch (currentState)
        {
            case EnemyState.Chasing:
                UpdateChasing();
                break;
        }
    }

    #region State Management
    protected virtual void TransitionTo(EnemyState newState)
    {
        if (currentState == EnemyState.Dead && newState != EnemyState.Dead) return;

        OnExitState(currentState);
        EnemyState previous = currentState;
        currentState = newState;
        OnEnterState(newState, previous);
    }

    protected virtual void OnExitState(EnemyState state)
    {
        if (state == EnemyState.Chasing && rb != null)
            rb.linearVelocity = Vector2.zero;
        if (animator != null)
            animator.SetBool("IsMoving", false);
    }

    protected virtual void OnEnterState(EnemyState state, EnemyState previousState)
    {
        switch (state)
        {
            case EnemyState.Idle:
                rb.linearVelocity = Vector2.zero;
                if (animator != null) animator.SetBool("IsMoving", false);
                break;

            case EnemyState.Chasing:
                if (animator != null) animator.SetBool("IsMoving", true);
                break;

            case EnemyState.Attacking:
                canAttack = false;
                rb.linearVelocity = Vector2.zero;
                if (animator != null)
                {
                    animator.SetBool("IsMoving", false);
                    animator.Play(attackAnimationName);
                }
                break;

            case EnemyState.Cooldown:
                if (animator != null) animator.SetBool("IsMoving", false);
                StartCoroutine(AttackCooldown());
                break;
        }
    }
    #endregion

    #region Core Behaviors
    protected virtual void UpdateChasing()
    {
        if (player == null)
        {
            TransitionTo(EnemyState.Idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && canAttack)
        {
            TransitionTo(EnemyState.Attacking);
            return;
        }

        if (distance > chaseRange)
        {
            TransitionTo(EnemyState.Idle);
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        if (animator != null) animator.SetBool("IsMoving", true);
    }

    protected virtual IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;

        if (player != null && currentState != EnemyState.Dead)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            TransitionTo(dist <= chaseRange ? EnemyState.Chasing : EnemyState.Idle);
        }
        else
        {
            TransitionTo(EnemyState.Idle);
        }
    }
    #endregion

    #region Animation Events
    public virtual void OnAttackAnimEvent()
    {
        TransitionTo(EnemyState.Cooldown);
    }

    public virtual void OnDamageFrameAnimEvent()
    {
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRange + 0.5f)
                DamagePlayer();
        }
    }
    #endregion

    #region Damage
    protected virtual void DamagePlayer()
    {
        Debug.Log($"{gameObject.name} damaged player!");
    }
    #endregion

    #region Overrides
    public override void OnInteract(GameObject interactor)
    {
        if (!isShowingText || currentState == EnemyState.Dead) return;
        base.OnInteract(interactor);
        // Override in child to define specific activation
    }

    public override void HandleDialogueComplete() { base.HandleDialogueComplete(); }
    #endregion

    #region Editor
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}