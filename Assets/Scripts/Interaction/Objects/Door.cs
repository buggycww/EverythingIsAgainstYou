using UnityEngine;

public class Door : BaseInteractable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private int requiredKeyId;
    private bool isOpen;

    public override bool IsInteractable() => !isOpen && !isLocked;

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
            var playerInventory = interactor.GetComponent<Inventory>();
            if (playerInventory != null && playerInventory.HasItem(requiredKeyId))
            {
                isLocked = false;
                OpenDoor();
            }
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        // Animation, sound, etc.
    }
}