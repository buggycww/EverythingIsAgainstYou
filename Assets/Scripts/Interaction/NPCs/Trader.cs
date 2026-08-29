using Cainos.PixelArtTopDown_Basic;
using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Trader : NPC
{
    [SerializeField] private float knockbackForce = 100f;

    private Animator animator;
    [SerializeField] private int giftDialogueIndex;
    [SerializeField] private int killPlayerDialogueIndex;

    private PlayerController playerController;
    private bool isInteracting = false;
    private bool isPlayerCursed = false;
    [SerializeField] private Item cursedKey;
    [SerializeField] private Item normalKey;
    [SerializeField] private InventoryUI inventoryUI;
    private bool isPlayerBehind = false;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnItemPopUpClosed += CursePlayer;
        }
        playerController = FindAnyObjectByType<PlayerController>();
    }

    private void OnDestroy()
    {
        // CRITICAL: Unsubscribe from all events to prevent calling destroyed object
        if (inventoryUI != null)
        {
            inventoryUI.OnItemPopUpClosed -= CursePlayer;
        }

        DialogueSystem.OnOption1Clicked -= GivePlayerKey;
        DialogueSystem.OnOption2Clicked -= KillPlayer;

        // Clear references
        animator = null;
        playerController = null;
        inventoryUI = null;
    }

    private void OnDisable()
    {
        // Also unsubscribe when disabled to be safe
        DialogueSystem.OnOption1Clicked -= GivePlayerKey;
        DialogueSystem.OnOption2Clicked -= KillPlayer;
    }

    private void Update()
    {
        // Only run if still valid
        if (this == null || gameObject == null) return;

        CheckIsPlayerBehind();

        if (isShowingText)
        {
            interactText.text = GetInteractionPrompt();
        }
    }

    public void CheckIsPlayerBehind()
    {
        if (playerController == null) return;

        var dir = (playerController.transform.position - transform.position).normalized;
        isPlayerBehind = Vector2.Dot(new Vector2(dir.x, dir.y), Vector2.down) < 0;
    }

    public override bool IsInteractable()
    {
        if (playerController == null || playerController.isDead)
        {
            return false;
        }

        if (isInteracting)
        {
            return false;
        }

        return base.IsInteractable();
    }

    public override string GetInteractionPrompt()
    {
        if (isPlayerBehind) return "[E] Steal Key";
        return "[E] Talk";
    }

    public override void OnInteract(GameObject interactor)
    {
        if (!isShowingText)
            return;

        isInteracting = true;
        HideInteractionPrompt();
        playerController = interactor.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Stop();
        }

        if (isPlayerBehind)
        {
            StealKey();
        }
        else
        {
            DialogueSystem.StartDialogue(dialogues[giftDialogueIndex]);
            DialogueSystem.OnOption1Clicked += GivePlayerKey;
            DialogueSystem.OnOption2Clicked += KillPlayer;
        }
    }

    public void GivePlayerKey()
    {
        Debug.Log("GivePlayerKey");
        hasInteracted = true;
        Inventory.instance.ObtainItem(cursedKey);
        isPlayerCursed = true;

        // Unsubscribe after use
        DialogueSystem.OnOption1Clicked -= GivePlayerKey;
        DialogueSystem.OnOption2Clicked -= KillPlayer;
    }

    public void CursePlayer()
    {
        if (isPlayerCursed)
        {
            Debug.Log("Player is cursed");
            if (playerController != null)
            {
                playerController.enabled = true;
                playerController.DamagePlayerAndKill();
            }
            isInteracting = false;
        }
    }

    public void StealKey()
    {
        // Check if the GameObject and Animator are still valid
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Trader GameObject is destroyed or inactive");
            return;
        }

        Debug.Log("KeyStolen");
        hasInteracted = true;
        Inventory.instance.ObtainItem(normalKey);
        isInteracting = false;
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    public void KillPlayer()
    {
        // Check if the GameObject and Animator are still valid
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Trader GameObject is destroyed or inactive - ignoring kill");
            // Unsubscribe to prevent future calls
            DialogueSystem.OnOption2Clicked -= KillPlayer;
            return;
        }

        // Unsubscribe immediately to prevent double calls
        DialogueSystem.OnOption2Clicked -= KillPlayer;
        DialogueSystem.OnOption1Clicked -= GivePlayerKey;

        if (animator != null && animator.gameObject != null && animator.isActiveAndEnabled)
        {
            animator.Play("Attack");
        }
        else
        {
            Debug.LogWarning("Animator is not available");
        }
        StartCoroutine(KillPlayerAfterDelay(playerController, 0.3f, playerController?.gameObject));
    }

    private IEnumerator KillPlayerAfterDelay(PlayerController controller, float delay, GameObject interactor)
    {
        yield return new WaitForSeconds(delay);

        if (interactor == null || controller == null)
        {
            Debug.LogWarning("Player reference is null, cannot kill");
            isInteracting = false;
            yield break;
        }

        Vector2 direction = (interactor.transform.position - transform.position).normalized;
        Rigidbody2D playerRb = interactor.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            Vector2 force = new Vector2(0.5f * knockbackForce, -1 * knockbackForce);
            playerRb.AddForce(force, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(delay);

        SoundManager.Instance.PlaySFX("TraderAttack");
        controller.explosionVFX.SetActive(true);

        if (controller != null)
        {
            controller.enabled = true;
            controller.Die();
        }

        DialogueSystem.StartDialogue(dialogues[killPlayerDialogueIndex]);
        isInteracting = false;
    }

    public override void HandleDialogueComplete()
    {
        base.HandleDialogueComplete();
        // Clean up events when dialogue completes
        DialogueSystem.OnOption1Clicked -= GivePlayerKey;
        DialogueSystem.OnOption2Clicked -= KillPlayer;
        isInteracting = false;
    }
}