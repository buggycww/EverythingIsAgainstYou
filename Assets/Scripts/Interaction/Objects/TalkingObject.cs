using UnityEngine;

public class TalkingNPC : BaseInteractable
{
    [SerializeField] private int dialogueIndex = 0;

    public override void OnInteract(GameObject interactor)
    {
        base.OnInteract(interactor);

        DialogueSystem.StartDialogue(dialogues[dialogueIndex]);
    }
}
