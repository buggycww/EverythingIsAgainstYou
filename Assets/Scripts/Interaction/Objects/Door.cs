using UnityEngine;

public class Door : BaseInteractable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private int requiredKeyId;
    private bool isOpen;

    [SerializeField] private Sprite openedIcon;
    [SerializeField] private Sprite openedShadowIcon;
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private int getKeyDialogue;

    public override bool IsInteractable() => !(isOpen && !isLocked);

    public override string GetInteractionPrompt()
    {
        if (isLocked) return "[E] Unlock";
        return isOpen ? "" : "[E] Open Door";
    }

    public override void OnInteract(GameObject interactor)
    {
        base.OnInteract(interactor);

        if (isLocked)
        {
            if (Inventory.instance.HasItem(requiredKeyId))
            {
                isLocked = false;
                OpenDoor();
            }
            else
            {
                DialogueSystem.StartDialogue(dialogues[getKeyDialogue]);
            }
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        SoundManager.Instance.PlaySFX("DoorOpen");
        isOpen = true;
        transform.GetComponent<Collider2D>().enabled = false;
        transform.GetComponent<SpriteRenderer>().sprite = openedIcon;
        shadowRenderer.sprite = openedShadowIcon;
    }
}