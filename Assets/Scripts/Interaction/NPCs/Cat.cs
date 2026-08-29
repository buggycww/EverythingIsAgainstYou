using UnityEngine;

public class Cat : NPC
{
    [SerializeField] private int goInDialogueIndex;
    [SerializeField] private int mockDialogueIndex;

    private bool hasTalked;

    public override void OnInteract(GameObject interactor)
    {
        base.OnInteract(interactor);

        hasTalked = true;
        DialogueSystem.StartDialogue(dialogues[goInDialogueIndex]);
    }

    public void MockPlayer()
    {
        if (hasTalked)
        {
            DialogueSystem.StartDialogue(dialogues[mockDialogueIndex]);
        }
    }
}
