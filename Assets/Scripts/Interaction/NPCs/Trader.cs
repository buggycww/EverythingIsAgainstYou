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
        inventoryUI.OnItemPopUpClosed += CursePlayer;
        playerController = FindAnyObjectByType<PlayerController>();
    }

    private void Update()
    {
        CheckIsPlayerBehind();

        if (isShowingText)
        {
            interactText.text = GetInteractionPrompt();
        }
    }

    public void CheckIsPlayerBehind()
    {
        var dir = (playerController.transform.position - transform.position).normalized;
        if (Vector2.Dot(new Vector2(dir.x, dir.y), Vector2.down) < 0)
        {
            isPlayerBehind = true;
        }
        else
        {
            isPlayerBehind = false;
        }
    }

    public override bool IsInteractable()
    {
        if (playerController.isDead)
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
    }

    public void CursePlayer()
    {
        if (isPlayerCursed)
        {
            Debug.Log("Player is cursed");
            playerController.enabled = true;
            playerController.DamagePlayerAndKill();
            isInteracting = false;
        }
    }

    public void StealKey()
    {
        Debug.Log("KeyStolen");
        hasInteracted = true;
        Inventory.instance.ObtainItem(normalKey);
        isInteracting = false;
        playerController.enabled = true;
    }

    public void KillPlayer()
    {
        animator.Play("Attack");
        StartCoroutine(KillPlayerAfterDelay(playerController, 0.3f, playerController.gameObject));
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
        DialogueSystem.StartDialogue(dialogues[killPlayerDialogueIndex]);
        isInteracting = false;
    }

    public override void HandleDialogueComplete()
    {
        base.HandleDialogueComplete();
    }
}
